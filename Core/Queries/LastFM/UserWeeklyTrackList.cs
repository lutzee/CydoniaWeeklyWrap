using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Models;
using Cww.Core.Queries.Spotify;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using MediatR;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Cww.Core.Queries.LastFM
{
    public class UserWeeklyTrackList
    {
        public class Query : IRequest<IAsyncEnumerable<Track>>
        {
            public string UserName { get; set; }

            public int? Limit { get; set; }
        }

        public class Handler : IRequestHandler<Query, IAsyncEnumerable<Track>>
        {
            private readonly IUserApi userApi;
            private readonly IMediator mediator;
            private ILogger<Handler> logger;

            public Handler(
                IUserApi userApi,
                IMediator mediator,
                ILoggerFactory loggerFactory)
            {
                this.userApi = userApi;
                this.mediator = mediator;
                this.logger = loggerFactory.CreateLogger<Handler>();
            }

            public async Task<IAsyncEnumerable<Track>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                return await GetTracks(message, cancellationToken);
            }

            private async Task<IAsyncEnumerable<Track>> GetTracks(Query message, CancellationToken cancellationToken)
            {
                var results = await userApi.GetWeeklyTrackChartAsync(message.UserName);
                
                return ParseTracks(results.Content, message, cancellationToken);
            }

            private async IAsyncEnumerable<Track> ParseTracks(IEnumerable<LastTrack> tracks, Query message,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                foreach (var result in tracks.Take(5))
                {
                    var track = new Track(result);

                    logger.LogInformation($"Getting spotify id for {result.ArtistName} - {result.Name} for user {message.UserName}");

                    var spotifyTrack = await mediator.Send(new FindTrack.Query
                    {
                        ArtistName = result.ArtistName,
                        TrackName = result.Name
                    }, cancellationToken);

                    if (spotifyTrack != null)
                    {
                        track.SpotifyUrl = spotifyTrack.ExternUrls["spotify"];
                        track.SpotifyUid = spotifyTrack.Id;
                    }

                    yield return track;
                }
            }
        }
    }
}
