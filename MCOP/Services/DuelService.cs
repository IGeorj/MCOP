using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Globalization;

namespace MCOP.Services
{
    public class DuelService
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

        public DiscordEmbedBuilder CreateDuelEmbed(CommandContext ctx, string timeoutString, int cooldownDurationMinutes)
        {
            SafeRandom rng = new();
            KeyValuePair<string, string> randomNoun = _nouns.ElementAt(rng.Next(0, _nouns.Count));

            return new DiscordEmbedBuilder()
                .WithTitle($"{randomNoun.Key}")
                .AddField("Время бана", $"{timeoutString}", true)
                .AddField("Кулдаун", $"{cooldownDurationMinutes} минут", true)
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl);
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

        public async Task HandleSpecificUserDuel(CommandContext ctx, DiscordUser user, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
        {
            var member2 = await ctx.Guild.GetMemberAsync(user.Id);

            if (ctx.User.Id == user.Id)
            {
                await HandleSelfDuel(ctx, member2, timeoutMinutes, embed, duelButton);
                return;
            }

            embed.WithThumbnail(member2.AvatarUrl);
            embed.AddField("Бойцы", $"{ctx.Member.DisplayName} vs {member2.DisplayName}");

            var duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

            var interactivityResult = await duelMessage.WaitForButtonAsync(member2, TimeSpan.FromMinutes(5));
            if (interactivityResult.TimedOut)
            {
                await DeleteDuelMessage(duelMessage);
                return;
            }

            duelButton.Disable();

            duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

            await StartDuelAnimation(ctx, ctx.Member, member2, duelMessage, timeoutMinutes);
        }

        private async Task HandleSelfDuel(CommandContext ctx, DiscordMember member2, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
        {
            var mcopGuild = await ctx.Client.GetGuildAsync(GlobalVariables.McopServerId);
            var durka = await mcopGuild.GetEmojiAsync(839771710265229314);

            embed.AddField("Результат", $"🥇**{durka}** vs {member2.DisplayName}");
            embed.WithThumbnail(member2.AvatarUrl);
            duelButton = duelButton.Disable();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

            try
            {
                await member2.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Проебал дуель");
            }
            catch (Exception)
            {
                Log.Information("Duel failed timeout User: {DisplayName}", member2.DisplayName);
            }
        }

        public async Task HandleOpenDuel(CommandContext ctx, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
        {
            var webhookBuilder = new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton);
            var duelMessage = await ctx.EditResponseAsync(webhookBuilder);

            var interactivity = ctx.Client.ServiceProvider.GetRequiredService<InteractivityExtension>();
            var interactivityResult = await interactivity.WaitForButtonAsync(duelMessage,
                e =>
                {
                    if (ctx.User.Id == e.User.Id || e.User.IsBot)
                        return false;
                    if (e.Message == duelMessage)
                    {
                        duelButton = duelButton.Disable();
                        return true;
                    }

                    return false;
                },
                TimeSpan.FromMinutes(5)
            );

            if (interactivityResult.TimedOut)
            {
                await DeleteDuelMessage(duelMessage);
                return;
            }

            var member2 = await ctx.Guild.GetMemberAsync(interactivityResult.Result.User.Id);
            embed.WithThumbnail(member2.AvatarUrl);

            await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(duelButton));

            await StartDuelAnimation(ctx, ctx.Member, member2, duelMessage, timeoutMinutes);
        }

        private async Task DeleteDuelMessage(DiscordMessage duelMessage)
        {
            try
            {
                await duelMessage.DeleteAsync();
            }
            catch (Exception)
            {
                Log.Information("Failed to delete duel message: {messageId}", duelMessage.Id);
            }
        }

        public async Task StartDuelAnimation(CommandContext ctx, DiscordMember member1, DiscordMember member2, DiscordMessage duelMessage, int timeoutMinutes)
        {
            int player1HP = 120;
            int player2HP = 120;

            SafeRandom rng = new();
            bool isPlayer1First = rng.Next(2) == 0;

            var embed = await InitializeDuelEmbedAsync(ctx, member1, member2, player1HP, player2HP);
            await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));

            await Task.Delay(1500);

            while (player1HP > 0 && player2HP > 0)
            {
                if (isPlayer1First)
                {
                    (player1HP, player2HP) = await ProcessPlayerTurn(ctx, embed, member1, member2, member1, member2, player1HP, player2HP, duelMessage);
                    if (player2HP <= 0) break;
                }
                else
                {
                    (player1HP, player2HP) = await ProcessPlayerTurn(ctx, embed, member2, member1, member1, member2, player1HP, player2HP, duelMessage);
                    if (player1HP <= 0) break;
                }

                isPlayer1First = !isPlayer1First;
            }

