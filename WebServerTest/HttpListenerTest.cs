using System;
using System.Net;

namespace HttpListenerTest
{
    class Program
    {
        HttpListener _listener = new HttpListener();

        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            _listener.Prefixes.Add("http://localhost:1234/");
            _listener.Start();
            Console.WriteLine("Listening, hit enter to stop");
            _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
            Console.ReadLine();
            _listener.Stop();
        }

        public void GetContextCallback(IAsyncResult result)
        {
            HttpListenerContext context = _listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("");
            sb.Append(string.Format("HttpMethod: {0}", request.HttpMethod));
            sb.Append(string.Format("Uri:        {0}", request.Url.AbsoluteUri));
            sb.Append(string.Format("LocalPath:  {0}", request.Url.LocalPath));
            foreach (string key in request.QueryString.Keys)
            {
                sb.Append(string.Format("Query:      {0} = {1}", key, request.QueryString[key]));
            }
            sb.Append("");

            string responseString = sb.ToString();
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;

            using (System.IO.Stream outputStream = response.OutputStream)
            {
                outputStream.Write(buffer, 0, buffer.Length);
            }
            _listener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
        }
    }
}