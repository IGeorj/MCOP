using DSharpPlus.Entities;

namespace MCOP.Core.Models
{
    public sealed record BotStatusDto(
        int Id,
        string Status,
        DiscordActivityType Activity
    );
}
