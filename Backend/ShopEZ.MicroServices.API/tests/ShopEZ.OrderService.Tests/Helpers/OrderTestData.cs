using ShopEZ.OrderService.DTOs;
using ShopEZ.OrderService.Models;

namespace ShopEZ.OrderService.Tests.Helpers
{
    
    /// Canonical test objects reused across every test class.
    /// Every method returns a fresh instance — no shared mutable state.
    public static class OrderTestData
    {
        // ── User IDs ──────────────────────────────────────────────────────────
        public const int ValidUserId = 1;
        public const int AnotherUserId = 2;
        public const int InvalidUserId = 0;

        // ── Product IDs ───────────────────────────────────────────────────────
        public const int MouseProductId = 1;
        public const int KeyboardProductId = 2;
        public const int HubProductId = 3;

        // ── CartItemDTOs ──────────────────────────────────────────────────────

        public static CartItemDTO MouseCartItem(int quantity = 1) => new()
        {
            ProductId = MouseProductId,
            ProductName = "Wireless Mouse",
            Price = 29.99m,
            Quantity = quantity
        };

        public static CartItemDTO KeyboardCartItem(int quantity = 1) => new()
        {
            ProductId = KeyboardProductId,
            ProductName = "Mechanical Keyboard",
            Price = 79.99m,
            Quantity = quantity
        };

        public static CartItemDTO HubCartItem(int quantity = 2) => new()
        {
            ProductId = HubProductId,
            ProductName = "USB-C Hub",
            Price = 49.99m,
            Quantity = quantity
        };

        // ── CreateOrderDTOs ───────────────────────────────────────────────────

        public static CreateOrderDTO SingleItemOrder(
            int userId = ValidUserId,
            int quantity = 1) => new()
            {
                UserId = userId,
                CartItems = new List<CartItemDTO> { MouseCartItem(quantity) }
            };

        public static CreateOrderDTO MultiItemOrder(int userId = ValidUserId) => new()
        {
            UserId = userId,
            CartItems = new List<CartItemDTO>
            {
                MouseCartItem(1),
                KeyboardCartItem(1),
                HubCartItem(2)
            }
        };

        public static CreateOrderDTO EmptyCartOrder(int userId = ValidUserId) => new()
        {
            UserId = userId,
            CartItems = new List<CartItemDTO>()
        };

        public static CreateOrderDTO NullCartOrder(int userId = ValidUserId) => new()
        {
            UserId = userId,
            CartItems = null!
        };

        // ── Order models ──────────────────────────────────────────────────────

        public static Order BuildOrder(
            int orderId = 1,
            int userId = ValidUserId,
            decimal totalAmount = 29.99m,
            string status = "Pending") => new()
            {
                OrderId = orderId,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = status,
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderItemId = 1,
                    OrderId     = orderId,
                    ProductId   = MouseProductId,
                    ProductName = "Wireless Mouse",
                    Quantity    = 1,
                    Price       = totalAmount
                }
            }
            };

        public static Order BuildMultiItemOrder(
            int orderId = 2,
            int userId = ValidUserId) => new()
            {
                OrderId = orderId,
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 209.96m,   // 29.99 + 79.99 + 49.99×2
                Status = "Pending",
                OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    OrderItemId = 1, OrderId = orderId,
                    ProductId = MouseProductId,    ProductName = "Wireless Mouse",
                    Quantity = 1, Price = 29.99m
                },
                new OrderItem
                {
                    OrderItemId = 2, OrderId = orderId,
                    ProductId = KeyboardProductId, ProductName = "Mechanical Keyboard",
                    Quantity = 1, Price = 79.99m
                },
                new OrderItem
                {
                    OrderItemId = 3, OrderId = orderId,
                    ProductId = HubProductId,      ProductName = "USB-C Hub",
                    Quantity = 2, Price = 49.99m
                }
            }
            };

        public static List<Order> TwoOrdersForUser(int userId = ValidUserId) => new()
        {
            BuildOrder(orderId: 1, userId: userId, totalAmount: 29.99m),
            BuildOrder(orderId: 2, userId: userId, totalAmount: 79.99m, status: "Confirmed")
        };

        public static List<Order> EmptyOrderList() => new();
    }
}
