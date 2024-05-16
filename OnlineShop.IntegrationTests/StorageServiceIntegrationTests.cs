using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Cache;
using OnlineShop.Infrastructure.DataAccess;
using OnlineShop.Infrastructure.Repositories;
using StackExchange.Redis;

namespace OnlineShop.IntegrationTests
{
    public class StorageServiceIntegrationTests : IDisposable
    {
        private readonly IStorageService _storageService;
        private readonly OnlineShopDbContext _dbContext; 

        public StorageServiceIntegrationTests()
        {
            var services = new ServiceCollection();
            services.AddDbContext<OnlineShopDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConnectionString = "localhost:6379";
                return ConnectionMultiplexer.Connect(redisConnectionString);
            });
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IPurchasedProductRepository, PurchasedProductRepository>();
            services.AddScoped<ICacheClient, RedisClient>();
            services.AddScoped<IStorageService, StorageService>();
            services.AddScoped<IProductService, ProductService>(); 
            services.AddLogging(builder => builder.AddConsole().AddDebug().SetMinimumLevel(LogLevel.Information));

            var serviceProvider = services.BuildServiceProvider();
            _storageService = serviceProvider.GetRequiredService<IStorageService>();
            _dbContext = serviceProvider.GetRequiredService<OnlineShopDbContext>(); 
        }
        [Fact]
        public async Task CanAddProductToCart()
        {
            // Arrange
            var product = new Product { Name = "Test Product", Quantity = 10 };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var cartItem = new CartItem { ProductId = product.ProductId, Quantity = 2 };
            var userId = 1;

            // Act
            await _storageService.AddToCartAsync(userId, cartItem);
            var cartItems = await _storageService.GetCartAsync(userId);

            // Assert
            Assert.Single(cartItems); 
            Assert.Equal(cartItem.Quantity, cartItems.First().Quantity); 
        }

        [Fact]
        public async Task CanBuyProductsFromCart()
        {
            // Arrange
            var product = new Product { Name = "Test Product", Quantity = 10 };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            var cartItem = new CartItem { ProductId = product.ProductId, Quantity = 2 };
            var userId = 1;

            // Act
            await _storageService.AddToCartAsync(userId, cartItem);
            await _storageService.BuyProductsFromCartAsync(userId);
            var updatedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
            var purchasedProduct = await _dbContext.PurchasedProducts.FirstOrDefaultAsync(p => p.ProductId == product.ProductId && p.UserId == userId);

            // Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal(product.Quantity - cartItem.Quantity, updatedProduct.Quantity); 
            Assert.NotNull(purchasedProduct); 
            Assert.Equal(cartItem.Quantity, purchasedProduct.Quantity); 
        }

        [Fact]
        public async Task CanRemoveProductFromCart()
        {
            // Arrange
            var cartItem = new CartItem { ProductId = 1, Quantity = 2 };
            var userId = 1;
            await _storageService.AddToCartAsync(userId, cartItem);

            // Act
            await _storageService.RemoveFromCartAsync(userId, cartItem.ProductId);
            var cartItems = await _storageService.GetCartAsync(userId);

            // Assert
            Assert.Empty(cartItems);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

    }

}
