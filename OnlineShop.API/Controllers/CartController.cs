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

        public CartController(IStorageService cacheService, IProductService productService)
        {
            _storageService = cacheService;
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
            await _storageService.AddToCartAsync(userId, cartItem);

            product.Quantity -= cartItem.Quantity;
            await _productService.AddOrUpdateProductAsync(product);
            return Ok();
        }


        [HttpDelete("{userId}/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int productId)
        {
            await _storageService.RemoveFromCartAsync(userId, productId);
            return Ok();
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var cartItems = await _storageService.GetCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return NotFound(new { error = "Cart is empty for the specified user ID." });
            }
            return Ok(cartItems);
        }

        [HttpDelete("{cartId}/checkout")]
        public async Task<IActionResult> BuyProductsFromCart(int userId)
        {
            await _storageService.BuyProductsFromCartAsync(userId);
            // add 404 handling
            var cartItems = await _storageService.GetCartAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return NotFound(new { error = "Cart not found." });
            }
            return Ok();
        }
    }
}