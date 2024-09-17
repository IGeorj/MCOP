using DSharpPlus.Entities;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Shared;
using MCOP.Core.ViewModels;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Core.Services.Scoped
{
    public class ImageHashService : IScoped
    {
        private readonly McopDbContext _context;

        public ImageHashService(McopDbContext context, MessageService messageService)
        {
            _context = context;
        }


        public async Task<List<ImageHash>> GetAllHashesAsync()
        {
            try
            {
                return await _context.ImageHashes.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<ImageHash>> GetHashesByGuildAsync(ulong guildId)
        {
            try
            {
                return await _context.ImageHashes.Where(x => x.GuildId == guildId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<int> SaveHashAsync(ulong guildId, ulong messageId, ulong userId, byte[] hash)
        {
            try
            {
                GuildMessage? message = await _context.GuildMessages.FindAsync(guildId, messageId);
                if (message is null)
                {
                    var guild = await _context.Guilds.FindAsync(guildId);
                    if (guild is null)
                    {
                        guild = (await _context.Guilds.AddAsync(new Guild { Id = guildId })).Entity;
                        await _context.SaveChangesAsync();
                    }
                    var user = await _context.Users.FindAsync(userId);
                    if (user is null)
                    {
                        user = (await _context.Users.AddAsync(new User { Id = userId })).Entity;
                        await _context.SaveChangesAsync();
                    }
                    var guildUser = await _context.GuildUsers.FindAsync(guildId, userId);
                    if (guildUser is null)
                    {
                        guildUser = (await _context.GuildUsers.AddAsync(new GuildUser { GuildId = guildId, UserId = user.Id })).Entity;
                        await _context.SaveChangesAsync();
                    }

                    message = (await _context.GuildMessages.AddAsync(new GuildMessage
                    {
                        GuildId = guildUser.GuildId,
                        Id = messageId,
                        UserId = guildUser.UserId,
                    })).Entity;

                    await _context.SaveChangesAsync();
                }

                await _context.ImageHashes.AddAsync(new ImageHash
                {
                    Hash = hash,
                    MessageId = message.Id,
                    GuildId = message.GuildId
                });

                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<HashFoundVM?> SearchHashAsync(byte[] hash, double diffProcent = 90)
        {
            try
            {
                List<ImageHash> hashes = await GetAllHashesAsync();
                foreach (var item in hashes)
                {
                    double diff = SkiaSharpService.GetPercentageDifference(hash, item.Hash);
                    if (diff >= diffProcent)
                    {
                        return new HashFoundVM
                        {
                            MessageId = item.MessageId,
                            Difference = diff,
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
        public async Task<HashFoundVM?> SearchHashByGuildAsync(ulong guildId, byte[] hash, double diffProcent = 90)
        {
            try
            {
                List<ImageHash> hashes = await GetHashesByGuildAsync(guildId);
                foreach (var item in hashes)
                {
                    double diff = SkiaSharpService.GetPercentageDifference(hash, item.Hash);
                    if (diff >= diffProcent)
                    {
                        return new HashFoundVM
                        {
                            MessageId = item.MessageId,
                            Difference = diff,
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
        public async Task<List<byte[]>> GetHashesFromMessageAsync(DiscordMessage message)
        {
            try
            {
                List<byte[]> hashes = new();

                var links = message.Content.Split("\t\n ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where(s => s.StartsWith("http://") || s.StartsWith("www.") || s.StartsWith("https://"));
                var link = links.FirstOrDefault();

                if (link is not null && (link.Contains(".png") || link.Contains(".jpg") || link.Contains(".jpeg") || link.Contains(".webp")))
                {
                    byte[] imgBytes = await HttpService.GetByteArrayAsync(link);
                    using var bitmap = SkiaSharp.SKBitmap.Decode(imgBytes);
                    hashes.Add(SkiaSharpService.GetBitmapHash(bitmap));
                }

                foreach (var item in message.Attachments.ToList())
                {
                    if (item.MediaType is null)
                    {
                        continue;
                    }

                    string type = item.MediaType;

                    if ((type.Contains("png") || type.Contains("jpeg") || type.Contains("webp")) && item.Url is not null)
                    {
                        byte[] imgBytes = await HttpService.GetByteArrayAsync(item.Url);
                        using var bitmap = SkiaSharp.SKBitmap.Decode(imgBytes);
                        hashes.Add(SkiaSharpService.GetBitmapHash(bitmap));
                    }
                }

                return hashes;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
        public async Task RemoveHashesByMessageId(ulong guildId, ulong messageId)
        {
            var toRemove = await _context.ImageHashes.Where(x => x.GuildId == guildId && x.MessageId == messageId).ToListAsync();

            if (toRemove.Count > 0)
            {
                _context.ImageHashes.RemoveRange(toRemove);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _context.ImageHashes.CountAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }
    }
}
