using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using InstaPhotoServer;

namespace LogsServer2
{
    public class Program
    {
        public static List<Log> logs = new List<Log>();
        public static void Main(string[] args)
        {
            var threadServer = new Thread(()=> MessageQueue());
            threadServer.Start();
            CreateHostBuilder(args).Build().Run();
        }

        static void MessageQueue()
        {
            using var channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var log = JsonSerializer.Deserialize<Log>(message);
                logs.Add(log);
            };
            channel.BasicConsume(queue: "log_queue",
                autoAck: true,
                consumer: consumer);

            var exit = false;
            while (!exit)
            {
                Console.WriteLine("--------Bienvenido al servidor de Logs--------");
                Console.WriteLine("Opciones:\n 1 - Ver Lista de Logs.\n 2 - Salir");
                var option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        if (logs != null && logs.Count > 0)
                        {
                            foreach (var log in logs)
                            {
                                string output = "Nivel: " + log.Level + " | Mensaje: " + log.Message + " |  Hora: " + log.DateTime;
                                Console.WriteLine(output);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No hay logs para mostrar");
                        }
                        
                        break;
                    case "2":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Opcion Incorrecta");
                        break;
                }
            }
        }
        
        
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}