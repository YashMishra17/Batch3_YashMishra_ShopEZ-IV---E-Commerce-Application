namespace ShopEZ.UserService.DTOs
{
    /// <summary>
    /// Identical to monolith's AuthResponseDTO.
    /// Field names, casing, and types must not change — Angular reads them directly.
    /// </summary>
    public class AuthResponseDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}