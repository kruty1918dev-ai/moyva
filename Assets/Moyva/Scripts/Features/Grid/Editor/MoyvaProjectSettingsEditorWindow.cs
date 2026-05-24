using System.IO;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    public sealed class MoyvaProjectSettingsEditorWindow : EditorWindow
    {
        private MoyvaProjectSettingsSO _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scroll;

        [MenuItem("Moyva/Project/Global Settings")]
        public static void Open()
        {
            var window = GetWindow<MoyvaProjectSettingsEditorWindow>();
            window.titleContent = new GUIContent("Moyva Project Settings");
            window.minSize = new Vector2(520f, 420f);
            window.Show();
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/Moyva", SettingsScope.Project)
            {
                label = "Moyva",
                guiHandler = _ => DrawSettingsProviderGui(),
                keywords = new[] { "Moyva", "Grid", "Projection", "Isometric", "Hex" },
            };
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            DrawHeader();

            if (_settings == null)
                LoadSettings();

            if (_serializedSettings == null)
            {
                EditorGUILayout.HelpBox("Moyva project settings asset is not available.", MessageType.Warning);
                if (GUILayout.Button("Create Settings Asset"))
                    LoadSettings(createIfMissing: true);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSerializedSettings(_serializedSettings);
            EditorGUILayout.EndScrollView();
        }

        private static void DrawSettingsProviderGui()
        {
            var settings = GetOrCreateSettingsAsset();
            if (settings == null)
            {
                EditorGUILayout.HelpBox("Could not create Moyva project settings asset.", MessageType.Error);
                return;
            }

            DrawSerializedSettings(new SerializedObject(settings));
        }

        private static void DrawSerializedSettings(SerializedObject serializedSettings)
        {
            serializedSettings.Update();

            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultGridTopology)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultProjectionMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultRenderMode)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.DefaultNeighborhood)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Orthogonal", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.OrthogonalCellWidth)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.OrthogonalCellHeight)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Isometric", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.IsometricTileWidth)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.IsometricTileHeight)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Hex", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HexOrientation)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HexRadius)));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Height Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.HeightScale)));
            EditorGUILayout.PropertyField(serializedSettings.FindProperty(nameof(MoyvaProjectSettingsSO.UseHeightForPreview)));

            if (serializedSettings.ApplyModifiedProperties())
            {
                var target = serializedSettings.targetObject as MoyvaProjectSettingsSO;
                target?.Normalize();
                EditorUtility.SetDirty(serializedSettings.targetObject);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Asset", GUILayout.Width(120f)))
                    EditorGUIUtility.PingObject(serializedSettings.targetObject);
                GUILayout.Label(MoyvaProjectSettingsSO.DefaultAssetPath, EditorStyles.miniLabel);
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Moyva Project Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Central defaults for grid topology, projection, render mode, and preview height. Runtime installers can use this asset while current orthogonal 2D behavior remains the default.", MessageType.Info);
        }

        private void LoadSettings(bool createIfMissing = true)
        {
            _settings = createIfMissing ? GetOrCreateSettingsAsset() : LoadSettingsAsset();
            _serializedSettings = _settings != null ? new SerializedObject(_settings) : null;
        }

        private static MoyvaProjectSettingsSO LoadSettingsAsset()
            => AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);

        private static MoyvaProjectSettingsSO GetOrCreateSettingsAsset()
        {
            var settings = LoadSettingsAsset();
            if (settings != null)
                return settings;

            string directory = Path.GetDirectoryName(MoyvaProjectSettingsSO.DefaultAssetPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            settings = CreateInstance<MoyvaProjectSettingsSO>();
            settings.Normalize();
            AssetDatabase.CreateAsset(settings, MoyvaProjectSettingsSO.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}