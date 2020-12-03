using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Config;
using InstaPhotoServer;

namespace AdministrativeClient
{
    class Program
    {
        private static readonly HttpClient _client = new HttpClient();
        private static string rootUserName;
        private static string rootPassword;
        static async Task Main(string[] args)
        {
            bool _keepConnection = true;
            Config.StartConfiguration(@"..\\..\\..\\..\\config.txt");
            
            rootUserName = Config.RootUser;
            rootPassword = Config.RootPassword;
            
            Console.WriteLine("Ingrese usuario y contraseña del administrador separados por #:");
            string input = Console.ReadLine();
            string[] credentials = input.Split("#");

            if (credentials.Length == 2)
            {
                if (credentials[0].Equals(rootUserName) && credentials[1].Equals(rootPassword))
                {
                    while (_keepConnection)
                    {
                        Console.WriteLine("\n........ MENU PRINCIPAL ........\n" + 
                                          "(a) - Ver logs del sistema\n" +
                                          "(b) - Agregar un nuevo usuario\n"+
                                          "(c) - Borrar usuario\n"+
                                          "(d) - Modificar usuario\n" +
                                          "(exit) - Desconectarse");
                
                        input = Console.ReadLine();
                        Stream response;
                
                        switch (input)
                        {
                            case "a":
                                GetLogsFunction();
                                break; 
                            case "b":
                                await AddUserFunction();
                                break;
                            case "c":
                                await DeleteFunction();
                                break; 
                            case "d":
                                await ModifyUserFunction();
                                break;
                            case "exit":
                                _keepConnection = false;
                                break;
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error. Credenciales invalidas");
                    Console.WriteLine("Desconectando cliente administrativo...");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error. Debe ingresar los 2 parametros separados por #");
                Console.WriteLine("Desconectando cliente administrativo...");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private static async Task ModifyUserFunction()
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                var resp = await _client.GetStreamAsync(Config.UsersAPIUri);
                List<userModel> users = await JsonSerializer.DeserializeAsync<List<userModel>>(resp);
                
                for (int i = 0; i < users.Count; i++)
                {
                    Console.WriteLine("{0}. {1} {2} - {3}", i,users[i].name,users[i].surname, users[i].username);
                }
                
                Console.WriteLine("Ingrese el numero del usuario que desea eliminar: ");
                string input = Console.ReadLine();
                int selectedIndex = Convert.ToInt32(input);

                if (selectedIndex <= users.Count-1  && selectedIndex >= 0)
                {
                    Console.WriteLine("Ingrese Nombre, Apellido y Contraseña del usuario a modificar separados por #: ");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("[Info: Si desea mantener los valores de ciertos campos, por favor, reingesar el mismo valor.]");
                    Console.ForegroundColor = ConsoleColor.White;
                    
                    string input2 = Console.ReadLine();
                    string[] newData = input2.Split("#");

                    if (newData.Length == 3)
                    {
                        User user = new User(newData[0],newData[1], 
                            users[selectedIndex].username, newData[2]);
                    
                        StringContent content = new StringContent(JsonSerializer.Serialize(user),Encoding.UTF8,"application/json");
                        var response = await _client.PutAsync(Config.UsersAPIUri, content);
                        var message = await response.Content.ReadAsStringAsync();
                
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(message);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error. Debe ingresar los 3 parametros separados por #");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error. Debe ingresar un numero válido");
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

        private static async Task DeleteFunction()
        {
            try
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                var resp = await _client.GetStreamAsync(Config.UsersAPIUri);
                List<userModel> users = await JsonSerializer.DeserializeAsync<List<userModel>>(resp);
                
                for (int i = 0; i < users.Count; i++)
                {
                    Console.WriteLine("{0}. {1} {2} - {3}", i,users[i].name,users[i].surname, users[i].username);
                }
                
                Console.WriteLine("Ingrese el numero del usuario que desea eliminar: ");
                string input = Console.ReadLine();
                int selectedIndex = Convert.ToInt32(input);

                if (selectedIndex <= users.Count-1  && selectedIndex >= 0)
                {
                    User user = new User(users[selectedIndex].name,users[selectedIndex].surname, 
                        users[selectedIndex].username, users[selectedIndex].password);
                
                    var request = new HttpRequestMessage {
                        Method = HttpMethod.Delete,
                        RequestUri = new Uri("https://localhost:44359/users"),
                        Content = new StringContent(JsonSerializer.Serialize(user),Encoding.UTF8,"application/json")
                    };
                
                    var response = await _client.SendAsync(request);
                    var message = await response.Content.ReadAsStringAsync();
                
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(message);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error. Debe ingresar un numero válido");
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

        private static async void GetLogsFunction()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            var response = await _client.GetStreamAsync(Config.LogsAPIUri);
            IEnumerable<logModel> logs = await JsonSerializer.DeserializeAsync<List<logModel>>(response);
                       
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var log in logs)
            {
                Console.WriteLine("[{0}] {1} - {2}", log.level, log.message, log.dateTime);
            }
            Console.ForegroundColor = ConsoleColor.White;

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
                    var response = await _client.PostAsync(Config.UsersAPIUri, content);
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
        
        public class userModel
        {
            public string name { get; set; }
            public string surname { get; set; }
            public string username { get; set; }
            public string password { get; set; }
        }
}
