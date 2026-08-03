using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    /// <summary>
    /// Friendly editor for the terrain catalog. Serialized field names and runtime data
    /// types intentionally remain untouched; this class is presentation only.
    /// </summary>
    [CustomEditor(typeof(TerrainLayerProfileSO))]
    public sealed class TerrainLayerProfileSOEditor : UnityEditor.Editor
    {
        private const string ProfilesPropertyName = "_profiles";
        private const string FallbackPropertyName = "_fallback";
        private const string LayerIdPropertyName = "_layerId";
        private const string DisplayNamePropertyName = "_displayName";
        private const string WalkablePropertyName = "_walkable";
        private const string MovementCostPropertyName = "_movementCost";
        private const string BuildBlockedPropertyName = "_buildBlocked";
        private const string SurfaceOffsetPropertyName = "_surfaceOffset";
        private const string TagsPropertyName = "_tags";

        private SerializedProperty _profiles;
        private SerializedProperty _fallback;

        private void OnEnable()
        {
            _profiles = serializedObject.FindProperty(ProfilesPropertyName);
            _fallback = serializedObject.FindProperty(FallbackPropertyName);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawGuideHeader();
            DrawProfileList();
            DrawFallback();

            if (serializedObject.ApplyModifiedProperties())
            {
                TerrainTagEditorCatalog.Invalidate();
                EditorUtility.SetDirty(target);
            }
        }

        private static void DrawGuideHeader()
        {
            EditorGUILayout.LabelField("Каталог terrain і тегів", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Зробіть лише три кроки:\n" +
                "1. Відкрийте картку потрібного terrain (наприклад Water).\n" +
                "2. Натисніть «+ Додати тег зі списку» та оберіть water, land тощо.\n" +
                "3. Для води увімкніть «Будівництво заборонено». Готово.",
                MessageType.Info);
            EditorGUILayout.LabelField(
                "Тег — це проста мітка. Правило будівлі бачить мітку #water і розуміє, що це вода.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(6f);
        }

        private void DrawProfileList()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"Terrain-профілі ({(_profiles != null ? _profiles.arraySize : 0)})",
                    EditorStyles.boldLabel);

                if (GUILayout.Button("+ Додати terrain", GUILayout.Width(145f)))
                {
                    AddProfile();
                    GUIUtility.ExitGUI();
                }
            }

            if (_profiles == null)
            {
                EditorGUILayout.HelpBox("Не знайдено serialized поле _profiles.", MessageType.Error);
                return;
            }

            if (_profiles.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "Список порожній. Натисніть «+ Додати terrain», щоб описати Water, Land або інший шар.",
                    MessageType.Warning);
            }

            for (int index = 0; index < _profiles.arraySize; index++)
                DrawProfileCard(_profiles.GetArrayElementAtIndex(index), index);

            DrawDuplicateIdWarnings();
            EditorGUILayout.Space(8f);
        }

        private void DrawProfileCard(SerializedProperty profile, int index)
        {
            var displayName = profile.FindPropertyRelative(DisplayNamePropertyName);
            var layerId = profile.FindPropertyRelative(LayerIdPropertyName);
            string title = !string.IsNullOrWhiteSpace(displayName?.stringValue)
                ? displayName.stringValue
                : !string.IsNullOrWhiteSpace(layerId?.stringValue)
                    ? layerId.stringValue
                    : $"Новий terrain {index + 1}";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUILayout.HorizontalScope())
            {
                profile.isExpanded = EditorGUILayout.Foldout(
                    profile.isExpanded,
                    $"{index + 1}. {title}",
                    toggleOnLabelClick: true,
                    EditorStyles.foldoutHeader);

                using (new EditorGUI.DisabledScope(index == 0))
                {
                    if (GUILayout.Button(new GUIContent("▲", "Перемістити вище"), EditorStyles.miniButton, GUILayout.Width(26f)))
                    {
                        MoveProfile(index, index - 1);
                        GUIUtility.ExitGUI();
                    }
                }

                using (new EditorGUI.DisabledScope(index >= _profiles.arraySize - 1))
                {
                    if (GUILayout.Button(new GUIContent("▼", "Перемістити нижче"), EditorStyles.miniButton, GUILayout.Width(26f)))
                    {
                        MoveProfile(index, index + 1);
                        GUIUtility.ExitGUI();
                    }
                }

                if (GUILayout.Button(new GUIContent("×", "Видалити terrain-профіль"), EditorStyles.miniButton, GUILayout.Width(26f)) &&
                    EditorUtility.DisplayDialog(
                        "Видалити terrain-профіль?",
                        $"Видалити «{title}»? Цю дію можна скасувати через Undo.",
                        "Видалити",
                        "Скасувати"))
                {
                    DeleteProfile(index);
                    GUIUtility.ExitGUI();
                }
            }

            if (profile.isExpanded)
                DrawProfileFields(profile, isFallback: false);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2f);
        }

        private void DrawFallback()
        {
            EditorGUILayout.LabelField("Запасне правило", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Fallback використовується для шару, якого немає у списку вище. Зазвичай це безпечна звичайна суша.",
                MessageType.None);

            if (_fallback == null)
            {
                EditorGUILayout.HelpBox("Не знайдено serialized поле _fallback.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _fallback.isExpanded = EditorGUILayout.Foldout(
                _fallback.isExpanded,
                "Fallback (для невідомого шару)",
                toggleOnLabelClick: true,
                EditorStyles.foldoutHeader);
            if (_fallback.isExpanded)
                DrawProfileFields(_fallback, isFallback: true);
            EditorGUILayout.EndVertical();
        }

        private void DrawProfileFields(SerializedProperty profile, bool isFallback)
        {
            EditorGUI.indentLevel++;

            DrawProperty(
                profile,
                DisplayNamePropertyName,
                "Зрозуміла назва",
                "Назва лише для людей, наприклад Water, Land або Forest.");

            if (!isFallback)
            {
                DrawProperty(
                    profile,
                    LayerIdPropertyName,
                    "ID шару генератора",
                    "Точний ID (GUID) або назва шару з Generator Graph. За цим значенням гра знаходить профіль.");
            }

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Рух і будівництво", EditorStyles.boldLabel);
            DrawProperty(profile, WalkablePropertyName, "Можна ходити", "Чи можуть юніти проходити цим terrain.");
            DrawProperty(
                profile,
                MovementCostPropertyName,
                "Вартість руху",
                "1 = звичайний крок. Більше число робить шлях дорожчим. 0 = непрохідно.");
            DrawProperty(
                profile,
                BuildBlockedPropertyName,
                "Будівництво заборонено",
                "Увімкніть для води та інших terrain, де за замовчуванням не можна ставити будівлі.");
            DrawProperty(
                profile,
                SurfaceOffsetPropertyName,
                "Зсув поверхні Y",
                "Додатковий вертикальний зсув для юнітів і будівель. Зазвичай залишайте 0.");

            EditorGUILayout.Space(5f);
            DrawTags(profile.FindPropertyRelative(TagsPropertyName));

            var layerId = profile.FindPropertyRelative(LayerIdPropertyName);
            var buildBlocked = profile.FindPropertyRelative(BuildBlockedPropertyName);
            var tags = profile.FindPropertyRelative(TagsPropertyName);
            if (!isFallback && layerId != null && string.IsNullOrWhiteSpace(layerId.stringValue))
                EditorGUILayout.HelpBox("Вкажіть ID шару генератора, інакше цей профіль не буде знайдено.", MessageType.Warning);

            if (HasTag(tags, "water") && buildBlocked != null && !buildBlocked.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Цей terrain має тег #water, але будівництво на ньому глобально дозволене. Якщо це звичайна вода — увімкніть «Будівництво заборонено».",
                    MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawTags(SerializedProperty tags)
        {
            EditorGUILayout.LabelField("Terrain tags", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Не вписуйте water вручну — оберіть його кнопкою нижче.",
                EditorStyles.wordWrappedMiniLabel);

            if (tags == null || !tags.isArray)
            {
                EditorGUILayout.HelpBox("Не знайдено список _tags.", MessageType.Error);
                return;
            }

            if (tags.arraySize == 0)
                EditorGUILayout.HelpBox("Тегів ще немає. Додайте хоча б #water або #land.", MessageType.Info);

            for (int index = 0; index < tags.arraySize; index++)
            {
                string tag = tags.GetArrayElementAtIndex(index).stringValue;
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label(
                        string.IsNullOrWhiteSpace(tag) ? "# (порожній тег)" : $"# {tag}",
                        EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (!TerrainTagEditorCatalog.IsSuggested(tag))
                        GUILayout.Label("власний", EditorStyles.miniLabel, GUILayout.Width(48f));

                    if (GUILayout.Button(new GUIContent("×", "Прибрати цей тег"), EditorStyles.miniButton, GUILayout.Width(25f)))
                    {
                        RemoveTag(tags.propertyPath, index);
                        GUIUtility.ExitGUI();
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Додати тег зі списку ▾"))
                {
                    Rect anchor = GUILayoutUtility.GetLastRect();
                    ShowMultiTagMenu(anchor, tags.propertyPath, ReadTagSet(tags));
                }

                if (GUILayout.Button("+ Створити власний тег…"))
                {
                    Rect anchor = GUILayoutUtility.GetLastRect();
                    string propertyPath = tags.propertyPath;
                    PopupWindow.Show(anchor, new CreateTerrainTagPopup(tag => AddTag(propertyPath, tag)));
                }
            }

            if (HasDuplicateTags(tags))
                EditorGUILayout.HelpBox("Один і той самий тег додано кілька разів. Приберіть зайвий кнопкою ×.", MessageType.Warning);
        }

        private void ShowMultiTagMenu(Rect anchorRect, string propertyPath, ISet<string> selectedTags)
        {
            var menu = new GenericMenu();
            for (int index = 0; index < TerrainTagEditorCatalog.Suggestions.Length; index++)
            {
                var suggestion = TerrainTagEditorCatalog.Suggestions[index];
                string tag = suggestion.Value;
                menu.AddItem(
                    new GUIContent($"Рекомендовані/{suggestion.MenuLabel}"),
                    selectedTags.Contains(tag),
                    () => ToggleTag(propertyPath, tag));
            }

            IReadOnlyList<string> customTags = TerrainTagEditorCatalog.GetCustomTags();
            if (customTags.Count > 0)
            {
                menu.AddSeparator("З terrain-профілів/");
                for (int index = 0; index < customTags.Count; index++)
                {
                    string tag = customTags[index];
                    menu.AddItem(
                        new GUIContent($"З terrain-профілів/{tag}"),
                        selectedTags.Contains(tag),
                        () => ToggleTag(propertyPath, tag));
                }
            }

            menu.DropDown(anchorRect);
        }

        private void AddProfile()
        {
            Undo.RecordObject(target, "Додати terrain-профіль");
            serializedObject.Update();
            int index = _profiles.arraySize;
            _profiles.arraySize++;
            SerializedProperty profile = _profiles.GetArrayElementAtIndex(index);
            SetString(profile, LayerIdPropertyName, string.Empty);
            SetString(profile, DisplayNamePropertyName, "Новий terrain");
            SetBool(profile, WalkablePropertyName, true);
            SetFloat(profile, MovementCostPropertyName, 1f);
            SetBool(profile, BuildBlockedPropertyName, false);
            SetFloat(profile, SurfaceOffsetPropertyName, 0f);
            var tags = profile.FindPropertyRelative(TagsPropertyName);
            if (tags != null && tags.isArray)
                tags.arraySize = 0;
            profile.isExpanded = true;
            ApplyButtonChange();
        }

        private void DeleteProfile(int index)
        {
            Undo.RecordObject(target, "Видалити terrain-профіль");
            serializedObject.Update();
            _profiles.DeleteArrayElementAtIndex(index);
            ApplyButtonChange();
        }

        private void MoveProfile(int sourceIndex, int destinationIndex)
        {
            Undo.RecordObject(target, "Перемістити terrain-профіль");
            serializedObject.Update();
            _profiles.MoveArrayElement(sourceIndex, destinationIndex);
            ApplyButtonChange();
        }

        private void ToggleTag(string propertyPath, string tag)
        {
            Undo.RecordObject(target, "Змінити terrain tags");
            serializedObject.Update();
            var tags = serializedObject.FindProperty(propertyPath);
            if (tags == null || !tags.isArray)
                return;

            int existingIndex = FindTag(tags, tag);
            if (existingIndex >= 0)
                tags.DeleteArrayElementAtIndex(existingIndex);
            else
                AppendTag(tags, tag);

            ApplyButtonChange();
        }

        private void AddTag(string propertyPath, string tag)
        {
            Undo.RecordObject(target, "Додати terrain tag");
            serializedObject.Update();
            var tags = serializedObject.FindProperty(propertyPath);
            if (tags == null || !tags.isArray || FindTag(tags, tag) >= 0)
                return;

            AppendTag(tags, tag);
            ApplyButtonChange();
        }

        private void RemoveTag(string propertyPath, int index)
        {
            Undo.RecordObject(target, "Прибрати terrain tag");
            serializedObject.Update();
            var tags = serializedObject.FindProperty(propertyPath);
            if (tags == null || !tags.isArray || index < 0 || index >= tags.arraySize)
                return;

            tags.DeleteArrayElementAtIndex(index);
            ApplyButtonChange();
        }

        private void ApplyButtonChange()
        {
            serializedObject.ApplyModifiedProperties();
            TerrainTagEditorCatalog.Invalidate();
            EditorUtility.SetDirty(target);
            Repaint();
        }

        private void DrawDuplicateIdWarnings()
        {
            if (_profiles == null)
                return;

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < _profiles.arraySize; index++)
            {
                var idProperty = _profiles.GetArrayElementAtIndex(index).FindPropertyRelative(LayerIdPropertyName);
                string id = idProperty?.stringValue?.Trim();
                if (!string.IsNullOrEmpty(id) && !ids.Add(id))
                    duplicates.Add(id);
            }

            foreach (string duplicate in duplicates)
            {
                EditorGUILayout.HelpBox(
                    $"ID «{duplicate}» використано кілька разів. Залиште лише один профіль із цим ID.",
                    MessageType.Error);
            }
        }

        private static void DrawProperty(
            SerializedProperty profile,
            string propertyName,
            string label,
            string tooltip)
        {
            var property = profile.FindPropertyRelative(propertyName);
            if (property != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label, tooltip), includeChildren: true);
        }

        private static ISet<string> ReadTagSet(SerializedProperty tags)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < tags.arraySize; index++)
            {
                string tag = tags.GetArrayElementAtIndex(index).stringValue?.Trim();
                if (!string.IsNullOrEmpty(tag))
                    result.Add(tag);
            }

            return result;
        }

        private static int FindTag(SerializedProperty tags, string tag)
        {
            for (int index = 0; index < tags.arraySize; index++)
            {
                if (string.Equals(
                        tags.GetArrayElementAtIndex(index).stringValue?.Trim(),
                        tag?.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        private static bool HasTag(SerializedProperty tags, string tag) => FindTag(tags, tag) >= 0;

        private static bool HasDuplicateTags(SerializedProperty tags)
        {
            var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < tags.arraySize; index++)
            {
                string tag = tags.GetArrayElementAtIndex(index).stringValue?.Trim();
                if (!string.IsNullOrEmpty(tag) && !known.Add(tag))
                    return true;
            }

            return false;
        }

        private static void AppendTag(SerializedProperty tags, string tag)
        {
            int index = tags.arraySize;
            tags.arraySize++;
            tags.GetArrayElementAtIndex(index).stringValue = TerrainTagEditorCatalog.Normalize(tag);
        }

        private static void SetString(SerializedProperty parent, string name, string value)
        {
            var property = parent.FindPropertyRelative(name);
            if (property != null)
                property.stringValue = value;
        }

        private static void SetBool(SerializedProperty parent, string name, bool value)
        {
            var property = parent.FindPropertyRelative(name);
            if (property != null)
                property.boolValue = value;
        }

        private static void SetFloat(SerializedProperty parent, string name, float value)
        {
            var property = parent.FindPropertyRelative(name);
            if (property != null)
                property.floatValue = value;
        }
    }
}
