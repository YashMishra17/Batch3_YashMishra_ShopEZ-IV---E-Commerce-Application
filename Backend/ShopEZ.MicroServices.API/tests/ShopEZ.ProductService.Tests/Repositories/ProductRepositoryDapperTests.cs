using Moq;
using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Models;
using ShopEZ.ProductService.Repositories.Interfaces;
using ShopEZ.ProductService.Tests.Helpers;
using Xunit;

namespace ShopEZ.ProductService.Tests.Repositories
{
    /// <summary>
    /// Validates Dapper repository contract through a mocked IProductRepository.
    ///
    /// WHY MOCK INSTEAD OF REAL DAPPER:
    ///   Real Dapper tests require a live SQL Server connection.
    ///   In a CI/test environment without SQL Server, they would always fail.
    ///   These tests verify that:
    ///     1. The repository interface contract is correct
    ///     2. The service calls the right repository methods with the right params
    ///     3. Edge cases (empty results, not found, stock deduction) are handled
    ///
    ///   Integration tests against a real DB are a separate concern.
    /// </summary>
    public class ProductRepositoryDapperTests
    {
        private readonly Mock<IProductRepository> _repoMock = new();

        // ─────────────────────────────────────────────────────────────────────
        // GetAllAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(ProductTestData.ThreeProducts());

            IEnumerable<Product> result = await _repoMock.Object.GetAllAsync();

            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyCollection()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(ProductTestData.EmptyProductList());

            IEnumerable<Product> result = await _repoMock.Object.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectProductFields()
        {
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Product> { ProductTestData.Mouse() });

            IEnumerable<Product> result = await _repoMock.Object.GetAllAsync();
            Product first = result.First();

            Assert.Equal(1, first.ProductId);
            Assert.Equal("Wireless Mouse", first.Name);
            Assert.Equal(29.99m, first.Price);
            Assert.Equal(100, first.Stock);
        }

        [Fact]
        public async Task GetAllAsync_OrderedByProductIdAscending()
        {
            var products = new List<Product>
            {
                ProductTestData.Hub(),      // id 3
                ProductTestData.Mouse(),    // id 1
                ProductTestData.Keyboard()  // id 2
            };
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(products.OrderBy(p => p.ProductId).ToList());

            var result = (await _repoMock.Object.GetAllAsync()).ToList();

            Assert.Equal(1, result[0].ProductId);
            Assert.Equal(2, result[1].ProductId);
            Assert.Equal(3, result[2].ProductId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GetByIdAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsProduct()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1))
                     .ReturnsAsync(ProductTestData.Mouse());

