using System.Collections.Generic;

namespace Pomoku.Board
{
    public struct CompletedPomokuLine
    {
        public TeamId TeamId;
        public List<int> CellIndices;
        public string DirectionName;
        public bool ContainsAnchorJari;

        public CompletedPomokuLine(TeamId teamId, List<int> cellIndices, string directionName, bool containsAnchorJari)
        {
            TeamId = teamId;
            CellIndices = cellIndices;
            DirectionName = directionName;
            ContainsAnchorJari = containsAnchorJari;
        }
    }
}
