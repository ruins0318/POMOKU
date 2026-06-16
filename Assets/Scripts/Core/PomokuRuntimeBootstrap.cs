using Pomoku.Board;
using Pomoku.UI;
using UnityEngine;

namespace Pomoku.Core
{
    public static class PomokuRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateBoardPreview()
        {
            if (GameObject.Find("PomokuRuntimeBootstrap") != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject("PomokuRuntimeBootstrap");

            BoardManager boardManager = bootstrapObject.AddComponent<BoardManager>();
            boardManager.CreateBoard();

            BoardView boardView = bootstrapObject.AddComponent<BoardView>();
            boardView.ShowBoard(boardManager.BoardCells, BoardManager.BoardSize);
        }
    }
}
