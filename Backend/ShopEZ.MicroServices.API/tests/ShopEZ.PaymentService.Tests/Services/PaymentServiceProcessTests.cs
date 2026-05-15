using Moq;
using ShopEZ.PaymentService.DTOs;
using ShopEZ.PaymentService.Exceptions;
using ShopEZ.PaymentService.Models;
using ShopEZ.PaymentService.Repositories.Interfaces;
using PaymentServiceClass = ShopEZ.PaymentService.Services.PaymentService;
using ShopEZ.PaymentService.Tests.Helpers;
using Xunit;

namespace ShopEZ.PaymentService.Tests.Services
{
    public class PaymentServiceProcessTests
    {
        private readonly Mock<IPaymentRepository> _repoMock = new();
        private readonly PaymentServiceClass _service;

        public PaymentServiceProcessTests()
            => _service = new PaymentServiceClass(_repoMock.Object);

        private void SetupNoExistingPayment(int orderId)
            => _repoMock.Setup(r => r.ExistsForOrderAsync(orderId))
                        .ReturnsAsync(false);

        private void SetupCreateReturns(Payment returned)
            => _repoMock.Setup(r => r.CreateAsync(It.IsAny<Payment>()))
                        .ReturnsAsync(returned);

        private void SetupUpdateReturns(Payment returned)
            => _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
                        .ReturnsAsync(returned);

