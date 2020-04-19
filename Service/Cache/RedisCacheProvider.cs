using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Cww.Service.Cache
{
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly IDatabase database;

        public RedisCacheProvider()
        {
            IConnectionMultiplexer redis = ConnectionMultiplexer.Connect("192.168.1.184");
            database = redis.GetDatabase(1);
        }

        public T Get<T>(string key)
        {
            var val = Encoding.UTF8.GetString(database.StringGet(key));
            return JsonConvert.DeserializeObject<T>(val);
        }

        public T TryGet<T>(string key, Func<T> callback)
        {
            return Exists(key) ? Get<T>(key) : callback();
        }

        public T TryGet<T>(string key, Func<T> callback, TimeSpan expires)
        {
            if (Exists(key))
            {
                return Get<T>(key);
            }

            var result = callback();
            Set(key, result, expires);
            return result;
        }

        public async Task<T> TryGet<T>(string key, Func<Task<T>> callback)
        {
            return await TryGet(key, callback, TimeSpan.Zero);
        }

        public async Task<T> TryGet<T>(string key, Func<Task<T>> callback, TimeSpan expires)
        {
            if (Exists(key))
            {
                return Get<T>(key);
            }

            var result = await callback();
            Set(key, result, expires);
            return result;
        }

        public void Set<T>(string key, T value)
        {
            Set(key, value, TimeSpan.Zero);
        }

        public void Set<T>(string key, T value, TimeSpan timeout)
        {
            database.StringSet(key, JsonConvert.SerializeObject(value), timeout);
        }

        public bool Exists(string key)
        {
            return database.KeyExists(key);
        }
    }
}
