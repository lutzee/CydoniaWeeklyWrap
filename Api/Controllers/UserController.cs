using System.Collections.Generic;
using System.Threading.Tasks;
using Cww.Core.Models;
using Cww.Core.Queries.LastFM;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cww.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Route("{username}/recent")]
        public async Task<IEnumerable<Track>> Get(string username)
        {
            return await mediator.Send(new UserWeeklyTrackList.Query { UserName = username });
        }

    }
}