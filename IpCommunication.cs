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
        protected IPEndPoint _socketServer;
        public IPEndPoint SocketServer
        {
            set { _socketServer = value; }
            get { return _socketServer; }
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

    class UdpCommunication : IpCommunication
    {
        protected Thread _threadMessageListener;
        protected Thread _threadKeepalive;
        protected Thread _threadMessageSending;

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

        protected List<ControlledMessage> _messageSendingControlList = new List<ControlledMessage>();
        protected List<string> _messageReceivingControlList = new List<string>();

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
            _online = true;
            _threadMessageListener = new Thread(new ThreadStart(ReceiveMessage));
            _threadMessageListener.Name = "Listen Thread (ReceiveMessage method)";
            _threadMessageListener.Start();
            _threadKeepalive = new Thread(new ThreadStart(Keepalive));
            _threadKeepalive.Name = "Keepalive Thread (Keepalive method)";
            _threadKeepalive.Start();
            _threadMessageSending = new Thread(new ThreadStart(MessageTransmissionControl));
            _threadMessageSending.Name = "Message Ecoding Thread (MessageTransmissionControl method)";
            _threadMessageSending.Start();
            return true;
        }

        public void Stop()
        {
            if (_online)
            {
                _online = false;
                //_threadKeepalive.Abort();
                _threadKeepalive.Join();
                //_threadMessageListener.Abort(); // This caused some problems.
                _threadMessageListener.Join(); // Wait for ListenThread's next "am I online?" check.
                _client.Close();
            }
        }

        protected byte[] CreateAck(string hash)
        {
            byte[] ackTag = new byte[] { 0xCE }; // 206
            byte[] ackBytes = ByteHelper.ConcatinateArray(ackTag, Convert.FromBase64String(hash), ackTag);
            return ackBytes;
        }

        protected string OpenAck(byte[] bytes)
        {
            string ackHash = Convert.ToBase64String(bytes, 1, 20);
            return ackHash;
        }

        public void ReceiveMessage() // Threaded looping method.
        {
            while (_online)
            {
                try
                {
                    IPEndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = _client.Receive(ref remoteSender);
                    if (receivedBytes != null && receivedBytes.Length != 0)
                    {
                        if (!(receivedBytes[0] == 0xCE && receivedBytes[receivedBytes.Length - 1] == 0xCE))
                        {
                            // Send back an ACK message.
                            string hash = ByteHelper.GetHashString(receivedBytes);
                            byte[] ackBytes = CreateAck(hash);
                            _client.Send(ackBytes, ackBytes.Length, remoteSender);

                            // Check if the message is a duplicate.
                            if (!_messageReceivingControlList.Any(x => x == hash))
                            {
                                // Add the message's hash to a list so we don't react on the same message twice.
                                _messageReceivingControlList.Add(hash);
                                if (_messageReceivingControlList.Count > 5) // Only keep the latest 5 messages.
                                    _messageReceivingControlList.RemoveAt(0);

                                // Fire an MessageReceived event.
                                MessageReceivedEventArgs args = new MessageReceivedEventArgs();
                                args.Sender = remoteSender;
                                args.Data = receivedBytes;
                                OnMessageReceived(args);
                            }
                        }
                        else if (_messageSendingControlList.Count != 0)
                        {
                            // The received message is a ACK message.
                            string hash = OpenAck(receivedBytes);
                            ControlledMessage message = _messageSendingControlList.Find(x => x.Hash == hash);
                            _messageSendingControlList.Remove(message);
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

        public void SendMessage(object sender, MessageTransmissionEventArgs args)
        {
            ControlledMessage ctrlMessage = new ControlledMessage();
            if (args.Ip != null)
                ctrlMessage.Recipient = args.Ip;
            else
                ctrlMessage.Recipient = _socketServer;
            ctrlMessage.Data = args.MessageBytes;

            _messageSendingControlList.Add(ctrlMessage);
        }

        protected void MessageTransmissionControl() // Threaded method.
        {
            try
            {
                while (_online)
                {
                    CheckMessageControlList();
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadMessageSending.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
        }

        protected void CheckMessageControlList()
        {
            if (_messageSendingControlList.Count != 0)
            {
                List<ControlledMessage> temp = _messageSendingControlList.FindAll(x => (x.LastTry == null || (DateTime.Now - x.LastTry).TotalMilliseconds > 400) && x.Attempts < 5);
                foreach (ControlledMessage ctrlMessage in temp)
                {
                    if (SendControlledMessage(ctrlMessage))
                    {
                        ctrlMessage.LastTry = DateTime.Now;
                        ctrlMessage.Attempts++;
#if DEBUG                  
                        //_messageSendingControlList.Remove(ctrlMessage);
#endif
                    }
                }
            }
        }

        protected bool SendControlledMessage(ControlledMessage message)
        {
            try
            {
                _client.Send(message.Data, message.Data.Length, message.Recipient); // Send the message.
            }
            catch (SocketException ex)
            {
                throw;
                return false;
            }
            return true;
        }
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; set; }
        public byte[] Data { get; set; }
    }

    internal class ControlledMessage
    {
        public IPEndPoint Recipient { get; set; }
        public byte[] Data { get; set; }
        public string Hash { get { return ByteHelper.GetHashString(Data); } }
        public DateTime LastTry { get; set; }
        public int Attempts { get; set; }
    }
}
