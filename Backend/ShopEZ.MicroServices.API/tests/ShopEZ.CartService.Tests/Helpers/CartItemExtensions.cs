using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Models;

namespace ShopEZ.CartService.Tests.Helpers
{
    /// <summary>
    /// Extension methods that make test arrange code more concise.
    /// </summary>
    public static class CartItemExtensions
    {
        public static CartItemDTO ToDTO(this CartItem item) => new()
        {
            ProductId = item.ProductId,
            Name = item.Name,
            Price = item.Price,
            Quantity = item.Quantity,
            Stock = item.Stock,
            ImageUrl = item.ImageUrl
        };

        public static CartItem ToModel(this CartItemDTO dto) => new()
        {
            ProductId = dto.ProductId,
            Name = dto.Name,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl
        };
    }
}