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
    public class ProductsControllerMutationTests
    {
        private readonly Mock<IProductService> _serviceMock = new();
        private readonly ProductsController _controller;

        public ProductsControllerMutationTests()
            => _controller = new ProductsController(_serviceMock.Object);

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/products
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateProduct_ValidDto_Returns201WithSuccessTrue()
        {
            CreateProductDTO dto = ProductTestData.ValidCreateDTO();
            ProductDTO created = new()
            {
                ProductId = 5,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                Stock = dto.Stock
            };

            _serviceMock.Setup(s => s.CreateProductAsync(dto)).ReturnsAsync(created);

            IActionResult result = await _controller.CreateProduct(dto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdAt.StatusCode);

            var success = createdAt.Value!.GetType()
                .GetProperty("success")?.GetValue(createdAt.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task CreateProduct_ValidDto_LocationHeaderPointsToGetById()
        {
            CreateProductDTO dto = ProductTestData.ValidCreateDTO();
            ProductDTO created = new() { ProductId = 5, Name = dto.Name, Price = dto.Price, Stock = dto.Stock };

            _serviceMock.Setup(s => s.CreateProductAsync(dto)).ReturnsAsync(created);

            IActionResult result = await _controller.CreateProduct(dto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ProductsController.GetProductById), createdAt.ActionName);
            Assert.Equal(5, createdAt.RouteValues!["id"]);
        }

        [Fact]
        public async Task CreateProduct_ZeroPrice_Returns400()
        {
            _serviceMock.Setup(s => s.CreateProductAsync(
                    It.IsAny<CreateProductDTO>()))
                        .ThrowsAsync(new AppException(
                            "Product price must be greater than zero.", 400));

            IActionResult result = await _controller.CreateProduct(
                ProductTestData.CreateDTOWithZeroPrice());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_EmptyName_Returns400()
        {
            _serviceMock.Setup(s => s.CreateProductAsync(
                    It.IsAny<CreateProductDTO>()))
                        .ThrowsAsync(new AppException(
                            "Product name is required.", 400));

            IActionResult result = await _controller.CreateProduct(
                ProductTestData.CreateDTOWithEmptyName());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task CreateProduct_InvalidModelState_Returns400WithoutCallingService()
        {
            _controller.ModelState.AddModelError("Price", "Price must be > 0.");

            IActionResult result = await _controller.CreateProduct(
                new CreateProductDTO());

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, bad.StatusCode);

            _serviceMock.Verify(s => s.CreateProductAsync(
                It.IsAny<CreateProductDTO>()), Times.Never);
        }

        [Fact]
        public async Task CreateProduct_AppException_ResponseContainsMessage()
        {
            const string errorMsg = "Product name is required.";

            _serviceMock.Setup(s => s.CreateProductAsync(
                    It.IsAny<CreateProductDTO>()))
                        .ThrowsAsync(new AppException(errorMsg, 400));

            IActionResult result = await _controller.CreateProduct(
                ProductTestData.CreateDTOWithEmptyName());

            var obj = Assert.IsType<ObjectResult>(result);
            var message = obj.Value!.GetType()
                .GetProperty("message")?.GetValue(obj.Value);
            Assert.Equal(errorMsg, message);
        }

        [Fact]
        public async Task CreateProduct_GenericException_Returns500()
        {
            _serviceMock.Setup(s => s.CreateProductAsync(
                    It.IsAny<CreateProductDTO>()))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.CreateProduct(
                ProductTestData.ValidCreateDTO());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateProduct_ExistingId_Returns200()
        {
            UpdateProductDTO dto = ProductTestData.ValidUpdateDTO();
            ProductDTO updated = new()
            {
                ProductId = 1,
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            _serviceMock.Setup(s => s.UpdateProductAsync(1, dto))
                        .ReturnsAsync(updated);

            IActionResult result = await _controller.UpdateProduct(1, dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_NonExistentId_Returns404()
        {
            _serviceMock.Setup(s => s.UpdateProductAsync(9999,
                    It.IsAny<UpdateProductDTO>()))
                        .ReturnsAsync((ProductDTO?)null);

            IActionResult result = await _controller.UpdateProduct(
                9999, ProductTestData.ValidUpdateDTO());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_NonExistentId_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.UpdateProductAsync(9999,
                    It.IsAny<UpdateProductDTO>()))
                        .ReturnsAsync((ProductDTO?)null);

            IActionResult result = await _controller.UpdateProduct(
                9999, ProductTestData.ValidUpdateDTO());

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType()
                .GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task UpdateProduct_ValidationError_Returns400()
        {
            _serviceMock.Setup(s => s.UpdateProductAsync(
                    1, It.IsAny<UpdateProductDTO>()))
                        .ThrowsAsync(new AppException(
                            "Product price must be greater than zero.", 400));

            var dto = new UpdateProductDTO
            {
                Name = "Test",
                Price = 0m,
                Stock = 10
            };

            IActionResult result = await _controller.UpdateProduct(1, dto);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_InvalidModelState_Returns400WithoutCallingService()
        {
            _controller.ModelState.AddModelError("Price", "Price must be > 0.");

            IActionResult result = await _controller.UpdateProduct(
                1, new UpdateProductDTO());

            Assert.IsType<BadRequestObjectResult>(result);
            _serviceMock.Verify(s => s.UpdateProductAsync(
                It.IsAny<int>(), It.IsAny<UpdateProductDTO>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProduct_GenericException_Returns500()
        {
            _serviceMock.Setup(s => s.UpdateProductAsync(
                    1, It.IsAny<UpdateProductDTO>()))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.UpdateProduct(
                1, ProductTestData.ValidUpdateDTO());

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteProduct_ExistingId_Returns200WithSuccessTrue()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync(true);

            IActionResult result = await _controller.DeleteProduct(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);

            var success = ok.Value!.GetType()
                .GetProperty("success")?.GetValue(ok.Value);
            Assert.Equal(true, success);
        }

        [Fact]
        public async Task DeleteProduct_NonExistentId_Returns404()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(9999)).ReturnsAsync(false);

            IActionResult result = await _controller.DeleteProduct(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_NonExistentId_SuccessIsFalse()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(9999)).ReturnsAsync(false);

            IActionResult result = await _controller.DeleteProduct(9999);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var success = notFound.Value!.GetType()
                .GetProperty("success")?.GetValue(notFound.Value);
            Assert.Equal(false, success);
        }

        [Fact]
        public async Task DeleteProduct_AppException400_Returns400()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(0))
                        .ThrowsAsync(new AppException(
                            "Product ID must be a positive integer.", 400));

            IActionResult result = await _controller.DeleteProduct(0);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, obj.StatusCode);
        }

        [Fact]
        public async Task DeleteProduct_ValidId_CallsServiceOnce()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync(true);

            await _controller.DeleteProduct(1);

            _serviceMock.Verify(s => s.DeleteProductAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_GenericException_Returns500()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(1))
                        .ThrowsAsync(new Exception("DB error"));

            IActionResult result = await _controller.DeleteProduct(1);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Response envelope shape
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateProduct_SuccessResponse_HasSuccessAndDataFields()
        {
            _serviceMock.Setup(s => s.CreateProductAsync(
                    It.IsAny<CreateProductDTO>()))
                        .ReturnsAsync(new ProductDTO
                        {
                            ProductId = 5,
                            Name = "X",
                            Price = 1m,
                            Stock = 1
                        });

            var result = await _controller.CreateProduct(
                ProductTestData.ValidCreateDTO()) as CreatedAtActionResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("data"));
        }

        [Fact]
        public async Task DeleteProduct_SuccessResponse_HasSuccessAndMessageFields()
        {
            _serviceMock.Setup(s => s.DeleteProductAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteProduct(1) as OkObjectResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("message"));
        }

        [Fact]
        public async Task UpdateProduct_FailureResponse_HasSuccessAndMessageFields()
        {
            _serviceMock.Setup(s => s.UpdateProductAsync(
                    It.IsAny<int>(), It.IsAny<UpdateProductDTO>()))
                        .ThrowsAsync(new AppException("Error.", 400));

            var result = await _controller.UpdateProduct(
                1, ProductTestData.ValidUpdateDTO()) as ObjectResult;

            var type = result!.Value!.GetType();
            Assert.NotNull(type.GetProperty("success"));
            Assert.NotNull(type.GetProperty("message"));
        }
    }
}