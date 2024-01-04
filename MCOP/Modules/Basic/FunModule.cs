using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MCOP.Common;
using MCOP.Core.Common;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.Modules.Basic
{
    [SlashCooldown(1, 5, SlashCooldownBucketType.Channel)]
    public sealed class FunModule : ApplicationCommandModule
    {
        private static readonly Dictionary<string, string> _nouns = new()
        {
                {"яйца", "НЕ ТРОГАЙ ЯЯЯЯЯИИИЦАААААА"},
                {"крысу", "Крыса укусила за жэпу"},
                {"cum", "Выпил просроченный cum"},
                {"гавно", "Вкусно пожрал.."},
                {"хуй", "Хуй оказался сзади...."},
                {"шаверму", "Мясо в шавухе просроченное..."},
                {"мать", "Проебали..."},
                {"майонез", "Это не майонез"},
                {"битву", "Умер от кринжа"},
                {"плотину", "Камень не дали"},
                {"сыр с плесенью", "Плесень была настоящая..."},
                {"лупой", "А ты в ней пупа..."},
                {"мид", "Пудж хукнул из кустов"},
                {"туалет", "Утонул..."},
                {"бипки", "Да кто такие эти ваши бипки"},
            };

        [SlashRequirePermissions(Permissions.Administrator)]
        [SlashCommand("test", "Проверка бота на ответ")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("success"));
        }

        [SlashRequireOwner]
        [SlashCommand("hash", "Хеширует изображение из сообщения")]
        public async Task Hash(InteractionContext ctx,
            [Option("messageId", "Message ID")] string messageId)
        {
            ulong ulongMessageId = ulong.Parse(messageId);
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            int count = 0;
            DiscordMessage message;
            List<byte[]> hashes;

            try
            {
                var hashService = ctx.Services.GetRequiredService<ImageHashService>();
                message = await ctx.Channel.GetMessageAsync(ulongMessageId);
                hashes = await hashService.GetHashesFromMessageAsync(message);

                foreach (var hash in hashes)
                {
                    var hashFound = await hashService.SearchHashAsync(hash, 94);

                    if (hashFound is not null)
                    {
                        continue;
                    }

                    await hashService.SaveHashAsync(ctx.Guild.Id, message.Id, message.Author.Id, hash);
                    count++;
                }
            }
            catch (Exception)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message not found!"));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {count} of {hashes.Count} images"));
        }



        [SlashCommand("duel", "Дуель за таймач")]
        public async Task Duel(InteractionContext ctx,
            [Option("user", "Кому кидаем дуель")] DiscordUser? user = null,
            [Minimum(20)][Maximum(80)][Option("minutes", "20 - 80 минут, по умолчанию рандомит")] long? timeout = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                SafeRandom rng = new();

                string strTimeout = rng.Next(19, 80).ToString();

                if (timeout.HasValue)
                {
                    strTimeout = timeout.Value.ToString();
                }

                KeyValuePair<string, string> randomNoun = _nouns.ElementAt(rng.Next(0, _nouns.Count));
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Битва за {randomNoun.Key}")
                    .AddField("Время бана", $"{strTimeout} минут", true)
                    .AddField("Кулдаун", "5 минут", true)
                    .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl);

                var duelButton = new DiscordButtonComponent(
                    ButtonStyle.Primary,
                    "duel_button",
                    null,
                    false,
                    new DiscordComponentEmoji("⚔️"));

                DiscordMember member2;
                DiscordMessage duelMessage;

                if (user is not null)
                {
                    member2 = (DiscordMember)user;

                    if (ctx.User.Id == user.Id)
                    {
                        var mcopGuild = await ctx.Client.GetGuildAsync(GlobalVariables.McopServerId);
                        var durka = DiscordEmoji.FromGuildEmote(ctx.Client, 839771710265229314);

                        embed.AddField("Победитель", durka);
                        embed.WithThumbnail(member2.AvatarUrl);
                        duelButton = duelButton.Disable();

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(duelButton));

                        try
                        {
                            await member2.TimeoutAsync(DateTime.Now.AddMinutes(int.Parse(strTimeout)), randomNoun.Value);
                        }
                        catch (Exception)
                        {
                            Log.Information("Duel failed timeout User");
                        }

                        return;
                    }

                    embed.WithThumbnail(member2.AvatarUrl);

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

                    var interactivity = ctx.Client.GetInteractivity();
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

                embed.AddField("Победитель", winnerLoser.Item1.DisplayName);


                UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
                await statsService.ChangeWinAsync(ctx.Guild.Id, winnerLoser.Item1.Id, 1);
                await statsService.ChangeLoseAsync(ctx.Guild.Id, winnerLoser.Item2.Id, 1);

                var emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 475694805691793409);
                await ctx.Channel.SendMessageAsync($"{winnerLoser.Item2.Mention} - {randomNoun.Value}, помянем {emoji}");

                await duelMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(duelButton));

                try
                {
                    await winnerLoser.Item2.TimeoutAsync(DateTime.Now.AddMinutes(int.Parse(strTimeout)), randomNoun.Value);
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
