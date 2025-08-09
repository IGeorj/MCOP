namespace MCOP.Services.Duels.Anomalies
{
    public sealed class InstantWinAnomaly : DuelAnomaly
    {
        public InstantWinAnomaly()
        {
            Name = "СОЛЯНОГО";
            Description = "Первый удар гарантированно побеждает!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                if (attacker.TurnCounter > 0)
                {
                    duel.LastActionString = $"💥 {attacker.Name} с криком НЫЫЫЫЫЫА набрасывается на соперника и вырубает его!";
                    defender.ApplyDamage(defender.HP);
                }
            };
        }
    }
}
