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

        public static string SocketLocalPort(Socket socket)
        {
            var ep = socket.LocalEndPoint as IPEndPoint;

            if (ep == null)
            {
                return "unknown";
            }
            else
            {
                return ep.Port.ToString();
            }
        }

        public static string SocketRemotePort(Socket socket)
        {
            var ep = socket.RemoteEndPoint as IPEndPoint;

            if (ep == null)
            {
                return "unknown";
            }
            else
            {
                return ep.Port.ToString();
            }
        }
    }
}