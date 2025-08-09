using MCOP.Common.Attributes;
using MCOP.Utils;
using Microsoft.AspNetCore.Mvc;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/images")]
    public sealed class ImagesController : ControllerBase
    {
        private readonly string? _rootPath;

        public ImagesController(ConfigurationService configurationService)
        {
            _rootPath = configurationService.CurrentConfiguration.SharedFilesPath;
        }

        [HttpGet("random")]
        [AuthorizeUserId(226810751308791809)]
        public IActionResult GetRandomImages([FromQuery] int count = 10)
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();

            try
            {
                var allImages = Directory.EnumerateFiles(_rootPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                   file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (allImages.Count == 0)
                    return NotFound("No images found");

                var random = new Random();
                var randomImages = allImages
                    .OrderBy(x => random.Next())
                    .Take(count)
                    .Select(file => new
                    {
                        Path = file.Replace(_rootPath, "").TrimStart('\\'),
                        FullPath = file,
                        Size = new FileInfo(file).Length
                    })
                    .ToList();

                return Ok(randomImages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("content/{*imagePath}")]
        [AuthorizeUserId(226810751308791809)]
        public IActionResult GetImageContent(string imagePath)
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();

            try
            {
                var fullPath = Path.GetFullPath(Path.Combine(_rootPath, imagePath));

                if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Invalid path.");
                }

                if (!System.IO.File.Exists(fullPath))
                    return NotFound();

                var mimeType = GetMimeType(fullPath);
                var fileStream = System.IO.File.OpenRead(fullPath);

                return File(fileStream, mimeType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }
}
