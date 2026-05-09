using System;
using System.IO;
using Kruty1918.Moyva.Bootstrap;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.HomeMenu.Runtime;
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

        private static readonly string[] Tabs = { "Світ", "Розміри", "Старт", "Туман", "Превʼю" };

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
                    RebuildSerializedObjects();

                if (GUILayout.Button(_worldDefaults == null ? "Створити" : "Ping", GUILayout.Width(92f)))
                {
                    if (_worldDefaults == null)
                        _worldDefaults = CreateAsset<WorldCreationDefaultsSO>(DefaultWorldAssetPath);
                    EditorGUIUtility.PingObject(_worldDefaults);
                    RebuildSerializedObjects();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                _bootstrapConfig = (BootstrapInstallerConfigSO)EditorGUILayout.ObjectField("Bootstrap config", _bootstrapConfig, typeof(BootstrapInstallerConfigSO), false);
                if (EditorGUI.EndChangeCheck())
                    RebuildSerializedObjects();

                if (GUILayout.Button(_bootstrapConfig == null ? "Створити" : "Ping", GUILayout.Width(92f)))
                {
                    if (_bootstrapConfig == null)
                        _bootstrapConfig = CreateAsset<BootstrapInstallerConfigSO>(DefaultBootstrapAssetPath);
                    EditorGUIUtility.PingObject(_bootstrapConfig);
                    RebuildSerializedObjects();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _fogSettings = (FogOfWarSettings)EditorGUILayout.ObjectField("Fog settings", _fogSettings, typeof(FogOfWarSettings), false);

                if (GUILayout.Button(_fogSettings == null ? "Знайти" : "Ping", GUILayout.Width(92f)))
                {
                    if (_fogSettings == null)
                        _fogSettings = FindFogSettings();

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

            var gameSettings = _bootstrapSerialized.FindProperty("_gameSettings");
            BeginCard();
            EditorGUILayout.LabelField("Стартова взаємодія", _sectionStyle);
            EditorGUILayout.PropertyField(gameSettings.FindPropertyRelative("DefaultBuildingId"), new GUIContent("Стартова будівля"));
            EditorGUILayout.PropertyField(gameSettings.FindPropertyRelative("InitialResources"), new GUIContent("Стартові ресурси"), true);
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
            _worldDefaults = AssetDatabase.LoadAssetAtPath<WorldCreationDefaultsSO>(DefaultWorldAssetPath);
            _bootstrapConfig = AssetDatabase.LoadAssetAtPath<BootstrapInstallerConfigSO>(DefaultBootstrapAssetPath);
            _fogSettings = FindFogSettings();
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