using Moq;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.Cache;
using StackExchange.Redis;
using System.Text.Json;

namespace OnlineShop.UnitTests
{
    public class RedisClientTests
    {
        private Mock<IConnectionMultiplexer> _mockMultiplexer;
        private Mock<IDatabase> _mockDatabase;
        private RedisClient _sut;

        public RedisClientTests()
        {
            _mockMultiplexer = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();
            _mockMultiplexer.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);
            _sut = new RedisClient(_mockMultiplexer.Object);
        }

        [Fact]
        public async Task SetHashAsync_ShouldSetHashInRedis()
        {
            // Arrange
            var key = "cart";
            var field = "1";
            var cartItem = new CartItem { ProductId = 1, Quantity = 2 };

            // Act
            await _sut.SetHashAsync(key, field, cartItem);

            // Assert
            _mockDatabase.Verify(db => db.HashSetAsync(key, field, It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task GetHashAsync_ShouldGetHashFromRedis()
        {
            // Arrange
            var key = "cart";
            var field = "1";
            var cartItem = new CartItem { ProductId = 1, Quantity = 2 };
            var serializedCartItem = JsonSerializer.Serialize(cartItem);

            _mockDatabase.Setup(db => db.HashGetAsync(key, field, It.IsAny<CommandFlags>())).ReturnsAsync(serializedCartItem);

            // Act
            var result = await _sut.GetHashAsync<CartItem>(key, field);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cartItem.ProductId, result.ProductId);
            Assert.Equal(cartItem.Quantity, result.Quantity);
        }

        [Fact]
        public async Task RemoveHashAsync_ShouldRemoveHashFromRedis()
        {
            // Arrange
            var key = "cart";
            var field = "1";

            // Act
            await _sut.RemoveHashAsync(key, field);

            // Assert
            _mockDatabase.Verify(db => db.HashDeleteAsync(key, field, It.IsAny<CommandFlags>()), Times.Once);
        }
    }
}
