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
            Console.WriteLine("Starting gRPC client example......");
            
            
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new ABMUsers.ABMUsersClient(channel);
            var response = await client.AddUserAsync(new UserRequest() {User = "Hola"});
            Console.WriteLine("Respuesta: " + response.Message);
            Console.ReadLine();
            
        }
        
        
    }
}