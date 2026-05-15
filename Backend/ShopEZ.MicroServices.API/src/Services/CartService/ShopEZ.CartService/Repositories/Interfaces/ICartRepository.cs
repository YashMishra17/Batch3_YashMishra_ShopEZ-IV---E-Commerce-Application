using ShopEZ.CartService.Models;

namespace ShopEZ.CartService.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart> UpsertAsync(Cart cart);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task<bool> ExistsAsync(int userId);
    }
}