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
    public static class GameplayTurnHudSceneAuthoring
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string HudRootName = "GameplayTurnHud";
        private const int RecipeSlotCount = 12;
        private const string LogTag = "[MOYVA_UI_CLI]";

        public static void ApplyFromCommandLine()
        {
            try
            {
                Apply();
                Debug.Log($"{LogTag} SUCCESS scene={ScenePath} root={HudRootName} slots={RecipeSlotCount}");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError($"{LogTag} FAILED {exception.Message}");
                throw;
            }
        }

        [MenuItem("Moyva/Gameplay/Author Scene Turn HUD")]
        public static void Apply()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException($"No scene Canvas found in '{ScenePath}'.");

            Transform existing = FindDirectChild(canvas.transform, HudRootName);
            if (existing != null)
                UnityEngine.Object.DestroyImmediate(existing.gameObject);

            GameObject root = CreateRect(HudRootName, canvas.transform);
            RectTransform rootRect = (RectTransform)root.transform;
            Stretch(rootRect);
            GameplayTurnHudView view = root.AddComponent<GameplayTurnHudView>();

            // Top-left compact turn card: deliberately avoids the authored economy resource
            // summary occupying the top-centre of the gameplay canvas.
            GameObject turnPanel = CreatePanel("TurnPanel", root.transform, new Color32(22, 27, 32, 236));
            RectTransform turnPanelRect = (RectTransform)turnPanel.transform;
            SetTopLeft(turnPanelRect, 24f, 24f, 600f, 118f);

            TMP_Text turnText = CreateText(
                "TurnSummary",
                turnPanel.transform,
                18f,
                TextAlignmentOptions.MidlineLeft,
                new Color32(241, 242, 236, 255));
            RectTransform turnTextRect = turnText.rectTransform;
            turnTextRect.anchorMin = new Vector2(0f, 0f);
            turnTextRect.anchorMax = new Vector2(1f, 1f);
            turnTextRect.pivot = new Vector2(0f, 0.5f);
            turnTextRect.offsetMin = new Vector2(16f, 34f);
            turnTextRect.offsetMax = new Vector2(-172f, -10f);
            turnText.textWrappingMode = TextWrappingModes.NoWrap;
            turnText.text = "Фракція: —    Раунд 1    Хід 1\nФаза —    Дії 0";

            Button endTurnButton = CreateButton(
                "EndTurnButton",
                turnPanel.transform,
                "Завершити хід",
                new Color32(55, 104, 85, 255));
            RectTransform endTurnRect = (RectTransform)endTurnButton.transform;
            endTurnRect.anchorMin = new Vector2(1f, 1f);
            endTurnRect.anchorMax = new Vector2(1f, 1f);
            endTurnRect.pivot = new Vector2(1f, 1f);
            endTurnRect.anchoredPosition = new Vector2(-12f, -12f);
            endTurnRect.sizeDelta = new Vector2(148f, 42f);

            TMP_Text statusText = CreateText(
                "TurnStatus",
                turnPanel.transform,
                14f,
                TextAlignmentOptions.MidlineLeft,
                new Color32(190, 216, 205, 255));
            RectTransform statusRect = statusText.rectTransform;
            statusRect.anchorMin = new Vector2(0f, 0f);
            statusRect.anchorMax = new Vector2(1f, 0f);
            statusRect.pivot = new Vector2(0f, 0f);
            statusRect.anchoredPosition = new Vector2(16f, 10f);
            statusRect.sizeDelta = new Vector2(-32f, 28f);
            statusText.text = "Ваш хід";

            // Unit selection readout stays in the bottom-left and does not block world input.
            GameObject unitPanel = CreatePanel("UnitStaminaPanel", root.transform, new Color32(22, 27, 32, 214));
            Image unitPanelImage = unitPanel.GetComponent<Image>();
            unitPanelImage.raycastTarget = false;
            RectTransform unitPanelRect = (RectTransform)unitPanel.transform;
            unitPanelRect.anchorMin = new Vector2(0f, 0f);
            unitPanelRect.anchorMax = new Vector2(0f, 0f);
            unitPanelRect.pivot = new Vector2(0f, 0f);
            unitPanelRect.anchoredPosition = new Vector2(24f, 24f);
            unitPanelRect.sizeDelta = new Vector2(410f, 48f);

            TMP_Text unitText = CreateText(
                "UnitStamina",
                unitPanel.transform,
                16f,
                TextAlignmentOptions.MidlineLeft,
                new Color32(241, 242, 236, 255));
            StretchWithPadding(unitText.rectTransform, 14f, 14f, 4f, 4f);
            unitText.text = string.Empty;

            // Recruitment is authored as a fixed scene hierarchy. Runtime only fills/activates
            // these slots; no GameObject/Component creation is performed by the presenter.
            GameObject recruitmentPanel = CreatePanel(
                "RecruitmentPanel",
                root.transform,
                new Color32(25, 30, 35, 246));
            RectTransform recruitmentRect = (RectTransform)recruitmentPanel.transform;
            recruitmentRect.anchorMin = new Vector2(1f, 0.5f);
            recruitmentRect.anchorMax = new Vector2(1f, 0.5f);
            recruitmentRect.pivot = new Vector2(1f, 0.5f);
            recruitmentRect.anchoredPosition = new Vector2(-24f, 0f);
            recruitmentRect.sizeDelta = new Vector2(360f, 500f);

            TMP_Text recruitmentTitle = CreateText(
                "Title",
                recruitmentPanel.transform,
                22f,
                TextAlignmentOptions.Center,
                new Color32(241, 242, 236, 255));
            SetTopStretch(recruitmentTitle.rectTransform, 12f, 12f, 10f, 42f);
            recruitmentTitle.text = "Найм юнітів";

            GameObject viewport = CreatePanel("RecipeViewport", recruitmentPanel.transform, new Color32(13, 17, 20, 80));
            RectTransform viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = new Vector2(0f, 0f);
            viewportRect.anchorMax = new Vector2(1f, 1f);
            viewportRect.offsetMin = new Vector2(12f, 138f);
            viewportRect.offsetMax = new Vector2(-12f, -62f);
            viewport.GetComponent<Image>().raycastTarget = true;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = CreateRect("RecipeSlots", viewport.transform);
            RectTransform contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, RecipeSlotCount * 46f + 8f);

            VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scroll = recruitmentPanel.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            var recipeButtons = new List<Button>(RecipeSlotCount);
            var recipeLabels = new List<TMP_Text>(RecipeSlotCount);
            for (int index = 0; index < RecipeSlotCount; index++)
            {
                Button button = CreateButton(
                    $"RecipeSlot_{index + 1:00}",
                    content.transform,
                    string.Empty,
                    new Color32(48, 82, 72, 255));
                LayoutElement element = button.gameObject.AddComponent<LayoutElement>();
                element.minHeight = 40f;
                element.preferredHeight = 40f;
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                button.gameObject.SetActive(false);
                recipeButtons.Add(button);
                recipeLabels.Add(label);
            }

            TMP_Text queueText = CreateText(
                "Queue",
                recruitmentPanel.transform,
                15f,
                TextAlignmentOptions.TopLeft,
                new Color32(225, 229, 221, 255));
            RectTransform queueRect = queueText.rectTransform;
            queueRect.anchorMin = new Vector2(0f, 0f);
            queueRect.anchorMax = new Vector2(1f, 0f);
            queueRect.pivot = new Vector2(0.5f, 0f);
            queueRect.anchoredPosition = new Vector2(0f, 12f);
            queueRect.sizeDelta = new Vector2(-28f, 112f);
            queueText.textWrappingMode = TextWrappingModes.Normal;
            queueText.text = string.Empty;

            ConfigureView(
                view,
                turnText,
                statusText,
                endTurnButton,
                unitText,
                recruitmentPanel,
                queueText,
                recipeButtons.ToArray(),
                recipeLabels.ToArray());

            GameplayTurnHudProjectStyler.ApplyToOpenScene(scene, canvas, root.transform);
            recruitmentPanel.SetActive(false);
            root.transform.SetAsLastSibling();
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            ValidateAuthoredScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ValidateFromCommandLine()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ValidateAuthoredScene(scene);
            Debug.Log($"{LogTag} VALIDATION SUCCESS scene={ScenePath}");
        }

        private static void ValidateAuthoredScene(Scene scene)
        {
            Canvas canvas = FindPrimaryCanvas(scene);
            if (canvas == null)
                throw new InvalidOperationException("Gameplay Canvas is missing after HUD authoring.");

            Transform root = FindDirectChild(canvas.transform, HudRootName);
            if (root == null)
                throw new InvalidOperationException($"Authored HUD root '{HudRootName}' is missing from the scene Canvas.");

            GameplayTurnHudView view = root.GetComponent<GameplayTurnHudView>();
            if (view == null)
                throw new InvalidOperationException("GameplayTurnHudView is missing from authored HUD root.");

            view.ValidateConfiguration();

            foreach (Canvas sceneCanvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (sceneCanvas != null && sceneCanvas.name == "Turn HUD (Runtime)")
                    throw new InvalidOperationException("Legacy runtime Turn HUD Canvas still exists in the authored scene.");
            }
        }

        private static Canvas FindPrimaryCanvas(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);
                foreach (Canvas canvas in canvases)
                {
                    if (canvas != null && canvas.name == "Canvas")
                        return canvas;
                }
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Canvas canvas = root.GetComponentInChildren<Canvas>(true);
                if (canvas != null)
                    return canvas;
            }

            return null;
        }

        private static Transform FindDirectChild(Transform parent, string childName)
        {
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == childName)
                    return child;
            }

            return null;
        }

        private static GameObject CreateRect(string name, Transform parent)
        {
            GameObject go = new(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return go;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Color color)
        {
            GameObject go = CreatePanel(name, parent, color);
            Image image = go.GetComponent<Image>();
            image.raycastTarget = true;
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;

            TMP_Text text = CreateText(
                "Label",
                go.transform,
                15f,
                TextAlignmentOptions.Center,
                new Color32(242, 241, 232, 255));
            StretchWithPadding(text.rectTransform, 8f, 8f, 2f, 2f);
            text.text = label;
            return button;
        }

        private static void ConfigureView(
            GameplayTurnHudView view,
            TMP_Text turnText,
            TMP_Text statusText,
            Button endTurnButton,
            TMP_Text unitText,
            GameObject recruitmentPanel,
            TMP_Text queueText,
            Button[] recipeButtons,
            TMP_Text[] recipeLabels)
        {
            SerializedObject serialized = new(view);
            serialized.FindProperty("_turnText").objectReferenceValue = turnText;
            serialized.FindProperty("_statusText").objectReferenceValue = statusText;
            serialized.FindProperty("_endTurnButton").objectReferenceValue = endTurnButton;
            serialized.FindProperty("_unitText").objectReferenceValue = unitText;
            serialized.FindProperty("_recruitmentPanel").objectReferenceValue = recruitmentPanel;
            serialized.FindProperty("_queueText").objectReferenceValue = queueText;
            AssignObjectArray(serialized.FindProperty("_recipeButtons"), recipeButtons);
            AssignObjectArray(serialized.FindProperty("_recipeLabels"), recipeLabels);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignObjectArray<T>(SerializedProperty property, T[] values)
            where T : UnityEngine.Object
        {
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }

        private static void StretchWithPadding(
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

        private static void SetTopLeft(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetTopStretch(
            RectTransform rect,
            float left,
            float right,
            float top,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -top);
            rect.sizeDelta = new Vector2(-(left + right), height);
        }
    }
}
