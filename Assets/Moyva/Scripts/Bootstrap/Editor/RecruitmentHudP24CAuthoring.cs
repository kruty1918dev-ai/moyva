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
    /// P24C visual re-authoring for recruitment:
    /// - explicit building state
    /// - readable training/ready feedback
    /// - non-overlapping recipe/detail/queue sections
    /// - per-queue progress bars
    /// </summary>
    public static class RecruitmentHudP24CAuthoring
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const int QueueRowCount = 3;

        public static string ApplyAndSave()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return "P24C_ERROR_PLAY_MODE";

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");
            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view = panel.GetComponent<GameplayRecruitmentPanelView>();
            if (turnView == null || view == null)
                throw new InvalidOperationException("P24C requires P24B GameplayTurnHudView + GameplayRecruitmentPanelView.");

            TMP_Text sampleText = RequireChild(panel, "RecipeViewport/RecipeSlots/RecipeSlot_01/Label").GetComponent<TMP_Text>();
            Button sampleButton = RequireChild(panel, "RecipeViewport/RecipeSlots/RecipeSlot_01").GetComponent<Button>();
            if (sampleText == null || sampleButton == null)
                throw new InvalidOperationException("P24C sample recruitment visuals missing.");

            RectTransform panelRect = (RectTransform)panel;
            float oldWidth = panelRect.sizeDelta.x;
            const float targetWidth = 520f;
            if (oldWidth < targetWidth - 0.5f)
            {
                panelRect.anchoredPosition += new Vector2(-(targetWidth - oldWidth), 0f);
                panelRect.sizeDelta = new Vector2(targetWidth, panelRect.sizeDelta.y);
            }

            // Top state card.
            GameObject statePanel = EnsurePanel(panel, "BuildingStatePanel", sampleButton);
            RectTransform stateRect = (RectTransform)statePanel.transform;
            SetBottomStretch(stateRect, 16f, 16f, 418f, 56f);

            TMP_Text stateTitle = EnsureText(statePanel.transform, "Title", sampleText);
            SetStretch(stateTitle.rectTransform, 12f, 12f, 26f, 7f);
            stateTitle.alignment = TextAlignmentOptions.TopLeft;
            stateTitle.fontStyle = FontStyles.Bold;
            stateTitle.enableAutoSizing = true;
            stateTitle.fontSizeMin = 11f;
            stateTitle.fontSizeMax = 14.5f;
            stateTitle.textWrappingMode = TextWrappingModes.NoWrap;

            TMP_Text stateDetail = EnsureText(statePanel.transform, "Detail", sampleText);
            SetStretch(stateDetail.rectTransform, 12f, 12f, 8f, 26f);
            stateDetail.alignment = TextAlignmentOptions.BottomLeft;
            stateDetail.enableAutoSizing = true;
            stateDetail.fontSizeMin = 8.5f;
            stateDetail.fontSizeMax = 10.5f;
            stateDetail.textWrappingMode = TextWrappingModes.NoWrap;
            stateDetail.overflowMode = TextOverflowModes.Ellipsis;

            Image stateTrack = EnsureImage(statePanel.transform, "ProgressTrack");
            RectTransform stateTrackRect = stateTrack.rectTransform;
            stateTrackRect.anchorMin = new Vector2(0f, 0f);
            stateTrackRect.anchorMax = new Vector2(1f, 0f);
            stateTrackRect.offsetMin = new Vector2(12f, 4f);
            stateTrackRect.offsetMax = new Vector2(-12f, 9f);
            stateTrack.color = new Color32(48, 37, 29, 220);
            stateTrack.raycastTarget = false;

            Image stateFill = EnsureImage(stateTrack.transform, "Fill");
            Stretch(stateFill.rectTransform);
            stateFill.color = new Color32(111, 171, 88, 255);
            stateFill.type = Image.Type.Filled;
            stateFill.fillMethod = Image.FillMethod.Horizontal;
            stateFill.fillOrigin = 0;
            stateFill.fillAmount = 0f;
            stateFill.raycastTarget = false;

            // Recipe list: top section only. Prevents overlap with detail/queue.
            RectTransform viewport = (RectTransform)RequireChild(panel, "RecipeViewport");
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(16f, 260f);
            viewport.offsetMax = new Vector2(-16f, -132f);

            for (int i = 1; i <= turnView.RecipeSlotCount; i++)
            {
                Transform slot = RequireChild(panel, $"RecipeViewport/RecipeSlots/RecipeSlot_{i:00}");
                LayoutElement layout = slot.GetComponent<LayoutElement>() ?? slot.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 49f;
                layout.preferredHeight = 49f;

                TMP_Text label = RequireChild(slot, "Label").GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.rectTransform.offsetMin = new Vector2(54f, 4f);
                    label.rectTransform.offsetMax = new Vector2(-10f, -4f);
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 9.5f;
                    label.fontSizeMax = 12.5f;
                    label.alignment = TextAlignmentOptions.MidlineLeft;
                }

                Transform iconTr = slot.Find("UnitIcon");
                if (iconTr != null)
                {
                    RectTransform ir = (RectTransform)iconTr;
                    ir.anchoredPosition = new Vector2(29f, 0f);
                    ir.sizeDelta = new Vector2(36f, 36f);
                }
            }

            // Detail card.
            RectTransform selectionRect = (RectTransform)RequireChild(panel, "SelectionPanel");
            SetBottomStretch(selectionRect, 16f, 16f, 138f, 112f);

            RectTransform selectionIcon = (RectTransform)RequireChild(selectionRect, "UnitIcon");
            selectionIcon.anchorMin = selectionIcon.anchorMax = new Vector2(0f, 0.5f);
            selectionIcon.pivot = new Vector2(0.5f, 0.5f);
            selectionIcon.anchoredPosition = new Vector2(34f, 13f);
            selectionIcon.sizeDelta = new Vector2(52f, 52f);

            TMP_Text unitName = RequireChild(selectionRect, "UnitName").GetComponent<TMP_Text>();
            unitName.rectTransform.anchorMin = new Vector2(0f, 1f);
            unitName.rectTransform.anchorMax = new Vector2(1f, 1f);
            unitName.rectTransform.pivot = new Vector2(0f, 1f);
            unitName.rectTransform.anchoredPosition = new Vector2(70f, -8f);
            unitName.rectTransform.sizeDelta = new Vector2(-190f, 24f);
            unitName.enableAutoSizing = true;
            unitName.fontSizeMin = 11f;
            unitName.fontSizeMax = 15f;
            unitName.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_Text stats = RequireChild(selectionRect, "UnitStats").GetComponent<TMP_Text>();
            stats.rectTransform.anchorMin = Vector2.zero;
            stats.rectTransform.anchorMax = Vector2.one;
            stats.rectTransform.offsetMin = new Vector2(70f, 33f);
            stats.rectTransform.offsetMax = new Vector2(-124f, -33f);
            stats.alignment = TextAlignmentOptions.TopLeft;
            stats.enableAutoSizing = true;
            stats.fontSizeMin = 8.5f;
            stats.fontSizeMax = 10.5f;
            stats.textWrappingMode = TextWrappingModes.Normal;

            RectTransform costs = (RectTransform)RequireChild(selectionRect, "Costs");
            costs.anchorMin = new Vector2(0f, 0f);
            costs.anchorMax = new Vector2(1f, 0f);
            costs.pivot = new Vector2(0.5f, 0f);
            costs.anchoredPosition = new Vector2(-32f, 6f);
            costs.sizeDelta = new Vector2(-170f, 23f);

            RectTransform hire = (RectTransform)RequireChild(selectionRect, "HireButton");
            hire.anchorMin = hire.anchorMax = new Vector2(1f, 0f);
            hire.pivot = new Vector2(1f, 0f);
            hire.anchoredPosition = new Vector2(-8f, 7f);
            hire.sizeDelta = new Vector2(148f, 29f);

            TMP_Text hireLabel = RequireChild(hire, "Label").GetComponent<TMP_Text>();
            hireLabel.enableAutoSizing = true;
            hireLabel.fontSizeMin = 8.5f;
            hireLabel.fontSizeMax = 11.5f;
            hireLabel.textWrappingMode = TextWrappingModes.NoWrap;

            // Queue section.
            TMP_Text queueHeader = RequireChild(panel, "Queue").GetComponent<TMP_Text>();
            RectTransform qh = queueHeader.rectTransform;
            SetBottomStretch(qh, 16f, 16f, 114f, 20f);
            queueHeader.fontStyle = FontStyles.Bold;
            queueHeader.fontSize = 11.5f;
            queueHeader.enableAutoSizing = true;
            queueHeader.fontSizeMin = 9.5f;
            queueHeader.fontSizeMax = 11.5f;

            Transform queueRows = RequireChild(panel, "QueueRows");
            RectTransform qr = (RectTransform)queueRows;
            SetBottomStretch(qr, 16f, 16f, 8f, 102f);

            VerticalLayoutGroup queueLayout = queueRows.GetComponent<VerticalLayoutGroup>();
            if (queueLayout != null)
            {
                queueLayout.spacing = 3f;
                queueLayout.childControlHeight = false;
                queueLayout.childForceExpandHeight = false;
            }

            var progressFills = new List<Image>(QueueRowCount);
            for (int i = 1; i <= QueueRowCount; i++)
            {
                Transform row = RequireChild(queueRows, $"QueueRow_{i:00}");
                LayoutElement le = row.GetComponent<LayoutElement>() ?? row.gameObject.AddComponent<LayoutElement>();
                le.minHeight = 32f;
                le.preferredHeight = 32f;

                RectTransform rowIcon = (RectTransform)RequireChild(row, "UnitIcon");
                rowIcon.anchoredPosition = new Vector2(5f, 3f);
                rowIcon.sizeDelta = new Vector2(22f, 22f);

                TMP_Text rowLabel = RequireChild(row, "Label").GetComponent<TMP_Text>();
                rowLabel.rectTransform.offsetMin = new Vector2(31f, 5f);
                rowLabel.rectTransform.offsetMax = new Vector2(-6f, -2f);
                rowLabel.alignment = TextAlignmentOptions.MidlineLeft;
                rowLabel.enableAutoSizing = true;
                rowLabel.fontSizeMin = 7.8f;
                rowLabel.fontSizeMax = 10f;
                rowLabel.textWrappingMode = TextWrappingModes.Normal;
                rowLabel.overflowMode = TextOverflowModes.Ellipsis;

                Image track = EnsureImage(row, "ProgressTrack");
                RectTransform tr = track.rectTransform;
                tr.anchorMin = new Vector2(0f, 0f);
                tr.anchorMax = new Vector2(1f, 0f);
                tr.offsetMin = new Vector2(4f, 2f);
                tr.offsetMax = new Vector2(-4f, 5f);
                track.color = new Color32(46, 34, 26, 235);
                track.raycastTarget = false;

                Image fill = EnsureImage(track.transform, "Fill");
                Stretch(fill.rectTransform);
                fill.color = new Color32(111, 171, 88, 255);
                fill.type = Image.Type.Filled;
                fill.fillMethod = Image.FillMethod.Horizontal;
                fill.fillOrigin = 0;
                fill.fillAmount = 0f;
                fill.raycastTarget = false;
                progressFills.Add(fill);
            }

            SerializedObject so = new SerializedObject(view);
            so.FindProperty("_buildingStatePanel").objectReferenceValue = statePanel;
            so.FindProperty("_buildingStateTitle").objectReferenceValue = stateTitle;
            so.FindProperty("_buildingStateDetail").objectReferenceValue = stateDetail;
            so.FindProperty("_buildingStateProgressFill").objectReferenceValue = stateFill;
            AssignArray(so.FindProperty("_queueProgressFills"), progressFills);
            so.ApplyModifiedPropertiesWithoutUndo();

            view.ValidateConfiguration(turnView.RecipeSlotCount);
            panel.gameObject.SetActive(false);

            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            return $"P24C_AUTHORING_OK width={panelRect.sizeDelta.x:0.#} state=1 queueProgress={progressFills.Count}";
        }

        public static string ValidateCurrent()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");
            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view = panel.GetComponent<GameplayRecruitmentPanelView>();
            if (turnView == null || view == null)
                return "P24C_VALIDATE_FAIL missing-view";

            try
            {
                turnView.ValidateConfiguration();
                view.ValidateConfiguration(turnView.RecipeSlotCount);
            }
            catch (Exception ex)
            {
                return "P24C_VALIDATE_FAIL " + ex.Message;
            }

            RectTransform pr = (RectTransform)panel;
            return $"P24C_VALIDATE_OK width={pr.sizeDelta.x:0.#} " +
                   $"state={(view.BuildingStatePanel != null)} queueRows={view.QueueRowCount}";
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

        private static GameObject EnsurePanel(Transform parent, string name, Button sampleButton)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            if (existing == null)
                go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            if (sampleButton != null && sampleButton.targetGraphic is Image sample)
            {
                image.sprite = sample.sprite;
                image.type = sample.type;
                image.color = new Color32(73, 48, 31, 245);
            }
            image.raycastTarget = false;
            return go;
        }

        private static Image EnsureImage(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject go = existing != null
                ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

            if (existing == null)
                go.transform.SetParent(parent, false);

            return go.GetComponent<Image>() ?? go.AddComponent<Image>();
        }

        private static TMP_Text EnsureText(Transform parent, string name, TMP_Text sample)
        {
            Transform existing = parent.Find(name);
            TMP_Text text;

            if (existing != null)
            {
                text = existing.GetComponent<TMP_Text>() ?? existing.gameObject.AddComponent<TextMeshProUGUI>();
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

        private static void SetBottomStretch(RectTransform rect, float left, float right, float bottom, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottom);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }

        private static void SetStretch(RectTransform rect, float left, float right, float bottom, float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void AssignArray<T>(SerializedProperty property, IReadOnlyList<T> values)
            where T : UnityEngine.Object
        {
            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
