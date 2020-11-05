using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Config;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using Common.Protocol;
using ProtocolLibrary;
using RabbitMQ.Client;
using Shared;

namespace Server
{
    class ServerHandler
    {
        private readonly TcpListener _tcpListener;
        private readonly IFileHandler _fileHandler;
        private readonly IFileStreamHandler _fileStreamHandler;
        private static UserSessionsHandler _userSessions;
        private static List<TcpClient> _clients;
        private static bool _exit;

        

        public ServerHandler(UserSessionsHandler userSessions)
        {
            
            _tcpListener = new TcpListener(IPAddress.Parse(Config.Ipserver), Convert.ToInt32(Config.Portserver));
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
            _userSessions = userSessions;
            _exit = false;
            _clients = new List<TcpClient>();
        }

        public void StartServer()
        {
            Console.WriteLine("Waiting for client...");
            _tcpListener.Start(10);
            
            var threadConnections = new Thread(()=> ListenForConnections());
            threadConnections.Start();

            while (!_exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "exit":
                        _exit = true;
                        
                        TcpClient tcpClientTrap = new TcpClient(new IPEndPoint(IPAddress.Parse(Config.Ipclient), Convert.ToInt32(Config.Portclient)));
                        tcpClientTrap.Connect(IPAddress.Parse(Config.Ipserver), Convert.ToInt32(Config.Portserver));
                        break;
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }

        public void ListenForConnections()
        {
            while(!_exit)
            {
                var tcpClientSocket = _tcpListener.AcceptTcpClient(); // Gets the first client in the queue
                lock (_userSessions)
                {
                    _userSessions.Sessions.Add(tcpClientSocket, null);    //Insert new client without login
                }
                new Thread(() => Handler(tcpClientSocket)).Start(); //Agarro el cliente y lo meto en un hilo
                Console.WriteLine("New client connected...  Total: {0}", _userSessions.Sessions.Count);
            }
            
            Console.WriteLine("Disconnecting Server");
            foreach (var client in _userSessions.Sessions)
            {
                TcpClient socket = client.Key;
                socket.GetStream().Close();
                socket.Close();
            }
            
            _tcpListener.Stop();
        }
        
        public void Handler(TcpClient tcpClient)
        {    
            try
            {
                bool localExit = false;
                while (!_exit && !localExit)
                {
                    var headerLength = HeaderConstants.Request.Length +
                                       HeaderConstants.CommandLength +
                                       HeaderConstants.DataLength;

                    var header = new Header();
                    Receive(tcpClient.GetStream(), header);  //La data entrante se guarda en el header

                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            LoginFunction(tcpClient, header);
                            break;
                        case CommandConstants.Register:
                            RegisterFunction(tcpClient, header);
                            break;
                        case CommandConstants.ListUsers:
                            ListUserFunction(tcpClient, header);
                            break;
                        case CommandConstants.UploadPicture:
                            UploadPictureFunction(tcpClient, header);
                            break;
                        case CommandConstants.GetComment:
                            GetCommentFunction(tcpClient, header);
                            break;
                        case CommandConstants.AddComment:
                            AddCommentFunction(tcpClient, header);
                            break;
                        case CommandConstants.GenerateLog:
                            GenerateLog(tcpClient, header);
                            break;
                        case CommandConstants.Exit:
                            localExit = ClientExitFunction(tcpClient);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                if(!_exit)
                    Console.WriteLine(e.Message);
            }
        }

        private async Task GenerateLog(TcpClient tcpClient, Header header)
        {
            
            var channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            string[] logInfo = header.IData.Split("#");

            var log = new Log();
            log.Level = logInfo[0];
            log.Message = logInfo[1];
            log.DateTime = DateTime.Now;
            
            var stringLog = JsonSerializer.Serialize(log);
            await SendMessage(channel, stringLog);
        }
        
        private static Task<bool> SendMessage(IModel channel, string message)
        {
            bool returnVal;
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "",
                    routingKey: "log_queue",
                    basicProperties: null,
                    body: body);
                returnVal = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                returnVal = false;
            }

            return Task.FromResult(returnVal);
        }


        private bool ClientExitFunction(TcpClient tcpClient)
        {
            //Find user in map
            User disconnectedUser;
            _userSessions.Sessions.TryGetValue(tcpClient, out disconnectedUser);
            
            //Disconnecting client
            if (disconnectedUser != null)
            {
                Console.WriteLine("Desconectando cliente: {0} \n", disconnectedUser.UserName);
                disconnectedUser.IsLogued = false;
            }
            else
            {
                Console.WriteLine("Desconectando cliente no identificado... \n");
            }
            
            _userSessions.Sessions.Remove(tcpClient);
            tcpClient.GetStream().Close();
            tcpClient.Close();
            
            return true;
        }

