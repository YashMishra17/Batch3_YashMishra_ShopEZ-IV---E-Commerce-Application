using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Exceptions;
using ShopEZ.ProductService.Services.Interfaces;

namespace ShopEZ.ProductService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                IEnumerable<ProductDTO> products =
                    await _productService.GetAllProductsAsync();
                return Ok(new { success = true, data = products });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                ProductDTO? product = await _productService.GetProductByIdAsync(id);

                if (product is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} was not found."
                    });

                return Ok(new { success = true, data = product });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET /api/products/search
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet("search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] ProductSearchDTO searchDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                PagedResultDTO<ProductDTO> result =
                    await _productService.SearchProductsAsync(searchDto);
                return Ok(new { success = true, data = result });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/products
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                ProductDTO created = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = created.ProductId },
                    new { success = true, data = created });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // PUT /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromBody] UpdateProductDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                ProductDTO? updated = await _productService.UpdateProductAsync(id, dto);

                if (updated is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} was not found."
                    });

                return Ok(new { success = true, data = updated });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE /api/products/{id}
        // ─────────────────────────────────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                bool deleted = await _productService.DeleteProductAsync(id);

                if (!deleted)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Product with ID {id} was not found."
                    });

                return Ok(new
                {
                    success = true,
                    message = $"Product with ID {id} was deleted successfully."
                });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode,
                    new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred.",
                    detail = ex.Message
                });
            }
        }
    }
}