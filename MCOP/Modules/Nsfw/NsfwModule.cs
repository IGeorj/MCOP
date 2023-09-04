using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MCOP.Attributes.SlashCommands;
using MCOP.Core.Common.Booru;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;

namespace MCOP.Modules.Nsfw
{
    [SlashCommandGroup("nsfw", "18+ комманды")]
    [SlashRequireNsfw]
    [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
    public sealed class NsfwModule : ApplicationCommandModule
    {
        public SankakuService Sankaku { get; set; }
        public E621Service E621 { get; set; }
        public GelbooruService Gelbooru { get; set; }
        public GuildService GuildService { get; set; }

        public NsfwModule(SankakuService sankaku, E621Service e621, GelbooruService gelbooru, GuildService guildService)
        {
            Sankaku = sankaku;
            E621 = e621;
            Gelbooru = gelbooru;
            GuildService = guildService;
        }

        [SlashCommand("lewd", "Скидывает рандомную 18+ аниме картику")]
        public async Task Lewd(InteractionContext ctx,
            [Maximum(5)][Option("amount", "Кол-во картинок за раз. Максимум - 5 шт.")] long amount = 1,
            [Option("tags", "Пример: genshin_impact female")] string tags = "")
        {
            await ctx.DeferAsync();

            try
            {
                SearchResult searchResult = await Sankaku.GetRandomSearchResultAsync(tags: tags);

                var posts = searchResult.ToBooruPosts();
                int take = (int)amount;

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Скамлю сайт на картиночки..."));

                for (int i = 0; i < posts.Count; i += take)
                {
                    foreach (var post in posts.GetRange(i, Math.Min(take, posts.Count - i)))
                    {
                        var booruMessage = await Sankaku.SendBooruPostAsync(ctx.Channel, post);
                        _ = CreateDeleteReactionAsync(ctx, booruMessage);
                    }

                    var nextButton = new DiscordButtonComponent(
                        ButtonStyle.Success,
                        "lewd_next_button",
                        "Дальше",
                        false);

                    var cancelButton = new DiscordButtonComponent(
                        ButtonStyle.Danger,
                        "lewd_cancel_button",
                        "Отмена",
                        false);

                    var repeatMessageBuilder = new DiscordMessageBuilder()
                        .AddComponents(nextButton, cancelButton);

                    var repeatMessage = await ctx.Channel.SendMessageAsync(repeatMessageBuilder);

                    var buttonResult = await repeatMessage.WaitForButtonAsync(e =>
                    {
                        if (e.User.IsBot || e.Message != repeatMessage)
                            return false;

                        if ((e.User.Id == ctx.User.Id || ((DiscordMember)e.User).IsAdmin()))
                        {
                            return true;
                        }

                        return false;
                    });

                    if (buttonResult.TimedOut || buttonResult.Result.Id == cancelButton.CustomId)
                    {
                        try
                        {
                            searchResult.DeleteAllFiles();
                            await repeatMessage.DeleteAsync();
                            return;
                        }
                        catch (Exception)
                        {
                        }

                    }

                    await repeatMessage.DeleteAsync();
                }

                searchResult.DeleteAllFiles();

                await ctx.Channel.SendMessageAsync("Ничего не найдено или картинки закончились...");

            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync(e.Message);
                return;
            }

        }

        [SlashCommand("furry", "Скидывает рандомную 18+ фурри картику")]
        public async Task Furry(InteractionContext ctx,
            [Maximum(5)][Option("amount", "Кол-во картинок за раз. Максимум - 5 шт.")] long amount = 1,
            [Option("tags", "Пример: black_nose female")] string tags = "")
        {
            await ctx.DeferAsync();

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
            [Maximum(5)][Option("amount", "Кол-во гифок за раз. Максимум - 5 шт.")] long amount = 1,
            [Option("tags", "Пример: genshin_impact female")] string tags = "")
        {
            await ctx.DeferAsync();

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

        [SlashRequireOwner]
        [SlashCommand("daily-top", "Скидывает ежедневный топ картинок")]
        public async Task LewdTop(InteractionContext ctx,
            [Maximum(80)][Option("amount", "Кол-во картинок за раз. Максимум - 80 шт.")] long amount = 80,
            [Option("days", "Сколько дней назад (по умолчанию - 1)")] long days = 1)
        {
            await ctx.DeferAsync();

            try
            {
                List<DiscordMessage> messages = await Sankaku.SendDailyTopToChannelsAsync(new List<DiscordChannel> { ctx.Channel }, (int)amount, daysShift: (int)days);

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }

        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("set-daily-channel", "Устанавливает канал для ежедневного топа")]
        public async Task SetLewdChannel(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            await GuildService.SetLewdChannelAsync(ctx.Guild.Id, ctx.Channel.Id);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Успешно! Дневной топ будет высылаться сюда в <t:1672592400:t>"));
        }

        [SlashRequireOwner]
        [SlashCommand("send-daily", "Отправляет ежедневные на все сервера")]
        public async Task SendDaily(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            var guildConfigs = await GuildService.GetGuildConfigsWithLewdChannelAsync();
            List<DiscordChannel> channels = new List<DiscordChannel>();
            foreach (var config in guildConfigs)
            {
                channels.Add(await ctx.Client.GetChannelAsync(config.LewdChannelId.Value));
            }
            await Sankaku.SendDailyTopToChannelsAsync(channels);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(string.Join(",", channels)));
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
