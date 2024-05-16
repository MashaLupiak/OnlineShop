using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure;
using OnlineShop.Infrastructure.Cache;

namespace OnlineShop.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(IStorageService cacheService, IProductService productService, ILogger<CartController> logger)
        {
            _storageService = cacheService;
            _productService = productService;
            _logger = logger;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddToCart(int userId, CartItem cartItem)
        {
            _logger.LogInformation($"Adding product with ID {cartItem.ProductId} to cart for user ID {userId}");
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {cartItem.ProductId} not found.");
                return NotFound(new { error = "Product with the specified ID doesn't exist." });
            }
            if (product.Quantity < cartItem.Quantity)
            {
                _logger.LogWarning($"Not enough quantity available for product with ID {cartItem.ProductId}.");
                return BadRequest(new { error = "Not enough quantity available." });
            }
            await _storageService.AddToCartAsync(userId, cartItem);

            product.Quantity -= cartItem.Quantity;
            await _productService.AddOrUpdateProductAsync(product);
            _logger.LogInformation($"Product with ID {cartItem.ProductId} added to cart successfully.");
            return Ok();
        }


        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId)
        {
            _logger.LogInformation($"Removing product with ID {productId} from cart for user ID {userId}");
            await _storageService.RemoveFromCartAsync(userId, productId);
            _logger.LogInformation($"Product with ID {productId} removed from cart for user ID {userId}");
            return Ok();
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            _logger.LogInformation($"Buying products from cart for user ID {userId}");
            var cartItems = await _storageService.GetCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning($"Cart is empty for user ID {userId}");
                return NotFound(new { error = "Cart is empty for the specified user ID." });
            }
            return Ok(cartItems);
        }

       
        [HttpDelete("{userId}/checkout")]
        public async Task<IActionResult> BuyProductsFromCart(int userId)
        {
            // add 404 handling
            var cartItems = await _storageService.GetCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                _logger.LogWarning($"Cart not found for user ID {userId}");
                return NotFound(new { error = "Cart not found." });
            }
            await _storageService.BuyProductsFromCartAsync(userId);
            _logger.LogInformation($"Products bought from cart for user ID {userId}");
            return Ok();
        }
    }
}