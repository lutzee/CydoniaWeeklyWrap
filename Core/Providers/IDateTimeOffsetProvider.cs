using System;

namespace Cww.Core.Providers
{
    public interface IDateTimeOffsetProvider
    {
        TimeZoneInfo TimeZone { get; }

        DateTimeOffset Now { get; }

        DateTimeOffset UtcNow { get; }
    }
}