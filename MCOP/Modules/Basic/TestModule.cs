using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Services.Image;
using MCOP.Core.Services.Shared;

namespace MCOP.Modules.Basic
{
    [Command("test")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class TestModule
    {
        [Command("images")]
        public async Task Random(CommandContext ctx, params DiscordAttachment[] images)
        {
            await ctx.DeferResponseAsync();

            byte[] img1Bytes = await HttpService.GetByteArrayAsync(images[0].Url);
            byte[] img2Bytes = await HttpService.GetByteArrayAsync(images[1].Url);
            using var bitmap1 = SkiaSharp.SKBitmap.Decode(img1Bytes);
            using var bitmap2 = SkiaSharp.SKBitmap.Decode(img2Bytes);
            var test1 = SkiaSharpService.GetPercentageDifference(bitmap1, bitmap2);
            var test2 = SkiaSharpService.GetNormalizedDifference(bitmap1, bitmap2);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Default: {test1}% Normalized:{test2}%"));
        }
    }
}
