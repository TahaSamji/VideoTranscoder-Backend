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


        [HttpPost("create-encoding-profiles")]
        public async Task<IActionResult> CreateEncodingProfile([FromBody] EncodingProfile profile)
        {
            Console.WriteLine(profile);
            var result = await _encodingProfileService.CreateProfileAsync(profile);

            return Ok(result);
        }

        [HttpGet("getallEncodings")]
        public async Task<IActionResult> GetAllEncodingProfiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (profiles, totalCount) = await _encodingProfileService.GetAllProfilesAsync(page, pageSize);
            return Ok(new
            {
                items = profiles,
                total = totalCount,
                page,
                pageSize
            });
        }
    }
}