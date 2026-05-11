using System;
using System.Collections.Generic;
using System.Globalization;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow
    {
        private enum WorkspaceMode
        {
            UnitDesigner,
            GeneratorMap,
            CombatSystem
        }

        private const string GeneratorAssetGuidPrefsKey = "Moyva.UnitDesigner.GeneratorAssetGuid";
        private const string NoiseSettingsGuidPrefsKey = "Moyva.UnitDesigner.NoiseSettingsGuid";
        private const string HeightSettingsGuidPrefsKey = "Moyva.UnitDesigner.HeightSettingsGuid";
        private const string GeneratorPreviewWidthPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewWidth";
        private const string GeneratorPreviewHeightPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewHeight";
        private const string GeneratorPreviewSeedPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewSeed";
        private const string GeneratorPreviewUnitXPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewUnitX";
        private const string GeneratorPreviewUnitYPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewUnitY";
        private const string GeneratorPreviewFogUnionPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewFogUnion";
        private const string GeneratorPreviewAutoPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewAuto";
        private const string GeneratorPreviewFogPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewFog";
        private const string GeneratorPreviewZoomPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewZoom";
        private const string GeneratorVisionGlobalBoostPrefsKey = "Moyva.UnitDesigner.GeneratorVisionGlobalBoost";
        private const string GeneratorVisionTerrainLosPrefsKey = "Moyva.UnitDesigner.GeneratorVisionTerrainLos";
        private const string GeneratorVisionOcclusionTolerancePrefsKey = "Moyva.UnitDesigner.GeneratorVisionOcclusionTolerance";
        private const string GeneratorVisionDownhillPenaltyPrefsKey = "Moyva.UnitDesigner.GeneratorVisionDownhillPenalty";
        private const string GeneratorVisionUphillPeekStrengthPrefsKey = "Moyva.UnitDesigner.GeneratorVisionUphillPeekStrength";
        private const string GeneratorVisionUphillEdgeDropPrefsKey = "Moyva.UnitDesigner.GeneratorVisionUphillEdgeDrop";
        private const string GeneratorVisionEdgePeekDistancePrefsKey = "Moyva.UnitDesigner.GeneratorVisionEdgePeekDistance";
        private const string GeneratorVisionEdgeBlindZonePrefsKey = "Moyva.UnitDesigner.GeneratorVisionEdgeBlindZone";
        private const string GeneratorVisionEdgeMaxBlindZonePrefsKey = "Moyva.UnitDesigner.GeneratorVisionEdgeMaxBlindZone";
        private const string GeneratorVisionEdgeDistanceScalePrefsKey = "Moyva.UnitDesigner.GeneratorVisionEdgeDistanceScale";
        private const string GeneratorFogHiddenOpacityPrefsKey = "Moyva.UnitDesigner.GeneratorFogHiddenOpacity";
        private const string GeneratorFogVisibleTintPrefsKey = "Moyva.UnitDesigner.GeneratorFogVisibleTint";

        private const string DataNoiseSettingsTypeName = "Kruty1918.Moyva.Generator.API.DataNoiseSettings";
        private const string HeightMapSettingsTypeName = "Kruty1918.Moyva.Generator.API.HeightMapSettings";
        private const string GraphAssetTypeName = "Kruty1918.Moyva.GraphSystem.API.GraphAsset";
        private const string HillGeneratorNodeTypeName = "Kruty1918.Moyva.Generator.Runtime.Nodes.HillGeneratorNode";

        private static readonly Color[] GeneratorLevelPalette =
        {
            new Color(0.08f, 0.26f, 0.46f),
            new Color(0.14f, 0.48f, 0.72f),
            new Color(0.26f, 0.58f, 0.35f),
            new Color(0.55f, 0.62f, 0.33f),
            new Color(0.62f, 0.48f, 0.33f),
            new Color(0.58f, 0.54f, 0.52f),
            new Color(0.82f, 0.84f, 0.82f),
        };

        private WorkspaceMode _workspaceMode;
        private ScriptableObject _generatorAsset;
        private ScriptableObject _noiseSettingsAsset;
        private ScriptableObject _heightSettingsAsset;
        private ScriptableObject _hillGeneratorNodeAsset;
        private SerializedObject _noiseSettingsObject;
        private SerializedObject _heightSettingsObject;
        private Texture2D _generatorPreviewTexture;
        private Texture2D _generatorVisibilityOverlayTexture;
        private float[,] _generatorPreviewNoiseMap;
        private string[,] _generatorPreviewTileMap;
        private int[,] _generatorPreviewLevelMap;
        private bool[,] _generatorPreviewVisibleMap;
        private byte[,] _generatorPreviewViewerCountMap;
        private Vector2 _generatorLevelsScroll;
        private Vector2 _generatorPreviewScroll;
        private Vector2 _generatorLegendScroll;
        private Vector2 _generatorMapCanvasScroll;
        private Vector2Int _generatorPreviewUnitPosition = new Vector2Int(32, 32);
        private readonly List<PreviewScenarioUnit> _generatorScenarioUnits = new List<PreviewScenarioUnit>();
        private int _generatorScenarioActiveIndex;
        private bool _generatorPreviewFogUnion;
        private Rect _lastGeneratorPreviewRect;
        private Vector2 _generatorPreviewMouse;
        private bool _generatorPreviewIsPanning;
        private Vector2 _generatorPreviewPanLastMouse;
        private bool _generatorPreviewIsDraggingUnit;
        private int _generatorPreviewDraggingUnitIndex = -1;
        private bool _generatorPreviewFocusActiveUnitOnNextDraw = true;
        private bool _generatorMapSettingsFoldout = true;
        private bool _generatorScenarioFoldout = true;
        private bool _generatorVisionFogFoldout = true;
        private bool _generatorSummaryFoldout = true;
        private bool _generatorLegendFoldout;
        private int _generatorPreviewWidth = 64;
        private int _generatorPreviewHeight = 64;
        private int _generatorPreviewSeed = 1918;
        private bool _generatorPreviewAutoRefresh = true;
        private bool _generatorPreviewShowFog = true;
        private float _generatorPreviewZoom = 1f;
        private float _generatorFogHiddenOpacity = 0.68f;
        private float _generatorFogVisibleTint = 0.24f;
        private float _generatorVisionGlobalBoost = 0.35f;
        private bool _generatorVisionUseTerrainLos = true;
        private float _generatorVisionOcclusionTolerance = 0.035f;
        private float _generatorVisionDownhillPenalty = 0.45f;
        private float _generatorVisionUphillPeekStrength = 0.6f;
        private float _generatorVisionUphillEdgeDrop = 0.05f;
        private int _generatorVisionEdgePeekDistanceTiles = 1;
        private int _generatorVisionEdgeBlindZoneTiles = 2;
        private int _generatorVisionEdgeMaxBlindZoneTiles = 4;
        private float _generatorVisionEdgeDistanceScale = 0.35f;
        private bool _generatorPreviewDirty = true;
        private string _generatorPreviewStatus = "Preview ще не побудовано.";
        private int _generatorPreviewVisibleCells;
        private int _generatorPreviewTotalCells;
        private bool _generatorPreviewUsesHillNodeLevels;
        private string _generatorPreviewLevelSource = "HeightLayers";
        private float _generatorPreviewMinHeight;
        private float _generatorPreviewMaxHeight;
        private float _generatorPreviewAverageHeight;
        private int _generatorPreviewRuntimeSignature;
        private int _generatorVisionPreviewRuntimeSignature;
        private double _generatorPreviewDirtySince;
        private bool _generatorVisionPreviewDirty = true;
        private double _generatorVisionPreviewDirtySince;

        private const double GeneratorPreviewRebuildDebounceSeconds = 0.06d;
        private const double GeneratorVisionPreviewRebuildDebounceSeconds = 0.025d;

        private void InitializeGeneratorMapDesigner()
        {
            _generatorPreviewWidth = Mathf.Clamp(EditorPrefs.GetInt(GeneratorPreviewWidthPrefsKey, 64), 8, 256);
            _generatorPreviewHeight = Mathf.Clamp(EditorPrefs.GetInt(GeneratorPreviewHeightPrefsKey, 64), 8, 256);
            _generatorPreviewSeed = EditorPrefs.GetInt(GeneratorPreviewSeedPrefsKey, 1918);
            _generatorPreviewUnitPosition = new Vector2Int(
                Mathf.Clamp(EditorPrefs.GetInt(GeneratorPreviewUnitXPrefsKey, _generatorPreviewWidth / 2), 0, _generatorPreviewWidth - 1),
                Mathf.Clamp(EditorPrefs.GetInt(GeneratorPreviewUnitYPrefsKey, _generatorPreviewHeight / 2), 0, _generatorPreviewHeight - 1));
            _generatorPreviewAutoRefresh = EditorPrefs.GetBool(GeneratorPreviewAutoPrefsKey, true);
            _generatorPreviewShowFog = EditorPrefs.GetBool(GeneratorPreviewFogPrefsKey, true);
            _generatorPreviewFogUnion = EditorPrefs.GetBool(GeneratorPreviewFogUnionPrefsKey, false);
            _generatorPreviewZoom = Mathf.Clamp(EditorPrefs.GetFloat(GeneratorPreviewZoomPrefsKey, 1.5f), 0.25f, 8f);
            _generatorFogHiddenOpacity = Mathf.Clamp01(EditorPrefs.GetFloat(GeneratorFogHiddenOpacityPrefsKey, 0.68f));
            _generatorFogVisibleTint = Mathf.Clamp01(EditorPrefs.GetFloat(GeneratorFogVisibleTintPrefsKey, 0.24f));
            _generatorVisionGlobalBoost = Mathf.Clamp(EditorPrefs.GetFloat(GeneratorVisionGlobalBoostPrefsKey, 0.35f), 0f, 4f);
            _generatorVisionUseTerrainLos = EditorPrefs.GetBool(GeneratorVisionTerrainLosPrefsKey, true);
            _generatorVisionOcclusionTolerance = Mathf.Clamp(EditorPrefs.GetFloat(GeneratorVisionOcclusionTolerancePrefsKey, 0.035f), 0f, 0.2f);
            _generatorVisionDownhillPenalty = Mathf.Clamp01(EditorPrefs.GetFloat(GeneratorVisionDownhillPenaltyPrefsKey, 0.45f));
            _generatorVisionUphillPeekStrength = Mathf.Clamp01(EditorPrefs.GetFloat(GeneratorVisionUphillPeekStrengthPrefsKey, 0.6f));
            _generatorVisionUphillEdgeDrop = Mathf.Clamp(EditorPrefs.GetFloat(GeneratorVisionUphillEdgeDropPrefsKey, 0.05f), 0f, 0.2f);
            _generatorVisionEdgePeekDistanceTiles = Mathf.Clamp(EditorPrefs.GetInt(GeneratorVisionEdgePeekDistancePrefsKey, 1), 0, 6);
            _generatorVisionEdgeBlindZoneTiles = Mathf.Clamp(EditorPrefs.GetInt(GeneratorVisionEdgeBlindZonePrefsKey, 2), 0, 8);
            _generatorVisionEdgeMaxBlindZoneTiles = Mathf.Clamp(EditorPrefs.GetInt(GeneratorVisionEdgeMaxBlindZonePrefsKey, 4), _generatorVisionEdgeBlindZoneTiles, 10);
            _generatorVisionEdgeDistanceScale = Mathf.Clamp(EditorPrefs.GetFloat(GeneratorVisionEdgeDistanceScalePrefsKey, 0.35f), 0f, 1.5f);

            _generatorAsset = LoadScriptableObjectPreference(GeneratorAssetGuidPrefsKey);
            _noiseSettingsAsset = LoadScriptableObjectPreference(NoiseSettingsGuidPrefsKey) ?? FindFirstAssetOfType(DataNoiseSettingsTypeName);
            _heightSettingsAsset = LoadScriptableObjectPreference(HeightSettingsGuidPrefsKey) ?? FindFirstAssetOfType(HeightMapSettingsTypeName);

            if (_generatorAsset == null)
                _generatorAsset = FindFirstAssetOfType(GraphAssetTypeName);

            if (_generatorAsset != null && (_noiseSettingsAsset == null || _heightSettingsAsset == null))
                ExtractGeneratorReferencesFromAsset(applyMapSize: true);

            RefreshGeneratorMapSerializedObjects();
            EnsureGeneratorScenarioInitialized();
            _generatorPreviewFocusActiveUnitOnNextDraw = true;
            MarkGeneratorPreviewDirty();
        }

        private void DisposeGeneratorMapDesigner()
        {
            SaveScriptableObjectPreference(GeneratorAssetGuidPrefsKey, _generatorAsset);
            SaveScriptableObjectPreference(NoiseSettingsGuidPrefsKey, _noiseSettingsAsset);
            SaveScriptableObjectPreference(HeightSettingsGuidPrefsKey, _heightSettingsAsset);
            EditorPrefs.SetInt(GeneratorPreviewWidthPrefsKey, _generatorPreviewWidth);
            EditorPrefs.SetInt(GeneratorPreviewHeightPrefsKey, _generatorPreviewHeight);
            EditorPrefs.SetInt(GeneratorPreviewSeedPrefsKey, _generatorPreviewSeed);
            EditorPrefs.SetInt(GeneratorPreviewUnitXPrefsKey, _generatorPreviewUnitPosition.x);
            EditorPrefs.SetInt(GeneratorPreviewUnitYPrefsKey, _generatorPreviewUnitPosition.y);
            EditorPrefs.SetBool(GeneratorPreviewAutoPrefsKey, _generatorPreviewAutoRefresh);
            EditorPrefs.SetBool(GeneratorPreviewFogPrefsKey, _generatorPreviewShowFog);
            EditorPrefs.SetBool(GeneratorPreviewFogUnionPrefsKey, _generatorPreviewFogUnion);
            EditorPrefs.SetFloat(GeneratorPreviewZoomPrefsKey, _generatorPreviewZoom);
            EditorPrefs.SetFloat(GeneratorFogHiddenOpacityPrefsKey, _generatorFogHiddenOpacity);
            EditorPrefs.SetFloat(GeneratorFogVisibleTintPrefsKey, _generatorFogVisibleTint);
            EditorPrefs.SetFloat(GeneratorVisionGlobalBoostPrefsKey, _generatorVisionGlobalBoost);
            EditorPrefs.SetBool(GeneratorVisionTerrainLosPrefsKey, _generatorVisionUseTerrainLos);
            EditorPrefs.SetFloat(GeneratorVisionOcclusionTolerancePrefsKey, _generatorVisionOcclusionTolerance);
            EditorPrefs.SetFloat(GeneratorVisionDownhillPenaltyPrefsKey, _generatorVisionDownhillPenalty);
            EditorPrefs.SetFloat(GeneratorVisionUphillPeekStrengthPrefsKey, _generatorVisionUphillPeekStrength);
            EditorPrefs.SetFloat(GeneratorVisionUphillEdgeDropPrefsKey, _generatorVisionUphillEdgeDrop);
            EditorPrefs.SetInt(GeneratorVisionEdgePeekDistancePrefsKey, _generatorVisionEdgePeekDistanceTiles);
            EditorPrefs.SetInt(GeneratorVisionEdgeBlindZonePrefsKey, _generatorVisionEdgeBlindZoneTiles);
            EditorPrefs.SetInt(GeneratorVisionEdgeMaxBlindZonePrefsKey, _generatorVisionEdgeMaxBlindZoneTiles);
            EditorPrefs.SetFloat(GeneratorVisionEdgeDistanceScalePrefsKey, _generatorVisionEdgeDistanceScale);

            if (_generatorPreviewTexture != null)
                DestroyImmediate(_generatorPreviewTexture);

            if (_generatorVisibilityOverlayTexture != null)
                DestroyImmediate(_generatorVisibilityOverlayTexture);
        }

        private void RefreshGeneratorMapSerializedObjects()
        {
            _noiseSettingsObject = _noiseSettingsAsset != null ? new SerializedObject(_noiseSettingsAsset) : null;
            _heightSettingsObject = _heightSettingsAsset != null ? new SerializedObject(_heightSettingsAsset) : null;
        }

        private void UpdateGeneratorMapSerializedObjects()
        {
            _noiseSettingsObject?.Update();
            _heightSettingsObject?.Update();
        }

        private bool IsGeneratorMapWorkspaceActive() => _workspaceMode == WorkspaceMode.GeneratorMap;

        private void DrawWorkspaceModeToolbar(bool compactMode)
        {
            string[] modes = { "Юніти", "Мапа + Fog", "Бій" };

            GUILayout.Space(compactMode ? 4f : 8f);

            if (compactMode || _workspaceTabsInlineMenu)
            {
                if (!compactMode && GUILayout.Button(IconContent("d_FilterByLabel", "Tabs", EditorTooltipStandard.Build("Розгорнути вкладки назад у toolbar.", "Зараз робочі простори показані як компактне inline меню.")), EditorStyles.toolbarButton, GUILayout.Width(54f)))
                    _workspaceTabsInlineMenu = false;

                int next = EditorGUILayout.Popup(
                    Mathf.Clamp((int)_workspaceMode, 0, modes.Length - 1),
                    modes,
                    EditorStyles.toolbarPopup,
                    GUILayout.Width(compactMode ? 118f : 160f));
                SetWorkspaceMode((WorkspaceMode)next);
                return;
            }

            int toolbarNext = GUILayout.Toolbar((int)_workspaceMode, modes, EditorStyles.toolbarButton, GUILayout.Width(260f));
            SetWorkspaceMode((WorkspaceMode)toolbarNext);

            if (GUILayout.Button(IconContent("d_FilterByLabel", "Menu", EditorTooltipStandard.Build("Згорнути вкладки у компактне inline меню.", "Активна вкладка лишається видимою, а перемикання йде через dropdown.")), EditorStyles.toolbarButton, GUILayout.Width(58f)))
                _workspaceTabsInlineMenu = true;
        }

        private void SetWorkspaceMode(WorkspaceMode nextMode)
        {
            if (_workspaceMode == nextMode)
                return;

            _workspaceMode = nextMode;
            if (_workspaceMode == WorkspaceMode.GeneratorMap)
                MarkGeneratorPreviewDirty();
            if (_workspaceMode == WorkspaceMode.CombatSystem)
                _combatFacade.OnWorkspaceSelected();
        }

        private void DrawGeneratorMapWorkspace()
        {
            MaybeRebuildGeneratorPreview();
            ClampLayoutWidths();

            EditorGUILayout.BeginHorizontal();
            DrawUnitListPanel(GUILayout.Width(_generatorListPanelWidth), GUILayout.ExpandHeight(true));
            DrawColumnSplitter(ref _generatorListPanelWidth, ref _generatorSettingsPanelWidth, MinUnitListPanelWidth, MinDetailsPanelWidth, GeneratorListWidthPrefsKey, GeneratorSettingsWidthPrefsKey);
            DrawGeneratorLevelsPanel(GUILayout.Width(_generatorSettingsPanelWidth), GUILayout.MinWidth(MinDetailsPanelWidth), GUILayout.ExpandHeight(true));
            DrawColumnSplitter(ref _generatorSettingsPanelWidth, ref _generatorPreviewPanelWidth, MinDetailsPanelWidth, MinPreviewPanelWidth, GeneratorSettingsWidthPrefsKey, GeneratorPreviewPanelWidthPrefsKey);
            DrawGeneratorMapPreviewPanel(GUILayout.Width(_generatorPreviewPanelWidth), GUILayout.MinWidth(MinPreviewPanelWidth), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGeneratorLevelsPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Рівні генератора", IconContent("d_Terrain Icon", string.Empty, "Налаштування шуму та HeightMapSettings для preview юнітів на мапі."));

            _generatorLevelsScroll = EditorGUILayout.BeginScrollView(_generatorLevelsScroll);
            DrawGeneratorAssetSection();
            DrawNoiseSettingsSection();
            DrawHeightLevelsSection();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawGeneratorAssetSection()
        {
            BeginSection("Генератор", "d_Project", "GraphAsset або інший ScriptableObject генератора, з якого можна підтягнути noise/height settings.");

            EditorGUI.BeginChangeCheck();
            var nextGenerator = EditorGUILayout.ObjectField(
                new GUIContent("Generator Asset", "Передай GraphAsset/генератор. Unit Designer спробує знайти всередині DataNoiseSettings та HeightMapSettings."),
                _generatorAsset,
                typeof(ScriptableObject),
                false) as ScriptableObject;
            if (EditorGUI.EndChangeCheck())
            {
                _generatorAsset = nextGenerator;
                SaveScriptableObjectPreference(GeneratorAssetGuidPrefsKey, _generatorAsset);
                ExtractGeneratorReferencesFromAsset(applyMapSize: true);
                RefreshGeneratorMapSerializedObjects();
                MarkGeneratorPreviewDirty();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("Витягнути settings", "Сканує generator asset/sub-assets і підставляє знайдені noise/height settings."), GUILayout.Height(24f)))
                {
                    ExtractGeneratorReferencesFromAsset(applyMapSize: true);
                    RefreshGeneratorMapSerializedObjects();
                    MarkGeneratorPreviewDirty();
                }

                using (new EditorGUI.DisabledScope(_generatorAsset == null))
                {
                    if (GUILayout.Button(new GUIContent("Ping", "Показати generator asset у Project."), GUILayout.Width(58f), GUILayout.Height(24f)))
                    {
                        Selection.activeObject = _generatorAsset;
                        EditorGUIUtility.PingObject(_generatorAsset);
                    }
                }
            }

            EndSection();
        }

        private void DrawNoiseSettingsSection()
        {
            BeginSection("Шум висоти", "d_PreMatSphere", "DataNoiseSettings визначає базову карту шуму 0..1.");

            EditorGUI.BeginChangeCheck();
            var nextNoise = EditorGUILayout.ObjectField(
                new GUIContent("Noise Settings", "DataNoiseSettings, який використовується для генерації height/noise preview."),
                _noiseSettingsAsset,
                ResolveUnityType(DataNoiseSettingsTypeName) ?? typeof(ScriptableObject),
                false) as ScriptableObject;
            if (EditorGUI.EndChangeCheck())
            {
                _noiseSettingsAsset = nextNoise;
                _noiseSettingsObject = _noiseSettingsAsset != null ? new SerializedObject(_noiseSettingsAsset) : null;
                SaveScriptableObjectPreference(NoiseSettingsGuidPrefsKey, _noiseSettingsAsset);
                MarkGeneratorPreviewDirty();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Знайти", GUILayout.Height(24f)))
                {
                    _noiseSettingsAsset = FindFirstAssetOfType(DataNoiseSettingsTypeName);
                    _noiseSettingsObject = _noiseSettingsAsset != null ? new SerializedObject(_noiseSettingsAsset) : null;
                    SaveScriptableObjectPreference(NoiseSettingsGuidPrefsKey, _noiseSettingsAsset);
                    MarkGeneratorPreviewDirty();
                }

                if (GUILayout.Button("Створити", GUILayout.Height(24f)))
                    CreateNoiseSettingsAsset();
            }

            if (_noiseSettingsObject == null)
            {
                EditorGUILayout.HelpBox("Noise settings не задано. Preview мапи не зможе згенерувати карту шуму.", MessageType.Warning);
                EndSection();
                return;
            }

            EditorGUI.BeginChangeCheck();
            DrawSerializedObjectProperties(_noiseSettingsObject);
            if (EditorGUI.EndChangeCheck())
            {
                _noiseSettingsObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_noiseSettingsAsset);
                MarkGeneratorPreviewDirty();
            }
            else
            {
                _noiseSettingsObject.ApplyModifiedProperties();
            }

            EndSection();
        }

        private void DrawHeightLevelsSection()
        {
            BeginSection("Рівні висоти", "d_TerrainInspector.TerrainToolSetheight", "HeightMapSettings: які діапазони шуму вважаються водою, землею, пагорбом, горами тощо.");

            EditorGUI.BeginChangeCheck();
            var nextHeight = EditorGUILayout.ObjectField(
                new GUIContent("Height Settings", "HeightMapSettings з масивом HeightLayers."),
                _heightSettingsAsset,
                ResolveUnityType(HeightMapSettingsTypeName) ?? typeof(ScriptableObject),
                false) as ScriptableObject;
            if (EditorGUI.EndChangeCheck())
            {
                _heightSettingsAsset = nextHeight;
                _heightSettingsObject = _heightSettingsAsset != null ? new SerializedObject(_heightSettingsAsset) : null;
                SaveScriptableObjectPreference(HeightSettingsGuidPrefsKey, _heightSettingsAsset);
                MarkGeneratorPreviewDirty();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Знайти", GUILayout.Height(24f)))
                {
                    _heightSettingsAsset = FindFirstAssetOfType(HeightMapSettingsTypeName);
                    _heightSettingsObject = _heightSettingsAsset != null ? new SerializedObject(_heightSettingsAsset) : null;
                    SaveScriptableObjectPreference(HeightSettingsGuidPrefsKey, _heightSettingsAsset);
                    MarkGeneratorPreviewDirty();
                }

                if (GUILayout.Button("Створити", GUILayout.Height(24f)))
                    CreateHeightSettingsAsset();
            }

            if (_heightSettingsObject == null)
            {
                EditorGUILayout.HelpBox("HeightMapSettings не задано. Створи або вибери asset, щоб налаштувати рівні.", MessageType.Warning);
                EndSection();
                return;
            }

            var layers = _heightSettingsObject.FindProperty("HeightLayers");
            if (layers == null || !layers.isArray)
            {
                EditorGUILayout.HelpBox("У вибраному asset не знайдено масив HeightLayers.", MessageType.Error);
                EndSection();
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Додати рівень", GUILayout.Height(24f)))
                {
                    layers.InsertArrayElementAtIndex(layers.arraySize);
                    InitializeHeightLayer(layers.GetArrayElementAtIndex(layers.arraySize - 1), layers.arraySize - 1, layers.arraySize);
                    ApplyHeightLevelChanges();
                }

                using (new EditorGUI.DisabledScope(layers.arraySize == 0))
                {
                    if (GUILayout.Button("Розкласти 0..1", GUILayout.Height(24f)))
                    {
                        NormalizeHeightLayers(layers);
                        ApplyHeightLevelChanges();
                    }
                }
            }

            DrawHeightLevelValidation(layers);

            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                if (DrawHeightLayerEditor(layer, i, layers.arraySize))
                {
                    layers.DeleteArrayElementAtIndex(i);
                    ApplyHeightLevelChanges();
                    break;
                }
            }

            _heightSettingsObject.ApplyModifiedProperties();
            EndSection();
        }

        private void DrawGeneratorMapPreviewPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Map + Fog Preview", IconContent("d_scenevis_visible_hover", string.Empty, "Preview згенерованої мапи з Fog of War та радіусом огляду вибраного юніта."));

            DrawGeneratorPreviewNavigationStrip();
            DrawGeneratorPreviewTexture();

            _generatorPreviewScroll = EditorGUILayout.BeginScrollView(_generatorPreviewScroll);
            DrawGeneratorPreviewControls();
            if (BeginFoldoutSection(ref _generatorLegendFoldout, "Легенда висот", "d_FilterByLabel", "Поточні рівні висоти та їхні кольори у preview."))
            {
                DrawGeneratorPreviewLegend();
                EndFoldoutSection();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawGeneratorPreviewNavigationStrip()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Навігація", EditorStyles.miniBoldLabel, GUILayout.Width(72f));

                if (GUILayout.Button("-", EditorStyles.toolbarButton, GUILayout.Width(24f)))
                    _generatorPreviewZoom = Mathf.Clamp(_generatorPreviewZoom * 0.9f, 0.25f, 8f);

                _generatorPreviewZoom = GUILayout.HorizontalSlider(_generatorPreviewZoom, 0.25f, 8f, GUILayout.Width(Mathf.Clamp(_generatorPreviewPanelWidth * 0.28f, 90f, 170f)));

                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(24f)))
                    _generatorPreviewZoom = Mathf.Clamp(_generatorPreviewZoom * 1.1f, 0.25f, 8f);

                GUILayout.Label($"{_generatorPreviewZoom:0.00}x", EditorStyles.miniLabel, GUILayout.Width(42f));

                if (GUILayout.Button("Fit", EditorStyles.toolbarButton, GUILayout.Width(42f)))
                {
                    _generatorPreviewZoom = CalculateGeneratorFitZoom(_lastGeneratorPreviewRect);
                    _generatorPreviewFocusActiveUnitOnNextDraw = true;
                }

                if (GUILayout.Button("1x", EditorStyles.toolbarButton, GUILayout.Width(34f)))
                    _generatorPreviewZoom = 1f;

                if (GUILayout.Button("Фокус", EditorStyles.toolbarButton, GUILayout.Width(58f)))
                {
                    _generatorPreviewFocusActiveUnitOnNextDraw = true;
                    Repaint();
                }

                if (GUILayout.Button("Центр", EditorStyles.toolbarButton, GUILayout.Width(58f)))
                {
                    var active = GetActiveScenarioUnit();
                    if (active != null)
                    {
                        active.Position = new Vector2Int(_generatorPreviewWidth / 2, _generatorPreviewHeight / 2);
                        _generatorPreviewUnitPosition = active.Position;
                        _generatorPreviewFocusActiveUnitOnNextDraw = true;
                        MarkGeneratorVisionDirty();
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("wheel zoom | MMB pan | LMB drag", EditorStyles.miniLabel, GUILayout.Width(176f));
            }
        }

        private void DrawGeneratorPreviewControls()
        {
            BeginSection("Control center", "d_SceneViewCamera", "Згортні блоки для мапи, сценарію, видимості та туману.");

            if (BeginFoldoutSection(ref _generatorMapSettingsFoldout, "Мапа", "d_Terrain Icon", "Розмір, seed і rebuild preview texture."))
            {
                EditorGUI.BeginChangeCheck();
                _generatorPreviewWidth = EditorGUILayout.IntSlider(new GUIContent("Width", "Ширина preview-мапи у тайлах."), _generatorPreviewWidth, 8, 256);
                _generatorPreviewHeight = EditorGUILayout.IntSlider(new GUIContent("Height", "Висота preview-мапи у тайлах."), _generatorPreviewHeight, 8, 256);
                _generatorPreviewSeed = EditorGUILayout.IntField(new GUIContent("Seed", "Seed для deterministic preview."), _generatorPreviewSeed);
                _generatorPreviewAutoRefresh = EditorGUILayout.ToggleLeft(new GUIContent("Auto refresh", "Автоматично перебудовувати texture після зміни noise/height settings."), _generatorPreviewAutoRefresh);
                if (EditorGUI.EndChangeCheck())
                {
                    ClampScenarioUnitPositions();
                    MarkGeneratorPreviewDirty();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Оновити preview", GeneratorPrimaryButtonStyle(), GUILayout.Height(24f)))
                        RebuildGeneratorMapPreview();
                    if (GUILayout.Button("Новий seed", GUILayout.Width(90f), GUILayout.Height(24f)))
                    {
                        _generatorPreviewSeed = Environment.TickCount & 0x7FFFFFFF;
                        MarkGeneratorPreviewDirty();
                    }
                }

                EndFoldoutSection();
            }

            if (BeginFoldoutSection(ref _generatorScenarioFoldout, "Сценарій юнітів", "d_AvatarSelector", "Активний юніт, тип, позиція і швидке редагування параметрів."))
            {
                DrawGeneratorScenarioControls();
                EndFoldoutSection();
            }

            if (BeginFoldoutSection(ref _generatorVisionFogFoldout, "Огляд і туман", "d_scenevis_visible_hover", "Як юніт бачить рельєф і як це лягає на Fog of War."))
            {
                DrawGeneratorVisionFogControls();
                EndFoldoutSection();
            }

            if (BeginFoldoutSection(ref _generatorSummaryFoldout, "Підсумок", "d_Profiler.GlobalIllumination", "Поточний результат visibility simulation."))
            {
                DrawGeneratorPreviewSummary();
                DrawScenarioVisibilityMatrix();
                EndFoldoutSection();
            }

            EndSection();
        }

        private void DrawGeneratorScenarioControls()
        {
            EnsureGeneratorScenarioInitialized();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Сценарій юнітів", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    string[] labels = BuildScenarioUnitLabels();
                    _generatorScenarioActiveIndex = EditorGUILayout.Popup(new GUIContent("Активний", "Активний юніт: ЛКМ по мапі рухає саме його."), Mathf.Clamp(_generatorScenarioActiveIndex, 0, Mathf.Max(0, labels.Length - 1)), labels);
                    if (GUILayout.Button("+", GUILayout.Width(26f), GUILayout.Height(18f)))
                    {
                        AddScenarioUnit();
                        _generatorPreviewFocusActiveUnitOnNextDraw = true;
                        GUI.FocusControl(null);
                    }

                    using (new EditorGUI.DisabledScope(_generatorScenarioUnits.Count <= 1))
                    {
                        if (GUILayout.Button("-", GUILayout.Width(26f), GUILayout.Height(18f)))
                        {
                            RemoveActiveScenarioUnit();
                            GUI.FocusControl(null);
                        }
                    }
                }

                var active = GetActiveScenarioUnit();
                if (active != null)
                {
                    bool changed = false;
                    BuildScenarioTypeOptions(out string[] typeIds, out string[] typeLabels);
                    int typeIndex = IndexOfTypeId(typeIds, active.TypeId);
                    int nextTypeIndex = EditorGUILayout.Popup(new GUIContent("Тип юніта", "Профіль юніта для активного сценарного екземпляра."), typeIndex, typeLabels);
                    if (nextTypeIndex >= 0 && nextTypeIndex < typeIds.Length)
                    {
                        string nextTypeId = typeIds[nextTypeIndex];
                        if (!string.Equals(active.TypeId, nextTypeId, StringComparison.Ordinal))
                        {
                            active.TypeId = nextTypeId;
                            changed = true;
                        }
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Позиція", GUILayout.Width(48f));
                        int px = EditorGUILayout.IntSlider(active.Position.x, 0, Mathf.Max(0, _generatorPreviewWidth - 1));
                        int py = EditorGUILayout.IntSlider(active.Position.y, 0, Mathf.Max(0, _generatorPreviewHeight - 1));
                        if (px != active.Position.x || py != active.Position.y)
                        {
                            active.Position = new Vector2Int(px, py);
                            _generatorPreviewUnitPosition = active.Position;
                            changed = true;
                        }
                    }

                    EditorGUILayout.LabelField("ЛКМ по юніту: вибір. ЛКМ drag: переміщення. Колесо: zoom. Середня кнопка: панорамування.", EditorStyles.miniLabel);

                    var activeConfig = ResolveScenarioUnitConfig(active);
                    if (activeConfig != null)
                    {
                        EditorGUILayout.Space(3f);
                        EditorGUILayout.LabelField("Швидке редагування вибраного юніта", EditorStyles.boldLabel);

                        var vision = activeConfig.FindPropertyRelative("VisionRange");
                        var hp = activeConfig.FindPropertyRelative("HitPoints");
                        var level = activeConfig.FindPropertyRelative("BaseLevel");
                        var boost = activeConfig.FindPropertyRelative("VisionHeightBoostPerLevel");
                        var canSeeCrest = activeConfig.FindPropertyRelative("CanSeeCrest");
                        var crestFactor = activeConfig.FindPropertyRelative("CrestVisibilityFactor");
                        var downSlopeBonus = activeConfig.FindPropertyRelative("DownSlopeVisionBonus");
                        var silhouettePenalty = activeConfig.FindPropertyRelative("SilhouettePenalty");

                        EditorGUI.BeginChangeCheck();
                        if (vision != null)
                        {
                            vision.intValue = EditorGUILayout.IntSlider(new GUIContent("Огляд", "Базовий радіус огляду вибраного юніта."), Mathf.Clamp(vision.intValue, 1, 64), 1, 20);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "VisionRange", UnitDesignerPreviewFocus.Vision);
                        }
                        if (hp != null)
                        {
                            hp.intValue = EditorGUILayout.IntSlider(new GUIContent("HP", "Очки здоров'я вибраного юніта."), Mathf.Max(1, hp.intValue), 1, 300);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "HitPoints", UnitDesignerPreviewFocus.Health);
                        }
                        if (level != null)
                        {
                            level.intValue = EditorGUILayout.IntSlider(new GUIContent("Рівень", "Базовий рівень вибраного юніта."), Mathf.Max(1, level.intValue), 1, 10);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "BaseLevel", UnitDesignerPreviewFocus.Level);
                        }
                        if (boost != null)
                        {
                            boost.floatValue = EditorGUILayout.Slider(new GUIContent("Буст огляду за висоту", "Індивідуальний бустер огляду за кожен рівень висоти."), Mathf.Max(0f, boost.floatValue), 0f, 4f);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "VisionHeightBoostPerLevel", UnitDesignerPreviewFocus.TerrainVision);
                        }

                        EditorGUILayout.Space(2f);
                        EditorGUILayout.LabelField("Рельєфна видимість активного юніта", EditorStyles.miniBoldLabel);
                        if (canSeeCrest != null)
                        {
                            canSeeCrest.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Бачить crest знизу", "Дозволяє бачити верхній край схилу по реальному LOS-променю."), canSeeCrest.boolValue);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "CanSeeCrest", UnitDesignerPreviewFocus.TerrainVision);
                        }
                        if (crestFactor != null)
                        {
                            crestFactor.floatValue = EditorGUILayout.Slider(new GUIContent("Сила crest", "Скільки видимості дає верхній край при погляді знизу."), Mathf.Clamp01(crestFactor.floatValue), 0f, 1f);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "CrestVisibilityFactor", UnitDesignerPreviewFocus.TerrainVision);
                        }
                        if (downSlopeBonus != null)
                        {
                            downSlopeBonus.floatValue = EditorGUILayout.Slider(new GUIContent("Бонус вниз", "Додатковий радіус, коли спостерігач дивиться вниз зі схилу."), Mathf.Max(0f, downSlopeBonus.floatValue), 0f, 6f);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "DownSlopeVisionBonus", UnitDesignerPreviewFocus.TerrainVision);
                        }
                        if (silhouettePenalty != null)
                        {
                            silhouettePenalty.floatValue = EditorGUILayout.Slider(new GUIContent("Силует", "Наскільки легко побачити цього юніта на верхньому краю."), Mathf.Clamp01(silhouettePenalty.floatValue), 0f, 1f);
                            TrackParameterFocus(GUILayoutUtility.GetLastRect(), "SilhouettePenalty", UnitDesignerPreviewFocus.TerrainVision);
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            changed = true;
                            MarkGeneratorVisionDirty();
                        }

                        DrawScenarioQuickBars(
                            vision != null ? Mathf.Clamp(vision.intValue, 1, 64) : 1,
                            hp != null ? Mathf.Max(1, hp.intValue) : 1,
                            level != null ? Mathf.Max(1, level.intValue) : 1,
                            boost != null ? Mathf.Max(0f, boost.floatValue) : 0f);
                        DrawScenarioTerrainQuickBars(
                            canSeeCrest == null || canSeeCrest.boolValue,
                            crestFactor != null ? Mathf.Clamp01(crestFactor.floatValue) : 0f,
                            downSlopeBonus != null ? Mathf.Max(0f, downSlopeBonus.floatValue) : 0f,
                            silhouettePenalty != null ? Mathf.Clamp01(silhouettePenalty.floatValue) : 0f);
                        DrawFocusedParameterDocCard(activeConfig);
                    }

                    if (changed)
                        MarkGeneratorVisionDirty();
                }
            }
        }

        private static void DrawScenarioQuickBars(int vision, int hp, int level, float boost)
        {
            DrawScenarioQuickBar("Vision", Mathf.InverseLerp(1f, 20f, vision), new Color(0.15f, 0.72f, 0.86f), vision.ToString(CultureInfo.InvariantCulture));
            DrawScenarioQuickBar("HP", Mathf.InverseLerp(1f, 300f, hp), new Color(0.2f, 0.72f, 0.45f), hp.ToString(CultureInfo.InvariantCulture));
            DrawScenarioQuickBar("Level", Mathf.InverseLerp(1f, 10f, level), new Color(0.2f, 0.62f, 0.67f), level.ToString(CultureInfo.InvariantCulture));
            DrawScenarioQuickBar("Boost", Mathf.InverseLerp(0f, 4f, boost), new Color(0.42f, 0.72f, 1f), boost.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private static void DrawScenarioTerrainQuickBars(bool canSeeCrest, float crestFactor, float downSlopeBonus, float silhouettePenalty)
        {
            DrawScenarioQuickBar("Crest", crestFactor, canSeeCrest ? VisionOutline : new Color(0.45f, 0.45f, 0.45f), canSeeCrest ? crestFactor.ToString("0.00", CultureInfo.InvariantCulture) : "off");
            DrawScenarioQuickBar("Down", Mathf.InverseLerp(0f, 6f, downSlopeBonus), new Color(0.42f, 0.72f, 1f), downSlopeBonus.ToString("0.0", CultureInfo.InvariantCulture));
            DrawScenarioQuickBar("Silh", silhouettePenalty, Warn, silhouettePenalty.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private void DrawGeneratorVisionFogControls()
        {
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Fog source", GUILayout.Width(82f));
                bool activeOnly = GUILayout.Toggle(!_generatorPreviewFogUnion, "Активний", EditorStyles.miniButtonLeft, GUILayout.Height(22f));
                bool allUnits = GUILayout.Toggle(_generatorPreviewFogUnion, "Усі юніти", EditorStyles.miniButtonRight, GUILayout.Height(22f));
                if (activeOnly)
                    _generatorPreviewFogUnion = false;
                if (allUnits)
                    _generatorPreviewFogUnion = true;
            }

            _generatorPreviewShowFog = EditorGUILayout.ToggleLeft(new GUIContent("Показувати Fog of War", "Затемнює усе поза видимістю."), _generatorPreviewShowFog);
            _generatorFogHiddenOpacity = EditorGUILayout.Slider(new GUIContent("Сила туману", "Наскільки сильно затемнюються невидимі тайли."), Mathf.Clamp01(_generatorFogHiddenOpacity), 0f, 1f);
            _generatorFogVisibleTint = EditorGUILayout.Slider(new GUIContent("Підсвітка видимого", "Наскільки помітно підфарбовувати тайли, які юніт бачить."), Mathf.Clamp01(_generatorFogVisibleTint), 0f, 1f);

            EditorGUILayout.Space(2f);
            _generatorVisionUseTerrainLos = EditorGUILayout.ToggleLeft(new GUIContent("Розумний LOS по рельєфу", "Видимість враховує схили, вершини, краї й перекриття рельєфом."), _generatorVisionUseTerrainLos);
            _generatorVisionGlobalBoost = EditorGUILayout.Slider(new GUIContent("Глобальний бустер висоти", "Додає огляд за рівень висоти активного юніта."), _generatorVisionGlobalBoost, 0f, 4f);
            _generatorVisionOcclusionTolerance = EditorGUILayout.Slider(new GUIContent("Поріг перекриття", "Вищий поріг робить рельєф менш агресивним блокером видимості."), _generatorVisionOcclusionTolerance, 0f, 0.2f);
            _generatorVisionDownhillPenalty = EditorGUILayout.Slider(new GUIContent("Сила сліпої зони", "Множник того, наскільки агресивно край ховає тайли одразу за обривом."), _generatorVisionDownhillPenalty, 0f, 1f);
            _generatorVisionUphillPeekStrength = EditorGUILayout.Slider(new GUIContent("Погляд угору на край", "Полегшує видимість верхнього краю/виступу знизу."), _generatorVisionUphillPeekStrength, 0f, 1f);
            _generatorVisionUphillEdgeDrop = EditorGUILayout.Slider(new GUIContent("Мін. перепад краю", "Наскільки різким має бути край, щоб спрацювала edge-aware логіка."), _generatorVisionUphillEdgeDrop, 0.005f, 0.2f);
            _generatorVisionEdgePeekDistanceTiles = EditorGUILayout.IntSlider(new GUIContent("Позиція на краю", "Скільки тайлів від обриву ще вважаються достатньо близькими, щоб бачити вниз без blind zone."), _generatorVisionEdgePeekDistanceTiles, 0, 6);
            _generatorVisionEdgeBlindZoneTiles = EditorGUILayout.IntSlider(new GUIContent("Сліпа зона за краєм", "Скільки нижніх тайлів одразу за краєм не видно, якщо юніт стоїть не біля краю."), _generatorVisionEdgeBlindZoneTiles, 0, 8);
            _generatorVisionEdgeMaxBlindZoneTiles = EditorGUILayout.IntSlider(new GUIContent("Макс. сліпа зона", "Верхня межа blind zone, коли юніт стоїть далеко від краю."), Mathf.Max(_generatorVisionEdgeBlindZoneTiles, _generatorVisionEdgeMaxBlindZoneTiles), _generatorVisionEdgeBlindZoneTiles, 10);
            _generatorVisionEdgeDistanceScale = EditorGUILayout.Slider(new GUIContent("Вплив відстані до краю", "Наскільки blind zone росте, якщо юніт стоїть глибше на плато."), _generatorVisionEdgeDistanceScale, 0f, 1.5f);

            var activeScenario = GetActiveScenarioUnit();
            var activeScenarioUnitConfig = ResolveScenarioUnitConfig(activeScenario);
            if (activeScenarioUnitConfig != null)
            {
                var unitVisionBoost = activeScenarioUnitConfig.FindPropertyRelative("VisionHeightBoostPerLevel");
                if (unitVisionBoost != null)
                {
                    float nextUnitBoost = EditorGUILayout.Slider(new GUIContent("Індивідуальний бустер юніта", "Додається тільки для профілю активного юніта."), Mathf.Max(0f, unitVisionBoost.floatValue), 0f, 4f);
                    if (!Mathf.Approximately(nextUnitBoost, unitVisionBoost.floatValue))
                        unitVisionBoost.floatValue = nextUnitBoost;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                PersistGeneratorVisionPrefs();
                MarkGeneratorVisionDirty();
            }

            DrawGeneratorVisionPresetButtons();
        }

        private void DrawGeneratorVisionPresetButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Presets", GUILayout.Width(58f));
                if (GUILayout.Button("Radius", GUILayout.Height(22f)))
                {
                    _generatorVisionUseTerrainLos = false;
                    _generatorVisionGlobalBoost = 0f;
                    _generatorVisionEdgePeekDistanceTiles = 0;
                    _generatorVisionEdgeBlindZoneTiles = 0;
                    _generatorVisionEdgeMaxBlindZoneTiles = 0;
                    _generatorVisionEdgeDistanceScale = 0f;
                    _generatorFogHiddenOpacity = 0.62f;
                    _generatorFogVisibleTint = 0.18f;
                    PersistGeneratorVisionPrefs();
                    MarkGeneratorVisionDirty();
                }

                if (GUILayout.Button("Tactical", GUILayout.Height(22f)))
                {
                    _generatorVisionUseTerrainLos = true;
                    _generatorVisionGlobalBoost = 0.35f;
                    _generatorVisionOcclusionTolerance = 0.035f;
                    _generatorVisionDownhillPenalty = 0.45f;
                    _generatorVisionUphillPeekStrength = 0.6f;
                    _generatorVisionUphillEdgeDrop = 0.05f;
                    _generatorVisionEdgePeekDistanceTiles = 1;
                    _generatorVisionEdgeBlindZoneTiles = 2;
                    _generatorVisionEdgeMaxBlindZoneTiles = 4;
                    _generatorVisionEdgeDistanceScale = 0.35f;
                    _generatorFogHiddenOpacity = 0.68f;
                    _generatorFogVisibleTint = 0.24f;
                    PersistGeneratorVisionPrefs();
                    MarkGeneratorVisionDirty();
                }

                if (GUILayout.Button("High ground", GUILayout.Height(22f)))
                {
                    _generatorVisionUseTerrainLos = true;
                    _generatorVisionGlobalBoost = 0.75f;
                    _generatorVisionOcclusionTolerance = 0.055f;
                    _generatorVisionDownhillPenalty = 0.32f;
                    _generatorVisionUphillPeekStrength = 0.82f;
                    _generatorVisionUphillEdgeDrop = 0.035f;
                    _generatorVisionEdgePeekDistanceTiles = 1;
                    _generatorVisionEdgeBlindZoneTiles = 1;
                    _generatorVisionEdgeMaxBlindZoneTiles = 3;
                    _generatorVisionEdgeDistanceScale = 0.25f;
                    _generatorFogHiddenOpacity = 0.7f;
                    _generatorFogVisibleTint = 0.3f;
                    PersistGeneratorVisionPrefs();
                    MarkGeneratorVisionDirty();
                }
            }
        }

        private void DrawGeneratorPreviewSummary()
        {
            var activeScenarioUnit = GetActiveScenarioUnit();
            int baseVision = ResolveSelectedUnitVisionRange(activeScenarioUnit);
            int heightLevel = ResolveSelectedUnitHeightLevel(activeScenarioUnit);
            float totalBoostPerLevel = ResolveSelectedVisionBoostPerLevel(activeScenarioUnit);
            int vision = ResolveSelectedEffectiveVisionRange(activeScenarioUnit);
            int maxVision = ResolveSelectedMaximumVisionRange(activeScenarioUnit);
            float visiblePercent = _generatorPreviewTotalCells > 0
                ? (float)_generatorPreviewVisibleCells / _generatorPreviewTotalCells * 100f
                : 0f;

            DrawGeneratorSummaryRow("База", baseVision.ToString(CultureInfo.InvariantCulture), "Рівень", (heightLevel + 1).ToString(CultureInfo.InvariantCulture), "Еф./макс", maxVision > vision ? $"{vision}/{maxVision}" : vision.ToString(CultureInfo.InvariantCulture));
            DrawGeneratorSummaryRow("Fog", _generatorPreviewFogUnion ? "усі" : "активний", "Visible", $"{visiblePercent:0.0}%", "LOS", _generatorVisionUseTerrainLos ? "terrain" : "radius");
            EditorGUILayout.LabelField($"Level source: {_generatorPreviewLevelSource} | Boost/level {totalBoostPerLevel:0.00} | edge peek {_generatorVisionEdgePeekDistanceTiles}, blind {_generatorVisionEdgeBlindZoneTiles}-{_generatorVisionEdgeMaxBlindZoneTiles} | noise min {_generatorPreviewMinHeight:0.000}, avg {_generatorPreviewAverageHeight:0.000}, max {_generatorPreviewMaxHeight:0.000}", EditorStyles.wordWrappedMiniLabel);
        }

        private static void DrawGeneratorSummaryRow(string labelA, string valueA, string labelB, string valueB, string labelC, string valueC)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawGeneratorSummaryPill(labelA, valueA);
                DrawGeneratorSummaryPill(labelB, valueB);
                DrawGeneratorSummaryPill(labelC, valueC);
            }
        }

        private static void DrawGeneratorSummaryPill(string label, string value)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.13f, 0.17f, 0.18f) : new Color(0.82f, 0.88f, 0.89f));
            GUI.Label(new Rect(rect.x + 6f, rect.y + 3f, rect.width - 12f, 11f), label, EditorStyles.centeredGreyMiniLabel);
            GUI.Label(new Rect(rect.x + 6f, rect.y + 13f, rect.width - 12f, 13f), value, EditorStyles.miniBoldLabel);
        }

        private void PersistGeneratorVisionPrefs()
        {
            EditorPrefs.SetFloat(GeneratorVisionGlobalBoostPrefsKey, _generatorVisionGlobalBoost);
            EditorPrefs.SetBool(GeneratorVisionTerrainLosPrefsKey, _generatorVisionUseTerrainLos);
            EditorPrefs.SetFloat(GeneratorVisionOcclusionTolerancePrefsKey, _generatorVisionOcclusionTolerance);
            EditorPrefs.SetFloat(GeneratorVisionDownhillPenaltyPrefsKey, _generatorVisionDownhillPenalty);
            EditorPrefs.SetFloat(GeneratorVisionUphillPeekStrengthPrefsKey, _generatorVisionUphillPeekStrength);
            EditorPrefs.SetFloat(GeneratorVisionUphillEdgeDropPrefsKey, _generatorVisionUphillEdgeDrop);
            EditorPrefs.SetInt(GeneratorVisionEdgePeekDistancePrefsKey, _generatorVisionEdgePeekDistanceTiles);
            EditorPrefs.SetInt(GeneratorVisionEdgeBlindZonePrefsKey, _generatorVisionEdgeBlindZoneTiles);
            EditorPrefs.SetInt(GeneratorVisionEdgeMaxBlindZonePrefsKey, _generatorVisionEdgeMaxBlindZoneTiles);
            EditorPrefs.SetFloat(GeneratorVisionEdgeDistanceScalePrefsKey, _generatorVisionEdgeDistanceScale);
            EditorPrefs.SetBool(GeneratorPreviewFogUnionPrefsKey, _generatorPreviewFogUnion);
            EditorPrefs.SetFloat(GeneratorFogHiddenOpacityPrefsKey, _generatorFogHiddenOpacity);
            EditorPrefs.SetFloat(GeneratorFogVisibleTintPrefsKey, _generatorFogVisibleTint);
        }

        private static void DrawScenarioQuickBar(string label, float value01, Color color, string valueText)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 14f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y, 46f, rect.height), label, EditorStyles.miniLabel);
            Rect bar = new Rect(rect.x + 48f, rect.y + 3f, rect.width - 98f, 7f);
            EditorGUI.DrawRect(bar, new Color(0f, 0f, 0f, 0.26f));
            EditorGUI.DrawRect(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(value01), bar.height), color);
            GUI.Label(new Rect(bar.xMax + 6f, rect.y - 1f, 40f, rect.height), valueText, EditorStyles.miniLabel);
        }

        private void DrawScenarioVisibilityMatrix()
        {
            if (_generatorScenarioUnits.Count <= 1)
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Взаємна видимість", EditorStyles.boldLabel);
                for (int i = 0; i < _generatorScenarioUnits.Count; i++)
                {
                    for (int j = i + 1; j < _generatorScenarioUnits.Count; j++)
                    {
                        bool iSeesJ = CanScenarioUnitSeeScenarioUnit(i, j);
                        bool jSeesI = CanScenarioUnitSeeScenarioUnit(j, i);
                        string left = BuildScenarioUnitLabel(i);
                        string right = BuildScenarioUnitLabel(j);
                        string relation = iSeesJ && jSeesI
                            ? "бачать один одного"
                            : iSeesJ
                                ? "бачить тільки лівий"
                                : jSeesI
                                    ? "бачить тільки правий"
                                    : "не бачать";
                        EditorGUILayout.LabelField($"{left} <-> {right}: {relation}", EditorStyles.miniLabel);
                    }
                }
            }
        }

        private void DrawGeneratorPreviewTexture()
        {
            float viewportHeight = Mathf.Clamp(position.height * 0.5f, 300f, 620f);
            Rect viewportRect = GUILayoutUtility.GetRect(320f, viewportHeight, GUILayout.ExpandWidth(true));
            _lastGeneratorPreviewRect = viewportRect;
            DrawPanelBackground(viewportRect, EditorGUIUtility.isProSkin ? new Color(0.08f, 0.09f, 0.1f) : new Color(0.78f, 0.82f, 0.84f));

            if (_generatorPreviewTexture == null)
            {
                GUI.Label(new Rect(viewportRect.x + 12f, viewportRect.center.y - 18f, viewportRect.width - 24f, 36f), _generatorPreviewStatus, CenterMiniStyle());
                return;
            }

            float cellSize = GetGeneratorPreviewCellSize();
            Vector2 mapPixelSize = GetGeneratorPreviewMapPixelSize(cellSize);
            float contentWidth = Mathf.Max(viewportRect.width, mapPixelSize.x);
            float contentHeight = Mathf.Max(viewportRect.height, mapPixelSize.y);

            if (_generatorPreviewFocusActiveUnitOnNextDraw)
            {
                _generatorMapCanvasScroll = CenterGeneratorScrollOnActiveUnit(viewportRect, contentWidth, contentHeight, cellSize);
                _generatorPreviewFocusActiveUnitOnNextDraw = false;
            }
            else
            {
                _generatorMapCanvasScroll = ClampGeneratorScroll(_generatorMapCanvasScroll, viewportRect, contentWidth, contentHeight);
            }

            Rect contentDrawRect = new Rect(0f, 0f, mapPixelSize.x, mapPixelSize.y);
            bool geometryChanged = HandleGeneratorPreviewInput(viewportRect, contentDrawRect, contentWidth, contentHeight, cellSize);
            if (geometryChanged)
            {
                cellSize = GetGeneratorPreviewCellSize();
                mapPixelSize = GetGeneratorPreviewMapPixelSize(cellSize);
                contentWidth = Mathf.Max(viewportRect.width, mapPixelSize.x);
                contentHeight = Mathf.Max(viewportRect.height, mapPixelSize.y);
                _generatorMapCanvasScroll = ClampGeneratorScroll(_generatorMapCanvasScroll, viewportRect, contentWidth, contentHeight);
                contentDrawRect = new Rect(0f, 0f, mapPixelSize.x, mapPixelSize.y);
            }

            Rect drawRect = new Rect(-_generatorMapCanvasScroll.x, -_generatorMapCanvasScroll.y, mapPixelSize.x, mapPixelSize.y);
            GUI.BeginGroup(viewportRect);
            GUI.DrawTexture(drawRect, _generatorPreviewTexture, ScaleMode.StretchToFill, false);
            DrawGeneratorPreviewOverlay(drawRect, viewportRect.size);
            GUI.EndGroup();

            DrawGeneratorPreviewViewportHud(viewportRect, cellSize);
            DrawGeneratorPreviewHover(viewportRect, contentDrawRect);
        }

        private void DrawGeneratorPreviewOverlay(Rect drawRect, Vector2 viewportSize)
        {
            float cellW = drawRect.width / Mathf.Max(1, _generatorPreviewWidth);
            float cellH = drawRect.height / Mathf.Max(1, _generatorPreviewHeight);

            DrawGeneratorVisibilityCellsOverlay(drawRect, viewportSize, cellW, cellH);
            DrawGeneratorPreviewGridOverlay(drawRect, viewportSize, cellW, cellH);

            Handles.BeginGUI();
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                var unit = _generatorScenarioUnits[i];
                Color unitColor = ResolveScenarioUnitColor(i);
                Vector2 center = PreviewCellCenter(drawRect, unit.Position, cellW, cellH);
                float radiusPx = ResolveSelectedEffectiveVisionRange(unit) * Mathf.Min(cellW, cellH);

                Handles.color = new Color(unitColor.r, unitColor.g, unitColor.b, i == _generatorScenarioActiveIndex ? 0.12f : 0.06f);
                Handles.DrawSolidDisc(center, Vector3.forward, radiusPx);
                Handles.color = new Color(unitColor.r, unitColor.g, unitColor.b, i == _generatorScenarioActiveIndex ? 0.92f : 0.58f);
                Handles.DrawWireDisc(center, Vector3.forward, radiusPx);
            }

            DrawGeneratorScenarioVisibilityLinks(drawRect, cellW, cellH);

            Handles.EndGUI();

            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
                DrawGeneratorScenarioUnitMarker(i, drawRect, cellW, cellH);
        }

        private void DrawGeneratorScenarioVisibilityLinks(Rect drawRect, float cellW, float cellH)
        {
            if (_generatorScenarioUnits.Count <= 1)
                return;

            int activeIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1);
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                for (int j = i + 1; j < _generatorScenarioUnits.Count; j++)
                {
                    bool involvesActive = i == activeIndex || j == activeIndex;
                    bool iSeesJ = CanScenarioUnitSeeScenarioUnit(i, j);
                    bool jSeesI = CanScenarioUnitSeeScenarioUnit(j, i);
                    Vector2 from = PreviewCellCenter(drawRect, _generatorScenarioUnits[i].Position, cellW, cellH);
                    Vector2 to = PreviewCellCenter(drawRect, _generatorScenarioUnits[j].Position, cellW, cellH);

                    Color linkColor;
                    string label;
                    if (iSeesJ && jSeesI)
                    {
                        linkColor = new Color(0.28f, 0.92f, 0.58f, involvesActive ? 0.86f : 0.42f);
                        label = "mutual";
                    }
                    else if (iSeesJ || jSeesI)
                    {
                        linkColor = new Color(1f, 0.72f, 0.24f, involvesActive ? 0.82f : 0.34f);
                        label = iSeesJ ? $"{i + 1}->{j + 1}" : $"{j + 1}->{i + 1}";
                    }
                    else
                    {
                        linkColor = new Color(0.9f, 0.9f, 0.9f, involvesActive ? 0.22f : 0.08f);
                        label = "blocked";
                    }

                    Handles.color = linkColor;
                    Handles.DrawAAPolyLine(involvesActive ? 3f : 1.4f, from, to);

                    if (!involvesActive)
                        continue;

                    Vector2 mid = Vector2.Lerp(from, to, 0.5f);
                    Vector2 dir = (to - from).normalized;
                    if (dir.sqrMagnitude < 0.001f)
                        dir = Vector2.right;
                    Vector2 normal = new Vector2(-dir.y, dir.x);
                    Rect labelRect = new Rect(mid.x + normal.x * 9f - 34f, mid.y + normal.y * 9f - 9f, 68f, 18f);
                    EditorGUI.DrawRect(labelRect, new Color(0f, 0f, 0f, 0.68f));
                    GUI.Label(labelRect, label, EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private bool HandleGeneratorPreviewInput(Rect viewportRect, Rect drawRect, float contentWidth, float contentHeight, float cellSize)
        {
            var evt = Event.current;
            if (evt == null)
                return false;

            if (evt.type == EventType.MouseMove || evt.type == EventType.Repaint)
                _generatorPreviewMouse = evt.mousePosition;

            if (evt.type == EventType.ScrollWheel && viewportRect.Contains(evt.mousePosition))
            {
                Vector2 localMouse = evt.mousePosition - viewportRect.position;
                Vector2 contentAnchor = localMouse + _generatorMapCanvasScroll;

                float factor = evt.delta.y > 0f ? 0.9f : 1.1f;
                float oldCellSize = Mathf.Max(0.001f, cellSize);
                _generatorPreviewZoom = Mathf.Clamp(_generatorPreviewZoom * factor, 0.25f, 8f);

                float updatedCellSize = GetGeneratorPreviewCellSize();
                Vector2 updatedMapPixelSize = GetGeneratorPreviewMapPixelSize(updatedCellSize);
                float updatedContentWidth = Mathf.Max(viewportRect.width, updatedMapPixelSize.x);
                float updatedContentHeight = Mathf.Max(viewportRect.height, updatedMapPixelSize.y);
                float scale = updatedCellSize / oldCellSize;
                _generatorMapCanvasScroll = ClampGeneratorScroll(contentAnchor * scale - localMouse, viewportRect, updatedContentWidth, updatedContentHeight);

                evt.Use();
                Repaint();
                return true;
            }

            if (evt.type == EventType.MouseDown && evt.button == 2 && viewportRect.Contains(evt.mousePosition))
            {
                _generatorPreviewIsPanning = true;
                _generatorPreviewPanLastMouse = evt.mousePosition;
                evt.Use();
                return false;
            }

            if (evt.type == EventType.MouseDrag && _generatorPreviewIsPanning)
            {
                Vector2 delta = evt.mousePosition - _generatorPreviewPanLastMouse;
                _generatorMapCanvasScroll = ClampGeneratorScroll(_generatorMapCanvasScroll - delta, viewportRect, contentWidth, contentHeight);
                _generatorPreviewPanLastMouse = evt.mousePosition;
                evt.Use();
                Repaint();
                return false;
            }

            if (evt.type == EventType.MouseUp && evt.button == 2 && _generatorPreviewIsPanning)
            {
                _generatorPreviewIsPanning = false;
                evt.Use();
                return false;
            }

            if (evt.type == EventType.MouseUp && evt.button == 0 && _generatorPreviewIsDraggingUnit)
            {
                _generatorPreviewIsDraggingUnit = false;
                _generatorPreviewDraggingUnitIndex = -1;
                evt.Use();
                return false;
            }

            if ((evt.type != EventType.MouseDown && evt.type != EventType.MouseDrag) || evt.button != 0)
                return false;

            if (!viewportRect.Contains(evt.mousePosition))
                return false;

            Vector2 contentMouse = ViewportToContentPoint(viewportRect, evt.mousePosition);
            if (!drawRect.Contains(contentMouse))
                return false;

            Vector2Int cell = ScreenToPreviewCell(drawRect, contentMouse);

            if (evt.type == EventType.MouseDown)
            {
                if (TryGetScenarioUnitIndexAtCell(cell, out int clickedIndex))
                {
                    if (_generatorScenarioActiveIndex != clickedIndex)
                    {
                        _generatorScenarioActiveIndex = clickedIndex;
                        _generatorPreviewUnitPosition = _generatorScenarioUnits[clickedIndex].Position;
                        MarkGeneratorVisionDirty();
                    }

                    _generatorPreviewIsDraggingUnit = true;
                    _generatorPreviewDraggingUnitIndex = clickedIndex;
                }
                else
                {
                    _generatorPreviewIsDraggingUnit = true;
                    _generatorPreviewDraggingUnitIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, Mathf.Max(0, _generatorScenarioUnits.Count - 1));
                }

                evt.Use();
                return false;
            }

            if (evt.type == EventType.MouseDrag && _generatorPreviewIsDraggingUnit)
            {
                int targetIndex = Mathf.Clamp(_generatorPreviewDraggingUnitIndex, 0, Mathf.Max(0, _generatorScenarioUnits.Count - 1));
                if (targetIndex < _generatorScenarioUnits.Count)
                {
                    var moving = _generatorScenarioUnits[targetIndex];
                    if (moving.Position != cell)
                    {
                        moving.Position = cell;
                        _generatorPreviewUnitPosition = cell;
                        _generatorScenarioActiveIndex = targetIndex;
                        MarkGeneratorVisionDirty();
                    }
                }

                evt.Use();
            }

            return false;
        }

        private float GetGeneratorPreviewCellSize()
        {
            return Mathf.Clamp(8f * _generatorPreviewZoom, 2f, 64f);
        }

        private Vector2 GetGeneratorPreviewMapPixelSize(float cellSize)
        {
            return new Vector2(
                Mathf.Max(1, _generatorPreviewWidth) * cellSize,
                Mathf.Max(1, _generatorPreviewHeight) * cellSize);
        }

        private float CalculateGeneratorFitZoom(Rect viewportRect)
        {
            if (viewportRect.width <= 1f || viewportRect.height <= 1f)
                return Mathf.Clamp(_generatorPreviewZoom, 0.25f, 8f);

            float fitCellWidth = (viewportRect.width - 18f) / Mathf.Max(1, _generatorPreviewWidth);
            float fitCellHeight = (viewportRect.height - 18f) / Mathf.Max(1, _generatorPreviewHeight);
            float fitCellSize = Mathf.Max(2f, Mathf.Min(fitCellWidth, fitCellHeight));
            return Mathf.Clamp(fitCellSize / 8f, 0.25f, 8f);
        }

        private Vector2 CenterGeneratorScrollOnActiveUnit(Rect viewportRect, float contentWidth, float contentHeight, float cellSize)
        {
            var active = GetActiveScenarioUnit();
            if (active == null)
                return ClampGeneratorScroll(_generatorMapCanvasScroll, viewportRect, contentWidth, contentHeight);

            float centerX = (active.Position.x + 0.5f) * cellSize;
            float centerY = (_generatorPreviewHeight - active.Position.y - 0.5f) * cellSize;
            Vector2 target = new Vector2(centerX - viewportRect.width * 0.5f, centerY - viewportRect.height * 0.5f);
            return ClampGeneratorScroll(target, viewportRect, contentWidth, contentHeight);
        }

        private void DrawGeneratorVisibilityCellsOverlay(Rect drawRect, Vector2 viewportSize, float cellW, float cellH)
        {
            if (_generatorVisibilityOverlayTexture == null)
                return;

            GUI.DrawTexture(drawRect, _generatorVisibilityOverlayTexture, ScaleMode.StretchToFill, true);
        }

        private void DrawGeneratorPreviewGridOverlay(Rect drawRect, Vector2 viewportSize, float cellW, float cellH)
        {
            if (cellW < 6f || cellH < 6f)
                return;

            int startX = Mathf.Clamp(Mathf.FloorToInt((-drawRect.x) / Mathf.Max(1f, cellW)) - 1, 0, Mathf.Max(0, _generatorPreviewWidth));
            int endX = Mathf.Clamp(Mathf.CeilToInt((viewportSize.x - drawRect.x) / Mathf.Max(1f, cellW)) + 1, 0, Mathf.Max(0, _generatorPreviewWidth));
            int startY = Mathf.Clamp(Mathf.FloorToInt((-drawRect.y) / Mathf.Max(1f, cellH)) - 1, 0, Mathf.Max(0, _generatorPreviewHeight));
            int endY = Mathf.Clamp(Mathf.CeilToInt((viewportSize.y - drawRect.y) / Mathf.Max(1f, cellH)) + 1, 0, Mathf.Max(0, _generatorPreviewHeight));

            Handles.BeginGUI();
            Handles.color = new Color(GridLine.r, GridLine.g, GridLine.b, 0.32f);
            for (int x = startX; x <= endX; x++)
            {
                float px = drawRect.x + x * cellW;
                Handles.DrawLine(new Vector3(px, Mathf.Max(0f, drawRect.y)), new Vector3(px, Mathf.Min(viewportSize.y, drawRect.yMax)));
            }

            for (int y = startY; y <= endY; y++)
            {
                float py = drawRect.y + y * cellH;
                Handles.DrawLine(new Vector3(Mathf.Max(0f, drawRect.x), py), new Vector3(Mathf.Min(viewportSize.x, drawRect.xMax), py));
            }

            Handles.EndGUI();
        }

        private static Vector2 PreviewCellCenter(Rect drawRect, Vector2Int cell, float cellW, float cellH)
        {
            return new Vector2(
                drawRect.x + (cell.x + 0.5f) * cellW,
                drawRect.yMax - (cell.y + 0.5f) * cellH);
        }

        private void DrawGeneratorScenarioUnitMarker(int index, Rect drawRect, float cellW, float cellH)
        {
            if (index < 0 || index >= _generatorScenarioUnits.Count)
                return;

            var unit = _generatorScenarioUnits[index];
            bool active = index == _generatorScenarioActiveIndex;
            Color unitColor = ResolveScenarioUnitColor(index);
            Vector2 center = PreviewCellCenter(drawRect, unit.Position, cellW, cellH);
            float markerSize = Mathf.Clamp(Mathf.Min(cellW, cellH) * 1.8f, active ? 24f : 18f, active ? 52f : 40f);

            Handles.BeginGUI();
            Handles.color = new Color(0f, 0f, 0f, 0.5f);
            Handles.DrawSolidDisc(center + new Vector2(1.5f, 1.5f), Vector3.forward, markerSize * 0.58f);
            Handles.color = new Color(unitColor.r, unitColor.g, unitColor.b, active ? 0.95f : 0.78f);
            Handles.DrawSolidDisc(center, Vector3.forward, markerSize * 0.54f);
            Handles.color = active ? Color.white : new Color(1f, 1f, 1f, 0.62f);
            Handles.DrawWireDisc(center, Vector3.forward, markerSize * 0.59f);
            Handles.EndGUI();

            SerializedProperty config = ResolveScenarioUnitConfig(unit);
            GameObject prefab = GetObject<GameObject>(config, "Prefab");
            Sprite customSprite = GetObject<Sprite>(config, "CustomSprite");
            Sprite sprite = customSprite != null ? customSprite : ResolveSprite(prefab);
            Rect markerRect = new Rect(center.x - markerSize * 0.42f, center.y - markerSize * 0.42f, markerSize * 0.84f, markerSize * 0.84f);
            DrawSpriteOrPrefab(markerRect, sprite, prefab, false);

            string indexText = (index + 1).ToString(CultureInfo.InvariantCulture);
            var numberStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            Rect numberRect = new Rect(center.x - markerSize * 0.56f, center.y - markerSize * 0.64f, markerSize * 0.42f, markerSize * 0.32f);
            EditorGUI.DrawRect(numberRect, new Color(0f, 0f, 0f, 0.68f));
            GUI.Label(numberRect, indexText, numberStyle);

            if (!active)
                return;

            string typeLabel = string.IsNullOrWhiteSpace(unit.TypeId) ? "<TypeId?>" : unit.TypeId;
            int effectiveVision = ResolveSelectedEffectiveVisionRange(unit);
            int maximumVision = ResolveSelectedMaximumVisionRange(unit);
            string visionLabel = maximumVision > effectiveVision
                ? $"{typeLabel} | огляд {effectiveVision}/{maximumVision}"
                : $"{typeLabel} | огляд {effectiveVision}";
            var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                normal = { textColor = Color.white },
                padding = new RectOffset(5, 5, 2, 2)
            };
            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(visionLabel));
            Rect labelRect = new Rect(center.x + markerSize * 0.58f, center.y - labelSize.y * 0.5f, labelSize.x + 10f, labelSize.y + 4f);
            EditorGUI.DrawRect(labelRect, new Color(0f, 0f, 0f, 0.78f));
            GUI.Label(labelRect, visionLabel, labelStyle);
        }

        private void DrawGeneratorPreviewViewportHud(Rect viewportRect, float cellSize)
        {
            string status = $"{_generatorPreviewStatus} | zoom {_generatorPreviewZoom:0.00}x | cell {cellSize:0.#}px";
            var statusStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                normal = { textColor = Color.white },
                padding = new RectOffset(6, 6, 3, 3)
            };
            Vector2 statusSize = statusStyle.CalcSize(new GUIContent(status));
            Rect statusRect = new Rect(viewportRect.x + 8f, viewportRect.y + 8f, Mathf.Min(viewportRect.width - 16f, statusSize.x + 12f), statusSize.y + 6f);
            EditorGUI.DrawRect(statusRect, new Color(0f, 0f, 0f, 0.72f));
            GUI.Label(statusRect, status, statusStyle);

            string hint = "Wheel zoom | MMB pan | LMB drag unit";
            var hintStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.86f) },
                alignment = TextAnchor.MiddleRight,
                padding = new RectOffset(6, 6, 3, 3)
            };
            Vector2 hintSize = hintStyle.CalcSize(new GUIContent(hint));
            Rect hintRect = new Rect(viewportRect.xMax - hintSize.x - 20f, viewportRect.yMax - hintSize.y - 14f, hintSize.x + 12f, hintSize.y + 6f);
            EditorGUI.DrawRect(hintRect, new Color(0f, 0f, 0f, 0.54f));
            GUI.Label(hintRect, hint, hintStyle);
        }

        private bool TryGetScenarioUnitIndexAtCell(Vector2Int cell, out int index)
        {
            for (int i = _generatorScenarioUnits.Count - 1; i >= 0; i--)
            {
                if (_generatorScenarioUnits[i].Position == cell)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private void DrawGeneratorPreviewHover(Rect viewportRect, Rect drawRect)
        {
            if (_generatorPreviewTexture == null || !viewportRect.Contains(_generatorPreviewMouse))
                return;

            Vector2 contentMouse = ViewportToContentPoint(viewportRect, _generatorPreviewMouse);
            if (!drawRect.Contains(contentMouse))
                return;

            Vector2Int cell = ScreenToPreviewCell(drawRect, contentMouse);
            if (_generatorPreviewNoiseMap == null)
                return;

            float h = _generatorPreviewNoiseMap[cell.x, cell.y];
            string tile = _generatorPreviewTileMap != null ? _generatorPreviewTileMap[cell.x, cell.y] : string.Empty;
            int level = _generatorPreviewLevelMap != null ? _generatorPreviewLevelMap[cell.x, cell.y] : -1;
            bool visible = IsCachedScenarioVisible(cell.x, cell.y);
            int viewers = GetCachedScenarioViewers(cell.x, cell.y);
            var active = GetActiveScenarioUnit();
            bool activeSees = IsInsideSelectedVision(active, cell.x, cell.y);
            int activeRange = ResolveSelectedEffectiveVisionRange(active, cell.x, cell.y);
            string terrainHint = ResolveGeneratorTerrainHoverHint(active, cell.x, cell.y);
            string source = _generatorPreviewUsesHillNodeLevels ? "hill" : "height";
            string text = $"[{cell.x}, {cell.y}] h={h:0.000} level={level + 1} ({source}) tile={tile} {(visible ? "visible" : "fog")}, viewers={viewers}/{Mathf.Max(1, _generatorScenarioUnits.Count)} | active {(activeSees ? "sees" : "blocked")} r={activeRange} {terrainHint}";

            var style = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white },
                fontSize = 11,
                padding = new RectOffset(6, 6, 3, 3),
                wordWrap = true
            };

            var content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);
            size.x = Mathf.Min(Mathf.Max(220f, size.x), Mathf.Max(220f, viewportRect.width - 16f));
            size.y = style.CalcHeight(content, size.x);
            float x = Mathf.Min(_generatorPreviewMouse.x + 14f, viewportRect.xMax - size.x - 8f);
            float y = Mathf.Clamp(_generatorPreviewMouse.y - size.y - 6f, viewportRect.y + 8f, viewportRect.yMax - size.y - 8f);
            Rect bg = new Rect(x - 2f, y - 2f, size.x + 4f, size.y + 4f);
            EditorGUI.DrawRect(bg, new Color(0f, 0f, 0f, 0.78f));
            GUI.Label(new Rect(x, y, size.x, size.y), content, style);
        }

        private string ResolveGeneratorTerrainHoverHint(PreviewScenarioUnit observer, int targetX, int targetY)
        {
            if (!_generatorVisionUseTerrainLos || observer == null || _generatorPreviewNoiseMap == null)
                return "radius";

            int observerX = Mathf.Clamp(observer.Position.x, 0, _generatorPreviewWidth - 1);
            int observerY = Mathf.Clamp(observer.Position.y, 0, _generatorPreviewHeight - 1);
            if (observerX == targetX && observerY == targetY)
                return "self";

            float observerHeight = SampleNoiseHeight(observerX, observerY);
            float targetHeight = SampleNoiseHeight(targetX, targetY);
            float threshold = GetGeneratorEdgeHeightThreshold();
            int dx = targetX - observerX;
            int dy = targetY - observerY;
            int gridDistance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));

            if (observerHeight - targetHeight >= threshold
                && TryFindGeneratorDownhillEdge(observerX, observerY, targetX, targetY, out int downhillEdgeStep, out int distanceToEdge))
            {
                if (IsGeneratorTargetHiddenByDownhillEdge(gridDistance, downhillEdgeStep, distanceToEdge))
                    return "blind zone";

                float bonus = ResolveDirectionalDownSlopeVisionBonus(observer, targetX, targetY);
                return bonus > 0.01f ? $"down +{bonus:0.#}" : "down slope";
            }

            if (targetHeight - observerHeight >= threshold)
            {
                float crest = ResolveGeneratorUphillEdgePeekFactor(observerX, observerY, targetX, targetY, targetHeight);
                if (crest > 0f)
                    return ResolveScenarioCanSeeCrest(observer) ? $"crest {crest:0.##}" : "crest hidden";
            }

            return HasTerrainLineOfSight(observer, targetX, targetY) ? "LOS" : "terrain block";
        }

        private Vector2 ViewportToContentPoint(Rect viewportRect, Vector2 mousePosition)
        {
            return mousePosition - viewportRect.position + _generatorMapCanvasScroll;
        }

        private static Vector2 ClampGeneratorScroll(Vector2 scroll, Rect viewportRect, float contentWidth, float contentHeight)
        {
            float maxX = Mathf.Max(0f, contentWidth - viewportRect.width);
            float maxY = Mathf.Max(0f, contentHeight - viewportRect.height);
            return new Vector2(Mathf.Clamp(scroll.x, 0f, maxX), Mathf.Clamp(scroll.y, 0f, maxY));
        }

        private void DrawGeneratorPreviewLegend()
        {
            BeginSection("Легенда", "d_FilterByLabel", "Поточні рівні висоти та їхні кольори у preview.");
            var levels = _generatorPreviewUsesHillNodeLevels
                ? BuildHillGeneratorPreviewLevels()
                : BuildPreviewLevels();
            if (levels.Count == 0)
            {
                EditorGUILayout.HelpBox(_generatorPreviewUsesHillNodeLevels
                    ? "HillGeneratorNode не має доступних рівнів для легенди."
                    : "Немає HeightLayers для легенди.", MessageType.Info);
                EndSection();
                return;
            }

            _generatorLegendScroll = EditorGUILayout.BeginScrollView(_generatorLegendScroll, GUILayout.MaxHeight(150f));
            for (int i = 0; i < levels.Count; i++)
                DrawLevelLegendRow(levels[i], i);
            EditorGUILayout.EndScrollView();
            EndSection();
        }

        private bool DrawHeightLayerEditor(SerializedProperty layer, int index, int total)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = GeneratorLevelPalette[index % GeneratorLevelPalette.Length] * 1.2f;
            EditorGUILayout.BeginVertical(SectionStyle());
            GUI.backgroundColor = old;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Рівень {index + 1}", SectionHeaderStyle());
                if (GUILayout.Button("X", GUILayout.Width(26f), GUILayout.Height(20f)))
                {
                    EditorGUILayout.EndVertical();
                    return true;
                }
            }

            var tile = layer.FindPropertyRelative("TileID");
            var min = layer.FindPropertyRelative("MinHeight");
            var max = layer.FindPropertyRelative("MaxHeight");
            var chance = layer.FindPropertyRelative("TileIDChance");
            var variants = layer.FindPropertyRelative("WeightedVariants");

            if (tile != null && string.IsNullOrWhiteSpace(tile.stringValue))
                tile.stringValue = ResolveFirstExistingTileId();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(tile, new GUIContent("Tile ID", "Базовий tile id для цього рівня."));

            float minValue = min != null ? Mathf.Clamp01(min.floatValue) : 0f;
            float maxValue = max != null ? Mathf.Clamp01(max.floatValue) : 1f;
            EditorGUILayout.MinMaxSlider(new GUIContent("Noise Range", "Діапазон значення шуму 0..1, який належить цьому рівню."), ref minValue, ref maxValue, 0f, 1f);
            using (new EditorGUILayout.HorizontalScope())
            {
                minValue = EditorGUILayout.FloatField("Min", minValue);
                maxValue = EditorGUILayout.FloatField("Max", maxValue);
            }

            if (min != null)
                min.floatValue = Mathf.Clamp01(Mathf.Min(minValue, maxValue));
            if (max != null)
                max.floatValue = Mathf.Clamp01(Mathf.Max(minValue, maxValue));

            if (chance != null)
                chance.floatValue = EditorGUILayout.Slider(new GUIContent("Base Chance", "Шанс використати базовий tile перед weighted variants."), Mathf.Clamp01(chance.floatValue), 0f, 1f);

            if (variants != null)
                EditorGUILayout.PropertyField(variants, new GUIContent("Weighted Variants", "Варіанти тайлів всередині цього рівня."), true);

            if (EditorGUI.EndChangeCheck())
                ApplyHeightLevelChanges();

            Rect bar = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
            DrawHeightLayerRangeBar(bar, min?.floatValue ?? 0f, max?.floatValue ?? 1f, index);

            EditorGUILayout.EndVertical();
            return false;
        }

        private void DrawHeightLayerRangeBar(Rect rect, float min, float max, int index)
        {
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.18f));
            float x = rect.x + rect.width * Mathf.Clamp01(min);
            float w = rect.width * Mathf.Clamp01(max - min);
            EditorGUI.DrawRect(new Rect(x, rect.y, Mathf.Max(2f, w), rect.height), GeneratorLevelPalette[index % GeneratorLevelPalette.Length]);
        }

        private void DrawHeightLevelValidation(SerializedProperty layers)
        {
            if (layers.arraySize == 0)
            {
                EditorGUILayout.HelpBox("HeightLayers порожній. Додай хоча б один рівень.", MessageType.Warning);
                return;
            }

            float coverage = 0f;
            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                float min = Mathf.Clamp01(layer.FindPropertyRelative("MinHeight")?.floatValue ?? 0f);
                float max = Mathf.Clamp01(layer.FindPropertyRelative("MaxHeight")?.floatValue ?? 0f);
                coverage += Mathf.Max(0f, max - min);
            }

            if (coverage < 0.98f)
                EditorGUILayout.HelpBox($"Сумарне покриття діапазонів приблизно {coverage:0.00}. Можливі ділянки шуму без явного рівня.", MessageType.Warning);
            else if (coverage > 1.08f)
                EditorGUILayout.HelpBox($"Сумарне покриття діапазонів {coverage:0.00}. Є шанс перекриття рівнів.", MessageType.Info);
        }

        private void ApplyHeightLevelChanges()
        {
            _heightSettingsObject?.ApplyModifiedProperties();
            if (_heightSettingsAsset != null)
                EditorUtility.SetDirty(_heightSettingsAsset);
            MarkGeneratorPreviewDirty();
        }

        private void NormalizeHeightLayers(SerializedProperty layers)
        {
            if (layers == null || layers.arraySize == 0)
                return;

            float step = 1f / layers.arraySize;
            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                var min = layer.FindPropertyRelative("MinHeight");
                var max = layer.FindPropertyRelative("MaxHeight");
                if (min != null)
                    min.floatValue = i * step;
                if (max != null)
                    max.floatValue = i == layers.arraySize - 1 ? 1f : (i + 1) * step;
            }
        }

        private void InitializeHeightLayer(SerializedProperty layer, int index, int total)
        {
            float step = total <= 0 ? 1f : 1f / total;
            SetRelativeString(layer, "TileID", index switch
            {
                0 => "water",
                1 => "lowland",
                2 => "ground",
                3 => "hill",
                _ => "mountain"
            });
            SetRelativeFloat(layer, "TileIDChance", 1f);
            SetRelativeFloat(layer, "MinHeight", index * step);
            SetRelativeFloat(layer, "MaxHeight", index == total - 1 ? 1f : (index + 1) * step);
        }

        private void MaybeRebuildGeneratorPreview()
        {
            int runtimeSignature = ComputeGeneratorPreviewRuntimeSignature();
            if (_generatorPreviewRuntimeSignature != runtimeSignature)
            {
                _generatorPreviewRuntimeSignature = runtimeSignature;
                MarkGeneratorPreviewDirty();
            }

            int visionRuntimeSignature = ComputeGeneratorVisionPreviewRuntimeSignature();
            if (_generatorVisionPreviewRuntimeSignature != visionRuntimeSignature)
            {
                _generatorVisionPreviewRuntimeSignature = visionRuntimeSignature;
                MarkGeneratorVisionDirty();
            }

            if (!_generatorPreviewDirty || !_generatorPreviewAutoRefresh)
            {
                if (!_generatorVisionPreviewDirty || !_generatorPreviewAutoRefresh)
                    return;

                double visionNow = EditorApplication.timeSinceStartup;
                if (visionNow - _generatorVisionPreviewDirtySince < GeneratorVisionPreviewRebuildDebounceSeconds)
                    return;

                if (!_livePreviewThrottle.ShouldRunCostlyTick())
                    return;

                RebuildGeneratorVisibilityPreview();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now - _generatorPreviewDirtySince < GeneratorPreviewRebuildDebounceSeconds)
                return;

            // Heavy map rebuild is throttled to keep editor UI responsive during rapid input.
            if (!_livePreviewThrottle.ShouldRunCostlyTick())
                return;

            RebuildGeneratorMapPreview();
        }

        private void MarkGeneratorPreviewDirty()
        {
            _generatorPreviewDirty = true;
            _generatorPreviewDirtySince = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void MarkGeneratorVisionDirty()
        {
            _generatorVisionPreviewDirty = true;
            _generatorVisionPreviewDirtySince = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void RebuildGeneratorMapPreview()
        {
            _generatorPreviewDirty = false;
            _generatorPreviewWidth = Mathf.Clamp(_generatorPreviewWidth, 8, 256);
            _generatorPreviewHeight = Mathf.Clamp(_generatorPreviewHeight, 8, 256);
            EnsureGeneratorScenarioInitialized();
            ClampScenarioUnitPositions();

            bool canUseHillNode = _hillGeneratorNodeAsset != null;
            if (_noiseSettingsObject == null || (!canUseHillNode && _heightSettingsObject == null))
            {
                _generatorPreviewStatus = canUseHillNode
                    ? "Потрібні Noise Settings для Hill Generator preview."
                    : "Потрібні Noise Settings і Height Settings.";
                return;
            }

            var levels = BuildPreviewLevels();
            if (!canUseHillNode && levels.Count == 0)
            {
                _generatorPreviewStatus = "HeightLayers порожній.";
                return;
            }

            _generatorPreviewNoiseMap = GeneratePreviewNoiseMap(_noiseSettingsObject, _generatorPreviewWidth, _generatorPreviewHeight, _generatorPreviewSeed);
            _generatorPreviewTileMap = new string[_generatorPreviewWidth, _generatorPreviewHeight];
            _generatorPreviewLevelMap = new int[_generatorPreviewWidth, _generatorPreviewHeight];
            EnsureGeneratorPreviewTexture(_generatorPreviewWidth, _generatorPreviewHeight);

            string fallbackTileId = ResolveFirstExistingTileId();
            string hillBuildMessage = string.Empty;
            bool usedHillNodeLevels = TryBuildHillGeneratorPreview(
                _generatorPreviewNoiseMap,
                levels,
                fallbackTileId,
                out var hillTileMap,
                out var hillLevelMap,
                out hillBuildMessage);

            _generatorPreviewUsesHillNodeLevels = usedHillNodeLevels;
            _generatorPreviewLevelSource = usedHillNodeLevels
                ? $"Hill Generator ({_hillGeneratorNodeAsset.name})"
                : "HeightLayers fallback";

            int totalCells = _generatorPreviewWidth * _generatorPreviewHeight;
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            float heightSum = 0f;
            var pixels = new Color32[totalCells];
            for (int y = 0; y < _generatorPreviewHeight; y++)
            {
                for (int x = 0; x < _generatorPreviewWidth; x++)
                {
                    float heightValue = _generatorPreviewNoiseMap[x, y];
                    minHeight = Mathf.Min(minHeight, heightValue);
                    maxHeight = Mathf.Max(maxHeight, heightValue);
                    heightSum += heightValue;
                    int levelIndex;
                    string tileId;
                    if (usedHillNodeLevels)
                    {
                        levelIndex = Mathf.Max(0, hillLevelMap[x, y]);
                        tileId = !string.IsNullOrWhiteSpace(hillTileMap[x, y])
                            ? hillTileMap[x, y]
                            : fallbackTileId;
                    }
                    else
                    {
                        levelIndex = ResolvePreviewLevel(levels, heightValue);
                        var level = levels[Mathf.Clamp(levelIndex, 0, levels.Count - 1)];
                        tileId = SelectPreviewTile(level, x, y, _generatorPreviewSeed, fallbackTileId);
                    }

                    _generatorPreviewTileMap[x, y] = tileId;
                    _generatorPreviewLevelMap[x, y] = levelIndex;
                    Color color = ResolvePreviewColor(levelIndex, heightValue);
                    pixels[y * _generatorPreviewWidth + x] = color;
                }
            }

            _generatorPreviewTexture.SetPixels32(pixels);
            _generatorPreviewTexture.Apply(false, false);
            _generatorPreviewTotalCells = Mathf.Max(1, totalCells);
            _generatorPreviewMinHeight = minHeight < float.MaxValue ? minHeight : 0f;
            _generatorPreviewMaxHeight = maxHeight > float.MinValue ? maxHeight : 0f;
            _generatorPreviewAverageHeight = totalCells > 0 ? heightSum / totalCells : 0f;
            RebuildGeneratorVisibilityPreview();
            string sourceSuffix = usedHillNodeLevels
                ? "levels HillGeneratorNode"
                : string.IsNullOrWhiteSpace(hillBuildMessage)
                    ? "levels HeightLayers"
                    : $"levels HeightLayers ({hillBuildMessage})";
            _generatorPreviewStatus = $"{_generatorPreviewWidth}x{_generatorPreviewHeight} | seed {_generatorPreviewSeed} | {sourceSuffix} | scenario {_generatorScenarioUnits.Count} | visible {_generatorPreviewVisibleCells}";
        }

        private void RebuildGeneratorVisibilityPreview()
        {
            _generatorVisionPreviewDirty = false;

            if (_generatorPreviewNoiseMap == null || _generatorPreviewLevelMap == null)
            {
                _generatorPreviewVisibleCells = 0;
                _generatorPreviewTotalCells = Mathf.Max(1, _generatorPreviewWidth * _generatorPreviewHeight);
                _generatorPreviewVisibleMap = null;
                _generatorPreviewViewerCountMap = null;
                EnsureGeneratorVisibilityOverlayTexture(_generatorPreviewWidth, _generatorPreviewHeight);
                ClearGeneratorVisibilityOverlayTexture();
                return;
            }

            int width = Mathf.Max(1, _generatorPreviewWidth);
            int height = Mathf.Max(1, _generatorPreviewHeight);
            int totalCells = width * height;
            _generatorPreviewVisibleMap = new bool[width, height];
            _generatorPreviewViewerCountMap = new byte[width, height];
            EnsureGeneratorVisibilityOverlayTexture(width, height);

            var overlayPixels = new Color32[totalCells];
            byte hiddenAlpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(_generatorFogHiddenOpacity) * 255f);
            byte visibleAlpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(_generatorFogVisibleTint) * 255f);
            byte visibleOnlyAlpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(_generatorFogVisibleTint) * 150f);
            Color32 fogColor = new Color32(0, 0, 0, hiddenAlpha);
            Color32 visibleColor = _generatorPreviewFogUnion
                ? new Color32(77, 230, 174, visibleAlpha)
                : new Color32(26, 184, 219, visibleAlpha);
            Color32 visibleOnlyColor = _generatorPreviewFogUnion
                ? new Color32(77, 230, 174, visibleOnlyAlpha)
                : new Color32(26, 184, 219, visibleOnlyAlpha);
            Color32 clear = new Color32(0, 0, 0, 0);
            int visibleCells = 0;

            int activeIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, Mathf.Max(0, _generatorScenarioUnits.Count - 1));
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                var observer = _generatorScenarioUnits[i];
                if (observer == null)
                    continue;

                int radius = ResolveSelectedMaximumVisionRange(observer);
                int minX = Mathf.Max(0, observer.Position.x - radius - 1);
                int maxX = Mathf.Min(width - 1, observer.Position.x + radius + 1);
                int minY = Mathf.Max(0, observer.Position.y - radius - 1);
                int maxY = Mathf.Min(height - 1, observer.Position.y + radius + 1);
                bool contributesToVisible = _generatorPreviewFogUnion || i == activeIndex;

                for (int y = minY; y <= maxY; y++)
                {
                    for (int x = minX; x <= maxX; x++)
                    {
                        if (!IsInsideSelectedVision(observer, x, y))
                            continue;

                        if (_generatorPreviewViewerCountMap[x, y] < byte.MaxValue)
                            _generatorPreviewViewerCountMap[x, y]++;

                        if (contributesToVisible)
                            _generatorPreviewVisibleMap[x, y] = true;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool visible = _generatorPreviewVisibleMap[x, y];
                    if (visible)
                        visibleCells++;

                    overlayPixels[y * width + x] = _generatorPreviewShowFog
                        ? (visible ? visibleColor : fogColor)
                        : (visible ? visibleOnlyColor : clear);
                }
            }

            _generatorVisibilityOverlayTexture.SetPixels32(overlayPixels);
            _generatorVisibilityOverlayTexture.Apply(false, false);
            _generatorPreviewVisibleCells = visibleCells;
            _generatorPreviewTotalCells = Mathf.Max(1, totalCells);
            _generatorPreviewStatus = $"{width}x{height} | seed {_generatorPreviewSeed} | scenario {_generatorScenarioUnits.Count} | visible {visibleCells}";
        }

        private int ComputeGeneratorPreviewRuntimeSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _generatorPreviewWidth;
                hash = hash * 31 + _generatorPreviewHeight;
                hash = hash * 31 + _generatorPreviewSeed;
                hash = hash * 31 + ComputeNoiseSettingsSignature();
                hash = hash * 31 + ComputeHeightSettingsSignature();
                hash = hash * 31 + ComputeSerializedObjectSignature(_hillGeneratorNodeAsset);
                return hash;
            }
        }

        private int ComputeGeneratorVisionPreviewRuntimeSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _generatorPreviewWidth;
                hash = hash * 31 + _generatorPreviewHeight;
                hash = hash * 31 + (_generatorPreviewShowFog ? 1 : 0);
                hash = hash * 31 + (_generatorPreviewFogUnion ? 1 : 0);
                hash = hash * 31 + (_generatorVisionUseTerrainLos ? 1 : 0);
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionGlobalBoost * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionOcclusionTolerance * 10000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionDownhillPenalty * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionUphillPeekStrength * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionUphillEdgeDrop * 10000f);
                hash = hash * 31 + _generatorVisionEdgePeekDistanceTiles;
                hash = hash * 31 + _generatorVisionEdgeBlindZoneTiles;
                hash = hash * 31 + _generatorVisionEdgeMaxBlindZoneTiles;
                hash = hash * 31 + Mathf.RoundToInt(_generatorVisionEdgeDistanceScale * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorFogHiddenOpacity * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(_generatorFogVisibleTint * 1000f);
                hash = hash * 31 + ComputeScenarioSignature();
                return hash;
            }
        }

        private int ComputeNoiseSettingsSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Mathf.RoundToInt(GetSerializedFloat(_noiseSettingsObject, "Scale", 20f) * 1000f);
                hash = hash * 31 + GetSerializedInt(_noiseSettingsObject, "Octaves", 4);
                hash = hash * 31 + Mathf.RoundToInt(GetSerializedFloat(_noiseSettingsObject, "Persistance", 0.5f) * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(GetSerializedFloat(_noiseSettingsObject, "Lacunarity", 2f) * 1000f);
                Vector2 offset = GetSerializedVector2(_noiseSettingsObject, "Offset", Vector2.zero);
                hash = hash * 31 + Mathf.RoundToInt(offset.x * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(offset.y * 1000f);
                return hash;
            }
        }

        private int ComputeHeightSettingsSignature()
        {
            var layers = _heightSettingsObject?.FindProperty("HeightLayers");
            if (layers == null || !layers.isArray)
                return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + layers.arraySize;
                for (int i = 0; i < layers.arraySize; i++)
                {
                    var layer = layers.GetArrayElementAtIndex(i);
                    string tileId = layer.FindPropertyRelative("TileID")?.stringValue ?? string.Empty;
                    float min = layer.FindPropertyRelative("MinHeight")?.floatValue ?? 0f;
                    float max = layer.FindPropertyRelative("MaxHeight")?.floatValue ?? 0f;
                    float chance = layer.FindPropertyRelative("TileIDChance")?.floatValue ?? 0f;
                    hash = hash * 31 + tileId.GetHashCode();
                    hash = hash * 31 + Mathf.RoundToInt(min * 10000f);
                    hash = hash * 31 + Mathf.RoundToInt(max * 10000f);
                    hash = hash * 31 + Mathf.RoundToInt(chance * 10000f);

                    var variants = layer.FindPropertyRelative("WeightedVariants");
                    int variantsCount = variants != null && variants.isArray ? variants.arraySize : 0;
                    hash = hash * 31 + variantsCount;
                    for (int v = 0; v < variantsCount; v++)
                    {
                        var variant = variants.GetArrayElementAtIndex(v);
                        string variantTileId = variant.FindPropertyRelative("TileID")?.stringValue ?? string.Empty;
                        float variantChance = variant.FindPropertyRelative("Chance")?.floatValue ?? 0f;
                        hash = hash * 31 + variantTileId.GetHashCode();
                        hash = hash * 31 + Mathf.RoundToInt(variantChance * 10000f);
                    }
                }

                return hash;
            }
        }

        private int ComputeScenarioSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _generatorScenarioUnits.Count;
                hash = hash * 31 + _generatorScenarioActiveIndex;
                for (int i = 0; i < _generatorScenarioUnits.Count; i++)
                {
                    var scenario = _generatorScenarioUnits[i];
                    hash = hash * 31 + (scenario.TypeId ?? string.Empty).GetHashCode();
                    hash = hash * 31 + scenario.Position.x;
                    hash = hash * 31 + scenario.Position.y;

                    var config = ResolveScenarioUnitConfig(scenario);
                    hash = hash * 31 + Mathf.Clamp(GetInt(config, "VisionRange"), 1, 64);
                    var boost = config?.FindPropertyRelative("VisionHeightBoostPerLevel");
                    hash = hash * 31 + Mathf.RoundToInt(Mathf.Max(0f, boost != null ? boost.floatValue : 0f) * 1000f);
                    hash = hash * 31 + (GetBool(config, "CanSeeCrest", true) ? 1 : 0);
                    hash = hash * 31 + Mathf.RoundToInt(Mathf.Clamp01(GetFloat(config, "CrestVisibilityFactor")) * 1000f);
                    hash = hash * 31 + Mathf.RoundToInt(Mathf.Max(0f, GetFloat(config, "DownSlopeVisionBonus")) * 1000f);
                    hash = hash * 31 + Mathf.RoundToInt(Mathf.Clamp01(GetFloat(config, "SilhouettePenalty")) * 1000f);
                }

                return hash;
            }
        }

        private static int ComputeSerializedObjectSignature(UnityEngine.Object source)
        {
            if (source == null)
                return 0;

            unchecked
            {
                int hash = 17;
                hash = hash * 31 + source.GetInstanceID();

                try
                {
                    var serialized = new SerializedObject(source);
                    var property = serialized.GetIterator();
                    bool enterChildren = true;
                    while (property.NextVisible(enterChildren))
                    {
                        enterChildren = true;
                        hash = hash * 31 + property.propertyPath.GetHashCode();
                        hash = hash * 31 + (int)property.propertyType;

                        switch (property.propertyType)
                        {
                            case SerializedPropertyType.Integer:
                            case SerializedPropertyType.Enum:
                                hash = hash * 31 + property.intValue;
                                break;
                            case SerializedPropertyType.Boolean:
                                hash = hash * 31 + (property.boolValue ? 1 : 0);
                                break;
                            case SerializedPropertyType.Float:
                                hash = hash * 31 + Mathf.RoundToInt(property.floatValue * 10000f);
                                break;
                            case SerializedPropertyType.String:
                                hash = hash * 31 + (property.stringValue ?? string.Empty).GetHashCode();
                                break;
                            case SerializedPropertyType.ObjectReference:
                                hash = hash * 31 + (property.objectReferenceValue != null ? property.objectReferenceValue.GetInstanceID() : 0);
                                break;
                        }
                    }
                }
                catch
                {
                    hash = hash * 31 + source.name.GetHashCode();
                }

                return hash;
            }
        }

        private float[,] GeneratePreviewNoiseMap(SerializedObject noiseObject, int width, int height, int seed)
        {
            float scale = Mathf.Max(0.0001f, GetSerializedFloat(noiseObject, "Scale", 20f));
            int octaves = Mathf.Clamp(GetSerializedInt(noiseObject, "Octaves", 4), 1, 12);
            float persistance = Mathf.Clamp(GetSerializedFloat(noiseObject, "Persistance", 0.5f), 0.01f, 1f);
            float lacunarity = Mathf.Max(1f, GetSerializedFloat(noiseObject, "Lacunarity", 2f));
            Vector2 offset = GetSerializedVector2(noiseObject, "Offset", Vector2.zero);

            float[,] noiseMap = new float[width, height];
            var random = new System.Random(seed);
            var octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + offset.x;
                float offsetY = random.Next(-100000, 100000) + offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float minNoise = float.MaxValue;
            float maxNoise = float.MinValue;
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int octave = 0; octave < octaves; octave++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[octave].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[octave].y;
                        float perlin = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                        noiseHeight += perlin * amplitude;
                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    minNoise = Mathf.Min(minNoise, noiseHeight);
                    maxNoise = Mathf.Max(maxNoise, noiseHeight);
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    noiseMap[x, y] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y]);
            }

            return noiseMap;
        }

        private bool TryBuildHillGeneratorPreview(
            float[,] heightMap,
            IReadOnlyList<PreviewHeightLevel> fallbackLevels,
            string fallbackTileId,
            out string[,] tileMap,
            out int[,] levelMap,
            out string message)
        {
            tileMap = null;
            levelMap = null;
            message = string.Empty;

            if (_hillGeneratorNodeAsset == null)
                return false;

            if (heightMap == null)
            {
                message = "HillGeneratorNode без HeightMap";
                return false;
            }

            try
            {
                string[,] sourceTileMap = BuildHillSourceTileMap(heightMap, fallbackLevels, fallbackTileId);
                var executeMethod = _hillGeneratorNodeAsset.GetType().GetMethod("Execute");
                if (executeMethod == null)
                {
                    message = "HillGeneratorNode.Execute не знайдено";
                    return false;
                }

                object[] inputs = { heightMap, sourceTileMap, null, null, null };
                object output = executeMethod.Invoke(_hillGeneratorNodeAsset, new object[] { inputs, null });
                if (!TryGetNodeOutputValues(output, out var values, out string status, out string nodeMessage))
                {
                    message = string.IsNullOrWhiteSpace(nodeMessage) ? "HillGeneratorNode не повернув outputs" : nodeMessage;
                    return false;
                }

                if (string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase))
                {
                    message = string.IsNullOrWhiteSpace(nodeMessage) ? "HillGeneratorNode error" : nodeMessage;
                    return false;
                }

                var outputTileMap = values.Length > 0 ? values[0] as string[,] : null;
                var outputLevelMap = values.Length > 1 ? values[1] as int[,] : null;
                if (!HasSamePreviewSize(outputLevelMap, _generatorPreviewWidth, _generatorPreviewHeight))
                {
                    message = "HillGeneratorNode LevelMap має інший розмір";
                    return false;
                }

                tileMap = HasSamePreviewSize(outputTileMap, _generatorPreviewWidth, _generatorPreviewHeight)
                    ? outputTileMap
                    : sourceTileMap;
                levelMap = outputLevelMap;
                message = string.Equals(status, "Warning", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(nodeMessage)
                    ? nodeMessage
                    : string.Empty;
                return true;
            }
            catch (Exception e)
            {
                message = e.InnerException != null ? e.InnerException.Message : e.Message;
                return false;
            }
        }

        private string[,] BuildHillSourceTileMap(float[,] heightMap, IReadOnlyList<PreviewHeightLevel> fallbackLevels, string fallbackTileId)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            var result = new string[width, height];
            bool hasFallbackLevels = fallbackLevels != null && fallbackLevels.Count > 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (hasFallbackLevels)
                    {
                        int levelIndex = ResolvePreviewLevel(fallbackLevels, heightMap[x, y]);
                        var level = fallbackLevels[Mathf.Clamp(levelIndex, 0, fallbackLevels.Count - 1)];
                        result[x, y] = SelectPreviewTile(level, x, y, _generatorPreviewSeed, fallbackTileId);
                    }
                    else
                    {
                        result[x, y] = fallbackTileId;
                    }
                }
            }

            return result;
        }

        private static bool TryGetNodeOutputValues(object output, out object[] values, out string status, out string message)
        {
            values = null;
            status = string.Empty;
            message = string.Empty;

            if (output == null)
                return false;

            var outputType = output.GetType();
            values = outputType.GetProperty("Values")?.GetValue(output) as object[];
            object statusValue = outputType.GetProperty("Status")?.GetValue(output);
            status = statusValue != null ? statusValue.ToString() : string.Empty;
            message = outputType.GetProperty("Message")?.GetValue(output) as string ?? string.Empty;
            return values != null;
        }

        private static bool HasSamePreviewSize(Array map, int width, int height)
        {
            return map != null
                && map.Rank == 2
                && map.GetLength(0) == width
                && map.GetLength(1) == height;
        }

        private List<PreviewHeightLevel> BuildPreviewLevels()
        {
            var result = new List<PreviewHeightLevel>();
            if (_heightSettingsObject == null)
                return result;

            var layers = _heightSettingsObject.FindProperty("HeightLayers");
            if (layers == null || !layers.isArray)
                return result;

            for (int i = 0; i < layers.arraySize; i++)
            {
                var layer = layers.GetArrayElementAtIndex(i);
                result.Add(new PreviewHeightLevel
                {
                    TileId = layer.FindPropertyRelative("TileID")?.stringValue ?? string.Empty,
                    TileChance = Mathf.Clamp01(layer.FindPropertyRelative("TileIDChance")?.floatValue ?? 1f),
                    MinHeight = Mathf.Clamp01(layer.FindPropertyRelative("MinHeight")?.floatValue ?? 0f),
                    MaxHeight = Mathf.Clamp01(layer.FindPropertyRelative("MaxHeight")?.floatValue ?? 1f),
                    Variants = ReadWeightedVariants(layer),
                });
            }

            return result;
        }

        private List<PreviewHeightLevel> BuildHillGeneratorPreviewLevels()
        {
            var result = new List<PreviewHeightLevel>();
            if (_hillGeneratorNodeAsset == null)
                return result;

            try
            {
                var serializedNode = new SerializedObject(_hillGeneratorNodeAsset);
                int levelCount = Mathf.Max(1, serializedNode.FindProperty("_levels")?.intValue ?? 1);
                bool useCustomThresholds = serializedNode.FindProperty("_useCustomThresholds")?.boolValue ?? false;
                var thresholds = serializedNode.FindProperty("_levelThresholds");
                float previous = 0f;

                for (int i = 0; i < levelCount; i++)
                {
                    float max = 1f;
                    if (i < levelCount - 1)
                    {
                        max = useCustomThresholds && thresholds != null && thresholds.isArray && i < thresholds.arraySize
                            ? Mathf.Clamp01(thresholds.GetArrayElementAtIndex(i).floatValue)
                            : (float)(i + 1) / levelCount;
                    }

                    if (max < previous)
                        max = previous;

                    result.Add(new PreviewHeightLevel
                    {
                        TileId = $"Hill level {i + 1}",
                        TileChance = 1f,
                        MinHeight = previous,
                        MaxHeight = max,
                        Variants = Array.Empty<PreviewWeightedTile>(),
                    });

                    previous = max;
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        private PreviewWeightedTile[] ReadWeightedVariants(SerializedProperty layer)
        {
            var variants = layer.FindPropertyRelative("WeightedVariants");
            if (variants == null || !variants.isArray || variants.arraySize == 0)
                return Array.Empty<PreviewWeightedTile>();

            var result = new PreviewWeightedTile[variants.arraySize];
            for (int i = 0; i < variants.arraySize; i++)
            {
                var variant = variants.GetArrayElementAtIndex(i);
                result[i] = new PreviewWeightedTile
                {
                    TileId = variant.FindPropertyRelative("TileID")?.stringValue ?? string.Empty,
                    Chance = Mathf.Clamp01(variant.FindPropertyRelative("Chance")?.floatValue ?? 0f),
                };
            }

            return result;
        }

        private int ResolvePreviewLevel(IReadOnlyList<PreviewHeightLevel> levels, float heightValue)
        {
            int fallback = Mathf.Max(0, levels.Count - 1);
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                fallback = i;
                if (heightValue >= level.MinHeight && heightValue <= level.MaxHeight)
                    return i;
            }

            return fallback;
        }

        private string SelectPreviewTile(PreviewHeightLevel level, int x, int y, int seed, string fallbackTileId)
        {
            if (level.Variants == null || level.Variants.Length == 0)
                return ResolveTileIdOrFallback(level.TileId, fallbackTileId);

            float roll = (PositiveHash(seed, x, y) % 100000) / 100000f;
            float cumulative = 0f;
            if (!string.IsNullOrEmpty(level.TileId))
            {
                cumulative += level.TileChance;
                if (roll < cumulative)
                    return ResolveTileIdOrFallback(level.TileId, fallbackTileId);
            }

            for (int i = 0; i < level.Variants.Length; i++)
            {
                var variant = level.Variants[i];
                if (string.IsNullOrEmpty(variant.TileId))
                    continue;

                cumulative += variant.Chance;
                if (roll < cumulative)
                    return variant.TileId;
            }

            return ResolveTileIdOrFallback(level.TileId, fallbackTileId);
        }

        private static string ResolveTileIdOrFallback(string tileId, string fallbackTileId)
        {
            if (!string.IsNullOrWhiteSpace(tileId))
                return tileId.Trim();

            return !string.IsNullOrWhiteSpace(fallbackTileId) ? fallbackTileId : "ground";
        }

        private static string ResolveFirstExistingTileId()
        {
            var tileRegistryType = ResolveUnityType("Kruty1918.Moyva.Grid.API.TileRegistrySO");
            if (tileRegistryType == null)
                return "ground";

            string[] guids = AssetDatabase.FindAssets($"t:{tileRegistryType.Name}");
            if (guids == null || guids.Length == 0)
                return "ground";

            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var registry = AssetDatabase.LoadAssetAtPath(path, tileRegistryType) as ScriptableObject;
                if (registry == null)
                    continue;

                SerializedObject so;
                try
                {
                    so = new SerializedObject(registry);
                }
                catch
                {
                    continue;
                }

                var definitions = so.FindProperty("_definitions");
                if (definitions == null || !definitions.isArray)
                    continue;

                for (int j = 0; j < definitions.arraySize; j++)
                {
                    var definition = definitions.GetArrayElementAtIndex(j);
                    var idProp = definition?.FindPropertyRelative("_id");
                    string id = idProp != null ? idProp.stringValue : string.Empty;
                    if (!string.IsNullOrWhiteSpace(id))
                        return id.Trim();
                }
            }

            return "ground";
        }

        private void EnsureGeneratorScenarioInitialized()
        {
            if (_generatorScenarioUnits.Count > 0)
            {
                _generatorScenarioActiveIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1);
                _generatorPreviewUnitPosition = _generatorScenarioUnits[_generatorScenarioActiveIndex].Position;
                return;
            }

            _generatorScenarioUnits.Add(new PreviewScenarioUnit
            {
                TypeId = ResolveDefaultScenarioTypeId(),
                Position = new Vector2Int(Mathf.Clamp(_generatorPreviewUnitPosition.x, 0, _generatorPreviewWidth - 1), Mathf.Clamp(_generatorPreviewUnitPosition.y, 0, _generatorPreviewHeight - 1))
            });
            _generatorScenarioActiveIndex = 0;
        }

        private PreviewScenarioUnit GetActiveScenarioUnit()
        {
            if (_generatorScenarioUnits.Count == 0)
                return null;

            _generatorScenarioActiveIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1);
            return _generatorScenarioUnits[_generatorScenarioActiveIndex];
        }

        private void ClampScenarioUnitPositions()
        {
            EnsureGeneratorScenarioInitialized();
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                var unit = _generatorScenarioUnits[i];
                unit.Position = new Vector2Int(
                    Mathf.Clamp(unit.Position.x, 0, _generatorPreviewWidth - 1),
                    Mathf.Clamp(unit.Position.y, 0, _generatorPreviewHeight - 1));
            }

            var active = GetActiveScenarioUnit();
            if (active != null)
                _generatorPreviewUnitPosition = active.Position;
        }

        private void AddScenarioUnit()
        {
            Vector2Int start = new Vector2Int(_generatorPreviewWidth / 2, _generatorPreviewHeight / 2);
            if (_generatorScenarioUnits.Count > 0)
            {
                Vector2Int current = _generatorScenarioUnits[Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1)].Position;
                start = new Vector2Int(Mathf.Clamp(current.x + 1, 0, _generatorPreviewWidth - 1), current.y);
            }

            _generatorScenarioUnits.Add(new PreviewScenarioUnit
            {
                TypeId = ResolveDefaultScenarioTypeId(),
                Position = start
            });
            _generatorScenarioActiveIndex = _generatorScenarioUnits.Count - 1;
            _generatorPreviewUnitPosition = start;
            MarkGeneratorVisionDirty();
        }

        private void RemoveActiveScenarioUnit()
        {
            if (_generatorScenarioUnits.Count <= 1)
                return;

            _generatorScenarioUnits.RemoveAt(Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1));
            _generatorScenarioActiveIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, _generatorScenarioUnits.Count - 1);
            var active = GetActiveScenarioUnit();
            if (active != null)
                _generatorPreviewUnitPosition = active.Position;
            MarkGeneratorVisionDirty();
        }

        private string ResolveDefaultScenarioTypeId()
        {
            if (HasSelectedUnit())
            {
                string selectedTypeId = GetString(SelectedUnitProperty(), "TypeId");
                if (!string.IsNullOrWhiteSpace(selectedTypeId))
                    return selectedTypeId;
            }

            if (_configs != null)
            {
                for (int i = 0; i < _configs.arraySize; i++)
                {
                    var unit = _configs.GetArrayElementAtIndex(i);
                    string typeId = GetString(unit, "TypeId");
                    if (!string.IsNullOrWhiteSpace(typeId))
                        return typeId;
                }
            }

            return string.Empty;
        }

        private void BuildScenarioTypeOptions(out string[] typeIds, out string[] labels)
        {
            var ids = new List<string>();
            var names = new List<string>();
            if (_configs != null)
            {
                for (int i = 0; i < _configs.arraySize; i++)
                {
                    var unit = _configs.GetArrayElementAtIndex(i);
                    string typeId = GetString(unit, "TypeId");
                    if (string.IsNullOrWhiteSpace(typeId))
                        continue;

                    ids.Add(typeId);
                    names.Add(typeId);
                }
            }

            if (ids.Count == 0)
            {
                ids.Add(string.Empty);
                names.Add("<немає юнітів>");
            }

            typeIds = ids.ToArray();
            labels = names.ToArray();
        }

        private static int IndexOfTypeId(string[] typeIds, string typeId)
        {
            if (typeIds == null || typeIds.Length == 0)
                return 0;

            for (int i = 0; i < typeIds.Length; i++)
            {
                if (string.Equals(typeIds[i], typeId, StringComparison.Ordinal))
                    return i;
            }

            return 0;
        }

        private string[] BuildScenarioUnitLabels()
        {
            string[] labels = new string[_generatorScenarioUnits.Count];
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
                labels[i] = BuildScenarioUnitLabel(i);
            return labels;
        }

        private string BuildScenarioUnitLabel(int scenarioIndex)
        {
            if (scenarioIndex < 0 || scenarioIndex >= _generatorScenarioUnits.Count)
                return "<invalid>";

            var unit = _generatorScenarioUnits[scenarioIndex];
            string typeName = string.IsNullOrWhiteSpace(unit.TypeId) ? "<TypeId?>" : unit.TypeId;
            return $"#{scenarioIndex + 1} {typeName} ({unit.Position.x},{unit.Position.y})";
        }

        private bool CanScenarioUnitSeeScenarioUnit(int observerIndex, int targetIndex)
        {
            if (observerIndex < 0 || targetIndex < 0 || observerIndex >= _generatorScenarioUnits.Count || targetIndex >= _generatorScenarioUnits.Count)
                return false;

            var observer = _generatorScenarioUnits[observerIndex];
            var target = _generatorScenarioUnits[targetIndex];
            return IsInsideSelectedVision(observer, target.Position.x, target.Position.y, target);
        }

        private SerializedProperty ResolveScenarioUnitConfig(PreviewScenarioUnit observer)
        {
            if (observer == null)
                return null;

            string typeId = observer.TypeId;
            if (_configs != null && !string.IsNullOrWhiteSpace(typeId))
            {
                for (int i = 0; i < _configs.arraySize; i++)
                {
                    var unit = _configs.GetArrayElementAtIndex(i);
                    if (string.Equals(GetString(unit, "TypeId"), typeId, StringComparison.Ordinal))
                        return unit;
                }
            }

            return HasSelectedUnit() ? SelectedUnitProperty() : null;
        }

        private static Color ResolveScenarioUnitColor(int index)
        {
            float hue = Mathf.Repeat(0.07f + index * 0.17f, 1f);
            return Color.HSVToRGB(hue, 0.72f, 0.96f);
        }

        private bool IsInsideScenarioVision(int x, int y)
        {
            if (_generatorPreviewFogUnion)
            {
                for (int i = 0; i < _generatorScenarioUnits.Count; i++)
                {
                    if (IsInsideSelectedVision(_generatorScenarioUnits[i], x, y))
                        return true;
                }

                return false;
            }

            return IsInsideSelectedVision(GetActiveScenarioUnit(), x, y);
        }

        private void EvaluateScenarioVisionAtCell(int x, int y, out bool visible, out byte viewers)
        {
            int viewerCount = 0;
            bool activeVisible = false;
            int activeIndex = Mathf.Clamp(_generatorScenarioActiveIndex, 0, Mathf.Max(0, _generatorScenarioUnits.Count - 1));

            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                if (!IsInsideSelectedVision(_generatorScenarioUnits[i], x, y))
                    continue;

                viewerCount++;
                if (i == activeIndex)
                    activeVisible = true;
            }

            visible = _generatorPreviewFogUnion ? viewerCount > 0 : activeVisible;
            viewers = (byte)Mathf.Clamp(viewerCount, 0, byte.MaxValue);
        }

        private bool IsCachedScenarioVisible(int x, int y)
        {
            if (_generatorPreviewVisibleMap == null)
                return IsInsideScenarioVision(x, y);

            int cx = Mathf.Clamp(x, 0, _generatorPreviewVisibleMap.GetLength(0) - 1);
            int cy = Mathf.Clamp(y, 0, _generatorPreviewVisibleMap.GetLength(1) - 1);
            return _generatorPreviewVisibleMap[cx, cy];
        }

        private int GetCachedScenarioViewers(int x, int y)
        {
            if (_generatorPreviewViewerCountMap == null)
                return CountScenarioViewers(x, y);

            int cx = Mathf.Clamp(x, 0, _generatorPreviewViewerCountMap.GetLength(0) - 1);
            int cy = Mathf.Clamp(y, 0, _generatorPreviewViewerCountMap.GetLength(1) - 1);
            return _generatorPreviewViewerCountMap[cx, cy];
        }

        private int CountScenarioViewers(int x, int y)
        {
            int count = 0;
            for (int i = 0; i < _generatorScenarioUnits.Count; i++)
            {
                if (IsInsideSelectedVision(_generatorScenarioUnits[i], x, y))
                    count++;
            }

            return count;
        }

        private bool IsInsideSelectedVision(PreviewScenarioUnit observer, int x, int y, PreviewScenarioUnit targetUnit = null)
        {
            if (observer == null)
                return false;

            int radius = ResolveSelectedEffectiveVisionRange(observer, x, y);
            int dx = x - observer.Position.x;
            int dy = y - observer.Position.y;
            float limit = (radius + 0.5f) * (radius + 0.5f);
            if (dx * dx + dy * dy > limit)
                return false;

            return !_generatorVisionUseTerrainLos || HasTerrainLineOfSight(observer, x, y, targetUnit);
        }

        private int ResolveSelectedUnitVisionRange(PreviewScenarioUnit observer)
        {
            var unitConfig = ResolveScenarioUnitConfig(observer);
            if (unitConfig == null)
                return 1;

            return Mathf.Clamp(GetInt(unitConfig, "VisionRange"), 1, 64);
        }

        private int ResolveSelectedUnitHeightLevel(PreviewScenarioUnit observer)
        {
            if (_generatorPreviewLevelMap == null || observer == null)
                return 0;

            int x = Mathf.Clamp(observer.Position.x, 0, _generatorPreviewWidth - 1);
            int y = Mathf.Clamp(observer.Position.y, 0, _generatorPreviewHeight - 1);
            return Mathf.Max(0, _generatorPreviewLevelMap[x, y]);
        }

        private float ResolveSelectedVisionBoostPerLevel(PreviewScenarioUnit observer)
        {
            float unitBoost = 0f;
            var unitConfig = ResolveScenarioUnitConfig(observer);
            if (unitConfig != null)
            {
                var unitBoostProp = unitConfig.FindPropertyRelative("VisionHeightBoostPerLevel");
                if (unitBoostProp != null)
                    unitBoost = Mathf.Max(0f, unitBoostProp.floatValue);
            }

            return Mathf.Max(0f, _generatorVisionGlobalBoost) + unitBoost;
        }

        private int ResolveSelectedEffectiveVisionRange(PreviewScenarioUnit observer)
        {
            if (observer == null)
                return 1;

            return ResolveSelectedEffectiveVisionRange(observer, observer.Position.x, observer.Position.y);
        }

        private int ResolveSelectedMaximumVisionRange(PreviewScenarioUnit observer)
        {
            int baseVision = ResolveSelectedEffectiveVisionRange(observer);
            float downSlopeBonus = ResolveScenarioDownSlopeVisionBonus(observer);
            return Mathf.Clamp(baseVision + Mathf.CeilToInt(downSlopeBonus), 1, 128);
        }

        private int ResolveSelectedEffectiveVisionRange(PreviewScenarioUnit observer, int targetX, int targetY)
        {
            int baseVision = ResolveSelectedUnitVisionRange(observer);
            int heightLevel = ResolveSelectedUnitHeightLevel(observer);
            float boostPerLevel = ResolveSelectedVisionBoostPerLevel(observer);
            int extraVision = Mathf.RoundToInt(heightLevel * boostPerLevel);
            int downSlopeVision = Mathf.CeilToInt(ResolveDirectionalDownSlopeVisionBonus(observer, targetX, targetY));
            return Mathf.Clamp(baseVision + Mathf.Max(0, extraVision) + Mathf.Max(0, downSlopeVision), 1, 128);
        }

        private float ResolveDirectionalDownSlopeVisionBonus(PreviewScenarioUnit observer, int targetX, int targetY)
        {
            if (!_generatorVisionUseTerrainLos || _generatorPreviewNoiseMap == null || observer == null)
                return 0f;

            int observerX = Mathf.Clamp(observer.Position.x, 0, _generatorPreviewWidth - 1);
            int observerY = Mathf.Clamp(observer.Position.y, 0, _generatorPreviewHeight - 1);
            if (observerX == targetX && observerY == targetY)
                return 0f;

            float observerHeight = SampleNoiseHeight(observerX, observerY);
            float targetHeight = SampleNoiseHeight(targetX, targetY);
            if (observerHeight - targetHeight < GetGeneratorEdgeHeightThreshold())
                return 0f;

            if (!TryFindGeneratorDownhillEdge(observerX, observerY, targetX, targetY, out _, out int distanceToEdge))
                return 0f;

            int peekDistance = Mathf.Max(0, _generatorVisionEdgePeekDistanceTiles);
            if (distanceToEdge > peekDistance)
                return 0f;

            float distanceFactor = 1f - distanceToEdge / (peekDistance + 1f);
            return ResolveScenarioDownSlopeVisionBonus(observer) * Mathf.Clamp01(distanceFactor);
        }

        private bool HasTerrainLineOfSight(PreviewScenarioUnit observer, int targetX, int targetY, PreviewScenarioUnit targetUnit = null)
        {
            if (_generatorPreviewNoiseMap == null || observer == null)
                return true;

            int observerX = Mathf.Clamp(observer.Position.x, 0, _generatorPreviewWidth - 1);
            int observerY = Mathf.Clamp(observer.Position.y, 0, _generatorPreviewHeight - 1);
            if (observerX == targetX && observerY == targetY)
                return true;

            float observerHeight = SampleNoiseHeight(observerX, observerY);
            float targetHeight = SampleNoiseHeight(targetX, targetY);
            int observerLevel = ResolveSelectedUnitHeightLevel(observer);
            int targetLevel = ResolveHeightLevelAt(targetX, targetY);
            float edgeThreshold = GetGeneratorEdgeHeightThreshold();
            float targetSilhouette = ResolveScenarioSilhouettePenalty(targetUnit);

            float observerEyeHeight = observerHeight + observerLevel * 0.025f + 0.08f;
            float targetEyeHeight = targetHeight + targetLevel * 0.015f + 0.04f + targetSilhouette * 0.065f;

            int dx = targetX - observerX;
            int dy = targetY - observerY;
            int gridDistance = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            int ignoredTerrainSteps = 0;
            if (observerHeight - targetHeight >= edgeThreshold)
            {
                if (TryFindGeneratorDownhillEdge(observerX, observerY, targetX, targetY, out int downhillEdgeStep, out int distanceToEdge))
                {
                    if (IsGeneratorTargetHiddenByDownhillEdge(gridDistance, downhillEdgeStep, distanceToEdge))
                        return false;

                    ignoredTerrainSteps = downhillEdgeStep;
                }
            }

            int steps = Mathf.Clamp(Mathf.CeilToInt(distance * 0.8f), 2, 28);
            float ignoredTerrainT = gridDistance > 0
                ? Mathf.Clamp01((ignoredTerrainSteps + 0.1f) / gridDistance)
                : 0f;
            bool blockedByTerrain = false;
            float firstBlockT = 0f;
            float maxBlockExcess = 0f;

            for (int i = 1; i < steps; i++)
            {
                float t = i / (float)steps;
                if (t <= ignoredTerrainT)
                    continue;

                float px = Mathf.Lerp(observerX + 0.5f, targetX + 0.5f, t);
                float py = Mathf.Lerp(observerY + 0.5f, targetY + 0.5f, t);
                float terrainHeight = SampleNoiseHeight(px, py);
                float expectedSightHeight = Mathf.Lerp(observerEyeHeight, targetEyeHeight, t);
                float excess = terrainHeight - (expectedSightHeight + _generatorVisionOcclusionTolerance);
                if (excess > 0f)
                {
                    blockedByTerrain = true;
                    if (firstBlockT <= 0f)
                        firstBlockT = t;
                    maxBlockExcess = Mathf.Max(maxBlockExcess, excess);
                }
            }

            if (blockedByTerrain)
            {
                float uphillDelta = targetHeight - observerHeight;
                float uphillEdgeFactor = ResolveGeneratorUphillEdgePeekFactor(observerX, observerY, targetX, targetY, targetHeight);
                bool canSeeCrest = ResolveScenarioCanSeeCrest(observer);
                float crestFactor = ResolveScenarioCrestVisibilityFactor(observer);
                bool silhouetteRevealsTarget = targetSilhouette > 0.01f && uphillEdgeFactor > 0f;
                bool canPeekUphill = uphillDelta > edgeThreshold
                    && distance > 1.5f
                    && uphillEdgeFactor > 0f
                    && (canSeeCrest || silhouetteRevealsTarget);

                if (!canPeekUphill)
                    return false;

                float nearTargetFactor = Mathf.Clamp01((firstBlockT - 0.45f) / 0.45f);
                float crestStrength = canSeeCrest ? Mathf.Clamp01(_generatorVisionUphillPeekStrength * crestFactor) : 0f;
                float silhouetteStrength = Mathf.Clamp01(targetSilhouette * 0.55f);
                float allowedExcess = Mathf.Lerp(0.005f, 0.12f, Mathf.Clamp01(crestStrength + silhouetteStrength) * uphillEdgeFactor * nearTargetFactor);
                if (maxBlockExcess > allowedExcess)
                    return false;
            }

            return true;
        }

        private bool ResolveScenarioCanSeeCrest(PreviewScenarioUnit observer)
        {
            var config = ResolveScenarioUnitConfig(observer);
            return GetBool(config, "CanSeeCrest", true);
        }

        private float ResolveScenarioCrestVisibilityFactor(PreviewScenarioUnit observer)
        {
            var config = ResolveScenarioUnitConfig(observer);
            var crest = config?.FindPropertyRelative("CrestVisibilityFactor");
            return Mathf.Clamp01(crest != null ? crest.floatValue : 0.65f);
        }

        private float ResolveScenarioDownSlopeVisionBonus(PreviewScenarioUnit observer)
        {
            var config = ResolveScenarioUnitConfig(observer);
            var bonus = config?.FindPropertyRelative("DownSlopeVisionBonus");
            return Mathf.Max(0f, bonus != null ? bonus.floatValue : 0f);
        }

        private float ResolveScenarioSilhouettePenalty(PreviewScenarioUnit targetUnit)
        {
            if (targetUnit == null)
                return 0f;

            var config = ResolveScenarioUnitConfig(targetUnit);
            var silhouette = config?.FindPropertyRelative("SilhouettePenalty");
            return Mathf.Clamp01(silhouette != null ? silhouette.floatValue : 0f);
        }

        private bool TryFindGeneratorDownhillEdge(int observerX, int observerY, int targetX, int targetY, out int edgeStep, out int distanceToEdge)
        {
            edgeStep = -1;
            distanceToEdge = 0;

            float previousHeight = SampleNoiseHeight(observerX, observerY);
            int currentX = observerX;
            int currentY = observerY;
            int dx = Mathf.Abs(targetX - observerX);
            int dy = Mathf.Abs(targetY - observerY);
            int sx = observerX < targetX ? 1 : -1;
            int sy = observerY < targetY ? 1 : -1;
            int error = dx - dy;
            int stepIndex = 0;
            float edgeThreshold = GetGeneratorEdgeHeightThreshold();

            while (currentX != targetX || currentY != targetY)
            {
                int twiceError = error * 2;
                if (twiceError > -dy)
                {
                    error -= dy;
                    currentX += sx;
                }

                if (twiceError < dx)
                {
                    error += dx;
                    currentY += sy;
                }

                stepIndex++;
                float currentHeight = SampleNoiseHeight(currentX, currentY);
                if (previousHeight - currentHeight >= edgeThreshold)
                {
                    edgeStep = stepIndex;
                    distanceToEdge = Mathf.Max(0, stepIndex - 1);
                    return true;
                }

                previousHeight = currentHeight;
            }

            return false;
        }

        private bool IsGeneratorTargetHiddenByDownhillEdge(int gridDistance, int edgeStep, int distanceToEdge)
        {
            int blindZone = ResolveGeneratorDownhillBlindZoneTiles(distanceToEdge);
            if (blindZone <= 0)
                return false;

            int distancePastEdge = Mathf.Max(1, gridDistance - edgeStep + 1);
            return distancePastEdge <= blindZone;
        }

        private int ResolveGeneratorDownhillBlindZoneTiles(int distanceToEdge)
        {
            int peekDistance = Mathf.Max(0, _generatorVisionEdgePeekDistanceTiles);
            if (distanceToEdge <= peekDistance)
                return 0;

            float strength = Mathf.Clamp01(_generatorVisionDownhillPenalty);
            int baseBlindZone = Mathf.RoundToInt(Mathf.Max(0, _generatorVisionEdgeBlindZoneTiles) * Mathf.Lerp(0.5f, 1.25f, strength));
            int maxBlindZone = Mathf.Max(baseBlindZone, _generatorVisionEdgeMaxBlindZoneTiles);
            float extraBlindZone = Mathf.Max(0, distanceToEdge - peekDistance) * Mathf.Max(0f, _generatorVisionEdgeDistanceScale);
            return Mathf.Clamp(Mathf.RoundToInt(baseBlindZone + extraBlindZone), 0, maxBlindZone);
        }

        private float ResolveGeneratorUphillEdgePeekFactor(int observerX, int observerY, int targetX, int targetY, float targetHeight)
        {
            if (observerX == targetX && observerY == targetY)
                return 0f;

            int peekDistance = Mathf.Max(0, _generatorVisionEdgePeekDistanceTiles);
            if (!TryFindGeneratorUphillEdgeTowardObserver(observerX, observerY, targetX, targetY, targetHeight, out int distanceToEdge))
                return 0f;

            if (distanceToEdge > peekDistance)
                return 0f;

            return 1f - distanceToEdge / (peekDistance + 1f);
        }

        private bool TryFindGeneratorUphillEdgeTowardObserver(int observerX, int observerY, int targetX, int targetY, float targetHeight, out int distanceToEdge)
        {
            distanceToEdge = 0;

            int currentX = targetX;
            int currentY = targetY;
            int dx = Mathf.Abs(observerX - targetX);
            int dy = Mathf.Abs(observerY - targetY);
            if (dx == 0 && dy == 0)
                return false;

            int sx = targetX < observerX ? 1 : -1;
            int sy = targetY < observerY ? 1 : -1;
            int error = dx - dy;
            int maxSteps = Mathf.Max(1, _generatorVisionEdgePeekDistanceTiles + 1);
            float threshold = GetGeneratorEdgeHeightThreshold();

            for (int step = 1; step <= maxSteps && (currentX != observerX || currentY != observerY); step++)
            {
                int twiceError = error * 2;
                if (twiceError > -dy)
                {
                    error -= dy;
                    currentX += sx;
                }

                if (twiceError < dx)
                {
                    error += dx;
                    currentY += sy;
                }

                if (!IsGeneratorCellInBounds(currentX, currentY))
                {
                    distanceToEdge = Mathf.Max(0, step - 1);
                    return true;
                }

                if (targetHeight - SampleNoiseHeight(currentX, currentY) >= threshold)
                {
                    distanceToEdge = Mathf.Max(0, step - 1);
                    return true;
                }
            }

            return false;
        }

        private bool IsGeneratorCellInBounds(int x, int y)
        {
            return x >= 0 && x < _generatorPreviewWidth && y >= 0 && y < _generatorPreviewHeight;
        }

        private float GetGeneratorEdgeHeightThreshold()
        {
            return Mathf.Max(0.001f, _generatorVisionUphillEdgeDrop);
        }

        private int ResolveHeightLevelAt(int x, int y)
        {
            if (_generatorPreviewLevelMap == null)
                return 0;

            int cx = Mathf.Clamp(x, 0, _generatorPreviewWidth - 1);
            int cy = Mathf.Clamp(y, 0, _generatorPreviewHeight - 1);
            return Mathf.Max(0, _generatorPreviewLevelMap[cx, cy]);
        }

        private float SampleNoiseHeight(int x, int y)
        {
            if (_generatorPreviewNoiseMap == null)
                return 0f;

            int cx = Mathf.Clamp(x, 0, _generatorPreviewWidth - 1);
            int cy = Mathf.Clamp(y, 0, _generatorPreviewHeight - 1);
            return _generatorPreviewNoiseMap[cx, cy];
        }

        private float SampleNoiseHeight(float x, float y)
        {
            if (_generatorPreviewNoiseMap == null)
                return 0f;

            float fx = Mathf.Clamp(x - 0.5f, 0f, _generatorPreviewWidth - 1f);
            float fy = Mathf.Clamp(y - 0.5f, 0f, _generatorPreviewHeight - 1f);

            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);
            int x1 = Mathf.Min(x0 + 1, _generatorPreviewWidth - 1);
            int y1 = Mathf.Min(y0 + 1, _generatorPreviewHeight - 1);
            float tx = fx - x0;
            float ty = fy - y0;

            float h00 = _generatorPreviewNoiseMap[x0, y0];
            float h10 = _generatorPreviewNoiseMap[x1, y0];
            float h01 = _generatorPreviewNoiseMap[x0, y1];
            float h11 = _generatorPreviewNoiseMap[x1, y1];

            float hx0 = Mathf.Lerp(h00, h10, tx);
            float hx1 = Mathf.Lerp(h01, h11, tx);
            return Mathf.Lerp(hx0, hx1, ty);
        }

        private Color ResolvePreviewColor(int levelIndex, float heightValue)
        {
            Color baseColor = GeneratorLevelPalette[Mathf.Abs(levelIndex) % GeneratorLevelPalette.Length];
            float shade = Mathf.Lerp(0.82f, 1.18f, Mathf.Clamp01(heightValue));
            return new Color(
                Mathf.Clamp01(baseColor.r * shade),
                Mathf.Clamp01(baseColor.g * shade),
                Mathf.Clamp01(baseColor.b * shade),
                1f);
        }

        private void EnsureGeneratorPreviewTexture(int width, int height)
        {
            if (_generatorPreviewTexture != null && _generatorPreviewTexture.width == width && _generatorPreviewTexture.height == height)
                return;

            if (_generatorPreviewTexture != null)
                DestroyImmediate(_generatorPreviewTexture);

            _generatorPreviewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "UnitDesignerGeneratorFogPreview"
            };
        }

        private void EnsureGeneratorVisibilityOverlayTexture(int width, int height)
        {
            if (_generatorVisibilityOverlayTexture != null && _generatorVisibilityOverlayTexture.width == width && _generatorVisibilityOverlayTexture.height == height)
                return;

            if (_generatorVisibilityOverlayTexture != null)
                DestroyImmediate(_generatorVisibilityOverlayTexture);

            _generatorVisibilityOverlayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                name = "UnitDesignerGeneratorVisibilityOverlay"
            };
        }

        private void ClearGeneratorVisibilityOverlayTexture()
        {
            if (_generatorVisibilityOverlayTexture == null)
                return;

            int width = Mathf.Max(1, _generatorVisibilityOverlayTexture.width);
            int height = Mathf.Max(1, _generatorVisibilityOverlayTexture.height);
            var pixels = new Color32[width * height];
            _generatorVisibilityOverlayTexture.SetPixels32(pixels);
            _generatorVisibilityOverlayTexture.Apply(false, false);
        }

        private Rect FitTexture(Rect rect, int textureWidth, int textureHeight)
        {
            float aspect = textureWidth / Mathf.Max(1f, textureHeight);
            float width = rect.width;
            float height = width / aspect;
            if (height > rect.height)
            {
                height = rect.height;
                width = height * aspect;
            }

            return new Rect(rect.x + (rect.width - width) * 0.5f, rect.y + (rect.height - height) * 0.5f, width, height);
        }

        private Vector2Int ScreenToPreviewCell(Rect drawRect, Vector2 mouse)
        {
            float relX = Mathf.Clamp01((mouse.x - drawRect.x) / Mathf.Max(1f, drawRect.width));
            float relY = Mathf.Clamp01((mouse.y - drawRect.y) / Mathf.Max(1f, drawRect.height));
            int x = Mathf.Clamp(Mathf.FloorToInt(relX * _generatorPreviewWidth), 0, _generatorPreviewWidth - 1);
            int y = Mathf.Clamp(_generatorPreviewHeight - 1 - Mathf.FloorToInt(relY * _generatorPreviewHeight), 0, _generatorPreviewHeight - 1);
            return new Vector2Int(x, y);
        }

        private void DrawLevelLegendRow(PreviewHeightLevel level, int index)
        {
            Rect row = GUILayoutUtility.GetRect(0f, 26f, GUILayout.ExpandWidth(true));
            Rect colorRect = new Rect(row.x + 4f, row.y + 5f, 42f, 16f);
            EditorGUI.DrawRect(colorRect, GeneratorLevelPalette[index % GeneratorLevelPalette.Length]);
            string label = $"{index + 1}. {level.MinHeight:0.00}-{level.MaxHeight:0.00}  {level.TileId}";
            GUI.Label(new Rect(colorRect.xMax + 8f, row.y + 4f, row.width - 56f, 18f), label, EditorStyles.miniLabel);
        }

        private void ExtractGeneratorReferencesFromAsset(bool applyMapSize)
        {
            _hillGeneratorNodeAsset = null;

            if (_generatorAsset == null)
                return;

            if (IsObjectOfType(_generatorAsset, DataNoiseSettingsTypeName))
                _noiseSettingsAsset = _generatorAsset;
            if (IsObjectOfType(_generatorAsset, HeightMapSettingsTypeName))
                _heightSettingsAsset = _generatorAsset;
            if (IsObjectOfType(_generatorAsset, HillGeneratorNodeTypeName))
                _hillGeneratorNodeAsset = _generatorAsset;

            string path = AssetDatabase.GetAssetPath(_generatorAsset);
            if (!string.IsNullOrEmpty(path))
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                for (int i = 0; i < assets.Length; i++)
                    ScanSerializedObjectForGeneratorReferences(assets[i], applyMapSize);
            }

            ScanSerializedObjectForGeneratorReferences(_generatorAsset, applyMapSize);
            SaveScriptableObjectPreference(NoiseSettingsGuidPrefsKey, _noiseSettingsAsset);
            SaveScriptableObjectPreference(HeightSettingsGuidPrefsKey, _heightSettingsAsset);
        }

        private void ScanSerializedObjectForGeneratorReferences(UnityEngine.Object source, bool applyMapSize)
        {
            if (source == null)
                return;

            try
            {
                if (applyMapSize && IsObjectOfType(source, GraphAssetTypeName))
                    TryApplyGraphSharedMapSize(source);
                if (_hillGeneratorNodeAsset == null && IsObjectOfType(source, HillGeneratorNodeTypeName))
                    _hillGeneratorNodeAsset = source as ScriptableObject;

                var serialized = new SerializedObject(source);
                var property = serialized.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    var referenced = property.objectReferenceValue as ScriptableObject;
                    if (referenced == null)
                        continue;

                    if (_noiseSettingsAsset == null && IsObjectOfType(referenced, DataNoiseSettingsTypeName))
                        _noiseSettingsAsset = referenced;
                    if (_heightSettingsAsset == null && IsObjectOfType(referenced, HeightMapSettingsTypeName))
                        _heightSettingsAsset = referenced;
                    if (_hillGeneratorNodeAsset == null && IsObjectOfType(referenced, HillGeneratorNodeTypeName))
                        _hillGeneratorNodeAsset = referenced;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UnitDesigner] Не вдалося просканувати generator asset '{source.name}': {e.Message}");
            }
        }

        private void TryApplyGraphSharedMapSize(UnityEngine.Object graphAsset)
        {
            var serialized = new SerializedObject(graphAsset);
            var shared = serialized.FindProperty("_sharedSettings");
            if (shared == null)
                return;

            int width = shared.FindPropertyRelative("_mapWidth")?.intValue ?? 0;
            int height = shared.FindPropertyRelative("_mapHeight")?.intValue ?? 0;
            if (width > 0 && height > 0)
            {
                _generatorPreviewWidth = Mathf.Clamp(width, 8, 256);
                _generatorPreviewHeight = Mathf.Clamp(height, 8, 256);
                _generatorPreviewUnitPosition = new Vector2Int(_generatorPreviewWidth / 2, _generatorPreviewHeight / 2);
                var active = GetActiveScenarioUnit();
                if (active != null)
                    active.Position = _generatorPreviewUnitPosition;
            }
        }

        private void CreateNoiseSettingsAsset()
        {
            var type = ResolveUnityType(DataNoiseSettingsTypeName);
            if (type == null)
            {
                EditorUtility.DisplayDialog("Noise Settings", "Тип DataNoiseSettings не знайдено.", "OK");
                return;
            }

            EnsureFolder("Assets/Moyva/SO/Generator");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Moyva/SO/Generator/DataNoiseSettings.asset");
            _noiseSettingsAsset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(_noiseSettingsAsset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _noiseSettingsObject = new SerializedObject(_noiseSettingsAsset);
            SaveScriptableObjectPreference(NoiseSettingsGuidPrefsKey, _noiseSettingsAsset);
            Selection.activeObject = _noiseSettingsAsset;
            EditorGUIUtility.PingObject(_noiseSettingsAsset);
            MarkGeneratorPreviewDirty();
        }

        private void CreateHeightSettingsAsset()
        {
            var type = ResolveUnityType(HeightMapSettingsTypeName);
            if (type == null)
            {
                EditorUtility.DisplayDialog("Height Settings", "Тип HeightMapSettings не знайдено.", "OK");
                return;
            }

            EnsureFolder("Assets/Moyva/SO/Generator");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Moyva/SO/Generator/HeightMapSettings.asset");
            _heightSettingsAsset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(_heightSettingsAsset, path);
            _heightSettingsObject = new SerializedObject(_heightSettingsAsset);
            var layers = _heightSettingsObject.FindProperty("HeightLayers");
            if (layers != null && layers.isArray)
            {
                layers.arraySize = 4;
                for (int i = 0; i < layers.arraySize; i++)
                    InitializeHeightLayer(layers.GetArrayElementAtIndex(i), i, layers.arraySize);
                _heightSettingsObject.ApplyModifiedProperties();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            SaveScriptableObjectPreference(HeightSettingsGuidPrefsKey, _heightSettingsAsset);
            Selection.activeObject = _heightSettingsAsset;
            EditorGUIUtility.PingObject(_heightSettingsAsset);
            MarkGeneratorPreviewDirty();
        }

        private void DrawSerializedObjectProperties(SerializedObject serializedObject)
        {
            var property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.PropertyField(property, true);
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }
        }

        private static ScriptableObject LoadScriptableObjectPreference(string key)
        {
            string contextTypeName = ResolveProjectContextTypeNameForPreference(key);
            if (!string.IsNullOrWhiteSpace(contextTypeName))
            {
                var contextType = MoyvaProjectEditorContext.ResolveScriptableObjectType(contextTypeName);
                var contextAsset = MoyvaProjectEditorContext.Get(contextTypeName, contextType) as ScriptableObject;
                if (contextAsset != null)
                    return contextAsset;
            }

            string guid = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }

        private static void SaveScriptableObjectPreference(string key, ScriptableObject asset)
        {
            string contextTypeName = ResolveProjectContextTypeNameForPreference(key);
            if (!string.IsNullOrWhiteSpace(contextTypeName))
                MoyvaProjectEditorContext.Set(contextTypeName, asset);

            if (asset == null)
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            if (!string.IsNullOrWhiteSpace(guid))
                EditorPrefs.SetString(key, guid);
        }

        private static string ResolveProjectContextTypeNameForPreference(string key)
        {
            if (key == GeneratorAssetGuidPrefsKey)
                return "GraphAsset";
            if (key == NoiseSettingsGuidPrefsKey)
                return "DataNoiseSettings";
            if (key == HeightSettingsGuidPrefsKey)
                return "HeightMapSettings";
            return string.Empty;
        }

        private static ScriptableObject FindFirstAssetOfType(string fullTypeName)
        {
            var type = ResolveUnityType(fullTypeName);
            if (type == null)
                return null;

            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;
                if (asset != null)
                    return asset;
            }

            return null;
        }

        private static Type ResolveUnityType(string fullTypeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullTypeName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static bool IsObjectOfType(UnityEngine.Object asset, string fullTypeName)
        {
            return asset != null && string.Equals(asset.GetType().FullName, fullTypeName, StringComparison.Ordinal);
        }

        private static float GetSerializedFloat(SerializedObject serializedObject, string propertyName, float fallback)
        {
            var property = serializedObject?.FindProperty(propertyName);
            return property != null ? property.floatValue : fallback;
        }

        private static int GetSerializedInt(SerializedObject serializedObject, string propertyName, int fallback)
        {
            var property = serializedObject?.FindProperty(propertyName);
            return property != null ? property.intValue : fallback;
        }

        private static Vector2 GetSerializedVector2(SerializedObject serializedObject, string propertyName, Vector2 fallback)
        {
            var property = serializedObject?.FindProperty(propertyName);
            return property != null ? property.vector2Value : fallback;
        }

        private static void SetRelativeString(SerializedProperty property, string relativeName, string value)
        {
            var relative = property?.FindPropertyRelative(relativeName);
            if (relative != null)
                relative.stringValue = value;
        }

        private static void SetRelativeFloat(SerializedProperty property, string relativeName, float value)
        {
            var relative = property?.FindPropertyRelative(relativeName);
            if (relative != null)
                relative.floatValue = value;
        }

        private static int PositiveHash(int seed, int x, int y)
        {
            unchecked
            {
                uint hash = 2166136261u;
                hash = (hash ^ (uint)seed) * 16777619u;
                hash = (hash ^ (uint)x) * 16777619u;
                hash = (hash ^ (uint)y) * 16777619u;
                hash ^= hash >> 17;
                hash *= 0xbf58476du;
                hash ^= hash >> 31;
                hash *= 0x94d049bbu;
                hash ^= hash >> 16;
                return (int)(hash & 0x7FFFFFFFu);
            }
        }

        private static GUIStyle GeneratorPrimaryButtonStyle()
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
            };
        }

        private sealed class PreviewHeightLevel
        {
            public string TileId;
            public float TileChance;
            public float MinHeight;
            public float MaxHeight;
            public PreviewWeightedTile[] Variants;
        }

        private sealed class PreviewScenarioUnit
        {
            public string TypeId;
            public Vector2Int Position;
        }

        private struct PreviewWeightedTile
        {
            public string TileId;
            public float Chance;
        }
    }
}