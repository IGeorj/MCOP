using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MCOP.Attributes.SlashCommands;
using MCOP.Extensions;
using MCOP.Modules.Nsfw.Common;
using MCOP.Modules.Nsfw.Services;

namespace MCOP.Modules.Nsfw
{
    [SlashCommandGroup("nsfw", "18+ комманды")]
    [SlashRequireNsfw]
    [SlashRequireChannelId(857354195866615808, 549313253541543951)]
    [SlashCooldown(1,5, CooldownBucketType.User)]
    public sealed class NsfwModule : ApplicationCommandModule
    {
        public SankakuService Sankaku { get; set; }
        public E621Service E621 { get; set; }
        public GelbooruService Gelbooru { get; set; }


        [SlashCommand("lewd", "Скидывает рандомную 18+ аниме картику")]
        public async Task Lewd(InteractionContext ctx,
            [Maximum(5)][Option("amount", "Кол-во картинок за раз. Предел - 5 шт.")] long amount = 1,
            [Option("tags", "Пример: genshin_impact female")] string tags = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                SearchResult result = await Sankaku.GetRandomSearchResultAsync(tags: tags);

                var posts = result.ToList();
                int take = (int)amount;

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Скамлю сайт на картиночки..."));

                for (int i = 0; i < posts.Count; i += take)
                {
                    foreach (var post in posts.GetRange(i, Math.Min(take, posts.Count - i)))
                    {
                        var booruMessage = await Sankaku.SendBooruPostAsync(ctx.Channel, post);
                        _ = CreateDeleteReactionAsync(ctx, booruMessage);
                    }

                    var repeatMessage = await ctx.Channel.SendMessageAsync("Едем дальше?");

                    DiscordEmoji yes = DiscordEmoji.FromName(ctx.Client, ":thumbsup:");
                    DiscordEmoji no = DiscordEmoji.FromName(ctx.Client, ":thumbsdown:");

                    DiscordEmoji.TryFromGuildEmote(ctx.Client, 864097093110595615, out yes);
                    DiscordEmoji.TryFromGuildEmote(ctx.Client, 844213126026231899, out no);

                    await repeatMessage.CreateReactionAsync(yes);
                    await repeatMessage.CreateReactionAsync(no);

                    var interactivity = ctx.Client.GetInteractivity();

                    InteractivityResult<MessageReactionAddEventArgs> res = await interactivity.WaitForReactionAsync(
                        e => 
                        {
                            if (e.User.IsBot || e.Message != repeatMessage)
                                return false;

                            if ( (e.User.Id == ctx.User.Id || ((DiscordMember)e.User).IsAdmin()) && (e.Emoji == yes || e.Emoji == no) )
                            {
                                return true;
                            }

                            return false;
                        },
                        TimeSpan.FromSeconds(30)
                    );

                    if (res.TimedOut || res.Result.Emoji == no) 
                    {
                        await repeatMessage.DeleteAsync();
                        return; 
                    }

                    await repeatMessage.DeleteAsync();
                }
                await ctx.Channel.SendMessageAsync("Картинки закончились...");

            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
                return;
            }

        }

        [SlashCommand("furry", "Скидывает рандомную 18+ фурри картику")]
        public async Task Furry(InteractionContext ctx,
            [Option("amount", "Кол-во картинок")] long amount = 1,
            [Option("tags", "Пример: black_nose female")] string tags = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                (List<DiscordMessage>, string?) result = await E621.SendRandomImagesAsync(ctx.Channel, (int)amount, tags);

                foreach (var item in result.Item1)
                {
                    _ = CreateDeleteReactionAsync(ctx, item);
                }
            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }


        [SlashCommand("gif", "Скидывает рандомную 18+ аниме гифку")]
        public async Task Gif(InteractionContext ctx,
            [Option("amount", "Кол-во картинок")] long amount = 1,
            [Option("tags", "Пример: genshin_impact female")] string tags = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            tags += " animated_gif";
            try
            {
                (List<DiscordMessage>, string?) result = await Gelbooru.SendRandomImagesAsync(ctx.Channel, (int)amount, tags);

                foreach (var item in result.Item1)
                {
                    _ = CreateDeleteReactionAsync(ctx, item);
                }

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }

        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("lewdtop", "Скидывает ежедневный топ")]
        public async Task LewdTop(InteractionContext ctx,
            [Option("amount", "Кол-во картинок")] long amount = 80)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            try
            {
                (List<DiscordMessage>, string?) result = await Sankaku.SendDailyTopAsync(ctx.Channel, (int)amount);

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }

        private static async Task CreateDeleteReactionAsync(InteractionContext ctx, DiscordMessage message)
        {
            var removeEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry_sign:");

            await message.CreateReactionAsync(removeEmoji);

            var interactivity = ctx.Client.GetInteractivity();

            InteractivityResult<MessageReactionAddEventArgs> res = await interactivity.WaitForReactionAsync(
                e =>
                {
                    if (e.User.IsBot || e.Message != message)
                        return false;

                    if ((e.User.Id == ctx.User.Id || ((DiscordMember)e.User).IsAdmin()) && (e.Emoji == removeEmoji))
                    {
                        return true;
                    }

                    return false;
                },
                TimeSpan.FromMinutes(1)
            );

            if (res.TimedOut)
            {
                await message.DeleteReactionsEmojiAsync(removeEmoji);
                return;
            }

            if (res.Result.Emoji == removeEmoji)
            {
                await message.DeleteAsync();
            }
        }

    }
}
