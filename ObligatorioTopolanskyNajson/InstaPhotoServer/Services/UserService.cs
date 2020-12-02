using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using InstaPhotoServer;
using Microsoft.Extensions.Logging;

namespace InstaPhotoServer
{
    public class UsersService : ABMUsers.ABMUsersBase
    {
        private readonly ILogger<UsersService> _logger;
        
        public UsersService(ILogger<UsersService> logger)
        {
            _logger = logger;
        }

        public override Task<ListUser> GetUsers(Empty request, ServerCallContext context)
        {
            var list = new ListUser();
            lock (ServerHandler._repository.Users)
            {
                List<User> users = ServerHandler._repository.Users;
                foreach (var user in users)
                {
                    list.Users.Add(new UserModel()
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Username = user.UserName,
                        Password = user.Password
                    });
                }
            }

            return Task.FromResult(list);
        }
        
        public override Task<InfoResponse> AddUser(UserModel request, ServerCallContext context)
        {
            UserModel userModel = request;
            lock (ServerHandler._repository.Users)
            {
                if (ServerHandler._repository.FindUserByUsername(userModel.Username) == null)
                {
                    User newUser = new User(userModel.Name, userModel.Surname, userModel.Username, userModel.Password);
                    ServerHandler._repository.AddUser(newUser);
                    
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "Usuario "+ userModel.Username +" registrado correctamente "
                        });
                }
                else
                {
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "El nombre de usuario "+ userModel.Username +" ya esta en uso"
                        });
                }
            }
        }
        
        public override Task<InfoResponse> ModifyUser(UserModel request, ServerCallContext context)
        {
            UserModel userModel = request;
            lock (ServerHandler._repository.Users)
            {
                if (ServerHandler._repository.FindUserByUsername(userModel.Username) != null)
                {
                    User modifiedUser = new User(userModel.Name, userModel.Surname, userModel.Username, userModel.Password);
                    ServerHandler._repository.ModifyUser(modifiedUser);
                    
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "Usuario "+ userModel.Username +" modificado correctamente "
                        });
                }
                else
                {
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "El usuario "+ userModel.Username +" no existe"
                        });
                }
            }
        }

        public override Task<InfoResponse> DeleteUser(UserModel request, ServerCallContext context)
        {
            UserModel userModel = request;
            lock (ServerHandler._repository.Users)
            {
                if (ServerHandler._repository.FindUserByUsername(userModel.Username) != null)
                {
                    User modifiedUser = new User(userModel.Name, userModel.Surname, userModel.Username, userModel.Password);
                    ServerHandler._repository.DeleteUser(modifiedUser);
                    
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "Usuario "+ userModel.Username +" eliminado correctamente "
                        });
                }
                else
                {
                    return Task.FromResult(
                        new InfoResponse
                        {
                            Message = "El usuario "+ userModel.Username +" no existe"
                        });
                }
            }
        }
    }
}