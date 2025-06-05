using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CustomTCPServerLibrary.Base
{
    public class NetEndPoint
    {
        public IPAddress? Address { get; }

        public int Port { get; }

        public static implicit operator IPEndPoint(NetEndPoint endPoint)
        {
            if (endPoint.Address is null)
            {
                return new IPEndPoint(0, endPoint.Port);
            }
            else
            {
                return new IPEndPoint(endPoint.Address, endPoint.Port);
            }
        }

        public static implicit operator NetEndPoint(IPEndPoint? endPoint)
        {
            if (endPoint is null)
            {
                return new NetEndPoint(null, 0);
            }
            else
            {
                return new NetEndPoint(endPoint.Address, endPoint.Port);
            }
        }

        public NetEndPoint(IPAddress? address, int port)
        {
            Address = address;
            Port = port;
        }

        public override string ToString()
        {
            return $"{(Address != null ? Address.ToString() : "-.-.-.-")}:{Port}";
        }
    }
}
