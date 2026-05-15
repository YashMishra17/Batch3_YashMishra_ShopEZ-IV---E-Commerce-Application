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
    public class PaymentServiceQueryRefundTests
    {
        private readonly Mock<IPaymentRepository> _repoMock = new();
        private readonly PaymentServiceClass _service;

        public PaymentServiceQueryRefundTests()
            => _service = new PaymentServiceClass(_repoMock.Object);

        [Fact]
        public async Task GetByIdAsync_ExistingPayment_ReturnsMappedDTO()
        {
            Payment payment = PaymentTestData.PaidPayment(paymentId: 1);
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(payment);

            PaymentDTO? result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.PaymentId);
            Assert.Equal("Paid", result.Status);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentPayment_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9999)).ReturnsAsync((Payment?)null);

            PaymentDTO? result = await _service.GetByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ZeroId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.GetByIdAsync(0));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetByOrderIdAsync_ExistingOrder_ReturnsMappedDTO()
        {
            Payment payment = PaymentTestData.PaidPayment(paymentId: 2, orderId: 10);
            _repoMock.Setup(r => r.GetByOrderIdAsync(10)).ReturnsAsync(payment);

            PaymentDTO? result = await _service.GetByOrderIdAsync(10);

            Assert.NotNull(result);
            Assert.Equal(10, result!.OrderId);
        }

        [Fact]
        public async Task GetByUserIdAsync_UserWithPayments_ReturnsMappedDTOs()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(PaymentTestData.ValidUserId))
                     .ReturnsAsync(PaymentTestData.TwoPaymentsForUser());

            IEnumerable<PaymentDTO> result =
                await _service.GetByUserIdAsync(PaymentTestData.ValidUserId);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task RefundAsync_PaidPayment_ReturnsRefundedDTO()
        {
            Payment paid = PaymentTestData.PaidPayment(paymentId: 1);
            Payment refunded = PaymentTestData.RefundedPayment(paymentId: 1);

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(paid);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>())).ReturnsAsync(refunded);

            PaymentDTO? result = await _service.RefundAsync(1, PaymentTestData.ValidRefund());

            Assert.NotNull(result);
            Assert.Equal("Refunded", result!.Status);
        }

        [Fact]
        public async Task RefundAsync_NonExistentPayment_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9999)).ReturnsAsync((Payment?)null);

            PaymentDTO? result =
                await _service.RefundAsync(9999, PaymentTestData.ValidRefund());

            Assert.Null(result);
        }

        [Fact]
        public async Task RefundAsync_FailedPayment_ThrowsAppException400()
        {
            Payment failed = PaymentTestData.FailedPayment(paymentId: 10);
            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(failed);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(10, PaymentTestData.ValidRefund()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RefundAsync_PendingPayment_ThrowsAppException400()
        {
            Payment pending = PaymentTestData.PendingPayment(paymentId: 11);
            _repoMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(pending);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(11, PaymentTestData.ValidRefund()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RefundAsync_AlreadyRefundedPayment_ThrowsAppException400()
        {
            Payment refunded = PaymentTestData.RefundedPayment(paymentId: 12);
            _repoMock.Setup(r => r.GetByIdAsync(12)).ReturnsAsync(refunded);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(12, PaymentTestData.ValidRefund()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RefundAsync_ZeroPaymentId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(0, PaymentTestData.ValidRefund()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RefundAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(1, null!));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RefundAsync_EmptyReason_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RefundAsync(1, PaymentTestData.EmptyReasonRefund()));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}