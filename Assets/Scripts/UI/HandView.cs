using System;
using System.Collections.Generic;
using Pomoku.Cards;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class HandView : MonoBehaviour
    {
        private Canvas canvas;
        private RectTransform handPanel;
        private RectTransform cardsPanel;
        private Text playerLabelText;
        private Font labelFont;
        private CardView selectedCardView;
        private Action<CardData> cardSelected;

        public void ShowCurrentPlayerHand(int playerIndex, IReadOnlyList<CardData> handCards, Action<CardData> onCardSelected)
        {
            cardSelected = onCardSelected;
            selectedCardView = null;

            EnsureCanvas();
            EnsureHandPanel();
            ClearCardViews();

            playerLabelText.text = "Player " + (playerIndex + 1) + " Hand";

            if (handCards == null)
            {
                return;
            }

            for (int i = 0; i < handCards.Count; i++)
            {
                CreateCardView(handCards[i], i);
            }
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

            EnsureEventSystem();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("PomokuEventSystem", typeof(EventSystem));
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
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
            handPanel.sizeDelta = new Vector2(1100f, 170f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.10f, 0.12f, 0.88f);
            panelImage.raycastTarget = false;

            GameObject labelObject = new GameObject("CurrentPlayerLabelText", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(handPanel, false);

            RectTransform labelRectTransform = labelObject.GetComponent<RectTransform>();
            labelRectTransform.anchorMin = new Vector2(0f, 1f);
            labelRectTransform.anchorMax = new Vector2(1f, 1f);
            labelRectTransform.pivot = new Vector2(0.5f, 1f);
            labelRectTransform.anchoredPosition = new Vector2(0f, -8f);
            labelRectTransform.sizeDelta = new Vector2(0f, 32f);

            playerLabelText = labelObject.GetComponent<Text>();
            playerLabelText.font = GetLabelFont();
            playerLabelText.fontSize = 22;
            playerLabelText.alignment = TextAnchor.MiddleCenter;
            playerLabelText.color = Color.white;
            playerLabelText.raycastTarget = false;

            GameObject cardsPanelObject = new GameObject("CurrentPlayerCardsPanel", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardsPanelObject.transform.SetParent(handPanel, false);

            cardsPanel = cardsPanelObject.GetComponent<RectTransform>();
            cardsPanel.anchorMin = new Vector2(0f, 0f);
            cardsPanel.anchorMax = new Vector2(1f, 1f);
            cardsPanel.offsetMin = new Vector2(32f, 18f);
            cardsPanel.offsetMax = new Vector2(-32f, -48f);

            HorizontalLayoutGroup horizontalLayoutGroup = cardsPanelObject.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            horizontalLayoutGroup.spacing = 14f;
            horizontalLayoutGroup.childControlWidth = false;
            horizontalLayoutGroup.childControlHeight = false;
            horizontalLayoutGroup.childForceExpandWidth = false;
            horizontalLayoutGroup.childForceExpandHeight = false;
        }

        private void ClearCardViews()
        {
            for (int i = cardsPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(cardsPanel.GetChild(i).gameObject);
            }
        }

        private void CreateCardView(CardData cardData, int cardIndex)
        {
            GameObject cardObject = new GameObject("Card_" + cardIndex, typeof(RectTransform), typeof(Image), typeof(Button), typeof(CardView));
            cardObject.transform.SetParent(cardsPanel, false);

            RectTransform cardRectTransform = cardObject.GetComponent<RectTransform>();
            cardRectTransform.sizeDelta = new Vector2(120f, 86f);

            CardView cardView = cardObject.GetComponent<CardView>();
            cardView.SetCardData(cardData, GetLabelFont(), SelectCardView);
        }

        private void SelectCardView(CardView cardView, CardData cardData)
        {
            if (selectedCardView != null)
            {
                selectedCardView.SetSelected(false);
            }

            selectedCardView = cardView;
            selectedCardView.SetSelected(true);

            if (cardSelected != null)
            {
                cardSelected(cardData);
            }
        }

        public void ClearSelectedCard()
        {
            if (selectedCardView != null)
            {
                selectedCardView.SetSelected(false);
                selectedCardView = null;
            }
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
