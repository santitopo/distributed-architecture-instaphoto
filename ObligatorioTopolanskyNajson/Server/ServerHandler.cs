using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;

namespace Server
{
    class ServerHandler
    {
        private int clientsConnected;
        private readonly TcpListener _tcpListener;
        private readonly IFileHandler _fileHandler;
        private readonly IFileStreamHandler _fileStreamHandler;
        private INetworkStreamHandler _networkStreamHandler;

        public ServerHandler()
        {
            clientsConnected = 0;
            _tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
            _fileHandler = new FileHandler();
            _fileStreamHandler = new FileStreamHandler();
        }

        public void StartServer()
        {
            Console.WriteLine("Waiting for client...");
            _tcpListener.Start(10);
            
            while(true) // This while (true) should only be valid for examples
            {
                //TcpClient permite establecer una conexion TCP entre dos IPEndPoint. 
                var tcpClientSocket = _tcpListener.AcceptTcpClient(); // Gets the first client in the queue
                new Thread(() => Handler(tcpClientSocket, clientsConnected +=1)).Start(); //Agarro el primer cliente y lo meto en un hilo
                Console.WriteLine("Client connected - Total Clients: {0}", clientsConnected);
            }
        }
        
        public void Handler(TcpClient tcpClient, int clientId)
        {    
            var isClientConnected = true;
            try
            {
                var networkStream = tcpClient.GetStream();
                Send("RES000000", networkStream);    //Envio Menu1 a traves de comandos
                
                while (isClientConnected)
                {
                    var command = Receive(clientId, networkStream);

                    if (command.Equals("REQ990000"))    //Exit command
                    {
                        isClientConnected = false;
                        Console.WriteLine("Client is leaving");
                        Console.WriteLine("Total Clients: {0}", clientsConnected -=1);
                    }
                    else     //Some command
                    {
                        switch (command)
                        {
                            case "00":
                                Console.WriteLine("Opcion 1 - Menu 1");
                                Send("RES010000", networkStream);
                                break;
                            default:
                                isClientConnected = false;
                                Console.WriteLine("Client is leaving");
                                Console.WriteLine("Total Clients: {0}", clientsConnected -=1);
                            break;
                        }
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("The client connection was interrupted");
            }
        }

        public string Receive(int clientId,NetworkStream networkStream)
        {
            var dataLength = new byte[4];    //Protocol.WordLength == 4
                    var totalReceived = 0;
                    while (totalReceived < 4)    //Recibo los primeros 4 bytes(tamaño) para preparar la llegada de datos
                    {
                        var received = networkStream.Read(dataLength, totalReceived, 4 - totalReceived);
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
                        var received = networkStream.Read(data, totalReceived, length - totalReceived);
                        if (received == 0)
                        {
                            throw new SocketException();
                        }
                        totalReceived += received;
                    }
                    //Desencripto la palabra escrita en bytes a string
                    var word = Encoding.UTF8.GetString(data);
                    
                    //Palabra enviada por el cliente
                    Console.WriteLine("Client {0} says: " + word, clientId);

                    return word;
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
        
        /*
         public void SendFile(string path)
        {
            //1. Obtenemos nombre y largo del archivo
            //2. Envio largo del nombre y tamaño del archivo
            //3. Envio nombre del archivo
            //4. Envio el archivo de a partes (cada paerte 32kb o menos)
            
            long fileSize = _fileHandler.GetFileSize(path);
            string fileName = _fileHandler.GetFileName(path);
            var header = new Header().Create(fileName, fileSize);
            _networkStreamHandler.Write(header);

            _networkStreamHandler.Write(Encoding.UTF8.GetBytes(fileName));

            long parts = SpecificationHelper.GetParts(fileSize);
            Console.WriteLine("Will Send {0} parts",parts);
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
        }
        */
    }
}