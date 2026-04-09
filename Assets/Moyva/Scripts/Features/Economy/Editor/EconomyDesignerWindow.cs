using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyDesignerWindow : EditorWindow
    {
        private enum Tab
        {
            Settlements = 0,
            Resources = 1,
            Warehouses = 2,
            Production = 3,
            Caravans = 4,
            AiRules = 5,
            Validation = 6,
            Simulation = 7,
        }

        private static readonly string[] TabLabels =
        {
            "Settlements",
            "Resources",
            "Warehouses",
            "Production",
            "Caravans",
            "AI Rules",
            "Validation",
            "Simulation",
        };

        private readonly Dictionary<Tab, string> _searchByTab = new Dictionary<Tab, string>();
        private readonly Dictionary<Tab, UnityEngine.Object> _selectedByTab = new Dictionary<Tab, UnityEngine.Object>();
        private readonly Dictionary<UnityEngine.Object, UnityEditor.Editor> _cachedEditors = new Dictionary<UnityEngine.Object, UnityEditor.Editor>();

        private readonly EconomyValidationService _validationService = new EconomyValidationService();
        private readonly EconomyAutoFixService _autoFixService = new EconomyAutoFixService();
        private readonly EconomySimulationService _simulationService = new EconomySimulationService();
        private readonly EconomyDataMigrationService _migrationService = new EconomyDataMigrationService();

        private EconomyDatabaseSO _database;
        private SerializedObject _databaseSo;

        private Tab _tab;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _validationScroll;
        private Vector2 _simulationScroll;

        private List<EconomyValidationIssue> _validationIssues = new List<EconomyValidationIssue>();
        private EconomyMigrationReport _migrationReport;

        private EconomySettlementDefinition _simulationSettlement;
        private float _simulationDurationMinutes = 10f;
        private readonly HashSet<EconomyProductionProfile> _simulationProfiles = new HashSet<EconomyProductionProfile>();
        private EconomySimulationResult _simulationResult;

        [MenuItem("Moyva/Tools/Economy Designer")]
        public static void OpenWindow()
        {
            var window = GetWindow<EconomyDesignerWindow>("Economy Designer");
            window.minSize = new Vector2(940f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            if (_database == null)
                _database = FindFirstAsset<EconomyDatabaseSO>();

            RebuildSerializedObjects();
        }

        private void OnDisable()
        {
            foreach (var editor in _cachedEditors.Values)
            {
                if (editor != null)
                    DestroyImmediate(editor);
            }

            _cachedEditors.Clear();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawDatabaseSelector();
            DrawTabToolbar();

            EditorGUILayout.Space(4f);
            switch (_tab)
            {
                case Tab.Settlements:
                    DrawEntityTab<EconomySettlementDefinition>(
                        "Settlements",
                        "SettlementId, type, center building and build radius.",
                        "_settlements",
                        settlement => settlement == null ? string.Empty : settlement.SettlementId,
                        "Settlement");
                    break;

                case Tab.Resources:
                    DrawEntityTab<EconomyResourceDefinition>(
                        "Resources",
                        "Resource Id, display name, category, icon and stack limit.",
                        "_resources",
                        resource => resource == null ? string.Empty : resource.Id,
                        "Resource");
                    break;

                case Tab.Warehouses:
                    DrawEntityTab<EconomyWarehousePolicy>(
                        "Warehouses",
                        "Per-warehouse policies and resource-level priorities/reserves.",
                        "_warehousePolicies",
                        warehouse => warehouse == null ? string.Empty : warehouse.WarehouseType.ToString(),
                        "WarehousePolicy");
                    break;

                case Tab.Production:
                    DrawEntityTab<EconomyProductionProfile>(
                        "Production",
                        "Production cycles with recipe/output configuration.",
                        "_productionProfiles",
                        profile => profile == null ? string.Empty : $"{profile.BuildingId} / {profile.RecipeId}",
                        "ProductionProfile");
                    break;

                case Tab.Caravans:
                    DrawEntityTab<EconomyCaravanTemplate>(
                        "Caravans",
                        "Templates for carrying allowed resources and priorities.",
                        "_caravanTemplates",
                        caravan => caravan == null ? string.Empty : caravan.TemplateId,
                        "CaravanTemplate");
                    break;

                case Tab.AiRules:
                    DrawEntityTab<EconomyAiRuleProfile>(
                        "AI Rules",
                        "Thresholds and conservative spending toggles for AI economy behavior.",
                        "_aiRuleProfiles",
                        profile => profile == null ? string.Empty : profile.ProfileId,
                        "AiRuleProfile");
                    break;

                case Tab.Validation:
                    DrawValidationTab();
                    break;

                case Tab.Simulation:
                    DrawSimulationTab();
                    break;
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Economy Designer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Central editor for economy assets. Use tabs to configure entities, validate data and run deterministic simulation preview.",
                MessageType.Info);
        }

        private void DrawDatabaseSelector()
        {
            EditorGUI.BeginChangeCheck();
            _database = (EconomyDatabaseSO)EditorGUILayout.ObjectField("Economy Database", _database, typeof(EconomyDatabaseSO), false);
            if (EditorGUI.EndChangeCheck())
                RebuildSerializedObjects();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Auto Find", GUILayout.Width(120f)))
            {
                _database = FindFirstAsset<EconomyDatabaseSO>();
                RebuildSerializedObjects();
            }

            if (GUILayout.Button("Create Database Asset", GUILayout.Width(160f)))
                CreateDatabaseAsset();

            using (new EditorGUI.DisabledScope(_database == null))
            {
                if (GUILayout.Button("Ping", GUILayout.Width(80f)))
                    EditorGUIUtility.PingObject(_database);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_database == null)
                EditorGUILayout.HelpBox("Assign EconomyDatabaseSO to start editing all tabs.", MessageType.Warning);
        }

        private void DrawTabToolbar()
        {
            _tab = (Tab)GUILayout.Toolbar((int)_tab, TabLabels);
        }

        private void DrawEntityTab<T>(string title, string hint, string listPropertyName, Func<T, string> displayName, string defaultAssetName)
            where T : ScriptableObject
        {
            if (!EnsureDatabaseSerialized())
                return;

            _databaseSo.Update();
            var listProperty = _databaseSo.FindProperty(listPropertyName);
            if (listProperty == null)
            {
                EditorGUILayout.HelpBox($"Serialized list '{listPropertyName}' was not found.", MessageType.Error);
                return;
            }

            var items = ReadObjectList<T>(listProperty);
            var selected = GetSelected<T>();
            var search = GetSearch();
            var filtered = items
                .Where(item => item != null)
                .Where(item => string.IsNullOrWhiteSpace(search) || (displayName(item) ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(hint, MessageType.None);

            EditorGUILayout.BeginHorizontal();
            DrawEntityListPanel(items, filtered, selected, listPropertyName, displayName, defaultAssetName);
            DrawEntityInspectorPanel(selected);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntityListPanel<T>(
            List<T> allItems,
            List<T> filteredItems,
            T selected,
            string listPropertyName,
            Func<T, string> displayName,
            string defaultAssetName)
            where T : ScriptableObject
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(320f));
            SetSearch(EditorGUILayout.TextField("Search", GetSearch()));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create", GUILayout.Width(90f)))
                CreateAndAddAsset<T>(listPropertyName, defaultAssetName);

            if (GUILayout.Button("Add Selected", GUILayout.Width(110f)))
            {
                var active = Selection.activeObject as T;
                if (active != null)
                    AddAssetReference(listPropertyName, active);
            }

            using (new EditorGUI.DisabledScope(selected == null))
            {
                if (GUILayout.Button("Remove", GUILayout.Width(90f)))
                    RemoveAssetReference(listPropertyName, selected);
            }

            EditorGUILayout.EndHorizontal();

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, "box");
            if (filteredItems.Count == 0)
            {
                EditorGUILayout.HelpBox("No entries match current filter.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < filteredItems.Count; i++)
                {
                    var item = filteredItems[i];
                    var label = string.IsNullOrWhiteSpace(displayName(item)) ? item.name : displayName(item);
                    var isSelected = selected == item;
                    if (GUILayout.Toggle(isSelected, label, "Button"))
                        SetSelected(item);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"Total: {allItems.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawEntityInspectorPanel<T>(T selected) where T : ScriptableObject
        {
            EditorGUILayout.BeginVertical("box");
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

            if (selected == null)
            {
                EditorGUILayout.HelpBox("Select an item on the left to edit its fields.", MessageType.None);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ping", GUILayout.Width(70f)))
                EditorGUIUtility.PingObject(selected);
            EditorGUILayout.EndHorizontal();

            DrawObjectInspector(selected);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawObjectInspector(UnityEngine.Object target)
        {
            if (!_cachedEditors.TryGetValue(target, out var editor) || editor == null)
            {
                UnityEditor.Editor.CreateCachedEditor(target, null, ref editor);
                _cachedEditors[target] = editor;
            }

            if (editor != null)
                editor.OnInspectorGUI();
        }

        private void DrawValidationTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Checks for IDs, references and numeric ranges. Fix Common Issues applies safe auto-fixes only.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Validation", GUILayout.Height(28f)))
                _validationIssues = _validationService.Validate(_database).ToList();

            if (GUILayout.Button("Fix Common Issues", GUILayout.Height(28f)))
            {
                Undo.RecordObject(_database, "Economy Fix Common Issues");
                var fixedCount = _autoFixService.FixCommonIssues(_database);
                _validationIssues = _validationService.Validate(_database).ToList();
                ShowNotification(new GUIContent($"Applied {fixedCount} fixes."));
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button("Run Migration", GUILayout.Height(28f)))
            {
                Undo.RecordObject(_database, "Economy Data Migration");
                _migrationReport = _migrationService.Migrate(_database);
                _databaseSo?.Update();
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndHorizontal();

            _validationScroll = EditorGUILayout.BeginScrollView(_validationScroll, "box");
            if (_validationIssues.Count == 0)
            {
                EditorGUILayout.HelpBox("No validation issues. Run validation to refresh.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < _validationIssues.Count; i++)
                {
                    var issue = _validationIssues[i];
                    var messageType = issue.Severity == EconomyValidationSeverity.Error ? MessageType.Error : MessageType.Warning;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox(issue.Message, messageType);
                    if (issue.Context != null && GUILayout.Button("Ping", GUILayout.Width(64f), GUILayout.Height(38f)))
                        EditorGUIUtility.PingObject(issue.Context);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6f);
            DrawMigrationStatus();
        }

        private void DrawMigrationStatus()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Schema", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Current Supported Version", EconomySchema.CurrentVersion.ToString());
            EditorGUILayout.LabelField("Database Version", _database != null ? _database.SchemaVersion.ToString() : "N/A");

            if (_migrationReport == null)
            {
                EditorGUILayout.HelpBox("Run migration to see steps.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            var header = _migrationReport.Changed
                ? $"Migration applied: {_migrationReport.FromVersion} -> {_migrationReport.ToVersion}"
                : $"No migration changes: {_migrationReport.FromVersion} -> {_migrationReport.ToVersion}";
            EditorGUILayout.HelpBox(header, _migrationReport.Changed ? MessageType.Info : MessageType.None);

            for (var i = 0; i < _migrationReport.Steps.Count; i++)
                EditorGUILayout.LabelField($"{i + 1}. {_migrationReport.Steps[i]}", EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawSimulationTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            EditorGUILayout.LabelField("Simulation Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Deterministic approximation. Uses production profiles and duration to estimate final resource deltas.",
                MessageType.Info);

            _simulationSettlement = (EconomySettlementDefinition)EditorGUILayout.ObjectField(
                "Settlement",
                _simulationSettlement,
                typeof(EconomySettlementDefinition),
                false);

            _simulationDurationMinutes = EditorGUILayout.FloatField("Duration (minutes)", Mathf.Max(0f, _simulationDurationMinutes));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Defaults", GUILayout.Width(140f)))
                SelectDefaultSimulationProfiles();

            if (GUILayout.Button("Select All", GUILayout.Width(120f)))
            {
                _simulationProfiles.Clear();
                foreach (var profile in _database.ProductionProfiles.Where(profile => profile != null))
                    _simulationProfiles.Add(profile);
            }

            if (GUILayout.Button("Clear", GUILayout.Width(90f)))
                _simulationProfiles.Clear();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            _simulationScroll = EditorGUILayout.BeginScrollView(_simulationScroll, "box", GUILayout.Height(220f));
            var profiles = _database.ProductionProfiles.Where(profile => profile != null).OrderBy(profile => profile.BuildingId ?? string.Empty, StringComparer.Ordinal).ToList();
            if (profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("No production profiles in database.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    var selected = _simulationProfiles.Contains(profile);
                    var next = EditorGUILayout.ToggleLeft(
                        $"{profile.BuildingId} -> {profile.RecipeId} (cycle {profile.CycleDurationSeconds:0.##}s, output {profile.OutputAmountPerCycle})",
                        selected);
                    if (next && !selected)
                        _simulationProfiles.Add(profile);
                    else if (!next && selected)
                        _simulationProfiles.Remove(profile);
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Run Deterministic Preview", GUILayout.Height(30f)))
            {
                var input = new EconomySimulationInput
                {
                    Settlement = _simulationSettlement,
                    DurationMinutes = _simulationDurationMinutes,
                    ProductionProfiles = _simulationProfiles.OrderBy(profile => profile.BuildingId ?? string.Empty, StringComparer.Ordinal).ToList(),
                };
                _simulationResult = _simulationService.Simulate(input);
            }

            DrawSimulationResults();
        }

        private void DrawSimulationResults()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);

            if (_simulationResult == null)
            {
                EditorGUILayout.HelpBox("Run preview to see estimated totals.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Settlement", _simulationResult.Settlement == null ? "(not specified)" : _simulationResult.Settlement.SettlementId);
            EditorGUILayout.LabelField("Duration", $"{_simulationResult.DurationMinutes:0.##} minutes");

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Estimated Resource Totals", EditorStyles.miniBoldLabel);
            if (_simulationResult.ResourceTotals.Count == 0)
            {
                EditorGUILayout.LabelField("No output for selected profiles and duration.");
            }
            else
            {
                foreach (var pair in _simulationResult.ResourceTotals)
                    EditorGUILayout.LabelField(pair.Key, pair.Value.ToString());
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Log", EditorStyles.miniBoldLabel);
            if (_simulationResult.Log.Count == 0)
            {
                EditorGUILayout.LabelField("No entries.");
            }
            else
            {
                for (var i = 0; i < _simulationResult.Log.Count; i++)
                    EditorGUILayout.LabelField($"{i + 1}. {_simulationResult.Log[i]}", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private bool EnsureDatabaseSerialized()
        {
            if (_database == null)
            {
                EditorGUILayout.HelpBox("Assign EconomyDatabaseSO first.", MessageType.Warning);
                return false;
            }

            if (_databaseSo == null || _databaseSo.targetObject != _database)
                _databaseSo = new SerializedObject(_database);

            return true;
        }

        private void RebuildSerializedObjects()
        {
            _databaseSo = _database == null ? null : new SerializedObject(_database);
            _simulationResult = null;
            _migrationReport = null;
        }

        private void CreateDatabaseAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Economy Database", "EconomyDatabase", "asset", "Select location for EconomyDatabaseSO.");
            if (string.IsNullOrEmpty(path))
                return;

            var asset = CreateInstance<EconomyDatabaseSO>();
            asset.SchemaVersion = EconomySchema.CurrentVersion;
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _database = asset;
            RebuildSerializedObjects();
            EditorGUIUtility.PingObject(asset);
        }

        private void CreateAndAddAsset<T>(string listPropertyName, string defaultAssetName)
            where T : ScriptableObject
        {
            var path = EditorUtility.SaveFilePanelInProject(
                $"Create {typeof(T).Name}",
                $"Economy{defaultAssetName}",
                "asset",
                $"Select location for {typeof(T).Name} asset.");
            if (string.IsNullOrEmpty(path))
                return;

            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            AddAssetReference(listPropertyName, asset);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        private void AddAssetReference(string listPropertyName, UnityEngine.Object target)
        {
            if (target == null || !EnsureDatabaseSerialized())
                return;

            Undo.RecordObject(_database, "Economy Add Reference");

            _databaseSo.Update();
            var property = _databaseSo.FindProperty(listPropertyName);
            if (property == null)
                return;

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue == target)
                    return;
            }

            var insertIndex = property.arraySize;
            property.InsertArrayElementAtIndex(insertIndex);
            property.GetArrayElementAtIndex(insertIndex).objectReferenceValue = target;
            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);
            SetSelected(target);
        }

        private void RemoveAssetReference(string listPropertyName, UnityEngine.Object target)
        {
            if (target == null || !EnsureDatabaseSerialized())
                return;

            Undo.RecordObject(_database, "Economy Remove Reference");

            _databaseSo.Update();
            var property = _databaseSo.FindProperty(listPropertyName);
            if (property == null)
                return;

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue != target)
                    continue;

                property.DeleteArrayElementAtIndex(i);
                break;
            }

            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);
            SetSelected<UnityEngine.Object>(null);
        }

        private static List<T> ReadObjectList<T>(SerializedProperty listProperty)
            where T : UnityEngine.Object
        {
            var result = new List<T>();
            if (listProperty == null || !listProperty.isArray)
                return result;

            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                if (element != null && element.objectReferenceValue is T typed)
                    result.Add(typed);
            }

            return result;
        }

        private void SelectDefaultSimulationProfiles()
        {
            _simulationProfiles.Clear();
            foreach (var profile in _database.ProductionProfiles.Where(profile => profile != null && profile.IsActiveByDefault))
                _simulationProfiles.Add(profile);
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private string GetSearch()
        {
            return _searchByTab.TryGetValue(_tab, out var value) ? value : string.Empty;
        }

        private void SetSearch(string value)
        {
            _searchByTab[_tab] = value ?? string.Empty;
        }

        private T GetSelected<T>() where T : UnityEngine.Object
        {
            return _selectedByTab.TryGetValue(_tab, out var value) ? value as T : null;
        }

        private void SetSelected<T>(T value) where T : UnityEngine.Object
        {
            _selectedByTab[_tab] = value;
        }
    }
}
