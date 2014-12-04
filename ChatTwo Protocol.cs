using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ChatTwo_Server
{
    static class ChatTwo_Protocol
    {
        const byte _version = 0x00;

        private static Dictionary<int, int> _userSecrets = new Dictionary<int, int>(); 

        public enum MessageType
        {
            Login, // Login attempt.
            LoginReply, // Login attempt response.
            Status, // Tell server your online status and IP address.
            ContactRequest, // A request to make someone your contact.
            ContactRevoke, // Remove someone from your contacts.
            ContactStatus, // Tell client the online status and IP address of a contact.
            Message, // Message to another user.
            RelayMessage // Request for the server to relay a message to another user. Used if peer-to-peer fail?
        }

        public static bool ValidateSignature(byte[] bytes)
        {
            bool signatureValid = (bytes.Length >= 2 && bytes[0] == 0x92);

            bool macValid = false;
            if (signatureValid)
            {
                int mac = ByteHelper.ToInt32(bytes, 2);
                int messageHash = ByteHelper.SubArray(bytes, 6).GetHashCode();
                macValid = _userSecrets.Any(x => (messageHash.ToString() + x.Value.ToString()).GetHashCode() == mac);
            }

            return signatureValid && macValid;
        }

        public static byte[] AddSignature(byte[] bytes)
        {
            TimeSpan sinceMidnight = DateTime.Now - DateTime.Today;
            int timez = (int)sinceMidnight.TotalMilliseconds;
            bytes = ByteHelper.ConcatinateArray(BitConverter.GetBytes(timez), bytes); // Add a milisecond timestamp to the meassage.

            byte[] macBytes = BitConverter.GetBytes(bytes.GetHashCode());
            byte[] singatureBytes = new byte[] { 0x92, _version }; // Signature byte and version byte.
            
            bytes = ByteHelper.ConcatinateArray(singatureBytes, macBytes, bytes);
            return bytes;
        }

        public static byte[] RemoveSignature(byte[] bytes)
        {
            bytes = ByteHelper.SubArray(bytes, 10); // Remove the signature, the version number and the MAC.
            return bytes;
        }

        public static Message MessageReceivedHandler(MessageReceivedEventArgs args)
        {
            if (ValidateSignature(args.Data))
            {
                args.Data = ChatTwo_Protocol.RemoveSignature(args.Data);

                Message messageObj = new Message();
                messageObj.Sender = args.Sender;
                messageObj.Type = (MessageType)args.Data[0];
                messageObj.Data = args.Data;

                return messageObj;
            }
            else
                throw new NotImplementedException("Could not validate the received message.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
        }

        public static byte[] MessageTransmissionHandler(Message message)
        {
            byte[] textBytes = new byte[0];
            if (!String.IsNullOrEmpty(message.Text))
                textBytes = Encoding.Unicode.GetBytes(message.Text);

            byte[] messageBytes = ByteHelper.ConcatinateArray(new byte[] { (byte)message.Type }, message.Data, textBytes);
            messageBytes = ChatTwo_Protocol.AddSignature(textBytes); // Add the TAG and the version number.

            return messageBytes;
        }
    }

    static class ChatTwo_Client_Protocol
    {
        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

            IPEndPoint messageSender = message.Sender;
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


            // Fire an MessageReceived event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.To = message.Sender;
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

    static class ChatTwo_Server_Protocol
    {
        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            Message message = ChatTwo_Protocol.MessageReceivedHandler(args);

            IPEndPoint messageSender = message.Sender;
            byte[] messageData = message.Data;
            string messageText = message.Text;



        }

        public static void MessageTransmissionHandler(Message message)
        {



            byte[] messageBytes = ChatTwo_Protocol.MessageTransmissionHandler(message);


            // Fire an MessageReceived event.
            MessageTransmissionEventArgs args = new MessageTransmissionEventArgs();
            args.To = message.Sender;
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

    public class Message
    {
        public IPEndPoint Sender { get; set; }
        public ChatTwo_Protocol.MessageType Type { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
    }

    public class MessageTransmissionEventArgs : EventArgs
    {
        public IPEndPoint To { get; set; }
        public byte[] MessageBytes { get; set; }
    }
}
