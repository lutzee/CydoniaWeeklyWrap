using System;
using System.Collections.Generic;
using Cww.Core.Models;
using Cww.Core.Providers;

namespace Cww.Core.Messages
{
    public class GroupRecentMusic
    {
        public class Request : IRequestMessage
        {
            public Guid CorrelationId { get; protected set; }

            public DateTimeOffset RequestTime { get; protected set; }

            public string Username { get; protected set; }

            public static Request Create(string username)
            {
                return new Request
                {
                    CorrelationId = Guid.NewGuid(),
                    RequestTime = DateTimeOffsetProvider.Instance.Now,
                    Username = username
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
            public IEnumerable<Track> Tracks { get; set; }
        }
    }
}
