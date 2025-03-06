using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel;
using System.Globalization;

namespace MCOP.Modules.Basic
{
    public sealed class FunModule
    {
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

        [RequirePermissions(DiscordPermission.Administrator)]
        [Command("test")]
        [Description("Проверка бота на ответ")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("success"));
        }

        [Command("duel")]
        [Description("Дуель за таймач")]
        public async Task Duel(CommandContext ctx,
            [Description("Кому кидаем дуель")] DiscordUser? user = null,
            [MinMaxValue(20, 80)][Description("20 - 80 минут, по умолчанию рандомит")] int? timeout = null)
        {
            await ctx.DeferResponseAsync();


            if (ctx.Member is null || ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("User or Guild not found!"));
                return;
            }

            try
            {
                SafeRandom rng = new();

                int timeoutMinutes = timeout ?? rng.Next(19, 80);
                string timeoutString = TimeSpan.FromMinutes(timeoutMinutes).Humanize(culture: new CultureInfo("ru"));


                KeyValuePair<string, string> randomNoun = _nouns.ElementAt(rng.Next(0, _nouns.Count));
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"{randomNoun.Key}")
                    .AddField("Время бана", $"{timeoutString}", true)
                    .AddField("Кулдаун", "5 минут", true)
                    .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl);

                var duelButton = new DiscordButtonComponent(
                    DiscordButtonStyle.Primary,
                    "duel_button",
                    "",
                    false,
                    new DiscordComponentEmoji("⚔️"));

                DiscordMember member2;
                DiscordMessage duelMessage;

                if (user is not null)
                {
                    member2 = await ctx.Guild.GetMemberAsync(user.Id);

                    if (ctx.User.Id == user.Id)
                    {
                        var mcopGuild = await ctx.Client.GetGuildAsync(GlobalVariables.McopServerId);
                        var durka = DiscordEmoji.FromGuildEmote(ctx.Client, 839771710265229314);

                        embed.AddField("Результат", $"🥇**{durka}** vs {ctx.Member.DisplayName}");
                        embed.WithThumbnail(member2.AvatarUrl);
                        duelButton = duelButton.Disable();

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

                        try
                        {
                            await member2.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), randomNoun.Value);
                        }
                        catch (Exception)
                        {
                            Log.Information("Duel failed timeout User");
                        }

                        return;
                    }

                    embed.WithThumbnail(member2.AvatarUrl);
                    embed.AddField("Бойцы", $"{ctx.Member.DisplayName} vs {member2.DisplayName}");

                    duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

                    var interactivityResult = await duelMessage.WaitForButtonAsync(member2, TimeSpan.FromMinutes(5));
                    if (interactivityResult.TimedOut)
                    {
                        try
                        {
                            await duelMessage.DeleteAsync();
                        }
                        catch (Exception)
                        {
                            Log.Information("Duel message doesn't exist");
                            return;
                        }
                        return;
                    }
                }
                else
                {
                    var webhookBuilder = new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton);
                    duelMessage = await ctx.EditResponseAsync(webhookBuilder);

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
                        try
                        {
                            await duelMessage.DeleteAsync();
                        }
                        catch (Exception)
                        {
                            Log.Information("Duel message doesn't exist");
                            return;
                        }
                        return;
                    }

                    member2 = await ctx.Guild.GetMemberAsync(interactivityResult.Result.User.Id);

                    embed.WithThumbnail(member2.AvatarUrl);

                    await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(duelButton));
                }

                (DiscordMember, DiscordMember) winnerLoser = rng.Next(2) == 1 ? (ctx.Member, member2) : (member2, ctx.Member);

                embed.AddField("Результат", $"🥇**{winnerLoser.Item1.DisplayName}** vs {winnerLoser.Item2.DisplayName}");


                GuildUserStatsService statsService = ctx.ServiceProvider.GetRequiredService<GuildUserStatsService>();
                await statsService.AddDuelWinAsync(ctx.Guild.Id, winnerLoser.Item1.Id);
                await statsService.AddDuelLoseAsync(ctx.Guild.Id, winnerLoser.Item2.Id);

                var emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 475694805691793409);
                await ctx.Channel.SendMessageAsync($"{winnerLoser.Item2.Mention} - {randomNoun.Value} помянем {emoji}");

                await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(duelButton));

                try
                {
                    await winnerLoser.Item2.TimeoutAsync(DateTime.Now.AddMinutes(timeoutMinutes), "Проебал дуель");
                }
                catch (Exception)
                {
                    Log.Information("Duel failed timeout User");
                }

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
