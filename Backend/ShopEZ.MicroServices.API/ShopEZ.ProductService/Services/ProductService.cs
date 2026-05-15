using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Exceptions;
using ShopEZ.ProductService.Models;
using ShopEZ.ProductService.Repositories.Interfaces;
using ShopEZ.ProductService.Services.Interfaces;

namespace ShopEZ.ProductService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;

        public ProductService(IProductRepository repo)
        {
            _repo = repo;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            IEnumerable<Product> products = await _repo.GetAllAsync();
            return products.Select(MapToDTO);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ProductDTO?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                throw new AppException(
                    "Product ID must be a positive integer.", 400);

            Product? product = await _repo.GetByIdAsync(id);
            return product is null ? null : MapToDTO(product);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO dto)
        {
            if (dto is null)
                throw new AppException("Product data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("Product name is required.", 400);

            if (dto.Price <= 0)
                throw new AppException(
                    "Product price must be greater than zero.", 400);

            if (dto.Stock < 0)
                throw new AppException("Stock cannot be negative.", 400);

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl?.Trim() ?? string.Empty,
                Stock = dto.Stock
            };

            Product created = await _repo.CreateAsync(product);
            return MapToDTO(created);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<ProductDTO?> UpdateProductAsync(int id, UpdateProductDTO dto)
        {
            if (id <= 0)
                throw new AppException(
                    "Product ID must be a positive integer.", 400);

            if (dto is null)
                throw new AppException("Update data cannot be null.", 400);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new AppException("Product name is required.", 400);

            if (dto.Price <= 0)
                throw new AppException(
                    "Product price must be greater than zero.", 400);

            if (dto.Stock < 0)
                throw new AppException("Stock cannot be negative.", 400);

            bool exists = await _repo.ExistsAsync(id);
            if (!exists) return null;

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim() ?? string.Empty,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl?.Trim() ?? string.Empty,
                Stock = dto.Stock
            };

            Product? updated = await _repo.UpdateAsync(id, product);
            return updated is null ? null : MapToDTO(updated);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
                throw new AppException(
                    "Product ID must be a positive integer.", 400);

            return await _repo.DeleteAsync(id);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SEARCH
        // ─────────────────────────────────────────────────────────────────────
        public async Task<PagedResultDTO<ProductDTO>> SearchProductsAsync(
            ProductSearchDTO dto)
        {
            if (dto is null)
                throw new AppException("Search parameters cannot be null.", 400);

            if (dto.MinPrice.HasValue && dto.MaxPrice.HasValue
                && dto.MinPrice > dto.MaxPrice)
                throw new AppException(
                    "MinPrice cannot be greater than MaxPrice.", 400);

            (IEnumerable<Product> items, int total) =
                await _repo.SearchAsync(dto);

            return new PagedResultDTO<ProductDTO>
            {
                Items = items.Select(MapToDTO),
                TotalCount = total,
                Page = dto.Page,
                PageSize = dto.PageSize
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — mapping
        // ─────────────────────────────────────────────────────────────────────
        private static ProductDTO MapToDTO(Product p) => new()
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImageUrl = p.ImageUrl,
            Stock = p.Stock
        };
    }
}
