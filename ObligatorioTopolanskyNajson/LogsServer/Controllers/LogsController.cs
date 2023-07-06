using System.Collections.Generic;
using InstaPhotoServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


namespace LogsServer.Controllers
{
    [ApiController]
    [Route("/logs")]
    public class LogsController : ControllerBase
    {
        private readonly ILogger<LogsController> _logger;
        public LogsController(ILogger<LogsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Log> Get()
        {
            return Program.logs;
        }
    }
}