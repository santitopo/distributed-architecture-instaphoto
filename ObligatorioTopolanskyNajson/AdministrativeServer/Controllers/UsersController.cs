using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AdministrativeServer;
using InstaPhotoServer;

namespace AdministrativeServer.Controllers
{        
    [ApiController]
    [Route("/users")]
    public class UsersController: ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {            
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = client.GetUsers(new Empty());
                return Ok(response.Users);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost]
        public IActionResult Post([FromBody] User user)
        {            
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = client.AddUser(
                    new UserModel()
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Password = user.Password,
                        Username = user.UserName
                    });
                
                return Ok(response.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpDelete]
        public IActionResult Delete([FromBody] User user)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = client.DeleteUser(
                    new UserModel()
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Password = user.Password,
                        Username = user.UserName
                    });
                
                return Ok(response.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] User user)
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = client.ModifyUser(
                    new UserModel()
                    {
                        Name = user.Name,
                        Surname = user.Surname,
                        Password = user.Password,
                        Username = user.UserName
                    });
                
                return Ok(response.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}