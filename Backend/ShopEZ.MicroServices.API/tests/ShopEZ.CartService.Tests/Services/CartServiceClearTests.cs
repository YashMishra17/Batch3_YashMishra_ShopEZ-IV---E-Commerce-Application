using Moq;
using ShopEZ.CartService.Exceptions;
using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories.Interfaces;
using CartServiceClass = ShopEZ.CartService.Services.CartService;
using ShopEZ.CartService.Tests.Helpers;
using Xunit;

namespace ShopEZ.CartService.Tests.Services
{
    public class CartServiceClearTests
    {
        private readonly Mock<ICartRepository> _repoMock = new();
        private readonly CartServiceClass _service;

        public CartServiceClearTests()
            => _service = new CartServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // ClearCartAsync — happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClearCartAsync_ExistingCart_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(true);

            bool result = await _service.ClearCartAsync(CartTestData.UserIdAlice);

            Assert.True(result);
        }

        [Fact]
        public async Task ClearCartAsync_NonExistentCart_ReturnsFalse()
        {
            _repoMock.Setup(r => r.DeleteByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync(false);

            bool result = await _service.ClearCartAsync(CartTestData.UserIdBob);

            Assert.False(result);
        }

        [Fact]
        public async Task ClearCartAsync_CallsDeleteByUserIdOnce()
        {
            _repoMock.Setup(r => r.DeleteByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(true);

            await _service.ClearCartAsync(CartTestData.UserIdAlice);

            _repoMock.Verify(r => r.DeleteByUserIdAsync(CartTestData.UserIdAlice), Times.Once);
        }

        [Fact]
        public async Task ClearCartAsync_MultiItemCart_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteByUserIdAsync(CartTestData.UserIdCharlie))
                     .ReturnsAsync(true);

            bool result = await _service.ClearCartAsync(CartTestData.UserIdCharlie);

            Assert.True(result);
        }

        [Fact]
        public async Task ClearCartAsync_CalledTwice_SecondReturnsFalse()
        {
            _repoMock.SetupSequence(r => r.DeleteByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(true)
                     .ReturnsAsync(false);

            bool first = await _service.ClearCartAsync(CartTestData.UserIdAlice);
            bool second = await _service.ClearCartAsync(CartTestData.UserIdAlice);

            Assert.True(first);
            Assert.False(second);
        }

        [Fact]
        public async Task ClearCartAsync_DoesNotCallGetOrUpsert()
        {
            _repoMock.Setup(r => r.DeleteByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(true);

            await _service.ClearCartAsync(CartTestData.UserIdAlice);

            _repoMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Never);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ClearCartAsync — invalid input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ClearCartAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ClearCartAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(
                r => r.DeleteByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ClearCartAsync_NegativeUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ClearCartAsync(-1));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public async Task ClearCartAsync_InvalidUserIds_AlwaysThrow400(int userId)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.ClearCartAsync(userId));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task ClearCartAsync_InvalidUserId_NeverCallsRepository()
        {
            await Assert.ThrowsAsync<AppException>(
                () => _service.ClearCartAsync(0));

            _repoMock.Verify(
                r => r.DeleteByUserIdAsync(It.IsAny<int>()), Times.Never);
        }
    }
}