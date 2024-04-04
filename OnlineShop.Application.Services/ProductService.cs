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

        public async Task<int> AddOrUpdateProduct(Product product)
        {
            return await _productRepository.AddOrUpdateProduct(product);
        }

        public async Task<Product> GetProductDetails(int productId)
        {
            return await _productRepository.GetProductDetails(productId);
        }

    }
}
