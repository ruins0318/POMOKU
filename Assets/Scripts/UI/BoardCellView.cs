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

        private BoardCellData cellData;
        private Image backgroundImage;
        private Text labelText;

        public BoardCellData CellData
        {
            get { return cellData; }
        }

        public void SetCellData(BoardCellData cellData, Font labelFont)
        {
            this.cellData = cellData;

            EnsureViewObjects(labelFont);

            labelText.text = cellData.GetDisplayName();
            SetHighlighted(false);
        }

        public void SetHighlighted(bool isHighlighted)
        {
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

        private void EnsureViewObjects(Font labelFont)
        {
            backgroundImage = GetComponent<Image>();

            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            backgroundImage.raycastTarget = false;

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
        }
    }
}
