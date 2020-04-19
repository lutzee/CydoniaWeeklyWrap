using System;

namespace Cww.Core.Providers
{
    public interface IDateTimeProvider
    {
        TimeZoneInfo TimeZone { get; }

        DateTime Now { get; }

        DateTime UtcNow { get; }
    }
}