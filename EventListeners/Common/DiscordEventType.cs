﻿using DSharpPlus.Entities;

namespace MCOP.EventListeners.Common;

public enum DiscordEventType
{
    #region Event types
    ApplicationCommandCreated,
    ApplicationCommandDeleted,
    ApplicationCommandUpdated,
    ChannelCreated,
    ChannelDeleted,
    ChannelPinsUpdated,
    ChannelUpdated,
    ClientErrored,
    CommandErrored,
    CommandExecuted,
    ComponentInteractionCreated,
    DmChannelDeleted,
    GuildAvailable,
    GuildBanAdded,
    GuildBanRemoved,
    GuildCreated,
    GuildDeleted,
    GuildDownloadCompleted,
    GuildEmojisUpdated,
    GuildIntegrationsUpdated,
    GuildMemberAdded,
    GuildMemberRemoved,
    GuildMemberUpdated,
    GuildMembersChunked,
    GuildRoleCreated,
    GuildRoleDeleted,
    GuildRoleUpdated,
    GuildStickersUpdated,
    GuildUnavailable,
    GuildUpdated,
    Heartbeated,
    InviteCreated,
    InviteDeleted,
    MessageAcknowledged,
    MessageCreated,
    MessageDeleted,
    MessageReactionAdded,
    MessageReactionRemoved,
    MessageReactionRemovedEmoji,
    MessageReactionsCleared,
    MessageUpdated,
    MessagesBulkDeleted,
    PresenceUpdated,
    Ready,
    Resumed,
    SlashCommandErrored,
    SlashCommandExecuted,
    SlashCommandInvoked,
    SocketClosed,
    SocketErrored,
    SocketOpened,
    TypingStarted,
    UnknownEvent,
    UserSettingsUpdated,
    UserUpdated,
    VoiceServerUpdated,
    VoiceStateUpdated,
    WebhooksUpdated,
    #endregion
}
