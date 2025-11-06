using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using Polly;
using static MCOP.Core.Services.Scoped.ReactionService;

namespace MCOP.Core.Services.Scoped
{
    public interface IReactionService
    {
        Task<bool> AddUnicodeReactionAsync(
            ulong guildId, ulong channelId, ulong messageId, ulong createdbyUserId, ulong messageUserId, string emoji);

        Task<bool> AddCustomReactionAsync(
            ulong guildId, ulong channelId, ulong messageId, ulong createdByUserId, ulong messageUserId, string emojiName, ulong emojiId);

        Task<bool> RemoveReactionAsync(
            ulong guildId, ulong channelId, ulong messageId, ulong createdByUserId, string emoji, ulong emojiId = 0);

        Task<int> GetMessageReactionCountAsync(
            ulong guildId, ulong messageId, string emoji, ulong? emojiId = null);

        Task<int> GetMessageTotalReactionCountAsync(
            ulong guildId, ulong messageId);

        Task<List<ulong>> GetMessageReactorsAsync(
            ulong guildId, ulong messageId, string emoji, ulong? emojiId = null);

        Task<List<EmojiInfo>> GetMessageEmojisAsync(
            ulong guildId, ulong messageId);

        Task<int> GetUserReactionsCount(
            ulong guildId, ulong userId, string emoji, ulong? emojiId = null);

        Task<List<UserReactionCount>> GetUserTopReactionsAsync(
            ulong guildId, ulong userId, int limit = 6);
    }

    public class ReactionService: IReactionService
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;
        private readonly IGuildConfigService _guildConfigService;

        public ReactionService(
            IDbContextFactory<McopDbContext> contextFactory,
            IGuildConfigService guildConfigService)
        {
            _contextFactory = contextFactory;
            _guildConfigService = guildConfigService;
        }

        public record EmojiInfo(string Emoji, ulong EmojiId);
        public record UserReactionCount(string Emoji, ulong EmojiId, int Count);

