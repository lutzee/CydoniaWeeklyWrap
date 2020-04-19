using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Commands.Database;
using Cww.Core.Models;
using Cww.Core.Queries.Spotify;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using MediatR;
using Serilog;

namespace Cww.Core.Queries.LastFM
{
    public class UserWeeklyTrackList
    {
        public class Query : IRequest<IEnumerable<Track>>
        {
            public string UserName { get; set; }

            public int? Limit { get; set; }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<Track>>
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

            public async Task<IEnumerable<Track>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                return await GetTracks(message);
            }

            private async Task<IEnumerable<Track>> GetTracks(Query message)
            {
                var results = await userApi.GetWeeklyTrackChartAsync(message.UserName);
                
                return await ParseTracksParallel(results.Content, message);
            }

            //private async IAsyncEnumerable<Track> ParseTracks(IReadOnlyList<LastTrack> tracks, Query message,
            //    [EnumeratorCancellation] CancellationToken cancellationToken)
            //{
            //    Parallel.ForEach(tracks, (track) => { });
            //    foreach (var result in tracks)
            //    {
            //        var track = new Track(result);

            //        Log.Logger.Debug($"Getting spotify id for {result.ArtistName} - {result.Name} for user {message.UserName}");

            //        var spotifyTrack = await mediator.Send(new FindTrack.Query
            //        {
            //            ArtistName = result.ArtistName,
            //            TrackName = result.Name
            //        }, cancellationToken);

            //        if (spotifyTrack != null)
            //        {
            //            track.SpotifyUrl = spotifyTrack.ExternUrls["spotify"];
            //            track.SpotifyUid = spotifyTrack.Id;
            //        }
                    
            //        var t = await mediator.Send(new GetOrAddTrack.Command
            //        {
            //            Track = track
            //        }, cancellationToken);

            //        yield return t;
            //    }
            //}

            private async Task<IEnumerable<Track>> ParseTracksParallel(IReadOnlyList<LastTrack> tracks, Query message)
            {
                var bag = new ConcurrentBag<Track>();
                Parallel.ForEach(tracks, async result =>
                {
                    var track = new Track(result);

                    Log.Logger.Debug($"Getting spotify id for {result.ArtistName} - {result.Name} for user {message.UserName}");

                    var spotifyTrack = await mediator.Send(new FindTrack.Query
                    {
                        ArtistName = track.ArtistName,
                        TrackName = track.TrackName
                    });

                    if (spotifyTrack != null)
                    {
                        track.SpotifyUrl = spotifyTrack.ExternUrls["spotify"];
                        track.SpotifyUid = spotifyTrack.Id;
                    }
                    
                    bag.Add(track);
                });

                foreach (var track in bag)
                {
                    var t = await mediator.Send(new GetOrAddTrack.Command
                    {
                        Track = track
                    });
                }

                return bag;
            }
        }
    }
}
