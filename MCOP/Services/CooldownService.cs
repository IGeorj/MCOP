using DSharpPlus.Entities;

namespace MCOP.Services
{
    public sealed class CooldownService
    {
        private static readonly Dictionary<(ulong UserId, string CommandName), (DateTime LastUsed, TimeSpan CooldownDuration)> _cooldowns = new();

        public bool IsOnCooldown(DiscordUser user, string commandName)
        {
            if (_cooldowns.TryGetValue((user.Id, commandName), out var cooldownData))
            {
                var (lastUsed, cooldownDuration) = cooldownData;
                var timeRemaining = lastUsed + cooldownDuration - DateTime.UtcNow;
                return timeRemaining > TimeSpan.Zero;
            }
            return false;
        }

        public TimeSpan GetRemainingCooldown(DiscordUser user, string commandName)
        {
            if (_cooldowns.TryGetValue((user.Id, commandName), out var cooldownData))
            {
                var (lastUsed, cooldownDuration) = cooldownData;
                return lastUsed + cooldownDuration - DateTime.UtcNow;
            }
            return TimeSpan.Zero;
        }

        public void UpdateCooldown(DiscordUser user, string commandName, TimeSpan cooldownDuration)
        {
            _cooldowns[(user.Id, commandName)] = (DateTime.UtcNow, cooldownDuration);
        }
    }
}
