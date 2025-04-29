using DSharpPlus.Entities;
using MCOP.Core.Exceptions;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Shared;
using MCOP.Core.ViewModels;
using MCOP.Data;
using MCOP.Data.Models;
using MCOP.Utils.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Core.Services.Scoped
{
    public class ImageHashService : IScoped
    {
        private readonly IDbContextFactory<McopDbContext> _contextFactory;

        private const double _defaultNormalizedThreshold = 99.5;
        private const double _defaultDiffThreshold = 95;

        public ImageHashService(IDbContextFactory<McopDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<ImageHash>> GetAllHashesAsync()
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                return (await context.ImageHashes.ToListAsync()).OrderByDescending(x => x.Id).ToList();
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
                await using var context = _contextFactory.CreateDbContext();

                return (await context.ImageHashes.Where(x => x.GuildId == guildId).ToListAsync()).OrderByDescending(x => x.Id).ToList();
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
                await using var context = _contextFactory.CreateDbContext();

                GuildMessage? message = await context.GuildMessages.FindAsync(guildId, messageId);
                if (message is null)
                {
                    message = (await context.GuildMessages.AddAsync(new GuildMessage
                    {
                        GuildId = guildId,
                        Id = messageId,
                        UserId = userId,
                    })).Entity;

                    await context.SaveChangesAsync();
                }

                await context.ImageHashes.AddAsync(new ImageHash
                {
                    Hash = hash,
                    MessageId = message.Id,
                    GuildId = message.GuildId
                });

                await context.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<HashSearchResultVM>> SearchHashesAsync(List<byte[]> hashes, double diffThreshold = _defaultDiffThreshold, double normalizedThreshold = _defaultNormalizedThreshold)
        {
            try
            {
                List<ImageHash> hashesDB = await GetAllHashesAsync();
                return FindBestMatches(hashesDB, hashes, diffThreshold, normalizedThreshold);

            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public async Task<List<HashSearchResultVM>> SearchHashesByGuildAsync(ulong guildId, List<byte[]> hashes, double diffThreshold = _defaultDiffThreshold, double normalizedThreshold = _defaultNormalizedThreshold)
        {
            try
            {
                List<ImageHash> hashesDB = await GetHashesByGuildAsync(guildId);
                return FindBestMatches(hashesDB, hashes, diffThreshold, normalizedThreshold);
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
                    try
                    {
                        byte[] imgBytes = await HttpService.GetByteArrayAsync(link);
                        using var bitmap = SkiaSharp.SKBitmap.Decode(imgBytes);
                        hashes.Add(SkiaSharpService.GetBitmapHash(bitmap));
                    }
                    catch (Exception)
                    {
                        Log.Information("GetHashesFromMessageAsync link image not working: {link}", link);
                    }
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
            await using var context = _contextFactory.CreateDbContext();

            var toRemove = await context.ImageHashes.Where(x => x.GuildId == guildId && x.MessageId == messageId).ToListAsync();

            if (toRemove.Count > 0)
            {
                context.ImageHashes.RemoveRange(toRemove);
            }

            await context.SaveChangesAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                await using var context = _contextFactory.CreateDbContext();

                return await context.ImageHashes.CountAsync();
            }
            catch (Exception ex)
            {
                throw new McopException(ex, ex.Message);
            }
        }

        public List<HashSearchResultVM> FindBestMatches(List<ImageHash> imageHashes, List<byte[]> hashesToCheck, double diffThreshold = _defaultDiffThreshold, double normalizedThreshold = _defaultNormalizedThreshold)
        {
            List<HashSearchResultVM> results = [];
            hashesToCheck.ForEach(checkedHash => results.Add(new HashSearchResultVM() { HashToCheck = checkedHash}));

            foreach (var imageHash in imageHashes)
            {
                if (results.All(x => x.Difference > diffThreshold))
                {
                    foreach (var res in results)
                    {
                        Log.Information("FindBestMatches messageId:{res.MessageId} {bestMatch}%", res.MessageId, res.Difference);
                    }
                    return results;
                }

                for (int i = 0; i < hashesToCheck.Count; i++)
                {
                    double diff = SkiaSharpService.GetPercentageDifference(imageHash.Hash, hashesToCheck[i]);

                    if (diff > results[i].Difference)
                    {
                        results[i].Difference = diff;
                        if(diff > diffThreshold)
                        {
                            results[i].MessageId = imageHash.MessageId;
                            results[i].HashFound = imageHash.Hash;
                        }
                    }
                }
            }

            foreach (var imageHash in imageHashes)
            {
                if (results.All(x => x.Difference > diffThreshold || x.DifferenceNormalized > normalizedThreshold))
                {
                    foreach (var res in results)
                    {
                        Log.Information("FindBestMatches normalized messageId:{res.MessageId} {bestMatch}%", res.MessageId, res.DifferenceNormalized);
                    }
                    return results;
                }

                for (int i = 0; i < hashesToCheck.Count; i++)
                {
                    if (results[i].Difference > diffThreshold || results[i].DifferenceNormalized > normalizedThreshold)
                    {
                        continue;
                    }

                    double diff = SkiaSharpService.GetNormalizedDifference(imageHash.Hash, hashesToCheck[i]);

                    if (diff > results[i].DifferenceNormalized)
                    {
                        results[i].DifferenceNormalized = diff;
                        if (diff > normalizedThreshold)
                        {
                            results[i].MessageIdNormalized = imageHash.MessageId;
                            results[i].HashFoundNormalized = imageHash.Hash;
                        }
                    }
                }
            }

            return results;
        }
    }
}
