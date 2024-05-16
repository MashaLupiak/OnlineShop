using Microsoft.Extensions.Logging;
using Moq;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.Cache;
using System.Text.Json;

namespace OnlineShop.UnitTests
{
    public class StorageServiceTests
    {
        private Mock<ICacheClient> _mockCacheClient;
        private Mock<IProductRepository> _mockProductRepository;
        private Mock<IPurchasedProductRepository> _mockPurchasedProductRepository;
        private Mock<ILogger<StorageService>> _mockLogger;

        private StorageService CreateStorageService()
        {
            return new StorageService(
                _mockCacheClient.Object,
                _mockProductRepository.Object,
                _mockPurchasedProductRepository.Object,
                _mockLogger.Object
            );
        }

        public StorageServiceTests()
        {
            _mockCacheClient = new Mock<ICacheClient>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockPurchasedProductRepository = new Mock<IPurchasedProductRepository>();
            _mockLogger = new Mock<ILogger<StorageService>>();
        }

        [Fact]
        public async Task AddToCartAsync_ShouldAddCartItemToCache()
        {
            // Arrange
            var storageService = CreateStorageService();

            var userId = 123;
            var cartItem = new CartItem { ProductId = 1, Quantity = 2 };

            // Act
            await storageService.AddToCartAsync(userId, cartItem);

            // Assert
            _mockCacheClient.Verify(
                client => client.SetHashAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CartItem>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task RemoveFromCartAsync_ShouldRemoveCartItemFromCache()
        {
            // Arrange
            var storageService = CreateStorageService();

            var userId = 123;
            var productId = 1;

            // Act
            await storageService.RemoveFromCartAsync(userId, productId);

            // Assert
            _mockCacheClient.Verify(
                client => client.RemoveHashAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetCartAsync_ShouldReturnCartItemsFromCache()
        {
            // Arrange
            var storageService = CreateStorageService();

            var userId = 123;
            var mockCartItems = new List<CartItem>
            {
                new CartItem { ProductId = 1, Quantity = 2 },
                new CartItem { ProductId = 2, Quantity = 1 }
            };

            _mockCacheClient.Setup(client => client.GetAllHashAsync(It.IsAny<string>()))
                .ReturnsAsync(mockCartItems.ToDictionary(item => item.ProductId.ToString(), item => JsonSerializer.Serialize(item)));

            // Act
            var result = await storageService.GetCartAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal(3, result.Sum(item => item.Quantity));
        }

        [Fact]
        public async Task BuyProductsFromCartAsync_ShouldBuyProductsAndClearCart()
        {
            // Arrange
            var storageService = CreateStorageService();

            var userId = 123;
            var mockCartItems = new List<CartItem>
            {
                new CartItem { ProductId = 1, Quantity = 2 },
                new CartItem { ProductId = 2, Quantity = 1 }
            };

            _mockCacheClient.Setup(client => client.GetAllHashAsync(It.IsAny<string>()))
                .ReturnsAsync(mockCartItems.ToDictionary(item => item.ProductId.ToString(), item => JsonSerializer.Serialize(item)));

            _mockProductRepository.SetupSequence(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Product { ProductId = 1, Quantity = 5 })  // Assuming product quantity is available
                 .ReturnsAsync(new Product { ProductId = 2, Quantity = 3 }); // Assuming product quantity is available

            // Act
            await storageService.BuyProductsFromCartAsync(userId);

            // Assert
            _mockProductRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<int>()), Times.Exactly(2)); // Assuming 2 items in cart
            _mockProductRepository.Verify(repo => repo.AddOrUpdateAsync(It.IsAny<Product>()), Times.Exactly(2)); // Assuming 2 items in cart
            _mockPurchasedProductRepository.Verify(repo => repo.AddPurchasedProductAsync(It.IsAny<PurchasedProduct>()), Times.Exactly(2)); // Assuming 2 items in cart
            _mockCacheClient.Verify(client => client.RemoveKeyAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
