using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cww.Service.Cache
{
    public interface ICacheProvider
    {
        T Get<T>(string key);

        T TryGet<T>(string key, Func<T> callback);

        T TryGet<T>(string key, Func<T> callback, TimeSpan expires);

        Task<T> TryGet<T>(string key, Func<Task<T>> callback);

        Task<T> TryGet<T>(string key, Func<Task<T>> callback, TimeSpan expires);

        void Set<T>(string key, T value);

        void Set<T>(string key, T value, TimeSpan timeout);

        bool Exists(string key);

        IEnumerable<T> GetAll<T>(IEnumerable<string> keys);

        IAsyncEnumerable<T> GetAllAsync<T>(IEnumerable<string> keys);
    }
}