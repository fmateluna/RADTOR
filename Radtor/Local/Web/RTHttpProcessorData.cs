using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using LocalRT.radtor.local.core;
using LocalRT.radtor.utils;

namespace LocalRT.radtor.local.web
{
    public class RTHttpProcessorData
    {
        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        private const int BUF_SIZE = 4096;

        public TcpClient socket;
        public RTServer srv;    
        private Stream inputStream;
        public StreamWriter outputStream;
        public String httpMethod;
        public String httpUrl;
        public String httpVersionString;
        public Hashtable httpHeaders = new Hashtable();
        private EncripTOR encripTOR = new EncripTOR();

        public RTRequest RadtorRequest { get; internal set; }

        public RTHttpProcessorData(TcpClient s, RTServer srv)
        {
            this.socket = s;
            this.srv = srv;
           
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }

        public void process()
        {
            try
            {
                //recepcion de datos limpia.. solo buffer stream
                inputStream = new BufferedStream(socket.GetStream());

                // we probably shouldn't be using a streamwriter for all output from handlers either
                outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
                try
                {
                    parseRequest();
                    readHeaders();
                    if (httpMethod.Equals("GET"))
                    {                        
                        string urldecore = RadTorUtil.DecodeUrlString(httpUrl);
                        RadtorRequest = getRADTORRequestFromGetURL(urldecore);
                        handleGETRequest();
                    }
                    else if (httpMethod.Equals("POST"))
                    {
                        handlePOSTRequest();
                    }
                }
                catch (Exception e)
                {
                    //("Exception: " + e.ToString());
                    RadTorLog.log("Server>> Error:\t" + e.StackTrace);
                    writeBadRadtorRequest();
                }
                outputStream.Flush();
                inputStream.Flush(); // flush any remaining output
                inputStream = null;
                outputStream = null; // bs = null;            
                socket.Close();
            }catch(IOException IOE){
                RadTorLog.log("Server>> Error:\t"+ IOE.StackTrace);
            }
        }

        private RTRequest getRADTORRequestFromGetURL(String http_url)
        {
            try
            {
                RTRequest radtorRQ = new RTRequest();
                if (http_url.Split('?').Length > 1) {
                    radtorRQ.prefixJsonP = http_url.Split('?')[1].Split('=')[1].Split('&')[0];
                    http_url = http_url.Split('?')[0];                    
                }
                string roar = encripTOR.decrypt(http_url.Substring(1));
                char rtPiper = roar[3];
                radtorRQ.cmd = roar.Split(rtPiper)[0];
                radtorRQ.args = roar.Split(rtPiper)[1];
                //TODO :Validar Token
                radtorRQ.token = roar.Split(rtPiper)[2];
                return radtorRQ;
            }
            catch (Exception w) {
                RadTorLog.log("La peticion no posee comando RT");
                return null;
            }
        }

        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            httpMethod = tokens[0].ToUpper();
            httpUrl = tokens[1];
            httpVersionString = tokens[2];
        }

        public void readHeaders()
        {            
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {                
                    return;
                }
                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                string value = line.Substring(separator + 1).Replace(" ", "");
                httpHeaders[name] = value;
            }
            httpHeaders["Access-Control-Allow-Origin"] = "*";
            httpHeaders["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS, PUT, PATCH, DELETE";
            httpHeaders["Access-Control-Allow-Headers"] = "X-Requested-With,contenttype";
            httpHeaders["Access-Control-Allow-Credentials"] = true;
        }

        public void handleGETRequest()
        {
            srv.handleGETRequest(this);
        }
        
        public void handlePOSTRequest()
        {
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length"))
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(
                        String.Format("POST Content-Length({0}) too big for this simple server",
                          content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {                    
                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));                    
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }

        public void writeSuccess(string content_type = "text/html")
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }

        public void writeBadRadtorRequest()
        {
            outputStream.WriteLine("HTTP/1.0 403 Service Unavailable");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }
}