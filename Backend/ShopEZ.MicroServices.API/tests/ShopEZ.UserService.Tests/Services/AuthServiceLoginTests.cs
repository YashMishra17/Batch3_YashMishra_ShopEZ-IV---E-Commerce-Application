using Moq;
using ShopEZ.UserService.DTOs;
using ShopEZ.UserService.Exceptions;
using ShopEZ.UserService.Models;
using ShopEZ.UserService.Repositories.Interfaces;
using ShopEZ.UserService.Services;
using ShopEZ.UserService.Tests.Helpers;
using Xunit;

namespace ShopEZ.UserService.Tests.Services
{
    public class AuthServiceLoginTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly AuthService _service;

        public AuthServiceLoginTests()
        {
            _repoMock = new Mock<IUserRepository>();
            _service = new AuthService(
                _repoMock.Object,
                JwtTestHelper.BuildConfiguration());
        }

        // ─────────────────────────────────────────────────────────────────────
        // Happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_ValidAdminCredentials_ReturnsTokenAndAdminRole()
        {
            User admin = UserTestData.AdminUser();
            _repoMock.Setup(r => r.GetByEmailAsync("alice@shopez.com"))
                     .ReturnsAsync(admin);

            var dto = new LoginDTO { Email = "alice@shopez.com", Password = "Admin@123" };
            var result = await _service.LoginAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(admin.UserId, result.UserId);
            Assert.Equal("Admin", result.Role);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_ValidCustomerCredentials_ReturnsAuthResponse()
        {
            User customer = UserTestData.CustomerUser();
            _repoMock.Setup(r => r.GetByEmailAsync("bob@shopez.com"))
                     .ReturnsAsync(customer);

            var dto = new LoginDTO { Email = "bob@shopez.com", Password = "Customer@123" };
            var result = await _service.LoginAsync(dto);

            Assert.Equal("Customer", result.Role);
            Assert.Equal("bob@shopez.com", result.Email);
        }

        [Fact]
        public async Task LoginAsync_ValidLogin_TokenStartsWithEy()
        {
            User user = UserTestData.CustomerUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "Customer@123" };
            var result = await _service.LoginAsync(dto);

            Assert.True(result.Token.StartsWith("ey"));
        }

        [Fact]
        public async Task LoginAsync_ValidLogin_ExpiryIsAtLeast23HoursAhead()
        {
            User user = UserTestData.CustomerUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "Customer@123" };
            var result = await _service.LoginAsync(dto);

            Assert.True(result.ExpiresAt > DateTime.UtcNow.AddHours(23));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Null / empty input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(null!));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_EmptyEmail_ThrowsAppException400()
        {
            var dto = new LoginDTO { Email = "", Password = "pass123" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_WhitespaceEmail_ThrowsAppException400()
        {
            var dto = new LoginDTO { Email = "   ", Password = "pass123" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_EmptyPassword_ThrowsAppException400()
        {
            var dto = new LoginDTO { Email = "t@t.com", Password = "" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Password", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Invalid credentials
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsAppException401()
        {
            User user = UserTestData.AdminUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "WrongPassword!" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_NonExistentEmail_ThrowsAppException401()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            var dto = new LoginDTO { Email = "ghost@shopez.com", Password = "pass" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal(401, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_MessageIsGeneric()
        {
            User user = UserTestData.CustomerUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "WrongPassword!" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_NonExistentEmail_MessageIsGeneric()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            var dto = new LoginDTO { Email = "nobody@shopez.com", Password = "pass" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(dto));

            Assert.Equal("Invalid email or password.", ex.Message);
        }

        [Fact]
        public async Task LoginAsync_WrongAndMissing_SameStatusCodeAndMessage()
        {
            User user = UserTestData.CustomerUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
            _repoMock.Setup(r => r.GetByEmailAsync("nobody@shopez.com"))
                     .ReturnsAsync((User?)null);

            var exWrong = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(
                    new LoginDTO { Email = user.Email, Password = "bad" }));

            var exMissing = await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(
                    new LoginDTO { Email = "nobody@shopez.com", Password = "pass" }));

            Assert.Equal(exWrong.StatusCode, exMissing.StatusCode);
            Assert.Equal(exWrong.Message, exMissing.Message);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Repository verification
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginAsync_Success_CallsGetByEmailOnce()
        {
            User user = UserTestData.AdminUser();
            _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "Admin@123" };
            await _service.LoginAsync(dto);

            _repoMock.Verify(r => r.GetByEmailAsync(user.Email), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_Failure_NeverCallsCreateAsync()
        {
            _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<AppException>(
                () => _service.LoginAsync(
                    new LoginDTO { Email = "x@x.com", Password = "pass" }));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}