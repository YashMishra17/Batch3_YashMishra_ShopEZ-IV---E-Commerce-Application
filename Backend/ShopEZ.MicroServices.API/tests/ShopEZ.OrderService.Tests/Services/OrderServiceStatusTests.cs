using Moq;
using ShopEZ.OrderService.DTOs;
using ShopEZ.OrderService.Exceptions;
using ShopEZ.OrderService.Models;
using ShopEZ.OrderService.Repositories.Interfaces;
using OrderServiceClass = ShopEZ.OrderService.Services.OrderService;
using ShopEZ.OrderService.Tests.Helpers;
using Xunit;

namespace ShopEZ.OrderService.Tests.Services
{
    public class OrderServiceStatusTests
    {
        private readonly Mock<IOrderRepository> _repoMock = new();
        private readonly OrderServiceClass _service;

        public OrderServiceStatusTests()
            => _service = new OrderServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // UpdateOrderStatusAsync — valid statuses
        // ─────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("Pending")]
        [InlineData("Confirmed")]
        [InlineData("Paid")]
        [InlineData("Shipped")]
        [InlineData("Delivered")]
        [InlineData("Cancelled")]
        public async Task UpdateOrderStatusAsync_ValidStatus_ReturnsUpdatedDTO(string status)
        {
            Order updated = OrderTestData.BuildOrder(orderId: 1, status: status);

            _repoMock.Setup(r => r.UpdateStatusAsync(1, status))
                     .ReturnsAsync(updated);

            OrderDTO? result = await _service.UpdateOrderStatusAsync(1, status);

            Assert.NotNull(result);
            Assert.Equal(status, result!.Status);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_ValidStatus_CallsRepositoryOnce()
        {
            Order updated = OrderTestData.BuildOrder(orderId: 1, status: "Confirmed");

            _repoMock.Setup(r => r.UpdateStatusAsync(1, "Confirmed"))
                     .ReturnsAsync(updated);

            await _service.UpdateOrderStatusAsync(1, "Confirmed");

            _repoMock.Verify(r => r.UpdateStatusAsync(1, "Confirmed"), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_NonExistentOrder_ReturnsNull()
        {
            _repoMock.Setup(r => r.UpdateStatusAsync(9999, "Paid"))
                     .ReturnsAsync((Order?)null);

            OrderDTO? result = await _service.UpdateOrderStatusAsync(9999, "Paid");

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_StatusCaseInsensitive_Succeeds()
        {
            Order updated = OrderTestData.BuildOrder(orderId: 1, status: "Confirmed");

            _repoMock.Setup(r => r.UpdateStatusAsync(1, It.IsAny<string>()))
                     .ReturnsAsync(updated);

            OrderDTO? result = await _service.UpdateOrderStatusAsync(1, "confirmed");

            Assert.NotNull(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateOrderStatusAsync — invalid status
        // ─────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("Processing")]
        [InlineData("Unknown")]
        [InlineData("Refunded")]
        [InlineData("Awaiting")]
        [InlineData("Done")]
        public async Task UpdateOrderStatusAsync_InvalidStatus_ThrowsAppException400(string status)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(1, status));

            Assert.Equal(400, ex.StatusCode);

            _repoMock.Verify(r => r.UpdateStatusAsync(
                It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_EmptyStatus_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(1, ""));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_WhitespaceStatus_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(1, "   "));

            Assert.Equal(400, ex.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateOrderStatusAsync — invalid orderId
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateOrderStatusAsync_ZeroOrderId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(0, "Confirmed"));

            Assert.Equal(400, ex.StatusCode);

            _repoMock.Verify(r => r.UpdateStatusAsync(
                It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatusAsync_NegativeOrderId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(-1, "Paid"));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task UpdateOrderStatusAsync_InvalidOrderIds_AlwaysThrow400(int orderId)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateOrderStatusAsync(orderId, "Confirmed"));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}