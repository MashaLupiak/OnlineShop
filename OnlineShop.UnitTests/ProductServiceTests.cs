using Moq;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace OnlineShop.UnitTests
{
    public class ProductServiceTests
    {
        private Mock<IProductRepository> _mockProductRepository;
        private ProductService _sut;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _sut = new ProductService(_mockProductRepository.Object);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Arrange
            var mockProducts = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product 1", Price = 10 },
                new Product { ProductId = 2, Name = "Product 2", Price = 20 },
                new Product { ProductId = 3, Name = "Product 3", Price = 30 }
            };
            _mockProductRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(mockProducts);

            // Act
            var result = await _sut.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetProductByIdAsync_ShouldReturnProductWithGivenId()
        {
            // Arrange
            var productId = 1;
            var mockProduct = new Product { ProductId = productId, Name = "Product 1", Price = 10 };
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(productId)).ReturnsAsync(mockProduct);

            // Act
            var result = await _sut.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
        }

        [Fact]
        public async Task AddOrUpdateProductAsync_ShouldCallRepositoryMethod()
        {
            // Arrange
            var productToAddOrUpdate = new Product { ProductId = 1, Name = "Product 1", Price = 10 };

            // Act
            await _sut.AddOrUpdateProductAsync(productToAddOrUpdate);

            // Assert
            _mockProductRepository.Verify(repo => repo.AddOrUpdateAsync(productToAddOrUpdate), Times.Once);
        }
    }
}
