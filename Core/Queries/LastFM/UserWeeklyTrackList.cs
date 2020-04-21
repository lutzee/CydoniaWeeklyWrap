using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Models;
using Cww.Core.Queries.Database;
using Cww.Core.Queries.Spotify;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using MediatR;
using Serilog;

namespace Cww.Core.Queries.LastFM
{
    public class UserWeeklyTrackList
    {
        public class Query : IRequest<IAsyncEnumerable<UserTrack>>
        {
            public string UserName { get; set; }

            public int? Limit { get; set; }
        }

        public class Handler : IRequestHandler<Query, IAsyncEnumerable<UserTrack>>
        {
            private readonly IUserApi userApi;
            private readonly IMediator mediator;

            public Handler(
                IUserApi userApi,
                IMediator mediator)
            {
                this.userApi = userApi;
                this.mediator = mediator;
            }

            public async Task<IAsyncEnumerable<UserTrack>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                return await GetTracks(message, cancellationToken);
            }

            private async Task<IAsyncEnumerable<UserTrack>> GetTracks(Query message, CancellationToken cancellationToken)
            {
                var results = await userApi.GetWeeklyTrackChartAsync(message.UserName);
                
                return ParseTracks(results.Content, message, cancellationToken);
            }

            private async IAsyncEnumerable<UserTrack> ParseTracks(IEnumerable<LastTrack> tracks, Query message,
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                foreach (var result in tracks.Take(message.Limit ?? 30))
                {
                    var track = UserTrack.Create(result, message.UserName);

                    var dbTrack = await mediator.Send(new GetTrack.Query
                    {
                        Mbid = track.Mbid,
                        TrackName = track.TrackName,
                        ArtistName = track.ArtistName
                    }, cancellationToken);

                    if (dbTrack == null || string.IsNullOrEmpty(dbTrack.SpotifyUid))
                    {
                        Log.Logger.Information($"Getting spotify id for {result.ArtistName} - {result.Name} for user {message.UserName}");
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
                    }
                    else
                    {
                        track.SpotifyUid = dbTrack.SpotifyUid;
                        track.SpotifyUrl = dbTrack.SpotifyUrl;
                        track.TrackId = dbTrack.TrackId;
                        track.Mbid = dbTrack.Mbid;
                    }

                    yield return track;
                }
            }
        }
    }
}
