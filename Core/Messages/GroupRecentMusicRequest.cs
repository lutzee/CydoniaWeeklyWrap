using System;
using Cww.Core.Providers;

namespace Cww.Core.Messages
{
    public class GroupRecentMusic
    {
        public class Request : IRequestMessage
        {
            public Guid CorrelationId { get; protected set; }

            public DateTimeOffset RequestTime { get; protected set; }

            public static Request Create()
            {
                return new Request
                {
                    CorrelationId = Guid.NewGuid(),
                    RequestTime = DateTimeOffsetProvider.Instance.Now
                };
            }
        }

        public class Response : IResponseMessage<Request, Result>
        {
            public Guid CorrelationId => Request.CorrelationId;
            public Request Request { get; protected set; }
            public Result Result { get; protected set; }

            public static Response Create(Request request, Result result)
            {
                return new Response
                {
                    Request = request,
                    Result = result
                };
            }
        }

        public class Result
        {
            public string Message { get; set; }
        }
    }
}
