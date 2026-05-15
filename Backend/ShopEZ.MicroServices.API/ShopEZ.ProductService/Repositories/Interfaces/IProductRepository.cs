using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Models;

namespace ShopEZ.ProductService.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(int id, Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(
            ProductSearchDTO searchDto);
        Task<bool> DeductStockAsync(int productId, int quantity);
    }
}
