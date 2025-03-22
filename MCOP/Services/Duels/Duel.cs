using DSharpPlus.Entities;
using MCOP.Core.Common;
using MCOP.Services.Duels.Anomalies;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

namespace MCOP.Services.Duels
{
    public class Duel
    {
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
        public Duel(DiscordMember player1, DiscordMember player2, DiscordMessage duelMessage, bool activateAnomaly = true)
        {
            DuelMember1 = new DuelMember(player1);
            DuelMember2 = new DuelMember(player2);
            DuelMessage = duelMessage;

            if (activateAnomaly)
            {
                ActivateRandomAnomaly();
            }
        }

        private void ActivateRandomAnomaly()
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
                new SecondChanceAnomaly()
            };

            if (new SafeRandom().Next(100) < 3)
            {
                Log.Information("GlitchHorrorAnomaly activated");
                ActiveAnomaly = new GlitchHorrorAnomaly();
                ActiveAnomaly.ApplyEffect(this);
                return;
            }

            ActiveAnomaly = new SafeRandom().ChooseRandomElement(anomalies);
            ActiveAnomaly.ApplyEffect(this);
        }

        public (int Player1HP, int Player2HPб, string ActionString) ProcessTurn(DuelMember attacker, DuelMember defender)
        {
            attacker.IncrementTurnCounter();

            LastActionString = "";

            int damage = new SafeRandom().Next(10, 25);
            string actionString = "";

            OnDamageCalculated?.Invoke(attacker, defender, damage);

            if (!string.IsNullOrEmpty(LastActionString))
                actionString = $"{LastActionString}";
            else
                actionString = $"{attacker.Name} бьет вилкой и наносит {damage} урона! ⚔️";

            if (DuelMember1.HP <= 20 && DuelMember2.HP <= 20 && new SafeRandom().Next(100) < 8 && ActiveAnomaly is not GlitchHorrorAnomaly)
            {
                DuelMember1.HP = 0;
                DuelMember2.HP = 0;

                if (ActiveAnomaly is HarakiriAnomaly)
                {
                    actionString = $"Блядь, мы себя захуярили!";
                }
                else
                {
                    actionString = $"Вы оба умерли от кринжа!";
                }
            }

            LastActionString = "";

            OnTurnEnded?.Invoke(attacker, defender);

            if (!string.IsNullOrEmpty(LastActionString))
            {
                actionString += $"\n{LastActionString}";
            }

            return (DuelMember1.HP, DuelMember2.HP, actionString);
        }
    }
}
