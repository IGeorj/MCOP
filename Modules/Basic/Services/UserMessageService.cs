using MCOP.Database;
using MCOP.Database.Models;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.Modules.Basic.Services
{
    public sealed class UserMessageService : DbAbstractionServiceBase<UserMessage, ulong, ulong>
    {
        public UserMessageService(BotDbContextBuilder dbb) : base(dbb)
        {
        }

        public override DbSet<UserMessage> DbSetSelector(BotDbContext db)
            => db.UserMessages;
        public override IQueryable<UserMessage> GroupSelector(IQueryable<UserMessage> bds, ulong gid)
            => bds.Where(bd => bd.GuildIdDb == (long)gid);
        public override ulong EntityGroupSelector(UserMessage bd)
            => bd.GuildId;
        public override UserMessage EntityFactory(ulong gid, ulong mid) => new UserMessage { GuildId = gid, MessageId = mid };
        public override ulong EntityIdSelector(UserMessage entity) => entity.MessageId;
        public override object[] EntityPrimaryKeySelector(ulong gid, ulong mid) => new object[] { (long)gid, (long)mid };

        public async Task<UserMessage> GetOrAddAsync(ulong gid, ulong mid)
        {
            UserMessage? message = await this.GetAsync(gid, mid);

            if (message is not null)
            {
                return message;
            }

            UserMessage createdMessage = new UserMessage()
            {
                GuildId = gid,
                MessageId = mid
            };
            await this.AddAsync(createdMessage);
            return createdMessage;
        }
    }
}
