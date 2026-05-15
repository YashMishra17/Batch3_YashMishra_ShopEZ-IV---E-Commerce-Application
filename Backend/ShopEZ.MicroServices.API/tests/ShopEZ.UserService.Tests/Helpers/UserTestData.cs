using ShopEZ.UserService.Models;

namespace ShopEZ.UserService.Tests.Helpers
{
    public static class UserTestData
    {
        public static User AdminUser() => new()
        {
            UserId = 1,
            Name = "Alice Johnson",
            Email = "alice@shopez.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 4),
            Role = "Admin"
        };

        public static User CustomerUser() => new()
        {
            UserId = 2,
            Name = "Bob Smith",
            Email = "bob@shopez.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Customer@123", workFactor: 4),
            Role = "Customer"
        };
    }
}
