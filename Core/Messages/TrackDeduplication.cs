using System;
using System.Collections.Generic;
using Cww.Core.Models;

namespace Cww.Core.Messages
{
    public class TrackDeduplication
    {
        public class Request : IRequestMessage
        {
            public Guid CorrelationId { get; protected set; }

            public DateTimeOffset RequestTime { get; protected set; }
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