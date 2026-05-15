using System.ComponentModel.DataAnnotations;

namespace ShopEZ.UserService.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Customer";
    }
}