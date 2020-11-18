using System;
using System.Threading.Tasks;
using Grpc.Net.Client;

namespace AdministrativeServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
            Console.WriteLine("Starting gRPC client example......");
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            var response =  await client.SayHelloAsync(new HelloRequest{Name = "Hola"});
            Console.WriteLine("Respuesta: " + response.Message);
            
            
            Console.ReadLine();
        }
    }
}