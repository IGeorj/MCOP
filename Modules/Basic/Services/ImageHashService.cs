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
        private ConcurrentHashSet<ImageHash> hashes;

        public ImageHashService(BotDbContextBuilder dbb, bool loadData = true) : base(dbb)
        {
            hashes = new ConcurrentHashSet<ImageHash>();

            if (loadData)
            {
                using (BotDbContext db = this.dbb.CreateContext())
                {
                    hashes = new ConcurrentHashSet<ImageHash>(db.ImageHashes.Include(m => m.Message).AsEnumerable());
                    Log.Information("Image Hash loaded: {count}", hashes.Count);
                }
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
            foreach (var item in entities)
            {
                hashes.Add(item);
            }
            return await base.AddAsync(entities);
        }

        public int RemoveFromHashByMessageId(ulong guildId, ulong messageId)
        {
            var count = 0;
            var toRemove = hashes.Where(m => m.GuildId == guildId && m.MessageId == messageId);
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

        public bool TryGetSimilar(byte[] hash, double minProcent, out ulong messageId, out double procent)
        {
            foreach (var item in hashes)
            {
                double diff = ImageProcessorService.GetPercentageDifference(hash, item.Hash);
                if (diff >= minProcent)
                {
                    messageId = item.MessageId;
                    procent = diff;
                    return true;
                }
            }

            messageId = 0;
            procent = 0;
            return false;
        }
    }
}
