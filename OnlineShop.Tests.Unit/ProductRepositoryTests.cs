using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineShop.API.Middleware;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.DataAccess;
using OnlineShop.Infrastructure.Repositories;
using StackExchange.Redis;
using OnlineShop.Infrastructure.Cache;
using Newtonsoft.Json.Linq;
using OnlineShop.Domain.Entities;
using Serilog;

namespace OnlineShop.Tests.Unit
{
    public class ProductRepositoryTests
    {
        private readonly Mock<DbSet<Product>> _mockDbSet;
        private readonly Mock<OnlineShopDbContext> _mockDbContext;
        private readonly Mock<ILogger<ProductRepository>> _mockLogger;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            _mockDbSet = new Mock<DbSet<Product>>();
            _mockDbContext = new Mock<OnlineShopDbContext>();
            _mockLogger = new Mock<ILogger<ProductRepository>>();

            _mockDbContext.Setup(x => x.Products).Returns(_mockDbSet.Object);

            _repository = new ProductRepository(_mockDbContext.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product 1" },
                new Product { ProductId = 2, Name = "Product 2" },
                new Product { ProductId = 3, Name = "Product 3" }
            };
            _mockDbSet.Setup(x => x.ToListAsync()).ReturnsAsync(products);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(products, result);
        }

        [Fact]
        public async Task AddOrUpdateAsync_AddsNewProduct()
        {
            // Arrange
            var newProduct = new Product { Name = "New Product" };

            // Act
            await _repository.AddOrUpdateAsync(newProduct);

            // Assert
            _mockDbSet.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateAsync_UpdatesExistingProduct()
        {
            // Arrange
            var existingProduct = new Product { ProductId = 1, Name = "Existing Product" };
            _mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(existingProduct);

            // Act
            existingProduct.Name = "Updated Product";
            await _repository.AddOrUpdateAsync(existingProduct);

            // Assert
            _mockDbContext.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct()
        {
            // Arrange
            var product = new Product { ProductId = 1, Name = "Product 1" };
            _mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Equal(product, result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenProductNotFound()
        {
            // Arrange
            _mockDbSet.Setup(x => x.FindAsync(1)).ReturnsAsync((Product)null);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

    }
}