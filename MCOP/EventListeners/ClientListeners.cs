using DSharpPlus;
using DSharpPlus.EventArgs;
using MCOP.Core.Services.Scoped;
using Serilog;

namespace MCOP.EventListeners;

public sealed class ClientListeners
{
    private IGuildUserStatsService _guildUserStatsService;
    public ClientListeners(IGuildUserStatsService guildUserStatsService) 
    {
        _guildUserStatsService = guildUserStatsService;
    }

    public Task GuildAvailableEventHandler(DiscordClient client, GuildCreatedEventArgs e)
    {
        Log.Information("Available: {AvailableGuild}", e.Guild);
        return Task.CompletedTask;
    }

    public Task GuildUnvailableEventHandler(DiscordClient client, GuildDeletedEventArgs e)
    {
        Log.Warning("Unvailable: {UnvailableGuild}", e.Guild);
        return Task.CompletedTask;
    }

    public Task GuildDownloadCompletedEventHandler(DiscordClient client, GuildDownloadCompletedEventArgs e)
    {
        Log.Information("All guilds are now downloaded ({Count} total)", e.Guilds.Count);
        return Task.CompletedTask;
    }

    public async Task GuildCreateEventHandler(DiscordClient client, GuildCreatedEventArgs e)
    {
        Log.Information("Joined {NewGuild}", e.Guild);
        var guildService = client.ServiceProvider.GetRequiredService<IGuildConfigService>();
        await guildService.GetOrAddGuildConfigAsync(e.Guild.Id);
    }

    public Task SocketOpenedEventHandler(DiscordClient client, SocketEventArgs _)
    {
        Log.Debug("Socket opened");
        client.ServiceProvider.GetRequiredService<IBotStatusesService>().GetUptimeInfo().SocketStartTime = DateTimeOffset.Now;
        return Task.CompletedTask;
    }

    public Task SocketClosedEventHandler(DiscordClient client, SocketClosedEventArgs e)
    {
        Log.Debug("Socket closed with code {Code}: {Message}", e.CloseCode, e.CloseMessage);
        return Task.CompletedTask;
    }

    public Task SocketErroredEventHandler(DiscordClient client, SocketErrorEventArgs e)
    {
        Log.Debug("Socket errored {Message}", e.Exception.Message);
        return Task.CompletedTask;
    }

    public Task UnknownEventHandler(DiscordClient client, UnknownEventArgs e)
    {
        Log.Debug("Unknown event ({UnknownEvent}) occured", e.EventName);
        return Task.CompletedTask;
    }

    public Task UserUpdatedEventHandler(DiscordClient client, UserUpdatedEventArgs e)
    {
        Log.Information("Bot updated");
        return Task.CompletedTask;
    }

    public Task UserSettingsUpdatedEventHandler(DiscordClient client, UserSettingsUpdatedEventArgs e)
    {
        Log.Information("User settings updated");
        return Task.CompletedTask;
    }

    public async Task GuildMemberUpdatedHandler(DiscordClient client, GuildMemberUpdatedEventArgs e)
    {
        if (e.AvatarHashAfter != e.AvatarHashBefore || e.MemberBefore.DisplayName != e.MemberAfter.DisplayName)
            await _guildUserStatsService.UpdateUserInfo(e.Guild.Id, e.Member.Id, e.MemberAfter.DisplayName, e.AvatarHashAfter);
    }

    public async Task GuildMemberAddedHandler(DiscordClient client, GuildMemberAddedEventArgs e)
    {
        await _guildUserStatsService.UpdateUserInfo(e.Guild.Id, e.Member.Id, e.Member.DisplayName, e.Member.AvatarHash);
    }
}
