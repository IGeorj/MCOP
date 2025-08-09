using Humanizer;
using MCOP.Core.Common;

namespace MCOP.Services.Duels.Anomalies.PokerAnomaly
{
    public sealed class PokerAnomaly : DuelAnomaly
    {
        public List<Card> Deck { get; set; } = [];
        public List<Card> BoardCards { get; set; } = [];
        public Dictionary<DuelMember, List<Card>> PlayerHands { get; set; } = [];

        private readonly PokerHandEvaluator _evaluator = new();

        public PokerAnomaly()
        {
            Name = "Покерная дуэль";
            Description = "Игроки получают по 2 карты, на каждом ходу вскрываются 5 общих карт. Победитель раунда наносит урон!";
        }

        public override void ApplyEffect(Duel duel)
        {
            duel.DelayBetweenTurn = 2000;

            InitializeDeck();
            ShuffleDeck();

            PlayerHands[duel.DuelMember1] = [DrawCard(), DrawCard()];
            PlayerHands[duel.DuelMember2] = [DrawCard(), DrawCard()];

            duel.OnDamageCalculated += (attacker, defender, originalDamage) =>
            {
                if (BoardCards.Count == 0)
                    DrawBoardCards();

                var fullHand1 = PlayerHands[duel.DuelMember1].Concat(BoardCards).ToList();
                var fullHand2 = PlayerHands[duel.DuelMember2].Concat(BoardCards).ToList();

                var hand1Result = _evaluator.EvaluateHand(fullHand1);
                var hand2Result = _evaluator.EvaluateHand(fullHand2);

                int pokerDamage = new SafeRandom().Next(15, 30);
                string resultMessage;

                if (hand1Result.Rank > hand2Result.Rank)
                {
                    resultMessage = $"{duel.DuelMember2.Name} получает {pokerDamage} урона!";
                    duel.DuelMember2.ApplyDamage(pokerDamage);
                }
                else if (hand2Result.Rank > hand1Result.Rank)
                {
                    resultMessage = $"{duel.DuelMember1.Name} получает {pokerDamage} урона!";
                    duel.DuelMember1.ApplyDamage(pokerDamage);
                }
                else
                {
                    var comparison = CompareHands(hand1Result, hand2Result);
                    if (comparison > 0)
                    {
                        resultMessage = $"{duel.DuelMember2.Name} проигрывает по старшим картам и получает {pokerDamage} урона!";
                        duel.DuelMember2.ApplyDamage(pokerDamage);
                    }
                    else if (comparison < 0)
                    {
                        resultMessage = $"{duel.DuelMember1.Name} проигрывает по старшим картам и получает {pokerDamage} урона!";
                        duel.DuelMember1.ApplyDamage(pokerDamage);
                    }
                    else
                    {
                        resultMessage = $"Абсолютная ничья! ({hand1Result.Rank.Humanize()}) Никто не получает урона.";
                    }
                }

                duel.LastActionString = $"{GetCardsDescription(duel.DuelMember1, duel.DuelMember2)}\n\n" +
                                       $"{duel.DuelMember1.Name}: {FormatHandResult(hand1Result)}\n" +
                                       $"{duel.DuelMember2.Name}: {FormatHandResult(hand2Result)}\n\n" +
                                       $"{resultMessage}";

                PrepareNextRound();
            };
        }

        private string FormatHandResult(PokerHandEvaluator.EvaluationResult result)
        {
            var cardsStr = string.Join(" ", result.CardsInCombination
                .OrderByDescending(c => c.Rank)
                .Select(c => $"{c.Rank.Humanize()}{c.Suit.Humanize()}"));

            return $"{result.Rank.Humanize()} ({cardsStr})";
        }

        private int CompareHands(PokerHandEvaluator.EvaluationResult hand1, PokerHandEvaluator.EvaluationResult hand2)
        {
            if (hand1.Rank != hand2.Rank)
                return hand1.Rank.CompareTo(hand2.Rank);

            var sorted1 = hand1.CardsInCombination
                .OrderByDescending(c => c.Rank)
                .ThenByDescending(c => c.Suit)
                .ToList();

            var sorted2 = hand2.CardsInCombination
                .OrderByDescending(c => c.Rank)
                .ThenByDescending(c => c.Suit)
                .ToList();

            for (int i = 0; i < Math.Min(sorted1.Count, sorted2.Count); i++)
            {
                int rankComparison = sorted1[i].Rank.CompareTo(sorted2[i].Rank);
                if (rankComparison != 0)
                    return rankComparison;

                int suitComparison = sorted1[i].Suit.CompareTo(sorted2[i].Suit);
                if (suitComparison != 0)
                    return suitComparison;
            }

            return 0;
        }

        private string GetCardsDescription(DuelMember player1, DuelMember player2)
        {
            string GetCardString(Card card) => $"{card.Rank.Humanize()}{card.Suit.Humanize()}";

            return $"{player1.Name}: {string.Join(" ", PlayerHands[player1].Select(GetCardString))}\n" +
                   $"{player2.Name}: {string.Join(" ", PlayerHands[player2].Select(GetCardString))}\n" +
                   $"Общие карты: {string.Join(" ", BoardCards.Select(GetCardString))}";
        }

        private void PrepareNextRound()
        {
            BoardCards.Clear();
            DrawBoardCards();

            if (Deck.Count < 10)
            {
                InitializeDeck();
                ShuffleDeck();
            }
        }

        private void DrawBoardCards()
        {
            for (int i = 0; i < 5; i++)
            {
                BoardCards.Add(DrawCard());
            }
        }

        private void InitializeDeck()
        {
            Deck = [];
            foreach (Suit suit in Enum.GetValues<Suit>())
            {
                foreach (Rank rank in Enum.GetValues<Rank>())
                {
                    Deck.Add(new Card(suit, rank));
                }
            }
        }

        private void ShuffleDeck()
        {
            Deck = Deck.OrderBy(c => new SafeRandom().Next()).ToList();
        }

        private Card DrawCard()
        {
            if (Deck.Count == 0)
            {
                InitializeDeck();
                ShuffleDeck();
            }

            var card = Deck[0];
            Deck.RemoveAt(0);
            return card;
        }
    }
}