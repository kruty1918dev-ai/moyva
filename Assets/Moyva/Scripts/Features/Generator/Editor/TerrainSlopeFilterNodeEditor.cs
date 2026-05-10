#if UNITY_EDITOR
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(TerrainSlopeFilterNode))]
    public sealed class TerrainSlopeFilterNodeEditor : UnityEditor.Editor
    {
        private SerializedProperty _maxSlopeDelta;
        private SerializedProperty _maxIterations;
        private SerializedProperty _cardinalOnly;

        private bool _statsFoldout = true;
        private bool _helpFoldout;

        private Texture2D _previewTexture;

        private void OnEnable()
        {
            _maxSlopeDelta = serializedObject.FindProperty("_maxSlopeDelta");
            _maxIterations = serializedObject.FindProperty("_maxIterations");
            _cardinalOnly  = serializedObject.FindProperty("_cardinalOnly");
        }

        private void OnDisable()
        {
            DestroyPreviewTexture();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var node = (TerrainSlopeFilterNode)target;

            // ── Заголовок ──
            var info = (NodeInfoAttribute)System.Attribute.GetCustomAttribute(
                node.GetType(), typeof(NodeInfoAttribute));
            if (info != null && !string.IsNullOrWhiteSpace(info.Description))
            {
                EditorGUILayout.HelpBox(info.Description, MessageType.Info);
                EditorGUILayout.Space(4);
            }

            // ── Налаштування ──
            EditorGUILayout.LabelField("Slope Constraint", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_maxSlopeDelta,
                new GUIContent("Max Slope Delta",
                    "Максимальна різниця висот між сусідами. Менше = плавніші схили."));
            EditorGUILayout.PropertyField(_maxIterations,
                new GUIContent("Max Iterations",
                    "Ліміт проходів. Зупиняється раніше якщо карта стабільна."));
            EditorGUILayout.PropertyField(_cardinalOnly,
                new GUIContent("Cardinal Only (4 neighbors)",
                    "Враховувати 4 або 8 сусідів."));
            bool settingsChanged = EditorGUI.EndChangeCheck();

            // Показуємо рекомендацію під полем дельти
            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox(
                "Рекомендація: Max Slope Delta ≈ 1 ÷ кількість рівнів у HillGenerator.\n" +
                "Наприклад, 3 рівні → 0.33, 5 рівнів → 0.20.",
                MessageType.None);

            EditorGUILayout.Space(8);

            // ── Статистика ──
            _statsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_statsFoldout, "Статистика останнього запуску");

            if (_statsFoldout)
            {
                EditorGUILayout.Space(2);

                int usedIter = node.LastIterationsUsed;
                int raised   = node.LastCellsRaised;
                int maxIter  = node.MaxIterations;

                if (usedIter == 0 && raised == 0)
                {
                    EditorGUILayout.HelpBox("Нода ще не виконувалась. Запустіть граф щоб побачити статистику.", MessageType.None);
                }
                else
                {
                    bool hitLimit = usedIter >= maxIter && raised > 0;

                    DrawStatRow("Ітерацій використано:",
                        $"{usedIter} / {maxIter}",
                        hitLimit ? new Color(1f, 0.55f, 0.05f) : Color.white);

                    DrawStatRow("Клітинок піднято:", $"{raised}");

                    bool stable = !hitLimit;
                    if (stable)
                    {
                        EditorGUILayout.HelpBox(
                            $"✓ Карта стабілізувалась за {usedIter} {IterLabel(usedIter)}.",
                            MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            $"⚠ Досягнуто ліміт {maxIter} ітерацій. Карта може бути нестабільною.\n" +
                            "Збільшіть Max Iterations або Max Slope Delta.",
                            MessageType.Warning);
                    }
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(4);

            // ── Довідка по алгоритму ──
            _helpFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_helpFoldout, "Алгоритм (довідка)");

            if (_helpFoldout)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox(
                    "Slope Constraint Algorithm:\n\n" +
                    "Кожен прохід сканує карту у двох напрямках (↗ і ↙ для усунення упередженості).\n" +
                    "Для кожної клітинки перевіряє сусідів: якщо різниця висот перевищує Max Slope Delta — " +
                    "сусід підіймається до рівня (peak − delta).\n\n" +
                    "Властивості:\n" +
                    "• Значення лише збільшуються — піки ніколи не зрізаються.\n" +
                    "• Результат: плавні схили навколо кожного піку.\n" +
                    "• Збіжність: O(radius × iterations), де radius = відстань поширення схилу.\n\n" +
                    "Типове використання:\n" +
                    "PerlinNoise → TerrainSlopeFilter → HillGenerator",
                    MessageType.None);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            if (serializedObject.ApplyModifiedProperties() || settingsChanged)
                NotifyGraphEditorChanged();
        }

        // ── Helpers ──

        private static void DrawStatRow(string label, string value, Color? valueColor = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(180f));
                var style = EditorStyles.label;
                if (valueColor.HasValue)
                {
                    style = new GUIStyle(EditorStyles.label)
                    {
                        normal = { textColor = valueColor.Value }
                    };
                }
                EditorGUILayout.LabelField(value, style);
            }
        }

        private static string IterLabel(int n) =>
            n % 10 == 1 && n % 100 != 11 ? "ітерацію" :
            n % 10 is >= 2 and <= 4 && n % 100 is not (>= 12 and <= 14) ? "ітерації" :
            "ітерацій";

        private void DestroyPreviewTexture()
        {
            if (_previewTexture != null)
            {
                DestroyImmediate(_previewTexture);
                _previewTexture = null;
            }
        }

        private void NotifyGraphEditorChanged()
        {
            EditorUtility.SetDirty(target);
            Repaint();

            var graphWindowType = System.Type.GetType(
                "Kruty1918.Moyva.GraphSystem.Editor.GraphEditorWindow, Kruty1918.Moyva.GraphSystem.Editor");
            if (graphWindowType == null) return;

            var requestAutoRun = graphWindowType.GetMethod(
                "RequestAutoRun",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            foreach (var window in Resources.FindObjectsOfTypeAll(graphWindowType))
            {
                requestAutoRun?.Invoke(window, null);
                if (window is EditorWindow editorWindow)
                    editorWindow.Repaint();
            }
        }
    }
}
#endif
