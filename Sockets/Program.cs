using System;

namespace Sockets
{
    class Program
    {
        static int Main(string[] args)
        {
            try {
                if (args.Length == 0)
                {
                    Console.WriteLine($"Usage: Sockets <listen port>");
                    return 1;
                }

                int port = Int32.Parse(args[0]);
                Server app = new Server(port);
                return 0;
            } catch (FormatException)
            {
                Console.WriteLine($"Error: unable to parse port argument: '{args[0]}'");
                return 1;
            }
        }
    }
}