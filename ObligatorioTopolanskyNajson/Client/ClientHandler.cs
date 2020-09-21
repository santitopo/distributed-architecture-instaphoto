using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Common.FileHandler;
using Common.FileHandler.Interfaces;
using Common.NetworkUtils;
using Common.NetworkUtils.Interfaces;
using Common.Protocol;

namespace Client
{
    class ClientHandler
    {
        
        private readonly TcpClient _tcpClient;
        private readonly IFileStreamHandler _fileStreamHandler;
        private INetworkStreamHandler _networkStreamHandler;

        public ClientHandler()
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0));
            _fileStreamHandler = new FileStreamHandler();
        }

        public void StartClient()
        {
            _tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 6000);
            _networkStreamHandler = new NetworkStreamHandler(_tcpClient.GetStream());
        }

        public void Chat()
        {
            //Comienzo a enviar datos
            var keepConnection = true;
            using (var networkStream = _tcpClient.GetStream())
            {
                while (keepConnection)
                {
                    //Leo la palabra entrante por consola
                    var word = Console.ReadLine();
                    
                    //La encripto
                    byte[] data = Encoding.UTF8.GetBytes(word);
                    byte[] dataLength = BitConverter.GetBytes(data.Length);
                    
                    //Le mando al servidor: 1. el tamaño del string
                    //                      2. el valor del string
                    networkStream.Write(dataLength, 0, 4);
                    networkStream.Write(data, 0, data.Length);
                    
                    //Me desconecto
                    if (word.Equals("exit"))
                    {
                        keepConnection = false;
                    }
                }
            }

            _tcpClient.Close();
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