using System;
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
    /// P24UI.2 visual rebuild. Presentation only.
    /// Removes oversized ornamental buttons/cards and applies compact RTS styling.
    /// </summary>
    public static class RecruitmentPanelVisualPolishAuthoring
    {
        private const string ScenePath =
            "Assets/Moyva/Scenes/Gamplay_Scene.unity";

        private static readonly Color32 Root =
            new(81, 41, 24, 252);
        private static readonly Color32 Card =
            new(46, 31, 25, 244);
        private static readonly Color32 Card2 =
            new(54, 35, 27, 244);
        private static readonly Color32 ButtonNormal =
            new(137, 77, 47, 255);
        private static readonly Color32 ButtonHover =
            new(161, 96, 58, 255);
        private static readonly Color32 ButtonSelected =
            new(186, 117, 67, 255);
        private static readonly Color32 Hire =
            new(188, 109, 54, 255);
        private static readonly Color32 Text =
            new(244, 225, 199, 255);
        private static readonly Color32 Border =
            new(25, 16, 12, 190);

        public static string ApplyAndSave()
        {
            if (EditorApplication.isPlaying
                || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "P24UI2_ERROR_PLAY_MODE";
            }

            Scene scene = EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);

            Transform hud =
                RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel =
                RequireChild(hud, "RecruitmentPanel");

            GameplayTurnHudView turnView =
                hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view =
                panel.GetComponent<GameplayRecruitmentPanelView>();

            if (turnView == null || view == null)
                throw new InvalidOperationException(
                    "P24UI2 requires P24C recruitment views.");

            TMP_Text sampleText =
                RequireChild(
                    panel,
                    "RecipeViewport/RecipeSlots/RecipeSlot_01/Label")
                .GetComponent<TMP_Text>();

            SetFlat(
                panel.GetComponent<Image>(),
                Root,
                raycast: true);
            EnsureOutline(panel.gameObject, Border, 2f);

            Transform state =
                RequireChild(panel, "BuildingStatePanel");
            Transform available =
                RequireChild(panel, "AvailableUnitsCard");
            Transform selection =
                RequireChild(panel, "SelectionPanel");
            Transform queue =
                RequireChild(panel, "QueueCard");

            SetFlat(
                state.GetComponent<Image>(),
                Card2,
                raycast: false);
            SetFlat(
                available.GetComponent<Image>(),
                Card,
                raycast: false);
            SetFlat(
                selection.GetComponent<Image>(),
                Card,
                raycast: false);
            SetFlat(
                queue.GetComponent<Image>(),
                Card,
                raycast: false);

            EnsureOutline(state.gameObject, Border, 1f);
            EnsureOutline(available.gameObject, Border, 1f);
            EnsureOutline(selection.gameObject, Border, 1f);
            EnsureOutline(queue.gameObject, Border, 1f);

            StyleTitle(panel, sampleText);
            StyleState(panel);
            StyleHeader(
                RequireChild(
                    available,
                    "Header").GetComponent<TMP_Text>(),
                "Доступні");
            StyleHeader(
                RequireChild(
                    selection,
                    "DetailsHeader").GetComponent<TMP_Text>(),
                "Юніт");
            StyleQueue(panel);

            // Recipe buttons: compact flat rows, not ornamental tiles.
            for (int i = 1; i <= turnView.RecipeSlotCount; i++)
            {
                Transform row = RequireChild(
                    panel,
                    $"RecipeViewport/RecipeSlots/RecipeSlot_{i:00}");

                Button button = row.GetComponent<Button>();
                Image image = button != null
                    ? button.targetGraphic as Image
                    : row.GetComponent<Image>();

                SetFlat(image, Color.white, raycast: true);
                EnsureOutline(row.gameObject, Border, 1f);
                StyleButton(
                    button,
                    ButtonNormal,
                    ButtonHover,
                    ButtonSelected);

                TMP_Text label =
                    row.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.color = Text;
                    label.fontStyle = FontStyles.Normal;
                }
            }

            // Hire is the only strong CTA.
            if (view.HireButton != null)
            {
                Image image =
                    view.HireButton.targetGraphic as Image;
                SetFlat(image, Color.white, raycast: true);
                EnsureOutline(
                    view.HireButton.gameObject,
                    Border,
                    1f);
                StyleButton(
                    view.HireButton,
                    Hire,
                    new Color32(214, 133, 73, 255),
                    new Color32(224, 148, 82, 255));
            }

            for (int i = 1; i <= view.QueueRowCount; i++)
            {
                Transform row = RequireChild(
                    panel,
                    $"QueueRows/QueueRow_{i:00}");

                Button button = row.GetComponent<Button>();
                Image image = button != null
                    ? button.targetGraphic as Image
                    : row.GetComponent<Image>();

                SetFlat(image, Color.white, raycast: true);
                StyleButton(
                    button,
                    new Color32(73, 48, 36, 255),
                    new Color32(91, 58, 42, 255),
                    new Color32(107, 68, 47, 255));

                TMP_Text label =
                    row.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                    label.color = Text;
            }

            RecruitmentPanelVisualPolishLayout layout =
                panel.GetComponent<
                    RecruitmentPanelVisualPolishLayout>();

            if (layout == null)
            {
                layout = panel.gameObject.AddComponent<
                    RecruitmentPanelVisualPolishLayout>();
            }

            layout.ApplyNow();
            view.ValidateConfiguration(turnView.RecipeSlotCount);

            panel.gameObject.SetActive(false);

            EditorUtility.SetDirty(layout);
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            return
                $"P24UI2_AUTHORING_OK compact=True " +
                $"recipes={turnView.RecipeSlotCount} " +
                $"queueRows={view.QueueRowCount}";
        }

        public static string ValidateCurrent()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()
                || !scene.isLoaded
                || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(
                    ScenePath,
                    OpenSceneMode.Single);
            }

            Transform hud =
                RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel =
                RequireChild(hud, "RecruitmentPanel");

            GameplayTurnHudView turnView =
                hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view =
                panel.GetComponent<GameplayRecruitmentPanelView>();
            RecruitmentPanelVisualPolishLayout layout =
                panel.GetComponent<
                    RecruitmentPanelVisualPolishLayout>();

            if (turnView == null || view == null || layout == null)
                return "P24UI2_VALIDATE_FAIL missing-components";

            try
            {
                turnView.ValidateConfiguration();
                view.ValidateConfiguration(turnView.RecipeSlotCount);
            }
            catch (Exception ex)
            {
                return "P24UI2_VALIDATE_FAIL " + ex.Message;
            }

            return
                $"P24UI2_VALIDATE_OK recipes={turnView.RecipeSlotCount} " +
                $"queueRows={view.QueueRowCount}";
        }

        private static void StyleTitle(
            Transform panel,
            TMP_Text sample)
        {
            TMP_Text title =
                RequireChild(panel, "Title")
                    .GetComponent<TMP_Text>();

            if (title == null)
                return;

            if (sample != null)
            {
                title.font = sample.font;
                title.fontSharedMaterial =
                    sample.fontSharedMaterial;
            }

            title.text = "Найм";
            title.color = Text;
            title.fontStyle = FontStyles.Bold;
            title.enableAutoSizing = true;
            title.fontSizeMin = 14f;
            title.fontSizeMax = 18f;
            title.alignment =
                TextAlignmentOptions.MidlineLeft;
            title.textWrappingMode =
                TextWrappingModes.NoWrap;
        }

        private static void StyleState(Transform panel)
        {
            TMP_Text title =
                RequireChild(
                    panel,
                    "BuildingStatePanel/Title")
                .GetComponent<TMP_Text>();

            TMP_Text detail =
                RequireChild(
                    panel,
                    "BuildingStatePanel/Detail")
                .GetComponent<TMP_Text>();

            if (title != null)
            {
                title.color = Text;
                title.fontStyle = FontStyles.Bold;
                title.enableAutoSizing = true;
                title.fontSizeMin = 10.5f;
                title.fontSizeMax = 13f;
            }

            if (detail != null)
            {
                detail.color = Text;
                detail.enableAutoSizing = true;
                detail.fontSizeMin = 8.5f;
                detail.fontSizeMax = 10f;
                detail.overflowMode =
                    TextOverflowModes.Ellipsis;
            }
        }

        private static void StyleHeader(
            TMP_Text text,
            string value)
        {
            if (text == null)
                return;

            text.text = value;
            text.color = Text;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.fontSizeMin = 9.5f;
            text.fontSizeMax = 11f;
            text.alignment =
                TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode =
                TextWrappingModes.NoWrap;
        }

        private static void StyleQueue(Transform panel)
        {
            TMP_Text queue =
                RequireChild(panel, "Queue")
                    .GetComponent<TMP_Text>();

            if (queue == null)
                return;

            queue.color = Text;
            queue.fontStyle = FontStyles.Bold;
            queue.enableAutoSizing = true;
            queue.fontSizeMin = 9f;
            queue.fontSizeMax = 10.5f;
            queue.alignment =
                TextAlignmentOptions.MidlineLeft;
            queue.textWrappingMode =
                TextWrappingModes.NoWrap;
        }

        private static void StyleButton(
            Button button,
            Color normal,
            Color hover,
            Color selected)
        {
            if (button == null)
                return;

            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = hover;
            colors.pressedColor = selected;
            colors.selectedColor = selected;
            colors.disabledColor =
                new Color(
                    normal.r * 0.55f,
                    normal.g * 0.55f,
                    normal.b * 0.55f,
                    0.72f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
            button.transition =
                Selectable.Transition.ColorTint;
        }

        private static void SetFlat(
            Image image,
            Color color,
            bool raycast)
        {
            if (image == null)
                return;

            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = color;
            image.preserveAspect = false;
            image.raycastTarget = raycast;
        }

        private static void EnsureOutline(
            GameObject go,
            Color color,
            float distance)
        {
            if (go == null)
                return;

            Outline outline =
                go.GetComponent<Outline>()
                ?? go.AddComponent<Outline>();

            outline.effectColor = color;
            outline.effectDistance =
                new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static Transform RequirePath(
            Scene scene,
            string path)
        {
            string[] parts = path.Split('/');
            Transform current = null;

            foreach (GameObject root in
                     scene.GetRootGameObjects())
            {
                if (root.name == parts[0])
                {
                    current = root.transform;
                    break;
                }
            }

            if (current == null)
                throw new InvalidOperationException(
                    $"Missing {parts[0]}");

            for (int i = 1; i < parts.Length; i++)
                current = RequireChild(current, parts[i]);

            return current;
        }

        private static Transform RequireChild(
            Transform parent,
            string relative)
        {
            Transform child = parent.Find(relative);
            if (child == null)
                throw new InvalidOperationException(
                    $"Missing {parent.name}/{relative}");
            return child;
        }
    }
}
