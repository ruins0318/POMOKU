using System;
using Pomoku.Cards;

namespace Pomoku.Board
{
    [Serializable]
    public class BoardCellData
    {
        public int Row;
        public int Column;
        public BoardCellType CellType;
        public CardData Card;
        public TeamId ChipOwnerTeam;
        public bool IsLocked;

        public BoardCellData(int row, int column, BoardCellType cellType, CardData card)
        {
            Row = row;
            Column = column;
            CellType = cellType;
            Card = card;
            ChipOwnerTeam = TeamId.None;
            IsLocked = false;
        }

        public string GetDisplayName()
        {
            if (CellType == BoardCellType.AnchorJari)
            {
                return "AnchorJari";
            }

            return Card.GetDisplayName();
        }
    }
}
