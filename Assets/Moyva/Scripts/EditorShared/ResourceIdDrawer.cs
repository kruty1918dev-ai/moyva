using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    [CustomPropertyDrawer(typeof(ResourceIdAttribute))]
    public class ResourceIdDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.String)
            {
                string val = property.stringValue;
                if (!string.IsNullOrEmpty(val))
                {
                    if (System.Array.IndexOf(GetResourceIds(), val) < 0)
                        h += EditorGUIUtility.singleLineHeight + 4;
                }
            }
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] ids = GetResourceIds();
            string currentValue = property.stringValue;
            bool isValid = !string.IsNullOrEmpty(currentValue) && System.Array.IndexOf(ids, currentValue) >= 0;

            float lineH = EditorGUIUtility.singleLineHeight;
            float spriteSize = lineH;
            Rect popupRect = new Rect(position.x, position.y, position.width - spriteSize - 4, lineH);
            Rect spriteRect = new Rect(position.xMax - spriteSize, position.y, spriteSize, spriteSize);

            Color prevColor = GUI.color;
            if (!isValid && !string.IsNullOrEmpty(currentValue))
                GUI.color = Color.red;

            EditorGUI.BeginProperty(position, label, property);

            var serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;
            string buttonText = BuildCurrentValueLabel(currentValue, isValid);
            Rect buttonRect = EditorGUI.PrefixLabel(popupRect, label);

            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(buttonText), FocusType.Keyboard))
            {
                PopupWindow.Show(buttonRect, new ResourcePickerPopup(
                    currentValue,
                    selectedId =>
                    {
                        serializedObject.Update();
                        var currentProperty = serializedObject.FindProperty(propertyPath);
                        if (currentProperty == null || currentProperty.propertyType != SerializedPropertyType.String)
                            return;

                        currentProperty.stringValue = selectedId;
                        serializedObject.ApplyModifiedProperties();
                    }));
            }

            GUI.color = prevColor;

            // Sprite preview
            Sprite sprite = GetSpriteForResourceId(currentValue);
            if (sprite != null && sprite.texture != null)
                DrawSprite(spriteRect, sprite);

            // Error box (ID not found)
            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                float y2 = position.y + lineH + 2;
                float btnW = 80;
                Rect errorRect = new Rect(position.x, y2, position.width - btnW - 4, lineH);
                Rect btnRect = new Rect(position.xMax - btnW, y2, btnW, lineH);

                EditorGUI.HelpBox(errorRect, $"ID \"{currentValue}\" не знайдено в EconomyDatabaseSO!", MessageType.Error);
                if (GUI.Button(btnRect, "+ Створити"))
                    CreateResourceEntry(currentValue);
            }

            EditorGUI.EndProperty();
        }

        private static string BuildCurrentValueLabel(string currentValue, bool isValid)
        {
            if (string.IsNullOrEmpty(currentValue))
                return "(none)";

            if (!isValid)
                return $"⚠ {currentValue}";

            return BuildResourceLabel(currentValue);
        }

        private static string BuildResourceLabel(string resourceId)
        {
            string displayName = GetResourceDisplayName(resourceId);
            return $"{resourceId} — {displayName}";
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(rect, null, sprite);
        }

        private sealed class ResourcePickerPopup : PopupWindowContent
        {
            private const float PopupWidth = 560f;
            private const float PopupHeight = 380f;
            private const float HeaderHeight = 20f;
            private const float RowHeight = 22f;
            private const float Padding = 6f;
            private const float ColumnGap = 8f;
            private const float IconSize = 16f;

            private readonly string _currentValue;
            private readonly Action<string> _onSelected;
            private Vector2 _materialsScroll;
            private Vector2 _foodScroll;
            private Vector2 _moneyScroll;

            public ResourcePickerPopup(string currentValue, Action<string> onSelected)
            {
                _currentValue = currentValue;
                _onSelected = onSelected;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);

            public override void OnGUI(Rect rect)
            {
                DrawNoneButton();

                Rect columnsRect = new Rect(
                    Padding,
                    Padding + RowHeight + Padding,
                    rect.width - Padding * 2,
                    rect.height - (Padding * 3 + RowHeight)
                );

                float columnWidth = (columnsRect.width - ColumnGap * 2f) / 3f;
                Rect leftRect = new Rect(columnsRect.x, columnsRect.y, columnWidth, columnsRect.height);
                Rect middleRect = new Rect(columnsRect.x + columnWidth + ColumnGap, columnsRect.y, columnWidth, columnsRect.height);
                Rect rightRect = new Rect(columnsRect.x + (columnWidth + ColumnGap) * 2f, columnsRect.y, columnWidth, columnsRect.height);

                DrawCategoryColumn(leftRect, "Materials", EconomyResourceCategory.Materials, ref _materialsScroll);
                DrawCategoryColumn(middleRect, "Food", EconomyResourceCategory.Food, ref _foodScroll);
                DrawCategoryColumn(rightRect, "Money", EconomyResourceCategory.Money, ref _moneyScroll);
            }

            private void DrawNoneButton()
            {
                Rect noneRect = new Rect(Padding, Padding, PopupWidth - Padding * 2, RowHeight);
                bool isNoneSelected = string.IsNullOrEmpty(_currentValue);

                if (isNoneSelected)
                    EditorGUI.DrawRect(noneRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));

                if (GUI.Button(noneRect, "(none)", EditorStyles.miniButton))
                {
                    _onSelected?.Invoke(string.Empty);
                    editorWindow.Close();
                }
            }

            private void DrawCategoryColumn(Rect rect, string header, EconomyResourceCategory category, ref Vector2 scroll)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

                Rect headerRect = new Rect(rect.x + 2, rect.y + 2, rect.width - 4, HeaderHeight);
                EditorGUI.LabelField(headerRect, header, EditorStyles.boldLabel);

                string[] ids = GetResourceIdsByCategory(category);

                Rect viewRect = new Rect(rect.x + 2, rect.y + HeaderHeight + 4, rect.width - 4, rect.height - HeaderHeight - 6);
                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f, Mathf.Max(2f, ids.Length * RowHeight));

                scroll = GUI.BeginScrollView(viewRect, scroll, contentRect);
                for (int i = 0; i < ids.Length; i++)
                {
                    Rect rowRect = new Rect(0f, i * RowHeight, contentRect.width, RowHeight - 1f);
                    DrawResourceRow(rowRect, ids[i]);
                }
                GUI.EndScrollView();
            }

            private void DrawResourceRow(Rect rowRect, string id)
            {
                bool isSelected = string.Equals(_currentValue, id, StringComparison.Ordinal);
                if (isSelected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));

                if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
                {
                    _onSelected?.Invoke(id);
                    editorWindow.Close();
                    return;
                }

                Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
                Sprite sprite = GetSpriteForResourceId(id);
                if (sprite != null && sprite.texture != null)
                    DrawSprite(iconRect, sprite);

                Rect labelRect = new Rect(iconRect.xMax + 6f, rowRect.y + 2f, rowRect.width - iconRect.width - 10f, RowHeight - 2f);
                GUI.Label(labelRect, BuildResourceLabel(id), EditorStyles.label);
            }
        }

        // ── Static cache (1-second TTL) ──
        private static EconomyDatabaseSO _cachedDatabase;
        private static string[] _cachedIds = System.Array.Empty<string>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        private static readonly Dictionary<string, string> _displayNameCache = new();
        private static readonly Dictionary<string, EconomyResourceCategory> _categoryCache = new();
        private static double _cacheTime;
        private const double CacheTTL = 1.0;

        private static void EnsureCache()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedDatabase != null && now - _cacheTime < CacheTTL) return;

            _cachedDatabase = FindDatabaseInternal();
            _cacheTime = now;
            _spriteCache.Clear();
            _displayNameCache.Clear();
            _categoryCache.Clear();

            if (_cachedDatabase?.Resources == null)
            {
                _cachedIds = System.Array.Empty<string>();
                return;
            }

            var ids = new List<string>();
            foreach (var resource in _cachedDatabase.Resources)
            {
                if (string.IsNullOrEmpty(resource.Id)) continue;
                ids.Add(resource.Id);
                if (resource.Icon != null)
                    _spriteCache[resource.Id] = resource.Icon;
                _displayNameCache[resource.Id] = resource.DisplayName ?? resource.Id;
                _categoryCache[resource.Id] = resource.Category;
            }
            ids.Sort();
            _cachedIds = ids.ToArray();
        }

        private static void InvalidateCache() => _cacheTime = 0;

        private static EconomyDatabaseSO FindDatabaseInternal()
        {
            string[] guids = AssetDatabase.FindAssets("t:EconomyDatabaseSO");
            if (guids.Length == 0)
                return null;

            // Беремо перший знайдений, або найбільш релевантний
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<EconomyDatabaseSO>(path);
        }

        private static EconomyDatabaseSO FindDatabase()
        {
            EnsureCache();
            return _cachedDatabase;
        }

        private static string[] GetResourceIds()
        {
            EnsureCache();
            return _cachedIds;
        }

        private static string[] GetResourceIdsByCategory(EconomyResourceCategory category)
        {
            EnsureCache();
            if (_cachedIds.Length == 0)
                return System.Array.Empty<string>();

            var filtered = new List<string>();
            for (int i = 0; i < _cachedIds.Length; i++)
            {
                string id = _cachedIds[i];
                if (_categoryCache.TryGetValue(id, out var itemCategory) && itemCategory == category)
                    filtered.Add(id);
            }

            return filtered.ToArray();
        }

        private static Sprite GetSpriteForResourceId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            return _spriteCache.TryGetValue(id, out var s) ? s : null;
        }

        private static string GetResourceDisplayName(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            EnsureCache();
            return _displayNameCache.TryGetValue(id, out var name) ? name : id;
        }

        private static void CreateResourceEntry(string id)
        {
            var database = FindDatabase();
            if (database == null)
            {
                EditorUtility.DisplayDialog("Помилка", "EconomyDatabaseSO не знайдено в проєкті.\nСтворіть його через меню: Moyva → Economy → Economy Catalog", "OK");
                return;
            }

            var so = new SerializedObject(database);
            var resourcesProp = so.FindProperty("_resources");
            if (resourcesProp == null)
            {
                EditorUtility.DisplayDialog("Помилка", "Не вдалося знайти поле _resources в EconomyDatabaseSO.", "OK");
                return;
            }

            resourcesProp.arraySize++;
            var newElem = resourcesProp.GetArrayElementAtIndex(resourcesProp.arraySize - 1);
            newElem.FindPropertyRelative("_id").stringValue = id;
            newElem.FindPropertyRelative("_displayName").stringValue = id;
            newElem.FindPropertyRelative("_category").enumValueIndex = 0;
            newElem.FindPropertyRelative("_icon").objectReferenceValue = null;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            InvalidateCache();

            Debug.Log($"[ResourceIdDrawer] Створено новий ресурс з ID: \"{id}\" в {AssetDatabase.GetAssetPath(database)}");
        }
    }
}
