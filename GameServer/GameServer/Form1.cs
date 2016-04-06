using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaransferingLib;

namespace GameServer
{
    public partial class Form1 : Form
    {
        const int BUFFER_SIZE = 4096;

        List<IPEndPoint> clients;
        UdpListener listener;
        const int udpPort = 11000;
        const int tcpPort = 11001;

        TcpListener tcpListener;
        Socket tableSender;

        public Form1()
        {
            InitializeComponent();

            clients = new List<IPEndPoint>();

            listener = new UdpListener();
            listener.Received += listener_Received;
            listener.Start(udpPort);

            //Отвечает на запросы свзяанные с таблицей клиентов
            InitializeTableSender();
        }

        void listener_Received(IPEndPoint e, byte[] data)
        {
            //перестаем слушать т.к частые колбэки срывают выполнение кода
            listener.Stop();
            AddClient(e);
            listener.Start(udpPort);
        }

        void AddClient(IPEndPoint e)
        {
            //есть ли в таблице уже этот адресс
            IPEndPoint ep = clients.FirstOrDefault(x => x.Address == e.Address);
            if (ep == null)
                clients.Add(e);

            UpdateTable();
        }

        void UpdateTable()
        {
            //clientListBox.Items.Clear();
            foreach (IPEndPoint client in clients)
            {
                //изменение clientListBox в том потоке, в котором он был создан(инча ошибка);
                Invoke((MethodInvoker)delegate
                {
                    clientListBox.BeginUpdate();
                    clientListBox.Items.Clear();
                    clientListBox.Items.Add(client.Address.ToString());
                    clientListBox.EndUpdate();
                });
            }
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            clientListBox.Items.Clear();
        }



        void InitializeTableSender()
        {
            tcpListener = new TcpListener();
            tcpListener.Accepted += tableSender_Accepted;
            tcpListener.Start(tcpPort);
        }

        //Если кто-то подключился по tcp, шлем ему таблицу со всеи ip 
        void tableSender_Accepted(Socket e)
        {
            Invoke((MethodInvoker)delegate
            {
                connectionStateLabel.Text = (e.RemoteEndPoint as IPEndPoint).Address.ToString() + " has connected";
            });

            tableSender = e;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();

            IpTable table = new IpTable{Table = new List<IPEndPoint>()};
            foreach(var ip in clients)
            {
                table.Table.Add(ip);
            }

            bf.Serialize(ms, table);


            Invoke((MethodInvoker)delegate
            {
                connectionStateLabel.Text += "\nTable has been sended";
            });
            tableSender.BeginSend(ms.ToArray(), 0, (int)ms.Length, SocketFlags.None, SendedCallback, null);
        }

        //После того, как таблица будет успешно передана, разрываем tcp подключение с текущим клиентом
        void SendedCallback(IAsyncResult ar)
        {
            tableSender.EndSend(ar);
            tableSender.Disconnect(true);

            Invoke((MethodInvoker)delegate
            {
                connectionStateLabel.Text = "Table has been sended";
            });
        }
    }
}
