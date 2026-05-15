using ShopEZ.PaymentService.DTOs;
using ShopEZ.PaymentService.Exceptions;
using ShopEZ.PaymentService.Models;
using ShopEZ.PaymentService.Repositories.Interfaces;
using ShopEZ.PaymentService.Services.Interfaces;

namespace ShopEZ.PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repo;

        public PaymentService(IPaymentRepository repo)
        {
            _repo = repo;
        }

        public async Task<PaymentDTO> ProcessPaymentAsync(ProcessPaymentDTO dto)
        {
            if (dto is null)
                throw new AppException("Payment data cannot be null.", 400);

            if (dto.OrderId <= 0)
                throw new AppException("OrderId must be a positive integer.", 400);

            if (dto.UserId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            if (dto.Amount <= 0)
                throw new AppException("Amount must be greater than zero.", 400);

            string[] validMethods = { "Card", "COD", "Wallet" };
            if (!validMethods.Contains(dto.Method, StringComparer.OrdinalIgnoreCase))
                throw new AppException(
                    $"Invalid payment method. Allowed: {string.Join(", ", validMethods)}.",
                    400);

            bool alreadyPaid = await _repo.ExistsForOrderAsync(dto.OrderId);
            if (alreadyPaid)
                throw new AppException(
                    $"A payment for Order {dto.OrderId} already exists.", 409);

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                UserId = dto.UserId,
                Amount = dto.Amount,
                Status = "Pending",
                Method = dto.Method,
                CreatedAt = DateTime.UtcNow
            };

            Payment created = await _repo.CreateAsync(payment);

            (bool success, string transactionId, string failureReason) =
                MockPaymentGateway.Charge(dto.CardNumber, dto.Amount, dto.Method);

            created.Status = success ? "Paid" : "Failed";
            created.TransactionId = transactionId;
            created.FailureReason = failureReason;
            created.ProcessedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(created);

            if (!success)
                throw new AppException($"Payment failed: {failureReason}", 402);

            return MapToDTO(created);
        }

        public async Task<PaymentDTO?> GetByIdAsync(int paymentId)
        {
            if (paymentId <= 0)
                throw new AppException("PaymentId must be a positive integer.", 400);

            Payment? payment = await _repo.GetByIdAsync(paymentId);
            return payment is null ? null : MapToDTO(payment);
        }

        public async Task<PaymentDTO?> GetByOrderIdAsync(int orderId)
        {
            if (orderId <= 0)
                throw new AppException("OrderId must be a positive integer.", 400);

            Payment? payment = await _repo.GetByOrderIdAsync(orderId);
            return payment is null ? null : MapToDTO(payment);
        }

        public async Task<IEnumerable<PaymentDTO>> GetByUserIdAsync(int userId)
        {
            if (userId <= 0)
                throw new AppException("UserId must be a positive integer.", 400);

            IEnumerable<Payment> payments = await _repo.GetByUserIdAsync(userId);
            return payments.Select(MapToDTO);
        }

        public async Task<PaymentDTO?> RefundAsync(int paymentId, RefundPaymentDTO dto)
        {
            if (paymentId <= 0)
                throw new AppException("PaymentId must be a positive integer.", 400);

            if (dto is null || string.IsNullOrWhiteSpace(dto.Reason))
                throw new AppException("Refund reason is required.", 400);

            Payment? payment = await _repo.GetByIdAsync(paymentId);
            if (payment is null) return null;

            if (payment.Status != "Paid")
                throw new AppException(
                    $"Only 'Paid' payments can be refunded. Current status: {payment.Status}.",
                    400);

            payment.Status = "Refunded";
            payment.FailureReason = $"Refunded: {dto.Reason.Trim()}";
            payment.ProcessedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(payment);
            return MapToDTO(payment);
        }

        private static PaymentDTO MapToDTO(Payment p) => new()
        {
            PaymentId = p.PaymentId,
            OrderId = p.OrderId,
            UserId = p.UserId,
            Amount = p.Amount,
            Status = p.Status,
            Method = p.Method,
            TransactionId = p.TransactionId,
            FailureReason = p.FailureReason,
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.ProcessedAt
        };
    }
}