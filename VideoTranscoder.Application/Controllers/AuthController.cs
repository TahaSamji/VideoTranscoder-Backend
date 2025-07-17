using Microsoft.AspNetCore.Mvc;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user with the provided signup details.
        /// </summary>
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpDto dto)
        {
            try
            {
                var result = await _authService.SignUpAsync(dto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Thrown when input validation fails (e.g., email already exists)
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Fallback for any other unhandled exception
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Logs in a user with the provided credentials.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Thrown when credentials are incorrect
                return Unauthorized(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // Thrown when input data is missing or invalid
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Fallback for unexpected errors
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
