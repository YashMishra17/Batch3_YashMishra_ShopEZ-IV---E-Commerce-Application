using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Models;

namespace ShopEZ.ProductService.Tests.Helpers
{
    /// <summary>
    /// Central factory for all test objects.
    /// Every method returns a fresh instance — no shared mutable state.
    /// </summary>
    public static class ProductTestData
    {
        // ── Model helpers ─────────────────────────────────────────────────────

        public static Product Mouse() => new()
        {
            ProductId = 1,
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse with USB receiver",
            Price = 29.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Mouse",
            Stock = 100
        };

        public static Product Keyboard() => new()
        {
            ProductId = 2,
            Name = "Mechanical Keyboard",
            Description = "RGB mechanical keyboard with blue switches",
            Price = 79.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Keyboard",
            Stock = 50
        };

        public static Product Hub() => new()
        {
            ProductId = 3,
            Name = "USB-C Hub",
            Description = "7-in-1 USB-C hub with HDMI and SD card reader",
            Price = 49.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Hub",
            Stock = 75
        };

        public static Product OutOfStockProduct() => new()
        {
            ProductId = 4,
            Name = "Out of Stock Item",
            Description = "No stock available",
            Price = 9.99m,
            ImageUrl = string.Empty,
            Stock = 0
        };

        public static List<Product> ThreeProducts()
            => new() { Mouse(), Keyboard(), Hub() };

        public static List<Product> EmptyProductList()
            => new();

        // ── DTO helpers ───────────────────────────────────────────────────────

        public static ProductDTO MouseDTO() => new()
        {
            ProductId = 1,
            Name = "Wireless Mouse",
            Description = "Ergonomic wireless mouse with USB receiver",
            Price = 29.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Mouse",
            Stock = 100
        };

        public static CreateProductDTO ValidCreateDTO() => new()
        {
            Name = "Gaming Headset",
            Description = "Surround sound gaming headset",
            Price = 59.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Headset",
            Stock = 30
        };

        public static CreateProductDTO CreateDTOWithZeroPrice() => new()
        {
            Name = "Zero Price Product",
            Description = "Test",
            Price = 0m,
            ImageUrl = string.Empty,
            Stock = 10
        };

        public static CreateProductDTO CreateDTOWithNegativePrice() => new()
        {
            Name = "Negative Price",
            Description = "Test",
            Price = -5m,
            ImageUrl = string.Empty,
            Stock = 10
        };

        public static CreateProductDTO CreateDTOWithNegativeStock() => new()
        {
            Name = "Negative Stock",
            Description = "Test",
            Price = 9.99m,
            ImageUrl = string.Empty,
            Stock = -1
        };

        public static CreateProductDTO CreateDTOWithEmptyName() => new()
        {
            Name = "",
            Description = "Test",
            Price = 9.99m,
            ImageUrl = string.Empty,
            Stock = 10
        };

        public static CreateProductDTO CreateDTOWithWhitespaceName() => new()
        {
            Name = "   ",
            Description = "Test",
            Price = 9.99m,
            ImageUrl = string.Empty,
            Stock = 10
        };

        public static UpdateProductDTO ValidUpdateDTO() => new()
        {
            Name = "Updated Mouse",
            Description = "Updated description",
            Price = 34.99m,
            ImageUrl = "https://via.placeholder.com/300?text=Updated",
            Stock = 80
        };

        // ── Search DTO helpers ────────────────────────────────────────────────

        public static ProductSearchDTO DefaultSearch() => new()
        {
            Page = 1,
            PageSize = 10
        };

        public static ProductSearchDTO KeywordSearch(string keyword) => new()
        {
            Keyword = keyword,
            Page = 1,
            PageSize = 10
        };

        public static ProductSearchDTO PriceRangeSearch(
            decimal min, decimal max) => new()
            {
                MinPrice = min,
                MaxPrice = max,
                Page = 1,
                PageSize = 10
            };

        public static ProductSearchDTO InvalidPriceRangeSearch() => new()
        {
            MinPrice = 100m,
            MaxPrice = 10m,      // min > max — invalid
            Page = 1,
            PageSize = 10
        };

        public static ProductSearchDTO PaginatedSearch(int page, int pageSize) => new()
        {
            Page = page,
            PageSize = pageSize
        };
    }
}