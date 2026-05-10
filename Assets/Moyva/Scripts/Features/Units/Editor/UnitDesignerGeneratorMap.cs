using System;
using System.Collections.Generic;
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
        private const string GeneratorPreviewAutoPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewAuto";
        private const string GeneratorPreviewFogPrefsKey = "Moyva.UnitDesigner.GeneratorPreviewFog";

        private const string DataNoiseSettingsTypeName = "Kruty1918.Moyva.Generator.API.DataNoiseSettings";
        private const string HeightMapSettingsTypeName = "Kruty1918.Moyva.Generator.API.HeightMapSettings";
        private const string GraphAssetTypeName = "Kruty1918.Moyva.GraphSystem.API.GraphAsset";

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
        private SerializedObject _noiseSettingsObject;
        private SerializedObject _heightSettingsObject;
        private Texture2D _generatorPreviewTexture;
        private float[,] _generatorPreviewNoiseMap;
        private string[,] _generatorPreviewTileMap;
        private int[,] _generatorPreviewLevelMap;
        private Vector2 _generatorLevelsScroll;
        private Vector2 _generatorPreviewScroll;
        private Vector2 _generatorLegendScroll;
        private Vector2Int _generatorPreviewUnitPosition = new Vector2Int(32, 32);
        private Rect _lastGeneratorPreviewRect;
        private Vector2 _generatorPreviewMouse;
        private int _generatorPreviewWidth = 64;
        private int _generatorPreviewHeight = 64;
        private int _generatorPreviewSeed = 1918;
        private bool _generatorPreviewAutoRefresh = true;
        private bool _generatorPreviewShowFog = true;
        private bool _generatorPreviewDirty = true;
        private string _generatorPreviewStatus = "Preview ще не побудовано.";

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

            _generatorAsset = LoadScriptableObjectPreference(GeneratorAssetGuidPrefsKey);
            _noiseSettingsAsset = LoadScriptableObjectPreference(NoiseSettingsGuidPrefsKey) ?? FindFirstAssetOfType(DataNoiseSettingsTypeName);
            _heightSettingsAsset = LoadScriptableObjectPreference(HeightSettingsGuidPrefsKey) ?? FindFirstAssetOfType(HeightMapSettingsTypeName);

            if (_generatorAsset == null)
                _generatorAsset = FindFirstAssetOfType(GraphAssetTypeName);

            if (_generatorAsset != null && (_noiseSettingsAsset == null || _heightSettingsAsset == null))
                ExtractGeneratorReferencesFromAsset(applyMapSize: true);

            RefreshGeneratorMapSerializedObjects();
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

            if (_generatorPreviewTexture != null)
                DestroyImmediate(_generatorPreviewTexture);
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

        private void DrawWorkspaceModeToolbar()
        {
            GUILayout.Space(8f);
            string[] modes = { "Юніти", "Мапа + Fog", "Бій" };
            int next = GUILayout.Toolbar((int)_workspaceMode, modes, EditorStyles.toolbarButton, GUILayout.Width(260f));
            if (next != (int)_workspaceMode)
            {
                _workspaceMode = (WorkspaceMode)next;
                if (_workspaceMode == WorkspaceMode.GeneratorMap)
                    MarkGeneratorPreviewDirty();
                if (_workspaceMode == WorkspaceMode.CombatSystem)
                    OnCombatWorkspaceSelected();
            }
        }

        private void DrawGeneratorMapWorkspace()
        {
            MaybeRebuildGeneratorPreview();

            EditorGUILayout.BeginHorizontal();
            DrawUnitListPanel(GUILayout.Width(Mathf.Clamp(position.width * 0.24f, 280f, 360f)));
            DrawGeneratorLevelsPanel(GUILayout.MinWidth(360f), GUILayout.Width(Mathf.Clamp(position.width * 0.34f, 360f, 520f)));
            DrawGeneratorMapPreviewPanel(GUILayout.MinWidth(380f));
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
            BeginSection("Генератор", "d_GraphView Icon", "GraphAsset або інший ScriptableObject генератора, з якого можна підтягнути noise/height settings.");

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

            _generatorPreviewScroll = EditorGUILayout.BeginScrollView(_generatorPreviewScroll);
            DrawGeneratorPreviewControls();
            DrawGeneratorPreviewTexture();
            DrawGeneratorPreviewLegend();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawGeneratorPreviewControls()
        {
            BeginSection("Preview", "d_SceneViewCamera", "Параметри мапи, позиція юніта і Fog overlay.");

            EditorGUI.BeginChangeCheck();
            _generatorPreviewWidth = EditorGUILayout.IntSlider(new GUIContent("Width", "Ширина preview-мапи у тайлах."), _generatorPreviewWidth, 8, 256);
            _generatorPreviewHeight = EditorGUILayout.IntSlider(new GUIContent("Height", "Висота preview-мапи у тайлах."), _generatorPreviewHeight, 8, 256);
            _generatorPreviewSeed = EditorGUILayout.IntField(new GUIContent("Seed", "Seed для deterministic preview."), _generatorPreviewSeed);
            if (EditorGUI.EndChangeCheck())
            {
                _generatorPreviewUnitPosition.x = Mathf.Clamp(_generatorPreviewUnitPosition.x, 0, _generatorPreviewWidth - 1);
                _generatorPreviewUnitPosition.y = Mathf.Clamp(_generatorPreviewUnitPosition.y, 0, _generatorPreviewHeight - 1);
                MarkGeneratorPreviewDirty();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Unit", GUILayout.Width(38f));
                _generatorPreviewUnitPosition.x = EditorGUILayout.IntSlider(_generatorPreviewUnitPosition.x, 0, Mathf.Max(0, _generatorPreviewWidth - 1));
                _generatorPreviewUnitPosition.y = EditorGUILayout.IntSlider(_generatorPreviewUnitPosition.y, 0, Mathf.Max(0, _generatorPreviewHeight - 1));
            }

            EditorGUI.BeginChangeCheck();
            _generatorPreviewShowFog = EditorGUILayout.ToggleLeft(new GUIContent("Показувати Fog of War", "Затемнює усе поза радіусом огляду вибраного юніта."), _generatorPreviewShowFog);
            _generatorPreviewAutoRefresh = EditorGUILayout.ToggleLeft(new GUIContent("Auto refresh", "Автоматично перебудовувати texture після зміни noise/height settings."), _generatorPreviewAutoRefresh);
            if (EditorGUI.EndChangeCheck())
                MarkGeneratorPreviewDirty();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Оновити preview", GeneratorPrimaryButtonStyle(), GUILayout.Height(28f)))
                    RebuildGeneratorMapPreview();

                if (GUILayout.Button("Центр", GUILayout.Width(64f), GUILayout.Height(28f)))
                {
                    _generatorPreviewUnitPosition = new Vector2Int(_generatorPreviewWidth / 2, _generatorPreviewHeight / 2);
                    MarkGeneratorPreviewDirty();
                }
            }

            int vision = ResolveSelectedVisionRange();
            EditorGUILayout.HelpBox($"Вибраний юніт відкриває радіус {vision}. Клікни по мапі, щоб переставити юніта.", MessageType.None);
            EndSection();
        }

        private void DrawGeneratorPreviewTexture()
        {
            Rect rect = GUILayoutUtility.GetRect(320f, 360f, GUILayout.ExpandWidth(true));
            _lastGeneratorPreviewRect = rect;

            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.08f, 0.09f, 0.1f) : new Color(0.78f, 0.82f, 0.84f));

            if (_generatorPreviewTexture == null)
            {
                GUI.Label(new Rect(rect.x + 12f, rect.center.y - 18f, rect.width - 24f, 36f), _generatorPreviewStatus, CenterMiniStyle());
                return;
            }

            Rect drawRect = FitTexture(rect, _generatorPreviewTexture.width, _generatorPreviewTexture.height);
            GUI.DrawTexture(drawRect, _generatorPreviewTexture, ScaleMode.StretchToFill, false);
            DrawGeneratorPreviewOverlay(drawRect);
            HandleGeneratorPreviewInput(drawRect);
            DrawGeneratorPreviewHover(drawRect);
        }

        private void DrawGeneratorPreviewOverlay(Rect drawRect)
        {
            GUI.Label(new Rect(drawRect.x + 8f, drawRect.y + 8f, drawRect.width - 16f, 20f), _generatorPreviewStatus, PreviewTitleStyle());

            float cellW = drawRect.width / Mathf.Max(1, _generatorPreviewWidth);
            float cellH = drawRect.height / Mathf.Max(1, _generatorPreviewHeight);
            float ux = drawRect.x + _generatorPreviewUnitPosition.x * cellW;
            float uy = drawRect.yMax - (_generatorPreviewUnitPosition.y + 1) * cellH;
            Rect unitRect = new Rect(ux, uy, Mathf.Max(3f, cellW), Mathf.Max(3f, cellH));
            EditorGUI.DrawRect(unitRect, new Color(1f, 0.82f, 0.22f, 1f));

            Handles.BeginGUI();
            Handles.color = VisionOutline;
            float radiusPx = ResolveSelectedVisionRange() * Mathf.Min(cellW, cellH);
            Handles.DrawWireDisc(new Vector3(unitRect.center.x, unitRect.center.y, 0f), Vector3.forward, radiusPx);
            Handles.EndGUI();
        }

        private void HandleGeneratorPreviewInput(Rect drawRect)
        {
            var evt = Event.current;
            if (evt == null)
                return;

            if (evt.type == EventType.MouseMove || evt.type == EventType.Repaint)
                _generatorPreviewMouse = evt.mousePosition;

            if ((evt.type != EventType.MouseDown && evt.type != EventType.MouseDrag) || evt.button != 0)
                return;

            if (!drawRect.Contains(evt.mousePosition))
                return;

            Vector2Int cell = ScreenToPreviewCell(drawRect, evt.mousePosition);
            if (cell != _generatorPreviewUnitPosition)
            {
                _generatorPreviewUnitPosition = cell;
                MarkGeneratorPreviewDirty();
            }

            evt.Use();
        }

        private void DrawGeneratorPreviewHover(Rect drawRect)
        {
            if (_generatorPreviewTexture == null || !drawRect.Contains(_generatorPreviewMouse))
                return;

            Vector2Int cell = ScreenToPreviewCell(drawRect, _generatorPreviewMouse);
            if (_generatorPreviewNoiseMap == null)
                return;

            float h = _generatorPreviewNoiseMap[cell.x, cell.y];
            string tile = _generatorPreviewTileMap != null ? _generatorPreviewTileMap[cell.x, cell.y] : string.Empty;
            int level = _generatorPreviewLevelMap != null ? _generatorPreviewLevelMap[cell.x, cell.y] : -1;
            bool visible = IsInsideSelectedVision(cell.x, cell.y);
            string text = $"[{cell.x}, {cell.y}] h={h:0.000} level={level + 1} tile={tile} {(visible ? "visible" : "fog")}";

            var style = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.white },
                fontSize = 11,
                padding = new RectOffset(6, 6, 3, 3)
            };

            var content = new GUIContent(text);
            Vector2 size = style.CalcSize(content);
            float x = Mathf.Min(_generatorPreviewMouse.x + 14f, position.width - size.x - 8f);
            float y = Mathf.Max(drawRect.y + 8f, _generatorPreviewMouse.y - size.y - 6f);
            Rect bg = new Rect(x - 2f, y - 2f, size.x + 4f, size.y + 4f);
            EditorGUI.DrawRect(bg, new Color(0f, 0f, 0f, 0.78f));
            GUI.Label(new Rect(x, y, size.x, size.y), content, style);
        }

        private void DrawGeneratorPreviewLegend()
        {
            BeginSection("Легенда", "d_FilterByLabel", "Поточні рівні висоти та їхні кольори у preview.");
            var levels = BuildPreviewLevels();
            if (levels.Count == 0)
            {
                EditorGUILayout.HelpBox("Немає HeightLayers для легенди.", MessageType.Info);
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
            if (!_generatorPreviewDirty || !_generatorPreviewAutoRefresh)
                return;

            RebuildGeneratorMapPreview();
        }

        private void MarkGeneratorPreviewDirty()
        {
            _generatorPreviewDirty = true;
            Repaint();
        }

        private void RebuildGeneratorMapPreview()
        {
            _generatorPreviewDirty = false;
            _generatorPreviewWidth = Mathf.Clamp(_generatorPreviewWidth, 8, 256);
            _generatorPreviewHeight = Mathf.Clamp(_generatorPreviewHeight, 8, 256);
            _generatorPreviewUnitPosition.x = Mathf.Clamp(_generatorPreviewUnitPosition.x, 0, _generatorPreviewWidth - 1);
            _generatorPreviewUnitPosition.y = Mathf.Clamp(_generatorPreviewUnitPosition.y, 0, _generatorPreviewHeight - 1);

            if (_noiseSettingsObject == null || _heightSettingsObject == null)
            {
                _generatorPreviewStatus = "Потрібні Noise Settings і Height Settings.";
                return;
            }

            var levels = BuildPreviewLevels();
            if (levels.Count == 0)
            {
                _generatorPreviewStatus = "HeightLayers порожній.";
                return;
            }

            _generatorPreviewNoiseMap = GeneratePreviewNoiseMap(_noiseSettingsObject, _generatorPreviewWidth, _generatorPreviewHeight, _generatorPreviewSeed);
            _generatorPreviewTileMap = new string[_generatorPreviewWidth, _generatorPreviewHeight];
            _generatorPreviewLevelMap = new int[_generatorPreviewWidth, _generatorPreviewHeight];
            EnsureGeneratorPreviewTexture(_generatorPreviewWidth, _generatorPreviewHeight);

            int visibleCells = 0;
            for (int y = 0; y < _generatorPreviewHeight; y++)
            {
                for (int x = 0; x < _generatorPreviewWidth; x++)
                {
                    float heightValue = _generatorPreviewNoiseMap[x, y];
                    int levelIndex = ResolvePreviewLevel(levels, heightValue);
                    var level = levels[Mathf.Clamp(levelIndex, 0, levels.Count - 1)];
                    string tileId = SelectPreviewTile(level, x, y, _generatorPreviewSeed);
                    bool visible = IsInsideSelectedVision(x, y);
                    if (visible)
                        visibleCells++;

                    _generatorPreviewTileMap[x, y] = tileId;
                    _generatorPreviewLevelMap[x, y] = levelIndex;
                    Color color = ResolvePreviewColor(levelIndex, heightValue);
                    if (_generatorPreviewShowFog && !visible)
                        color = Color.Lerp(new Color(0.02f, 0.025f, 0.03f), color, 0.22f);
                    if (_generatorPreviewShowFog && visible)
                        color = Color.Lerp(color, VisionFill, 0.18f);

                    _generatorPreviewTexture.SetPixel(x, y, color);
                }
            }

            _generatorPreviewTexture.Apply(false, false);
            _generatorPreviewStatus = $"{_generatorPreviewWidth}x{_generatorPreviewHeight} | seed {_generatorPreviewSeed} | visible {visibleCells}";
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

        private string SelectPreviewTile(PreviewHeightLevel level, int x, int y, int seed)
        {
            if (level.Variants == null || level.Variants.Length == 0)
                return level.TileId;

            float roll = (PositiveHash(seed, x, y) % 100000) / 100000f;
            float cumulative = 0f;
            if (!string.IsNullOrEmpty(level.TileId))
            {
                cumulative += level.TileChance;
                if (roll < cumulative)
                    return level.TileId;
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

            return level.TileId;
        }

        private bool IsInsideSelectedVision(int x, int y)
        {
            int radius = ResolveSelectedVisionRange();
            int dx = x - _generatorPreviewUnitPosition.x;
            int dy = y - _generatorPreviewUnitPosition.y;
            float limit = (radius + 0.5f) * (radius + 0.5f);
            return dx * dx + dy * dy <= limit;
        }

        private int ResolveSelectedVisionRange()
        {
            return HasSelectedUnit() ? Mathf.Clamp(GetInt(SelectedUnitProperty(), "VisionRange"), 1, 64) : 1;
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
            if (_generatorAsset == null)
                return;

            if (IsObjectOfType(_generatorAsset, DataNoiseSettingsTypeName))
                _noiseSettingsAsset = _generatorAsset;
            if (IsObjectOfType(_generatorAsset, HeightMapSettingsTypeName))
                _heightSettingsAsset = _generatorAsset;

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
            string guid = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
        }

        private static void SaveScriptableObjectPreference(string key, ScriptableObject asset)
        {
            if (asset == null)
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            if (!string.IsNullOrWhiteSpace(guid))
                EditorPrefs.SetString(key, guid);
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

        private struct PreviewWeightedTile
        {
            public string TileId;
            public float Chance;
        }
    }
}