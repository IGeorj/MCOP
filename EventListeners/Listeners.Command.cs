using System.Reflection;
using MCOP.Common;
using MCOP.EventListeners.Attributes;
using MCOP.EventListeners.Common;
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
    [AsyncEventListener(DiscordEventType.CommandExecuted)]
    public static Task CommandExecutionEventHandler(Bot bot, CommandExecutionEventArgs e)
    {
        if (e.Command is null || e.Command.QualifiedName.StartsWith("help"))
            return Task.CompletedTask;

        Log.Information(
            "Executed: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Command.QualifiedName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    [AsyncEventListener(DiscordEventType.CommandErrored)]
    public static Task CommandErrorEventHandlerAsync(Bot bot, CommandErrorEventArgs e)
    {
        if (e.Exception is null)
            return Task.CompletedTask;

        Exception ex = e.Exception;
        while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
            ex = ex.InnerException;

        Log.Debug(
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
                return Task.CompletedTask;
            case CommandNotFoundException:
                return e.Context.Message.CreateReactionAsync(Emojis.Question);
            case UnauthorizedException _:
                emb.WithDescription("403");
                break;
            case NpgsqlException _:
            case DbUpdateException _:
                Log.Error(ex, "Database error");
                return Task.CompletedTask;
            case CommandCancelledException:
                break;
            default:
                emb.WithDescription(ex.Message);
                Log.Error(ex, "Unhandled error");
                break;
        }

        return e.Context.RespondAsync(embed: emb.Build());
    }

    [AsyncEventListener(DiscordEventType.SlashCommandInvoked)]
    public static Task SlashCommandInvokedEventHandler(Bot bot, SlashCommandInvokedEventArgs e)
    {
        Log.Information(
            "Slash Invoked: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Context.CommandName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    [AsyncEventListener(DiscordEventType.SlashCommandExecuted)]
    public static Task SlashCommandExecutionEventHandler(Bot bot, SlashCommandExecutedEventArgs e)
    {
        Log.Information(
            "Slash Executed: {ExecutedCommand} {User} {Guild} {Channel}",
            e.Context.CommandName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );
        return Task.CompletedTask;
    }

    [AsyncEventListener(DiscordEventType.SlashCommandErrored)]
    public static Task SlashCommandErrorEventHandlerAsync(Bot bot, SlashCommandErrorEventArgs e)
    {
        if (e.Exception is null)
            return Task.CompletedTask;

        Exception ex = e.Exception;
        while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
            ex = ex.InnerException;

        Log.Debug(
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
                return Task.CompletedTask;
            case UnauthorizedException _:
                error = "403";
                break;
            case NpgsqlException _:
            case DbUpdateException _:
                Log.Error(ex, "Database error");
                return Task.CompletedTask;
            case CommandCancelledException:
                break;
            default:
                error = ex.Message;
                Log.Error(ex, "Unhandled error");
                break;
        }

        return e.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent(error));
    }

}
