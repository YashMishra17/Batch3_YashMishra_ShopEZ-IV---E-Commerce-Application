using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ.PaymentService.Controllers;
using ShopEZ.PaymentService.DTOs;
using ShopEZ.PaymentService.Exceptions;
using ShopEZ.PaymentService.Services.Interfaces;
using ShopEZ.PaymentService.Tests.Helpers;
using Xunit;

namespace ShopEZ.PaymentService.Tests.Controllers
{
    public class PaymentsControllerTests
    {
        private readonly Mock<IPaymentService> _serviceMock = new();
        private readonly PaymentsController _controller;

        public PaymentsControllerTests()
            => _controller = new PaymentsController(_serviceMock.Object);

        private static PaymentDTO BuildPaidDTO(
            int paymentId = 1,
            int orderId = PaymentTestData.ValidOrderId,
            int userId = PaymentTestData.ValidUserId,
            decimal amount = PaymentTestData.ValidAmount) => new()
            {
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = userId,
                Amount = amount,
                Status = "Paid",
                Method = "COD",
                TransactionId = "TXN-TEST12345678901",
                FailureReason = string.Empty,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                ProcessedAt = DateTime.UtcNow
            };

        private static PaymentDTO BuildFailedDTO(
            int paymentId = 2,
            string reason = "Card declined by issuer.") => new()
            {
                PaymentId = paymentId,
                OrderId = PaymentTestData.ValidOrderId,
                UserId = PaymentTestData.ValidUserId,
                Amount = PaymentTestData.ValidAmount,
                Status = "Failed",
                Method = "Card",
                TransactionId = string.Empty,
                FailureReason = reason,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                ProcessedAt = DateTime.UtcNow
            };

