using System;

namespace Server
{
    public class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsLogued { get; set; }
        public DateTime LastConnection { get; set; }

        public User()
        {
            
        }

        public User(string aName, string aSurname, string aUserName, string aPassword)
        {
            Name = aName;
            Surname = aSurname;
            UserName = aUserName;
            Password = aPassword;
            IsLogued = false;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is User))
            {
                return false;
            }
            return UserName == ((User)obj).UserName;
        }
    }
}