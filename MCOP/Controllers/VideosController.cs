using MCOP.Common.Attributes;
using MCOP.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/videos")]
    public sealed class VideosController : ControllerBase
    {
        private readonly string? _rootPath;

        public VideosController(ConfigurationService configurationService)
        {
            _rootPath = configurationService.CurrentConfiguration.SharedVideosPath;
        }

        private static readonly string[] VideoExtensions = [".mp4", ".webm", ".mkv", ".avi", ".mov", ".m4v"];

        [HttpGet("random")]
        [AuthorizeUserId(226810751308791809)]
        public IActionResult GetRandomVideos([FromQuery] int count = 10)
        {
            if (string.IsNullOrEmpty(_rootPath)) return NotFound();

            try
            {
                var allVideos = Directory.EnumerateFiles(_rootPath, "*.*", SearchOption.AllDirectories)
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
                var safePath = Path.GetFullPath(Path.Combine(_rootPath, videoPath.TrimStart('/', '\\')));

                if (!safePath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Invalid path.");
                }

                if (!System.IO.File.Exists(safePath))
                    return NotFound();

                var fileInfo = new FileInfo(safePath);
                var mimeType = GetMimeType(safePath);

                var rangeHeader = Request.Headers.Range.ToString();
                if (string.IsNullOrEmpty(rangeHeader))
                    return PhysicalFile(safePath, mimeType, enableRangeProcessing: true);

                var match = Regex.Match(rangeHeader, @"bytes=(\d*)-(\d*)");
                if (!match.Success)
                    return PhysicalFile(safePath, mimeType, enableRangeProcessing: true);

                long start = 0;
                long end = fileInfo.Length - 1;

                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    start = long.Parse(match.Groups[1].Value);

                if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    end = long.Parse(match.Groups[2].Value);

                if (start > end || start < 0 || end >= fileInfo.Length)
                    return StatusCode(416);

                var length = end - start + 1;
                var stream = new FileStream(safePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                stream.Position = start;

                Response.StatusCode = 206; // Partial Content
                Response.Headers.AcceptRanges = "bytes";
                Response.Headers.ContentRange = $"bytes {start}-{end}/{fileInfo.Length}";
                return File(stream, mimeType, enableRangeProcessing: false, fileDownloadName: null, lastModified: fileInfo.LastWriteTimeUtc, entityTag: null);
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
