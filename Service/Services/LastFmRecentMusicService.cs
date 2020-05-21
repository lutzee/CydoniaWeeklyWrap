using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Commands.Database;
using Cww.Core.Extensions;
using Cww.Core.Models;
using Cww.Core.Queries.Database;
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
            var allUserTracks = await GetEveryonesMusic();

            var finalTracks = Combine(allUserTracks);

            CalculateMostListens(finalTracks);
            await PersistData(finalTracks);

            finalTracks = SortTracks(finalTracks);

            cacheProvider.Set(Known.Cache.CombinedMusicCacheKey, finalTracks);
            Log.Logger.Information(nameof(LastFmRecentMusicService) + " Done");
        }

        private static List<Track> SortTracks(List<Track> finalTracks)
        {
            return finalTracks.OrderByDescending(x => x.PlayCount).ThenBy(x => x.ArtistName).ThenBy(x => x.TrackName).ToList();
        }

        private async Task PersistData(IEnumerable<Track> finalTracks)
        {
            Log.Logger.Information("Persisting tracks to database");

            foreach (var track in finalTracks)
            {
                Log.Logger.Debug($"Peristing track {track.TrackName} - {track.ArtistName}");
                var dbTrack = await mediator.Send(new GetTrack.Query
                {
                    TrackName = track.TrackName,
                    ArtistName = track.ArtistName,
                    SpotifyUid = track.SpotifyUid,
                    Mbid = track.Mbid
                });

                if (dbTrack != null)
                {
                    await mediator.Publish(new UpdateTrack.Command
                    {
                        Track = track
                    });
                }
                else
                {
                    await mediator.Publish(new AddTrack.Command
                    {
                        Track = track
                    });
                }
            }
        }

        private async Task<List<IEnumerable<UserTrack>>> GetEveryonesMusic()
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
                            Limit = 100
                        });

                        return await result.ToListAsync();
                    }, cacheTimeoutProvider.Medium());

                    allUserTracks.Add(userTracks);
                }
            }

            return allUserTracks;
        }

        private static void CalculateMostListens(List<Track> finalTracks)
        {
            Log.Logger.Information($"Calculating Most Plays");

            foreach (var track in finalTracks)
            {
                track.MostListens = track.UserPlayCounts.OrderByDescending(x => x.Value.Value).First().Key;
            }
        }

        private static List<Track> Combine(List<IEnumerable<UserTrack>> allUserTracks)
        {
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
                                Log.Logger.Debug($"Checking {track.TrackName} against {toCheck.TrackName} ({distance})");

                                if (distance <= 2)
                                {
                                    toCheck.PlayCount += track.PlayCount;
                                    if (toCheck.UserPlayCounts.ContainsKey(username))
                                    {
                                        toCheck.UserPlayCounts[username] += track.PlayCount;
                                    }
                                    else
                                    {
                                        toCheck.UserPlayCounts.Add(username, track.PlayCount);
                                    }

                                    if (string.IsNullOrEmpty(toCheck.Mbid) && !string.IsNullOrEmpty(track.Mbid))
                                    {
                                        Log.Logger.Debug($"Found new mbid {track.Mbid}");
                                        toCheck.Mbid = track.Mbid;
                                    }

                                    match = true;
                                    break;
                                }
                            }

                            if (!match)
                            {
                                Log.Logger.Debug($"Adding {track.TrackName} as no match found");
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
                                Log.Logger.Debug($"MATCH FOUND!!!!");
                            }
                        }
                        else
                        {
                            Log.Logger.Debug($"No tracks for artist {track.ArtistName} present, adding");
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

            return finalTracks;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
