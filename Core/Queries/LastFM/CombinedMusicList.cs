using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Cww.Core.Queries.LastFM
{
    public class CombinedMusicList
    {
        public class Query : IRequest<IList<Track>>
        {
        }

        public class Handler : IRequestHandler<Query, IList<Track>>
        {
            private readonly IMediator mediator;
            private readonly IMemoryCache cache;
            
            public Handler(IMediator mediator, IMemoryCache cache)
            {
                this.mediator = mediator;
                this.cache = cache;
            }

            public async Task<IList<Track>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                return await GetCachedTracks();
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
