using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentCollections;
using MCOP.Database;
using MCOP.Database.Models;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MCOP.Modules.Basic.Services
{
    public sealed class ImageHashService : DbAbstractionServiceBase<ImageHash, int>
    {
        private ConcurrentDictionary<int, ImageHash> hashes;

        public ImageHashService(BotDbContextBuilder dbb, bool loadData = true) : base(dbb)
        {
            hashes = new ConcurrentDictionary<int, ImageHash>();

            if (loadData)
            {
                using BotDbContext db = this.dbb.CreateContext();
                hashes = new ConcurrentDictionary<int, ImageHash>(db.ImageHashes.Include(m => m.Message).ToDictionary(h => h.Id, h => h));
                Log.Information("Image Hash loaded: {count}", hashes.Count);
            }
        }

        public override DbSet<ImageHash> DbSetSelector(BotDbContext db) => db.ImageHashes;
        public override ImageHash EntityFactory(int id) => new() { Id = id };
        public override int EntityIdSelector(ImageHash entity) => entity.Id;
        public override object[] EntityPrimaryKeySelector(int id) => new object[] { id };


        public int GetTotalHashes()
        {
            return hashes.Count;
        }

        public new async Task<int> AddAsync(params ImageHash[] entities)
        {
            try
            {
                var count = await base.AddAsync(entities);
                foreach (var item in entities)
                {
                    hashes.TryAdd(item.Id, item);
                }
                return count;
            }
            catch (Exception e)
            {
                Log.Error("ImageHashService AddAsync: {e}", e);
                throw;
            }

        }

        public int RemoveFromHashByMessageId(ulong guildId, ulong messageId)
        {
            try
            {
                var count = 0;
                var toRemove = hashes.Where(m => m.Value.GuildId == guildId && m.Value.MessageId == messageId);
                if (toRemove.Any())
                {
                    foreach (var item in toRemove)
                    {
                        var isRemoved = hashes.TryRemove(item);
                        if (isRemoved) { count += 1; }
                    }
                }
                return count;
            }
            catch (Exception e)
            {
                Log.Error("ImageHashService RemoveFromHashByMessageId: {e}", e);
                throw;
            }
        }

        public bool TryGetSimilar(byte[] hash, double minProcent, out ulong messageId, out double procent)
        {
            try
            {
                foreach (var item in hashes)
                {
                    double diff = ImageProcessorService.GetPercentageDifference(hash, item.Value.Hash);
                    if (diff >= minProcent)
                    {
                        messageId = item.Value.MessageId;
                        procent = diff;
                        return true;
                    }
                }

                messageId = 0;
                procent = 0;
                return false;
            }
            catch (Exception e)
            {
                Log.Error("ImageHashService TryGetSimilar: {e}", e);
                throw;
            }

        }
    }
}
