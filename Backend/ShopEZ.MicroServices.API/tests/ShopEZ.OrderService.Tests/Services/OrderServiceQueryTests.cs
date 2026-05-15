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
    public class OrderServiceQueryTests
    {
        private readonly Mock<IOrderRepository> _repoMock = new();
        private readonly OrderServiceClass _service;

        public OrderServiceQueryTests()
            => _service = new OrderServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // GetAllOrdersAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllOrdersAsync_TwoOrders_ReturnsMappedDTOs()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(OrderTestData.TwoOrdersForUser());

            IEnumerable<OrderDTO> result = await _service.GetAllOrdersAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllOrdersAsync_EmptyTable_ReturnsEmptyEnumerable()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(OrderTestData.EmptyOrderList());

            IEnumerable<OrderDTO> result = await _service.GetAllOrdersAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllOrdersAsync_FieldsMappedCorrectly()
        {
            Order order = OrderTestData.BuildOrder(orderId: 1, userId: 1, totalAmount: 29.99m);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Order> { order });

            OrderDTO dto = (await _service.GetAllOrdersAsync()).First();

            Assert.Equal(order.OrderId, dto.OrderId);
            Assert.Equal(order.UserId, dto.UserId);
            Assert.Equal(order.TotalAmount, dto.TotalAmount);
            Assert.Equal(order.Status, dto.Status);
        }

        [Fact]
        public async Task GetAllOrdersAsync_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(OrderTestData.EmptyOrderList());

            await _service.GetAllOrdersAsync();

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllOrdersAsync_OrderItemsSubtotalsPopulated()
        {
            Order order = OrderTestData.BuildOrder(orderId: 1, totalAmount: 29.99m);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Order> { order });

            OrderDTO dto = (await _service.GetAllOrdersAsync()).First();

            Assert.All(dto.OrderItems,
                item => Assert.Equal(item.Price * item.Quantity, item.Subtotal));
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetOrdersByUserIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrdersByUserIdAsync_ValidUser_ReturnsHisOrders()
        {
            List<Order> orders = OrderTestData.TwoOrdersForUser(OrderTestData.ValidUserId);

            _repoMock.Setup(r => r.GetByUserIdAsync(OrderTestData.ValidUserId))
                     .ReturnsAsync(orders);

            IEnumerable<OrderDTO> result =
                await _service.GetOrdersByUserIdAsync(OrderTestData.ValidUserId);

            Assert.Equal(2, result.Count());
            Assert.All(result, o => Assert.Equal(OrderTestData.ValidUserId, o.UserId));
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_UserWithNoOrders_ReturnsEmpty()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(999))
                     .ReturnsAsync(OrderTestData.EmptyOrderList());

            IEnumerable<OrderDTO> result = await _service.GetOrdersByUserIdAsync(999);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetOrdersByUserIdAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_NegativeUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetOrdersByUserIdAsync(-1));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task GetOrdersByUserIdAsync_InvalidIds_AlwaysThrow400(int userId)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetOrdersByUserIdAsync(userId));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetOrdersByUserIdAsync_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(OrderTestData.ValidUserId))
                     .ReturnsAsync(OrderTestData.EmptyOrderList());

            await _service.GetOrdersByUserIdAsync(OrderTestData.ValidUserId);

            _repoMock.Verify(r => r.GetByUserIdAsync(OrderTestData.ValidUserId), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetOrderByIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrderByIdAsync_ExistingOrder_ReturnsMappedDTO()
        {
            Order order = OrderTestData.BuildOrder(orderId: 5);

            _repoMock.Setup(r => r.GetByIdAsync(5))
                     .ReturnsAsync(order);

            OrderDTO? result = await _service.GetOrderByIdAsync(5);

            Assert.NotNull(result);
            Assert.Equal(5, result!.OrderId);
        }

        [Fact]
        public async Task GetOrderByIdAsync_NonExistentId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9999))
                     .ReturnsAsync((Order?)null);

            OrderDTO? result = await _service.GetOrderByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ZeroId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetOrderByIdAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetOrderByIdAsync_NegativeId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetOrderByIdAsync(-5));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ValidId_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(OrderTestData.BuildOrder(orderId: 1));

            await _service.GetOrderByIdAsync(1);

            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetOrderByIdAsync_OrderItemsIncluded()
        {
            Order order = OrderTestData.BuildOrder(orderId: 7);

            _repoMock.Setup(r => r.GetByIdAsync(7))
                     .ReturnsAsync(order);

            OrderDTO? result = await _service.GetOrderByIdAsync(7);

            Assert.NotNull(result);
            Assert.NotEmpty(result!.OrderItems);
        }
    }
}