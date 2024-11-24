using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Common;
using MCOP.Core.Common;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;
using MCOP.Core.ViewModels;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners;
internal static partial class Listeners
{
    public static async Task MessageCreateEventHandler(DiscordClient client, MessageCreatedEventArgs e)
    {
        if (e.Author.IsBot || e.Guild is null)
        {
            return;
        }

        if (e.Guild.Id == GlobalVariables.McopServerId && e.Message.Content.Contains("@everyone"))
        {
            var member = await e.Guild.GetMemberAsync(e.Author.Id);
            if (member is not null && !member.IsAdmin())
            {
                await e.Message.DeleteAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithAuthor(member.Username, null, member.AvatarUrl)
                    .WithColor(DiscordColor.Yellow)
                    .AddField("Пользователь", $"<@!{member.Id}>", true)
                    .AddField("Модератор", client.CurrentUser.Mention, true)
                    .AddField("Результат", "Вадана роль САСЁШЬ", true)
                    .AddField("Канал", $"<#{e.Channel.Id}>")
                    .AddField("Сообщение", e.Message.Content);

                DiscordChannel? publicChannel = await e.Guild.GetPublicUpdatesChannelAsync();

                if (publicChannel is not null)
                {
                    await publicChannel.SendMessageAsync(embed.Build());
                }

                // САСЁШЬ
                DiscordRole? role = await e.Guild.GetRoleAsync(622772942761361428);

                if (role is not null)
                {
                    await member.GrantRoleAsync(role);
                }
            }
        }
        // TODO: remove hardcoded channels
        var mcopLewdChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var mcopNsfwChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 539145624868749327;
        var gaysAdminChannel = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;

        if ((mcopNsfwChannel || mcopLewdChannel || gaysAdminChannel) && e.Message.Attachments.Count > 0)
        {
            await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":heart:"));
        }

        if (mcopLewdChannel || gaysAdminChannel)
        {
            var hashService = Services.GetRequiredService<ImageHashService>();
            List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);

            if (hashes.Count > 0)
            {
                int added = 0;
                var searchHashResult = await hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

                foreach (var hashResult in searchHashResult)
                {
                    var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
                    if (resultMessageId is not null)
                    {
                        DiscordMessage messageFromHash;
                        try
                        {
                            messageFromHash = await e.Channel.GetMessageAsync(resultMessageId.Value);
                        }
                        catch (Exception)
                        {
                            Log.Warning("Can't get message from hash. messageId:{hashMessageId}, channelId:{channelId}, guildId:{guildId}", resultMessageId, e.Channel.Id, e.Guild.Id);
                            await hashService.RemoveHashesByMessageId(e.Guild.Id, resultMessageId.Value);
                            continue;
                        }

                        await SendCopyFoundMessageAsync(client, e, hashResult, messageFromHash);
                    }
                    else
                    {
                        added += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
                    }
                }

                if (added > 0)
                {
                    Log.Information("Added {Amount} hashes ({Total} total)", added, await hashService.GetTotalCountAsync());
                }
            }

        }
    }


    public static async Task MessageDeleteEventHandler(DiscordClient client, MessageDeletedEventArgs e)
    {
        // TODO: remove hardcoded channels
        var mcopLewdChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var gaysAdminChannel = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;

        if (mcopLewdChannel || gaysAdminChannel)
        {
            var messageService = Services.GetRequiredService<MessageService>();
            await messageService.RemoveMessageAsync(e.Guild.Id, e.Message.Id);
        }
    }

    public static async Task MessageUpdatedEventHandler(DiscordClient client, MessageUpdatedEventArgs e)
    {
        // TODO: remove hardcoded channels
        var mcopLewdChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var gaysAdminChannel = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;

        if (mcopLewdChannel || gaysAdminChannel)
        {
            if (IsAttachmentsChanged(e.Message, e.MessageBefore))
            {
                var hashService = Services.GetRequiredService<ImageHashService>();

                List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);
                int updated = 0;

                await hashService.RemoveHashesByMessageId(e.Guild.Id, e.Message.Id);

                var searchHashResult = await hashService.SearchHashesByGuildAsync(e.Guild.Id, hashes);

                foreach (var hashResult in searchHashResult)
                {
                    var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
                    if (resultMessageId is not null)
                    {
                        continue;
                    }
                    updated += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hashResult.HashToCheck);
                }

                if (updated > 0)
                {
                    Log.Information("Updated {Amount} hashes ({Total} total)", updated, await hashService.GetTotalCountAsync());
                }
            }
        }
    }

    public static async Task ComponentInteractionCreatedEventHandler(DiscordClient client, ComponentInteractionCreatedEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

        if (e.Id.StartsWith(GlobalNames.Buttons.RemoveMessage))
        {
            ulong userId = ulong.Parse(e.Id[(e.Id.LastIndexOf(':') + 1)..]);
            if (userId == e.User.Id)
            {
                await e.Message.DeleteAsync();
            }
        }
    }

    private static bool IsAttachmentsChanged(DiscordMessage message, DiscordMessage? messageBefore)
    {
        if (messageBefore is null)
        {
            return true;
        }

        if (message.Attachments.Count < messageBefore.Attachments.Count)
        {
            return true;
        }

        var attahcmentsNow = message.Attachments.ToList();
        var attahcmentsBefore = messageBefore.Attachments.ToList();

        foreach (var attachment in attahcmentsBefore)
        {
            if (!attahcmentsNow.Any(x => x.Id == attachment.Id))
            {
                return true;
            }
        }

        return false;
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

    private static async Task SendCopyFoundMessageAsync(DiscordClient client, MessageCreatedEventArgs e, HashSearchResultVM hashResult, DiscordMessage messageFromHash)
    {
        var matchIndex = await GetAttachmentsHashIndex(messageFromHash, hashResult.HashFound ?? hashResult.HashFoundNormalized);
        var attachemnt = messageFromHash.Attachments[matchIndex];
        var diff = hashResult.MessageId.HasValue ? hashResult.Difference : hashResult.DifferenceNormalized;
        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
        .WithTitle("Найдено совпадение")
        .AddField("Новое", e.Author.Username, true)
        .AddField("Прошлое", messageFromHash.Author.Username, true)
        .AddField("Совпадение", $"{diff:0.00}")
        .WithThumbnail(attachemnt.Url);

        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
        .AddEmbed(embedBuilder)
        .AddComponents(new DiscordComponent[]
        {
                            new DiscordLinkButtonComponent(e.Message.JumpLink.ToString(), "Новое"),
                            new DiscordLinkButtonComponent(messageFromHash.JumpLink.ToString(), "Прошлое"),
                            new DiscordButtonComponent(
                                DiscordButtonStyle.Success,
                                GlobalNames.Buttons.RemoveMessage + $"UID:{e.Author.Id}",
                                "Понял",
                                false,
                                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":heavy_check_mark:" ))),
        });
        await e.Channel.SendMessageAsync(messageBuilder);
    }

}
