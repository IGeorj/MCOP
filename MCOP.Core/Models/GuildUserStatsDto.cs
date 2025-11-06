
using MCOP.Core.Helpers;

namespace MCOP.Core.Models
{
    public sealed class GuildUserStatsDto
    {
        public string GuildId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? AvatarHash { get; set; }
        public int DuelWin { get; set; }
        public int DuelLose { get; set; }
        public int Likes { get; set; }
        public int Exp { get; set; }
        public int CurrentLevelExp => (int)LevelingHelper.GetTotalXPForLevel(Level);
        public int NextLevelExp => (int)LevelingHelper.GetTotalXPForLevel(Level + 1);
        public int Level => LevelingHelper.GetLevelFromTotalExp(Exp);
        public double WinRate => DuelWin + DuelLose > 0
            ? Math.Round((double)DuelWin / (DuelWin + DuelLose) * 100, 2)
            : 0;
        public ulong CustomLikeEmojiId { get; set; } = 0;
        public string CustomLikeEmojiName { get; set; } = "❤️";
    }
}
