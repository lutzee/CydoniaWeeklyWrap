using System.Threading;
using System.Threading.Tasks;
using Cww.Core.Database;
using Cww.Core.Models;
using MediatR;

namespace Cww.Core.Commands.Database
{
    public class AddTrack
    {
        public class Command : INotification
        {
            public Track Track { get; set; }
        }

        public class Handler : INotificationHandler<Command>
        {
            private readonly CwwDbContext dbContext;
            private readonly IMediator mediator;

            public Handler(CwwDbContext dbContext,
                IMediator mediator)
            {
                this.dbContext = dbContext;
                this.mediator = mediator;
            }

            public async Task Handle(
                Command message,
                CancellationToken cancellationToken)
            {
                await dbContext.AddAsync(message.Track, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}