using Microsoft.EntityFrameworkCore;
using ShopEZ.UserService.Data;
using ShopEZ.UserService.Models;
using ShopEZ.UserService.Repositories;
using ShopEZ.UserService.Tests.Helpers;
using Xunit;

namespace ShopEZ.UserService.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private static UserDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new UserDbContext(options);
        }

        private static UserRepository CreateRepository(UserDbContext ctx)
            => new UserRepository(ctx);

        // ─────────────────────────────────────────────────────────────────────
        // GetByEmailAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByEmailAsync_ExistingEmail_ReturnsUser()
        {
            await using var ctx = CreateContext(
                nameof(GetByEmailAsync_ExistingEmail_ReturnsUser));
            var user = UserTestData.AdminUser();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var result = await repo.GetByEmailAsync("alice@shopez.com");

            Assert.NotNull(result);
            Assert.Equal("alice@shopez.com", result!.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_CaseInsensitive_ReturnsUser()
        {
            await using var ctx = CreateContext(
                nameof(GetByEmailAsync_CaseInsensitive_ReturnsUser));
            var user = UserTestData.AdminUser();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var result = await repo.GetByEmailAsync("ALICE@SHOPEZ.COM");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByEmailAsync_NonExistentEmail_ReturnsNull()
        {
            await using var ctx = CreateContext(
                nameof(GetByEmailAsync_NonExistentEmail_ReturnsNull));
            var repo = CreateRepository(ctx);

            var result = await repo.GetByEmailAsync("nobody@shopez.com");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_EmptyDatabase_ReturnsNull()
        {
            await using var ctx = CreateContext(
                nameof(GetByEmailAsync_EmptyDatabase_ReturnsNull));
            var repo = CreateRepository(ctx);

            var result = await repo.GetByEmailAsync("test@test.com");

            Assert.Null(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetByIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsUser()
        {
            await using var ctx = CreateContext(
                nameof(GetByIdAsync_ExistingId_ReturnsUser));
            var user = UserTestData.CustomerUser();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            var result = await repo.GetByIdAsync(user.UserId);

            Assert.NotNull(result);
            Assert.Equal(user.UserId, result!.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            await using var ctx = CreateContext(
                nameof(GetByIdAsync_NonExistentId_ReturnsNull));
            var repo = CreateRepository(ctx);

            var result = await repo.GetByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ZeroId_ReturnsNull()
        {
            await using var ctx = CreateContext(
                nameof(GetByIdAsync_ZeroId_ReturnsNull));
            var repo = CreateRepository(ctx);

            var result = await repo.GetByIdAsync(0);

            Assert.Null(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EmailExistsAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task EmailExistsAsync_ExistingEmail_ReturnsTrue()
        {
            await using var ctx = CreateContext(
                nameof(EmailExistsAsync_ExistingEmail_ReturnsTrue));
            await ctx.Users.AddAsync(UserTestData.AdminUser());
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            bool exists = await repo.EmailExistsAsync("alice@shopez.com");

            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExistsAsync_CaseInsensitive_ReturnsTrue()
        {
            await using var ctx = CreateContext(
                nameof(EmailExistsAsync_CaseInsensitive_ReturnsTrue));
            await ctx.Users.AddAsync(UserTestData.AdminUser());
            await ctx.SaveChangesAsync();

            var repo = CreateRepository(ctx);
            bool exists = await repo.EmailExistsAsync("ALICE@SHOPEZ.COM");

            Assert.True(exists);
        }

        [Fact]
        public async Task EmailExistsAsync_NonExistentEmail_ReturnsFalse()
        {
            await using var ctx = CreateContext(
                nameof(EmailExistsAsync_NonExistentEmail_ReturnsFalse));
            var repo = CreateRepository(ctx);

            bool exists = await repo.EmailExistsAsync("ghost@shopez.com");

            Assert.False(exists);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CreateAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidUser_ReturnsSavedUserWithGeneratedId()
        {
            await using var ctx = CreateContext(
                nameof(CreateAsync_ValidUser_ReturnsSavedUserWithGeneratedId));
            var repo = CreateRepository(ctx);

            var newUser = new User
            {
                Name = "New User",
                Email = "new@shopez.com",
                Password = BCrypt.Net.BCrypt.HashPassword("pass123", 4),
                Role = "Customer"
            };

            var created = await repo.CreateAsync(newUser);

            Assert.True(created.UserId > 0);
            Assert.Equal("new@shopez.com", created.Email);
        }

        [Fact]
        public async Task CreateAsync_ValidUser_IsPersisted()
        {
            await using var ctx = CreateContext(
                nameof(CreateAsync_ValidUser_IsPersisted));
            var repo = CreateRepository(ctx);

            var newUser = new User
            {
                Name = "Persisted",
                Email = "persisted@shopez.com",
                Password = BCrypt.Net.BCrypt.HashPassword("pass123", 4),
                Role = "Customer"
            };

            var created = await repo.CreateAsync(newUser);
            var fetched = await repo.GetByIdAsync(created.UserId);

            Assert.NotNull(fetched);
            Assert.Equal("persisted@shopez.com", fetched!.Email);
        }

        [Fact]
        public async Task CreateAsync_MultipleUsers_EachGetsUniqueId()
        {
            await using var ctx = CreateContext(
                nameof(CreateAsync_MultipleUsers_EachGetsUniqueId));
            var repo = CreateRepository(ctx);

            var u1 = new User
            {
                Name = "U1",
                Email = "u1@test.com",
                Password = "h1",
                Role = "Customer"
            };
            var u2 = new User
            {
                Name = "U2",
                Email = "u2@test.com",
                Password = "h2",
                Role = "Admin"
            };

            var c1 = await repo.CreateAsync(u1);
            var c2 = await repo.CreateAsync(u2);

            Assert.NotEqual(c1.UserId, c2.UserId);
        }
    }
}