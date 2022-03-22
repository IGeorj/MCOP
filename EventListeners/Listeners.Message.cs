using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MCOP.Database.Models;
using MCOP.EventListeners.Attributes;
using MCOP.EventListeners.Common;
using MCOP.Extensions;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCOP.EventListeners;
internal static partial class Listeners
{

    [AsyncEventListener(DiscordEventType.MessageCreated)]
    public static async Task MessageCreateEventHandlerAsync(Bot bot, MessageCreateEventArgs e)
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
                    .AddField("Модератор", bot.Client.CurrentUser.Mention, true)
                    .AddField("Результат", "Вадана роль САСЁШЬ", true)
                    .AddField("Канал", $"<#{e.Channel.Id}>")
                    .AddField("Сообщение", e.Message.Content);

                await e.Guild.PublicUpdatesChannel.SendMessageAsync(embed.Build());
                await member.GrantRoleAsync(e.Guild.GetRole(622772942761361428)); // САСЁШЬ
            }
        }

        if (!e.Message.Content.StartsWith(bot.Config.CurrentConfiguration.Prefix))
        {
            if (e.Channel.Id == nsfwAnimeChannelId)
	        {
                if (e.Message.Attachments == null)
				{
					return;
				}

                bool withImage = e.Message.Attachments.Any(a => a.MediaType.Contains("png") || a.MediaType.Contains("jpeg") || a.MediaType.Contains("webp"));

                if (withImage)
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(bot.Client, ":heart:"));

                    List<byte[]> hashes = await e.Message.GetImageHashesAsync();

                    var hashService = bot.Services.GetRequiredService<ImageHashService>();
                    var messageService = bot.Services.GetRequiredService<UserMessageService>();

                    int added = 0;

                    foreach (var hash in hashes)
                    {
                        bool result = hashService.TryGetSimilar(hash, 90, out ulong messageId, out double procent);

                        if (result)
                        {
                            DiscordMessage foundMessage;
                            try
                            {
                                foundMessage = await e.Channel.GetMessageAsync(messageId);
                            }
                            catch (Exception)
                            {
                                Log.Warning($"Can't get messageID:{messageId}, channelID:{e.Channel.Id}");
                                continue;
                            }

                            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                            {
                                Title = "Найдено совпадение"
                            }
                            .AddField("Новое", e.Author.ToDiscriminatorString(), true)
                            .AddField("Прошлое", foundMessage.Author.ToDiscriminatorString(), true)
                            .AddField("Шанс", ((int)procent).ToString())
                            .WithThumbnail("https://media.discordapp.net/attachments/549313253541543951/843572826778239074/pngwing.com.png");

                            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                            {
                                Embed = embedBuilder
                            }
                            .AddComponents(new DiscordComponent[]
                            {
                                new DiscordLinkButtonComponent(e.Message.JumpLink.ToString(), "Новое"),
                                new DiscordLinkButtonComponent(foundMessage.JumpLink.ToString(), "Прошлое")
                            });
                            await e.Channel.SendMessageAsync(messageBuilder);
                            continue;
                        }

                        ImageHash imageHash = new();
                        imageHash.Hash = hash;

                        UserMessage userMessage = await messageService.GetOrAddAsync(e.Guild.Id, e.Message.Id);
                        imageHash.GuildId = userMessage.GuildId;
                        imageHash.MessageId = userMessage.MessageId;

                        added += await hashService.AddAsync(imageHash);
                    }

                    Log.Information("Added {Amount} hashes ({Total} total)", added, hashService.GetTotalHashes());
                }

            }
        }
    }

    [AsyncEventListener(DiscordEventType.MessageDeleted)]
    public static async Task MessageDeleteEventHandlerAsync(Bot bot, MessageDeleteEventArgs e)
    {
        if (e.Channel.Id == nsfwAnimeChannelId)
        {
            var messageService = bot.Services.GetRequiredService<UserMessageService>();
            var hashService = bot.Services.GetRequiredService<ImageHashService>();

            ulong gid = e.Guild.Id;
            ulong mid = e.Message.Id;

            if (await messageService.ContainsAsync(gid, mid))
            {
                var countDB = await messageService.RemoveAsync(gid, mid);
                var countHash = hashService.RemoveFromHashByMessageId(gid, mid);
                Log.Debug("Removed from DB messages: {countDB}, hashes: {countHash}", countDB, countHash);
            }
        }
    }
}
