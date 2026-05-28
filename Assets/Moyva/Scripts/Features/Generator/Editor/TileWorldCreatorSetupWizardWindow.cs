using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    public sealed class TileWorldCreatorSetupWizardWindow : EditorWindow
    {
        private const string DefaultAssetFolder = "Assets/Moyva/SO/Generation/TileWorldCreator";
        private const string DefaultConfigurationName = "MoyvaTileWorldCreatorConfiguration.asset";
        private const string DefaultMappingName = "MoyvaTileWorldCreatorIdMapping.asset";

        private TileWorldCreatorManager _manager;
        private GeneratorInstaller _generatorInstaller;
        private TileRegistrySO _tileRegistry;
        private TileWorldCreatorIdMappingSO _mapping;
        private string _assetFolder = DefaultAssetFolder;
        private bool _seedMappingFromTileRegistry = true;
        private bool _createTerrainBuildLayers = true;
        private bool _createObjectBuildLayers;
        private bool _syncTileRegistry = true;
        private bool _rebuildTileRegistryFromMapping = true;
        private bool _assignGeneratorInstaller = true;
        private bool _drawMappingInspector = true;
        private Vector2 _scroll;
        private UnityEditor.Editor _mappingEditor;

        [MenuItem("Moyva/Tools/TileWorldCreator/Setup Wizard")]
        public static void Open()
        {
            GetWindow<TileWorldCreatorSetupWizardWindow>("TWC Setup Wizard");
        }

        private void OnEnable()
        {
            AutoFindReferences();
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("TileWorldCreator Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Цей wizard створює TWC Manager/Configuration, заповнює TWC layers із ID Mapping, перебудовує TileRegistry як gameplay/TWC-реєстр і підключає все до GeneratorInstaller.",
                MessageType.Info);

            DrawReferencesSection();
            DrawAutomationSection();
            DrawMappingSection();
            DrawActionsSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Scene & Assets", EditorStyles.boldLabel);

            _manager = (TileWorldCreatorManager)EditorGUILayout.ObjectField("TWC Manager", _manager, typeof(TileWorldCreatorManager), true);
            _generatorInstaller = (GeneratorInstaller)EditorGUILayout.ObjectField("Generator Installer", _generatorInstaller, typeof(GeneratorInstaller), true);
            _tileRegistry = (TileRegistrySO)EditorGUILayout.ObjectField("Tile Registry", _tileRegistry, typeof(TileRegistrySO), false);
            _mapping = (TileWorldCreatorIdMappingSO)EditorGUILayout.ObjectField("ID Mapping", _mapping, typeof(TileWorldCreatorIdMappingSO), false);
            _assetFolder = EditorGUILayout.TextField("Generated Assets Folder", _assetFolder);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Auto Find"))
                    AutoFindReferences();

                if (GUILayout.Button("Create Missing Assets"))
                    CreateMissingAssetsOnly();
            }
        }

        private void DrawAutomationSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Automation", EditorStyles.boldLabel);

            _seedMappingFromTileRegistry = EditorGUILayout.Toggle("Seed Mapping From TileRegistry", _seedMappingFromTileRegistry);
            _createTerrainBuildLayers = EditorGUILayout.Toggle("Create Terrain TWC Layers", _createTerrainBuildLayers);
            _createObjectBuildLayers = EditorGUILayout.Toggle("Create Object TWC Layers", _createObjectBuildLayers);
            _syncTileRegistry = EditorGUILayout.Toggle("Sync TileRegistry From TWC", _syncTileRegistry);
            using (new EditorGUI.DisabledScope(!_syncTileRegistry))
                _rebuildTileRegistryFromMapping = EditorGUILayout.Toggle("Rebuild TileRegistry Entries", _rebuildTileRegistryFromMapping);
            _assignGeneratorInstaller = EditorGUILayout.Toggle("Assign GeneratorInstaller", _assignGeneratorInstaller);
        }

        private void DrawMappingSection()
        {
            EditorGUILayout.Space(8f);
            _drawMappingInspector = EditorGUILayout.Foldout(_drawMappingInspector, "ID Mapping Editor", true);
            if (!_drawMappingInspector || _mapping == null)
                return;

            if (_mappingEditor == null || _mappingEditor.target != _mapping)
                UnityEditor.Editor.CreateCachedEditor(_mapping, null, ref _mappingEditor);

            _mappingEditor.OnInspectorGUI();
        }

        private void DrawActionsSection()
        {
            EditorGUILayout.Space(12f);

            using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_assetFolder)))
            {
                if (GUILayout.Button("Build Full TileWorldCreator Setup", GUILayout.Height(34f)))
                    BuildFullSetup();
            }

            EditorGUILayout.HelpBox(
                "Після натискання кнопки тобі лишається у Mapping виставити TilePreset для кожної групи ID, якщо wizard не зміг підібрати його автоматично.",
                MessageType.None);
        }

        private void AutoFindReferences()
        {
            _manager ??= FindFirstSceneObject<TileWorldCreatorManager>();
            _generatorInstaller ??= FindFirstSceneObject<GeneratorInstaller>();
            _tileRegistry ??= FindFirstAsset<TileRegistrySO>();
            _mapping ??= FindFirstAsset<TileWorldCreatorIdMappingSO>();
            Repaint();
        }

        private void CreateMissingAssetsOnly()
        {
            EnsureAssetFolder(_assetFolder);
            EnsureManagerAndConfiguration();
            EnsureMappingAsset();
            SaveAll();
        }

        private void BuildFullSetup()
        {
            EnsureAssetFolder(_assetFolder);
            EnsureManagerAndConfiguration();
            EnsureMappingAsset();

            if (_seedMappingFromTileRegistry)
                SeedMappingFromTileRegistry();

            if (_createTerrainBuildLayers)
                EnsureTerrainLayersAndBuildLayers();

            if (_createObjectBuildLayers)
                EnsureObjectLayersAndBuildLayers();

            if (_syncTileRegistry)
            {
                if (_rebuildTileRegistryFromMapping)
                    TileWorldCreatorRegistrySyncUtility.RebuildTerrainIds(_tileRegistry, _mapping, out _, out _, out _);
                else
                    TileWorldCreatorRegistrySyncUtility.SyncTerrainIds(_tileRegistry, _mapping, true, out _, out _, out _);
            }

            if (_assignGeneratorInstaller)
                AssignGeneratorInstaller();

            SaveAll();
            EditorUtility.DisplayDialog("TWC Setup Wizard", "TileWorldCreator setup оновлено.", "OK");
        }

        private void EnsureManagerAndConfiguration()
        {
            if (_manager == null)
            {
                var go = new GameObject("TileWorldCreator");
                _manager = go.AddComponent<TileWorldCreatorManager>();
                Undo.RegisterCreatedObjectUndo(go, "Create TileWorldCreator Manager");
            }

            if (_manager.configuration == null)
            {
                var configuration = CreateInstance<Configuration>();
                configuration.name = "Moyva TileWorldCreator Configuration";
                AssetDatabase.CreateAsset(configuration, AssetDatabase.GenerateUniqueAssetPath($"{_assetFolder}/{DefaultConfigurationName}"));
                _manager.configuration = configuration;
                EditorUtility.SetDirty(_manager);
            }

            EnsureConfigurationFolders(_manager.configuration);
        }

        private void EnsureMappingAsset()
        {
            if (_mapping != null)
                return;

            _mapping = CreateInstance<TileWorldCreatorIdMappingSO>();
            AssetDatabase.CreateAsset(_mapping, AssetDatabase.GenerateUniqueAssetPath($"{_assetFolder}/{DefaultMappingName}"));
            EditorUtility.SetDirty(_mapping);
        }

        private void SeedMappingFromTileRegistry()
        {
            if (_tileRegistry == null || _mapping == null || _tileRegistry.Definitions == null)
                return;

            var mappingObject = new SerializedObject(_mapping);
            var terrainLayers = mappingObject.FindProperty("_terrainLayers");
            int added = 0;

            foreach (var definition in _tileRegistry.Definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id) || ContainsIdPattern(terrainLayers, definition.Id))
                    continue;

                int index = terrainLayers.arraySize;
                terrainLayers.InsertArrayElementAtIndex(index);
                var element = terrainLayers.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("_idPattern").stringValue = definition.Id;
                element.FindPropertyRelative("_blueprintLayerName").stringValue = GuessLayerName(definition.Id);
                element.FindPropertyRelative("_tilePreset").objectReferenceValue = definition.TileWorldCreatorPreset != null
                    ? definition.TileWorldCreatorPreset
                    : GuessTilePreset(definition.Id);
                element.FindPropertyRelative("_registryVisualPrefab").objectReferenceValue = definition.VisualPrefab != null
                    ? definition.VisualPrefab
                    : definition.SurfaceReferencePrefab;
                element.FindPropertyRelative("_movementCost").floatValue = definition.MovementCost;
                added++;
            }

            mappingObject.ApplyModifiedProperties();
            if (added > 0)
                EditorUtility.SetDirty(_mapping);
        }

        private void EnsureTerrainLayersAndBuildLayers()
        {
            var configuration = _manager?.configuration;
            if (configuration == null || _mapping == null)
                return;

            var mappingObject = new SerializedObject(_mapping);
            var terrainLayers = mappingObject.FindProperty("_terrainLayers");
            var configuredBuildLayerGuids = new HashSet<string>();

            for (int i = 0; i < terrainLayers.arraySize; i++)
            {
                var element = terrainLayers.GetArrayElementAtIndex(i);
                string idPattern = element.FindPropertyRelative("_idPattern").stringValue;
                if (string.IsNullOrWhiteSpace(idPattern))
                    continue;

                string layerName = ResolveLayerName(element, idPattern);
                var blueprintLayer = EnsureBlueprintLayer(configuration, layerName);
                WriteLayerReference(element, blueprintLayer);

                var tilePreset = element.FindPropertyRelative("_tilePreset").objectReferenceValue as TilePreset;
                if (tilePreset == null)
                    continue;

                NormalizeMappingGridSettings(tilePreset, element);
                var buildLayer = EnsureBuildLayer<TilesBuildLayer>(configuration, $"Build {layerName}");
                buildLayer.SetBlueprintLayer(blueprintLayer);
                if (configuredBuildLayerGuids.Add(blueprintLayer.guid) || !HasUsableTopPreset(buildLayer))
                {
                    buildLayer.SetNewTilePreset(tilePreset);
                    ApplyTilesBuildLayerSettings(buildLayer, tilePreset, element);
                }
                EditorUtility.SetDirty(buildLayer);
            }

            mappingObject.ApplyModifiedProperties();
        }

        private void EnsureObjectLayersAndBuildLayers()
        {
            var configuration = _manager?.configuration;
            if (configuration == null || _mapping == null)
                return;

            var mappingObject = new SerializedObject(_mapping);
            var objectLayers = mappingObject.FindProperty("_objectLayers");

            for (int i = 0; i < objectLayers.arraySize; i++)
            {
                var element = objectLayers.GetArrayElementAtIndex(i);
                string idPattern = element.FindPropertyRelative("_idPattern").stringValue;
                if (string.IsNullOrWhiteSpace(idPattern))
                    continue;

                string layerName = ResolveLayerName(element, idPattern);
                var blueprintLayer = EnsureBlueprintLayer(configuration, layerName);
                WriteLayerReference(element, blueprintLayer);

                var prefab = element.FindPropertyRelative("_registryVisualPrefab").objectReferenceValue as GameObject;
                if (prefab == null)
                    continue;

                var buildLayer = EnsureBuildLayer<ObjectBuildLayer>(configuration, $"Build {layerName}");
                buildLayer.SetBlueprintLayer(blueprintLayer);
                if (!ContainsPrefab(buildLayer.prefabObjects, prefab))
                    buildLayer.AddPrefabObject(prefab);
                EditorUtility.SetDirty(buildLayer);
            }

            mappingObject.ApplyModifiedProperties();
        }

        private void AssignGeneratorInstaller()
        {
            if (_generatorInstaller == null || _manager == null || _mapping == null)
                return;

            var installerObject = new SerializedObject(_generatorInstaller);
            installerObject.FindProperty("_useTileWorldCreatorVisuals").boolValue = true;
            installerObject.FindProperty("_tileWorldCreatorManager").objectReferenceValue = _manager;
            installerObject.FindProperty("_tileWorldCreatorIdMapping").objectReferenceValue = _mapping;

            var options = installerObject.FindProperty("_tileWorldCreatorBuildOptions");
            if (options != null)
            {
                options.FindPropertyRelative("_replaceMappedTerrainVisuals").boolValue = true;
                options.FindPropertyRelative("_suppressMoyvaLayerDataWhenTerrainMapped").boolValue = true;
                options.FindPropertyRelative("_resetConfigurationBeforeBuild").boolValue = true;
                options.FindPropertyRelative("_syncConfigurationSize").boolValue = true;
                options.FindPropertyRelative("_useWorldSeed").boolValue = true;
            }

            installerObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_generatorInstaller);
        }

        private BlueprintLayer EnsureBlueprintLayer(Configuration configuration, string layerName)
        {
            string existingGuid = configuration.GetBlueprintLayerGuid(layerName);
            if (!string.IsNullOrWhiteSpace(existingGuid))
                return configuration.GetBlueprintLayerByGuid(existingGuid);

            var layer = _manager.AddNewBlueprintLayer(layerName);
            EditorUtility.SetDirty(layer);
            EditorUtility.SetDirty(configuration);
            return layer;
        }

        private T EnsureBuildLayer<T>(Configuration configuration, string layerName) where T : BuildLayer
        {
            var existing = _manager.GetBuildLayer(layerName) as T;
            if (existing != null)
                return existing;

            var layer = _manager.AddNewBuildLayer<T>(layerName);
            EditorUtility.SetDirty(layer);
            EditorUtility.SetDirty(configuration);
            return layer;
        }

        private static void EnsureConfigurationFolders(Configuration configuration)
        {
            configuration.blueprintLayerFolders ??= new List<BlueprintLayerFolder>();
            configuration.buildLayerFolders ??= new List<BuildLayerFolder>();

            if (configuration.blueprintLayerFolders.Count == 0)
                configuration.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));

            if (configuration.buildLayerFolders.Count == 0)
                configuration.buildLayerFolders.Add(new BuildLayerFolder("Root"));

            EditorUtility.SetDirty(configuration);
        }

        private static string ResolveLayerName(SerializedProperty element, string idPattern)
        {
            var nameProperty = element.FindPropertyRelative("_blueprintLayerName");
            if (!string.IsNullOrWhiteSpace(nameProperty.stringValue))
                return nameProperty.stringValue.Trim();

            string guessed = GuessLayerName(idPattern);
            nameProperty.stringValue = guessed;
            return guessed;
        }

        private static void WriteLayerReference(SerializedProperty element, BlueprintLayer layer)
        {
            if (layer == null)
                return;

            element.FindPropertyRelative("_blueprintLayerGuid").stringValue = layer.guid;
            element.FindPropertyRelative("_blueprintLayerName").stringValue = layer.layerName;
        }

        private static bool ContainsIdPattern(SerializedProperty arrayProperty, string idPattern)
        {
            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                var idProperty = arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative("_idPattern");
                if (idProperty != null && idProperty.stringValue == idPattern)
                    return true;
            }

            return false;
        }

        private static bool ContainsPrefab(IReadOnlyList<ObjectBuildLayer.PrefabObject> prefabs, GameObject prefab)
        {
            if (prefabs == null || prefab == null)
                return false;

            for (int i = 0; i < prefabs.Count; i++)
            {
                if (prefabs[i] != null && prefabs[i].prefabObject == prefab)
                    return true;
            }

            return false;
        }

        private static void ApplyTilesBuildLayerSettings(TilesBuildLayer buildLayer, TilePreset tilePreset, SerializedProperty mappingElement)
        {
            if (buildLayer == null || tilePreset == null || mappingElement == null)
                return;

            bool useDualGrid = NormalizeMappingGridSettings(tilePreset, mappingElement);
            buildLayer.useDualGrid = useDualGrid;
            buildLayer.scaleTileToCellSize = mappingElement.FindPropertyRelative("_scaleTileToCellSize")?.boolValue ?? useDualGrid;
            buildLayer.layerYOffset = 0f;

            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

            for (int i = 0; i < buildLayer.tileLayers.Count; i++)
            {
                buildLayer.tileLayers[i] ??= new TilesBuildLayer.TileLayers();
                buildLayer.tileLayers[i].heightOffset = 0f;
            }
        }

        private static bool NormalizeMappingGridSettings(TilePreset tilePreset, SerializedProperty mappingElement)
        {
            if (tilePreset == null || mappingElement == null)
                return false;

            var useDualGridProperty = mappingElement.FindPropertyRelative("_useDualGrid");
            var scaleToCellSizeProperty = mappingElement.FindPropertyRelative("_scaleTileToCellSize");

            bool useDualGrid = ShouldUseDualGrid(tilePreset, useDualGridProperty?.boolValue ?? false);
            bool scaleTileToCellSize = (scaleToCellSizeProperty?.boolValue ?? false) || useDualGrid;

            if (useDualGridProperty != null)
                useDualGridProperty.boolValue = useDualGrid;
            if (scaleToCellSizeProperty != null)
                scaleToCellSizeProperty.boolValue = scaleTileToCellSize;

            return useDualGrid;
        }

        private static bool HasUsableTopPreset(TilesBuildLayer buildLayer)
        {
            if (buildLayer?.tilePresetsTop == null || buildLayer.tilePresetsTop.Count == 0)
                return false;

            for (int i = 0; i < buildLayer.tilePresetsTop.Count; i++)
            {
                var preset = buildLayer.tilePresetsTop[i]?.preset;
                if (HasUsablePreset(preset, ShouldUseDualGrid(preset, buildLayer.useDualGrid)))
                    return true;
            }

            return false;
        }

        private static bool ShouldUseDualGrid(TilePreset preset, bool mappingPreference)
            => mappingPreference
                || preset != null && preset.gridtype == TilePreset.GridType.dual
                || preset != null && !HasUsablePreset(preset, false) && HasUsablePreset(preset, true);

        private static bool HasUsablePreset(TilePreset preset, bool useDualGrid)
        {
            if (preset == null)
                return false;

            if (useDualGrid)
            {
                return preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null;
            }

            return preset.NRMGRD_fillTile != null
                || preset.NRMGRD_singleTile != null
                || preset.NRMGRD_edgeFillTile != null
                || preset.NRMGRD_cornerFillTile != null
                || preset.NRMGRD_interiorCornerTile != null
                || preset.NRMGRD_doubleCornerTile != null
                || preset.NRMGRD_threeWayFillTile != null
                || preset.NRMGRD_edgeCornerFillTile != null;
        }

        private static string GuessLayerName(string id)
        {
            string normalized = (id ?? string.Empty).ToLowerInvariant();
            if (normalized.Contains("water") || normalized.Contains("ocean") || normalized.Contains("river")) return "Water";
            if (normalized.Contains("sand") || normalized.Contains("coast") || normalized.Contains("beach")) return "Sand";
            if (normalized.Contains("mountain") || normalized.Contains("cliff")) return "Mountain";
            if (normalized.Contains("hill") || normalized.Contains("stone")) return "Hill";
            if (normalized.Contains("snow")) return "Snow";
            if (normalized.Contains("road")) return "Road";
            if (normalized.Contains("forest")) return "Forest";
            return "Grass";
        }

        private static TilePreset GuessTilePreset(string id)
        {
            var presets = LoadTilePresets();
            string layerName = GuessLayerName(id).ToLowerInvariant();

            TilePreset fallback = null;
            foreach (var preset in presets)
            {
                if (preset == null)
                    continue;

                string presetName = preset.name.ToLowerInvariant();
                fallback ??= preset;

                if (layerName == "water" && presetName.Contains("water")) return preset;
                if (layerName == "sand" && presetName.Contains("sand")) return preset;
                if (layerName == "mountain" && (presetName.Contains("cliff") || presetName.Contains("ramp"))) return preset;
                if (layerName == "hill" && (presetName.Contains("green") || presetName.Contains("grass"))) return preset;
                if (layerName == "grass" && (presetName.Contains("grass") || presetName.Contains("green"))) return preset;
            }

            return fallback;
        }

        private static List<TilePreset> LoadTilePresets()
        {
            var result = new List<TilePreset>();
            string[] guids = AssetDatabase.FindAssets("t:TilePreset", new[] { "Assets/TileWorldCreator/Tiles URP" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var preset = AssetDatabase.LoadAssetAtPath<TilePreset>(path);
                if (preset != null)
                    result.Add(preset);
            }

            return result;
        }

        private static T FindFirstSceneObject<T>() where T : UnityEngine.Object
        {
#if UNITY_2023_1_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
#else
            var objects = Resources.FindObjectsOfTypeAll<T>();
            foreach (var obj in objects)
            {
                if (obj is Component component && component.gameObject.scene.IsValid())
                    return obj;
            }

            return null;
#endif
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }

        private static void SaveAll()
        {
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkAllScenesDirty();
        }
    }
}