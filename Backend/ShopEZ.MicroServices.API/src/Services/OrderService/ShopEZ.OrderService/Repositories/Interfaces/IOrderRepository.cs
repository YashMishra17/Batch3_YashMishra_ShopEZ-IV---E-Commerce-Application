using ShopEZ.OrderService.Models;

namespace ShopEZ.OrderService.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order?> GetByIdAsync(int orderId);
        Task<Order> CreateAsync(Order order);
        Task<Order?> UpdateStatusAsync(int orderId, string status);
        Task<bool> ExistsAsync(int orderId);
    }
}