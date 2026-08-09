using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using Sirenix.OdinInspector.Editor;
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

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            DrawToolbar();
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
                            "Нова будівля стане доступною системі після коректного налаштування.")))
                        CreateBuilding();
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Дублювати вибрану",
                            "Створює копію вибраної будівлі з новим ID.",
                            "Дає незалежну конфігурацію на основі існуючої.")))
                        DuplicateSelected();
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Видалити вибрану",
                            "Видаляє вибраний asset після підтвердження.",
                            "Будівля зникне з реєстру й не буде доступна в грі.")))
                        DeleteSelected();
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Перевірити реєстр",
                            "Запускає повну валідацію всіх будівель.",
                            "Знаходить конфігурації, які можуть зламати меню або будівництво.")))
                        ValidateRegistry();
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Перебудувати реєстр",
                            "Повторно збирає всі BuildingDefinition asset у реєстр.",
                            "Відновлює пропущені посилання без зміни самих будівель.")))
                        RebuildRegistryFromAssets();
                    if (GUILayout.Button(EditorTooltipStandard.Content(
                            "Мігрувати старі дані",
                            "Перетворює legacy inline definitions на окремі asset.",
                            "Зберігає старі будівлі в актуальному data-driven форматі.")))
                        MigrateLegacy();
                }
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
