using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    /// <summary>
    /// Редакторне вікно для управління колекціями стін у BuildingRegistrySO.
    /// Відкрити: Moyva → Construction → Wall Registry Editor.
    /// </summary>
    public sealed class WallRegistryWindow : EditorWindow
    {
        // ── Метадані варіантів (fieldName, UA назва, UA підказка) ──────────────
        private static readonly (string Field, string UaName, string UaHint)[] VariantMeta =
        {
            ("HorizontalPrefab",
             "Горизонтальна ←→",
             "Прямий сегмент вздовж осі X.\n" +
             "Використовується: немає сусідів, є лише E/W, або fallback для хреста."),

            ("VerticalPrefab",
             "Вертикальна ↑↓",
             "Прямий сегмент вздовж осі Y.\n" +
             "Використовується: є лише N/S сусіди."),

            ("CornerNorthEastPrefab",
             "Кут правий верхній — NE ↑→",
             "З'єднує Північ і Схід.\n" +
             "Використовується: є сусіди зверху (N) і праворуч (E)."),

            ("CornerNorthWestPrefab",
             "Кут лівий верхній — NW ↑←",
             "З'єднує Північ і Захід.\n" +
             "Використовується: є сусіди зверху (N) і ліворуч (W)."),

            ("CornerSouthEastPrefab",
             "Кут правий нижній — SE ↓→",
             "З'єднує Південь і Схід.\n" +
             "Використовується: є сусіди знизу (S) і праворуч (E)."),

            ("CornerSouthWestPrefab",
             "Кут лівий нижній — SW ↓←",
             "З'єднує Південь і Захід.\n" +
             "Використовується: є сусіди знизу (S) і ліворуч (W)."),
        };

        // ── Стан вікна ─────────────────────────────────────────────────────────
        private BuildingRegistrySO _registry;
        private Vector2 _scroll;
        private int _openIndex = -1;

        // ── Відкрити вікно ─────────────────────────────────────────────────────
        [MenuItem("Moyva/Construction/Wall Registry Editor")]
        public static void Open()
        {
            var window = GetWindow<WallRegistryWindow>("Реєстр стін");
            window.minSize = new Vector2(500f, 600f);
            window.Show();
        }

        // ── GUI ────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            DrawHeader();

            _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField(
                new GUIContent("BuildingRegistrySO", "ScriptableObject реєстру будівель"),
                _registry, typeof(BuildingRegistrySO), false);

            if (_registry == null)
            {
                EditorGUILayout.HelpBox(
                    "Перетягніть BuildingRegistrySO із проєкту, або натисніть «Знайти автоматично».",
                    MessageType.Warning);
                if (GUILayout.Button("Знайти автоматично", GUILayout.Height(28f)))
                    AutoFindRegistry();
                return;
            }

            EditorGUILayout.Space(4f);

            var so = new SerializedObject(_registry);
            so.Update();

            var collections = so.FindProperty("WallCollections");
            DrawToolbar(collections);

            EditorGUILayout.Space(4f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < collections.arraySize; i++)
                DrawCollection(so, collections, i);

            EditorGUILayout.EndScrollView();
            so.ApplyModifiedProperties();
        }

        // ── Заголовок ──────────────────────────────────────────────────────────
        private static void DrawHeader()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Редактор колекцій стін", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Кожна колекція описує один архітектурний стиль:\n" +
                "6 варіантів стіни (горизонтальна, вертикальна, 4 кути) та ворота.\n" +
                "Система автоматично обирає потрібний варіант залежно від сусідів тайла.",
                MessageType.Info);
            EditorGUILayout.Space(2f);
        }

        // ── Панель інструментів ────────────────────────────────────────────────
        private void DrawToolbar(SerializedProperty collections)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Колекцій: {collections.arraySize}", EditorStyles.boldLabel,
                GUILayout.Width(110f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Додати колекцію", EditorStyles.toolbarButton, GUILayout.Width(130f)))
            {
                collections.arraySize++;
                _openIndex = collections.arraySize - 1;

                // Встановлюємо ID за замовчуванням
                var newItem = collections.GetArrayElementAtIndex(_openIndex);
                newItem.FindPropertyRelative("CollectionId").stringValue = $"wall-collection-{_openIndex}";
                newItem.FindPropertyRelative("WallBuildingId").stringValue = "wall";
                newItem.FindPropertyRelative("GateBuildingId").stringValue = "gate";
            }
            EditorGUILayout.EndHorizontal();
        }

        // ── Одна колекція ──────────────────────────────────────────────────────
        private void DrawCollection(SerializedObject so, SerializedProperty collections, int index)
        {
            var col = collections.GetArrayElementAtIndex(index);
            var idProp = col.FindPropertyRelative("CollectionId");
            string label = string.IsNullOrWhiteSpace(idProp?.stringValue)
                ? $"Колекція #{index}"
                : idProp.stringValue;

            bool isOpen = _openIndex == index;

            // Фон для активної колекції
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isOpen ? new Color(0.75f, 0.92f, 1f) : new Color(0.95f, 0.95f, 0.95f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            // ── Рядок з кнопками ──
            EditorGUILayout.BeginHorizontal();

            bool newOpen = EditorGUILayout.Foldout(isOpen, label, true, EditorStyles.foldoutHeader);
            if (newOpen != isOpen)
                _openIndex = newOpen ? index : -1;

            DrawValidationIcon(col);

            if (GUILayout.Button("✕", GUILayout.Width(22f), GUILayout.Height(18f)))
            {
                if (EditorUtility.DisplayDialog(
                    "Видалити колекцію?",
                    $"Видалити колекцію «{label}»?\nЦю дію не можна скасувати.",
                    "Видалити", "Скасувати"))
                {
                    so.ApplyModifiedProperties();
                    collections.DeleteArrayElementAtIndex(index);
                    so.ApplyModifiedProperties();
                    if (_openIndex >= collections.arraySize)
                        _openIndex = collections.arraySize - 1;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            // ── Тіло (якщо розкрито) ──
            if (_openIndex == index)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(4f);

                DrawIds(col);
                EditorGUILayout.Space(6f);
                DrawWallVariants(col);
                EditorGUILayout.Space(6f);
                DrawGate(col);
                EditorGUILayout.Space(4f);
                DrawValidationMessages(col);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3f);
        }

        // ── Секція ідентифікаторів ─────────────────────────────────────────────
        private static void DrawIds(SerializedProperty col)
        {
            EditorGUILayout.LabelField("Ідентифікатори", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                col.FindPropertyRelative("CollectionId"),
                new GUIContent("ID колекції",
                    "Унікальний ідентифікатор колекції. " +
                    "Приклади: 'stone-wall', 'wooden-wall', 'castle-wall'."));
            EditorGUILayout.PropertyField(
                col.FindPropertyRelative("WallBuildingId"),
                new GUIContent("ID стіни",
                    "BuildingId сегмента стіни у BuildingRegistrySO. " +
                    "Присвоюється тайлу при розміщенні будь-якого варіанта стіни."));
            EditorGUILayout.PropertyField(
                col.FindPropertyRelative("GateBuildingId"),
                new GUIContent("ID воріт",
                    "BuildingId воріт у BuildingRegistrySO. " +
                    "Ворота замінюють стіну тієї ж колекції."));
        }

        // ── Секція 6 варіантів стіни ───────────────────────────────────────────
        private static void DrawWallVariants(SerializedProperty col)
        {
            EditorGUILayout.LabelField("Варіанти стіни (6 штук)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Варіант обирається автоматично за кількістю та розташуванням сусідів-стін.",
                MessageType.None);
            EditorGUILayout.Space(2f);

            foreach (var (field, uaName, uaHint) in VariantMeta)
                DrawPrefabSlot(col, field, uaName, uaHint);
        }

        // ── Секція воріт ───────────────────────────────────────────────────────
        private static void DrawGate(SerializedProperty col)
        {
            EditorGUILayout.LabelField("Ворота", EditorStyles.boldLabel);
            DrawPrefabSlot(col, "GatePrefab", "Prefab воріт",
                "Прохідний сегмент. Замінює стіну цієї ж колекції. " +
                "Встановлюється лише на тайл з існуючою стіною (WallBuildingId).");
        }

        // ── Одне поле prefab зі стилізацією та tooltipом ───────────────────────
        private static void DrawPrefabSlot(SerializedProperty col, string fieldName,
            string uaLabel, string uaTooltip)
        {
            var prop = col.FindPropertyRelative(fieldName);
            if (prop == null) return;

            bool missing = prop.objectReferenceValue == null;
            Color prev = GUI.color;
            if (missing) GUI.color = new Color(1f, 0.72f, 0.72f);

            EditorGUILayout.PropertyField(prop, new GUIContent(uaLabel, uaTooltip));

            GUI.color = prev;
        }

        // ── Іконка валідації поруч із заголовком ──────────────────────────────
        private static void DrawValidationIcon(SerializedProperty col)
        {
            int missing = CountMissingPrefabs(col);
            if (missing == 0)
            {
                GUI.color = Color.green;
                GUILayout.Label("✓", GUILayout.Width(18f));
            }
            else
            {
                GUI.color = new Color(1f, 0.6f, 0f);
                GUILayout.Label($"⚠{missing}", GUILayout.Width(26f));
            }
            GUI.color = Color.white;
        }

        // ── Повідомлення валідації всередині розкритого блоку ─────────────────
        private static void DrawValidationMessages(SerializedProperty col)
        {
            var missing = new List<string>();

            foreach (var (field, uaName, _) in VariantMeta)
            {
                var p = col.FindPropertyRelative(field);
                if (p != null && p.objectReferenceValue == null)
                    missing.Add(uaName);
            }

            var gateProp = col.FindPropertyRelative("GatePrefab");
            if (gateProp != null && gateProp.objectReferenceValue == null)
                missing.Add("Ворота");

            if (missing.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ Усі prefab'и призначено.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"⚠ Відсутні prefab'и ({missing.Count}):\n• " + string.Join("\n• ", missing),
                    MessageType.Warning);
            }
        }

        // ── Лічильник відсутніх prefab'ів ─────────────────────────────────────
        private static int CountMissingPrefabs(SerializedProperty col)
        {
            int count = 0;
            foreach (var (field, _, _) in VariantMeta)
            {
                var p = col.FindPropertyRelative(field);
                if (p != null && p.objectReferenceValue == null) count++;
            }
            var g = col.FindPropertyRelative("GatePrefab");
            if (g != null && g.objectReferenceValue == null) count++;
            return count;
        }

        // ── Автоматичний пошук реєстру ────────────────────────────────────────
        private void AutoFindRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingRegistrySO");
            if (guids.Length == 0)
            {
                ShowNotification(new GUIContent("BuildingRegistrySO не знайдено в проєкті"));
                return;
            }
            _registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
