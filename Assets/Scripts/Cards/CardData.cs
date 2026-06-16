using System;

namespace Pomoku.Cards
{
    [Serializable]
    public struct CardData
    {
        public Suit Suit;
        public Rank Rank;
        public bool IsJoker;

        public CardData(Suit suit, Rank rank, bool isJoker = false)
        {
            Suit = suit;
            Rank = rank;
            IsJoker = isJoker;
        }

        public static CardData CreateJoker()
        {
            return new CardData(Suit.Spade, Rank.Joker, true);
        }

        public string GetDisplayName()
        {
            if (IsJoker)
            {
                return "Joker";
            }

            return GetSuitText(Suit) + GetRankText(Rank);
        }

        private static string GetSuitText(Suit suit)
        {
            switch (suit)
            {
                case Suit.Spade:
                    return "S";
                case Suit.Heart:
                    return "H";
                case Suit.Diamond:
                    return "D";
                case Suit.Club:
                    return "C";
                default:
                    return "?";
            }
        }

        private static string GetRankText(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return "A";
                case Rank.Two:
                    return "2";
                case Rank.Three:
                    return "3";
                case Rank.Four:
                    return "4";
                case Rank.Five:
                    return "5";
                case Rank.Six:
                    return "6";
                case Rank.Seven:
                    return "7";
                case Rank.Eight:
                    return "8";
                case Rank.Nine:
                    return "9";
                case Rank.Ten:
                    return "10";
                case Rank.Jack:
                    return "J";
                case Rank.Queen:
                    return "Q";
                case Rank.King:
                    return "K";
                case Rank.Joker:
                    return "Joker";
                default:
                    return "?";
            }
        }
    }
}
