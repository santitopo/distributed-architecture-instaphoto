using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common.Config;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using Common.Protocol;
using ProtocolLibrary;
using Server;
using Shared;

namespace Client
{
    class ClientHandler
    {
        
        private readonly TcpClient _tcpClient;
        private readonly IFileStreamHandler _fileStreamHandler;
        private readonly IFileHandler _fileHandler;
        private INetworkStreamHandler _networkStreamHandler;
        private NetworkStream _networkStream;
        private static bool _keepConnection;
        
        public ClientHandler()
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(Config.Ipclient), Convert.ToInt32(Config.Portclient)));
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();

        }

        public void StartClient()
        {
            _tcpClient.Connect(IPAddress.Parse(Config.Ipserver), Convert.ToInt32(Config.Portserver));
            _networkStreamHandler = new NetworkStreamHandler(_tcpClient.GetStream());
            _networkStream = _tcpClient.GetStream();
        }

        public void Handler()
        {
            _keepConnection = true;
            
            while (_keepConnection)
            {
                try
                {
                    Menu1();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Desconectando debido a un error...");
                    _keepConnection = false;
                }
            }
        }

        public void Send( int command, string message)
        {
            var header = new Header(HeaderConstants.Request, command, message.Length);  //REQ030004
            var data = header.GetRequest();  //REQ030004 escrito en bytes
            
            //Comienzo a enviar datos
            //Le mando al servidor: 1. El comando
            //                      2. El data del comando (si es que existe)
            _networkStream.Write(data, 0, data.Length);
            if (message.Length > 0)
            {
                _networkStream.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
            }
        }
        
        public void Receive(Header header)
        {
            var command = new byte[9];    //Protocol.WordLength == 9
            var totalReceived = 0;
            while (totalReceived < 9)    //Recibo los primeros 9 bytes(tamaño) para preparar la llegada de datos
            {
                var received = _networkStream.Read(command, totalReceived, 9 - totalReceived);
                if (received == 0) // if receive 0 bytes this means that connection was interrupted between the two points
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }

            //Desencripto el comando
            header.DecodeData(command);

            if (header.IDataLength > 0)
            {
                //Creo un array de tamaño "length" para recibir el string
                var data = new byte[header.IDataLength];    
                totalReceived = 0;
                
                //Comienzo a recibir el string (si es que hay datos)
                while (totalReceived < header.IDataLength)
                {
                    var received = _networkStream.Read(data, totalReceived, header.IDataLength - totalReceived);
                    if (received == 0)
                    {
                        throw new SocketException();
                    }
                    totalReceived += received;
                }

                header.IData = Encoding.UTF8.GetString(data);
            }
        }
        
        public void Menu0()
        {
            Console.WriteLine("\nBienvenido al sistema \n" + "(1) - Conectarse al servidor\n(2) - Salir");
                
            string option = Console.ReadLine();
            
            if (option.Equals("1"))
            {
                Console.WriteLine("Conectando al servidor...");
                this.StartClient();
                Console.WriteLine("Conectado al servidor");
                this.Handler();
                Console.WriteLine("Hasta luego...");
                
            }else if(option.Equals("2"))
            {
                Console.WriteLine("Cerrando cliente..");
            }
        }
        
        public void Menu1()
        {
            while (_keepConnection)
            {
                Console.WriteLine("\n........ AUTENTICACIÓN ........\n" +
                                  "(a) - Loguearse\n" +
                                  "(b) - Registrar un usuario nuevo\n"+
                                  "(exit) - Salir");
                
                string input = Console.ReadLine();
                
                switch (input)
                {
                    case "a":
                        LoginFunction();
                        break; 
                    case "b":
                        RegisterFunction();
                        break;
                    case "exit":
                        ExitFunction();
                        _keepConnection = false;
                        break;
                }
            }
            
        }
        private void LoginFunction()
        {
            Console.WriteLine("\nIngrese usuario y contraseña separados por '#':\n");
            string input = Console.ReadLine();
            Send(CommandConstants.Login, input);
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Menu2();  
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void RegisterFunction()
        {
            Console.WriteLine("\nIngrese nombre, apellido, usuario y contraseña separados por '#':\n");
            string input = Console.ReadLine();
            Send(CommandConstants.Register, input);
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Console.WriteLine(header.IData);
            } 
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        
        public void Menu2()
        {
            while (_keepConnection)
            {
                Console.WriteLine("\n........ MENU PRINCIPAL ........\n" + 
                                  "(a) - Ver lista de Usuarios\n" +
                                  "(b) - Subir una foto\n"+
                                  "(c) - Ver y comentar fotos de usuarios\n"+
                                  "(d) - Ver comentarios de mis foto\n" +
                                  "(exit) - Desconectarse");
                
                string input = Console.ReadLine();
                
                switch (input)
                {
                    case "a":
                        ListUsersFunction();
                        break; 
                    case "b":
                        UploadPictureFunction();
                        break;
                    case "c":
                        GetPhotosFunction();
                        break; 
                    case "d":
                        GetCommentsFunction();
                        break;
                    case "exit":
                        ExitFunction();
                        _keepConnection = false;
                        break;
                }
            }
   
            
        }

        private void GetPhotosFunction()
        {
            Console.WriteLine("\nIngrese el usuario el cual desea ver las fotos:\n");
            string username = Console.ReadLine();
            Send(CommandConstants.ListPhotos, username);
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Mostrando fotos del usuario: {0} \n", username);
                var photos = JsonSerializer.Deserialize<List<string>>(header.IData);
                foreach (var photoName in photos)
                {
                    Console.WriteLine("- {0}", photoName);
                }
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n....OPCIONES....\n" + 
                                  "(a) - Comentar una foto\n" +
                                  "(b) - Volver\n");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "a":
                        AddCommentFunction(username);
                        break;
                }
            } 
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void ExitFunction()
        {
            Send(CommandConstants.Exit, "");
        }

        private void AddCommentFunction(string username)
        {
            Console.WriteLine("\nIngrese el nombre de la foto (con terminación) y comentario, separados por # \n");
            string input = Console.ReadLine();
            string data = username + "#" + input;
            Send(CommandConstants.AddComment, data);
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Comentario agregado correctamente\n");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        private void GetCommentsFunction()
        {
            Console.WriteLine("\nIngrese el nombre de la foto (con terminación) para ver los comentarios" +
                              "\n Nota: Solo es posible ver comentarios en sus propias fotos");
            string data = Console.ReadLine();
            Send(CommandConstants.GetComments, data);
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Mostrando los comentarios de la foto: {0} \n", data);
                var comments = JsonSerializer.Deserialize<List<string>>(header.IData);
                foreach (var comment in comments)
                {
                    Console.WriteLine(comment);
                }
                Console.ForegroundColor = ConsoleColor.White;
            } 
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        public void UploadPictureFunction()
        {
            Console.WriteLine("Ingrese la ruta completa de la foto a transferir:");
            string path = Console.ReadLine();
            
            
            IFileHandler fileHandler = new FileHandler();
            while(path == null || path.Equals(string.Empty) || !fileHandler.FileExists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ruta invalida, vuelva a ingresar una ruta correcta...");
                Console.ForegroundColor = ConsoleColor.White;
                path = Console.ReadLine();
            }
            
            //1. Obtenemos nombre y largo del archivo
            long fileSize = _fileHandler.GetFileSize(path);
            string fileName = _fileHandler.GetFileName(path);

            string fileInfo = fileName +"#"+ fileSize;
            
            //2. Envio el nombre y tamaño del archivo
             Send(CommandConstants.UploadPicture, fileInfo);

            //4. Envio el archivo de a partes (cada paerte 32kb o menos)
            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.Read(path, offset, lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _fileStreamHandler.Read(path, offset, Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }

                _networkStreamHandler.Write(data);
                currentPart++;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Imagen: {0} enviada...", fileName);
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        private void ListUsersFunction()
        {
            Send(CommandConstants.ListUsers, "");
            Header header = new Header();
            Receive(header);
            if (header.ICommand == CommandConstants.OK)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Usuarios Conectados:\n");
                var sessions = JsonSerializer.Deserialize<List<User>>(header.IData);
                foreach (var user in sessions)
                {
                    if (user != null)
                    {
                        Console.WriteLine("Usuario: {0}",user.UserName);
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: {0}", header.IData);
                Console.ForegroundColor = ConsoleColor.White;
            }
            
        }
    }
}