using MCOP.Core.Services.Scoped.OAuth;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Core.Services.Scoped
{
    public interface IAppUserService
    {
        Task<string?> GetAccessTokenForUserAsync(string userId);
        Task StoreTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt);
    }

    public sealed class AppUserService : IAppUserService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly IDiscordOAuthService _oAuthService;

        public AppUserService(IDbContextFactory<McopDbContext> contextFactory, IDiscordOAuthService oAuthService)
        {
            _contextFactory = contextFactory;
            _oAuthService = oAuthService;
        }

        public async Task<string?> GetAccessTokenForUserAsync(string userId)
        {
            var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.AppUsers.FindAsync(userId);

            if (user == null || string.IsNullOrEmpty(user.DiscordAccessToken))
                return null;

            if (user.DiscordTokenExpiresAt > DateTime.UtcNow)
                return user.DiscordAccessToken;

            if (string.IsNullOrEmpty(user.DiscordRefreshToken))
                return null;

            var newTokens = await _oAuthService.RefreshTokenAsync(user.DiscordRefreshToken);
            if (newTokens == null)
                return null;

            user.DiscordAccessToken = newTokens.AccessToken;
            user.DiscordRefreshToken = newTokens.RefreshToken;
            user.DiscordTokenExpiresAt = newTokens.ExpiresAt;

            await context.SaveChangesAsync();

            return newTokens.AccessToken;
        }

        public async Task StoreTokensAsync(string userId, string accessToken, string refreshToken, DateTime expiresAt)
        {
            var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.AppUsers.FindAsync(userId);

            if (user != null)
            {
                user.DiscordAccessToken = accessToken;
                user.DiscordRefreshToken = refreshToken;
                user.DiscordTokenExpiresAt = expiresAt;
                await context.SaveChangesAsync();
                return;
            }

            user = new AppUser()
            {
                Id = userId,
                DiscordAccessToken = accessToken,
                DiscordRefreshToken = refreshToken,
                DiscordTokenExpiresAt = expiresAt
            };
            await context.AppUsers.AddAsync(user);
            await context.SaveChangesAsync();

        }
    }
}
