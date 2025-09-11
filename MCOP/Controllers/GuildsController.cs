using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DSharpPlus;
using System.Net.Http.Headers;
using MCOP.Core.Services.Scoped;
using Newtonsoft.Json;
using MCOP.Core.Models;
using MCOP.Common.Helpers;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/guilds")]
    public sealed class GuildsController : ControllerBase
    {
        private readonly DiscordClient _discordClient;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAppUserService _appUserService;
        private readonly IGuildRoleService _guildRoleService;
        private readonly Serilog.ILogger _logger;

        public GuildsController(DiscordClient discordClient, IHttpClientFactory httpClientFactory, IAppUserService appUserService, Serilog.ILogger logger, IGuildRoleService guildRoleService)
        {
            _discordClient = discordClient;
            _httpClientFactory = httpClientFactory;
            _appUserService = appUserService;
            _logger = logger;
            _guildRoleService = guildRoleService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetGuilds()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var discordToken = await _appUserService.GetAccessTokenForUserAsync(userId);
                if (discordToken == null)
                    return Unauthorized();

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", discordToken);

                var res = await client.GetAsync("https://discord.com/api/users/@me/guilds");
                if (!res.IsSuccessStatusCode)
                    return StatusCode((int)res.StatusCode, "Couldn't fetch user's guilds from Discord");

                var payload = await res.Content.ReadAsStringAsync();
                var userGuilds = JsonConvert.DeserializeObject<DiscordPartialGuild[]>(payload);

                if (userGuilds is null)
                    return Ok(Array.Empty<object>());
                var botGuildsIds = _discordClient.Guilds.Keys.Select(x => x.ToString()).ToList();

                var filteredGuilds = userGuilds
                    .Where(g => g.Owner || PermissionsHelper.HasManageServerPermission(g.Permissions))
                    .Select(g => new
                    {
                        id = g.Id,
                        name = g.Name,
                        icon = g.Icon,
                        botPresent = botGuildsIds.Contains(g.Id),
                        isOwner = g.Owner
                    })
                    .OrderByDescending(x => x.botPresent)
                    .ThenBy(x => x.name)
                    .ToList();

                return Ok(filteredGuilds);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching guilds");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("{guildId}")]
        public async Task<IActionResult> GetGuild(string guildId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var discordToken = await _appUserService.GetAccessTokenForUserAsync(userId);
                if (discordToken == null)
                    return Unauthorized();

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", discordToken);

                var res = await client.GetAsync("https://discord.com/api/users/@me/guilds");
                if (!res.IsSuccessStatusCode)
                    return StatusCode((int)res.StatusCode, "Couldn't fetch user's guilds from Discord");

                var payload = await res.Content.ReadAsStringAsync();
                var userGuilds = JsonConvert.DeserializeObject<DiscordPartialGuild[]>(payload);

                if (userGuilds is null)
                    return Ok(null);

                var botGuildsIds = _discordClient.Guilds.Keys.Select(x => x.ToString()).ToList();

                var guild = userGuilds
                    .FirstOrDefault(g => g.Id == guildId && (g.Owner || PermissionsHelper.HasManageServerPermission(g.Permissions)));

                if (guild == null)
                    return NotFound("Guild not found or you don't have permissions");

                return Ok(new
                {
                    id = guild.Id,
                    name = guild.Name,
                    icon = guild.Icon,
                    botPresent = botGuildsIds.Contains(guild.Id),
                    isOwner = guild.Owner
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching guild {GuildId} for user", guildId);
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("{guildId}/roles")]
        public async Task<IActionResult> GetGuildRoles(string guildId)
        {
            try
            {
                var id = ulong.Parse(guildId);
                var guild = await _discordClient.GetGuildAsync(id);
                var roleSettings = await _guildRoleService.GetGuildRolesAsync(id);

                // Создаем словарь для быстрого поиска настроек по ID роли
                var roleSettingsDict = roleSettings.ToDictionary(x => x.Id, x => x);

                var combinedRoles = guild.Roles.Values
                    .Select(role =>
                    {
                        roleSettingsDict.TryGetValue(role.Id, out var settings);

                        return new
                        {
                            id = role.Id.ToString(),
                            name = role.Name,
                            position = role.Position,
                            color = role.Color.ToString(),
                            iconUrl = role.IconUrl,
                            levelToGetRole = settings?.LevelToGetRole,
                            isGainExpBlocked = settings?.IsGainExpBlocked ?? false,
                            levelUpMessageTemplate = settings?.LevelUpMessageTemplate
                        };
                    })
                    .OrderByDescending(x => x.position)
                    .ToList();

                return Ok(combinedRoles);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching roles for guild {GuildId}", guildId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}