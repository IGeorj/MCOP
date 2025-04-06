using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public class DodgeAnomaly : DuelAnomaly
    {
        private const int DodgeChance = 25;
        public DodgeAnomaly()
        {
            Name = "Бабочки в животе";
            Description = $"Вы можете уклониться от удара с шансом {DodgeChance}%!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                bool isDodged = new SafeRandom().Next(101) < DodgeChance;

                if (isDodged)
                {
                    duel.LastActionString = $"{attacker.Name} бьет вилкой, но {defender.Name} уворачивается! 🛡️";
                    return;
                }

                defender.ApplyDamage(damage);
            };
        }
    }
}
