using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client");
            var clientHandler = new ClientHandler();
            clientHandler.Menu0();
        }
    }
}