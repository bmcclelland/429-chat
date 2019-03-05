using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    class PeerManager
    {
        Mutex peerMutex = new Mutex();
        List<Socket> peers = new List<Socket>();

        public void AddPeer(Socket peerSocket)
        {
            peerMutex.WaitOne();
            peers.Add(peerSocket);
            peerMutex.ReleaseMutex();
        }

        public void SendToAll(string message)
        {
            peerMutex.WaitOne();

            Encoding e = new ASCIIEncoding();
            Byte[] bytes = e.GetBytes(message);

            foreach (Socket peer in peers)
            {
                peer.Send(bytes);
            }

            peerMutex.ReleaseMutex();
        }
    }
}

/*
private static Socket ConnectSocket(string server, int port)
{
    Socket s = null;
    IPHostEntry hostEntry = null;

    // Get host related information.
    hostEntry = Dns.GetHostEntry(server);

    // Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
    // an exception that occurs when the host IP Address is not compatible with the address family
    // (typical in the IPv6 case).
    foreach (IPAddress address in hostEntry.AddressList)
    {
        IPEndPoint ipe = new IPEndPoint(address, port);
        Socket tempSocket =
            new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        tempSocket.Connect(ipe);

        if (tempSocket.Connected)
        {
            s = tempSocket;
            break;
        }
        else
        {
            continue;
        }
    }
    return s;
}
*/