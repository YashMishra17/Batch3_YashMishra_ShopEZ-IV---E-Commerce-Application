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
    public class OrderServiceCreateTests
    {
        private readonly Mock<IOrderRepository> _repoMock = new();
        private readonly OrderServiceClass _service;

        public OrderServiceCreateTests()
            => _service = new OrderServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // CreateOrderAsync — success
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateOrderAsync_SingleItem_ReturnsOrderDTOWithCorrectTotal()
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder();
            Order saved = OrderTestData.BuildOrder(orderId: 1);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(1, result.OrderId);
            Assert.Equal(saved.TotalAmount, result.TotalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_SingleItem_OrderItemsMapped()
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder();
            Order saved = OrderTestData.BuildOrder(orderId: 5);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.Single(result.OrderItems);
            Assert.Equal(OrderTestData.MouseProductId, result.OrderItems[0].ProductId);
        }

        [Fact]
        public async Task CreateOrderAsync_MultiItem_TotalIsCorrect()
        {
            CreateOrderDTO dto = OrderTestData.MultiItemOrder();
            Order saved = OrderTestData.BuildMultiItemOrder(orderId: 10);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.Equal(209.96m, result.TotalAmount);
            Assert.Equal(3, result.OrderItems.Count);
        }

        [Fact]
        public async Task CreateOrderAsync_MultiItem_SubtotalsCorrect()
        {
            CreateOrderDTO dto = OrderTestData.MultiItemOrder();
            Order saved = OrderTestData.BuildMultiItemOrder(orderId: 11);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            foreach (var item in result.OrderItems)
                Assert.Equal(item.Price * item.Quantity, item.Subtotal);
        }

        [Fact]
        public async Task CreateOrderAsync_StatusDefaultsToPending()
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder();
            Order saved = OrderTestData.BuildOrder(orderId: 3, status: "Pending");

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.Equal("Pending", result.Status);
        }

        [Fact]
        public async Task CreateOrderAsync_UserIdPreservedInOrder()
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder(userId: OrderTestData.AnotherUserId);
            Order saved = OrderTestData.BuildOrder(orderId: 4, userId: OrderTestData.AnotherUserId);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.Equal(OrderTestData.AnotherUserId, result.UserId);
        }

        [Fact]
        public async Task CreateOrderAsync_OrderDateIsSetToUtcNow()
        {
            DateTime before = DateTime.UtcNow.AddSeconds(-1);
            Order saved = OrderTestData.BuildOrder(orderId: 6);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(OrderTestData.SingleItemOrder());

            Assert.True(result.OrderDate >= before);
        }

        [Fact]
        public async Task CreateOrderAsync_TotalCalculatedBeforeRepoCall()
        {
            Order? captured = null;
            Order saved = OrderTestData.BuildOrder(orderId: 7);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .Callback<Order>(o => captured = o)
                     .ReturnsAsync(saved);

            await _service.CreateOrderAsync(OrderTestData.SingleItemOrder(quantity: 2));

            Assert.NotNull(captured);
            Assert.Equal(59.98m, captured!.TotalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_ItemNamesSnapshotted()
        {
            Order? captured = null;
            Order saved = OrderTestData.BuildOrder(orderId: 8);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .Callback<Order>(o => captured = o)
                     .ReturnsAsync(saved);

            await _service.CreateOrderAsync(OrderTestData.SingleItemOrder());

            Assert.NotNull(captured);
            Assert.Equal("Wireless Mouse", captured!.OrderItems.First().ProductName);
        }

        [Fact]
        public async Task CreateOrderAsync_CallsRepositoryCreateOnce()
        {
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(OrderTestData.BuildOrder(orderId: 9));

            await _service.CreateOrderAsync(OrderTestData.SingleItemOrder());

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public async Task CreateOrderAsync_DuplicateProductIds_EachLineItemPreserved()
        {
            var dto = new CreateOrderDTO
            {
                UserId = OrderTestData.ValidUserId,
                CartItems = new List<CartItemDTO>
                {
                    OrderTestData.MouseCartItem(1),
                    OrderTestData.MouseCartItem(2)
                }
            };

            Order saved = new Order
            {
                OrderId = 12,
                UserId = OrderTestData.ValidUserId,
                TotalAmount = 89.97m,
                Status = "Pending",
                OrderDate = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new() { OrderItemId = 1, ProductId = 1, ProductName = "Wireless Mouse", Quantity = 1, Price = 29.99m },
                    new() { OrderItemId = 2, ProductId = 1, ProductName = "Wireless Mouse", Quantity = 2, Price = 29.99m }
                }
            };

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Order>()))
                     .ReturnsAsync(saved);

            OrderDTO result = await _service.CreateOrderAsync(dto);

            Assert.Equal(2, result.OrderItems.Count);
            Assert.Equal(89.97m, result.TotalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyCartItems_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(OrderTestData.EmptyCartOrder()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyCartItems_MessageMentionsCart()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(OrderTestData.EmptyCartOrder()));

            Assert.Contains("cart", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateOrderAsync_EmptyCartItems_RepositoryNeverCalled()
        {
            await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(OrderTestData.EmptyCartOrder()));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_NullCartItems_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(OrderTestData.NullCartOrder()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_NullCartItems_RepositoryNeverCalled()
        {
            await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(OrderTestData.NullCartOrder()));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(null!));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_NullDto_RepositoryNeverCalled()
        {
            await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(null!));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public async Task CreateOrderAsync_InvalidUserIds_AlwaysThrow400(int userId)
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder(userId: userId);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_InvalidUserId_RepositoryNeverCalled()
        {
            CreateOrderDTO dto = OrderTestData.SingleItemOrder(userId: 0);

            await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrderAsync_ZeroQuantity_ThrowsAppException400()
        {
            var dto = new CreateOrderDTO
            {
                UserId = OrderTestData.ValidUserId,
                CartItems = new List<CartItemDTO>
                {
                    new() { ProductId = 1, ProductName = "Mouse", Price = 29.99m, Quantity = 0 }
                }
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_NegativeQuantity_ThrowsAppException400()
        {
            var dto = new CreateOrderDTO
            {
                UserId = OrderTestData.ValidUserId,
                CartItems = new List<CartItemDTO>
                {
                    new() { ProductId = 1, ProductName = "Mouse", Price = 29.99m, Quantity = -5 }
                }
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_ZeroPrice_ThrowsAppException400()
        {
            var dto = new CreateOrderDTO
            {
                UserId = OrderTestData.ValidUserId,
                CartItems = new List<CartItemDTO>
                {
                    new() { ProductId = 1, ProductName = "Mouse", Price = 0m, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateOrderAsync_NegativePrice_ThrowsAppException400()
        {
            var dto = new CreateOrderDTO
            {
                UserId = OrderTestData.ValidUserId,
                CartItems = new List<CartItemDTO>
                {
                    new() { ProductId = 1, ProductName = "Mouse", Price = -10m, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.CreateOrderAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}