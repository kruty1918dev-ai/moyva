using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// P24B scene authoring for the existing GameplayTurnHud/RecruitmentPanel.
    /// Adds unit icons, a selected-unit detail card, explicit Hire button,
    /// resource-cost rows and clickable queue rows without replacing the panel root.
    /// </summary>
    public static class RecruitmentHudP24BAuthoring
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const int RecipeCount = 12;
        private const int QueueRowCount = 3;
        private const int CostRowCount = 3;

        public static string ApplyAndSave()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return "P24B_ERROR_PLAY_MODE";

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");
            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            if (turnView == null)
                throw new InvalidOperationException("GameplayTurnHudView missing on GameplayTurnHud.");

            GameplayRecruitmentPanelView enhanced = panel.GetComponent<GameplayRecruitmentPanelView>();
            if (enhanced == null)
                enhanced = panel.gameObject.AddComponent<GameplayRecruitmentPanelView>();

            TMP_Text sampleText = RequireChild(panel, "RecipeViewport/RecipeSlots/RecipeSlot_01/Label")
                .GetComponent<TMP_Text>();
            Button sampleButton = RequireChild(panel, "RecipeViewport/RecipeSlots/RecipeSlot_01")
                .GetComponent<Button>();
            if (sampleText == null || sampleButton == null)
                throw new InvalidOperationException("P24B sample recruitment slot is incomplete.");

            RectTransform viewport = RequireChild(panel, "RecipeViewport") as RectTransform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(16f, 234f);
            viewport.offsetMax = new Vector2(-16f, -58f);

            var recipeIcons = new List<Image>(RecipeCount);
            for (int i = 1; i <= RecipeCount; i++)
            {
                Transform slot = RequireChild(panel, $"RecipeViewport/RecipeSlots/RecipeSlot_{i:00}");
                LayoutElement layout = slot.GetComponent<LayoutElement>();
                if (layout == null)
                    layout = slot.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 54f;
                layout.preferredHeight = 54f;

                TMP_Text label = RequireChild(slot, "Label").GetComponent<TMP_Text>();
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(52f, 4f);
                labelRect.offsetMax = new Vector2(-8f, -4f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.enableAutoSizing = true;
                label.fontSizeMin = 9.5f;
                label.fontSizeMax = 12.5f;
                label.textWrappingMode = TextWrappingModes.Normal;

                Image icon = EnsureImage(slot, "UnitIcon");
                RectTransform iconRect = icon.rectTransform;
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = new Vector2(28f, 0f);
                iconRect.sizeDelta = new Vector2(36f, 36f);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                icon.enabled = false;
                recipeIcons.Add(icon);
            }

            GameObject selectionPanel = EnsurePanel(panel, "SelectionPanel", new Color32(24, 29, 33, 225));
            RectTransform selectionRect = (RectTransform)selectionPanel.transform;
            selectionRect.anchorMin = new Vector2(0f, 0f);
            selectionRect.anchorMax = new Vector2(1f, 0f);
            selectionRect.pivot = new Vector2(0.5f, 0f);
            selectionRect.anchoredPosition = new Vector2(0f, 112f);
            selectionRect.sizeDelta = new Vector2(-32f, 112f);

            Image selectionIcon = EnsureImage(selectionPanel.transform, "UnitIcon");
            RectTransform selectionIconRect = selectionIcon.rectTransform;
            selectionIconRect.anchorMin = new Vector2(0f, 0.5f);
            selectionIconRect.anchorMax = new Vector2(0f, 0.5f);
            selectionIconRect.pivot = new Vector2(0.5f, 0.5f);
            selectionIconRect.anchoredPosition = new Vector2(34f, 6f);
            selectionIconRect.sizeDelta = new Vector2(54f, 54f);
            selectionIcon.preserveAspect = true;
            selectionIcon.raycastTarget = false;

            TMP_Text selectionName = EnsureText(selectionPanel.transform, "UnitName", sampleText);
            SetRect(selectionName.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 1f), new Vector2(70f, -8f), new Vector2(-112f, 24f));
            selectionName.alignment = TextAlignmentOptions.MidlineLeft;
            selectionName.fontStyle = FontStyles.Bold;
            selectionName.fontSize = 15f;
            selectionName.enableAutoSizing = true;
            selectionName.fontSizeMin = 11f;
            selectionName.fontSizeMax = 15f;

            TMP_Text selectionStats = EnsureText(selectionPanel.transform, "UnitStats", sampleText);
            RectTransform statsRect = selectionStats.rectTransform;
            statsRect.anchorMin = new Vector2(0f, 0f);
            statsRect.anchorMax = new Vector2(1f, 1f);
            statsRect.offsetMin = new Vector2(70f, 32f);
            statsRect.offsetMax = new Vector2(-108f, -34f);
            selectionStats.alignment = TextAlignmentOptions.TopLeft;
            selectionStats.fontStyle = FontStyles.Normal;
            selectionStats.enableAutoSizing = true;
            selectionStats.fontSizeMin = 8.5f;
            selectionStats.fontSizeMax = 11f;
            selectionStats.textWrappingMode = TextWrappingModes.Normal;

            Button hireButton = EnsureButton(selectionPanel.transform, "HireButton", sampleButton);
            RectTransform hireRect = (RectTransform)hireButton.transform;
            hireRect.anchorMin = new Vector2(1f, 0f);
            hireRect.anchorMax = new Vector2(1f, 0f);
            hireRect.pivot = new Vector2(1f, 0f);
            hireRect.anchoredPosition = new Vector2(-8f, 8f);
            hireRect.sizeDelta = new Vector2(94f, 31f);
            TMP_Text hireLabel = EnsureText(hireButton.transform, "Label", sampleText);
            Stretch(hireLabel.rectTransform, 6f, 6f, 2f, 2f);
            hireLabel.text = "Найняти";
            hireLabel.alignment = TextAlignmentOptions.Center;
            hireLabel.fontStyle = FontStyles.Bold;
            hireLabel.fontSize = 11.5f;
            hireLabel.raycastTarget = false;

            Transform costs = EnsureRect(selectionPanel.transform, "Costs");
            RectTransform costsRect = (RectTransform)costs;
            costsRect.anchorMin = new Vector2(0f, 0f);
            costsRect.anchorMax = new Vector2(1f, 0f);
            costsRect.pivot = new Vector2(0.5f, 0f);
            costsRect.anchoredPosition = new Vector2(-18f, 5f);
            costsRect.sizeDelta = new Vector2(-142f, 24f);
            HorizontalLayoutGroup costsLayout = costs.GetComponent<HorizontalLayoutGroup>();
            if (costsLayout == null)
                costsLayout = costs.gameObject.AddComponent<HorizontalLayoutGroup>();
            costsLayout.spacing = 5f;
            costsLayout.padding = new RectOffset(0, 0, 0, 0);
            costsLayout.childAlignment = TextAnchor.MiddleLeft;
            costsLayout.childControlWidth = true;
            costsLayout.childControlHeight = true;
            costsLayout.childForceExpandWidth = true;
            costsLayout.childForceExpandHeight = true;

            var costRows = new List<GameObject>(CostRowCount);
            var costIcons = new List<Image>(CostRowCount);
            var costLabels = new List<TMP_Text>(CostRowCount);
            for (int i = 0; i < CostRowCount; i++)
            {
                GameObject row = EnsurePlainRect(costs, $"CostRow_{i + 1:00}");
                LayoutElement rowLayout = row.GetComponent<LayoutElement>();
                if (rowLayout == null)
                    rowLayout = row.AddComponent<LayoutElement>();
                rowLayout.minHeight = 22f;
                rowLayout.preferredHeight = 22f;

                Image icon = EnsureImage(row.transform, "Icon");
                RectTransform iconRect = icon.rectTransform;
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(1f, 0f);
                iconRect.sizeDelta = new Vector2(18f, 18f);
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                TMP_Text label = EnsureText(row.transform, "Label", sampleText);
                RectTransform lr = label.rectTransform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = new Vector2(22f, 0f);
                lr.offsetMax = Vector2.zero;
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.fontStyle = FontStyles.Normal;
                label.enableAutoSizing = true;
                label.fontSizeMin = 7.5f;
                label.fontSizeMax = 9.5f;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Ellipsis;
                label.raycastTarget = false;

                row.SetActive(false);
                costRows.Add(row);
                costIcons.Add(icon);
                costLabels.Add(label);
            }

            TMP_Text queueHeader = RequireChild(panel, "Queue").GetComponent<TMP_Text>();
            RectTransform queueHeaderRect = queueHeader.rectTransform;
            queueHeaderRect.anchorMin = new Vector2(0f, 0f);
            queueHeaderRect.anchorMax = new Vector2(1f, 0f);
            queueHeaderRect.pivot = new Vector2(0.5f, 0f);
            queueHeaderRect.anchoredPosition = new Vector2(0f, 87f);
            queueHeaderRect.sizeDelta = new Vector2(-32f, 20f);
            queueHeader.alignment = TextAlignmentOptions.MidlineLeft;
            queueHeader.fontSize = 11.5f;
            queueHeader.enableAutoSizing = true;
            queueHeader.fontSizeMin = 9.5f;
            queueHeader.fontSizeMax = 11.5f;
            queueHeader.textWrappingMode = TextWrappingModes.NoWrap;

            Transform queueRows = EnsureRect(panel, "QueueRows");
            RectTransform queueRowsRect = (RectTransform)queueRows;
            queueRowsRect.anchorMin = new Vector2(0f, 0f);
            queueRowsRect.anchorMax = new Vector2(1f, 0f);
            queueRowsRect.pivot = new Vector2(0.5f, 0f);
            queueRowsRect.anchoredPosition = new Vector2(0f, 12f);
            queueRowsRect.sizeDelta = new Vector2(-32f, 72f);
            VerticalLayoutGroup queueLayout = queueRows.GetComponent<VerticalLayoutGroup>();
            if (queueLayout == null)
                queueLayout = queueRows.gameObject.AddComponent<VerticalLayoutGroup>();
            queueLayout.spacing = 3f;
            queueLayout.padding = new RectOffset(0, 0, 0, 0);
            queueLayout.childAlignment = TextAnchor.UpperCenter;
            queueLayout.childControlWidth = true;
            queueLayout.childControlHeight = false;
            queueLayout.childForceExpandWidth = true;
            queueLayout.childForceExpandHeight = false;

            var queueButtons = new List<Button>(QueueRowCount);
            var queueIcons = new List<Image>(QueueRowCount);
            var queueLabels = new List<TMP_Text>(QueueRowCount);
            for (int i = 0; i < QueueRowCount; i++)
            {
                Button button = EnsureButton(queueRows, $"QueueRow_{i + 1:00}", sampleButton);
                LayoutElement layout = button.GetComponent<LayoutElement>();
                if (layout == null)
                    layout = button.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 22f;
                layout.preferredHeight = 22f;

                Image icon = EnsureImage(button.transform, "UnitIcon");
                RectTransform ir = icon.rectTransform;
                ir.anchorMin = new Vector2(0f, 0.5f);
                ir.anchorMax = new Vector2(0f, 0.5f);
                ir.pivot = new Vector2(0f, 0.5f);
                ir.anchoredPosition = new Vector2(4f, 0f);
                ir.sizeDelta = new Vector2(18f, 18f);
                icon.preserveAspect = true;
                icon.raycastTarget = false;

                TMP_Text label = EnsureText(button.transform, "Label", sampleText);
                RectTransform lr = label.rectTransform;
                lr.anchorMin = Vector2.zero;
                lr.anchorMax = Vector2.one;
                lr.offsetMin = new Vector2(28f, 1f);
                lr.offsetMax = new Vector2(-5f, -1f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.fontStyle = FontStyles.Normal;
                label.enableAutoSizing = true;
                label.fontSizeMin = 8.5f;
                label.fontSizeMax = 10.5f;
                label.textWrappingMode = TextWrappingModes.NoWrap;
                label.overflowMode = TextOverflowModes.Ellipsis;
                label.raycastTarget = false;

                button.gameObject.SetActive(false);
                queueButtons.Add(button);
                queueIcons.Add(icon);
                queueLabels.Add(label);
            }

            SerializedObject so = new SerializedObject(enhanced);
            AssignArray(so.FindProperty("_recipeIcons"), recipeIcons);
            so.FindProperty("_selectionPanel").objectReferenceValue = selectionPanel;
            so.FindProperty("_selectionIcon").objectReferenceValue = selectionIcon;
            so.FindProperty("_selectionName").objectReferenceValue = selectionName;
            so.FindProperty("_selectionStats").objectReferenceValue = selectionStats;
            so.FindProperty("_hireButton").objectReferenceValue = hireButton;
            so.FindProperty("_hireButtonLabel").objectReferenceValue = hireLabel;
            AssignArray(so.FindProperty("_costRows"), costRows);
            AssignArray(so.FindProperty("_costIcons"), costIcons);
            AssignArray(so.FindProperty("_costLabels"), costLabels);
            AssignArray(so.FindProperty("_queueButtons"), queueButtons);
            AssignArray(so.FindProperty("_queueIcons"), queueIcons);
            AssignArray(so.FindProperty("_queueLabels"), queueLabels);
            so.ApplyModifiedPropertiesWithoutUndo();

            enhanced.ValidateConfiguration(turnView.RecipeSlotCount);
            panel.gameObject.SetActive(false);
            EditorUtility.SetDirty(enhanced);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            return $"P24B_AUTHORING_OK recipes={recipeIcons.Count} costs={costRows.Count} queueRows={queueButtons.Count}";
        }

        public static string ValidateCurrent()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");
            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView enhanced = panel.GetComponent<GameplayRecruitmentPanelView>();
            if (turnView == null || enhanced == null)
                return "P24B_VALIDATE_FAIL missing-view";

            try
            {
                turnView.ValidateConfiguration();
                enhanced.ValidateConfiguration(turnView.RecipeSlotCount);
            }
            catch (Exception ex)
            {
                return "P24B_VALIDATE_FAIL " + ex.Message;
            }

            ScrollRect scroll = panel.GetComponent<ScrollRect>();
            bool scrollOk = scroll != null && scroll.viewport != null && scroll.content != null && scroll.vertical;
            return $"P24B_VALIDATE_OK recipes={enhanced.RecipeIconCount} costs={enhanced.CostRowCount} queueRows={enhanced.QueueRowCount} scroll={scrollOk}";
        }

        private static Transform RequirePath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            Transform current = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == parts[0])
                {
                    current = root.transform;
                    break;
                }
            }
            if (current == null)
                throw new InvalidOperationException($"Scene object missing: {parts[0]}");
            for (int i = 1; i < parts.Length; i++)
                current = RequireChild(current, parts[i]);
            return current;
        }

        private static Transform RequireChild(Transform parent, string relativePath)
        {
            Transform child = parent.Find(relativePath);
            if (child == null)
                throw new InvalidOperationException($"Scene object missing: {parent.name}/{relativePath}");
            return child;
        }

        private static Transform EnsureRect(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing;
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject EnsurePlainRect(Transform parent, string name)
            => EnsureRect(parent, name).gameObject;

        private static GameObject EnsurePanel(Transform parent, string name, Color color)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (existing == null)
                go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            if (image == null)
                image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static Image EnsureImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (existing == null)
                go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            if (image == null)
                image = go.AddComponent<Image>();
            return image;
        }

        private static TMP_Text EnsureText(Transform parent, string name, TMP_Text sample)
        {
            Transform existing = parent.Find(name);
            TMP_Text text;
            if (existing != null)
            {
                text = existing.GetComponent<TMP_Text>();
                if (text == null)
                    text = existing.gameObject.AddComponent<TextMeshProUGUI>();
            }
            else
            {
                GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                go.transform.SetParent(parent, false);
                text = go.GetComponent<TMP_Text>();
            }

            if (sample != null)
            {
                text.font = sample.font;
                text.fontSharedMaterial = sample.fontSharedMaterial;
                text.color = sample.color;
            }
            text.raycastTarget = false;
            return text;
        }

        private static Button EnsureButton(Transform parent, string name, Button sample)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            if (existing == null)
                go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            if (image == null)
                image = go.AddComponent<Image>();
            Button button = go.GetComponent<Button>();
            if (button == null)
                button = go.AddComponent<Button>();

            if (sample != null)
            {
                Image sampleImage = sample.targetGraphic as Image;
                if (sampleImage != null)
                {
                    image.sprite = sampleImage.sprite;
                    image.type = sampleImage.type;
                    image.color = sampleImage.color;
                }
                button.transition = sample.transition;
                button.colors = sample.colors;
                button.spriteState = sample.spriteState;
            }
            button.targetGraphic = image;
            return button;
        }

        private static void AssignArray<T>(SerializedProperty property, IList<T> values) where T : UnityEngine.Object
        {
            if (property == null)
                throw new InvalidOperationException("P24B serialized array field missing after compile.");
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static void Stretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }
    }
}
