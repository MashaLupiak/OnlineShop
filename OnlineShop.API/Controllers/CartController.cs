using Microsoft.AspNetCore.Mvc;
using OnlineShop.Application.Services;
using OnlineShop.Domain.Entities;
using OnlineShop.Domain.Interfaces;
using OnlineShop.Infrastructure;

namespace OnlineShop.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;

        public CartController(ICacheService cacheService, IProductService productService)
        {
            _cacheService = cacheService;
            _productService = productService;
        }

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddToCart(int userId, CartItem cartItem)
        {
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            if (product == null)
            {
                return NotFound(new { error = "Product with the specified ID doesn't exist." });
            }
            if (product.Quantity < cartItem.Quantity)
            {
                return BadRequest(new { error = "Not enough quantity available." });
            }
            await _cacheService.AddToCartAsync(userId, cartItem);

            product.Quantity -= cartItem.Quantity;
            await _productService.AddOrUpdateProductAsync(product);
            return Ok();
        }


        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId)
        {
            await _cacheService.RemoveFromCartAsync(userId, productId);
            return Ok();
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var cartItems = await _cacheService.GetCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return NotFound(new { error = "Cart is empty for the specified user ID." });
            }
            return Ok(cartItems);
        }
    }
}