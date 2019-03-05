using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    class Program
    {
        static void Main(string[] args)
        {
            Peer app = new Peer(8000);
        }
    }

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

    class Peer
    {
        PeerManager peers = new PeerManager();
        Socket listenSocket;
        int localPort;
        
        public Peer(int port)
        {
            localPort = port;
            Run();
        }

        void OnIncomingConnect(IAsyncResult result)
        {
            Socket peerSocket = listenSocket.EndAccept(result);
            Console.WriteLine("onIncomingConnect " + peerSocket.RemoteEndPoint.ToString());
            peers.AddPeer(peerSocket);
            Console.WriteLine("added peer " + peerSocket.RemoteEndPoint.ToString());
        } 

        void Run()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(10);
            var callback = new AsyncCallback(OnIncomingConnect);
            listenSocket.BeginAccept(callback, listenSocket);
            Console.WriteLine("Listening");

            while (true)
            {}
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