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
        private int clientsConnected;
        private readonly TcpListener _tcpListener;
        private readonly IFileHandler _fileHandler;
        private readonly IFileStreamHandler _fileStreamHandler;
        private INetworkStreamHandler _networkStreamHandler;

        private static List<TcpClient> _clients;
        private static bool _exit; 

        public ServerHandler()
        {
            clientsConnected = 0;
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();

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
                            Console.WriteLine("Login...");
                            string[] loginData = header.IData.Split("#");
                            Console.WriteLine("User: {0} ", loginData[0]);
                            Console.WriteLine("Password: {0}", loginData[1]);
                            break;
                        case CommandConstants.ListUsers:
                            Console.WriteLine("List Users...");
                            break;
                        case CommandConstants.Message:
                            Console.WriteLine("Message.. ");
                            break;
                    }
                }
            }
            catch
            {
                // ignored
            }
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
        
        public void Send(string command, NetworkStream networkStream)
        {
            //Comienzo a enviar datos
                    //La encripto
                    byte[] data = Encoding.UTF8.GetBytes(command);
                    byte[] dataLength = BitConverter.GetBytes(data.Length);
                    
                    //Le mando al cliente: 1. el tamaño del string
                    //                     2. el valor del string
                    networkStream.Write(dataLength, 0, 4);
                    networkStream.Write(data, 0, data.Length);
        }
        
    }
}