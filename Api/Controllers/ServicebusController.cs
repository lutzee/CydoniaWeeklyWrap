using System;
using System.Threading.Tasks;
using Cww.Core.Messages;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Cww.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicebusController : ControllerBase
    {
        private readonly IBus bus;

        public ServicebusController(IBus bus)
        {
            this.bus = bus;
        }
        public async Task<dynamic> Get()
        {
            var request = GroupRecentMusic.Request.Create();
            var client = bus.CreateRequestClient<GroupRecentMusic.Request>();
            var response = await client.GetResponse<GroupRecentMusic.Response>(request);
            return response.Message.Result;
        }
    }
}