using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameClient
{
    public class UdpSpeaker
    {
        Socket speaker;
        IPEndPoint ep;
        byte[] buffer;

        public bool Running { get; private set; }

        public UdpSpeaker(int port)
        {
            ep = new IPEndPoint(IPAddress.Broadcast, port);
        }

        public void StartBroadcasting(string text)
        {
            if (Running)
                return;

            speaker = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                ProtocolType.Udp);
            speaker.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            buffer = Encoding.ASCII.GetBytes(text);
            speaker.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, ep, SendToCallback, null);

            Running = true;
        }

        public void Stop()
        {
            if (!Running)
                return;

            speaker.Close();
            Running = false;
        }

        private void SendToCallback(IAsyncResult ar)
        {
            try
            {
                int sendedBytes = speaker.EndSendTo(ar);
            }
            catch
            {
                // ignored
            }

            if (!Running) return;
            try
            {
                Thread.Sleep(2000);
                speaker.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, ep, SendToCallback, null);
            }
            catch
            {
                // ignored
            }
        }
    }
}
