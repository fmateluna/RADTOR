using LocalRT.Radtor.Local.Config;
using LocalRT.radtor.utils;
using System;
using System.IO;
using LocalRT.radtor.local.core;

namespace LocalRT.radtor.local.config.tor
{
    public class ConfigTOR
    {
        public string AvoidDiskWrites { get; set; }
        public string DataDirectory { get; set; }
        public string GeoIPFile { get; set; }
        public string HTTPProxy { get; set; }
        public string HTTPSProxy { get; set; }
        public string HiddenServiceDir { get; set; }
        public string HiddenServicePort { get; set; }
        public bool Proxy { get; set; }

        public string SocksListenAddress { get; set; }
        public string SocksPort { get; set; }

        //public string ControlPort { get; set; }

        //public static string hashedControlPassword = "16:9BB8A0D603BA0868607F1BE6FB98FD2450BA1030D0149E18058AEF28A8";

        private String torc;

        public ConfigTOR(string _configFile) {
            this.torc = _configFile;
        }

        public string createConfigFile()
        {
            ProxyIssues getpx = new ProxyIssues();
            if (getpx.thisNeedProxy())
            {
                getpx.scan();
                Proxy = true;
                string httpHttpsProxy = "";
                if (getpx.ProxyOS==null) {
                    httpHttpsProxy = getpx.ProxyFireFox;
                }
                else {
                    httpHttpsProxy = getpx.ProxyOS;
                }
                String pingAddress = (httpHttpsProxy.StartsWith("http") ? httpHttpsProxy.Substring("http://".Length) : httpHttpsProxy.Substring("https://".Length));
                String host = pingAddress.Split(':')[0];
                int port = int.Parse(pingAddress.Split(':')[1].Replace("/", ""));
                HTTPProxy = String.Format("{0}:{1}",host,port);
                HTTPSProxy = HTTPProxy;
            }
            else {
                Proxy = false;
            }
           
            String loremIpsu = "";
            for (int i = 0; i < 1000; i++)
            {
                loremIpsu += "\r\n";
            }
            String configInfo = loremIpsu;
            //configInfo += "\r\nAllowSingleHopCircuits 1";
            configInfo += string.Format("\r\nAvoidDiskWrites {0}", AvoidDiskWrites);

            /*
            configInfo += string.Format("\r\nControlPort {0}", ControlPort);
            configInfo += string.Format("\r\nHashedControlPassword {0}", hashedControlPassword);
            configInfo += "\r\nCookieAuthentication 1";
            */
            configInfo += string.Format("\r\nDataDirectory {0}", DataDirectory);
            configInfo += string.Format("\r\nGeoIPFile {0}", GeoIPFile);
            configInfo += string.Format("\r\nHiddenServiceDir {0}", HiddenServiceDir);
            configInfo += string.Format("\r\nHiddenServicePort {0}", HiddenServicePort);
            
            //CORREO :D
            //configInfo += "\r\nHiddenServicePort 25 mail.bitmessage.ch:25";
             
            if (Proxy)
            {
                configInfo += string.Format("\r\nHTTPProxy {0}", HTTPProxy);
                configInfo += string.Format("\r\nHTTPSProxy {0}", HTTPSProxy);
            }
            configInfo += string.Format("\r\nSocksListenAddress {0}", SocksListenAddress);
            LocalConfiguration.ProxyFireFox = SocksListenAddress;
            LocalConfiguration.ProxyOS = SocksListenAddress;
            configInfo += string.Format("\r\nSocksPort {0}", SocksPort);
            RadTorLog.log(configInfo.Substring(loremIpsu.Length));
            String cmd = "ADD!" + LocalConfiguration.getMac() + "!" + HiddenServicePort.Split(' ')[1].Replace("127.0.0.1","localhost") ; 
            EncripTOR encrip = new EncripTOR();
            RadTorLog.log("TEST URL = " + encrip.encript(cmd));
            return configInfo;
        }

        public void save()
        {
            if (File.Exists(torc)) {
                File.Delete(torc);
            }
            RadTorLog.log("CONFIG TOR CUARDADO");
            File.WriteAllText(torc, createConfigFile());
        }
    }
}

