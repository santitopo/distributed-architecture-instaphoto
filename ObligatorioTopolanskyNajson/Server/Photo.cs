using System.Collections.Generic;

namespace Server
{
    public class Photo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Dictionary<User, string> Comments { get; set; }

        public Photo()
        {
            Comments = new Dictionary<User, string>();
        }
        
        public Photo(string aName, string aPath)
        {
            Name = aName;
            Path = aPath;
            Comments = new Dictionary<User, string>();
        }
        
    }
}