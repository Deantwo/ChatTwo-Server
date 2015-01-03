﻿using System;
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

        private static Dictionary<IPEndPoint, string> _tempUsers = new Dictionary<IPEndPoint, string>();

        public static void MessageReceivedHandler(object sender, PacketReceivedEventArgs args)
        {
            if (!DatabaseCommunication.Active)
                throw new NotImplementedException("Database connection was not active and a reply for this have not been implemented yet.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
                // Also need to make some kind of error message I can send back to the client.

            if (args.Data[0] == 0x92 )
            {
                string sharedSecret;
                // Position of the Type byte is 30 (SignatureByteLength + MacByteLength + TimezByteLength + UserIdByteLength).
                ChatTwo_Protocol.MessageType type = (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4 + 4];
                if (type == ChatTwo_Protocol.MessageType.CreateUser)
                {
                    sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
                }
                else if (type == ChatTwo_Protocol.MessageType.Login)
                {
#if DEBUG
                    byte[] test = ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4);
#endif
                    // Don't take the Timez as part of the sharedSecret. This is mostly because of a problem I have in the client where I make the sharedSecrt before I add the Timez.
                    sharedSecret = ByteHelper.GetHashString(ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4));
                }
                else
                {
                    // Position of the UserID bytes is 26 (SignatureByteLength + MacByteLength + TimezByteLength) with a length of 4.
                    int userId = ByteHelper.ToInt32(args.Data, ChatTwo_Protocol.SignatureByteLength + ByteHelper.HashByteLength + 4);
                    sharedSecret = _users.Find(x => x.ID == userId).Secret;
                }


                if (ChatTwo_Protocol.ValidateMac(args.Data, sharedSecret))
                {
                    Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

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
                                    _tempUsers.Add(message.Ip, sharedSecret);
                                    MessageToIp(message.Ip, ChatTwo_Protocol.MessageType.LoginReply, new byte[] { 0x01 });
                                    return;
                                }
                                if (_users.Any(x => x.ID == user.ID))
                                {
                                    // Have to send back a LoginReply message here with a "User is already online" error.
                                    _tempUsers.Add(message.Ip, sharedSecret);
                                    MessageToIp(message.Ip, ChatTwo_Protocol.MessageType.LoginReply, new byte[] { 0x02 });
                                    return;
                                }
                                user.Secret = sharedSecret;
                                user.Socket = message.Ip;
                                _users.Add(user);
                                MessageToUser(user.ID, ChatTwo_Protocol.MessageType.LoginReply, ByteHelper.ConcatinateArray(new byte[] { 0x00 }, BitConverter.GetBytes(user.ID)), user.Name);
                                DatabaseCommunication.UpdateUser(user.ID, user.Socket);
                                break;
                            }
                        case ChatTwo_Protocol.MessageType.Status:
                            {
                                UserObj user = _users.Find(x => x.ID == message.From);
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
                sharedSecret = ChatTwo_Protocol.DefaultSharedSecret;
            }
            else if (message.Type == ChatTwo_Protocol.MessageType.LoginReply && message.To == 0)
            {
                // Not sure I like the idea of having "_tempUser", but it works for now I guess?
                // It could cause problems if multiple login attempts are made from the same client in rapid succession.
                sharedSecret = _tempUsers[message.Ip];
                _tempUsers.Remove(message.Ip);
            }
            else
            {
                sharedSecret = _users.Find(x => x.ID == message.To).Secret;
            }

            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);

            messageBytes = ChatTwo_Protocol.AddSignatureAndMac(messageBytes, sharedSecret);

            // Fire an OnMessageTransmission event.
            PacketTransmissionEventArgs args = new PacketTransmissionEventArgs();
            args.Destination = message.Ip;
            args.PacketContent = messageBytes;
            OnMessageTransmission(args);
        }

        public static void TellUserAboutContactstatusChange(object sender, UserStatusChangeEventArgs args)
        {
            byte[] dataBytes = ByteHelper.ConcatinateArray(BitConverter.GetBytes(args.IdIs), new byte[] { Convert.ToByte(args.Online) });
            if (args.Online)
            {
                IPEndPoint socket;
                if (args.Socket != null)
                    socket = args.Socket;
                else if (_users.Any(x => x.ID == args.IdIs))
                    socket = _users.Find(x => x.ID == args.IdIs).Socket;
                else
                    // This shouldn't really happen. I should make the server simply manage online status and sockets only in the memory, and not in the database.
                    throw new NotImplementedException("Could not find a socket for the user[" + args.IdIs + "].");
                // 0x01 for UDP only.
                byte[] socketBytes = ByteHelper.ConcatinateArray(new byte[] {0x01}, BitConverter.GetBytes(args.Socket.Port), args.Socket.Address.GetAddressBytes());
                dataBytes = ByteHelper.ConcatinateArray(dataBytes, socketBytes);
            }
            MessageToUser(args.TellId, ChatTwo_Protocol.MessageType.ContactStatus, dataBytes);
        }

        private static void OnMessageTransmission(PacketTransmissionEventArgs e)
        {
            EventHandler<PacketTransmissionEventArgs> handler = MessageTransmission;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<PacketTransmissionEventArgs> MessageTransmission;
    }
}
