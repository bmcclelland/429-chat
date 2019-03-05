using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Sockets
{
    class Peer
    {
        public int id;
        public string address;

        public Peer(int id, string address)
        {
            this.id = id;
            this.address = address;
        }

        public static int CompareById(Peer a, Peer b)
        {
            return a.id.CompareTo(b.id);
        }
    }

    class PeerManager
    {
        Mutex peerMutex = new Mutex();
        int nextPeerID = 0;
        Dictionary<int,Socket> peers = new Dictionary<int,Socket>();

        public void AddPeer(Socket peerSocket)
        {
            peerMutex.WaitOne();
            peers.Add(nextPeerID, peerSocket);
            nextPeerID++;
            peerMutex.ReleaseMutex();
        }

        // Sends message to peer with given id. 
        // If no such peer, nothing happens.
        public void SendToPeer(int id, string message)
        {
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                var socket = peers[id];
                var bytes = toByteString(message);
                socket.Send(bytes);
            }
     
            peerMutex.ReleaseMutex();

        }

        private static Byte[] toByteString(string s)
        {
            Encoding e = new ASCIIEncoding();
            return e.GetBytes(s);
        }

        // Returns a list of Peers sorted by id.
        public List<Peer> GetPeers()
        {
            List<Peer> result = new List<Peer>();
            peerMutex.WaitOne();
                      
            foreach (var kv in peers)
            {
                var id = kv.Key;
                var address = kv.Value.RemoteEndPoint.ToString();
                result.Add(new Peer(id, address));
            }

            peerMutex.ReleaseMutex();
            result.Sort(Peer.CompareById);
            return result;
        }

        // Closes the connection with given peer id.
        // If no such peer, does nothing.
        public void TerminatePeer(int id)
        {
            peerMutex.WaitOne();

            if (peers.ContainsKey(id))
            {
                var socket = peers[id];
                socket.Close();
                peers.Remove(id);
            }

            peerMutex.ReleaseMutex();
        }
    }
}