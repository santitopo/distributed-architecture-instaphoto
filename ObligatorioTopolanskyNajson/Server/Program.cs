using System;
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
            UserSessionsHandler userSessions = new UserSessionsHandler();
            LoadSessions(userSessions);
            var serverHandler = new ServerHandler(userSessions);
            serverHandler.StartServer();
        }

        private static void LoadSessions(UserSessionsHandler userSessions)
        {
            User u1 = new User("Jose", "Hernandez", "jh12", "user");
            User u2 = new User("Martina", "Perez", "mp10", "user");
            userSessions.Users.Add(u1);
            userSessions.Users.Add(u2);
        }
    }
}