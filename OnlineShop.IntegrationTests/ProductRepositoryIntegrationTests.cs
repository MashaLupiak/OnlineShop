using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.DataAccess;
using OnlineShop.Infrastructure.Repositories;

namespace OnlineShop.IntegrationTests
{
    public class ProductRepositoryIntegrationTests : IDisposable
    {
        private readonly OnlineShopDbContext _dbContext;
        private readonly IProductRepository _productRepository;

        public ProductRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<OnlineShopDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new OnlineShopDbContext(options);
            _productRepository = new ProductRepository(_dbContext, NullLogger<ProductRepository>.Instance); 
        }

        [Fact]
        public async Task CanAddProductToDatabase()
        {
            // Arrange
            var product = new Product { Name = "Test Product", Quantity = 10 };

            // Act
            await _productRepository.AddOrUpdateAsync(product);
            var retrievedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == "Test Product");

            // Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal(product.Quantity, retrievedProduct.Quantity);
        }

        [Fact]
        public async Task CanUpdateProductInDatabase()
        {
            // Arrange
            var product = new Product { Name = "Test Product", Quantity = 10 };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            product.Quantity = 20;
            await _productRepository.AddOrUpdateAsync(product);
            var updatedProduct = await _dbContext.Products.FirstOrDefaultAsync(p => p.Name == "Test Product");

            // Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal(20, updatedProduct.Quantity);
        }

        [Fact]
        public async Task CanGetProductByIdFromDatabase()
        {
            // Arrange
            var product = new Product { Name = "Test Product", Quantity = 10 };
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrievedProduct = await _productRepository.GetByIdAsync(product.ProductId);

            // Assert
            Assert.NotNull(retrievedProduct);
            Assert.Equal(product.Quantity, retrievedProduct.Quantity);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
