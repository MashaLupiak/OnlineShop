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

        public async Task SetHashAsync(string key, string field, string value)
        {
            await _database.HashSetAsync(key, field, value);
        }

        public async Task<string> GetHashAsync(string key, string field)
        {
            return await _database.HashGetAsync(key, field);
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

        public async Task ExpireKeyAsync(string key, TimeSpan expiration)
        {
            await _database.KeyExpireAsync(key, expiration);
        }
    }
}
