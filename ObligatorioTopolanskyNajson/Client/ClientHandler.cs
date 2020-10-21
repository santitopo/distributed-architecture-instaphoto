using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            
        public ClientHandler()
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            _fileStreamHandler = new FileStreamHandler();

        }

        public void StartClient()
        {
            _tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 6000);
            _networkStreamHandler = new NetworkStreamHandler(_tcpClient.GetStream());
            _networkStream = _tcpClient.GetStream();
        }

        public void Handler()
        {
            bool keepConnection = true;
            
            while (keepConnection)
            {
                Menu1();
                string input = Console.ReadLine();
                try
                {
                    switch (input)
                    {
                        case "a":
                            Console.WriteLine("\nIngrese usuario y contraseña:\n");
                            string message = Console.ReadLine();
                            Send(CommandConstants.Login, message);
                            Header header = new Header();
                            Receive(header);
                            if (header.ICommand == CommandConstants.OK)
                            {
                                Menu2();
                            }
                            else
                            {
                                Console.WriteLine("Error: {0}", header.IData);
                                break; //Volver al mismo menu
                            }
                            break; 
                        case "b":
                            Console.WriteLine("Message..");
                            Send(CommandConstants.Message, "");
                            break;
                        case "exit":
                            keepConnection = false;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Desconectando debido a un error...");
                    keepConnection = false;
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
        
        
        /*
        public string Receive()
        {
            var dataLength = new byte[4];    //Protocol.WordLength == 4
            var totalReceived = 0;
            while (totalReceived < 4)    //Recibo los primeros 4 bytes(tamaño) para preparar la llegada de datos
            {
                var received = _networkStream.Read(dataLength, totalReceived, 4 - totalReceived);
                if (received == 0) // if receive 0 bytes this means that connection was interrupted between the two points
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }
            //Recibi el tamaño en bytes y lo transformo a entero
            var length = BitConverter.ToInt32(dataLength, 0); 
                    
            //Creo un array de tamaño "length" para recibir el string
            var data = new byte[length];    
            totalReceived = 0;
                    
            //Comienzo a recibir el string
            while (totalReceived < length)
            {
                var received = _networkStream.Read(data, totalReceived, length - totalReceived);
                if (received == 0)
                {
                    throw new SocketException();
                }
                totalReceived += received;
            }
            //Desencripto la palabra escrita en bytes a string
            var word = Encoding.UTF8.GetString(data);
                    
            //Palabra enviada por el server
            Console.WriteLine("Server says: " + word);

            return word;
        }
        */
        
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
                
            }else if(option.Equals("2"))
            {
                Console.WriteLine("Cerrando cliente..");
            }
        }
        
        public void Menu1()
        {
            Console.WriteLine("\n........ AUTENTICACIÓN ........\n" +
                              "(a) - Loguearse\n" +
                              "(b) - Registrar un usuario nuevo\n"+
                              "(exit) - Salir");
        }
        
        public void Menu2()
        {
            Console.WriteLine("\n........ MENU PRINCIPAL ........\n" + 
                              "(d) - Ver Lista de Usuarios\n" +
                              "(e) - Subir una foto\n"+
                              "(f) - Ver comentarios de una foto\n" +
                              "(g) - Agregar un comentario a una foto\n" +
                              "(exit) - Salir");
        }
        
    }
}