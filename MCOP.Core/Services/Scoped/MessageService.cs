using MCOP.Core.Exceptions;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class MessageService : IScoped
    {
        private readonly McopDbContext _context;
        private readonly UserService _userService;

        public MessageService(McopDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<bool> ChangeLikeAsync(ulong guildId, ulong messageId, int count)
        {
            try
            {
                GuildMessage? message = await _context.GuildMessages.FindAsync(guildId, messageId);
                if (message is null)
                {
                    return false;
                }
                message.Likes += count;

                _context.GuildMessages.Update(message);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<GuildMessage> GetOrAddAsync(ulong guildId, ulong messageId, ulong userId)
        {
            try
            {
                GuildMessage? message = await _context.GuildMessages.FindAsync(guildId, messageId);
                if (message is null)
                {
                    var guildUser = await _userService.GetOrAddUserAsync(guildId, userId);
                    message = (await _context.GuildMessages.AddAsync(new GuildMessage
                    {
                        GuildId = guildUser.GuildId,
                        Id = messageId,
                        UserId = guildUser.UserId,
                    })).Entity;
                    await _context.SaveChangesAsync();
                }

                return message;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task RemoveMessageAsync(ulong guildId, ulong messageId)
        {
            try
            {
                var message = await _context.GuildMessages.FindAsync(guildId, messageId);
                if (message is not null)
                {
                    var hashesCount = await _context.ImageHashes.CountAsync(x => x.GuildId == guildId && x.MessageId == messageId);
                    _context.GuildMessages.Remove(message);
                    await _context.SaveChangesAsync();
                    var removedHashes = await _context.ImageHashes.CountAsync();
                    Log.Information("Removed {Amount} hashes ({Total} total)", hashesCount, removedHashes);
                }
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
    }
}
