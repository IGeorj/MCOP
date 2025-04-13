using DSharpPlus;
using DSharpPlus.EventArgs;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    public static Task GuildAvailableEventHandler(DiscordClient client, GuildCreatedEventArgs e)
    {
        Log.Information("Available: {AvailableGuild}", e.Guild);
        return Task.CompletedTask;
    }

    public static Task GuildUnvailableEventHandler(DiscordClient client, GuildDeletedEventArgs e)
    {
        Log.Warning("Unvailable: {UnvailableGuild}", e.Guild);
        return Task.CompletedTask;
    }

    public static Task GuildDownloadCompletedEventHandler(DiscordClient client, GuildDownloadCompletedEventArgs e)
    {
        Log.Information("All guilds are now downloaded ({Count} total)", e.Guilds.Count);
        return Task.CompletedTask;
    }

    public static async Task GuildCreateEventHandler(DiscordClient client, GuildCreatedEventArgs e)
    {
        Log.Information("Joined {NewGuild}", e.Guild);
        var guildService = Services.GetRequiredService<GuildConfigService>();
        await guildService.GetOrAddGuildConfigAsync(e.Guild.Id);
    }

    public static Task SocketOpenedEventHandler(DiscordClient client, SocketEventArgs _)
    {
        Log.Debug("Socket opened");
        Services.GetRequiredService<BotStatusesService>().UptimeInformation.SocketStartTime = DateTimeOffset.Now;
        return Task.CompletedTask;
    }

    public static Task SocketClosedEventHandler(DiscordClient client, SocketClosedEventArgs e)
    {
        Log.Debug("Socket closed with code {Code}: {Message}", e.CloseCode, e.CloseMessage);
        return Task.CompletedTask;
    }

    public static Task SocketErroredEventHandler(DiscordClient client, SocketErrorEventArgs e)
    {
        Log.Debug("Socket errored {Message}", e.Exception.Message);
        return Task.CompletedTask;
    }

    public static Task UnknownEventHandler(DiscordClient client, UnknownEventArgs e)
    {
        Log.Debug("Unknown event ({UnknownEvent}) occured", e.EventName);
        return Task.CompletedTask;
    }

    public static Task UserUpdatedEventHandler(DiscordClient client, UserUpdatedEventArgs e)
    {
        Log.Information("Bot updated");
        return Task.CompletedTask;
    }

    public static Task UserSettingsUpdatedEventHandler(DiscordClient client, UserSettingsUpdatedEventArgs e)
    {
        Log.Information("User settings updated");
        return Task.CompletedTask;
    }
}
