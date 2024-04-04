using System;
using OnlineShop.Domain.Entities;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<int> AddOrUpdateProduct(Product product);
        Task<Product> GetProductDetails(int productId);

    }
}
