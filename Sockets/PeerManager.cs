using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    class Peer
    {
        public const int BUFSIZE = 1024;
        public const SocketFlags SOCKETFLAGS = SocketFlags.None;

        public Socket socket;
        public byte[] receiveBuf = new byte[BUFSIZE];
        public StringBuilder receiveString = new StringBuilder();

        public void BeginReceive(AsyncCallback callback, int id)
        {
            socket.BeginReceive(receiveBuf, 0, BUFSIZE, SOCKETFLAGS, callback, (this, id));
        }
    }

    class PeerManager
    {
        Mutex peerMutex = new Mutex();
        int nextPeerID = 0;
        Dictionary<int,Peer> peers = new Dictionary<int,Peer>();

        private void PrintMessage(Peer peer)
        {
            string address = Util.SocketRemoteIP(peer.socket);
            int port = Util.SocketRemotePort(peer.socket);
            string message = peer.receiveString.ToString();
            peer.receiveString.Clear();
            Console.WriteLine(String.Format(
                "Message received from {0}\nSender's Port: {1}\nMessage: \"{2}\"", 
                address, port, message
                ));
        }

        private void OnReceive(IAsyncResult result)
        {
            (Peer peer, int id) = ((Peer, int))(result.AsyncState);
            try {
                int read = peer.socket.EndReceive(result);

                if (read > 0)
                {
                    peer.receiveString.Append(Encoding.ASCII.GetString(peer.receiveBuf, 0, read));

                    if (read != Peer.BUFSIZE)
                    {
                        PrintMessage(peer);
                    }
                }
                else
                {
                    if (peer.receiveString.Length > 1)
                    {
                        PrintMessage(peer);
                    }
                    else
                    {
                        if (peer.receiveString.Length > 1)
                        {
                            string s = peer.receiveString.ToString();
                            Console.WriteLine(String.Format("Read {0} byte from socket" + "data = {1} ", s.Length, s));
                            peer.receiveString.Clear();
                        }
                    }
                    peer.BeginReceive(new AsyncCallback(OnReceive), id);
                }
            } catch (System.Net.Sockets.SocketException e)
            {
                TerminatePeer(id);
                Console.WriteLine("client id " + id + " closed connection.");
            }
        }

        public void ConnectPeer(string ipaddress, int port)
        {
            Socket peer = Util.CreateSocket();
            peer.Connect(ipaddress, port);
            AddPeer(peer);
        }

        public void AddPeer(Socket peerSocket)
        {
            peerMutex.WaitOne();
            Peer peer = new Peer();
            peer.socket = peerSocket;
            peers.Add(nextPeerID, peer);
            peer.BeginReceive(new AsyncCallback(OnReceive), nextPeerID++);
            peerMutex.ReleaseMutex();
        }

        // Sends message to peer with given id. 
        // If no such peer, nothing happens.
        public void SendToPeer(int id, string message)
        {
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                Peer peer = peers[id];
                Socket socket = peer.socket;
                byte[] bytes = ToByteString(message);
                socket.Send(bytes);
            }
     
            peerMutex.ReleaseMutex();

        }

        private static byte[] ToByteString(string s)
        {
            Encoding e = new ASCIIEncoding();
            return e.GetBytes(s);
        }

        private static int CompareById((int, string, int) a, (int, string, int) b)
        {
            return a.Item1.CompareTo(b.Item1);
        }

        // Returns a list of Peers sorted by id.
        public List<(int, string, int)> GetPeerList()
        {
            List<(int, string, int)> result = new List<(int, string, int)>();
            peerMutex.WaitOne();
                      
            foreach (var kv in peers)
            {
                var id = kv.Key;
                var address = Util.SocketRemoteIP(kv.Value.socket);
                var port = Util.SocketRemotePort(kv.Value.socket);
                result.Add((id, address, port));
            }

            peerMutex.ReleaseMutex();
            result.Sort(CompareById);
            return result;
        }

        // Closes the connection with given peer id.
        // If no such peer, does nothing.
        public void TerminatePeer(int id)
        {
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                var peer = peers[id];
                peer.socket.Close();
                peers.Remove(id);
            }

            peerMutex.ReleaseMutex();
        }
    }
}