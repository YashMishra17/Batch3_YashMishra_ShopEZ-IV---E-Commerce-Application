//using Moq;
//using ShopEZ.CartService.DTOs;
//using ShopEZ.CartService.Exceptions;
//using ShopEZ.CartService.Models;
//using ShopEZ.CartService.Repositories.Interfaces;
//using ShopEZ.CartService.Services;
//using ShopEZ.CartService.Tests.Helpers;
//using Xunit;

//namespace ShopEZ.CartService.Tests.Services
//{
//    public class CartServiceUpdateRemoveTests
//    {
//        private readonly Mock<ICartRepository> _repoMock = new();
//        private readonly CartService _service;

//        public CartServiceUpdateRemoveTests()
//            => _service = new CartService(_repoMock.Object);

//        // ─────────────────────────────────────────────────────────────────────
//        // UpdateItemAsync — happy paths
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task UpdateItemAsync_ValidQuantity_ReturnsUpdatedCart()
//        {
//            // Arrange
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            var dto = new UpdateCartItemDTO { Quantity = 5 };

//            // Act
//            CartDTO result = await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId, dto);

//            // Assert
//            Assert.Equal(5, result.Items.First().Quantity);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_ValidQuantity_TotalRecalculated()
//        {
//            // Arrange — Mouse 29.99 × new qty 3 = 89.97
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            // Act
//            CartDTO result = await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice,
//                CartTestData.MouseProductId,
//                new UpdateCartItemDTO { Quantity = 3 });

//            // Assert
//            Assert.Equal(89.97m, result.Total);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_QuantityOne_IsValid()
//        {
//            // Arrange
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            // Act — update to quantity 1 (minimum allowed)
//            CartDTO result = await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice,
//                CartTestData.MouseProductId,
//                new UpdateCartItemDTO { Quantity = 1 });

//            // Assert
//            Assert.Equal(1, result.Items.First().Quantity);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_ValidUpdate_CallsUpsertOnce()
//        {
//            // Arrange
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            // Act
//            await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice,
//                CartTestData.MouseProductId,
//                new UpdateCartItemDTO { Quantity = 2 });

//            // Assert
//            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_OnlyTargetItemQuantityChanges()
//        {
//            // Arrange — multi-item cart
//            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            // Act — update only Mouse
//            CartDTO result = await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice,
//                CartTestData.MouseProductId,
//                new UpdateCartItemDTO { Quantity = 7 });

//            // Assert — keyboard and hub quantities unchanged
//            Assert.Equal(7, result.Items
//                .First(i => i.ProductId == CartTestData.MouseProductId).Quantity);
//            Assert.Equal(1, result.Items
//                .First(i => i.ProductId == CartTestData.KeyboardProductId).Quantity);
//            Assert.Equal(2, result.Items
//                .First(i => i.ProductId == CartTestData.HubProductId).Quantity);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // UpdateItemAsync — stock validation
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task UpdateItemAsync_QuantityExceedsStock_ThrowsAppException400()
//        {
//            // Arrange — Mouse has Stock 100
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);

//            // Act + Assert — request 200 (> 100 stock)
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.UpdateItemAsync(
//                    CartTestData.UserIdAlice,
//                    CartTestData.MouseProductId,
//                    new UpdateCartItemDTO { Quantity = 200 }));

//            Assert.Equal(400, ex.StatusCode);
//            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Never);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_QuantityExactlyAtStock_Succeeds()
//        {
//            // Arrange — Mouse Stock = 100
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
//                     .ReturnsAsync((Cart c) => c);

//            // Act
//            CartDTO result = await _service.UpdateItemAsync(
//                CartTestData.UserIdAlice,
//                CartTestData.MouseProductId,
//                new UpdateCartItemDTO { Quantity = 100 });

//            // Assert
//            Assert.Equal(100, result.Items.First().Quantity);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // UpdateItemAsync — not found
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task UpdateItemAsync_CartNotFound_ThrowsAppException404()
//        {
//            // Arrange
//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
//                     .ReturnsAsync((Cart?)null);

//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.UpdateItemAsync(
//                    CartTestData.UserIdBob,
//                    CartTestData.MouseProductId,
//                    new UpdateCartItemDTO { Quantity = 2 }));

//            Assert.Equal(404, ex.StatusCode);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_ProductNotInCart_ThrowsAppException404()
//        {
//            // Arrange — cart exists but does not contain KeyboardProductId
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);

