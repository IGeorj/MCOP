using DSharpPlus.Entities;
using MCOP.Core.Common;
using MCOP.Core.Exceptions;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class GuildEmojiService : IScoped
    {
        private readonly McopDbContext _context;
        private readonly UserService _userService;

        public GuildEmojiService(McopDbContext context, GuildService guildService, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<GuildEmoji> GetOrAddGuildEmojiAsync(ulong guildId, ulong emojiId, string emojiName, bool isLocked = false)
        {
            try
            {
                string cacheKey = $"GuildEmoji_{guildId}_{emojiId}";

                if (!isLocked)
                {
                    var lockKey = $"{cacheKey}_lock";
                    using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                    {
                        Log.Information("GetOrAddGuildEmojiAsync guildId: {guildId}, emoji: {emoji}, isLocked: {isLocked}", guildId, emojiName, isLocked);

                        return await GetOrAddGuildEmojiInternalAsync(guildId, emojiId, emojiName, cacheKey);
                    }
                }
                else
                {
                    Log.Information("GetOrAddGuildEmojiAsync guildId: {guildId}, emoji: {emoji}, isLocked: {isLocked}", guildId, emojiName, isLocked);

                    return await GetOrAddGuildEmojiInternalAsync(guildId, emojiId, emojiName, cacheKey);
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        private async Task<GuildEmoji> GetOrAddGuildEmojiInternalAsync(ulong guildId, ulong emojiId, string emojiName, string cacheKey)
        {
            GuildEmoji? guildEmoji = await _context.GuildEmoji.FindAsync(emojiId);

            if (guildEmoji is null)
            {
                guildEmoji = new GuildEmoji { Id = emojiId, GuildId = guildId, DiscordName = emojiName };
                _context.GuildEmoji.Add(guildEmoji);
                await _context.SaveChangesAsync();
            }

            return guildEmoji;
        }

        public async Task<GuildUserEmoji> GetOrAddGuildUserEmojiAsync(DiscordEmoji emoji, ulong guildId, ulong userId, bool isLocked = false)
        {
            try
            {
                string cacheKey = GetGuildUserEmojiCacheKey(guildId, userId, emoji.Id);

                if (!isLocked)
                {
                    var lockKey = $"{cacheKey}_lock";
                    using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                    {
                        Log.Information("GetOrAddGuildUserEmojiAsync guildId: {guildId}, userId: {userId}, emoji: {emoji}, isLocked: {isLocked}", guildId, userId, emoji.Name, isLocked);

                        return await GetOrAddGuildUserEmojiInternalAsync(emoji, guildId, userId, cacheKey);
                    }
                }
                else
                {
                    Log.Information("GetOrAddGuildUserEmojiAsync guildId: {guildId}, userId: {userId}, emoji: {emoji}, isLocked: {isLocked}", guildId, userId, emoji.Name, isLocked);

                    return await GetOrAddGuildUserEmojiInternalAsync(emoji, guildId, userId, cacheKey);
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        private async Task<GuildUserEmoji> GetOrAddGuildUserEmojiInternalAsync(DiscordEmoji emoji, ulong guildId, ulong userId, string cacheKey)
        {
            GuildUserEmoji? guildUserEmoji = await _context.GuildUserEmoji.FindAsync(guildId, userId, emoji.Id);

            if (guildUserEmoji is null)
            {
                GuildUser guildUser = await _userService.GetOrAddUserAsync(guildId, userId);

                guildUserEmoji = new GuildUserEmoji
                {
                    GuildId = guildUser.GuildId,
                    UserId = guildUser.UserId,
                    EmojiId = emoji.Id
                };

                _context.GuildUserEmoji.Add(guildUserEmoji);

                await _context.SaveChangesAsync();
            }

            return guildUserEmoji;
        }

        public async Task<bool> ChangeUserRecievedEmojiCountAsync(DiscordEmoji emoji, ulong guildId, ulong userId, int count)
        {
            try
            {
                var cacheKey = GetGuildUserEmojiCacheKey(guildId, userId, emoji.Id);
                var lockKey = $"{cacheKey}_lock";

                using (var lockHandle = await SemaphoreLock.LockAsync(lockKey))
                {
                    GuildEmoji guildEmoji = await GetOrAddGuildEmojiAsync(guildId, emoji.Id, emoji.GetDiscordName());
                    GuildUserEmoji guildUserEmoji = await GetOrAddGuildUserEmojiAsync(emoji, guildId, userId, isLocked: true);

                    guildUserEmoji.RecievedAmount += count;

                    _context.GuildUserEmoji.Update(guildUserEmoji);
                    await _context.SaveChangesAsync();

                    Log.Information("ChangeUserRecievedEmojiCountAsync guildId: {guildId}, userId: {userId}, emoji: {emoji}, total:{RecievedAmount}, changed: {count}", guildId, userId, emoji.GetDiscordName(), guildUserEmoji.RecievedAmount, count);

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, "Failed to update emoji received count.");
            }
        }

        public async Task<List<GuildUserEmoji>> GetUserTopEmojiAsync(ulong guildId, ulong userId, int topAmount = 5)
        {
            try
            {
                Log.Information("GetUserTopEmojiAsync guildId: {guildId}, userId: {userId}", guildId, userId);

                return await _context.GuildUserEmoji
                    .Include(x => x.GuildEmoji)
                    .Where(x => x.GuildId == guildId && x.UserId == userId)
                    .OrderByDescending(x => x.RecievedAmount)
                    .Take(topAmount).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task UpdateGuildEmojiesAsync(ulong guildId, Dictionary<ulong, DiscordEmoji> discordEmojis)
        {
            try
            {
                foreach (var emoji in discordEmojis)
                {
                    GuildEmoji? guildEmoji = await _context.GuildEmoji.FirstOrDefaultAsync(x => x.GuildId == guildId && x.Id == emoji.Value.Id);

                    if (guildEmoji == null) { continue; }

                    guildEmoji.DiscordName = emoji.Value.GetDiscordName();

                    _context.GuildEmoji.Update(guildEmoji);
                }

                await _context.SaveChangesAsync();

                Log.Information("UpdateGuildEmojiesAsync guildId: {guildId}, {count}", guildId, discordEmojis.Count);
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task SetGuildUserEmojiCountAsync(ulong guildId, ulong userId, DiscordEmoji discordEmoji, int count)
        {
            try
            {
                GuildUserEmoji? guildUserEmoji = await _context.GuildUserEmoji.FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId && x.EmojiId == discordEmoji.Id);

                if (guildUserEmoji == null) return;

                guildUserEmoji.RecievedAmount = count;

                _context.GuildUserEmoji.Update(guildUserEmoji);

                await _context.SaveChangesAsync();

                Log.Information("SetGuildUserEmojiCountAsync guildId: {guildId}, userId: {userId}, emoji: {emoji}, {count}", guildId, userId, discordEmoji.GetDiscordName(), count);
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public string GetGuildUserEmojiCacheKey(ulong guildId, ulong userId, ulong emojiId)
        {
            return $"GuildUserEmoji_{guildId}_{userId}_{emojiId}";
        }
    }
}
