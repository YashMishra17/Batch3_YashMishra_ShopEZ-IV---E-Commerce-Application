using ShopEZ.CartService.DTOs;

namespace ShopEZ.CartService.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetCartAsync(int userId);
        Task<CartDTO> AddItemAsync(int userId, CartItemDTO itemDto);
        Task<CartDTO> UpdateItemAsync(int userId, int productId, UpdateCartItemDTO dto);
        Task<CartDTO> RemoveItemAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
    }
}