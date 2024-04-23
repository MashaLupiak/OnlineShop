using System;
using OnlineShop.Domain.Entities;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface IProductService
    {

        Task<IEnumerable<Product>> GetAllProductsAsync();

        Task<Product> GetProductByIdAsync(int productId);

        Task AddOrUpdateProductAsync(Product product);
    }
}
