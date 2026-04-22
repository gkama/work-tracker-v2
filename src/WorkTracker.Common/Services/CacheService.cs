using System.Net;
using StackExchange.Redis;
using WorkTracker.Common.Exceptions;
using WorkTracker.Common.Helpers;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(1);

        public CacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            if (connectionMultiplexer != null)
            {
                _cache = connectionMultiplexer.GetDatabase();
            }
            else
            {
                throw new ArgumentNullException(nameof(connectionMultiplexer));
            }
        }

        public async Task<T?> GetAsync<T>(string key)
            where T : class
        {
            var cached = await _cache.StringGetAsync(key);

            if (cached.HasValue)
            {
                try
                {
                    var deserialized = JsonHelper.DeserializeObject<T?>(cached!);

                    return deserialized;
                }
                catch (Exception ex)
                {
                    throw new ApiException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            return null;
        }

        public async Task<IEnumerable<T>> GetOrSetAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiration = null)
            where T : class
        {
            var cached = await _cache.StringGetAsync(key);

            if (cached.HasValue)
            {
                try
                {
                    var deserialized = JsonHelper.DeserializeObject<IEnumerable<T>>(cached!);
                    if (deserialized != null)
                        return deserialized;
                }
                catch (Exception ex)
                {
                    throw new ApiException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            var serialized = JsonHelper.SerializeObject(values);

            await _cache.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);

            return values;
        }

        public async Task<T> GetOrSetAsync<T>(string key, T value, TimeSpan? expiration = null)
            where T : class
        {
            var cached = await _cache.StringGetAsync(key);

            if (cached.HasValue)
            {
                try
                {
                    var deserialized = JsonHelper.DeserializeObject<T?>(cached!);
                    if (deserialized != null)
                        return deserialized;
                }
                catch (Exception ex)
                {
                    throw new ApiException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            var serialized = JsonHelper.SerializeObject(value);

            await _cache.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);

            return value;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fallback, TimeSpan? expiration = null)
            where T : class
        {
            var cached = await _cache.StringGetAsync(key);

            if (cached.HasValue)
            {
                var deserialized = JsonHelper.DeserializeObject<T>(cached!);
                if (deserialized != null)
                    return deserialized;
            }

            var value = await fallback() ?? throw new ApiException(HttpStatusCode.InternalServerError, $"Fallback returned null for key '{key}'");

            var serialized = JsonHelper.SerializeObject(value);
            await _cache.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);

            return value;
        }

        public async Task<T?> GetOrSetNullableAsync<T>(string key, Func<Task<T?>> fallback, TimeSpan? expiration = null)
            where T : class
        {
            var cached = await _cache.StringGetAsync(key);

            if (cached.HasValue)
            {
                var deserialized = JsonHelper.DeserializeObject<T>(cached!);
                if (deserialized != null)
                    return deserialized;
            }

            var value = await fallback();

            if (value != null)
            {
                var serialized = JsonHelper.SerializeObject(value);

                await _cache.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);
            }

            return value;
        }

        public async Task<T> SetAsync<T>(string key, T value, TimeSpan? expiration = null)
            where T : class
        {
            try
            {
                var serialized = JsonHelper.SerializeObject(value);

                await _cache.StringSetAsync(key, serialized, expiration ?? _defaultExpiration);

            }
            catch (Exception ex)
            {
                throw new ApiException(HttpStatusCode.InternalServerError, ex.Message);
            }

            return value;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                await _cache.KeyDeleteAsync(key);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
