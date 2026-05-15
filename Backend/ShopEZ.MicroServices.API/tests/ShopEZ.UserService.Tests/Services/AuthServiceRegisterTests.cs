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
    public class AuthServiceRegisterTests
    {
        private readonly Mock<IUserRepository> _repoMock;
        private readonly AuthService _service;

        public AuthServiceRegisterTests()
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
        public async Task RegisterAsync_ValidCustomer_ReturnsAuthResponseWithToken()
        {
            var dto = new RegisterDTO
            {
                Name = "John",
                Email = "john@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 10; return u; });

            var result = await _service.RegisterAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(10, result.UserId);
            Assert.Equal("john@test.com", result.Email);
            Assert.Equal("Customer", result.Role);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task RegisterAsync_ValidAdmin_ReturnsAdminRole()
        {
            var dto = new RegisterDTO
            {
                Name = "Admin",
                Email = "admin@test.com",
                Password = "pass123",
                Role = "Admin"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 5; return u; });

            var result = await _service.RegisterAsync(dto);

            Assert.Equal("Admin", result.Role);
        }

        [Fact]
        public async Task RegisterAsync_EmptyRole_DefaultsToCustomer()
        {
            var dto = new RegisterDTO
            {
                Name = "X",
                Email = "x@test.com",
                Password = "pass123",
                Role = ""
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 7; return u; });

            var result = await _service.RegisterAsync(dto);

            Assert.Equal("Customer", result.Role);
        }

        [Fact]
        public async Task RegisterAsync_NameIsTrimmed()
        {
            User? captured = null;

            var dto = new RegisterDTO
            {
                Name = "  Padded Name  ",
                Email = "p@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .Callback<User>(u => captured = u)
                     .ReturnsAsync((User u) => { u.UserId = 9; return u; });

            await _service.RegisterAsync(dto);

            Assert.Equal("Padded Name", captured!.Name);
        }

        [Fact]
        public async Task RegisterAsync_EmailNormalisedToLowerCase()
        {
            User? captured = null;

            var dto = new RegisterDTO
            {
                Name = "X",
                Email = "UPPER@TEST.COM",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .Callback<User>(u => captured = u)
                     .ReturnsAsync((User u) => { u.UserId = 11; return u; });

            await _service.RegisterAsync(dto);

            Assert.Equal("upper@test.com", captured!.Email);
        }

        [Fact]
        public async Task RegisterAsync_PasswordIsHashed()
        {
            const string plain = "myplainpassword";
            User? captured = null;

            var dto = new RegisterDTO
            {
                Name = "Hash",
                Email = "hash@test.com",
                Password = plain,
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .Callback<User>(u => captured = u)
                     .ReturnsAsync((User u) => { u.UserId = 12; return u; });

            await _service.RegisterAsync(dto);

            Assert.NotEqual(plain, captured!.Password);
            Assert.True(BCrypt.Net.BCrypt.Verify(plain, captured.Password));
        }

        [Fact]
        public async Task RegisterAsync_TokenStartsWithEy()
        {
            var dto = new RegisterDTO
            {
                Name = "Tok",
                Email = "tok@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 20; return u; });

            var result = await _service.RegisterAsync(dto);

            Assert.True(result.Token.StartsWith("ey"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Null / empty input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(null!));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RegisterAsync_EmptyName_ThrowsAppException400()
        {
            var dto = new RegisterDTO
            {
                Name = "",
                Email = "t@t.com",
                Password = "pass123",
                Role = "Customer"
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Name", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_WhitespaceName_ThrowsAppException400()
        {
            var dto = new RegisterDTO
            {
                Name = "   ",
                Email = "t@t.com",
                Password = "pass123",
                Role = "Customer"
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RegisterAsync_EmptyEmail_ThrowsAppException400()
        {
            var dto = new RegisterDTO
            {
                Name = "Test",
                Email = "",
                Password = "pass123",
                Role = "Customer"
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Email", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_EmptyPassword_ThrowsAppException400()
        {
            var dto = new RegisterDTO
            {
                Name = "Test",
                Email = "t@t.com",
                Password = "",
                Role = "Customer"
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Password", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Password length
        // ─────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("abc")]
        [InlineData("abcd")]
        [InlineData("abcde")]
        public async Task RegisterAsync_PasswordTooShort_ThrowsAppException400(
            string shortPw)
        {
            var dto = new RegisterDTO
            {
                Name = "Test",
                Email = "t@t.com",
                Password = shortPw,
                Role = "Customer"
            };

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task RegisterAsync_PasswordExactly6Chars_Succeeds()
        {
            var dto = new RegisterDTO
            {
                Name = "Min",
                Email = "min@test.com",
                Password = "abc123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 30; return u; });

            var result = await _service.RegisterAsync(dto);

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Invalid role
        // ─────────────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("Moderator")]
        [InlineData("SuperAdmin")]
        [InlineData("guest")]
        [InlineData("USER")]
        public async Task RegisterAsync_InvalidRole_ThrowsAppException400(
            string invalidRole)
        {
            var dto = new RegisterDTO
            {
                Name = "R",
                Email = "r@test.com",
                Password = "pass123",
                Role = invalidRole
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("Role", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Duplicate email
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ThrowsAppException409()
        {
            var dto = new RegisterDTO
            {
                Name = "Dupe",
                Email = "dupe@shopez.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            Assert.Equal(409, ex.StatusCode);
            Assert.Contains("already exists", ex.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_NeverCallsCreateAsync()
        {
            var dto = new RegisterDTO
            {
                Name = "D",
                Email = "d@d.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(true);

            await Assert.ThrowsAsync<AppException>(
                () => _service.RegisterAsync(dto));

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Repository verification
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_Success_CallsEmailExistsAndCreateOnce()
        {
            var dto = new RegisterDTO
            {
                Name = "Once",
                Email = "once@test.com",
                Password = "pass123",
                Role = "Customer"
            };

            _repoMock.Setup(r => r.EmailExistsAsync(dto.Email))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<User>()))
                     .ReturnsAsync((User u) => { u.UserId = 99; return u; });

            await _service.RegisterAsync(dto);

            _repoMock.Verify(r => r.EmailExistsAsync(dto.Email), Times.Once);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }
    }
}