using ShopEZ.OrderService.DTOs;

namespace ShopEZ.OrderService.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByUserIdAsync(int userId);
        Task<OrderDTO?> GetOrderByIdAsync(int orderId);
        Task<OrderDTO> CreateOrderAsync(CreateOrderDTO dto);
        Task<OrderDTO?> UpdateOrderStatusAsync(int orderId, string status);
    }
}
