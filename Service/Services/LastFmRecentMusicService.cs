using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Queries.LastFM;
using Cww.Service.Cache;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cww.Service.Services
{
    public class LastFmRecentMusicService : IHostedService
    {
        private readonly ICacheProvider cacheProvider;
        private readonly ICacheTimeoutProvider cacheTimeoutProvider;
        private readonly IConfiguration configuration;
        private readonly IMediator mediator;
        private Timer timer;
        private ILogger<LastFmRecentMusicService> logger;

        public LastFmRecentMusicService(
            IMediator mediator,
            ICacheProvider cacheProvider,
            ICacheTimeoutProvider cacheTimeoutProvider,
            IConfiguration configuration, 
            ILoggerFactory loggerFactory)
        {
            this.mediator = mediator;
            this.cacheProvider = cacheProvider;
            this.cacheTimeoutProvider = cacheTimeoutProvider;
            this.configuration = configuration;
            this.logger = loggerFactory.CreateLogger<LastFmRecentMusicService>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(GetEveryonesRecentMusic, null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        public void GetEveryonesRecentMusic(object state)
        {
            var users = configuration.GetSection("Users").Get<string[]>().AsEnumerable();
            if (users != null)
            {
                foreach (var user in users)
                {
                    var key = Known.Cache.UserRecentKey(user);
                    cacheProvider.TryGet(key, async () =>
                    {
                        var result = await mediator.Send(new UserWeeklyTrackList.Query
                        {
                            UserName = user,
                            Limit = 5
                        });

                        return await result.ToListAsync();
                    }, cacheTimeoutProvider.Medium());
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
