using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Common.Config;
using Common.FileHandler;
using Common.FileHandler.Interfaces;

namespace InstaPhotoServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var threadServer = new Thread(()=> BootServer());
            threadServer.Start();
            CreateHostBuilder(args).Build().Run();
        }

        public static void BootServer()
        {
            Console.WriteLine("Booting up server...");
            Config.StartConfiguration(@"..\\config.txt");
            Repository userSessions = new Repository();
            LoadSessions(userSessions);
            var serverHandler = new ServerHandler(userSessions);
            serverHandler.StartServer();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(5001, o => o.Protocols = HttpProtocols.Http2);
                    });
                    webBuilder.UseStartup<Startup>();
                });
        
        private static void LoadSessions(Repository repository)
        {
            User u1 = new User("Jose", "Hernandez", "jh12", "user");
            User u2 = new User("Martina", "Perez", "mp10", "user");
            User u3 = new User("Santiago", "Topolansky", "santi", "topo");
            repository.Users.Add(u1);
            repository.Users.Add(u2);
            repository.Users.Add(u3);
            repository.Photos.Add(u1, new List<Photo>());
            repository.Photos.Add(u2, new List<Photo>());
            repository.Photos.Add(u3, new List<Photo>());
        }
    }
}