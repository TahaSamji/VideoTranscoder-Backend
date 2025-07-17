// Controllers/AzureController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CloudStorageController : ControllerBase
    {
        private readonly ICloudStorageService _cloudStorageService;

        public CloudStorageController(ICloudStorageService cloudStorageService)
        {
            _cloudStorageService = cloudStorageService;
        }

        /// <summary>
        /// Generates a SAS URL for the given file name to allow secure access.
        /// </summary>
      [HttpGet("sas-url")]
        public async Task<IActionResult> GetSasUrl([FromQuery] string fileName)
        {
            try
            {
                var sasUrl = await _cloudStorageService.GenerateSasUriAsync(fileName);
                return Ok(sasUrl);
            }
            catch (Exception ex)
            {
                // Generic fallback
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
