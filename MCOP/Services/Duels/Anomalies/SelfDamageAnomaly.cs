using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public class SelfDamageAnomaly : DuelAnomaly
    {
        private const int SelfDamageChance = 25;
        public SelfDamageAnomaly()
        {
            Name = "Пьяная вилка";
            Description = $"С шансом {SelfDamageChance}% атака может попасть не туда!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                bool isSelfDamaged = new SafeRandom().Next(101) < SelfDamageChance;

                if (isSelfDamaged)
                {
                    int selfDamage = new SafeRandom().Next(5, 25);
                    
                    duel.LastActionString = $"{attacker.Name} бьет вилкой, но попадает себе в палец нанося {selfDamage} урона! ⚔️";
                    attacker.ApplyDamage(selfDamage);
                    return;
                }

                defender.ApplyDamage(damage);
            };
        }
    }
}
