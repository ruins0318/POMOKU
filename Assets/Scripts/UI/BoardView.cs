using System.Collections.Generic;
using System;
using Pomoku.Board;
using Pomoku.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class BoardView : MonoBehaviour
    {
        private const float BoardPanelSize = 760f;
        private const float CellSize = 72f;
        private const float CellSpacing = 4f;

        private Canvas canvas;
        private RectTransform boardPanel;
        private Font labelFont;
        private readonly List<BoardCellView> boardCellViews = new List<BoardCellView>();
        private Action<int, BoardCellData> cellClicked;

        public void ShowBoard(IReadOnlyList<BoardCellData> boardCells, int boardSize, Action<int, BoardCellData> onCellClicked)
        {
            if (boardCells == null || boardCells.Count != boardSize * boardSize)
            {
                Debug.LogError("BoardView received invalid board data.");
                return;
            }

            cellClicked = onCellClicked;

            EnsureCanvas();
            EnsureBoardPanel();
            ClearBoardCells();
            boardCellViews.Clear();

            for (int i = 0; i < boardCells.Count; i++)
            {
                CreateBoardCell(boardCells[i]);
            }
        }

        public bool IsCellHighlighted(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= boardCellViews.Count)
            {
                return false;
            }

            return boardCellViews[cellIndex].IsHighlighted;
        }

        public void ShowChipAtCell(int cellIndex, TeamId teamId)
        {
            if (cellIndex < 0 || cellIndex >= boardCellViews.Count)
            {
                Debug.LogError("Cannot show chip because cell index is invalid: " + cellIndex);
                return;
            }

            BoardCellView cellView = boardCellViews[cellIndex];
            cellView.CellData.ChipOwnerTeam = teamId;
            cellView.RefreshChipDisplay();
        }

        public int HighlightCellsMatchingCard(CardData selectedCard)
        {
            ClearHighlightedCells();

            if (!MvpCardRules.IsMvpRegularCard(selectedCard))
            {
                return 0;
            }

            int highlightedCellCount = 0;

            for (int i = 0; i < boardCellViews.Count; i++)
            {
                BoardCellView cellView = boardCellViews[i];
                BoardCellData cellData = cellView.CellData;

                if (IsNormalCellMatchingCard(cellData, selectedCard))
                {
                    cellView.SetHighlighted(true);
                    highlightedCellCount++;
                }
            }

            return highlightedCellCount;
        }

        public void ClearHighlightedCells()
        {
            for (int i = 0; i < boardCellViews.Count; i++)
            {
                boardCellViews[i].SetHighlighted(false);
            }
        }

        public void HighlightPomokuLine(IReadOnlyList<int> cellIndices, TeamId teamId)
        {
            if (cellIndices == null)
            {
                return;
            }

            for (int i = 0; i < cellIndices.Count; i++)
            {
                int cellIndex = cellIndices[i];

                if (cellIndex < 0 || cellIndex >= boardCellViews.Count)
                {
                    Debug.LogWarning("Cannot highlight Pomoku line because cell index is invalid: " + cellIndex);
                    continue;
                }

                boardCellViews[cellIndex].SetPomokuLineHighlighted(true, teamId);
            }
        }

        public void ClearPomokuLineHighlights()
        {
            for (int i = 0; i < boardCellViews.Count; i++)
            {
                boardCellViews[i].SetPomokuLineHighlighted(false, TeamId.None);
            }
        }

        private void EnsureCanvas()
        {
            if (canvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("PomokuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureBoardPanel()
        {
            if (boardPanel != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("BoardPanel", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            boardPanel = panelObject.GetComponent<RectTransform>();
            boardPanel.anchorMin = new Vector2(0.5f, 0.5f);
            boardPanel.anchorMax = new Vector2(0.5f, 0.5f);
            boardPanel.pivot = new Vector2(0.5f, 0.5f);
            boardPanel.anchoredPosition = Vector2.zero;
            boardPanel.sizeDelta = new Vector2(BoardPanelSize, BoardPanelSize);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.18f, 0.20f, 0.23f);
            panelImage.raycastTarget = false;

            GridLayoutGroup gridLayoutGroup = panelObject.GetComponent<GridLayoutGroup>();
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = BoardManager.BoardSize;
            gridLayoutGroup.cellSize = new Vector2(CellSize, CellSize);
            gridLayoutGroup.spacing = new Vector2(CellSpacing, CellSpacing);
            gridLayoutGroup.padding = new RectOffset(4, 4, 4, 4);
        }

        private void ClearBoardCells()
        {
            for (int i = boardPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(boardPanel.GetChild(i).gameObject);
            }
        }

        private void CreateBoardCell(BoardCellData cellData)
        {
            GameObject cellObject = new GameObject("Cell_" + cellData.Row + "_" + cellData.Column, typeof(RectTransform), typeof(Image), typeof(BoardCellView));
            cellObject.transform.SetParent(boardPanel, false);

            BoardCellView cellView = cellObject.GetComponent<BoardCellView>();
            cellView.SetCellData(cellData, boardCellViews.Count, GetLabelFont(), cellClicked);
            boardCellViews.Add(cellView);
        }

        private static bool IsNormalCellMatchingCard(BoardCellData cellData, CardData selectedCard)
        {
            if (cellData == null)
            {
                return false;
            }

            if (cellData.CellType != BoardCellType.Normal)
            {
                return false;
            }

            if (cellData.ChipOwnerTeam != TeamId.None)
            {
                return false;
            }

            return cellData.Card.Suit == selectedCard.Suit && cellData.Card.Rank == selectedCard.Rank;
        }

        private Font GetLabelFont()
        {
            if (labelFont == null)
            {
                labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return labelFont;
        }
    }
}
