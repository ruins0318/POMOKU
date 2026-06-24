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
            CreateBoard(BoardGenerationMode.RandomMvp);
        }

        public void CreateBoard(BoardGenerationMode boardGenerationMode)
        {
            boardCells.Clear();

            if (boardGenerationMode == BoardGenerationMode.TestScoringPreset)
            {
                CreateTestScoringPresetBoard();
                ValidateBoard();
                LogTestScoringPresetLines();
                return;
            }

            CreateRandomMvpBoard();
            ValidateBoard();
        }

        private void CreateRandomMvpBoard()
        {
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
        }

        private void CreateTestScoringPresetBoard()
        {
            List<CardData> boardCardPool = MvpCardRules.CreateMvpRegularCardPool(MvpCardRules.CopiesPerRegularCard);
            Dictionary<int, CardData> fixedCardsByCellIndex = CreateTestScoringFixedCards(boardCardPool);

            for (int row = 0; row < BoardSize; row++)
            {
                for (int column = 0; column < BoardSize; column++)
                {
                    int cellIndex = GetCellIndex(row, column);
                    BoardCellType cellType = IsTemporaryAnchorJariPosition(row, column)
                        ? BoardCellType.AnchorJari
                        : BoardCellType.Normal;

                    CardData card = new CardData(Suit.Spade, Rank.Ace, false);

                    if (cellType == BoardCellType.Normal)
                    {
                        if (fixedCardsByCellIndex.ContainsKey(cellIndex))
                        {
                            card = fixedCardsByCellIndex[cellIndex];
                        }
                        else
                        {
                            card = DrawNextBoardCard(boardCardPool);
                        }
                    }

                    boardCells.Add(new BoardCellData(row, column, cellType, card));
                }
            }
        }

        private static Dictionary<int, CardData> CreateTestScoringFixedCards(List<CardData> boardCardPool)
        {
            Dictionary<int, CardData> fixedCardsByCellIndex = new Dictionary<int, CardData>();

            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 1, Suit.Club, Rank.Five);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 2, Suit.Club, Rank.Six);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 3, Suit.Club, Rank.Seven);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 4, Suit.Club, Rank.Eight);

            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 10, Suit.Spade, Rank.Seven);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 11, Suit.Spade, Rank.Eight);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 12, Suit.Spade, Rank.Nine);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 13, Suit.Spade, Rank.Ten);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 14, Suit.Spade, Rank.Queen);

            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 20, Suit.Heart, Rank.Two);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 21, Suit.Heart, Rank.Five);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 22, Suit.Heart, Rank.Eight);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 23, Suit.Heart, Rank.Ten);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 24, Suit.Heart, Rank.Ace);

            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 30, Suit.Club, Rank.Three);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 31, Suit.Diamond, Rank.Three);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 32, Suit.Spade, Rank.Three);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 33, Suit.Heart, Rank.Seven);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 34, Suit.Diamond, Rank.Nine);

            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 40, Suit.Club, Rank.Four);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 41, Suit.Diamond, Rank.Four);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 42, Suit.Spade, Rank.Eight);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 43, Suit.Heart, Rank.Eight);
            AddFixedCard(fixedCardsByCellIndex, boardCardPool, 44, Suit.Club, Rank.Ace);

            return fixedCardsByCellIndex;
        }

        private static void AddFixedCard(Dictionary<int, CardData> fixedCardsByCellIndex, List<CardData> boardCardPool, int cellIndex, Suit suit, Rank rank)
        {
            CardData card = new CardData(suit, rank, false);

            if (!RemoveFirstMatchingCard(boardCardPool, card))
            {
                Debug.LogError("Test scoring board setup failed. Missing card: " + card.GetDisplayName());
                return;
            }

            fixedCardsByCellIndex.Add(cellIndex, card);
        }

        private static bool RemoveFirstMatchingCard(List<CardData> cards, CardData cardToRemove)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CardData card = cards[i];

                if (card.Suit == cardToRemove.Suit && card.Rank == cardToRemove.Rank)
                {
                    cards.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private static int GetCellIndex(int row, int column)
        {
            return row * BoardSize + column;
        }

        private static void LogTestScoringPresetLines()
        {
            Debug.Log("TestScoringPreset line - AnchorJari: 0, 1, 2, 3, 4");
            Debug.Log("TestScoringPreset line - Straight: 10, 11, 12, 13, 14");
            Debug.Log("TestScoringPreset line - Flush: 20, 21, 22, 23, 24");
            Debug.Log("TestScoringPreset line - Triple: 30, 31, 32, 33, 34");
            Debug.Log("TestScoringPreset line - TwoPair: 40, 41, 42, 43, 44");
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
