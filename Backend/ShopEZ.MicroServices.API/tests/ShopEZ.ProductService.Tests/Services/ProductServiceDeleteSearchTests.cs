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
    public class ProductServiceDeleteSearchTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();
        private readonly ProductServiceClass _service;

        public ProductServiceDeleteSearchTests()
            => _service = new ProductServiceClass(_repoMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // DeleteProductAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteProductAsync_ExistingId_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            bool result = await _service.DeleteProductAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProductAsync_NonExistentId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.DeleteAsync(9999)).ReturnsAsync(false);

            bool result = await _service.DeleteProductAsync(9999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProductAsync_ZeroId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.DeleteProductAsync(0));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteProductAsync_NegativeId_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.DeleteProductAsync(-5));

            Assert.Equal(400, ex.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(int.MinValue)]
        public async Task DeleteProductAsync_InvalidIds_AlwaysThrow400(int id)
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.DeleteProductAsync(id));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task DeleteProductAsync_ValidId_CallsDeleteOnce()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            await _service.DeleteProductAsync(1);

            _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SearchProductsAsync — Dapper WHERE clause edge cases
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchProductsAsync_DefaultSearch_ReturnsPagedResult()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)ProductTestData.ThreeProducts(),
                         TotalCount: 3));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(ProductTestData.DefaultSearch());

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count());
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task SearchProductsAsync_EmptyResults_ReturnEmptyPagedResult()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync(((IEnumerable<Product>)new List<Product>(), TotalCount: 0));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.KeywordSearch("zzznomatch"));

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public async Task SearchProductsAsync_InvalidPriceRange_ThrowsAppException400()
        {
            // MinPrice > MaxPrice — invalid filter
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.SearchProductsAsync(
                    ProductTestData.InvalidPriceRangeSearch()));

            Assert.Equal(400, ex.StatusCode);
            Assert.Contains("MinPrice", ex.Message, StringComparison.OrdinalIgnoreCase);
            _repoMock.Verify(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()), Times.Never);
        }

        [Fact]
        public async Task SearchProductsAsync_NullDto_ThrowsAppException400()
        {
            var ex = await Assert.ThrowsAsync<AppException>(
                () => _service.SearchProductsAsync(null!));

            Assert.Equal(400, ex.StatusCode);
            _repoMock.Verify(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()), Times.Never);
        }

        [Fact]
        public async Task SearchProductsAsync_TotalPagedCalculatedCorrectly()
        {
            // 3 total, pageSize 2 → 2 pages
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)new List<Product>
                         {
                             ProductTestData.Mouse(),
                             ProductTestData.Keyboard()
                         },
                         TotalCount: 3));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.PaginatedSearch(1, 2));

            Assert.Equal(2, result.TotalPages);
            Assert.True(result.HasNext);
            Assert.False(result.HasPrevious);
        }

        [Fact]
        public async Task SearchProductsAsync_LastPage_HasNextFalseHasPreviousTrue()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)new List<Product>
                         {
                             ProductTestData.Hub()
                         },
                         TotalCount: 3));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.PaginatedSearch(2, 2));

            Assert.False(result.HasNext);
            Assert.True(result.HasPrevious);
        }

        [Fact]
        public async Task SearchProductsAsync_SinglePage_BothHasNextAndHasPreviousFalse()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)ProductTestData.ThreeProducts(),
                         TotalCount: 3));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.PaginatedSearch(1, 10));

            Assert.False(result.HasNext);
            Assert.False(result.HasPrevious);
        }

        [Fact]
        public async Task SearchProductsAsync_KeywordSearch_PassesKeywordToRepository()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)new List<Product> { ProductTestData.Mouse() },
                         TotalCount: 1));

            await _service.SearchProductsAsync(
                ProductTestData.KeywordSearch("mouse"));

            _repoMock.Verify(r => r.SearchAsync(
                It.Is<ProductSearchDTO>(dto => dto.Keyword == "mouse")), Times.Once);
        }

        [Fact]
        public async Task SearchProductsAsync_PriceRangeSearch_ItemsMappedToDTO()
        {
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)new List<Product>
                         {
                             ProductTestData.Mouse(),
                             ProductTestData.Hub()
                         },
                         TotalCount: 2));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.PriceRangeSearch(20m, 60m));

            Assert.Equal(2, result.Items.Count());
            Assert.All(result.Items, dto =>
            {
                Assert.True(dto.Price >= 20m);
                Assert.True(dto.Price <= 60m);
            });
        }

        [Fact]
        public async Task SearchProductsAsync_EqualMinAndMaxPrice_IsValid()
        {
            // MinPrice == MaxPrice is a valid edge case (exact price match)
            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync((
                         (IEnumerable<Product>)new List<Product> { ProductTestData.Mouse() },
                         TotalCount: 1));

            PagedResultDTO<ProductDTO> result =
                await _service.SearchProductsAsync(
                    ProductTestData.PriceRangeSearch(29.99m, 29.99m));

            Assert.Single(result.Items);
        }
    }
}

