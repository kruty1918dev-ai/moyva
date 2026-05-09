using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    public sealed class BootstrapInstallerConfigWindow : EditorWindow
    {
        private const string DefaultAssetPath = "Assets/Moyva/SO/Bootstrap/BootstrapInstallerConfig.asset";

        private BootstrapInstallerConfigSO _config;
        private FogOfWarSettings _fogSettings;
        private SerializedObject _serializedConfig;
        private Vector2 _scroll;
        private int _tabIndex;
        private int _fogPreviewMapSide = 25;
        private int _fogPreviewRadiusOverride;
        private int _fogPreviewCellPixels = 10;

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _tabStyle;

        private static readonly string[] Tabs = { "Гра", "Стартова позиція" };

        [MenuItem("Moyva/Bootstrap/Installer Config Editor")]
        private static void OpenWindow()
        {
            var window = GetWindow<BootstrapInstallerConfigWindow>("Bootstrap Config");
            window.minSize = new Vector2(580f, 480f);
            window.Show();
        }

        private void OnEnable()
        {
            if (_config == null)
                TryAssignFromSelection();

            _fogSettings ??= FindFogSettings();

            EnsureSerializedObject();
            BuildStyles();
        }

        private void OnSelectionChange()
        {
            if (_config != null)
                return;

            TryAssignFromSelection();
            Repaint();
        }

        private void OnGUI()
        {
            BuildStyles();
            DrawToolbar();

            if (_config == null)
            {
                DrawNoConfigState();
                return;
            }

            EnsureSerializedObject();
            if (_serializedConfig == null)
                return;

            _serializedConfig.Update();

            GUILayout.Space(8f);
            EditorGUILayout.LabelField("Bootstrap Installer Config", _titleStyle);
            EditorGUILayout.LabelField("Налаштування bootstrap без інлайн-інспектора, у виділеному вікні", _subtitleStyle);
            GUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                _tabIndex = GUILayout.Toolbar(_tabIndex, Tabs, _tabStyle, GUILayout.MaxWidth(420f), GUILayout.Height(28f));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(6f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(6f);

            if (_tabIndex == 0)
                DrawGameTab();
            else
                DrawStartPositionTab();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save Assets", GUILayout.Width(120f)))
                {
                    _serializedConfig.ApplyModifiedProperties();
                    EditorUtility.SetDirty(_config);
                    AssetDatabase.SaveAssets();
                }
            }

            _serializedConfig.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var newConfig = (BootstrapInstallerConfigSO)EditorGUILayout.ObjectField(
                    _config,
                    typeof(BootstrapInstallerConfigSO),
                    false,
                    GUILayout.MinWidth(240f));

                if (newConfig != _config)
                {
                    _config = newConfig;
                    EnsureSerializedObject();
                }

                if (GUILayout.Button("Use Selected", EditorStyles.toolbarButton, GUILayout.Width(92f)))
                {
                    TryAssignFromSelection();
                    EnsureSerializedObject();
                }

                if (GUILayout.Button("Create", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                {
                    CreateConfigAsset();
                }

                if (GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(55f)) && _config != null)
                {
                    EditorGUIUtility.PingObject(_config);
                    Selection.activeObject = _config;
                }
            }
        }

        private void DrawNoConfigState()
        {
            EditorGUILayout.HelpBox(
                "Призначте BootstrapInstallerConfigSO у верхньому полі або створіть новий через кнопку Create.",
                MessageType.Info);
        }

        private void DrawGameTab()
        {
            var settings = _serializedConfig.FindProperty("_gameSettings");
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Property '_gameSettings' не знайдено.", MessageType.Error);
                return;
            }

            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("Default Building", EditorStyles.boldLabel);
                EditorGUILayout.Space(3f);
                var defaultBuildingId = settings.FindPropertyRelative("DefaultBuildingId");
                EditorGUILayout.PropertyField(defaultBuildingId, new GUIContent("Building ID"));
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("Initial Resources", EditorStyles.boldLabel);
                EditorGUILayout.Space(3f);
                var resources = settings.FindPropertyRelative("InitialResources");
                EditorGUILayout.PropertyField(resources, new GUIContent("Ресурси на старті"), includeChildren: true);
            }
        }

        private void DrawStartPositionTab()
        {
            var settings = _serializedConfig.FindProperty("_startingPositionSettings");
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Property '_startingPositionSettings' не знайдено.", MessageType.Error);
                return;
            }

            DrawStartSection(
                "Стартова позиція",
                settings,
                "minMarginFromBorder",
                "relativeMarginFactor");

            DrawStartSection(
                "Висота стартового тайла",
                settings,
                "startMinHeight",
                "startMaxHeight",
                "requireHeightMapForStart");

            DrawStartSection(
                "Форма розкриття туману",
                settings,
                "revealShape",
                "revealedCircleRadius",
                "minimumExploredTilesBeforeRepair",
                "keepCoreFullyVisible",
                "coreVisibleRadiusOverride");

            DrawFogPreviewSection(settings);

            DrawStartSection(
                "Стартові позиції мультиплеєра",
                settings,
                "multiplayerStartSlots",
                "minAStarDistanceBetweenPlayers",
                "startCandidateAttempts");

            DrawStartSection(
                "Камера",
                settings,
                "cameraZ");
        }

        private void DrawStartSection(string title, SerializedProperty settings, params string[] propertyNames)
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.Space(3f);
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    var prop = settings.FindPropertyRelative(propertyNames[i]);
                    if (prop == null)
                    {
                        EditorGUILayout.HelpBox($"Property '{propertyNames[i]}' не знайдено.", MessageType.Warning);
                        continue;
                    }

                    EditorGUILayout.PropertyField(prop, includeChildren: true);
                }
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawFogPreviewSection(SerializedProperty settings)
        {
            using (new EditorGUILayout.VerticalScope(_cardStyle))
            {
                EditorGUILayout.LabelField("Превʼю форми туману", EditorStyles.boldLabel);
                EditorGUILayout.Space(3f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _fogSettings = (FogOfWarSettings)EditorGUILayout.ObjectField("Fog settings", _fogSettings, typeof(FogOfWarSettings), false);
                    if (GUILayout.Button(_fogSettings == null ? "Знайти" : "Ping", GUILayout.Width(70f)))
                    {
                        if (_fogSettings == null)
                            _fogSettings = FindFogSettings();

                        if (_fogSettings != null)
                            EditorGUIUtility.PingObject(_fogSettings);
                    }
                }

                int configuredRadius = Mathf.Max(1, settings.FindPropertyRelative("revealedCircleRadius")?.intValue ?? 8);
                _fogPreviewMapSide = EditorGUILayout.IntSlider("Preview map side", Mathf.Max(5, _fogPreviewMapSide), 9, 65);
                _fogPreviewRadiusOverride = EditorGUILayout.IntSlider("Preview radius override", _fogPreviewRadiusOverride, 0, Mathf.Max(1, _fogPreviewMapSide / 2));
                _fogPreviewCellPixels = EditorGUILayout.IntSlider("Preview cell size", Mathf.Max(4, _fogPreviewCellPixels), 4, 22);

                var rect = GUILayoutUtility.GetRect(0f, 260f, GUILayout.ExpandWidth(true));
                DrawFogShapePreview(rect, settings, ResolveFogPreviewRadius(configuredRadius));
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawFogShapePreview(Rect rect, SerializedProperty settings, int radius)
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

            int side = Mathf.Max(radius * 2 + 3, _fogPreviewMapSide);
            if (side % 2 == 0)
                side++;

            float cell = Mathf.Max(4f, _fogPreviewCellPixels);
            float gridPixels = side * cell;
            float scale = Mathf.Min(1f, Mathf.Min((rect.width - 24f) / gridPixels, (rect.height - 44f) / gridPixels));
            cell *= Mathf.Max(0.1f, scale);

            var gridRect = new Rect(rect.center.x - side * cell * 0.5f, rect.y + 14f, side * cell, side * cell);
            DrawFogPreviewGrid(gridRect, side, radius, shape);
            GUI.Label(new Rect(rect.x + 10f, gridRect.yMax + 8f, rect.width - 20f, 22f), $"{shape} · {ResolveFogSpriteName()}", EditorStyles.miniLabel);
        }

        private void DrawFogPreviewGrid(Rect gridRect, int side, int radius, FogRevealShape shape)
        {
            var fogSprite = _fogSettings != null ? _fogSettings.FogTileSprite : null;
            var texture = fogSprite != null ? fogSprite.texture : null;
            var pixelSize = _fogSettings != null ? _fogSettings.FogTileSpritePixelSize : new Vector2Int(16, 16);
            var uv = BuildSpriteUvRect(fogSprite, texture, pixelSize);
            int center = side / 2;
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

            Rect spriteRect = sprite.textureRect;
            float width = Mathf.Clamp(Mathf.Max(1, pixelSize.x), 1f, texture.width - spriteRect.x);
            float height = Mathf.Clamp(Mathf.Max(1, pixelSize.y), 1f, texture.height - spriteRect.y);
            return new Rect(
                spriteRect.x / texture.width,
                spriteRect.y / texture.height,
                width / texture.width,
                height / texture.height);
        }

        private static FogOfWarSettings FindFogSettings()
        {
            foreach (var installer in Object.FindObjectsByType<FogOfWarInstaller>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(installer);
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

        private static void DrawCenteredText(Rect rect, string text)
        {
            GUI.Label(rect, text, new GUIStyle(EditorStyles.centeredGreyMiniLabel) { alignment = TextAnchor.MiddleCenter });
        }

        private void TryAssignFromSelection()
        {
            if (Selection.activeObject is BootstrapInstallerConfigSO selected)
                _config = selected;
        }

        private void EnsureSerializedObject()
        {
            _serializedConfig = _config != null ? new SerializedObject(_config) : null;
        }

        private void BuildStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
            };

            _subtitleStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                fontSize = 11,
            };

            _cardStyle ??= new GUIStyle("HelpBox")
            {
                padding = new RectOffset(10, 10, 8, 10),
                margin = new RectOffset(8, 8, 4, 4),
            };

            _tabStyle ??= new GUIStyle(EditorStyles.toolbarButton)
            {
                fontSize = 12,
                fixedHeight = 28f,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private void CreateConfigAsset()
        {
            EnsureFolder("Assets/Moyva/SO");
            EnsureFolder("Assets/Moyva/SO/Bootstrap");

            var created = ScriptableObject.CreateInstance<BootstrapInstallerConfigSO>();
            string path = AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath);
            AssetDatabase.CreateAsset(created, path);
            AssetDatabase.SaveAssets();

            _config = created;
            EnsureSerializedObject();

            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