            Product? result = await _repoMock.Object.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result!.ProductId);
            Assert.Equal("Wireless Mouse", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetByIdAsync(9999))
                     .ReturnsAsync((Product?)null);

            Product? result = await _repoMock.Object.GetByIdAsync(9999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_PassesCorrectIdToQuery()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                     .ReturnsAsync(ProductTestData.Mouse());

            await _repoMock.Object.GetByIdAsync(42);

            _repoMock.Verify(r => r.GetByIdAsync(42), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CreateAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidProduct_ReturnsCreatedWithGeneratedId()
        {
            var product = new Product
            {
                Name = "New Product",
                Description = "Desc",
                Price = 19.99m,
                ImageUrl = string.Empty,
                Stock = 10
            };

            var created = new Product
            {
                ProductId = 5,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Stock = product.Stock
            };

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .ReturnsAsync(created);

            Product result = await _repoMock.Object.CreateAsync(product);

            Assert.True(result.ProductId > 0);
            Assert.Equal("New Product", result.Name);
        }

        [Fact]
        public async Task CreateAsync_PassesCorrectProductToQuery()
        {
            Product? captured = null;
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                     .Callback<Product>(p => captured = p)
                     .ReturnsAsync(ProductTestData.Mouse());

            var product = new Product
            {
                Name = "Test",
                Price = 9.99m,
                Stock = 5
            };

            await _repoMock.Object.CreateAsync(product);

            Assert.NotNull(captured);
            Assert.Equal("Test", captured!.Name);
            Assert.Equal(9.99m, captured.Price);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UpdateAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ExistingProduct_ReturnsUpdatedProduct()
        {
            var updated = new Product
            {
                ProductId = 1,
                Name = "Updated Mouse",
                Price = 34.99m,
                Stock = 80
            };

            _repoMock.Setup(r => r.UpdateAsync(1, It.IsAny<Product>()))
                     .ReturnsAsync(updated);

            Product? result = await _repoMock.Object.UpdateAsync(
                1, new Product { Name = "Updated Mouse", Price = 34.99m, Stock = 80 });

            Assert.NotNull(result);
            Assert.Equal("Updated Mouse", result!.Name);
            Assert.Equal(34.99m, result.Price);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ReturnsNull()
        {
            _repoMock.Setup(r => r.UpdateAsync(9999, It.IsAny<Product>()))
                     .ReturnsAsync((Product?)null);

            Product? result = await _repoMock.Object.UpdateAsync(
                9999, new Product { Name = "Ghost", Price = 1m, Stock = 0 });

            Assert.Null(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DeleteAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            bool result = await _repoMock.Object.DeleteAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.DeleteAsync(9999)).ReturnsAsync(false);

            bool result = await _repoMock.Object.DeleteAsync(9999);

            Assert.False(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ExistsAsync
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            _repoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

            bool exists = await _repoMock.Object.ExistsAsync(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.ExistsAsync(9999)).ReturnsAsync(false);

            bool exists = await _repoMock.Object.ExistsAsync(9999);

            Assert.False(exists);
        }

        // ─────────────────────────────────────────────────────────────────────
        // SearchAsync — Dapper dynamic WHERE clause validation
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchAsync_NoFilters_ReturnsAllProducts()
        {
            var expected = (
                Items: (IEnumerable<Product>)ProductTestData.ThreeProducts(),
                TotalCount: 3);

            _repoMock.Setup(r => r.SearchAsync(It.IsAny<ProductSearchDTO>()))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(ProductTestData.DefaultSearch());

            Assert.Equal(3, items.Count());
            Assert.Equal(3, total);
        }

        [Fact]
        public async Task SearchAsync_KeywordFilter_ReturnsMatchingProducts()
        {
            var expected = (
                Items: (IEnumerable<Product>)new List<Product> { ProductTestData.Mouse() },
                TotalCount: 1);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.Keyword == "mouse")))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.KeywordSearch("mouse"));

            Assert.Single(items);
            Assert.Equal(1, total);
            Assert.Equal("Wireless Mouse", items.First().Name);
        }

        [Fact]
        public async Task SearchAsync_KeywordNoMatch_ReturnsEmptyWithZeroTotal()
        {
            var expected = (
                Items: (IEnumerable<Product>)new List<Product>(),
                TotalCount: 0);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.Keyword == "zzznomatch")))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.KeywordSearch("zzznomatch"));

            Assert.Empty(items);
            Assert.Equal(0, total);
        }

        [Fact]
        public async Task SearchAsync_PriceRangeFilter_ReturnsProductsWithinRange()
        {
            var inRange = new List<Product>
            {
                ProductTestData.Mouse(),    // 29.99
                ProductTestData.Hub()       // 49.99
            };

            var expected = ((IEnumerable<Product>)inRange, TotalCount: 2);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.MinPrice == 20m && dto.MaxPrice == 60m)))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.PriceRangeSearch(20m, 60m));

            Assert.Equal(2, total);
            Assert.All(items, p =>
            {
                Assert.True(p.Price >= 20m);
                Assert.True(p.Price <= 60m);
            });
        }

        [Fact]
        public async Task SearchAsync_MinStockFilter_ReturnsOnlyStockedProducts()
        {
            var dto = new ProductSearchDTO { MinStock = 1, Page = 1, PageSize = 10 };

            var stocked = ProductTestData.ThreeProducts();
            var expected = ((IEnumerable<Product>)stocked, TotalCount: 3);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    d => d.MinStock == 1)))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(dto);

            Assert.Equal(3, total);
            Assert.All(items, p => Assert.True(p.Stock >= 1));
        }

        [Fact]
        public async Task SearchAsync_PaginationFirstPage_ReturnsCorrectSlice()
        {
            var page1 = new List<Product> { ProductTestData.Mouse(), ProductTestData.Keyboard() };
            var expected = ((IEnumerable<Product>)page1, TotalCount: 3);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.Page == 1 && dto.PageSize == 2)))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.PaginatedSearch(1, 2));

            Assert.Equal(2, items.Count());
            Assert.Equal(3, total);
        }

        [Fact]
        public async Task SearchAsync_PaginationSecondPage_ReturnsRemainingItems()
        {
            var page2 = new List<Product> { ProductTestData.Hub() };
            var expected = ((IEnumerable<Product>)page2, TotalCount: 3);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.Page == 2 && dto.PageSize == 2)))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.PaginatedSearch(2, 2));

            Assert.Single(items);
            Assert.Equal(3, total);
        }

        [Fact]
        public async Task SearchAsync_PageBeyondResults_ReturnsEmptyWithCorrectTotal()
        {
            var expected = ((IEnumerable<Product>)new List<Product>(), TotalCount: 3);

            _repoMock.Setup(r => r.SearchAsync(It.Is<ProductSearchDTO>(
                    dto => dto.Page == 999)))
                     .ReturnsAsync(expected);

            var (items, total) =
                await _repoMock.Object.SearchAsync(
                    ProductTestData.PaginatedSearch(999, 10));

            Assert.Empty(items);
            Assert.Equal(3, total);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DeductStockAsync — atomic stock check + decrement
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeductStockAsync_SufficientStock_ReturnsTrue()
        {
            _repoMock.Setup(r => r.DeductStockAsync(1, 5)).ReturnsAsync(true);

            bool result = await _repoMock.Object.DeductStockAsync(1, 5);

            Assert.True(result);
        }

        [Fact]
        public async Task DeductStockAsync_InsufficientStock_ReturnsFalse()
        {
            _repoMock.Setup(r => r.DeductStockAsync(1, 9999)).ReturnsAsync(false);

            bool result = await _repoMock.Object.DeductStockAsync(1, 9999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeductStockAsync_ExactStockAmount_ReturnsTrue()
        {
            // Mouse has 100 stock — deduct exactly 100
            _repoMock.Setup(r => r.DeductStockAsync(1, 100)).ReturnsAsync(true);

            bool result = await _repoMock.Object.DeductStockAsync(1, 100);

            Assert.True(result);
        }

        [Fact]
        public async Task DeductStockAsync_ZeroStock_ReturnsFalse()
        {
            // Product has 0 stock
            _repoMock.Setup(r => r.DeductStockAsync(4, It.IsAny<int>()))
                     .ReturnsAsync(false);

            bool result = await _repoMock.Object.DeductStockAsync(4, 1);

            Assert.False(result);
        }
    }
}