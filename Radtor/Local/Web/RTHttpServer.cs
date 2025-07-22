using LocalRT.radtor.local.core;
using LocalRT.radtor.utils;
using System.IO;

namespace LocalRT.radtor.local.web
{
    public class RTHttpServer : RTServer
    {
        private RTCommand protocol = new RTCommand();

        public RTHttpServer(int port) : base(port)
        {
        }

        public override void handleGETRequest(RTHttpProcessorData p)
        {
            //if (p.http_url.Equals("/Test.png"))
            //{
            //    Stream fs = File.Open("../../Test.png", FileMode.Open);
            //    p.writeSuccess("image/png");
            //    fs.CopyTo(p.outputStream.BaseStream);
            //    p.outputStream.BaseStream.Flush();
            //}
            ////("request: {0}", p.http_url);            
            p.httpMethod = "GET, POST, OPTIONS, PUT, PATCH, DELETE";            
            p.httpHeaders["Access-Control-Allow-Origin"] = "*";
            p.httpHeaders["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS, PUT, PATCH, DELETE";
            p.httpHeaders["Access-Control-Allow-Headers"] = "X-Requested-With,contenttype";
            p.httpHeaders["Access-Control-Allow-Credentials"] = true;
            p.writeSuccess();
            RadTorLog.log("p.RadtorRequest CMD  : \t" + p.RadtorRequest.cmd);
            RadTorLog.log("p.RadtorRequest ARGS : \t" + p.RadtorRequest.args);
            string response = protocol.execute(p.RadtorRequest);
            if (p.RadtorRequest.prefixJsonP != null)
            {
                response = string.Format("{0}({1})", p.RadtorRequest.prefixJsonP, response);
            }
            RadTorLog.log("Server Response before encript: \n" + response);
            EncripTOR encrip = new EncripTOR();
            //p.outputStream.WriteLine(encrip.encript(response));
            p.outputStream.WriteLine(response);

        }

        public override void handlePOSTRequest(RTHttpProcessorData p, StreamReader inputData)
        {
            string data = inputData.ReadToEnd();
            p.writeSuccess();
            /*
            p.outputStream.WriteLine("<html><body><h1>test server</h1>");
            p.outputStream.WriteLine("<a href=/test>return</a><p>");
            p.outputStream.WriteLine("postbody: <pre>{0}</pre>", data);
            */
        }

    }
}
