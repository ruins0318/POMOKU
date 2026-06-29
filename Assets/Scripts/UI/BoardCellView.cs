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
        private readonly Color teamBChipColor = new Color(0.88f, 0.18f, 0.20f);
        private readonly Color teamAChipOverlayColor = new Color(0.18f, 0.35f, 0.95f, 0.34f);
        private readonly Color teamBChipOverlayColor = new Color(0.88f, 0.18f, 0.20f, 0.34f);
        private readonly Color teamAPomokuLineOutlineColor = new Color(0.16f, 0.62f, 1f);
        private readonly Color teamBPomokuLineOutlineColor = new Color(1f, 0.22f, 0.24f);

        private BoardCellData cellData;
        private int cellIndex;
        private bool isHighlighted;
        private Image backgroundImage;
        private Image chipOverlayImage;
        private Outline pomokuLineOutline;
        private Button cellButton;
        private Text labelText;
        private Text chipText;
        private Text lockedText;
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

            isHighlighted = false;
            RefreshView();
        }

        public void SetHighlighted(bool isHighlighted)
        {
            this.isHighlighted = isHighlighted;
            RefreshBackgroundDisplay();
        }

        public void SetPomokuLineHighlighted(bool isPomokuLineHighlighted, TeamId teamId)
        {
            EnsurePomokuLineOutline();
            pomokuLineOutline.effectColor = GetPomokuLineOutlineColor(teamId);
            pomokuLineOutline.enabled = isPomokuLineHighlighted;
        }

        public void SetLocked(bool isLocked)
        {
            if (cellData == null)
            {
                return;
            }

            cellData.IsLocked = isLocked;
            RefreshLockedDisplay();
        }

        public void RefreshChipDisplay()
        {
            if (cellData == null)
            {
                return;
            }

            RefreshChipOverlayDisplay();
            RefreshChipLabelDisplay();
        }

        public void RefreshLockedDisplay()
        {
            if (lockedText == null || cellData == null)
            {
                return;
            }

            lockedText.text = cellData.IsLocked ? "L" : string.Empty;
        }

        private void RefreshView()
        {
            if (cellData == null)
            {
                return;
            }

            RefreshCardTextDisplay();
            RefreshBackgroundDisplay();
            RefreshChipDisplay();
            RefreshLockedDisplay();
        }

        private void RefreshCardTextDisplay()
        {
            if (labelText == null || cellData == null)
            {
                return;
            }

            labelText.text = cellData.GetDisplayName();
            labelText.color = cellData.CellType == BoardCellType.AnchorJari
                ? anchorJariTextColor
                : normalTextColor;
        }

        private void RefreshBackgroundDisplay()
        {
            if (backgroundImage == null || cellData == null)
            {
                return;
            }

            if (isHighlighted && cellData.CellType == BoardCellType.Normal)
            {
                backgroundImage.color = highlightedCellColor;
                return;
            }

            if (cellData.CellType == BoardCellType.AnchorJari)
            {
                backgroundImage.color = anchorJariCellColor;
            }
            else
            {
                backgroundImage.color = normalCellColor;
            }
        }

        private void RefreshChipOverlayDisplay()
        {
            if (chipOverlayImage == null || cellData == null)
            {
                return;
            }

            if (cellData.ChipOwnerTeam == TeamId.TeamA)
            {
                chipOverlayImage.enabled = true;
                chipOverlayImage.color = teamAChipOverlayColor;
                return;
            }

            if (cellData.ChipOwnerTeam == TeamId.TeamB)
            {
                chipOverlayImage.enabled = true;
                chipOverlayImage.color = teamBChipOverlayColor;
                return;
            }

            chipOverlayImage.enabled = false;
        }

        private void RefreshChipLabelDisplay()
        {
            if (chipText == null || cellData == null)
            {
                return;
            }

            if (cellData.ChipOwnerTeam == TeamId.TeamA)
            {
                chipText.text = "A";
                chipText.color = teamAChipColor;
                return;
            }

            if (cellData.ChipOwnerTeam == TeamId.TeamB)
            {
                chipText.text = "B";
                chipText.color = teamBChipColor;
                return;
            }

            chipText.text = string.Empty;
        }

        private void EnsureViewObjects(Font labelFont)
        {
            backgroundImage = GetComponent<Image>();

            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            backgroundImage.raycastTarget = true;

            EnsurePomokuLineOutline();
            EnsureButton();

            if (labelText != null)
            {
                return;
            }

            GameObject chipOverlayObject = new GameObject("ChipOverlayLayer", typeof(RectTransform), typeof(Image));
            chipOverlayObject.transform.SetParent(transform, false);

            RectTransform chipOverlayTransform = chipOverlayObject.GetComponent<RectTransform>();
            chipOverlayTransform.anchorMin = Vector2.zero;
            chipOverlayTransform.anchorMax = Vector2.one;
            chipOverlayTransform.offsetMin = new Vector2(8f, 8f);
            chipOverlayTransform.offsetMax = new Vector2(-8f, -8f);

            chipOverlayImage = chipOverlayObject.GetComponent<Image>();
            chipOverlayImage.enabled = false;
            chipOverlayImage.raycastTarget = false;

            GameObject labelObject = new GameObject("CardTextLayer", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);

            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            labelRectTransform.anchorMin = Vector2.zero;
            labelRectTransform.anchorMax = Vector2.one;
            labelRectTransform.offsetMin = new Vector2(5f, 14f);
            labelRectTransform.offsetMax = new Vector2(-5f, -8f);

            labelText = labelObject.GetComponent<Text>();
            labelText.font = labelFont;
            labelText.fontSize = 16;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.raycastTarget = false;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 8;
            labelText.resizeTextMaxSize = 16;

            GameObject chipObject = new GameObject("ChipLabelLayer", typeof(RectTransform), typeof(Text), typeof(Outline));
            chipObject.transform.SetParent(transform, false);

            RectTransform chipRectTransform = chipObject.GetComponent<RectTransform>();
            chipRectTransform.anchorMin = new Vector2(1f, 0f);
            chipRectTransform.anchorMax = new Vector2(1f, 0f);
            chipRectTransform.pivot = new Vector2(1f, 0f);
            chipRectTransform.anchoredPosition = new Vector2(-6f, 6f);
            chipRectTransform.sizeDelta = new Vector2(22f, 22f);

            chipText = chipObject.GetComponent<Text>();
            chipText.font = labelFont;
            chipText.fontSize = 16;
            chipText.fontStyle = FontStyle.Bold;
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.raycastTarget = false;

            Outline chipLabelOutline = chipObject.GetComponent<Outline>();
            chipLabelOutline.effectColor = Color.white;
            chipLabelOutline.effectDistance = new Vector2(1f, -1f);
            chipLabelOutline.useGraphicAlpha = false;

            GameObject lockedObject = new GameObject("LockIndicatorLayer", typeof(RectTransform), typeof(Text), typeof(Outline));
            lockedObject.transform.SetParent(transform, false);

            RectTransform lockedRectTransform = lockedObject.GetComponent<RectTransform>();
            lockedRectTransform.anchorMin = new Vector2(0f, 1f);
            lockedRectTransform.anchorMax = new Vector2(0f, 1f);
            lockedRectTransform.pivot = new Vector2(0f, 1f);
            lockedRectTransform.anchoredPosition = new Vector2(6f, -4f);
            lockedRectTransform.sizeDelta = new Vector2(18f, 18f);

            lockedText = lockedObject.GetComponent<Text>();
            lockedText.font = labelFont;
            lockedText.fontSize = 14;
            lockedText.fontStyle = FontStyle.Bold;
            lockedText.alignment = TextAnchor.MiddleCenter;
            lockedText.color = new Color(0.08f, 0.08f, 0.08f);
            lockedText.raycastTarget = false;

            Outline lockTextOutline = lockedObject.GetComponent<Outline>();
            lockTextOutline.effectColor = Color.white;
            lockTextOutline.effectDistance = new Vector2(1f, -1f);
            lockTextOutline.useGraphicAlpha = false;
        }

        private void EnsurePomokuLineOutline()
        {
            pomokuLineOutline = GetComponent<Outline>();

            if (pomokuLineOutline == null)
            {
                pomokuLineOutline = gameObject.AddComponent<Outline>();
            }

            pomokuLineOutline.effectColor = GetPomokuLineOutlineColor(TeamId.None);
            pomokuLineOutline.effectDistance = new Vector2(4f, -4f);
            pomokuLineOutline.useGraphicAlpha = false;
            pomokuLineOutline.enabled = false;
        }

        private Color GetPomokuLineOutlineColor(TeamId teamId)
        {
            if (teamId == TeamId.TeamA)
            {
                return teamAPomokuLineOutlineColor;
            }

            if (teamId == TeamId.TeamB)
            {
                return teamBPomokuLineOutlineColor;
            }

            return Color.white;
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
