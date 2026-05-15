using ShopEZ.PaymentService.DTOs;
using ShopEZ.PaymentService.Models;

namespace ShopEZ.PaymentService.Tests.Helpers
{
    public static class PaymentTestData
    {
        // ── Constants ──────────────────────────────────────────────────────────
        public const int ValidUserId = 1;
        public const int AnotherUserId = 2;
        public const int ValidOrderId = 10;
        public const int AnotherOrderId = 20;
        public const int InvalidOrderId = 0;
        public const int InvalidUserId = 0;

        public const decimal ValidAmount = 59.98m;
        public const decimal ZeroAmount = 0m;
        public const decimal NegativeAmount = -10m;

        public const string ApprovedCard = "4111111111111111"; // last4=1111 approved
        public const string DeclinedCard = "4111111111110000"; // last4=0000 always declined
        public const string TimeoutCard = "4111111111119999"; // last4=9999 timeout
        public const string EmptyCard = "";

        // ── ProcessPaymentDTOs ────────────────────────────────────────────────

        public static ProcessPaymentDTO ValidCardPayment(
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            decimal amount = ValidAmount) => new()
            {
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Method = "Card",
                CardNumber = ApprovedCard,
                CardExpiry = "12/26",
                CardCvv = "123"
            };

        public static ProcessPaymentDTO DeclinedCardPayment(
            int orderId = ValidOrderId,
            int userId = ValidUserId) => new()
            {
                OrderId = orderId,
                UserId = userId,
                Amount = ValidAmount,
                Method = "Card",
                CardNumber = DeclinedCard,
                CardExpiry = "12/26",
                CardCvv = "123"
            };

        public static ProcessPaymentDTO TimeoutCardPayment(
            int orderId = ValidOrderId,
            int userId = ValidUserId) => new()
            {
                OrderId = orderId,
                UserId = userId,
                Amount = ValidAmount,
                Method = "Card",
                CardNumber = TimeoutCard,
                CardExpiry = "12/26",
                CardCvv = "123"
            };

        public static ProcessPaymentDTO CodPayment(
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            decimal amount = ValidAmount) => new()
            {
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Method = "COD",
                CardNumber = EmptyCard
            };

        public static ProcessPaymentDTO WalletPayment(
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            decimal amount = ValidAmount) => new()
            {
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Method = "Wallet",
                CardNumber = EmptyCard
            };

        // ── Payment models ────────────────────────────────────────────────────

        public static Payment PendingPayment(
            int paymentId = 1,
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            decimal amount = ValidAmount) => new()
            {
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Status = "Pending",
                Method = "Card",
                TransactionId = string.Empty,
                FailureReason = string.Empty,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

        public static Payment PaidPayment(
            int paymentId = 1,
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            decimal amount = ValidAmount) => new()
            {
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Status = "Paid",
                Method = "Card",
                TransactionId = "TXN-ABC123DEF456GHI7",
                FailureReason = string.Empty,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                ProcessedAt = DateTime.UtcNow
            };

        public static Payment FailedPayment(
            int paymentId = 2,
            int orderId = ValidOrderId,
            int userId = ValidUserId,
            string reason = "Card declined by issuer.") => new()
            {
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = userId,
                Amount = ValidAmount,
                Status = "Failed",
                Method = "Card",
                TransactionId = string.Empty,
                FailureReason = reason,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                ProcessedAt = DateTime.UtcNow
            };

        public static Payment RefundedPayment(
            int paymentId = 3,
            int orderId = ValidOrderId) => new()
            {
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = ValidUserId,
                Amount = ValidAmount,
                Status = "Refunded",
                Method = "Card",
                TransactionId = "TXN-REFUNDED123",
                FailureReason = "Refunded: Customer requested cancellation",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ProcessedAt = DateTime.UtcNow
            };

        public static List<Payment> TwoPaymentsForUser(int userId = ValidUserId) => new()
        {
            PaidPayment(paymentId: 1, orderId: 10, userId: userId),
            PaidPayment(paymentId: 2, orderId: 11, userId: userId, amount: 79.99m)
        };

        public static RefundPaymentDTO ValidRefund() => new()
        {
            Reason = "Customer requested cancellation"
        };

        public static RefundPaymentDTO EmptyReasonRefund() => new()
        {
            Reason = string.Empty
        };
    }
}
