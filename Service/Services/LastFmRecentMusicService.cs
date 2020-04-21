using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Extensions;
using Cww.Core.Models;
using Cww.Core.Queries.LastFM;
using Cww.Service.Cache;
using F23.StringSimilarity;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Cww.Service.Services
{
    public class LastFmRecentMusicService : IHostedService
    {
        private readonly ICacheProvider cacheProvider;
        private readonly ICacheTimeoutProvider cacheTimeoutProvider;
        private readonly IConfiguration configuration;
        private readonly IMediator mediator;
        private Timer timer;

        public LastFmRecentMusicService(
            IMediator mediator,
            ICacheProvider cacheProvider,
            ICacheTimeoutProvider cacheTimeoutProvider,
            IConfiguration configuration)
        {
            this.mediator = mediator;
            this.cacheProvider = cacheProvider;
            this.cacheTimeoutProvider = cacheTimeoutProvider;
            this.configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(GetEveryonesRecentMusic, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        public async void GetEveryonesRecentMusic(object state)
        {
            var users = configuration.Users();
            var allUserTracks = new List<IEnumerable<UserTrack>>();
            if (users != null)
            {
                foreach (var user in users)
                {
                    Log.Logger.Information($"Getting lastfm weekly track list for {user}");
                    var key = Known.Cache.UserRecentKey(user);
                    var userTracks = await cacheProvider.TryGet(key, async () =>
                    {
                        var result = await mediator.Send(new UserWeeklyTrackList.Query
                        {
                            UserName = user,
                            Limit = 5
                        });
                        
                        return await result.ToListAsync();
                    }, cacheTimeoutProvider.Medium());

                    allUserTracks.Add(userTracks);
                }
            }
            
            var finalTracks = new List<Track>();
            var damerau = new Damerau();
            foreach (var trackList in allUserTracks)
            {
                var userTracks = trackList.ToList();
                if (userTracks.Any())
                {
                    var username = userTracks.FirstOrDefault()?.Username;
                    foreach (var track in userTracks)
                    {
                        Log.Logger.Information(
                            $"Checking {track.TrackName} - {track.ArtistName} for possible duplicates");
                        var tracksToCheck = finalTracks
                            .Where(t => t.ArtistName.Equals(track.ArtistName, StringComparison.Ordinal)).ToList();

                        if (tracksToCheck.Any())
                        {
                            var match = false;
                            foreach (var toCheck in tracksToCheck)
                            {
                                var distance = damerau.Distance(track.TrackName, toCheck.TrackName);
                                Log.Logger.Information($"Checking {track.TrackName} against {toCheck.TrackName} ({distance})");
                                
                                if (distance <= 2)
                                {
                                    toCheck.PlayCount += track.PlayCount;
                                    toCheck.UserPlayCounts.Add(username, track.PlayCount);

                                    if (string.IsNullOrEmpty(toCheck.Mbid) && !string.IsNullOrEmpty(track.Mbid))
                                    {
                                        Log.Logger.Information($"Found new mbid {track.Mbid}");
                                        toCheck.Mbid = track.Mbid;
                                    }

                                    match = true;
                                    break;
                                }
                            }

                            if (!match)
                            {
                                Log.Logger.Information($"Adding {track.TrackName} as no match found");
                                var t = new Track
                                {
                                    TrackName = track.TrackName,
                                    ArtistName = track.ArtistName,
                                    SpotifyUid = track.SpotifyUid,
                                    PlayCount = track.PlayCount,
                                    TrackId = track.TrackId,
                                    Url = track.Url,
                                    SpotifyUrl = track.SpotifyUrl,
                                    UserPlayCounts = new Dictionary<string, int?>()
                                };

                                t.UserPlayCounts.Add(username, track.PlayCount);

                                finalTracks.Add(t);
                            }
                            else
                            {
                                Log.Logger.Information($"MATCH FOUND!!!!");
                            }
                        }
                        else
                        {
                            Log.Logger.Information($"No tracks for artist {track.ArtistName} present, adding");
                            var t = new Track
                            {
                                TrackName = track.TrackName,
                                ArtistName = track.ArtistName,
                                SpotifyUid = track.SpotifyUid,
                                PlayCount = track.PlayCount,
                                TrackId = track.TrackId,
                                Url = track.Url,
                                SpotifyUrl = track.SpotifyUrl,
                                UserPlayCounts = new Dictionary<string, int?>()
                            };

                            t.UserPlayCounts.Add(username, track.PlayCount);
                            finalTracks.Add(t);
                        }
                    }
                }
            }

            foreach (var track in finalTracks)
            {
                track.MostListens = track.UserPlayCounts.OrderByDescending(x => x.Value.Value).First().Key;
            }

            cacheProvider.Set(Known.Cache.CombinedMusicCacheKey, finalTracks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
