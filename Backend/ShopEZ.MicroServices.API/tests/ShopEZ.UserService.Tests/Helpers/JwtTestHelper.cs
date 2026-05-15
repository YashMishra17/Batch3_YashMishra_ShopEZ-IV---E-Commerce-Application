using Microsoft.Extensions.Configuration;

namespace ShopEZ.UserService.Tests.Helpers
{
    public static class JwtTestHelper
    {
        public const string SecretKey = "ShopEZ@SuperSecret#Key!2025$Secure&Long@Enough32Chars";
        public const string Issuer = "ShopEZ.API";
        public const string Audience = "ShopEZ.Client";
        public const string ExpiryHours = "24";

        public static IConfiguration BuildConfiguration()
        {
            var settings = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = SecretKey,
                ["JwtSettings:Issuer"] = Issuer,
                ["JwtSettings:Audience"] = Audience,
                ["JwtSettings:ExpiryHours"] = ExpiryHours
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        public static IConfiguration BuildShortKeyConfiguration()
        {
            var settings = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TooShort",
                ["JwtSettings:Issuer"] = Issuer,
                ["JwtSettings:Audience"] = Audience,
                ["JwtSettings:ExpiryHours"] = ExpiryHours
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        public static IConfiguration BuildMissingKeyConfiguration()
        {
            var settings = new Dictionary<string, string?>
            {
                ["JwtSettings:Issuer"] = Issuer,
                ["JwtSettings:Audience"] = Audience,
                ["JwtSettings:ExpiryHours"] = ExpiryHours
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }
    }
}