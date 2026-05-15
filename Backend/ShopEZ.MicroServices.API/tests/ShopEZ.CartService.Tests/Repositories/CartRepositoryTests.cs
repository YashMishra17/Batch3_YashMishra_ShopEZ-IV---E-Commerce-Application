using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories;
using ShopEZ.CartService.Tests.Helpers;
using Xunit;

namespace ShopEZ.CartService.Tests.Repositories
{
    /// <summary>
    /// Tests the in-memory CartRepository directly — no mocking required
    /// because the implementation is pure in-process state.
    /// Each test class instance gets a shared static ConcurrentDictionary,
    /// so tests use distinct user IDs to avoid cross-test contamination.
    /// </summary>
    public class CartRepositoryTests
    {
        private readonly CartRepository _repo = new CartRepository();

        // ── unique user IDs per test to avoid static-state collisions ──────────
        private static int _uidSeed = 1000;
        private static int NextUserId() => Interlocked.Increment(ref _uidSeed);

        // ─────────────────────────────────────────────────────────────────────
        // GetByUserIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByUserIdAsync_NewUser_ReturnsNull()
        {
            // Arrange
            int uid = NextUserId();

            // Act
            Cart? result = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_AfterUpsert_ReturnsCart()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.SingleItemCart(uid);
            await _repo.UpsertAsync(cart);

            // Act
            Cart? result = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(uid, result!.UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsCorrectUsersCart()
        {
            // Arrange — two users with different carts
            int uid1 = NextUserId();
            int uid2 = NextUserId();
            Cart cart1 = CartTestData.SingleItemCart(uid1);
            Cart cart2 = CartTestData.MultiItemCart(uid2);

            await _repo.UpsertAsync(cart1);
            await _repo.UpsertAsync(cart2);

            // Act
            Cart? result1 = await _repo.GetByUserIdAsync(uid1);
            Cart? result2 = await _repo.GetByUserIdAsync(uid2);

            // Assert
            Assert.Equal(uid1, result1!.UserId);
            Assert.Single(result1.Items);
            Assert.Equal(uid2, result2!.UserId);
            Assert.Equal(3, result2.Items.Count);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpsertAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpsertAsync_NewCart_ReturnsSavedCart()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.EmptyCart(uid);

            // Act
            Cart result = await _repo.UpsertAsync(cart);

            // Assert
            Assert.Equal(uid, result.UserId);
        }

        [Fact]
        public async Task UpsertAsync_NewCart_UpdatesTimestamp()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.EmptyCart(uid);
            cart.UpdatedAt = DateTime.UtcNow.AddDays(-1); // stale timestamp

            // Act
            Cart result = await _repo.UpsertAsync(cart);

            // Assert
            Assert.True(result.UpdatedAt >= DateTime.UtcNow.AddSeconds(-5));
        }

        [Fact]
        public async Task UpsertAsync_ExistingCart_OverwritesItems()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.SingleItemCart(uid);
            await _repo.UpsertAsync(cart);

            // Modify cart and upsert again
            cart.Items.Add(CartTestData.KeyboardItem());
            await _repo.UpsertAsync(cart);

            // Act
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.Equal(2, stored!.Items.Count);
        }

        [Fact]
        public async Task UpsertAsync_EmptyCart_IsStoredSuccessfully()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.EmptyCart(uid);

            // Act
            await _repo.UpsertAsync(cart);
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.NotNull(stored);
            Assert.Empty(stored!.Items);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DeleteByUserIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteByUserIdAsync_ExistingUser_ReturnsTrue()
        {
            // Arrange
            int uid = NextUserId();
            await _repo.UpsertAsync(CartTestData.SingleItemCart(uid));

            // Act
            bool result = await _repo.DeleteByUserIdAsync(uid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteByUserIdAsync_ExistingUser_CartNoLongerRetrievable()
        {
            // Arrange
            int uid = NextUserId();
            await _repo.UpsertAsync(CartTestData.SingleItemCart(uid));
            await _repo.DeleteByUserIdAsync(uid);

            // Act
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.Null(stored);
        }

        [Fact]
        public async Task DeleteByUserIdAsync_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            int uid = NextUserId(); // never upserted

            // Act
            bool result = await _repo.DeleteByUserIdAsync(uid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteByUserIdAsync_DeletedTwice_SecondCallReturnsFalse()
        {
            // Arrange
            int uid = NextUserId();
            await _repo.UpsertAsync(CartTestData.SingleItemCart(uid));
            await _repo.DeleteByUserIdAsync(uid);

            // Act
            bool secondDelete = await _repo.DeleteByUserIdAsync(uid);

            // Assert
            Assert.False(secondDelete);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RemoveItemAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_ReturnsTrue()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.SingleItemCart(uid);
            await _repo.UpsertAsync(cart);

            // Act
            bool result = await _repo.RemoveItemAsync(uid, CartTestData.MouseProductId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_ItemNoLongerInCart()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.MultiItemCart(uid);
            await _repo.UpsertAsync(cart);
            await _repo.RemoveItemAsync(uid, CartTestData.MouseProductId);

            // Act
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.DoesNotContain(stored!.Items,
                i => i.ProductId == CartTestData.MouseProductId);
        }

        [Fact]
        public async Task RemoveItemAsync_ExistingItem_OtherItemsIntact()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.MultiItemCart(uid);
            await _repo.UpsertAsync(cart);
            await _repo.RemoveItemAsync(uid, CartTestData.MouseProductId);

            // Act
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert — keyboard and hub still present
            Assert.Equal(2, stored!.Items.Count);
            Assert.Contains(stored.Items, i => i.ProductId == CartTestData.KeyboardProductId);
            Assert.Contains(stored.Items, i => i.ProductId == CartTestData.HubProductId);
        }

        [Fact]
        public async Task RemoveItemAsync_NonExistentItem_ReturnsFalse()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.SingleItemCart(uid);
            await _repo.UpsertAsync(cart);

            // Act — try to remove a product not in cart
            bool result = await _repo.RemoveItemAsync(uid, 9999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveItemAsync_NonExistentUser_ReturnsFalse()
        {
            // Arrange
            int uid = NextUserId(); // never upserted

            // Act
            bool result = await _repo.RemoveItemAsync(uid, CartTestData.MouseProductId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task RemoveItemAsync_LastItem_CartBecomesEmpty()
        {
            // Arrange
            int uid = NextUserId();
            Cart cart = CartTestData.SingleItemCart(uid);
            await _repo.UpsertAsync(cart);

            await _repo.RemoveItemAsync(uid, CartTestData.MouseProductId);

            // Act
            Cart? stored = await _repo.GetByUserIdAsync(uid);

            // Assert
            Assert.NotNull(stored);
            Assert.Empty(stored!.Items);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ExistsAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_NewUser_ReturnsFalse()
        {
            // Arrange
            int uid = NextUserId();

            // Act
            bool exists = await _repo.ExistsAsync(uid);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task ExistsAsync_AfterUpsert_ReturnsTrue()
        {
            // Arrange
            int uid = NextUserId();
            await _repo.UpsertAsync(CartTestData.EmptyCart(uid));

            // Act
            bool exists = await _repo.ExistsAsync(uid);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_AfterDelete_ReturnsFalse()
        {
            // Arrange
            int uid = NextUserId();
            await _repo.UpsertAsync(CartTestData.EmptyCart(uid));
            await _repo.DeleteByUserIdAsync(uid);

            // Act
            bool exists = await _repo.ExistsAsync(uid);

            // Assert
            Assert.False(exists);
        }
    }
}