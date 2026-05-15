using ShopEZ.UserService.Models;

namespace ShopEZ.UserService.Repositories.Interfaces
{
    public interface IUserRepository
    {
        /// <summary>Returns the user whose email matches (case-insensitive), or null.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Returns the user with the given PK, or null.</summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>Returns true if any user record has this email (case-insensitive).</summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>Persists a new user and returns the tracked entity with its generated PK.</summary>
        Task<User> CreateAsync(User user);
    }
}