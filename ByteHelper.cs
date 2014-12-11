using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ChatTwo_Server
{
    class ByteHelper
    {
        /// <summary>
        /// Converts a byte array to a Hexadecimal string.
        /// </summary>
        /// <param name="singleByte">Byte to be converted.</param>
        static public string ToHex(byte singleByte) // Based on http://stackoverflow.com/a/10048895
        {
            char[] hex = new char[2];

            byte b;

            b = ((byte)(singleByte >> 4));
            hex[0] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

            b = ((byte)(singleByte & 0x0F));
            hex[1] = (char)(b > 9 ? b - 10 + 'A' : b + '0');

            return new string(hex);
        }
        /// <summary>
        /// Converts a byte array to a Hexadecimal string.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        static public string ToHex(byte[] bytes)
        {
            List<string> hexs = new List<string>();
            foreach (byte singleByte in bytes)
                hexs.Add(ToHex(singleByte));
            return string.Join("-", hexs.ToArray());
        }

        /// <summary>
        /// Converts four bytes from a byte array to a int32.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        static public int ToInt32(byte[] bytes, int startIndex)
        {
            if (!BitConverter.IsLittleEndian)
            {
                bytes = ByteHelper.SubArray(bytes, startIndex, 4);
                Array.Reverse(bytes);
                startIndex = 0;
            }
            return BitConverter.ToInt32(bytes, startIndex);
        }

        /// <summary>
        /// Converts four bytes from a byte array to a float.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        static public float ToFloat(byte[] bytes, int startIndex)
        {
            byte[] subBytes = ByteHelper.SubArray(bytes, startIndex, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(subBytes);
            return BitConverter.ToSingle(subBytes, 0);
        }

        /// <summary>
        /// Converts a byte array to a string, using BigEndianUnicode.
        /// </summary>
        /// <param name="bytes">Bytes to be converted.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        /// <param name="length">Number of bytes to convert.</param>
        static public string ToBigEndianUnicodeString(byte[] bytes, int startIndex, int length)
        {
            byte[] subBytes = ByteHelper.SubArray(bytes, startIndex, length);
            //subBytes = Helper.ConcatinateArray(new byte[] { 0xFE, 0xFF }, subBytes);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(subBytes);
            string text = Encoding.BigEndianUnicode.GetString(subBytes); // UTF-16 BigEndian to string.
            return text;
        }

        /// <summary>
        /// Returns the Hexavigesimal letters only ID.
        /// </summary>
        /// <param name="numberID">Int32 version of the ID.</param>
        static public string ToID(int numberID)
        { // http://en.wikipedia.org/wiki/Hexavigesimal
            numberID = Math.Abs(numberID);
            String converted = "";
            // Repeatedly divide the number by 26 and convert the
            // remainder into the appropriate letter.
            while (numberID > 0)
            {
                int remainder = (numberID) % 26;
                converted = converted + (char)(remainder + 'A');
                numberID = (numberID - remainder) / 26;
            }

            return converted;
        }

        /// <summary>
        /// Returns the SHA1 hash of a byte array in a Base64String.
        /// </summary>
        /// <param name="numberID">Byte array to be hashed.</param>
        static public string GetHashString(byte[] bytes)
        {
            string hash = Convert.ToBase64String(GetHashBytes(bytes));
            return hash;
        }

        /// <summary>
        /// Returns the SHA1 hash of a byte array in a byte array.
        /// </summary>
        /// <param name="numberID">Byte array to be hashed.</param>
        static public byte[] GetHashBytes(byte[] bytes)
        {
            byte[] hash;
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                hash = sha1.ComputeHash(bytes);
            }
            return hash;
        }

        /// <summary>
        /// Returns selected part of a byte array.
        /// </summary>
        /// <param name="bytes">Full byte array.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        static public byte[] SubArray(byte[] bytes, int startIndex)
        {
            if (startIndex == 0)
                return bytes;
            return SubArray(bytes, startIndex, bytes.Length - startIndex);
        }
        /// <summary>
        /// Returns selected part of a byte array.
        /// </summary>
        /// <param name="bytes">Full byte array.</param>
        /// <param name="startIndex">Index of the starting byte.</param>
        /// <param name="length">Number of bytes to return.</param>
        static public byte[] SubArray(byte[] bytes, int startIndex, int length)
        {
            byte[] rv = new byte[length];
            System.Buffer.BlockCopy(bytes, startIndex, rv, 0, length);
            return rv;
            //return new List<byte>(bytes).GetRange(startIndex, length).ToArray(); // Another ways of doing it.
        }

        /// <summary>
        /// Combine multiple arrays into one.
        /// One after the other.
        /// </summary>
        static public byte[] ConcatinateArray(byte[] array1, byte[] array2)
        {
            byte[] rv = new byte[array1.Length + array2.Length];
            System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
            return rv;
        }
        /// <summary>
        /// Combine multiple arrays into one.
        /// One after the other.
        /// </summary>
        static public byte[] ConcatinateArray(byte[] array1, byte[] array2, byte[] array3)
        {
            byte[] rv = new byte[array1.Length + array2.Length + array3.Length];
            System.Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            System.Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);
            System.Buffer.BlockCopy(array3, 0, rv, array1.Length + array2.Length, array3.Length);
            return rv;
        }

        /// <summary>
        /// Returns true if flag is in bitCode.
        /// ((bitCode AND flag) == flag)
        /// </summary>
        /// <param name="bitCode">Byte or int32 used for bit flags.</param>
        /// <param name="flag">Flag to check for.</param>
        static public bool FlagCheck(int bitCode, int flag)
        {
            return ((bitCode & flag) == flag);
        }

        /// <summary>
        /// Returns a string that is void of HTML tags.
        /// Attempts to add newlines where needed.
        /// </summary>
        /// <param name="input">HTML string.</param>
        static public string CleanText(string input) // Removes the html code tags.
        {
            if (input.Contains("<") && input.Contains(">"))
            {
                int tagStart, tagEnd;
                string processed = "";
                while (input.Contains("<") && input.Contains(">"))
                {
                    tagStart = input.IndexOf('<');
                    tagEnd = input.IndexOf('>') - tagStart;
                    processed += input.Remove(tagStart);
                    string tag = input.Substring(tagStart + 1, tagEnd - 1);
                    input = input.Substring(tagStart + tagEnd + 1);
                    switch (tag.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0])
                    {
                        case "br":
                        case "div":
                        case "/div":
                        case "/td":
                        case "/tr":
                            processed += Environment.NewLine;
                            break;
                        //case "b":
                        //case "/b":
                        //case "td":
                        //case "tr":
                        //    break;
                    }
                }
                processed = processed.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine); // Remove triple NewLines.
                processed = processed.Replace("&nbsp;", " "); // Replace "&nbsp;" with a normal " ".
                return processed.Trim(); // Trim for good measure before reuturning the text.
            }
            else
                return input.Trim();
        }
    }
}
