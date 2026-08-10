using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Kruty1918.Moyva.InputRouting.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    internal static class GameplayUiRedesignService
    {
        internal const string ScenePath = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
        private const string AppliedMarker = "MOYVA_GAMEPLAY_UI_PASS73";
        private const string DefaultFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private static readonly Color PanelDark = new(0.055f, 0.060f, 0.050f, 0.94f);
        private static readonly Color PanelMid = new(0.085f, 0.090f, 0.075f, 0.96f);
        private static readonly Color PanelSoft = new(0.12f, 0.13f, 0.105f, 0.96f);
        private static readonly Color Gold = new(0.82f, 0.66f, 0.28f, 1f);
        private static readonly Color Ivory = new(0.95f, 0.93f, 0.84f, 1f);
        private static readonly Color Muted = new(0.78f, 0.80f, 0.72f, 1f);

        internal static string Plan()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = New("plan", scene);
            if (!TryGetTargetScene(out scene, out string error))
                return Fail(result, error);

            string[] required =
            {
                "Canvas/EconomyPlayerSymmary/Root/Top",
                "Canvas/EconomyPlayerSymmary/Root/Top/Text Summary Food",
                "Canvas/EconomyPlayerSymmary/Root/Top/Text Summary Materials",
                "Canvas/GameModeUI/Build Button",
                "Canvas/ConstructionUI/Root",
                "Canvas/ConstructionUI/Root/StatusPanel",
                "Canvas/ConstructionUI/Root/BuildingSelectionPanel",
                "Canvas/ConstructionUI/Root/ActionBar",
                "Canvas/ConstructionUI/Root/CloseButton",
                "Canvas/ConstructionUI/Root/PreviewPanelInfo",
            };

            foreach (string path in required)
            {
                if (Find(path) == null)
                    Add(result, "error", "missing-required-object", path, "Required Gameplay UI object is missing.");
            }

            GameObject economy = Find("Canvas/EconomyPlayerSymmary");
            if (economy != null)
            {
                SerializedObject so = SerializedMono(economy, "Kruty1918.Moyva.Economy.Runtime.EconomyPlayerResourceSummaryUIController");
                if (so == null)
                    Add(result, "error", "economy-controller-missing", economy.name, "Economy summary controller was not found.");
                else
                {
                    string materialsRef = RefName(so.FindProperty("_totalMaterialsText"));
                    string foodRef = RefName(so.FindProperty("_totalFoodText"));
                    string moneyRef = RefName(so.FindProperty("_totalMoneyText"));
                    if (materialsRef == "Text Summary Food" && foodRef == "Text Summary Materials")
                        Add(result, "warning", "economy-bindings-swapped", economy.name, "Materials/Food serialized references are swapped and will be corrected.");
                    if (string.IsNullOrEmpty(moneyRef))
                        Add(result, "warning", "money-hud-missing", economy.name, "Money text is not wired and will be added.");
                }
            }

            GameObject viewport = Find("Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport");
            Image viewportImage = viewport != null ? viewport.GetComponent<Image>() : null;
            if (viewportImage != null && viewportImage.raycastTarget && viewportImage.color.a <= 0.03f)
                Add(result, "warning", "transparent-scroll-raycast", PathOf(viewport.transform), "Viewport is almost transparent while receiving raycasts; it will become a deliberate visible scroll surface.");

            result.errors = result.checks.Count(c => c.severity == "error");
            result.warnings = result.checks.Count(c => c.severity == "warning");
            result.ok = result.errors == 0;
            result.message = result.ok
                ? "Pass73 plan is applicable. Existing controller references will be preserved; resource wiring and responsive layout will be improved."
                : "Pass73 plan blocked by missing required objects.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string Apply(string confirm)
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = New("apply", scene);

            if (!string.Equals(confirm, "APPLY", StringComparison.Ordinal))
                return Fail(result, "Confirmation must be exactly APPLY.");
            if (!TryGetTargetScene(out scene, out string error))
                return Fail(result, error);
            if (scene.isDirty)
                return Fail(result, "Gameplay scene already has unsaved changes; Pass73 refuses to overwrite/merge an unknown dirty state.");
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return Fail(result, "Apply requires idle Edit Mode.");

            string backup = BackupScene(scene.path);
            if (string.IsNullOrWhiteSpace(backup))
                return Fail(result, "External scene backup failed; apply aborted.");
            result.backupPath = backup;

            string planJson = Plan();
            if (planJson.Contains("\"ok\": false", StringComparison.OrdinalIgnoreCase))
                return Fail(result, "Plan preflight failed. Run moyva-gameplay-ui-redesign-plan for details.");

            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Moyva Gameplay UI Pass73");
            int changed = 0;

            try
            {
                changed += StyleResourceBar();
                changed += StyleBuildButton();
                changed += StyleConstructionStatus();
                changed += StyleConstructionSelection();
                changed += StyleConstructionActions();
                changed += StyleConstructionClose();
                changed += StylePreviewPanel();
                changed += NormalizeGameplayFonts();

                GameObject canvas = Find("Canvas");
                if (canvas != null)
                {
                    if (canvas.GetComponent<GameplayUiPass73Marker>() == null)
                    {
                        Undo.AddComponent<GameplayUiPass73Marker>(canvas);
                        changed++;
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group);

                result.changedObjects = changed;
                result.message = "PC Gameplay UI foundation applied in memory. Scene is dirty but not saved; validate and capture before saving.";
                return JsonUtility.ToJson(result, true);
            }
            catch (Exception exception)
            {
                Undo.RevertAllDownToGroup(group);
                return Fail(result, exception.GetType().Name + ": " + exception.Message);
            }
        }

        internal static string Validate()
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = New("validate", scene);
            if (!TryGetTargetScene(out scene, out string error))
                return Fail(result, error);

            GameObject canvas = Find("Canvas");
            if (canvas == null || canvas.GetComponent<GameplayUiPass73Marker>() == null)
                Add(result, "error", "pass73-marker-missing", "Canvas", "Pass73 marker is missing; redesign may not have been applied.");

            GameObject economy = Find("Canvas/EconomyPlayerSymmary");
            SerializedObject eso = economy != null
                ? SerializedMono(economy, "Kruty1918.Moyva.Economy.Runtime.EconomyPlayerResourceSummaryUIController")
                : null;
            if (eso == null)
                Add(result, "error", "economy-controller-missing", "Canvas/EconomyPlayerSymmary", "Economy controller unavailable.");
            else
            {
                string materialsRef = RefName(eso.FindProperty("_totalMaterialsText"));
                string foodRef = RefName(eso.FindProperty("_totalFoodText"));
                string moneyRef = RefName(eso.FindProperty("_totalMoneyText"));
                if (materialsRef != "Text Summary Materials")
                    Add(result, "error", "materials-binding", economy.name, "Materials field is not wired to Text Summary Materials.");
                if (foodRef != "Text Summary Food")
                    Add(result, "error", "food-binding", economy.name, "Food field is not wired to Text Summary Food.");
                if (moneyRef != "Text Summary Money")
                    Add(result, "error", "money-binding", economy.name, "Money field is not wired to Text Summary Money.");
            }

            GameObject build = Find("Canvas/GameModeUI/Build Button");
            RectTransform buildRect = build != null ? build.GetComponent<RectTransform>() : null;
            if (buildRect == null || buildRect.sizeDelta.x < 104f || buildRect.sizeDelta.y < 40f)
                Add(result, "warning", "build-pc-target", "Canvas/GameModeUI/Build Button", "Build command should be at least 104x40 reference pixels.");

            GameObject viewport = Find("Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport");
            Image viewportImage = viewport != null ? viewport.GetComponent<Image>() : null;
            if (viewportImage == null || viewportImage.color.a < 0.05f)
                Add(result, "warning", "scroll-surface", ".../Viewport", "Scroll viewport should have a deliberate visible surface rather than an invisible raycast blocker.");

            foreach (GameObject go in SceneObjects(scene))
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missing > 0)
                    Add(result, "error", "missing-script", PathOf(go.transform), $"Missing MonoBehaviour count={missing}.");

                TMP_Text text = go.GetComponent<TMP_Text>();
                if (text != null && text.font == null)
                    Add(result, "error", "tmp-font-missing", PathOf(go.transform), "TMP text has no font asset.");
            }

            result.errors = result.checks.Count(c => c.severity == "error");
            result.warnings = result.checks.Count(c => c.severity == "warning");
            result.ok = result.errors == 0;
            result.message = result.ok
                ? "Pass73 validation passed."
                : "Pass73 validation failed; do not save.";
            return JsonUtility.ToJson(result, true);
        }

        internal static string PreviewMode(string mode)
        {
            Scene scene = SceneManager.GetActiveScene();
            var result = New("preview", scene);
            if (!TryGetTargetScene(out scene, out string error))
                return Fail(result, error);

            GameObject construction = Find("Canvas/ConstructionUI/Root");
            GameObject economyTop = Find("Canvas/EconomyPlayerSymmary/Root/Top");
            GameObject buildButton = Find("Canvas/GameModeUI/Build Button");
            if (construction == null || economyTop == null || buildButton == null)
                return Fail(result, "Preview roots are missing.");

            bool constructionMode;
            if (string.Equals(mode, "construction", StringComparison.OrdinalIgnoreCase))
                constructionMode = true;
            else if (string.Equals(mode, "normal", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(mode, "restore", StringComparison.OrdinalIgnoreCase))
                constructionMode = false;
            else
                return Fail(result, "mode must be normal, construction, or restore.");

            construction.SetActive(constructionMode);
            economyTop.SetActive(!constructionMode);
            buildButton.SetActive(!constructionMode);
            SceneView.RepaintAll();

            result.message = constructionMode
                ? "Construction shell preview enabled without saving."
                : "Normal HUD preview enabled without saving.";
            return JsonUtility.ToJson(result, true);
        }

        private static int StyleResourceBar()
        {
            int changed = 0;
            GameObject top = Require("Canvas/EconomyPlayerSymmary/Root/Top");
            GameObject foodGo = Require("Canvas/EconomyPlayerSymmary/Root/Top/Text Summary Food");
            GameObject materialsGo = Require("Canvas/EconomyPlayerSymmary/Root/Top/Text Summary Materials");
            GameObject economy = Require("Canvas/EconomyPlayerSymmary");

            Record(top);
            RectTransform topRect = top.GetComponent<RectTransform>();
            topRect.anchorMin = topRect.anchorMax = new Vector2(0.5f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.anchoredPosition = new Vector2(0f, -18f);
            topRect.sizeDelta = new Vector2(520f, 48f);

            Image bg = RequireComponent<Image>(top);
            bg.color = PanelDark;
            bg.raycastTarget = false;
            AddOutline(top, new Color(Gold.r, Gold.g, Gold.b, 0.58f), new Vector2(0f, -3f));

            HorizontalLayoutGroup layout = EnsureComponent<HorizontalLayoutGroup>(top);
            Record(layout);
            layout.padding = new RectOffset(12, 12, 6, 6);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            TMP_Text food = foodGo.GetComponent<TMP_Text>();
            TMP_Text materials = materialsGo.GetComponent<TMP_Text>();
            StyleResourceText(materials, "Materials");
            StyleResourceText(food, "Food");

            GameObject moneyGo = Find("Canvas/EconomyPlayerSymmary/Root/Top/Text Summary Money");
            if (moneyGo == null)
            {
                moneyGo = UnityEngine.Object.Instantiate(materialsGo, top.transform);
                Undo.RegisterCreatedObjectUndo(moneyGo, "Create Money HUD text");
                moneyGo.name = "Text Summary Money";
                changed++;
            }
            TMP_Text money = moneyGo.GetComponent<TMP_Text>();
            StyleResourceText(money, "Money");

            // Explicit hierarchy order: Materials · Food · Money.
            materialsGo.transform.SetSiblingIndex(0);
            foodGo.transform.SetSiblingIndex(1);
            moneyGo.transform.SetSiblingIndex(2);

            SerializedObject so = SerializedMono(economy, "Kruty1918.Moyva.Economy.Runtime.EconomyPlayerResourceSummaryUIController");
            if (so == null)
                throw new InvalidOperationException("EconomyPlayerResourceSummaryUIController missing.");
            so.Update();
            SetObjectRef(so, "_textsRoot", top.transform);
            SetObjectRef(so, "_totalMaterialsText", materials);
            SetObjectRef(so, "_totalFoodText", food);
            SetObjectRef(so, "_totalMoneyText", money);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(economy);
            changed += 4;
            return changed;
        }

        private static void StyleResourceText(TMP_Text text, string editModeLabel)
        {
            if (text == null) return;
            Record(text);
            text.text = editModeLabel;
            ApplyDefaultFont(text);
            text.fontSize = 16f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Ivory;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            LayoutElement le = EnsureComponent<LayoutElement>(text.gameObject);
            Record(le);
            le.minWidth = 140f;
            le.preferredWidth = 156f;
            le.preferredHeight = 36f;
            le.flexibleWidth = 1f;
        }

        private static int StyleBuildButton()
        {
            GameObject go = Require("Canvas/GameModeUI/Build Button");
            Record(go);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-24f, 24f);
            rect.sizeDelta = new Vector2(112f, 40f);

            Image bg = go.GetComponent<Image>();
            if (bg != null)
            {
                Record(bg);
                bg.color = Color.white;
                bg.raycastTarget = true;
            }

            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                Record(button);
                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.None;
                button.navigation = nav;
            }

            GameObject iconGo = Find("Canvas/GameModeUI/Build Button/Icon");
            if (iconGo != null)
            {
                Record(iconGo);
                RectTransform iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(10f, 0f);
                iconRect.sizeDelta = new Vector2(22f, 22f);
                Image icon = iconGo.GetComponent<Image>();
                if (icon != null) icon.raycastTarget = false;
            }
            GameObject labelGo = Find("Canvas/GameModeUI/Build Button/Label");
            if (labelGo == null)
            {
                labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(labelGo, "Create PC build button label");
                labelGo.transform.SetParent(go.transform, false);
            }
            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(36f, 0f);
            labelRect.offsetMax = new Vector2(-8f, 0f);
            TMP_Text buildLabel = labelGo.GetComponent<TMP_Text>();
            ApplyDefaultFont(buildLabel);
            buildLabel.text = "Build  [B]";
            buildLabel.fontSize = 14f;
            buildLabel.fontStyle = FontStyles.Bold;
            buildLabel.alignment = TextAlignmentOptions.Center;
            buildLabel.color = Ivory;
            buildLabel.raycastTarget = false;
            return 2;
        }

        private static int StyleConstructionStatus()
        {
            GameObject panel = Require("Canvas/ConstructionUI/Root/StatusPanel");
            Record(panel);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -18f);
            rect.sizeDelta = new Vector2(720f, 42f);

            Image bg = panel.GetComponent<Image>();
            bg.color = PanelDark;
            bg.raycastTarget = false;
            AddOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.45f), new Vector2(0f, -2f));

            HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
            Record(layout);
            layout.padding = new RectOffset(14, 14, 5, 5);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            foreach (string child in new[] { "StateLabel", "BuildingLabel", "PreviewLabel" })
            {
                GameObject labelGo = Require("Canvas/ConstructionUI/Root/StatusPanel/" + child);
                TMP_Text text = labelGo.GetComponent<TMP_Text>();
                Record(text);
                ApplyDefaultFont(text);
                text.fontSize = 15f;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.color = child == "PreviewLabel" ? Gold : Ivory;
                text.raycastTarget = false;
                LayoutElement le = EnsureComponent<LayoutElement>(labelGo);
                Record(le);
                le.minWidth = 180f;
                le.preferredWidth = 220f;
                le.preferredHeight = 32f;
                le.flexibleWidth = 1f;
            }
            return 4;
        }

        private static int StyleConstructionSelection()
        {
            GameObject panel = Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel");
            Record(panel);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 24f);
            panelRect.sizeDelta = new Vector2(-48f, 232f);

            Image panelBg = EnsureComponent<Image>(panel);
            Record(panelBg);
            panelBg.color = PanelDark;
            panelBg.raycastTarget = false;
            EnsureComponent<GameplayInputBlocker>(panel);
            AddOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.35f), new Vector2(0f, 2f));

            GameObject tabs = Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel/CategoryTabs");
            Record(tabs);
            RectTransform tabsRect = tabs.GetComponent<RectTransform>();
            tabsRect.anchorMin = new Vector2(0f, 1f);
            tabsRect.anchorMax = new Vector2(1f, 1f);
            tabsRect.pivot = new Vector2(0.5f, 1f);
            tabsRect.anchoredPosition = new Vector2(0f, -10f);
            tabsRect.sizeDelta = new Vector2(-300f, 48f);
            Image tabsBg = tabs.GetComponent<Image>();
            tabsBg.color = PanelMid;
            tabsBg.raycastTarget = false;

            GameObject tabContainer = Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel/CategoryTabs/TabContainer");
            HorizontalLayoutGroup tabLayout = tabContainer.GetComponent<HorizontalLayoutGroup>();
            Record(tabLayout);
            tabLayout.padding = new RectOffset(8, 8, 6, 6);
            tabLayout.spacing = 6f;
            tabLayout.childAlignment = TextAnchor.MiddleLeft;
            tabLayout.childControlWidth = false;
            tabLayout.childControlHeight = false;
            tabLayout.childForceExpandWidth = false;
            tabLayout.childForceExpandHeight = false;

            GameObject viewport = Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport");
            Record(viewport);
            RectTransform vpRect = viewport.GetComponent<RectTransform>();
            vpRect.anchorMin = Vector2.zero;
            vpRect.anchorMax = Vector2.one;
            vpRect.offsetMin = new Vector2(12f, 24f);
            vpRect.offsetMax = new Vector2(-12f, -60f);
            Image vpImage = viewport.GetComponent<Image>();
            vpImage.color = new Color(0.03f, 0.035f, 0.03f, 0.28f);
            vpImage.raycastTarget = true; // intentional scroll drag surface
            Mask mask = viewport.GetComponent<Mask>();
            if (mask != null) mask.showMaskGraphic = true;

            GameObject content = Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel/Viewport/Content");
            HorizontalLayoutGroup contentLayout = content.GetComponent<HorizontalLayoutGroup>();
            Record(contentLayout);
            contentLayout.padding = new RectOffset(12, 12, 8, 8);
            contentLayout.spacing = 8f;
            contentLayout.childAlignment = TextAnchor.MiddleLeft;
            contentLayout.childControlWidth = false;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;

            GameObject selectionPanel = panel;
            SerializedObject panelSo = SerializedMono(selectionPanel, "Kruty1918.Moyva.Construction.UI.BuildingSelectionPanelUI");
            if (panelSo != null)
            {
                panelSo.Update();
                SerializedProperty size = panelSo.FindProperty("buttonSize");
                if (size != null) size.vector2Value = new Vector2(112f, 128f);
                TMP_InputField search = EnsureConstructionSearch(panel);
                SerializedProperty searchProperty = panelSo.FindProperty("searchInput");
                if (searchProperty != null) searchProperty.objectReferenceValue = search;
                panelSo.ApplyModifiedProperties();
            }

            ScrollRect scrollRect = panel.GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                Record(scrollRect);
                scrollRect.scrollSensitivity = 32f;
                scrollRect.inertia = false;
                scrollRect.horizontal = true;
                scrollRect.vertical = false;
                EnsureHorizontalScrollbar(panel, scrollRect);
            }

            GameObject buildingTemplate = Require("Canvas/ConstructionUI/Root/Templates/BuildingButtonTemplate");
            StyleBuildingCardTemplate(buildingTemplate);
            GameObject tabTemplate = Require("Canvas/ConstructionUI/Root/Templates/CategoryTabButtonTemplate");
            StyleTabTemplate(tabTemplate);
            return 7;
        }

        private static TMP_InputField EnsureConstructionSearch(GameObject panel)
        {
            Transform existing = panel.transform.Find("Search");
            GameObject searchGo = existing != null ? existing.gameObject : null;
            if (searchGo == null)
            {
                searchGo = new GameObject(
                    "Search",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                    typeof(TMP_InputField));
                Undo.RegisterCreatedObjectUndo(searchGo, "Create construction catalogue search");
                searchGo.transform.SetParent(panel.transform, false);
            }

            RectTransform rect = searchGo.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-12f, -16f);
            rect.sizeDelta = new Vector2(268f, 36f);

            Image background = searchGo.GetComponent<Image>();
            background.color = new Color(0.045f, 0.05f, 0.043f, 0.98f);
            background.raycastTarget = true;
            AddOutline(searchGo, new Color(Gold.r, Gold.g, Gold.b, 0.32f), Vector2.one);
            EnsureComponent<GameplayInputBlocker>(searchGo);

            RectTransform textArea = EnsureRectChild(searchGo.transform, "Text Area");
            textArea.anchorMin = Vector2.zero;
            textArea.anchorMax = Vector2.one;
            textArea.offsetMin = new Vector2(10f, 4f);
            textArea.offsetMax = new Vector2(-10f, -4f);

            TextMeshProUGUI placeholder = EnsureTmpChild(textArea, "Placeholder");
            placeholder.text = "Пошук  [Ctrl+F]";
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = new Color(Muted.r, Muted.g, Muted.b, 0.7f);

            TextMeshProUGUI text = EnsureTmpChild(textArea, "Text");
            text.text = string.Empty;
            text.color = Ivory;

            TMP_InputField input = searchGo.GetComponent<TMP_InputField>();
            Record(input);
            input.textViewport = textArea;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 48;
            input.targetGraphic = background;
            return input;
        }

        private static RectTransform EnsureRectChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
                return existing as RectTransform;

            var child = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(child, "Create " + name);
            child.transform.SetParent(parent, false);
            return (RectTransform)child.transform;
        }

        private static TextMeshProUGUI EnsureTmpChild(RectTransform parent, string name)
        {
            Transform existing = parent.Find(name);
            GameObject child = existing != null ? existing.gameObject : null;
            if (child == null)
            {
                child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(child, "Create " + name);
                child.transform.SetParent(parent, false);
            }

            RectTransform rect = child.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            ApplyDefaultFont(label);
            label.fontSize = 14f;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static void StyleBuildingCardTemplate(GameObject go)
        {
            Record(go);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(112f, 128f);
            Image bg = go.GetComponent<Image>();
            bg.color = PanelSoft;
            bg.raycastTarget = true;
            AddOutline(go, new Color(Gold.r, Gold.g, Gold.b, 0.52f), new Vector2(0f, -2f));

            GameObject iconGo = Find("Canvas/ConstructionUI/Root/Templates/BuildingButtonTemplate/Icon");
            if (iconGo != null)
            {
                RectTransform ir = iconGo.GetComponent<RectTransform>();
                Record(ir);
                ir.anchorMin = ir.anchorMax = new Vector2(0.5f, 1f);
                ir.pivot = new Vector2(0.5f, 1f);
                ir.anchoredPosition = new Vector2(0f, -8f);
                ir.sizeDelta = new Vector2(72f, 72f);
                Image image = iconGo.GetComponent<Image>();
                if (image != null) image.raycastTarget = false;
            }

            GameObject labelGo = Find("Canvas/ConstructionUI/Root/Templates/BuildingButtonTemplate/Label");
            if (labelGo != null)
            {
                RectTransform lr = labelGo.GetComponent<RectTransform>();
                Record(lr);
                lr.anchorMin = new Vector2(0f, 0f);
                lr.anchorMax = new Vector2(1f, 0f);
                lr.pivot = new Vector2(0.5f, 0f);
                lr.anchoredPosition = new Vector2(0f, 5f);
                lr.sizeDelta = new Vector2(-10f, 38f);
                TMP_Text text = labelGo.GetComponent<TMP_Text>();
                ApplyDefaultFont(text);
                text.fontSize = 15f;
                text.fontStyle = FontStyles.Bold;
                text.color = Ivory;
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
            }
        }

        private static void StyleTabTemplate(GameObject go)
        {
            Record(go);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(132f, 36f);
            ApplyProjectButtonSkin(go);
            GameObject labelGo = Find("Canvas/ConstructionUI/Root/Templates/CategoryTabButtonTemplate/Label");
            if (labelGo != null)
            {
                TMP_Text text = labelGo.GetComponent<TMP_Text>();
                Record(text);
                ApplyDefaultFont(text);
                text.fontSize = 15f;
                text.fontStyle = FontStyles.Bold;
                text.color = Ivory;
                text.raycastTarget = false;
            }

            SerializedObject tabsSo = SerializedMono(
                Require("Canvas/ConstructionUI/Root/BuildingSelectionPanel/CategoryTabs"),
                "Kruty1918.Moyva.Construction.UI.BuildingCategoryTabsUI");
            if (tabsSo != null)
            {
                tabsSo.Update();
                SerializedProperty size = tabsSo.FindProperty("categoryButtonSize");
                if (size != null) size.vector2Value = new Vector2(132f, 36f);
                SerializedProperty active = tabsSo.FindProperty("activeColor");
                if (active != null) active.colorValue = Gold;
                SerializedProperty inactive = tabsSo.FindProperty("inactiveColor");
                if (inactive != null) inactive.colorValue = Ivory;
                tabsSo.ApplyModifiedProperties();
            }
        }

        private static int StyleConstructionActions()
        {
            GameObject panel = Require("Canvas/ConstructionUI/Root/ActionBar");
            Record(panel);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 268f);
            rect.sizeDelta = new Vector2(720f, 56f);

            Image bg = panel.GetComponent<Image>();
            bg.color = PanelDark;
            bg.raycastTarget = false;
            AddOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.35f), new Vector2(0f, -2f));

            HorizontalLayoutGroup layout = panel.GetComponent<HorizontalLayoutGroup>();
            Record(layout);
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            foreach (string name in new[] { "ConfirmButton", "CancelButton", "UndoButton", "RedoButton", "DemolishButton" })
            {
                GameObject buttonGo = Require("Canvas/ConstructionUI/Root/ActionBar/" + name);
                Record(buttonGo);
                RectTransform br = buttonGo.GetComponent<RectTransform>();
                br.sizeDelta = new Vector2(128f, 40f);
                LayoutElement le = EnsureComponent<LayoutElement>(buttonGo);
                Record(le);
                le.minWidth = 128f;
                le.preferredWidth = 128f;
                le.minHeight = 40f;
                le.preferredHeight = 40f;
                le.flexibleWidth = 0f;
                le.flexibleHeight = 0f;
                ApplyProjectButtonSkin(buttonGo);

                TMP_Text label = buttonGo.transform.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    Record(label);
                    ApplyDefaultFont(label);
                    label.fontSize = 14f;
                    label.fontStyle = FontStyles.Bold;
                    label.color = Ivory;
                    label.raycastTarget = false;
                    label.alignment = TextAlignmentOptions.Center;
                }
            }
            return 6;
        }

        private static int StyleConstructionClose()
        {
            GameObject go = Require("Canvas/ConstructionUI/Root/CloseButton");
            Record(go);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -18f);
            rect.sizeDelta = new Vector2(112f, 40f);
            ApplyProjectButtonSkin(go);
            TMP_Text label = go.transform.Find("Label")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                Record(label);
                ApplyDefaultFont(label);
                label.fontSize = 14f;
                label.fontStyle = FontStyles.Bold;
                label.color = Ivory;
                label.raycastTarget = false;
            }
            return 1;
        }

        private static int StylePreviewPanel()
        {
            GameObject panel = Require("Canvas/ConstructionUI/Root/PreviewPanelInfo");
            Record(panel);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-24f, 90f);
            rect.sizeDelta = new Vector2(320f, 380f);
            Image bg = panel.GetComponent<Image>();
            bg.color = new Color(0.045f, 0.05f, 0.04f, 0.97f);
            bg.raycastTarget = false;
            EnsureComponent<GameplayInputBlocker>(panel);
            AddOutline(panel, new Color(Gold.r, Gold.g, Gold.b, 0.50f), new Vector2(-2f, -2f));

            GameObject labelGo = Find("Canvas/ConstructionUI/Root/PreviewPanelInfo/Label Text");
            if (labelGo != null)
            {
                RectTransform lr = labelGo.GetComponent<RectTransform>();
                Record(lr);
                lr.anchorMin = new Vector2(0f, 1f);
                lr.anchorMax = new Vector2(1f, 1f);
                lr.pivot = new Vector2(0.5f, 1f);
                lr.anchoredPosition = new Vector2(0f, -14f);
                lr.sizeDelta = new Vector2(-28f, 54f);
                TMP_Text text = labelGo.GetComponent<TMP_Text>();
                ApplyDefaultFont(text);
                text.fontSize = 22f;
                text.color = Gold;
                text.raycastTarget = false;
            }

            GameObject infoGo = Find("Canvas/ConstructionUI/Root/PreviewPanelInfo/Info Text");
            if (infoGo != null)
            {
                RectTransform ir = infoGo.GetComponent<RectTransform>();
                Record(ir);
                ir.anchorMin = Vector2.zero;
                ir.anchorMax = Vector2.one;
                ir.offsetMin = new Vector2(18f, 18f);
                ir.offsetMax = new Vector2(-18f, -76f);
                TMP_Text text = infoGo.GetComponent<TMP_Text>();
                ApplyDefaultFont(text);
                text.fontSize = 15f;
                text.color = Ivory;
                text.raycastTarget = false;
            }
            return 3;
        }

        private static void EnsureHorizontalScrollbar(GameObject panel, ScrollRect scrollRect)
        {
            Transform existing = panel.transform.Find("HorizontalScrollbar");
            GameObject scrollbarGo = existing != null ? existing.gameObject : null;
            if (scrollbarGo == null)
            {
                scrollbarGo = new GameObject("HorizontalScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
                Undo.RegisterCreatedObjectUndo(scrollbarGo, "Create PC construction scrollbar");
                scrollbarGo.transform.SetParent(panel.transform, false);
            }

            RectTransform rect = scrollbarGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 6f);
            rect.sizeDelta = new Vector2(-28f, 10f);

            Image track = scrollbarGo.GetComponent<Image>();
            track.color = new Color(0.03f, 0.035f, 0.03f, 0.82f);
            track.raycastTarget = true;

            Transform slidingTransform = scrollbarGo.transform.Find("SlidingArea");
            GameObject sliding = slidingTransform != null ? slidingTransform.gameObject : null;
            if (sliding == null)
            {
                sliding = new GameObject("SlidingArea", typeof(RectTransform));
                Undo.RegisterCreatedObjectUndo(sliding, "Create scrollbar sliding area");
                sliding.transform.SetParent(scrollbarGo.transform, false);
            }
            RectTransform slidingRect = sliding.GetComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.offsetMin = new Vector2(2f, 2f);
            slidingRect.offsetMax = new Vector2(-2f, -2f);

            Transform handleTransform = sliding.transform.Find("Handle");
            GameObject handle = handleTransform != null ? handleTransform.gameObject : null;
            if (handle == null)
            {
                handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
                Undo.RegisterCreatedObjectUndo(handle, "Create scrollbar handle");
                handle.transform.SetParent(sliding.transform, false);
            }
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            Image handleImage = handle.GetComponent<Image>();
            handleImage.color = Gold;
            handleImage.raycastTarget = true;

            Scrollbar scrollbar = scrollbarGo.GetComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.direction = Scrollbar.Direction.LeftToRight;
            scrollbar.targetGraphic = handleImage;
            scrollRect.horizontalScrollbar = scrollbar;
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
            scrollRect.horizontalScrollbarSpacing = 4f;
        }

        private static int NormalizeGameplayFonts()
        {
            GameObject canvas = Find("Canvas");
            if (canvas == null)
                return 0;

            int changed = 0;
            TMP_FontAsset font = LoadDefaultFont();
            if (font == null)
                throw new InvalidOperationException($"Default TMP font not found at '{DefaultFontPath}'.");

            TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                TMP_Text text = texts[index];
                if (text == null || text.font == font)
                    continue;

                Record(text);
                text.font = font;
                changed++;
            }

            return changed;
        }

        private static void ApplyDefaultFont(TMP_Text text)
        {
            if (text == null)
                return;

            TMP_FontAsset font = LoadDefaultFont();
            if (font != null)
                text.font = font;
        }

        private static TMP_FontAsset LoadDefaultFont()
            => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontPath);

        private static void ApplyProjectButtonSkin(GameObject go)
        {
            Sprite idle = LoadSprite("Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Idle.png");
            Sprite hover = LoadSprite("Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Hover.png");
            Sprite clicked = LoadSprite("Assets/Moyva/Art/UI/Buttons/Blank/Blank-Button-Clicked.png");

            Image image = go.GetComponent<Image>();
            if (image != null)
            {
                Record(image);
                if (idle != null) image.sprite = idle;
                image.type = Image.Type.Sliced;
                image.color = Color.white;
                image.raycastTarget = true;
            }

            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                Record(button);
                if (idle != null && hover != null && clicked != null)
                {
                    button.transition = Selectable.Transition.SpriteSwap;
                    SpriteState state = button.spriteState;
                    state.highlightedSprite = hover;
                    state.selectedSprite = hover;
                    state.pressedSprite = clicked;
                    state.disabledSprite = clicked;
                    button.spriteState = state;
                }
                Navigation nav = button.navigation;
                nav.mode = Navigation.Mode.None;
                button.navigation = nav;
            }
        }

        private static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(go);
        }

        private static T RequireComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
                throw new InvalidOperationException($"{PathOf(go.transform)} has no {typeof(T).Name}.");
            return component;
        }

        private static void AddOutline(GameObject go, Color color, Vector2 distance)
        {
            Outline outline = go.GetComponent<Outline>();
            if (outline == null) outline = Undo.AddComponent<Outline>(go);
            Record(outline);
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void Record(UnityEngine.Object obj)
        {
            if (obj != null) Undo.RecordObject(obj, "Moyva Gameplay UI Pass73");
        }

        private static SerializedObject SerializedMono(GameObject go, string fullTypeName)
        {
            MonoBehaviour match = go.GetComponents<MonoBehaviour>()
                .FirstOrDefault(mb => mb != null && string.Equals(mb.GetType().FullName, fullTypeName, StringComparison.Ordinal));
            return match != null ? new SerializedObject(match) : null;
        }

        private static void SetObjectRef(SerializedObject so, string property, UnityEngine.Object value)
        {
            SerializedProperty p = so.FindProperty(property);
            if (p == null) throw new InvalidOperationException("Serialized property missing: " + property);
            p.objectReferenceValue = value;
        }

        private static string RefName(SerializedProperty property)
        {
            return property != null && property.objectReferenceValue != null
                ? property.objectReferenceValue.name
                : string.Empty;
        }

        private static GameObject Require(string path)
        {
            GameObject go = Find(path);
            if (go == null) throw new InvalidOperationException("Required object missing: " + path);
            return go;
        }

        private static GameObject Find(string path)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrWhiteSpace(path)) return null;
            string[] parts = path.Trim('/').Split('/');
            GameObject current = scene.GetRootGameObjects().FirstOrDefault(go => go.name == parts[0]);
            if (current == null) return null;
            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = null;
                for (int c = 0; c < current.transform.childCount; c++)
                {
                    Transform child = current.transform.GetChild(c);
                    if (child.name == parts[i]) { next = child; break; }
                }
                if (next == null) return null;
                current = next.gameObject;
            }
            return current;
        }

        private static IEnumerable<GameObject> SceneObjects(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                var stack = new Stack<Transform>();
                stack.Push(root.transform);
                while (stack.Count > 0)
                {
                    Transform t = stack.Pop();
                    yield return t.gameObject;
                    for (int i = t.childCount - 1; i >= 0; i--) stack.Push(t.GetChild(i));
                }
            }
        }

        private static string PathOf(Transform t)
        {
            if (t == null) return string.Empty;
            var names = new Stack<string>();
            while (t != null) { names.Push(t.name); t = t.parent; }
            return string.Join("/", names);
        }

        private static bool TryGetTargetScene(out Scene scene, out string error)
        {
            scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                error = "No valid active scene.";
                return false;
            }
            if (!string.Equals(scene.path, ScenePath, StringComparison.OrdinalIgnoreCase))
            {
                error = "Active scene is not canonical Gameplay scene: " + scene.path;
                return false;
            }
            error = string.Empty;
            return true;
        }

        private static string BackupScene(string assetPath)
        {
            try
            {
                string project = Directory.GetParent(Application.dataPath)?.FullName;
                if (string.IsNullOrWhiteSpace(project)) return string.Empty;
                string source = Path.Combine(project, assetPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(source)) return string.Empty;
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string root = Path.Combine(home, ".local", "share", "moyva-cli", "backups", "gameplay-ui-redesign", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string destination = Path.Combine(root, assetPath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(source, destination, true);
                if (File.Exists(source + ".meta")) File.Copy(source + ".meta", destination + ".meta", true);
                return destination;
            }
            catch (Exception e)
            {
                Debug.LogError("[MoyvaGameplayUI73] Backup failed: " + e);
                return string.Empty;
            }
        }

        private static GameplayUiRedesignResult New(string op, Scene scene)
        {
            return new GameplayUiRedesignResult
            {
                operation = op,
                scenePath = scene.IsValid() ? scene.path : string.Empty,
            };
        }

        private static void Add(GameplayUiRedesignResult result, string severity, string code, string path, string message)
        {
            result.checks.Add(new GameplayUiCheck { severity = severity, code = code, path = path, message = message });
        }

        private static string Fail(GameplayUiRedesignResult result, string message)
        {
            result.ok = false;
            result.errors++;
            result.message = message;
            return JsonUtility.ToJson(result, true);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class GameplayUiPass73Marker : MonoBehaviour
    {
        [SerializeField] private string marker = "MOYVA_GAMEPLAY_UI_PASS73";
    }
}
