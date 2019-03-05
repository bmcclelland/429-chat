using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets
{
    class Server
    {
        PeerManager peers = new PeerManager();
        Socket listenSocket;
        int localPort;
        
        public Server(int port)
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