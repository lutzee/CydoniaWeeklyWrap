using System;
using System.Threading.Tasks;
using Cww.Core.Messages;
using MassTransit;

namespace Cww.Service.Consumers
{
    public class GroupRecentMusicConsumer : IConsumer<GroupRecentMusic.Request>
    {
        public Task Consume(ConsumeContext<GroupRecentMusic.Request> context)
        {
            var response = GroupRecentMusic.Response.Create(
                context.Message, 
                new GroupRecentMusic.Result
                {
                    Message = "Hello world!"
                });

            return context.RespondAsync(response);
        }
    }
}
