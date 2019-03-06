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

        public bool HasPeer(string ipaddress, int port)
        {
            foreach((int id, string ip, int po) in GetPeerList())
            {
                if (ipaddress == ip && port == po)
                    return true;
            }
            return false;
        }

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
            } catch (System.Net.Sockets.SocketException)
            {
                TerminatePeer(id);
                Console.WriteLine("Peer with id " + id + " closed connection.");
            } catch (System.ObjectDisposedException)
            {
                Console.WriteLine("Peer with id " + id + " closed connection.");
            }
        }

        public void ConnectPeer(string ipaddress, int port)
        {
            Socket peer = Util.CreateSocket();
            try 
            {
                peer.Connect(ipaddress, port);
                AddPeer(peer);
                Console.WriteLine("Connected!");
            } catch(System.Net.Sockets.SocketException e)
            {
                Console.WriteLine("Error on connect, invalid IP/port");
            }
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
        // If no such peer, prints error.
        public bool SendToPeer(int id, string message)
        {
            bool result;
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                Peer peer = peers[id];
                Socket socket = peer.socket;
                byte[] bytes = ToByteString(message);

                try
                {
                    socket.Send(bytes);
                    result = true;
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    TerminatePeer(id);
                    Console.WriteLine("Error: " + e.Message + " when sending to peer with id " + id.ToString());
                    result = false;
                }
            }
            else
            {
                Console.WriteLine("Error: no peer with id " + id.ToString());
                result = false;
            }
     
            peerMutex.ReleaseMutex();
            return result;
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
        // If no such peer, prints error.
        public bool TerminatePeer(int id)
        {
            bool result;
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                var peer = peers[id];
                peer.socket.Close();
                peers.Remove(id);
                result = true;
            }
            else
            {
                Console.WriteLine("Error: no peer with id " + id.ToString());
                result = false;
            }

            peerMutex.ReleaseMutex();
            return result;
        }
    }
}