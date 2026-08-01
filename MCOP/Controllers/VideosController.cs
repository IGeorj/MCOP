using MCOP.Common.Attributes;
using MCOP.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/videos")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public sealed class VideosController : ControllerBase
    {
        private readonly string? _rootPath;

        public VideosController(ConfigurationService configurationService)
        {
            _rootPath = configurationService.CurrentConfiguration.SharedVideosPath;
        }

        private static readonly string[] VideoExtensions = [".mp4", ".webm", ".mkv", ".avi", ".mov", ".m4v"];

        [HttpGet("folders")]
        [AuthorizeUserId(226810751308791809)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetFolders()
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();
            try
            {
                var directories = Directory.GetDirectories(_rootPath)
                    .Select(dir => new DirectoryInfo(dir).Name)
                    .ToList();
                return Ok(directories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("random")]
        [AuthorizeUserId(226810751308791809)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetRandomVideos([FromQuery] int count = 10, [FromQuery] string? folder = null)
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();

            string searchRoot = _rootPath;
            if (!string.IsNullOrEmpty(folder))
            {
                var combined = Path.Combine(_rootPath, folder);
                var fullPath = Path.GetFullPath(combined);

                if (!fullPath.StartsWith(searchRoot, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid folder.");

                var target = Directory.ResolveLinkTarget(fullPath, true) as DirectoryInfo;
                string actualPath = target?.FullName ?? fullPath;
                if (!actualPath.StartsWith(searchRoot, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid folder.");

                if (!Directory.Exists(actualPath))
                    return NotFound($"Folder '{folder}' not found.");

                searchRoot = fullPath;
            }

            try
            {
                var allVideos = Directory.EnumerateFiles(searchRoot, "*.*", SearchOption.AllDirectories)
                    .Where(file => VideoExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (allVideos.Count == 0)
                    return NotFound("No videos found");

                var random = new Random();
                var randomVideos = allVideos
                    .OrderBy(x => random.Next())
                    .Take(count)
                    .Select(file => new
                    {
                        Path = file.Replace(_rootPath, "").TrimStart('\\'),
                        FullPath = file,
                        Size = new FileInfo(file).Length
                    })
                    .ToList();

                return Ok(randomVideos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("content/{*videoPath}")]
        [AuthorizeUserId(226810751308791809)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetVideoContent(string videoPath)
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();

            if (string.IsNullOrEmpty(videoPath) ||
                videoPath.Contains("..") ||
                videoPath.Contains(':') ||
                videoPath.Contains("//"))
            {
                return BadRequest("Invalid path.");
            }

            try
            {
                // 2. Remove blacklists. Just sanitize slashes and combine.
                var trimmedInput = videoPath.TrimStart('/', '\\');
                var combined = Path.Combine(_rootPath, trimmedInput);
                var canonicalPath = Path.GetFullPath(combined);

                if (!canonicalPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid path.");

                var linkTarget = System.IO.File.ResolveLinkTarget(canonicalPath, returnFinalTarget: false);
                string finalPath = linkTarget?.FullName ?? canonicalPath;

                if (!finalPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("Invalid path.");

                var extension = Path.GetExtension(finalPath);
                if (!VideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    return BadRequest("Invalid file type.");

                if (!System.IO.File.Exists(finalPath))
                    return NotFound();

                var mimeType = GetMimeType(finalPath);

                return PhysicalFile(finalPath, mimeType, enableRangeProcessing: true);
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
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mkv" => "video/x-matroska",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".m4v" => "video/x-m4v",
                _ => "application/octet-stream"
            };
        }
    }
}
