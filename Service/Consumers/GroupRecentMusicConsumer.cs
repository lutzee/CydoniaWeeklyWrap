using System.Linq;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Messages;
using Cww.Core.Queries.LastFM;
using Cww.Service.Cache;
using MassTransit;
using IMediator = MediatR.IMediator;

namespace Cww.Service.Consumers
{
    public class GroupRecentMusicConsumer : IConsumer<GroupRecentMusic.Request>
    {
        private readonly ICacheProvider cacheProvider;
        private readonly ICacheTimeoutProvider cacheTimeoutProvider;
        private readonly IMediator mediator;

        public GroupRecentMusicConsumer(
            ICacheProvider cacheProvider,
            ICacheTimeoutProvider cacheTimeoutProvider,
            IMediator mediator)
        {
            this.cacheProvider = cacheProvider;
            this.cacheTimeoutProvider = cacheTimeoutProvider;
            this.mediator = mediator;
        }

        public async Task Consume(ConsumeContext<GroupRecentMusic.Request> context)
        {
            var key = Known.Cache.UserRecentKey(context.Message.Username);
            var tracks = await cacheProvider.TryGet(key, async () =>
                            {
                                var result = await mediator.Send(new UserWeeklyTrackList.Query
                                {
                                    UserName = context.Message.Username,
                                    Limit = 100
                                });

                                var enumerable = await result.ToListAsync();

                                // Only cache if we have results
                                if (enumerable.Any())
                                {
                                    cacheProvider.Set(key, enumerable, cacheTimeoutProvider.Medium());
                                }
                                return enumerable;
                            }, cacheTimeoutProvider.Medium());

            var response = GroupRecentMusic.Response.Create(
                context.Message, 
                new GroupRecentMusic.Result
                {
                    Tracks = tracks
                });

            await context.RespondAsync(response);
        }
    }
}
