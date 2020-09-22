using System;
using System.Threading;
using Common.FileHandler;
using Common.FileHandler.Interfaces;

namespace Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Booting up server...");
            var serverHandler = new ServerHandler();
            serverHandler.StartServer();
            
            /*
            Console.WriteLine("Please enter the full path of the file to be transfered");
            string path = string.Empty;
            IFileHandler fileHandler = new FileHandler();
            while(path != null && path.Equals(string.Empty) && !fileHandler.FileExists(path))
            {
                path = Console.ReadLine();
            }
            serverHandler.SendFile(path);
            Console.WriteLine("Finished transferring file");
            */
            
            Console.ReadLine();
        }
    }
}