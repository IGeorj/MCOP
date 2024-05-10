using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Common;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.EventListeners;
internal static partial class Listeners
{
    public static async Task MessageCreateEventHandler(DiscordClient client, MessageCreateEventArgs e)
    {
        if (e.Author.IsBot || e.Guild is null)
        {
            return;
        }

        if (e.Guild.Id == GlobalVariables.McopServerId &&  e.Message.Content.Contains("@everyone"))
        {
            var member = e.Author as DiscordMember;
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

                await e.Guild.PublicUpdatesChannel.SendMessageAsync(embed.Build());
                await member.GrantRoleAsync(e.Guild.GetRole(622772942761361428)); // САСЁШЬ
            }
        }
        // TODO: remove hardcoded channels
        var mcopLewdChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var mcopNsfwChannel = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 539145624868749327;
        var gaysAdminChannel = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;

        if ((mcopNsfwChannel || mcopLewdChannel) && e.Message.Attachments.Count > 0)
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

                foreach (var hash in hashes)
                {
                    var hashFound = await hashService.SearchHashByGuildAsync(e.Guild.Id, hash, 94);

                    if (hashFound is not null)
                    {
                        DiscordMessage messageFromHash;
                        try
                        {
                            messageFromHash = await e.Channel.GetMessageAsync(hashFound.MessageId);
                        }
                        catch (Exception)
                        {
                            Log.Warning("Can't get message from hash. messageId:{hashMessageId}, channelId:{channelId}, guildId:{guildId}", hashFound.MessageId, e.Channel.Id, e.Guild.Id);
                            await hashService.RemoveHashesByMessageId(e.Guild.Id, hashFound.MessageId);
                            continue;
                        }

                        var attachemnt = messageFromHash.Attachments[hashes.IndexOf(hash)];

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                        .WithTitle("Найдено совпадение")
                        .AddField("Новое", e.Author.Username, true)
                        .AddField("Прошлое", messageFromHash.Author.Username, true)
                        .AddField("Процент", ((int)hashFound.Difference).ToString())
                        .WithThumbnail(attachemnt.Url);

                        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                        .WithEmbed(embedBuilder)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordLinkButtonComponent(e.Message.JumpLink.ToString(), "Новое"),
                            new DiscordLinkButtonComponent(messageFromHash.JumpLink.ToString(), "Прошлое"),
                            new DiscordButtonComponent(
                                ButtonStyle.Success, 
                                GlobalNames.Buttons.RemoveMessage + $"UID:{e.Author.Id}",
                                "Понял",
                                false, 
                                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":heavy_check_mark:" ))),
                        });
                        await e.Channel.SendMessageAsync(messageBuilder);
                        continue;
                    }

                    added += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hash);
                }
                if (added > 0)
                {
                    Log.Information("Added {Amount} hashes ({Total} total)", added, await hashService.GetTotalCountAsync());
                }
            }

        }
    }

    public static async Task MessageDeleteEventHandler(DiscordClient client, MessageDeleteEventArgs e)
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

    public static async Task MessageUpdatedEventHandler(DiscordClient client, MessageUpdateEventArgs e)
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

                foreach (var hash in hashes)
                {
                    var hashFound = await hashService.SearchHashByGuildAsync(e.Guild.Id, hash, 94);

                    if (hashFound is not null)
                    {
                        continue;
                    }

                    updated += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hash);
                }
                if (updated > 0)
                {
                    Log.Information("Updated {Amount} hashes ({Total} total)", updated, await hashService.GetTotalCountAsync());
                }
            }
        }
    }

    public static async Task ComponentInteractionCreatedEventHandler(DiscordClient client, ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

        if (e.Id.StartsWith(GlobalNames.Buttons.RemoveMessage))
        {
            ulong userId = ulong.Parse(e.Id[(e.Id.LastIndexOf(':') + 1)..]);
            if (userId == e.User.Id)
            {
                await e.Message.DeleteAsync();
            }
        }
    }

    private static bool IsAttachmentsChanged(DiscordMessage message, DiscordMessage messageBefore)
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
}
