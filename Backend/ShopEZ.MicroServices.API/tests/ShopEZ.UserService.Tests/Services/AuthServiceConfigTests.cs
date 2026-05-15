using Microsoft.Extensions.Configuration;
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
    public class AuthServiceConfigTests
    {
        [Fact]
        public async Task LoginAsync_SecretKeyTooShort_ThrowsAppException500()
        {
            var repoMock = new Mock<IUserRepository>();
            var service = new AuthService(
                repoMock.Object,
                JwtTestHelper.BuildShortKeyConfiguration());

            User user = UserTestData.AdminUser();
            repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "Admin@123" };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => service.LoginAsync(dto));

            Assert.Equal(500, ex.StatusCode);
            Assert.Contains("32", ex.Message);
        }

        [Fact]
        public async Task RegisterAsync_MissingSecretKey_ThrowsAppException500()
        {
            var repoMock = new Mock<IUserRepository>();
            var service = new AuthService(
                repoMock.Object,
                JwtTestHelper.BuildMissingKeyConfiguration());

            var dto = new RegisterDTO
            {
                Name = "X",
                Email = "x@x.com",
                Password = "pass123",
                Role = "Customer"
            };

            repoMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
            repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                    .ReturnsAsync((User u) => { u.UserId = 1; return u; });

            var ex = await Assert.ThrowsAsync<AppException>(
                () => service.RegisterAsync(dto));

            Assert.Equal(500, ex.StatusCode);
        }

        [Fact]
        public async Task LoginAsync_InvalidExpiryHours_FallsBackTo24Hours()
        {
            var settings = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = JwtTestHelper.SecretKey,
                ["JwtSettings:Issuer"] = JwtTestHelper.Issuer,
                ["JwtSettings:Audience"] = JwtTestHelper.Audience,
                ["JwtSettings:ExpiryHours"] = "not-a-number"
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();

            var repoMock = new Mock<IUserRepository>();
            var service = new AuthService(repoMock.Object, config);

            User user = UserTestData.AdminUser();
            repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            var dto = new LoginDTO { Email = user.Email, Password = "Admin@123" };
            var result = await service.LoginAsync(dto);

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True(result.ExpiresAt > DateTime.UtcNow.AddHours(23));
        }
    }
}