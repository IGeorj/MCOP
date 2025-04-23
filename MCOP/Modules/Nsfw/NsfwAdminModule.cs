using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using Serilog;
using System.ComponentModel;

namespace MCOP.Modules.Nsfw
{
    [Command("nsfw-daily")]
    [Description("18+ комманды")]
    [RequireNsfw]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class NsfwAdminModule
    {
        public SankakuService Sankaku { get; set; }
        public E621Service E621 { get; set; }
        public GelbooruService Gelbooru { get; set; }
        public GuildConfigService GuildService { get; set; }

        public NsfwAdminModule(SankakuService sankaku, E621Service e621, GelbooruService gelbooru, GuildConfigService guildService)
        {
            Sankaku = sankaku;
            E621 = e621;
            Gelbooru = gelbooru;
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

        [Command("set-channel")]
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

        [Command("resend-all")]
        [Description("Скидывает ежедневный топ картинок на все сервера")]
        public async Task SendDaily(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var guildConfigs = await GuildService.GetGuildConfigsWithLewdChannelAsync();
            List<DiscordChannel> channels = new List<DiscordChannel>();

            foreach (var config in guildConfigs)
            {
                if (ctx.Client.Guilds.ContainsKey(config.GuildId) && config.LewdChannelId is not null)
                {
                    try
                    {
                        channels.Add(await ctx.Client.GetChannelAsync(config.LewdChannelId.Value));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, $"Cannot send daily Guild: {config.GuildId}, Channel: {config.LewdChannelId.Value}");
                        continue;
                    }
                }
            }
            await Sankaku.SendDailyTopToChannelsAsync(channels);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(string.Join(",", channels)));
        }

    }
}
