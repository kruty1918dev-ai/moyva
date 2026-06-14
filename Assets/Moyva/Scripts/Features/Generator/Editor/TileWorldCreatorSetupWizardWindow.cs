using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Editor
{
    public sealed class TileWorldCreatorSetupWizardWindow : EditorWindow
    {
        public enum TileWorldCreatorSetupPreset
        {
            WaterGrassHills,
        }

        private readonly struct PresetLayerDefinition
        {
            public PresetLayerDefinition(string blueprintLayerName, string buildLayerName, float height, string tilePresetHint)
            {
                BlueprintLayerName = blueprintLayerName;
                BuildLayerName = buildLayerName;
                Height = height;
                TilePresetHint = tilePresetHint;
            }

            public string BlueprintLayerName { get; }
            public string BuildLayerName { get; }
            public float Height { get; }
            public string TilePresetHint { get; }
        }

        private const string DefaultAssetFolder = "Assets/Moyva/SO/Generation/TileWorldCreator";
        private const string DefaultConfigurationName = "MoyvaTileWorldCreatorConfiguration.asset";
        private const string DefaultMappingName = "MoyvaTileWorldCreatorIdMapping.asset";
        private const string DefaultSceneContextName = "SceneContext";
        private const string DefaultGridInstallerName = "GridInstaller";
        private const string DefaultPreviewCameraName = "Generator Preview Camera";
        private const string SignalsInstallerTypeName = "Kruty1918.Moyva.Signals.SignalBusInstaller, Kruty1918.Moyva.Signals";

        private TileWorldCreatorManager _manager;
        private SceneContext _sceneContext;
        private GridInstaller _gridInstaller;
        private MoyvaTileWorldCreatorGraphBinding _graphBinding;
        private GraphAsset _graphAsset;
        private TileRegistrySO _tileRegistry;
        private TileWorldCreatorIdMappingSO _mapping;
        private Camera _previewCamera;
        private string _assetFolder = DefaultAssetFolder;
        private bool _seedMappingFromTileRegistry = true;
        private bool _createTerrainBuildLayers = true;
        private bool _createObjectBuildLayers;
        private bool _syncTileRegistry = true;
        private bool _rebuildTileRegistryFromMapping = true;
        private bool _assignGraphBinding = true;
        private bool _autoSetupSceneContext = true;
        private bool _autoSetupSignalBusInstaller = true;
        private bool _autoSetupGridInstaller = true;
        private bool _autoSetupGraphBinding = true;
        private bool _autoSetupPreviewCamera = true;
        private bool _registerInstallersInSceneContext = true;
        private bool _drawMappingInspector = true;
        private TileWorldCreatorSetupPreset _selectedPreset = TileWorldCreatorSetupPreset.WaterGrassHills;
        private Vector2 _scroll;
        private UnityEditor.Editor _mappingEditor;

        [MenuItem("Moyva/Tools/TileWorldCreator/Setup Wizard")]
        public static void Open()
        {
            GetWindow<TileWorldCreatorSetupWizardWindow>("TWC Setup Wizard");
        }

        public static void OpenForManager(TileWorldCreatorManager manager, TileWorldCreatorSetupPreset preset = TileWorldCreatorSetupPreset.WaterGrassHills)
        {
            var window = GetWindow<TileWorldCreatorSetupWizardWindow>("TWC Setup Wizard");
            window._manager = manager;
            window._selectedPreset = preset;
            window.AutoFindReferences();
            window.Focus();
        }

        [MenuItem("CONTEXT/TileWorldCreatorManager/Moyva/Open Setup Wizard")]
        private static void OpenWizardFromContext(MenuCommand command)
        {
            OpenForManager(command.context as TileWorldCreatorManager);
        }

        [MenuItem("CONTEXT/TileWorldCreatorManager/Moyva/Apply Preset/Water Grass Hills")]
        private static void ApplyWaterGrassHillsPresetFromContext(MenuCommand command)
        {
            ApplyPreset(command.context as TileWorldCreatorManager, TileWorldCreatorSetupPreset.WaterGrassHills);
        }

        public static void ApplyPreset(TileWorldCreatorManager manager, TileWorldCreatorSetupPreset preset)
        {
            if (manager == null)
                return;

            var window = CreateInstance<TileWorldCreatorSetupWizardWindow>();
            window._manager = manager;
            window._selectedPreset = preset;
            window.AutoFindReferences();
            EnsureAssetFolder(window._assetFolder);
            window.EnsureManagerAndConfiguration();
            window.ApplySelectedPreset();
            SaveAll();
            DestroyImmediate(window);

            EditorUtility.DisplayDialog(
                "TWC Setup Wizard",
                $"Пресет '{GetPresetLabel(preset)}' застосовано до {manager.name}.",
                "OK");
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
                "Цей wizard створює TWC Manager/Configuration, заповнює TWC layers із ID Mapping, перебудовує TileRegistry як gameplay/TWC-реєстр і підключає runtime через Moyva TWC Graph Binding.",
                MessageType.Info);

            DrawReferencesSection();
            DrawAutomationSection();
            DrawPresetSection();
            DrawMappingSection();
            DrawActionsSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawReferencesSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Scene & Assets", EditorStyles.boldLabel);

            _manager = (TileWorldCreatorManager)EditorGUILayout.ObjectField("TWC Manager", _manager, typeof(TileWorldCreatorManager), true);
            _sceneContext = (SceneContext)EditorGUILayout.ObjectField("Scene Context", _sceneContext, typeof(SceneContext), true);
            _gridInstaller = (GridInstaller)EditorGUILayout.ObjectField("Grid Installer", _gridInstaller, typeof(GridInstaller), true);
            _graphBinding = (MoyvaTileWorldCreatorGraphBinding)EditorGUILayout.ObjectField("TWC Graph Binding", _graphBinding, typeof(MoyvaTileWorldCreatorGraphBinding), true);
            _graphAsset = (GraphAsset)EditorGUILayout.ObjectField("Graph Asset", _graphAsset, typeof(GraphAsset), false);
            _tileRegistry = (TileRegistrySO)EditorGUILayout.ObjectField("Tile Registry", _tileRegistry, typeof(TileRegistrySO), false);
            _mapping = (TileWorldCreatorIdMappingSO)EditorGUILayout.ObjectField("ID Mapping", _mapping, typeof(TileWorldCreatorIdMappingSO), false);
            _previewCamera = (Camera)EditorGUILayout.ObjectField("Preview/Main Camera", _previewCamera, typeof(Camera), true);
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
            _assignGraphBinding = EditorGUILayout.Toggle("Assign TWC Graph Binding", _assignGraphBinding);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Scene Auto-Setup", EditorStyles.boldLabel);
                _autoSetupSceneContext = EditorGUILayout.Toggle("Create SceneContext", _autoSetupSceneContext);
                _autoSetupSignalBusInstaller = EditorGUILayout.Toggle("Ensure SignalBusInstaller", _autoSetupSignalBusInstaller);
                _autoSetupGridInstaller = EditorGUILayout.Toggle("Ensure GridInstaller", _autoSetupGridInstaller);
                _autoSetupGraphBinding = EditorGUILayout.Toggle("Ensure TWC Graph Binding", _autoSetupGraphBinding);
                _registerInstallersInSceneContext = EditorGUILayout.Toggle("Register Installers in SceneContext", _registerInstallersInSceneContext);
                _autoSetupPreviewCamera = EditorGUILayout.Toggle("Ensure Preview Camera", _autoSetupPreviewCamera);
        }

        private void DrawPresetSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Quick Preset", EditorStyles.boldLabel);

            _selectedPreset = (TileWorldCreatorSetupPreset)EditorGUILayout.EnumPopup("Preset", _selectedPreset);

            EditorGUILayout.HelpBox(
                "Water Grass Hills створює paint-ready сценарій: Water = 0.00, Grass = 0.05 і декілька hill layers зі сходинками по 0.05 для ручного нарощування пагорбів.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(_manager == null))
            {
                if (GUILayout.Button("Apply Preset To Manager", GUILayout.Height(28f)))
                    ApplySelectedPreset();
            }
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

                if (GUILayout.Button("Auto-Setup Generator Scene", GUILayout.Height(34f)))
                    AutoSetupGeneratorScene();
            }

            EditorGUILayout.HelpBox(
                "Після натискання кнопки тобі лишається у Mapping виставити TilePreset для кожної групи ID, якщо wizard не зміг підібрати його автоматично.",
                MessageType.None);
        }

        private void ApplySelectedPreset()
        {
            EnsureManagerAndConfiguration();

            switch (_selectedPreset)
            {
                case TileWorldCreatorSetupPreset.WaterGrassHills:
                    ApplyWaterGrassHillsPreset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveAll();
        }

        private void AutoFindReferences()
        {
            _manager ??= FindFirstSceneObject<TileWorldCreatorManager>();
            _sceneContext ??= FindFirstSceneObject<SceneContext>();
            _gridInstaller ??= FindFirstSceneObject<GridInstaller>();
            _graphBinding ??= _manager != null
                ? _manager.GetComponent<MoyvaTileWorldCreatorGraphBinding>()
                : FindFirstSceneObject<MoyvaTileWorldCreatorGraphBinding>();
            _graphAsset ??= FindFirstAsset<GraphAsset>();
            _tileRegistry ??= FindFirstAsset<TileRegistrySO>();
            _mapping ??= FindFirstAsset<TileWorldCreatorIdMappingSO>();
            _previewCamera ??= Camera.main ?? FindFirstSceneObject<Camera>();
            Repaint();
        }

        private void AutoSetupGeneratorScene()
        {
            EnsureAssetFolder(_assetFolder);
            EnsureManagerAndConfiguration();
            EnsureMappingAsset();
            EnsureSceneScaffolding();

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

            EnsureGridInstallerBindings();
            if (_assignGraphBinding)
                AssignGraphBinding();

            SaveAll();
            EditorUtility.DisplayDialog("TWC Setup Wizard", "Scene auto-setup завершено. Сцена готова до запуску генератора.", "OK");
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

            if (_assignGraphBinding)
                AssignGraphBinding();

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

        private void ApplyWaterGrassHillsPreset()
        {
            var configuration = _manager?.configuration;
            if (configuration == null)
                return;

            Undo.RecordObject(_manager, "Apply TWC Water Grass Hills Preset");
            Undo.RecordObject(configuration, "Apply TWC Water Grass Hills Preset");

            var layers = new[]
            {
                new PresetLayerDefinition("Water", "Build Water", 0f, "water"),
                new PresetLayerDefinition("Grass", "Build Grass", 0.05f, "grass"),
                new PresetLayerDefinition("Hill 01", "Build Hill 01", 0.10f, "hill"),
                new PresetLayerDefinition("Hill 02", "Build Hill 02", 0.15f, "hill"),
                new PresetLayerDefinition("Hill 03", "Build Hill 03", 0.20f, "hill"),
            };

            for (int i = 0; i < layers.Length; i++)
            {
                ApplyPresetLayer(configuration, layers[i]);
            }

            EditorUtility.SetDirty(_manager);
            EditorUtility.SetDirty(configuration);

            if (_graphBinding != null)
                ApplyPresetToGraphBinding();
        }

        private void ApplyPresetLayer(Configuration configuration, PresetLayerDefinition definition)
        {
            var blueprintLayer = EnsureBlueprintLayer(configuration, definition.BlueprintLayerName);
            Undo.RecordObject(blueprintLayer, "Apply TWC Preset Layer");
            blueprintLayer.defaultLayerHeight = definition.Height;
            blueprintLayer.isEnabled = true;
            EditorUtility.SetDirty(blueprintLayer);

            var buildLayer = EnsureBuildLayer<TilesBuildLayer>(configuration, definition.BuildLayerName);
            Undo.RecordObject(buildLayer, "Apply TWC Preset Build Layer");
            buildLayer.SetBlueprintLayer(blueprintLayer);
            buildLayer.isEnabled = true;
            buildLayer.layerYOffset = 0f;

            TilePreset preset = GuessTilePreset(definition.TilePresetHint);
            if (preset != null)
            {
                buildLayer.SetNewTilePreset(preset);
                ApplyTilesBuildLayerSettings(buildLayer, preset, null);
            }
            else
            {
                buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
                if (buildLayer.tileLayers.Count == 0)
                    buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

                for (int i = 0; i < buildLayer.tileLayers.Count; i++)
                {
                    buildLayer.tileLayers[i] ??= new TilesBuildLayer.TileLayers();
                    buildLayer.tileLayers[i].heightOffset = 0f;
                }
            }

            EditorUtility.SetDirty(buildLayer);
        }

        private void ApplyPresetToGraphBinding()
        {
            var bindingObject = new SerializedObject(_graphBinding);

            var options = bindingObject.FindProperty("_tileWorldCreatorBuildOptions");
            if (options != null)
            {
                options.FindPropertyRelative("_applyIntegerTerrainHeights").boolValue = true;
                options.FindPropertyRelative("_normalizeTerrainLevelsForTileWorldCreator").boolValue = true;
                options.FindPropertyRelative("_waterTerrainLevel").intValue = 0;
                options.FindPropertyRelative("_shoreTerrainLevel").intValue = 1;
                options.FindPropertyRelative("_landTerrainLevel").intValue = 1;
                options.FindPropertyRelative("_hillTerrainLevel").intValue = 3;
                options.FindPropertyRelative("_maxTerrainLevel").intValue = 5;
            }

            bindingObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_graphBinding);
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

        private void AssignGraphBinding()
        {
            if (_manager == null || _mapping == null)
                return;

            _graphBinding = EnsureGraphBinding(_graphAsset != null ? _graphAsset : FindFirstAsset<GraphAsset>());
            if (_graphBinding == null)
                return;

            var bindingObject = new SerializedObject(_graphBinding);
            bindingObject.FindProperty("_manager").objectReferenceValue = _manager;
            bindingObject.FindProperty("_tileWorldCreatorIdMapping").objectReferenceValue = _mapping;

            var graphAsset = _graphAsset != null ? _graphAsset : FindFirstAsset<GraphAsset>();
            if (graphAsset != null)
            {
                bindingObject.FindProperty("_graphAsset").objectReferenceValue = graphAsset;
                EnsureGraphBinding(graphAsset);
            }

            var options = bindingObject.FindProperty("_tileWorldCreatorBuildOptions");
            if (options != null)
            {
                options.FindPropertyRelative("_replaceMappedTerrainVisuals").boolValue = true;
                options.FindPropertyRelative("_suppressMoyvaLayerDataWhenTerrainMapped").boolValue = true;
                options.FindPropertyRelative("_resetConfigurationBeforeBuild").boolValue = true;
                options.FindPropertyRelative("_syncConfigurationSize").boolValue = true;
                options.FindPropertyRelative("_useWorldSeed").boolValue = true;
            }

            bindingObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_graphBinding);
        }

        private MoyvaTileWorldCreatorGraphBinding EnsureGraphBinding(GraphAsset graphAsset)
        {
            if (_manager == null)
                return null;

            var binding = _manager.GetComponent<MoyvaTileWorldCreatorGraphBinding>();
            if (binding == null)
            {
                binding = Undo.AddComponent<MoyvaTileWorldCreatorGraphBinding>(_manager.gameObject);
            }

            if (graphAsset != null && binding.GraphAsset != graphAsset)
            {
                Undo.RecordObject(binding, "Assign Moyva graph binding");
                binding.SetGraphAsset(graphAsset);
                EditorUtility.SetDirty(binding);
            }

            return binding;
        }

        private void EnsureSceneScaffolding()
        {
            if (_autoSetupSceneContext)
                _sceneContext = EnsureSceneContext();

            if (_autoSetupGridInstaller)
                _gridInstaller = EnsureGridInstaller();

            if (_autoSetupGraphBinding)
                _graphBinding = EnsureGraphBinding(_graphAsset != null ? _graphAsset : FindFirstAsset<GraphAsset>());

            MonoBehaviour signalsInstaller = null;
            if (_autoSetupSignalBusInstaller)
                signalsInstaller = EnsureInstallerByTypeName(SignalsInstallerTypeName, "SignalBusInstaller");

            if (_registerInstallersInSceneContext && _sceneContext != null)
            {
                if (signalsInstaller != null)
                    EnsureSceneContextContainsInstaller(_sceneContext, signalsInstaller);
                if (_gridInstaller != null)
                    EnsureSceneContextContainsInstaller(_sceneContext, _gridInstaller);
                if (_graphBinding != null)
                    EnsureSceneContextContainsInstaller(_sceneContext, _graphBinding);
            }

            if (_autoSetupPreviewCamera)
                _previewCamera = EnsurePreviewCamera();
        }

        private SceneContext EnsureSceneContext()
        {
            if (_sceneContext != null)
                return _sceneContext;

            var go = new GameObject(DefaultSceneContextName);
            Undo.RegisterCreatedObjectUndo(go, "Create SceneContext");
            _sceneContext = go.AddComponent<SceneContext>();
            EditorUtility.SetDirty(_sceneContext);
            return _sceneContext;
        }

        private GridInstaller EnsureGridInstaller()
        {
            if (_gridInstaller != null)
                return _gridInstaller;

            var existing = FindFirstSceneObject<GridInstaller>();
            if (existing != null)
            {
                _gridInstaller = existing;
                return _gridInstaller;
            }

            var go = new GameObject(DefaultGridInstallerName);
            Undo.RegisterCreatedObjectUndo(go, "Create GridInstaller");
            _gridInstaller = go.AddComponent<GridInstaller>();
            EditorUtility.SetDirty(_gridInstaller);
            return _gridInstaller;
        }

        private MonoBehaviour EnsureInstallerByTypeName(string assemblyQualifiedTypeName, string fallbackGameObjectName)
        {
            var type = ResolveType(assemblyQualifiedTypeName);
            if (type == null || !typeof(MonoBehaviour).IsAssignableFrom(type))
                return null;

            var existing = FindFirstSceneObject(type) as MonoBehaviour;
            if (existing != null)
                return existing;

            var go = new GameObject(fallbackGameObjectName);
            Undo.RegisterCreatedObjectUndo(go, $"Create {fallbackGameObjectName}");
            var installer = go.AddComponent(type) as MonoBehaviour;
            if (installer != null)
                EditorUtility.SetDirty(installer);
            return installer;
        }

        private void EnsureGridInstallerBindings()
        {
            if (_gridInstaller == null || _tileRegistry == null)
                return;

            var so = new SerializedObject(_gridInstaller);
            var tileRegistryProperty = so.FindProperty("tileRegistry");
            if (tileRegistryProperty != null)
                tileRegistryProperty.objectReferenceValue = _tileRegistry;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_gridInstaller);
        }

        private static void EnsureSceneContextContainsInstaller(SceneContext sceneContext, MonoBehaviour installer)
        {
            if (sceneContext == null || installer == null)
                return;

            var sceneContextSo = new SerializedObject(sceneContext);
            var installersProperty = sceneContextSo.FindProperty("_monoInstallers");
            if (installersProperty == null || !installersProperty.isArray)
                return;

            for (int i = 0; i < installersProperty.arraySize; i++)
            {
                if (installersProperty.GetArrayElementAtIndex(i).objectReferenceValue == installer)
                    return;
            }

            int index = installersProperty.arraySize;
            installersProperty.InsertArrayElementAtIndex(index);
            installersProperty.GetArrayElementAtIndex(index).objectReferenceValue = installer;
            sceneContextSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(sceneContext);
        }

        private Camera EnsurePreviewCamera()
        {
            if (_previewCamera != null)
                return _previewCamera;

            _previewCamera = Camera.main ?? FindFirstSceneObject<Camera>();
            if (_previewCamera != null)
                return _previewCamera;

            var cameraGo = new GameObject(DefaultPreviewCameraName);
            Undo.RegisterCreatedObjectUndo(cameraGo, "Create Preview Camera");
            _previewCamera = cameraGo.AddComponent<Camera>();
            cameraGo.tag = "MainCamera";
            _previewCamera.transform.position = new Vector3(0f, 24f, -24f);
            _previewCamera.transform.rotation = Quaternion.Euler(35f, 45f, 0f);
            _previewCamera.clearFlags = CameraClearFlags.Skybox;
            _previewCamera.nearClipPlane = 0.01f;
            _previewCamera.farClipPlane = 1000f;
            EditorUtility.SetDirty(_previewCamera);
            return _previewCamera;
        }

        private static Type ResolveType(string assemblyQualifiedTypeName)
        {
            if (string.IsNullOrWhiteSpace(assemblyQualifiedTypeName))
                return null;

            var type = Type.GetType(assemblyQualifiedTypeName);
            if (type != null)
                return type;

            string fullTypeName = assemblyQualifiedTypeName.Split(',')[0].Trim();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                type = assemblies[i].GetType(fullTypeName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static UnityEngine.Object FindFirstSceneObject(Type type)
        {
            if (type == null)
                return null;

#if UNITY_2023_1_OR_NEWER
            var found = UnityEngine.Object.FindFirstObjectByType(type, FindObjectsInactive.Include);
            if (found is Component c && c.gameObject.scene.IsValid())
                return found;
            return null;
#else
            var objects = Resources.FindObjectsOfTypeAll(type);
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] is Component component && component.gameObject.scene.IsValid())
                    return objects[i];
            }

            return null;
#endif
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
            if (buildLayer == null || tilePreset == null)
                return;

            bool useDualGrid = NormalizeMappingGridSettings(tilePreset, mappingElement);
            buildLayer.useDualGrid = useDualGrid;
            buildLayer.scaleTileToCellSize = mappingElement?.FindPropertyRelative("_scaleTileToCellSize")?.boolValue ?? useDualGrid;
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
            if (tilePreset == null)
                return false;

            var useDualGridProperty = mappingElement?.FindPropertyRelative("_useDualGrid");
            var scaleToCellSizeProperty = mappingElement?.FindPropertyRelative("_scaleTileToCellSize");

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

        private static string GetPresetLabel(TileWorldCreatorSetupPreset preset)
        {
            return preset switch
            {
                TileWorldCreatorSetupPreset.WaterGrassHills => "Water Grass Hills",
                _ => preset.ToString(),
            };
        }
    }
}