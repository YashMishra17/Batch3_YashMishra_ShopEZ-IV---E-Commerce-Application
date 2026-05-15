using System.ComponentModel.DataAnnotations;

namespace ShopEZ.CartService.DTOs
{
    public class UpdateCartItemDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}