using ShopEZ.OrderService.DTOs;
using ShopEZ.OrderService.Exceptions;
using ShopEZ.OrderService.Models;
using ShopEZ.OrderService.Repositories.Interfaces;
using ShopEZ.OrderService.Services.Interfaces;

namespace ShopEZ.OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;

        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            IEnumerable<Order> orders = await _repo.GetAllAsync();
            return orders.Select(MapToDTO);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            IEnumerable<Order> orders = await _repo.GetByUserIdAsync(userId);
            return orders.Select(MapToDTO);
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            if (orderId <= 0)
                throw new AppException("Order ID must be a positive integer.", 400);

            Order? order = await _repo.GetByIdAsync(orderId);
            return order is null ? null : MapToDTO(order);
        }

        public async Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto)
        {
            if (dto is null)
                throw new AppException("Order data cannot be null.", 400);

            if (dto.UserId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            if (dto.CartItems is null || !dto.CartItems.Any())
                throw new AppException(
                    "Cart must contain at least one item.", 400);

            foreach (CartItemDTO ci in dto.CartItems)
            {
                if (ci.Quantity <= 0)
                    throw new AppException(
                        $"Quantity for ProductId {ci.ProductId} must be greater than zero.",
                        400);

                if (ci.Price <= 0)
                    throw new AppException(
                        $"Price for ProductId {ci.ProductId} must be greater than zero.",
                        400);
            }

            List<OrderItem> orderItems = dto.CartItems.Select(ci => new OrderItem
            {
                ProductId = ci.ProductId,
                ProductName = ci.ProductName,
                Quantity = ci.Quantity,
                Price = ci.Price
            }).ToList();

            decimal totalAmount = orderItems.Sum(oi => oi.Price * oi.Quantity);

            var order = new Order
            {
                UserId = dto.UserId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = orderItems
            };

            Order created = await _repo.CreateAsync(order);
            return MapToDTO(created);
        }

        public async Task<OrderDTO?> UpdateOrderStatusAsync(int orderId, string status)
        {
            if (orderId <= 0)
                throw new AppException("Order ID must be a positive integer.", 400);

            if (string.IsNullOrWhiteSpace(status))
                throw new AppException("Status cannot be empty.", 400);

            string[] valid =
            {
                "Pending", "Confirmed", "Paid",
                "Shipped", "Delivered", "Cancelled"
            };

            if (!valid.Contains(status, StringComparer.OrdinalIgnoreCase))
                throw new AppException(
                    $"Invalid status. Allowed values: {string.Join(", ", valid)}.",
                    400);

            Order? updated = await _repo.UpdateStatusAsync(orderId, status);
            return updated is null ? null : MapToDTO(updated);
        }

        private static OrderDTO MapToDTO(Order o) => new()
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            UserName = string.Empty,
            OrderDate = o.OrderDate,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDTO
            {
                OrderItemId = oi.OrderItemId,
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                Price = oi.Price,
                Subtotal = oi.Price * oi.Quantity
            }).ToList()
        };
    }
}