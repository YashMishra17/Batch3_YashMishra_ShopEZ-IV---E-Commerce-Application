using ShopEZ.PaymentService.Models;

namespace ShopEZ.PaymentService.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int paymentId);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment?> UpdateAsync(Payment payment);
        Task<bool> ExistsForOrderAsync(int orderId);
    }
}