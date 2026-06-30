using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingDesignerWindow : OdinMenuEditorWindow
    {
        private const string RegistryPrefKey = "Moyva.BuildingDesigner.AssetRegistryGuid";
        private const string OutputFolderPrefKey = "Moyva.BuildingDesigner.OutputFolder";
        private const string DefaultOutputFolder = "Assets/Moyva/SO/Construction/Buildings";

        [SerializeField] private BuildingRegistrySO _registry;
        [SerializeField] private BuildingTemplateLibrarySO _templateLibrary;
        [SerializeField] private BuildingArchetypeSO _newBuildingTemplate;
        [SerializeField] private string _newBuildingId = "new-building";
        [SerializeField] private string _newBuildingName = "New Building";
        [SerializeField] private string _outputFolder = DefaultOutputFolder;
        [SerializeField] private bool _filterByCategory;
        [SerializeField] private BuildingCategory _categoryFilter = BuildingCategory.Civilian;
        [SerializeField] private bool _migrationAddsFogReveal = true;

        [MenuItem("Moyva/Tools/Building Designer", priority = 32)]
        public static void Open()
        {
            var window = GetWindow<BuildingDesignerWindow>("Building Designer");
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

            tree.Add("Library/Registry", _registry);
            tree.Add("Library/Templates", _templateLibrary);

            if (_registry == null)
            {
                tree.Add("Library/No Registry Selected", this);
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
                tree.Add($"Library/{asset.Category}/{label}", asset);
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
                tree.Add($"Library/Legacy Inline/{label}", definition);
            }

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            DrawToolbar();
            GUILayout.Space(6f);
            base.OnBeginDrawEditors();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField("Registry", _registry, typeof(BuildingRegistrySO), false);
                    _templateLibrary = (BuildingTemplateLibrarySO)EditorGUILayout.ObjectField("Templates", _templateLibrary, typeof(BuildingTemplateLibrarySO), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        SavePreferences();
                        ForceMenuTreeRebuild();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _outputFolder = EditorGUILayout.TextField("Output Folder", string.IsNullOrWhiteSpace(_outputFolder) ? DefaultOutputFolder : _outputFolder);
                    if (GUILayout.Button("Pick", GUILayout.Width(64f)))
                        PickOutputFolder();
                    _filterByCategory = EditorGUILayout.ToggleLeft("Filter", _filterByCategory, GUILayout.Width(64f));
                    using (new EditorGUI.DisabledScope(!_filterByCategory))
                        _categoryFilter = (BuildingCategory)EditorGUILayout.EnumPopup(_categoryFilter, GUILayout.Width(110f));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _newBuildingId = EditorGUILayout.TextField("New ID", _newBuildingId);
                    _newBuildingName = EditorGUILayout.TextField("Name", _newBuildingName);
                    _newBuildingTemplate = (BuildingArchetypeSO)EditorGUILayout.ObjectField(_newBuildingTemplate, typeof(BuildingArchetypeSO), false, GUILayout.Width(210f));
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("New Building"))
                        CreateBuilding();
                    if (GUILayout.Button("Duplicate Selected"))
                        DuplicateSelected();
                    if (GUILayout.Button("Delete Selected"))
                        DeleteSelected();
                    if (GUILayout.Button("Validate Registry"))
                        ValidateRegistry();
                    if (GUILayout.Button("Rebuild Registry"))
                        RebuildRegistryFromAssets();
                    if (GUILayout.Button("Migrate Legacy"))
                        MigrateLegacy();
                }
            }
        }

        private void CreateBuilding()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Select a BuildingRegistry first.");
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
                Debug.LogWarning("[BuildingDesigner] Select a BuildingDefinition asset to duplicate.");
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
                Debug.LogWarning("[BuildingDesigner] Select a BuildingDefinition asset to delete.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (!EditorUtility.DisplayDialog("Delete Building", $"Delete '{selected.DisplayName}'?\n{path}", "Delete", "Cancel"))
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

            Debug.Log($"[BuildingDesigner] Registry validation: errors={errors}, warnings={warnings}, issues={issues.Count}");
            for (int i = 0; i < issues.Count; i++)
                Debug.Log($"[BuildingDesigner] {issues[i].Severity} {issues[i].Code}: {issues[i].Message}");
        }

        private void RebuildRegistryFromAssets()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Select a BuildingRegistry first.");
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
            Debug.Log($"[BuildingDesigner] Rebuilt registry with {assets.Count} BuildingDefinition assets.");
        }

        private void MigrateLegacy()
        {
            if (_registry == null)
            {
                Debug.LogWarning("[BuildingDesigner] Select a BuildingRegistry first.");
                return;
            }

            var report = BuildingMigrationUtility.MigrateLegacyRegistry(_registry, _outputFolder, _migrationAddsFogReveal);
            Debug.Log($"[BuildingDesigner] Migration complete: {report}\n{string.Join("\n", report.Messages)}");
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
            string selected = EditorUtility.OpenFolderPanel("Building Definition Output Folder", Application.dataPath, string.Empty);
            if (string.IsNullOrWhiteSpace(selected))
                return;

            selected = selected.Replace('\\', '/');
            string assetsPath = Application.dataPath.Replace('\\', '/');
            if (!selected.StartsWith(assetsPath, StringComparison.Ordinal))
            {
                Debug.LogWarning("[BuildingDesigner] Folder must be inside Assets.");
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
    }
}
