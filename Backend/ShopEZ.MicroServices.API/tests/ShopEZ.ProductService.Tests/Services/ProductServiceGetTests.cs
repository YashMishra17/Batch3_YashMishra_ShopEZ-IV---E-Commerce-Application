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
    public class ProductServiceGetTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly ProductServiceClass _service;

        public ProductServiceGetTests()
            => _service = new ProductServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // GetAllProductsAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllProductsAsync_ThreeProducts_ReturnsMappedDTOs()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(ProductTestData.ThreeProducts());

            IEnumerable<ProductDTO> result = await _service.GetAllProductsAsync();

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllProductsAsync_EmptyDatabase_ReturnsEmptyCollection()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(ProductTestData.EmptyProductList());

            IEnumerable<ProductDTO> result = await _service.GetAllProductsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProductsAsync_FieldsMappedCorrectly()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Product> { ProductTestData.Mouse() });

            ProductDTO dto = (await _service.GetAllProductsAsync()).First();

            Assert.Equal(1, dto.ProductId);
            Assert.Equal("Wireless Mouse", dto.Name);
            Assert.Equal(29.99m, dto.Price);
            Assert.Equal(100, dto.Stock);
            Assert.False(string.IsNullOrWhiteSpace(dto.ImageUrl));
        }

        [Fact]
        public async Task GetAllProductsAsync_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(ProductTestData.EmptyProductList());

            await _service.GetAllProductsAsync();

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_SingleProduct_ReturnsSingleDTO()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Product> { ProductTestData.Mouse() });

            IEnumerable<ProductDTO> result = await _service.GetAllProductsAsync();

            Assert.Single(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetProductByIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetProductByIdAsync_ExistingId_ReturnsMappedDTO()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(ProductTestData.Mouse());

            ProductDTO? result = await _service.GetProductByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.ProductId);
            Assert.Equal("Wireless Mouse", result.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_NonExistentId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9999))
                     .ReturnsAsync((Product?)null);

            ProductDTO? result = await _service.GetProductByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductByIdAsync_ZeroId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetProductByIdAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetProductByIdAsync_NegativeId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetProductByIdAsync(-1));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public async Task GetProductByIdAsync_InvalidIds_AlwaysThrow400(int id)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.GetProductByIdAsync(id));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task GetProductByIdAsync_ValidId_CallsRepositoryOnce()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(ProductTestData.Mouse());

            await _service.GetProductByIdAsync(1);

            _repoMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_AllFieldsMapped()
        {
            Product mouse = ProductTestData.Mouse();
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(mouse);

            ProductDTO? dto = await _service.GetProductByIdAsync(1);

            Assert.Equal(mouse.ProductId, dto!.ProductId);
            Assert.Equal(mouse.Name, dto.Name);
            Assert.Equal(mouse.Description, dto.Description);
            Assert.Equal(mouse.Price, dto.Price);
            Assert.Equal(mouse.ImageUrl, dto.ImageUrl);
            Assert.Equal(mouse.Stock, dto.Stock);
        }
    }
}