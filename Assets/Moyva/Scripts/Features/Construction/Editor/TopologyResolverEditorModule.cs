using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    internal sealed class TopologyResolverEditorModule
    {
        private const string DefaultPrefabRoot = "Assets/Moyva/Prefabs/Buildings/ResolverGenerated";
        private static readonly (string Field, TopologyCaseType CaseType, string Suffix)[] LegacyFieldMap =
        {
            ("HorizontalPrefab", TopologyCaseType.Horizontal, "horizontal"),
            ("VerticalPrefab", TopologyCaseType.Vertical, "vertical"),
            ("CornerNorthEastPrefab", TopologyCaseType.CornerNorthEast, "corner_ne"),
            ("CornerNorthWestPrefab", TopologyCaseType.CornerNorthWest, "corner_nw"),
            ("CornerSouthEastPrefab", TopologyCaseType.CornerSouthEast, "corner_se"),
            ("CornerSouthWestPrefab", TopologyCaseType.CornerSouthWest, "corner_sw"),
        };

        private readonly Dictionary<string, DraftState> _draftByKey = new();
        private readonly Dictionary<string, bool> _caseFoldout = new();

        private string _prefabRoot = DefaultPrefabRoot;

        private sealed class DraftState
        {
            public string VariantId = string.Empty;
            public GameObject Prefab;
            public Sprite Sprite;
            public bool AutoIdSeeded;
        }

        public void Draw(UnityEngine.Object registryAsset)
        {
            var adapter = ResolverRegistryAdapterFactory.Resolve(registryAsset);
            if (adapter == null)
            {
                EditorGUILayout.HelpBox(
                    "Для цього типу реєстру ще не підключено адаптер резолвера. " +
                    "Додайте нову реалізацію IResolverRegistryAdapter.",
                    MessageType.Warning);
                return;
            }

            var so = adapter.CreateSerializedObject(registryAsset);
            if (so == null)
            {
                EditorGUILayout.HelpBox("Не вдалося створити SerializedObject для реєстру.", MessageType.Error);
                return;
            }

            so.Update();
            var collections = adapter.GetCollectionsProperty(so);
            if (collections == null)
            {
                EditorGUILayout.HelpBox(
                    "Адаптер не знайшов поле колекцій. Перевірте реалізацію адаптера для цього реєстру.",
                    MessageType.Error);
                return;
            }

            DrawHeader(adapter.AdapterName);
            _prefabRoot = EditorGUILayout.TextField(
                new GUIContent("Папка для auto-prefab", "Сюди зберігаються prefab, згенеровані зі спрайтів"),
                string.IsNullOrWhiteSpace(_prefabRoot) ? DefaultPrefabRoot : _prefabRoot);

            if (collections.arraySize == 0)
            {
                EditorGUILayout.HelpBox("У реєстрі немає жодної колекції для резолвера.", MessageType.Info);
            }

            for (int i = 0; i < collections.arraySize; i++)
            {
                var col = collections.GetArrayElementAtIndex(i);
                DrawCollection(so, col, i, adapter.GetCollectionId(col));
            }

            if (so.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(registryAsset);
            }
        }

        private static void DrawHeader(string adapterName)
        {
            EditorGUILayout.HelpBox(
                "Універсальний резолвер-панель. " +
                "Працює поверх реєстру через адаптер і дозволяє декларативно керувати case -> варіації ID.\n\n" +
                "Сценарій: \n" +
                "1) Натисніть '+' біля колекції і додайте топологічний тип.\n" +
                "2) Додайте одну або кілька варіацій.\n" +
                "3) Передайте prefab або спрайт (спрайт -> prefab створиться автоматично).\n" +
                "4) ID автоматично зареєструється в реєстрі будівель.",
                MessageType.Info);

            EditorGUILayout.LabelField($"Активний адаптер: {adapterName}", EditorStyles.miniBoldLabel);
        }

        private void DrawCollection(SerializedObject so, SerializedProperty collection, int collectionIndex, string collectionId)
        {
            if (string.IsNullOrWhiteSpace(collectionId))
                collectionId = $"collection-{collectionIndex}";

            var wallId = collection.FindPropertyRelative("WallBuildingId")?.stringValue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Колекція: {collectionId}", EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent("+", "Додати новий тип у цю колекцію"), GUILayout.Width(28f)))
                ShowAddCaseMenu(so, collection, collectionId);

            EditorGUILayout.EndHorizontal();

            var bindings = EnsureBindingsProperty(collection);
            if (bindings.arraySize == 0)
            {
                int imported = TryImportLegacyBindings(so, collection, bindings, collectionId, wallId);
                if (imported > 0)
                {
                    EditorGUILayout.HelpBox(
                        $"Імпортовано {imported} тип(и) з legacy-полів колекції в TopologyBindings.",
                        MessageType.Info);
                }
            }

            int totalCases = Enum.GetValues(typeof(TopologyCaseType)).Length;
            int implementedCases = CountImplementedCases(bindings);
            EditorGUILayout.LabelField($"ID: {collectionId}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Реалізовано: {implementedCases}/{totalCases}", EditorStyles.miniBoldLabel);

            DrawSupportedOverview(bindings);

            if (bindings.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "Немає типів у резолвері для цієї колекції. Додайте тип через '+' або заповніть дані в колекції.",
                    MessageType.None);
            }

            int visibleEditable = 0;
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var binding = bindings.GetArrayElementAtIndex(i);
                var variants = binding.FindPropertyRelative("VariantBuildingIds");
                if (HasAnyVariantId(variants))
                    continue;

                visibleEditable++;
                DrawBinding(so, binding, bindings, i, collectionId, wallId);
            }

            if (visibleEditable == 0)
            {
                EditorGUILayout.HelpBox(
                    "Усі наявні типи вже реалізовані. Для додавання нових використайте '+'.",
                    MessageType.None);
            }

            EditorGUILayout.EndVertical();
        }

        private static int CountImplementedCases(SerializedProperty bindings)
        {
            int count = 0;
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var variants = bindings.GetArrayElementAtIndex(i).FindPropertyRelative("VariantBuildingIds");
                if (HasAnyVariantId(variants))
                    count++;
            }

            return count;
        }

        private static bool HasAnyVariantId(SerializedProperty variants)
        {
            if (variants == null || variants.arraySize == 0)
                return false;

            for (int i = 0; i < variants.arraySize; i++)
            {
                if (!string.IsNullOrWhiteSpace(variants.GetArrayElementAtIndex(i).stringValue))
                    return true;
            }

            return false;
        }

        private static void DrawSupportedOverview(SerializedProperty bindings)
        {
            var implemented = new HashSet<TopologyCaseType>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var b = bindings.GetArrayElementAtIndex(i);
                var variants = b.FindPropertyRelative("VariantBuildingIds");
                if (variants == null || variants.arraySize == 0)
                    continue;

                bool hasId = false;
                for (int j = 0; j < variants.arraySize; j++)
                {
                    if (!string.IsNullOrWhiteSpace(variants.GetArrayElementAtIndex(j).stringValue))
                    {
                        hasId = true;
                        break;
                    }
                }

                if (hasId)
                    implemented.Add((TopologyCaseType)b.FindPropertyRelative("CaseType").enumValueIndex);
            }

            int total = Enum.GetValues(typeof(TopologyCaseType)).Length;
            int implementedCount = implemented.Count;
            int canAdd = total - implementedCount;

            EditorGUILayout.LabelField(
                $"Підтримка типів: реалізовано {implementedCount}/{total}, можна додати ще {canAdd}",
                EditorStyles.miniBoldLabel);
        }

        private static int TryImportLegacyBindings(
            SerializedObject so,
            SerializedProperty collection,
            SerializedProperty bindings,
            string collectionId,
            string wallId)
        {
            if (bindings.arraySize > 0)
                return 0;

            int imported = 0;
            string baseId = !string.IsNullOrWhiteSpace(wallId)
                ? wallId
                : SanitizeFileName(collectionId).ToLowerInvariant();

            for (int i = 0; i < LegacyFieldMap.Length; i++)
            {
                var (field, caseType, suffix) = LegacyFieldMap[i];
                var prefabProp = collection.FindPropertyRelative(field);
                var prefab = prefabProp?.objectReferenceValue as GameObject;
                if (prefab == null)
                    continue;

                string variantId = $"{baseId}_{suffix}";
                UpsertBuildingDefinition(so, variantId, prefab, null);

                bindings.arraySize++;
                var binding = bindings.GetArrayElementAtIndex(bindings.arraySize - 1);
                binding.FindPropertyRelative("CaseType").enumValueIndex = (int)caseType;

                var variants = binding.FindPropertyRelative("VariantBuildingIds");
                variants.ClearArray();
                variants.arraySize = 1;
                variants.GetArrayElementAtIndex(0).stringValue = variantId;
                imported++;
            }

            return imported;
        }

        private void DrawBinding(
            SerializedObject so,
            SerializedProperty binding,
            SerializedProperty parentBindings,
            int bindingIndex,
            string collectionId,
            string defaultWallId)
        {
            var caseProp = binding.FindPropertyRelative("CaseType");
            var variantsProp = binding.FindPropertyRelative("VariantBuildingIds");
            var caseType = (TopologyCaseType)caseProp.enumValueIndex;
            string key = BuildCaseKey(collectionId, caseType);

            bool expanded = _caseFoldout.TryGetValue(key, out var foldout) ? foldout : true;
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            expanded = EditorGUILayout.Foldout(expanded, GetCaseLabel(caseType), true);
            _caseFoldout[key] = expanded;

            if (GUILayout.Button(new GUIContent("✕", "Видалити цей тип з колекції"), GUILayout.Width(22f)))
            {
                parentBindings.DeleteArrayElementAtIndex(bindingIndex);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(GetCaseTooltip(caseType), MessageType.None);

            if (expanded)
            {
                var availableBuildingIds = CollectBuildingIds(so);
                for (int i = 0; i < variantsProp.arraySize; i++)
                {
                    var variant = variantsProp.GetArrayElementAtIndex(i);
                    DrawExistingVariantRow(variant, variantsProp, i, availableBuildingIds);
                }

                DrawAddVariantBlock(so, collectionId, caseType, defaultWallId, variantsProp);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawExistingVariantRow(
            SerializedProperty variantProp,
            SerializedProperty variants,
            int index,
            IReadOnlyList<string> availableIds)
        {
            EditorGUILayout.BeginHorizontal();
            variantProp.stringValue = DrawIdPopup(
                new GUIContent("Variant ID", "ID, який резолвер може повернути для цього типу"),
                variantProp.stringValue,
                availableIds);

            if (GUILayout.Button(new GUIContent("-", "Видалити варіацію"), GUILayout.Width(22f)))
            {
                variants.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddVariantBlock(
            SerializedObject so,
            string collectionId,
            TopologyCaseType caseType,
            string defaultWallId,
            SerializedProperty variantsProp)
        {
            string key = BuildCaseKey(collectionId, caseType);
            if (!_draftByKey.TryGetValue(key, out var draft))
            {
                draft = new DraftState();
                _draftByKey[key] = draft;
            }

            if (!draft.AutoIdSeeded && string.IsNullOrWhiteSpace(draft.VariantId))
            {
                draft.VariantId = BuildDefaultVariantId(collectionId, caseType, variantsProp.arraySize);
                draft.AutoIdSeeded = true;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Додати варіацію", EditorStyles.miniBoldLabel);

            draft.VariantId = EditorGUILayout.TextField(
                new GUIContent("Новий ID", "ID нового типу. Якщо порожньо — буде згенеровано автоматично"),
                draft.VariantId);

            draft.Prefab = (GameObject)EditorGUILayout.ObjectField(
                new GUIContent("Готовий Prefab", "Передайте prefab напряму, якщо він уже існує"),
                draft.Prefab, typeof(GameObject), false);

            draft.Sprite = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Або лише Sprite", "Якщо prefab порожній, зі спрайта автоматично створиться prefab"),
                draft.Sprite, typeof(Sprite), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(
                    new GUIContent("Додати", "Додати варіацію у тип і синхронізувати її з реєстром"),
                    GUILayout.Width(120f)))
                {
                    string finalId = string.IsNullOrWhiteSpace(draft.VariantId)
                        ? BuildDefaultVariantId(collectionId, caseType, variantsProp.arraySize)
                        : draft.VariantId.Trim();

                    var prefab = draft.Prefab;
                    if (prefab == null && draft.Sprite != null)
                    {
                        string folder = CombineAssetPath(_prefabRoot, SanitizeFileName(collectionId));
                        EnsureFolder(folder);
                        prefab = CreatePrefabFromSprite(folder, finalId, draft.Sprite);
                    }

                    // Якщо нічого не передали, прив'язуємо до базової стіни (якщо є).
                    if (prefab == null && !string.IsNullOrWhiteSpace(defaultWallId))
                    {
                        UpsertBuildingDefinition(so, finalId, null, null);
                    }
                    else
                    {
                        UpsertBuildingDefinition(so, finalId, prefab, draft.Sprite);
                    }

                    if (!ContainsVariantId(variantsProp, finalId))
                    {
                        variantsProp.arraySize++;
                        variantsProp.GetArrayElementAtIndex(variantsProp.arraySize - 1).stringValue = finalId;
                    }

                    draft.VariantId = string.Empty;
                    draft.Prefab = null;
                    draft.Sprite = null;
                    draft.AutoIdSeeded = false;
                }
            }
        }

        private void ShowAddCaseMenu(SerializedObject so, SerializedProperty collection, string collectionId)
        {
            string collectionPath = collection.propertyPath;
            var bindings = EnsureBindingsProperty(collection);
            var used = new HashSet<TopologyCaseType>();
            for (int i = 0; i < bindings.arraySize; i++)
            {
                var b = bindings.GetArrayElementAtIndex(i);
                var c = (TopologyCaseType)b.FindPropertyRelative("CaseType").enumValueIndex;
                used.Add(c);
            }

            var menu = new GenericMenu();
            foreach (TopologyCaseType caseType in Enum.GetValues(typeof(TopologyCaseType)))
            {
                if (used.Contains(caseType))
                    continue;

                var captured = caseType;
                string label = GetCaseLabel(captured);
                menu.AddItem(new GUIContent(label, GetCaseTooltip(captured)), false, () =>
                {
                    AddCaseBinding(so, collectionPath, collectionId, captured);
                });
            }

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("Усі типи вже додані"));

            menu.ShowAsContext();
        }

        private void AddCaseBinding(SerializedObject sourceSo, string collectionPath, string collectionId, TopologyCaseType caseType)
        {
            if (sourceSo?.targetObject == null || string.IsNullOrWhiteSpace(collectionPath))
                return;

            // GenericMenu callback може виконуватися після завершення поточного GUI-циклу,
            // тому беремо свіжий SerializedObject і шукаємо колекцію за propertyPath.
            var freshSo = new SerializedObject(sourceSo.targetObject);
            freshSo.Update();

            var collection = freshSo.FindProperty(collectionPath);
            if (collection == null)
                return;

            var bindings = EnsureBindingsProperty(collection);

            for (int i = 0; i < bindings.arraySize; i++)
            {
                var existing = bindings.GetArrayElementAtIndex(i);
                if (existing.FindPropertyRelative("CaseType").enumValueIndex == (int)caseType)
                    return;
            }

            bindings.arraySize++;
            var newBinding = bindings.GetArrayElementAtIndex(bindings.arraySize - 1);
            newBinding.FindPropertyRelative("CaseType").enumValueIndex = (int)caseType;
            newBinding.FindPropertyRelative("VariantBuildingIds").ClearArray();

            if (freshSo.ApplyModifiedProperties())
                EditorUtility.SetDirty(sourceSo.targetObject);

            string key = BuildCaseKey(collectionId, caseType);
            _caseFoldout[key] = true;
        }

        private static SerializedProperty EnsureBindingsProperty(SerializedProperty collection)
        {
            var bindings = collection.FindPropertyRelative("TopologyBindings");
            if (bindings != null)
                return bindings;

            throw new InvalidOperationException("Поле TopologyBindings не знайдено у колекції.");
        }

        private static bool ContainsVariantId(SerializedProperty variants, string id)
        {
            for (int i = 0; i < variants.arraySize; i++)
            {
                if (variants.GetArrayElementAtIndex(i).stringValue == id)
                    return true;
            }

            return false;
        }

        private static List<string> CollectBuildingIds(SerializedObject so)
        {
            var ids = new List<string>();
            var buildings = so?.FindProperty("Buildings");
            if (buildings == null)
                return ids;

            for (int i = 0; i < buildings.arraySize; i++)
            {
                string id = buildings.GetArrayElementAtIndex(i).FindPropertyRelative("Id")?.stringValue;
                if (!string.IsNullOrWhiteSpace(id) && !ids.Contains(id))
                    ids.Add(id.Trim());
            }

            ids.Sort(StringComparer.Ordinal);
            return ids;
        }

        private static string DrawIdPopup(GUIContent label, string currentId, IReadOnlyList<string> knownIds)
        {
            currentId ??= string.Empty;

            var values = new List<string> { string.Empty };
            if (knownIds != null)
            {
                for (int i = 0; i < knownIds.Count; i++)
                {
                    string id = knownIds[i];
                    if (!string.IsNullOrWhiteSpace(id) && !values.Contains(id))
                        values.Add(id);
                }
            }

            bool hasMissingCurrent = !string.IsNullOrWhiteSpace(currentId) && !values.Contains(currentId);
            if (hasMissingCurrent)
                values.Add(currentId);

            string[] optionValues = values.ToArray();
            string[] optionLabels = new string[optionValues.Length];
            for (int i = 0; i < optionValues.Length; i++)
            {
                string value = optionValues[i];
                if (string.IsNullOrEmpty(value))
                    optionLabels[i] = "<none>";
                else if (hasMissingCurrent && value == currentId)
                    optionLabels[i] = $"{value} (missing)";
                else
                    optionLabels[i] = value;
            }

            int currentIndex = Array.IndexOf(optionValues, currentId);
            if (currentIndex < 0) currentIndex = 0;

            int selectedIndex = EditorGUILayout.Popup(label, currentIndex, optionLabels);
            if (selectedIndex < 0 || selectedIndex >= optionValues.Length)
                return currentId;

            return optionValues[selectedIndex];
        }

        private static string BuildCaseKey(string collectionId, TopologyCaseType caseType)
        {
            return $"{collectionId}|{caseType}";
        }

        private static string BuildDefaultVariantId(string collectionId, TopologyCaseType caseType, int index)
        {
            string collectionSlug = ToSnakeCase(SanitizeFileName(collectionId));
            string caseSlug = ToSnakeCase(caseType.ToString());

            if (index <= 0)
                return $"{collectionSlug}_{caseSlug}";

            return $"{collectionSlug}_{caseSlug}_{index + 1}";
        }

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var chars = new List<char>(value.Length + 8);
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (char.IsWhiteSpace(c) || c == '-' || c == '.' || c == '/')
                {
                    if (chars.Count > 0 && chars[^1] != '_')
                        chars.Add('_');
                    continue;
                }

                if (char.IsUpper(c))
                {
                    bool hasPrev = chars.Count > 0;
                    bool prevIsUnderscore = hasPrev && chars[^1] == '_';
                    bool nextIsLower = (i + 1 < value.Length) && char.IsLower(value[i + 1]);

                    if (hasPrev && !prevIsUnderscore)
                    {
                        char prevOriginal = value[i - 1];
                        if (char.IsLower(prevOriginal) || char.IsDigit(prevOriginal) || nextIsLower)
                            chars.Add('_');
                    }

                    chars.Add(char.ToLowerInvariant(c));
                    continue;
                }

                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    if (c == '_' && chars.Count > 0 && chars[^1] == '_')
                        continue;

                    chars.Add(char.ToLowerInvariant(c));
                }
            }

            while (chars.Count > 0 && chars[^1] == '_')
                chars.RemoveAt(chars.Count - 1);

            return chars.Count == 0 ? "element" : new string(chars.ToArray());
        }

        private static string GetCaseLabel(TopologyCaseType caseType)
        {
            return caseType switch
            {
                TopologyCaseType.CrossIntersection => "Хрестове перехрестя",
                TopologyCaseType.TJunctionOpenNorth => "Т-перехрестя (відкрито вгору)",
                TopologyCaseType.TJunctionOpenEast => "Т-перехрестя (відкрито праворуч)",
                TopologyCaseType.TJunctionOpenSouth => "Т-перехрестя (відкрито вниз)",
                TopologyCaseType.TJunctionOpenWest => "Т-перехрестя (відкрито ліворуч)",
                TopologyCaseType.CornerNorthEast => "Кут правий верхній (атлас NE, з'єднання S+W)",
                TopologyCaseType.CornerNorthWest => "Кут лівий верхній (атлас NW, з'єднання S+E)",
                TopologyCaseType.CornerSouthEast => "Кут правий нижній (атлас SE, з'єднання N+W)",
                TopologyCaseType.CornerSouthWest => "Кут лівий нижній (атлас SW, з'єднання N+E)",
                TopologyCaseType.Vertical => "Вертикальний сегмент",
                TopologyCaseType.VerticalLeft => "Вертикальний (зліва акцент)",
                TopologyCaseType.VerticalRight => "Вертикальний (справа акцент)",
                TopologyCaseType.Horizontal => "Горизонтальний сегмент",
                TopologyCaseType.HorizontalTop => "Горизонтальний (зверху акцент)",
                TopologyCaseType.HorizontalBottom => "Горизонтальний (знизу акцент)",
                TopologyCaseType.EndNorth => "Закінчення вгору",
                TopologyCaseType.EndEast => "Закінчення праворуч",
                TopologyCaseType.EndSouth => "Закінчення вниз",
                TopologyCaseType.EndWest => "Закінчення ліворуч",
                TopologyCaseType.DiagonalNorthEastSouthWest => "Діагональ NE-SW",
                TopologyCaseType.DiagonalNorthWestSouthEast => "Діагональ NW-SE",
                _ => "Ізольований"
            };
        }

        private static string GetCaseTooltip(TopologyCaseType caseType)
        {
            return caseType switch
            {
                TopologyCaseType.CrossIntersection => "Використовуйте коли об'єкт має 4 кардинальні сусіди (N, E, S, W).",
                TopologyCaseType.TJunctionOpenNorth => "Т-перехрестя без сусіда з півночі. Застосовується для трьох з'єднань E+S+W.",
                TopologyCaseType.TJunctionOpenEast => "Т-перехрестя без сусіда зі сходу. Застосовується для N+S+W.",
                TopologyCaseType.TJunctionOpenSouth => "Т-перехрестя без сусіда з півдня. Застосовується для N+E+W.",
                TopologyCaseType.TJunctionOpenWest => "Т-перехрестя без сусіда із заходу. Застосовується для N+E+S.",
                TopologyCaseType.CornerNorthEast => "Атласний слот NE (правий верхній). Логічні сусіди: S+W.",
                TopologyCaseType.CornerNorthWest => "Атласний слот NW (лівий верхній). Логічні сусіди: S+E.",
                TopologyCaseType.CornerSouthEast => "Атласний слот SE (правий нижній). Логічні сусіди: N+W.",
                TopologyCaseType.CornerSouthWest => "Атласний слот SW (лівий нижній). Логічні сусіди: N+E.",
                TopologyCaseType.Vertical => "Базовий вертикальний сегмент (N+S).",
                TopologyCaseType.VerticalLeft => "Вертикальний сегмент з лівою декоративною варіацією/акцентом.",
                TopologyCaseType.VerticalRight => "Вертикальний сегмент з правою декоративною варіацією/акцентом.",
                TopologyCaseType.Horizontal => "Базовий горизонтальний сегмент (E+W).",
                TopologyCaseType.HorizontalTop => "Горизонтальний сегмент з верхнім декоративним акцентом.",
                TopologyCaseType.HorizontalBottom => "Горизонтальний сегмент з нижнім декоративним акцентом.",
                TopologyCaseType.EndNorth => "Кінцевий елемент, відкритий на північ. Використовуйте для завершення лінії.",
                TopologyCaseType.EndEast => "Кінцевий елемент, відкритий на схід.",
                TopologyCaseType.EndSouth => "Кінцевий елемент, відкритий на південь.",
                TopologyCaseType.EndWest => "Кінцевий елемент, відкритий на захід.",
                TopologyCaseType.DiagonalNorthEastSouthWest => "Спеціальний діагональний випадок для сусідів NE та SW.",
                TopologyCaseType.DiagonalNorthWestSouthEast => "Спеціальний діагональний випадок для сусідів NW та SE.",
                _ => "Ізольований елемент без сусідів. Використовуйте як fallback-стан."
            };
        }

        private static GameObject CreatePrefabFromSprite(string folder, string id, Sprite sprite)
        {
            if (sprite == null)
                return null;

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(
                CombineAssetPath(folder, $"{SanitizeFileName(id)}.prefab"));

            var go = new GameObject(SanitizeFileName(id));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab;
        }

        private static void UpsertBuildingDefinition(SerializedObject so, string id, GameObject prefab, Sprite sprite)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var buildings = so.FindProperty("Buildings");
            if (buildings == null)
                return;

            SerializedProperty target = null;
            for (int i = 0; i < buildings.arraySize; i++)
            {
                var item = buildings.GetArrayElementAtIndex(i);
                if (item.FindPropertyRelative("Id")?.stringValue == id)
                {
                    target = item;
                    break;
                }
            }

            if (target == null)
            {
                buildings.arraySize++;
                target = buildings.GetArrayElementAtIndex(buildings.arraySize - 1);
                target.FindPropertyRelative("Id").stringValue = id;
                target.FindPropertyRelative("DisplayName").stringValue = id;
                target.FindPropertyRelative("Category").enumValueIndex = (int)BuildingCategory.Walls;
            }

            if (prefab != null)
                target.FindPropertyRelative("Prefab").objectReferenceValue = prefab;

            var icon = sprite;
            if (icon == null && prefab != null)
            {
                var sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
                icon = sr != null ? sr.sprite : null;
            }

            if (icon != null)
                target.FindPropertyRelative("Icon").objectReferenceValue = icon;
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "element";

            string cleaned = value.Trim().Replace(' ', '_');
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalid.Length; i++)
                cleaned = cleaned.Replace(invalid[i].ToString(), string.Empty);

            return string.IsNullOrWhiteSpace(cleaned) ? "element" : cleaned;
        }

        private static string CombineAssetPath(string left, string right)
        {
            left = (left ?? string.Empty).Replace('\\', '/').TrimEnd('/');
            right = (right ?? string.Empty).Replace('\\', '/').TrimStart('/');

            if (string.IsNullOrWhiteSpace(left)) return right;
            if (string.IsNullOrWhiteSpace(right)) return left;
            return $"{left}/{right}";
        }

        private static void EnsureFolder(string folder)
        {
            var parts = folder.Split('/');
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
    }
}
