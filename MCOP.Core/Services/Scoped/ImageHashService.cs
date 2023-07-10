using DSharpPlus.Entities;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Shared;
using MCOP.Core.ViewModels;
using MCOP.Utils.Interfaces;
using MCOP.Data;
using MCOP.Data.Models;
using Microsoft.EntityFrameworkCore;
using MCOP.Core.Exceptions;

namespace MCOP.Core.Services.Scoped
{
    public class ImageHashService : IScoped
    {
        private readonly McopDbContext _context;
        private readonly MessageService _messageService;

        public ImageHashService(McopDbContext context, MessageService messageService)
        {
            _context = context;
            _messageService = messageService;
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
                var message = await _messageService.GetOrAddAsync(guildId, messageId, userId);

                await _context.ImageHashes.AddAsync(new ImageHash
                {
                    Hash = hash,
                    GuildMessage = message
                });
                await _context.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<HashFoundVM?> FindHashAsync(byte[] hash, double diffProcent = 90)
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
        public async Task<HashFoundVM?> FindHashByGuildAsync(ulong guildId, byte[] hash, double diffProcent = 90)
        {
            try
            {
                List<ImageHash> hashes = await GetHashesByGuildAsync(18446744073709551600);
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

                foreach (var item in message.Attachments)
                {
                    string type = item.MediaType;

                    if (type.Contains("png") || type.Contains("jpeg") || type.Contains("webp"))
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
