﻿namespace Cww.Core
{
    public static class Known
    {
        public static class Cache
        {
            public const string CombinedMusicCacheKey = "CombinedMusic";
            public const string UncombinedMusicCacheKey = "UncombinedMusic";
            public const string UserTracksCacheKey = "UserTracks";
            public const string RecentListKey = "RecentList";

            public static string UserRecentKey(string username) => $"user:recent:{username}";
        }
    }
}
