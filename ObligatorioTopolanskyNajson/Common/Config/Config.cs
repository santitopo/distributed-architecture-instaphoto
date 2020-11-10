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

        public static void StartConfiguration()
        {
            string[] lines =
                System.IO.File.ReadAllLines(
                    @"..\\..\\..\\..\\config.txt");

            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (var line in lines)
            {
                values.Add(line.Split(":")[0],line.Split(":")[1]);
            }

            Ipserver = values["ipserver"];
            Portserver = values["portserver"];
            Ipclient = values["ipclient"];
            Portclient = values["portclient"];
            ImagesFolder = values["imagesFolder"];
        }
    }
}