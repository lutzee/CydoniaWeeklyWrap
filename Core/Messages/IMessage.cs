using System;
using MassTransit;

namespace Cww.Core.Messages
{
    public interface IMessage : CorrelatedBy<Guid>
    {
    }

    public interface IRequestMessage : IMessage
    {
        DateTimeOffset RequestTime { get; }
    }

    public interface IRequestMessage<out TParameters> : IRequestMessage
        where TParameters : class
    {
        TParameters Parameters { get; }
    }

    public interface IResponseMessage<out TRequest> : IMessage
        where TRequest : IRequestMessage
    {
        TRequest Request { get; }
    }

    public interface IResponseMessage<out TRequest, out TResult> : IResponseMessage<TRequest>
        where TRequest : IRequestMessage
    {
        TResult Result { get; }
    }
}