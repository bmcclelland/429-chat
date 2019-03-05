using System;

namespace Sockets
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            foreach(string arg in args)
                Console.WriteLine(arg);

            try {
                int port = Int32.Parse(args[0]);
                if(args[0].Length != 4)
                {
                    Console.WriteLine("Port must be 4 digits");
                    return;
                }
                Server app = new Server(port);
            } catch (FormatException)
            {
                Console.WriteLine($"Unable to parse '{args[0]}'");
            }
        }
    }
}