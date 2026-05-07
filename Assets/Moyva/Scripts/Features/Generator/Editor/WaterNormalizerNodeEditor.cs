using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(WaterNormalizerNode))]
    public sealed class WaterNormalizerNodeEditor : UnityEditor.Editor
    {
        private const float MinGap = 0.000001f;
        private const float SmallStep = 0.0001f;

        private SerializedProperty _waterMinHeight;
        private SerializedProperty _waterMaxHeight;
        private SerializedProperty _waterLevels;
        private SerializedProperty _deepDistance;
        private SerializedProperty _levelBlend;
        private SerializedProperty _cleanupIterations;
        private SerializedProperty _fillNeighborThreshold;
        private SerializedProperty _removeNeighborThreshold;
        private SerializedProperty _minWaterBodySize;
        private SerializedProperty _shoreWidth;
        private SerializedProperty _shoreTargetHeight;
        private SerializedProperty _shoreBlend;
        private SerializedProperty _shoreMaxSourceHeight;
        private SerializedProperty _smoothRadius;
        private SerializedProperty _smoothIterations;

        private Texture2D _previewTexture;
        private bool _previewStale = true;

        private void OnEnable()
        {
            _waterMinHeight = serializedObject.FindProperty("_waterMinHeight");
            _waterMaxHeight = serializedObject.FindProperty("_waterMaxHeight");
            _waterLevels = serializedObject.FindProperty("_waterLevels");
            _deepDistance = serializedObject.FindProperty("_deepDistance");
            _levelBlend = serializedObject.FindProperty("_levelBlend");
            _cleanupIterations = serializedObject.FindProperty("_cleanupIterations");
            _fillNeighborThreshold = serializedObject.FindProperty("_fillNeighborThreshold");
            _removeNeighborThreshold = serializedObject.FindProperty("_removeNeighborThreshold");
            _minWaterBodySize = serializedObject.FindProperty("_minWaterBodySize");
            _shoreWidth = serializedObject.FindProperty("_shoreWidth");
            _shoreTargetHeight = serializedObject.FindProperty("_shoreTargetHeight");
            _shoreBlend = serializedObject.FindProperty("_shoreBlend");
            _shoreMaxSourceHeight = serializedObject.FindProperty("_shoreMaxSourceHeight");
            _smoothRadius = serializedObject.FindProperty("_smoothRadius");
            _smoothIterations = serializedObject.FindProperty("_smoothIterations");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawWaterRange();
            DrawLevelSection();
            DrawCleanupSection();
            DrawShoreSection();
            DrawSmoothingSection();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                foreach (Object item in targets)
                {
                    var node = (WaterNormalizerNode)item;
                    node.SanitizeSettings();
                    EditorUtility.SetDirty(node);
                }

                _previewStale = true;
                Repaint();
            }

            DrawPreview();
        }

        private void DrawWaterRange()
        {
            EditorGUILayout.LabelField("Діапазон води", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Нода спершу бере всі клітинки, висота яких входить у цей діапазон. Це тільки кандидати у воду; дрібні випадкові плями можна відкинути нижче параметром мінімального розміру водойми.", MessageType.None);

            float min = _waterMinHeight.floatValue;
            float max = _waterMaxHeight.floatValue;
            EditorGUILayout.MinMaxSlider("Швидко", ref min, ref max, 0f, 1f);
            DrawPreciseFloatRow("Мінімальна висота", ref min);
            DrawPreciseFloatRow("Максимальна висота", ref max);

            SetOrderedRange(_waterMinHeight, _waterMaxHeight, min, max);
            EditorGUILayout.Space(8f);
        }

        private void DrawLevelSection()
        {
            EditorGUILayout.LabelField("Рівні води", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Ці параметри розбивають воду на дискретні рівні: біля берега мілко, далі глибше. Це допомагає Height To Tile стабільно ставити тайли води/мілководдя.", MessageType.None);
            EditorGUILayout.PropertyField(_waterLevels, new GUIContent("Кількість рівнів"));
            EditorGUILayout.PropertyField(_deepDistance, new GUIContent("Відстань до глибини"));
            EditorGUILayout.PropertyField(_levelBlend, new GUIContent("Чіткість рівнів"));
            EditorGUILayout.Space(8f);
        }

        private void DrawCleanupSection()
        {
            EditorGUILayout.LabelField("Очищення маски", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Саме тут прибираються випадкові плями води. Якщо по мапі розкиданий пісок — збільшуй 'Мін. розмір водойми' або зменшуй діапазон води.", MessageType.None);
            EditorGUILayout.PropertyField(_cleanupIterations, new GUIContent("Проходи очищення"));
            EditorGUILayout.PropertyField(_fillNeighborThreshold, new GUIContent("Заповнювати дірки від"));
            EditorGUILayout.PropertyField(_removeNeighborThreshold, new GUIContent("Прибирати шум до"));
            EditorGUILayout.PropertyField(_minWaterBodySize, new GUIContent("Мін. розмір водойми"));
            EditorGUILayout.Space(8f);
        }

        private void DrawShoreSection()
        {
            EditorGUILayout.LabelField("Берег", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Берег створюється тільки навколо валідної води після очищення. 'Цільова висота берега' — це абсолютна висота, в яку нода намагається перевести піщану смугу. Якщо пісок у Height To Tile має діапазон 0.34..0.4, став тут, наприклад, 0.36.", MessageType.None);
            EditorGUILayout.PropertyField(_shoreWidth, new GUIContent("Ширина берега"));
            DrawPrecisePropertyRow("Цільова висота берега", _shoreTargetHeight);
            EditorGUILayout.PropertyField(_shoreBlend, new GUIContent("Сила берега"));
            EditorGUILayout.PropertyField(_shoreMaxSourceHeight, new GUIContent("Макс. висота джерела для берега"));
            EditorGUILayout.Space(8f);
        }

        private void DrawSmoothingSection()
        {
            EditorGUILayout.LabelField("Згладження", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_smoothRadius, new GUIContent("Радіус"));
            EditorGUILayout.PropertyField(_smoothIterations, new GUIContent("Ітерації"));
            EditorGUILayout.Space(8f);
        }

        private void DrawPreview()
        {
            EditorGUILayout.LabelField("Прев'ю", EditorStyles.boldLabel);
            DrawStats();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(_previewStale ? "Оновити прев'ю *" : "Оновити прев'ю"))
                    RefreshPreview();
            }

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox("Прев'ю з'явиться після першого запуску графа, коли нода отримає HeightMap.", MessageType.None);
                return;
            }

            if (_previewStale)
                EditorGUILayout.HelpBox("Параметри змінені. Прев'ю застаріло, онови його кнопкою вище.", MessageType.None);

            EditorGUILayout.HelpBox("Синє = валідна вода за рівнями. Жовте = берег/пісок. Рожеве = кандидат у воду, який був відкинутий очищенням. Темне = незмінений рельєф.", MessageType.None);

            Rect rect = GUILayoutUtility.GetRect(256f, 256f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void DrawStats()
        {
            var node = (WaterNormalizerNode)target;
            EditorGUILayout.LabelField(
                $"Кандидати: {node.LastCandidateCells} | Вода: {node.LastWaterCells} | Відкинуто: {node.LastRemovedCandidateCells} | Берег: {node.LastShoreCells}",
                EditorStyles.miniLabel);
        }

        private void RefreshPreview()
        {
            var node = (WaterNormalizerNode)target;
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

        private static void DrawPrecisePropertyRow(string label, SerializedProperty property)
        {
            float value = property.floatValue;
            DrawPreciseFloatRow(label, ref value);
            property.floatValue = Mathf.Clamp01(value);
        }
    }
}