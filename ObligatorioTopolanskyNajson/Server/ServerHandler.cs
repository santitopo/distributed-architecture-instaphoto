using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using Common.Protocol;

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
            _tcpListener.Start(10);
            
            while(true) // This while (true) should only be valid for examples
            {
                //TcpClient permite establecer una conexion TCP entre dos IPEndPoint. 
                var tcpClientSocket = _tcpListener.AcceptTcpClient(); // Gets the first client in the queue
                new Thread(() => Chat(tcpClientSocket, clientsConnected +=1)).Start(); //Agarro el primer cliente y lo meto en un hilo
                Console.WriteLine("Client connected - Total Clients: {0}", clientsConnected);
            }
        }
        
        public void Chat(TcpClient tcpClient, int clientId)
        {    
            
            var isClientConnected = true;
            try
            {
                var networkStream = tcpClient.GetStream();
                while (isClientConnected)
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
                    
                    if (word.Equals("exit"))    //Comando de salida
                    {
                        isClientConnected = false;
                        Console.WriteLine("Client is leaving");
                        Console.WriteLine("Total Clients: {0}", clientsConnected -=1);
                    }
                    else    //Palabra enviada por el cliente
                    {
                        Console.WriteLine("Client {0} says: " + word, clientId);
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("The client connection was interrupted");
            }
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