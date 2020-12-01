using System.Threading.Tasks;
using Grpc.Core;
using InstaPhotoServer;
using Microsoft.Extensions.Logging;

namespace AdministrativeServer
{
    public class UsersService : ABMUsers.ABMUsersBase
    {
        private readonly ILogger<UsersService> _logger;

        public UsersService(ILogger<UsersService> logger)
        {
            _logger = logger;
        }

        public override Task<InfoResponse> AddUser(UserRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new InfoResponse
                {
                    Message = "Hello AddUser"
                });
        }
        
        public override Task<InfoResponse> ModifyUser(UserRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new InfoResponse
                {
                    Message = "Hello ModifyUser"
                });
        }

        public override Task<InfoResponse> DeleteUser(UserRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new InfoResponse
                {
                    Message = "Hello DeleteUser"
                });
        }
    }
}