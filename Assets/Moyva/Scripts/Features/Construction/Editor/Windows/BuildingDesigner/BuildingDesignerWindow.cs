using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingDesignerWindow : OdinMenuEditorWindow
    {
        private const string RegistryPrefKey = "Moyva.BuildingDesigner.AssetRegistryGuid";
        private const string OutputFolderPrefKey = "Moyva.BuildingDesigner.OutputFolder";
        private const string DefaultOutputFolder = "Assets/Moyva/Data/ScriptableObjects/Construction/Buildings";
        private const string ModuleLogTag = "[MoyvaConstructionModules]";

        [SerializeField] private BuildingRegistrySO _registry;
        [SerializeField] private BuildingTemplateLibrarySO _templateLibrary;
        [SerializeField] private BuildingArchetypeSO _newBuildingTemplate;
        [SerializeField] private string _newBuildingId = "new-building";
        [SerializeField] private string _newBuildingName = "Нова будівля";
        [SerializeField] private string _outputFolder = DefaultOutputFolder;
        [SerializeField] private bool _filterByCategory;
        [SerializeField] private BuildingCategory _categoryFilter = BuildingCategory.Civilian;
        [SerializeField] private bool _migrationAddsFogReveal = true;

        [MenuItem("Moyva/Tools/Building Designer", priority = 32)]
        public static void Open()
        {
            var window = GetWindow<BuildingDesignerWindow>("Редактор будівель");
            window.minSize = new Vector2(1080f, 680f);
            window.Show();
            window.Focus();
        }

        public static void OpenConstructionMenu()
        {
            Open();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadPreferences();
            _registry ??= FindFirstAsset<BuildingRegistrySO>();
            _templateLibrary ??= FindFirstAsset<BuildingTemplateLibrarySO>();
        }

        protected override void OnDisable()
        {
            SavePreferences();
            base.OnDisable();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree
            {
                Config =
                {
                    DrawSearchToolbar = true,
                    AutoHandleKeyboardNavigation = true,
                }
            };

            tree.Add("Бібліотека/Реєстр", _registry);
            tree.Add("Бібліотека/Шаблони", _templateLibrary);

            // MOYVA_CONTEXT_MENU_PASS64: category roots stay visible even when empty.
            tree.Add($"Бібліотека/{GetCategoryLabel(BuildingCategory.Military)}", null);
            tree.Add($"Бібліотека/{GetCategoryLabel(BuildingCategory.Civilian)}", null);
            tree.Add($"Бібліотека/{GetCategoryLabel(BuildingCategory.Industrial)}", null);
            tree.Add($"Бібліотека/{GetCategoryLabel(BuildingCategory.Walls)}", null);

            if (_registry == null)
            {
                tree.Add("Бібліотека/Реєстр не вибрано", this);
                return tree;
            }

            var assets = _registry.BuildingAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null)
                    continue;

                if (_filterByCategory && asset.Category != _categoryFilter)
                    continue;

                string label = string.IsNullOrWhiteSpace(asset.DisplayName) ? asset.name : asset.DisplayName;
                tree.Add($"Бібліотека/{GetCategoryLabel(asset.Category)}/{label}", asset);
            }

            var legacy = _registry.LegacyBuildings;
            for (int i = 0; i < legacy.Length; i++)
            {
                var definition = legacy[i];
                if (definition == null)
                    continue;

                if (_filterByCategory && definition.Category != _categoryFilter)
                    continue;

                if (_registry.GetAssetById(definition.Id) != null)
                    continue;

                string label = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName;
                tree.Add($"Бібліотека/Застарілі inline-дані/{label}", definition);
            }

            ConfigureTreeContextMenus(tree);
            ConfigureBuildingTreeIcons(tree); // MOYVA_BUILDING_ICON_PASS66_RIGHT
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            DrawToolbar();
            GUILayout.Space(6f);
            DrawSelectedBuildingIconHeader();
            GUILayout.Space(6f);
            Presets.BuildingDesignerPresetPanel.Draw(
                _registry,
                () => ForceMenuTreeRebuild());
            GUILayout.Space(6f);
            DrawSelectedBuildingModuleHealth();
            GUILayout.Space(6f);
            base.OnBeginDrawEditors();
        }

        private void DrawSelectedBuildingModuleHealth()
        {
            BuildingDefinitionAsset asset =
                MenuTree?.Selection?.SelectedValue
                    as BuildingDefinitionAsset;
            if (asset == null)
                return;

            IReadOnlyList<BuildingValidationIssue> issues =
                asset.ValidationIssues
                ?? Array.Empty<BuildingValidationIssue>();
            List<BuildingModuleDefinition> modules =
                asset.Modules;

            int enabledModules = 0;
            int runtimeConsumers = 0;
            int errors = 0;
            int warnings = 0;
            var activeNames = new List<string>();

            if (modules != null)
            {
                for (int index = 0;
                     index < modules.Count;
                     index++)
                {
                    BuildingModuleDefinition module =
                        modules[index];
                    if (module == null || !module.IsEnabled)
                        continue;

                    enabledModules++;
                    if (BuildingDefinitionCapabilities
                        .HasRuntimeConsumer(module.GetType()))
                    {
                        runtimeConsumers++;
                    }

                    BuildingModuleEditorDescriptor descriptor =
                        BuildingModuleEditorCatalog.Find(
                            module.GetType());
                    activeNames.Add(
                        descriptor?.DisplayName
                        ?? module.GetType().Name);
                }
            }

            for (int index = 0; index < issues.Count; index++)
            {
                BuildingValidationIssue issue = issues[index];
                if (issue == null)
                    continue;
                if (issue.Severity == BuildingValidationSeverity.Error)
                    errors++;
                else if (issue.Severity == BuildingValidationSeverity.Warning)
                    warnings++;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        $"Runtime-модулі: {runtimeConsumers}/{enabledModules}",
                        EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(
                        $"Помилки: {errors}   Попередження: {warnings}",
                        EditorStyles.miniBoldLabel,
                        GUILayout.Width(220f));
                }

                string moduleText = activeNames.Count == 0
                    ? "Активних модулів немає."
                    : string.Join("  •  ", activeNames);
                EditorGUILayout.LabelField(
                    moduleText,
                    EditorStyles.wordWrappedMiniLabel);

                MessageType statusType = errors > 0
                    ? MessageType.Error
                    : warnings > 0
                        ? MessageType.Warning
                        : MessageType.Info;
                EditorGUILayout.HelpBox(
                    errors > 0
                        ? "Конфігурація має runtime-блокуючі помилки."
                        : runtimeConsumers != enabledModules
                            ? "Є активний модуль без runtime consumer."
                            : "Усі активні модулі мають runtime consumer.",
                    statusType);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Оновити runtime"))
                    {
                        asset.NotifyEditorDataChanged();
                        EditorUtility.SetDirty(asset);
                        Repaint();
                        Debug.Log(
                            $"{ModuleLogTag} editor runtime refresh " +
                            $"building={asset.Id} revision=" +
                            BuildingDefinitionAsset.RuntimeRevision);
                    }

                    if (GUILayout.Button("Вивести перевірку в Console"))
                    {
                        Debug.Log(
                            $"{ModuleLogTag} validation building={asset.Id} " +
                            $"modules={enabledModules} errors={errors} warnings={warnings}");
                        for (int index = 0; index < issues.Count; index++)
                        {
                            BuildingValidationIssue issue = issues[index];
                            if (issue != null)
                                Debug.Log(
                                    $"{ModuleLogTag} {issue.Severity} " +
                                    $"{issue.Code}: {issue.Message}");
                        }
                    }
                }
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField(
                        EditorTooltipStandard.Content(
                            "Реєстр",
                            "Вибирає реєстр, із якого завантажуються будівлі.",
                            "Визначає набір будівель, доступних у грі та цьому редакторі."),
                        _registry,
                        typeof(BuildingRegistrySO),
                        false);
                    _templateLibrary = (BuildingTemplateLibrarySO)EditorGUILayout.ObjectField(
                        EditorTooltipStandard.Content(
                            "Шаблони",
                            "Вибирає бібліотеку заготовок будівель.",
                            "Дозволяє швидко створювати узгоджені конфігурації."),
                        _templateLibrary,
                        typeof(BuildingTemplateLibrarySO),
                        false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SavePreferences();
                        ForceMenuTreeRebuild();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _outputFolder = EditorGUILayout.TextField(
                        EditorTooltipStandard.Content(
                            "Папка збереження",
                            "Задає папку для нових BuildingDefinition asset.",
                            "Не впливає на runtime, але визначає структуру проєкту."),
                        string.IsNullOrWhiteSpace(_outputFolder) ? DefaultOutputFolder : _outputFolder);
                    if (GUILayout.Button(
                            EditorTooltipStandard.Content(
                                "Обрати",
                                "Відкриває вибір папки всередині Assets.",
                                "Змінює місце створення нових building asset."),
                            GUILayout.Width(64f)))
                        PickOutputFolder();
                    _filterByCategory = EditorGUILayout.ToggleLeft(
                        EditorTooltipStandard.Content(
                            "Фільтр",
                            "Вмикає показ лише однієї категорії.",
                            "Не змінює реєстр або доступність будівель у грі."),
                        _filterByCategory,
                        GUILayout.Width(72f));
                    using (new EditorGUI.DisabledScope(!_filterByCategory))
                        _categoryFilter = (BuildingCategory)EditorGUILayout.EnumPopup(
                            EditorTooltipStandard.Content(
                                "Категорія",
                                "Вибирає категорію для фільтра дерева.",
                                "Не змінює категорії самих будівель."),
                            _categoryFilter,
                            GUILayout.Width(190f));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _newBuildingId = EditorGUILayout.TextField(
                        EditorTooltipStandard.Content(
                            "Новий ID",
                            "Задає технічний ID нової будівлі.",
                            "Після створення ID використовується збереженням і мережею."),
                        _newBuildingId);
                    _newBuildingName = EditorGUILayout.TextField(
                        EditorTooltipStandard.Content(
                            "Назва",
                            "Задає видиму назву нової будівлі.",
                            "Відображається гравцю в меню та панелях."),
                        _newBuildingName);
                    _newBuildingTemplate = (BuildingArchetypeSO)EditorGUILayout.ObjectField(
                        EditorTooltipStandard.Content(
                            "Шаблон",
                            "Застосовує початковий набір полів і модулів.",
                            "Прискорює створення будівель однакового архетипу."),
                        _newBuildingTemplate,
                        typeof(BuildingArchetypeSO),
                        false,
                        GUILayout.Width(250f));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Нова будівля",
                            "Створює новий BuildingDefinition asset і додає його до реєстру.",
                            "Для створення одразу в конкретній категорії натисніть ПКМ по групі зліва."),
                            GUILayout.Width(180f)))
                        CreateBuilding();

                    GUILayout.Space(8f);
                    EditorGUILayout.LabelField(
                        "ПКМ по будівлі або групі зліва — додати, дублювати, видалити, перевірити та інші дії.",
                        EditorStyles.miniLabel);
                }
            }
        }

        private static readonly Dictionary<int, Texture> BuildingMenuIconPreviewCache =
            new Dictionary<int, Texture>();

        private void ConfigureBuildingTreeIcons(OdinMenuTree tree)
        {
            if (tree == null)
                return;

            if (tree.DefaultMenuStyle != null)
                tree.DefaultMenuStyle.Height = Mathf.Max(tree.DefaultMenuStyle.Height, 24);

            foreach (OdinMenuItem menuItem in tree.EnumerateTree())
            {
                if (menuItem.Value is BuildingDefinitionAsset asset)
                {
                    BuildingDefinitionAsset capturedAsset = asset;

                    // Built-in Odin icons appear on the left; Pass66 needs the icon on the right.
                    menuItem.Icon = null;
                    menuItem.IconSelected = null;
                    menuItem.IconGetter = null;
                    menuItem.OnDrawItem += item =>
                        DrawBuildingMenuIconRight(item, capturedAsset?.Presentation?.Icon);
                    continue;
                }

                if (menuItem.Value is BuildingDefinition legacy)
                {
                    BuildingDefinition capturedLegacy = legacy;
                    menuItem.Icon = null;
                    menuItem.IconSelected = null;
                    menuItem.IconGetter = null;
                    menuItem.OnDrawItem += item =>
                        DrawBuildingMenuIconRight(item, capturedLegacy?.Icon);
                }
            }
        }

        private static void DrawBuildingMenuIconRight(OdinMenuItem menuItem, Sprite sprite)
        {
            // No assigned Sprite means no icon at all.
            if (menuItem == null || sprite == null || Event.current.type != EventType.Repaint)
                return;

            Texture preview = ResolveBuildingMenuIcon(sprite);
            if (preview == null)
                return;

            Rect row = menuItem.Rect;
            if (row.width <= 1f || row.height <= 1f)
                return;

            float size = Mathf.Clamp(row.height - 5f, 14f, 20f);
            const float rightPadding = 6f;
            Rect iconRect = new Rect(
                row.xMax - rightPadding - size,
                row.y + (row.height - size) * 0.5f,
                size,
                size);

            GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit, true);
        }

        private static Texture ResolveBuildingMenuIcon(Sprite sprite)
        {
            if (sprite == null)
                return null;

            int key = sprite.GetInstanceID();
            if (BuildingMenuIconPreviewCache.TryGetValue(key, out Texture cached) && cached != null)
                return cached;

            Texture preview = AssetPreview.GetAssetPreview(sprite);
            if (preview != null)
            {
                BuildingMenuIconPreviewCache[key] = preview;
                return preview;
            }

            return AssetPreview.GetMiniThumbnail(sprite);
        }

        private void DrawSelectedBuildingIconHeader()
        {
            object selectedValue = MenuTree?.Selection?.SelectedValue;
            if (selectedValue is BuildingDefinitionAsset asset)
            {
                DrawBuildingIconHeader(
                    asset.DisplayName,
                    asset.Id,
                    asset.Category.ToString(),
                    asset.Presentation?.Icon,
                    nextIcon => SetBuildingAssetIcon(asset, nextIcon),
                    false);
                return;
            }

            if (selectedValue is BuildingDefinition legacy)
            {
                DrawBuildingIconHeader(
                    string.IsNullOrWhiteSpace(legacy.DisplayName) ? legacy.Id : legacy.DisplayName,
                    legacy.Id,
                    legacy.Category.ToString(),
                    legacy.Icon,
                    nextIcon => SetLegacyBuildingIcon(legacy, nextIcon),
                    true);
            }
        }

        private void DrawBuildingIconHeader(
            string displayName,
            string id,
            string category,
            Sprite icon,
            Action<Sprite> assignIcon,
            bool legacy)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        GUILayout.Space(4f);
                        EditorGUILayout.LabelField(
                            string.IsNullOrWhiteSpace(displayName) ? "Будівля" : displayName,
                            EditorStyles.boldLabel);
                        EditorGUILayout.LabelField($"ID: {id}", EditorStyles.miniLabel);
                        EditorGUILayout.LabelField($"Категорія: {category}", EditorStyles.miniLabel);
                        if (legacy)
                            EditorGUILayout.LabelField("Legacy inline building", EditorStyles.miniLabel);

                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField(
                            icon == null
                                ? "Іконку конструкції не задано."
                                : "Це іконка, яку використовує ця конструкція в UI.",
                            EditorStyles.wordWrappedMiniLabel);
                    }

                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(132f)))
                    {
                        var centeredMini = new GUIStyle(EditorStyles.miniBoldLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        EditorGUILayout.LabelField("Іконка конструкції", centeredMini);

                        EditorGUI.BeginChangeCheck();
                        // MOYVA_ODIN_PREVIEW_RECT_COMPAT_FIX4
                        Rect iconFieldRect = GUILayoutUtility.GetRect(
                            112f,
                            112f,
                            GUILayout.Width(112f),
                            GUILayout.Height(112f));
                        Texture iconPreview = ResolveBuildingMenuIcon(icon);
                        var nextIcon = (Sprite)SirenixEditorFields.UnityPreviewObjectField(
                            iconFieldRect,
                            icon,
                            iconPreview,
                            typeof(Sprite),
                            false);
if (EditorGUI.EndChangeCheck())
                            assignIcon?.Invoke(nextIcon);
                    }
                }
            }
        }

        private void SetBuildingAssetIcon(BuildingDefinitionAsset asset, Sprite nextIcon)
        {
            if (asset == null)
                return;

            asset.Presentation ??= new BuildingPresentation();
            if (asset.Presentation.Icon == nextIcon)
                return;

            Undo.RecordObject(asset, "Building Designer: change building icon");
            asset.Presentation.Icon = nextIcon;
            asset.NotifyEditorDataChanged();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssetIfDirty(asset);

            BuildingMenuIconPreviewCache.Clear();
            Repaint();
            Debug.Log($"[BuildingDesigner] Icon changed: '{asset.Id}' -> {(nextIcon != null ? nextIcon.name : "<none>")}.");
        }

        private void SetLegacyBuildingIcon(BuildingDefinition legacy, Sprite nextIcon)
        {
            if (legacy == null || legacy.Icon == nextIcon || _registry == null)
                return;

            Undo.RecordObject(_registry, "Building Designer: change legacy building icon");
            legacy.Icon = nextIcon;
            EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssetIfDirty(_registry);

            BuildingMenuIconPreviewCache.Clear();
            Repaint();
            Debug.Log($"[BuildingDesigner] Legacy icon changed: '{legacy.Id}' -> {(nextIcon != null ? nextIcon.name : "<none>")}.");
        }

        // MOYVA_CONTEXT_MENU_PASS64
        private void ConfigureTreeContextMenus(OdinMenuTree tree)
        {
            if (tree == null)
                return;

            var libraryItem = tree.GetMenuItem("Бібліотека");
            if (libraryItem != null)
                libraryItem.OnRightClick += _ => ShowLibraryContextMenu();

            var registryItem = tree.GetMenuItem("Бібліотека/Реєстр");
            if (registryItem != null)
                registryItem.OnRightClick += _ => ShowRegistryContextMenu();

            var templatesItem = tree.GetMenuItem("Бібліотека/Шаблони");
            if (templatesItem != null)
                templatesItem.OnRightClick += _ => ShowTemplatesContextMenu();

            BuildingCategory[] categories =
            {
                BuildingCategory.Military,
                BuildingCategory.Civilian,
                BuildingCategory.Industrial,
                BuildingCategory.Walls,
            };
            for (int i = 0; i < categories.Length; i++)
            {
                BuildingCategory category = categories[i];
                var categoryItem = tree.GetMenuItem($"Бібліотека/{GetCategoryLabel(category)}");
                if (categoryItem == null)
                    continue;

                BuildingCategory capturedCategory = category;
                categoryItem.OnRightClick += _ => ShowCategoryContextMenu(capturedCategory);
            }

            var legacyItem = tree.GetMenuItem("Бібліотека/Застарілі inline-дані");
            if (legacyItem != null)
                legacyItem.OnRightClick += _ => ShowLegacyContextMenu();

            foreach (OdinMenuItem menuItem in tree.EnumerateTree())
            {
                if (!(menuItem.Value is BuildingDefinitionAsset asset))
                    continue;

                BuildingDefinitionAsset capturedAsset = asset;
                OdinMenuItem capturedItem = menuItem;
                menuItem.OnRightClick += _ => ShowBuildingContextMenu(capturedItem, capturedAsset);
            }
        }

        private void ShowLibraryContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Нова будівля"), false, CreateBuilding);
            menu.AddSeparator(string.Empty);
            AddRegistryActions(menu);
            menu.ShowAsContext();
        }

        private void ShowRegistryContextMenu()
        {
            var menu = new GenericMenu();
            if (_registry != null)
            {
                menu.AddItem(new GUIContent("Показати реєстр у Project"), false, () => PingAsset(_registry));
                menu.AddSeparator(string.Empty);
                AddRegistryActions(menu);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Реєстр не вибрано"));
            }
            menu.ShowAsContext();
        }

        private void ShowTemplatesContextMenu()
        {
            var menu = new GenericMenu();
            if (_templateLibrary != null)
                menu.AddItem(new GUIContent("Показати бібліотеку шаблонів у Project"), false, () => PingAsset(_templateLibrary));
            else
                menu.AddDisabledItem(new GUIContent("Бібліотеку шаблонів не вибрано"));
            menu.ShowAsContext();
        }

        private void ShowCategoryContextMenu(BuildingCategory category)
        {
            string label = GetCategoryLabel(category);
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent($"Додати нову будівлю в «{label}»"), false, () => CreateBuildingInCategory(category));
            menu.AddItem(new GUIContent("Перевірити будівлі цієї групи"), false, () => ValidateCategory(category));
            menu.AddSeparator(string.Empty);

            bool filteredHere = _filterByCategory && _categoryFilter == category;
            if (filteredHere)
                menu.AddItem(new GUIContent("Показати всі групи"), false, ClearCategoryFilter);
            else
                menu.AddItem(new GUIContent("Показати тільки цю групу"), false, () => FilterToCategory(category));

            menu.ShowAsContext();
        }

        private void ShowLegacyContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Мігрувати всі старі дані"), false, MigrateLegacy);
            menu.AddItem(new GUIContent("Перевірити реєстр"), false, ValidateRegistry);
            menu.ShowAsContext();
        }

        private void ShowBuildingContextMenu(OdinMenuItem item, BuildingDefinitionAsset asset)
        {
            if (asset == null)
                return;

            item?.Select();
            Selection.activeObject = asset;

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Дублювати будівлю"), false, DuplicateSelected);
            menu.AddItem(new GUIContent("Видалити будівлю…"), false, DeleteSelected);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Перевірити цю будівлю"), false, () => ValidateBuilding(asset));
            menu.AddItem(new GUIContent("Показати asset у Project"), false, () => PingAsset(asset));

            AddPresetContextActions(menu, asset);
            menu.ShowAsContext();
        }

        private void AddRegistryActions(GenericMenu menu)
        {
            if (_registry == null)
            {
                menu.AddDisabledItem(new GUIContent("Перевірити реєстр"));
                menu.AddDisabledItem(new GUIContent("Перебудувати реєстр"));
                return;
            }

            menu.AddItem(new GUIContent("Перевірити реєстр"), false, ValidateRegistry);
            menu.AddItem(new GUIContent("Перебудувати реєстр з BuildingDefinition assets"), false, RebuildRegistryFromAssets);
            if (_registry.LegacyBuildings != null && _registry.LegacyBuildings.Length > 0)
                menu.AddItem(new GUIContent("Мігрувати старі inline-дані"), false, MigrateLegacy);
        }

        private void AddPresetContextActions(GenericMenu menu, BuildingDefinitionAsset asset)
        {
            try
            {
                var pack = Presets.BuildingJsonPresetSerializer.LoadPack();
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("JSON Presets/Експортувати цю будівлю в clipboard"), false, () => ExportBuildingJson(asset));

                for (int i = 0; i < pack.PresetIds.Count; i++)
                {
                    string presetId = pack.PresetIds[i];
                    string capturedPresetId = presetId;
                    menu.AddItem(
                        new GUIContent($"JSON Presets/Застосувати/{presetId}"),
                        false,
                        () => ApplyPresetToBuilding(capturedPresetId, asset));
                }
            }
            catch (Exception ex)
            {
                menu.AddSeparator(string.Empty);
                menu.AddDisabledItem(new GUIContent("JSON Presets/Pack недоступний: " + ex.Message));
            }
        }

        private void CreateBuildingInCategory(BuildingCategory category)
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Спочатку виберіть BuildingRegistry.");
                return;
            }

            EnsureOutputFolder();
            var asset = CreateInstance<BuildingDefinitionAsset>();
            asset.Identity.Id = SanitizeId(_newBuildingId);
            asset.Identity.DisplayName = string.IsNullOrWhiteSpace(_newBuildingName)
                ? asset.Identity.Id
                : _newBuildingName.Trim();
            _newBuildingTemplate?.ApplyTo(asset);
            asset.Identity.Category = category;
            asset.Normalize();

            string path = AssetDatabase.GenerateUniqueAssetPath($"{_outputFolder}/{SanitizeFileName(asset.Id)}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AddAssetToRegistry(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;

            if (_filterByCategory)
                _categoryFilter = category;

            ForceMenuTreeRebuild();
            Debug.Log($"[BuildingDesigner] Створено '{asset.Id}' у групі '{GetCategoryLabel(category)}'.");
        }

        private void ValidateBuilding(BuildingDefinitionAsset asset)
        {
            if (asset == null)
                return;

            var issues = BuildingValidator.Validate(asset.ToRuntimeDefinition());
            int errors = 0;
            int warnings = 0;
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] == null)
                    continue;
                if (issues[i].Severity == BuildingValidationSeverity.Error)
                    errors++;
                else if (issues[i].Severity == BuildingValidationSeverity.Warning)
                    warnings++;
                Debug.Log($"[BuildingDesigner] {asset.Id}: {issues[i].Severity} {issues[i].Code}: {issues[i].Message}");
            }

            Debug.Log($"[BuildingDesigner] Перевірка '{asset.Id}': помилок={errors}, попереджень={warnings}, усього={issues.Count}");
            EditorUtility.DisplayDialog(
                "Перевірка будівлі",
                $"{asset.DisplayName}\n\nПомилок: {errors}\nПопереджень: {warnings}\nУсього: {issues.Count}",
                "OK");
        }

        private void ValidateCategory(BuildingCategory category)
        {
            if (_registry == null)
                return;

            int buildings = 0;
            int errors = 0;
            int warnings = 0;
            var assets = _registry.BuildingAssets;
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (asset == null || asset.Category != category)
                    continue;

                buildings++;
                var issues = BuildingValidator.Validate(asset.ToRuntimeDefinition());
                for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++)
                {
                    var issue = issues[issueIndex];
                    if (issue == null)
                        continue;
                    if (issue.Severity == BuildingValidationSeverity.Error)
                        errors++;
                    else if (issue.Severity == BuildingValidationSeverity.Warning)
                        warnings++;
                    Debug.Log($"[BuildingDesigner] {asset.Id}: {issue.Severity} {issue.Code}: {issue.Message}");
                }
            }

            string groupName = GetCategoryLabel(category);
            Debug.Log($"[BuildingDesigner] Група '{groupName}': будівель={buildings}, помилок={errors}, попереджень={warnings}");
            EditorUtility.DisplayDialog(
                "Перевірка групи",
                $"{groupName}\n\nБудівель: {buildings}\nПомилок: {errors}\nПопереджень: {warnings}",
                "OK");
        }

        private void FilterToCategory(BuildingCategory category)
        {
            _filterByCategory = true;
            _categoryFilter = category;
            ForceMenuTreeRebuild();
        }

        private void ClearCategoryFilter()
        {
            _filterByCategory = false;
            ForceMenuTreeRebuild();
        }

        private static void PingAsset(UnityEngine.Object asset)
        {
            if (asset == null)
                return;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private void ApplyPresetToBuilding(string presetId, BuildingDefinitionAsset asset)
        {
            if (asset == null)
                return;

            try
            {
                var result = Presets.BuildingPresetBatchService.ApplyPresetToSelected(presetId, asset, true);
                Debug.Log($"[MoyvaBuildingPresets] Context apply '{presetId}' -> '{asset.Id}': {result.Summary}");
                Selection.activeObject = asset;
                ForceMenuTreeRebuild();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaBuildingPresets] Context apply failed '{presetId}' -> '{asset.Id}': {ex}");
                EditorUtility.DisplayDialog("Building Presets", ex.Message, "OK");
            }
        }

        private static void ExportBuildingJson(BuildingDefinitionAsset asset)
        {
            if (asset == null)
                return;

            try
            {
                EditorGUIUtility.systemCopyBuffer = Presets.BuildingJsonPresetSerializer.ExportSelected(asset);
                Debug.Log($"[MoyvaBuildingPresets] Exported '{asset.Id}' JSON to clipboard from context menu.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaBuildingPresets] Context export failed for '{asset.Id}': {ex}");
                EditorUtility.DisplayDialog("Building Presets", ex.Message, "OK");
            }
        }

        private void CreateBuilding()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Спочатку виберіть BuildingRegistry.");
                return;
            }

            EnsureOutputFolder();
            var asset = CreateInstance<BuildingDefinitionAsset>();
            asset.Identity.Id = SanitizeId(_newBuildingId);
            asset.Identity.DisplayName = string.IsNullOrWhiteSpace(_newBuildingName) ? asset.Identity.Id : _newBuildingName.Trim();
            _newBuildingTemplate?.ApplyTo(asset);
            asset.Normalize();

            string path = AssetDatabase.GenerateUniqueAssetPath($"{_outputFolder}/{SanitizeFileName(asset.Id)}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AddAssetToRegistry(asset);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            ForceMenuTreeRebuild();
        }

        private void DuplicateSelected()
        {
            var selected = MenuTree?.Selection?.SelectedValue as BuildingDefinitionAsset;
            if (selected == null)
            {
                Debug.LogWarning("[BuildingDesigner] Виберіть BuildingDefinition asset для дублювання.");
                return;
            }

            EnsureOutputFolder();
            var clone = Instantiate(selected);
            clone.Identity.Id = AssetDatabase.GenerateUniqueAssetPath($"{_outputFolder}/{SanitizeFileName(selected.Id)}.asset")
                .Replace(_outputFolder + "/", string.Empty)
                .Replace(".asset", string.Empty);
            clone.Identity.DisplayName = selected.DisplayName + " Copy";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{_outputFolder}/{SanitizeFileName(clone.Id)}.asset");
            AssetDatabase.CreateAsset(clone, path);
            AddAssetToRegistry(clone);
            AssetDatabase.SaveAssets();
            Selection.activeObject = clone;
            ForceMenuTreeRebuild();
        }

        private void DeleteSelected()
        {
            var selected = MenuTree?.Selection?.SelectedValue as BuildingDefinitionAsset;
            if (selected == null)
            {
                Debug.LogWarning("[BuildingDesigner] Виберіть BuildingDefinition asset для видалення.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (!EditorUtility.DisplayDialog(
                    "Видалення будівлі",
                    $"Видалити «{selected.DisplayName}»?\n{path}",
                    "Видалити",
                    "Скасувати"))
                return;

            RemoveAssetFromRegistry(selected);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            ForceMenuTreeRebuild();
        }

        private void ValidateRegistry()
        {
            var issues = BuildingValidator.ValidateRegistry(_registry);
            int errors = 0;
            int warnings = 0;
            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i] == null)
                    continue;
                if (issues[i].Severity == BuildingValidationSeverity.Error)
                    errors++;
                else if (issues[i].Severity == BuildingValidationSeverity.Warning)
                    warnings++;
            }

            Debug.Log($"[BuildingDesigner] Перевірка реєстру: помилок={errors}, попереджень={warnings}, усього={issues.Count}");
            for (int i = 0; i < issues.Count; i++)
                Debug.Log($"[BuildingDesigner] {issues[i].Severity} {issues[i].Code}: {issues[i].Message}");
        }

        private void RebuildRegistryFromAssets()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Спочатку виберіть BuildingRegistry.");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:BuildingDefinitionAsset");
            var assets = new List<BuildingDefinitionAsset>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<BuildingDefinitionAsset>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            _registry.SetBuildingAssets(assets);
            EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssets();
            ForceMenuTreeRebuild();
            Debug.Log($"[BuildingDesigner] Реєстр перебудовано: {assets.Count} BuildingDefinition asset.");
        }

        private void MigrateLegacy()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Спочатку виберіть BuildingRegistry.");
                return;
            }

            var report = BuildingMigrationUtility.MigrateLegacyRegistry(_registry, _outputFolder, _migrationAddsFogReveal);
            Debug.Log($"[BuildingDesigner] Міграцію завершено: {report}\n{string.Join("\n", report.Messages)}");
            ForceMenuTreeRebuild();
        }

        private void AddAssetToRegistry(BuildingDefinitionAsset asset)
        {
            if (_registry == null || asset == null)
                return;

            var assets = new List<BuildingDefinitionAsset>(_registry.BuildingAssets);
            if (!assets.Contains(asset))
                assets.Add(asset);
            _registry.SetBuildingAssets(assets);
            EditorUtility.SetDirty(_registry);
        }

        private void RemoveAssetFromRegistry(BuildingDefinitionAsset asset)
        {
            if (_registry == null || asset == null)
                return;

            var assets = new List<BuildingDefinitionAsset>(_registry.BuildingAssets);
            assets.Remove(asset);
            _registry.SetBuildingAssets(assets);
            EditorUtility.SetDirty(_registry);
        }

        private void PickOutputFolder()
        {
            string selected = EditorUtility.OpenFolderPanel("Папка для Building Definition", Application.dataPath, string.Empty);
            if (string.IsNullOrWhiteSpace(selected))
                return;

            selected = selected.Replace('\\', '/');
            string assetsPath = Application.dataPath.Replace('\\', '/');
            if (!selected.StartsWith(assetsPath, StringComparison.Ordinal))
            {
                Debug.LogWarning("[BuildingDesigner] Папка має бути всередині Assets.");
                return;
            }

            _outputFolder = "Assets" + selected.Substring(assetsPath.Length);
            SavePreferences();
        }

        private void EnsureOutputFolder()
        {
            if (string.IsNullOrWhiteSpace(_outputFolder))
                _outputFolder = DefaultOutputFolder;

            _outputFolder = _outputFolder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(_outputFolder))
                return;

            string[] parts = _outputFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private void LoadPreferences()
        {
            _outputFolder = EditorPrefs.GetString(OutputFolderPrefKey, DefaultOutputFolder);
            string registryGuid = EditorPrefs.GetString(RegistryPrefKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(registryGuid))
            {
                string path = AssetDatabase.GUIDToAssetPath(registryGuid);
                _registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
            }
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(OutputFolderPrefKey, string.IsNullOrWhiteSpace(_outputFolder) ? DefaultOutputFolder : _outputFolder);
            if (_registry != null)
                EditorPrefs.SetString(RegistryPrefKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_registry)));
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static string SanitizeId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "new-building";

            id = id.Trim().ToLowerInvariant();
            var chars = id.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]) && chars[i] != '-' && chars[i] != '_')
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }

        private static string SanitizeFileName(string value)
        {
            value = SanitizeId(value);
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '-');
            return string.IsNullOrWhiteSpace(value) ? "building-definition" : value;
        }

        private static string GetCategoryLabel(BuildingCategory category)
        {
            return category switch
            {
                BuildingCategory.Military => "Військові",
                BuildingCategory.Civilian => "Цивільні",
                BuildingCategory.Industrial => "Промислові",
                BuildingCategory.Walls => "Стіни та ворота",
                _ => category.ToString(),
            };
        }
    }
}
