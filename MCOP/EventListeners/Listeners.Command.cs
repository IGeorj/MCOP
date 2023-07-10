using System.Reflection;
using MCOP.Common;
using MCOP.Exceptions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using MCOP.Attributes.SlashCommands;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    public static Task CommandExecutionEventHandler(CommandsNextExtension cNext, CommandExecutionEventArgs e)
    {
        if (e.Command is null || e.Command.QualifiedName.StartsWith("help"))
            return Task.CompletedTask;

        Log.Information(
            "Executed: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Command.QualifiedName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    public static async Task CommandErrorEventHandler(CommandsNextExtension cNext, CommandErrorEventArgs e)
    {
        if (e.Exception is null)
            return;

        Exception ex = e.Exception;
        while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
            ex = ex.InnerException;

        Log.Error(
            "Command errored ({ExceptionName}): {ErroredCommand} {User} {Guild} {Channel}",
            e.Exception?.GetType().Name ?? "Unknown", e.Command?.QualifiedName ?? "Unknown",
            e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );

        var emb = new DiscordEmbedBuilder() {
            Title = "ОШИБОЧНАЯ!",
            Description = e.Command?.QualifiedName ?? "",
            Color = DiscordColor.Red,
        };

        switch (ex) {
            case ChecksFailedException _:
            case TaskCanceledException:
                return;
            case CommandNotFoundException:
                await e.Context.Message.CreateReactionAsync(Emojis.Question);
                return;
            case UnauthorizedException _:
                emb.WithDescription("403");
                break;
            case NpgsqlException _:
            case DbUpdateException _:
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

    public static Task SlashCommandInvokedEventHandler(SlashCommandsExtension sNext, SlashCommandInvokedEventArgs e)
    {
        Log.Information(
            "Slash Invoked: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Context.CommandName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    public static Task SlashCommandExecutionEventHandler(SlashCommandsExtension sNext, SlashCommandExecutedEventArgs e)
    {
        Log.Information(
            "Slash Executed: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Context.CommandName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    public static async Task SlashCommandErrorEventHandler(SlashCommandsExtension sNext, SlashCommandErrorEventArgs e)
    {
        if (e.Exception is null)
            return;

        Exception ex = e.Exception;
        while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
            ex = ex.InnerException;

        Log.Error(
            "Slash Errored ({ExceptionName}): {ErroredCommand} {User} {Guild} {Channel}",
            e.Exception?.GetType().Name ?? "Unknown", e.Context.CommandName,
            e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );

        string error = "";

        switch (ex)
        {
            case SlashExecutionChecksFailedException slex:
                foreach (var check in slex.FailedChecks)
                {
                    if (check is SlashRequireNsfwAttribute att)
                        error = $"Команда доступна только в nsfw каналах";
                }
                break;
            case TaskCanceledException:
                return;
            case UnauthorizedException _:
                error = "403";
                break;
            case NpgsqlException _:
            case DbUpdateException _:
                Log.Error(ex, "Database error");
                return;
            case CommandCancelledException:
                break;
            default:
                error = ex.Message;
                Log.Error(ex, "Unhandled error");
                break;
        }

        await e.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent(error));
    }

}
