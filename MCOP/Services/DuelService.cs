using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MCOP.Common.ChoiceProvider;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using MCOP.Services.Duels;
using MCOP.Services.Duels.Anomalies;
using Serilog;
using System.Globalization;

namespace MCOP.Services
{
    public sealed class DuelService
    {
        private readonly SafeRandom _rng = new();

        private static readonly Dictionary<string, string> _nouns = new()
        {
                {"Дуель за штангу", "Поперхнулся протеином..."},
                {"Дуель в аниме мире", "Переродился в жопную затычку..."},
                {"Дуель на члениксе", "Умер от кринжа..."},
                {"Дуель в туалете", "Утонул в говне..."},
                {"Дуель умом", "Потерял хромосому..."},
                {"Дуель за шаверму", "С фирменным соусом..."},
                {"Дуель за мать", "Та за шо..."},
                {"Дуель в dungeon", "А ты в ней Slave..."},
                {"Дуель в космосе", "Улетел за жопной тяге..."},
                {"Дуель за профурсетку", "Приехала мама..."},
                {"Дуель за круасан", "Круасан сгорел..."},
                {"Дуель под пледиком", "А ты в ней тяночка..."},
                {"Дуель на миде", "Слил мид..."},
                {"Старый бог?", "Старый бог..."},
                {"Дуель возле Сируса", "Наступил в жижу..."},
                {"Биба боба", "Соснул у долбаёба..."},
            };

        public int GetTimeoutMinutes(int? timeout)
        {
            return timeout ?? _rng.Next(19, 80);
        }

        public string GetTimeoutString(int timeoutMinutes)
        {
            return TimeSpan.FromMinutes(timeoutMinutes).Humanize(culture: new CultureInfo("ru"));
        }

        public DiscordEmbedBuilder CreateDuelEmbed(CommandContext ctx, string timeoutString, int cooldownDurationMinutes, string? anomaly = "")
        {
            SafeRandom rng = new();
            KeyValuePair<string, string> randomNoun = _nouns.ElementAt(rng.Next(0, _nouns.Count));

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{randomNoun.Key}")
                .AddField("Время бана", $"{timeoutString}", true)
                .AddField("Кулдаун", $"{cooldownDurationMinutes} минут", true)
                .WithAuthor(ctx.Member!.DisplayName, null, ctx.Member.AvatarUrl);

            if (!string.IsNullOrEmpty(anomaly))
                embed.AddField("Аномалия", anomaly, true);

            return embed;
        }

        private DiscordEmbedBuilder UpdateDuelEmbed(
            CommandContext ctx,
            DiscordEmbedBuilder embed,
            Duel duel,
            string actionString = "")
        {
            embed.ClearFields();

            embed.AddField("Здоровье", $"{duel.DuelMember1.Name}: {duel.DuelMember1.HP} ❤️\n{duel.DuelMember2.Name}: {duel.DuelMember2.HP} ❤️");

            if (!string.IsNullOrEmpty(actionString))
            {
                embed.AddField("Действие", actionString);
            }

            if (duel.ActiveAnomaly is not null)
            {
                string anomalyDescription = !string.IsNullOrEmpty(duel.ActiveAnomaly.Description) ? $": {duel.ActiveAnomaly.Description}" : "";
                embed.AddField("Аномалия", $"{duel.ActiveAnomaly.Name}{anomalyDescription}");
            }

            return embed;
        }

        public DiscordButtonComponent CreateDuelButton()
        {
            return new DiscordButtonComponent(
                DiscordButtonStyle.Primary,
                "duel_button",
                "",
                false,
                new DiscordComponentEmoji("⚔️"));
        }

        public async Task HandleSpecificUserDuelAsync(CommandContext ctx, DiscordUser user, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton, string anomaly = AnomalyProvider.Random)
        {
            var member2 = await ctx.Guild!.GetMemberAsync(user.Id);

            if (ctx.User.Id == user.Id)
            {
                await HandleSelfDuelAsync(ctx, member2, timeoutMinutes, embed, duelButton);
                return;
            }

            embed.WithThumbnail(member2.AvatarUrl);
            embed.AddField("Бойцы", $"{ctx.Member!.DisplayName} vs {member2.DisplayName}");

            var duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddActionRowComponent(duelButton));

            var interactivityResult = await duelMessage.WaitForButtonAsync(member2, TimeSpan.FromMinutes(5));
            if (interactivityResult.TimedOut)
            {
                await duelMessage.DeleteSilentAsync();
                return;
            }

            duelButton.Disable();

            duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddActionRowComponent(duelButton));

            await StartDuelAnimationAsync(ctx, ctx.Member, member2, duelMessage, timeoutMinutes, anomaly);
        }

        private async Task HandleSelfDuelAsync(CommandContext ctx, DiscordMember member2, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
        {
            var mcopGuild = await ctx.Client.GetGuildAsync(GlobalVariables.McopServerId);
            var durka = await mcopGuild.GetEmojiAsync(839771710265229314);

            embed.AddField("Результат", $"🥇**{durka}** vs {member2.DisplayName}");
            embed.WithThumbnail(member2.AvatarUrl);
            duelButton = duelButton.Disable();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddActionRowComponent(duelButton));

            await TryTimeoutUserAsync(member2, timeoutMinutes, "Дуельный шизиод");
        }

        public async Task HandleOpenDuelAsync(CommandContext ctx, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton, string anomaly = AnomalyProvider.Random)
        {
            var webhookBuilder = new DiscordWebhookBuilder().AddEmbed(embed).AddActionRowComponent(duelButton);
            var duelMessage = await ctx.EditResponseAsync(webhookBuilder);

            var interactivity = ctx.Client.ServiceProvider.GetRequiredService<InteractivityExtension>();
            var interactivityResult = await interactivity.WaitForButtonAsync(duelMessage,
                e =>
                {
                    if (ctx.User.Id == e.User.Id || e.User.IsBot)
                        return false;
                    if (e.Message == duelMessage)
                        return true;

                    return false;
                },
                TimeSpan.FromMinutes(5)
            );

            if (interactivityResult.TimedOut)
            {
                await duelMessage.DeleteSilentAsync();
                return;
            }

            var member2 = await ctx.Guild!.GetMemberAsync(interactivityResult.Result.User.Id);
            embed.WithThumbnail(member2.AvatarUrl);

            await StartDuelAnimationAsync(ctx, ctx.Member!, member2, duelMessage, timeoutMinutes, anomaly);
        }

        public async Task StartDuelAnimationAsync(CommandContext ctx, DiscordMember member1, DiscordMember member2, DiscordMessage duelMessage, int timeoutMinutes, string anomaly = AnomalyProvider.Random)
        {
            var duel = new Duel(member1, member2, duelMessage, anomaly);
            Log.Information("Duel Started guild:{guild}, channel:{channel} {anomaly}, {timeout}", ctx.Guild?.Name, ctx.Channel.Name, duel.ActiveAnomaly?.GetType().Name, timeoutMinutes);

            var embed = await GetInitialDuelEmbedAsync(ctx, duel);

            duel.DuelMessage = await ModifyDuelMessageAsync(duelMessage, duel, embed);

            if (duel.DuelMessage is null)
                return;

            await Task.Delay(duel.DelayBetweenTurn);

            bool isPlayer1First = new SafeRandom().Next(2) == 0;
            string actionString = "";

            while (IsDuelOngoing(duel))
            {
                var (attacker, defender) = isPlayer1First ? (duel.DuelMember1, duel.DuelMember2) : (duel.DuelMember2, duel.DuelMember1);

                (duel.DuelMember1.HP, duel.DuelMember2.HP, actionString) = duel.ProcessTurn(attacker, defender);

                if (duel.ActiveAnomaly is GlitchHorrorAnomaly && duel.IsDuelEndedPrematurely)
                    break;

                embed = UpdateDuelEmbed(ctx, embed, duel, actionString);

                duel.DuelMessage = await ModifyDuelMessageAsync(duelMessage, duel, embed);

                if (duel.DuelMessage is null)
                    return;

                if ((duel.DuelMember1.HP <= 0 || duel.DuelMember2.HP <= 0) && duel.ActiveAnomaly is not GlitchHorrorAnomaly)
                    break;

                isPlayer1First = !isPlayer1First;
                await Task.Delay(duel.DelayBetweenTurn);
            }

            await FinishDuel(ctx, embed, timeoutMinutes, duel, actionString);
        }

        private async Task<DiscordMessage?> ModifyDuelMessageAsync(DiscordMessage duelMessage, Duel duel, DiscordEmbedBuilder embed)
        {
            try
            {
                return await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                duel.DuelMessage = null;
                Log.Error(ex, "Не удалось модифицировать сообщение дуели");
                return null;
            }
        }

        private bool IsDuelOngoing(Duel duel)
        {
            return (duel.DuelMember1.HP > 0 && duel.DuelMember2.HP > 0) ||
                   (duel.ActiveAnomaly is GlitchHorrorAnomaly && !duel.IsDuelEndedPrematurely);
        }

        private async Task<DiscordEmbedBuilder> GetInitialDuelEmbedAsync(CommandContext ctx, Duel duel)
        {
            var appEmojies = await ctx.Client.GetApplicationEmojisAsync();
            var duelEmoji = appEmojies.FirstOrDefault(x => x.Name == "legionCommander");

            SafeRandom rng = new();
            var member1Emoji = rng.ChooseRandomElement(ctx.Guild!.Emojis.Where(x => !x.Value.IsManaged)).Value;
            var member2Emoji = rng.ChooseRandomElement(ctx.Guild.Emojis.Where(x => !x.Value.IsManaged)).Value;

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"{duelEmoji} Дуэль началась {duelEmoji}")
                .AddField("Здоровье", $"{duel.DuelMember1.Name}: {duel.DuelMember1.HP} ❤️\n{duel.DuelMember2.Name}: {duel.DuelMember2.HP} ❤️")
                .WithColor(DiscordColor.Red);

            if (duel.ActiveAnomaly is not GlitchHorrorAnomaly)
                embed.WithDescription($"{member1Emoji} {duel.DuelMember1.Name} vs {duel.DuelMember2.Name} {member2Emoji}");

            if (duel.ActiveAnomaly is not null)
            {
                embed.AddField("Аномалия", $"{duel.ActiveAnomaly.Name}: {duel.ActiveAnomaly.Description}");
            }

            return embed;
        }

        private async Task FinishDuel(CommandContext ctx, DiscordEmbedBuilder embed, int timeoutMinutes, Duel duel, string actionString)
        {
            if (duel.DuelMessage is null || ctx.Guild is null)
                return;

            if (duel.IsDuelEndedPrematurely && duel.DuelMessage is not null)
            {
                await duel.DuelMessage.DeleteSilentAsync();
                return;
            }

            string resultMessage;
            DiscordMember? winner;
            DiscordMember? loser;

            if (duel.DuelMember1.HP > 0 && duel.DuelMember2.HP <= 0)
            {
                resultMessage = $"{duel.DuelMember1.Name} побеждает! 🏆";
                winner = duel.DuelMember1.Member;
                loser = duel.DuelMember2.Member;
            }
            else if (duel.DuelMember2.HP > 0 && duel.DuelMember1.HP <= 0)
            {
                resultMessage = $"{duel.DuelMember2.Name} побеждает! 🏆";
                winner = duel.DuelMember2.Member;
                loser = duel.DuelMember1.Member;
            }
            else
            {
                resultMessage = "Ничья! 🤝";
                winner = null;
                loser = null;
            }

            embed
                .ClearFields()
                .WithTitle($"⚔️ Дуэль закончилась ⚔️")
                .AddField("Результат", resultMessage)
                .AddField("Здоровье", $"{duel.DuelMember1.Name}: {duel.DuelMember1.HP} ❤️\n{duel.DuelMember2.Name}: {duel.DuelMember2.HP} ❤️")
                .WithColor(DiscordColor.Green);

            if (duel.ActiveAnomaly is not null)
                embed.AddField("Аномалия", $"{duel.ActiveAnomaly.Name}: {duel.ActiveAnomaly.Description}");

            if (!string.IsNullOrWhiteSpace(actionString))
                embed.AddField("Последнее действие", actionString);

            if (duel.DuelMessage is not null)
                await duel.DuelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));

            var statsService = ctx.ServiceProvider.GetRequiredService<IGuildUserStatsService>();

            if (winner is not null)
            {
                await statsService.AddDuelWinAsync(ctx.Guild.Id, winner.Id);
            }

            if (loser is not null)
            {
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, loser.Id);
                await TryTimeoutUserAsync(loser, timeoutMinutes, "Проиграл дуель");
            }

            if (loser is null && winner is null)
            {
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, duel.DuelMember1.Member.Id);
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, duel.DuelMember2.Member.Id);

                await TryTimeoutUserAsync(duel.DuelMember1.Member, timeoutMinutes, "Ничья в дуели");
                await TryTimeoutUserAsync(duel.DuelMember2.Member, timeoutMinutes, "Ничья в дуели");
            }
        }

        private static async Task TryTimeoutUserAsync(DiscordMember? loser, int timeoutMinutes, string message)
        {
            if (loser is null)
                return;

            try
            {
                await loser.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), message);
            }
            catch (Exception)
            {
                Log.Information("Не удалось назначить таймаут пользователю.");
            }
        }
    }
}
