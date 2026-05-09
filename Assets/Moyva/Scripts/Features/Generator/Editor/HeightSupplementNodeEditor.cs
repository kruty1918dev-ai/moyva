using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(HeightSupplementNode))]
    public sealed class HeightSupplementNodeEditor : UnityEditor.Editor
    {
        private const float MinRangeGap = 0.001f;

        private SerializedProperty _minHeight;
        private SerializedProperty _maxHeight;
        private SerializedProperty _noiseMin;
        private SerializedProperty _noiseMax;
        private SerializedProperty _noiseTransition;
        private SerializedProperty _peakPower;
        private SerializedProperty _amount;
        private SerializedProperty _rangeTransition;
        private SerializedProperty _smoothRadius;
        private SerializedProperty _smoothIterations;
        private Texture2D _previewTexture;

        private void OnEnable()
        {
            _minHeight = serializedObject.FindProperty("_minHeight");
            _maxHeight = serializedObject.FindProperty("_maxHeight");
            _noiseMin = serializedObject.FindProperty("_noiseMin");
            _noiseMax = serializedObject.FindProperty("_noiseMax");
            _noiseTransition = serializedObject.FindProperty("_noiseTransition");
            _peakPower = serializedObject.FindProperty("_peakPower");
            _amount = serializedObject.FindProperty("_amount");
            _rangeTransition = serializedObject.FindProperty("_rangeTransition");
            _smoothRadius = serializedObject.FindProperty("_smoothRadius");
            _smoothIterations = serializedObject.FindProperty("_smoothIterations");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawHeightRange();
            DrawNoiseCut();
            DrawBlend();
            DrawSmoothing();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                foreach (Object obj in targets)
                {
                    var node = (HeightSupplementNode)obj;
                    node.SanitizeSettings();
                    EditorUtility.SetDirty(node);
                }

                serializedObject.Update();
                RefreshPreview();
            }

            DrawPreview();
        }

        private void DrawHeightRange()
        {
            EditorGUILayout.LabelField("Height Range", EditorStyles.boldLabel);

            float min = _minHeight.floatValue;
            float max = _maxHeight.floatValue;
            EditorGUILayout.MinMaxSlider("Work Range", ref min, ref max, 0f, 1f);

            min = EditorGUILayout.Slider("Min Height", min, 0f, 1f);
            max = EditorGUILayout.Slider("Max Height", max, 0f, 1f);
            SetOrderedRange(_minHeight, _maxHeight, min, max);

            EditorGUILayout.Space(6f);
        }

        private void DrawNoiseCut()
        {
            EditorGUILayout.LabelField("Noise Cut", EditorStyles.boldLabel);

            float min = _noiseMin.floatValue;
            float max = _noiseMax.floatValue;
            EditorGUILayout.MinMaxSlider("Allowed Noise", ref min, ref max, 0f, 1f);

            min = EditorGUILayout.Slider("Noise Min", min, 0f, 1f);
            max = EditorGUILayout.Slider("Noise Max", max, 0f, 1f);
            SetOrderedRange(_noiseMin, _noiseMax, min, max);

            EditorGUILayout.PropertyField(_noiseTransition, new GUIContent("Noise Transition"));
            EditorGUILayout.PropertyField(_peakPower, new GUIContent("Peak Shape"));
            EditorGUILayout.Space(6f);
        }

        private void DrawBlend()
        {
            EditorGUILayout.LabelField("Blend", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_amount, new GUIContent("Height Amount"));
            EditorGUILayout.PropertyField(_rangeTransition, new GUIContent("Range Transition"));
            EditorGUILayout.Space(6f);
        }

        private void DrawSmoothing()
        {
            EditorGUILayout.LabelField("Smoothing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_smoothRadius, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(_smoothIterations, new GUIContent("Iterations"));
            EditorGUILayout.Space(8f);
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Preview"))
                    RefreshPreview();
            }

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Preview appears after the graph runs and the node receives HeightMap + Noise Mask.", MessageType.None);
                return;
            }

            Rect rect = GUILayoutUtility.GetRect(256f, 256f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void RefreshPreview()
        {
            var node = (HeightSupplementNode)target;
            _previewTexture = node.GeneratePreview(256, 256);
            if (_previewTexture != null)
                _previewTexture.filterMode = FilterMode.Point;
            Repaint();
        }

        private static void SetOrderedRange(SerializedProperty minProperty, SerializedProperty maxProperty, float min, float max)
        {
            min = Mathf.Clamp01(min);
            max = Mathf.Clamp01(max);

            if (max <= min + MinRangeGap)
            {
                if (min <= 1f - MinRangeGap)
                    max = min + MinRangeGap;
                else
                    min = max - MinRangeGap;
            }

            minProperty.floatValue = Mathf.Clamp01(min);
            maxProperty.floatValue = Mathf.Clamp01(max);
        }
    }
}