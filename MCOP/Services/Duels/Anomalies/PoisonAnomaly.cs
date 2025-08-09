using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public sealed class PoisonAnomaly : DuelAnomaly
    {
        const int MaxPoisonDamage = 10;
        public PoisonAnomaly()
        {
            Name = "Лужа говна";
            Description = $"Каждые 3 хода оба игрока получают до {MaxPoisonDamage} урона от яда!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                defender.ApplyDamage(damage);
            };

            duel.OnTurnEnded += (attacker, defender) =>
            {
                if (attacker.TurnCounter % 3 == 0 && defender.TurnCounter % 3 == 0)
                {
                    int poisonDamagePlayer1 = new SafeRandom().Next(MaxPoisonDamage);
                    int poisonDamagePlayer2 = new SafeRandom().Next(MaxPoisonDamage);

                    attacker.ApplyDamage(poisonDamagePlayer1);
                    defender.ApplyDamage(poisonDamagePlayer2);

                    duel.LastActionString = $"☠️ Оба игрока отравлены! {attacker.Name} теряет {poisonDamagePlayer1} здоровья, {duel.DuelMember2.Name} теряет {poisonDamagePlayer2} здоровья!";
                }
            };
        }
    }
}
