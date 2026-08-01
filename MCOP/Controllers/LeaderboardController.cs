using MCOP.Core.Models;
using MCOP.Core.Services.Scoped;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public sealed class LeaderboardController : ControllerBase
    {
        private readonly IGuildUserStatsService _statsService;
        private readonly Serilog.ILogger _logger;

        public LeaderboardController(
            IGuildUserStatsService statsService, 
            Serilog.ILogger logger)
        {
            _statsService = statsService;
            _logger = logger;
        }

        [HttpGet("{guildId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<GuildUserStatsDto>>> GetLeaderboard(ulong guildId, int pageSize = 20, string? sortby = null, int page = 1, bool sortDescending = true)
        {
            try
            {
                var (stats, totalCount) = await _statsService.GetGuildUserStatsAsync(guildId, page, pageSize, sortby, sortDescending);
                await _statsService.UpdateMissingUserInfoAsync(guildId, stats);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting leaderboard for guild {GuildId}", guildId);
                return StatusCode(500, "Failed to get leaderboard");
            }
        }
    }
}
