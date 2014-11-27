using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatTwo_Server
{
    class IpCommunication
    {
        protected bool _online;
        public bool Active
        {
            get { return _online; }
        }

        protected IPEndPoint _socketLocal;
        public IPEndPoint SocketLocal
        {
            set { _socketLocal = value; }
            get { return _socketLocal; }
        }
        protected IPEndPoint _socketRemote;
        public IPEndPoint SocketRemote
        {
            set { _socketRemote = value; }
            get { return _socketRemote; }
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            EventHandler<MessageReceivedEventArgs> handler = MessageReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; set; }
        public byte[] Data { get; set; }
    }

    class UdpCommunication : IpCommunication
    {
        protected Thread _threadMessageListener;
        protected Thread _threadKeepalive;

        protected UdpClient _client;
        public UdpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public int Port
        {
            get { return ((IPEndPoint)_client.Client.LocalEndPoint).Port; }
        }

        public bool Start(int serverPort)
        {
            try
            {
                _client = new UdpClient(serverPort);
                _client.Client.ReceiveTimeout = 1000;
                #region _client.Client.IOControl // Windows UDP Bugfix
                // http://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host
                const uint IOC_IN = 0x80000000;
                const uint IOC_VENDOR = 0x18000000;
                uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                _client.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                #endregion
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine("### Starting the UdpClient on port \"" + serverPort + "\" failed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                return false;
            }
            _threadMessageListener = new Thread(new ThreadStart(ReceiveMessage));
            _threadMessageListener.Name = "Listen Thread (ReceiveMessage method)";
            _threadMessageListener.Start();
            _threadKeepalive = new Thread(new ThreadStart(Keepalive));
            _threadKeepalive.Name = "Keepalive Thread (Keepalive method)";
            _threadKeepalive.Start();
            return true;
        }

        public void ReceiveMessage() // Threaded looping method.
        {
            while (_online)
            {
                try
                {
                    IPEndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = _client.Receive(ref remoteSender);
                    if (!(receivedBytes != null && receivedBytes.Length != 0))
                    {
                        // Start a UdpCommunicationReceiver.
                        if (ChatTwo_Protocol.ValidateSingature(receivedBytes))
                        {
                            if (receivedBytes.Length != 2) // Ignore empty messages. These are just keepalive messages?
                            {
                                Thread threadMessageDecoding;
                                threadMessageDecoding = new Thread(() => ReadingMessage(remoteSender, receivedBytes));
                                threadMessageDecoding.Name = "Message Decoding Thread (ReadingMessage method)";
                                threadMessageDecoding.Start();
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("--- Unknown message received:");
                            System.Diagnostics.Debug.WriteLine("--- # 0x" + ByteHelper.ToHex(receivedBytes) + " #");
                            System.Diagnostics.Debug.WriteLine("--- From " + remoteSender.Address.ToString() + ":" + remoteSender.Port.ToString());
                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        System.Diagnostics.Debug.WriteLine("### " + _threadMessageListener.Name + " has crashed:");
                        System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                        System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                        break;
                    }
                    else
                        continue;
                }
            }
        }

        protected void ReadingMessage(IPEndPoint sender, byte[] messageBytes) // Threaded (threaded) method.
        {
            

            MessageReceivedEventArgs args = new MessageReceivedEventArgs();
            args.Sender = sender;
            args.Data = messageBytes;
            OnMessageReceived(args);
        }

        protected void Keepalive() // Threaded looping method.
        {
            try
            {
                while (_online)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadMessageListener.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
        }

        public void Close()
        {
            if (_online)
            {
                _threadKeepalive.Join();
                //_listenThread.Abort(); // This caused some problems.
                _threadMessageListener.Join(); // Wait for ListenThread's next "am I online?" check.
                _client.Close();
            }
        }

        public void SendMessage(string message = "", byte[] data = null)
        {
            Thread threadMessageSending;
            threadMessageSending = new Thread(() => ThreadedSendMessage(message, data));
            threadMessageSending.Name = "Message Ecoding Thread (SendMessage method)";
            threadMessageSending.Start();
        }

        protected void ThreadedSendMessage(string message = "", byte[] data = null) // Threaded method.
        {
            byte[] transmittedBytes = new byte[0];

            byte dataLength = 0;
            if (data != null && data.Length != 0)
                dataLength = (byte)data.Length;

            if (dataLength != 0 || !String.IsNullOrEmpty(message))
            {
                transmittedBytes = ByteHelper.ConcatinateArray(transmittedBytes, new byte[] { dataLength }); // Add dataLength byte.

                if (dataLength != 0)
                    transmittedBytes = ByteHelper.ConcatinateArray(transmittedBytes, data); // Add data.

                if (!String.IsNullOrEmpty(message))
                {
                    byte[] messageBytes = new byte[0];
                    messageBytes = System.Text.Encoding.Unicode.GetBytes(message); // Convert message to bytes.
                    transmittedBytes = ByteHelper.ConcatinateArray(transmittedBytes, messageBytes); // Add messageBytes.
                }
            }

            // Concatinate all the byte arrays together. (TAG . version . dataLength . dataBytes . messageBytes)
            transmittedBytes = ChatTwo_Protocol.AddSingature(transmittedBytes);

            _client.Send(transmittedBytes, transmittedBytes.Length, _socketRemote); // Send the message.
        }
    }
}
