using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using Serilog;
using System.ComponentModel;

namespace MCOP.Modules.Nsfw
{
    [Command("nsfw-owner")]
    [Description("18+ комманды")]
    [RequireNsfw]
    [RequireApplicationOwner]
    public sealed class NsfwModuleOwner
    {
        public SankakuService Sankaku { get; set; }
        public GuildConfigService GuildService { get; set; }

        public NsfwModuleOwner(SankakuService sankaku, GuildConfigService guildService)
        {
            Sankaku = sankaku;
            GuildService = guildService;
        }

        [Command("resend-top")]
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
