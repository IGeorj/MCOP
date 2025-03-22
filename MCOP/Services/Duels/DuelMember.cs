using DSharpPlus.Entities;

namespace MCOP.Services.Duels
{
    public class DuelMember
    {
        public readonly static int InitialHP = 120;

        public DiscordMember Member { get; }
        public int HP { get; set; } = InitialHP;

        private string? _customName;
        public string Name => _customName ?? Member.DisplayName;
        public int TurnCounter { get; private set; } = 0;
        public int ResetCounter { get; private set; } = 0;

        public DuelMember(DiscordMember member, string? customName = null)
        {
            Member = member;

            if (customName != null) 
                _customName = customName;
        }
        public void SetCustomName(string name) => _customName = name;
        public void IncrementTurnCounter() => TurnCounter++;
        public void ApplyDamage(int damage) => HP = Math.Max(0, HP - damage);

    }
}
