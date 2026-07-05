using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
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
        private const string DefaultPrefabRoot = "Assets/Moyva/Prefabs/Buildings/Walls";

        // ── Метадані варіантів (fieldName, UA назва, UA підказка) ─────────────
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
             "Атласний слот NE (правий верхній).\n" +
             "Логічні сусіди для цього слоту: знизу (S) і ліворуч (W)."),

            ("CornerNorthWestPrefab",
             "Кут лівий верхній — NW ↑←",
             "Атласний слот NW (лівий верхній).\n" +
             "Логічні сусіди для цього слоту: знизу (S) і праворуч (E)."),

            ("CornerSouthEastPrefab",
             "Кут правий нижній — SE ↓→",
             "Атласний слот SE (правий нижній).\n" +
             "Логічні сусіди для цього слоту: зверху (N) і ліворуч (W)."),

            ("CornerSouthWestPrefab",
             "Кут лівий нижній — SW ↓←",
             "Атласний слот SW (лівий нижній).\n" +
             "Логічні сусіди для цього слоту: зверху (N) і праворуч (E)."),
        };

        // ── Стан вікна ─────────────────────────────────────────────────────────
        private BuildingRegistrySO _registry;
        private Vector2 _scroll;
        private int _openIndex = -1;
        private int _generatorTargetIndex = -1;
        private bool _isGeneratorExpanded = true;
        private bool _isResolverModuleExpanded;
        private readonly TopologyResolverEditorModule _resolverModule = new();

        // ── Автогенерація зі спрайтів ──────────────────────────────────────────
        private string _generatorPrefabRoot = DefaultPrefabRoot;
        private int _generatorSortingOrder = 2;
        private float _generatorPixelsPerUnit = 100f;
        private bool _generatorCreateBuildingEntries;
        private bool _warnedAssetDefinitionFlow;
        private bool _generatorOverwriteBuildingPrefab = false;
        private bool _generatorOverwriteBuildingIcon = false;
        private bool _generatorAutoSaveAssets = true;

        private Sprite _spriteHorizontal;
        private Sprite _spriteVertical;
        private Sprite _spriteCornerNE;
        private Sprite _spriteCornerNW;
        private Sprite _spriteCornerSE;
        private Sprite _spriteCornerSW;
        private Sprite _spriteGate;

        // ── Стан UI стилів ─────────────────────────────────────────────────────
        private GUIStyle _titleStyle;
        private GUIStyle _sectionHeaderStyle;
        private GUIStyle _boxedContentStyle;

        // ── Відкрити вікно ─────────────────────────────────────────────────────
        [MenuItem("Moyva/Tools/Construction/Wall Registry", priority = 36)]
        public static void Open()
        {
            var window = GetWindow<WallRegistryWindow>("Реєстр стін");
            window.minSize = new Vector2(600f, 680f);
            window.Show();
        }

        private void OnEnable()
        {
            InitStyles();
        }

        // ── GUI ────────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            InitStyles();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();

            using (new EditorGUILayout.VerticalScope(_boxedContentStyle))
            {
                _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField(
                    new GUIContent("BuildingRegistrySO", "ScriptableObject реєстру будівель"),
                    _registry, typeof(BuildingRegistrySO), false);

                if (_registry == null)
                {
                    EditorGUILayout.HelpBox(
                        "Перетягніть BuildingRegistrySO із проєкту або натисніть «Знайти автоматично».\n" +
                        "Без реєстру інструмент не може автогенерувати prefab'и і виконувати сетап.",
                        MessageType.Warning);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Знайти автоматично", GUILayout.Height(26f)))
                            AutoFindRegistry();

                        if (GUILayout.Button("Створити новий BuildingRegistrySO", GUILayout.Height(26f)))
                            CreateRegistryAsset();
                    }
                    EditorGUILayout.EndScrollView();
                    return;
                }
            }

            EditorGUILayout.Space(4f);

            var so = new SerializedObject(_registry);
            so.Update();
            var collections = so.FindProperty("WallCollections");

            DrawToolbar(collections);
            EditorGUILayout.Space(6f);
            DrawAutomationPanel(so, collections);
            EditorGUILayout.Space(6f);
            DrawResolverModulePanel();
            EditorGUILayout.Space(6f);

            for (int i = 0; i < collections.arraySize; i++)
                DrawCollection(so, collections, i);

            // Авто-синхронізація BuildingDefinition для всіх колекцій (замість ручної кнопки)
            if (so.hasModifiedProperties)
            {
                for (int i = 0; i < collections.arraySize; i++)
                    EnsureBuildingEntries(so, collections.GetArrayElementAtIndex(i), collectOnly: false);
            }

            EditorGUILayout.EndScrollView();

            so.ApplyModifiedProperties();
        }

        // ── Заголовок ──────────────────────────────────────────────────────────
        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(_boxedContentStyle))
            {
                EditorGUILayout.LabelField("Редактор колекцій стін", _titleStyle);
                EditorGUILayout.HelpBox(
                    "Що вміє інструмент:\n" +
                    "• Створювати/редагувати колекції стін\n" +
                    "• Генерувати prefab'и з переданих спрайтів\n" +
                    "• Автоматично створювати або оновлювати BuildingDefinition для стін/воріт\n" +
                    "• Проставляти fallback'и, якщо частина варіантів відсутня\n\n" +
                    "Рекомендований workflow: 1) обрати реєстр 2) обрати/створити колекцію 3) заповнити спрайти 4) натиснути «Згенерувати і сетапити».",
                    MessageType.Info);
            }
        }

        // ── Панель інструментів ────────────────────────────────────────────────
        private void DrawToolbar(SerializedProperty collections)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField($"Колекцій: {collections.arraySize}", EditorStyles.boldLabel,
                GUILayout.Width(110f));

            if (_openIndex >= 0 && _openIndex < collections.arraySize)
            {
                int missing = CountMissingPrefabs(collections.GetArrayElementAtIndex(_openIndex));
                string status = missing == 0 ? "Готово до релізу" : $"Потрібно заповнити: {missing}";
                EditorGUILayout.LabelField(status, GUILayout.Width(170f));
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Знайти реєстр", EditorStyles.toolbarButton, GUILayout.Width(100f)))
                AutoFindRegistry();

            if (GUILayout.Button("+ Додати колекцію", EditorStyles.toolbarButton, GUILayout.Width(130f)))
            {
                collections.arraySize++;
                _openIndex = collections.arraySize - 1;
                _generatorTargetIndex = _openIndex;

                var newItem = collections.GetArrayElementAtIndex(_openIndex);
                newItem.FindPropertyRelative("CollectionId").stringValue = $"wall-collection-{_openIndex}";
                newItem.FindPropertyRelative("WallBuildingId").stringValue = "wall";
                newItem.FindPropertyRelative("GateBuildingId").stringValue = "gate";
            }

            if (GUILayout.Button("Resolver Editor", EditorStyles.toolbarButton, GUILayout.Width(110f)))
                TopologyResolverEditorWindow.Open();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawResolverModulePanel()
        {
            using (new EditorGUILayout.VerticalScope(_boxedContentStyle))
            {
                _isResolverModuleExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
                    _isResolverModuleExpanded,
                    new GUIContent("Вбудований модуль резолвера", "Редагування декларативної логіки підбору case -> варіації"));

                if (_isResolverModuleExpanded)
                    _resolverModule.Draw(_registry);

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        // ── Майстер автогенерації ─────────────────────────────────────────────
        private void DrawAutomationPanel(SerializedObject so, SerializedProperty collections)
        {
            using (new EditorGUILayout.VerticalScope(_boxedContentStyle))
            {
                _isGeneratorExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
                    _isGeneratorExpanded,
                    new GUIContent(
                        "Майстер автогенерації зі спрайтів",
                        "Згорніть або розгорніть цей блок. Всередині знаходяться всі інструменти швидкого створення стін."));

                if (_isGeneratorExpanded)
                {
                    EditorGUILayout.HelpBox(
                        "Передайте спрайти, натисніть кнопку і інструмент автоматично:\n" +
                        "1) Згенерує prefab'и у вказаній папці\n" +
                        "2) Призначить prefab'и у вибрану колекцію\n" +
                        "3) Створить/оновить BuildingDefinition для стіни та воріт\n" +
                        "4) Застосує fallback'и для відсутніх варіантів",
                        MessageType.Info);

                    DrawGeneratorTargetSelector(collections);
                    DrawGeneratorSourceSprites();
                    DrawGeneratorOptions();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(
                            new GUIContent("Згенерувати і сетапити", "Запускає повний цикл генерації та автоналаштування"),
                            GUILayout.Height(32f)))
                        {
                            RunAutoGeneration(so, collections);
                        }

                        if (GUILayout.Button(
                            new GUIContent("Очистити введення", "Скидає лише спрайти/опції генератора, не чіпає реєстр"),
                            GUILayout.Height(32f), GUILayout.Width(140f)))
                        {
                            ResetGeneratorInputs();
                        }
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void DrawGeneratorTargetSelector(SerializedProperty collections)
        {
            EditorGUILayout.LabelField("1) Куди генерувати", EditorStyles.boldLabel);

            if (collections.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Спочатку додайте хоча б одну колекцію натисканням «+ Додати колекцію».", MessageType.Warning);
                return;
            }

            if (_generatorTargetIndex < 0 || _generatorTargetIndex >= collections.arraySize)
                _generatorTargetIndex = Mathf.Clamp(_openIndex, 0, collections.arraySize - 1);

            string[] names = new string[collections.arraySize];
            for (int i = 0; i < collections.arraySize; i++)
            {
                var item = collections.GetArrayElementAtIndex(i);
                string id = item.FindPropertyRelative("CollectionId").stringValue;
                names[i] = string.IsNullOrWhiteSpace(id) ? $"Колекція #{i}" : id;
            }

            _generatorTargetIndex = EditorGUILayout.Popup(
                new GUIContent("Цільова колекція", "У цю колекцію будуть записані згенеровані prefab'и"),
                _generatorTargetIndex, names);

            _generatorPrefabRoot = EditorGUILayout.TextField(
                new GUIContent("Папка для prefab'ів", "Наприклад: Assets/Moyva/Prefabs/Buildings/Walls"),
                string.IsNullOrWhiteSpace(_generatorPrefabRoot) ? DefaultPrefabRoot : _generatorPrefabRoot);

            EditorGUILayout.HelpBox(
                "Структура буде створена автоматично, якщо папки не існують.",
                MessageType.None);
        }

        private void DrawGeneratorSourceSprites()
        {
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("2) Джерело: спрайти", EditorStyles.boldLabel);

            _spriteHorizontal = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Горизонтальна ←→", "Базовий fallback. Якщо бракує інших варіантів, система використає цей спрайт."),
                _spriteHorizontal, typeof(Sprite), false);

            _spriteVertical = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Вертикальна ↑↓", "Вертикальний сегмент стіни."),
                _spriteVertical, typeof(Sprite), false);

            _spriteCornerNE = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Кут NE (правий верхній)", "Атласний слот NE. Логічні сусіди: S + W."),
                _spriteCornerNE, typeof(Sprite), false);

            _spriteCornerNW = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Кут NW (лівий верхній)", "Атласний слот NW. Логічні сусіди: S + E."),
                _spriteCornerNW, typeof(Sprite), false);

            _spriteCornerSE = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Кут SE (правий нижній)", "Атласний слот SE. Логічні сусіди: N + W."),
                _spriteCornerSE, typeof(Sprite), false);

            _spriteCornerSW = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Кут SW (лівий нижній)", "Атласний слот SW. Логічні сусіди: N + E."),
                _spriteCornerSW, typeof(Sprite), false);

            _spriteGate = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Ворота", "Спрайт для воріт. Якщо порожньо — fallback на горизонтальну стіну."),
                _spriteGate, typeof(Sprite), false);
        }

        private void DrawGeneratorOptions()
        {
            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("3) Опції генерації", EditorStyles.boldLabel);

            _generatorSortingOrder = EditorGUILayout.IntField(
                new GUIContent("SpriteRenderer Sorting Order", "Порядок рендера для згенерованих prefab'ів"),
                _generatorSortingOrder);

            _generatorPixelsPerUnit = EditorGUILayout.FloatField(
                new GUIContent("Pixels Per Unit (інфо)", "Поля лише для документації процесу. Імпортний PPU змінюється в Sprite Import Settings."),
                _generatorPixelsPerUnit);

            _generatorCreateBuildingEntries = EditorGUILayout.ToggleLeft(
                new GUIContent("Відкрити Build Designer для wall/gate assets",
                    "Inline BuildingRegistrySO.Buildings більше не створюється. Wall/Gate definitions треба створювати як BuildingDefinition assets."),
                _generatorCreateBuildingEntries);

            _generatorOverwriteBuildingPrefab = EditorGUILayout.ToggleLeft(
                new GUIContent("Перезаписувати Prefab у вже існуючих BuildingDefinition",
                    "Якщо вимкнено — Prefab змінюється лише коли був порожній"),
                _generatorOverwriteBuildingPrefab);

            _generatorOverwriteBuildingIcon = EditorGUILayout.ToggleLeft(
                new GUIContent("Перезаписувати Icon у вже існуючих BuildingDefinition",
                    "Якщо вимкнено — Icon змінюється лише коли була порожня"),
                _generatorOverwriteBuildingIcon);

            _generatorAutoSaveAssets = EditorGUILayout.ToggleLeft(
                new GUIContent("Автоматично SaveAssets після генерації",
                    "Рекомендовано залишити увімкненим"),
                _generatorAutoSaveAssets);
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

            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = isOpen ? new Color(0.75f, 0.92f, 1f) : new Color(0.95f, 0.95f, 0.95f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = prevBg;

            // ── Рядок з кнопками ──
            EditorGUILayout.BeginHorizontal();

            bool newOpen = EditorGUILayout.Foldout(isOpen, label, true, EditorStyles.foldoutHeader);
            if (newOpen != isOpen)
            {
                _openIndex = newOpen ? index : -1;
                if (newOpen)
                    _generatorTargetIndex = index;
            }

            DrawReadinessMiniBar(col);
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

                    if (_generatorTargetIndex >= collections.arraySize)
                        _generatorTargetIndex = collections.arraySize - 1;
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

                EditorGUILayout.HelpBox(
                    "Порада: якщо хочете швидко згенерувати всю колекцію зі спрайтів, використайте «Майстер автогенерації» вище.",
                    MessageType.None);

                DrawIds(col);
                EditorGUILayout.Space(6f);
                DrawGate(col);
                EditorGUILayout.Space(6f);
                EditorGUILayout.HelpBox(
                    "Налаштування типів стін і варіацій виконується через вбудований модуль резолвера вище. " +
                    "У цьому блоці лишаються тільки ID колекції та ворота.",
                    MessageType.Info);
                EditorGUILayout.Space(6f);
                DrawPerCollectionActions(col);
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

            EditorGUILayout.HelpBox(
                "Fallback для ID: якщо залишити порожньо, інструмент при автогенерації підставить 'wall' і 'gate'.",
                MessageType.None);
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

            EditorGUILayout.HelpBox(
                "Рекомендація: призначайте окремий prefab воріт. Якщо GatePrefab порожній, система використовує fallback колекції.",
                MessageType.None);
        }

        private void DrawPerCollectionActions(SerializedProperty col)
        {
            EditorGUILayout.LabelField("Операції для цієї колекції", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(
                    new GUIContent("Відкрити Build Designer",
                        "Wall/Gate definitions створюються як BuildingDefinition assets, а не inline записи в registry."),
                    GUILayout.Height(28f), GUILayout.Width(250f)))
                {
                    EditorApplication.ExecuteMenuItem("Moyva/Tools/Building Designer");
                }
            }
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

        private static void DrawReadinessMiniBar(SerializedProperty col)
        {
            int total = VariantMeta.Length + 1; // + gate
            int missing = CountMissingPrefabs(col);
            float ready = Mathf.Clamp01((float)(total - missing) / total);

            Rect r = GUILayoutUtility.GetRect(80f, 6f, GUILayout.Width(80f), GUILayout.ExpandWidth(false));
            EditorGUI.ProgressBar(r, ready, string.Empty);
        }

        // ── Повідомлення валідації всередині розкритого блоку ─────────────────
        private static void DrawValidationMessages(SerializedProperty col)
        {
            var missing = new List<string>();

            var bindings = col.FindPropertyRelative("TopologyBindings");
            bool hasAnyBinding = bindings != null && bindings.arraySize > 0;
            if (!hasAnyBinding)
                missing.Add("Типи резолвера (додайте через вбудований модуль)");

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

            var bindings = col.FindPropertyRelative("TopologyBindings");
            bool hasAnyBinding = bindings != null && bindings.arraySize > 0;
            if (!hasAnyBinding)
                count++;

            var g = col.FindPropertyRelative("GatePrefab");
            if (g != null && g.objectReferenceValue == null) count++;
            return count;
        }

        private static void ApplyFallbacks(SerializedProperty col)
        {
            var horizontal = col.FindPropertyRelative("HorizontalPrefab");
            var vertical = col.FindPropertyRelative("VerticalPrefab");

            Object fallback = horizontal?.objectReferenceValue
                ?? vertical?.objectReferenceValue
                ?? FindFirstExistingPrefab(col);

            if (fallback == null)
            {
                EditorUtility.DisplayDialog(
                    "Fallback неможливий",
                    "У колекції немає жодного prefab, який можна використати як fallback.\n" +
                    "Призначте хоча б HorizontalPrefab або VerticalPrefab.",
                    "OK");
                return;
            }

            foreach (var (field, _, _) in VariantMeta)
            {
                var p = col.FindPropertyRelative(field);
                if (p != null && p.objectReferenceValue == null)
                    p.objectReferenceValue = fallback;
            }

            var gate = col.FindPropertyRelative("GatePrefab");
            if (gate != null && gate.objectReferenceValue == null)
                gate.objectReferenceValue = fallback;
        }

        private static Object FindFirstExistingPrefab(SerializedProperty col)
        {
            foreach (var (field, _, _) in VariantMeta)
            {
                var p = col.FindPropertyRelative(field);
                if (p != null && p.objectReferenceValue != null)
                    return p.objectReferenceValue;
            }

            var gate = col.FindPropertyRelative("GatePrefab");
            return gate?.objectReferenceValue;
        }

        private static void ApplyFallbacksToAll(SerializedProperty collections)
        {
            for (int i = 0; i < collections.arraySize; i++)
                ApplyFallbacks(collections.GetArrayElementAtIndex(i));
        }

        private void RunAutoGeneration(SerializedObject so, SerializedProperty collections)
        {
            if (collections.arraySize == 0)
            {
                EditorUtility.DisplayDialog("Немає колекцій", "Додайте хоча б одну колекцію перед генерацією.", "OK");
                return;
            }

            if (_generatorTargetIndex < 0 || _generatorTargetIndex >= collections.arraySize)
            {
                EditorUtility.DisplayDialog("Не обрано колекцію", "Оберіть цільову колекцію для генерації.", "OK");
                return;
            }

            var col = collections.GetArrayElementAtIndex(_generatorTargetIndex);
            EnsureCollectionDefaults(col);

            string collectionId = col.FindPropertyRelative("CollectionId").stringValue;
            if (string.IsNullOrWhiteSpace(collectionId))
                collectionId = $"wall-collection-{_generatorTargetIndex}";

            string collectionFolder = CombineAssetPath(
                string.IsNullOrWhiteSpace(_generatorPrefabRoot) ? DefaultPrefabRoot : _generatorPrefabRoot,
                SanitizeFileName(collectionId));

            EnsureFolder(collectionFolder);

            Sprite horizontalSprite = _spriteHorizontal;
            Sprite verticalSprite = _spriteVertical ?? horizontalSprite;
            Sprite neSprite = _spriteCornerNE ?? horizontalSprite ?? verticalSprite;
            Sprite nwSprite = _spriteCornerNW ?? horizontalSprite ?? verticalSprite;
            Sprite seSprite = _spriteCornerSE ?? horizontalSprite ?? verticalSprite;
            Sprite swSprite = _spriteCornerSW ?? horizontalSprite ?? verticalSprite;
            Sprite gateSprite = _spriteGate ?? horizontalSprite ?? verticalSprite;

            if (horizontalSprite == null && verticalSprite == null)
            {
                EditorUtility.DisplayDialog(
                    "Недостатньо даних",
                    "Для генерації потрібен хоча б один базовий спрайт: Horizontal або Vertical.",
                    "OK");
                return;
            }

            SetPrefabProperty(col, "HorizontalPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Horizontal", horizontalSprite));
            SetPrefabProperty(col, "VerticalPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Vertical", verticalSprite));
            SetPrefabProperty(col, "CornerNorthEastPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Corner_NE", neSprite));
            SetPrefabProperty(col, "CornerNorthWestPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Corner_NW", nwSprite));
            SetPrefabProperty(col, "CornerSouthEastPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Corner_SE", seSprite));
            SetPrefabProperty(col, "CornerSouthWestPrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Corner_SW", swSprite));
            SetPrefabProperty(col, "GatePrefab", CreateWallPrefabAsset(collectionFolder, collectionId, "Gate", gateSprite));

            ApplyFallbacks(col);
            EnsureBuildingEntries(so, col, collectOnly: false);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(_registry);
            AssetDatabase.Refresh();
            if (_generatorAutoSaveAssets)
                AssetDatabase.SaveAssets();

            ShowNotification(new GUIContent("Згенеровано і сетаплено успішно"));
            Repaint();
        }

        private static void EnsureCollectionDefaults(SerializedProperty col)
        {
            var collectionId = col.FindPropertyRelative("CollectionId");
            var wallId = col.FindPropertyRelative("WallBuildingId");
            var gateId = col.FindPropertyRelative("GateBuildingId");

            if (collectionId != null && string.IsNullOrWhiteSpace(collectionId.stringValue))
                collectionId.stringValue = "wall-collection";

            if (wallId != null && string.IsNullOrWhiteSpace(wallId.stringValue))
                wallId.stringValue = "wall";

            if (gateId != null && string.IsNullOrWhiteSpace(gateId.stringValue))
                gateId.stringValue = "gate";
        }

        private static void SetPrefabProperty(SerializedProperty col, string propertyName, GameObject prefab)
        {
            if (prefab == null)
                return;

            var p = col.FindPropertyRelative(propertyName);
            if (p != null)
                p.objectReferenceValue = prefab;
        }

        private GameObject CreateWallPrefabAsset(string folder, string collectionId, string variant, Sprite sprite)
        {
            if (sprite == null)
                return null;

            string safeCollection = SanitizeFileName(collectionId);
            string safeVariant = SanitizeFileName(variant);
            string fileName = $"{safeCollection}_{safeVariant}.prefab";
            string path = AssetDatabase.GenerateUniqueAssetPath(CombineAssetPath(folder, fileName));

            var go = new GameObject($"{safeCollection}_{safeVariant}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = _generatorSortingOrder;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            return prefab;
        }

        private void EnsureBuildingEntries(SerializedObject so, SerializedProperty col, bool collectOnly)
        {
            if (collectOnly || !_generatorCreateBuildingEntries)
                return;

            if (!_warnedAssetDefinitionFlow)
            {
                _warnedAssetDefinitionFlow = true;
                Debug.LogWarning(
                    "[WallRegistry] Inline BuildingDefinition creation is disabled. " +
                    "Create wall/gate BuildingDefinition assets in Moyva/Tools/Building Designer.",
                    _registry);
            }

            EditorApplication.ExecuteMenuItem("Moyva/Tools/Building Designer");
            return;

            /*
            var buildings = so.FindProperty("Buildings");
            if (buildings == null)
                return;

            string wallId = col.FindPropertyRelative("WallBuildingId")?.stringValue;
            string gateId = col.FindPropertyRelative("GateBuildingId")?.stringValue;
            if (string.IsNullOrWhiteSpace(wallId)) wallId = "wall";
            if (string.IsNullOrWhiteSpace(gateId)) gateId = "gate";

            var horizontalPrefab = col.FindPropertyRelative("HorizontalPrefab")?.objectReferenceValue as GameObject;
            var gatePrefab = col.FindPropertyRelative("GatePrefab")?.objectReferenceValue as GameObject;
            var horizontalIcon = _spriteHorizontal ?? ExtractSpriteFromPrefab(horizontalPrefab);
            var gateIcon = _spriteGate ?? ExtractSpriteFromPrefab(gatePrefab) ?? horizontalIcon;

            UpsertBuildingDefinition(
                buildings,
                wallId,
                "Стіна",
                horizontalIcon,
                horizontalPrefab,
                BuildingCategory.Walls,
                collectOnly);

            UpsertBuildingDefinition(
                buildings,
                gateId,
                "Ворота",
                gateIcon,
                gatePrefab,
                BuildingCategory.Walls,
                collectOnly);
            */
        }

        private static Sprite ExtractSpriteFromPrefab(GameObject prefab)
        {
            return AdaptivePrefabPreviewUtility.TryGetPrimarySprite(prefab, out var sprite, out _)
                ? sprite
                : null;
        }

        private void UpsertBuildingDefinition(
            SerializedProperty buildings,
            string id,
            string displayName,
            Sprite icon,
            GameObject prefab,
            BuildingCategory category,
            bool collectOnly)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            SerializedProperty target = null;
            for (int i = 0; i < buildings.arraySize; i++)
            {
                var item = buildings.GetArrayElementAtIndex(i);
                var idProp = item.FindPropertyRelative("Id");
                if (idProp != null && idProp.stringValue == id)
                {
                    target = item;
                    break;
                }
            }

            if (target == null)
            {
                if (collectOnly)
                    return;

                buildings.arraySize++;
                target = buildings.GetArrayElementAtIndex(buildings.arraySize - 1);
                target.FindPropertyRelative("Id").stringValue = id;
                target.FindPropertyRelative("DisplayName").stringValue = displayName;
                target.FindPropertyRelative("Category").enumValueIndex = (int)category;
            }

            var nameProp = target.FindPropertyRelative("DisplayName");
            if (nameProp != null && string.IsNullOrWhiteSpace(nameProp.stringValue))
                nameProp.stringValue = displayName;

            var categoryProp = target.FindPropertyRelative("Category");
            if (categoryProp != null)
                categoryProp.enumValueIndex = (int)category;

            var iconProp = target.FindPropertyRelative("Icon");
            if (iconProp != null)
            {
                if (icon != null && iconProp.objectReferenceValue != icon)
                    iconProp.objectReferenceValue = icon;
            }

            var prefabProp = target.FindPropertyRelative("Prefab");
            if (prefabProp != null)
            {
                if (prefab != null && prefabProp.objectReferenceValue != prefab)
                    prefabProp.objectReferenceValue = prefab;
            }
        }

        private void ResetGeneratorInputs()
        {
            _generatorPrefabRoot = DefaultPrefabRoot;
            _generatorSortingOrder = 2;
            _generatorPixelsPerUnit = 100f;
            _generatorCreateBuildingEntries = false;
            _generatorOverwriteBuildingPrefab = false;
            _generatorOverwriteBuildingIcon = false;
            _generatorAutoSaveAssets = true;

            _spriteHorizontal = null;
            _spriteVertical = null;
            _spriteCornerNE = null;
            _spriteCornerNW = null;
            _spriteCornerSE = null;
            _spriteCornerSW = null;
            _spriteGate = null;
        }

        private static string CombineAssetPath(string left, string right)
        {
            left = (left ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            right = (right ?? string.Empty).Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrEmpty(left)) return right;
            if (string.IsNullOrEmpty(right)) return left;
            return $"{left}/{right}";
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "wall";

            string cleaned = value.Trim();
            cleaned = cleaned.Replace(' ', '-');

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalid)
                cleaned = cleaned.Replace(c.ToString(), string.Empty);

            return string.IsNullOrWhiteSpace(cleaned) ? "wall" : cleaned;
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            if (parts.Length == 0)
                return;

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
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

        private void CreateRegistryAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Створити BuildingRegistrySO",
                "BuildingRegistry",
                "asset",
                "Оберіть місце для нового BuildingRegistrySO");

            if (string.IsNullOrWhiteSpace(path))
                return;

            var asset = CreateInstance<BuildingRegistrySO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _registry = asset;
            ShowNotification(new GUIContent("BuildingRegistrySO створено"));
        }

        private void InitStyles()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_sectionHeaderStyle == null)
            {
                _sectionHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 12,
                    richText = true,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (_boxedContentStyle == null)
            {
                _boxedContentStyle = new GUIStyle("HelpBox")
                {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(6, 6, 4, 4)
                };
            }
        }
    }
}
