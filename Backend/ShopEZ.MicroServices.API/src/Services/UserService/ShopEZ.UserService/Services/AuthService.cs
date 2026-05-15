using Microsoft.IdentityModel.Tokens;
using ShopEZ.UserService.DTOs;
using ShopEZ.UserService.Exceptions;
using ShopEZ.UserService.Models;
using ShopEZ.UserService.Repositories.Interfaces;
using ShopEZ.UserService.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopEZ.UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        // ─────────────────────────────────────────────────────────────────────
        // REGISTER
        // ─────────────────────────────────────────────────────────────────────
        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO dto)
        {
            if (dto == null)
                throw new AppException("Registration data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("Name is required.", 400);

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new AppException("Email is required.", 400);

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new AppException("Password is required.", 400);

            if (dto.Password.Length < 6)
                throw new AppException("Password must be at least 6 characters.", 400);

            bool emailExists = await _userRepository.EmailExistsAsync(dto.Email);
            if (emailExists)
                throw new AppException("An account with this email already exists.", 409);

            string role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role.Trim();
            if (role != "Admin" && role != "Customer")
                throw new AppException("Role must be either 'Admin' or 'Customer'.", 400);

            // BCrypt work factor 12 — identical to monolith
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Password = hashedPassword,
                Role = role
            };

            User created = await _userRepository.CreateAsync(user);

            string token = GenerateJwtToken(created);
            DateTime expires = DateTime.UtcNow.AddHours(GetTokenExpiryHours());

            return new AuthResponseDTO
            {
                UserId = created.UserId,
                Name = created.Name,
                Email = created.Email,
                Role = created.Role,
                Token = token,
                ExpiresAt = expires
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // LOGIN
        // ─────────────────────────────────────────────────────────────────────
        public async Task<AuthResponseDTO> LoginAsync(LoginDTO dto)
        {
            if (dto == null)
                throw new AppException("Login data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new AppException("Email is required.", 400);

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new AppException("Password is required.", 400);

            User? user = await _userRepository.GetByEmailAsync(dto.Email);

            // Generic message — prevents user-enumeration attacks (same as monolith)
            if (user == null)
                throw new AppException("Invalid email or password.", 401);

            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            if (!passwordValid)
                throw new AppException("Invalid email or password.", 401);

            string token = GenerateJwtToken(user);
            DateTime expires = DateTime.UtcNow.AddHours(GetTokenExpiryHours());

            return new AuthResponseDTO
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Token = token,
                ExpiresAt = expires
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — Generate signed JWT
        // Claims and signing algorithm are IDENTICAL to the monolith so tokens
        // issued here are accepted by all downstream services using the same key.
        // ─────────────────────────────────────────────────────────────────────
        private string GenerateJwtToken(User user)
        {
            string? secretKey = _configuration["JwtSettings:SecretKey"];
            string? issuer = _configuration["JwtSettings:Issuer"];
            string? audience = _configuration["JwtSettings:Audience"];

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new AppException("JWT SecretKey is not configured.", 500);
            if (string.IsNullOrWhiteSpace(issuer))
                throw new AppException("JWT Issuer is not configured.", 500);
            if (string.IsNullOrWhiteSpace(audience))
                throw new AppException("JWT Audience is not configured.", 500);
            if (secretKey.Length < 32)
                throw new AppException("JWT SecretKey must be at least 32 characters.", 500);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,     user.UserId.ToString()),
                new Claim(ClaimTypes.Name,               user.Name),
                new Claim(ClaimTypes.Email,              user.Email),
                new Claim(ClaimTypes.Role,               user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(GetTokenExpiryHours()),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — Read expiry hours from config, default 24
        // ─────────────────────────────────────────────────────────────────────
        private double GetTokenExpiryHours()
        {
            string? expiryStr = _configuration["JwtSettings:ExpiryHours"];
            if (double.TryParse(expiryStr, out double hours) && hours > 0)
                return hours;
            return 24;
        }
    }
}