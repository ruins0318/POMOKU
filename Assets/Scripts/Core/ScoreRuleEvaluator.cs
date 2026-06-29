using System.Collections.Generic;
using Pomoku.Board;
using Pomoku.Cards;

namespace Pomoku.Core
{
    public static class ScoreRuleEvaluator
    {
        private const int NormalScore = 1;
        private const int TwoPairScore = 2;
        private const int TripleScore = 3;
        private const int FlushScore = 4;
        private const int StraightScore = 5;
        private const int AnchorJariScore = 1;

        public static PomokuScoreResult EvaluateCompletedLine(CompletedPomokuLine completedPomokuLine, IReadOnlyList<BoardCellData> boardCells)
        {
            List<CardData> cards = new List<CardData>();
            bool containsAnchorJari = completedPomokuLine.ContainsAnchorJari;
            string cardsDescription = BuildCardsDescription(completedPomokuLine.CellIndices, boardCells, cards, ref containsAnchorJari);

            if (containsAnchorJari)
            {
                return new PomokuScoreResult(PomokuHandType.AnchorJari, AnchorJariScore, true, "AnchorJari Pomoku", cardsDescription);
            }

            if (cards.Count != 5)
            {
                return new PomokuScoreResult(PomokuHandType.Normal, NormalScore, false, "Normal Pomoku", cardsDescription);
            }

            if (IsStraight(cards))
            {
                return new PomokuScoreResult(PomokuHandType.Straight, StraightScore, false, "Straight Pomoku", cardsDescription);
            }

            if (IsFlush(cards))
            {
                return new PomokuScoreResult(PomokuHandType.Flush, FlushScore, false, "Flush Pomoku", cardsDescription);
            }

            if (IsTriple(cards))
            {
                return new PomokuScoreResult(PomokuHandType.Triple, TripleScore, false, "Triple Pomoku", cardsDescription);
            }

            if (IsTwoPair(cards))
            {
                return new PomokuScoreResult(PomokuHandType.TwoPair, TwoPairScore, false, "TwoPair Pomoku", cardsDescription);
            }

            return new PomokuScoreResult(PomokuHandType.Normal, NormalScore, false, "Normal Pomoku", cardsDescription);
        }

        private static string BuildCardsDescription(IReadOnlyList<int> cellIndices, IReadOnlyList<BoardCellData> boardCells, List<CardData> cards, ref bool containsAnchorJari)
        {
            List<string> cardNames = new List<string>();

            if (cellIndices == null || boardCells == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < cellIndices.Count; i++)
            {
                int cellIndex = cellIndices[i];

                if (cellIndex < 0 || cellIndex >= boardCells.Count || boardCells[cellIndex] == null)
                {
                    cardNames.Add("Invalid");
                    continue;
                }

                BoardCellData cellData = boardCells[cellIndex];

                if (cellData.CellType == BoardCellType.AnchorJari)
                {
                    containsAnchorJari = true;
                    cardNames.Add("AnchorJari");
                    continue;
                }

                cards.Add(cellData.Card);
                cardNames.Add(cellData.Card.GetDisplayName());
            }

            return string.Join(", ", cardNames);
        }

        private static bool IsStraight(IReadOnlyList<CardData> cards)
        {
            List<int> rankValues = new List<int>();

            for (int i = 0; i < cards.Count; i++)
            {
                int rankValue = GetMvpStraightRankValue(cards[i].Rank);

                if (rankValue < 0 || rankValues.Contains(rankValue))
                {
                    return false;
                }

                rankValues.Add(rankValue);
            }

            rankValues.Sort();

            for (int i = 1; i < rankValues.Count; i++)
            {
                if (rankValues[i] != rankValues[i - 1] + 1)
                {
                    return false;
                }
            }

            return rankValues.Count == 5;
        }

        private static bool IsFlush(IReadOnlyList<CardData> cards)
        {
            Suit firstSuit = cards[0].Suit;

            for (int i = 1; i < cards.Count; i++)
            {
                if (cards[i].Suit != firstSuit)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsTriple(IReadOnlyList<CardData> cards)
        {
            Dictionary<Rank, int> rankCounts = CountRanks(cards);

            foreach (KeyValuePair<Rank, int> rankCount in rankCounts)
            {
                if (rankCount.Value >= 3)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTwoPair(IReadOnlyList<CardData> cards)
        {
            Dictionary<Rank, int> rankCounts = CountRanks(cards);
            int pairCount = 0;

            foreach (KeyValuePair<Rank, int> rankCount in rankCounts)
            {
                if (rankCount.Value >= 2)
                {
                    pairCount++;
                }
            }

            return pairCount >= 2;
        }

        private static Dictionary<Rank, int> CountRanks(IReadOnlyList<CardData> cards)
        {
            Dictionary<Rank, int> rankCounts = new Dictionary<Rank, int>();

            for (int i = 0; i < cards.Count; i++)
            {
                Rank rank = cards[i].Rank;

                if (!rankCounts.ContainsKey(rank))
                {
                    rankCounts.Add(rank, 0);
                }

                rankCounts[rank]++;
            }

            return rankCounts;
        }

        private static int GetMvpStraightRankValue(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:
                    return 0;
                case Rank.Two:
                    return 1;
                case Rank.Three:
                    return 2;
                case Rank.Four:
                    return 3;
                case Rank.Five:
                    return 4;
                case Rank.Six:
                    return 5;
                case Rank.Seven:
                    return 6;
                case Rank.Eight:
                    return 7;
                case Rank.Nine:
                    return 8;
                case Rank.Ten:
                    return 9;
                case Rank.Queen:
                    return 10;
                case Rank.King:
                    return 11;
                default:
                    return -1;
            }
        }
    }
}
