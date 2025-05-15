using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DSharpPlus;
using System.Net.Http.Headers;
using MCOP.Core.Services.Scoped;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using MCOP.Core.Models;

[ApiController]
[Route("api/guilds")]
public class GuildsController : ControllerBase
{
    private readonly DiscordClient _discordClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAppUserService _appUserService;
    private readonly Serilog.ILogger _logger;

    public GuildsController(DiscordClient discordClient, IHttpClientFactory httpClientFactory, IAppUserService appUserService, Serilog.ILogger logger)
    {
        _discordClient = discordClient;
        _httpClientFactory = httpClientFactory;
        _appUserService = appUserService;
        _logger = logger;
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
                .Where(g => g.Owner || HasManageServerPermission(g.Permissions))
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

    private bool HasManageServerPermission(string permissionBits)
    {
        if (!ulong.TryParse(permissionBits, out var bits))
            return false;

        var permissions = (DiscordPermission)bits;

        return permissions.HasFlag(DiscordPermission.Administrator) ||
               permissions.HasFlag(DiscordPermission.ManageGuild);
    }
}