using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
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
    }
}
