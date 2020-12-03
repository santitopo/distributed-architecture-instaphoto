using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using System.Threading;
using Common.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using InstaPhotoServer;

namespace LogsServer
{
    public class Program
    {
        public static List<Log> logs = new List<Log>();
        public static void Main(string[] args)
        {
            Config.StartConfiguration(@"..\\config.txt");
            
            using var channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: Config.QueueName,
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
            channel.BasicConsume(queue: Config.QueueName,
                autoAck: true,
                consumer: consumer);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}