namespace Cww.Core.Providers
{
    public class DateTimeOffsetProvider
    {
        public static IDateTimeOffsetProvider Instance { get; set; } = new DateTimeProvider();
    }
}