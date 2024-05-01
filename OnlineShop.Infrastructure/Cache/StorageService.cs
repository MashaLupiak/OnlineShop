using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Repositories;
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
        private readonly IProductRepository _productRepository;
        private readonly IPurchasedProductRepository _purchasedProductRepository;


        public StorageService(ICacheClient cacheClient, IProductRepository productRepository, IPurchasedProductRepository purchasedProductRepository)
        {
            _cacheClient = cacheClient;
            _productRepository = productRepository;
            _purchasedProductRepository = purchasedProductRepository;
        }

        public string GetCartKey(int userId) => $"cart:{userId}";
        public string GetCartItemKey(int productId) => $"{productId}";

        public async Task AddToCartAsync(int userId, CartItem cartItem)
        {
            var cartKey = GetCartKey(userId);
            var cartItemKey = GetCartItemKey(cartItem.ProductId);

            var existingCartItem = await _cacheClient.GetHashAsync<CartItem>(cartKey, cartItemKey);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += cartItem.Quantity;
                await _cacheClient.SetHashAsync(cartKey, cartItemKey, existingCartItem);
            }
            else
            {
                await _cacheClient.SetHashAsync(cartKey, cartItemKey, cartItem);
            }

            await _cacheClient.ExpireKeyAsync(cartKey, TimeSpan.FromDays(1));
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cartKey = GetCartKey(userId);
            await _cacheClient.RemoveHashAsync(cartKey, productId.ToString());
        }

        public async Task<IEnumerable<CartItem>> GetCartAsync(int userId)
        {
            var cartKey = GetCartKey(userId);
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

        public async Task BuyProductsFromCartAsync(int userId)
        {
            var cartKey = GetCartKey(userId);

            // purchased products should be stored in SQL
            // table((id), userId, productId, quantity, timestamp)
            // timestamp should be saved directly in SQL (consider triger if there are no default)
            var cartItems = await GetCartAsync(userId);
            foreach (var cartItem in cartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null && product.Quantity >= cartItem.Quantity)
                {
                    product.Quantity -= cartItem.Quantity;
                    await _productRepository.AddOrUpdateAsync(product);

                    var purchasedProduct = new PurchasedProduct
                    {
                        UserId = userId,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity
                    };

                    await _purchasedProductRepository.AddPurchasedProductAsync(purchasedProduct);
                }
                else
                {
                    throw new Exception("Product not available in sufficient quantity.");
                }
            }
            await _cacheClient.RemoveKeyAsync(cartKey);

        }
    }

}

