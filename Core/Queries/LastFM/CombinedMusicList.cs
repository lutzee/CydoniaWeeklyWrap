using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Messages;
using Cww.Core.Models;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using IMediator = MediatR.IMediator;

namespace Cww.Core.Queries.LastFM
{
    public class CombinedMusicList
    {
        public class Query : IRequest<IEnumerable<Track>>
        {
        }

        public class Handler : IRequestHandler<Query, IEnumerable<Track>>
        {
            private readonly IMediator mediator;
            private readonly IMemoryCache cache;
            private readonly IBus bus;

            public Handler(
                IMediator mediator, 
                IMemoryCache cache,
                IBus bus)
            {
                this.mediator = mediator;
                this.cache = cache;
                this.bus = bus;
            }

            public async Task<IEnumerable<Track>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var requestClient = bus.CreateRequestClient<TrackDeduplication.Request>();
                var request = new TrackDeduplication.Request();
                var response = await requestClient.GetResponse<TrackDeduplication.Response>(request);
                return response.Message.Result.Tracks;
            }

            public async Task<IList<Track>> GetCachedTracks()
            {
                if (cache.TryGetValue<IList<Track>>(Known.Cache.CombinedMusicCacheKey, out var tracks))
                {
                    return tracks;
                }

                tracks = await GetTracks();
                cache.Set(Known.Cache.CombinedMusicCacheKey, tracks, TimeSpan.FromMinutes(5));

                return tracks;
            }

            public async Task<IList<Track>> GetTracks()
            {
                var combinedTracks = await mediator.Send(new UncombinedMusicList.Query());

                var maxPlays = from u in combinedTracks
                    group u by new { u.TrackName, u.ArtistName } into groupedTrack
                    select new 
                    {
                        ArtistName = groupedTrack.Key.ArtistName,
                        TrackName = groupedTrack.Key.TrackName,
                        Username = groupedTrack.OrderByDescending(g => g.PlayCount).First().Username,
                        PlayCount = groupedTrack.Sum(g => g.PlayCount)
                    };

                var playCount = from u in combinedTracks
                    group u by new { u.TrackName, u.ArtistName, u.Url, u.SpotifyUrl, u.SpotifyUid }
                    into groupedTrack
                    select new Track
                    {
                        Url = groupedTrack.Key.Url,
                        ArtistName = groupedTrack.Key.ArtistName,
                        TrackName = groupedTrack.Key.TrackName,
                        SpotifyUrl = groupedTrack.Key.SpotifyUrl,
                        SpotifyUid = groupedTrack.Key.SpotifyUid,
                        PlayCount = groupedTrack.Sum(g => g.PlayCount)
                    };

                var groupedTracks = from pc in playCount
                                    join mp in maxPlays on new { pc.ArtistName, pc.TrackName } equals new { mp.ArtistName, mp.TrackName }
                                    select new Track
                                    {
                                        Url = pc.Url,
                                        ArtistName = pc.ArtistName,
                                        TrackName = pc.TrackName,
                                        SpotifyUrl = pc.SpotifyUrl,
                                        SpotifyUid = pc.SpotifyUid,
                                        PlayCount = pc.PlayCount,
                                        MostListens = mp.Username
                                    };

                return groupedTracks.OrderByDescending(g => g.PlayCount).ThenBy(g => g.ArtistName).ThenBy(g => g.TrackName).ToList();
            }
        }
    }
}
