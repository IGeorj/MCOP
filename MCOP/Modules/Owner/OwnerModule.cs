using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Common;
using MCOP.Core.Services.Background;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Shared;
using MCOP.Exceptions;
using MCOP.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace MCOP.Modules.Owner;

[Command("owner")]
[RequireApplicationOwner]
public sealed class OwnerModule
{
    #region avatar
    [Command("avatar")]
    public async Task SetBotAvatarAsync(CommandContext ctx,
                                       [Description("Image URL")] string url)
    {
        if (!await new Uri(url).ContentTypeHeaderIsImageAsync(DiscordLimits.AvatarSizeLimit))
            throw new CommandFailedException($"URL must point to an image and use HTTP or HTTPS protocols and have size smaller than {DiscordLimits.AvatarSizeLimit}B");

        try
        {
            using MemoryStream ms = await HttpService.GetMemoryStreamAsync(url);
            await ctx.Client.ModifyCurrentUserAsync(avatar: ms);
        }
        catch (WebException e)
        {

            throw new CommandFailedException(e, "Failed to fetch the image");
        }
    }
    #endregion

    #region name
    [Command("name")]
    public async Task SetBotNameAsync(CommandContext ctx,
                                     [RemainingText, Description("New name")] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCommandUsageException("Name not provided!");

        if (name.Length > DiscordLimits.NameLimit)
            throw new InvalidCommandUsageException($"Name must be shorter than {DiscordLimits.NameLimit} characters!");

        await ctx.Client.ModifyCurrentUserAsync(username: name);
    }
    #endregion

    #region leaveguilds
    [Command("leaveguilds")]
    public async Task LeaveGuildsAsync(CommandContext ctx,
                                      [Description("Guilds to leave")] params ulong[] gids)
    {
        if (gids is null || !gids.Any())
        {
            if (ctx.Guild is null) return;
            await ctx.Guild.LeaveAsync();
            return;
        }

        var eb = new StringBuilder();
        foreach (ulong gid in gids)
        {
            try
            {
                if (ctx.Client.Guilds.TryGetValue(gid, out DiscordGuild? guild))
                    await guild.LeaveAsync();
                else
                    eb.AppendLine($"Error: Failed to leave the guild with ID: `{gid}`!");
            }
            catch
            {
                eb.AppendLine($"Warning: I am not a member of the guild with ID: `{gid}`!");
            }
        }

        if (ctx.Guild is { } && !gids.Contains(ctx.Guild.Id))
        {
            if (eb.Length > 0)
                await ctx.FailAsync("Action finished with following warnings/errors:\n\n{eb}");
        }
    }
    #endregion

    #region sendmessage
    [Command("sendmessage")]
    public async Task SendAsync(CommandContext ctx,
                               [RemainingText, Description("Message")] string message,
                               [Description("ChannelID")] ulong? cid = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidCommandUsageException("Message missing");

        DiscordChannel channel = await ctx.Client.GetChannelAsync(cid ?? ctx.Channel.Id);
        await channel.SendMessageAsync(message);
    }
    #endregion

    #region shutdown
    [Command("shutdown")]
    public async Task ExitAsync(CommandContext ctx,
                         [Description("Time until exit")] TimeSpan timespan,
                         [Description("Exit code")] int exitCode = 0)
        => await ctx.Client.ServiceProvider.GetRequiredService<BotBackgroundService>().StopBotAsync();
    #endregion

    #region restart
    [Command("restart")]
    public Task RestartAsync(CommandContext ctx)
        => ExitAsync(ctx, new TimeSpan(0), 100);
    #endregion

    #region update
    [Command("update")]
    public Task UpdateAsync(CommandContext ctx)
        => ExitAsync(ctx, new TimeSpan(0), 101);
    #endregion

    #region uptime
    [Command("uptime")]
    public async Task UptimeAsync(CommandContext ctx)
    {
        IBotStatusesService bas = ctx.ServiceProvider.GetRequiredService<IBotStatusesService>();
        TimeSpan processUptime = bas.GetUptimeInfo().ProgramUptime;
        TimeSpan socketUptime = bas.GetUptimeInfo().SocketUptime;

        var emb = new DiscordEmbedBuilder
        {
            Title = "Uptime information",
            Color = DiscordColor.Gold,
        };
        emb.AddField("Bot uptime", processUptime.ToString(@"dd\.hh\:mm\:ss"), inline: true);
        emb.AddField("Socket uptime", socketUptime.ToString(@"dd\.hh\:mm\:ss"), inline: true);

        await ctx.RespondAsync(emb.Build());
    }
    #endregion
}
