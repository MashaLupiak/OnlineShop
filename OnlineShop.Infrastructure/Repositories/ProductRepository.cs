using System;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.DataAccess;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OnlineShopDbContext _dbContext;

        public ProductRepository(OnlineShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> AddOrUpdateProduct(Product product)
        {
            _dbContext.Products.Add(product);
            return await _dbContext.SaveChangesAsync();
        }

        public async Task<Product> GetProductDetails(int productId)
        {
            return await _dbContext.Products.FindAsync(productId);
        }
    }
}
