using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        // ✅ Public endpoint - no token required
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("✅ This is a public endpoint. No token needed.");
        }

        // 🔐 Protected endpoint - token required
        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
            return Ok($"🔐 Hello, {username}. You accessed a protected endpoint!");
        }

        // 🔐 Protected endpoint with role-based claim
        [Authorize]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("🔐 Welcome Admin! This is a role-protected endpoint.");
        }
    }
}
