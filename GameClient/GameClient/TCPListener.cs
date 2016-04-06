using System;
using System.Net;
using System.Net.Sockets;

namespace GameClient
{
    public class TcpListener
    {
        Socket listener;

        public delegate void AcceptedHandler(Socket s);
        public event AcceptedHandler Accepted;

        public bool Running
        {
            get;
            private set;
        }

        public void Start(int port)
        {
            if (Running)
                return;

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Any, port));
            listener.Listen(0);

            listener.BeginAccept(AcceptedCallback, null);
            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                return;

            listener.Close();
            Running = false;
        }

        private void AcceptedCallback(IAsyncResult ar)
        {
            try
            {
                Socket s = listener.EndAccept(ar);
                if (Accepted != null)
                    Accepted(s);

                this.Stop();
            }
            catch
            {
                // ignored
            }
        }
    }
}
