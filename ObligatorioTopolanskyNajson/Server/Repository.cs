using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Server
{
    public class Repository
    {
        public List<User> Users { get; set; }
        public Dictionary<TcpClient, User> Sessions;
        public Dictionary<User, List<Photo>> Photos;

        public Repository()
        {
            Sessions = new Dictionary<TcpClient, User>();
            Users = new List<User>();
            Photos = new Dictionary<User, List<Photo>>();
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
        public List<Photo> FindPhotosByUsername(string aUserName)
        {
            User user = FindUserByUsername(aUserName);
            List<Photo> asocciatedPhotos = Photos[user];
            return asocciatedPhotos;
        }
    }

}