using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets
{
    class Util
    {
        
        public static string SocketLocalIP(Socket socket)
        {
            IPEndPoint ep = socket.LocalEndPoint as IPEndPoint;
            
            if (ep == null)
            {
                return "unknown";
            }
            else
            {
                return ep.Address.MapToIPv4().ToString();
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string SocketRemoteIP(Socket socket)
        {
            IPEndPoint ep = socket.RemoteEndPoint as IPEndPoint;

            if (ep == null)
            {
                return "unknown";
            }
            else
            {
                return ep.Address.MapToIPv4().ToString();
            }
        }

        public static int SocketLocalPort(Socket socket)
        {
            var ep = socket.LocalEndPoint as IPEndPoint;

            if (ep == null)
            {
                return 0;
            }
            else
            {
                return ep.Port;
            }
        }

        public static int SocketRemotePort(Socket socket)
        {
            var ep = socket.RemoteEndPoint as IPEndPoint;

            if (ep == null)
            {
                return 0;
            }
            else
            {
                return ep.Port;
            }
        }

        public static Socket CreateSocket()
        {
            return new Socket
                    (
                        AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp
                    );
        }
    }
}