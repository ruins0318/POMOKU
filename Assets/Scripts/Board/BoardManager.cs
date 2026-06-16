using System;
using System.Collections.Generic;
using Pomoku.Cards;
using UnityEngine;

namespace Pomoku.Board
{
    public class BoardManager : MonoBehaviour
    {
        public const int BoardSize = 10;
        public const int TotalCellCount = BoardSize * BoardSize;
        private const int AnchorJariCellCount = 4;
        private const int NormalCardCellCount = TotalCellCount - AnchorJariCellCount;
        private const int ExpectedCardsPerSuit = 24;

        private readonly List<BoardCellData> boardCells = new List<BoardCellData>(TotalCellCount);

        public IReadOnlyList<BoardCellData> BoardCells
        {
            get { return boardCells; }
        }

        public void CreateBoard()
        {
            boardCells.Clear();

            System.Random random = new System.Random();
            List<CardData> boardCardPool = MvpCardRules.CreateMvpRegularCardPool(MvpCardRules.CopiesPerRegularCard);
            ShuffleCards(boardCardPool, random);

            for (int row = 0; row < BoardSize; row++)
            {
                for (int column = 0; column < BoardSize; column++)
                {
                    BoardCellType cellType = IsTemporaryAnchorJariPosition(row, column)
                        ? BoardCellType.AnchorJari
                        : BoardCellType.Normal;

                    CardData card = cellType == BoardCellType.Normal
                        ? DrawNextBoardCard(boardCardPool)
                        : new CardData(Suit.Spade, Rank.Ace, false);

                    boardCells.Add(new BoardCellData(row, column, cellType, card));
                }
            }

            ValidateBoard();
        }

        private static bool IsTemporaryAnchorJariPosition(int row, int column)
        {
            bool isTopLeft = row == 0 && column == 0;
            bool isTopRight = row == 0 && column == BoardSize - 1;
            bool isBottomLeft = row == BoardSize - 1 && column == 0;
            bool isBottomRight = row == BoardSize - 1 && column == BoardSize - 1;

            return isTopLeft || isTopRight || isBottomLeft || isBottomRight;
        }

        private static CardData DrawNextBoardCard(List<CardData> boardCardPool)
        {
            int lastIndex = boardCardPool.Count - 1;
            CardData card = boardCardPool[lastIndex];
            boardCardPool.RemoveAt(lastIndex);
            return card;
        }

        private static void ShuffleCards(List<CardData> cards, System.Random random)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                CardData temporaryCard = cards[i];
                cards[i] = cards[randomIndex];
                cards[randomIndex] = temporaryCard;
            }
        }

        private void ValidateBoard()
        {
            int normalCellCount = 0;
            int anchorJariCount = 0;
            Dictionary<string, int> cardCounts = new Dictionary<string, int>();
            Dictionary<Suit, int> suitCounts = CreateEmptySuitCounts();

            for (int i = 0; i < boardCells.Count; i++)
            {
                BoardCellData cellData = boardCells[i];

                if (cellData.CellType == BoardCellType.AnchorJari)
                {
                    anchorJariCount++;
                    continue;
                }

                normalCellCount++;

                if (!MvpCardRules.IsMvpRegularCard(cellData.Card))
                {
                    Debug.LogError("Board validation failed: invalid board card " + cellData.Card.GetDisplayName());
                    continue;
                }

                string cardKey = MvpCardRules.GetCardKey(cellData.Card);

                if (!cardCounts.ContainsKey(cardKey))
                {
                    cardCounts.Add(cardKey, 0);
                }

                cardCounts[cardKey]++;
                suitCounts[cellData.Card.Suit]++;
            }

            bool isValid = true;
            isValid &= LogExpectedCount("normal card cells", normalCellCount, NormalCardCellCount);
            isValid &= LogExpectedCount("AnchorJari cells", anchorJariCount, AnchorJariCellCount);
            isValid &= LogExpectedCount("regular card types", cardCounts.Count, MvpCardRules.RegularCardTypeCount);

            bool allCardCopiesAreValid = true;

            foreach (KeyValuePair<string, int> cardCount in cardCounts)
            {
                if (cardCount.Value != MvpCardRules.CopiesPerRegularCard)
                {
                    Debug.LogError("Board validation failed: " + cardCount.Key + " count is " + cardCount.Value + ", expected " + MvpCardRules.CopiesPerRegularCard);
                    allCardCopiesAreValid = false;
                }
            }

            if (allCardCopiesAreValid)
            {
                Debug.Log("Board validation: every regular card type has exactly " + MvpCardRules.CopiesPerRegularCard + " copies.");
            }

            bool allSuitCountsAreValid = true;

            foreach (KeyValuePair<Suit, int> suitCount in suitCounts)
            {
                if (suitCount.Value != ExpectedCardsPerSuit)
                {
                    Debug.LogError("Board validation failed: " + suitCount.Key + " suit count is " + suitCount.Value + ", expected " + ExpectedCardsPerSuit);
                    allSuitCountsAreValid = false;
                }
                else
                {
                    Debug.Log("Board validation: " + suitCount.Key + " suit count = " + suitCount.Value);
                }
            }

            isValid &= allCardCopiesAreValid;
            isValid &= allSuitCountsAreValid;

            if (isValid)
            {
                Debug.Log("Board validation passed: 96 normal card cells, 4 AnchorJari cells, 48 card types x 2 copies, 24 cards per suit.");
            }
        }

        private static Dictionary<Suit, int> CreateEmptySuitCounts()
        {
            Dictionary<Suit, int> suitCounts = new Dictionary<Suit, int>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                suitCounts.Add(suit, 0);
            }

            return suitCounts;
        }

        private static bool LogExpectedCount(string label, int actualCount, int expectedCount)
        {
            if (actualCount == expectedCount)
            {
                Debug.Log("Board validation: " + label + " = " + actualCount);
                return true;
            }

            Debug.LogError("Board validation failed: " + label + " = " + actualCount + ", expected " + expectedCount);
            return false;
        }
    }
}
