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
        const byte version = 0x00;

        public enum MessageType
        {
            Keepalive,
            Status, // Tell server your online status and IP address.
            ContactRequest, // A request to make someone your contact.
            ContactStatus, // Tell client the online status and IP address of a contact.
            RelayMessage // Request for the server to relay a message to another user. Used if peer-to-peer fail?
        }

        public static bool ValidateSingature(byte[] bytes)
        {
            return (bytes.Length >= 2 && bytes[0] == 0x92);
        }

        public static byte[] AddSingature(byte[] bytes)
        {
            byte[] singatureBytes = new byte[] { 0x92, version }; // TAG byte and version byte.
            bytes = ByteHelper.ConcatinateArray(singatureBytes, bytes);
            return bytes;
        }

        public static byte[] RemoveSingature(byte[] bytes)
        {
            bytes = ByteHelper.SubArray(bytes, 2); // remove the TAG and the version number.
            return bytes;
        }

        public static void HandleReceivedMessage(ref MessageReceivedEventArgs args)
        {
            args.Data = ChatTwo_Protocol.RemoveSingature(args.Data); // remove the TAG and the version number.

            byte dataLength = 0;
            if (args.Data != null && args.Data.Length != 0)
            {
                dataLength = args.Data[0];
                args.Data = ByteHelper.SubArray(args.Data, 1);
            }
            byte[] messageData = new byte[0];
            if (dataLength != 0)
                messageData = ByteHelper.SubArray(args.Data, 0, dataLength);
            string message = "";
            if (args.Data.Length > dataLength)
                message = Encoding.Unicode.GetString(ByteHelper.SubArray(args.Data, dataLength + 1));
        }
    }

    static class ChatTwo_Client_Protocol
    {
        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            ChatTwo_Protocol.HandleReceivedMessage(ref args);

            byte[] messageBytes = args.Data;
            IPEndPoint messageSender = args.Sender;

            
            
        }
    }

    class ChatTwo_Server_Protocol
    {
        public static void MessageReceivedHandler(object sender, MessageReceivedEventArgs args)
        {
            ChatTwo_Protocol.HandleReceivedMessage(ref args);

            byte[] messageBytes = args.Data;
            IPEndPoint messageSender = args.Sender;

            
            
        }
    }
}
