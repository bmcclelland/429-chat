using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Sockets
{
    class Server
    {
        PeerManager peerManager = new PeerManager();
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
            peerManager.AddPeer(peerSocket);
            Console.WriteLine("Connection from " + Util.SocketRemoteIP(peerSocket));
            var callback = new AsyncCallback(OnIncomingConnect);
            listenSocket.BeginAccept(callback, listenSocket);
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

            Console.WriteLine("Listening on " + IPAddress.Any.ToString() + ":" + localPort);            

            string line = "";
            do
            {
                Console.WriteLine("");

                switch (line)
                {
                    case "help":
                        {
                            Console.WriteLine("Commands:\n" +
                                "[myip]:\n" +
                                " Display the IP address of this process.\n" +
                                "[myport]:\n" +
                                " Display the port on which this process is listening for\n" +
                                " incoming connections.\n" +
                                "[connect <destination> <port no>]:\n" +
                                " new TCP connection to <destination>\n" +
                                "at the specified <port no>.\n" +
                                "[list]:\n" +
                                " Display a numbered list of all connections to this client.\n" +
                                "[terminate <connection id>]:\n" +
                                " This command will terminate the connection\n" +
                                "listed under the specified number when LIST is used to display all\n" +
                                "connections.\n" +
                                "[send <connection id> <message>]:\n" +
                                " This will send the message to the\n" +
                                "host on the connection that is designated by the id.\n" +
                                "[exit]:\n" +
                                " Close all connections and terminate this process.\n");
                            break;
                        }
                    case "myip":
                        {
                            Console.WriteLine(Util.GetLocalIPAddress());
                            break;
                        }
                    case "myport":
                        {
                            Console.WriteLine(localPort);
                            break;
                        }
                    case var val when new Regex(@"^connect\s+(.+)\s+(.+)$").IsMatch(val):
                        {
                            var m = new Regex(@"^connect\s+(.+)\s+(.+)$").Match(line);
                            string address = m.Groups[1].Captures[0].Value;

                            try
                            {
                                int port = Int32.Parse(m.Groups[2].Captures[0].Value);

                                if ((address == Util.GetLocalIPAddress() || address == "127.0.0.1") && port == localPort)
                                {
                                    Console.WriteLine("Cannot connect to yourself.");
                                }
                                else if (peerManager.HasPeer(address, port))
                                {
                                    Console.WriteLine("Connection already exists.");
                                }
                                else
                                {
                                    Console.WriteLine($"connecting to {address}:{port}...");
                                    peerManager.ConnectPeer(address, port);
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Error: invalid IP/port");
                            }
                            break;
                        }
                    case "list":
                        {
                            foreach((int id, string address, int port) in peerManager.GetPeerList())
                                Console.WriteLine(id + " " + address + " " + port);
                            break;
                        }
                    case var val when new Regex(@"^terminate\s+(\d{1})$").IsMatch(val):
                        {
                            var m = new Regex(@"^terminate\s+(\d{1})$").Match(line);
                            int id = Int32.Parse(m.Groups[1].Captures[0].Value);
                            
                            if (peerManager.TerminatePeer(id))
                                Console.WriteLine("Terminated peer with id " + id.ToString());

                            break;
                        }
                    case var val when new Regex(@"^send\s+(\d{1})\s+(.+)$").IsMatch(val):
                        {
                            var m = new Regex(@"^send\s+(\d{1})\s+(.+)$").Match(line);
                            int id = Int32.Parse(m.Groups[1].Captures[0].Value);
                            string msg = m.Groups[2].Captures[0].Value;

                            if (msg.Length > 100)
                            {
                                Console.WriteLine("Error: message must be 100 characters or less");
                            }
                            else
                            {
                                if (peerManager.SendToPeer(id, msg))
                                    Console.WriteLine("Message sent to peer with id " + id.ToString());
                            }
                            break;
                        }
                    case "exit":
                        {
                            Console.WriteLine("All connections closing, good bye...");
                            foreach((int id, string address, int port) in peerManager.GetPeerList())
                            {
                                peerManager.TerminatePeer(id);
                            }
                            Console.WriteLine("Terminating connections");
                            return;
                        }
                    default:
                        {
                            Console.WriteLine("Type [help] for a list of commands...");
                            break;
                        }
                }
            } while ((line = Console.ReadLine()) != null);
        }
    }
}