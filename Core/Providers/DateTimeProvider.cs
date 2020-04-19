using System;

namespace Cww.Core.Providers
{
    public class DateTimeProvider : IDateTimeProvider, IDateTimeOffsetProvider
    {
        public static IDateTimeProvider Instance { get; set; } = new DateTimeProvider();

        public TimeZoneInfo TimeZone => TimeZoneInfo.Local;

        DateTime IDateTimeProvider.Now => DateTime.Now;

        DateTime IDateTimeProvider.UtcNow => DateTime.UtcNow;

        DateTimeOffset IDateTimeOffsetProvider.Now => DateTimeOffset.Now;

        DateTimeOffset IDateTimeOffsetProvider.UtcNow => DateTimeOffset.UtcNow;
    }
}