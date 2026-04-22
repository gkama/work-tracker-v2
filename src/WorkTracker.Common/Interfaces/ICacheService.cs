namespace WorkTracker.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<IEnumerable<T>> GetOrSetAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiration = null) where T : class;
        Task<T> GetOrSetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fallback, TimeSpan? expiration = null) where T : class;
        Task<T?> GetOrSetNullableAsync<T>(string key, Func<Task<T?>> fallback, TimeSpan? expiration = null) where T : class;
        Task<T> SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        Task<bool> RemoveAsync(string key);
    }
}
