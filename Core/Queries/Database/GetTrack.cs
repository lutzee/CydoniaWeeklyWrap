using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Database;
using Cww.Core.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cww.Core.Queries.Database
{
    public class GetTrack
    {
        public class Query : IRequest<Track>
        {
            public string TrackName { get; set; }

            public string ArtistName { get; set; }

            public string SpotifyUid { get; set; }
        }

        public class Handler : IRequestHandler<Query, Track>
        {
            private readonly CwwDbContext dbContext;

            public Handler(CwwDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public async Task<Track> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                Track track = null;
                if (!string.IsNullOrEmpty(message.SpotifyUid))
                {
                    track = await dbContext.Tracks.SingleOrDefaultAsync(
                        t => t.SpotifyUid == message.SpotifyUid, cancellationToken);
                }

                track ??= await dbContext.Tracks.SingleOrDefaultAsync(
                    t => t.TrackName == message.TrackName
                         && t.ArtistName == message.ArtistName, cancellationToken);

                return track;
            }
        }
    }
}
