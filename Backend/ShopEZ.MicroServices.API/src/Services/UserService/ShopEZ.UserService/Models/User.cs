using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopEZ.UserService.Models
{
    /// <summary>
    /// Exact field set from the monolith's User model.
    /// ICollection<Order> is removed — orders live in OrderService.
    /// </summary>
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "Customer";
    }
}
