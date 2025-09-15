// File: Form1.cs
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace FileReceiverServer
{
    public partial class Form1 : Form
    {
        private ArrayList alSockets;
        private Label lblStatus;
        private ListBox lbConnections;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.lblStatus = new Label();
            this.lbConnections = new ListBox();
            this.SuspendLayout();
            // lblStatus
            this.lblStatus.Location = new Point(12, 9);
            this.lblStatus.Size = new Size(460, 23);
            this.lblStatus.Text = "My IP address is ...";
            // lbConnections
            this.lbConnections.Location = new Point(12, 40);
            this.lbConnections.Size = new Size(460, 200);
            // Form
            this.ClientSize = new Size(484, 261);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lbConnections);
            this.Text = "TCP Server";
            this.Load += new EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Menggunakan GetHostEntry (modern) — sesuai maksud modul
            IPHostEntry IPHost = Dns.GetHostEntry(Dns.GetHostName());
            lblStatus.Text = "My IP address is " + IPHost.AddressList[0].ToString();

            alSockets = new ArrayList();

            Thread thdListener = new Thread(new ThreadStart(listenerThread));
            thdListener.IsBackground = true;
            thdListener.Start();
        }

        public void listenerThread()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();
            while (true)
            {
                Socket handlerSocket = tcpListener.AcceptSocket();
                if (handlerSocket.Connected)
                {
                    AddConnection(handlerSocket.RemoteEndPoint.ToString() + " connected.");
                    lock (this)
                    {
                        alSockets.Add(handlerSocket);
                    }
                    Thread thdHandler = new Thread(new ThreadStart(handlerThread));
                    thdHandler.IsBackground = true;
                    thdHandler.Start();
                }
            }
        }

        public void handlerThread()
        {
            Socket handlerSocket = null;
            lock (this)
            {
                if (alSockets.Count > 0)
                {
                    handlerSocket = (Socket)alSockets[alSockets.Count - 1];
                    alSockets.RemoveAt(alSockets.Count - 1);
                }
            }
            if (handlerSocket == null) return;

            NetworkStream networkStream = new NetworkStream(handlerSocket);
            int thisRead = 0;
            int blockSize = 1024;
            byte[] dataByte = new byte[blockSize];

            lock (this) // hanya satu thread menulis file di satu waktu (sesuai modul)
            {
                Stream fileStream = File.OpenWrite("c:\\my documents\\upload.txt");
                while (true)
                {
                    thisRead = networkStream.Read(dataByte, 0, blockSize);
                    if (thisRead <= 0) break;
                    fileStream.Write(dataByte, 0, thisRead);
                }
                fileStream.Close();
            }

            AddConnection("File Written");
            try { handlerSocket.Close(); } catch { }
        }

        // helper thread-safe untuk menambahkan ke listbox (UI thread)
        private void AddConnection(string text)
        {
            if (this.lbConnections.InvokeRequired)
            {
                this.lbConnections.Invoke(new Action(() => this.lbConnections.Items.Add(text)));
            }
            else
            {
                this.lbConnections.Items.Add(text);
            }
        }
    }
}
