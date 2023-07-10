using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Text;
using MCOP.Attributes;
using MCOP.Common;
using MCOP.Exceptions;
using MCOP.Extensions;
using MCOP.Modules.Owner.Common;
using MCOP.Modules.Owner.Services;
using MCOP.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Newtonsoft.Json;
using MCOP.Core.Common;
using MCOP.Core.Services.Shared;
using MCOP.Core.Services.Scoped;

namespace MCOP.Modules.Owner;

[Group("owner"), Hidden]
[Aliases("admin", "o")]
public sealed class OwnerModule : BotModule
{
    [Command("guildinfo")]
    [Aliases("gi")]
    [RequireOwner]
    public async Task GuildInfo(CommandContext ctx, string guildId)
    {
        var id = ulong.Parse(guildId);
        var guild = await ctx.Client.GetGuildAsync(id);
        var members = await guild.GetAllMembersAsync();
        var jsonG = JsonConvert.SerializeObject(guild, Formatting.Indented);
        var jsonM = JsonConvert.SerializeObject(members, Formatting.Indented);

        File.WriteAllText(@"test/gi.json", jsonG);
        File.WriteAllText(@"test/mi.json", jsonM);
    }

    #region avatar
    [Command("avatar")]
    [Aliases("setavatar", "setbotavatar", "profilepic", "a")]
    [RequireOwner]
    public async Task SetBotAvatarAsync(CommandContext ctx,
                                       [Description("Image URL")] Uri url)
    {
        if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.AvatarSizeLimit))
            throw new CommandFailedException($"URL must point to an image and use HTTP or HTTPS protocols and have size smaller than {DiscordLimits.AvatarSizeLimit}B");

        try {
            using MemoryStream ms = await HttpService.GetMemoryStreamAsync(url);
            await ctx.Client.UpdateCurrentUserAsync(avatar: ms);
        } catch (WebException e) {
            throw new CommandFailedException(e, "Failed to fetch the image");
        }

        await ctx.InfoAsync();
    }
    #endregion

    #region name
    [Command("name")]
    [Aliases("botname", "setbotname", "setname")]
    [RequireOwner]
    public async Task SetBotNameAsync(CommandContext ctx,
                                     [RemainingText, Description("New name")] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCommandUsageException("Name not provided!");

        if (name.Length > DiscordLimits.NameLimit)
            throw new InvalidCommandUsageException($"Name must be shorter than {DiscordLimits.NameLimit} characters!");

        await ctx.Client.UpdateCurrentUserAsync(username: name);
        await ctx.InfoAsync();
    }
    #endregion

    #region eval
    [Command("eval")]
    [Aliases("evaluate", "compile", "run", "e", "c", "r", "exec")]
    [RequireOwner]
    public async Task EvaluateAsync(CommandContext ctx,
                                   [RemainingText, Description("C# code snippet to evaluate")] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidCommandUsageException("You need to wrap the code into a code block.");

        Script<object>? snippet = CSharpCompilationService.Compile(code, out ImmutableArray<Diagnostic> diag, out Stopwatch compileTime);
        if (snippet is null)
            throw new InvalidCommandUsageException("You need to wrap the code into a code block.");

        var emb = new DiscordEmbedBuilder();
        if (diag.Any(d => d.Severity == DiagnosticSeverity.Error)) {
            emb.WithTitle("Evaluation failed");
            emb.WithDescription($"Compilation failed after {compileTime.ElapsedMilliseconds}ms with {diag.Length} errors.");
            emb.WithColor(DiscordColor.Red);

            foreach (Diagnostic d in diag.Take(3)) {
                FileLinePositionSpan ls = d.Location.GetLineSpan();
                emb.AddField($"Error at {ls.StartLinePosition.Line}L:{ls.StartLinePosition.Character}C", Formatter.BlockCode(d.GetMessage()));
            }

            if (diag.Length > 3)
                emb.AddField("...", $"**{diag.Length - 3}** errors not displayed.");

            await ctx.RespondAsync(emb.Build());
            return;
        }

        Exception? exc = null;
        ScriptState<object>? res = null;
        var runTime = Stopwatch.StartNew();
        try {
            res = await snippet.RunAsync(new EvaluationEnvironment(ctx));
        } catch (Exception e) {
            exc = e;
        }
        runTime.Stop();

        if (exc is { } || res is null) {
            emb.WithTitle("Program run failed");
            emb.WithDescription("Execution failed after {runTime.ElapsedMilliseconds}ms with `{exc?.GetType()}`: ```{exc?.Message}```");
            emb.WithColor(DiscordColor.Red);
        } else {
            emb.WithTitle("Evaluation successful!");
            emb.WithColor(DiscordColor.Green);
            if (res.ReturnValue is { }) {
                emb.AddField("Result", res.ReturnValue.ToString(), false);
                emb.AddField("Result type", res.ReturnValue.GetType().ToString(), true);
            } else {
                emb.AddField("Result", "No result returned", inline: true);
            }
            emb.AddField("Compilation time (ms)", compileTime.ElapsedMilliseconds.ToString(), true);
            emb.AddField("Evaluation time (ms)", runTime.ElapsedMilliseconds.ToString(), true);
        }

        await ctx.RespondAsync(emb.Build());
    }
    #endregion

    #region leaveguilds
    [Command("leaveguilds"), Priority(1)]
    [Aliases("leave", "gtfo")]
    [RequireOwner]
    public Task LeaveGuildsAsync(CommandContext ctx,
                                [Description("Guilds to leave")] params DiscordGuild[] guilds)
        => this.LeaveGuildsAsync(ctx, guilds.Select(g => g.Id).ToArray());

    [Command("leaveguilds"), Priority(0)]
    public async Task LeaveGuildsAsync(CommandContext ctx,
                                      [Description("Guilds to leave")] params ulong[] gids)
    {
        if (gids is null || !gids.Any()) {
            await ctx.Guild.LeaveAsync();
            return;
        }

        var eb = new StringBuilder();
        foreach (ulong gid in gids) {
            try {
                if (ctx.Client.Guilds.TryGetValue(gid, out DiscordGuild? guild))
                    await guild.LeaveAsync();
                else
                    eb.AppendLine($"Error: Failed to leave the guild with ID: `{gid}`!");
            } catch {
                eb.AppendLine($"Warning: I am not a member of the guild with ID: `{gid}`!");
            }
        }

        if (ctx.Guild is { } && !gids.Contains(ctx.Guild.Id)) {
            if (eb.Length > 0)
                await ctx.FailAsync("Action finished with following warnings/errors:\n\n{eb}");
            else
                await ctx.InfoAsync();
        } else {
            await ctx.InfoAsync();
        }
    }
    #endregion

    #region sendmessage
    [Command("sendmessage")]
    [Aliases("send", "s")]
    public async Task SendAsync(CommandContext ctx,
                               [Description("ChannelID")] ulong cid,
                               [RemainingText, Description("Message")] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidCommandUsageException("Message missing");

            DiscordChannel channel = await ctx.Client.GetChannelAsync(cid);
            await channel.SendMessageAsync(message);

        await ctx.InfoAsync();
    }
    #endregion

    #region shutdown
    [Command("shutdown"), Priority(1)]
    [Aliases("disable", "poweroff", "exit", "quit")]
    public Task ExitAsync(CommandContext _,
                         [Description("Time until exit")] TimeSpan timespan,
                         [Description("Exit code")] int exitCode = 0)
        => Program.Stop(exitCode, timespan);

    [Command("shutdown"), Priority(0)]
    public Task ExitAsync(CommandContext _,
                         [Description("Exit code")] int exitCode = 0)
        => Program.Stop(exitCode);
    #endregion

    #region restart
    [Command("restart")]
    [Aliases("reboot")]
    public Task RestartAsync(CommandContext ctx)
        => this.ExitAsync(ctx, 100);
    #endregion

    #region update
    [Command("update")]
    [RequireOwner]
    public Task UpdateAsync(CommandContext ctx)
        => this.ExitAsync(ctx, 101);
    #endregion

    #region uptime
    [Command("uptime")]
    public Task UptimeAsync(CommandContext ctx)
    {
        ActivityService bas = ctx.Services.GetRequiredService<ActivityService>();
        TimeSpan processUptime = bas.UptimeInformation.ProgramUptime;
        TimeSpan socketUptime = bas.UptimeInformation.SocketUptime;

        var emb = new DiscordEmbedBuilder {
            Title = "Uptime information",
            Description = $"{Program.ApplicationName} {Program.ApplicationVersion}",
            Color = DiscordColor.Gold,
        };
        emb.AddField("Bot uptime", processUptime.ToString(@"dd\.hh\:mm\:ss"), inline: true);
        emb.AddField("Socket uptime", socketUptime.ToString(@"dd\.hh\:mm\:ss"), inline: true);

        return ctx.RespondAsync(emb.Build());
    }
    #endregion
}
