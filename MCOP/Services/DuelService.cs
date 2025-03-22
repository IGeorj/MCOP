using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using MCOP.Services.Duels;
using MCOP.Services.Duels.Anomalies;
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

            if (duel.ActiveAnomaly != null)
            {
                string anomalyDescription = string.IsNullOrEmpty(duel.ActiveAnomaly.Description) ? $": {duel.ActiveAnomaly.Description}" : "";
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

        public async Task HandleSpecificUserDuelAsync(CommandContext ctx, DiscordUser user, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
        {
            var member2 = await ctx.Guild.GetMemberAsync(user.Id);

            if (ctx.User.Id == user.Id)
            {
                await HandleSelfDuelAsync(ctx, member2, timeoutMinutes, embed, duelButton);
                return;
            }

            embed.WithThumbnail(member2.AvatarUrl);
            embed.AddField("Бойцы", $"{ctx.Member.DisplayName} vs {member2.DisplayName}");

            var duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

            var interactivityResult = await duelMessage.WaitForButtonAsync(member2, TimeSpan.FromMinutes(5));
            if (interactivityResult.TimedOut)
            {
                await DeleteDuelMessageAsync(duelMessage);
                return;
            }

            duelButton.Disable();

            duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

            await StartDuelAnimationAsync(ctx, ctx.Member, member2, duelMessage, timeoutMinutes);
        }

        private async Task HandleSelfDuelAsync(CommandContext ctx, DiscordMember member2, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
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

        public async Task HandleOpenDuelAsync(CommandContext ctx, int timeoutMinutes, DiscordEmbedBuilder embed, DiscordButtonComponent duelButton)
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
                await DeleteDuelMessageAsync(duelMessage);
                return;
            }

            var member2 = await ctx.Guild.GetMemberAsync(interactivityResult.Result.User.Id);
            embed.WithThumbnail(member2.AvatarUrl);

            await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(duelButton));

            await StartDuelAnimationAsync(ctx, ctx.Member, member2, duelMessage, timeoutMinutes);
        }

        private async Task DeleteDuelMessageAsync(DiscordMessage duelMessage)
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

        public async Task StartDuelAnimationAsync(CommandContext ctx, DiscordMember member1, DiscordMember member2, DiscordMessage duelMessage, int timeoutMinutes, bool activateAnomaly = true)
        {
            var duel = new Duel(member1, member2, duelMessage, activateAnomaly);

            var embed = await InitializeDuelEmbedAsync(ctx, duel);
            var newDuelMessage = await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
            duel.DuelMessage = newDuelMessage;

            await Task.Delay(1500);

            bool isPlayer1First = new SafeRandom().Next(2) == 0;

            string actionString = "";
            while (duel.DuelMember1.HP > 0 && duel.DuelMember2.HP > 0 || (duel.ActiveAnomaly is GlitchHorrorAnomaly && duel.IsDuelEndedPrematurely == false))
            {
                if (isPlayer1First)
                {
                    (duel.DuelMember1.HP, duel.DuelMember2.HP, actionString) = duel.ProcessTurn(duel.DuelMember1, duel.DuelMember2);
                    if (duel.ActiveAnomaly is GlitchHorrorAnomaly && duel.IsDuelEndedPrematurely == true) break;

                    embed = UpdateDuelEmbed(ctx, embed, duel, actionString);
                    newDuelMessage = await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
                    duel.DuelMessage = newDuelMessage;

                    if (duel.DuelMember2.HP <= 0 && duel.ActiveAnomaly is not GlitchHorrorAnomaly) break;
                }
                else
                {
                    (duel.DuelMember1.HP, duel.DuelMember2.HP, actionString) = duel.ProcessTurn(duel.DuelMember2, duel.DuelMember1);
                    if (duel.ActiveAnomaly is GlitchHorrorAnomaly && duel.IsDuelEndedPrematurely == true) break;

                    embed = UpdateDuelEmbed(ctx, embed, duel, actionString);
                    newDuelMessage = await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed));
                    duel.DuelMessage = newDuelMessage;

                    if (duel.DuelMember1.HP <= 0 && duel.ActiveAnomaly is not GlitchHorrorAnomaly) break;
                }

                isPlayer1First = !isPlayer1First;
                await Task.Delay(1500);
            }

            await FinishDuel(ctx, embed, timeoutMinutes, duel, actionString);
        }

        private async Task<DiscordEmbedBuilder> InitializeDuelEmbedAsync(CommandContext ctx, Duel duel)
        {
            var appEmojies = await ctx.Client.GetApplicationEmojisAsync();
            var duelEmoji = appEmojies.FirstOrDefault(x => x.Name == "legionCommander");

            SafeRandom rng = new();
            var member1Emoji = rng.ChooseRandomElement(ctx.Guild.Emojis.Where(x => !x.Value.IsManaged)).Value;
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
            if (duel.IsDuelEndedPrematurely && duel.DuelMessage is not null)
            {
                await DeleteDuelMessageAsync(duel.DuelMessage);
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
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, duel.DuelMember1.Member.Id);
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, duel.DuelMember2.Member.Id);
                try
                {
                    await duel.DuelMember1.Member.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Ничья в дуели");
                    await duel.DuelMember2.Member.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Ничья в дуели");
                }
                catch (Exception)
                {
                    Log.Information("Не удалось назначить таймаут пользователю.");
                }
            }
        }
    }
}
