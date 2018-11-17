using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChatRoom2
{
    class StateObject
    {
        public Socket wSocket { get; set; }
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();

    }
    class Server
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        int PORT = 9000;

        public string Name { get; set; }

        private string receivedMessage;

        private List<Socket> clients;
        public Server()
        {
            Name = "Server";
            clients = new List<Socket>();
        }
        public void Start()
        {
            //Getting ip from DNS
            IPHostEntry iPHostEntry= Dns.GetHostEntry("localhost");
            IPAddress ip = iPHostEntry.AddressList[0];
            IPEndPoint ipe = new IPEndPoint(ip, PORT);

            //Create a new Socket
            Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //Bind the server socket to the endpoint and then listen for clients
                server.Bind(ipe);
                server.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("WAiting for Client");
                    server.BeginAccept(AcceptCallback, server);
                    allDone.WaitOne();
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Exception here");
                Console.WriteLine(e.ToString());
                
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();

                Console.WriteLine("Client Connected");
                Socket server = (Socket)ar.AsyncState;
                Socket client = server.EndAccept(ar);

                clients.Add(client);

                StateObject state = new StateObject();
                state.wSocket = client;


                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
                
                
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
           
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.wSocket;

                int bytesReceived = client.EndReceive(ar);

                if (bytesReceived > 0)
                {
                    Console.WriteLine("Message Received");
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesReceived));
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);

                    if (state.sb.Length > 1)
                    {
                        receivedMessage = state.sb.ToString();
                        Console.WriteLine("Message processed. Sending to clients");
                        var data = Encoding.ASCII.GetBytes(receivedMessage);
                        foreach (var _client in clients)
                        {
                            _client.BeginSend(data, 0, data.Length, SocketFlags.None, sendCallback, _client);
                        }
                    }

                }
                else
                {
                    //if (state.sb.Length > 1)
                    //{
                    //    receivedMessage = state.sb.ToString();
                    //    Console.WriteLine("Message processed. Sending to clients");
                    //    var data = Encoding.ASCII.GetBytes(receivedMessage);
                    //    foreach (var _client in clients)
                    //    {
                    //        _client.BeginSend(data, 0, data.Length, SocketFlags.None, sendCallback, _client);
                    //    }
                    //}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine( e.ToString());
            }
        }

        private void sendCallback(IAsyncResult ar)
        {
            Socket _client = (Socket)ar.AsyncState;
            int bytes =_client.EndSend(ar);
            Console.WriteLine("Message Sent : {0}", bytes);
        }
    }
}
