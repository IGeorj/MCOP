using System.ComponentModel;

namespace MCOP.Services.Duels.Anomalies.PokerAnomaly
{
    public class PokerHandEvaluator
    {
        public enum HandRank
        {
            [Description("Старшая карта")]
            HighCard,
            [Description("Пара")]
            OnePair,
            [Description("Две пары")]
            TwoPair,
            [Description("Cет")]
            ThreeOfAKind,
            [Description("Стрит")]
            Straight,
            [Description("Флэш")]
            Flush,
            [Description("Фулл-хаус")]
            FullHouse,
            [Description("Каре")]
            FourOfAKind,
            [Description("Стрит-флэш")]
            StraightFlush,
            [Description("Роял-флэш")]
            RoyalFlush
        }

        public class EvaluationResult
        {
            public HandRank Rank { get; set; }
            public List<Card> CardsInCombination { get; set; } = new List<Card>();
        }

        public EvaluationResult EvaluateHand(List<Card> hand)
        {
            var result = new EvaluationResult();

            if (IsRoyalFlush(hand, out var royalFlushCards))
            {
                result.Rank = HandRank.RoyalFlush;
                result.CardsInCombination = royalFlushCards;
            }
            else if (IsStraightFlush(hand, out var straightFlushCards))
            {
                result.Rank = HandRank.StraightFlush;
                result.CardsInCombination = straightFlushCards;
            }
            else if (IsFourOfAKind(hand, out var fourOfAKindCards))
            {
                result.Rank = HandRank.FourOfAKind;
                result.CardsInCombination = fourOfAKindCards;
            }
            else if (IsFullHouse(hand, out var fullHouseCards))
            {
                result.Rank = HandRank.FullHouse;
                result.CardsInCombination = fullHouseCards;
            }
            else if (IsFlush(hand, out var flushCards))
            {
                result.Rank = HandRank.Flush;
                result.CardsInCombination = flushCards;
            }
            else if (IsStraight(hand, out var straightCards))
            {
                result.Rank = HandRank.Straight;
                result.CardsInCombination = straightCards;
            }
            else if (IsThreeOfAKind(hand, out var threeOfAKindCards))
            {
                result.Rank = HandRank.ThreeOfAKind;
                result.CardsInCombination = threeOfAKindCards;
            }
            else if (IsTwoPair(hand, out var twoPairCards))
            {
                result.Rank = HandRank.TwoPair;
                result.CardsInCombination = twoPairCards;
            }
            else if (IsOnePair(hand, out var onePairCards))
            {
                result.Rank = HandRank.OnePair;
                result.CardsInCombination = onePairCards;
            }
            else
            {
                result.Rank = HandRank.HighCard;
                result.CardsInCombination = hand.OrderByDescending(c => c.Rank).Take(1).ToList();
            }

            return result;
        }

        private bool IsRoyalFlush(List<Card> hand, out List<Card> cards)
        {
            if (IsStraightFlush(hand, out cards) && hand.All(card => card.Rank >= Rank.Ten))
            {
                cards = hand.OrderBy(c => c.Rank).ToList();
                return true;
            }
            return false;
        }

