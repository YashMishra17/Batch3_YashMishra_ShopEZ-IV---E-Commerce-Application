namespace ShopEZ.ProductService.Models
{
    /// <summary>
    /// Plain POCO — no EF navigation properties.
    /// Dapper maps SQL column names to these properties by name (case-insensitive).
    /// </summary>
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}