using ShopEZ.ProductService.DTOs;

namespace ShopEZ.ProductService.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO?> GetProductByIdAsync(int id);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO dto);
        Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto);
        Task<bool> DeleteProductAsync(int id);
        Task<PagedResultDTO<ProductDTO>> SearchProductsAsync(ProductSearchDTO dto);
    }
}