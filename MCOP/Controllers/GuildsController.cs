using DSharpPlus;
using DSharpPlus.Entities;
using MCOP.Common.Helpers;
using MCOP.Controllers.Responses;
using MCOP.Core.Models;
using MCOP.Core.Services.Scoped;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;

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

                var isAdminUser = userId == "226810751308791809";

                var userGuilds = await GetUserGuildsAsync(userId);
                if (userGuilds == null && !isAdminUser)
                    return Unauthorized();

                var botGuilds = _discordClient.Guilds;
                var filteredGuilds = isAdminUser
                    ? GetAdminGuilds(userGuilds, botGuilds)
                    : GetUserFilteredGuilds(userGuilds, botGuilds);

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

                var userGuilds = await GetUserGuildsAsync(userId);

                if (userGuilds is null)
                    return Ok(null);

                var botGuildsIds = _discordClient.Guilds.Keys.Select(x => x.ToString()).ToList();

                var guild = userGuilds
                    .FirstOrDefault(g => g.Id == guildId && (g.Owner || PermissionsHelper.HasManageServerPermission(g.Permissions)));

                if (guild == null)
                    return NotFound("Guild not found or you don't have permissions");

                return Ok(new GuildResponse
                {
                    Id = guild.Id,
                    Name = guild.Name,
                    Icon = guild.Icon,
                    BotPresent = botGuildsIds.Contains(guild.Id),
                    IsOwner = guild.Owner
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error fetching guild {GuildId} for user", guildId);
                return StatusCode(500, "Internal server error");
            }
        }


        private async Task<DiscordPartialGuild[]?> GetUserGuildsAsync(string userId)
        {
            var discordToken = await _appUserService.GetAccessTokenForUserAsync(userId);
            if (discordToken == null)
                return null;

            using var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", discordToken);

            var response = await client.GetAsync("https://discord.com/api/users/@me/guilds");
            if (!response.IsSuccessStatusCode)
                return null;

            var payload = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DiscordPartialGuild[]>(payload);
        }

        private static List<GuildResponse> GetAdminGuilds(DiscordPartialGuild[]? userGuilds, IReadOnlyDictionary<ulong, DiscordGuild> botGuilds)
        {
            var botGuildIds = botGuilds.Keys.Select(x => x.ToString()).ToHashSet();
            var allGuilds = new List<GuildResponse>();

            allGuilds.AddRange(botGuilds.Values.Select(g => new GuildResponse
            {
                Id = g.Id.ToString(),
                Name = g.Name,
                Icon = g.IconHash ?? "",
                BotPresent = true,
                IsOwner = false
            }));

            if (userGuilds != null)
            {
                var userGuildsToAdd = userGuilds
                    .Where(g => g.Owner || PermissionsHelper.HasManageServerPermission(g.Permissions))
                    .Where(g => !botGuildIds.Contains(g.Id))
                    .Select(g => new GuildResponse
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Icon = g.Icon ?? "",
                        BotPresent = false,
                        IsOwner = g.Owner
                    });

                allGuilds.AddRange(userGuildsToAdd);
            }

            return allGuilds.OrderByDescending(x => x.BotPresent)
                            .ThenBy(x => x.Name)
                            .ToList();
        }

        private static List<GuildResponse> GetUserFilteredGuilds(DiscordPartialGuild[]? userGuilds, IReadOnlyDictionary<ulong, DiscordGuild> botGuilds)
        {
            if (userGuilds is null) return [];

            var botGuildIds = botGuilds.Keys.Select(x => x.ToString()).ToHashSet();

            return userGuilds
                .Where(g => g.Owner || PermissionsHelper.HasManageServerPermission(g.Permissions))
                .Select(g => new GuildResponse
                {
                    Id = g.Id,
                    Name = g.Name,
                    Icon = g.Icon ?? "",
                    BotPresent = botGuildIds.Contains(g.Id),
                    IsOwner = g.Owner
                })
                .OrderByDescending(x => x.BotPresent)
                .ThenBy(x => x.Name)
                .ToList();
        }
    }
}