using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using System.ComponentModel;

namespace MCOP.Modules.Nsfw
{
    [Command("nsfw-admin")]
    [Description("18+ комманды")]
    [RequireNsfw]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class NsfwModuleAdmin
    {
        public SankakuService Sankaku { get; set; }
        public IGuildConfigService GuildService { get; set; }

        public NsfwModuleAdmin(SankakuService sankaku, IGuildConfigService guildService)
        {
            Sankaku = sankaku;
            GuildService = guildService;
        }

        [Command("top")]
        [Description("Скидывает ежедневный топ картинок")]
        public async Task LewdTop(CommandContext ctx,
            [MinMaxValue(1, 80)][Description("Кол-во картинок за раз. Максимум - 80 шт.")] int amount = 80,
            [Description("Сколько дней назад (по умолчанию - 1)")] long days = 1)
        {
            await ctx.DeferResponseAsync();

            try
            {
                List<DiscordMessage> messages = await Sankaku.SendDailyTopToChannelsAsync(new List<DiscordChannel> { ctx.Channel }, amount, daysShift: (int)days);

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }

        [Command("channel")]
        [Description("Устанавливает канал для ежедневного топа")]
        public async Task SetLewdChannel(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            await GuildService.SetLewdChannelAsync(ctx.Guild.Id, ctx.Channel.Id);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Успешно! Дневной топ будет высылаться сюда в <t:1672592400:t>"));
        }
    }
}
