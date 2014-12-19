using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ChatTwo_Server
{
    public static class ChatTwo_Protocol
    {
        const byte _version = 0x00;

        public const int SignatureByteLength = 2;

        public const int ServerReserrvedUserID = 0;

        public enum MessageType
        {
            CreateUser, // When a new user is joining the server,, creating a username and password.
            CreateUserReply, // Reply of success or failure of user creation.
            Login, // Login attempt.
            LoginReply, // Login attempt response.
            Status, // Tell server your online status and IP address. A form of keepalive.
            ContactRequest, // A request to make someone your contact.
            ContactRevoke, // Remove someone from your contacts.
            ContactStatus, // Tell client the online status and IP address of a contact.
            Message, // Message to another user.
            RelayMessage, // Request for the server to relay a message to another user. Used if peer-to-peer fail?
            Logout // Terminate all communication.
        }

        public static bool ValidateMac(byte[] bytes, string sharedSecret)
        {
            string mac = Convert.ToBase64String(bytes, 2, ByteHelper.HashByteLength);
#if DEBUG
            string test = CreateMac(ByteHelper.SubArray(bytes, SignatureByteLength + ByteHelper.HashByteLength), sharedSecret);
#endif
            bool macValid = CreateMac(ByteHelper.SubArray(bytes, SignatureByteLength + ByteHelper.HashByteLength), sharedSecret) == mac;
            return macValid;
        }

        public static byte[] AddSignatureAndMac(byte[] bytes, string sharedSecret)
        {
            TimeSpan sinceMidnight = DateTime.Now - DateTime.Today;
            int timez = (int)sinceMidnight.TotalMilliseconds;
            bytes = ByteHelper.ConcatinateArray(BitConverter.GetBytes(timez), bytes); // Add a milisecond timestamp to the meassage.

            byte[] macBytes = Convert.FromBase64String(CreateMac(bytes, sharedSecret));

            byte[] singatureBytes = new byte[] { 0x92, _version }; // Signature byte and version byte.

            bytes = ByteHelper.ConcatinateArray(singatureBytes, macBytes, bytes);
            return bytes;
        }

        public static byte[] RemoveSignatureAndMac(byte[] bytes)
        {
            bytes = ByteHelper.SubArray(bytes, SignatureByteLength + ByteHelper.HashByteLength); // Remove the signature, the version number and the MAC.
            return bytes;
        }

        private static string CreateMac(byte[] messageBytes, string sharedSecret)
        {
            return ByteHelper.GetHashString(ByteHelper.ConcatinateArray(ByteHelper.GetHashBytes(messageBytes), Convert.FromBase64String(sharedSecret)));
        }

        public static Message MessageReceivedHandler(MessageReceivedEventArgs args)
        {
            args.Data = ChatTwo_Protocol.RemoveSignatureAndMac(args.Data);

            Message messageObj = new Message();
            messageObj.Ip = args.Sender;
            int milliseconds = ByteHelper.ToInt32(args.Data, 0);
            messageObj.Timez = String.Format("{0}:{1}:{2}", (milliseconds / (60 * 60 * 1000)) % 24, (milliseconds / (60 * 1000)) % 60, (milliseconds / (1000)) % 60);
            messageObj.From = ByteHelper.ToInt32(args.Data, 4);
            messageObj.Type = (MessageType)args.Data[8];
            messageObj.Data = ByteHelper.SubArray(args.Data, 9);

            return messageObj;
        }

        public static byte[] MessageTransmissionHandler(Message message)
        {
            byte[] textBytes = new byte[0];
            if (!String.IsNullOrEmpty(message.Text))
                textBytes = Encoding.Unicode.GetBytes(message.Text);

            byte[] dataBytes = new byte[0];
            if (message.Data != null)
                dataBytes = message.Data;

            byte[] messageBytes = ByteHelper.ConcatinateArray(new byte[] { (byte)message.Type }, BitConverter.GetBytes(message.From));
            messageBytes = ByteHelper.ConcatinateArray(messageBytes, dataBytes, textBytes);

            return messageBytes;
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
        public int To { get; set; }
        public int From { get; set; }
        public IPEndPoint Ip { get; set; }
        public ChatTwo_Protocol.MessageType Type { get; set; }
        public string Timez { get; set; }
        public byte[] Data { get; set; }
        public string Text { get; set; }
    }

    public class MessageTransmissionEventArgs : EventArgs
    {
        public IPEndPoint Ip { get; set; }
        public byte[] MessageBytes { get; set; }
    }
}
