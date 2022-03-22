﻿using System.Diagnostics;
using MCOP.Database;
using MCOP.Database.Models;
using MCOP.Extensions;
using MCOP.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace MCOP.Services;

public sealed class BotActivityService : DbAbstractionServiceBase<BotStatus, int>, IDisposable
{
    public bool IsBotListening {
        get => this.isBotListening;
        set {
            lock (this.lck)
                this.isBotListening = value;
        }
    }
    public bool StatusRotationEnabled {
        get => this.statusRotationEnabled;
        set {
            lock (this.lck)
                this.statusRotationEnabled = value;
        }
    }
    public CancellationTokenSource MainLoopCts { get; }
    public UptimeInformation UptimeInformation { get; }

    private bool statusRotationEnabled;
    private bool isBotListening;
    private readonly object lck = new();


    public BotActivityService(BotDbContextBuilder dbb)
        : base(dbb)
    {
        this.IsBotListening = true;
        this.MainLoopCts = new CancellationTokenSource();
        this.StatusRotationEnabled = true;
        this.UptimeInformation = new UptimeInformation(Process.GetCurrentProcess().StartTime);
    }


    public bool ToggleListeningStatus()
    {
        lock (this.lck)
            this.IsBotListening = !this.IsBotListening;
        return this.IsBotListening;
    }

    public void Dispose()
    {
        this.MainLoopCts.Dispose();
    }

    public BotStatus? GetRandomStatus()
    {
        using BotDbContext db = this.dbb.CreateContext();
        return this.DbSetSelector(db).Shuffle().FirstOrDefault();
    }

    public override DbSet<BotStatus> DbSetSelector(BotDbContext db) => db.BotStatuses;
    public override BotStatus EntityFactory(int id) => new() { Id = id };
    public override int EntityIdSelector(BotStatus entity) => entity.Id;
    public override object[] EntityPrimaryKeySelector(int id) => new object[] { id };
}
