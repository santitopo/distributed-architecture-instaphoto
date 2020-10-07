using System;
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
                string input = Console.ReadLine();
                switch (input)
                {
                    case "exit":
                        break;
                    case "1":
                        Console.WriteLine("Ingrese usuario y contraseña: \n");
                        string message = Console.ReadLine();
                        Send(CommandConstants.Login, message);
                        break;
                    case "2":
                        Console.WriteLine("Otra cosa..");
                        break;
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
            //_networkStream.Write(Encoding.UTF8.GetBytes(message), 0, message.Length);
        }
        
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
        
        public void Menu0()
        {
            Console.WriteLine("Bienvenido al sistema \n" + "1- Conectarse al servidor \n 2- Salir");

            string option = Console.ReadLine();
            
                if (option.Equals("1"))
                {
                    Console.WriteLine("Connecting to server...");
                    this.StartClient();
                    Console.WriteLine("Connected to server");
                    this.Handler();
                    
                }else if(option.Equals("2"))
                {
                    Console.WriteLine("Closing client..");
                }
        }
        
         /*
        public void ReceiveFile()
        {
            //1. Recibimos el header para ver el nombre y largo del archivo
            //2. Recibo el nombre del archivo
            //3. Calculo partes a recibir
            //4. Recibo archivos por partes
            
            var header = _networkStreamHandler.Read(Header.GetLength());
            var fileNameSize = BitConverter.ToInt32(header, 0);
            var fileSize = BitConverter.ToInt64(header, Specification.FixedFileNameLength);

            var fileName = Encoding.UTF8.GetString(_networkStreamHandler.Read(fileNameSize));

            long parts = SpecificationHelper.GetParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart == parts)
                {
                    var lastPartSize = (int)(fileSize - offset);
                    data = _networkStreamHandler.Read(lastPartSize);
                    offset += lastPartSize;
                }
                else
                {
                    data = _networkStreamHandler.Read(Specification.MaxPacketSize);
                    offset += Specification.MaxPacketSize;
                }
                _fileStreamHandler.Write(fileName, data);
                currentPart++;
            }
        }
        */
    }
}