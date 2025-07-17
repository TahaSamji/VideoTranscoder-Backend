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
            Console.WriteLine(profile);
            var result = await _encodingProfileService.CreateProfileAsync(profile);

            return Ok(result);
        }

        [HttpPut("update-encoding-profile/{id}")]
        public async Task<IActionResult> UpdateEncodingProfile(int id, [FromBody] EncodingProfile updatedProfile)
        {
            if (id != updatedProfile.Id)
                return BadRequest("Encoding profile ID mismatch.");

            var result = await _encodingProfileService.UpdateProfileAsync(id, updatedProfile);

            if (result == null)
                return NotFound($"Encoding profile with ID {id} not found.");

            return Ok(result);
        }

        [HttpDelete("delete-encoding-profile/{id}")]
        public async Task<IActionResult> DeleteEncodingProfile(int id)
        {
            var deleted = await _encodingProfileService.DeleteProfileAsync(id);
            if (!deleted)
                return NotFound($"EncodingProfile with ID {id} not found.");

            return Ok($"EncodingProfile with ID {id} has been deleted.");
        }



    }
}
