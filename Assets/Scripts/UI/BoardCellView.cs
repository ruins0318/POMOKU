using System;
using Pomoku.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class BoardCellView : MonoBehaviour
    {
        private readonly Color normalCellColor = new Color(0.93f, 0.95f, 0.97f);
        private readonly Color anchorJariCellColor = new Color(0.95f, 0.74f, 0.28f);
        private readonly Color highlightedCellColor = new Color(0.44f, 0.82f, 0.46f);
        private readonly Color normalTextColor = new Color(0.08f, 0.12f, 0.16f);
        private readonly Color anchorJariTextColor = new Color(0.12f, 0.09f, 0.04f);
        private readonly Color teamAChipColor = new Color(0.18f, 0.35f, 0.95f);

        private BoardCellData cellData;
        private int cellIndex;
        private bool isHighlighted;
        private Image backgroundImage;
        private Button cellButton;
        private Text labelText;
        private Text chipText;
        private Action<int, BoardCellData> cellClicked;

        public BoardCellData CellData
        {
            get { return cellData; }
        }

        public bool IsHighlighted
        {
            get { return isHighlighted; }
        }

        public void SetCellData(BoardCellData cellData, int cellIndex, Font labelFont, Action<int, BoardCellData> onCellClicked)
        {
            this.cellData = cellData;
            this.cellIndex = cellIndex;
            cellClicked = onCellClicked;

            EnsureViewObjects(labelFont);

            labelText.text = cellData.GetDisplayName();
            SetHighlighted(false);
            RefreshChipDisplay();
        }

        public void SetHighlighted(bool isHighlighted)
        {
            this.isHighlighted = isHighlighted;

            if (cellData == null)
            {
                return;
            }

            if (isHighlighted && cellData.CellType == BoardCellType.Normal)
            {
                backgroundImage.color = highlightedCellColor;
                labelText.color = normalTextColor;
                return;
            }

            if (cellData.CellType == BoardCellType.AnchorJari)
            {
                backgroundImage.color = anchorJariCellColor;
                labelText.color = anchorJariTextColor;
            }
            else
            {
                backgroundImage.color = normalCellColor;
                labelText.color = normalTextColor;
            }
        }

        public void RefreshChipDisplay()
        {
            if (chipText == null || cellData == null)
            {
                return;
            }

            if (cellData.ChipOwnerTeam == TeamId.TeamA)
            {
                chipText.text = "A";
                chipText.color = teamAChipColor;
            }
            else
            {
                chipText.text = string.Empty;
            }
        }

        private void EnsureViewObjects(Font labelFont)
        {
            backgroundImage = GetComponent<Image>();

            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            backgroundImage.raycastTarget = true;

            EnsureButton();

            if (labelText != null)
            {
                return;
            }

            GameObject labelObject = new GameObject("CardLabel", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);

            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            labelRectTransform.anchorMin = Vector2.zero;
            labelRectTransform.anchorMax = Vector2.one;
            labelRectTransform.offsetMin = new Vector2(4f, 4f);
            labelRectTransform.offsetMax = new Vector2(-4f, -4f);

            labelText = labelObject.GetComponent<Text>();
            labelText.font = labelFont;
            labelText.fontSize = 15;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.raycastTarget = false;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 8;
            labelText.resizeTextMaxSize = 15;

            GameObject chipObject = new GameObject("ChipLabel", typeof(RectTransform), typeof(Text));
            chipObject.transform.SetParent(transform, false);

            RectTransform chipRectTransform = chipObject.GetComponent<RectTransform>();
            chipRectTransform.anchorMin = new Vector2(1f, 0f);
            chipRectTransform.anchorMax = new Vector2(1f, 0f);
            chipRectTransform.pivot = new Vector2(1f, 0f);
            chipRectTransform.anchoredPosition = new Vector2(-6f, 6f);
            chipRectTransform.sizeDelta = new Vector2(24f, 24f);

            chipText = chipObject.GetComponent<Text>();
            chipText.font = labelFont;
            chipText.fontSize = 22;
            chipText.fontStyle = FontStyle.Bold;
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.raycastTarget = false;
        }

        private void EnsureButton()
        {
            cellButton = GetComponent<Button>();

            if (cellButton == null)
            {
                cellButton = gameObject.AddComponent<Button>();
            }

            cellButton.targetGraphic = backgroundImage;
            cellButton.transition = Selectable.Transition.None;
            cellButton.onClick.RemoveListener(ClickCell);
            cellButton.onClick.AddListener(ClickCell);
        }

        private void ClickCell()
        {
            if (cellClicked != null)
            {
                cellClicked(cellIndex, cellData);
            }
        }
    }
}
