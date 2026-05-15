namespace ShopEZ.CartService.Models
{
    public class Cart
    {
        public int UserId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public decimal Total => Items.Sum(i => i.Price * i.Quantity);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
