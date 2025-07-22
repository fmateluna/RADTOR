using LocalRT.radtor.utils;
using System;
using System.Collections;
using System.IO;

namespace LocalRT.Radtor.Local.Config
{
    internal class FireFoxConfig
    {
        public string Proxy { get; internal set; }


        private string profile = @"\prefs.js";

        private string firefoxPath;
        private readonly string PROXY_HTTP_HOST = "user_pref(\"network.proxy.http\", \"";
        private readonly string PROXY_HTTP_PORT = "user_pref(\"network.proxy.http_port\",";

        internal bool inTheHouse()
        {
            firefoxPath = LocalConfiguration.translateSystemVar(@"%appdata%\Mozilla\Firefox\");
            return Directory.Exists(firefoxPath);

        }

        internal void readProfilesConfig()
        {
            var directories = Directory.GetDirectories(firefoxPath + @"\Profiles");
            DateTime lastWriteTime = new DateTime();
            int i = 0;
            foreach (String folder in directories)
            {
                String path = folder + profile;
                if (File.Exists(path))
                {
                    FileInfo profile = new FileInfo(path);
                    if (i == 0 || lastWriteTime > profile.LastWriteTime)
                    {
                        i = 1;
                        Proxy = readProfilesConfig(path);
                        lastWriteTime = profile.LastWriteTime;
                    }
                }
            }
        }

        private string readProfilesConfig(string firefoxProfile)
        {
            string line;
            string proxy = "";
            string host = "";
            string port = "";
            System.IO.StreamReader file = new System.IO.StreamReader(firefoxProfile);
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith(PROXY_HTTP_HOST))
                {
                    host = (line.Substring(PROXY_HTTP_HOST.Length)).Replace("\");", "").TrimEnd().TrimStart();
                }
                if (line.StartsWith(PROXY_HTTP_PORT))
                {
                    port = (line.Substring(PROXY_HTTP_PORT.Length)).Replace(");", "").TrimEnd().TrimStart();
                }
            }
            proxy = String.Format("http://{0}:{1}/", host, port);
            return proxy;
        }
    }
}