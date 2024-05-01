using OnlineShop.Domain.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Cache
{
    public class RedisClient : ICacheClient
    {
        private readonly IDatabase _database;

        public RedisClient(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public async Task SetHashAsync<T>(string key, string field, T value)
        {
            // serialization
            var serializedValue = JsonSerializer.Serialize(value);
            await _database.HashSetAsync(key, field, serializedValue);
        }

        public async Task<T> GetHashAsync<T>(string key, string field)
        {
            var response = await _database.HashGetAsync(key, field);
            if (response.HasValue)
            {
                // deserialization
                var deserializedValue = JsonSerializer.Deserialize<T>(response);
                return deserializedValue;
            }
            else
            {
                // 1 - throw Ex
                // 2 - return default
                //throw new KeyNotFoundException($"Key '{key}' with field '{field}' not found.");
                return default(T);
            }
        }

        public async Task RemoveHashAsync(string key, string field)
        {
            await _database.HashDeleteAsync(key, field);
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> GetAllHashAsync(string key)
        {
            var result = await _database.HashGetAllAsync(key);
            return result.Select(entry => new KeyValuePair<string, string>(entry.Name.ToString(), entry.Value.ToString()));
        }

        public async Task RemoveKeyAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
        }

        public async Task ExpireKeyAsync(string key, TimeSpan expiration)
        {
            await _database.KeyExpireAsync(key, expiration);
        }
    }
}
