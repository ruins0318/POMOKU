using Pomoku.Board;
using UnityEngine;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class BoardCellView : MonoBehaviour
    {
        private Image backgroundImage;
        private Text labelText;

        public void SetCellData(BoardCellData cellData, Font labelFont)
        {
            EnsureViewObjects(labelFont);

            labelText.text = cellData.GetDisplayName();

            if (cellData.CellType == BoardCellType.AnchorJari)
            {
                backgroundImage.color = new Color(0.95f, 0.74f, 0.28f);
                labelText.color = new Color(0.12f, 0.09f, 0.04f);
            }
            else
            {
                backgroundImage.color = new Color(0.93f, 0.95f, 0.97f);
                labelText.color = new Color(0.08f, 0.12f, 0.16f);
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
