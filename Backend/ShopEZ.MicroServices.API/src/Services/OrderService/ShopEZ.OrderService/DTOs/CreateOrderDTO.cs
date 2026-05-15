using System.ComponentModel.DataAnnotations;

namespace ShopEZ.OrderService.DTOs
{
    public class CreateOrderDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        public List<CartItemDTO> CartItems { get; set; } = new();
    }
}