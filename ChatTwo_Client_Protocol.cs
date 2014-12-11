using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ChatTwo_Server
{
    class ChatTwo_Client_Protocol
    {
        private static string _tempLoginSecret = "";
        public static string TempLoginSecret
        {
            get { return _tempLoginSecret; }
            set { _tempLoginSecret = value; }
        }

        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

            IPEndPoint messageSender = message.Ip;
            ChatTwo_Protocol.MessageType type = message.Type;
            byte[] messageBytes = message.Data;

            byte[] messageData = new byte[0];
            string messageText = "";

            switch (type)
            {
                case ChatTwo_Protocol.MessageType.Message:
                    messageData = ByteHelper.SubArray(args.Data, 0, 7);
                    messageText = Encoding.Unicode.GetString(ByteHelper.SubArray(messageBytes, 8));
                    break;
            }
        }

        public static void MessageTransmissionHandler(Message message)
        {


            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);



            // Fire an MessageTransmission event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.Ip = message.Ip;
            args.MessageBytes = messageBytes;
            OnMessageTransmission(args);
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
