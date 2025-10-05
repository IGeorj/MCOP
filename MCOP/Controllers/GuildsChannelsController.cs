using Microsoft.AspNetCore.Mvc;
using DSharpPlus;
using MCOP.Core.Services.Scoped;
using Microsoft.AspNetCore.Authorization;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/guilds")]
    public sealed class GuildsChannelsController : BasePermissionController
    {
        private readonly IGuildConfigService _guildConfigService;
        private readonly DiscordClient _discordClient;
        private readonly Serilog.ILogger _logger;

        public GuildsChannelsController(
            DiscordClient discordClient,
            IGuildConfigService guildConfigService,
            Serilog.ILogger logger) : base(discordClient)
        {
            _discordClient = discordClient;
            _guildConfigService = guildConfigService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("{guildId}/daily-nsfw-channel")]
        public async Task<IActionResult> AddOrUpdateLevelRole(
            string guildId,
            [FromBody] DailyNsfwRequest request)
        {
            try
            {
                if (!ulong.TryParse(guildId, out var guildUlongId))
                    return BadRequest("Invalid guild ID");

                ulong? channelId = null;
                if (ulong.TryParse(request.ChannelId, out var parsedChannelId))
                    channelId = parsedChannelId;

                var forbiddenResult = await CheckUserPermissionsAsync(guildUlongId);
                if (forbiddenResult is not null)
                    return forbiddenResult;

                await _guildConfigService.SetLewdChannelAsync(guildUlongId, channelId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating daily nsfw channel for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to update daily nsfw channel");
            }
        }


        [Authorize]
        [HttpGet("{guildId}/channels")]
        public async Task<IActionResult> GetGuildChannels(string guildId)
        {
            try
            {
                var id = ulong.Parse(guildId);
                var guild = await _discordClient.GetGuildAsync(id);
                var channels = await guild.GetChannelsAsync();
                var guildConfig = await _guildConfigService.GetOrAddGuildConfigAsync(id);
                channels = channels.OrderBy(x => x.Position).Where(x => x.Type == DSharpPlus.Entities.DiscordChannelType.Text).ToList();

                return Ok(channels.Select(channel => new
                {
                    id = channel.Id.ToString(),
                    name = channel.Name,
                    isNsfw = channel.IsNSFW,
                    isDailyNsfw = guildConfig.LewdChannelId == channel.Id
                }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching roles for guild {GuildId}", guildId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class DailyNsfwRequest
    {
        public string? ChannelId { get; set; }
    }
}