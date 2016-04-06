using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaransferingLib;

namespace GameClient
{
    public partial class Form1 : Form
    {
        const int BUFFER_SIZE = 4096;

        UdpSpeaker speaker;
        TcpListener tcpListener;

        const int serverUdpPor = 11000;
        const int serverTcpPort = 11001;
        const int gameTcpPort = 11002;

        IPEndPoint ServerIP;
        Socket socketTableRequester;

        public Form1()
        {
            InitializeComponent();

            speaker = new UdpSpeaker(serverUdpPor);

            tcpListener = new TcpListener();
            tcpListener.Accepted += tcpListener_Accepted;
            tcpListener.Start(gameTcpPort);


            ServerIP = new IPEndPoint(IPAddress.Loopback, serverTcpPort);
        }

        void tcpListener_Accepted(Socket s)
        {
            StartGame(s);
        }

        void StartGame(Socket s)
        {
            //используешь s для передачи игровых данных
            connectionStateLabel.Text = "Game has started with " + (s.RemoteEndPoint as IPEndPoint).Address;
        }

        private void CreateGameBtn_Click(object sender, EventArgs e)
        {
            speaker.StartBroadcasting("Alexander");
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            speaker.Stop();
        }



        //Подключаешься к серверу по TCP
        private void RefreshBtn_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                connectionStateLabel.Text = "Connecting...";
            });

            socketTableRequester = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketTableRequester.BeginConnect(ServerIP, ConnectedCallback, null);
        }

        public byte[] buff = new byte[BUFFER_SIZE];
        //При подключении ждем таблицу от сервера
        void ConnectedCallback(IAsyncResult ar)
        {
            try
            {
                Invoke((MethodInvoker)delegate
                {
                    connectionStateLabel.Text = "Waiting table";
                });

                socketTableRequester.EndConnect(ar);
                socketTableRequester.BeginReceive(buff, 0, BUFFER_SIZE, SocketFlags.None, ReceivedCallback, null);
            }
            catch
            {
                Invoke((MethodInvoker)delegate
                {
                    connectionStateLabel.Text = "Time out, repeat pls";
                });
            }
        }

        //Преобразуешь пришедшие данные в норм формат(данные в buff)
        void ReceivedCallback(IAsyncResult ar)
        {
            Invoke((MethodInvoker)delegate
            {
                connectionStateLabel.Text = "Table has been received";
            });

            int rec = socketTableRequester.EndReceive(ar);
            if (rec == 0)
                return;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buff, 0, rec);

            var ipTable = (IpTable)bf.Deserialize(ms);
            foreach (var ip in ipTable.Table)
            {
                //чтоб свой не отображало раскоменть(для проверки модно оставить)
                //if (ip.Address == IPAddress.Any) continue;

                //изменение clientListBox в том потоке, в котором он был создан(инча ошибка);
                Invoke((MethodInvoker)delegate
                {
                    clientListBox.BeginUpdate();
                    clientListBox.Items.Clear();
                    clientListBox.Items.Add(ip.Address.ToString());
                    clientListBox.EndUpdate();
                });
            }
        }



        private void connectBtn_Click(object sender, EventArgs e)
        {

        }

        private void clientListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            playerIP.Text = clientListBox.SelectedItem.ToString() + ":" + gameTcpPort.ToString();
        }
    }
}