//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.UpdateItemAsync(
//                    CartTestData.UserIdAlice,
//                    CartTestData.KeyboardProductId,  // not in cart
//                    new UpdateCartItemDTO { Quantity = 1 }));

//            Assert.Equal(404, ex.StatusCode);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // UpdateItemAsync — invalid input
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task UpdateItemAsync_ZeroUserId_ThrowsAppException400()
//        {
//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.UpdateItemAsync(
//                    0, CartTestData.MouseProductId,
//                    new UpdateCartItemDTO { Quantity = 1 }));

//            Assert.Equal(400, ex.StatusCode);
//        }

//        [Fact]
//        public async Task UpdateItemAsync_NullDto_ThrowsAppException400()
//        {
//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.UpdateItemAsync(
//                    CartTestData.UserIdAlice,
//                    CartTestData.MouseProductId,
//                    null!));

//            Assert.Equal(400, ex.StatusCode);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // RemoveItemAsync — happy paths
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task RemoveItemAsync_ExistingItem_ReturnsCartWithoutItem()
//        {
//            // Arrange
//            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
//                     .ReturnsAsync(true);

//            // Simulate stored cart minus mouse
//            Cart cartAfter = new()
//            {
//                UserId = CartTestData.UserIdAlice,
//                Items = new List<CartItem> { CartTestData.KeyboardItem(), CartTestData.HubItem() }
//            };

//            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart)      // first call: full cart
//                     .ReturnsAsync(cartAfter); // second call: after removal

//            // Act
//            CartDTO result = await _service.RemoveItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId);

//            // Assert
//            Assert.Equal(2, result.Items.Count);
//            Assert.DoesNotContain(result.Items,
//                i => i.ProductId == CartTestData.MouseProductId);
//        }

//        [Fact]
//        public async Task RemoveItemAsync_SingleItemCart_ReturnsEmptyCart()
//        {
//            // Arrange
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart)           // initial get
//                     .ReturnsAsync(CartTestData.EmptyCart(CartTestData.UserIdAlice)); // after removal

//            _repoMock.Setup(r => r.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
//                     .ReturnsAsync(true);

//            // Act
//            CartDTO result = await _service.RemoveItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId);

//            // Assert
//            Assert.Empty(result.Items);
//            Assert.Equal(0m, result.Total);
//        }

//        [Fact]
//        public async Task RemoveItemAsync_ExistingItem_TotalUpdated()
//        {
//            // Arrange — remove Mouse 29.99; remaining Keyboard 79.99 + Hub 99.98 = 179.97
//            Cart cartBefore = CartTestData.MultiItemCart(CartTestData.UserIdAlice);
//            Cart cartAfter = new()
//            {
//                UserId = CartTestData.UserIdAlice,
//                Items = new List<CartItem> { CartTestData.KeyboardItem(), CartTestData.HubItem() }
//            };

//            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cartBefore)
//                     .ReturnsAsync(cartAfter);

//            _repoMock.Setup(r => r.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
//                     .ReturnsAsync(true);

//            // Act
//            CartDTO result = await _service.RemoveItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId);

//            // Assert — Keyboard(79.99×1) + Hub(49.99×2)
//            Assert.Equal(179.97m, result.Total);
//        }

//        [Fact]
//        public async Task RemoveItemAsync_CallsRemoveItemOnRepository()
//        {
//            // Arrange
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart)
//                     .ReturnsAsync(CartTestData.EmptyCart(CartTestData.UserIdAlice));

//            _repoMock.Setup(r => r.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
//                     .ReturnsAsync(true);

//            // Act
//            await _service.RemoveItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId);

//            // Assert
//            _repoMock.Verify(r => r.RemoveItemAsync(
//                CartTestData.UserIdAlice, CartTestData.MouseProductId), Times.Once);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // RemoveItemAsync — not found
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task RemoveItemAsync_CartNotFound_ThrowsAppException404()
//        {
//            // Arrange
//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
//                     .ReturnsAsync((Cart?)null);

//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.RemoveItemAsync(
//                    CartTestData.UserIdBob, CartTestData.MouseProductId));

//            Assert.Equal(404, ex.StatusCode);
//        }

//        [Fact]
//        public async Task RemoveItemAsync_ItemNotInCart_ThrowsAppException404()
//        {
//            // Arrange — cart exists but item not in it
//            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

