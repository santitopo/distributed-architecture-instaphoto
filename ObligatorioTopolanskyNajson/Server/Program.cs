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
        }
    }
}