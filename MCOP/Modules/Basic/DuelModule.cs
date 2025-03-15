using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MCOP.Core.Common;
using MCOP.Extensions;
using MCOP.Services;
using Serilog;
using System.ComponentModel;

namespace MCOP.Modules.Basic
{
    public sealed class DuelModule
    {
        private readonly DuelService _duelService;
        private readonly CooldownService _cooldownService;

        public DuelModule(DuelService duelService, CooldownService cooldownService)
        {
            _duelService = duelService;
            _cooldownService = cooldownService;
        }

        [RequirePermissions(DiscordPermission.Administrator)]
        [Command("test")]
        [Description("Проверка бота на ответ")]
        public async Task Test(CommandContext ctx)
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

        [Command("duel")]
        [Description("Дуель за таймач")]
        public async Task Duel(CommandContext ctx,
            [Description("Кому кидаем дуель")] DiscordUser? user = null,
            [MinMaxValue(20, 120)][Description("20 - 120 минут, по умолчанию рандомит")] int? timeout = null)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Member is null || ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("User or Guild not found!"));
                return;
            }

            string commandName = "duel";
            int cooldownDurationMinutes = 5;
            TimeSpan cooldownDuration = TimeSpan.FromMinutes(cooldownDurationMinutes);

            if (_cooldownService.IsOnCooldown(ctx.User, commandName) && !ctx.Member.IsAdmin())
            {
                var timeRemaining = _cooldownService.GetRemainingCooldown(ctx.User, commandName);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Вы можете использовать команду `{commandName}` снова через {timeRemaining:mm\\:ss}."));
                return;
            }

            _cooldownService.UpdateCooldown(ctx.User, commandName, cooldownDuration);

            try
            {
                int timeoutMinutes = _duelService.GetTimeoutMinutes(timeout);
                string timeoutString = _duelService.GetTimeoutString(timeoutMinutes);

                var embed = _duelService.CreateDuelEmbed(ctx, timeoutString, cooldownDurationMinutes);
                var duelButton = _duelService.CreateDuelButton();

                if (user is not null)
                {
                    await _duelService.HandleSpecificUserDuel(ctx, user, timeoutMinutes, embed, duelButton);
                }
                else
                {
                    await _duelService.HandleOpenDuel(ctx, timeoutMinutes, embed, duelButton);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during the duel command.");
                throw;
            }
        }
    }
}
