using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using InstaPhotoServer;


namespace LogsServer
{
    class Program
    {
        public static List<Log> logs = new List<Log>();

        static void Main(string[] args)
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
            Console.WriteLine("--------Bienvenido al servidor de Logs--------");
            Console.WriteLine("Opciones:\n 1 - Ver Lista de Logs.\n 2 - Salir");
            while (!exit)
            {
                var option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        foreach (var log in logs)
                        {
                            string output = "Nivel: " + log.Level + " | Mensaje: " + log.Message + " |  Hora: " + log.DateTime;
                            Console.WriteLine(output);
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
    }
}