using System.Collections.Concurrent;
using ShopEZ.CartService.Models;
using ShopEZ.CartService.Repositories.Interfaces;

namespace ShopEZ.CartService.Repositories
{
    public class CartRepository : ICartRepository
    {
        private static readonly ConcurrentDictionary<int, Cart> _store
            = new();

        public Task<Cart?> GetByUserIdAsync(int userId)
        {
            _store.TryGetValue(userId, out Cart? cart);
            return Task.FromResult(cart);
        }

        public Task<Cart> UpsertAsync(Cart cart)
        {
            cart.UpdatedAt = DateTime.UtcNow;
            _store[cart.UserId] = cart;
            return Task.FromResult(cart);
        }

        public Task<bool> DeleteByUserIdAsync(int userId)
        {
            bool removed = _store.TryRemove(userId, out _);
            return Task.FromResult(removed);
        }

        public Task<bool> RemoveItemAsync(int userId, int productId)
        {
            if (!_store.TryGetValue(userId, out Cart? cart))
                return Task.FromResult(false);

            int before = cart.Items.Count;
            cart.Items.RemoveAll(i => i.ProductId == productId);
            cart.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult(cart.Items.Count < before);
        }

        public Task<bool> ExistsAsync(int userId)
            => Task.FromResult(_store.ContainsKey(userId));
    }
}