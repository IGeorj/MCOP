namespace MCOP.Controllers.Responses
{
    public class GuildResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public bool BotPresent { get; set; }
        public bool IsOwner { get; set; }
    }
}
