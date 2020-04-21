using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Extensions;
using Cww.Core.Messages;
using Cww.Core.Models;
using Cww.Service.Cache;
using F23.StringSimilarity;
using F23.StringSimilarity.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using MoreLinq;

namespace Cww.Service.Consumers
{
    public class TrackDeduplicationConsumer : IConsumer<TrackDeduplication.Request>
    {
        private readonly ICacheProvider cacheProvider;
        private readonly IConfiguration configuration;

        public TrackDeduplicationConsumer(
            ICacheProvider cacheProvider, 
            IConfiguration configuration)
        {
            this.cacheProvider = cacheProvider;
            this.configuration = configuration;
        }

        public async Task Consume(ConsumeContext<TrackDeduplication.Request> context)
        {
            //var usersRecentTracks = cacheProvider.GetAllAsync<IEnumerable<Track>>(configuration.Users().Select(Known.Cache.UserRecentKey));
            //var allTracks = new ConcurrentBag<Track>();
            //await foreach (var tracks in usersRecentTracks)
            //{
            //    foreach (var track in tracks)
            //    {
            //        allTracks.Add(track);
            //    }
            //}

            //var simpleUniqueTracks = allTracks.AsEnumerable().DistinctBy(x => new { x.TrackName, x.ArtistName }).ToList();
            //var finalTracks = new List<Track>();
            //var damerau = new Damerau();
            //foreach (var track in 
            //    from track in simpleUniqueTracks
            //    from finalTrack in finalTracks
            //        .Where(t => t.ArtistName.Equals(track.ArtistName, StringComparison.Ordinal)).ToList()
            //    where damerau.Distance(track.TrackName, finalTrack.TrackName) >= 2
            //    select track)
            //{
            //    finalTracks.Add(track);
            //}

            await context.RespondAsync(TrackDeduplication.Response.Create(context.Message, new TrackDeduplication.Result
            {
                Tracks = cacheProvider.Get<IEnumerable<Track>>(Known.Cache.CombinedMusicCacheKey)
            }));
        }
    }
}