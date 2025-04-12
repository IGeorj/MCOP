using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MCOP.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MCOP.Extensions;

internal static class CommandContextExtensions
{
    public static async Task FailAsync(this CommandContext ctx, string msg)
    {
        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
        {
            Description = $"{Emojis.X} {msg}",
            Color = DiscordColor.IndianRed
        });
    }

    public static async Task DeferEphemeralAsync(this CommandContext ctx)
    {
        if (ctx is SlashCommandContext slashContext)
        {
            await slashContext.DeferResponseAsync(ephemeral: true);
        }
        else
        {
            await ctx.DeferResponseAsync();
        }
    }

    public static Task PaginateAsync<T>(this CommandContext ctx, string title, IEnumerable<T> collection,
                                        Func<T, string> selector, DiscordColor? color = null, int pageSize = 10)
    {
        T[] arr = collection.ToArray();

        var pages = new List<Page>();
        int pageCount = (arr.Length - 1) / pageSize + 1;
        int from = 0;
        for (int i = 1; i <= pageCount; i++)
        {
            int to = from + pageSize > arr.Length ? arr.Length : from + pageSize;
            pages.Add(new Page(embed: new DiscordEmbedBuilder
            {
                Title = title,
                Description = arr[from..to].Select(selector).JoinWith(),
                Color = color ?? DiscordColor.Black,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Showing {from + 1}-{to} out of {arr.Length} ; Page {i}/{pageCount}",
                }
            }));
            from += pageSize;
        }

        return pages.Count > 1
            ? ctx.Client.ServiceProvider.GetRequiredService<InteractivityExtension>().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages)
            : ctx.Channel.SendMessageAsync(content: pages.First().Content, embed: pages.First().Embed);
    }

    public static Task PaginateAsync<T>(this CommandContext ctx, IEnumerable<T> collection,
                                        Func<DiscordEmbedBuilder, T, DiscordEmbedBuilder> formatter, DiscordColor? color = null)
    {
        int count = collection.Count();

        IEnumerable<Page> pages = collection
            .Select((e, i) =>
            {
                var emb = new DiscordEmbedBuilder();
                emb.WithFooter($"Showing #{i + 1} out of {count}", null);
                emb.WithColor(color ?? DiscordColor.Black);
                emb = formatter(emb, e);
                return new Page { Embed = emb.Build() };
            });

        return count > 1
            ? ctx.Client.ServiceProvider.GetRequiredService<InteractivityExtension>().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages)
            : ctx.Channel.SendMessageAsync(content: pages.Single().Content, embed: pages.Single().Embed);
    }
}
