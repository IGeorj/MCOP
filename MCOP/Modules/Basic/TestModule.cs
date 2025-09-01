using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Scoped;

namespace MCOP.Modules.Basic
{
    [Command("test")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class TestModule
    {
        [Command("images")]
        public async Task Random(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            IGuildUserStatsService guildUserStatsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();

            var topTen =  await guildUserStatsService.GetGuildUserStatsAsync(ctx.Guild.Id, pageSize: 10);
            UserTopRendered userTopRendered = new UserTopRendered();
            var sKImage = userTopRendered.RenderTable(topTen.stats, 1000);

            var sKData = sKImage.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 95);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddFile("top10.jpg", sKData.AsStream()));
        }
    }
}
