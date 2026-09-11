using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    /// <summary>
    /// Deterministic Unity-CLI migration for the Moyva PC gameplay HUD / construction UI.
    /// It changes only scene-side presentation and preserves existing gameplay controller bindings.
    /// </summary>
    public static class MoyvaGameplayUiPass74Installer
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string MarkerName = "MoyvaUiPass74Marker";

        private static readonly Color PanelDark = new Color(0.105f, 0.078f, 0.045f, 0.97f);
        private static readonly Color PanelMid = new Color(0.145f, 0.105f, 0.060f, 0.97f);
        private static readonly Color PanelSoft = new Color(0.205f, 0.145f, 0.075f, 0.98f);
        private static readonly Color PanelHover = new Color(0.265f, 0.185f, 0.090f, 1f);
        private static readonly Color PanelPressed = new Color(0.115f, 0.080f, 0.045f, 1f);
        private static readonly Color Gold = new Color(0.88f, 0.67f, 0.24f, 1f);
        private static readonly Color GoldSoft = new Color(0.73f, 0.54f, 0.19f, 0.78f);
        private static readonly Color Ivory = new Color(0.96f, 0.92f, 0.80f, 1f);
        private static readonly Color Muted = new Color(0.77f, 0.72f, 0.60f, 1f);

        public static void Apply()
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Require(scene, "Canvas");
                Require(scene, "Canvas/EconomyPlayerSymmary/Root/Top");
                Require(scene, "Canvas/GameModeUI/Build Button");
                Require(scene, "Canvas/ConstructionUI/Root");
                Require(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel");
                Require(scene, "Canvas/ConstructionUI/Root/ActionBar");
                Require(scene, "Canvas/ConstructionUI/Root/PreviewPanelInfo");

                StyleResourceBar(scene);
                StyleBuildButton(scene);
                StyleConstructionRoot(scene);
                StyleConstructionStatus(scene);
                StyleConstructionSelection(scene);
                StyleActionBar(scene);
                StyleCloseButton(scene);
                StylePreviewPanel(scene);
                StylePauseMenu(scene);
                EnsureMarker(scene);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unity refused to save Gamplay_Scene.unity.");

                AssetDatabase.SaveAssets();
                Debug.Log("[MoyvaUiPass74] APPLY PASS: gameplay HUD and construction UI saved.");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        public static void Validate()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();

            GameObject canvas = Find(scene, "Canvas");
            if (canvas == null)
                errors.Add("Canvas missing.");
            if (Find(scene, "Canvas/" + MarkerName) == null)
                errors.Add("Pass74 marker missing.");

            GameObject resourceBar = Find(scene, "Canvas/EconomyPlayerSymmary/Root/Top");
            RectTransform resourceRect = resourceBar != null ? resourceBar.GetComponent<RectTransform>() : null;
            if (resourceRect == null || resourceRect.sizeDelta.y < 38f || resourceRect.sizeDelta.y > 52f)
                errors.Add("Resource bar geometry is outside Pass74 contract.");

            GameObject build = Find(scene, "Canvas/GameModeUI/Build Button");
            RectTransform buildRect = build != null ? build.GetComponent<RectTransform>() : null;
            TMP_Text buildText = build != null ? build.GetComponentInChildren<TMP_Text>(true) : null;
            if (buildRect == null || buildRect.sizeDelta.x < 148f || buildRect.sizeDelta.y < 44f)
                errors.Add("Build button target is too small.");
            if (buildText == null || !buildText.text.Contains("Будувати", StringComparison.Ordinal))
                errors.Add("Build button is not player-facing Ukrainian UI.");

            GameObject selection = Find(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel");
            RectTransform selectionRect = selection != null ? selection.GetComponent<RectTransform>() : null;
            if (selectionRect == null || selectionRect.sizeDelta.y < 150f || selectionRect.sizeDelta.y > 210f)
                errors.Add("Construction catalogue height is outside compact Pass74 contract.");

            GameObject preview = Find(scene, "Canvas/ConstructionUI/Root/PreviewPanelInfo");
            RectTransform previewRect = preview != null ? preview.GetComponent<RectTransform>() : null;
            if (previewRect == null || previewRect.sizeDelta.x > 340f || previewRect.sizeDelta.y > 250f)
                errors.Add("Building info panel is not compact.");

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                    if (missing > 0)
                        errors.Add($"Missing script x{missing}: {HierarchyPath(transform)}");
                }
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("[MoyvaUiPass74] VALIDATION FAILED:\n" + string.Join("\n", errors));

            Debug.Log("[MoyvaUiPass74] VALIDATION PASS");
        }

        private static void StyleResourceBar(Scene scene)
        {
            GameObject top = Require(scene, "Canvas/EconomyPlayerSymmary/Root/Top");
            RectTransform rect = RequireRect(top);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -14f);
            rect.sizeDelta = new Vector2(500f, 44f);

            Image image = GetOrAdd<Image>(top);
            image.color = PanelDark;
            image.raycastTarget = false;
            EnsureOutline(top, GoldSoft, new Vector2(0f, -2f));

            HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(top);
            layout.padding = new RectOffset(12, 12, 4, 4);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            foreach (TMP_Text text in top.GetComponentsInChildren<TMP_Text>(true))
            {
                text.fontSize = 15f;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
                text.color = Ivory;
                text.raycastTarget = false;
                text.textWrappingMode = TextWrappingModes.NoWrap;

                LayoutElement element = GetOrAdd<LayoutElement>(text.gameObject);
                element.minWidth = 94f;
                element.preferredHeight = 34f;
                element.flexibleWidth = 1f;
            }
        }

        private static void StyleBuildButton(Scene scene)
        {
            GameObject go = Require(scene, "Canvas/GameModeUI/Build Button");
            RectTransform rect = RequireRect(go);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-22f, 22f);
            rect.sizeDelta = new Vector2(160f, 48f);

            Image image = GetOrAdd<Image>(go);
            image.color = PanelMid;
            image.raycastTarget = true;
            EnsureOutline(go, Gold, new Vector2(1f, -1f));
            StyleButton(go);

            GameObject icon = Find(scene, "Canvas/GameModeUI/Build Button/Icon");
            if (icon != null)
            {
                RectTransform iconRect = RequireRect(icon);
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(12f, 0f);
                iconRect.sizeDelta = new Vector2(24f, 24f);
                if (icon.TryGetComponent<Image>(out var iconImage)) iconImage.raycastTarget = false;
            }

            TMP_Text label = go.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(go.transform, false);
                label = labelObject.GetComponent<TextMeshProUGUI>();
            }
            RectTransform labelRect = RequireRect(label.gameObject);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(icon != null ? 40f : 8f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            label.text = "Будувати  [B]";
            label.fontSize = 15f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Ivory;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
        }

        private static void StyleConstructionRoot(Scene scene)
        {
            GameObject root = Require(scene, "Canvas/ConstructionUI/Root");
            RectTransform rect = RequireRect(root);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StyleConstructionStatus(Scene scene)
        {
            GameObject panel = Find(scene, "Canvas/ConstructionUI/Root/StatusPanel");
            if (panel == null) return;

            RectTransform rect = RequireRect(panel);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -64f);
            rect.sizeDelta = new Vector2(560f, 36f);

            Image image = GetOrAdd<Image>(panel);
            image.color = PanelDark;
            image.raycastTarget = false;
            EnsureOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.52f), new Vector2(0f, -1f));

            HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(panel);
            layout.padding = new RectOffset(10, 10, 3, 3);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].fontSize = 13f;
                texts[i].fontStyle = i == 0 ? FontStyles.Bold : FontStyles.Normal;
                texts[i].color = i == 0 ? Gold : Ivory;
                texts[i].alignment = TextAlignmentOptions.Center;
                texts[i].raycastTarget = false;
                texts[i].textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        private static void StyleConstructionSelection(Scene scene)
        {
            GameObject panel = Require(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel");
            RectTransform rect = RequireRect(panel);
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 18f);
            rect.sizeDelta = new Vector2(-48f, 176f);

            Image image = GetOrAdd<Image>(panel);
            image.color = PanelDark;
            image.raycastTarget = false;
            EnsureOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.58f), new Vector2(0f, 2f));

            GameObject tabs = Find(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel/CategoryTabs");
            if (tabs != null)
            {
                RectTransform tabsRect = RequireRect(tabs);
                tabsRect.anchorMin = new Vector2(0f, 1f);
                tabsRect.anchorMax = new Vector2(1f, 1f);
                tabsRect.pivot = new Vector2(0.5f, 1f);
                tabsRect.anchoredPosition = new Vector2(0f, -6f);
                tabsRect.sizeDelta = new Vector2(-24f, 38f);
                Image tabsImage = GetOrAdd<Image>(tabs);
                tabsImage.color = PanelMid;
                tabsImage.raycastTarget = false;

                HorizontalLayoutGroup tabLayout = tabs.GetComponentInChildren<HorizontalLayoutGroup>(true);
                if (tabLayout != null)
                {
                    tabLayout.padding = new RectOffset(6, 6, 4, 4);
                    tabLayout.spacing = 5f;
                    tabLayout.childAlignment = TextAnchor.MiddleLeft;
                    tabLayout.childControlWidth = false;
                    tabLayout.childControlHeight = false;
                    tabLayout.childForceExpandWidth = false;
                    tabLayout.childForceExpandHeight = false;
                }
            }

            GameObject viewport = Find(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport");
            if (viewport != null)
            {
                RectTransform viewportRect = RequireRect(viewport);
                viewportRect.anchorMin = Vector2.zero;
                viewportRect.anchorMax = Vector2.one;
                viewportRect.offsetMin = new Vector2(10f, 12f);
                viewportRect.offsetMax = new Vector2(-10f, -48f);
                Image viewportImage = GetOrAdd<Image>(viewport);
                viewportImage.color = new Color(0.035f, 0.026f, 0.017f, 0.52f);
                viewportImage.raycastTarget = true;
            }

            GameObject content = Find(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport/Content");
            if (content != null)
            {
                if (content.TryGetComponent<HorizontalLayoutGroup>(out var contentLayout))
                {
                    contentLayout.padding = new RectOffset(8, 8, 6, 6);
                    contentLayout.spacing = 7f;
                    contentLayout.childAlignment = TextAnchor.MiddleLeft;
                    contentLayout.childControlWidth = false;
                    contentLayout.childControlHeight = false;
                    contentLayout.childForceExpandWidth = false;
                    contentLayout.childForceExpandHeight = false;
                }
            }

            GameObject search = Find(scene, "Canvas/ConstructionUI/Root/BuildingSelectionPanel/Search");
            if (search != null)
            {
                RectTransform searchRect = RequireRect(search);
                searchRect.anchorMin = searchRect.anchorMax = new Vector2(1f, 1f);
                searchRect.pivot = new Vector2(1f, 1f);
                searchRect.anchoredPosition = new Vector2(-10f, -8f);
                searchRect.sizeDelta = new Vector2(220f, 32f);
                Image searchImage = GetOrAdd<Image>(search);
                searchImage.color = new Color(0.07f, 0.05f, 0.03f, 1f);
                EnsureOutline(search, new Color(Gold.r, Gold.g, Gold.b, 0.42f), Vector2.one);
                foreach (TMP_Text text in search.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.fontSize = 13f;
                    text.color = text.name.Contains("Placeholder", StringComparison.OrdinalIgnoreCase) ? Muted : Ivory;
                }
            }

            GameObject buildingTemplate = Find(scene, "Canvas/ConstructionUI/Root/Templates/BuildingButtonTemplate");
            if (buildingTemplate != null)
                StyleBuildingCardTemplate(buildingTemplate);

            GameObject tabTemplate = Find(scene, "Canvas/ConstructionUI/Root/Templates/CategoryTabButtonTemplate");
            if (tabTemplate != null)
                StyleTabTemplate(tabTemplate);
        }

        private static void StyleBuildingCardTemplate(GameObject card)
        {
            RectTransform rect = RequireRect(card);
            rect.sizeDelta = new Vector2(98f, 102f);
            Image image = GetOrAdd<Image>(card);
            image.color = PanelSoft;
            EnsureOutline(card, new Color(Gold.r, Gold.g, Gold.b, 0.46f), new Vector2(0f, -1f));
            StyleButton(card);

            TMP_Text label = card.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.fontSize = 13f;
                label.fontStyle = FontStyles.Bold;
                label.color = Ivory;
                label.alignment = TextAlignmentOptions.Center;
                label.textWrappingMode = TextWrappingModes.Normal;
                label.raycastTarget = false;
            }

            Image[] images = card.GetComponentsInChildren<Image>(true);
            foreach (Image childImage in images)
                if (childImage.gameObject != card) childImage.raycastTarget = false;
        }

        private static void StyleTabTemplate(GameObject tab)
        {
            RectTransform rect = RequireRect(tab);
            rect.sizeDelta = new Vector2(118f, 30f);
            Image image = GetOrAdd<Image>(tab);
            image.color = PanelSoft;
            EnsureOutline(tab, new Color(Gold.r, Gold.g, Gold.b, 0.36f), Vector2.one);
            StyleButton(tab);

            TMP_Text label = tab.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.fontSize = 13f;
                label.fontStyle = FontStyles.Bold;
                label.color = Ivory;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
                label.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        private static void StyleActionBar(Scene scene)
        {
            GameObject bar = Require(scene, "Canvas/ConstructionUI/Root/ActionBar");
            RectTransform rect = RequireRect(bar);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 202f);
            rect.sizeDelta = new Vector2(600f, 42f);

            Image image = GetOrAdd<Image>(bar);
            image.color = PanelDark;
            image.raycastTarget = false;
            EnsureOutline(bar, new Color(Gold.r, Gold.g, Gold.b, 0.44f), new Vector2(0f, 1f));

            HorizontalLayoutGroup layout = GetOrAdd<HorizontalLayoutGroup>(bar);
            layout.padding = new RectOffset(8, 8, 4, 4);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            foreach (Button button in bar.GetComponentsInChildren<Button>(true))
            {
                StyleButton(button.gameObject);
                LayoutElement element = GetOrAdd<LayoutElement>(button.gameObject);
                element.minWidth = 92f;
                element.preferredHeight = 34f;
                element.flexibleWidth = 1f;
                TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
                if (text != null)
                {
                    text.fontSize = 13f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = Ivory;
                    text.alignment = TextAlignmentOptions.Center;
                    text.raycastTarget = false;
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                }
            }
        }

        private static void StyleCloseButton(Scene scene)
        {
            GameObject close = Find(scene, "Canvas/ConstructionUI/Root/CloseButton");
            if (close == null) return;
            RectTransform rect = RequireRect(close);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-28f, 204f);
            rect.sizeDelta = new Vector2(40f, 36f);
            StyleButton(close);
            EnsureOutline(close, new Color(Gold.r, Gold.g, Gold.b, 0.52f), Vector2.one);
            TMP_Text text = close.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.fontSize = 13f;
                text.fontStyle = FontStyles.Bold;
                text.color = Ivory;
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
            }
        }

        private static void StylePreviewPanel(Scene scene)
        {
            GameObject panel = Require(scene, "Canvas/ConstructionUI/Root/PreviewPanelInfo");
            RectTransform rect = RequireRect(panel);
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-22f, 14f);
            rect.sizeDelta = new Vector2(310f, 220f);

            Image image = GetOrAdd<Image>(panel);
            image.color = PanelDark;
            image.raycastTarget = true;
            EnsureOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.62f), new Vector2(-2f, 0f));

            TMP_Text[] texts = panel.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                bool header = i == 0 || text.name.Contains("Label", StringComparison.OrdinalIgnoreCase) || text.name.Contains("Title", StringComparison.OrdinalIgnoreCase);
                text.fontSize = header ? 18f : 13f;
                text.fontStyle = header ? FontStyles.Bold : FontStyles.Normal;
                text.color = header ? Gold : Ivory;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.textWrappingMode = TextWrappingModes.Normal;
                text.overflowMode = TextOverflowModes.Ellipsis;
                text.raycastTarget = false;
                text.lineSpacing = header ? 0f : 4f;
            }
        }

        private static void StylePauseMenu(Scene scene)
        {
            GameObject pause = FindByNameContains(scene, "Pause");
            if (pause == null) return;
            if (pause.GetComponentsInChildren<Button>(true).Length < 2) return;

            RectTransform rect = pause.GetComponent<RectTransform>();
            if (rect != null && rect.sizeDelta.x > 0f)
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(Mathf.Clamp(rect.sizeDelta.x, 300f, 420f), Mathf.Clamp(rect.sizeDelta.y, 190f, 300f));
            }

            if (pause.TryGetComponent<Image>(out var image))
            {
                image.color = PanelDark;
                EnsureOutline(pause, new Color(Gold.r, Gold.g, Gold.b, 0.55f), Vector2.one);
            }

            foreach (Button button in pause.GetComponentsInChildren<Button>(true))
            {
                StyleButton(button.gameObject);
                TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
                if (text != null)
                {
                    text.fontSize = 14f;
                    text.fontStyle = FontStyles.Bold;
                    text.color = Ivory;
                }
            }
        }

        private static void EnsureMarker(Scene scene)
        {
            GameObject canvas = Require(scene, "Canvas");
            Transform marker = canvas.transform.Find(MarkerName);
            if (marker != null) return;
            GameObject markerObject = new GameObject(MarkerName, typeof(RectTransform));
            markerObject.transform.SetParent(canvas.transform, false);
            markerObject.SetActive(false);
        }

        private static void StyleButton(GameObject go)
        {
            Image image = GetOrAdd<Image>(go);
            if (image.color.a < 0.1f) image.color = PanelSoft;
            image.raycastTarget = true;

            if (!go.TryGetComponent<Button>(out var button)) return;
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = PanelSoft;
            colors.highlightedColor = PanelHover;
            colors.pressedColor = PanelPressed;
            colors.selectedColor = PanelHover;
            colors.disabledColor = new Color(PanelDark.r, PanelDark.g, PanelDark.b, 0.48f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
        }

        private static void EnsureOutline(GameObject go, Color color, Vector2 distance)
        {
            if (!go.TryGetComponent<Outline>(out var outline)) outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        private static RectTransform RequireRect(GameObject go)
        {
            if (!go.TryGetComponent<RectTransform>(out var rect))
                throw new InvalidOperationException("RectTransform missing: " + HierarchyPath(go.transform));
            return rect;
        }

        private static GameObject Require(Scene scene, string path)
        {
            GameObject result = Find(scene, path);
            if (result == null)
                throw new InvalidOperationException("Required UI object missing: " + path);
            return result;
        }

        private static GameObject Find(Scene scene, string path)
        {
            if (!scene.IsValid() || string.IsNullOrWhiteSpace(path)) return null;
            string[] parts = path.Split('/');
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(item => item.name == parts[0]);
            if (root == null) return null;
            Transform current = root.transform;
            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);
                if (current == null) return null;
            }
            return current.gameObject;
        }

        private static GameObject FindByNameContains(Scene scene, string value)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
                        return transform.gameObject;
                }
            }
            return null;
        }

        private static string HierarchyPath(Transform transform)
        {
            var names = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
