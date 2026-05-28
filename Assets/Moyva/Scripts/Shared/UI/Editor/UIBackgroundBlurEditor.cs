using Kruty1918.Moyva.Shared.UI;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Shared.UI.Editor
{
    [CustomEditor(typeof(UIBackgroundBlur))]
    [CanEditMultipleObjects]
    public sealed class UIBackgroundBlurEditor : UnityEditor.Editor
    {
        private SerializedProperty _blurStrength;
        private SerializedProperty _downsample;
        private SerializedProperty _fadeInDuration;
        private SerializedProperty _fadeOutDuration;
        private SerializedProperty _fadeCurve;
        private SerializedProperty _shader;
        private SerializedProperty _enableDebugLogs;

        private void OnEnable()
        {
            _blurStrength = serializedObject.FindProperty("_blurStrength");
            _downsample = serializedObject.FindProperty("_downsample");
            _fadeInDuration = serializedObject.FindProperty("_fadeInDuration");
            _fadeOutDuration = serializedObject.FindProperty("_fadeOutDuration");
            _fadeCurve = serializedObject.FindProperty("_fadeCurve");
            _shader = serializedObject.FindProperty("_shader");
            _enableDebugLogs = serializedObject.FindProperty("_enableDebugLogs");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Blur", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_blurStrength, new GUIContent("Blur Strength"));
            EditorGUILayout.PropertyField(_downsample, new GUIContent("Downsample"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Fade", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_fadeInDuration, new GUIContent("Fade In Duration"));
            EditorGUILayout.PropertyField(_fadeOutDuration, new GUIContent("Fade Out Duration"));
            EditorGUILayout.PropertyField(_fadeCurve, new GUIContent("Fade Curve"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Render", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_shader, new GUIContent("Shader"));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableDebugLogs, new GUIContent("Enable Debug Logs"));

            if (_downsample.intValue < 1 || _downsample.intValue > 8)
                EditorGUILayout.HelpBox("Downsample should be in the [1..8] range.", MessageType.Warning);

            if (_blurStrength.floatValue < 0f || _blurStrength.floatValue > 20f)
                EditorGUILayout.HelpBox("Blur Strength should be in the [0..20] range.", MessageType.Warning);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Refresh Blur", GUILayout.Height(26f)))
            {
                foreach (Object targetObject in targets)
                {
                    if (targetObject is UIBackgroundBlur blur)
                    {
                        Undo.RecordObject(blur, "Refresh UI Background Blur");
                        blur.Refresh();
                        EditorUtility.SetDirty(blur);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
