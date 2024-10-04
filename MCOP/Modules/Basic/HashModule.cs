using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel;

namespace MCOP.Modules.Basic
{
    public sealed class HashModule
    {
        [SlashCommandTypes(DiscordApplicationCommandType.MessageContextMenu)]
        [RequireApplicationOwner]
        [Command("hash")]
        [Description("Хеширует изображение из сообщения")]
        public async Task Hash(CommandContext ctx,
            DiscordMessage targetMessage)
        {
            if (ctx is SlashCommandContext slashContext)
            {
                await slashContext.DeferResponseAsync(ephemeral: true);
            }
            else
            {
                await ctx.DeferResponseAsync();
            }
            ulong ulongMessageId = targetMessage.Id;


            int count = 0;
            DiscordMessage message;
            List<byte[]> hashes;

            try
            {
                var hashService = ctx.ServiceProvider.GetRequiredService<ImageHashService>();
                message = await ctx.Channel.GetMessageAsync(ulongMessageId);
                hashes = await hashService.GetHashesFromMessageAsync(message);

                if (ctx.Guild is null || message.Author is null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild or Author not found!"));
                    return;
                }

                var searchHashResult = await hashService.SearchHashesAsync(hashes);

                foreach (var hashResult in searchHashResult)
                {
                    var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
                    if (resultMessageId is not null)
                    {
                        continue;
                    }

                    await hashService.SaveHashAsync(ctx.Guild.Id, message.Id, message.Author.Id, hashResult.HashToCheck);
                    count++;
                }

                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":heart:"));
            }
            catch (Exception)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message not found!"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {count} of {hashes.Count} images"));
        }

        [RequireApplicationOwner]
        [Command("compare")]
        [Description("Тест нового алгоритма сравнения")]
        public async Task CompareImageFromMessages(CommandContext ctx,
            [Description("Message ID 1")] string messageId1,
            [Description("Message ID 2")] string messageId2)
        {
            await ctx.DeferResponseAsync();

            ulong ulongMessageId1 = ulong.Parse(messageId1);
            ulong ulongMessageId2 = ulong.Parse(messageId2);

            try
            {
                var hashService = ctx.ServiceProvider.GetRequiredService<ImageHashService>();
                var message1 = await ctx.Channel.GetMessageAsync(ulongMessageId1);
                var message2 = await ctx.Channel.GetMessageAsync(ulongMessageId2);
                var hash1 = (await hashService.GetHashesFromMessageAsync(message1)).FirstOrDefault();
                var hash2 = (await hashService.GetHashesFromMessageAsync(message2)).FirstOrDefault();


                if (ctx.Guild is null || hash1 is null || hash2 is null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild or Image not found!"));
                    return;
                }

                var percent = SkiaSharpService.GetNormalizedDifference(hash1, hash2);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Шанк корреляции двух изображений: {percent}"));
            }
            catch (Exception)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message not found!"));
                return;
            }
        }
    }
}
