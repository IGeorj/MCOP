using MCOP.Core.Models;
using MCOP.Core.Services.Scoped;
using Microsoft.AspNetCore.Mvc;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class LeaderboardController : ControllerBase
    {
        private readonly IGuildUserStatsService _statsService;

        public LeaderboardController(
            IGuildUserStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet("{guildId}")]
        public async Task<ActionResult<List<GuildUserStatsDto>>> GetLeaderboard(ulong guildId, int pageSize = 20, string? sortby = null, int page = 1, bool sortDescending = true)
        {
            var (stats, totalCount) = await _statsService.GetGuildUserStatsAsync(guildId, page, pageSize, sortby, sortDescending);
            await _statsService.UpdateMissingUserInfoAsync(guildId, stats);
            return Ok(stats);
        }
    }
}
