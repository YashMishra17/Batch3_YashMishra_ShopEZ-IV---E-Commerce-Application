using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.CartService.DTOs;
using ShopEZ.CartService.Exceptions;
using ShopEZ.CartService.Services.Interfaces;

namespace ShopEZ.CartService.Controllers
{
    [Route("api/cart")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                CartDTO cart = await _cartService.GetCartAsync(userId);
                return Ok(new { success = true, data = cart });
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

        [HttpPost("{userId:int}/items")]
        public async Task<IActionResult> AddItem(
            int userId, [FromBody] CartItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                CartDTO cart = await _cartService.AddItemAsync(userId, dto);
                return Ok(new { success = true, data = cart });
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

        [HttpPut("{userId:int}/items/{productId:int}")]
        public async Task<IActionResult> UpdateItem(
            int userId, int productId, [FromBody] UpdateCartItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                CartDTO cart = await _cartService.UpdateItemAsync(
                    userId, productId, dto);
                return Ok(new { success = true, data = cart });
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

        [HttpDelete("{userId:int}/items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(int userId, int productId)
        {
            try
            {
                CartDTO cart = await _cartService.RemoveItemAsync(userId, productId);
                return Ok(new { success = true, data = cart });
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

        [HttpDelete("{userId:int}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                await _cartService.ClearCartAsync(userId);
                return Ok(new
                {
                    success = true,
                    message = $"Cart for user {userId} was cleared successfully."
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
