using System;
using System.Collections.Generic;
using System.Threading;
using Common.Config;
using Common.FileHandler;
using Common.FileHandler.Interfaces;

namespace Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Booting up server...");
            Config.StartConfiguration();
            Repository userSessions = new Repository();
            LoadSessions(userSessions);
            var serverHandler = new ServerHandler(userSessions);
            serverHandler.StartServer();
        }

        private static void LoadSessions(Repository repository)
        {
            User u1 = new User("Jose", "Hernandez", "jh12", "user");
            User u2 = new User("Martina", "Perez", "mp10", "user");
            User u3 = new User("Santiago", "Topolansky", "santi", "topo");
            repository.Users.Add(u1);
            repository.Users.Add(u2);
            repository.Photos.Add(u1, new List<Photo>());
            repository.Photos.Add(u2, new List<Photo>());
        }
    }
}