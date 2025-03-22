using MCOP.Core.Common;
using System;

namespace MCOP.Services.Duels.Anomalies
{
    public class SecondChanceAnomaly : DuelAnomaly
    {
        private const double SecondChance = 50;

        public SecondChanceAnomaly()
        {
            Name = "Временной откат";
            Description = $"Каждый игрок может 1 раз откатить бой при смерти с шансом {SecondChance}%";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                if (defender.HP - damage <= 0 && defender.ResetCounter == 0 && new SafeRandom().Next(101) < SecondChance)
                {
                    duel.LastActionString = $"⚡ {defender.Name} успел прожать откат перед летальным уроном! ⏳";

                    defender.HP = DuelMember.InitialHP;
                    attacker.HP = DuelMember.InitialHP;

                    return;
                }

                defender.ApplyDamage(damage);
            };

        }
    }
}