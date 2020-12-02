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
            //var threadServer = new Thread(()=> GrpcClient());
            //threadServer.Start(); 
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        
        static async Task GrpcClient()
        {
               
        }
    }
}