namespace MCOP.Controllers.Responses
{
    public class GuildRoleResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int? LevelToGetRole { get; set; }
        public bool IsGainExpBlocked { get; set; }
        public string? LevelUpMessageTemplate { get; set; }
    }
}
