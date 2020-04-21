using System.Collections.Generic;
using System.Threading.Tasks;
using Cww.Core.Models;
using Cww.Core.Queries.LastFM;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Cww.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IMediator mediator;

        public GroupController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IEnumerable<Track>> Get()
        {
            Log.Logger.Debug($"Getting group tracks");
            return await mediator.Send(new CombinedMusicList.Query());
        }

        [HttpGet]
        [Route("Uncombined")]
        public async Task<IList<UserTrack>> GetUncombined()
        {
            Log.Logger.Debug($"Getting uncombined group tracks");
            return await mediator.Send(new UncombinedMusicList.Query());
        }
    }
}