using Kruty1918.Moyva.HomeMenu.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class HomeMenuRuntimeUiFactory
    {
        private const string PasswordPanelName = "PasswordPanel";
        private const string RuntimeUiRootName = "RuntimeHomeMenuPanels";

        public static void EnsureRequiredPanels(string infoPanelName)
        {
            var canvas = GetOrCreateCanvas();
            if (canvas == null)
                return;

            var parent = GetOrCreateRuntimeRoot(canvas.transform);
            EnsureInfoPanel(parent, string.IsNullOrWhiteSpace(infoPanelName) ? "InfoPanel" : infoPanelName.Trim());
            EnsurePasswordPanel(parent);
        }

        private static void EnsureInfoPanel(Transform parent, string panelName)
        {
            if (Object.FindFirstObjectByType<InfoPanelViewController>(FindObjectsInactive.Include) != null)
                return;

            var root = CreateModalRoot(parent, panelName);
            var card = CreateCard(root.transform, new Vector2(520f, 260f));
            CreateVerticalLayout(card, 18f, 18f);

            var title = CreateText(card.transform, "Title", "Повідомлення", 24, FontStyles.Bold, TextAlignmentOptions.Center);
            var message = CreateText(card.transform, "Message", string.Empty, 18, FontStyles.Normal, TextAlignmentOptions.Center);
            SetFlexible(message.gameObject, preferredHeight: 104f, flexibleHeight: 1f);
            var okButton = CreateButton(card.transform, "Button_OK", "OK", new Vector2(160f, 44f));

            var controller = root.AddComponent<InfoPanelViewController>();
            controller.ConfigureReferences(root, title, message, okButton);
        }

        private static void EnsurePasswordPanel(Transform parent)
        {
            if (Object.FindFirstObjectByType<PasswordPanelViewController>(FindObjectsInactive.Include) != null)
                return;

            var root = CreateModalRoot(parent, PasswordPanelName);
            var card = CreateCard(root.transform, new Vector2(520f, 320f));
            CreateVerticalLayout(card, 16f, 18f);

            var title = CreateText(card.transform, "Title", "Введіть пароль", 24, FontStyles.Bold, TextAlignmentOptions.Center);
            var input = CreateInputField(card.transform, "Input_Password", "Пароль");
            var error = CreateText(card.transform, "Error", string.Empty, 16, FontStyles.Normal, TextAlignmentOptions.Center);
            error.color = new Color(1f, 0.42f, 0.42f, 1f);
            error.gameObject.SetActive(false);

            var buttons = new GameObject("Actions", typeof(RectTransform));
            buttons.transform.SetParent(card.transform, false);
            var buttonsRect = buttons.GetComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(360f, 48f);
            var horizontal = buttons.AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 12f;
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlWidth = false;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;
            SetFlexible(buttons, preferredHeight: 48f);

            var okButton = CreateButton(buttons.transform, "Button_OK", "OK", new Vector2(150f, 44f));
            var cancelButton = CreateButton(buttons.transform, "Button_Cancel", "Скасувати", new Vector2(150f, 44f));

            var controller = root.AddComponent<PasswordPanelViewController>();
            controller.ConfigureReferences(root, title, input, error, okButton, cancelButton);
        }

        private static Canvas GetOrCreateCanvas()
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas != null && canvas.renderMode != RenderMode.WorldSpace)
                    return canvas;
            }

            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvasComponent = canvasObject.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvasComponent;
        }

        private static Transform GetOrCreateRuntimeRoot(Transform canvasTransform)
        {
            var existing = canvasTransform.Find(RuntimeUiRootName);
            if (existing != null)
                return existing;

            var root = new GameObject(RuntimeUiRootName, typeof(RectTransform));
            root.transform.SetParent(canvasTransform, false);
            var rect = root.GetComponent<RectTransform>();
            Stretch(rect);
            rect.SetAsLastSibling();
            return root.transform;
        }

        private static GameObject CreateModalRoot(Transform parent, string name)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            Stretch(rect);
            var image = root.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.58f);
            image.raycastTarget = true;
            root.SetActive(false);
            return root;
        }

        private static GameObject CreateCard(Transform parent, Vector2 size)
        {
            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(parent, false);
            var rect = card.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            card.GetComponent<Image>().color = new Color(0.08f, 0.09f, 0.12f, 0.98f);
            return card;
        }

        private static void CreateVerticalLayout(GameObject target, float spacing, float padding)
        {
            var layout = target.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        internal static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, FontStyles style, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, size + 18f);
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = new Color(0.94f, 0.95f, 0.98f, 1f);
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            SetFlexible(go, preferredHeight: size + 18f);
            return label;
        }

        private static TMP_InputField CreateInputField(Transform parent, string name, string placeholderText)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 48f);
            go.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.21f, 1f);

            var text = CreateText(go.transform, "Text", string.Empty, 18, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            var textRect = text.GetComponent<RectTransform>();
            Stretch(textRect);
            textRect.offsetMin = new Vector2(14f, 4f);
            textRect.offsetMax = new Vector2(-14f, -4f);

            var placeholder = CreateText(go.transform, "Placeholder", placeholderText, 18, FontStyles.Italic, TextAlignmentOptions.MidlineLeft);
            placeholder.color = new Color(0.62f, 0.64f, 0.7f, 1f);
            var placeholderRect = placeholder.GetComponent<RectTransform>();
            Stretch(placeholderRect);
            placeholderRect.offsetMin = new Vector2(14f, 4f);
            placeholderRect.offsetMax = new Vector2(-14f, -4f);

            var input = go.GetComponent<TMP_InputField>();
            input.textViewport = rect;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.contentType = TMP_InputField.ContentType.Password;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.targetGraphic = go.GetComponent<Image>();
            SetFlexible(go, preferredHeight: 48f);
            return input;
        }

        internal static Button CreateButton(Transform parent, string name, string labelText, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.22f, 0.42f, 0.84f, 1f);
            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText(go.transform, "Label", labelText, 18, FontStyles.Bold, TextAlignmentOptions.Center);
            var textRect = text.GetComponent<RectTransform>();
            Stretch(textRect);
            SetFlexible(go, preferredWidth: size.x, preferredHeight: size.y);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetFlexible(GameObject go, float preferredWidth = -1f, float preferredHeight = -1f, float flexibleHeight = 0f)
        {
            var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            layout.preferredWidth = preferredWidth;
            layout.preferredHeight = preferredHeight;
            layout.flexibleHeight = flexibleHeight;
        }
    }
}
