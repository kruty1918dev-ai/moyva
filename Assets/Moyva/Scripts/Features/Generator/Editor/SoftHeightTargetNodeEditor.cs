using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(SoftHeightTargetNode))]
    public sealed class SoftHeightTargetNodeEditor : UnityEditor.Editor
    {
        private const float MinGap = 0.000001f;
        private const float SmallStep = 0.0001f;

        private SerializedProperty _minHeight;
        private SerializedProperty _maxHeight;
        private SerializedProperty _targetHeight;
        private SerializedProperty _strength;
        private SerializedProperty _rangeTransition;
        private SerializedProperty _softness;
        private SerializedProperty _neighborRadius;
        private SerializedProperty _softnessIterations;

        private Texture2D _previewTexture;
        private bool _previewStale = true;

        private void OnEnable()
        {
            _minHeight = serializedObject.FindProperty("_minHeight");
            _maxHeight = serializedObject.FindProperty("_maxHeight");
            _targetHeight = serializedObject.FindProperty("_targetHeight");
            _strength = serializedObject.FindProperty("_strength");
            _rangeTransition = serializedObject.FindProperty("_rangeTransition");
            _softness = serializedObject.FindProperty("_softness");
            _neighborRadius = serializedObject.FindProperty("_neighborRadius");
            _softnessIterations = serializedObject.FindProperty("_softnessIterations");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawRangeSection();
            DrawTargetSection();
            DrawSoftnessSection();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                foreach (Object item in targets)
                {
                    var node = (SoftHeightTargetNode)item;
                    node.SanitizeSettings();
                    EditorUtility.SetDirty(node);
                }

                _previewStale = true;
                Repaint();
            }

            DrawPreview();
        }

        private void DrawRangeSection()
        {
            EditorGUILayout.LabelField("Діапазон дії", EditorStyles.boldLabel);

            float min = _minHeight.floatValue;
            float max = _maxHeight.floatValue;
            EditorGUILayout.MinMaxSlider("Швидко", ref min, ref max, 0f, 1f);
            DrawPreciseFloatRow("Мінімальна висота", ref min);
            DrawPreciseFloatRow("Максимальна висота", ref max);

            SetOrderedRange(_minHeight, _maxHeight, min, max);
            EditorGUILayout.PropertyField(_rangeTransition, new GUIContent("М'якість країв"));
            EditorGUILayout.Space(8f);
        }

        private static void DrawPreciseFloatRow(string label, ref float value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(label);
                if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(24f)))
                    value -= SmallStep;

                string text = EditorGUILayout.DelayedTextField(value.ToString("0.######"), GUILayout.MinWidth(90f));
                if (float.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed))
                    value = parsed;

                if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(24f)))
                    value += SmallStep;
            }
        }

        private void DrawTargetSection()
        {
            EditorGUILayout.LabelField("Цільова висота", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_targetHeight, new GUIContent("Приблизна ціль"));
            EditorGUILayout.PropertyField(_strength, new GUIContent("Сила підтягування"));
            EditorGUILayout.Space(8f);
        }

        private void DrawSoftnessSection()
        {
            EditorGUILayout.LabelField("М'якість із сусідами", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_softness, new GUIContent("М'якість"));
            EditorGUILayout.PropertyField(_neighborRadius, new GUIContent("Радіус сусідів"));
            EditorGUILayout.PropertyField(_softnessIterations, new GUIContent("Ітерації"));
            EditorGUILayout.Space(8f);
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Прев'ю", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(_previewStale ? "Оновити прев'ю *" : "Оновити прев'ю"))
                    RefreshPreview();
            }

            EditorGUILayout.HelpBox("Жовтий/білий контур показує регіон дії. Помаранчевий означає підняття висоти, синій — зниження.", MessageType.None);

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Прев'ю з'явиться після першого запуску графа, коли нода отримає HeightMap.", MessageType.None);
                return;
            }

            if (_previewStale)
                EditorGUILayout.HelpBox("Параметри змінені. Прев'ю застаріло, онови його кнопкою вище.", MessageType.None);

            Rect rect = GUILayoutUtility.GetRect(256f, 256f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void RefreshPreview()
        {
            var node = (SoftHeightTargetNode)target;
            _previewTexture = node.GeneratePreview(256, 256);
            if (_previewTexture != null)
                _previewTexture.filterMode = FilterMode.Point;
            _previewStale = false;
            Repaint();
        }

        private static void SetOrderedRange(SerializedProperty minProperty, SerializedProperty maxProperty, float min, float max)
        {
            min = Mathf.Clamp01(min);
            max = Mathf.Clamp01(max);
            if (max <= min + MinGap)
            {
                if (min <= 1f - MinGap)
                    max = min + MinGap;
                else
                    min = max - MinGap;
            }

            minProperty.floatValue = Mathf.Clamp01(min);
            maxProperty.floatValue = Mathf.Clamp01(max);
        }
    }
}