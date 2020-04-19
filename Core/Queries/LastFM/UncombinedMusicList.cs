using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Cww.Core.Queries.LastFM
{
    public class UncombinedMusicList
    {
        public class Query : IRequest<IList<UserTrack>>
        {
        }

        public class Handler : IRequestHandler<Query, IList<UserTrack>>
        {
            private readonly IMediator mediator;
            private readonly IMemoryCache cache;

            private readonly IList<UserTrack> combinedTracks;

            private static IList<string> Users = new List<string>
            {
                "lutzee_",
                "suckmyolipop",
                "englishhorrors",
                "rebeccacp1",
                "brickfaced",
                "jollygreen29",
                "deathstrtrucker"
            };

            public Handler(IMediator mediator, IMemoryCache cache)
            {
                this.mediator = mediator;
                this.cache = cache;
                this.combinedTracks = new List<UserTrack>();
            }

            public async Task<IList<UserTrack>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                return await GetCachedTracks();
            }

            public async Task<IList<UserTrack>> GetCachedTracks()
            {
                if (cache.TryGetValue<IList<UserTrack>>(Known.Cache.UncombinedMusicCacheKey, out var tracks))
                {
                    return tracks;
                }

                tracks = await GetTracks();
                cache.Set(Known.Cache.CombinedMusicCacheKey, tracks, TimeSpan.FromMinutes(30));

                return tracks;
            }

            public async Task<IList<UserTrack>> GetTracks()
            {
                foreach (var user in Users)
                {
                    var userTracks = await GetCachedUserTracks(user);

                    foreach (var track in userTracks)
                    {
                        if (track != null && !string.IsNullOrEmpty(track.SpotifyUid))
                        {
                            var userTrack = UserTrack.Create(track);
                            userTrack.Username = user;
                            userTrack.PlayCount = track.PlayCount;

                            combinedTracks.Add(userTrack);
                        }
                    }
                }

                return combinedTracks;
            }

            private async Task<List<Track>> GetCachedUserTracks(string username)
            {
                Log.Logger.Debug($"Getting recent music for {username}");
                if (cache.TryGetValue<List<Track>>($"{Known.Cache.UserTracksCacheKey}-{username}", out var tracks))
                {
                    return tracks;
                }

                var newTracks = await mediator.Send(new UserWeeklyTrackList.Query { UserName = username, Limit = 30 });
                var consumed = await newTracks.ToListAsync();
                cache.Set($"{Known.Cache.UserTracksCacheKey}-{username}", consumed, TimeSpan.FromMinutes(30));

                return consumed;
            }
        }
    }
}
