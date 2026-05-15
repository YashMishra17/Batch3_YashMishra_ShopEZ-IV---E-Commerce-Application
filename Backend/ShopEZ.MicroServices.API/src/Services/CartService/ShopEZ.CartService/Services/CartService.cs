using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Exceptions;
using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories.Interfaces;
using ShopEZ.CartService.Services.Interfaces;

namespace ShopEZ.CartService.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repo;

        public CartService(ICartRepository repo)
        {
            _repo = repo;
        }

        public async Task<CartDTO> GetCartAsync(int userId)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            Cart? cart = await _repo.GetByUserIdAsync(userId);
            return cart is null
                ? new CartDTO { UserId = userId }
                : MapToDTO(cart);
        }

        public async Task<CartDTO> AddItemAsync(int userId, CartItemDTO itemDto)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            if (itemDto is null)
                throw new AppException("Cart item data cannot be null.", 400);

            Cart? cart = await _repo.GetByUserIdAsync(userId)
                         ?? new Cart { UserId = userId };

            CartItem? existing = cart.Items
                .FirstOrDefault(i => i.ProductId == itemDto.ProductId);

            if (existing is not null)
            {
                int newQty = existing.Quantity + itemDto.Quantity;

                if (itemDto.Stock > 0 && newQty > itemDto.Stock)
                    throw new AppException(
                        $"Requested quantity ({newQty}) exceeds available stock ({itemDto.Stock}).",
                        400);

                existing.Quantity = newQty;
                existing.Price = itemDto.Price;
                existing.Stock = itemDto.Stock;
                existing.Name = itemDto.Name;
                existing.ImageUrl = itemDto.ImageUrl;
            }
            else
            {
                if (itemDto.Stock > 0 && itemDto.Quantity > itemDto.Stock)
                    throw new AppException(
                        $"Requested quantity ({itemDto.Quantity}) exceeds available stock ({itemDto.Stock}).",
                        400);

                cart.Items.Add(new CartItem
                {
                    ProductId = itemDto.ProductId,
                    Name = itemDto.Name,
                    Price = itemDto.Price,
                    Quantity = itemDto.Quantity,
                    Stock = itemDto.Stock,
                    ImageUrl = itemDto.ImageUrl
                });
            }

            Cart saved = await _repo.UpsertAsync(cart);
            return MapToDTO(saved);
        }

        public async Task<CartDTO> UpdateItemAsync(
            int userId, int productId, UpdateCartItemDTO dto)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            if (dto is null)
                throw new AppException("Update data cannot be null.", 400);

            Cart? cart = await _repo.GetByUserIdAsync(userId);

            if (cart is null)
                throw new AppException(
                    $"Cart for user {userId} was not found.", 404);

            CartItem? item = cart.Items
                .FirstOrDefault(i => i.ProductId == productId);

            if (item is null)
                throw new AppException(
                    $"Product with ID {productId} was not found in the cart.", 404);

            if (item.Stock > 0 && dto.Quantity > item.Stock)
                throw new AppException(
                    $"Requested quantity ({dto.Quantity}) exceeds available stock ({item.Stock}).",
                    400);

            item.Quantity = dto.Quantity;

            Cart saved = await _repo.UpsertAsync(cart);
            return MapToDTO(saved);
        }

        public async Task<CartDTO> RemoveItemAsync(int userId, int productId)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            Cart? cart = await _repo.GetByUserIdAsync(userId);

            if (cart is null)
                throw new AppException(
                    $"Cart for user {userId} was not found.", 404);

            bool removed = await _repo.RemoveItemAsync(userId, productId);

            if (!removed)
                throw new AppException(
                    $"Product with ID {productId} was not found in the cart.", 404);

            Cart? updated = await _repo.GetByUserIdAsync(userId);
            return MapToDTO(updated ?? new Cart { UserId = userId });
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            return await _repo.DeleteByUserIdAsync(userId);
        }

        private static CartDTO MapToDTO(Cart cart) => new()
        {
            UserId = cart.UserId,
            Total = cart.Total,
            TotalItems = cart.TotalItems,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items.Select(i => new CartItemDTO
            {
                ProductId = i.ProductId,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity,
                Stock = i.Stock,
                ImageUrl = i.ImageUrl
            }).ToList()
        };
    }
}