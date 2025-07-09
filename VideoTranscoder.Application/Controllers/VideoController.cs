using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IAuthService _authService;

        public VideoController(IVideoService videoService, IAuthService authService)
        {
            _videoService = videoService;
            _authService = authService;
        }

        [HttpPost("mergeComplete")]
        public async Task<IActionResult> MergeCompleteAndRequestThumbnailUrl([FromBody] MergeRequestDto request)
        {
            int userId = _authService.GetCurrentUserId(User);
            Console.WriteLine(userId);

            var thumbnailUrl = await _videoService.StoreFileAndReturnThumbnailUrlAsync(
                request.TotalChunks,
                request.OutputFileName,
                userId,
                request.FileSize,
                request.EncodingId
            );

            return Ok(new { thumbnailUrl });
        }


    }
}
