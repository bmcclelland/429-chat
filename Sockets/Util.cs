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
    }
}