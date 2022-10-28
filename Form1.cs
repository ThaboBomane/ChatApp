using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ChatApp
{
    public partial class Form1 : Form
    {

        Socket sck;
        EndPoint epLocal, epRemote;
        byte[] buffer;



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // set up socket
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // get own IP
            textLocalIp.Text = GetLocalIP();
            textRemoteIp.Text = GetLocalIP();

        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // binding socket
                epLocal = new IPEndPoint(IPAddress.Parse(textLocalIp.Text),
                Convert.ToInt32(textLocalPort.Text));
                sck.Bind(epLocal);
                // connect to remote IP and port
                epRemote = new IPEndPoint(IPAddress.Parse(textRemoteIp.Text),
                Convert.ToInt32(textRemotePort.Text));
                sck.Connect(epRemote);
                // starts to listen to an specific port
                buffer = new byte[1500];
                sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                // release button to send message
                buttonSend.Enabled = true;
                buttonConnect.Text = "Connected";
                buttonConnect.Enabled = false;
                textMessage.Focus();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }


        }

        // Return your own IP
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private void buttonSend_Click(object sender, EventArgs e)
        
        {
            // converts from string to byte[]
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] msg = new byte[1500];
            msg = enc.GetBytes(textMessage.Text);

            // sending the message
            sck.Send(msg);

            // add to listbox
            listMessage.Items.Add("Me: " + textMessage.Text);
            textMessage.Text = "";
        }

        private void MessageCallBack(IAsyncResult aResult)
        {
            try
            {
                int size = sck.EndReceiveFrom(aResult, ref epRemote);
                // check if theres actually information
                if (size > 0)
                {
                    byte[] receivedData = new byte[1500];
                    receivedData = (byte[])aResult.AsyncState;
                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);
                    // adding Message to the listbox
                    listMessage.Items.Add("Friend: " + receivedMessage);

                    // starts to listen the socket again
                    buffer = new byte[1500];
                    sck.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref epRemote, new AsyncCallback(MessageCallBack), buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}


