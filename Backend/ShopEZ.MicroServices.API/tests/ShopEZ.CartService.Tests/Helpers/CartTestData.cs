using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Models;

namespace ShopEZ.CartService.Tests.Helpers
{
    /// <summary>
    /// Canonical test objects reused across every test class.
    /// Each method returns a fresh instance so tests cannot share state.
    /// </summary>
    public static class CartTestData
    {
        // ── Constants ──────────────────────────────────────────────────────────
        public const int UserIdAlice = 1;
        public const int UserIdBob = 2;
        public const int UserIdCharlie = 3;

        public const int MouseProductId = 1;
        public const int KeyboardProductId = 2;
        public const int HubProductId = 3;

        // ── CartItem models ───────────────────────────────────────────────────

        public static CartItem MouseItem() => new()
        {
            ProductId = MouseProductId,
            Name = "Wireless Mouse",
            Price = 29.99m,
            Quantity = 1,
            Stock = 100,
            ImageUrl = "https://via.placeholder.com/300?text=Mouse"
        };

        public static CartItem KeyboardItem() => new()
        {
            ProductId = KeyboardProductId,
            Name = "Mechanical Keyboard",
            Price = 79.99m,
            Quantity = 1,
            Stock = 50,
            ImageUrl = "https://via.placeholder.com/300?text=Keyboard"
        };

        public static CartItem HubItem() => new()
        {
            ProductId = HubProductId,
            Name = "USB-C Hub",
            Price = 49.99m,
            Quantity = 2,
            Stock = 75,
            ImageUrl = "https://via.placeholder.com/300?text=Hub"
        };

        public static CartItem OutOfStockItem() => new()
        {
            ProductId = 99,
            Name = "OOS Item",
            Price = 9.99m,
            Quantity = 1,
            Stock = 0,
            ImageUrl = string.Empty
        };

        public static CartItem LowStockItem() => new()
        {
            ProductId = 10,
            Name = "Low Stock Item",
            Price = 19.99m,
            Quantity = 1,
            Stock = 2,
            ImageUrl = string.Empty
        };

        // ── CartItemDTOs ──────────────────────────────────────────────────────

        public static CartItemDTO MouseDTO() => new()
        {
            ProductId = MouseProductId,
            Name = "Wireless Mouse",
            Price = 29.99m,
            Quantity = 1,
            Stock = 100,
            ImageUrl = "https://via.placeholder.com/300?text=Mouse"
        };

        public static CartItemDTO KeyboardDTO() => new()
        {
            ProductId = KeyboardProductId,
            Name = "Mechanical Keyboard",
            Price = 79.99m,
            Quantity = 1,
            Stock = 50,
            ImageUrl = "https://via.placeholder.com/300?text=Keyboard"
        };

        public static CartItemDTO HubDTO() => new()
        {
            ProductId = HubProductId,
            Name = "USB-C Hub",
            Price = 49.99m,
            Quantity = 2,
            Stock = 75,
            ImageUrl = string.Empty
        };

        // ── Cart models ────────────────────────────────────────────────────────

        public static Cart EmptyCart(int userId = UserIdAlice) => new()
        {
            UserId = userId,
            Items = new List<CartItem>(),
            UpdatedAt = DateTime.UtcNow
        };

        public static Cart SingleItemCart(int userId = UserIdAlice) => new()
        {
            UserId = userId,
            Items = new List<CartItem> { MouseItem() },
            UpdatedAt = DateTime.UtcNow
        };

        public static Cart MultiItemCart(int userId = UserIdAlice) => new()
        {
            UserId = userId,
            Items = new List<CartItem> { MouseItem(), KeyboardItem(), HubItem() },
            UpdatedAt = DateTime.UtcNow
        };
    }
}