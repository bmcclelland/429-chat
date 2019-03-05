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

            Server app = new Server(8000);

            Console.WriteLine("Listening on " /*+ IPAddress.Any.ToString()*/ + ":" + app.LocalPort);

            string line = "";
            while ((line = Console.ReadLine()) != null)
            {
                Console.WriteLine("");

                switch (line)
                {
                    case "help":
                        Console.WriteLine("Option " + line);
                        break;
                    case "myip":
                        Console.WriteLine("Option " + line);
                        break;
                    case "myport":
                        Console.WriteLine("Option " + app.LocalPort);
                        break;
                    default:
                        Console.WriteLine("You dun scrued up...");
                        break;
                }
            }
        }
    }
}