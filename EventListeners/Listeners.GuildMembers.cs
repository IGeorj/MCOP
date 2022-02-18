using MCOP.EventListeners.Attributes;
using MCOP.EventListeners.Common;
using MCOP.Services;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MCOP.Modules.Basic.Services;

namespace MCOP.EventListeners;

internal static partial class Listeners
{
    [AsyncEventListener(DiscordEventType.GuildMemberAdded)]
    public static async Task GuildMemberAddedHandlerAsync(Bot bot, GuildMemberAddEventArgs e)
    {
        var statsService = bot.Services.GetRequiredService<UserStatsService>();

        await statsService.ChangeActiveStatus(e.Guild.Id, e.Member.Id, true);
    }

    [AsyncEventListener(DiscordEventType.GuildMemberRemoved)]
    public static async Task GuildMemberRemovedHandlerAsync(Bot bot, GuildMemberRemoveEventArgs e)
    {
        var statsService = bot.Services.GetRequiredService<UserStatsService>();

        await statsService.ChangeActiveStatus(e.Guild.Id, e.Member.Id, false);
    }

}
