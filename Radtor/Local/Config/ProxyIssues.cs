using LocalRT.radtor.utils;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace LocalRT.Radtor.Local.Config
{
    internal class ProxyIssues
    {
        public string ProxyOS { get; internal set; }
        public string ProxyFireFox { get; internal set; }

        private string dummyPage = "http://www.microsoft.com/";

        private FireFoxConfig firefox = new FireFoxConfig();

        internal bool thisNeedProxy()
        {
            var webRequest = WebRequest.Create(dummyPage);
            webRequest.Timeout = 10000;
            try
            {
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content))
                {
                    var strContent = reader.ReadToEnd();
                }
                //CHECKEAR SI SE NECESITA
                /*
                try
                {
                    var proxy = System.Net.HttpWebRequest.GetSystemWebProxy();
                    Uri proxyUri = proxy.GetProxy(new Uri(dummyPage));
                    if (!dummyPage.Equals(proxyUri.AbsoluteUri))
                    {
                        if (CanPing(proxyUri.AbsoluteUri))
                        {
                            ProxyOS = proxyUri.AbsoluteUri;
                            //LocalConfiguration.ProxyOS = ProxyOS;
                        }
                    }
                }
                catch {
                }
                */
                return false;
            }
            catch
            {
                return true;
            }
        }


        private static bool CanPing(string address)
        {
            String pingAddress = (address.StartsWith("http") ? address.Substring("http://".Length) : address.Substring("https://".Length));
            String host = pingAddress.Split(':')[0];
            int port = int.Parse(pingAddress.Split(':')[1].Replace("/", ""));
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect(host, port);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        internal void scan()
        {
            var proxy = System.Net.HttpWebRequest.GetSystemWebProxy();
            Uri proxyUri = proxy.GetProxy(new Uri(dummyPage));
            if (!dummyPage.Equals(proxyUri.AbsoluteUri))
            {
                if (CanPing(proxyUri.AbsoluteUri))
                {
                    ProxyOS = proxyUri.AbsoluteUri;
                }
            }
            
            if (firefox.inTheHouse())
            {
                firefox.readProfilesConfig();
                if (CanPing(firefox.Proxy))
                {
                    ProxyFireFox = firefox.Proxy;
                }
            }

            if (ProxyOS == null && ProxyFireFox == null)
            {
                RadTorLog.log("Proxy's no responden!");
                scan();
            }
            else {
                RadTorLog.log("Proxy's Detectados");
                RadTorLog.log("ProxyFireFox :" + ProxyFireFox);
                RadTorLog.log("ProxyOS :" + ProxyOS);
            }
        }
    }
}