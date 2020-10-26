using System.Collections.Generic;
using System.Net.Sockets;

namespace Server
{
    public class UserSessionsHandler
    {
        public List<User> Users { get; set; }
        public Dictionary<TcpClient, User> Sessions;
        public UserSessionsHandler()
        {
            Sessions = new Dictionary<TcpClient, User>();
            Users = new List<User>();
        }
        
        public User FindUserByUsernamePassword(string aUserName, string aPassword)
        {
            User aUser = null;
            foreach (var user in Users)
            {
                if (user.UserName.Equals(aUserName) && user.Password.Equals(aPassword))
                    aUser = user;
            }

            return aUser;
        }
        
        public User FindUserByUsername(string aUserName)
        {
            User aUser = null;
            foreach (var user in Users)
            {
                if (user.UserName.Equals(aUserName))
                    aUser = user;
            }

            return aUser;
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsLogued { get; set; }

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
        
        
    }
}