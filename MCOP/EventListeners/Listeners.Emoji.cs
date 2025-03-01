using DSharpPlus;
using DSharpPlus.EventArgs;
using MCOP.Core.Services.Scoped;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
namespace MCOP.EventListeners;

internal static partial class Listeners
{
    public static async Task GuildEmojisUpdated(DiscordClient client, GuildEmojisUpdatedEventArgs e)
    {
        var guildEmojiService = Services.GetRequiredService<GuildEmojiService>();
        var changedEmojis = e.EmojisAfter
            .Where(after => !e.EmojisBefore.ContainsKey(after.Key) || e.EmojisBefore[after.Key].GetDiscordName() != after.Value.GetDiscordName())
            .ToDictionary(after => after.Key, after => after.Value);

        await guildEmojiService.UpdateGuildEmojiesAsync(e.Guild.Id, changedEmojis);
    }
}
