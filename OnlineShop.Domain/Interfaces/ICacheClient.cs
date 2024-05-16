namespace OnlineShop.Domain.Interfaces
{
    public interface ICacheClient
    {
        Task SetHashAsync<T>(string key, string field, T value);
        Task<T> GetHashAsync<T>(string key, string field);
        Task RemoveHashAsync(string key, string field);
        Task<IEnumerable<KeyValuePair<string, string>>> GetAllHashAsync(string key);
        Task ExpireKeyAsync(string key, TimeSpan expiration);
        Task RemoveKeyAsync(string key);
    }
}
