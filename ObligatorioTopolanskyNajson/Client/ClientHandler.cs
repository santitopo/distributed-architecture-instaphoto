using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.Config;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using ProtocolLibrary;

namespace Client
{
    class ClientHandler
    {
        
        private readonly TcpClient _tcpClient;
        private readonly IFileStreamHandler _fileStreamHandler;
        private INetworkStreamHandler _networkStreamHandler;
        private NetworkStream _networkStream;
        private static bool _keepConnection;
        
        public ClientHandler()
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse(Config.Ipclient), Convert.ToInt32(Config.Portclient)));
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
                Menu2();  //Meterse en otro switch
            }
            else
            {
                Console.WriteLine("Error: {0}", header.IData);
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
                Console.WriteLine("Error: {0}", header.IData);
            }
        }
        
        public void Menu2()
        {
            while (_keepConnection)
            {
                Console.WriteLine("\n........ MENU PRINCIPAL ........\n" + 
                                  "(a) - Ver Lista de Usuarios\n" +
                                  "(b) - Subir una foto\n"+
                                  "(c) - Ver comentarios de una foto\n" +
                                  "(d) - Agregar un comentario a una foto\n" +
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
                        GetCommentsFunction();
                        break; 
                    case "d":
                        AddCommentFunction();
                        break;
                    case "exit":
                        ExitFunction();
                        _keepConnection = false;
                        break;
                }
            }
   
            
        }

        private void ExitFunction()
        {
            Send(CommandConstants.Exit, "");
        }

        private void AddCommentFunction()
        {
            throw new NotImplementedException();
        }

        private void GetCommentsFunction()
        {
            throw new NotImplementedException();
        }

        private void UploadPictureFunction()
        {
            throw new NotImplementedException();
        }

        private void ListUsersFunction()
        {
            throw new NotImplementedException();
        }
    }
}