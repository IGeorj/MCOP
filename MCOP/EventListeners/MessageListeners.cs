using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MCOP.Common;
using MCOP.Common.Helpers;
using MCOP.Core.Common;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Scoped.AI;
using MCOP.Core.ViewModels;
using MCOP.Extensions;
using Serilog;

namespace MCOP.EventListeners;
public sealed class MessageListeners
{
    private static readonly ulong McopLewdChannelId = 586295440358506496;
    private static readonly ulong GaysAdminChannelId = 549313253541543951;

    private readonly IImageHashService _hashService;
    private readonly IAIService _aiService;
    private readonly IGuildUserStatsService _levelingService;
    private readonly IGuildMessageService _messageService;

    public MessageListeners(
        IImageHashService hashService,
        IAIService aiService,
        IGuildUserStatsService levelingService,
        IGuildMessageService messageService)
    {
        _hashService = hashService;
        _aiService = aiService;
        _levelingService = levelingService;
        _messageService = messageService;
    }

    public async Task MessageCreateEventHandler(DiscordClient client, MessageCreatedEventArgs e)
    {
        if (ShouldIgnoreMessage(e))
            return;

        await ProcessMessageCreation(client, e);
    }

    public async Task MessageUpdatedEventHandler(DiscordClient client, MessageUpdatedEventArgs e)
    {
        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
            await ProcessMessageUpdate(client, e);
    }

    public async Task MessageDeleteEventHandler(DiscordClient client, MessageDeletedEventArgs e)
    {
        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
        {
            await _messageService.RemoveMessageAsync(e.Guild.Id, e.Message.Id);
        }
    }

    public async Task ComponentInteractionCreatedEventHandler(DiscordClient client, ComponentInteractionCreatedEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        if (e.Id.StartsWith(GlobalNames.Buttons.RemoveMessage))
            await HandleRemoveMessageButton(e);
    }



