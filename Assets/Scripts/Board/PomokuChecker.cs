using System.Collections.Generic;

namespace Pomoku.Board
{
    public struct PomokuLineResult
    {
        public bool IsCompleted;
        public List<int> CellIndices;
        public string DirectionName;
        public TeamId TeamId;
        public bool ContainsAnchorJari;

        public PomokuLineResult(bool isCompleted, List<int> cellIndices, string directionName, TeamId teamId, bool containsAnchorJari)
        {
            IsCompleted = isCompleted;
            CellIndices = cellIndices;
            DirectionName = directionName;
            TeamId = teamId;
            ContainsAnchorJari = containsAnchorJari;
        }

        public static PomokuLineResult NotCompleted(TeamId teamId)
        {
            return new PomokuLineResult(false, new List<int>(), string.Empty, teamId, false);
        }
    }

    public static class PomokuChecker
    {
        private const int RequiredLineLength = 5;

        public static PomokuLineResult CheckForPomoku(
            IReadOnlyList<BoardCellData> boardCells,
            int boardSize,
            int placedCellIndex,
            TeamId teamId)
        {
            if (boardCells == null || boardSize <= 0 || teamId == TeamId.None)
            {
                return PomokuLineResult.NotCompleted(teamId);
            }

            if (placedCellIndex < 0 || placedCellIndex >= boardCells.Count)
            {
                return PomokuLineResult.NotCompleted(teamId);
            }

            if (boardCells[placedCellIndex].ChipOwnerTeam != teamId)
            {
                return PomokuLineResult.NotCompleted(teamId);
            }

            int placedRow = placedCellIndex / boardSize;
            int placedColumn = placedCellIndex % boardSize;

            PomokuLineResult horizontalResult = CheckDirection(boardCells, boardSize, placedRow, placedColumn, 0, 1, "Horizontal", teamId);
            if (horizontalResult.IsCompleted)
            {
                return horizontalResult;
            }

            PomokuLineResult verticalResult = CheckDirection(boardCells, boardSize, placedRow, placedColumn, 1, 0, "Vertical", teamId);
            if (verticalResult.IsCompleted)
            {
                return verticalResult;
            }

            PomokuLineResult diagonalDownResult = CheckDirection(boardCells, boardSize, placedRow, placedColumn, 1, 1, "Diagonal TL-BR", teamId);
            if (diagonalDownResult.IsCompleted)
            {
                return diagonalDownResult;
            }

            PomokuLineResult diagonalUpResult = CheckDirection(boardCells, boardSize, placedRow, placedColumn, -1, 1, "Diagonal TR-BL", teamId);
            if (diagonalUpResult.IsCompleted)
            {
                return diagonalUpResult;
            }

            return PomokuLineResult.NotCompleted(teamId);
        }

        private static PomokuLineResult CheckDirection(
            IReadOnlyList<BoardCellData> boardCells,
            int boardSize,
            int placedRow,
            int placedColumn,
            int rowStep,
            int columnStep,
            string directionName,
            TeamId teamId)
        {
            List<int> connectedCells = new List<int>();

            AddMatchingCells(boardCells, boardSize, placedRow, placedColumn, -rowStep, -columnStep, teamId, connectedCells);
            connectedCells.Reverse();

            int placedCellIndex = placedRow * boardSize + placedColumn;
            connectedCells.Add(placedCellIndex);

            AddMatchingCells(boardCells, boardSize, placedRow, placedColumn, rowStep, columnStep, teamId, connectedCells);

            if (connectedCells.Count < RequiredLineLength)
            {
                return PomokuLineResult.NotCompleted(teamId);
            }

            return GetCompletedFiveCellLine(boardCells, connectedCells, placedCellIndex, directionName, teamId);
        }

        private static void AddMatchingCells(
            IReadOnlyList<BoardCellData> boardCells,
            int boardSize,
            int startRow,
            int startColumn,
            int rowStep,
            int columnStep,
            TeamId teamId,
            List<int> connectedCells)
        {
            int row = startRow + rowStep;
            int column = startColumn + columnStep;

            while (IsInsideBoard(row, column, boardSize))
            {
                int cellIndex = row * boardSize + column;

                if (!IsConnectableForTeam(boardCells[cellIndex], teamId))
                {
                    break;
                }

                connectedCells.Add(cellIndex);

                row += rowStep;
                column += columnStep;
            }
        }

        private static PomokuLineResult GetCompletedFiveCellLine(
            IReadOnlyList<BoardCellData> boardCells,
            List<int> connectedCells,
            int placedCellIndex,
            string directionName,
            TeamId teamId)
        {
            int placedIndexInLine = connectedCells.IndexOf(placedCellIndex);
            int latestPossibleStartIndex = connectedCells.Count - RequiredLineLength;

            for (int startIndex = 0; startIndex <= latestPossibleStartIndex; startIndex++)
            {
                int endIndex = startIndex + RequiredLineLength - 1;

                if (placedIndexInLine < startIndex || placedIndexInLine > endIndex)
                {
                    continue;
                }

                List<int> candidateLine = connectedCells.GetRange(startIndex, RequiredLineLength);

                if (IsCompletedPomokuLine(boardCells, candidateLine, teamId, out bool containsAnchorJari))
                {
                    return new PomokuLineResult(true, candidateLine, directionName, teamId, containsAnchorJari);
                }
            }

            return PomokuLineResult.NotCompleted(teamId);
        }

        private static bool IsCompletedPomokuLine(
            IReadOnlyList<BoardCellData> boardCells,
            List<int> candidateLine,
            TeamId teamId,
            out bool containsAnchorJari)
        {
            int teamChipCount = 0;
            int anchorJariCount = 0;

            for (int i = 0; i < candidateLine.Count; i++)
            {
                BoardCellData cellData = boardCells[candidateLine[i]];

                if (cellData.CellType == BoardCellType.AnchorJari)
                {
                    anchorJariCount++;
                }
                else if (cellData.ChipOwnerTeam == teamId)
                {
                    teamChipCount++;
                }
            }

            containsAnchorJari = anchorJariCount == 1;

            bool isNormalPomoku = teamChipCount == RequiredLineLength && anchorJariCount == 0;
            bool isAnchorJariPomoku = teamChipCount == RequiredLineLength - 1 && anchorJariCount == 1;

            return isNormalPomoku || isAnchorJariPomoku;
        }

        private static bool IsConnectableForTeam(BoardCellData cellData, TeamId teamId)
        {
            if (cellData.CellType == BoardCellType.AnchorJari)
            {
                return true;
            }

            return cellData.ChipOwnerTeam == teamId;
        }

        private static bool IsInsideBoard(int row, int column, int boardSize)
        {
            return row >= 0 && row < boardSize && column >= 0 && column < boardSize;
        }
    }
}
