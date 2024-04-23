using System;
using OnlineShop.Domain.Entities;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddOrUpdateAsync(Product product);
    }
}
