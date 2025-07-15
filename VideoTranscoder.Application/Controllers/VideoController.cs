using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;
namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly IAuthService _authService;

        private readonly IThumbnailService _thumbnailService;

        public VideoController(IVideoService videoService, IAuthService authService, IThumbnailService thumbnailService)
        {
            _videoService = videoService;
            _authService = authService;
            _thumbnailService = thumbnailService;
        }

        [HttpPost("mergeComplete")]
        public async Task<IActionResult> MergeCompleteAndRequestThumbnailUrl([FromBody] MergeRequestDto request)
        {
            int userId = _authService.GetCurrentUserId(User);
            // Console.WriteLine(userId);

            var thumbnailUrl = await _videoService.StoreFileAndReturnThumbnailUrlAsync(
                request.TotalChunks,
                request.OutputFileName,
                userId,
                request.FileSize,
                request.EncodingId
            );

            return Ok(new { thumbnailUrl });
        }
        [HttpGet("my-uploads")]
        public async Task<IActionResult> GetMyUploads([FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            int userId = _authService.GetCurrentUserId(User);

            var pagedVideos = await _videoService.GetAllVideosByUserIdAsync(userId, page, pageSize);

            return Ok(pagedVideos);
        }

        [HttpGet("get-video-renditions")]
        public async Task<IActionResult> GetRenditions([FromQuery] int fileId)
        {
            var renditions = await _videoService.GetVideoRenditionsByFileIdAsync(fileId);
            if (renditions == null || renditions.Count == 0)
                return NotFound("No completed renditions found for this video.");

            return Ok(renditions);
        }
        [HttpGet("get-all-video-thumbnails")]
        public async Task<IActionResult> GetAllThumbnails([FromQuery] int fileId)
        {
            List<ThumbnailDto> thumbnails = await _thumbnailService.GetAllThumbnailsAsync(fileId);
            return Ok(thumbnails);
        }

        [HttpPost("set-default-thumbnail")]
        public async Task<IActionResult> SetDefaultThumbnail(

       [FromQuery] int thumbnailId,
       [FromQuery] int fileId)
        {
            try
            {
                await _thumbnailService.SetDefaultThumbnailAsync(thumbnailId, fileId);
                return Ok(new { message = "Default thumbnail updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        

    }




}

