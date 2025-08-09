using System.ComponentModel;

namespace MCOP.Services.Duels.Anomalies.PokerAnomaly
{
    public enum Suit
    {
        [Description("♥")]
        Hearts,
        [Description("♦")]
        Diamonds,
        [Description("♣")]
        Clubs,
        [Description("♠")]
        Spades
    }

    public enum Rank
    {
        [Description("2")]
        Two = 2,
        [Description("3")]
        Three,
        [Description("4")]
        Four,
        [Description("5")]
        Five,
        [Description("6")]
        Six,
        [Description("7")]
        Seven,
        [Description("8")]
        Eight,
        [Description("9")]
        Nine,
        [Description("10")]
        Ten,
        [Description("J")]
        Jack,
        [Description("Q")]
        Queen,
        [Description("K")]
        King,
        [Description("A")]
        Ace
    }

    public sealed class Card
    {
        public Suit Suit { get; }
        public Rank Rank { get; }
        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }
}
