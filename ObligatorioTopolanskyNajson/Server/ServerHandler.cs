using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using ProtocolLibrary;

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
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
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
                        
                        TcpClient tcpClientTrap = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
                        tcpClientTrap.Connect(IPAddress.Parse("127.0.0.1"), 6000);
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
                _clients.Add(tcpClientSocket);
                new Thread(() => Handler(tcpClientSocket)).Start(); //Agarro el cliente y lo meto en un hilo
                Console.WriteLine("New client connected");
            }
            
            Console.WriteLine("Disconnecting Server");
            foreach (var client in _clients)
            {
                client.GetStream().Close();
                client.Close();
            }
            
            _tcpListener.Stop();
        }
        
        public void Handler(TcpClient tcpClient)
        {    
            try
            {
                var networkStream = tcpClient.GetStream();
                while (!_exit)
                {
                    var headerLength = HeaderConstants.Request.Length +
                                       HeaderConstants.CommandLength +
                                       HeaderConstants.DataLength;

                    var header = new Header();
                    Receive(networkStream, header);  //La data entrante se guarda en el header

                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                            LoginFunction(networkStream, header);
                            break;
                        case CommandConstants.Register:
                            RegisterFunction(networkStream, header);
                            break;
                        case CommandConstants.ListUsers:
                            ListUserFunction(networkStream, header);
                            break;
                        case CommandConstants.UploadPicture:
                            UploadPictureFunction(networkStream, header);
                            break;
                        case CommandConstants.GetComment:
                            GetCommentFunction(networkStream, header);
                            break;
                        case CommandConstants.AddComment:
                            AddCommentFunction(networkStream, header);
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

        private void LoginFunction(NetworkStream networkStream, Header header)
        {
            Console.WriteLine("Login...");
            string[] loginData = header.IData.Split("#");
            //AccesoConcurrente
            User user = _userSessions.FindUserByUsernamePassword(loginData[0], loginData[1]);
            if (user != null)
            {
                if (!user.IsLogued)
                {
                    user.IsLogued = true; //(AccesoConcurrente)
                    Send(networkStream, CommandConstants.OK, "");
                }
                else
                {
                    string message = "La sesion ya esta iniciada para el usuario " + loginData[0];
                    Send(networkStream, CommandConstants.Error, message);
                }
            }
            else
            {
                string message = "El usuario no existe";
                Send(networkStream, CommandConstants.Error, message);
            }
        }
        private void AddCommentFunction(NetworkStream networkStream, Header header)
        {
            throw new NotImplementedException();
        }
        private void GetCommentFunction(NetworkStream networkStream, Header header)
        {
            throw new NotImplementedException();
        }
        private void UploadPictureFunction(NetworkStream networkStream, Header header)
        {
            throw new NotImplementedException();
        }
        private void ListUserFunction(NetworkStream networkStream, Header header)
        {
            throw new NotImplementedException();
        }
        private void RegisterFunction(NetworkStream networkStream, Header header)
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