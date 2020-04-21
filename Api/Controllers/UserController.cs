using System.Collections.Generic;
using System.Threading.Tasks;
using Cww.Core.Messages;
using Cww.Core.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Cww.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IBus bus;

        public UserController(IBus bus)
        {
            this.bus = bus;
        }

        [Route("{username}/recent")]
        public async Task<IEnumerable<Track>> Get(string username)
        {
            Log.Logger.Debug($"Getting tracks for {username}");
            var request = GroupRecentMusic.Request.Create(username);
            var client = bus.CreateRequestClient<GroupRecentMusic.Request>();
            var response = await client.GetResponse<GroupRecentMusic.Response>(request);
            return response.Message.Result.Tracks;
        }

    }
}