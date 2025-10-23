using DSharpPlus.Entities;
using MCOP.Core.Models;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public interface IGuildUserEmojiService
    {
        public Task AddRecievedAmountAsync(ulong guildId, ulong userId, ulong emojiId, int amount);
        public Task RemoveRecievedAmountAsync(ulong guildId, ulong userId, ulong emojiId, int amount);
        public Task<List<GuildUserEmojiDto>> GetTopEmojisForUserAsync(ulong guildId, ulong userId, int topAmount = 5);
        public Task SetGuildUserEmojiCountAsync(ulong guildId, ulong userId, DiscordEmoji discordEmoji, int count);
    }

    public sealed class GuildUserEmojiService : IGuildUserEmojiService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        public GuildUserEmojiService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddRecievedAmountAsync(ulong guildId, ulong userId, ulong emojiId, int amount)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var userEmoji = await GetOrCreateUserEmojiAsync(context, guildId, userId, emojiId);
                userEmoji.RecievedAmount += amount;
                await context.SaveChangesAsync();

                Log.Information("AddRecievedAmountAsync guildId: {guildId}, userId: {userId}, emojiId: {emojiId}, {amount}", guildId, userId, emojiId, amount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in AddRecievedAmountAsync for guildId: {guildId}, userId: {userId}, emojiId: {emojiId}, {amount}", guildId, userId, emojiId, amount);
            }
        }

        public async Task RemoveRecievedAmountAsync(ulong guildId, ulong userId, ulong emojiId, int amount)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var userEmoji = await GetOrCreateUserEmojiAsync(context, guildId, userId, emojiId);
                if (userEmoji.RecievedAmount >= amount)
                {
                    userEmoji.RecievedAmount -= amount;
                    await context.SaveChangesAsync();
                }
                Log.Information("RemoveRecievedAmountAsync guildId: {guildId}, userId: {userId}, emojiId: {emojiId}, {amount}", guildId, userId, emojiId, amount);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in RemoveRecievedAmountAsync for guildId: {guildId}, userId: {userId}, emojiId: {emojiId}, {amount}", guildId, userId, emojiId, amount);
                throw;
            }
        }

        private async Task<GuildUserEmoji> GetOrCreateUserEmojiAsync(McopDbContext context, ulong guildId, ulong userId, ulong emojiId)
        {
            var userEmoji = await context.GuildUserEmojies
                .SingleOrDefaultAsync(ue => ue.GuildId == guildId && ue.UserId == userId && ue.EmojiId == emojiId);
            if (userEmoji == null)
            {
                userEmoji = new GuildUserEmoji
                {
                    GuildId = guildId,
                    UserId = userId,
                    EmojiId = emojiId,
                    RecievedAmount = 0
                };
                context.GuildUserEmojies.Add(userEmoji);
            }
            return userEmoji;
        }

        public async Task<List<GuildUserEmojiDto>> GetTopEmojisForUserAsync(ulong guildId, ulong userId, int topAmount = 5)
        {
            try
            {
                Log.Information("GetTopEmojisForUserAsync guildId: {guildId}, userId: {userId}", guildId, userId);

                await using var context = await _contextFactory.CreateDbContextAsync();

                return await context.GuildUserEmojies
                    .Where(x => x.GuildId == guildId && x.UserId == userId)
                    .OrderByDescending(x => x.RecievedAmount)
                    .Take(topAmount)
                    .Select(e => new GuildUserEmojiDto(e.GuildId, e.UserId, e.EmojiId, e.RecievedAmount))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetTopEmojisForUserAsync for guildId: {guildId}, userId: {userId}", guildId, userId);
                throw;
            }
        }

        public async Task SetGuildUserEmojiCountAsync(ulong guildId, ulong userId, DiscordEmoji discordEmoji, int count)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                var guildUserEmoji = await context.GuildUserEmojies
                    .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId && x.EmojiId == discordEmoji.Id);

                if (guildUserEmoji == null)
                {
                    guildUserEmoji = new GuildUserEmoji
                    {
                        GuildId = guildId,
                        UserId = userId,
                        EmojiId = discordEmoji.Id,
                        RecievedAmount = count
                    };
                    context.GuildUserEmojies.Add(guildUserEmoji);
                }
                else
                {
                    guildUserEmoji.RecievedAmount = count;
                    context.GuildUserEmojies.Update(guildUserEmoji);
                }

                await context.SaveChangesAsync();

                Log.Information(
                    "SetGuildUserEmojiCountAsync: Updated emoji count for guildId: {guildId}, userId: {userId}, emoji: {emoji}, count: {count}",
                    guildId, userId, discordEmoji.GetDiscordName(), count);
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "Error in SetGuildUserEmojiCountAsync for guildId: {guildId}, userId: {userId}, emoji: {emoji}",
                    guildId, userId, discordEmoji.GetDiscordName());
                throw;
            }
        }
    }
}
