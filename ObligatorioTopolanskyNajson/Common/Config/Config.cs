using System;
using System.Collections.Generic;

namespace Common.Config
{
    public static class Config
    {
        public static string Ipserver;
        public static string Portserver;
        public static string Ipclient;
        public static string Portclient;
        public static string ImagesFolder;
        public static string RootUser; 
        public static string RootPassword; 
        public static string GrpcServerIp; 
        public static string GrpcPort; 
        public static string UsersAPIUri; 
        public static string LogsAPIUri; 
        public static string QueueName; 

        public static void StartConfiguration(string configPath)
        {
            try
            {
                string[] lines =
                    System.IO.File.ReadAllLines(
                        configPath);

                Dictionary<string, string> values = new Dictionary<string, string>();
                foreach (var line in lines)
                {
                    values.Add(line.Split("#")[0],line.Split("#")[1]);
                }

                Ipserver = values["ipserver"];
                Portserver = values["portserver"];
                Ipclient = values["ipclient"];
                Portclient = values["portclient"];
                ImagesFolder = values["imagesFolder"];
                RootUser = values["rootUser"];
                RootPassword = values["rootPassword"];
                GrpcServerIp = values["grpcServerIp"];
                GrpcPort = values["grpcPort"];
                UsersAPIUri = values["usersAPIUri"];
                LogsAPIUri = values["logsAPIUri"];
                QueueName = values["queueName"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}