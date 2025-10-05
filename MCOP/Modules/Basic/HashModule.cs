using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using MCOP.Common.Helpers;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
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
            await ctx.DeferEphemeralAsync();

            ulong ulongMessageId = targetMessage.Id;

            int count = 0;
            DiscordMessage message;
            List<byte[]> hashes;

            try
            {
                var hashService = ctx.ServiceProvider.GetRequiredService<IImageHashService>();
                message = await ctx.Channel.GetMessageAsync(ulongMessageId);
                hashes = await hashService.GetHashesFromMessageAsync(message);

                var (guild, member) = await CommandContextHelper.ValidateAndGetMemberAsync(ctx, message.Author);
                if (guild is null || member is null) return;

                count = await ProcessMessageHashesAsync(count, message, hashes, hashService, guild, member);

                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":heart:"));
            }
            catch (Exception)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message not found!"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {count} of {hashes.Count} images"));
        }

        private static async Task<int> ProcessMessageHashesAsync(
            int count,
            DiscordMessage message,
            List<byte[]> hashes,
            IImageHashService hashService,
            DiscordGuild guild, 
            DiscordMember member)
        {
            var searchHashResult = await hashService.SearchHashesAsync(guild.Id, hashes);

            foreach (var hashResult in searchHashResult)
            {
                var resultMessageId = hashResult.MessageId ?? hashResult.MessageIdNormalized;
                if (resultMessageId is not null)
                {
                    continue;
                }

                await hashService.SaveHashAsync(guild.Id, message.ChannelId, message.Id, member.Id, hashResult.HashToCheck);
                count++;
            }

            return count;
        }
    }
}
