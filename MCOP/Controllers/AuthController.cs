using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using DSharpPlus.Entities;
using MCOP.Core.Services.Scoped.OAuth;
using MCOP.Utils;
using Microsoft.AspNetCore.Authorization;
using MCOP.Core.Services.Scoped;

namespace MCOP.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IDiscordOAuthService _discordService;
        private readonly IAppUserService _appUserService;
        private readonly ConfigurationService _config;

        public AuthController(IDiscordOAuthService discordService, ConfigurationService config, IAppUserService appUserService)
        {
            _discordService = discordService;
            _config = config;
            _appUserService = appUserService;
        }

        [HttpPost("discord/callback")]
        public async Task<IActionResult> DiscordCallback([FromBody] AuthRequest model)
        {
            try
            {
                var tokenResponse = await _discordService.ExchangeCodeAsync(model.Code);

                if (tokenResponse is null)
                    return BadRequest(new { error = "Discord Token Exchange Failed" });

                var user = await _discordService.FetchDiscordUserAsync(tokenResponse.AccessToken);

                if (user is null)
                    return BadRequest(new { error = "Fetch Discord User failed" });

                var sessionToken = GenerateJwt(user, tokenResponse.ExpiresAt);

                await _appUserService.StoreTokensAsync(user.Id.ToString(), tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.ExpiresAt);

                return Ok(new
                {
                    session = sessionToken,
                    id = user.Id.ToString(),
                    username = user.Username,
                    avatarUrl = user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var avatarUrl = User.FindFirst("avatarUrl")?.Value;

            if (userId == null || username == null)
                return Unauthorized();

            return Ok(new
            {
                id = userId.ToString(),
                username,
                avatarUrl
            });
        }

        private string GenerateJwt(DiscordUser user, DateTime expiresAt)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.CurrentConfiguration.JwtConfig.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("avatarUrl", user.AvatarUrl)
        };

            var token = new JwtSecurityToken(
                issuer: _config.CurrentConfiguration.JwtConfig.Issuer,
                audience: _config.CurrentConfiguration.JwtConfig.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}