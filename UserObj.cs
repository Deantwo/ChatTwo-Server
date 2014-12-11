using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;

namespace ChatTwo_Server
{

    public class UserObj
    {
        public int ID { set; get; }
        public string Name { set; get; }
        public string Password { set; get; }
        public bool Online { set; get; }
        public IPEndPoint Socket { set; get; }
        public DateTime LastOnline { set; get; }
        public DateTime Registered { set; get; }
        public string Secret { set; get; }

        public UserObj()
        {
        }

        public void StringSocket(string socket)
        {
            if (String.IsNullOrEmpty(socket))
                Socket = null;
            else
                Socket = CreateIPEndPoint(socket);
        }

        public override string ToString()
        {
            return "user[" + ID + "] Name: " + Name + Environment.NewLine +
                   "user[" + ID + "] Password: " + Password + Environment.NewLine +
                   "user[" + ID + "] Online: " + Online + Environment.NewLine +
                   "user[" + ID + "] Socket: " + Socket + Environment.NewLine +
                   "user[" + ID + "] LastOnline: " + LastOnline.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine +
                   "user[" + ID + "] Registered: " + Registered.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Handles IPv4 and IPv6 notation.
        // http://stackoverflow.com/questions/2727609/best-way-to-create-ipendpoint-from-string
        private IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
