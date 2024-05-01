using System;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using System.Threading.Tasks;

namespace OnlineShop.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
       
        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

       public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product> GetProductByIdAsync(int productId)
        {
            return await _productRepository.GetByIdAsync(productId);
        }

        public async Task AddOrUpdateProductAsync(Product product)
        {
            await _productRepository.AddOrUpdateAsync(product);
        }
    }
}
