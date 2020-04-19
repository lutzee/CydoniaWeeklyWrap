using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Models;
using Cww.Core.Queries.Database;
using MediatR;
using Serilog;

namespace Cww.Core.Commands.Database
{
    public class GetOrAddTrack
    {
        public class Command : IRequest<Track>
        {
            public Track Track { get; set; }
        }

        public class Handler : IRequestHandler<Command, Track>
        {
            private readonly IMediator mediator;

            public Handler(IMediator mediator)
            {
                this.mediator = mediator;
            }

            public async Task<Track> Handle(
                Command message,
                CancellationToken cancellationToken)
            {
                var fromDb =
                    await mediator.Send(
                        new GetTrack.Query
                        {
                            SpotifyUid = message.Track.SpotifyUid
                        },
                        cancellationToken);


                if (fromDb != null)
                {
                    Log.Logger.Debug($"{message.Track.ArtistName} - {message.Track.TrackName} already exists in db, returning");
                    return fromDb;
                }

                Log.Logger.Debug($"Creating {message.Track.ArtistName} - {message.Track.TrackName} in db");
                await mediator.Publish(new AddTrack.Command { Track = message.Track }, cancellationToken);
                return await mediator.Send(
                    new GetTrack.Query
                    {
                        SpotifyUid = message.Track.SpotifyUid
                    },
                    cancellationToken);
            }
        }
    }
}
