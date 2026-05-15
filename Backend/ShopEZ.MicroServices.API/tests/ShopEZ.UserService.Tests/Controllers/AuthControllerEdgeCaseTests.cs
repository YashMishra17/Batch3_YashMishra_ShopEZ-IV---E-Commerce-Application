using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ.UserService.Controllers;
using ShopEZ.UserService.DTOs;
using ShopEZ.UserService.Exceptions;
using ShopEZ.UserService.Services.Interfaces;
using Xunit;

namespace ShopEZ.UserService.Tests.Controllers
{
    public class AuthControllerEdgeCaseTests
    {
        private readonly Mock<IAuthService> _serviceMock = new();
        private readonly AuthController _controller;

        public AuthControllerEdgeCaseTests()
            => _controller = new AuthController(_serviceMock.Object);

        [Fact]
        public async Task Register_MultipleModelStateErrors_Returns400WithAllErrors()
        {
            var dto = new RegisterDTO();
            _controller.ModelState.AddModelError("Name", "Name is required.");
            _controller.ModelState.AddModelError("Email", "Email is required.");
            _controller.ModelState.AddModelError("Password", "Password is required.");

            var result = await _controller.Register(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
            Assert.NotNull(bad.Value);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(409)]
        [InlineData(500)]
        public async Task Register_AppExceptionVariousStatuses_ForwardsStatusCode(
            int statusCode)
        {
            var dto = new RegisterDTO
            {
                Name = "T",
                Email = "t@t.com",
                Password = "pass123",
                Role = "Customer"
            };

            _serviceMock.Setup(s => s.RegisterAsync(dto))
                        .ThrowsAsync(new AppException("Error.", statusCode));

            var result = await _controller.Register(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(statusCode, obj.StatusCode);
        }

        [Theory]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(500)]
        public async Task Login_AppExceptionVariousStatuses_ForwardsStatusCode(
            int statusCode)
        {
            var dto = new LoginDTO { Email = "t@t.com", Password = "pass" };

            _serviceMock.Setup(s => s.LoginAsync(dto))
                        .ThrowsAsync(new AppException("Error.", statusCode));

            var result = await _controller.Login(dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(statusCode, obj.StatusCode);
        }

        [Fact]
        public async Task Register_ModelStateInvalid_ServiceNeverCalled()
        {
            var dto = new RegisterDTO();
            _controller.ModelState.AddModelError("Name", "Required");

            await _controller.Register(dto);

            _serviceMock.Verify(s => s.RegisterAsync(
                It.IsAny<RegisterDTO>()), Times.Never);
        }

        [Fact]
        public async Task Login_ModelStateInvalid_ServiceNeverCalled()
        {
            var dto = new LoginDTO();
            _controller.ModelState.AddModelError("Email", "Required");

            await _controller.Login(dto);

            _serviceMock.Verify(s => s.LoginAsync(
                It.IsAny<LoginDTO>()), Times.Never);
        }
    }
}