    private async Task ProcessMessageCreation(DiscordClient client, MessageCreatedEventArgs e)
    {
        await MessageHelper.CheckEveryoneAsync(client, e);
        await MessageHelper.CheckDulyaAsync(client, e);

        await _aiService.GenerateAIResponseOnMentionAsync(e);
        await _levelingService.AddMessageExpAsync(e.Guild.Id, e.Channel.Id, e.Author.Id);

        if (ShouldReactWithHeart(e))
        {
            try
            {
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":heart:"));
            }
            catch (Exception ex) 
            {
                if (ex is UnauthorizedException) 
                {
                    Log.Warning("Can't add reaction, probably user have blocked bot");
                }
                Log.Error("Can't add reaction, something wrong", ex);
            }
        }

        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
            await ProcessImageHashes(client, e);
    }

    private async Task ProcessMessageUpdate(DiscordClient client, MessageUpdatedEventArgs e)
    {
        if (!HasAttachmentsChanged(e.Message, e.MessageBefore))
            return;

        List<byte[]> hashes = await _hashService.GetHashesFromMessageAsync(e.Message);
        int updated = 0;

        await _hashService.RemoveHashesByMessageId(e.Guild.Id, e.Message.Id);

        var searchHashResult = await _hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

        foreach (var hashResult in searchHashResult)
        {
            var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
            if (resultMessageId is not null)
                continue;

            updated += await _hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
        }

        if (updated > 0)
            Log.Information("Updated {Amount} hashes ({Total} total)", updated, await _hashService.GetTotalCountAsync());
    }

    private async Task ProcessImageHashes(DiscordClient client, MessageCreatedEventArgs e)
    {
        List<byte[]> hashes = await _hashService.GetHashesFromMessageAsync(e.Message);

        if (hashes.Count == 0)
            return;

        int added = 0;
        var searchHashResult = await _hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

        foreach (var hashResult in searchHashResult)
        {
            var messageFoundId = hashResult.MessageId ?? hashResult.MessageIdNormalized;

            if (messageFoundId is not null)
                await TrySendCopyFoundMessageAsync(client, e, hashResult, messageFoundId.Value);
            else
                added += await _hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
        }

        if (added > 0)
            Log.Information("Added {Amount} hashes ({Total} total)", added, await _hashService.GetTotalCountAsync());
    }

    private static bool HasAttachmentsChanged(DiscordMessage message, DiscordMessage? messageBefore)
    {
        if (messageBefore is null)
            return true;

        if (message.Attachments.Count < messageBefore.Attachments.Count)
            return true;

        var currentAttachments = message.Attachments.ToList();
        var previousAttachments = messageBefore.Attachments.ToList();

        return previousAttachments.Any(attachment => 
            !currentAttachments.Any(x => x.Id == attachment.Id));
    }

    private async Task<int> GetAttachmentsHashIndex(DiscordClient client, DiscordMessage messageFromHash, byte[] hash)
    {
        List<byte[]> oldHashes = await _hashService.GetHashesFromMessageAsync(messageFromHash);

        double bestMatch = 0;
        int indexMatch = 0;

        for (int i = 0; i < oldHashes.Count; i++)
        {
            var percentage = SkiaSharpService.GetPercentageDifference(oldHashes[i], hash);
            if (percentage > bestMatch)
            {
                indexMatch = i;
                bestMatch = percentage;
            }
        }

        return indexMatch;
    }

    private async Task TrySendCopyFoundMessageAsync(DiscordClient client, MessageCreatedEventArgs e, HashSearchResultVM hashResult, ulong messageFoundId)
    {
        DiscordMessage messageFromHash;
        try
        {
            messageFromHash = await e.Channel.GetMessageAsync(messageFoundId);
        }
        catch (Exception)
        {
            Log.Warning("Can't get message from hash. messageId:{hashMessageId}, channelId:{channelId}, guildId:{guildId}", messageFoundId, e.Channel.Id, e.Guild.Id);
            await _hashService.RemoveHashesByMessageId(e.Guild.Id, messageFoundId);
            return;
        }


        if (hashResult.HashFound is null && hashResult.HashFoundNormalized is null)
            return;

        var matchIndex = await GetAttachmentsHashIndex(client, messageFromHash, hashResult.HashFound ?? hashResult.HashFoundNormalized ?? []);
        var attachemnt = messageFromHash.Attachments[matchIndex];
        var diff = hashResult.MessageId.HasValue ? hashResult.Difference : hashResult.DifferenceNormalized;

        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Найдено совпадение")
            .AddField("Новое", e.Author.Username, true)
            .AddField("Прошлое", messageFromHash.Author?.Username ?? "", true)
            .AddField("Совпадение", $"{diff:0.00}")
            .WithThumbnail(attachemnt.Url ?? "");

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embedBuilder)
            .AddComponents(
            [
                new DiscordLinkButtonComponent(e.Message.JumpLink.ToString(), "Новое"),
                new DiscordLinkButtonComponent(messageFromHash.JumpLink.ToString(), "Прошлое"),
                new DiscordButtonComponent(
                    DiscordButtonStyle.Success,
                    GlobalNames.Buttons.RemoveMessage + $"UID:{e.Author.Id}",
                    "Понял",
                    false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":heavy_check_mark:" ))),
            ]);

        await e.Channel.SendMessageAsync(messageBuilder);
    }

    private async Task HandleRemoveMessageButton(ComponentInteractionCreatedEventArgs e)
    {
        ulong userId = ulong.Parse(e.Id[(e.Id.LastIndexOf(':') + 1)..]);
        if (userId == e.User.Id)
            await e.Message.DeleteSilentAsync();
    }

    private bool ShouldReactWithHeart(MessageCreatedEventArgs e)
    {
        return (e.Guild.Id == GlobalVariables.McopServerId || IsSpecialChannel(e.Guild.Id, e.Channel.Id)) &&
               (e.Message.Attachments.Count > 0 || e.Message.IsContainsImageLink());
    }

    private bool ShouldIgnoreMessage(MessageCreatedEventArgs e)
    {
        return e.Author.IsBot || e.Guild is null;
    }

    private bool IsSpecialChannel(ulong guildId, ulong channelId)
    {
        return (guildId == GlobalVariables.McopServerId && channelId == McopLewdChannelId) ||
               (guildId == GlobalVariables.MyServerId && channelId == GaysAdminChannelId);
    }
}
