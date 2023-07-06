using System;
using System.Collections.Generic;

namespace InstaPhotoServer
{
    public class Photo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Tuple<User, string>> Comments { get; set; }

        public Photo()
        {
            Comments = new List<Tuple<User, string>>();
        }
        
        public Photo(string aName, string aPath)
        {
            Name = aName;
            Path = aPath;
            Comments = new List<Tuple<User, string>>();
        }
        
    }
}