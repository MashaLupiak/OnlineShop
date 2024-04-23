using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Cache
{
    public class StorageService : IStorageService
    {
        private readonly ICacheClient _cacheClient;
        private readonly Func<int, string> _getCartKey;
        private readonly Func<int, string> _getCartItemKey;
        public StorageService(ICacheClient cacheClient)
        {
            _cacheClient = cacheClient;
            _getCartKey = userId => $"cart:{userId}";
            _getCartItemKey = productId => $"{productId}";
        }

        public async Task AddToCartAsync(int userId, CartItem cartItem)
        {
            var cartKey = _getCartKey(userId);
            var cartItemKey = _getCartItemKey(cartItem.ProductId);

            var existingCartItemJson = await _cacheClient.GetHashAsync(cartKey, cartItemKey);
            if (!string.IsNullOrEmpty(existingCartItemJson))
            {
                var existingCartItem = JsonSerializer.Deserialize<CartItem>(existingCartItemJson);
                existingCartItem.Quantity += cartItem.Quantity;
                var updatedCartItemJson = JsonSerializer.Serialize(existingCartItem);
                await _cacheClient.SetHashAsync(cartKey, cartItemKey, updatedCartItemJson);
            }
            else
            {
                var cartItemJson = JsonSerializer.Serialize(cartItem);
                await _cacheClient.SetHashAsync(cartKey, cartItemKey, cartItemJson);
            }

            await _cacheClient.ExpireKeyAsync(cartKey, TimeSpan.FromDays(1));
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cartKey = _getCartKey(userId);
            await _cacheClient.RemoveHashAsync(cartKey, productId.ToString());
        }

        public async Task<IEnumerable<CartItem>> GetCartAsync(int userId)
        {
            var cartKey = _getCartKey(userId);
            var cartItemsHash = await _cacheClient.GetAllHashAsync(cartKey);

            var cartItems = cartItemsHash
                .Select(cartItemJson => JsonSerializer.Deserialize<CartItem>(cartItemJson.Value))
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

