﻿using MCOP.Database;
using MCOP.Database.Models;
using MCOP.Services;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Modules.Owner.Services;

public sealed class PrivilegedUserService : DbAbstractionServiceBase<PrivilegedUser, ulong>
{
    public PrivilegedUserService(BotDbContextBuilder dbb)
        : base(dbb) { }


    public override DbSet<PrivilegedUser> DbSetSelector(BotDbContext db) => db.PrivilegedUsers;
    public override PrivilegedUser EntityFactory(ulong id) => new PrivilegedUser { UserId = id };
    public override ulong EntityIdSelector(PrivilegedUser entity) => entity.UserId;
    public override object[] EntityPrimaryKeySelector(ulong id) => new object[] { (long)id };
}
