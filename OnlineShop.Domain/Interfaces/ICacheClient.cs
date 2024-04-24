using OnlineShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface ICacheClient
    {
        Task SetHashAsync(string key, string field, string value);
        Task<string> GetHashAsync(string key, string field);
        Task RemoveHashAsync(string key, string field);
        Task<IEnumerable<KeyValuePair<string, string>>> GetAllHashAsync(string key);
        Task ExpireKeyAsync(string key, TimeSpan expiration);
        Task RemoveKeyAsync(string key);
    }
}
