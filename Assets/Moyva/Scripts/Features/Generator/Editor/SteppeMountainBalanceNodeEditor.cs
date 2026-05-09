using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(SteppeMountainBalanceNode))]
    public sealed class SteppeMountainBalanceNodeEditor : UnityEditor.Editor
    {
        private const float MinGap = 0.000001f;

        private SerializedProperty _mountainCoveragePercent;
        private SerializedProperty _steppeStartHeight;
        private SerializedProperty _liftCurve;

        private Texture2D _previewTexture;
        private bool _previewStale = true;

        private void OnEnable()
        {
            _mountainCoveragePercent = serializedObject.FindProperty("_mountainCoveragePercent");
            _steppeStartHeight = serializedObject.FindProperty("_steppeStartHeight");
            _liftCurve = serializedObject.FindProperty("_liftCurve");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawSteppeSection();
            DrawCoverageSection();
            DrawShapeSection();
            bool changed = EditorGUI.EndChangeCheck();

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                foreach (Object item in targets)
                {
                    var node = (SteppeMountainBalanceNode)item;
                    node.SanitizeSettings();
                    EditorUtility.SetDirty(node);
                }

                _previewStale = true;
                Repaint();
            }

            DrawStats();
            DrawPreview();
        }

        private void DrawSteppeSection()
        {
            EditorGUILayout.LabelField("Початок степу", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Усе, що нижче цього значення, ця нода не змінює: значення проходить далі як було. " +
                "Тільки клітинки з висотою >= 'Початок степу' беруть участь у розподілі степ/гори.",
                MessageType.None);

            DrawClampedFloatField("Початок степу", _steppeStartHeight, 0f, 1f - MinGap);
            EditorGUILayout.Space(8f);
        }

        private void DrawCoverageSection()
        {
            EditorGUILayout.LabelField("Розподіл рельєфу", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Відсоток рахується тільки від діапазону [Початок степу..1].\n" +
                "0% — рельєф над степом повністю прибраний (усе вирівняне до Початку степу, гір немає).\n" +
                "100% — рельєф над степом доходить аж до 1 (повноцінні піки).\n" +
                "20% — рельєф над степом обмежений верхньою межею Початок степу + 20% * (1 - Початок степу).",
                MessageType.None);

            DrawClampedFloatField("Відсоток гір", _mountainCoveragePercent, 0f, 100f);
            EditorGUILayout.Space(8f);
        }

        private void DrawShapeSection()
        {
            EditorGUILayout.LabelField("Форма піднесення", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Крива визначає форму переходу від рівнини до піків у межах дозволеного діапазону. " +
                "1 — лінійно, більше 1 — пласкі рівнини й різкі піки, менше 1 — швидко росте від степу й вирівнюється на верху.",
                MessageType.None);
            DrawClampedFloatField("Крива піднесення", _liftCurve, 0.1f, 4f);
            EditorGUILayout.Space(8f);
        }

        private void DrawStats()
        {
            var node = (SteppeMountainBalanceNode)target;
            EditorGUILayout.LabelField("Статистика", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"Макс. вихідна висота: {node.LastMaxOutputHeight:0.######}",
                EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"Поріг входу для гір: {node.LastMountainInputThreshold:0.######}",
                EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"Pass-through: {node.LastPassThroughCells} | Степ: {node.LastSteppeCells} | Гори: {node.LastMountainCells}",
                EditorStyles.miniLabel);
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

            if (_previewTexture == null)
            {
                EditorGUILayout.HelpBox(
                    "Прев'ю з'явиться після першого запуску графа, коли нода отримає Noise. " +
                    "Запусти граф (Run Graph), потім натисни 'Оновити прев'ю'.",
                    MessageType.None);
                return;
            }

            if (_previewStale)
                EditorGUILayout.HelpBox("Параметри змінені. Прев'ю застаріло, онови його кнопкою вище.", MessageType.None);

            EditorGUILayout.HelpBox(
                "Темно-синє = pass-through нижче початку степу. " +
                "Жовто-зелене = степ. Коричнево-сіре/біле = гори/піки (тільки коли %>0).",
                MessageType.None);

            Rect rect = GUILayoutUtility.GetRect(256f, 256f, GUILayout.ExpandWidth(false));
            EditorGUI.DrawPreviewTexture(rect, _previewTexture, null, UnityEngine.ScaleMode.ScaleToFit);
        }

        private void RefreshPreview()
        {
            var node = (SteppeMountainBalanceNode)target;
            _previewTexture = node.GeneratePreview(256, 256);
            if (_previewTexture != null)
                _previewTexture.filterMode = FilterMode.Point;
            _previewStale = false;
            Repaint();
        }

        private static void DrawClampedFloatField(string label, SerializedProperty property, float min, float max)
        {
            float value = EditorGUILayout.DelayedFloatField(label, property.floatValue);
            property.floatValue = Mathf.Clamp(value, min, max);
        }
    }
}
