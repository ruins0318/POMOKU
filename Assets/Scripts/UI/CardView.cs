using System;
using Pomoku.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class CardView : MonoBehaviour
    {
        private readonly Color normalColor = new Color(0.95f, 0.96f, 0.98f);
        private readonly Color selectedColor = new Color(0.98f, 0.78f, 0.25f);

        private CardData cardData;
        private Image backgroundImage;
        private Button cardButton;
        private Text cardNameText;
        private Action<CardView, CardData> cardSelected;

        public CardData CardData
        {
            get { return cardData; }
        }

        public void SetCardData(CardData newCardData, Font labelFont, Action<CardView, CardData> onCardSelected)
        {
            cardData = newCardData;
            cardSelected = onCardSelected;

            EnsureViewObjects(labelFont);

            cardNameText.text = cardData.GetDisplayName();
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            EnsureBackgroundImage();

            backgroundImage.color = isSelected ? selectedColor : normalColor;
            cardNameText.color = isSelected ? new Color(0.12f, 0.08f, 0.02f) : new Color(0.08f, 0.12f, 0.16f);
        }

        private void EnsureViewObjects(Font labelFont)
        {
            EnsureBackgroundImage();
            EnsureButton();
            EnsureText(labelFont);
        }

        private void EnsureBackgroundImage()
        {
            backgroundImage = GetComponent<Image>();

            if (backgroundImage == null)
            {
                backgroundImage = gameObject.AddComponent<Image>();
            }

            backgroundImage.raycastTarget = true;
        }

        private void EnsureButton()
        {
            cardButton = GetComponent<Button>();

            if (cardButton == null)
            {
                cardButton = gameObject.AddComponent<Button>();
            }

            cardButton.targetGraphic = backgroundImage;
            cardButton.transition = Selectable.Transition.None;
            cardButton.onClick.RemoveListener(SelectCard);
            cardButton.onClick.AddListener(SelectCard);
        }

        private void EnsureText(Font labelFont)
        {
            if (cardNameText != null)
            {
                return;
            }

            GameObject textObject = new GameObject("CardNameText", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(transform, false);

            RectTransform textRectTransform = textObject.GetComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.offsetMin = new Vector2(8f, 8f);
            textRectTransform.offsetMax = new Vector2(-8f, -8f);

            cardNameText = textObject.GetComponent<Text>();
            cardNameText.font = labelFont;
            cardNameText.fontSize = 24;
            cardNameText.alignment = TextAnchor.MiddleCenter;
            cardNameText.raycastTarget = false;
            cardNameText.resizeTextForBestFit = true;
            cardNameText.resizeTextMinSize = 12;
            cardNameText.resizeTextMaxSize = 24;
        }

        private void SelectCard()
        {
            if (cardSelected != null)
            {
                cardSelected(this, cardData);
            }
        }
    }
}
