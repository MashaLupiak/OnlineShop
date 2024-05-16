using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OnlineShop.Domain.Entities;
using OnlineShop.Infrastructure.DataAccess;
using OnlineShop.Infrastructure.Repositories;

namespace OnlineShop.UnitTests
{
    public class ProductRepositoryTests
    {
        private readonly Mock<OnlineShopDbContext> _mockDbContext;
        private readonly Mock<DbSet<Product>> _mockProductSet;
        private readonly Mock<ILogger<ProductRepository>> _mockLogger;
        private readonly ProductRepository _sut;

        public ProductRepositoryTests()
        {
            _mockDbContext = new Mock<OnlineShopDbContext>();
            _mockProductSet = new Mock<DbSet<Product>>();
            _mockLogger = new Mock<ILogger<ProductRepository>>();
            _mockDbContext.Setup(ctx => ctx.Products).Returns(_mockProductSet.Object);
            _sut = new ProductRepository(_mockDbContext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
        {
            var existingProductId = 1;
            var existingProduct = new Product { ProductId = existingProductId, Name = "Existing Product" };

            _mockDbContext.Setup(db => db.Products.FindAsync(existingProductId))
                .Returns(ValueTask.FromResult(existingProduct));

            var productRepository = new ProductRepository(_mockDbContext.Object, _mockLogger.Object);

            // Act
            var retrievedProduct = await productRepository.GetByIdAsync(existingProductId);

            // Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal(existingProductId, retrievedProduct.ProductId);
            Assert.Equal("Existing Product", retrievedProduct.Name);
            _mockDbContext.Verify(db => db.Products.FindAsync(existingProductId), Times.Once);

        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowException_WhenInvalidId()
        {
            // Arrange
            int invalidId = -1; 
            _mockProductSet.Setup(set => set.FindAsync(invalidId))
                .ThrowsAsync(new InvalidOperationException("Invalid product ID."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetByIdAsync(invalidId));
        }


        [Fact]
        public async Task GetByIdAsync_ShouldReturnNullWhenNotExists()
        {
            // Arrange
            int nonExistentProductId = 999;
            _mockProductSet.Setup(set => set.Find(nonExistentProductId))
                .Returns((Product)null);

            // Act
            var actualProduct = await _sut.GetByIdAsync(nonExistentProductId);

            // Assert
            Assert.Null(actualProduct);
        }

        [Fact]
        public async Task AddOrUpdateAsync_ShouldAddNewProduct()
        {
            // Arrange
            var newProduct = new Product { Name = "New Product", Price = 9, Quantity = 20 };

            // Act
            await _sut.AddOrUpdateAsync(newProduct);

            // Assert
            _mockProductSet.Verify(set => set.AddAsync(newProduct, It.IsAny<CancellationToken>()), Times.Once);
            _mockDbContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateAsync_ShouldUpdateExistingProduct()
        {
            // Arrange
            var existingProduct = new Product { ProductId = 1, Name = "Existing Product", Price = 14, Quantity = 15 };
            _mockProductSet.Setup(set => set.Find(existingProduct.ProductId))
                .Returns(existingProduct);

            // Act
            existingProduct.Price = 19;
            await _sut.AddOrUpdateAsync(existingProduct);

            // Assert
            _mockProductSet.Verify(set => set.Update(existingProduct), Times.Once);
            _mockDbContext.Verify(ctx => ctx.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}


