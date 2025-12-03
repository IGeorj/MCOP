namespace MCOP.Data
{
    public class GuildUserStatsProjection
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public string? Username { get; set; }
        public string? AvatarHash { get; set; }
        public int DuelWin { get; set; }
        public int DuelLose { get; set; }
        public int Exp { get; set; }
        public int Likes { get; set; }
    }
}
