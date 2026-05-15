using System.ComponentModel.DataAnnotations;

namespace ShopEZ.OrderService.DTOs
{
    public class UpdateOrderStatusDTO
    {
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}
