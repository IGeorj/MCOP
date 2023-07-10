using Microsoft.Extensions.DependencyInjection;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    private static ServiceProvider Services { get; set; } = default!;
    public static void RegisterEvents(Bot bot)
    {
        Services = bot.Services;
        bot.Client.ClientErrored += ClientErrorEventHandler;
        bot.Client.GuildAvailable += GuildAvailableEventHandler;
        bot.Client.GuildUnavailable += GuildUnvailableEventHandler;
        bot.Client.GuildDownloadCompleted += GuildDownloadCompletedEventHandler;
        bot.Client.GuildCreated += GuildCreateEventHandler;
        bot.Client.SocketOpened += SocketOpenedEventHandler;
        bot.Client.SocketClosed += SocketClosedEventHandler;
        bot.Client.SocketErrored += SocketErroredEventHandler;
        bot.Client.UnknownEvent += UnknownEventHandler;
        bot.Client.UserUpdated += UserUpdatedEventHandler;
        bot.Client.UserSettingsUpdated += UserSettingsUpdatedEventHandler;
        bot.CNext.CommandExecuted += CommandExecutionEventHandler;
        bot.CNext.CommandErrored += CommandErrorEventHandler;
        bot.CSlash.SlashCommandInvoked += SlashCommandInvokedEventHandler;
        bot.CSlash.SlashCommandExecuted += SlashCommandExecutionEventHandler;
        bot.CSlash.SlashCommandErrored += SlashCommandErrorEventHandler;
        bot.Client.MessageCreated += MessageCreateEventHandler;
        bot.Client.MessageDeleted += MessageDeleteEventHandler;
        bot.Client.MessageUpdated += MessageUpdatedEventHandler;
        bot.Client.MessageReactionAdded += MessageReactionAddedEventHandler;
        bot.Client.MessageReactionRemoved += MessageReactionRemovedEventHandler;
        bot.Client.ComponentInteractionCreated += ComponentInteractionCreatedEventHandler;
    }
}
