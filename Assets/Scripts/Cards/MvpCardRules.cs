using System;
using System.Collections.Generic;

namespace Pomoku.Cards
{
    public static class MvpCardRules
    {
        public const int RegularCardTypeCount = 48;
        public const int CopiesPerRegularCard = 2;

        private static readonly Rank[] MvpRegularRanks =
        {
            Rank.Ace,
            Rank.Two,
            Rank.Three,
            Rank.Four,
            Rank.Five,
            Rank.Six,
            Rank.Seven,
            Rank.Eight,
            Rank.Nine,
            Rank.Ten,
            Rank.Queen,
            Rank.King
        };

        public static List<CardData> CreateMvpRegularCardPool(int copiesPerCard)
        {
            List<CardData> cardPool = new List<CardData>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                for (int rankIndex = 0; rankIndex < MvpRegularRanks.Length; rankIndex++)
                {
                    for (int copyIndex = 0; copyIndex < copiesPerCard; copyIndex++)
                    {
                        cardPool.Add(new CardData(suit, MvpRegularRanks[rankIndex], false));
                    }
                }
            }

            return cardPool;
        }

        public static bool IsMvpRegularCard(CardData card)
        {
            if (card.IsJoker)
            {
                return false;
            }

            for (int i = 0; i < MvpRegularRanks.Length; i++)
            {
                if (card.Rank == MvpRegularRanks[i])
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetCardKey(CardData card)
        {
            return card.Suit + "_" + card.Rank;
        }
    }
}
