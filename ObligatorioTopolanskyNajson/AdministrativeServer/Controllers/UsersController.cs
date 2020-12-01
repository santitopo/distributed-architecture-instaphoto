using System;
using System.Collections.Generic;
using System.Linq;
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
                return Ok();
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
        public IActionResult Put([FromBody] User user)
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
    }
}