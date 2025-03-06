using DSharpPlus;
using DSharpPlus.Commands;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    private static IServiceProvider Services { get; set; } = default!;
    public static void RegisterEvents(DiscordClientBuilder clientBuilder)
    {
        clientBuilder.ConfigureEventHandlers(x =>
        {
            x.HandleGuildAvailable(GuildAvailableEventHandler);
            x.HandleGuildUnavailable(GuildUnvailableEventHandler);
            x.HandleGuildAvailable(GuildAvailableEventHandler);
            x.HandleGuildUnavailable(GuildUnvailableEventHandler);
            x.HandleGuildDownloadCompleted(GuildDownloadCompletedEventHandler);
            x.HandleGuildCreated(GuildCreateEventHandler);
            x.HandleSocketOpened(SocketOpenedEventHandler);
            x.HandleSocketClosed(SocketClosedEventHandler);
            x.HandleUnknownEvent(UnknownEventHandler);
            x.HandleUserUpdated(UserUpdatedEventHandler);
            x.HandleUserSettingsUpdated(UserSettingsUpdatedEventHandler);
            x.HandleMessageCreated(MessageCreateEventHandler);
            x.HandleMessageDeleted(MessageDeleteEventHandler);
            x.HandleMessageUpdated(MessageUpdatedEventHandler);
            x.HandleMessageReactionAdded(MessageReactionAddedEventHandler);
            x.HandleMessageReactionRemoved(MessageReactionRemovedEventHandler);
            x.HandleComponentInteractionCreated(ComponentInteractionCreatedEventHandler);
        });
    }

    public static void RegisterCommandsEvent(CommandsExtension commandsExtension)
    {
        commandsExtension.CommandExecuted += CommandExecutionEventHandler;
        commandsExtension.CommandErrored += CommandErrorEventHandler;
    }

    public static void RegisterServiceProvider(IServiceProvider serviceProvider)
    {
        Services = serviceProvider;
    }
}
