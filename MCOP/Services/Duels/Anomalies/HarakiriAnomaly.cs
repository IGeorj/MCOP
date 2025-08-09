using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies
{
    public sealed class HarakiriAnomaly : DuelAnomaly
    {
        private const int HarakiriChance = 10;
        public HarakiriAnomaly()
        {
            Name = "Харакурим";
            Description = $"С шансом {HarakiriChance}% атакующий убивает сам себя!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                if (new SafeRandom().Next(101) < HarakiriChance)
                {
                    attacker.ApplyDamage(attacker.HP);

                    duel.LastActionString = $"💀 {attacker.Name}: блядь, я себя захуярил...";
                    return;
                }

                defender.ApplyDamage(attacker.HP);
            };
        }
    }
}
