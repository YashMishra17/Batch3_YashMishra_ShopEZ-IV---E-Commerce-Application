using Microsoft.AspNetCore.Mvc;
using Moq;
using ShopEZ.ProductService.Controllers;
using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Exceptions;
using ShopEZ.ProductService.Services.Interfaces;
using ShopEZ.ProductService.Tests.Helpers;
using Xunit;

namespace ShopEZ.ProductService.Tests.Controllers
{
    public class ProductsControllerGetTests
    {
        private readonly Mock<IProductService> _serviceMock = new();
        private readonly ProductsController _controller;

        public ProductsControllerGetTests()
            => _controller = new ProductsController(_serviceMock.Object);

        // ── helper ────────────────────────────────────────────────────────────
        private static List<ProductDTO> ThreeDTOs() => new()
        {
            ProductTestData.MouseDTO(),
            new ProductDTO { ProductId=2, Name="Mechanical Keyboard",
                Description="RGB", Price=79.99m, Stock=50, ImageUrl="" },
            new ProductDTO { ProductId=3, Name="USB-C Hub",
                Description="7-in-1", Price=49.99m, Stock=75, ImageUrl="" }
        };

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllProducts_ThreeProducts_Returns200WithSuccessTrue()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ReturnsAsync(ThreeDTOs());

            IActionResult result = await _controller.GetAllProducts();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);

            var success = ok.Value!.GetType()
                .GetProperty("success")?.GetValue(ok.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task GetAllProducts_ThreeProducts_DataContainsThreeItems()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ReturnsAsync(ThreeDTOs());

            IActionResult result = await _controller.GetAllProducts();

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType()
                .GetProperty("data")?.GetValue(ok.Value)
                as IEnumerable<ProductDTO>;

            Assert.NotNull(data);
            Assert.Equal(3, data!.Count());
        }

        [Fact]
        public async Task GetAllProducts_EmptyDatabase_Returns200WithEmptyList()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ReturnsAsync(new List<ProductDTO>());

            IActionResult result = await _controller.GetAllProducts();

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType()
                .GetProperty("data")?.GetValue(ok.Value)
                as IEnumerable<ProductDTO>;

            Assert.Empty(data!);
        }

        [Fact]
        public async Task GetAllProducts_ServiceThrowsAppException_ReturnsCorrectStatusCode()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ThrowsAsync(new AppException("DB error.", 500));

            IActionResult result = await _controller.GetAllProducts();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetAllProducts_ServiceThrowsGenericException_Returns500()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ThrowsAsync(new Exception("Unexpected"));

            IActionResult result = await _controller.GetAllProducts();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        [Fact]
        public async Task GetAllProducts_CallsServiceOnce()
        {
            _serviceMock.Setup(s => s.GetAllProductsAsync())
                        .ReturnsAsync(new List<ProductDTO>());

            await _controller.GetAllProducts();

            _serviceMock.Verify(s => s.GetAllProductsAsync(), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetProductById_ExistingId_Returns200()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(1))
                        .ReturnsAsync(ProductTestData.MouseDTO());

            IActionResult result = await _controller.GetProductById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task GetProductById_ExistingId_DataNotNull()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(1))
                        .ReturnsAsync(ProductTestData.MouseDTO());

            IActionResult result = await _controller.GetProductById(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType()
                .GetProperty("data")?.GetValue(ok.Value);
            Assert.NotNull(data);
        }

        [Fact]
        public async Task GetProductById_NonExistentId_Returns404()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(9999))
                        .ReturnsAsync((ProductDTO?)null);

            IActionResult result = await _controller.GetProductById(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task GetProductById_NonExistentId_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(9999))
                        .ReturnsAsync((ProductDTO?)null);

            IActionResult result = await _controller.GetProductById(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType()
                .GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task GetProductById_AppException400_Returns400()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(0))
                        .ThrowsAsync(new AppException(
                            "Product ID must be a positive integer.", 400));

            IActionResult result = await _controller.GetProductById(0);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task GetProductById_GenericException_Returns500()
        {
            _serviceMock.Setup(s => s.GetProductByIdAsync(1))
                        .ThrowsAsync(new Exception("DB timeout"));

            IActionResult result = await _controller.GetProductById(1);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products/search
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SearchProducts_DefaultSearch_Returns200()
        {
            var pagedResult = new PagedResultDTO<ProductDTO>
            {
                Items = ThreeDTOs(),
                TotalCount = 3,
                Page = 1,
                PageSize = 10
            };

            _serviceMock.Setup(s => s.SearchProductsAsync(
                    It.IsAny<ProductSearchDTO>()))
                        .ReturnsAsync(pagedResult);

            IActionResult result = await _controller.SearchProducts(
                ProductTestData.DefaultSearch());

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task SearchProducts_EmptyResults_Returns200WithEmptyItems()
        {
            var pagedResult = new PagedResultDTO<ProductDTO>
            {
                Items = new List<ProductDTO>(),
                TotalCount = 0,
                Page = 1,
                PageSize = 10
            };

            _serviceMock.Setup(s => s.SearchProductsAsync(
                    It.IsAny<ProductSearchDTO>()))
                        .ReturnsAsync(pagedResult);

            IActionResult result = await _controller.SearchProducts(
                ProductTestData.KeywordSearch("zzznomatch"));

            var ok = Assert.IsType<OkObjectResult>(result);
            var data = ok.Value!.GetType()
                .GetProperty("data")?.GetValue(ok.Value)
                as PagedResultDTO<ProductDTO>;
            Assert.Empty(data!.Items);
        }

        [Fact]
        public async Task SearchProducts_InvalidPriceRange_Returns400()
        {
            _serviceMock.Setup(s => s.SearchProductsAsync(
                    It.IsAny<ProductSearchDTO>()))
                        .ThrowsAsync(new AppException(
                            "MinPrice cannot be greater than MaxPrice.", 400));

            IActionResult result = await _controller.SearchProducts(
                ProductTestData.InvalidPriceRangeSearch());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task SearchProducts_InvalidModelState_Returns400WithoutCallingService()
        {
            _controller.ModelState.AddModelError(
                "PageSize", "PageSize must be between 1 and 100.");

            IActionResult result = await _controller.SearchProducts(
                new ProductSearchDTO { Page = 1, PageSize = 9999 });

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);
            _serviceMock.Verify(s => s.SearchProductsAsync(
                It.IsAny<ProductSearchDTO>()), Times.Never);
        }

        [Fact]
        public async Task SearchProducts_GenericException_Returns500()
        {
            _serviceMock.Setup(s => s.SearchProductsAsync(
                    It.IsAny<ProductSearchDTO>()))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.SearchProducts(
                ProductTestData.DefaultSearch());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
