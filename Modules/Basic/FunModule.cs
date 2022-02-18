using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MCOP.Attributes.SlashCommands;
using MCOP.Common;
using MCOP.Database.Models;
using MCOP.Extensions;
using MCOP.Modules.Basic.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MCOP.Modules.Basic
{
    [SlashCooldown(1, 5, CooldownBucketType.Channel)]
    public sealed class FunModule : ApplicationCommandModule
    {
        private static readonly Dictionary<string, string> _nouns = new Dictionary<string, string>
            {
                {"яйца", "НЕ ТРОГАЙ ЯЯЯЯЯИИИЦАААААА"},
                {"крысу", "Крыса укусила за жэпу"},
                {"cum", "Выпил cum"},
                {"грывню", "Доллар победил"},
                {"гавно", "Ладно..."},
                {"хуй", "У оппонента длиннее"},
                {"шаверму", "Мясо в шавухе просроченное"},
                {"мать", "Проебали..."},
                {"честь и отвагу", "Ты из орды"},
                {"майонез", "Это не майонез"},
                {"битву", "Умер от кринжа"},
                {"плотину", "Камень не дали"},
                {"хабар", "Налутал консервных банок"},
                {"донбас", "Словил снаряд"},
                {"лупой", "А ты в ней пупа..."},
            };

        [SlashRequirePermissions(Permissions.Administrator)]
        [SlashCommand("test", "Test")]
        public async Task Test(InteractionContext ctx,
            [Option("guildID", "Message ID")] string guildID,
            [Option("userID", "Message ID")] string userID)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                var gid = ulong.Parse(guildID);
                var uid = ulong.Parse(userID);

                UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
                await statsService.AddWinAsync(gid, uid);
                await statsService.AddLoseAsync(gid, uid);
            }
            catch (Exception)
            {

                throw;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("success"));

        }

        [SlashRequirePermissions(Permissions.Administrator)]
        [SlashCommand("hash", "Hash images from message")]
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
                message = await ctx.Channel.GetMessageAsync(ulongMessageId);
                hashes = await message.GetImageHashesAsync();

                var hashService = ctx.Services.GetRequiredService<ImageHashService>();
                var messageService = ctx.Services.GetRequiredService<UserMessageService>();


                ulong outMessageId;

                foreach (var hash in hashes)
                {
                    bool result = hashService.TryGetSimilar(hash, 90, out outMessageId, out _);

                    if (result)
                    {
                        continue;
                    }

                    ImageHash imageHash = new();
                    imageHash.Hash = hash;

                    UserMessage? userMessage = await messageService.GetOrAddAsync(ctx.Guild.Id, ulongMessageId);
                    imageHash.GuildId = userMessage.GuildId;
                    imageHash.MessageId = userMessage.MessageId;

                    await hashService.AddAsync(imageHash);
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
            [Option("user", "Пользователь")] DiscordUser? user = null,
            [Minimum(20)][Maximum(120)][Option("timeout", "Минут таймача. 20 - 120 минут")] long? timeout = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                SecureRandom rng = new SecureRandom();

                string strTimeout = rng.Next(19, 120).ToString();

                if (timeout.HasValue)
                {
                    strTimeout = timeout.Value.ToString();
                }

                KeyValuePair<string, string> randomNoun = _nouns.ElementAt(rng.Next(0, _nouns.Count));
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Битва за {randomNoun.Key}",
                    Description = "Для принятия дулеи поставьте смайлик на это сообщение"
                }
                .AddField("Время бана", $"{strTimeout} минут", true)
                .AddField("Кулдаун", "5 минут", true)
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl);

                DiscordMember member2;
                DiscordMessage duelMessage;

                if (user is not null)
                {
                    if (ctx.User.Id == user.Id)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("https://tenor.com/view/idk-i-dont-know-sebastian-stan-lol-wtf-gif-5364867"));
                    }

                    member2 = (DiscordMember)user;

                    embed.WithThumbnail(member2.AvatarUrl);

                    duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                    var result = await duelMessage.WaitForReactionAsync(member2, TimeSpan.FromMinutes(5));
                    if (result.TimedOut)
                    {
                        await duelMessage.DeleteAsync();
                        return;
                    }
                }
                else
                {
                    duelMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                    var interactivity = ctx.Client.GetInteractivity();
                    InteractivityResult<MessageReactionAddEventArgs> res = await interactivity.WaitForReactionAsync(
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

                    if (res.TimedOut)
                    {
                        await duelMessage.DeleteAsync();
                        return;
                    }

                    member2 = (DiscordMember)res.Result.User;

                    embed.WithThumbnail(member2.AvatarUrl);
                    await duelMessage.ModifyAsync("", embed.Build());
                }

                (DiscordMember, DiscordMember) winnerLoser = rng.Next(2) == 1 ? (ctx.Member, member2) : (member2, ctx.Member);

                embed.AddField("Победитель", winnerLoser.Item1.DisplayName);


                UserStatsService statsService = ctx.Services.GetRequiredService<UserStatsService>();
                await statsService.AddWinAsync(ctx.Guild.Id, winnerLoser.Item1.Id);
                await statsService.AddLoseAsync(ctx.Guild.Id, winnerLoser.Item2.Id);

                await ctx.Channel.SendMessageAsync($"{winnerLoser.Item2.Mention} - {randomNoun.Value}, помянем");
                var emoji = DiscordEmoji.FromGuildEmote(ctx.Client, 475694805691793409);
                await ctx.Channel.SendMessageAsync($"{emoji}");

                await duelMessage.ModifyAsync("", embed.Build());

                await winnerLoser.Item2.TimeoutAsync(DateTime.Now.AddMinutes(int.Parse(strTimeout)), randomNoun.Value);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
