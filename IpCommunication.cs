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

        protected virtual void OnPacketReceived(PacketReceivedEventArgs e)
        {
            EventHandler<PacketReceivedEventArgs> handler = PacketReceived;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }

    class UdpCommunication : IpCommunication
    {
        protected Thread _threadPacketListener;
        protected Thread _threadPacketSending;

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

        protected List<ControlledPacket> _messageSendingControlList = new List<ControlledPacket>();
        protected List<string> _messageReceivingControlList = new List<string>();

        /// <summary>
        /// Creates a temp UdpClient and sends an EtherConnectionTest packet to the target address.
        /// </summary>
        /// <param name="address">Target address for the EtherConnectionTest packet.</param>
        public static bool TestPortforward(IPEndPoint address)
        {
            //// Check if the port number is in use.
            //bool isInUse = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(p => p.Port == port);

            // Rather than just checking if the portnumber is in use, which only causes "new UdpClient(port)" to fail, I want to test if the server can hit it self by pinging the external IP address.
            try
            {
                using (UdpClient tempClient = new UdpClient(0))
                {
                    #region tempClient.Client.IOControl // Windows UDP Bugfix
                    // This is a fix to make the UdpClient ignore some weird behavior from Windows.
                    // Read more here: http://stackoverflow.com/a/7478498
                    const uint IOC_IN = 0x80000000;
                    const uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    tempClient.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
                    #endregion
                    byte[] messageBytes = new byte[] { 0xEC };
                    tempClient.Send(messageBytes, messageBytes.Length, address); // Send the message.
                }
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine("### An error happened when trying to send out an EtherConnectionTest packet:"); // Called it "EtherConnection" because 0xEC was a nice hex value.
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                return false;
            }
            return true;
        }

        protected void OnEtherConnectionReply(EventArgs e)
        {
            EventHandler<EventArgs> handler = EtherConnectionReply;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<EventArgs> EtherConnectionReply;

        /// <summary>
        /// Starts the UdpClient and the threaded methods.
        /// </summary>
        /// <param name="serverPort">Port number the UdpClient should use. 0 (zero) will let the OS choose a random port.</param>
        public bool Start(int serverPort)
        {
            try
            {
                _client = new UdpClient(serverPort);
                _client.Client.ReceiveTimeout = 1000; // This causes the _client.Receive(ref remoteSender) methtod to actually timeout, else it would simply freeze the _threadPacketListener thread.
                #region _client.Client.IOControl // Windows UDP Bugfix
                // This is a fix to make the UdpClient ignore some weird behavior from Windows.
                // Read more here: http://stackoverflow.com/a/7478498
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
            _threadPacketListener = new Thread(new ThreadStart(ReceivePacket));
            _threadPacketListener.Name = "Packet Listening Thread (ReceivePacket method)";
            _threadPacketListener.Start();
            _threadPacketSending = new Thread(new ThreadStart(PacketTransmissionControl));
            _threadPacketSending.Name = "Packet Sending Thread (PacketTransmissionControl method)";
            _threadPacketSending.Start();
            return true;
        }

        /// <summary>
        /// Stops all threaded methods and stops the UdpClient. Use this before closing the application!
        /// </summary>
        public void Stop()
        {
            if (_online)
            {
                _online = false;
                //_threadPacketListener.Abort(); // This caused some problems.
                _threadPacketListener.Join(); // Wait for _threadPacketListener's next "am I online?" check.
                //_threadPacketSending.Abort(); // This caused some problems.
                _threadPacketSending.Join(); // Wait for _threadPacketSending's next "am I online?" check.
                _client.Close();
            }
        }

        /// <summary>
        /// Create an ACK packet from a base64 hash string.
        /// </summary>
        /// <param name="hash">Base64 hash string to be used.</param>
        protected byte[] CreateAck(string hash)
        {
            byte[] ackTag = new byte[] { 0xCE }; // 0xCE = 206
            byte[] ackBytes = ByteHelper.ConcatinateArray(ackTag, Convert.FromBase64String(hash), ackTag);
            return ackBytes;
        }

        /// <summary>
        /// Convert an ACK packet to a base64 hash string.
        /// </summary>
        /// <param name="packetBytes">ACK packet's byte content.</param>
        protected string OpenAck(byte[] packetBytes)
        {
            string ackHash = Convert.ToBase64String(packetBytes, 1, ByteHelper.HashByteLength);
            return ackHash;
        }

        /// <summary>
        /// This is a threaded method that keeps looping while _online is true.
        /// It will receive UDP messages on the UdpClient's port number and forward them to the OnPacketReceived event.
        /// </summary>
        public void ReceivePacket() // Threaded looping method.
        {
            while (_online)
            {
                try
                {
                    IPEndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] receivedBytes = _client.Receive(ref remoteSender);
                    if (receivedBytes != null && receivedBytes.Length != 0)
                    {
                        if (receivedBytes.Length == ByteHelper.HashByteLength + 2 && receivedBytes[0] == 0xCE && receivedBytes[receivedBytes.Length - 1] == 0xCE)
                        {
                            if (_messageSendingControlList.Count != 0)
                            {
                                // The received message is a ACK message.
                                string hash = OpenAck(receivedBytes);
                                ControlledPacket packet = _messageSendingControlList.Find(x => x.Hash == hash);
                                if (packet != null)
                                    _messageSendingControlList.Remove(packet);
                            }
                        }
                        else if (receivedBytes.Length == 1 && receivedBytes[0] == 0xEC)
                        {
                            // Fire an OnEtherConnectionReply event.
                            OnEtherConnectionReply(null);
                        }
                        else
                        {
                            // Send back an ACK packet.
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

                                // Fire an OnPacketReceived event.
                                PacketReceivedEventArgs args = new PacketReceivedEventArgs();
                                args.Sender = remoteSender;
                                args.Data = receivedBytes;
                                OnPacketReceived(args);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.TimedOut)
                    {
                        System.Diagnostics.Debug.WriteLine("### " + _threadPacketListener.Name + " has crashed:");
                        System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                        System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                        break;
                    }
                    else
                        continue;
                }
            }
        }

        /// <summary>
        /// Send a packet to a target IP address.
        /// </summary>
        /// <param name="sender">Default object parameter for event receiving methods. Unused here.</param>
        /// <param name="args">PacketTransmissionEventArgs object containing the byte array to be send and the destination IP address.</param>
        public void SendPacket(object sender, PacketTransmissionEventArgs args)
        {
            ControlledPacket ctrlPacket = new ControlledPacket();
            ctrlPacket.Recipient = args.Destination;
            ctrlPacket.Data = args.PacketContent;

            _messageSendingControlList.Add(ctrlPacket);
        }

        /// <summary>
        /// This is a threaded method that keeps looping while _online is true.
        /// It will check the _packetSendingControlList list and try to send all packets on the list 5 times per second.
        /// </summary>
        protected void PacketTransmissionControl() // Threaded looping method.
        {
            try
            {
                while (_online)
                {
                    CheckPacketControlList();
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("### " + _threadPacketSending.Name + " has crashed:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
            }
        }

        protected void CheckPacketControlList()
        {
            if (_messageSendingControlList.Count != 0)
            {
                List<ControlledPacket> temp = _messageSendingControlList.FindAll(x => (x.LastTry == null || (DateTime.Now - x.LastTry).TotalMilliseconds > 400) && x.Attempts < 5);
                foreach (ControlledPacket ctrlPacket in temp)
                {
                    if (SendControlledPacket(ctrlPacket))
                    {
                        ctrlPacket.LastTry = DateTime.Now;
                        ctrlPacket.Attempts++;
#if !DEBUG
                        if (ctrlPacket.Attempts == 5)
                            _messageSendingControlList.Remove(ctrlPacket);
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to send the controlled packet. Return true if successful.
        /// </summary>
        /// <param name="ctrlPacket">Packet to be sent.</param>
        protected bool SendControlledPacket(ControlledPacket ctrlPacket)
        {
            try
            {
                _client.Send(ctrlPacket.Data, ctrlPacket.Data.Length, ctrlPacket.Recipient); // Send the packet.
            }
            catch (SocketException ex)
            {
#if DEBUG
                throw;
#else
                System.Diagnostics.Debug.WriteLine("### An error happened when trying to send ControlledPacket:");
                System.Diagnostics.Debug.WriteLine("### " + ex.Message);
                System.Diagnostics.Debug.WriteLine("### " + ex.ToString());
                return false;
#endif
            }
            return true;
        }
    }

    public class PacketReceivedEventArgs : EventArgs
    {
        public IPEndPoint Sender { get; set; }
        public byte[] Data { get; set; }
    }

    public class PacketTransmissionEventArgs : EventArgs
    {
        public IPEndPoint Destination { get; set; }
        public byte[] PacketContent { get; set; }
    }

    internal class ControlledPacket
    {
        public IPEndPoint Recipient { get; set; }
        public byte[] Data { get; set; }
        public string Hash { get { return ByteHelper.GetHashString(Data); } }
        public DateTime LastTry { get; set; }
        public int Attempts { get; set; }
    }
}
