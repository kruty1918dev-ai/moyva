using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Bootstrap.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// Final compact RTS recruitment panel authoring.
    /// Rebuilds only Canvas/GameplayTurnHud/RecruitmentPanel.
    /// </summary>
    public static class RecruitmentPanelFinalAuthoring
    {
        private const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const int DefaultRecipeSlotCount = 12;
        private const int QueueRowCount = 3;
        private const int CostRowCount = 3;
        private const int StatRowCount = 6;

        private static readonly Color32 RootColor = new(55, 35, 28, 248);
        private static readonly Color32 PanelColor = new(36, 27, 23, 236);
        private static readonly Color32 PanelAltColor = new(47, 34, 27, 238);
        private static readonly Color32 RowColor = new(65, 45, 35, 235);
        private static readonly Color32 RowHoverColor = new(84, 58, 43, 245);
        private static readonly Color32 RowPressedColor = new(104, 73, 50, 250);
        private static readonly Color32 AccentColor = new(205, 145, 67, 255);
        private static readonly Color32 TextColor = new(244, 226, 199, 255);
        private static readonly Color32 SecondaryTextColor = new(187, 166, 137, 255);
        private static readonly Color32 MutedTextColor = new(143, 126, 105, 255);
        private static readonly Color32 ReadyColor = new(86, 176, 96, 255);
        private static readonly Color32 TrackColor = new(28, 22, 19, 230);
        private static readonly Color32 BorderColor = new(18, 12, 9, 190);

        public static string ApplyAndSave()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                return "FINAL_AUTHORING_ERROR_PLAY_MODE";

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");

            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view =
                panel.GetComponent<GameplayRecruitmentPanelView>()
                ?? panel.gameObject.AddComponent<GameplayRecruitmentPanelView>();

            if (turnView == null)
                throw new InvalidOperationException("GameplayTurnHudView is missing.");

            TMP_Text sampleText = panel.GetComponentInChildren<TMP_Text>(true)
                ?? hud.GetComponentInChildren<TMP_Text>(true);
            int recipeSlotCount = Mathf.Clamp(
                turnView.RecipeSlotCount > 0 ? turnView.RecipeSlotCount : DefaultRecipeSlotCount,
                3,
                DefaultRecipeSlotCount);

            RebuildPanel(panel, sampleText, recipeSlotCount, out AuthoredRefs refs);

            SerializedObject turnSo = new(turnView);
            SetObject(turnSo, "_recruitmentPanel", panel.gameObject);
            SetObject(turnSo, "_queueText", refs.QueueCapacity);
            AssignArray(turnSo, "_recipeButtons", refs.RecipeButtons);
            AssignArray(turnSo, "_recipeLabels", refs.RecipeNameTexts);
            turnSo.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject viewSo = new(view);
            SetObject(viewSo, "_buildingName", refs.BuildingName);
            SetObject(viewSo, "_subtitle", refs.Subtitle);
            SetObject(viewSo, "_stateChip", refs.StatusChip);
            SetObject(viewSo, "_stateChipLabel", refs.StatusChipLabel);
            SetObject(viewSo, "_closeButton", refs.CloseButton);
            AssignArray(viewSo, "_recipeIcons", refs.RecipeIcons);
            AssignArray(viewSo, "_recipeNameTexts", refs.RecipeNameTexts);
            AssignArray(viewSo, "_recipeTrainingTexts", refs.RecipeTrainingTexts);
            AssignArray(viewSo, "_recipeSelectedAccents", refs.RecipeSelectedAccents);
            SetObject(viewSo, "_selectionPanel", refs.DetailsPanel.gameObject);
            SetObject(viewSo, "_selectionIcon", refs.SelectionIcon);
            SetObject(viewSo, "_selectionName", refs.SelectionName);
            SetObject(viewSo, "_selectionClass", refs.SelectionClass);
            SetObject(viewSo, "_selectionStats", refs.LegacyStats);
            AssignArray(viewSo, "_statRows", refs.StatRows);
            AssignArray(viewSo, "_statLabels", refs.StatLabels);
            AssignArray(viewSo, "_statValues", refs.StatValues);
            SetObject(viewSo, "_hireButton", refs.HireButton);
            SetObject(viewSo, "_hireButtonLabel", refs.HireButtonLabel);
            SetObject(viewSo, "_actionHint", refs.ActionHint);
            AssignArray(viewSo, "_costRows", refs.CostRows);
            AssignArray(viewSo, "_costIcons", refs.CostIcons);
            AssignArray(viewSo, "_costLabels", refs.CostLabels);
            SetObject(viewSo, "_buildingStatePanel", refs.StatusStrip.gameObject);
            SetObject(viewSo, "_buildingStateTitle", refs.StatusTitle);
            SetObject(viewSo, "_buildingStateDetail", refs.StatusDetail);
            SetObject(viewSo, "_buildingStateProgressFill", refs.StatusFill);
            SetObject(viewSo, "_queueSection", refs.QueueSection.gameObject);
            SetObject(viewSo, "_queueTitle", refs.QueueTitle);
            SetObject(viewSo, "_queueCapacity", refs.QueueCapacity);
            SetObject(viewSo, "_queueEmptyText", refs.QueueEmptyText);
            AssignArray(viewSo, "_queueButtons", refs.QueueButtons);
            AssignArray(viewSo, "_queueIcons", refs.QueueIcons);
            AssignArray(viewSo, "_queueLabels", refs.QueueLabels);
            AssignArray(viewSo, "_queueProgressFills", refs.QueueProgressFills);
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            RecruitmentPanelVisualPolishLayout layout =
                panel.GetComponent<RecruitmentPanelVisualPolishLayout>()
                ?? panel.gameObject.AddComponent<RecruitmentPanelVisualPolishLayout>();
            bool previousActive = panel.gameObject.activeSelf;
            panel.gameObject.SetActive(true);
            layout.ApplyNow();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel);
            panel.gameObject.SetActive(false);

            turnView.ValidateConfiguration();
            view.ValidateConfiguration(recipeSlotCount);

            EditorUtility.SetDirty(turnView);
            EditorUtility.SetDirty(view);
            EditorUtility.SetDirty(layout);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            return $"FINAL_AUTHORING_OK previousActive={previousActive} recipes={recipeSlotCount} queueRows={QueueRowCount} costRows={CostRowCount} statRows={StatRowCount}";
        }

        public static string ValidateCurrent()
        {
            Scene scene = EnsureScene();
            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");

            GameplayTurnHudView turnView = hud.GetComponent<GameplayTurnHudView>();
            GameplayRecruitmentPanelView view = panel.GetComponent<GameplayRecruitmentPanelView>();
            RecruitmentPanelVisualPolishLayout layout =
                panel.GetComponent<RecruitmentPanelVisualPolishLayout>();

            if (turnView == null || view == null || layout == null)
                return "FINAL_VALIDATE_FAIL missing-components";

            try
            {
                turnView.ValidateConfiguration();
                view.ValidateConfiguration(turnView.RecipeSlotCount);
            }
            catch (Exception ex)
            {
                return "FINAL_VALIDATE_FAIL refs " + ex.Message;
            }

            bool previousActive = panel.gameObject.activeSelf;
            panel.gameObject.SetActive(true);
            layout.ApplyNow();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel);

            ValidationReport report = ValidateGeometry(hud as RectTransform, panel as RectTransform, view);

            panel.gameObject.SetActive(previousActive);

            int layoutOwnerCount = panel.GetComponents<RecruitmentPanelVisualPolishLayout>().Length;
            int readyPresenterSources = CountSourceOccurrences(
                "Assets/Moyva/Scripts/Bootstrap/Runtime/UnitRecruitmentReadyIndicatorPresenter.cs",
                "internal sealed class UnitRecruitmentReadyIndicatorPresenter");
            int readyPresenterBindings = CountSourceOccurrences(
                "Assets/Moyva/Scripts/Bootstrap/Runtime/BootstrapInstaller.cs",
                "UnitRecruitmentReadyIndicatorPresenter");
            string simulated = SimulateSizes();

            return report.Pass
                ? $"FINAL_VALIDATE_OK layoutOwnerCount={layoutOwnerCount} p24dReadyPresenterSources={readyPresenterSources} p24dReadyPresenterBindings={readyPresenterBindings} {report} {simulated}"
                : $"FINAL_VALIDATE_FAIL layoutOwnerCount={layoutOwnerCount} p24dReadyPresenterSources={readyPresenterSources} p24dReadyPresenterBindings={readyPresenterBindings} {report} {simulated}";
        }

        public static string DumpCurrent(string outputPath)
        {
            Scene scene = EnsureScene();
            Transform canvas = RequirePath(scene, "Canvas");
            Transform hud = RequirePath(scene, "Canvas/GameplayTurnHud");
            Transform panel = RequireChild(hud, "RecruitmentPanel");

            var sb = new StringBuilder();
            sb.AppendLine($"scene={scene.path}");
            DumpTransform(sb, canvas, includeChildren: false);
            DumpTransform(sb, hud, includeChildren: false);
            DumpTransform(sb, panel, includeChildren: true);
            sb.AppendLine("GameplayTurnHudViewRefs=" + RefState(hud.GetComponent<GameplayTurnHudView>()));
            sb.AppendLine("GameplayRecruitmentPanelViewRefs=" + RefState(panel.GetComponent<GameplayRecruitmentPanelView>()));

            System.IO.File.WriteAllText(outputPath, sb.ToString());
            return outputPath;
        }

        private static void RebuildPanel(
            Transform panel,
            TMP_Text sampleText,
            int recipeSlotCount,
            out AuthoredRefs refs)
        {
            refs = new AuthoredRefs();
            ClearChildren(panel);
            PreparePanelRoot(panel.gameObject);

            Image rootImage = panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>();
            SetFlat(rootImage, RootColor, raycast: true);
            EnsureOutline(panel.gameObject, BorderColor, 2f);

            VerticalLayoutGroup rootLayout =
                panel.GetComponent<VerticalLayoutGroup>() ?? panel.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(12, 12, 10, 10);
            rootLayout.spacing = 8f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            refs.Header = Section(panel, "Header", PanelAltColor);
            AddLayout(refs.Header, preferredHeight: 42f);
            HorizontalLayoutGroup headerLayout = refs.Header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(10, 8, 6, 6);
            headerLayout.spacing = 8f;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            RectTransform nameBlock = Empty(refs.Header, "NameBlock");
            AddLayout(nameBlock, preferredWidth: 0f, flexibleWidth: 1f);
            VerticalLayoutGroup nameLayout = nameBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            nameLayout.spacing = 0f;
            nameLayout.childControlWidth = true;
            nameLayout.childControlHeight = true;
            nameLayout.childForceExpandWidth = true;
            nameLayout.childForceExpandHeight = false;

            refs.BuildingName = Text(nameBlock, "BuildingName", "Казарма", sampleText, 16f, TextColor, TextAlignmentOptions.MidlineLeft);
            refs.BuildingName.fontStyle = FontStyles.Bold;
            AddLayout(refs.BuildingName.rectTransform, preferredHeight: 21f);
            refs.Subtitle = Text(nameBlock, "Subtitle", "Найм військ", sampleText, 11f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(refs.Subtitle.rectTransform, preferredHeight: 15f);

            refs.StatusChip = ImageObject(refs.Header, "StateChip", ReadyColor);
            AddLayout(refs.StatusChip.transform as RectTransform, preferredWidth: 82f, preferredHeight: 24f);
            refs.StatusChipLabel = Text(refs.StatusChip.transform, "Label", "ВІЛЬНА", sampleText, 10.5f, Color.white, TextAlignmentOptions.Center);
            Stretch(refs.StatusChipLabel.rectTransform, 4f, 4f, 2f, 2f);

            refs.CloseButton = Button(refs.Header, "CloseButton", "X", sampleText, new Color32(80, 54, 42, 255));
            AddLayout(refs.CloseButton.transform as RectTransform, preferredWidth: 28f, preferredHeight: 28f);

            refs.StatusStrip = Section(panel, "StateStrip", PanelAltColor);
            AddLayout(refs.StatusStrip, preferredHeight: 52f);
            VerticalLayoutGroup stateLayout = refs.StatusStrip.gameObject.AddComponent<VerticalLayoutGroup>();
            stateLayout.padding = new RectOffset(10, 10, 6, 6);
            stateLayout.spacing = 3f;
            stateLayout.childControlWidth = true;
            stateLayout.childControlHeight = true;
            stateLayout.childForceExpandWidth = true;
            stateLayout.childForceExpandHeight = false;

            refs.StatusTitle = Text(refs.StatusStrip, "StateTitle", "Вільна", sampleText, 13f, TextColor, TextAlignmentOptions.MidlineLeft);
            refs.StatusTitle.fontStyle = FontStyles.Bold;
            AddLayout(refs.StatusTitle.rectTransform, preferredHeight: 17f);
            refs.StatusDetail = Text(refs.StatusStrip, "StateDetail", "Черга 0/3", sampleText, 10.5f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(refs.StatusDetail.rectTransform, preferredHeight: 16f);
            Image stateTrack = Image(refs.StatusStrip, "ProgressTrack", TrackColor);
            AddLayout(stateTrack.rectTransform, preferredHeight: 5f);
            refs.StatusFill = Image(stateTrack.transform, "Fill", ReadyColor);
            refs.StatusFill.type = UnityEngine.UI.Image.Type.Filled;
            refs.StatusFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            refs.StatusFill.fillOrigin = 0;
            refs.StatusFill.fillAmount = 0f;
            Stretch(refs.StatusFill.rectTransform);

            refs.MainContent = Empty(panel, "MainContent");
            AddLayout(refs.MainContent, flexibleHeight: 1f);
            HorizontalLayoutGroup mainLayout = refs.MainContent.gameObject.AddComponent<HorizontalLayoutGroup>();
            mainLayout.spacing = 10f;
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = false;
            mainLayout.childForceExpandHeight = true;

            refs.Catalog = Section(refs.MainContent, "Catalog", PanelColor);
            AddLayout(refs.Catalog, preferredWidth: 198f);
            VerticalLayoutGroup catalogLayout = refs.Catalog.gameObject.AddComponent<VerticalLayoutGroup>();
            catalogLayout.padding = new RectOffset(8, 8, 8, 8);
            catalogLayout.spacing = 6f;
            catalogLayout.childControlWidth = true;
            catalogLayout.childControlHeight = true;
            catalogLayout.childForceExpandWidth = true;
            catalogLayout.childForceExpandHeight = false;

            TMP_Text catalogHeader = Text(refs.Catalog, "Header", "Доступні", sampleText, 11f, TextColor, TextAlignmentOptions.MidlineLeft);
            catalogHeader.fontStyle = FontStyles.Bold;
            AddLayout(catalogHeader.rectTransform, preferredHeight: 18f);

            RectTransform recipeScroll = Empty(refs.Catalog, "RecipeScroll", typeof(CanvasRenderer), typeof(Image), typeof(Mask), typeof(ScrollRect));
            AddLayout(recipeScroll, flexibleHeight: 1f);
            SetFlat(recipeScroll.GetComponent<Image>(), new Color32(24, 19, 17, 120), raycast: true);
            recipeScroll.GetComponent<Mask>().showMaskGraphic = false;
            ScrollRect scroll = recipeScroll.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 18f;
            refs.RecipeSlots = Empty(recipeScroll, "RecipeSlots");
            refs.RecipeSlots.anchorMin = new Vector2(0f, 1f);
            refs.RecipeSlots.anchorMax = new Vector2(1f, 1f);
            refs.RecipeSlots.pivot = new Vector2(0.5f, 1f);
            refs.RecipeSlots.anchoredPosition = Vector2.zero;
            refs.RecipeSlots.sizeDelta = Vector2.zero;
            scroll.viewport = recipeScroll;
            scroll.content = refs.RecipeSlots;
            VerticalLayoutGroup recipeLayout = refs.RecipeSlots.gameObject.AddComponent<VerticalLayoutGroup>();
            recipeLayout.spacing = 5f;
            recipeLayout.childControlWidth = true;
            recipeLayout.childControlHeight = false;
            recipeLayout.childForceExpandWidth = true;
            recipeLayout.childForceExpandHeight = false;
            ContentSizeFitter recipeFitter = refs.RecipeSlots.gameObject.AddComponent<ContentSizeFitter>();
            recipeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            for (int i = 0; i < recipeSlotCount; i++)
                BuildRecipeSlot(refs, sampleText, i);

            refs.DetailsPanel = Section(refs.MainContent, "Details", PanelColor);
            AddLayout(refs.DetailsPanel, flexibleWidth: 1f);
            BuildDetails(refs, sampleText);

            refs.QueueSection = Section(panel, "QueueSection", PanelColor);
            AddLayout(refs.QueueSection, preferredHeight: 38f);
            BuildQueue(refs, sampleText);
        }

        private static void BuildRecipeSlot(
            AuthoredRefs refs,
            TMP_Text sampleText,
            int index)
        {
            RectTransform row = Empty(
                refs.RecipeSlots,
                $"RecipeSlot_{index + 1:00}",
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            AddLayout(row, preferredHeight: 44f);
            Image rowImage = row.GetComponent<Image>();
            SetFlat(rowImage, RowColor, raycast: true);
            StyleButton(row.GetComponent<Button>(), rowImage, RowColor, RowHoverColor, RowPressedColor);

            GameObject accent = ImageObject(row, "SelectedAccent", AccentColor);
            RectTransform accentRect = accent.transform as RectTransform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(3f, 0f);
            accent.SetActive(false);

            Image icon = Image(row, "UnitIcon", new Color32(255, 255, 255, 255));
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(10f, 0f);
            iconRect.sizeDelta = new Vector2(30f, 30f);

            TMP_Text name = Text(row, "UnitName", string.Empty, sampleText, 12f, TextColor, TextAlignmentOptions.MidlineLeft);
            name.enableAutoSizing = true;
            name.fontSizeMin = 10f;
            name.fontSizeMax = 12.5f;
            name.overflowMode = TextOverflowModes.Ellipsis;
            Stretch(name.rectTransform, 48f, 46f, 4f, 4f);

            TMP_Text turns = Text(row, "TrainingTurns", string.Empty, sampleText, 11f, SecondaryTextColor, TextAlignmentOptions.MidlineRight);
            turns.enableAutoSizing = true;
            turns.fontSizeMin = 9f;
            turns.fontSizeMax = 11.5f;
            RectTransform turnsRect = turns.rectTransform;
            turnsRect.anchorMin = new Vector2(1f, 0f);
            turnsRect.anchorMax = new Vector2(1f, 1f);
            turnsRect.pivot = new Vector2(1f, 0.5f);
            turnsRect.anchoredPosition = new Vector2(-8f, 0f);
            turnsRect.sizeDelta = new Vector2(38f, -8f);

            refs.RecipeButtons.Add(row.GetComponent<Button>());
            refs.RecipeIcons.Add(icon);
            refs.RecipeNameTexts.Add(name);
            refs.RecipeTrainingTexts.Add(turns);
            refs.RecipeSelectedAccents.Add(accent);
        }

        private static void BuildDetails(AuthoredRefs refs, TMP_Text sampleText)
        {
            VerticalLayoutGroup layout = refs.DetailsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TMP_Text header = Text(refs.DetailsPanel, "Header", "Юніт", sampleText, 11f, TextColor, TextAlignmentOptions.MidlineLeft);
            header.fontStyle = FontStyles.Bold;
            AddLayout(header.rectTransform, preferredHeight: 18f);

            RectTransform unitHeader = Empty(refs.DetailsPanel, "UnitHeader");
            AddLayout(unitHeader, preferredHeight: 64f);
            HorizontalLayoutGroup unitHeaderLayout = unitHeader.gameObject.AddComponent<HorizontalLayoutGroup>();
            unitHeaderLayout.spacing = 8f;
            unitHeaderLayout.childControlWidth = true;
            unitHeaderLayout.childControlHeight = true;
            unitHeaderLayout.childForceExpandWidth = false;
            unitHeaderLayout.childForceExpandHeight = true;

            refs.SelectionIcon = Image(unitHeader, "UnitIcon", new Color32(255, 255, 255, 255));
            AddLayout(refs.SelectionIcon.rectTransform, preferredWidth: 64f, preferredHeight: 64f);

            RectTransform nameBlock = Empty(unitHeader, "NameBlock");
            AddLayout(nameBlock, flexibleWidth: 1f);
            VerticalLayoutGroup nameLayout = nameBlock.gameObject.AddComponent<VerticalLayoutGroup>();
            nameLayout.spacing = 2f;
            nameLayout.childControlWidth = true;
            nameLayout.childControlHeight = true;
            nameLayout.childForceExpandWidth = true;
            nameLayout.childForceExpandHeight = false;

            refs.SelectionName = Text(nameBlock, "UnitName", string.Empty, sampleText, 17f, TextColor, TextAlignmentOptions.MidlineLeft);
            refs.SelectionName.fontStyle = FontStyles.Bold;
            refs.SelectionName.enableAutoSizing = true;
            refs.SelectionName.fontSizeMin = 12f;
            refs.SelectionName.fontSizeMax = 17f;
            AddLayout(refs.SelectionName.rectTransform, preferredHeight: 26f);
            refs.SelectionClass = Text(nameBlock, "UnitClass", string.Empty, sampleText, 11f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(refs.SelectionClass.rectTransform, preferredHeight: 18f);

            refs.LegacyStats = Text(refs.DetailsPanel, "LegacyStats", string.Empty, sampleText, 1f, Color.clear, TextAlignmentOptions.TopLeft);
            refs.LegacyStats.gameObject.SetActive(false);

            RectTransform stats = Empty(refs.DetailsPanel, "Stats");
            AddLayout(stats, flexibleHeight: 1f);
            VerticalLayoutGroup statsLayout = stats.gameObject.AddComponent<VerticalLayoutGroup>();
            statsLayout.spacing = 3f;
            statsLayout.childControlWidth = true;
            statsLayout.childControlHeight = true;
            statsLayout.childForceExpandWidth = true;
            statsLayout.childForceExpandHeight = false;

            string[] statNames =
            {
                "HitPointsRow",
                "StaminaRow",
                "VisionRow",
                "DamageRow",
                "DefenseRow",
                "TrainingRow"
            };

            for (int i = 0; i < StatRowCount; i++)
            {
                RectTransform row = Empty(stats, statNames[i]);
                AddLayout(row, preferredHeight: 19f);
                row.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 19f);
                HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                rowLayout.spacing = 8f;
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = true;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = true;

                TMP_Text label = Text(row, "Label", string.Empty, sampleText, 10.5f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
                AddLayout(label.rectTransform, preferredWidth: 110f);
                TMP_Text value = Text(row, "Value", string.Empty, sampleText, 11f, TextColor, TextAlignmentOptions.MidlineRight);
                AddLayout(value.rectTransform, flexibleWidth: 1f);
                row.gameObject.SetActive(false);

                refs.StatRows.Add(row.gameObject);
                refs.StatLabels.Add(label);
                refs.StatValues.Add(value);
            }

            RectTransform costSection = Empty(refs.DetailsPanel, "CostSection");
            AddLayout(costSection, preferredHeight: 48f);
            VerticalLayoutGroup costLayout = costSection.gameObject.AddComponent<VerticalLayoutGroup>();
            costLayout.spacing = 3f;
            costLayout.childControlWidth = true;
            costLayout.childControlHeight = true;
            costLayout.childForceExpandWidth = true;
            costLayout.childForceExpandHeight = false;

            TMP_Text costTitle = Text(costSection, "CostTitle", "Вартість", sampleText, 10.5f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(costTitle.rectTransform, preferredHeight: 16f);
            RectTransform costs = Empty(costSection, "Costs");
            AddLayout(costs, preferredHeight: 24f);
            HorizontalLayoutGroup costsLayout = costs.gameObject.AddComponent<HorizontalLayoutGroup>();
            costsLayout.spacing = 10f;
            costsLayout.childControlWidth = true;
            costsLayout.childControlHeight = true;
            costsLayout.childForceExpandWidth = false;
            costsLayout.childForceExpandHeight = true;

            for (int i = 0; i < CostRowCount; i++)
                BuildCostRow(refs, costs, sampleText, i);

            RectTransform action = Empty(refs.DetailsPanel, "ActionRow");
            AddLayout(action, preferredHeight: 34f);
            HorizontalLayoutGroup actionLayout = action.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 8f;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            actionLayout.childForceExpandWidth = false;
            actionLayout.childForceExpandHeight = true;

            refs.ActionHint = Text(action, "ActionHint", string.Empty, sampleText, 10.5f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            refs.ActionHint.enableAutoSizing = true;
            refs.ActionHint.fontSizeMin = 8.5f;
            refs.ActionHint.fontSizeMax = 10.5f;
            refs.ActionHint.overflowMode = TextOverflowModes.Ellipsis;
            AddLayout(refs.ActionHint.rectTransform, flexibleWidth: 1f);

            refs.HireButton = Button(action, "HireButton", "Найняти", sampleText, AccentColor);
            AddLayout(refs.HireButton.transform as RectTransform, preferredWidth: 124f, preferredHeight: 34f);
            refs.HireButtonLabel = refs.HireButton.GetComponentInChildren<TMP_Text>(true);
        }

        private static void BuildCostRow(
            AuthoredRefs refs,
            RectTransform parent,
            TMP_Text sampleText,
            int index)
        {
            RectTransform row = Empty(parent, $"CostRow_{index + 1:00}");
            AddLayout(row, preferredWidth: 68f, preferredHeight: 24f);
            row.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 68f);
            row.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24f);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            Image icon = Image(row, "Icon", Color.white);
            AddLayout(icon.rectTransform, preferredWidth: 22f, preferredHeight: 22f);
            icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 22f);
            icon.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 22f);
            TMP_Text label = Text(row, "Label", string.Empty, sampleText, 11f, TextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(label.rectTransform, preferredWidth: 38f);
            label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 38f);
            label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24f);
            row.gameObject.SetActive(false);

            refs.CostRows.Add(row.gameObject);
            refs.CostIcons.Add(icon);
            refs.CostLabels.Add(label);
        }

        private static void BuildQueue(AuthoredRefs refs, TMP_Text sampleText)
        {
            VerticalLayoutGroup layout = refs.QueueSection.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform header = Empty(refs.QueueSection, "QueueHeader");
            AddLayout(header, preferredHeight: 22f);
            HorizontalLayoutGroup headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 8f;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            refs.QueueTitle = Text(header, "QueueTitle", "Черга", sampleText, 11f, TextColor, TextAlignmentOptions.MidlineLeft);
            refs.QueueTitle.fontStyle = FontStyles.Bold;
            AddLayout(refs.QueueTitle.rectTransform, preferredWidth: 64f);

            refs.QueueCapacity = Text(header, "QueueCapacity", "0/3", sampleText, 10.5f, SecondaryTextColor, TextAlignmentOptions.MidlineLeft);
            AddLayout(refs.QueueCapacity.rectTransform, preferredWidth: 48f);

            refs.QueueEmptyText = Text(header, "QueueEmpty", "Порожньо", sampleText, 10.5f, MutedTextColor, TextAlignmentOptions.MidlineRight);
            AddLayout(refs.QueueEmptyText.rectTransform, flexibleWidth: 1f);

            refs.QueueRows = Empty(refs.QueueSection, "QueueRows");
            refs.QueueRows.gameObject.SetActive(false);
            VerticalLayoutGroup rowsLayout = refs.QueueRows.gameObject.AddComponent<VerticalLayoutGroup>();
            rowsLayout.spacing = 4f;
            rowsLayout.childControlWidth = true;
            rowsLayout.childControlHeight = false;
            rowsLayout.childForceExpandWidth = true;
            rowsLayout.childForceExpandHeight = false;

            for (int i = 0; i < QueueRowCount; i++)
                BuildQueueRow(refs, sampleText, i);
        }

        private static void BuildQueueRow(
            AuthoredRefs refs,
            TMP_Text sampleText,
            int index)
        {
            RectTransform row = Empty(
                refs.QueueRows,
                $"QueueRow_{index + 1:00}",
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            AddLayout(row, preferredHeight: 30f);
            Image image = row.GetComponent<Image>();
            SetFlat(image, RowColor, raycast: true);
            StyleButton(row.GetComponent<Button>(), image, RowColor, RowHoverColor, RowPressedColor);

            Image icon = Image(row, "UnitIcon", Color.white);
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(6f, 1f);
            iconRect.sizeDelta = new Vector2(22f, 22f);

            TMP_Text label = Text(row, "Label", string.Empty, sampleText, 10f, TextColor, TextAlignmentOptions.MidlineLeft);
            label.enableAutoSizing = true;
            label.fontSizeMin = 8.5f;
            label.fontSizeMax = 10.5f;
            label.overflowMode = TextOverflowModes.Ellipsis;
            Stretch(label.rectTransform, 34f, 6f, 5f, 4f);

            Image track = Image(row, "ProgressTrack", TrackColor);
            RectTransform trackRect = track.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0.5f, 0f);
            trackRect.offsetMin = new Vector2(5f, 2f);
            trackRect.offsetMax = new Vector2(-5f, 5f);

            Image fill = Image(track.transform, "Fill", AccentColor);
            fill.type = UnityEngine.UI.Image.Type.Filled;
            fill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            Stretch(fill.rectTransform);

            row.gameObject.SetActive(false);
            refs.QueueButtons.Add(row.GetComponent<Button>());
            refs.QueueIcons.Add(icon);
            refs.QueueLabels.Add(label);
            refs.QueueProgressFills.Add(fill);
        }

        private static ValidationReport ValidateGeometry(
            RectTransform hud,
            RectTransform panel,
            GameplayRecruitmentPanelView view)
        {
            var report = new ValidationReport();
            if (hud == null || panel == null)
            {
                report.Fail("missing-rects");
                return report;
            }

            report.RecipeRowMaxHeight = MaxChildHeight(panel.Find("MainContent/Catalog/RecipeScroll/RecipeSlots") as RectTransform);
            report.QueueRowMaxHeight = MaxChildHeight(panel.Find("QueueSection/QueueRows") as RectTransform);
            report.HireButtonSize = SizeOf(view.HireButton != null ? view.HireButton.transform as RectTransform : null);
            report.CloseButtonSize = SizeOf(view.CloseButton != null ? view.CloseButton.transform as RectTransform : null);
            report.PanelSize = panel.rect.size;

            if (report.RecipeRowMaxHeight > 46.5f)
                report.Fail("recipe-row-height");
            if (report.QueueRowMaxHeight > 32.5f)
                report.Fail("queue-row-height");
            if (report.HireButtonSize.x > 132.5f || report.HireButtonSize.y > 36.5f)
                report.Fail("hire-button-size");
            if (report.CloseButtonSize.x > 30.5f || report.CloseButtonSize.y > 30.5f)
                report.Fail("close-button-size");
            if (!Contains(CanvasRect(hud, hud), CanvasRect(panel, hud)))
                report.Fail("panel-outside-hud");

            CheckNoOverlap(report, panel, "Header", "StateStrip");
            CheckNoOverlap(report, panel, "StateStrip", "MainContent");
            CheckNoOverlap(report, panel, "MainContent", "QueueSection");
            CheckNoOverlap(report, panel, "MainContent/Catalog", "MainContent/Details");

            report.QueueCollapsedWhenEmpty =
                CountActiveChildren(panel.Find("QueueSection/QueueRows")) == 0
                && (panel.Find("QueueSection") as RectTransform).rect.height <= 40.5f;
            if (!report.QueueCollapsedWhenEmpty)
                report.Fail("queue-empty-height");

            report.TechnicalIdLeaks = CountTechnicalIdLeaks(panel);
            if (report.TechnicalIdLeaks > 0)
                report.Fail("technical-id-leaks");

            return report;
        }

        private static string SimulateSizes()
        {
            Vector2[] sizes =
            {
                new(1920f, 1080f),
                new(1600f, 900f),
                new(1366f, 768f),
                new(1280f, 720f)
            };

            var sb = new StringBuilder("simulated=");
            for (int i = 0; i < sizes.Length; i++)
            {
                Vector2 size = sizes[i];
                float width = size.x >= 1600f ? 610f : size.x >= 1360f ? 548f : 520f;
                float height = size.y >= 900f ? 480f : size.y >= 760f ? 450f : 430f;
                bool pass = width <= size.x - 40f && height <= size.y - 40f;
                if (i > 0)
                    sb.Append(",");
                sb.Append(size.x.ToString("0"))
                    .Append("x")
                    .Append(size.y.ToString("0"))
                    .Append(":")
                    .Append(width.ToString("0"))
                    .Append("x")
                    .Append(height.ToString("0"))
                    .Append(pass ? ":pass" : ":fail");
            }
            return sb.ToString();
        }

        private static void CheckNoOverlap(
            ValidationReport report,
            RectTransform root,
            string aPath,
            string bPath)
        {
            RectTransform a = root.Find(aPath) as RectTransform;
            RectTransform b = root.Find(bPath) as RectTransform;
            if (a == null || b == null)
            {
                report.Fail("missing-" + aPath + "-or-" + bPath);
                return;
            }

            if (CanvasRect(a, root).Overlaps(CanvasRect(b, root)))
                report.Fail("overlap-" + aPath + "-" + bPath);
        }

        private static Rect CanvasRect(RectTransform rect, RectTransform root)
        {
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector3 min = root.InverseTransformPoint(corners[0]);
            Vector3 max = root.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool Contains(Rect outer, Rect inner)
            => inner.xMin >= outer.xMin - 0.5f
               && inner.xMax <= outer.xMax + 0.5f
               && inner.yMin >= outer.yMin - 0.5f
               && inner.yMax <= outer.yMax + 0.5f;

        private static float MaxChildHeight(RectTransform parent)
        {
            if (parent == null)
                return 0f;

            float max = 0f;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) is RectTransform child)
                    max = Mathf.Max(max, child.rect.height);
            }
            return max;
        }

        private static Vector2 SizeOf(RectTransform rect)
            => rect != null ? rect.rect.size : Vector2.zero;

        private static int CountActiveChildren(Transform parent)
        {
            int count = 0;
            if (parent == null)
                return count;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).gameObject.activeSelf)
                    count++;
            }
            return count;
        }

        private static int CountTechnicalIdLeaks(Transform root)
        {
            int count = 0;
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                string value = texts[i].text ?? string.Empty;
                if (value.Contains("-resources", StringComparison.Ordinal)
                    || value.Contains("_", StringComparison.Ordinal)
                    || value.Contains("TypeId", StringComparison.Ordinal)
                    || value.Contains("UnitTypeId", StringComparison.Ordinal))
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountType(string fullName)
        {
            int count = 0;
            Type target = FindType(fullName);
            if (target == null)
                return 0;

            Object[] objects = Resources.FindObjectsOfTypeAll(target);
            if (objects == null)
                return 0;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is Component c
                    && c != null
                    && c.gameObject != null
                    && c.gameObject.scene.IsValid())
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountSourceOccurrences(string path, string needle)
        {
            if (!System.IO.File.Exists(path) || string.IsNullOrEmpty(needle))
                return 0;

            string text = System.IO.File.ReadAllText(path);
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(needle, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += needle.Length;
            }

            return count;
        }

        private static Type FindType(string fullName)
        {
            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int index = 0; index < assemblies.Length; index++)
            {
                Type type = assemblies[index].GetType(fullName);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static Scene EnsureScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || scene.path != ScenePath)
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            return scene;
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
                throw new InvalidOperationException($"Missing {parts[0]}");

            for (int i = 1; i < parts.Length; i++)
                current = RequireChild(current, parts[i]);

            return current;
        }

        private static Transform RequireChild(Transform parent, string relative)
        {
            Transform child = parent.Find(relative);
            if (child == null)
                throw new InvalidOperationException($"Missing {parent.name}/{relative}");
            return child;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }

        private static void PreparePanelRoot(GameObject panel)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(panel);
            RemoveExactComponents<ScrollRect>(panel);
            RemoveExactComponents<Mask>(panel);
            RemoveExactComponents<Shadow>(panel);
            RemoveExactComponents<HorizontalLayoutGroup>(panel);
            RemoveExactComponents<GridLayoutGroup>(panel);
            RemoveExactComponents<ContentSizeFitter>(panel);
        }

        private static void RemoveExactComponents<T>(GameObject go)
            where T : Component
        {
            T[] components = go.GetComponents<T>();
            for (int index = components.Length - 1; index >= 0; index--)
            {
                T component = components[index];
                if (component != null && component.GetType() == typeof(T))
                    Object.DestroyImmediate(component);
            }
        }

        private static RectTransform Section(Transform parent, string name, Color color)
        {
            RectTransform rect = Empty(parent, name, typeof(CanvasRenderer), typeof(Image));
            SetFlat(rect.GetComponent<Image>(), color, raycast: false);
            EnsureOutline(rect.gameObject, BorderColor, 1f);
            return rect;
        }

        private static RectTransform Empty(Transform parent, string name, params Type[] components)
        {
            Type[] all = components == null || components.Length == 0
                ? new[] { typeof(RectTransform) }
                : BuildComponentList(components);

            var go = new GameObject(name, all);
            go.transform.SetParent(parent, false);
            return go.transform as RectTransform;
        }

        private static Type[] BuildComponentList(Type[] components)
        {
            var all = new List<Type> { typeof(RectTransform) };
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && components[i] != typeof(RectTransform))
                    all.Add(components[i]);
            }
            return all.ToArray();
        }

        private static GameObject ImageObject(Transform parent, string name, Color color)
            => Image(parent, name, color).gameObject;

        private static Image Image(Transform parent, string name, Color color)
        {
            RectTransform rect = Empty(parent, name, typeof(CanvasRenderer), typeof(Image));
            Image image = rect.GetComponent<Image>();
            SetFlat(image, color, raycast: false);
            return image;
        }

        private static TMP_Text Text(
            Transform parent,
            string name,
            string value,
            TMP_Text sample,
            float fontSize,
            Color color,
            TextAlignmentOptions alignment)
        {
            RectTransform rect = Empty(parent, name, typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            TMP_Text text = rect.GetComponent<TMP_Text>();
            if (sample != null)
            {
                text.font = sample.font;
                text.fontSharedMaterial = sample.fontSharedMaterial;
            }

            text.text = value ?? string.Empty;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        private static Button Button(
            Transform parent,
            string name,
            string label,
            TMP_Text sample,
            Color color)
        {
            RectTransform rect = Empty(parent, name, typeof(CanvasRenderer), typeof(Image), typeof(Button));
            Image image = rect.GetComponent<Image>();
            SetFlat(image, color, raycast: true);
            Button button = rect.GetComponent<Button>();
            StyleButton(button, image, color, RowHoverColor, RowPressedColor);

            TMP_Text text = Text(rect, "Label", label, sample, 13f, Color.white, TextAlignmentOptions.Center);
            text.enableAutoSizing = true;
            text.fontSizeMin = 10f;
            text.fontSizeMax = 13f;
            Stretch(text.rectTransform, 4f, 4f, 2f, 2f);
            return button;
        }

        private static void StyleButton(
            Button button,
            Graphic target,
            Color normal,
            Color hover,
            Color pressed)
        {
            if (button == null)
                return;

            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = hover;
            colors.pressedColor = pressed;
            colors.selectedColor = hover;
            colors.disabledColor = new Color(
                normal.r * 0.45f,
                normal.g * 0.45f,
                normal.b * 0.45f,
                0.76f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.06f;
            button.colors = colors;
        }

        private static void SetFlat(Image image, Color color, bool raycast)
        {
            if (image == null)
                return;

            image.sprite = null;
            image.type = UnityEngine.UI.Image.Type.Simple;
            image.color = color;
            image.preserveAspect = false;
            image.raycastTarget = raycast;
        }

        private static void EnsureOutline(GameObject go, Color color, float distance)
        {
            Outline outline = go.GetComponent<Outline>() ?? go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static LayoutElement AddLayout(
            RectTransform rect,
            float preferredWidth = -1f,
            float preferredHeight = -1f,
            float flexibleWidth = 0f,
            float flexibleHeight = 0f)
        {
            LayoutElement element = rect.GetComponent<LayoutElement>() ?? rect.gameObject.AddComponent<LayoutElement>();
            element.minWidth = preferredWidth > 0f ? preferredWidth : -1f;
            element.preferredWidth = preferredWidth;
            element.flexibleWidth = flexibleWidth;
            element.minHeight = preferredHeight > 0f ? preferredHeight : -1f;
            element.preferredHeight = preferredHeight;
            element.flexibleHeight = flexibleHeight;
            return element;
        }

        private static void Stretch(RectTransform rect)
            => Stretch(rect, 0f, 0f, 0f, 0f);

        private static void Stretch(
            RectTransform rect,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetObject(SerializedObject so, string propertyPath, Object value)
        {
            SerializedProperty property = so.FindProperty(propertyPath);
            if (property == null)
                throw new InvalidOperationException($"Missing serialized field {propertyPath} on {so.targetObject.name}.");
            property.objectReferenceValue = value;
        }

        private static void AssignArray<T>(
            SerializedObject so,
            string propertyPath,
            IReadOnlyList<T> values)
            where T : Object
        {
            SerializedProperty property = so.FindProperty(propertyPath);
            if (property == null)
                throw new InvalidOperationException($"Missing serialized array {propertyPath} on {so.targetObject.name}.");

            property.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        private static string RefState(Object owner)
        {
            if (owner == null)
                return "missing-component";

            var so = new SerializedObject(owner);
            SerializedProperty it = so.GetIterator();
            var result = new StringBuilder();
            bool enter = true;
            while (it.NextVisible(enter))
            {
                enter = false;
                if (it.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                result.Append(it.propertyPath)
                    .Append("=")
                    .Append(it.objectReferenceValue != null ? "ok" : "null")
                    .Append(";");
            }
            return result.ToString();
        }

        private static void DumpTransform(StringBuilder sb, Transform t, bool includeChildren)
        {
            if (t == null)
                return;

            RectTransform rect = t as RectTransform;
            sb.Append(HierarchyPath(t));
            if (rect != null)
            {
                sb.Append(" active=").Append(t.gameObject.activeSelf)
                    .Append(" anchorMin=").Append(Vec(rect.anchorMin))
                    .Append(" anchorMax=").Append(Vec(rect.anchorMax))
                    .Append(" pivot=").Append(Vec(rect.pivot))
                    .Append(" anchored=").Append(Vec(rect.anchoredPosition))
                    .Append(" sizeDelta=").Append(Vec(rect.sizeDelta))
                    .Append(" rect=").Append(Vec(rect.rect.size));
            }

            TMP_Text text = t.GetComponent<TMP_Text>();
            if (text != null)
                sb.Append(" text=").Append(text.text);

            sb.Append(" components=");
            Component[] components = t.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");
                sb.Append(components[i] != null ? components[i].GetType().Name : "null");
            }
            sb.AppendLine();

            if (!includeChildren)
                return;

            for (int i = 0; i < t.childCount; i++)
                DumpTransform(sb, t.GetChild(i), includeChildren: true);
        }

        private static string HierarchyPath(Transform transform)
        {
            var stack = new Stack<string>();
            for (Transform current = transform; current != null; current = current.parent)
                stack.Push(current.name);
            return string.Join("/", stack);
        }

        private static string Vec(Vector2 value)
            => $"({value.x:0.#},{value.y:0.#})";

        private sealed class AuthoredRefs
        {
            public RectTransform Header;
            public TMP_Text BuildingName;
            public TMP_Text Subtitle;
            public GameObject StateChip;
            public TMP_Text StateChipLabel;
            public Button CloseButton;
            public RectTransform StateStrip;
            public TMP_Text StateTitle;
            public TMP_Text StateDetail;
            public Image StateFill;
            public RectTransform MainContent;
            public RectTransform Catalog;
            public RectTransform RecipeSlots;
            public RectTransform DetailsPanel;
            public Image SelectionIcon;
            public TMP_Text SelectionName;
            public TMP_Text SelectionClass;
            public TMP_Text LegacyStats;
            public Button HireButton;
            public TMP_Text HireButtonLabel;
            public TMP_Text ActionHint;
            public RectTransform QueueSection;
            public TMP_Text QueueTitle;
            public TMP_Text QueueCapacity;
            public TMP_Text QueueEmptyText;
            public RectTransform QueueRows;

            public readonly List<Button> RecipeButtons = new();
            public readonly List<Image> RecipeIcons = new();
            public readonly List<TMP_Text> RecipeNameTexts = new();
            public readonly List<TMP_Text> RecipeTrainingTexts = new();
            public readonly List<GameObject> RecipeSelectedAccents = new();
            public readonly List<GameObject> StatRows = new();
            public readonly List<TMP_Text> StatLabels = new();
            public readonly List<TMP_Text> StatValues = new();
            public readonly List<GameObject> CostRows = new();
            public readonly List<Image> CostIcons = new();
            public readonly List<TMP_Text> CostLabels = new();
            public readonly List<Button> QueueButtons = new();
            public readonly List<Image> QueueIcons = new();
            public readonly List<TMP_Text> QueueLabels = new();
            public readonly List<Image> QueueProgressFills = new();
        }

        private sealed class ValidationReport
        {
            private readonly List<string> _failures = new();

            public Vector2 PanelSize;
            public Vector2 HireButtonSize;
            public Vector2 CloseButtonSize;
            public float RecipeRowMaxHeight;
            public float QueueRowMaxHeight;
            public int TechnicalIdLeaks;
            public bool QueueCollapsedWhenEmpty;
            public bool Pass => _failures.Count == 0;

            public void Fail(string reason)
            {
                if (!_failures.Contains(reason))
                    _failures.Add(reason);
            }

            public override string ToString()
            {
                string failures = _failures.Count == 0 ? "none" : string.Join("|", _failures);
                return $"panel={PanelSize.x:0.#}x{PanelSize.y:0.#} recipeRowMaxHeight={RecipeRowMaxHeight:0.#} queueRowMaxHeight={QueueRowMaxHeight:0.#} hireButtonSize={HireButtonSize.x:0.#}x{HireButtonSize.y:0.#} closeButtonSize={CloseButtonSize.x:0.#}x{CloseButtonSize.y:0.#} queueCollapsedWhenEmpty={QueueCollapsedWhenEmpty} technicalIdLeaks={TechnicalIdLeaks} failures={failures}";
            }
        }
    }
}
