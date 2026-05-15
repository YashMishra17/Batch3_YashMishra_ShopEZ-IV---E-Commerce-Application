
using Moq;
using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Exceptions;
using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories.Interfaces;
using CartServiceClass = ShopEZ.CartService.Services.CartService;
using ShopEZ.CartService.Tests.Helpers;
using Xunit;

namespace ShopEZ.CartService.Tests.Services
{
    public class CartServiceGetTests
    {
        private readonly Mock<ICartRepository> _repoMock = new();
        private readonly CartServiceClass _service;

        public CartServiceGetTests()
            => _service = new CartServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // GetCartAsync — happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetCartAsync_ExistingCart_ReturnsCartDTO()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            CartDTO result = await _service.GetCartAsync(CartTestData.UserIdAlice);

            Assert.Equal(CartTestData.UserIdAlice, result.UserId);
            Assert.Single(result.Items);
        }

        [Fact]
        public async Task GetCartAsync_NoCartExists_ReturnsEmptyCartDTO()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);

            CartDTO result = await _service.GetCartAsync(CartTestData.UserIdBob);

            Assert.Equal(CartTestData.UserIdBob, result.UserId);
            Assert.Empty(result.Items);
            Assert.Equal(0m, result.Total);
            Assert.Equal(0, result.TotalItems);
        }

        [Fact]
        public async Task GetCartAsync_MultipleItems_TotalCalculatedCorrectly()
        {
            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            CartDTO result = await _service.GetCartAsync(CartTestData.UserIdAlice);

            Assert.Equal(3, result.Items.Count);
            Assert.Equal(209.96m, result.Total);
        }

        [Fact]
        public async Task GetCartAsync_MultipleItems_TotalItemsCountedCorrectly()
        {
            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            CartDTO result = await _service.GetCartAsync(CartTestData.UserIdAlice);

            Assert.Equal(4, result.TotalItems);
        }

        [Fact]
        public async Task GetCartAsync_ItemFieldsMappedCorrectly()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            CartDTO result = await _service.GetCartAsync(CartTestData.UserIdAlice);
            CartItemDTO item = result.Items.First();

            Assert.Equal(CartTestData.MouseProductId, item.ProductId);
            Assert.Equal("Wireless Mouse", item.Name);
            Assert.Equal(29.99m, item.Price);
            Assert.Equal(1, item.Quantity);
            Assert.Equal(100, item.Stock);
        }

        [Fact]
        public async Task GetCartAsync_ValidId_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync((Cart?)null);

            await _service.GetCartAsync(CartTestData.UserIdAlice);

            _repoMock.Verify(r => r.GetByUserIdAsync(CartTestData.UserIdAlice), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetCartAsync — invalid input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetCartAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetCartAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.GetByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetCartAsync_NegativeUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetCartAsync(-1));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(int.MinValue)]
        public async Task GetCartAsync_InvalidUserIds_AlwaysThrow400(int userId)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetCartAsync(userId));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}