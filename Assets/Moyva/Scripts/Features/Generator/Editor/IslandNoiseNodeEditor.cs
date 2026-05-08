using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(IslandNoiseNode))]
    public sealed class IslandNoiseNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _islandCount;
        private SerializedProperty _minRadius;
        private SerializedProperty _maxRadius;
        private SerializedProperty _sizeBias;
        private SerializedProperty _spacing;
        private SerializedProperty _offset;
        private SerializedProperty _aspectVariance;
        private SerializedProperty _shoreSoftness;
        private SerializedProperty _falloffPower;
        private SerializedProperty _shoreCutoff;
        private SerializedProperty _coastNoiseScale;
        private SerializedProperty _coastNoiseStrength;
        private SerializedProperty _coastOctaves;
        private SerializedProperty _detailNoiseScale;
        private SerializedProperty _detailStrength;
        private SerializedProperty _detailOctaves;
        private SerializedProperty _heightPower;
        private SerializedProperty _outputMin;
        private SerializedProperty _outputMax;
        private SerializedProperty _heightReduction;
        private SerializedProperty _normalize;

        private Texture2D _previewTexture;
        private bool _previewStale = true;
        private int _previewResolution = 128;

        private void OnEnable()
        {
            _islandCount = serializedObject.FindProperty("_islandCount");
            _minRadius = serializedObject.FindProperty("_minRadius");
            _maxRadius = serializedObject.FindProperty("_maxRadius");
            _sizeBias = serializedObject.FindProperty("_sizeBias");
            _spacing = serializedObject.FindProperty("_spacing");
            _offset = serializedObject.FindProperty("_offset");
            _aspectVariance = serializedObject.FindProperty("_aspectVariance");
            _shoreSoftness = serializedObject.FindProperty("_shoreSoftness");
            _falloffPower = serializedObject.FindProperty("_falloffPower");
            _shoreCutoff = serializedObject.FindProperty("_shoreCutoff");
            _coastNoiseScale = serializedObject.FindProperty("_coastNoiseScale");
            _coastNoiseStrength = serializedObject.FindProperty("_coastNoiseStrength");
            _coastOctaves = serializedObject.FindProperty("_coastOctaves");
            _detailNoiseScale = serializedObject.FindProperty("_detailNoiseScale");
            _detailStrength = serializedObject.FindProperty("_detailStrength");
            _detailOctaves = serializedObject.FindProperty("_detailOctaves");
            _heightPower = serializedObject.FindProperty("_heightPower");
            _outputMin = serializedObject.FindProperty("_outputMin");
            _outputMax = serializedObject.FindProperty("_outputMax");
            _heightReduction = serializedObject.FindProperty("_heightReduction");
            _normalize = serializedObject.FindProperty("_normalize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawLayoutSection();
            DrawShapeSection();
            DrawCoastSection();
            DrawInteriorSection();
            DrawOutputSection();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                foreach (Object item in targets)
                {
                    var node = (IslandNoiseNode)item;
                    node.SanitizeSettings();
                    node.ClearPreviewCache();
                    EditorUtility.SetDirty(node);
                }

                _previewStale = true;
                Repaint();
            }

            DrawPreviewSection();
        }

        private void DrawLayoutSection()
        {
            EditorGUILayout.LabelField("Розміщення", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_islandCount, new GUIContent("Кількість островів"));
            DrawRangeSlider("Діапазон радіуса", _minRadius, _maxRadius, 0.001f, 0.75f);
            EditorGUILayout.PropertyField(_sizeBias, new GUIContent("Перевага малих островів"));
            EditorGUILayout.PropertyField(_spacing, new GUIContent("Відступ між островами"));
            EditorGUILayout.PropertyField(_offset, new GUIContent("Зміщення"));

            EditorGUILayout.Space(8f);
        }

        private void DrawShapeSection()
        {
            EditorGUILayout.LabelField("Форма островів", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_aspectVariance, new GUIContent("Варіативність форми"));
            EditorGUILayout.PropertyField(_shoreSoftness, new GUIContent("М'якість берега"));
            EditorGUILayout.PropertyField(_falloffPower, new GUIContent("Спад висоти"));
            EditorGUILayout.PropertyField(_shoreCutoff, new GUIContent("Обрізання берега"));
            EditorGUILayout.Space(8f);
        }

        private void DrawCoastSection()
        {
            EditorGUILayout.LabelField("Деформація берегів", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_coastNoiseScale, new GUIContent("Масштаб шуму"));
            EditorGUILayout.PropertyField(_coastNoiseStrength, new GUIContent("Сила"));
            EditorGUILayout.PropertyField(_coastOctaves, new GUIContent("Октави"));
            EditorGUILayout.Space(8f);
        }

        private void DrawInteriorSection()
        {
            EditorGUILayout.LabelField("Деталі всередині", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_detailNoiseScale, new GUIContent("Масштаб деталей"));
            EditorGUILayout.PropertyField(_detailStrength, new GUIContent("Сила деталей"));
            EditorGUILayout.PropertyField(_detailOctaves, new GUIContent("Октави деталей"));
            EditorGUILayout.PropertyField(_heightPower, new GUIContent("Контраст висоти"));
            EditorGUILayout.Space(8f);
        }

        private void DrawOutputSection()
        {
            EditorGUILayout.LabelField("Вихід", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_normalize, new GUIContent("Нормалізувати"));
            DrawRangeSlider("Діапазон виходу", _outputMin, _outputMax, 0f, 1f);
            EditorGUILayout.PropertyField(_heightReduction, new GUIContent("Зниження висоти"));
            EditorGUILayout.Space(8f);
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("Прев'ю", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _previewResolution = EditorGUILayout.IntPopup(
                    _previewResolution,
                    new[] { "96", "128", "192", "256" },
                    new[] { 96, 128, 192, 256 },
                    GUILayout.Width(78f));

                if (GUILayout.Button(_previewStale ? "Оновити прев'ю *" : "Оновити прев'ю"))
                    RefreshPreview();

                if (GUILayout.Button("Очистити кеш"))
                {
                    ((IslandNoiseNode)target).ClearPreviewCache();
                    _previewTexture = null;
                    _previewStale = true;
                    Repaint();
                }
            }

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Прев'ю з'явиться після першого запуску графа, бо seed береться тільки з налаштувань графа.", MessageType.None);
                return;
            }

            if (_previewStale)
                EditorGUILayout.HelpBox("Параметри змінені. Прев'ю застаріло, онови його кнопкою вище.", MessageType.None);

            Rect previewRect = GUILayoutUtility.GetRect(256f, 256f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(previewRect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void DrawRangeSlider(string label, SerializedProperty minProperty, SerializedProperty maxProperty, float minLimit, float maxLimit)
        {
            float minValue = minProperty.floatValue;
            float maxValue = maxProperty.floatValue;
            EditorGUILayout.MinMaxSlider(label, ref minValue, ref maxValue, minLimit, maxLimit);

            using (new EditorGUILayout.HorizontalScope())
            {
                minValue = EditorGUILayout.FloatField("Мін", minValue);
                maxValue = EditorGUILayout.FloatField("Макс", maxValue);
            }

            minValue = Mathf.Clamp(minValue, minLimit, maxLimit);
            maxValue = Mathf.Clamp(maxValue, minLimit, maxLimit);
            if (maxValue < minValue)
                maxValue = minValue;

            minProperty.floatValue = minValue;
            maxProperty.floatValue = maxValue;
        }

        private void RefreshPreview()
        {
            var node = (IslandNoiseNode)target;
            _previewTexture = node.GeneratePreview(_previewResolution, _previewResolution);
            if (_previewTexture != null)
                _previewTexture.filterMode = FilterMode.Point;
            _previewStale = false;
            Repaint();
        }

    }
}