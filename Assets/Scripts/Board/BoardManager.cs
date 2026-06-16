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

        private readonly List<BoardCellData> boardCells = new List<BoardCellData>(TotalCellCount);

        public IReadOnlyList<BoardCellData> BoardCells
        {
            get { return boardCells; }
        }

        public void CreateBoard()
        {
            boardCells.Clear();

            System.Random random = new System.Random();

            for (int row = 0; row < BoardSize; row++)
            {
                for (int column = 0; column < BoardSize; column++)
                {
                    BoardCellType cellType = IsTemporaryAnchorJariPosition(row, column)
                        ? BoardCellType.AnchorJari
                        : BoardCellType.Normal;

                    CardData card = cellType == BoardCellType.Normal
                        ? CreateRandomCard(random)
                        : new CardData(Suit.Spade, Rank.Joker, false);

                    boardCells.Add(new BoardCellData(row, column, cellType, card));
                }
            }
        }

        private static bool IsTemporaryAnchorJariPosition(int row, int column)
        {
            bool isTopLeft = row == 0 && column == 0;
            bool isTopRight = row == 0 && column == BoardSize - 1;
            bool isBottomLeft = row == BoardSize - 1 && column == 0;
            bool isBottomRight = row == BoardSize - 1 && column == BoardSize - 1;

            return isTopLeft || isTopRight || isBottomLeft || isBottomRight;
        }

        private static CardData CreateRandomCard(System.Random random)
        {
            Suit suit = (Suit)random.Next(Enum.GetValues(typeof(Suit)).Length);
            Rank rank = (Rank)random.Next(0, 13);

            return new CardData(suit, rank, false);
        }
    }
}
