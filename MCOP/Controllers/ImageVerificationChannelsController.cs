using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DSharpPlus;
using MCOP.Core.Services.Scoped;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/guilds")]
    public sealed class ImageVerificationChannelsController : BasePermissionController
    {
        private readonly IImageVerificationChannelService _imageVerificationChannelService;
        private readonly Serilog.ILogger _logger;

        public ImageVerificationChannelsController(
            DiscordClient discordClient,
            IImageVerificationChannelService imageVerificationChannelService,
            Serilog.ILogger logger) : base(discordClient)
        {
            _imageVerificationChannelService = imageVerificationChannelService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("{guildId}/image-verification-channels")]
        public async Task<IActionResult> GetImageVerificationChannels(string guildId)
        {
            try
            {
                if (!ulong.TryParse(guildId, out var guildUlongId))
                    return BadRequest("Invalid guild ID");

                var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
                if (forbiddenResult is not null)
                    return forbiddenResult;

                var channels = await _imageVerificationChannelService.GetImageVerificationChannelsAsync(guildUlongId);
                return Ok(channels.Select(x => x.ToString()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting image verification channels for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to get image verification channels");
            }
        }

        [Authorize]
        [HttpPost("{guildId}/image-verification-channels")]
        public async Task<IActionResult> AddImageVerificationChannel(string guildId, [FromBody] ImageVerificationChannelRequest request)
        {
            try
            {
                if (!ulong.TryParse(guildId, out var guildUlongId))
                    return BadRequest("Invalid guild ID");

                var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
                if (forbiddenResult is not null)
                    return forbiddenResult;

                if (request is null)
                    return BadRequest("Missing body");

                await _imageVerificationChannelService.AddImageVerificationChannelAsync(guildUlongId, ulong.Parse(request.ChannelId));
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding image verification channel for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to add image verification channel");
            }
        }

        [Authorize]
        [HttpDelete("{guildId}/image-verification-channels/{channelId}")]
        public async Task<IActionResult> RemoveImageVerificationChannel(string guildId, string channelId)
        {
            try
            {
                if (!ulong.TryParse(guildId, out var guildUlongId) || !ulong.TryParse(channelId, out var channelUlongId))
                    return BadRequest("Invalid guild ID or channel ID");

                var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
                if (forbiddenResult is not null)
                    return forbiddenResult;

                await _imageVerificationChannelService.RemoveImageVerificationChannelAsync(guildUlongId, channelUlongId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error removing image verification channel for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to remove image verification channel");
            }
        }
    }

    public sealed class ImageVerificationChannelRequest
    {
        public required string ChannelId { get; set; }
    }
}