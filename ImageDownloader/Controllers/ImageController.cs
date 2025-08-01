using ImageDownloader.Models;
using ImageDownloader.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageDownloader.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageDownloadService _imageDownloadService;

        public ImageController(IImageDownloadService imageDownloadService)
        {
            _imageDownloadService = imageDownloadService;
        }

        [HttpPost("download")]
        public async Task<ActionResult<ResponseDownload>> DownloadImages([FromBody] RequestDownload request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!request.ImageUrls.Any())
                {
                    return BadRequest(new ResponseDownload
                    {
                        Success = false,
                        Message = "At least one image URL is required."
                    });
                }

                var result = await _imageDownloadService.DownloadImagesAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDownload
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        [HttpGet("get-image-by-name/{imageName}")]
        public async Task<ActionResult<object>> GetImageByName(string imageName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageName))
                {
                    return BadRequest(new { Message = "Image name is required." });
                }

                var base64Image = await _imageDownloadService.GetImageAsBase64Async(imageName);

                if (base64Image == null)
                {
                    return NotFound(new { Message = $"Image '{imageName}' not found." });
                }

                return Ok(new
                {
                    ImageName = imageName,
                    Base64Data = base64Image,
                    Message = "Image retrieved successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}