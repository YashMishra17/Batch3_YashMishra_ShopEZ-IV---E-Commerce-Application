using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ.OrderService.Controllers;
using ShopEZ.OrderService.DTOs;
using ShopEZ.OrderService.Exceptions;
using ShopEZ.OrderService.Services.Interfaces;
using ShopEZ.OrderService.Tests.Helpers;
using Xunit;

namespace ShopEZ.OrderService.Tests.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _serviceMock = new();
        private readonly OrdersController _controller;

        public OrdersControllerTests()
            => _controller = new OrdersController(_serviceMock.Object);

        // ── Helper ─────────────────────────────────────────────────────────────
        private static OrderDTO BuildDTO(
            int orderId = 1,
            int userId = OrderTestData.ValidUserId,
            decimal total = 29.99m,
            string status = "Pending") => new()
            {
                OrderId = orderId,
                UserId = userId,
                UserName = string.Empty,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                Status = status,
                OrderItems = new List<OrderItemDTO>
            {
                new()
                {
                    OrderItemId = 1, ProductId = 1, ProductName = "Wireless Mouse",
                    Quantity = 1, Price = total, Subtotal = total
                }
            }
            };

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/orders
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllOrders_TwoOrders_Returns200WithSuccessTrue()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllOrdersAsync())
                        .ReturnsAsync(new List<OrderDTO> { BuildDTO(1), BuildDTO(2) });

            // Act
            IActionResult result = await _controller.GetAllOrders();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
            var success = ok.Value!.GetType().GetProperty("success")?.GetValue(ok.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task GetAllOrders_EmptyTable_Returns200WithEmptyData()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllOrdersAsync())
                        .ReturnsAsync(new List<OrderDTO>());

            // Act
            IActionResult result = await _controller.GetAllOrders();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType().GetProperty("data")?.GetValue(ok.Value)
                       as IEnumerable<OrderDTO>;
            Assert.Empty(data!);
        }

        [Fact]
        public async Task GetAllOrders_ServiceThrowsGenericException_Returns500()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllOrdersAsync())
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            IActionResult result = await _controller.GetAllOrders();

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetAllOrders_CallsServiceOnce()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllOrdersAsync())
                        .ReturnsAsync(new List<OrderDTO>());

            // Act
            await _controller.GetAllOrders();

            // Assert
            _serviceMock.Verify(s => s.GetAllOrdersAsync(), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/orders/{id}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrderById_ExistingId_Returns200()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(BuildDTO(1));

            // Act
            IActionResult result = await _controller.GetOrderById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_ExistingId_DataNotNull()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(BuildDTO(1));

            // Act
            IActionResult result = await _controller.GetOrderById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType().GetProperty("data")?.GetValue(ok.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task GetOrderById_NonExistentId_Returns404()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(9999))
                        .ReturnsAsync((OrderDTO?)null);

            // Act
            IActionResult result = await _controller.GetOrderById(9999);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_NonExistentId_SuccessIsFalse()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(9999))
                        .ReturnsAsync((OrderDTO?)null);

            // Act
            IActionResult result = await _controller.GetOrderById(9999);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType().GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task GetOrderById_ServiceThrowsAppException400_Returns400()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(0))
                        .ThrowsAsync(new AppException("Order ID must be positive.", 400));

            // Act
            IActionResult result = await _controller.GetOrderById(0);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_ServiceThrowsGenericException_Returns500()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(1))
                        .ThrowsAsync(new Exception("Timeout"));

            // Act
            IActionResult result = await _controller.GetOrderById(1);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/orders/user/{userId}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetOrdersByUser_ValidUser_Returns200WithOrders()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrdersByUserIdAsync(OrderTestData.ValidUserId))
                        .ReturnsAsync(new List<OrderDTO> { BuildDTO(1), BuildDTO(2) });

            // Act
            IActionResult result = await _controller.GetOrdersByUser(OrderTestData.ValidUserId);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetOrdersByUser_UserWithNoOrders_Returns200WithEmptyList()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrdersByUserIdAsync(999))
                        .ReturnsAsync(new List<OrderDTO>());

            // Act
            IActionResult result = await _controller.GetOrdersByUser(999);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType().GetProperty("data")?.GetValue(ok.Value)
                       as IEnumerable<OrderDTO>;
            Assert.Empty(data!);
        }

        [Fact]
        public async Task GetOrdersByUser_ServiceThrowsAppException400_Returns400()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrdersByUserIdAsync(0))
                        .ThrowsAsync(new AppException("UserId must be positive.", 400));

            // Act
            IActionResult result = await _controller.GetOrdersByUser(0);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/orders
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateOrder_ValidRequest_Returns201()
        {
            // Arrange
            CreateOrderDTO dto = OrderTestData.SingleItemOrder();
            OrderDTO created = BuildDTO(orderId: 10);

            _serviceMock.Setup(s => s.CreateOrderAsync(dto)).ReturnsAsync(created);

            // Act
            IActionResult result = await _controller.CreateOrder(dto);

            // Assert
            var obj = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, obj.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_SuccessIsTrue()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ReturnsAsync(BuildDTO());

            // Act
            IActionResult result = await _controller.CreateOrder(
                OrderTestData.SingleItemOrder());

            // Assert
            var obj = Assert.IsType<CreatedAtActionResult>(result);
            var success = obj.Value!.GetType().GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_LocationHeaderPointsToGetById()
        {
            // Arrange
            OrderDTO created = BuildDTO(orderId: 55);

            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ReturnsAsync(created);

            // Act
            var result = await _controller.CreateOrder(
                OrderTestData.SingleItemOrder()) as CreatedAtActionResult;

            // Assert
            Assert.Equal(nameof(OrdersController.GetOrderById), result!.ActionName);
            Assert.Equal(55, result.RouteValues!["id"]);
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_Returns400()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new AppException("Cart must contain at least one item.", 400));

            // Act
            IActionResult result = await _controller.CreateOrder(
                OrderTestData.EmptyCartOrder());

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_SuccessIsFalse()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new AppException("Cart must contain at least one item.", 400));

            // Act
            IActionResult result = await _controller.CreateOrder(OrderTestData.EmptyCartOrder());

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            var success = obj.Value!.GetType().GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task CreateOrder_EmptyCart_MessageMentionsCart()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new AppException("Cart must contain at least one item.", 400));

            // Act
            IActionResult result = await _controller.CreateOrder(OrderTestData.EmptyCartOrder());

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            var message = obj.Value!.GetType().GetProperty("message")?.GetValue(obj.Value)?.ToString();
            Assert.Contains("cart", message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateOrder_InvalidUserId_Returns400()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new AppException("UserId must be a positive integer.", 400));

            // Act
            IActionResult result = await _controller.CreateOrder(
                OrderTestData.SingleItemOrder(userId: 0));

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_ServiceThrowsGenericException_Returns500()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new Exception("DB connection lost"));

            // Act
            IActionResult result = await _controller.CreateOrder(
                OrderTestData.SingleItemOrder());

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task CreateOrder_InvalidModelState_Returns400WithoutCallingService()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserId", "UserId is required.");

            // Act
            IActionResult result = await _controller.CreateOrder(new CreateOrderDTO());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(
                s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()), Times.Never);
        }

        [Fact]
        public async Task CreateOrder_ValidRequest_CallsServiceOnce()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ReturnsAsync(BuildDTO());

            // Act
            await _controller.CreateOrder(OrderTestData.SingleItemOrder());

            // Assert
            _serviceMock.Verify(s => s.CreateOrderAsync(
                It.IsAny<CreateOrderDTO>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PATCH /api/orders/{id}/status
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateOrderStatus_ValidStatus_Returns200()
        {
            // Arrange
            var dto = new UpdateOrderStatusDTO { Status = "Confirmed" };
            var updated = BuildDTO(orderId: 1, status: "Confirmed");

            _serviceMock.Setup(s => s.UpdateOrderStatusAsync(1, "Confirmed"))
                        .ReturnsAsync(updated);

            // Act
            IActionResult result = await _controller.UpdateOrderStatus(1, dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_NonExistentOrder_Returns404()
        {
            // Arrange
            var dto = new UpdateOrderStatusDTO { Status = "Paid" };

            _serviceMock.Setup(s => s.UpdateOrderStatusAsync(9999, "Paid"))
                        .ReturnsAsync((OrderDTO?)null);

            // Act
            IActionResult result = await _controller.UpdateOrderStatus(9999, dto);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_InvalidStatus_Returns400()
        {
            // Arrange
            var dto = new UpdateOrderStatusDTO { Status = "Blah" };

            _serviceMock.Setup(s => s.UpdateOrderStatusAsync(1, "Blah"))
                        .ThrowsAsync(new AppException("Invalid status.", 400));

            // Act
            IActionResult result = await _controller.UpdateOrderStatus(1, dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_InvalidModelState_Returns400WithoutCallingService()
        {
            // Arrange
            _controller.ModelState.AddModelError("Status", "Required.");

            // Act
            IActionResult result = await _controller.UpdateOrderStatus(
                1, new UpdateOrderStatusDTO());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(
                s => s.UpdateOrderStatusAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateOrderStatus_ServiceThrowsGenericException_Returns500()
        {
            // Arrange
            var dto = new UpdateOrderStatusDTO { Status = "Shipped" };

            _serviceMock.Setup(s => s.UpdateOrderStatusAsync(1, "Shipped"))
                        .ThrowsAsync(new Exception("DB error"));

            // Act
            IActionResult result = await _controller.UpdateOrderStatus(1, dto);

            // Assert
            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Response envelope
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateOrder_SuccessResponse_HasSuccessAndDataFields()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ReturnsAsync(BuildDTO());

            // Act
            var result = await _controller.CreateOrder(
                OrderTestData.SingleItemOrder()) as CreatedAtActionResult;

            // Assert
            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }

        [Fact]
        public async Task CreateOrder_FailureResponse_HasSuccessAndMessageFields()
        {
            // Arrange
            _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<CreateOrderDTO>()))
                        .ThrowsAsync(new AppException("Cart must contain at least one item.", 400));

            // Act
            var result = await _controller.CreateOrder(
                OrderTestData.EmptyCartOrder()) as ObjectResult;

            // Assert
            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("message"));
        }

        [Fact]
        public async Task GetOrderById_SuccessResponse_HasSuccessAndDataFields()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(BuildDTO(1));

            // Act
            var result = await _controller.GetOrderById(1) as OkObjectResult;

            // Assert
            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }
    }
}