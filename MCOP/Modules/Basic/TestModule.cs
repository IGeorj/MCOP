using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Common;

namespace MCOP.Modules.Basic
{
    [Command("test")]
    [RequirePermissions(DiscordPermission.Administrator)]
    public sealed class TestModule
    {
        [Command("random")]
        public async Task Random(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();
            SafeRandom rng = new SafeRandom();
            Dictionary<int, int> keyValues = new Dictionary<int, int>();

            keyValues[0] = 0;
            keyValues[1] = 0;
            keyValues[2] = 0;

            for (int i = 0; i < 1000; i++)
            {
                int randomNumber = rng.Next(2);
                keyValues[randomNumber]++;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{keyValues[0]} {keyValues[1]} {keyValues[2]}"));
        }
    }
}
