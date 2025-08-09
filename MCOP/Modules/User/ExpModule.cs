using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Common.Helpers;
using MCOP.Core.Models;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using System.ComponentModel;
using System.Text;

namespace MCOP.Modules.User
{
    [Command("lvl")]
    [Description("Команды Лвла/Опыта")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class ExpModule
    {
        [Command("add_exp")]
        [Description("Добавляет опыт")]
        public async Task AddExp(CommandContext ctx,
            [Description("Кол-во опыта")] int exp,
            [Description("Пользователь")] DiscordUser? user = null)
        {
            await HandleExpModificationAsync(ctx, user, (service, guildId, channelId, userId) =>
                service.AddExpAsync(guildId, channelId, userId, exp));
        }

        [Command("remove_exp")]
        [Description("Отнимает опыт")]
        public async Task RemoveExp(CommandContext ctx,
            [Description("Кол-во опыта")] int exp,
            [Description("Пользователь")] DiscordUser? user = null)
        {
            await HandleExpModificationAsync(ctx, user, (service, guildId, channelId, userId) =>
                service.RemoveExpAsync(guildId, channelId, userId, exp));
        }

        [Command("set_role")]
        [Description("Устанавливает уровень для роли")]
        public async Task SetRole(CommandContext ctx,
            [Description("Роль")] DiscordRole role,
            [Description("Уровень, если пусто - > убирает уровень с роли")] int? level)
        {
            await HandleRoleOperationAsync(ctx, async service =>
            {
                await service.SetRoleLevelAsync(ctx.Guild!.Id, role.Id, level);
                return await BuildRolesEmbedAsync(service, ctx.Guild.Id, "Роли", r => r.LevelToGetRole?.ToString() ?? "0 уровень");
            });
        }

        [Command("set_blocked_role")]
        [Description("Устанавливает блок опыта для роли")]
        public async Task SetRole(CommandContext ctx,
            [Description("Роль")] DiscordRole role,
            [Description("Заблокировать да/нет")] bool isBlocked)
        {
            await HandleRoleOperationAsync(ctx, async service =>
            {
                await service.SetBlockedRoleAsync(ctx.Guild!.Id, role.Id, isBlocked);
                return await BuildRolesEmbedAsync(service, ctx.Guild.Id, "Заблокированные роли", _ => string.Empty);
            });
        }

        private async Task HandleExpModificationAsync(
            CommandContext ctx,
            DiscordUser? user,
            Func<IGuildUserStatsService, ulong, ulong, ulong, Task> operation)
        {
            await ctx.DeferEphemeralAsync();

            var (guild, member) = await CommandContextHelper.ValidateAndGetMemberAsync(ctx, user);
            if (guild is null || member is null) return;

            var statsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();
            await operation(statsService, guild.Id, ctx.Channel.Id, member.Id);
            await ctx.EditResponseAsync("👌");
        }

        private async Task HandleRoleOperationAsync(
            CommandContext ctx,
            Func<IGuildRoleService, Task<DiscordEmbed>> operation)
        {
            await ctx.DeferEphemeralAsync();

            var guild = await CommandContextHelper.ValidateAndGetGuildAsync(ctx);
            if (guild is null) return;

            var roleService = ctx.ServiceProvider.GetRequiredService<IGuildRoleService>();
            var embed = await operation(roleService);
            await ctx.EditResponseAsync(embed);
        }

        private async Task<DiscordEmbed> BuildRolesEmbedAsync(
            IGuildRoleService service,
            ulong guildId,
            string title,
            Func<GuildRoleDto, string> roleInfoFormatter)
        {
            var roles = await service.GetGuildRolesAsync(guildId);
            var embedBuilder = new DiscordEmbedBuilder().WithTitle(title);

            var descriptionBuilder = new StringBuilder();
            foreach (var role in roles)
            {
                descriptionBuilder.AppendLine($"<@&{role.Id}> - {roleInfoFormatter(role)}");
            }

            return embedBuilder.WithDescription(descriptionBuilder.ToString()).Build();
        }
    }
}
