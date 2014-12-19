using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ChatTwo_Server
{
    class ChatTwo_Server_Protocol
    {
        private static List<UserObj> _users = new List<UserObj>();
        public static List<UserObj> Users
        {
            get { return _users; }
            set { _users = value; }
        }

        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            if (args.Data[0] == 0x92)
            {
                string sharedSecret;
                // Position of the Type byte is 30 (SignatureByteLength + MacByteLength + TimezByteLength + UserIdByteLength).
                ChatTwo_Protocol.MessageType type = (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4 + 4];
                if (type == ChatTwo_Protocol.MessageType.CreateUser)
                {
                    sharedSecret = "5ny1mzFo4S6nh7hDcqsHVg+DBNU="; // Default hardcoded sharedSecret.
                }
                if (type == ChatTwo_Protocol.MessageType.Login)
                {
#if DEBUG
                    byte[] test = ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength);
#endif
                    sharedSecret = ByteHelper.GetHashString(ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength));
                }
                else
                {
                    // Position of the UserID bytes is 26 (SignatureByteLength + MacByteLength + TimezByteLength + UserIdByteLength) with a length of 4.
                    int userId = ByteHelper.ToInt32(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4);
                    sharedSecret = _users.Find(x => x.ID == userId).Secret;
                }


                if (ChatTwo_Protocol.ValidateMac(args.Data, sharedSecret))
                {
                    Message message = ChatTwo_Protocol.MessageReceivedHandler(args);
                    IPEndPoint messageSender = message.Ip;
                    byte[] messageBytes = message.Data;

                    switch (message.Type)
                    {
                        case ChatTwo_Protocol.MessageType.CreateUser:
                            {
                                string passwordHash = Convert.ToBase64String(message.Data, 0, ByteHelper.HashByteLength);
                                string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, ByteHelper.HashByteLength));
                                bool worked = DatabaseCommunication.CreateUser(username, passwordHash);
                                if (worked)
                                {
                                    // Uesr creation worked!
                                    MessageToIp(message.Ip, ChatTwo_Protocol.MessageType.CreateUserReply, new byte[] { 0x00 });
                                }
                                else
                                {
                                    // Some error prevented the user from being created. Best guess is that a user with that name already exist.
                                    MessageToIp(message.Ip, ChatTwo_Protocol.MessageType.CreateUserReply, new byte[] { 0x01 });
                                }
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.Login:
                            {
                                string passwordHash = Convert.ToBase64String(message.Data, 0, ByteHelper.HashByteLength);
                                string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, ByteHelper.HashByteLength));
                                UserObj user = DatabaseCommunication.LoginUser(username, passwordHash);
                                if (user == null)
                                {
                                    // Have to send back a LoginReply message here with a "wrong username/password" error.
                                    MessageToIp(message.Ip, ChatTwo_Protocol.MessageType.LoginReply, new byte[] { 0x01 });
                                    return;
                                }
                                user.Secret = sharedSecret;
                                user.Socket = message.Ip;
                                _users.Add(user);
                                MessageToUser(user.ID, ChatTwo_Protocol.MessageType.LoginReply, ByteHelper.ConcatinateArray(new byte[] { 0x00 }, BitConverter.GetBytes(user.ID)));
                                DatabaseCommunication.UpdateUser(user.ID, user.Socket);
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.Status:
                            {
                                UserObj user = _users[0]; // HOW!!!!!!!!!!!!!!!!!!!!??!??!?!?!?!??!?!?!?!??!?!?!?!?!?!!!!!?11?
                                if (user.Socket != message.Ip)
                                {
                                    // Message all contacts of the user with the new IP change!!!
                                    user.Socket = message.Ip;
                                }
                                DatabaseCommunication.UpdateUser(user.ID, user.Socket);
                                break;
                            }
                    }
                }
                else
                    throw new NotImplementedException("Could not validate the MAC of the received message.");
                    // Need to add a simple debug message here, but this works as a great breakpoint until then.
            }
            else
                throw new NotImplementedException("Could not validate the signature of the received message. The signature was \"0x" + args.Data[0] + "\" but only \"0x92\" is allowed.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
        }

        public static void MessageToIp(IPEndPoint toIp, ChatTwo_Protocol.MessageType type, byte[] data = null, string text = null)
        {
            Message message = new Message();
            message.From = ChatTwo_Protocol.ServerReserrvedUserID;
            message.Type = type;
            if (data != null && data.Length != 0)
                message.Data = data;
            if (!String.IsNullOrEmpty(text))
                message.Text = text;
            message.Ip = toIp;
            MessageTransmissionHandler(message);
        }

        public static void MessageToUser(int to, ChatTwo_Protocol.MessageType type, byte[] data = null, string text = null)
        {
            Message message = new Message();
            message.From = ChatTwo_Protocol.ServerReserrvedUserID;
            message.To = to;
            message.Type = type;
            if (data != null && data.Length != 0)
                message.Data = data;
            if (!String.IsNullOrEmpty(text))
                message.Text = text;
            if (_users.Any(x => x.ID == to))
                message.Ip = _users.Find(x => x.ID == to).Socket;
            MessageTransmissionHandler(message);
        }

        public static void MessageTransmissionHandler(Message message)
        {
            string sharedSecret;
            if (message.Type == ChatTwo_Protocol.MessageType.CreateUserReply)
            {
                sharedSecret = "5ny1mzFo4S6nh7hDcqsHVg+DBNU="; // Default hardcoded sharedSecret.
            }
            else
            {
                sharedSecret = _users.Find(x => x.ID == message.To).Secret;
            }

            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);

            messageBytes = ChatTwo_Protocol.AddSignatureAndMac(messageBytes, sharedSecret);

            // Fire an OnMessageReceived event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.Ip = _users.Find(x => x.ID == message.To).Socket;
            args.MessageBytes = messageBytes;
            OnMessageTransmission(args);
        }

        public static void TellUserAboutContactstatusChange(object sender, OnUserStatusChangeEventArgs args)
        {
            Message message = new Message() { To = args.TellId, Type = ChatTwo_Protocol.MessageType.ContactStatus, Data = new byte[] { (byte)args.IdIs, Convert.ToByte(args.Online) } };

            MessageTransmissionHandler(message);
        }

        private static void OnMessageTransmission(MessageTransmissionEventArgs e)
        {
            EventHandler<MessageTransmissionEventArgs> handler = MessageTransmission;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<MessageTransmissionEventArgs> MessageTransmission;
    }
}
