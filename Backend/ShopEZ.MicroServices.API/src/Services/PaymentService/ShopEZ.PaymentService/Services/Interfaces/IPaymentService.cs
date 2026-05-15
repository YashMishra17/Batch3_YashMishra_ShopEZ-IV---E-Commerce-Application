using ShopEZ.PaymentService.DTOs;

namespace ShopEZ.PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDTO> ProcessPaymentAsync(ProcessPaymentDTO dto);
        Task<PaymentDTO?> GetByIdAsync(int paymentId);
        Task<PaymentDTO?> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<PaymentDTO>> GetByUserIdAsync(int userId);
        Task<PaymentDTO?> RefundAsync(int paymentId, RefundPaymentDTO dto);
    }
}