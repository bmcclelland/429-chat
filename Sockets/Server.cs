using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

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

            Console.WriteLine("Listening on " + IPAddress.Any.ToString() + ":" + localPort);

            for(int i = 0; i < 5; i++)
            {
                peers.AddPeer
                    (
                    new Socket
                    (
                        new IPEndPoint(IPAddress.Parse("192.168.1." + i), (5550+i))
                        .AddressFamily, SocketType.Stream, ProtocolType.Tcp
                    )
                    );
            }
            

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
                            Console.WriteLine("Option " + line);
                            break;
                        }
                    case "myport":
                        {
                            Console.WriteLine("Option " + localPort);
                            break;
                        }
                    case var val when new Regex(@"^connect\s+\d{1,5}$").IsMatch(val):
                        {
                            string port = Regex.Replace(line, @"connect\s+", "");
                            Console.WriteLine($"connecting to port: {port}");
                            break;
                        }
                    case "list":
                        {
                            Console.WriteLine(peers.GetPeers());
                            break;
                        }
                    case var val when new Regex(@"^terminate\s+(\d{1})$").IsMatch(val):
                        {
                            var m = new Regex(@"^terminate\s+(\d{1})$").Match(line);
                            int id = Int32.Parse(m.Groups[0].Captures[0].Value);
                            Console.WriteLine($"teminating connection {id}");
                            break;
                        }
                    case var val when new Regex(@"^send\s+(\d{1})\s+([a..zA..Z]+)}$").IsMatch(val):
                        {
                            var m = new Regex(@"^send\s+(\d{1})\s+)\s+([a..zA..Z]+)}$").Match(line);
                            int id = Int32.Parse(m.Groups[0].Captures[0].Value);
                            string msg = m.Groups[1].Captures[0].Value;
                            Console.WriteLine($"sending {msg} to id {id}");
                            break;
                        }
                    case "exit":
                        {
                            Console.WriteLine("All connections closing, good bye...");
                            // TODO: close connections
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