        private static PaymentDTO BuildRefundedDTO(int paymentId = 3) => new()
        {
            PaymentId = paymentId,
            OrderId = PaymentTestData.ValidOrderId,
            UserId = PaymentTestData.ValidUserId,
            Amount = PaymentTestData.ValidAmount,
            Status = "Refunded",
            Method = "Card",
            TransactionId = "TXN-REFUNDED",
            FailureReason = "Refunded: Customer requested cancellation",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ProcessedAt = DateTime.UtcNow
        };

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/payments — successful payment
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_ValidCodRequest_Returns201()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO());

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment());

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_ValidRequest_SuccessIsTrue()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO());

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment());

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var success = created.Value!.GetType()
                .GetProperty("success")?.GetValue(created.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task ProcessPayment_ValidRequest_DataNotNull()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO());

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment());

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var data = created.Value!.GetType()
                .GetProperty("data")?.GetValue(created.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task ProcessPayment_ValidRequest_LocationHeaderPointsToGetById()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO(paymentId: 99));

            var result = await _controller.ProcessPayment(PaymentTestData.CodPayment())
                         as CreatedAtActionResult;

            Assert.Equal(nameof(PaymentsController.GetPaymentById), result!.ActionName);
            Assert.Equal(99, result.RouteValues!["id"]);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/payments — failed payment
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_DeclinedCard_Returns402()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "Payment failed: Card declined by issuer.", 402));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.DeclinedCardPayment());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(402, obj.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_DeclinedCard_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "Payment failed: Card declined by issuer.", 402));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.DeclinedCardPayment());

            var obj = Assert.IsType<ObjectResult>(result);
            var success = obj.Value!.GetType()
                .GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task ProcessPayment_GatewayTimeout_Returns402()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "Payment failed: Payment gateway timeout. Please try again.",
                            402));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.TimeoutCardPayment());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(402, obj.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_DuplicateOrder_Returns409()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "A payment for Order 10 already exists.", 409));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, obj.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_InvalidInput_Returns400()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "OrderId must be a positive integer.", 400));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment(orderId: 0));

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_ServiceThrowsGenericException_Returns500()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.ProcessPayment(
                PaymentTestData.CodPayment());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task ProcessPayment_InvalidModelState_Returns400WithoutCallingService()
        {
            _controller.ModelState.AddModelError("Amount", "Amount must be > 0.");

            IActionResult result = await _controller.ProcessPayment(
                new ProcessPaymentDTO());

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(
                s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()), Times.Never);
        }

        [Fact]
        public async Task ProcessPayment_ValidRequest_CallsServiceOnce()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO());

            await _controller.ProcessPayment(PaymentTestData.CodPayment());

            _serviceMock.Verify(
                s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/payments/{id}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetPaymentById_ExistingPayment_Returns200()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(BuildPaidDTO(1));

            IActionResult result = await _controller.GetPaymentById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetPaymentById_ExistingPayment_DataNotNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(BuildPaidDTO(1));

            IActionResult result = await _controller.GetPaymentById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType().GetProperty("data")?.GetValue(ok.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task GetPaymentById_NonExistentPayment_Returns404()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(9999)).ReturnsAsync((PaymentDTO?)null);

            IActionResult result = await _controller.GetPaymentById(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task GetPaymentById_NonExistentPayment_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(9999)).ReturnsAsync((PaymentDTO?)null);

            IActionResult result = await _controller.GetPaymentById(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType()
                .GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task GetPaymentById_ServiceThrowsAppException400_Returns400()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(0))
                        .ThrowsAsync(new AppException("PaymentId must be positive.", 400));

            IActionResult result = await _controller.GetPaymentById(0);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task GetPaymentById_ServiceThrowsGenericException_Returns500()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(1))
                        .ThrowsAsync(new Exception("Timeout"));

            IActionResult result = await _controller.GetPaymentById(1);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/payments/order/{orderId}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetPaymentByOrder_ExistingOrder_Returns200()
        {
            _serviceMock.Setup(s => s.GetByOrderIdAsync(10)).ReturnsAsync(BuildPaidDTO());

            IActionResult result = await _controller.GetPaymentByOrder(10);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetPaymentByOrder_NonExistentOrder_Returns404()
        {
            _serviceMock.Setup(s => s.GetByOrderIdAsync(9999))
                        .ReturnsAsync((PaymentDTO?)null);

            IActionResult result = await _controller.GetPaymentByOrder(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task GetPaymentByOrder_ServiceThrowsAppException400_Returns400()
        {
            _serviceMock.Setup(s => s.GetByOrderIdAsync(0))
                        .ThrowsAsync(new AppException("OrderId must be positive.", 400));

            IActionResult result = await _controller.GetPaymentByOrder(0);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/payments/user/{userId}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetPaymentsByUser_ValidUser_Returns200WithPayments()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync(1))
                        .ReturnsAsync(new List<PaymentDTO>
                        {
                            BuildPaidDTO(1), BuildPaidDTO(2)
                        });

            IActionResult result = await _controller.GetPaymentsByUser(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetPaymentsByUser_UserWithNoPayments_Returns200WithEmptyList()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync(999))
                        .ReturnsAsync(new List<PaymentDTO>());

            IActionResult result = await _controller.GetPaymentsByUser(999);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType().GetProperty("data")?.GetValue(ok.Value)
                       as IEnumerable<PaymentDTO>;
            Assert.Empty(data!);
        }

        [Fact]
        public async Task GetPaymentsByUser_ServiceThrowsAppException400_Returns400()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync(0))
                        .ThrowsAsync(new AppException("UserId must be positive.", 400));

            IActionResult result = await _controller.GetPaymentsByUser(0);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task GetPaymentsByUser_ServiceThrowsGenericException_Returns500()
        {
            _serviceMock.Setup(s => s.GetByUserIdAsync(1))
                        .ThrowsAsync(new Exception("Storage error"));

            IActionResult result = await _controller.GetPaymentsByUser(1);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/payments/{id}/refund
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RefundPayment_PaidPayment_Returns200()
        {
            _serviceMock.Setup(s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync(BuildRefundedDTO());

            IActionResult result = await _controller.RefundPayment(
                1, PaymentTestData.ValidRefund());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task RefundPayment_PaidPayment_SuccessIsTrue()
        {
            _serviceMock.Setup(s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync(BuildRefundedDTO());

            IActionResult result = await _controller.RefundPayment(
                1, PaymentTestData.ValidRefund());

            var ok = Assert.IsType<OkObjectResult>(result);
            var success = ok.Value!.GetType()
                .GetProperty("success")?.GetValue(ok.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task RefundPayment_NonExistentPayment_Returns404()
        {
            _serviceMock.Setup(s => s.RefundAsync(9999, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync((PaymentDTO?)null);

            IActionResult result = await _controller.RefundPayment(
                9999, PaymentTestData.ValidRefund());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task RefundPayment_NonExistentPayment_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.RefundAsync(9999, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync((PaymentDTO?)null);

            IActionResult result = await _controller.RefundPayment(
                9999, PaymentTestData.ValidRefund());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType()
                .GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task RefundPayment_NotPaidStatus_Returns400()
        {
            _serviceMock.Setup(s => s.RefundAsync(2, It.IsAny<RefundPaymentDTO>()))
                        .ThrowsAsync(new AppException(
                            "Only 'Paid' payments can be refunded. Current status: Failed.",
                            400));

            IActionResult result = await _controller.RefundPayment(
                2, PaymentTestData.ValidRefund());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task RefundPayment_ServiceThrowsGenericException_Returns500()
        {
            _serviceMock.Setup(s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.RefundPayment(
                1, PaymentTestData.ValidRefund());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task RefundPayment_InvalidModelState_Returns400WithoutCallingService()
        {
            _controller.ModelState.AddModelError("Reason", "Reason is required.");

            IActionResult result = await _controller.RefundPayment(
                1, new RefundPaymentDTO());

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(
                s => s.RefundAsync(It.IsAny<int>(), It.IsAny<RefundPaymentDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task RefundPayment_CallsServiceOnce()
        {
            _serviceMock.Setup(s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync(BuildRefundedDTO());

            await _controller.RefundPayment(1, PaymentTestData.ValidRefund());

            _serviceMock.Verify(
                s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Response envelope
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ProcessPayment_SuccessResponse_HasSuccessAndDataFields()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ReturnsAsync(BuildPaidDTO());

            var result = await _controller.ProcessPayment(PaymentTestData.CodPayment())
                         as CreatedAtActionResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }

        [Fact]
        public async Task ProcessPayment_FailureResponse_HasSuccessAndMessageFields()
        {
            _serviceMock.Setup(s => s.ProcessPaymentAsync(It.IsAny<ProcessPaymentDTO>()))
                        .ThrowsAsync(new AppException("Card declined.", 402));

            var result = await _controller.ProcessPayment(
                PaymentTestData.DeclinedCardPayment()) as ObjectResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("message"));
        }

        [Fact]
        public async Task RefundPayment_SuccessResponse_HasSuccessAndDataFields()
        {
            _serviceMock.Setup(s => s.RefundAsync(1, It.IsAny<RefundPaymentDTO>()))
                        .ReturnsAsync(BuildRefundedDTO());

            var result = await _controller.RefundPayment(
                1, PaymentTestData.ValidRefund()) as OkObjectResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }
    }
}
