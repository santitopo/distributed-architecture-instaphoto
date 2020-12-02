using System;
using System.Threading;
using System.Threading.Tasks;
using AdministrativeServer;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AdministrativeServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var threadServer = new Thread(()=> GrpcClient());
            threadServer.Start(); 
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
        static async Task GrpcClient()
        {
               AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = await client.AddUserAsync( new UserModel()
                {
                    Name = "Pepe",
                    Surname = "pp",
                    Password = "blala",
                    Username = "dasdsa"
                });

                Console.WriteLine(response.Message);
                Console.ReadLine();
        }
    }
}