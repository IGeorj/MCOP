using DSharpPlus.Entities;
using MCOP.Core.Exceptions;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Core.Services.Scoped
{
    public class GuildEmojiService : IScoped
    {
        private readonly McopDbContext _context;
        private readonly GuildService _guildService;
        private readonly UserService _userService;

        public GuildEmojiService(McopDbContext context, GuildService guildService, UserService userService)
        {
            _context = context;
            _guildService = guildService;
            _userService = userService;
        }

        public async Task<GuildEmoji> GetOrAddGuildEmojiAsync(ulong guildId, ulong emojiId, string emojiName)
        {
            try
            {
                GuildEmoji? guildEmoji = await _context.GuildEmoji.FindAsync(emojiId);


                if (guildEmoji is null)
                {
                    guildEmoji = (await _context.GuildEmoji.AddAsync(new GuildEmoji() { Id = emojiId, GuildId = guildId, DiscordName = emojiName })).Entity;
                    await _context.SaveChangesAsync();
                }
                return guildEmoji;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<GuildUserEmoji> GetOrAddGuildUserEmojiAsync(DiscordEmoji emoji, ulong guildId, ulong userId)
        {
            try
            {
                GuildUserEmoji? guildUserEmoji = await _context.GuildUserEmoji.FindAsync(guildId, userId, emoji.Id);

                if (guildUserEmoji is null)
                {
                    GuildUser guildUser = await _userService.GetOrAddUserAsync(guildId, userId);

                    guildUserEmoji = (await _context.GuildUserEmoji.AddAsync(new GuildUserEmoji() { GuildId = guildUser.GuildId, UserId = guildUser.UserId, EmojiId = emoji.Id })).Entity;
                    await _context.SaveChangesAsync();
                }

                return guildUserEmoji;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<bool> ChangeGuildUserEmojiRecievedCount(DiscordEmoji emoji, ulong guildId, ulong userId, int count)
        {
            try
            {
                GuildEmoji guildEmoji = await GetOrAddGuildEmojiAsync(guildId, emoji.Id, emoji.GetDiscordName());

                GuildUserEmoji guildUserEmoji = await GetOrAddGuildUserEmojiAsync(emoji, guildId, userId);

                guildUserEmoji.RecievedAmount += count;

                _context.GuildUserEmoji.Update(guildUserEmoji);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<GuildUserEmoji>> GetUserTopEmoji(ulong guildId, ulong userId, int topAmount = 5)
        {
            try
            {
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
    }
}