            await FinishDuel(ctx, embed, member1, member2, player1HP, player2HP, duelMessage, timeoutMinutes);
        }

        private async Task<DiscordEmbedBuilder> InitializeDuelEmbedAsync(CommandContext ctx, DiscordMember member1, DiscordMember member2, int player1HP, int player2HP)
        {
            var appEmojies = await ctx.Client.GetApplicationEmojisAsync();
            var duelEmoji = appEmojies.FirstOrDefault(x => x.Name == "legionCommander");

            SafeRandom rng = new();
            var member1Emoji = rng.ChooseRandomElement(ctx.Guild.Emojis.Where(x => !x.Value.IsManaged)).Value;
            var member2Emoji = rng.ChooseRandomElement(ctx.Guild.Emojis.Where(x => !x.Value.IsManaged)).Value;

            return new DiscordEmbedBuilder()
                .WithTitle($"{duelEmoji} Дуэль началась {duelEmoji}")
                .WithDescription($"{member1Emoji} {member1.DisplayName} vs {member2.DisplayName} {member2Emoji}")
                .AddField("Здоровье", $"{member1.DisplayName}: {player1HP} ❤️\n{member2.DisplayName}: {player2HP} ❤️")
                .WithColor(DiscordColor.Red);
        }

        private async Task<(int Member1HP, int Member2HP)> ProcessPlayerTurn(
            CommandContext ctx,
            DiscordEmbedBuilder embed,
            DiscordMember attacker,
            DiscordMember defender,
            DiscordMember player1,
            DiscordMember player2,
            int member1HP,
            int member2HP,
            DiscordMessage duelMessage)
        {
            SafeRandom rng = new();
            int damage = rng.Next(10, 25);
            bool isDodged = rng.Next(100) < 20;

            var mcopGuild = await ctx.Client.GetGuildAsync(GlobalVariables.McopServerId);
            var jokergeEmoji = await mcopGuild.GetEmojiAsync(1220499135745097809);

            embed.ClearFields();

            string actionString = "";

            if (member1HP <= 20 && member2HP <= 20 && rng.Next(100) < 10)
            {
                member1HP = 0;
                member2HP = 0;

                actionString = $"Вы оба умерли от кринжа!";
            }

            bool isDraw = member1HP == 0 && member2HP == 0;

            if (isDodged && !isDraw)
            {
                actionString = $"{attacker.DisplayName} бьет вилкой, но {defender.DisplayName} уворачивается! {jokergeEmoji}";
            }
            else if(!isDraw)
            {
                if (defender.Id == player1.Id)
                {
                    member1HP = Math.Max(0, member1HP - damage);
                }
                else
                {
                    member2HP = Math.Max(0, member2HP - damage);
                }

                actionString = $"{attacker.DisplayName} бьет вилкой и наносит {damage} урона! ⚔️";
            }

            string healthField = $"{player1.DisplayName}: {member1HP} ❤️\n{player2.DisplayName}: {member2HP} ❤️";
            embed.AddField("Здоровье", healthField);
            embed.AddField("Действие", actionString);

            await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
            await Task.Delay(1500);

            return (member1HP, member2HP);
        }

        private async Task FinishDuel(CommandContext ctx, DiscordEmbedBuilder embed, DiscordMember member1, DiscordMember member2, int player1HP, int player2HP, DiscordMessage duelMessage, int timeoutMinutes)
        {
            string resultMessage;
            DiscordMember? winner;
            DiscordMember? loser;

            if (player1HP > 0 && player2HP <= 0)
            {
                resultMessage = $"{member1.DisplayName} побеждает! 🏆";
                winner = member1;
                loser = member2;
            }
            else if (player2HP > 0 && player1HP <= 0)
            {
                resultMessage = $"{member2.DisplayName} побеждает! 🏆";
                winner = member2;
                loser = member1;
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
                .AddField("Здоровье", $"{member1.DisplayName}: {player1HP} ❤️\n{member2.DisplayName}: {player2HP} ❤️")
                .WithColor(DiscordColor.Green);

            await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));

            GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();
            if (winner is not null)
            {
                await statsService.AddDuelWinAsync(ctx.Guild.Id, winner.Id);
            }
            if (loser is not null)
            {
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, loser.Id);
                try
                {
                    await loser.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Проиграл дуэль");
                }
                catch (Exception)
                {
                    Log.Information("Не удалось назначить таймаут пользователю.");
                }
            }
            if (loser is null && winner is null)
            {
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, member1.Id);
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, member2.Id);
                try
                {
                    await member1.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Ничья в дуели");
                    await member2.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Ничья в дуели");
                }
                catch (Exception)
                {
                    Log.Information("Не удалось назначить таймаут пользователю.");
                }
            }
        }
    }
}
