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
                "Мінімальні налаштування камери: 3D ізометричний perspective-режим, межі, єдиний профіль керування та базові стартові параметри.",
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
            EditorGUILayout.LabelField("3D Camera", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serialized.FindProperty("adaptToProject3DMode"), new GUIContent("Adapt To Project 3D"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("useOrthographicCameraIn3D"), new GUIContent("Use Orthographic In 3D"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("default3DCameraDistance"), new GUIContent("Default 3D Distance"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("default3DFieldOfView"), new GUIContent("Default 3D Field Of View"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("isometric3DEuler"), new GUIContent("Isometric Euler"));
            EditorGUILayout.PropertyField(_serialized.FindProperty("orthographic3DEuler"), new GUIContent("Orthographic Euler"));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("World Bounds", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serialized.FindProperty("boundsOverflowTiles"),
                new GUIContent("Bounds Overflow (Tiles)", "На скільки тайлів дозволено вихід камери за межі мапи по X/Y."));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Control Profile", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Єдиний профіль для всіх платформ. Рекомендовано збільшувати лише Smooth Time і зменшувати Zoom Speed для більш ніжної камери.", MessageType.None);
            EditorGUILayout.PropertyField(_serialized.FindProperty("controlProfile"), true);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Min/Max zoom підлаштовуються автоматично від реального розміру мапи після генерації.", MessageType.None);

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

                if (GUILayout.Button("Reset Profile to Gentle Defaults", GUILayout.Width(220f)))
                {
                    Undo.RecordObject(_settingsAsset, "Reset Camera Control Profile");
                    _settingsAsset.controlProfile = CameraControlProfile.CreateGentleDefaults();
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
                _settingsAsset.controlProfile = CameraControlProfile.CreateGentleDefaults();
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
