using DSharpPlus.Entities;
using MCOP.Common.ChoiceProvider;
using MCOP.Core.Common;
using MCOP.Services.Duels.Anomalies;
using MCOP.Services.Duels.Anomalies.PokerAnomaly;

namespace MCOP.Services.Duels
{
    public class Duel
    {
        public int DelayBetweenTurn { get; set; } = 1500;
        public DuelMember DuelMember1 { get; }
        public DuelMember DuelMember2 { get; }
        public DiscordMessage? DuelMessage { get; set; }
        public DuelAnomaly? ActiveAnomaly { get; private set; } = null;
        public string LastActionString { get; set; } = "";
        public bool IsDuelEndedPrematurely { get; set; } = false;

        public delegate void DamageCalculator(DuelMember attacker, DuelMember defender, int damage);
        public delegate void TurnEndedHandler(DuelMember attacker, DuelMember defender);

        public event DamageCalculator? OnDamageCalculated;
        public event TurnEndedHandler? OnTurnEnded;
        public Duel(DiscordMember player1, DiscordMember player2, DiscordMessage duelMessage, string anomaly = AnomalyProvider.Random)
        {
            DuelMember1 = new DuelMember(player1);
            DuelMember2 = new DuelMember(player2);
            DuelMessage = duelMessage;

            ActiveAnomaly = GetAnomalyFromChoice(anomaly);
            ActiveAnomaly?.ApplyEffect(this);
        }

        private DuelAnomaly GetRandomAnomaly()
        {
            var anomalies = new List<DuelAnomaly>
            {
                new DoubleDamageAnomaly(),
                new InstantWinAnomaly(),
                new PoisonAnomaly(),
                new DodgeAnomaly(),
                new HarakiriAnomaly(),
                new ElementalAnomaly(),
                new CounterAttackAnomaly(),
                new SelfDamageAnomaly(),
                new SecondChanceAnomaly(),
                new PokerAnomaly()
            };

            if (new SafeRandom().Next(100) < 3)
                return new GlitchHorrorAnomaly();

            return new SafeRandom().ChooseRandomElement(anomalies);
        }

        private DuelAnomaly? GetAnomalyFromChoice(string choiceValue)
        {
            return choiceValue switch
            {
                AnomalyProvider.Random => GetRandomAnomaly(),
                AnomalyProvider.NoAnomaly => null,
                nameof(DoubleDamageAnomaly) => new DoubleDamageAnomaly(),
                nameof(InstantWinAnomaly) => new InstantWinAnomaly(),
                nameof(PoisonAnomaly) => new PoisonAnomaly(),
                nameof(DodgeAnomaly) => new DodgeAnomaly(),
                nameof(HarakiriAnomaly) => new HarakiriAnomaly(),
                nameof(ElementalAnomaly) => new ElementalAnomaly(),
                nameof(CounterAttackAnomaly) => new CounterAttackAnomaly(),
                nameof(SelfDamageAnomaly) => new SelfDamageAnomaly(),
                nameof(SecondChanceAnomaly) => new SecondChanceAnomaly(),
                nameof(GlitchHorrorAnomaly) => new GlitchHorrorAnomaly(),
                nameof(PokerAnomaly) => new PokerAnomaly(),
                _ => throw new ArgumentException("Invalid anomaly choice", nameof(choiceValue))
            };
        }

        public (int Player1HP, int Player2HPб, string ActionString) ProcessTurn(DuelMember attacker, DuelMember defender)
        {
            attacker.IncrementTurnCounter();

            LastActionString = "";

            int damage = new SafeRandom().Next(10, 25);
            string actionString = "";

            if (OnDamageCalculated is not null)
                OnDamageCalculated.Invoke(attacker, defender, damage);
            else
                defender.ApplyDamage(damage);

            if (!string.IsNullOrEmpty(LastActionString))
                actionString = $"{LastActionString}";
            else
                actionString = $"{attacker.Name} бьет вилкой и наносит {damage} урона! ⚔️";

            if (DuelMember1.HP <= 20 && DuelMember2.HP <= 20 && new SafeRandom().Next(100) < 8 && ActiveAnomaly is not GlitchHorrorAnomaly)
            {
                DuelMember1.HP = 0;
                DuelMember2.HP = 0;

                if (ActiveAnomaly is HarakiriAnomaly)
                    actionString = $"Блядь, мы себя захуярили!";
                else
                    actionString = $"Вы оба умерли от кринжа!";
            }

            LastActionString = "";

            OnTurnEnded?.Invoke(attacker, defender);

            if (!string.IsNullOrEmpty(LastActionString))
                actionString += $"\n{LastActionString}";

            return (DuelMember1.HP, DuelMember2.HP, actionString);
        }
    }
}
