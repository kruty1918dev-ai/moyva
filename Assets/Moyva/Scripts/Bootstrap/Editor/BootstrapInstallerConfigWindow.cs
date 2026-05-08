using Kruty1918.Moyva.Bootstrap.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Editor
{
    public sealed class BootstrapInstallerConfigWindow : EditorWindow
    {
        private const string DefaultAssetPath = "Assets/Moyva/SO/Bootstrap/BootstrapInstallerConfig.asset";

        private BootstrapInstallerConfigSO _config;
        private SerializedObject _serializedConfig;
        private Vector2 _scroll;
        private int _tabIndex;

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
                "Start Position",
                settings,
                "minMarginFromBorder",
                "relativeMarginFactor");

            DrawStartSection(
                "Fog Reveal Shape",
                settings,
                "revealedCircleRadius",
                "keepCoreFullyVisible",
                "coreVisibleRadiusOverride");

            DrawStartSection(
                "Camera",
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
