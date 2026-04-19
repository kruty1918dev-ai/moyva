using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public static class BuildingModuleEditorShared
    {
        private sealed class ModuleOption
        {
            public ModuleOption(string typeName, string displayName, string description, Func<BuildingModuleDefinition> factory)
            {
                TypeName = typeName;
                DisplayName = displayName;
                Description = description;
                Factory = factory;
            }

            public string TypeName { get; }
            public string DisplayName { get; }
            public string Description { get; }
            public Func<BuildingModuleDefinition> Factory { get; }
        }

        private sealed class ModulePickerPopup : PopupWindowContent
        {
            private readonly SerializedProperty _modulesProp;
            private readonly Action _onChanged;
            private readonly List<BuildingModuleDefinition> _currentModules;
            private Vector2 _scroll;

            public ModulePickerPopup(SerializedProperty modulesProp, Action onChanged)
            {
                _modulesProp = modulesProp;
                _onChanged = onChanged;
                _currentModules = ExtractCurrentModules(modulesProp);
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(460f, 390f);
            }

            public override void OnGUI(Rect rect)
            {
                EditorGUILayout.LabelField("Оберіть модуль", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Сіризовані модулі заблоковані через конфлікти. Наведіть курсор на пункт для опису.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(6f);

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                for (int i = 0; i < ModuleOptions.Length; i++)
                {
                    DrawOption(ModuleOptions[i]);
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(6f);
                string tooltip = string.IsNullOrWhiteSpace(GUI.tooltip)
                    ? "Опис модуля з'явиться тут при наведенні курсора на пункт списку."
                    : GUI.tooltip;
                EditorGUILayout.HelpBox(tooltip, MessageType.None);
            }

            private void DrawOption(ModuleOption option)
            {
                string conflictReason = GetModuleConflictReason(option);
                bool isConflicted = !string.IsNullOrEmpty(conflictReason);

                EditorGUILayout.BeginVertical("box");
                
                string displayText = option.DisplayName;
                if (isConflicted)
                    displayText = $"❌ {displayText}";

                EditorGUI.BeginDisabledGroup(isConflicted);
                
                string tooltip = isConflicted ? $"{option.Description}\n\n⚠️ Конфлікт: {conflictReason}" : option.Description;
                if (GUILayout.Button(new GUIContent(displayText, tooltip), EditorStyles.miniButton))
                {
                    AddModule(_modulesProp, option.Factory(), _onChanged);
                    editorWindow.Close();
                }

                EditorGUI.EndDisabledGroup();

                string description = isConflicted 
                    ? $"{option.Description}\n<color=red>⚠️ {conflictReason}</color>"
                    : option.Description;
                    
                EditorGUILayout.LabelField(new GUIContent(description), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2f);
            }

            private string GetModuleConflictReason(ModuleOption option)
            {
                // Перевіряємо правила сумісності
                bool hasTownHall = HasModule<TownHallBuildingModule>();
                bool hasHousing = HasModule<HousingBuildingModule>();
                bool hasWorkerless = HasModule<WorkerlessBuildingModule>();
                bool hasWall = HasModule<WallBuildingModule>();
                bool hasGate = HasModule<GateBuildingModule>();
                bool hasProduction = HasModule<ProductionBuildingModule>();

                switch (option.TypeName)
                {
                    case "TownHallBuildingModule":
                        if (hasHousing) return "не може бути з Housing";
                        break;
                    
                    case "HousingBuildingModule":
                        if (hasTownHall) return "не може бути з TownHall";
                        if (hasWorkerless || hasWall || hasGate) return "не може бути з Workerless/Wall/Gate";
                        break;
                    
                    case "WorkerlessBuildingModule":
                    case "WallBuildingModule":
                    case "GateBuildingModule":
                        if (hasHousing) return "не може бути з Housing";
                        if (hasProduction) return "не може бути з Production";
                        break;
                    
                    case "ProductionBuildingModule":
                        if (hasWorkerless || hasWall || hasGate) return "не може бути з Workerless/Wall/Gate";
                        break;
                }

                return null;
            }

            private bool HasModule<T>() where T : BuildingModuleDefinition
            {
                if (_currentModules == null) return false;
                for (int i = 0; i < _currentModules.Count; i++)
                {
                    if (_currentModules[i] is T) return true;
                }
                return false;
            }
        }

        private static List<BuildingModuleDefinition> ExtractCurrentModules(SerializedProperty modulesProp)
        {
            var result = new List<BuildingModuleDefinition>();
            if (modulesProp == null || !modulesProp.isArray)
                return result;

            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var moduleProp = modulesProp.GetArrayElementAtIndex(i);
                if (moduleProp.managedReferenceValue is BuildingModuleDefinition module)
                    result.Add(module);
            }

            return result;
        }

        private static readonly ModuleOption[] ModuleOptions =
        {
            new ModuleOption(
                "HousingBuildingModule",
                "Житловий модуль",
                "Додає місткість житла та, за потреби, підтримку гарнізону для населення.",
                () => new HousingBuildingModule()),
            new ModuleOption(
                "TownHallBuildingModule",
                "Модуль ратуші",
                "Позначає будівлю як центр поселення та задає радіус його дії.",
                () => new TownHallBuildingModule()),
            new ModuleOption(
                "CastleBuildingModule",
                "Модуль замку",
                "Позначає окрему центральну точку з гарнізоном і радіусом виключення.",
                () => new CastleBuildingModule()),
            new ModuleOption(
                "WarehouseBuildingModule",
                "Модуль складу",
                "Надає будівлі функцію зберігання нехарчових ресурсів.",
                () => new WarehouseBuildingModule()),
            new ModuleOption(
                "BarnBuildingModule",
                "Модуль амбару",
                "Надає будівлі функцію зберігання харчових ресурсів.",
                () => new BarnBuildingModule()),
            new ModuleOption(
                "ProductionBuildingModule",
                "Виробничий модуль",
                "Додає виробництво ресурсу, кількість робітників і пріоритет будівлі.",
                () => new ProductionBuildingModule()),
            new ModuleOption(
                "TileRequirementBuildingModule",
                "Модуль вимог до тайлів",
                "Додає перевірку навколишніх тайлів: біом, радіус і мінімальну кількість.",
                () => new TileRequirementBuildingModule()),
            new ModuleOption(
                "WorkerlessBuildingModule",
                "Модуль без робітників",
                "Позначає будівлю як таку, що працює без призначення населення.",
                () => new WorkerlessBuildingModule()),
            new ModuleOption(
                "WallBuildingModule",
                "Модуль стіни",
                "Надає будівлі логіку стіни з міцністю та прапором прохідності.",
                () => new WallBuildingModule()),
            new ModuleOption(
                "GateBuildingModule",
                "Модуль воріт",
                "Надає будівлі логіку воріт з міцністю та швидкістю відкриття.",
                () => new GateBuildingModule()),
        };

        public static void DrawModulesSection(SerializedProperty modulesProp, GUIStyle sectionStyle = null, Action onChanged = null)
        {
            if (modulesProp == null || !modulesProp.isArray)
                return;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Модулі (нова модель)", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            DrawAddModuleMenu(modulesProp, onChanged);
            if (GUILayout.Button("Очистити модулі", EditorStyles.miniButton, GUILayout.Width(120f)))
            {
                modulesProp.arraySize = 0;
                onChanged?.Invoke();
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var moduleProp = modulesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical(sectionStyle ?? "box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(GetManagedReferenceTypeName(moduleProp), EditorStyles.miniBoldLabel);
                if (GUILayout.Button("Видалити", EditorStyles.miniButton, GUILayout.Width(80f)))
                {
                    modulesProp.DeleteArrayElementAtIndex(i);
                    onChanged?.Invoke();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(moduleProp, includeChildren: true);
                EditorGUILayout.EndVertical();
            }

            if (modulesProp.arraySize == 0)
                EditorGUILayout.HelpBox("Список модулів порожній. Додайте модулі для визначення можливостей будівлі.", MessageType.Warning);
        }

        public static void DrawValidationIssuesForDefinition(BuildingDefinition definition, SerializedProperty buildingProperty, Action onChanged = null, EconomyBuildingRules rules = null)
        {
            if (definition == null || buildingProperty == null)
                return;

            DrawValidationIssues(GetFilteredIssues(definition, rules), buildingProperty, onChanged);
        }

        public static void DrawValidationIssues(IReadOnlyList<BuildingValidationIssue> issues, SerializedProperty buildingProperty, Action onChanged = null)
        {
            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Модулі валідні.", MessageType.None);
                return;
            }

            for (int i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                if (issue == null)
                    continue;

                MessageType type = MessageType.Info;
                if (issue.Severity == BuildingValidationSeverity.Warning)
                    type = MessageType.Warning;
                else if (issue.Severity == BuildingValidationSeverity.Error)
                    type = MessageType.Error;

                EditorGUILayout.HelpBox($"[{issue.Code}] {issue.Message}", type);
                DrawIssueQuickFix(issue, buildingProperty, onChanged);
            }
        }

        public static IReadOnlyList<BuildingValidationIssue> GetFilteredIssues(BuildingDefinition definition, EconomyBuildingRules rules = null)
        {
            var source = BuildingModuleValidation.Validate(definition);
            if (rules == null || source == null || source.Count == 0)
                return source;

            var filtered = new List<BuildingValidationIssue>(source.Count);
            for (int i = 0; i < source.Count; i++)
            {
                var issue = source[i];
                if (issue != null && ShouldIncludeIssue(issue, rules))
                    filtered.Add(issue);
            }

            return filtered;
        }



        public static void CountValidationIssues(IEnumerable<BuildingDefinition> definitions, out int errors, out int warnings, EconomyBuildingRules rules = null)
        {
            errors = 0;
            warnings = 0;
            if (definitions == null)
                return;

            foreach (var definition in definitions)
            {
                if (definition == null)
                    continue;

                var issues = GetFilteredIssues(definition, rules);
                for (int i = 0; i < issues.Count; i++)
                {
                    var issue = issues[i];
                    if (issue == null)
                        continue;

                    if (issue.Severity == BuildingValidationSeverity.Error)
                        errors++;
                    else if (issue.Severity == BuildingValidationSeverity.Warning)
                        warnings++;
                }
            }
        }

        public static List<string> CollectInvalidBuildingIds(IEnumerable<BuildingDefinition> definitions, EconomyBuildingRules rules = null)
        {
            var invalid = new List<string>();
            if (definitions == null)
                return invalid;

            int index = 0;
            foreach (var definition in definitions)
            {
                if (definition != null && BuildingModuleValidation.HasErrors(GetFilteredIssues(definition, rules)))
                    invalid.Add(string.IsNullOrWhiteSpace(definition.Id) ? $"<building:{index}>" : definition.Id);
                index++;
            }

            return invalid;
        }

        private static bool ShouldIncludeIssue(BuildingValidationIssue issue, EconomyBuildingRules rules)
        {
            switch (issue.Code)
            {
                case "INV_TOWNHALL_HOUSING":
                    return rules.ValidateTownHallHousingConflict;
                case "INV_WORKERLESS_HOUSING":
                    return rules.ValidateWorkerlessHousingConflict;
                case "INV_WORKERLESS_PRODUCTION":
                    return rules.ValidateWorkerlessProductionConflict;
                case "INV_PRODUCTION_RESOURCE":
                    return rules.RequireProductionResourceId;
                case "INV_TILE_REQUIREMENTS_EMPTY":
                case "INV_TILE_REQUIREMENTS_INVALID":
                    return rules.RequireTileRequirementEntries;
                case "INV_WORKERS_AUTOFIX":
                    return rules.WarnWorkerlessProductionWorkers;
                case "INV_SINGLETON_DUPLICATE":
                    return rules.EnforceSingletonModules;
                default:
                    return true;
            }
        }

        public static string GetManagedReferenceTypeName(SerializedProperty moduleProperty)
        {
            string typeName = GetManagedReferenceTypeKey(moduleProperty);
            if (string.IsNullOrWhiteSpace(typeName))
                return "Модуль";

            for (int i = 0; i < ModuleOptions.Length; i++)
            {
                if (string.Equals(ModuleOptions[i].TypeName, typeName, StringComparison.Ordinal))
                    return ModuleOptions[i].DisplayName;
            }

            return typeName;
        }

        private static void DrawAddModuleMenu(SerializedProperty modulesProp, Action onChanged)
        {
            if (!GUILayout.Button("Додати модуль", EditorStyles.miniButton, GUILayout.Width(140f)))
                return;

            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new ModulePickerPopup(modulesProp, onChanged));
        }

        private static void AddModule(SerializedProperty modulesProp, BuildingModuleDefinition module, Action onChanged)
        {
            int idx = modulesProp.arraySize;
            modulesProp.InsertArrayElementAtIndex(idx);
            modulesProp.GetArrayElementAtIndex(idx).managedReferenceValue = module;
            onChanged?.Invoke();
        }

        private static void DrawIssueQuickFix(BuildingValidationIssue issue, SerializedProperty buildingProperty, Action onChanged)
        {
            if (issue == null || buildingProperty == null)
                return;

            var modulesProp = buildingProperty.FindPropertyRelative("Modules");
            if (modulesProp == null || !modulesProp.isArray)
                return;

            switch (issue.Code)
            {
                case "INV_TOWNHALL_HOUSING":
                case "INV_WORKERLESS_HOUSING":
                    if (GUILayout.Button("Швидке виправлення: видалити житловий модуль", EditorStyles.miniButton))
                    {
                        RemoveModuleByTypeName(modulesProp, "HousingBuildingModule");
                        onChanged?.Invoke();
                    }
                    break;

                case "INV_WORKERLESS_PRODUCTION":
                    if (GUILayout.Button("Швидке виправлення: видалити виробничий модуль", EditorStyles.miniButton))
                    {
                        RemoveModuleByTypeName(modulesProp, "ProductionBuildingModule");
                        onChanged?.Invoke();
                    }
                    break;

                case "INV_WORKERS_AUTOFIX":
                    if (GUILayout.Button("Швидке виправлення: WorkersRequired = 0", EditorStyles.miniButton))
                    {
                        NormalizeProductionWorkers(modulesProp);
                        onChanged?.Invoke();
                    }
                    break;

                case "INV_TILE_REQUIREMENTS_EMPTY":
                case "INV_TILE_REQUIREMENTS_INVALID":
                    if (GUILayout.Button("Швидке виправлення: додати базову вимогу тайла", EditorStyles.miniButton))
                    {
                        EnsureDefaultTileRequirement(modulesProp);
                        onChanged?.Invoke();
                    }
                    break;

                case "INV_PRODUCTION_RESOURCE":
                    EditorGUILayout.HelpBox("Встановіть ResourceId в ProductionModule або видаліть модуль.", MessageType.None);
                    break;
            }
        }

        private static void RemoveModuleByTypeName(SerializedProperty modulesProp, string typeName)
        {
            for (int i = modulesProp.arraySize - 1; i >= 0; i--)
            {
                var moduleProp = modulesProp.GetArrayElementAtIndex(i);
                if (string.Equals(GetManagedReferenceTypeKey(moduleProp), typeName, StringComparison.Ordinal))
                    modulesProp.DeleteArrayElementAtIndex(i);
            }
        }

        private static void NormalizeProductionWorkers(SerializedProperty modulesProp)
        {
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var moduleProp = modulesProp.GetArrayElementAtIndex(i);
                if (!string.Equals(GetManagedReferenceTypeKey(moduleProp), "ProductionBuildingModule", StringComparison.Ordinal))
                    continue;

                var workersProp = moduleProp.FindPropertyRelative("WorkersRequired");
                if (workersProp != null)
                    workersProp.intValue = 0;
            }
        }

        private static void EnsureDefaultTileRequirement(SerializedProperty modulesProp)
        {
            SerializedProperty tileModule = null;
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var moduleProp = modulesProp.GetArrayElementAtIndex(i);
                if (string.Equals(GetManagedReferenceTypeKey(moduleProp), "TileRequirementBuildingModule", StringComparison.Ordinal))
                {
                    tileModule = moduleProp;
                    break;
                }
            }

            if (tileModule == null)
            {
                int idx = modulesProp.arraySize;
                modulesProp.InsertArrayElementAtIndex(idx);
                tileModule = modulesProp.GetArrayElementAtIndex(idx);
                tileModule.managedReferenceValue = new TileRequirementBuildingModule();
            }

            var reqArray = tileModule.FindPropertyRelative("Requirements");
            if (reqArray == null || !reqArray.isArray)
                return;

            if (reqArray.arraySize == 0)
                reqArray.InsertArrayElementAtIndex(0);

            var req = reqArray.GetArrayElementAtIndex(0);
            var tileId = req.FindPropertyRelative("TileId");
            var radius = req.FindPropertyRelative("Radius");
            var minCount = req.FindPropertyRelative("MinimumTileCount");

            if (tileId != null && string.IsNullOrWhiteSpace(tileId.stringValue))
                tileId.stringValue = "forest";
            if (radius != null && radius.intValue <= 0)
                radius.intValue = 3;
            if (minCount != null && minCount.intValue <= 0)
                minCount.intValue = 1;
        }

        private static string GetManagedReferenceTypeKey(SerializedProperty moduleProperty)
        {
            if (moduleProperty == null)
                return null;

            string fullType = moduleProperty.managedReferenceFullTypename;
            if (string.IsNullOrWhiteSpace(fullType))
                return null;

            int lastSpace = fullType.LastIndexOf(' ');
            string typeName = lastSpace >= 0 && lastSpace + 1 < fullType.Length
                ? fullType.Substring(lastSpace + 1)
                : fullType;

            int lastDot = typeName.LastIndexOf('.');
            return lastDot >= 0 && lastDot + 1 < typeName.Length
                ? typeName.Substring(lastDot + 1)
                : typeName;
        }
    }
}