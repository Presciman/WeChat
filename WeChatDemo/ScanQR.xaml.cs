using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeChatTest.DAL;

namespace WeChatDemo
{
    /// <summary>
    /// ScanQR.xaml 的交互逻辑
    /// </summary>
    public partial class ScanQR : UserControl
    {
        public Action<List<string>> SetPicSource;
        readonly string connString = "Data Source=xuyuex.date,1433;Initial Catalog=StarrySkyDB;Persist Security Info=True;User ID=sa;Password=Nich0las";
        bool isLogin = false;
        string SceneStr = string.Empty;

        public ScanQR()
        {
            InitializeComponent();
            Loaded += ScanQR_Loaded;

        }
        private void ScanQR_Loaded(object sender, RoutedEventArgs e)
        {
            SceneStr = System.Configuration.ConfigurationManager.AppSettings["SceneStr"];


            if (!isLogin)
            {
                isLogin = true;

                InitSocket();
                GetQRCode();
                string loginMsg = "Login " + SceneStr;
                Send(loginMsg);
            }
        }



        private void GetQRCode()
        {
            try
            {
                string apiUri = "http://starrysky.xuyuex.date/api/Values/GetQRCodeLimit?sceneStr=" + SceneStr;

                HttpClient client = new HttpClient();
                var response = client.GetStreamAsync(apiUri);
                BitmapImage bit = new BitmapImage();
                bit.BeginInit();
                bit.StreamSource = response.Result;
                bit.EndInit();
                img.Source = bit;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void ShowThis()
        {
            this.Visibility = Visibility.Visible;
            GetQRCode();
        }


        private byte[] msgBuff = new byte[50];		// Receive data buffer
        private Socket clientSock;

        void InitSocket()
        {
            Connect("118.190.45.106", 8712);
        }

        /// <summary>
        /// Connect to the server, setup a callback to connect
        /// </summary>
        /// <param name="serverAdd">server ip address</param>
        /// <param name="port">port</param>
        public void Connect(string serverAdd, int port)
        {
            try
            {
                // Create the socket object
                clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Define the Server address and port
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(serverAdd), port);

                // Connect to server non-Blocking method
                clientSock.Blocking = false;

                // Setup a callback to be notified of connection success 
                clientSock.BeginConnect(epServer, new AsyncCallback(OnConnect), clientSock);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server Connect failed!");
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Callback used when a server accept a connection. 
        /// Setup to receive message
        /// </summary>
        /// <param name="ar"></param>
        public void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                //sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    Console.WriteLine("Unable to connect to remote machine", "Connect Failed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Unusual error during Connect!");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        /// <param name="app">socket used to receive</param>
        public void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(msgBuff, 0, msgBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Recieve callback setup failed! {0}", ex.Message);
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Callback used when receive data., both for server or client
        /// Note: If not data was recieved the connection has probably died.
        /// </summary>
        /// <param name="ar"></param>
        public void OnRecievedData(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            // Check if we got any data
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    // Get the received message 
                    string sRecieved = Encoding.ASCII.GetString(msgBuff, 0, nBytesRec);
                    // Process it
                    ProcessMessage(sock, sRecieved);

                    SetupRecieveCallback(sock);
                }
                else
                {
                    // If no data was recieved then the connection is probably dead
                    Console.WriteLine("disconnect from server {0}", sock.RemoteEndPoint);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Unusual error druing Recieve!");
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Parse the message process it 
        /// </summary>
        /// <param name="msg">received message</param>
        public void ProcessMessage(Socket sock, string msg)
        {
            Console.WriteLine(msg);
            string[] messages = msg.Split(' ');
            int count = messages.Length;
            if (count > 0)
            {
                string first = messages[0].ToLower();
                //if client want get a grid value 
                if (first == "openid" && (count == 2))
                {
                    Console.WriteLine(msg);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        string openid = messages[1];
                        openid = openid.Replace("\r\n", string.Empty);
                        GetData(openid);
                    }));

                    //string answer = GetGrid(int.Parse(messages[1]), int.Parse(messages[2])).ToString();
                    //answer = string.Format("Grid [{0}][{1}] ={2}", messages[1], messages[2], answer);
                    //Byte[] byteAnswer = System.Text.Encoding.ASCII.GetBytes(answer.ToCharArray());
                    // send back the value of corresponding grid 


                    //sock.Send(byteAnswer);
                }
                //else
                //{
                //    //if client want set a grid value
                //    if (first == "set" && (count == 4))
                //    {
                //        SetGrid(int.Parse(messages[1]), int.Parse(messages[2]), int.Parse(messages[3]));
                //    }
                //    else
                //    {
                //        Console.WriteLine(msg);
                //    }
                //}
            }
        }

        void GetData(string openid)
        {
            SQLHelper db = new SQLHelper(connString);

            var tmpList = db.GetPicListByOpenID(openid).ToList();
            if (tmpList.Count > 0)
            {
                SetPicSource(tmpList);
                //Grid parent = this.Parent as Grid;
                //parent.Children.Remove(this);
                this.Visibility = Visibility.Collapsed;
            }
            else
            {
                MessageBox.Show("您还未上传照片到公众号");
            }
        }

        /// <summary>
        /// Send message to client scoket
        /// </summary>
        /// <param name="msg">message</param>
        public void Send(string msg)
        {
            try
            {
                msg += "\r\n";
                Byte[] byteMsg = System.Text.Encoding.ASCII.GetBytes(msg.ToCharArray());
                clientSock.Send(byteMsg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Send("Login starrysky");
        }
    }
}
