using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Common.Helpers;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using System.ComponentModel;

namespace MCOP.Modules.User
{
    [Command("reaction")]
    [Description("Команды Реакций")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class ReactionsModule
    {
        [Command("settings")]
        [Description("Установка настроек для реакций")]
        [RequirePermissions(DiscordPermission.Administrator)]
        public async Task SetReactionSettings(CommandContext ctx, bool? enabled = null, DiscordEmoji? emoji = null)
        {
            await ctx.DeferEphemeralAsync();

            var guild = await CommandContextHelper.ValidateAndGetGuildAsync(ctx);
            if (guild is null) return;

            if (emoji is not null && emoji.IsManaged)
            {
                await ctx.EditResponseAsync("Ошибочная. Емодзи добавлен сторонней интеграцией и не может быть использован");
                return;
            }

            var configService = ctx.ServiceProvider.GetRequiredService<IGuildConfigService>();

            if (enabled.HasValue)
                await configService.SetGuildReactionTrackingAsync(guild.Id, enabled.Value);

            if (emoji is not null)
                await configService.SetGuildLikeEmojiAsync(guild.Id, emoji.Name, emoji.Id);

            await ctx.EditResponseAsync("Настройки успешно применены!");
        }
    }
}
