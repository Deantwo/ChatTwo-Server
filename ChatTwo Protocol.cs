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

        public const int MacByteLength = 20;
        public const int SignatureByteLength = 2;

        private static List<UserObj> _users = new List<UserObj>();
        public static List<UserObj> Users
        {
            get { return _users; }
            set { _users = value; }
        }

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
            string mac = Convert.ToBase64String(bytes, 2, MacByteLength);
#if DEBUG
            string test = CreateMac(ByteHelper.SubArray(bytes, SignatureByteLength + MacByteLength), _users[0].Secret);
#endif
            bool macValid = _users.Any(x => CreateMac(ByteHelper.SubArray(bytes, SignatureByteLength + MacByteLength), x.Secret) == mac);
            return macValid;
        }

        public static byte[] AddSignature(byte[] bytes, int to)
        {
            TimeSpan sinceMidnight = DateTime.Now - DateTime.Today;
            int timez = (int)sinceMidnight.TotalMilliseconds;
            bytes = ByteHelper.ConcatinateArray(BitConverter.GetBytes(timez), bytes); // Add a milisecond timestamp to the meassage.

            byte[] macBytes;
            if ((MessageType)bytes[4] == MessageType.Login)
            {
                ChatTwo_Client_Protocol.TempLoginSecret = ByteHelper.GetHashString(bytes);
                macBytes = Convert.FromBase64String(CreateMac(bytes, ChatTwo_Client_Protocol.TempLoginSecret));
            }
            else
                macBytes = Convert.FromBase64String(CreateMac(bytes, _users.Find(x => x.ID == to).Secret));
            
            byte[] singatureBytes = new byte[] { 0x92, _version }; // Signature byte and version byte.
            
            bytes = ByteHelper.ConcatinateArray(singatureBytes, macBytes, bytes);
            return bytes;
        }

        public static byte[] RemoveSignature(byte[] bytes)
        {
            bytes = ByteHelper.SubArray(bytes, SignatureByteLength + MacByteLength); // Remove the signature, the version number and the MAC.
            return bytes;
        }

        private static string CreateMac(byte[] messageBytes, string sharedSecret)
        {
            return ByteHelper.GetHashString(ByteHelper.ConcatinateArray(ByteHelper.GetHashBytes(messageBytes), Convert.FromBase64String(sharedSecret)));
        }

        public static Message MessageReceivedHandler(MessageReceivedEventArgs args)
        {
            if (args.Data[0] == 0x92 && ValidateSignature(args.Data))
            {
                args.Data = ChatTwo_Protocol.RemoveSignature(args.Data);

                Message messageObj = new Message();
                messageObj.Ip = args.Sender;
                messageObj.Type = (MessageType)args.Data[4];
                messageObj.Data = args.Data;

                return messageObj;
            }
            else
#if DEBUG
                return new Message() { From = 0, Ip = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 9020), Data = Encoding.Unicode.GetBytes("test failed! I love Valoree ♥") };
#else
                throw new NotImplementedException("Could not validate the received message.");
                // Need to add a simple debug message here, but this works as a great breakpoint until then.
#endif
        }

        public static byte[] MessageTransmissionHandler(Message message)
        {
            byte[] textBytes = new byte[0];
            if (!String.IsNullOrEmpty(message.Text))
                textBytes = Encoding.Unicode.GetBytes(message.Text);

            byte[] dataBytes = new byte[0];
            if (message.Data != null)
                dataBytes = message.Data;

            byte[] messageBytes = ByteHelper.ConcatinateArray(new byte[] { (byte)message.Type }, dataBytes, textBytes);
#if DEBUG
            string test1 = Encoding.Unicode.GetString(ByteHelper.SubArray(messageBytes, 1));
#endif

            messageBytes = ChatTwo_Protocol.AddSignature(messageBytes, message.To);
#if DEBUG
            string test2 = Encoding.Unicode.GetString(ByteHelper.SubArray(messageBytes, 1));
#endif

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
        public byte[] Data { get; set; }
        public string Text { get; set; }
    }

    public class MessageTransmissionEventArgs : EventArgs
    {
        public IPEndPoint Ip { get; set; }
        public byte[] MessageBytes { get; set; }
    }
}
