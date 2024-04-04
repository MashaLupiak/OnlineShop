﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPut]
        public async Task<IActionResult> AddOrUpdateProduct(Product product)
        {
            try
            {
                product.ProductId = 0;
                var result = await _productService.AddOrUpdateProduct(product);
                if (result > 0)
                {
                    return Ok(new { Message = "Product added/updated successfully.", ProductId = result });
                }
                else
                {
                    return BadRequest(new { Message = "Failed to add/update product." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error.", Error = ex.Message });
            }
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            try
            {
                var product = await _productService.GetProductDetails(productId);
                if (product != null)
                {
                    return Ok(product);
                }
                else
                {
                    return NotFound(new { Message = "Product not found." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error.", Error = ex.Message });
            }
        }
    }
}
