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

        [HttpGet("sas-url")]
        public async Task<IActionResult> GetSasUrl([FromQuery] string fileName)
        {
            var sasUrl = await _cloudStorageService.GenerateSasUriAsync(fileName);
            Console.WriteLine(sasUrl);

            return Ok(sasUrl);
        }

        // [HttpPost("merge")]
        // public async Task<IActionResult> MergeChunks([FromBody] MergeRequestDto dto)
        // {
        //     await _azureService.MergeChunksAsync(dto.FileId, dto.TotalChunks, dto.OutputFileName,dto.EncodingId);
        // return Ok(new { message = " Merge completed." });
        // }
    }

}
