using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Database;
using Cww.Core.Extensions;
using Cww.Core.Models;
using MediatR;

namespace Cww.Core.Commands.Database
{
    public class UpdateTrack
    {
        public class Command : INotification
        {
            public Track Track { get; set; }
        }

        public class Handler : INotificationHandler<Command>
        {
            private readonly CwwDbContext dbContext;

            public Handler(CwwDbContext dbContext)
            {
                this.dbContext = dbContext;
            }

            public async Task Handle(
                Command message,
                CancellationToken cancellationToken)
            {
                var dbTrack = dbContext.Tracks.SingleOrDefault(t =>
                    t.ArtistName == message.Track.ArtistName && t.TrackName == message.Track.TrackName);

                if (dbTrack != null)
                {
                    if (string.IsNullOrEmpty(dbTrack.SpotifyUid))
                    {
                        dbTrack.SpotifyUrl = message.Track.SpotifyUrl;
                        dbTrack.SpotifyUid = message.Track.SpotifyUid;
                    }

                    if (dbTrack.Mbid.IsNullOrEmpty())
                    {
                        dbTrack.Mbid = message.Track.Mbid;
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
