namespace MCOP.Services.Duels.Anomalies
{
    public abstract class DuelAnomaly
    {
        public string Name { get; protected set; } = "";
        public string Description { get; protected set; } = "";

        public abstract void ApplyEffect(Duel duel);
    }
}