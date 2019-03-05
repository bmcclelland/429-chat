using System;
using System.Net;
using System.Net.Sockets;

namespace Sockets
{
    class Server
    {
        PeerManager peers = new PeerManager();
        Socket listenSocket;
        public int LocalPort { get; private set; }
        public string serverIP { get { return IPAddress.Any.ToString(); } }
        
        public Server(int port)
        {
            LocalPort = port;
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
            var localEndPoint = new IPEndPoint(IPAddress.Any, LocalPort);
            listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(10);
            var callback = new AsyncCallback(OnIncomingConnect);
            listenSocket.BeginAccept(callback, listenSocket);
            Console.WriteLine("Listening");
        }
    }
}