        private void LoginFunction(TcpClient tcpClient, Header header)
        {
            var networkStream = tcpClient.GetStream();
            string[] loginData = header.IData.Split("#");
            
            User user;
            lock (_userSessions)
            {
                 user = _userSessions.FindUserByUsernamePassword(loginData[0], loginData[1]);
            }
            
            if (user != null)
            {
                if (!user.IsLogued)
                {
                    user.IsLogued = true; 
                    lock (_userSessions)
                    {
                        _userSessions.Sessions[tcpClient] = user;
                    }
                    Send(networkStream, CommandConstants.OK, "");
                }
                else
                {
                    string message = "La sesión ya esta iniciada para el usuario " + loginData[0];
                    Send(networkStream, CommandConstants.Error, message);
                }
            }
            else
            {
                string message = "El usuario no existe";
                Send(networkStream, CommandConstants.Error, message);
            }
        }
        private void RegisterFunction(TcpClient tcpClient, Header header)
        {
            string[] registerData = header.IData.Split("#");
            
            User user = new User(registerData[0],registerData[1], registerData[2], registerData[3]);
            
            lock (_userSessions.Users)
            {
                if (_userSessions.FindUserByUsername(user.UserName) == null)
                {
                    _userSessions.Users.Add(user);
                    string message = "Usuario registrado correctamente";
                    Send(tcpClient.GetStream(), CommandConstants.OK,message);
                }
                else
                {
                    string message = "El nombre de usuario "+ user.UserName +" ya esta en uso";
                    Send(tcpClient.GetStream(), CommandConstants.Error,message );
                }
                    
            }
        }
        private void AddCommentFunction(TcpClient tcpClient, Header header)
        {
            throw new NotImplementedException();
        }
        private void GetCommentFunction(TcpClient tcpClient, Header header)
        {
            throw new NotImplementedException();
        }
        private void UploadPictureFunction(TcpClient tcpClient, Header header)
        {
                NetworkStreamHandler networkStreamHandler = new NetworkStreamHandler(tcpClient.GetStream());
            
                //1. Recibimos el header para ver el nombre y largo del archivo
                string[] pictureInfo = header.IData.Split("#");
                
                var fileName = pictureInfo[0]; 
                var fileSize = Convert.ToInt32(pictureInfo[1]); 

                //2. Calculo partes a recibir
                long parts = SpecificationHelper.GetParts(fileSize);
                long offset = 0;
                long currentPart = 1;
                
                //3. Recibo archivos por partes
                while (fileSize > offset)
                {
                    byte[] data;
                    if (currentPart == parts)
                    {
                        var lastPartSize = (int)(fileSize - offset);
                        data = networkStreamHandler.Read(lastPartSize);
                        offset += lastPartSize;
                    }
                    else
                    {
                        data = networkStreamHandler.Read(Specification.MaxPacketSize);
                        offset += Specification.MaxPacketSize;
                    }
                    _fileStreamHandler.Write(fileName, data);
                    currentPart++;
                }
        }
        private void ListUserFunction(TcpClient tcpClient, Header header)
        {
            throw new NotImplementedException();
        }
        

        public void Receive(NetworkStream networkStream, Header header)
        {
                var command = new byte[9];    //Protocol.WordLength == 9
                var totalReceived = 0;
                while (totalReceived < 9)    //Recibo los primeros 9 bytes(tamaño) para preparar la llegada de datos
                {
                    var received = networkStream.Read(command, totalReceived, 9 - totalReceived);
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
                        var received = networkStream.Read(data, totalReceived, header.IDataLength - totalReceived);
                        if (received == 0)
                        {
                            throw new SocketException();
                        }
                        totalReceived += received;
                    }

                    header.IData = Encoding.UTF8.GetString(data);
                }
        }
        public void Send(NetworkStream networkStream, int command, string message)
        {
            var header = new Header(HeaderConstants.Response, command, message.Length);  //REQ030004
            var data = header.GetRequest();  //REQ030004 escrito en bytes
            
            //Comienzo a enviar datos
            //Le mando al servidor: 1. El comando
            //                      2. El data del comando (si es que existe)
            networkStream.Write(data, 0, data.Length);
            if (message.Length > 0)
            {
                networkStream.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
            }
        }


    }
}