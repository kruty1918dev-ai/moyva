using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kruty1918.Moyva.Bootstrap;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.WorldCreation.Editor
{
    public sealed class WorldDefaultsEditorWindow : EditorWindow
    {
        private const string DefaultWorldAssetPath = "Assets/Moyva/SO/WorldCreation/WorldCreationDefaults.asset";
        private const string DefaultBootstrapAssetPath = "Assets/Moyva/SO/Bootstrap/BootstrapInstallerConfig.asset";

        private static readonly string[] Tabs = { "Світ", "Розміри", "Старт", "Туман", "Розміщення", "Превʼю" };

        private WorldCreationDefaultsSO _worldDefaults;
        private BootstrapInstallerConfigSO _bootstrapConfig;
        private FogOfWarSettings _fogSettings;
        private SerializedObject _worldSerialized;
        private SerializedObject _bootstrapSerialized;
        private Vector2 _scroll;
        private int _tab;
        private int _fogPreviewMapSide = 25;
        private int _fogPreviewRadiusOverride;
        private int _fogPreviewCellPixels = 10;

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _mutedStyle;
        private GUIStyle _pillStyle;

        [MenuItem("Moyva/World/Базові налаштування світу")]
        public static void Open()
        {
            var window = GetWindow<WorldDefaultsEditorWindow>();
            window.titleContent = new GUIContent("World Defaults");
            window.minSize = new Vector2(780f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            TryAutoLoadAssets();
            RebuildSerializedObjects();
        }

        private void OnGUI()
        {
            BuildStyles();
            DrawHeader();
            DrawAssetStrip();

            _tab = GUILayout.Toolbar(_tab, Tabs, GUILayout.Height(34f));
            EditorGUILayout.Space(8f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            using (new EditorGUI.DisabledScope(_worldDefaults == null && _bootstrapConfig == null))
            {
                _worldSerialized?.Update();
                _bootstrapSerialized?.Update();

                switch (_tab)
                {
                    case 0: DrawWorldTab(); break;
                    case 1: DrawSizeTab(); break;
                    case 2: DrawStartTab(); break;
                    case 3: DrawFogTab(); break;
                    case 4: DrawPlacementRulesTab(); break;
                    default: DrawPreviewTab(); break;
                }

                ApplyModifiedProperties();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            var rect = GUILayoutUtility.GetRect(0f, 88f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.10f, 0.12f, 0.14f));
            var accent = new Rect(rect.x, rect.yMax - 3f, rect.width, 3f);
            EditorGUI.DrawRect(accent, new Color(0.23f, 0.55f, 0.48f));

            var content = new Rect(rect.x + 18f, rect.y + 14f, rect.width - 36f, rect.height - 20f);
            GUI.Label(new Rect(content.x, content.y, content.width, 30f), "Базові налаштування світу", _titleStyle);
            GUI.Label(new Rect(content.x, content.y + 34f, content.width, 24f), "Пресети мапи, стартові ресурси, назви світу та масштабування туману в одному місці.", _subtitleStyle);
        }

        private void DrawAssetStrip()
        {
            BeginCard();
            EditorGUILayout.LabelField("Assets", _sectionStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _worldDefaults = (WorldCreationDefaultsSO)EditorGUILayout.ObjectField("World defaults", _worldDefaults, typeof(WorldCreationDefaultsSO), false);
                if (EditorGUI.EndChangeCheck())
                {
                    MoyvaProjectEditorContext.Set(_worldDefaults);
                    RebuildSerializedObjects();
                }

                if (GUILayout.Button(_worldDefaults == null ? "Створити" : "Ping", GUILayout.Width(92f)))
                {
                    if (_worldDefaults == null)
                        _worldDefaults = CreateAsset<WorldCreationDefaultsSO>(DefaultWorldAssetPath);
                    MoyvaProjectEditorContext.Set(_worldDefaults);
                    EditorGUIUtility.PingObject(_worldDefaults);
                    RebuildSerializedObjects();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _bootstrapConfig = (BootstrapInstallerConfigSO)EditorGUILayout.ObjectField("Bootstrap config", _bootstrapConfig, typeof(BootstrapInstallerConfigSO), false);
                if (EditorGUI.EndChangeCheck())
                {
                    MoyvaProjectEditorContext.Set(_bootstrapConfig);
                    RebuildSerializedObjects();
                }

                if (GUILayout.Button(_bootstrapConfig == null ? "Створити" : "Ping", GUILayout.Width(92f)))
                {
                    if (_bootstrapConfig == null)
                        _bootstrapConfig = CreateAsset<BootstrapInstallerConfigSO>(DefaultBootstrapAssetPath);
                    MoyvaProjectEditorContext.Set(_bootstrapConfig);
                    EditorGUIUtility.PingObject(_bootstrapConfig);
                    RebuildSerializedObjects();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _fogSettings = (FogOfWarSettings)EditorGUILayout.ObjectField("Fog settings", _fogSettings, typeof(FogOfWarSettings), false);
                if (EditorGUI.EndChangeCheck())
                    MoyvaProjectEditorContext.Set(_fogSettings);

                if (GUILayout.Button(_fogSettings == null ? "Знайти" : "Ping", GUILayout.Width(92f)))
                {
                    if (_fogSettings == null)
                    {
                        _fogSettings = MoyvaProjectEditorContext.GetOrFindFirst<FogOfWarSettings>() ?? FindFogSettings();
                        MoyvaProjectEditorContext.Set(_fogSettings);
                    }

                    if (_fogSettings != null)
                        EditorGUIUtility.PingObject(_fogSettings);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Проставити у відкритій сцені", GUILayout.Width(220f), GUILayout.Height(28f)))
                    AssignAssetsToOpenSceneInstallers();
            }

            EndCard();
        }

        private void DrawWorldTab()
        {
            if (_worldSerialized == null)
            {
                DrawMissingWorldDefaults();
                return;
            }

            BeginCard();
            EditorGUILayout.LabelField("Назва та вибір світу", _sectionStyle);
            Property("DefaultWorldName", "Стартова назва");
            Property("AppendIndexWhenSaveExists", "Додавати індекс, якщо є сейви");
            Property("FirstWorldIndex", "Перший індекс");
            Property("DefaultSizePreset", "Розмір за замовчуванням");
            Property("DefaultMapType", "Тип карти");
            EndCard();

            BeginCard();
            EditorGUILayout.LabelField("Правила гри", _sectionStyle);
            Property("DefaultDifficulty", "Складність");
            Property("DefaultEnableBots", "Боти увімкнені");
            Property("DefaultHumanPlayerCount", "Людей");
            Property("DefaultBotCount", "Ботів");
            Property("DefaultStartingGold", "Стартове золото");
            Property("DefaultStartingFood", "Стартова їжа");
            EndCard();
        }

        private void DrawSizeTab()
        {
            if (_worldSerialized == null)
            {
                DrawMissingWorldDefaults();
                return;
            }

            BeginCard();
            EditorGUILayout.LabelField("Що означає Small / Medium / Large", _sectionStyle);
            DrawPresetDefinition("SmallWorld", "Маленький світ");
            DrawPresetDefinition("MediumWorld", "Середній світ");
            DrawPresetDefinition("LargeWorld", "Великий світ");
            EditorGUILayout.Space(6f);
            Property("DefaultCustomWidth", "Custom ширина");
            Property("DefaultCustomHeight", "Custom висота");
            EndCard();

            BeginCard();
            EditorGUILayout.LabelField("Візуальне порівняння", _sectionStyle);
            DrawWorldSizePreview(GUILayoutUtility.GetRect(0f, 190f, GUILayout.ExpandWidth(true)));
            EndCard();
        }

        private void DrawStartTab()
        {
            if (_bootstrapSerialized == null)
            {
                DrawMissingBootstrapConfig();
                return;
            }

            BeginCard();
            EditorGUILayout.LabelField("Стартова економіка", _sectionStyle);
            EditorGUILayout.HelpBox(
                "Стартові ресурси централізовано редагуються в Economy Designer (Single Source of Truth).",
                MessageType.Info);
            if (GUILayout.Button("Відкрити Economy Designer → Стартова економіка", GUILayout.Height(24f)))
                OpenEconomyDesignerAtStartingEconomy();
            EndCard();

            var startSettings = _bootstrapSerialized.FindProperty("_startingPositionSettings");
            BeginCard();
            EditorGUILayout.LabelField("Позиція та камера", _sectionStyle);
            DrawRelative(startSettings, "minMarginFromBorder", "Мін. відступ від краю");
            DrawRelative(startSettings, "relativeMarginFactor", "Відносний відступ");
            DrawRelative(startSettings, "startMinHeight", "Мін. висота тайла");
            DrawRelative(startSettings, "startMaxHeight", "Макс. висота тайла");
            DrawRelative(startSettings, "requireHeightMapForStart", "Вимагати HeightMap");
            DrawRelative(startSettings, "multiplayerStartSlots", "Слотів старту MP");
            DrawRelative(startSettings, "minAStarDistanceBetweenPlayers", "Мін. дистанція між гравцями");
            DrawRelative(startSettings, "startCandidateAttempts", "Спроб пошуку старту");
            DrawRelative(startSettings, "cameraZ", "Camera Z");
            EndCard();
        }

        private static void OpenEconomyDesignerAtStartingEconomy()
        {
            var type = Type.GetType("Kruty1918.Moyva.Economy.Editor.EconomyDesignerWindow, Kruty1918.Moyva.Economy.Editor");
            if (type == null || !typeof(EditorWindow).IsAssignableFrom(type))
            {
                Debug.LogWarning("[WorldDefaults] EconomyDesignerWindow не знайдено.");
                return;
            }

            EditorWindow.GetWindow(type, false, "Редактор Економіки");
        }

        private void DrawFogTab()
        {
            if (_bootstrapSerialized == null)
            {
                DrawMissingBootstrapConfig();
                return;
            }

            var settings = _bootstrapSerialized.FindProperty("_startingPositionSettings");
            BeginCard();
            EditorGUILayout.LabelField("Стартовий туман", _sectionStyle);
            DrawRelative(settings, "revealShape", "Форма відкриття");
            DrawRelative(settings, "revealedCircleRadius", "Fallback радіус");
            DrawRelative(settings, "minimumExploredTilesBeforeRepair", "Мін. розвіданих тайлів");
            DrawRelative(settings, "keepCoreFullyVisible", "Тримати ядро видимим");
            DrawRelative(settings, "coreVisibleRadiusOverride", "Override ядра");
            DrawRelative(settings, "useMapSizeScaledFog", "Масштабувати за розміром мапи");
            EditorGUILayout.PropertyField(settings.FindPropertyRelative("fogScaleByMapSize"), new GUIContent("Точки масштабування"), true);
            EndCard();

            BeginCard();
            EditorGUILayout.LabelField("Крива розкриття", _sectionStyle);
            DrawFogCurvePreview(GUILayoutUtility.GetRect(0f, 230f, GUILayout.ExpandWidth(true)), settings.FindPropertyRelative("fogScaleByMapSize"));
            EndCard();

            BeginCard();
            EditorGUILayout.LabelField("Превʼю форми", _sectionStyle);
            DrawFogShapePreviewControls(settings);
            DrawFogShapePreview(GUILayoutUtility.GetRect(0f, 320f, GUILayout.ExpandWidth(true)), settings);
            EndCard();
        }

        private void DrawPlacementRulesTab()
        {
            if (_worldSerialized == null)
            {
                DrawMissingWorldDefaults();
                return;
            }

            BeginCard();
            EditorGUILayout.LabelField("Обмеження розміщення", _sectionStyle);

            var graphProperty = _worldSerialized.FindProperty("PlacementRulesGraph");
            EditorGUILayout.PropertyField(graphProperty, new GUIContent("Граф генерації"));

            var tileRegistryProperty = _worldSerialized.FindProperty("TileRegistry");
            EditorGUILayout.PropertyField(tileRegistryProperty, new GUIContent("Реєстр тайлів (опційно)"));

            int hillLevelCount;
            List<string> graphTileIds;
            string graphStatus;
            bool graphValid = TryResolvePlacementGraphData(
                graphProperty?.objectReferenceValue as GraphAsset,
                tileRegistryProperty?.objectReferenceValue as UnityEngine.Object,
                out hillLevelCount,
                out graphTileIds,
                out graphStatus);

            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(graphStatus, graphValid ? MessageType.Info : MessageType.Warning);

            var blockedBuildingLevels = _worldSerialized.FindProperty("BlockedBuildingHillLevelRanges");
            var blockedUnitLevels = _worldSerialized.FindProperty("BlockedUnitHillLevelRanges");
            var blockedBuildingTiles = _worldSerialized.FindProperty("BlockedBuildingTileIds");
            var blockedUnitTiles = _worldSerialized.FindProperty("BlockedUnitTileIds");
            var effectiveTileRegistry = tileRegistryProperty?.objectReferenceValue as TileRegistrySO
                ?? (graphProperty?.objectReferenceValue as GraphAsset)?.TileRegistry;
            var tileVisualLookup = BuildTileVisualLookup(graphTileIds, effectiveTileRegistry);

            using (new EditorGUI.DisabledScope(!graphValid || hillLevelCount <= 0))
            {
                DrawLevelRangesEditor(
                    blockedBuildingLevels,
                    "Будівництво: заборонені діапазони рівнів",
                    hillLevelCount,
                    "На цих рівнях HillGenerator розміщення будівель буде заборонено.");

                DrawLevelRangesEditor(
                    blockedUnitLevels,
                    "Юніти: заборонені діапазони рівнів",
                    hillLevelCount,
                    "На цих рівнях HillGenerator розміщення або рух юнітів буде заборонено.");
            }

            EditorGUILayout.Space(6f);
            using (new EditorGUI.DisabledScope(graphTileIds == null || graphTileIds.Count == 0))
            {
                DrawTileMultiSelectDropdown(
                    blockedBuildingTiles,
                    graphTileIds,
                    "Будівництво: заборонені Tile ID",
                    tileVisualLookup);

                DrawTileMultiSelectDropdown(
                    blockedUnitTiles,
                    graphTileIds,
                    "Юніти: заборонені Tile ID",
                    tileVisualLookup);
            }

            EndCard();
        }

        private static bool TryResolvePlacementGraphData(
            GraphAsset graphAsset,
            UnityEngine.Object overrideTileRegistry,
            out int hillLevelCount,
            out List<string> tileIds,
            out string status)
        {
            hillLevelCount = 0;
            tileIds = new List<string>();

            if (graphAsset == null)
            {
                if (overrideTileRegistry != null)
                {
                    TryCollectTileIdsFromOverride(overrideTileRegistry, tileIds);
                    status = tileIds.Count > 0
                        ? $"GraphAsset не задано — HillGenerator levels недоступні. Tile ID з реєстру: {tileIds.Count}."
                        : "GraphAsset не задано. Реєстр тайлів порожній або не містить Definitions.";
                }
                else
                {
                    status = "Признач GraphAsset генерації або задай Реєстр тайлів напряму.";
                }
                return false;
            }

            if (!TryReadGraphNodes(graphAsset, out var nodes))
            {
                status = "Об'єкт не схожий на GraphAsset (не знайдено список нод).";
                return false;
            }

            if (!TryReadHillLevels(nodes, out hillLevelCount))
            {
                status = "У графі не знайдено HillGeneratorNode з доступною кількістю рівнів.";
                return false;
            }

            TryCollectTileIdsFromGraphSettings(graphAsset, tileIds);
            if (overrideTileRegistry != null)
                tileIds = CollectTileIdsFromSource(overrideTileRegistry);

            if (tileIds.Count == 0)
            {
                status = "HillGenerator знайдено, але TileRegistry не задано ні в GraphAsset, ні напряму.";
                return true;
            }

            status = $"HillGenerator levels: {hillLevelCount}. Доступно Tile ID: {tileIds.Count}.";
            return true;
        }

        private static bool TryReadGraphNodes(GraphAsset graphAsset, out List<UnityEngine.Object> nodes)
        {
            nodes = new List<UnityEngine.Object>();
            var serialized = new SerializedObject(graphAsset);
            var nodesProperty = serialized.FindProperty("_nodes");
            if (nodesProperty == null || !nodesProperty.isArray)
                return false;

            for (int i = 0; i < nodesProperty.arraySize; i++)
            {
                var element = nodesProperty.GetArrayElementAtIndex(i);
                var node = element?.objectReferenceValue;
                if (node != null)
                    nodes.Add(node);
            }

            return true;
        }

        private static bool TryReadHillLevels(IReadOnlyList<UnityEngine.Object> nodes, out int levelCount)
        {
            levelCount = 0;
            if (nodes == null)
                return false;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                    continue;

                var type = node.GetType();
                if (type.Name != "HillGeneratorNode")
                    continue;

                var levelProperty = type.GetProperty("LevelCount", BindingFlags.Public | BindingFlags.Instance);
                if (levelProperty != null && levelProperty.PropertyType == typeof(int))
                {
                    levelCount = Mathf.Max(0, (int)levelProperty.GetValue(node, null));
                    return levelCount > 0;
                }

                var levelField = type.GetField("_levels", BindingFlags.NonPublic | BindingFlags.Instance);
                if (levelField != null && levelField.FieldType == typeof(int))
                {
                    levelCount = Mathf.Max(0, (int)levelField.GetValue(node));
                    return levelCount > 0;
                }
            }

            return false;
        }

        private static void TryCollectTileIdsFromGraphSettings(
            GraphAsset graphAsset,
            List<string> tileIds)
        {
            var uniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (graphAsset?.TileRegistry != null)
                TryExtractTileIdsFromRegistry(graphAsset.TileRegistry, uniqueIds);

            tileIds.Clear();
            tileIds.AddRange(uniqueIds);
            tileIds.Sort(StringComparer.OrdinalIgnoreCase);
        }

        private static void TryCollectTileIdsFromOverride(UnityEngine.Object source, List<string> tileIds)
        {
            tileIds.AddRange(CollectTileIdsFromSource(source));
        }

        private static List<string> CollectTileIdsFromSource(UnityEngine.Object source)
        {
            var uniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (source != null)
                TryExtractTileIdsFromRegistry(source, uniqueIds);
            var result = new List<string>(uniqueIds);
            result.Sort(StringComparer.OrdinalIgnoreCase);
            return result;
        }

        private static bool TryExtractTileIdsFromRegistry(UnityEngine.Object source, HashSet<string> uniqueIds)
        {
            var type = source.GetType();
            if (type.Name != "TileRegistrySO")
                return false;

            var definitionsProperty = type.GetProperty("Definitions", BindingFlags.Instance | BindingFlags.Public);
            if (definitionsProperty == null)
                return true;

            if (definitionsProperty.GetValue(source, null) is not IEnumerable definitions)
                return true;

            foreach (var definition in definitions)
            {
                if (definition == null)
                    continue;

                var idProperty = definition.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public);
                if (idProperty == null)
                    continue;

                string id = idProperty.GetValue(definition, null) as string;
                if (!string.IsNullOrWhiteSpace(id))
                    uniqueIds.Add(id.Trim());
            }

            return true;
        }

        private void DrawLevelRangesEditor(
            SerializedProperty ranges,
            string label,
            int maxLevel,
            string description)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Рівні у HillGenerator: 1..{maxLevel}", _mutedStyle);
            EditorGUILayout.LabelField(description, _mutedStyle);

            int removeIndex = -1;
            for (int i = 0; i < ranges.arraySize; i++)
            {
                var element = ranges.GetArrayElementAtIndex(i);
                var minProperty = element.FindPropertyRelative("MinLevel");
                var maxProperty = element.FindPropertyRelative("MaxLevel");
                if (minProperty == null || maxProperty == null)
                    continue;

                int minValue = Mathf.Clamp(minProperty.intValue, 1, maxLevel);
                int maxValue = Mathf.Clamp(maxProperty.intValue, 1, maxLevel);
                if (maxValue < minValue)
                    maxValue = minValue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Діапазон {i + 1}", GUILayout.Width(92f));
                    minValue = EditorGUILayout.IntSlider(minValue, 1, maxLevel);
                    GUILayout.Label("..", GUILayout.Width(14f));
                    maxValue = EditorGUILayout.IntSlider(maxValue, 1, maxLevel);
                    if (GUILayout.Button("До останнього", GUILayout.Width(98f)))
                        maxValue = maxLevel;
                    if (GUILayout.Button("✕", GUILayout.Width(24f)))
                        removeIndex = i;
                }

                minProperty.intValue = minValue;
                maxProperty.intValue = maxValue;
            }

            if (removeIndex >= 0)
                ranges.DeleteArrayElementAtIndex(removeIndex);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Додати діапазон", GUILayout.Width(148f)))
                {
                    int index = ranges.arraySize;
                    ranges.InsertArrayElementAtIndex(index);
                    var element = ranges.GetArrayElementAtIndex(index);
                    var minProperty = element.FindPropertyRelative("MinLevel");
                    var maxProperty = element.FindPropertyRelative("MaxLevel");
                    if (minProperty != null) minProperty.intValue = 1;
                    if (maxProperty != null) maxProperty.intValue = maxLevel;
                }

                if (GUILayout.Button("Очистити", GUILayout.Width(90f)))
                    ranges.ClearArray();
            }
        }

        private void DrawTileMultiSelectDropdown(
            SerializedProperty selectedTileIds,
            List<string> availableTileIds,
            string label,
            IReadOnlyDictionary<string, TileVisualPreview> tileVisualLookup)
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            string summary = selectedTileIds.arraySize > 0
                ? $"Вибрано: {selectedTileIds.arraySize}"
                : "Нічого не вибрано";

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(summary, _mutedStyle);
                if (GUILayout.Button("Вибрати…", GUILayout.Width(92f)))
                {
                    Rect buttonRect = GUILayoutUtility.GetLastRect();
                    var selectedValues = BuildSelectedValueSet(selectedTileIds);
                    var serializedObject = selectedTileIds.serializedObject;
                    string propertyPath = selectedTileIds.propertyPath;
                    PopupWindow.Show(
                        buttonRect,
                        new TileMultiSelectPopup(
                            availableTileIds,
                            selectedValues,
                            tileVisualLookup,
                            appliedSelection =>
                            {
                                serializedObject.Update();
                                var property = serializedObject.FindProperty(propertyPath);
                                if (property == null || !property.isArray)
                                    return;

                                ApplyStringArray(property, appliedSelection);
                                serializedObject.ApplyModifiedProperties();
                            }));
                }
            }
        }

        private static HashSet<string> BuildSelectedValueSet(SerializedProperty array)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < array.arraySize; i++)
            {
                string value = array.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrWhiteSpace(value))
                    result.Add(value.Trim());
            }

            return result;
        }

        private static void ApplyStringArray(SerializedProperty array, IReadOnlyCollection<string> values)
        {
            array.ClearArray();
            if (values == null || values.Count == 0)
                return;

            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                int index = array.arraySize;
                array.InsertArrayElementAtIndex(index);
                array.GetArrayElementAtIndex(index).stringValue = value;
            }
        }

        private static IReadOnlyDictionary<string, TileVisualPreview> BuildTileVisualLookup(
            IReadOnlyList<string> availableTileIds,
            TileRegistrySO tileRegistry)
        {
            var lookup = new Dictionary<string, TileVisualPreview>(StringComparer.OrdinalIgnoreCase);
            if (availableTileIds == null || tileRegistry?.Definitions == null)
                return lookup;
            var availableSet = new HashSet<string>(availableTileIds, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < tileRegistry.Definitions.Length; i++)
            {
                var definition = tileRegistry.Definitions[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;
                if (!availableSet.Contains(definition.Id))
                    continue;

                var renderer = definition.VisualPrefab != null
                    ? definition.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true)
                    : null;

                lookup[definition.Id] = new TileVisualPreview
                {
                    Sprite = renderer?.sprite,
                    Tint = renderer != null ? renderer.color : Color.white
                };
            }

            return lookup;
        }

        private readonly struct TileVisualPreview
        {
            public Sprite Sprite { get; init; }
            public Color Tint { get; init; }
        }

        private sealed class TileMultiSelectPopup : PopupWindowContent
        {
            private const float PopupWidth = 560f;
            private const float PopupHeight = 460f;
            private const float SearchHeight = 20f;
            private const float RowHeight = 24f;
            private const float IconSize = 18f;
            private const float Padding = 6f;
            private const string SearchControlName = "WorldDefaultsTileMultiSelectSearch";

            private readonly List<string> _availableTileIds;
            private readonly HashSet<string> _selectedIds;
            private readonly IReadOnlyDictionary<string, TileVisualPreview> _tileVisualLookup;
            private readonly Action<IReadOnlyCollection<string>> _onApply;

            private Vector2 _scroll;
            private string _search = string.Empty;
            private bool _focusSearch = true;
            private string _cachedFilterKey = string.Empty;
            private List<string> _cachedFilteredIds;

            public TileMultiSelectPopup(
                IEnumerable<string> availableTileIds,
                IEnumerable<string> selectedIds,
                IReadOnlyDictionary<string, TileVisualPreview> tileVisualLookup,
                Action<IReadOnlyCollection<string>> onApply)
            {
                _availableTileIds = availableTileIds != null
                    ? new List<string>(availableTileIds)
                    : new List<string>();
                _selectedIds = selectedIds != null
                    ? new HashSet<string>(selectedIds, StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _tileVisualLookup = tileVisualLookup ?? new Dictionary<string, TileVisualPreview>(StringComparer.OrdinalIgnoreCase);
                _onApply = onApply;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);

            public override void OnOpen()
            {
                _focusSearch = true;
            }

            public override void OnClose()
            {
                _onApply?.Invoke(_selectedIds);
            }

            public override void OnGUI(Rect rect)
            {
                DrawSearch(rect);
                DrawFilterActions(rect);
                DrawList(rect);
            }

            private void DrawSearch(Rect rect)
            {
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchHeight);
                GUI.SetNextControlName(SearchControlName);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck())
                {
                    _scroll = Vector2.zero;
                    _cachedFilteredIds = null;
                }

                if (_focusSearch)
                {
                    _focusSearch = false;
                    EditorGUI.FocusTextInControl(SearchControlName);
                }
            }

            private void DrawFilterActions(Rect rect)
            {
                float y = Padding + SearchHeight + Padding;
                Rect selectAllRect = new Rect(Padding, y, 162f, RowHeight);
                Rect clearRect = new Rect(selectAllRect.xMax + 8f, y, 152f, RowHeight);
                if (GUI.Button(selectAllRect, "Вибрати все (фільтр)"))
                {
                    foreach (string id in GetFilteredIds())
                        _selectedIds.Add(id);
                }

                if (GUI.Button(clearRect, "Очистити (фільтр)"))
                {
                    foreach (string id in GetFilteredIds())
                        _selectedIds.Remove(id);
                }
            }

            private void DrawList(Rect rect)
            {
                float y = Padding + SearchHeight + Padding + RowHeight + Padding;
                Rect viewRect = new Rect(
                    Padding,
                    y,
                    rect.width - Padding * 2f,
                    rect.height - y - Padding);

                var filtered = GetFilteredIds();
                if (filtered.Count == 0)
                {
                    EditorGUI.HelpBox(viewRect, "Нічого не знайдено за поточним фільтром.", MessageType.Info);
                    return;
                }

                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f, Mathf.Max(viewRect.height, filtered.Count * RowHeight));
                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
                for (int i = 0; i < filtered.Count; i++)
                {
                    Rect rowRect = new Rect(0f, i * RowHeight, contentRect.width, RowHeight - 1f);
                    DrawRow(rowRect, filtered[i]);
                }
                GUI.EndScrollView();
            }

            private List<string> GetFilteredIds()
            {
                string filterKey = _search ?? string.Empty;
                if (_cachedFilteredIds != null && string.Equals(_cachedFilterKey, filterKey, StringComparison.Ordinal))
                    return _cachedFilteredIds;

                _cachedFilterKey = filterKey;
                if (string.IsNullOrWhiteSpace(filterKey))
                {
                    _cachedFilteredIds = _availableTileIds;
                    return _cachedFilteredIds;
                }

                var result = new List<string>();
                string[] terms = filterKey.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < _availableTileIds.Count; i++)
                {
                    string id = _availableTileIds[i];
                    bool matches = true;
                    for (int t = 0; t < terms.Length; t++)
                    {
                        if (id.IndexOf(terms[t], StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                        result.Add(id);
                }

                _cachedFilteredIds = result;
                return _cachedFilteredIds;
            }

            private void DrawRow(Rect rowRect, string id)
            {
                bool selected = _selectedIds.Contains(id);
                if (selected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));
                else if (rowRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowRect, new Color(0.20f, 0.20f, 0.20f, 0.45f));

                Rect toggleRect = new Rect(rowRect.x + 4f, rowRect.y + 2f, 18f, RowHeight - 4f);
                bool next = EditorGUI.Toggle(toggleRect, selected);
                if (next != selected)
                {
                    if (next)
                        _selectedIds.Add(id);
                    else
                        _selectedIds.Remove(id);
                }

                Rect iconRect = new Rect(toggleRect.xMax + 4f, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
                if (_tileVisualLookup.TryGetValue(id, out var preview) && preview.Sprite != null)
                    DrawSprite(iconRect, preview.Sprite, preview.Tint);

                Rect labelRect = new Rect(iconRect.xMax + 6f, rowRect.y + 2f, rowRect.width - iconRect.width - 34f, RowHeight - 2f);
                GUI.Label(labelRect, id, EditorStyles.label);
            }

            private static void DrawSprite(Rect rect, Sprite sprite, Color tint)
            {
                var texture = sprite.texture;
                var sr = sprite.textureRect;
                var uv = new Rect(sr.x / texture.width, sr.y / texture.height, sr.width / texture.width, sr.height / texture.height);
                Color previous = GUI.color;
                GUI.color = tint;
                GUI.DrawTextureWithTexCoords(rect, texture, uv);
                GUI.color = previous;
            }
        }

        private static bool ArrayContains(SerializedProperty array, string value)
        {
            for (int i = 0; i < array.arraySize; i++)
            {
                if (string.Equals(array.GetArrayElementAtIndex(i).stringValue, value, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void EnsureStringInArray(SerializedProperty array, string value)
        {
            if (ArrayContains(array, value))
                return;

            int index = array.arraySize;
            array.InsertArrayElementAtIndex(index);
            array.GetArrayElementAtIndex(index).stringValue = value;
        }

        private static void RemoveStringFromArray(SerializedProperty array, string value)
        {
            for (int i = array.arraySize - 1; i >= 0; i--)
            {
                if (string.Equals(array.GetArrayElementAtIndex(i).stringValue, value, StringComparison.OrdinalIgnoreCase))
                    array.DeleteArrayElementAtIndex(i);
            }
        }

        private void DrawPreviewTab()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                BeginCard();
                EditorGUILayout.LabelField("Мапи", _sectionStyle);
                DrawWorldSizePreview(GUILayoutUtility.GetRect(0f, 260f, GUILayout.MinWidth(340f), GUILayout.ExpandWidth(true)));
                EndCard();

                BeginCard();
                EditorGUILayout.LabelField("Туман", _sectionStyle);
                var fogPoints = _bootstrapSerialized?.FindProperty("_startingPositionSettings")?.FindPropertyRelative("fogScaleByMapSize");
                DrawFogCurvePreview(GUILayoutUtility.GetRect(0f, 260f, GUILayout.MinWidth(340f), GUILayout.ExpandWidth(true)), fogPoints);
                var startSettings = _bootstrapSerialized?.FindProperty("_startingPositionSettings");
                if (startSettings != null)
                    DrawFogShapePreview(GUILayoutUtility.GetRect(0f, 260f, GUILayout.MinWidth(340f), GUILayout.ExpandWidth(true)), startSettings);
                EndCard();
            }
        }

        private void DrawPresetDefinition(string propertyName, string label)
        {
            var preset = _worldSerialized.FindProperty(propertyName);
            if (preset == null)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(160f));
                EditorGUILayout.PropertyField(preset.FindPropertyRelative("Width"), GUIContent.none, GUILayout.MinWidth(100f));
                GUILayout.Label("x", _mutedStyle, GUILayout.Width(18f));
                EditorGUILayout.PropertyField(preset.FindPropertyRelative("Height"), GUIContent.none, GUILayout.MinWidth(100f));
                GUILayout.Label("tiles", _mutedStyle, GUILayout.Width(46f));
            }
        }

        private void DrawWorldSizePreview(Rect rect)
        {
            if (_worldDefaults == null)
            {
                DrawCenteredText(rect, "World defaults asset не призначено.");
                return;
            }

            EditorGUI.DrawRect(rect, new Color(0.15f, 0.17f, 0.18f));
            var inner = new Rect(rect.x + 18f, rect.y + 18f, rect.width - 36f, rect.height - 36f);
            int maxSide = Mathf.Max(1, Mathf.Max(_worldDefaults.LargeWorld.Width, _worldDefaults.LargeWorld.Height));
            DrawWorldBox(inner, _worldDefaults.SmallWorld, maxSide, "Small", new Color(0.24f, 0.61f, 0.55f));
            DrawWorldBox(inner, _worldDefaults.MediumWorld, maxSide, "Medium", new Color(0.74f, 0.64f, 0.36f));
            DrawWorldBox(inner, _worldDefaults.LargeWorld, maxSide, "Large", new Color(0.76f, 0.38f, 0.32f));
        }

        private void DrawWorldBox(Rect bounds, WorldSizePresetDefinition preset, int maxSide, string label, Color color)
        {
            if (preset == null)
                return;

            float maxPreview = Mathf.Min(bounds.width, bounds.height) * 0.82f;
            float width = Mathf.Max(20f, maxPreview * preset.Width / maxSide);
            float height = Mathf.Max(20f, maxPreview * preset.Height / maxSide);
            var rect = new Rect(bounds.center.x - width * 0.5f, bounds.center.y - height * 0.5f, width, height);
            Handles.DrawSolidRectangleWithOutline(rect, new Color(color.r, color.g, color.b, 0.18f), color);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 34f), $"{label}\n{preset.Width} x {preset.Height}", _pillStyle);
        }

        private void DrawFogCurvePreview(Rect rect, SerializedProperty points)
        {
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.17f, 0.18f));
            if (points == null || points.arraySize == 0)
            {
                DrawCenteredText(rect, "Немає точок масштабування туману.");
                return;
            }

            var graph = new Rect(rect.x + 42f, rect.y + 20f, rect.width - 62f, rect.height - 48f);
            Handles.color = new Color(1f, 1f, 1f, 0.18f);
            Handles.DrawLine(new Vector3(graph.x, graph.yMax), new Vector3(graph.xMax, graph.yMax));
            Handles.DrawLine(new Vector3(graph.x, graph.y), new Vector3(graph.x, graph.yMax));

            int minTiles = int.MaxValue;
            int maxTiles = int.MinValue;
            int maxRadius = 1;
            for (int i = 0; i < points.arraySize; i++)
            {
                var point = points.GetArrayElementAtIndex(i);
                int tiles = point.FindPropertyRelative("MapSideTiles").intValue;
                int radius = point.FindPropertyRelative("RevealedRadius").intValue;
                minTiles = Mathf.Min(minTiles, tiles);
                maxTiles = Mathf.Max(maxTiles, tiles);
                maxRadius = Mathf.Max(maxRadius, radius);
            }

            minTiles = minTiles == int.MaxValue ? 16 : minTiles;
            maxTiles = Mathf.Max(minTiles + 1, maxTiles);

            Vector2 previous = Vector2.zero;
            bool hasPrevious = false;
            Handles.color = new Color(0.38f, 0.78f, 0.70f);
            for (int i = 0; i < points.arraySize; i++)
            {
                var point = points.GetArrayElementAtIndex(i);
                int tiles = point.FindPropertyRelative("MapSideTiles").intValue;
                int radius = point.FindPropertyRelative("RevealedRadius").intValue;
                var current = new Vector2(
                    Mathf.Lerp(graph.x, graph.xMax, Mathf.InverseLerp(minTiles, maxTiles, tiles)),
                    Mathf.Lerp(graph.yMax, graph.y, radius / (float)maxRadius));

                if (hasPrevious)
                    Handles.DrawLine(previous, current, 2f);

                Handles.DrawSolidDisc(current, Vector3.forward, 4f);
                GUI.Label(new Rect(current.x + 6f, current.y - 18f, 110f, 18f), $"{tiles}: {radius}", _mutedStyle);
                previous = current;
                hasPrevious = true;
            }

            GUI.Label(new Rect(graph.x, graph.yMax + 8f, graph.width, 20f), "X: менша сторона мапи у тайлах, Y: радіус стартового відкриття", _mutedStyle);
        }

        private void DrawFogShapePreviewControls(SerializedProperty settings)
        {
            if (settings == null)
                return;

            int configuredRadius = Mathf.Max(1, settings.FindPropertyRelative("revealedCircleRadius")?.intValue ?? 8);
            _fogPreviewMapSide = EditorGUILayout.IntSlider("Preview map side", Mathf.Max(5, _fogPreviewMapSide), 9, 65);
            _fogPreviewRadiusOverride = EditorGUILayout.IntSlider("Preview radius override", _fogPreviewRadiusOverride, 0, Mathf.Max(1, _fogPreviewMapSide / 2));
            _fogPreviewCellPixels = EditorGUILayout.IntSlider("Preview cell size", Mathf.Max(4, _fogPreviewCellPixels), 4, 22);

            int radius = ResolveFogPreviewRadius(configuredRadius);
            EditorGUILayout.LabelField("Preview", $"{_fogPreviewMapSide} x {_fogPreviewMapSide}, radius {radius}, sprite: {ResolveFogSpriteName()}", _mutedStyle);
        }

        private void DrawFogShapePreview(Rect rect, SerializedProperty settings)
        {
            EditorGUI.DrawRect(rect, new Color(0.11f, 0.13f, 0.14f));
            if (settings == null)
            {
                DrawCenteredText(rect, "Bootstrap fog settings не призначено.");
                return;
            }

            var shapeProperty = settings.FindPropertyRelative("revealShape");
            FogRevealShape shape = shapeProperty != null
                ? (FogRevealShape)shapeProperty.enumValueIndex
                : FogRevealShape.PixelCircle;

            int configuredRadius = Mathf.Max(1, settings.FindPropertyRelative("revealedCircleRadius")?.intValue ?? 8);
            int radius = ResolveFogPreviewRadius(configuredRadius);
            int side = Mathf.Max(radius * 2 + 3, _fogPreviewMapSide);
            if (side % 2 == 0)
                side++;

            float cell = Mathf.Max(4f, _fogPreviewCellPixels);
            float gridPixels = side * cell;
            float scale = Mathf.Min(1f, Mathf.Min((rect.width - 28f) / gridPixels, (rect.height - 48f) / gridPixels));
            cell *= Mathf.Clamp01(scale);
            var gridRect = new Rect(rect.center.x - side * cell * 0.5f, rect.y + 18f, side * cell, side * cell);

            DrawFogPreviewGrid(gridRect, side, radius, shape);
            GUI.Label(new Rect(rect.x + 12f, gridRect.yMax + 8f, rect.width - 24f, 22f), $"{shape} · {ResolveFogSpriteName()}", _mutedStyle);
        }

        private void DrawFogPreviewGrid(Rect gridRect, int side, int radius, FogRevealShape shape)
        {
            var fogSprite = _fogSettings != null ? _fogSettings.FogTileSprite : null;
            var texture = fogSprite != null ? fogSprite.texture : null;
            var pixelSize = _fogSettings != null ? _fogSettings.FogTileSpritePixelSize : new Vector2Int(16, 16);
            var uv = BuildSpriteUvRect(fogSprite, texture, pixelSize);
            var center = side / 2;
            float cell = gridRect.width / side;

            for (int x = 0; x < side; x++)
            {
                for (int y = 0; y < side; y++)
                {
                    bool revealed = IsInsideFogPreviewShape(x - center, y - center, radius, shape);
                    var cellRect = new Rect(gridRect.x + x * cell, gridRect.y + (side - 1 - y) * cell, cell, cell);
                    EditorGUI.DrawRect(cellRect, revealed ? new Color(0.22f, 0.37f, 0.30f, 0.9f) : new Color(0.03f, 0.04f, 0.05f, 1f));

                    if (!revealed)
                        DrawFogPreviewSprite(cellRect, texture, uv);
                }
            }

            Handles.color = new Color(1f, 1f, 1f, 0.08f);
            for (int i = 0; i <= side; i++)
            {
                float px = gridRect.x + i * cell;
                float py = gridRect.y + i * cell;
                Handles.DrawLine(new Vector3(px, gridRect.y), new Vector3(px, gridRect.yMax));
                Handles.DrawLine(new Vector3(gridRect.x, py), new Vector3(gridRect.xMax, py));
            }
        }

        private void DrawFogPreviewSprite(Rect rect, Texture2D texture, Rect uv)
        {
            if (texture == null)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.82f));
                return;
            }

            Color previous = GUI.color;
            var tint = _fogSettings != null ? _fogSettings.UnexploredColor : Color.black;
            tint.a = Mathf.Clamp01(_fogSettings != null ? _fogSettings.UnexploredAlpha : 1f);
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(rect, texture, uv, true);
            GUI.color = previous;
        }

        private int ResolveFogPreviewRadius(int configuredRadius)
        {
            return Mathf.Max(1, _fogPreviewRadiusOverride > 0 ? _fogPreviewRadiusOverride : configuredRadius);
        }

        private string ResolveFogSpriteName()
        {
            if (_fogSettings == null)
                return "FogOfWarSettings не призначено";

            return _fogSettings.FogTileSprite != null
                ? _fogSettings.FogTileSprite.name
                : "FogTileSprite не призначено";
        }

        private static bool IsInsideFogPreviewShape(int dx, int dy, int radius, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= radius * radius,
            };
        }

        private static Rect BuildSpriteUvRect(Sprite sprite, Texture2D texture, Vector2Int pixelSize)
        {
            if (sprite == null || texture == null)
                return new Rect(0f, 0f, 1f, 1f);

            Rect textureRect = sprite.textureRect;
            float width = Mathf.Clamp(Mathf.Max(1, pixelSize.x), 1f, texture.width - textureRect.x);
            float height = Mathf.Clamp(Mathf.Max(1, pixelSize.y), 1f, texture.height - textureRect.y);
            return new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                width / texture.width,
                height / texture.height);
        }

        private void Property(string propertyName, string label)
        {
            var property = _worldSerialized.FindProperty(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        private static void DrawRelative(SerializedProperty parent, string propertyName, string label)
        {
            var property = parent?.FindPropertyRelative(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        private void DrawMissingWorldDefaults()
        {
            BeginCard();
            EditorGUILayout.HelpBox("Признач або створи WorldCreationDefaultsSO, щоб налаштовувати дефолти світу.", MessageType.Info);
            EndCard();
        }

        private void DrawMissingBootstrapConfig()
        {
            BeginCard();
            EditorGUILayout.HelpBox("Признач або створи BootstrapInstallerConfigSO, щоб налаштовувати стартові ресурси та туман.", MessageType.Info);
            EndCard();
        }

        private void AssignAssetsToOpenSceneInstallers()
        {
            int assigned = 0;

            foreach (var installer in UnityEngine.Object.FindObjectsByType<HomeMenuInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(installer);
                var property = serialized.FindProperty("_worldCreationDefaults");
                if (property != null && _worldDefaults != null)
                {
                    property.objectReferenceValue = _worldDefaults;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(installer);
                    assigned++;
                }
            }

            foreach (var installer in UnityEngine.Object.FindObjectsByType<BootstrapInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(installer);
                var property = serialized.FindProperty("_config");
                if (property != null && _bootstrapConfig != null)
                {
                    property.objectReferenceValue = _bootstrapConfig;
                    serialized.ApplyModifiedProperties();
                    EditorUtility.SetDirty(installer);
                    assigned++;
                }
            }

            if (assigned > 0)
                EditorSceneManager.MarkAllScenesDirty();

            ShowNotification(new GUIContent(assigned > 0 ? $"Призначено references: {assigned}" : "У відкритих сценах installer-и не знайдено"));
        }

        private void TryAutoLoadAssets()
        {
            _worldDefaults = MoyvaProjectEditorContext.Get<WorldCreationDefaultsSO>()
                ?? AssetDatabase.LoadAssetAtPath<WorldCreationDefaultsSO>(DefaultWorldAssetPath);
            _bootstrapConfig = MoyvaProjectEditorContext.Get<BootstrapInstallerConfigSO>()
                ?? AssetDatabase.LoadAssetAtPath<BootstrapInstallerConfigSO>(DefaultBootstrapAssetPath);
            _fogSettings = MoyvaProjectEditorContext.Get<FogOfWarSettings>() ?? FindFogSettings();
        }

        private static FogOfWarSettings FindFogSettings()
        {
            foreach (var installer in UnityEngine.Object.FindObjectsByType<FogOfWarInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(installer);
                var property = serialized.FindProperty("_settings");
                if (property?.objectReferenceValue is FogOfWarSettings sceneSettings)
                    return sceneSettings;
            }

            foreach (var quad in UnityEngine.Object.FindObjectsByType<FogQuadController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(quad);
                var property = serialized.FindProperty("_settings");
                if (property?.objectReferenceValue is FogOfWarSettings sceneSettings)
                    return sceneSettings;
            }

            string[] guids = AssetDatabase.FindAssets("t:FogOfWarSettings");
            for (int i = 0; i < guids.Length; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<FogOfWarSettings>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (asset != null)
                    return asset;
            }

            return null;
        }

        private void RebuildSerializedObjects()
        {
            _worldSerialized = _worldDefaults != null ? new SerializedObject(_worldDefaults) : null;
            _bootstrapSerialized = _bootstrapConfig != null ? new SerializedObject(_bootstrapConfig) : null;
        }

        private void ApplyModifiedProperties()
        {
            if (_worldSerialized != null && _worldSerialized.ApplyModifiedProperties())
                EditorUtility.SetDirty(_worldDefaults);

            if (_bootstrapSerialized != null && _bootstrapSerialized.ApplyModifiedProperties())
                EditorUtility.SetDirty(_bootstrapConfig);
        }

        private static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private void BeginCard()
        {
            EditorGUILayout.BeginVertical(_cardStyle);
        }

        private static void EndCard()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8f);
        }

        private void BuildStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                normal = { textColor = Color.white }
            };

            _subtitleStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.82f, 0.84f) }
            };

            _sectionStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.92f, 0.94f, 0.92f) }
            };

            _mutedStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.72f, 0.76f, 0.76f) }
            };

            _pillStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                wordWrap = true,
                normal = { textColor = new Color(0.90f, 0.94f, 0.92f) }
            };

            if (_cardStyle == null)
            {
                _cardStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(14, 14, 12, 12),
                    margin = new RectOffset(8, 8, 4, 8)
                };
            }
        }

        private static void DrawCenteredText(Rect rect, string text)
        {
            GUI.Label(rect, text, new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter });
        }
    }
}
