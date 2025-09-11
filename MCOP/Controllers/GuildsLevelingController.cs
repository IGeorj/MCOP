using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DSharpPlus;
using MCOP.Core.Services.Scoped;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/guilds")]
    public sealed class GuildsLevelingController : BasePermissionController
    {
        private readonly IGuildConfigService _guildConfigService;
        private readonly Serilog.ILogger _logger;

        public GuildsLevelingController(
            DiscordClient discordClient,
            IGuildConfigService guildConfigService,
            Serilog.ILogger logger) : base(discordClient)
        {
            _guildConfigService = guildConfigService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("{guildId}/leveling/message-settings")]
        public async Task<IActionResult> GetMessageSettings(string guildId)
        {
            try
            {
                if (!ulong.TryParse(guildId, out var guildUlongId))
                    return BadRequest("Invalid guild ID");

                var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
                if (forbiddenResult is not null)
                    return forbiddenResult;

                var cfg = await _guildConfigService.GetOrAddGuildConfigAsync(guildUlongId);
                return Ok(new
                {
                    template = cfg.LevelUpMessageTemplate,
                    enabled = cfg.LevelUpMessagesEnabled,
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting message settings for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to get message settings");
            }
        }

        [Authorize]
        [HttpPost("{guildId}/leveling/message-settings")]
        public async Task<IActionResult> UpdateMessageSettings(string guildId, [FromBody] GuildLevelingMessageSettingsRequest request)
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

                if (request.Enabled.HasValue)
                    await _guildConfigService.SetLevelUpMessagesEnabledAsync(guildUlongId, request.Enabled.Value);
                if (request.TemplateProvided)
                    await _guildConfigService.SetLevelUpMessageTemplateAsync(guildUlongId, request.Template);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating message settings for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to update message settings");
            }
        }
    }

    public sealed class GuildLevelingMessageSettingsRequest
    {
        public string? Template { get; set; }
        public bool? Enabled { get; set; }

        // This flag allows clearing the template explicitly by sending Template = null and TemplateProvided = true
        public bool TemplateProvided { get; set; }
    }
}
