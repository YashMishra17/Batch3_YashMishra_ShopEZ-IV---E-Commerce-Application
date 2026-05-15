using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopEZ.PaymentService.DTOs;
using ShopEZ.PaymentService.Exceptions;
using ShopEZ.PaymentService.Services.Interfaces;

namespace ShopEZ.PaymentService.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> ProcessPayment(
            [FromBody] ProcessPaymentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                PaymentDTO payment = await _paymentService.ProcessPaymentAsync(dto);
                return CreatedAtAction(
                    nameof(GetPaymentById),
                    new { id = payment.PaymentId },
                    new { success = true, data = payment });
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
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                PaymentDTO? payment = await _paymentService.GetByIdAsync(id);

                if (payment is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Payment with ID {id} was not found."
                    });

                return Ok(new { success = true, data = payment });
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

        [HttpGet("order/{orderId:int}")]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> GetPaymentByOrder(int orderId)
        {
            try
            {
                PaymentDTO? payment = await _paymentService.GetByOrderIdAsync(orderId);

                if (payment is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"No payment found for Order {orderId}."
                    });

                return Ok(new { success = true, data = payment });
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
        public async Task<IActionResult> GetPaymentsByUser(int userId)
        {
            try
            {
                IEnumerable<PaymentDTO> payments =
                    await _paymentService.GetByUserIdAsync(userId);
                return Ok(new { success = true, data = payments });
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

        [HttpPost("{id:int}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RefundPayment(
            int id, [FromBody] RefundPaymentDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                PaymentDTO? refunded = await _paymentService.RefundAsync(id, dto);

                if (refunded is null)
                    return NotFound(new
                    {
                        success = false,
                        message = $"Payment with ID {id} was not found."
                    });

                return Ok(new { success = true, data = refunded });
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
