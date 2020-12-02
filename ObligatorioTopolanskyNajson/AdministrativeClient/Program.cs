﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using InstaPhotoServer;

namespace AdministrativeClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            bool _keepConnection = true;
            
            while (_keepConnection)
            {
                Console.WriteLine("\n........ MENU PRINCIPAL ........\n" + 
                                  "(a) - Ver logs del sistema\n" +
                                  "(b) - Agregar un nuevo usuario\n"+
                                  "(c) - Borrar usuario\n"+
                                  "(d) - Modificar usuario\n" +
                                  "(exit) - Desconectarse");
                
                string input = Console.ReadLine();
                
                switch (input)
                {
                    case "a":
                        client.DefaultRequestHeaders.Accept.Clear();
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        
                        var streamResult = await client.GetStreamAsync("https://localhost:44370/logs");
                        IEnumerable<logModel> logs = await JsonSerializer.DeserializeAsync<List<logModel>>(streamResult);
                        
                        foreach (var log in logs)
                        {
                            Console.WriteLine("[{0}] {1} - {2}", log.level, log.message, log.dateTime);
                        }
                        
                        break; 
                    case "b":
                        break;
                    case "c":
                        break; 
                    case "d":
                        break;
                    case "exit":
                        _keepConnection = false;
                        break;
                }
            }
        }
        
        
        
        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = client.GetStringAsync("https://localhost:44359/weatherforecast");

            var msg = await stringTask;
            Console.Write(msg);
        }
        
    }
        
        public class logModel
        {
            public string level { get; set; }
            public string message { get; set; }
            public DateTime dateTime { get; set; } 
        }
}
