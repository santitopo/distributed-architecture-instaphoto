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
            
            /*
            Console.WriteLine("File transfer will start once server begins sending");
            clientHandler.ReceiveFile();
            Console.WriteLine("File received");
             */

            Console.ReadLine();
        }
    }
}