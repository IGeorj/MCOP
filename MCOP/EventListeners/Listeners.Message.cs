using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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

        if (e.Message.Content.Contains("@everyone"))
        {
            var member = e.Author as DiscordMember;
            if (member is not null && !member.IsAdmin())
            {
                await e.Message.DeleteAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithAuthor(member.ToDiscriminatorString(), null, member.AvatarUrl)
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

        var mcopServer = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var gaysServer = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;
        if (mcopServer || gaysServer)
        {
            var hashService = Services.GetRequiredService<ImageHashService>();
            List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);

            if (hashes.Count > 0)
            {
                int added = 0;

                foreach (var hash in hashes)
                {
                    var hashFound = await hashService.FindHashByGuildAsync(e.Guild.Id, hash, 90);

                    if (hashFound is not null)
                    {
                        DiscordMessage messageFromHash;
                        try
                        {
                            messageFromHash = await e.Channel.GetMessageAsync(hashFound.MessageId);
                        }
                        catch (Exception)
                        {
                            Log.Warning($"Can't get message from hash. messageId:{hashFound.MessageId}, channelId:{e.Channel.Id}, guildId:{e.Guild.Id}");
                            continue;
                        }

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                        .WithTitle("Найдено совпадение")
                        .AddField("Новое", e.Author.ToDiscriminatorString(), true)
                        .AddField("Прошлое", messageFromHash.Author.ToDiscriminatorString(), true)
                        .AddField("Процент", ((int)hashFound.Difference).ToString())
                        .WithThumbnail("https://media.discordapp.net/attachments/549313253541543951/843572826778239074/pngwing.com.png");

                        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                        .WithEmbed(embedBuilder)
                        .AddComponents(new DiscordComponent[]
                        {
                            new DiscordLinkButtonComponent(e.Message.JumpLink.ToString(), "Новое"),
                            new DiscordLinkButtonComponent(messageFromHash.JumpLink.ToString(), "Прошлое"),
                        });
                        await e.Channel.SendMessageAsync(messageBuilder);
                        continue;
                    }

                    added += await hashService.SaveHashAsync(e.Guild.Id, e.Message.Id, e.Author.Id, hash);

                    if (added > 0)
                    {
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(client, ":heart:"));
                    }
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
        var mcopServer = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var gaysServer = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;
        if (mcopServer || gaysServer)
        {
            var messageService = Services.GetRequiredService<MessageService>();
            await messageService.RemoveMessageAsync(e.Guild.Id, e.Message.Id);
        }
    }

    public static async Task MessageUpdatedEventHandler(DiscordClient client, MessageUpdateEventArgs e)
    {
        var mcopServer = e.Guild.Id == GlobalVariables.McopServerId && e.Channel.Id == 586295440358506496;
        var gaysServer = e.Guild.Id == GlobalVariables.MyServerId && e.Channel.Id == 549313253541543951;
        if (mcopServer || gaysServer)
        {
            var hashService = Services.GetRequiredService<ImageHashService>();
            var messageService = Services.GetRequiredService<MessageService>();

            await messageService.RemoveMessageAsync(e.Guild.Id, e.Message.Id);

            List<byte[]> hashes = await hashService.GetHashesFromMessageAsync(e.Message);
            int updated = 0;

            foreach (var hash in hashes)
            {
                var hashFound = await hashService.FindHashByGuildAsync(e.Guild.Id, hash, 90);

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

    public static async Task ComponentInteractionCreatedEventHandler(DiscordClient client, ComponentInteractionCreateEventArgs e)
    {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }
}
