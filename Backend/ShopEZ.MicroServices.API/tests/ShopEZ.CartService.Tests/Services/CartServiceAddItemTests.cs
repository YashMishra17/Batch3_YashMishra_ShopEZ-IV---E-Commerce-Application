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
    public class CartServiceAddItemTests
    {
        private readonly Mock<ICartRepository> _repoMock = new();
        private readonly CartServiceClass _service;

        public CartServiceAddItemTests()
            => _service = new CartServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // AddItemAsync — happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task AddItemAsync_NewItemToEmptyCart_ReturnsCartWithOneItem()
        {
            int userId = CartTestData.UserIdAlice;
            CartItemDTO dto = CartTestData.MouseDTO();

            _repoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(userId, dto);

            Assert.Single(result.Items);
            Assert.Equal(CartTestData.MouseProductId, result.Items.First().ProductId);
        }

        [Fact]
        public async Task AddItemAsync_NewItemToEmptyCart_TotalEqualsItemPrice()
        {
            CartItemDTO dto = CartTestData.MouseDTO();

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(CartTestData.UserIdAlice, dto);

            Assert.Equal(29.99m, result.Total);
        }

        [Fact]
        public async Task AddItemAsync_SecondDistinctItem_ReturnsCartWithTwoItems()
        {
            Cart existingCart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(existingCart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(
                CartTestData.UserIdAlice, CartTestData.KeyboardDTO());

            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task AddItemAsync_SecondDistinctItem_TotalUpdatedCorrectly()
        {
            Cart existingCart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(existingCart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(
                CartTestData.UserIdAlice, CartTestData.KeyboardDTO());

            Assert.Equal(109.98m, result.Total);
        }

        [Fact]
        public async Task AddItemAsync_SameProductTwice_IncrementsQuantity()
        {
            Cart existingCart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(existingCart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseDTO());

            Assert.Single(result.Items);
            Assert.Equal(2, result.Items.First().Quantity);
        }

        [Fact]
        public async Task AddItemAsync_SameProductTwice_TotalDoubles()
        {
            Cart existingCart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(existingCart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseDTO());

            Assert.Equal(59.98m, result.Total);
        }

        [Fact]
        public async Task AddItemAsync_SameProduct_RefreshesPriceSnapshot()
        {
            Cart existingCart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(existingCart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartItemDTO updatedPrice = CartTestData.MouseDTO();
            updatedPrice.Price = 34.99m;

            CartDTO result = await _service.AddItemAsync(CartTestData.UserIdAlice, updatedPrice);

            Assert.Equal(34.99m, result.Items.First().Price);
        }

        [Fact]
        public async Task AddItemAsync_MultipleQuantity_StockedItem_Succeeds()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.AddItemAsync(
                CartTestData.UserIdBob, CartTestData.HubDTO());

            Assert.Equal(2, result.Items.First().Quantity);
        }

        [Fact]
        public async Task AddItemAsync_CallsUpsertOnce()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            await _service.AddItemAsync(CartTestData.UserIdAlice, CartTestData.MouseDTO());

            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task AddItemAsync_ItemAddedToCorrectUser()
        {
            Cart? capturedCart = null;

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .Callback<Cart>(c => capturedCart = c)
                     .ReturnsAsync((Cart c) => c);

            await _service.AddItemAsync(CartTestData.UserIdBob, CartTestData.MouseDTO());

            Assert.NotNull(capturedCart);
            Assert.Equal(CartTestData.UserIdBob, capturedCart!.UserId);
        }

        [Fact]
        public async Task AddItemAsync_QuantityExceedsStock_ThrowsAppException400()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync((Cart?)null);

            CartItemDTO dto = CartTestData.LowStockItem().ToDTO();
            dto.Quantity = 10;

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(CartTestData.UserIdAlice, dto));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("stock", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddItemAsync_QuantityExceedsStock_UpsertNeverCalled()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync((Cart?)null);

            CartItemDTO dto = CartTestData.LowStockItem().ToDTO();
            dto.Quantity = 99;

            await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(CartTestData.UserIdAlice, dto));

            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task AddItemAsync_CombinedQuantityExceedsStock_ThrowsAppException400()
        {
            var lowStock = CartTestData.LowStockItem();
            var cart = new Cart
            {
                UserId = CartTestData.UserIdAlice,
                Items = new List<CartItem> { lowStock }
            };

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            CartItemDTO dto = lowStock.ToDTO();
            dto.Quantity = 2;

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(CartTestData.UserIdAlice, dto));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task AddItemAsync_ExactlyAtStockLimit_Succeeds()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartItemDTO dto = CartTestData.MouseDTO();
            dto.Quantity = 100;

            CartDTO result = await _service.AddItemAsync(CartTestData.UserIdBob, dto);

            Assert.Equal(100, result.Items.First().Quantity);
        }

        
        [Fact]
        public async Task AddItemAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(CartTestData.UserIdAlice, null!));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task AddItemAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(0, CartTestData.MouseDTO()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task AddItemAsync_NegativeUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(-5, CartTestData.MouseDTO()));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public async Task AddItemAsync_InvalidUserIds_AlwaysThrow400(int userId)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.AddItemAsync(userId, CartTestData.MouseDTO()));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}
