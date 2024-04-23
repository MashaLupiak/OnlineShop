using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
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
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;    

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();            
        }

        public async Task AddToCartAsync(int userId, CartItem cartItem)
        {
            var cartKey = $"cart:{userId}";
            var cartItemKey = $"{cartItem.ProductId}";

            var existingCartItemJson = await _database.HashGetAsync(cartKey, cartItemKey);
            if (existingCartItemJson.HasValue)
            {
                var existingCartItem = JsonSerializer.Deserialize<CartItem>(existingCartItemJson);
                existingCartItem.Quantity += cartItem.Quantity;
                var updatedCartItemJson = JsonSerializer.Serialize(existingCartItem);
                await _database.HashSetAsync(cartKey, cartItemKey, updatedCartItemJson);
            }
            else
            {
                var cartItemJson = JsonSerializer.Serialize(cartItem);
                await _database.HashSetAsync(cartKey, cartItemKey, cartItemJson);
            }
            await _database.KeyExpireAsync(cartKey, TimeSpan.FromDays(1));
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        { 
            var cartKey = $"cart:{userId}";
            await _database.HashDeleteAsync(cartKey, productId);
        }

        public async Task<IEnumerable<CartItem>> GetCartAsync(int userId)
        {
            var cartKey = $"cart:{userId}";
            var cartItemsHash = await _database.HashGetAllAsync(cartKey);

            var cartItems = cartItemsHash
                .Select(cartItemEntry => JsonSerializer.Deserialize<CartItem>(cartItemEntry.Value)) 
                .GroupBy(cartItem => cartItem.ProductId) 
                .Select(group => new CartItem 
                {
                    ProductId = group.Key,
                    Quantity = group.Sum(item => item.Quantity)
                });

            return cartItems;    
        }
    }
}
