using Microsoft.AspNetCore.Mvc;
using ShopEZ.UserService.DTOs;
using ShopEZ.UserService.Exceptions;
using ShopEZ.UserService.Services.Interfaces;

namespace ShopEZ.UserService.Controllers
{
    /// <summary>
    /// Route: api/auth
    /// Matches monolith route exactly — no frontend change required.
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ─────────────────────────────────────────────────────────────────────
        // POST /api/auth/register
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                AuthResponseDTO response = await _authService.RegisterAsync(dto);
                return StatusCode(201, new { success = true, data = response });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
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
        // POST /api/auth/login
        // ─────────────────────────────────────────────────────────────────────
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState });

            try
            {
                AuthResponseDTO response = await _authService.LoginAsync(dto);
                return Ok(new { success = true, data = response });
            }
            catch (AppException ex)
            {
                return StatusCode(ex.StatusCode, new { success = false, message = ex.Message });
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
