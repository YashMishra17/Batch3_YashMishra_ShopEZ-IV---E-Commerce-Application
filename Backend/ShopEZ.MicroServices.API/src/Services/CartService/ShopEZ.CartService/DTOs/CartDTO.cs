namespace ShopEZ.CartService.DTOs
{
    public class CartDTO
    {
        public int UserId { get; set; }
        public List<CartItemDTO> Items { get; set; } = new();
        public decimal Total { get; set; }
        public int TotalItems { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}