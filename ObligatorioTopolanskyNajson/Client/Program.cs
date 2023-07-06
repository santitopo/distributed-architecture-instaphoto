using System;
using Common.Config;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client");
            Config.StartConfiguration(@"..\\..\\..\\..\\config.txt");
            var clientHandler = new ClientHandler();
            clientHandler.Menu0();
        }
    }
}