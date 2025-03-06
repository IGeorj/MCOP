using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MCOP.Core.Common.Booru;
using MCOP.Core.Services.Booru;
using MCOP.Core.Services.Scoped;
using MCOP.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MCOP.Modules.Nsfw
{
    [Command("nsfw")]
    [Description("18+ комманды")]
    [RequireNsfw]
    public sealed class NsfwModule
    {
        public SankakuService Sankaku { get; set; }
        public E621Service E621 { get; set; }
        public GelbooruService Gelbooru { get; set; }
        public GuildConfigService GuildService { get; set; }

        public NsfwModule(SankakuService sankaku, E621Service e621, GelbooruService gelbooru, GuildConfigService guildService)
        {
            Sankaku = sankaku;
            E621 = e621;
            Gelbooru = gelbooru;
            GuildService = guildService;
        }

        [Command("lewd")]
        [Description("Скидывает рандомную 18+ аниме картику")]
        public async Task Lewd(CommandContext ctx,
            [MinMaxValue(1, 5)][Description("Кол-во картинок за раз. Максимум - 5 шт.")] int amount = 1,
            [SlashAutoCompleteProvider<SankakuTagsCompleteProvider>][Description("Пример: genshin_impact female")] string tag_1 = "",
            [SlashAutoCompleteProvider<SankakuTagsCompleteProvider>][Description("Пример: genshin_impact female")] string tag_2 = "",
            [SlashAutoCompleteProvider<SankakuTagsCompleteProvider>][Description("Пример: genshin_impact female")] string tag_3 = "",
            [SlashAutoCompleteProvider<SankakuTagsCompleteProvider>][Description("Пример: genshin_impact female")] string tag_4 = "",
            [SlashAutoCompleteProvider<SankakuTagsCompleteProvider>][Description("Пример: genshin_impact female")] string tag_5 = "")
        {
            await ctx.DeferResponseAsync();

            try
            {
                var regEx = new Regex(@"\(\d* шт\.\)");
                string tags = Regex.Replace($"{regEx.Replace(tag_1, "")} {regEx.Replace(tag_2, "")} {regEx.Replace(tag_3, "")} {regEx.Replace(tag_4, "")} {regEx.Replace(tag_5, "")}", @"\s+", " ");
                SearchResult searchResult = await Sankaku.GetRandomSearchResultAsync(tags: tags);

                var posts = searchResult.ToBooruPosts();

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Скамлю сайт на картиночки..."));

                for (int i = 0; i < posts.Count; i += amount)
                {
                    foreach (var post in posts.GetRange(i, Math.Min(amount, posts.Count - i)))
                    {
                        var booruMessage = await Sankaku.SendBooruPostAsync(ctx.Channel, post);
                        _ = CreateDeleteReactionAsync(ctx, booruMessage);
                    }

                    var nextButton = new DiscordButtonComponent(
                        DiscordButtonStyle.Success,
                        "lewd_next_button",
                        "Дальше",
                        false);

                    var cancelButton = new DiscordButtonComponent(
                        DiscordButtonStyle.Danger,
                        "lewd_cancel_button",
                        "Отмена",
                        false);

                    var repeatMessageBuilder = new DiscordMessageBuilder()
                        .WithContent($"**Теги**: {tags} **Кол-во**: {amount}")
                        .AddComponents(nextButton, cancelButton);

                    var repeatMessage = await ctx.Channel.SendMessageAsync(repeatMessageBuilder);

                    var buttonResult = await repeatMessage.WaitForButtonAsync(e =>
                    {
                        if (e.User.IsBot || e.Message != repeatMessage)
                            return false;

                        if (e.User.Id == ctx.User.Id || e.Guild.GetMemberAsync(e.User.Id).GetAwaiter().GetResult().IsAdmin())
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

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Ничего не найдено или картинки закончились..."));

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

        }

        [Command("furry")]
        [Description("Скидывает рандомную 18+ фурри картику")]
        public async Task Furry(CommandContext ctx,
            [MinMaxValue(1, 5)][Description("Кол-во картинок за раз. Максимум - 5 шт.")] int amount = 1,
            [Description("Пример: black_nose female")] string tags = "")
        {
            await ctx.DeferResponseAsync();

            try
            {
                (List<DiscordMessage>, string?) result = await E621.SendRandomImagesAsync(ctx.Channel, amount, tags);

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


        [Command("gif")]
        [Description("Скидывает рандомную 18+ аниме гифку")]
        public async Task Gif(CommandContext ctx,
            [MinMaxValue(1, 5)][Description("Кол-во гифок за раз. Максимум - 5 шт.")] int amount = 1,
            [Description("Пример: genshin_impact female")] string tags = "")
        {
            await ctx.DeferResponseAsync();

            tags += " animated_gif";
            try
            {
                (List<DiscordMessage>, string?) result = await Gelbooru.SendRandomImagesAsync(ctx.Channel, amount, tags);

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

        [RequireApplicationOwner]
        [Command("send-top")]
        [Description("Скидывает ежедневный топ картинок")]
        public async Task LewdTop(CommandContext ctx,
            [MinMaxValue(1, 80)][Description("Кол-во картинок за раз. Максимум - 80 шт.")] int amount = 80,
            [Description("Сколько дней назад (по умолчанию - 1)")] long days = 1)
        {
            await ctx.DeferResponseAsync();

            try
            {
                List<DiscordMessage> messages = await Sankaku.SendDailyTopToChannelsAsync(new List<DiscordChannel> { ctx.Channel }, amount, daysShift: (int)days);

            }
            catch (Exception e)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(e.Message));
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Держи свои картиночки"));
        }

        [RequirePermissions(DiscordPermission.Administrator)]
        [Command("set-daily-channel")]
        [Description("Устанавливает канал для ежедневного топа")]
        public async Task SetLewdChannel(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            if (ctx.Guild is null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Guild not found!"));
                return;
            }

            await GuildService.SetLewdChannelAsync(ctx.Guild.Id, ctx.Channel.Id);
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Успешно! Дневной топ будет высылаться сюда в <t:1672592400:t>"));
        }

        [RequireApplicationOwner]
        [Command("send-daily-all")]
        [Description("Скидывает ежедневный топ картинок на все сервера")]
        public async Task SendDaily(CommandContext ctx)
        {
            await ctx.DeferResponseAsync();

            var guildConfigs = await GuildService.GetGuildConfigsWithLewdChannelAsync();
            List<DiscordChannel> channels = new List<DiscordChannel>();

            foreach (var config in guildConfigs)
            {
                if (ctx.Client.Guilds.ContainsKey(config.GuildId) && config.LewdChannelId is not null)
                {
                    try
                    {
                        channels.Add(await ctx.Client.GetChannelAsync(config.LewdChannelId.Value));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, $"Cannot send daily Guild: {config.GuildId}, Channel: {config.LewdChannelId.Value}");
                        continue;
                    }
                }
            }
            await Sankaku.SendDailyTopToChannelsAsync(channels);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(string.Join(",", channels)));
        }

        private static async Task CreateDeleteReactionAsync(CommandContext ctx, DiscordMessage message)
        {
            var removeEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry_sign:");

            await message.CreateReactionAsync(removeEmoji);

            var interactivity = ctx.Client.ServiceProvider.GetRequiredService<InteractivityExtension>();

            InteractivityResult<MessageReactionAddedEventArgs> res = await interactivity.WaitForReactionAsync(
                e =>
                {
                    if (e.User.IsBot || e.Message != message)
                        return false;

                    if ((e.User.Id == ctx.User.Id || e.Guild.GetMemberAsync(e.User.Id).GetAwaiter().GetResult().IsAdmin()) && (e.Emoji == removeEmoji))
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
