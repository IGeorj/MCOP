using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace MCOP.Common.Helpers
{
    public static class CommandContextHelper
    {
        public static async Task<(DiscordGuild?, DiscordMember?)> ValidateAndGetMemberAsync(CommandContext ctx, DiscordUser? user)
        {
            var guild = await ValidateAndGetGuildAsync(ctx);
            if (guild is null)
                return (null, null);

            try
            {
                var member = user is null ? ctx.Member : await guild.GetMemberAsync(user.Id);
                if (member is null)
                {
                    await ctx.EditResponseAsync("User not found!");
                    return (ctx.Guild, null);
                }

                return (ctx.Guild, member);
            }
            catch (NotFoundException)
            {
                await ctx.EditResponseAsync("User not found!");
                return (guild, null);
            }
        }

        public static async Task<DiscordGuild?> ValidateAndGetGuildAsync(CommandContext ctx)
        {
            if (ctx.Guild is not null)
                return ctx.Guild;

            await ctx.EditResponseAsync("Server not found!");
            return null;
        }
    }
}
