using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InstaPhotoServer;

namespace AdministrativeClient
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();

        static async Task Main(string[] args)
        {
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
                Stream response;
                
                switch (input)
                {
                    case "a":
                        GetLogsFunction();
                        break; 
                    case "b":
                        AddUserFunction();
                        break;
                    case "c":
                        DeleteFunction();
                        break; 
                    case "d":
                        ModifyUserFunction();
                        break;
                    case "exit":
                        _keepConnection = false;
                        break;
                }
            }
        }

        private static async void ModifyUserFunction()
        {
            try
            {
                List<User> users = ServerHandler._repository.Users;
                int i;
                for (i = 0; i < users.Count; i++)
                {
                    Console.WriteLine("{0}. {1} {2} - {3}", i,users[i].Name,users[i].Surname, users[i].UserName);
                }
                
                Console.WriteLine("Ingrese el numero del usuario que desea modificar: ");
                string input = Console.ReadLine();
                User selectedUser = users[i - 1];
                
                StringContent content = new StringContent(JsonSerializer.Serialize(selectedUser),Encoding.UTF8,"application/json");
                var response = await _client.PutAsync("https://localhost:44359/users", content);
                var message = await response.Content.ReadAsStringAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error procesando la solicitud, vuelva a intentarlo.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async void DeleteFunction()
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                var resp = await _client.GetStreamAsync("https://localhost:44359/users");
                List<User> users = await JsonSerializer.DeserializeAsync<List<User>>(resp);
                
                int i;
                for (i = 0; i < users.Count; i++)
                {
                    Console.WriteLine("{0}. {1} {2} - {3}", i,users[i].Name,users[i].Surname, users[i].UserName);
                }
                
                Console.WriteLine("Ingrese el numero del usuario que desea eliminar: ");
                string input = Console.ReadLine();
                User selectedUser = users[Convert.ToInt32(input)];
                
                var request = new HttpRequestMessage {
                    Method = HttpMethod.Delete,
                    RequestUri = new Uri("https://localhost:44359/users"),
                    Content = new StringContent(JsonSerializer.Serialize(selectedUser),Encoding.UTF8,"application/json")
                };
                var response = await _client.SendAsync(request);
                var message = await response.Content.ReadAsStringAsync();
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(message);
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error procesando la solicitud, vuelva a intentarlo.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async void GetLogsFunction()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            var response = await _client.GetStreamAsync("https://localhost:44370/logs");
            IEnumerable<logModel> logs = await JsonSerializer.DeserializeAsync<List<logModel>>(response);
                        
            foreach (var log in logs)
            {
                Console.WriteLine("[{0}] {1} - {2}", log.level, log.message, log.dateTime);
            }
        }

        private static async Task AddUserFunction()
        {
            try
            {
                Console.WriteLine("Ingrese los datos del nuevo usuario en el siguiente formato: Nombre#Apellido#Usuario#Contraseña");
                string input = Console.ReadLine();

                string[] userData = input.Split("#");
                if (userData.Length == 4)
                {
                    User newUser = new User(userData[0],userData[1],userData[2],userData[3]);

                    StringContent content = new StringContent(JsonSerializer.Serialize(newUser),Encoding.UTF8,"application/json");
                    var response = await _client.PostAsync("https://localhost:44359/users", content);
                    var message = await response.Content.ReadAsStringAsync();
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error. Debe ingresar los 4 parametros separados por #");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error procesando la solicitud, vuelva a intentarlo.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        
    }
        
        public class logModel
        {
            public string level { get; set; }
            public string message { get; set; }
            public DateTime dateTime { get; set; } 
        }
}
