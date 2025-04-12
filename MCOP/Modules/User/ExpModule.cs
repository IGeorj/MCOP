using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Scoped;
using MCOP.Data.Models;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text;

namespace MCOP.Modules.User
{
    [Command("lvl")]
    [Description("Команды Лвла/Опыта")]
    public sealed class ExpModule
    {
        [Command("add_exp")]
        [Description("Добавляет опыт")]
        [RequirePermissions(DiscordPermission.Administrator)]
        public async Task AddExp(CommandContext ctx,
            [Description("Кол-во опыта")] int exp,
            [Description("Пользователь")] DiscordUser? user = null)
        {
            await ctx.DeferEphemeralAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            var member = user is null ? ctx.Member : await ctx.Guild.GetMemberAsync(user.Id);
            if (member is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Member not found!"));
                return;
            }

            GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();

            await statsService.AddExpAsync(ctx.Client, ctx.Guild.Id, ctx.Channel.Id, member.Id, exp);

            await ctx.EditResponseAsync("👌");
        }

        [Command("remove_exp")]
        [Description("Отнимает опыт")]
        [RequirePermissions(DiscordPermission.Administrator)]
        public async Task RemoveExp(CommandContext ctx,
            [Description("Кол-во опыта")] int exp,
            [Description("Пользователь")] DiscordUser? user = null)
        {
            await ctx.DeferEphemeralAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            var member = user is null ? ctx.Member : await ctx.Guild.GetMemberAsync(user.Id);
            if (member is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Member not found!"));
                return;
            }

            GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();

            await statsService.RemoveExpAsync(ctx.Client, ctx.Guild.Id, ctx.Channel.Id, member.Id, exp);

            await ctx.EditResponseAsync("👌");
        }

        [Command("set_role")]
        [Description("Устанавливает уровень для роли")]
        [RequirePermissions(DiscordPermission.Administrator)]
        public async Task SetRole(CommandContext ctx,
            [Description("Роль")] DiscordRole role,
            [Description("Уровень, если пусто - > убирает уровень с роли")] int? level)
        {
            await ctx.DeferEphemeralAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            GuildRoleService guildRoleService = ctx.ServiceProvider.GetRequiredService<GuildRoleService>();

            await guildRoleService.SetRoleLevelAsync(ctx.Guild.Id, role.Id, level);

            List<GuildRole> guildRoles = await guildRoleService.GetGuildRolesAsync(ctx.Guild.Id);

            var embedBuilder = new DiscordEmbedBuilder();
            var stringBuilder = new StringBuilder();
            embedBuilder.WithTitle("Роли");

            foreach (GuildRole guildRole in guildRoles)
                stringBuilder.AppendLine($"<@&{guildRole.Id}> - {guildRole.LevelToGetRole.ToString() ?? "0"} уровень");

            embedBuilder.WithDescription(stringBuilder.ToString());

            await ctx.EditResponseAsync(embedBuilder.Build());
        }
    }
}
