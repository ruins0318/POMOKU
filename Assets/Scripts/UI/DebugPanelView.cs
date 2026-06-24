using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Pomoku.UI
{
    public class DebugPanelView : MonoBehaviour
    {
        private Canvas canvas;
        private RectTransform panelTransform;
        private Font labelFont;

        public void Show(Action normalModeClicked, Action debugScoringClicked, Action restartClicked)
        {
            EnsureCanvas();
            EnsurePanel();
            ClearButtons();

            CreateButton("Normal 4P Random", 0, normalModeClicked);
            CreateButton("Debug Scoring Test", 1, debugScoringClicked);
            CreateButton("Restart Current Mode", 2, restartClicked);
        }

        private void EnsureCanvas()
        {
            if (canvas != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("PomokuDebugPanelCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;

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

        private void EnsurePanel()
        {
            if (panelTransform != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("DebugPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            panelTransform = panelObject.GetComponent<RectTransform>();
            panelTransform.anchorMin = new Vector2(1f, 1f);
            panelTransform.anchorMax = new Vector2(1f, 1f);
            panelTransform.pivot = new Vector2(1f, 1f);
            panelTransform.anchoredPosition = new Vector2(-24f, -24f);
            panelTransform.sizeDelta = new Vector2(260f, 144f);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.05f, 0.06f, 0.07f, 0.88f);
            panelImage.raycastTarget = false;
        }

        private void ClearButtons()
        {
            for (int i = panelTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(panelTransform.GetChild(i).gameObject);
            }
        }

        private void CreateButton(string buttonText, int buttonIndex, Action clicked)
        {
            GameObject buttonObject = new GameObject(buttonText, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(panelTransform, false);

            RectTransform buttonTransform = buttonObject.GetComponent<RectTransform>();
            buttonTransform.anchorMin = new Vector2(0f, 1f);
            buttonTransform.anchorMax = new Vector2(1f, 1f);
            buttonTransform.pivot = new Vector2(0.5f, 1f);
            buttonTransform.anchoredPosition = new Vector2(0f, -12f - (buttonIndex * 42f));
            buttonTransform.sizeDelta = new Vector2(-24f, 34f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.23f, 0.28f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.RemoveAllListeners();

            if (clicked != null)
            {
                button.onClick.AddListener(() => clicked());
            }

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);

            RectTransform textTransform = textObject.GetComponent<RectTransform>();
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = new Vector2(8f, 4f);
            textTransform.offsetMax = new Vector2(-8f, -4f);

            Text labelText = textObject.GetComponent<Text>();
            labelText.font = GetLabelFont();
            labelText.fontSize = 16;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.raycastTarget = false;
            labelText.text = buttonText;
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
