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
        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            if (args.Data[0] == 0x92 && (ChatTwo_Protocol.MessageType)args.Data[ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.MacByteLength + 4] == ChatTwo_Protocol.MessageType.Login) // 26 (SignatureByteLength + MacByteLength + TimezByteLength) is the position of the Type byte.
            {
                UserObj user = DatabaseCommunication.LoginUser("Deantwo", "test");
                if (user == null)
                    return; // !?
#if DEBUG
                byte[] test = ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.MacByteLength);
#endif
                user.Secret = ByteHelper.GetHashString(ByteHelper.SubArray(args.Data, ChatTwo_Protocol.SignatureByteLength + ChatTwo_Protocol.MacByteLength));
                ChatTwo_Protocol.Users.Add(user);
            }
            Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

            IPEndPoint messageSender = message.Ip;
            ChatTwo_Protocol.MessageType type = message.Type;
            byte[] messageBytes = message.Data;

            byte[] messageData = new byte[0];
            string messageText = "";

            switch (type)
            {
                case ChatTwo_Protocol.MessageType.Login:
                    //messageData = ByteHelper.SubArray(args.Data, 0, 7);
                    messageText = Encoding.Unicode.GetString(ByteHelper.SubArray(messageBytes, 5));
#if DEBUG
                    MessageTestEventArgs newArgs = new MessageTestEventArgs();
                    newArgs.From = message.From;
                    newArgs.Ip = messageSender;
                    int milliseconds = ByteHelper.ToInt32(messageBytes, 0);
                    newArgs.Time = String.Format("{0}:{1}:{2}", (milliseconds / (60 * 60 * 1000)) % 24, (milliseconds / (60 * 1000)) % 60, (milliseconds / (1000)) % 60);
                    newArgs.Text = messageText;
                    OnMessageTest(newArgs);
#endif
                    break;
            }
        }

        public static void MessageTransmissionHandler(Message message)
        {


            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);


            // Fire an MessageReceived event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.Ip = ChatTwo_Protocol.Users.Find(x => x.ID == message.To).Socket;
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

#if DEBUG
        private static void OnMessageTest(MessageTestEventArgs e)
        {
            EventHandler<MessageTestEventArgs> handler = MessageTest;
            if (handler != null)
            {
                handler(null, e);
            }
        }
        public static event EventHandler<MessageTestEventArgs> MessageTest;
#endif
    }
    
#if DEBUG
    public class MessageTestEventArgs : EventArgs
    {
        public int From { get; set; }
        public IPEndPoint Ip { get; set; }
        public string Time { get; set; }
        public string Text { get; set; }
    }
#endif
}
