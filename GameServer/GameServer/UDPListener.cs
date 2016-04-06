using System;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class UdpListener
    {
        UdpClient client;

        public delegate void ReceivedHandler(IPEndPoint e, byte[] data);
        public event ReceivedHandler Received;

        public bool Running
        {
            get;
            private set;
        }

        public void Start(int port)
        {
            if (Running)
                return;

            client = new UdpClient(port);
            client.BeginReceive(ReceivedCallback, null);

            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                return;

            client.Close();
            Running = false;
        }

        private void ReceivedCallback(IAsyncResult ar)
        {
            try
            {
                var remoteEndPoin = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.EndReceive(ar, ref remoteEndPoin);

                if (Received != null)
                    Received(remoteEndPoin, data);
            }
            catch
            {
                // ignored
            }

            if (!Running) return;
            try
            {
                client.BeginReceive(ReceivedCallback, null);
            }
            catch
            {
                // ignored
            }
        }

    }
}
