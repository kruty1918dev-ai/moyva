using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    internal static class WorldInfoPanelInventoryUiBuilder
    {
        private const string InventoryRootName = "ResourceInventory";

        public static void Ensure(
            GameObject panelRoot,
            TMP_Text descriptionText,
            Button closeButton,
            Transform constructionCostContainer)
        {
            if (panelRoot == null)
                return;

            ConfigureExistingPanel(panelRoot, descriptionText, closeButton, constructionCostContainer);

            Transform existing = panelRoot.transform.Find(InventoryRootName);
            if (existing != null)
                return;

            TMP_Text textTemplate = descriptionText != null
                ? descriptionText
                : panelRoot.GetComponentInChildren<TMP_Text>(true);

            var inventoryRoot = CreateRect(InventoryRootName, panelRoot.transform);
            Stretch(inventoryRoot, 12f, 12f, 118f, 214f);

            var inventoryBackground = inventoryRoot.gameObject.AddComponent<Image>();
            inventoryBackground.color = new Color(0.03f, 0.04f, 0.05f, 0.28f);
            inventoryBackground.raycastTarget = false;

            var tabs = CreateRect("FilterTabs", inventoryRoot);
            AnchorTopStretch(tabs, 0f, 36f);

            var tabsLayout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 6f;
            tabsLayout.padding = new RectOffset(0, 0, 0, 0);
            tabsLayout.childAlignment = TextAnchor.MiddleCenter;
            tabsLayout.childControlWidth = true;
            tabsLayout.childControlHeight = true;
            tabsLayout.childForceExpandWidth = true;
            tabsLayout.childForceExpandHeight = true;

            CreateFilterButton("AllButton", "Все", tabs, textTemplate);
            CreateFilterButton("FoodButton", "Їжа", tabs, textTemplate);
            CreateFilterButton("MaterialsButton", "Матеріали", tabs, textTemplate);

            var scrollRoot = CreateRect("ResourceScroll", inventoryRoot);
            Stretch(scrollRoot, 0f, 0f, 0f, 42f);

            var scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.08f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 34f;

            var viewport = CreateRect("Viewport", scrollRoot);
            Stretch(viewport, 0f, 0f, 0f, 0f);

            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.10f);
            viewportImage.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(6, 6, 6, 6);
            contentLayout.spacing = 4f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var emptyState = CreateText("EmptyState", viewport, textTemplate, "Ресурсів немає");
            Stretch(emptyState.rectTransform, 16f, 16f, 16f, 16f);
            emptyState.alignment = TextAlignmentOptions.Center;
            emptyState.fontSize = 18f;
            emptyState.color = new Color(0.75f, 0.78f, 0.82f, 1f);
            emptyState.raycastTarget = false;

            scrollRect.viewport = viewport;
            scrollRect.content = content;

            inventoryRoot.gameObject.SetActive(false);
        }

        private static void ConfigureExistingPanel(
            GameObject panelRoot,
            TMP_Text descriptionText,
            Button closeButton,
            Transform constructionCostContainer)
        {
            if (descriptionText != null)
            {
                var rect = descriptionText.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -108f);
                rect.sizeDelta = new Vector2(-24f, 96f);

                descriptionText.fontSize = 18f;
                descriptionText.alignment = TextAlignmentOptions.TopLeft;
                descriptionText.overflowMode = TextOverflowModes.Ellipsis;
                descriptionText.raycastTarget = false;
            }

            if (constructionCostContainer is RectTransform costRect)
            {
                costRect.anchorMin = new Vector2(0f, 0f);
                costRect.anchorMax = new Vector2(1f, 0f);
                costRect.pivot = new Vector2(0.5f, 0f);
                costRect.offsetMin = new Vector2(12f, 14f);
                costRect.offsetMax = new Vector2(-164f, 104f);

                var group = constructionCostContainer.GetComponent<VerticalLayoutGroup>();
                if (group != null)
                {
                    group.spacing = 3f;
                    group.padding = new RectOffset(0, 0, 2, 2);
                }
            }

            if (closeButton != null)
            {
                var closeRect = closeButton.transform as RectTransform;
                if (closeRect != null)
                {
                    closeRect.anchorMin = Vector2.zero;
                    closeRect.anchorMax = Vector2.zero;
                    closeRect.pivot = Vector2.zero;
                    closeRect.anchoredPosition = new Vector2(276f, 12f);
                    closeRect.sizeDelta = new Vector2(132f, 44f);
                }
            }
        }

        private static Button CreateFilterButton(
            string objectName,
            string label,
            Transform parent,
            TMP_Text textTemplate)
        {
            var rect = CreateRect(objectName, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.18f, 0.21f, 0.96f);

            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
            colors.selectedColor = Color.white;
            button.colors = colors;

            var labelText = CreateText("Label", rect, textTemplate, label);
            Stretch(labelText.rectTransform, 4f, 4f, 2f, 2f);
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 16f;
            labelText.fontStyle = FontStyles.Bold;
            labelText.raycastTarget = false;

            return button;
        }

        private static TextMeshProUGUI CreateText(
            string name,
            Transform parent,
            TMP_Text template,
            string value)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.text = value ?? string.Empty;
            text.color = Color.white;
            text.fontSize = 17f;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;

            if (template != null && template.font != null)
                text.font = template.font;

            return text;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent != null ? parent.gameObject.layer : 5;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void AnchorTopStretch(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(0f, height);
        }
    }
}