        public async Task<bool> AddUnicodeReactionAsync(
            ulong guildId, ulong channelId, ulong messageId, ulong createdByUserId, ulong messageUserId, string emoji)
        {
            await EnsureMessageExistsAsync(guildId, messageId, messageUserId, channelId);

            var reaction = new GuildMessageReaction
                {
                    GuildId = guildId,
                    MessageId = messageId,
                    Emoji = emoji,
                    EmojiId = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = createdByUserId,
                };

            try
            {
                await using var context = _contextFactory.CreateDbContext();

                context.GuildMessageReactions.Add(reaction);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> AddCustomReactionAsync(
            ulong guildId, ulong channelId, ulong messageId, ulong createdByUserId, ulong messageUserId, string emoji, ulong emojiId)
        {
            await EnsureMessageExistsAsync(guildId, messageId, messageUserId, channelId);

            var reaction = new GuildMessageReaction
                {
                    GuildId = guildId,
                    MessageId = messageId,
                    Emoji = emoji,
                    EmojiId = emojiId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = createdByUserId,
                };

            try
            {
                await using var context = _contextFactory.CreateDbContext();

                context.GuildMessageReactions.Add(reaction);
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<bool> RemoveReactionAsync(ulong guildId, ulong channelId, ulong messageId, ulong userId, string emoji, ulong emojiId = 0)
        {
            await using var context = _contextFactory.CreateDbContext();

            var reaction = await context.GuildMessageReactions
                .FirstOrDefaultAsync(r => r.GuildId == guildId &&
                                        r.MessageId == messageId &&
                                        r.CreatedByUserId == userId &&
                                        r.EmojiId == emojiId &&
                                        r.Message.ChannelId == channelId &&
                                        (emojiId != 0 || r.Emoji == emoji));

            if (reaction != null)
            {
                context.GuildMessageReactions.Remove(reaction);
                await context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<int> GetMessageReactionCountAsync(ulong guildId, ulong messageId, string emoji, ulong? emojiId = null)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (emojiId.HasValue)
            {
                return await context.GuildMessageReactions
                    .CountAsync(r => r.GuildId == guildId &&
                                   r.MessageId == messageId &&
                                   r.EmojiId == emojiId);
            }
            else
            {
                return await context.GuildMessageReactions
                    .CountAsync(r => r.GuildId == guildId &&
                                   r.MessageId == messageId &&
                                   r.Emoji == emoji &&
                                   r.EmojiId == 0);
            }
        }

        public async Task<int> GetMessageTotalReactionCountAsync(ulong guildId, ulong messageId)
        {
            await using var context = _contextFactory.CreateDbContext();

            return await context.GuildMessageReactions
                .CountAsync(r => r.GuildId == guildId && r.MessageId == messageId);
        }

        public async Task<List<ulong>> GetMessageReactorsAsync(ulong guildId, ulong messageId, string emoji, ulong? emojiId = null)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (emojiId.HasValue)
            {
                return await context.GuildMessageReactions
                    .Where(r => r.GuildId == guildId &&
                               r.MessageId == messageId &&
                               r.EmojiId == emojiId &&
                               r.CreatedByUserId != 0)
                    .Select(r => r.CreatedByUserId)
                    .ToListAsync();
            }
            else
            {
                return await context.GuildMessageReactions
                    .Where(r => r.GuildId == guildId &&
                               r.MessageId == messageId &&
                               r.Emoji == emoji &&
                               r.EmojiId == 0 &&
                               r.CreatedByUserId != 0)
                    .Select(r => r.CreatedByUserId)
                    .ToListAsync();
            }
        }

        public async Task<List<EmojiInfo>> GetMessageEmojisAsync(ulong guildId, ulong messageId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var emojiData = await context.GuildMessageReactions
                .Where(r => r.GuildId == guildId && r.MessageId == messageId)
                .Select(r => new { r.Emoji, r.EmojiId })
                .Distinct()
                .ToListAsync();

            return emojiData.Select(x => new EmojiInfo(x.Emoji, x.EmojiId)).ToList();
        }

        public async Task<int> GetUserReactionsCount(
            ulong guildId,
            ulong userId,
            string emoji,
            ulong? emojiId = null)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (emojiId.HasValue)
            {
                return await context.GuildMessageReactions
                    .Where(r => r.GuildId == guildId &&
                                r.EmojiId == emojiId &&
                                r.Message.UserId == userId)
                    .CountAsync();
            }
            else
            {
                return await context.GuildMessageReactions
                    .Where(r => r.GuildId == guildId &&
                                r.Emoji == emoji &&
                                r.EmojiId == 0 &&
                                r.Message.UserId == userId)
                    .CountAsync();
            }
        }

        public async Task<List<UserReactionCount>> GetUserTopReactionsAsync(ulong guildId, ulong userId, int limit = 6)
        {
            await using var context = _contextFactory.CreateDbContext();

            int historicalLikeCount = 0;
            var userStats = await context.GuildUserStats
                .AsNoTracking()
                .FirstOrDefaultAsync(us => us.GuildId == guildId && us.UserId == userId);
            if (userStats != null)
                historicalLikeCount = userStats.Likes;

            var rawReactions = await context.GuildMessageReactions
                .Where(r => r.GuildId == guildId)
                .Join(
                    context.GuildMessages,
                    reaction => new { reaction.GuildId, reaction.MessageId },
                    message => new { message.GuildId, MessageId = message.Id },
                    (reaction, message) => new { reaction, message.UserId })
                .Where(x => x.UserId == userId)
                .GroupBy(x => new {
                    x.reaction.EmojiId,
                    EmojiGroup = x.reaction.EmojiId != 0 ? null : x.reaction.Emoji
                })
                .Select(g => new
                {
                    Emoji = g.First().reaction.EmojiId != 0 ? g.First().reaction.Emoji : g.Key.EmojiGroup!,
                    EmojiId = g.Key.EmojiId,
                    BaseCount = g.Count()
                })
                .ToListAsync();

            var allReactions = new List<UserReactionCount>();

            foreach (var r in rawReactions)
            {
                int totalCount = r.BaseCount;

                if (r.EmojiId == 0 && r.Emoji == "❤️")
                    totalCount += historicalLikeCount;

                allReactions.Add(new UserReactionCount(r.Emoji, r.EmojiId, totalCount));
            }

            if (historicalLikeCount > 0 && !allReactions.Any(rc => rc.EmojiId == 0 && rc.Emoji == "❤️"))
                allReactions.Add(new UserReactionCount("❤️", 0, historicalLikeCount));

            return allReactions
                .OrderByDescending(rc => rc.Count)
                .Take(limit)
                .ToList();
        }

        private async Task EnsureMessageExistsAsync(ulong guildId, ulong messageId, ulong userId, ulong channelId)
        {
            await using var context = _contextFactory.CreateDbContext();

            var existing = await context.GuildMessages
                .AnyAsync(m => m.GuildId == guildId && m.Id == messageId);

            if (!existing)
            {
                var message = new GuildMessage
                {
                    GuildId = guildId,
                    Id = messageId,
                    UserId = userId,
                    ChannelId = channelId,
                    ImageHashes = new List<ImageHash>()
                };

                context.GuildMessages.Add(message);
                await context.SaveChangesAsync();
            }
        }
    }
}
