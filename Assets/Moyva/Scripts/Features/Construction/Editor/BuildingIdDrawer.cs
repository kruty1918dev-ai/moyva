using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CustomPropertyDrawer(typeof(BuildingIdAttribute))]
    public class BuildingIdDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.String)
            {
                string val = property.stringValue;
                if (!string.IsNullOrEmpty(val))
                {
                    if (System.Array.IndexOf(GetBuildingIds(), val) < 0)
                        h += EditorGUIUtility.singleLineHeight + 4;
                    else if (!HasPrefabForBuildingId(val))
                        h += (EditorGUIUtility.singleLineHeight + 4) * 2;
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

            string[] ids = GetBuildingIds();
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
                PopupWindow.Show(buttonRect, new BuildingPickerPopup(
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
            Sprite sprite = GetSpriteForBuildingId(currentValue);
            if (sprite != null && sprite.texture != null)
                DrawSprite(spriteRect, sprite);

            // Error box + create button (ID not found)
            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                float y2 = position.y + lineH + 2;
                float btnW = 80;
                Rect errorRect = new Rect(position.x, y2, position.width - btnW - 4, lineH);
                Rect btnRect = new Rect(position.xMax - btnW, y2, btnW, lineH);

                EditorGUI.HelpBox(errorRect, $"ID \"{currentValue}\" не знайдено!", MessageType.Error);
                if (GUI.Button(btnRect, "+ Створити"))
                    CreateBuildingEntry(currentValue);
            }

            // Fatal: ID exists but prefab missing
            if (isValid && !HasPrefabForBuildingId(currentValue))
            {
                float y2 = position.y + lineH + 2;
                Rect errorRect = new Rect(position.x, y2, position.width, lineH);
                EditorGUI.HelpBox(errorRect, $"FATAL: \"{currentValue}\" \u2014 prefab \u0432\u0456\u0434\u0441\u0443\u0442\u043d\u0456\u0439!", MessageType.Error);

                float y3 = y2 + lineH + 4;
                float btnW = 110;
                Rect spriteFieldRect = new Rect(position.x, y3, position.width - btnW - 4, lineH);
                Rect genBtnRect = new Rect(position.xMax - btnW, y3, btnW, lineH);

                string key = property.propertyPath;
                _pendingSprites.TryGetValue(key, out Sprite pending);
                var newSprite = (Sprite)EditorGUI.ObjectField(spriteFieldRect, "Sprite", pending, typeof(Sprite), false);
                if (newSprite != pending) _pendingSprites[key] = newSprite;

                if (GUI.Button(genBtnRect, "\u2699 \u0417\u0433\u0435\u043d\u0435\u0440\u0443\u0432\u0430\u0442\u0438"))
                {
                    _pendingSprites.TryGetValue(key, out Sprite spr);
                    GeneratePrefabForBuildingId(currentValue, spr);
                    _pendingSprites.Remove(key);
                }
            }

            EditorGUI.EndProperty();
        }

        private static string BuildCurrentValueLabel(string currentValue, bool isValid)
        {
            if (string.IsNullOrEmpty(currentValue))
                return "(none)";

            if (!isValid)
                return $"⚠ {currentValue}";

            return BuildBuildingLabel(currentValue);
        }

        private static string BuildBuildingLabel(string buildingId)
        {
            string displayName = GetDisplayNameForBuildingId(buildingId);
            return $"{buildingId} — {displayName}";
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            Rect texRect = sprite.textureRect;
            Rect texCoords = new Rect(
                texRect.x / sprite.texture.width,
                texRect.y / sprite.texture.height,
                texRect.width / sprite.texture.width,
                texRect.height / sprite.texture.height
            );

            GUI.DrawTextureWithTexCoords(rect, sprite.texture, texCoords);
        }

        private sealed class BuildingPickerPopup : PopupWindowContent
        {
            private const float PopupWidth = 700f;
            private const float PopupHeight = 420f;
            private const float HeaderHeight = 20f;
            private const float SectionHeaderHeight = 18f;
            private const float RowHeight = 22f;
            private const float Padding = 6f;
            private const float ColumnGap = 8f;
            private const float SectionGap = 8f;
            private const float IconSize = 16f;

            private static readonly BuildingCategory[] OrderedCategories =
            {
                BuildingCategory.Military,
                BuildingCategory.Civilian,
                BuildingCategory.Industrial,
                BuildingCategory.Walls,
            };

            private readonly string _currentValue;
            private readonly Action<string> _onSelected;
            private Vector2 _leftScroll;
            private Vector2 _rightScroll;

            public BuildingPickerPopup(string currentValue, Action<string> onSelected)
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

                float columnWidth = (columnsRect.width - ColumnGap) * 0.5f;
                Rect leftRect = new Rect(columnsRect.x, columnsRect.y, columnWidth, columnsRect.height);
                Rect rightRect = new Rect(columnsRect.x + columnWidth + ColumnGap, columnsRect.y, columnWidth, columnsRect.height);

                DrawColumn(leftRect, 0, ref _leftScroll);
                DrawColumn(rightRect, 1, ref _rightScroll);
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

            private void DrawColumn(Rect rect, int columnIndex, ref Vector2 scroll)
            {
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

                Rect headerRect = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, HeaderHeight);
                EditorGUI.LabelField(headerRect, columnIndex == 0 ? "Building Classes A" : "Building Classes B", EditorStyles.boldLabel);

                Rect viewRect = new Rect(rect.x + 2f, rect.y + HeaderHeight + 4f, rect.width - 4f, rect.height - HeaderHeight - 6f);

                float contentHeight = 0f;
                for (int i = columnIndex; i < OrderedCategories.Length; i += 2)
                {
                    string[] ids = GetBuildingIdsByCategory(OrderedCategories[i]);
                    contentHeight += SectionHeaderHeight;
                    contentHeight += Mathf.Max(1, ids.Length) * RowHeight;
                    contentHeight += SectionGap;
                }

                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f, Mathf.Max(viewRect.height - 2f, contentHeight));

                scroll = GUI.BeginScrollView(viewRect, scroll, contentRect);

                float y = 0f;
                for (int i = columnIndex; i < OrderedCategories.Length; i += 2)
                {
                    var category = OrderedCategories[i];
                    y = DrawCategorySection(category, y, contentRect.width);
                }

                GUI.EndScrollView();
            }

            private float DrawCategorySection(BuildingCategory category, float y, float width)
            {
                Rect sectionHeaderRect = new Rect(0f, y, width, SectionHeaderHeight);
                EditorGUI.LabelField(sectionHeaderRect, category.ToString(), EditorStyles.boldLabel);
                y += SectionHeaderHeight;

                string[] ids = GetBuildingIdsByCategory(category);
                if (ids.Length == 0)
                {
                    Rect emptyRect = new Rect(0f, y, width, RowHeight);
                    EditorGUI.LabelField(emptyRect, "(порожньо)");
                    y += RowHeight + SectionGap;
                    return y;
                }

                for (int i = 0; i < ids.Length; i++)
                {
                    Rect rowRect = new Rect(0f, y, width, RowHeight - 1f);
                    DrawBuildingRow(rowRect, ids[i]);
                    y += RowHeight;
                }

                y += SectionGap;
                return y;
            }

            private void DrawBuildingRow(Rect rowRect, string id)
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
                Sprite sprite = GetSpriteForBuildingId(id);
                if (sprite != null && sprite.texture != null)
                    DrawSprite(iconRect, sprite);

                Rect labelRect = new Rect(iconRect.xMax + 6f, rowRect.y + 2f, rowRect.width - iconRect.width - 10f, RowHeight - 2f);
                GUI.Label(labelRect, BuildBuildingLabel(id), EditorStyles.label);
            }
        }

        // ── Static cache (1-second TTL) ──
        private static BuildingRegistrySO _cachedRegistry;
        private static string[] _cachedIds = System.Array.Empty<string>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        private static readonly Dictionary<string, bool> _prefabCache = new();
        private static readonly Dictionary<string, string> _displayNameCache = new();
        private static readonly Dictionary<string, BuildingCategory> _categoryCache = new();
        private static double _cacheTime;
        private const double CacheTTL = 1.0;

        private static void EnsureCache()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedRegistry != null && now - _cacheTime < CacheTTL) return;

            _cachedRegistry = FindRegistryInternal();
            _cacheTime = now;
            _spriteCache.Clear();
            _prefabCache.Clear();
            _displayNameCache.Clear();
            _categoryCache.Clear();

            if (_cachedRegistry?.Buildings == null)
            {
                _cachedIds = System.Array.Empty<string>();
                return;
            }

            var ids = new List<string>();
            foreach (var def in _cachedRegistry.Buildings)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                ids.Add(def.Id);
                _prefabCache[def.Id] = def.Prefab != null;
                _displayNameCache[def.Id] = string.IsNullOrWhiteSpace(def.DisplayName) ? def.Id : def.DisplayName;
                _categoryCache[def.Id] = def.Category;

                if (def.Icon != null)
                {
                    _spriteCache[def.Id] = def.Icon;
                }
                else if (def.Prefab != null)
                {
                    var sr = def.Prefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (sr != null && sr.sprite != null)
                        _spriteCache[def.Id] = sr.sprite;
                }
            }
            ids.Sort();
            _cachedIds = ids.ToArray();
        }

        private static void InvalidateCache() => _cacheTime = 0;

        private static BuildingRegistrySO FindRegistryInternal()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildingRegistrySO");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
        }

        private static BuildingRegistrySO FindRegistry()
        {
            EnsureCache();
            return _cachedRegistry;
        }

        private static string[] GetBuildingIds()
        {
            EnsureCache();
            return _cachedIds;
        }

        private static string[] GetBuildingIdsByCategory(BuildingCategory category)
        {
            EnsureCache();
            if (_cachedIds.Length == 0)
                return System.Array.Empty<string>();

            var filtered = new List<string>();
            for (int i = 0; i < _cachedIds.Length; i++)
            {
                string id = _cachedIds[i];
                if (_categoryCache.TryGetValue(id, out var cachedCategory) && cachedCategory == category)
                    filtered.Add(id);
            }

            return filtered.ToArray();
        }

        private static Sprite GetSpriteForBuildingId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            return _spriteCache.TryGetValue(id, out var s) ? s : null;
        }

        private static string GetDisplayNameForBuildingId(string id)
        {
            if (string.IsNullOrEmpty(id)) return id;
            EnsureCache();
            return _displayNameCache.TryGetValue(id, out var name) ? name : id;
        }

        private static void CreateBuildingEntry(string id)
        {
            var registry = FindRegistry();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Помилка", "BuildingRegistrySO не знайдено в проєкті.", "OK");
                return;
            }

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("Buildings");
            arr.arraySize++;
            var newElem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
            newElem.FindPropertyRelative("Id").stringValue = id;
            newElem.FindPropertyRelative("DisplayName").stringValue = id;
            newElem.FindPropertyRelative("Prefab").objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateCache();
            Debug.Log($"[BuildingIdDrawer] Створено новий запис будівлі з ID: \"{id}\" в {AssetDatabase.GetAssetPath(registry)}");
        }

        private static readonly Dictionary<string, Sprite> _pendingSprites = new();

        private static bool HasPrefabForBuildingId(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            EnsureCache();
            return !_prefabCache.TryGetValue(id, out bool has) || has;
        }

        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";

        private static void GeneratePrefabForBuildingId(string id, Sprite sprite)
        {
            var registry = FindRegistry();
            if (registry == null) return;

            EnsureFolder(BuildingPrefabFolder);
            string safe = id.Replace(' ', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingPrefabFolder}/{safe}.prefab");

            var go = new GameObject(safe);
            if (sprite != null) go.AddComponent<SpriteRenderer>().sprite = sprite;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("Buildings");
            for (int i = 0; i < arr.arraySize; i++)
            {
                var el = arr.GetArrayElementAtIndex(i);
                if (el.FindPropertyRelative("Id")?.stringValue == id)
                {
                    el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                    break;
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateCache();
            Debug.Log($"[BuildingIdDrawer] Згенеровано prefab для \"{id}\": {path}");
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }
}
