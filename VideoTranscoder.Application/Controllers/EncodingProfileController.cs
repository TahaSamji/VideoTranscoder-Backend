using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EncodingProfileController : ControllerBase
    {
        private readonly IEncodingProfileService _encodingProfileService;

        public EncodingProfileController(IEncodingProfileService encodingProfileService)
        {
            _encodingProfileService = encodingProfileService;
        }

        /// <summary>
        /// Creates a new encoding profile.
        /// </summary>
        [HttpPost("create-encoding-profiles")]
        public async Task<IActionResult> CreateEncodingProfile([FromBody] EncodingProfile profile)
        {
            try
            {

                var result = await _encodingProfileService.CreateProfileAsync(profile);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Handle domain-level custom exceptions
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpPut("update-encoding-profile/{id}")]
        public async Task<IActionResult> UpdateEncodingProfile(int id, [FromBody] EncodingProfile updatedProfile)
        {
            try
            {
                if (id != updatedProfile.Id)
                    return BadRequest("Encoding profile ID mismatch."); // ID validation

                var result = await _encodingProfileService.UpdateProfileAsync(id, updatedProfile);

                if (result == null)
                    return NotFound($"Encoding profile with ID {id} not found.");

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                // Handle domain-level custom exceptions
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpDelete("delete-encoding-profile/{id}")]
        public async Task<IActionResult> DeleteEncodingProfile(int id)
        {
            try
            {
                var deleted = await _encodingProfileService.DeleteProfileAsync(id);

                if (!deleted)
                    return NotFound($"EncodingProfile with ID {id} not found.");

                return Ok($"EncodingProfile with ID {id} has been deleted.");
            }
            catch (ArgumentException ex)
            {
                // Handle domain-level custom exceptions
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }

        [HttpGet("updateAdminSelection")]
        public async Task<IActionResult> UpdateAdminSelectionAsync([FromQuery] int profileId, [FromQuery] bool isSelected)
        {
            try
            {
                await _encodingProfileService.UpdateSelectionAsync(profileId, isSelected);

                return Ok(new { message = "Selection updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