        [Fact]
        public async Task ProcessPaymentAsync_CodPayment_ReturnsStatusPaid()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 1));
            SetupUpdateReturns(PaymentTestData.PaidPayment(paymentId: 1));

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.Equal("Paid", result.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_CodPayment_TransactionIdNotEmpty()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 2));
            SetupUpdateReturns(PaymentTestData.PaidPayment(paymentId: 2));

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
        }

        [Fact]
        public async Task ProcessPaymentAsync_CodPayment_ProcessedAtPopulated()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 3));
            SetupUpdateReturns(PaymentTestData.PaidPayment(paymentId: 3));

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.NotNull(result.ProcessedAt);
        }

        [Fact]
        public async Task ProcessPaymentAsync_CodPayment_FieldsMappedCorrectly()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment(
                orderId: PaymentTestData.ValidOrderId,
                userId: PaymentTestData.ValidUserId,
                amount: PaymentTestData.ValidAmount);

            SetupNoExistingPayment(dto.OrderId);

            var paidPayment = PaymentTestData.PaidPayment(
                paymentId: 5,
                orderId: PaymentTestData.ValidOrderId,
                userId: PaymentTestData.ValidUserId,
                amount: PaymentTestData.ValidAmount);

            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 5));
            SetupUpdateReturns(paidPayment);

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.Equal(PaymentTestData.ValidOrderId, result.OrderId);
            Assert.Equal(PaymentTestData.ValidUserId, result.UserId);
            Assert.Equal(PaymentTestData.ValidAmount, result.Amount);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WalletPayment_ReturnsStatusPaid()
        {
            ProcessPaymentDTO dto = PaymentTestData.WalletPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 10));
            SetupUpdateReturns(PaymentTestData.PaidPayment(paymentId: 10));

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.Equal("Paid", result.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WalletPayment_FailureReasonIsEmpty()
        {
            ProcessPaymentDTO dto = PaymentTestData.WalletPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 11));
            SetupUpdateReturns(PaymentTestData.PaidPayment(paymentId: 11));

            PaymentDTO result = await _service.ProcessPaymentAsync(dto);

            Assert.Equal(string.Empty, result.FailureReason);
        }

        [Fact]
        public async Task ProcessPaymentAsync_DeclinedCard_ThrowsAppException402()
        {
            ProcessPaymentDTO dto = PaymentTestData.DeclinedCardPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 20));
            SetupUpdateReturns(PaymentTestData.FailedPayment(paymentId: 20, reason: "Card declined by issuer."));

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(dto));

            Assert.Equal(402, ex.StatusCode);
        }

        [Fact]
        public async Task ProcessPaymentAsync_DeclinedCard_ExceptionMessageContainsDeclined()
        {
            ProcessPaymentDTO dto = PaymentTestData.DeclinedCardPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 21));
            SetupUpdateReturns(PaymentTestData.FailedPayment(paymentId: 21, reason: "Card declined by issuer."));

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(dto));

            Assert.Contains("declined", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessPaymentAsync_DeclinedCard_PaymentRecordSavedWithFailedStatus()
        {
            Payment? updatedCapture = null;
            ProcessPaymentDTO dto = PaymentTestData.DeclinedCardPayment();

            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 22));

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
                     .Callback<Payment>(p => updatedCapture = p)
                     .ReturnsAsync(PaymentTestData.FailedPayment(paymentId: 22));

            await Assert.ThrowsAsync<AppException>(() => _service.ProcessPaymentAsync(dto));

            Assert.NotNull(updatedCapture);
            Assert.Equal("Failed", updatedCapture!.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_DeclinedCard_FailureReasonPersisted()
        {
            Payment? updatedCapture = null;
            ProcessPaymentDTO dto = PaymentTestData.DeclinedCardPayment();

            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 23));

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
                     .Callback<Payment>(p => updatedCapture = p)
                     .ReturnsAsync(PaymentTestData.FailedPayment(paymentId: 23));

            await Assert.ThrowsAsync<AppException>(() => _service.ProcessPaymentAsync(dto));

            Assert.NotNull(updatedCapture);
            Assert.False(string.IsNullOrWhiteSpace(updatedCapture!.FailureReason));
        }

        [Fact]
        public async Task ProcessPaymentAsync_GatewayTimeout_ThrowsAppException402()
        {
            ProcessPaymentDTO dto = PaymentTestData.TimeoutCardPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 30));
            SetupUpdateReturns(PaymentTestData.FailedPayment(paymentId: 30, reason: "Payment gateway timeout. Please try again."));

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(dto));

            Assert.Equal(402, ex.StatusCode);
        }

        [Fact]
        public async Task ProcessPaymentAsync_GatewayTimeout_ExceptionMessageContainsTimeout()
        {
            ProcessPaymentDTO dto = PaymentTestData.TimeoutCardPayment();
            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 31));
            SetupUpdateReturns(PaymentTestData.FailedPayment(paymentId: 31, reason: "Payment gateway timeout. Please try again."));

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(dto));

            Assert.Contains("timeout", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ProcessPaymentAsync_GatewayTimeout_RecordSavedAsFailed()
        {
            Payment? captured = null;
            ProcessPaymentDTO dto = PaymentTestData.TimeoutCardPayment();

            SetupNoExistingPayment(dto.OrderId);
            SetupCreateReturns(PaymentTestData.PendingPayment(paymentId: 32));

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
                     .Callback<Payment>(p => captured = p)
                     .ReturnsAsync(PaymentTestData.FailedPayment(paymentId: 32));

            await Assert.ThrowsAsync<AppException>(() => _service.ProcessPaymentAsync(dto));

            Assert.NotNull(captured);
            Assert.Equal("Failed", captured!.Status);
        }

        [Fact]
        public async Task ProcessPaymentAsync_OrderAlreadyPaid_ThrowsAppException409()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment();

            _repoMock.Setup(r => r.ExistsForOrderAsync(dto.OrderId)).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(dto));

            Assert.Equal(409, ex.StatusCode);
        }

        [Fact]
        public async Task ProcessPaymentAsync_OrderAlreadyPaid_CreateNeverCalled()
        {
            ProcessPaymentDTO dto = PaymentTestData.CodPayment();

            _repoMock.Setup(r => r.ExistsForOrderAsync(dto.OrderId)).ReturnsAsync(true);

            await Assert.ThrowsAsync<AppException>(() => _service.ProcessPaymentAsync(dto));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Payment>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ProcessPaymentAsync(null!));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}