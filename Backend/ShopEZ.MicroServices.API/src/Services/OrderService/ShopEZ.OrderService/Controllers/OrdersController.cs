using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.OrderService.DTOs;
using ShopEZ.OrderService.Exceptions;
using ShopEZ.OrderService.Services.Interfaces;

namespace ShopEZ.OrderService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                IEnumerable<OrderDTO> orders = await _orderService.GetAllOrdersAsync();
                return Ok(new { success = true, data = orders });
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

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                OrderDTO? order = await _orderService.GetOrderByIdAsync(id);

                if (order is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Order with ID {id} was not found."
                    });

                return Ok(new { success = true, data = order });
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

        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetOrdersByUser(int userId)
        {
            try
            {
                IEnumerable<OrderDTO> orders =
                    await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(new { success = true, data = orders });
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

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                OrderDTO created = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(
                    nameof(GetOrderById),
                    new { id = created.OrderId },
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

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int id, [FromBody] UpdateOrderStatusDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                OrderDTO? updated =
                    await _orderService.UpdateOrderStatusAsync(id, dto.Status);

                if (updated is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Order with ID {id} was not found."
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
    }
}
