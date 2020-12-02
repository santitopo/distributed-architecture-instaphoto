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
        [HttpGet]
        public IActionResult Get()
        {            
            try
            {
                return Ok("hola");
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
                return Ok();
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }

        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            try
            {
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",true);
                var channel = GrpcChannel.ForAddress("http://localhost:5001");
                var client = new ABMUsers.ABMUsersClient(channel);
                var response = await client.ModifyUserAsync(new UserModel() { });
                        
                return Ok(response.Message);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}