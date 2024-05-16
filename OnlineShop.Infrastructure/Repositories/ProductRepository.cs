using System;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.DataAccess;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OnlineShopDbContext _dbContext;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(OnlineShopDbContext dbContext, ILogger<ProductRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var products = await _dbContext.Products.ToListAsync();
                _logger.LogDebug("Successfully retrieved all products.");
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting all products.");
                throw;
            }
        }

        public async Task AddOrUpdateAsync(Product product)
        {
            try
            {
                if (product.ProductId == 0)
                {
                    await _dbContext.Products.AddAsync(product);
                    _logger.LogDebug("Successfully added product.");
                }
                else
                {
                    _dbContext.Products.Update(product);
                    _logger.LogDebug("Successfully updated product.");
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding/updating product.");
                throw;
            }
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            try
            {
                var product = await _dbContext.Products.FindAsync(id);
                if (product != null)
                {
                    _logger.LogDebug("Successfully retrieved product with ID {ProductId}.", id);
                }
                else
                {
                    _logger.LogWarning("Product with ID {ProductId} not found.", id);
                }
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting product with ID {ProductId}.", id);
                throw;
            }
        }
    } 
}
