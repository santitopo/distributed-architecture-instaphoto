using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting client");
            var clientHandler = new ClientHandler();
            Console.WriteLine("Connecting to server...");
            clientHandler.StartClient();
            Console.WriteLine("Connected to server");
            
            clientHandler.Chat();
            
            /*
            Console.WriteLine("File transfer will start once server begins sending");
            clientHandler.ReceiveFile();
            Console.WriteLine("File received");
             */

            Console.ReadLine();
        }
    }
}