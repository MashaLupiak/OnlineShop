using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Infrastructure.Repositories
{
    public class PurchasedProductRepository : IPurchasedProductRepository
    {
        private readonly OnlineShopDbContext _dbContext;

        public PurchasedProductRepository(OnlineShopDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddPurchasedProductAsync(PurchasedProduct purchasedProduct)
        {
            await _dbContext.PurchasedProducts.AddAsync(purchasedProduct);
            await _dbContext.SaveChangesAsync();
        }
    }
}
