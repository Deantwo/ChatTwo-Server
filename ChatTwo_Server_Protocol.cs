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
                ChatTwo_Protocol.MessageType type = (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.HashByteLength + 4];
                if (type == ChatTwo_Protocol.MessageType.CreateUser)
                {
                    sharedSecret = "5ny1mzFo4S6nh7hDcqsHVg+DBNU="; // Default hardcoded sharedSecret.
                }
                if (type == ChatTwo_Protocol.MessageType.Login) // 26 (SignatureByteLength + MacByteLength + TimezByteLength) is the position of the Type byte.
                {
#if DEBUG
                    byte[] test = ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.HashByteLength);
#endif
                    sharedSecret = ByteHelper.GetHashString(ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.HashByteLength));
                }
                else
                    sharedSecret = ""; //?!?!?!?!



                if (ChatTwo_Protocol.ValidateMac(args.Data, sharedSecret))
                {
                    Message message = ChatTwo_Protocol.MessageReceivedHandler(args);
                    IPEndPoint messageSender = message.Ip;
                    byte[] messageBytes = message.Data;

                    switch (message.Type)
                    {
                        case ChatTwo_Protocol.MessageType.CreateUser:
                            {
                                string passwordHash = Convert.ToBase64String(message.Data, 0, ChatTwo_Protocol.HashByteLength);
                                string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, ChatTwo_Protocol.HashByteLength));
                                bool worked = DatabaseCommunication.CreateUser(username, passwordHash);
                                if (worked)
                                {
                                    // Have to send back a CreateUserReply here.
                                }
                                else
                                {
                                    // Have to send back a CreateUserReply with a "username already exist" error here.
                                }
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.Login:
                            {
                                string passwordHash = Convert.ToBase64String(message.Data, 0, ChatTwo_Protocol.HashByteLength);
                                string username = Encoding.Unicode.GetString(ByteHelper.SubArray(message.Data, ChatTwo_Protocol.HashByteLength));
                                UserObj user = DatabaseCommunication.LoginUser(username, passwordHash);
                                if (user == null)
                                    throw new NotImplementedException("Username or password was not correct for \"" + username + "\".");
                                // Have to send back a LoginReply message here with a "wrong username/password" error.
                                user.Secret = sharedSecret;
                                user.Socket = message.Ip;
                                DatabaseCommunication.UpdateUser(user.ID, user.Socket);
                                _users.Add(user);
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

        public static void MessageTransmissionHandler(Message message)
        {
            string sharedSecret;
            if (message.Type == ChatTwo_Protocol.MessageType.CreateUserReply)
            {
                sharedSecret = "5ny1mzFo4S6nh7hDcqsHVg+DBNU="; // Default hardcoded sharedSecret.
            }
            else if (message.Type == ChatTwo_Protocol.MessageType.LoginReply)
            {
                sharedSecret = _users.Find(x => x.ID == message.To).Secret;
            }
            else
                sharedSecret = ""; //?!?!?!?!

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
