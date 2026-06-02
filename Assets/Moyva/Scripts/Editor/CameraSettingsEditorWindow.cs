using Kruty1918.Moyva.Camera.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    public sealed class CameraSettingsEditorWindow : EditorWindow
    {
        private const string DefaultAssetPath = "Assets/Moyva/SO/Camera/CameraSettings.asset";

        private CameraSettingsSO _settingsAsset;
        private SerializedObject _serialized;
        private Vector2 _scroll;
        [MenuItem("Moyva/Tools/Camera/Settings Editor", priority = 41)]
        public static void Open()
        {
            var window = GetWindow<CameraSettingsEditorWindow>("Camera Settings");
            window.minSize = new Vector2(700f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureAssetLoaded();
        }

        private void OnGUI()
        {
            EnsureAssetLoaded();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Camera Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Окремий редактор налаштувань камери: базові параметри, офсет меж мапи та роздільні профілі Desktop/Mobile.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                var next = (CameraSettingsSO)EditorGUILayout.ObjectField("Settings Asset", _settingsAsset, typeof(CameraSettingsSO), false);
                if (next != _settingsAsset)
                {
                    _settingsAsset = next;
                    _serialized = _settingsAsset != null ? new SerializedObject(_settingsAsset) : null;
                }

                if (GUILayout.Button("Create/Find", GUILayout.Width(96f)))
                    CreateOrFindAsset();
            }

            if (_settingsAsset == null || _serialized == null)
            {
                EditorGUILayout.HelpBox("Asset CameraSettingsSO не знайдено. Натисни Create/Find.", MessageType.Warning);
                EditorGUILayout.EndScrollView();
                return;
            }

            _serialized.Update();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serialized.FindProperty("defaultCameraZ"), new GUIContent("Default Camera Z"));
            EditorGUILayout.PropertyField(
                _serialized.FindProperty("boundsOverflowTiles"),
                new GUIContent("Bounds Overflow (Tiles)", "На скільки тайлів дозволено вихід камери за межі мапи по X/Y."));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Desktop Profile", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serialized.FindProperty("desktopProfile"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Mobile Profile", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serialized.FindProperty("mobileProfile"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Map Render Mask", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapRenderMaskEnabled"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapMaskRefreshSeconds"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapMaskLayers"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapMaskSortingLayerName"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapMaskBackSortingOrder"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("mapMaskFrontSortingOrder"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("manualMapMaskCenter"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("manualMapMaskSize"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Shader / Mip Bias", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serialized.FindProperty("enableAutomaticMipBias"),
                new GUIContent("Enable Automatic Mip Bias", "Вмикає глобальний mip-bias від зуму. Для tile atlas часто краще вимкнути."));
            EditorGUILayout.PropertyField(
                _serialized.FindProperty("automaticMipBiasMax"),
                new GUIContent("Automatic Mip Bias Max", "Максимальна сила mip-bias, якщо опція увімкнена."));

            if (_serialized.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_settingsAsset);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Asset", GUILayout.Width(96f)))
                {
                    Selection.activeObject = _settingsAsset;
                    EditorGUIUtility.PingObject(_settingsAsset);
                }

                if (GUILayout.Button("Reset Profiles to Defaults", GUILayout.Width(180f)))
                {
                    Undo.RecordObject(_settingsAsset, "Reset Camera Profiles");
                    _settingsAsset.desktopProfile = CameraControlProfile.CreateDesktopDefaults();
                    _settingsAsset.mobileProfile = CameraControlProfile.CreateMobileDefaults();
                    EditorUtility.SetDirty(_settingsAsset);
                    AssetDatabase.SaveAssets();
                    _serialized.Update();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void EnsureAssetLoaded()
        {
            if (_settingsAsset != null && _serialized != null)
                return;

            _settingsAsset = AssetDatabase.LoadAssetAtPath<CameraSettingsSO>(DefaultAssetPath);
            if (_settingsAsset == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:CameraSettingsSO");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _settingsAsset = AssetDatabase.LoadAssetAtPath<CameraSettingsSO>(path);
                }
            }

            if (_settingsAsset != null)
                _serialized = new SerializedObject(_settingsAsset);
        }

        private void CreateOrFindAsset()
        {
            EnsureFolder("Assets/Moyva");
            EnsureFolder("Assets/Moyva/SO");
            EnsureFolder("Assets/Moyva/SO/Camera");

            _settingsAsset = AssetDatabase.LoadAssetAtPath<CameraSettingsSO>(DefaultAssetPath);
            if (_settingsAsset == null)
            {
                _settingsAsset = CreateInstance<CameraSettingsSO>();
                _settingsAsset.desktopProfile = CameraControlProfile.CreateDesktopDefaults();
                _settingsAsset.mobileProfile = CameraControlProfile.CreateMobileDefaults();
                AssetDatabase.CreateAsset(_settingsAsset, DefaultAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _serialized = new SerializedObject(_settingsAsset);
            Selection.activeObject = _settingsAsset;
            EditorGUIUtility.PingObject(_settingsAsset);
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
