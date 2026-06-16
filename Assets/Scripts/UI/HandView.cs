using System.Collections.Generic;
using Pomoku.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class HandView : MonoBehaviour
    {
        private Canvas canvas;
        private RectTransform handPanel;
        private Text handText;
        private Font labelFont;

        public void ShowCurrentPlayerHand(int playerIndex, IReadOnlyList<CardData> handCards)
        {
            EnsureCanvas();
            EnsureHandPanel();

            handText.text = "Player " + (playerIndex + 1) + " Hand: " + FormatHandCards(handCards);
        }

        private void EnsureCanvas()
        {
            if (canvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("PomokuHandCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
        }

        private void EnsureHandPanel()
        {
            if (handPanel != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("CurrentPlayerHandPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            handPanel = panelObject.GetComponent<RectTransform>();
            handPanel.anchorMin = new Vector2(0.5f, 0f);
            handPanel.anchorMax = new Vector2(0.5f, 0f);
            handPanel.pivot = new Vector2(0.5f, 0f);
            handPanel.anchoredPosition = new Vector2(0f, 28f);
            handPanel.sizeDelta = new Vector2(1100f, 70f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.10f, 0.12f, 0.88f);
            panelImage.raycastTarget = false;

            GameObject textObject = new GameObject("CurrentPlayerHandText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(handPanel, false);

            RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = new Vector2(18f, 8f);
            textRectTransform.offsetMax = new Vector2(-18f, -8f);

            handText = textObject.GetComponent<Text>();
            handText.font = GetLabelFont();
            handText.fontSize = 24;
            handText.alignment = TextAnchor.MiddleCenter;
            handText.color = Color.white;
            handText.raycastTarget = false;
            handText.resizeTextForBestFit = true;
            handText.resizeTextMinSize = 12;
            handText.resizeTextMaxSize = 24;
        }

        private string FormatHandCards(IReadOnlyList<CardData> handCards)
        {
            if (handCards == null || handCards.Count == 0)
            {
                return "(empty)";
            }

            List<string> cardNames = new List<string>();

            for (int i = 0; i < handCards.Count; i++)
            {
                cardNames.Add(handCards[i].GetDisplayName());
            }

            return string.Join(", ", cardNames);
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