//            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
//                     .ReturnsAsync(cart);
//            _repoMock.Setup(r => r.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.KeyboardProductId))
//                     .ReturnsAsync(false);

//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.RemoveItemAsync(
//                    CartTestData.UserIdAlice, CartTestData.KeyboardProductId));

//            Assert.Equal(404, ex.StatusCode);
//        }

//        // ─────────────────────────────────────────────────────────────────────
//        // RemoveItemAsync — invalid input
//        // ─────────────────────────────────────────────────────────────────────

//        [Fact]
//        public async Task RemoveItemAsync_ZeroUserId_ThrowsAppException400()
//        {
//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.RemoveItemAsync(0, CartTestData.MouseProductId));

//            Assert.Equal(400, ex.StatusCode);
//            _repoMock.Verify(r => r.RemoveItemAsync(
//                It.IsAny<int>(), It.IsAny<int>()), Times.Never);
//        }

//        [Fact]
//        public async Task RemoveItemAsync_NegativeUserId_ThrowsAppException400()
//        {
//            // Act + Assert
//            var ex = await Assert.ThrowsAsync<AppException>(
//                () => _service.RemoveItemAsync(-1, CartTestData.MouseProductId));

//            Assert.Equal(400, ex.StatusCode);
//        }
//    }
//}

//////////////////////////////////             
///

using Moq;
using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Exceptions;
using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories.Interfaces;
using ShopEZ.CartService.Services;
using ShopEZ.CartService.Tests.Helpers;
using Xunit;

namespace ShopEZ.CartService.Tests.Services
{
    public class CartServiceUpdateRemoveTests
    {
        private readonly Mock<ICartRepository> _repoMock = new();
        private readonly global::ShopEZ.CartService.Services.CartService _service;

        public CartServiceUpdateRemoveTests()
        {
            _service = new global::ShopEZ.CartService.Services.CartService(_repoMock.Object);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateItemAsync — happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateItemAsync_ValidQuantity_ReturnsUpdatedCart()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            var dto = new UpdateCartItemDTO { Quantity = 5 };

            CartDTO result = await _service.UpdateItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId, dto);

            Assert.Equal(5, result.Items.First().Quantity);
        }

        [Fact]
        public async Task UpdateItemAsync_ValidQuantity_TotalRecalculated()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.UpdateItemAsync(
                CartTestData.UserIdAlice,
                CartTestData.MouseProductId,
                new UpdateCartItemDTO { Quantity = 3 });

