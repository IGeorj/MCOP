namespace MCOP.Services.Duels.Anomalies
{
    public class DoubleDamageAnomaly : DuelAnomaly
    {
        public DoubleDamageAnomaly()
        {
            Name = "Да, это жестко";
            Description = "Каждый второй удар наносит двойной урон!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.OnDamageCalculated += (attacker, defender, damage) =>
            {
                if (attacker.TurnCounter % 2 == 0)
                {
                    duel.LastActionString = $"⚡ {defender.Name} жестко лупанул на {damage * 2} урона!";
                    defender.ApplyDamage(damage * 2);
                }

                defender.ApplyDamage(damage);
            };
        }
    }
}
