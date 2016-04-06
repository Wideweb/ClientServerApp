using System;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class TcpListener
    {
        Socket listener;

        public delegate void AcceptedHandler(Socket e);
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
            listener.Listen(100);

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
            Socket s = listener.EndAccept(ar);
            if (Accepted != null)
                Accepted(s);

            if (!Running) return;
            try
            {
                listener.BeginAccept(AcceptedCallback, null);
            }
            catch
            {
                // ignored
            }
        }
    }
}
