using Moq;
using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Exceptions;
using ShopEZ.ProductService.Models;
using ShopEZ.ProductService.Repositories.Interfaces;
using ProductServiceClass = ShopEZ.ProductService.Services.ProductService;
using ShopEZ.ProductService.Tests.Helpers;
using Xunit;

namespace ShopEZ.ProductService.Tests.Services
{
    public class ProductServiceCreateUpdateTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly ProductServiceClass _service;

        public ProductServiceCreateUpdateTests()
            => _service = new ProductServiceClass(_repoMock.Object);

        [Fact]
        public async Task CreateProductAsync_ValidDto_ReturnsMappedDTO()
        {
            CreateProductDTO dto = ProductTestData.ValidCreateDTO();

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .ReturnsAsync(new Product
                     {
                         ProductId = 5,
                         Name = dto.Name,
                         Description = dto.Description,
                         Price = dto.Price,
                         ImageUrl = dto.ImageUrl,
                         Stock = dto.Stock
                     });

            ProductDTO result = await _service.CreateProductAsync(dto);

            Assert.Equal(5, result.ProductId);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Price, result.Price);
        }

        [Fact]
        public async Task CreateProductAsync_NameIsTrimmedBeforeSave()
        {
            Product? captured = null;

            var dto = new CreateProductDTO
            {
                Name = "  Padded Name  ",
                Description = "Test",
                Price = 10m,
                ImageUrl = string.Empty,
                Stock = 5
            };

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => captured = p)
                     .ReturnsAsync(new Product { ProductId = 1, Name = "Padded Name", Price = 10m, Stock = 5 });

            await _service.CreateProductAsync(dto);

            Assert.Equal("Padded Name", captured!.Name);
        }

        [Fact]
        public async Task CreateProductAsync_ValidDto_CallsCreateOnce()
        {
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .ReturnsAsync(new Product { ProductId = 1, Name = "X", Price = 1m, Stock = 1 });

            await _service.CreateProductAsync(ProductTestData.ValidCreateDTO());

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task CreateProductAsync_ZeroStock_Succeeds()
        {
            var dto = new CreateProductDTO
            {
                Name = "Pre-order Item",
                Price = 99m,
                Stock = 0
            };

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .ReturnsAsync(new Product { ProductId = 9, Name = "Pre-order Item", Price = 99m, Stock = 0 });

            ProductDTO result = await _service.CreateProductAsync(dto);

            Assert.Equal(0, result.Stock);
        }

        [Fact]
        public async Task CreateProductAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(null!));
            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task CreateProductAsync_EmptyName_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(ProductTestData.CreateDTOWithEmptyName()));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateProductAsync_WhitespaceName_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(ProductTestData.CreateDTOWithWhitespaceName()));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateProductAsync_ZeroPrice_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(ProductTestData.CreateDTOWithZeroPrice()));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateProductAsync_NegativePrice_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(ProductTestData.CreateDTOWithNegativePrice()));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CreateProductAsync_NegativeStock_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => _service.CreateProductAsync(ProductTestData.CreateDTOWithNegativeStock()));
            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task UpdateProductAsync_ExistingId_ReturnsMappedDTO()
        {
            UpdateProductDTO dto = ProductTestData.ValidUpdateDTO();

            _repoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.UpdateAsync(1, It.IsAny<Product>()))
                     .ReturnsAsync(new Product { ProductId = 1, Name = dto.Name, Price = dto.Price, Stock = dto.Stock });

            ProductDTO? result = await _service.UpdateProductAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result!.Name);
        }

        [Fact]
        public async Task UpdateProductAsync_NonExistentId_ReturnsNull()
        {
            _repoMock.Setup(r => r.ExistsAsync(9999)).ReturnsAsync(false);

            ProductDTO? result = await _service.UpdateProductAsync(9999, ProductTestData.ValidUpdateDTO());

            Assert.Null(result);
        }
    }
}
