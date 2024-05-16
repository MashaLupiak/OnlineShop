using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;

namespace OnlineShop.API.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;   
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            _logger.LogInformation("Retrieving all products");
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            _logger.LogInformation($"Retrieving product with ID {id}");
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product not found with ID {id}");
                return NotFound(new { error = "Product not found with the specified ID."});
            }
            return Ok(product);
        }

        [HttpPut]
        public async Task<IActionResult> AddOrUpdateProduct(Product product)
        {
            _logger.LogInformation($"Adding or updating product with ID {product.ProductId}");
            await _productService.AddOrUpdateProductAsync(product);
            _logger.LogInformation($"Product with ID {product.ProductId} added or updated successfully.");
            return Ok();
        }
    }
}
