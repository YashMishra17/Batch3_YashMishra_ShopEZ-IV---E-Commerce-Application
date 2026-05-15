using System.ComponentModel.DataAnnotations;

namespace ShopEZ.PaymentService.DTOs
{
    public class RefundPaymentDTO
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}