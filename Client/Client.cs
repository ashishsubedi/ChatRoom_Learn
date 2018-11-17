using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class StateObject
    {
        public Socket wSocket { get; set; }
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
       

    }
    class Client
    {
        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        int PORT = 9000;

        public string Name { get; set; }
        
        private string receivedMessage;


        public Client( string name)
        {
            this.Name = name;
        }

        public void Start()
        {
            //Getting ip from DNS
            IPHostEntry iPHostEntry = Dns.GetHostEntry("localhost");
            IPAddress ip = iPHostEntry.AddressList[0];
            IPEndPoint ipe = new IPEndPoint(ip, PORT);

            Socket client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
              

                client.BeginConnect(ipe, ConnectCallback, client);
                connectDone.WaitOne();

                while (true)
                {
                    StartChat(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

            }
            finally
            {

                client.Close();
            }

        }

        private void StartChat(Socket client)
        {
            Console.WriteLine("{0} :" , this.Name);
            StringBuilder msg = new StringBuilder(Name);
            msg.Append(">>>");
            string temp = Console.ReadLine();
            msg.Append(temp);

            Send(client,msg.ToString());
           // sendDone.WaitOne();

            Receive(client);
            //receiveDone.WaitOne();
        }

        private void Receive(Socket client)
        {
            try
            {
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
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesReceived));
                    //client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, ReceiveCallback, state);
                    receivedMessage = state.sb.ToString();
                    Console.WriteLine(receivedMessage);
                }
                else
                {
                    if (state.sb.Length > 1)
                    {
                        Console.WriteLine("WOW");
                    }
                }
               // receiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send(Socket client, string data)
        {
            try
            {
                //byte[] buffer = Encoding.ASCII.GetBytes(data);
                StateObject state = new StateObject();
                state.wSocket = client;
                state.buffer = Encoding.ASCII.GetBytes(data);
                

                client.BeginSend(state.buffer, 0, state.buffer.Length,SocketFlags.None,SendCallback, state);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = ((StateObject)ar.AsyncState).wSocket;
                int bytesSent = client.EndSend(ar);

               // sendDone.Set();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);
                Console.WriteLine("Connected to server");
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }


        }
    }
}
