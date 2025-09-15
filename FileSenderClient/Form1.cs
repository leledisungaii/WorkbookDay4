// File: Form1.cs
using System;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;

namespace FileSenderClient
{
    public partial class Form1 : Form
    {
        TextBox tbFilename;
        TextBox tbServer;
        Button btnBrowse;
        Button btnSend;
        OpenFileDialog openFileDialog;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.tbFilename = new TextBox();
            this.tbServer = new TextBox();
            this.btnBrowse = new Button();
            this.btnSend = new Button();
            this.openFileDialog = new OpenFileDialog();
            this.SuspendLayout();

            // tbFilename
            this.tbFilename.Location = new Point(12, 12);
            this.tbFilename.Size = new Size(360, 23);

            // btnBrowse
            this.btnBrowse.Location = new Point(380, 12);
            this.btnBrowse.Size = new Size(75, 23);
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.Click += new EventHandler(btnBrowse_Click);

            // tbServer
            this.tbServer.Location = new Point(12, 45);
            this.tbServer.Size = new Size(360, 23);
            this.tbServer.Text = "localhost";

            // btnSend
            this.btnSend.Location = new Point(380, 45);
            this.btnSend.Size = new Size(75, 23);
            this.btnSend.Text = "Send";
            this.btnSend.Click += new EventHandler(btnSend_Click);

            // Form
            this.ClientSize = new Size(468, 85);
            this.Controls.Add(this.tbFilename);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbServer);
            this.Controls.Add(this.btnSend);
            this.Text = "TCP Client";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
            tbFilename.Text = openFileDialog.FileName;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // baca file
            Stream fileStream = File.OpenRead(tbFilename.Text);
            byte[] fileBuffer = new byte[fileStream.Length];
            fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
            fileStream.Close();

            // buka koneksi dan kirim
            TcpClient clientSocket = new TcpClient(tbServer.Text, 8080);
            NetworkStream networkStream = clientSocket.GetStream();
            networkStream.Write(fileBuffer, 0, fileBuffer.Length);
            networkStream.Close();
            clientSocket.Close();

            MessageBox.Show("File sent.");
        }
    }
}
