using OnlineShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Domain.Interfaces
{
    public interface IPurchasedProductRepository
    {
        Task AddPurchasedProductAsync(PurchasedProduct purchasedProduct);
    }
}
