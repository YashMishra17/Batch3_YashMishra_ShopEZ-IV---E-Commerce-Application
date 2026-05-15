using System.ComponentModel.DataAnnotations;

namespace ShopEZ.PaymentService.DTOs
{
    public class ProcessPaymentDTO
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = "Card";

        [MaxLength(19)]
        public string CardNumber { get; set; } = string.Empty;

        [MaxLength(5)]
        public string CardExpiry { get; set; } = string.Empty;

        [MaxLength(4)]
        public string CardCvv { get; set; } = string.Empty;
    }
}