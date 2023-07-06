using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace InstaPhotoServer
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

        public void AddUser(User aUser)
        {
            Users.Add(aUser);
            Photos.Add(aUser, new List<Photo>());
        }
        
        public void DeleteUser(User aUser)
        {
            foreach (var userPhotos in Photos)
            {
                foreach (var photo in userPhotos.Value)
                {
                    foreach (var comment in photo.Comments)
                    {
                        photo.Comments.RemoveAll(x => x.Item1.Equals(aUser));
                    }
                }
            }
            
            Users.Remove(aUser);
            Photos.Remove(aUser);
        }
        
        public void ModifyUser(User aUser)
        {
            User user = FindUserByUsername(aUser.UserName);
            user.Name = aUser.Name;
            user.Surname = aUser.Surname;
            user.Password = aUser.Password;
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

        public User FindUserByTcpClient(TcpClient aTcpClient)
        {
            User aUser = null;
            
                foreach (var session in Sessions)
                {
                    if (session.Key.Equals(aTcpClient))
                        aUser = session.Value;
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