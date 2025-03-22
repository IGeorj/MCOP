using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MCOP.Exceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
namespace MCOP.EventListeners;

internal static partial class Listeners
{
    public static Task CommandExecutionEventHandler(CommandsExtension commandEx, CommandExecutedEventArgs e)
    {
        if (e.CommandObject is null || e.Context.Command.Name.StartsWith("help"))
            return Task.CompletedTask;

        Log.Information(
            "Executed: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Context.Command.Name, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    public static async Task CommandErrorEventHandler(CommandsExtension commandEx, CommandErroredEventArgs e)
    {
        if (e.Exception is null)
            return;

        Exception ex = e.Exception;
        while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
            ex = ex.InnerException;

        Log.Error(
            "Command errored ({ExceptionName}): {ErroredCommand} {User} {Guild} {Channel}",
            e.Exception?.GetType().Name ?? "Unknown", e.Context.Command?.Name ?? "Unknown",
            e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );

        var emb = new DiscordEmbedBuilder()
        {
            Title = "ОШИБОЧНАЯ!",
            Description = e.Context.Command?.Name ?? "",
            Color = DiscordColor.Red,
        };

        switch (ex)
        {
            case TaskCanceledException:
                return;
            case UnauthorizedException _:
                emb.WithDescription("403");
                break;
            case DbUpdateException or DbUpdateConcurrencyException:
                Log.Error(ex, "Database error");
                return;
            case CommandCancelledException:
                break;
            default:
                emb.WithDescription(ex.Message);
                Log.Error(ex, "Unhandled error");
                break;
        }

        await e.Context.RespondAsync(embed: emb.Build());
    }
}
