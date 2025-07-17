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
         private readonly IEncodingProfileService _encodingProfileService;
        private readonly IThumbnailService _thumbnailService;

        public VideoController(IVideoService videoService, IAuthService authService, IThumbnailService thumbnailService, IEncodingProfileService encodingProfileService)
        {
            _videoService = videoService;
            _authService = authService;
            _thumbnailService = thumbnailService;
            _encodingProfileService = encodingProfileService;
        }

        /// <summary>
        /// Completes video merge process and triggers thumbnail generation.
        /// </summary>
        [HttpPost("mergeComplete")]
        public async Task<IActionResult> MergeCompleteAndRequestThumbnailUrl([FromBody] MergeRequestDto request)
        {
            int userId = _authService.GetCurrentUserId(User);

            await _videoService.StoreFileAndGenerateThumbnailsAsync(
                request,
                userId
            );

            return Ok(new
            {
                message = "🎬 Video successfully uploaded and sent for processing."
            });
        }

        /// <summary>
        /// Gets all uploaded videos for the currently authenticated user with pagination.
        /// </summary>
        [HttpGet("my-uploads")]
        public async Task<IActionResult> GetMyUploads([FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            int userId = _authService.GetCurrentUserId(User);

            var pagedVideos = await _videoService.GetAllVideosByUserIdAsync(userId, page, pageSize);

            return Ok(pagedVideos);
        }

        /// <summary>
        /// Gets all completed video renditions for a given file ID.
        /// </summary>
        [HttpGet("get-video-renditions")]
        public async Task<IActionResult> GetRenditions([FromQuery] int fileId)
        {
            var renditions = await _videoService.GetVideoRenditionsByFileIdAsync(fileId);
            if (renditions == null || renditions.Count == 0)
                return NotFound("No completed renditions found for this video.");

            return Ok(renditions);
        }

        /// <summary>
        /// Gets all thumbnails associated with a specific video file ID.
        /// </summary>
        [HttpGet("get-all-video-thumbnails")]
        public async Task<IActionResult> GetAllThumbnails([FromQuery] int fileId)
        {
            List<ThumbnailDto> thumbnails = await _thumbnailService.GetAllThumbnailsAsync(fileId);
            return Ok(thumbnails);
        }

        /// <summary>
        /// Sets a thumbnail as the default for a specific video file.
        /// </summary>
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

         /// <summary>
        /// Retrieves all encoding profiles with pagination support.
        /// </summary>
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
