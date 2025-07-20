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
        private readonly ILogger<VideoController> _logger;

        public VideoController(IVideoService videoService, ILogger<VideoController> logger, IAuthService authService, IThumbnailService thumbnailService, IEncodingProfileService encodingProfileService)
        {
            _videoService = videoService;
            _authService = authService;
            _thumbnailService = thumbnailService;
            _encodingProfileService = encodingProfileService;
            _logger = logger;
        }

        /// <summary>
        /// Completes video merge process and triggers thumbnail generation.
        /// </summary>
        [HttpPost("mergeComplete")]
        public async Task<IActionResult> MergeCompleteAndRequestThumbnailUrl([FromBody] MergeRequestDto request)
        {
            try
            {
                int userId = _authService.GetCurrentUserId(User);

                await _videoService.StoreFileAndGenerateThumbnailsAsync(request, userId);

                return Ok(new
                {
                    message = " Video successfully uploaded and sent for processing."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = "Unauthorized. Please log in again."
                });
            }
            catch (VideoAlreadyExistsException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = $" Invalid input: {ex.Message}"
                });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }


        /// <summary>
        /// Gets all uploaded videos for the currently authenticated user with pagination.
        /// </summary>
        [HttpGet("my-uploads")]
        public async Task<IActionResult> GetMyUploads([FromQuery] int page = 1, [FromQuery] int pageSize = 6)
        {
            try
            {
                // Get the currently authenticated user's ID
                int userId = _authService.GetCurrentUserId(User);

                // Retrieve paginated videos uploaded by the user
                var pagedVideos = await _videoService.GetAllVideosByUserIdAsync(userId, page, pageSize);

                // Return the videos in response
                return Ok(pagedVideos);
            }
            catch (UnauthorizedAccessException ex)
            {
                // If user is unauthorized
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // If request parameters are invalid
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // For any unexpected errors
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /// <summary>
        /// Gets all completed video renditions for a given file ID.
        /// </summary>
        [HttpGet("get-video-renditions")]
        public async Task<IActionResult> GetRenditions([FromQuery] int fileId)
        {
            try
            {
                var renditions = await _videoService.GetVideoRenditionsByFileIdAsync(fileId);
                return Ok(renditions);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /// <summary>
        /// Gets all thumbnails associated with a specific video file ID.
        /// </summary>
        [HttpGet("get-all-video-thumbnails")]
        public async Task<IActionResult> GetAllThumbnails([FromQuery] int fileId)
        {
            try
            {
                var thumbnails = await _thumbnailService.GetAllThumbnailsAsync(fileId);
                return Ok(thumbnails);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
                // Attempt to set the default thumbnail for the given file
                await _thumbnailService.SetDefaultThumbnailAsync(thumbnailId, fileId);
                return Ok(new { message = "Default thumbnail updated successfully." });
            }
            catch (NotFoundException ex)
            {
                // If thumbnail or file is not found
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // If the user is not authorized to modify this thumbnail
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // If the operation is not valid in current state (e.g., setting thumbnail not related to file)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // For any unexpected errors
                return StatusCode(500, new { message = ex.Message });
            }
        }


        /// <summary>
        /// Retrieves all encoding profiles with pagination support.
        /// </summary>
        [HttpGet("getallEncodings")]
        public async Task<IActionResult> GetAllEncodingProfiles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Attempt to fetch all encoding profiles with pagination
                var (profiles, totalCount) = await _encodingProfileService.GetAllProfilesAsync(page, pageSize);

                // Return the profiles along with pagination details
                return Ok(new
                {
                    items = profiles,
                    total = totalCount,
                    page,
                    pageSize
                });
            }
            catch (NotFoundException ex)
            {
                // If no profiles are found
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // If the user is not authorized to view profiles
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // If request parameters are invalid or cause failure
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // For any unexpected errors
                return StatusCode(500, new { message = ex.Message });
            }
        }



    }
}
