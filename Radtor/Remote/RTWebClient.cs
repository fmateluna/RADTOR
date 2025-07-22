using System;
using System.Net;

namespace LocalRT.Radtor.Remote
{
    class RTWebClient : WebClient
    {
        private string originalURL;
        private static string userAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

        public int timeOutMin { get; internal set; }

        public RTWebClient(string url, IWebProxy proxy)
        {
            originalURL = url;
            Proxy = proxy;
            Headers.Add("Accept-Encoding", "gzip, deflate");
            Headers.Add("Accept-Language", "es-ES,es;q=0.8");
            Headers.Add("Cache-Control", "max-age=0");
            Headers.Add("Cache-Control", "must-revalidate");
            Headers.Add("Cache-Control", "no-cache");
            Headers.Add("Cache-Control", "no-store");
            Headers.Add("Expires", "Mon, 8 Aug 2006 10:00:00 GMT");
            Uri onionUri = new Uri(originalURL);
            string originalHost = onionUri.Host;
            Headers.Add("Origin", originalHost);
            Headers.Add("Pragma", "no-cache");
            Headers.Add("Access-Control-Allow-Origin", "*");
            Headers.Add("Access-Control-Allow-Methods", "PUT, GET, POST, DELETE, OPTIONS");
            Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            Headers.Add("user-agent", userAgent);
            Headers.Add("X-JAVASCRIPT-ENABLED", "true");
            Headers.Add("X-UA-Compatible", "IE=EmulateIE7");
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest wreq = base.GetWebRequest(uri);
            wreq.Timeout = timeOutMin * 60 * 1000;
            return wreq;
        }
    }
}
