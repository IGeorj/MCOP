using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public class CounterAttackAnomaly : DuelAnomaly
    {
        private const int CounterAttackChance = 20;
        public CounterAttackAnomaly()
        {
            Name = "Контра";
            Description = $"Вы можете контратаковать с шансом {CounterAttackChance}%!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                bool isCountered = new SafeRandom().Next(101) < CounterAttackChance;

                if (isCountered)
                {
                    int counterDamage = new SafeRandom().Next(0, 25);
                    
                    duel.LastActionString = $"{attacker.Name} бьет вилкой и наносит {damage} урона, но {defender.Name} контратакует на {counterDamage} урона! ⚔️";
                    attacker.ApplyDamage(counterDamage);
                }

                defender.ApplyDamage(damage);
            };
        }
    }
}
