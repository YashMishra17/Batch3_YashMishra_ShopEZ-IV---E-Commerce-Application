using Microsoft.EntityFrameworkCore;
using ShopEZ.UserService.Data;
using ShopEZ.UserService.Models;
using ShopEZ.UserService.Repositories.Interfaces;

namespace ShopEZ.UserService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY EMAIL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // EMAIL EXISTS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}