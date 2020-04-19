using System;

namespace Cww.Service.Cache
{
    public class CacheTimeoutProvider : ICacheTimeoutProvider
    {
        public TimeSpan GetCacheTime(CacheDuration duration)
        {
            if (duration == CacheDuration.Forever)
            {
                return TimeSpan.FromSeconds((double) duration);
            }
            var intDuration = (int) duration;
            var tenPercent = intDuration / 10;
            var random = new Random();
            return TimeSpan.FromSeconds(random.Next(intDuration - tenPercent, intDuration + tenPercent));
        }

        public TimeSpan VeryShort()
        {
            return GetCacheTime(CacheDuration.Medium);
        }

        public TimeSpan Short()
        {
            return GetCacheTime(CacheDuration.Medium);
        }

        public TimeSpan Medium()
        {
            return GetCacheTime(CacheDuration.Medium);
        }

        public TimeSpan Long()
        {
            return GetCacheTime(CacheDuration.Medium);
        }

        public TimeSpan Forever()
        {
            return GetCacheTime(CacheDuration.Forever);
        }
    }
}
