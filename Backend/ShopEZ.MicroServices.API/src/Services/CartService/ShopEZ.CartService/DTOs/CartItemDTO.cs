using System.ComponentModel.DataAnnotations;

namespace ShopEZ.CartService.DTOs
{
    public class CartItemDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
    }
}