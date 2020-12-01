using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace InstaPhotoServer
{
    class ServerHandler
    {
        private readonly TcpListener _tcpListener;
        private readonly IFileStreamHandler _fileStreamHandler;
        private static Repository _repository;
        private static bool _exit;

        

        public ServerHandler(Repository repository)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(Config.Ipserver), Convert.ToInt32(Config.Portserver));
            _fileStreamHandler = new FileStreamHandler();
            _repository = repository;
            _exit = false;
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
                lock (_repository)
                {
                    _repository.Sessions.Add(tcpClientSocket, null);    //Insert new client without login
                }
                new Thread(() => Handler(tcpClientSocket)).Start(); //Agarro el cliente y lo meto en un hilo
                Console.WriteLine("New client connected...  Total: {0}", _repository.Sessions.Count);
            }
            
            Console.WriteLine("Disconnecting Server");
            foreach (var client in _repository.Sessions)
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
                        case CommandConstants.ListPhotos:
                            ListPhotos(tcpClient, header);
                            break;
                        case CommandConstants.GetComments:
                            GetCommentFunction(tcpClient, header);
                            break;
                        case CommandConstants.AddComment:
                            AddCommentFunction(tcpClient, header);
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

        private void ListPhotos(TcpClient tcpClient, Header header)
        {
            try
            {
                string[] data = header.IData.Split("#");
                string username = data[0];

                List<Photo> associatedPhotos;
                lock (_repository.Photos)
                {
                    associatedPhotos = _repository.FindPhotosByUsername(username);
                }

                List<string> photoName = new List<string>();
                foreach (var photo in associatedPhotos)
                {
                    photoName.Add(photo.Name);
                }

                Send(tcpClient.GetStream(), CommandConstants.OK, JsonSerializer.Serialize(photoName));
            }
            catch (Exception)
            {
                Send(tcpClient.GetStream(), CommandConstants.Error, "Error procesando la solicitud");
            }
        }

        private async void GenerateLog(string message, string level)
        {
            var channel = new ConnectionFactory() {HostName = "localhost"}.CreateConnection().CreateModel();
            channel.QueueDeclare(queue: "log_queue",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var log = new Log();
            log.Level = level;
            log.Message = message;
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
            _repository.Sessions.TryGetValue(tcpClient, out disconnectedUser);
            
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
            
            _repository.Sessions.Remove(tcpClient);
            tcpClient.GetStream().Close();
            tcpClient.Close();
            
            return true;
        }
        private void LoginFunction(TcpClient tcpClient, Header header)
        {
            var networkStream = tcpClient.GetStream();
            try
            {
                string[] loginData = header.IData.Split("#");

                User user;
                lock (_repository)
                {
                    user = _repository.FindUserByUsernamePassword(loginData[0], loginData[1]);
                }

                if (user != null)
                {
                    if (!user.IsLogued)
                    {
                        user.IsLogued = true;
                        user.LastConnection = DateTime.Now;
                        lock (_repository)
                        {
                            _repository.Sessions[tcpClient] = user;
                        }
                        
                        string message = "Sesión iniciada correctamente para el usuario " + loginData[0];
                        GenerateLog(message, LogConstants.Info);
                        Send(networkStream, CommandConstants.OK, "");
                    }
                    else
                    {
                        string message = "La sesión ya esta iniciada para el usuario " + loginData[0];
                        GenerateLog(message, LogConstants.Info);
                        Send(networkStream, CommandConstants.Error, message);
                    }
                }
                else
                {
                    string message = "El usuario "+ loginData[0] +" no existe";
                    GenerateLog(message, LogConstants.Info);
                    Send(networkStream, CommandConstants.Error, message);
                }
            }
            catch (Exception)
            {
                Send(networkStream, CommandConstants.Error, "Error procesando la solicitud");
            }
        }
        private void RegisterFunction(TcpClient tcpClient, Header header)
        {
            string[] registerData = header.IData.Split("#");
            User user = new User(registerData[0],registerData[1], registerData[2], registerData[3]);
            
            lock (_repository.Users)
            {
                if (_repository.FindUserByUsername(user.UserName) == null)
                {
                    _repository.Users.Add(user);
                    _repository.Photos.Add(user, new List<Photo>());
                    
                    string message = "Usuario "+ registerData[2] +" registrado correctamente ";
                    GenerateLog(message, LogConstants.Info);
                    Send(tcpClient.GetStream(), CommandConstants.OK,message);
                }
                else
                {
                    string message = "El nombre de usuario "+ user.UserName +" ya esta en uso";
                    GenerateLog(message, LogConstants.Info);
                    Send(tcpClient.GetStream(), CommandConstants.Error,message );
                }
                    
            }
        }
        private void AddCommentFunction(TcpClient tcpClient, Header header)
        {
            try
            {
                string[] data = header.IData.Split("#");
                string username = data[0];
                string fileName = data[1];
                string comment = data[2];
                Photo selectedPhoto;
                lock (_repository.Photos)
                {
                    List<Photo> associatedPhotos = _repository.FindPhotosByUsername(username);
                    selectedPhoto = associatedPhotos.Find(x => x.Name == fileName);

                    if (selectedPhoto != null)
                    {
                        User thisUser = _repository.FindUserByTcpClient(tcpClient);
                        Tuple<User, string> userComment = new Tuple<User, string>(thisUser, comment);
                        selectedPhoto.Comments.Add(userComment);
                        
                        string message = "Comentario agregado correctamente a la foto: "+ selectedPhoto.Name;
                        GenerateLog(message, LogConstants.Info);
                        Send(tcpClient.GetStream(), CommandConstants.OK, "");
                    }
                    else
                    {
                        string message = "No se encontró la foto seleccionada: "+ selectedPhoto.Name;
                        GenerateLog(message, LogConstants.Info);
                        Send(tcpClient.GetStream(), CommandConstants.Error, message);
                    }
                }
            }
            catch (Exception)
            {
                Send(tcpClient.GetStream(), CommandConstants.Error, "Error procesando su solicitud");
            }
        }
        private void GetCommentFunction(TcpClient tcpClient, Header header)
        {
            User username = _repository.FindUserByTcpClient(tcpClient);
            string fileName = header.IData;

            Photo selectedPhoto;
            lock (_repository.Photos)
            {
                List<Photo> associatedPhotos = _repository.FindPhotosByUsername(username.UserName);
                selectedPhoto = associatedPhotos.Find(x => x.Name == fileName);
            }

            if (selectedPhoto != null)
            {
                List<string> comments = new List<string>();
                foreach (var comment in selectedPhoto.Comments)
                {
                    comments.Add(comment.Item1.Name +" - "+ comment.Item2);
                }
                
                string message = "Comentarios correctamente mostrados para la foto: "+ selectedPhoto.Name;
                GenerateLog(message, LogConstants.Info);
                
                Send(tcpClient.GetStream(), CommandConstants.OK, JsonSerializer.Serialize(comments));
            }
            else
            {
                string message = "No se encontró la foto consultada: "+ selectedPhoto.Name;
                GenerateLog(message, LogConstants.Info);
                Send(tcpClient.GetStream(), CommandConstants.Error, message);
            }
            
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

                    string path = Config.ImagesFolder + fileName;
                    
                    _fileStreamHandler.Write(path, data);
                    currentPart++;
                }
                
                //Add new photo to the repo
                Photo photo = new Photo(fileName,Config.ImagesFolder + fileName);
                lock (_repository)
                {
                    User user = _repository.Sessions[tcpClient];
                    List<Photo> asocciatedPhotos = _repository.Photos[user];
                    asocciatedPhotos.Add(photo);
                }
                string message = "Imagen: "+ fileName +" recibida...";
                GenerateLog(message, LogConstants.Info);
        }
        private void ListUserFunction(TcpClient tcpClient, Header header)
        {
            var networkStream = tcpClient.GetStream();
            List<User> users = _repository.Users.ToList();
            var sessions = JsonSerializer.Serialize(users);
            
            string message = "Mostrando lista de usuarios..";
            GenerateLog(message, LogConstants.Info);
            Send(networkStream, CommandConstants.OK, sessions);
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