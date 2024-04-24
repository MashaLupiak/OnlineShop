using OnlineShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface IStorageService
    {
        Task AddToCartAsync(int userId, CartItem cartItem);
        Task RemoveFromCartAsync(int userId, int productId);
        Task<IEnumerable<CartItem>> GetCartAsync(int userId);
        Task BuyProductsFromCartAsync(int userId);
    }
}