        private bool IsStraightFlush(List<Card> hand, out List<Card> cards)
        {
            if (IsFlush(hand, out var flushCards) && IsStraight(hand, out var straightCards))
            {
                cards = straightCards;
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsFourOfAKind(List<Card> hand, out List<Card> cards)
        {
            var rankGroups = hand.GroupBy(card => card.Rank);
            var fourOfAKind = rankGroups.FirstOrDefault(group => group.Count() == 4);
            if (fourOfAKind != null)
            {
                cards = hand.Where(c => c.Rank == fourOfAKind.Key).ToList();
                cards.Add(hand.Where(c => c.Rank != fourOfAKind.Key).OrderByDescending(c => c.Rank).First());
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsFullHouse(List<Card> hand, out List<Card> cards)
        {
            var rankGroups = hand.GroupBy(card => card.Rank).ToList();
            var threeOfAKind = rankGroups.FirstOrDefault(group => group.Count() == 3);
            var pair = rankGroups.FirstOrDefault(group => group.Count() == 2);

            if (threeOfAKind != null && pair != null)
            {
                cards = hand.Where(c => c.Rank == threeOfAKind.Key || c.Rank == pair.Key).ToList();
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsFlush(List<Card> hand, out List<Card> cards)
        {
            var suitGroups = hand.GroupBy(card => card.Suit);
            var flushGroup = suitGroups.FirstOrDefault(group => group.Count() >= 5);
            if (flushGroup != null)
            {
                cards = flushGroup.OrderByDescending(c => c.Rank).Take(5).ToList();
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsStraight(List<Card> hand, out List<Card> cards)
        {
            var distinctRanks = hand.Select(card => (int)card.Rank).Distinct().OrderBy(rank => rank).ToList();

            if (distinctRanks.Contains((int)Rank.Ace) && distinctRanks.Contains((int)Rank.Two) &&
                distinctRanks.Contains((int)Rank.Three) && distinctRanks.Contains((int)Rank.Four) &&
                distinctRanks.Contains((int)Rank.Five))
            {
                cards = hand.Where(c => (int)c.Rank <= 5 || c.Rank == Rank.Ace)
                             .GroupBy(c => c.Rank)
                             .Select(g => g.OrderByDescending(c => c.Suit).First())
                             .OrderBy(c => c.Rank == Rank.Ace ? 0 : (int)c.Rank)
                             .ToList();
                return true;
            }

            for (int i = 0; i <= distinctRanks.Count - 5; i++)
            {
                if (distinctRanks[i + 4] == distinctRanks[i] + 4)
                {
                    var straightRanks = distinctRanks.Skip(i).Take(5);
                    cards = hand.Where(c => straightRanks.Contains((int)c.Rank))
                               .GroupBy(c => c.Rank)
                               .Select(g => g.OrderByDescending(c => c.Suit).First())
                               .OrderBy(c => c.Rank)
                               .ToList();
                    return true;
                }
            }

            cards = null;
            return false;
        }

        private bool IsThreeOfAKind(List<Card> hand, out List<Card> cards)
        {
            var rankGroups = hand.GroupBy(card => card.Rank);
            var threeOfAKind = rankGroups.FirstOrDefault(group => group.Count() == 3);
            if (threeOfAKind != null)
            {
                cards = hand.Where(c => c.Rank == threeOfAKind.Key).ToList();
                cards.AddRange(hand.Where(c => c.Rank != threeOfAKind.Key)
                                  .OrderByDescending(c => c.Rank)
                                  .Take(2));
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsTwoPair(List<Card> hand, out List<Card> cards)
        {
            var rankGroups = hand.GroupBy(card => card.Rank).Where(g => g.Count() == 2).OrderByDescending(g => g.Key).ToList();
            if (rankGroups.Count >= 2)
            {
                cards = hand.Where(c => c.Rank == rankGroups[0].Key || c.Rank == rankGroups[1].Key).ToList();
                cards.Add(hand.Where(c => c.Rank != rankGroups[0].Key && c.Rank != rankGroups[1].Key)
                             .OrderByDescending(c => c.Rank)
                             .First());
                return true;
            }
            cards = null;
            return false;
        }

        private bool IsOnePair(List<Card> hand, out List<Card> cards)
        {
            var rankGroups = hand.GroupBy(card => card.Rank);
            var pair = rankGroups.FirstOrDefault(group => group.Count() == 2);
            if (pair != null)
            {
                cards = hand.Where(c => c.Rank == pair.Key).ToList();
                cards.AddRange(hand.Where(c => c.Rank != pair.Key)
                                  .OrderByDescending(c => c.Rank)
                                  .Take(3));
                return true;
            }
            cards = null;
            return false;
        }
    }
}