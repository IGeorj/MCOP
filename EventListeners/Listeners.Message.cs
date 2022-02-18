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

        if (!e.Message.Content.StartsWith(bot.Config.CurrentConfiguration.Prefix))
        {
            if (e.Channel.Id == nsfwAnimeChannelId)
	        {
                if (e.Message.Attachments == null)
				{
					return;
				}

                bool withImage = e.Message.Attachments.Any(a => a.MediaType.Contains("png") || a.MediaType.Contains("jpeg"));

                if (withImage)
                {
                    await e.Message.CreateReactionAsync(DiscordEmoji.FromName(bot.Client, ":heart:"));

                    List<byte[]> hashes = await e.Message.GetImageHashesAsync();

                    var hashService = bot.Services.GetRequiredService<ImageHashService>();
                    var messageService = bot.Services.GetRequiredService<UserMessageService>();

                    int added = 0;
                    ulong messageId;
                    double procent;

                    foreach (var hash in hashes)
                    {
                        bool result = hashService.TryGetSimilar(hash, 90, out messageId, out procent);

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
                    Log.Information($"Added {added} hashes. Total:{hashService.GetTotalHashes()}");

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
                await messageService.RemoveAsync(gid, mid);
                hashService.RemoveFromHashByMessageId(gid, mid);
            }
        }
    }
}
