using OnlineShop.Domain.Entities;

namespace OnlineShop.Domain.Interfaces
{
    public interface IPurchasedProductRepository
    {
        Task AddPurchasedProductAsync(PurchasedProduct purchasedProduct);
    }
}