            Assert.Equal(89.97m, result.Total);
        }

        [Fact]
        public async Task UpdateItemAsync_QuantityOne_IsValid()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.UpdateItemAsync(
                CartTestData.UserIdAlice,
                CartTestData.MouseProductId,
                new UpdateCartItemDTO { Quantity = 1 });

            Assert.Equal(1, result.Items.First().Quantity);
        }

        [Fact]
        public async Task UpdateItemAsync_ValidUpdate_CallsUpsertOnce()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            await _service.UpdateItemAsync(
                CartTestData.UserIdAlice,
                CartTestData.MouseProductId,
                new UpdateCartItemDTO { Quantity = 2 });

            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Once);
        }

        [Fact]
        public async Task UpdateItemAsync_OnlyTargetItemQuantityChanges()
        {
            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.UpdateItemAsync(
                CartTestData.UserIdAlice,
                CartTestData.MouseProductId,
                new UpdateCartItemDTO { Quantity = 7 });

            Assert.Equal(7, result.Items.First(i => i.ProductId == CartTestData.MouseProductId).Quantity);
            Assert.Equal(1, result.Items.First(i => i.ProductId == CartTestData.KeyboardProductId).Quantity);
            Assert.Equal(2, result.Items.First(i => i.ProductId == CartTestData.HubProductId).Quantity);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateItemAsync — stock validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateItemAsync_QuantityExceedsStock_ThrowsAppException400()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateItemAsync(
                    CartTestData.UserIdAlice,
                    CartTestData.MouseProductId,
                    new UpdateCartItemDTO { Quantity = 200 }));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.UpsertAsync(It.IsAny<Cart>()), Times.Never);
        }

        [Fact]
        public async Task UpdateItemAsync_QuantityExactlyAtStock_Succeeds()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.UpsertAsync(It.IsAny<Cart>()))
                     .ReturnsAsync((Cart c) => c);

            CartDTO result = await _service.UpdateItemAsync(
                CartTestData.UserIdAlice,
                CartTestData.MouseProductId,
                new UpdateCartItemDTO { Quantity = 100 });

            Assert.Equal(100, result.Items.First().Quantity);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateItemAsync — not found
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateItemAsync_CartNotFound_ThrowsAppException404()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateItemAsync(
                    CartTestData.UserIdBob,
                    CartTestData.MouseProductId,
                    new UpdateCartItemDTO { Quantity = 2 }));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateItemAsync_ProductNotInCart_ThrowsAppException404()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateItemAsync(
                    CartTestData.UserIdAlice,
                    CartTestData.KeyboardProductId,
                    new UpdateCartItemDTO { Quantity = 1 }));

            Assert.Equal(404, ex.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateItemAsync — invalid input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateItemAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateItemAsync(
                    0, CartTestData.MouseProductId,
                    new UpdateCartItemDTO { Quantity = 1 }));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateItemAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.UpdateItemAsync(
                    CartTestData.UserIdAlice,
                    CartTestData.MouseProductId,
                    null!));

            Assert.Equal(400, ex.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RemoveItemAsync — happy paths
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_ReturnsCartWithoutItem()
        {
            Cart cart = CartTestData.MultiItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
                     .ReturnsAsync(true);

            Cart cartAfter = new()
            {
                UserId = CartTestData.UserIdAlice,
                Items = new List<CartItem> { CartTestData.KeyboardItem(), CartTestData.HubItem() }
            };

            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart)
                     .ReturnsAsync(cartAfter);

            CartDTO result = await _service.RemoveItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId);

            Assert.Equal(2, result.Items.Count);
            Assert.DoesNotContain(result.Items, i => i.ProductId == CartTestData.MouseProductId);
        }

        [Fact]
        public async Task RemoveItemAsync_SingleItemCart_ReturnsEmptyCart()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart)
                     .ReturnsAsync(CartTestData.EmptyCart(CartTestData.UserIdAlice));

            _repoMock.Setup(r => r.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
                     .ReturnsAsync(true);

            CartDTO result = await _service.RemoveItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId);

            Assert.Empty(result.Items);
            Assert.Equal(0m, result.Total);
        }

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_TotalUpdated()
        {
            Cart cartBefore = CartTestData.MultiItemCart(CartTestData.UserIdAlice);
            Cart cartAfter = new()
            {
                UserId = CartTestData.UserIdAlice,
                Items = new List<CartItem> { CartTestData.KeyboardItem(), CartTestData.HubItem() }
            };

            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cartBefore)
                     .ReturnsAsync(cartAfter);

            _repoMock.Setup(r => r.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
                     .ReturnsAsync(true);

            CartDTO result = await _service.RemoveItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId);

            Assert.Equal(179.97m, result.Total);
        }

        [Fact]
        public async Task RemoveItemAsync_CallsRemoveItemOnRepository()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.SetupSequence(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart)
                     .ReturnsAsync(CartTestData.EmptyCart(CartTestData.UserIdAlice));

            _repoMock.Setup(r => r.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.MouseProductId))
                     .ReturnsAsync(true);

            await _service.RemoveItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId);

            _repoMock.Verify(r => r.RemoveItemAsync(
                CartTestData.UserIdAlice, CartTestData.MouseProductId), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RemoveItemAsync — not found
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveItemAsync_CartNotFound_ThrowsAppException404()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdBob))
                     .ReturnsAsync((Cart?)null);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RemoveItemAsync(
                    CartTestData.UserIdBob, CartTestData.MouseProductId));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task RemoveItemAsync_ItemNotInCart_ThrowsAppException404()
        {
            Cart cart = CartTestData.SingleItemCart(CartTestData.UserIdAlice);

            _repoMock.Setup(r => r.GetByUserIdAsync(CartTestData.UserIdAlice))
                     .ReturnsAsync(cart);
            _repoMock.Setup(r => r.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.KeyboardProductId))
                     .ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RemoveItemAsync(
                    CartTestData.UserIdAlice, CartTestData.KeyboardProductId));

            Assert.Equal(404, ex.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RemoveItemAsync — invalid input
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveItemAsync_ZeroUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RemoveItemAsync(0, CartTestData.MouseProductId));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.RemoveItemAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task RemoveItemAsync_NegativeUserId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.RemoveItemAsync(-1, CartTestData.MouseProductId));

            Assert.Equal(400, ex.StatusCode);
        }
    }
}