using DSharpPlus.Commands;
using DSharpPlus.Extensions;
using ILogger = Serilog.ILogger;

namespace MCOP.EventListeners;

internal static class Listeners
{
    public static IServiceCollection AddDiscordEvents(this IServiceCollection services)
    {
        return services.ConfigureEventHandlers(x =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var messageListener = serviceProvider.GetRequiredService<MessageListeners>();
                var reactionsListeners = serviceProvider.GetRequiredService<ReactionsListeners>();
                var clientListeners = serviceProvider.GetRequiredService<ClientListeners>();
                x.HandleGuildAvailable(clientListeners.GuildAvailableEventHandler);
                x.HandleGuildUnavailable(clientListeners.GuildUnvailableEventHandler);
                x.HandleGuildDownloadCompleted(clientListeners.GuildDownloadCompletedEventHandler);
                x.HandleGuildCreated(clientListeners.GuildCreateEventHandler);
                x.HandleSocketOpened(clientListeners.SocketOpenedEventHandler);
                x.HandleSocketClosed(clientListeners.SocketClosedEventHandler);
                x.HandleUnknownEvent(clientListeners.UnknownEventHandler);
                x.HandleUserUpdated(clientListeners.UserUpdatedEventHandler);
                x.HandleUserSettingsUpdated(clientListeners.UserSettingsUpdatedEventHandler);
                x.HandleMessageCreated(messageListener.MessageCreateEventHandler);
                x.HandleMessageDeleted(messageListener.MessageDeleteEventHandler);
                x.HandleMessageUpdated(messageListener.MessageUpdatedEventHandler);
                x.HandleMessageReactionAdded(reactionsListeners.MessageReactionAddedEventHandler);
                x.HandleMessageReactionRemoved(reactionsListeners.MessageReactionRemovedEventHandler);
                x.HandleComponentInteractionCreated(messageListener.ComponentInteractionCreatedEventHandler);
                x.HandleGuildMemberUpdated(clientListeners.GuildMemberUpdatedHandler);
                x.HandleGuildMemberAdded(clientListeners.GuildMemberAddedHandler);
                x.HandleSessionCreated((client, args) =>
                {
                    var logger = client.ServiceProvider.GetRequiredService<ILogger>();
                    logger.Information("Discord session created");
                    return Task.CompletedTask;
                });
            });
    }
    public static void RegisterCommandsEvent(CommandsExtension commandsExtension)
    {
        using var scope = commandsExtension.ServiceProvider.CreateScope();
        var commandListeners = scope.ServiceProvider.GetRequiredService<CommandListeners>();
        commandsExtension.CommandExecuted += commandListeners.CommandExecutionEventHandler;
        commandsExtension.CommandErrored += commandListeners.CommandErrorEventHandler;
    }
}
