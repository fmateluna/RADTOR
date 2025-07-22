using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace LocalRT.radtor.local.web
{
    public abstract class RTServer
    {

        protected int port;
        TcpListener listener;
        bool active = true;

        public bool Active
        {
            get
            {
                return active;
            }

            set
            {
                active = value;
            }
        }

        public RTServer(int port)
        {
            this.port = port;
        }

        public void listen()
        {
            listener = new TcpListener(port);
            listener.Start();
            while (Active)
            {
                TcpClient clientTcpListen = listener.AcceptTcpClient();
                RTHttpProcessorData processor = new RTHttpProcessorData(clientTcpListen, this);
                Thread thread = new Thread(new ThreadStart(processor.process));
                thread.Start();
            }
        }

        public abstract void handleGETRequest(RTHttpProcessorData p);
        public abstract void handlePOSTRequest(RTHttpProcessorData p, StreamReader inputData);
    }
}