using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Common;
using MCOP.Common.Helpers;
using MCOP.Core.Common;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;
using MCOP.Core.Services.Scoped.AI;
using MCOP.Core.ViewModels;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners;
internal static partial class Listeners
{
    private static readonly ulong McopLewdChannelId = 586295440358506496;
    private static readonly ulong GaysAdminChannelId = 549313253541543951;

    public static async Task MessageCreateEventHandler(DiscordClient client, MessageCreatedEventArgs e)
    {
        if (ShouldIgnoreMessage(e))
            return;

        await ProcessMessageCreation(client, e);
    }

    public static async Task MessageUpdatedEventHandler(DiscordClient client, MessageUpdatedEventArgs e)
    {
        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
            await ProcessMessageUpdate(client, e);
    }

    public static async Task MessageDeleteEventHandler(DiscordClient client, MessageDeletedEventArgs e)
    {
        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
        {
            var messageService = Services.GetRequiredService<GuildMessageService>();
            await messageService.RemoveMessageAsync(e.Guild.Id, e.Message.Id);
        }
    }

    public static async Task ComponentInteractionCreatedEventHandler(DiscordClient client, ComponentInteractionCreatedEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        if (e.Id.StartsWith(GlobalNames.Buttons.RemoveMessage))
            await HandleRemoveMessageButton(e);
    }



    private static async Task ProcessMessageCreation(DiscordClient client, MessageCreatedEventArgs e)
    {
        await MessageHelper.CheckEveryoneAsync(client, e);
        await MessageHelper.CheckDulyaAsync(client, e);

        var aiService = Services.GetRequiredService<AIService>();
        await aiService.GenerateAIResponseOnMentionAsync(client, e);

        var levelingService = Services.GetRequiredService<GuildUserStatsService>();
        await levelingService.AddMessageExpAsync(client, e.Guild.Id, e.Channel.Id, e.Author.Id);

        if (ShouldReactWithHeart(e))
            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":heart:"));

        if (IsSpecialChannel(e.Guild.Id, e.Channel.Id))
            await ProcessImageHashes(client, e);
    }

    private static async Task ProcessMessageUpdate(DiscordClient client, MessageUpdatedEventArgs e)
    {
        if (!HasAttachmentsChanged(e.Message, e.MessageBefore))
            return;

        var hashService = Services.GetRequiredService<ImageHashService>();
        List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);
        int updated = 0;

        await hashService.RemoveHashesByMessageId(e.Guild.Id, e.Message.Id);

        var searchHashResult = await hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

        foreach (var hashResult in searchHashResult)
        {
            var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
            if (resultMessageId is not null)
                continue;

            updated += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
        }

        if (updated > 0)
            Log.Information("Updated {Amount} hashes ({Total} total)", updated, await hashService.GetTotalCountAsync());
    }

    private static async Task ProcessImageHashes(DiscordClient client, MessageCreatedEventArgs e)
    {
        var hashService = Services.GetRequiredService<ImageHashService>();
        List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);

        if (hashes.Count == 0)
            return;

        int added = 0;
        var searchHashResult = await hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

        foreach (var hashResult in searchHashResult)
        {
            var messageFoundId = hashResult.MessageId ?? hashResult.MessageIdNormalized;

            if (messageFoundId is not null)
                await TrySendCopyFoundMessageAsync(client, e, hashResult, messageFoundId.Value);
            else
                added += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
        }

        if (added > 0)
            Log.Information("Added {Amount} hashes ({Total} total)", added, await hashService.GetTotalCountAsync());
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

    private static async Task<int> GetAttachmentsHashIndex(DiscordMessage messageFromHash, byte[] hash)
    {
        var hashService = Services.GetRequiredService<ImageHashService>();
        List<byte[]> oldHashes = await hashService.GetHashesFromMessageAsync(messageFromHash);

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

    private static async Task TrySendCopyFoundMessageAsync(DiscordClient client, MessageCreatedEventArgs e, HashSearchResultVM hashResult, ulong messageFoundId)
    {
        DiscordMessage messageFromHash;
        try
        {
            messageFromHash = await e.Channel.GetMessageAsync(messageFoundId);
        }
        catch (Exception)
        {
            Log.Warning("Can't get message from hash. messageId:{hashMessageId}, channelId:{channelId}, guildId:{guildId}", messageFoundId, e.Channel.Id, e.Guild.Id);
            var hashService = Services.GetRequiredService<ImageHashService>();
            await hashService.RemoveHashesByMessageId(e.Guild.Id, messageFoundId);
            return;
        }


        if (hashResult.HashFound is null && hashResult.HashFoundNormalized is null)
            return;

        var matchIndex = await GetAttachmentsHashIndex(messageFromHash, hashResult.HashFound ?? hashResult.HashFoundNormalized ?? []);
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

    private static async Task HandleRemoveMessageButton(ComponentInteractionCreatedEventArgs e)
    {
        ulong userId = ulong.Parse(e.Id[(e.Id.LastIndexOf(':') + 1)..]);
        if (userId == e.User.Id)
            await e.Message.DeleteSilentAsync();
    }

    private static bool ShouldReactWithHeart(MessageCreatedEventArgs e)
    {
        return (e.Guild.Id == GlobalVariables.McopServerId || IsSpecialChannel(e.Guild.Id, e.Channel.Id)) &&
               (e.Message.Attachments.Count > 0 || e.Message.IsContainsImageLink());
    }

    private static bool ShouldIgnoreMessage(MessageCreatedEventArgs e)
    {
        return e.Author.IsBot || e.Guild is null;
    }

    private static bool IsSpecialChannel(ulong guildId, ulong channelId)
    {
        return (guildId == GlobalVariables.McopServerId && channelId == McopLewdChannelId) ||
               (guildId == GlobalVariables.MyServerId && channelId == GaysAdminChannelId);
    }
}
