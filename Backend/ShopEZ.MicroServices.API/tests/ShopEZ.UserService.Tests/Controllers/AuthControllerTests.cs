using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ.UserService.Controllers;
using ShopEZ.UserService.DTOs;
using ShopEZ.UserService.Exceptions;
using ShopEZ.UserService.Services.Interfaces;
using Xunit;

namespace ShopEZ.UserService.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _serviceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _serviceMock = new Mock<IAuthService>();
            _controller = new AuthController(_serviceMock.Object);
        }

        // ── helper ────────────────────────────────────────────────────────────
        private static AuthResponseDTO BuildResponse(
            int userId = 1,
            string email = "test@shopez.com",
            string role = "Customer") => new()
            {
                UserId = userId,
                Name = "Test User",
                Email = email,
                Role = role,
                Token = "eyJhbGciOiJIUzI1NiJ9.test.sig",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/auth/register
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_ValidDto_Returns201WithSuccessTrue()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                Email = "j@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ReturnsAsync(BuildResponse());

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, obj.StatusCode);

            var success = obj.Value!.GetType()
                .GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task Register_ValidDto_ResponseContainsData()
        {
            var dto = new RegisterDTO
            {
                Name = "Jane",
                Email = "jane@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ReturnsAsync(BuildResponse(5, "jane@test.com"));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            var data = obj.Value!.GetType()
                .GetProperty("data")?.GetValue(obj.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            var dto = new RegisterDTO
            {
                Name = "Dupe",
                Email = "dupe@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new AppException(
                            "An account with this email already exists.", 409));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, obj.StatusCode);

            var success = obj.Value!.GetType()
                .GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task Register_ValidationError_Returns400()
        {
            var dto = new RegisterDTO
            {
                Name = "",
                Email = "x@x.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new AppException("Name is required.", 400));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task Register_UnexpectedException_Returns500()
        {
            var dto = new RegisterDTO
            {
                Name = "T",
                Email = "t@t.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new Exception("DB unreachable"));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidModelState_Returns400WithoutCallingService()
        {
            var dto = new RegisterDTO { Name = "", Email = "bad", Password = "x" };
            _controller.ModelState.AddModelError("Name", "Name is required.");
            _controller.ModelState.AddModelError("Email", "Invalid email.");

            var result = await _controller.Register(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);

            _serviceMock.Verify(s => s.RegisterAsync(It.IsAny<RegisterDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task Register_ValidDto_CallsServiceOnce()
        {
            var dto = new RegisterDTO
            {
                Name = "Once",
                Email = "once@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ReturnsAsync(BuildResponse());

            await _controller.Register(dto);

            _serviceMock.Verify(s => s.RegisterAsync(dto), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/auth/login — 200
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_ValidCredentials_Returns200WithSuccessTrue()
        {
            var dto = new LoginDTO
            {
                Email = "alice@shopez.com",
                Password = "Admin@123"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync(BuildResponse(1, "alice@shopez.com", "Admin"));

            var result = await _controller.Login(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);

            var success = ok.Value!.GetType()
                .GetProperty("success")?.GetValue(ok.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task Login_ValidCredentials_ResponseDataNotNull()
        {
            var dto = new LoginDTO
            {
                Email = "bob@shopez.com",
                Password = "Customer@123"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync(BuildResponse(2, "bob@shopez.com", "Customer"));

            var result = await _controller.Login(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType()
                .GetProperty("data")?.GetValue(ok.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task Login_WrongPassword_Returns401()
        {
            var dto = new LoginDTO
            {
                Email = "alice@shopez.com",
                Password = "WrongPass"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException(
                            "Invalid email or password.", 401));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);

            var success = obj.Value!.GetType()
                .GetProperty("success")?.GetValue(obj.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task Login_NonExistentEmail_Returns401()
        {
            var dto = new LoginDTO
            {
                Email = "ghost@shopez.com",
                Password = "AnyPass"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException(
                            "Invalid email or password.", 401));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
        }

        [Fact]
        public async Task Login_ServiceThrowsAppException400_Returns400()
        {
            var dto = new LoginDTO { Email = "", Password = "" };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException("Email is required.", 400));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task Login_UnexpectedException_Returns500()
        {
            var dto = new LoginDTO
            {
                Email = "test@test.com",
                Password = "pass"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new Exception("Unexpected DB error"));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task Login_InvalidModelState_Returns400WithoutCallingService()
        {
            var dto = new LoginDTO { Email = "not-an-email", Password = "" };
            _controller.ModelState.AddModelError("Email", "Invalid email.");
            _controller.ModelState.AddModelError("Password", "Password required.");

            var result = await _controller.Login(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);

            _serviceMock.Verify(s => s.LoginAsync(It.IsAny<LoginDTO>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_ValidDto_CallsServiceOnce()
        {
            var dto = new LoginDTO
            {
                Email = "once@test.com",
                Password = "pass123"
            };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync(BuildResponse());

            await _controller.Login(dto);

            _serviceMock.Verify(s => s.LoginAsync(dto), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // 401 vs 404 disambiguation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_AppException401_DoesNotReturn404()
        {
            var dto = new LoginDTO { Email = "x@x.com", Password = "wrong" };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException("Invalid email or password.", 401));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(401, obj.StatusCode);
            Assert.NotEqual(404, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Error message propagation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_AppException_ResponseContainsMessage()
        {
            const string errorMsg = "An account with this email already exists.";
            var dto = new RegisterDTO
            {
                Name = "X",
                Email = "x@x.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new AppException(errorMsg, 409));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            var message = obj.Value!.GetType()
                .GetProperty("message")?.GetValue(obj.Value);
            Assert.Equal(errorMsg, message);
        }

        [Fact]
        public async Task Login_AppException_ResponseContainsMessage()
        {
            const string errorMsg = "Invalid email or password.";
            var dto = new LoginDTO { Email = "x@x.com", Password = "bad" };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException(errorMsg, 401));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            var message = obj.Value!.GetType()
                .GetProperty("message")?.GetValue(obj.Value);
            Assert.Equal(errorMsg, message);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Response envelope shape
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Register_Success_ResponseHasSuccessAndDataFields()
        {
            var dto = new RegisterDTO
            {
                Name = "Env",
                Email = "env@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ReturnsAsync(BuildResponse());

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            var type = obj.Value!.GetType();

            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }

        [Fact]
        public async Task Login_Success_ResponseHasSuccessAndDataFields()
        {
            var dto = new LoginDTO { Email = "e@t.com", Password = "pass123" };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ReturnsAsync(BuildResponse());

            var result = await _controller.Login(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var type = ok.Value!.GetType();

            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }

        [Fact]
        public async Task Register_Failure_ResponseHasSuccessAndMessageFields()
        {
            var dto = new RegisterDTO
            {
                Name = "F",
                Email = "f@f.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new AppException("Failure.", 400));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            var type = obj.Value!.GetType();

            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("message"));
        }
    }
}
