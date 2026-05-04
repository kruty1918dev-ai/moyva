using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Editor
{
    [CustomPropertyDrawer(typeof(TileIdAttribute))]
    public class TileIdDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.String)
            {
                string val = property.stringValue;
                if (!string.IsNullOrEmpty(val))
                {
                    if (System.Array.IndexOf(GetTileIds(), val) < 0)
                        h += EditorGUIUtility.singleLineHeight + 4;
                    else if (!HasPrefabForTileId(val))
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

            string[] ids = GetTileIds();
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
                PopupWindow.Show(buttonRect, new TilePickerPopup(
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
            Sprite sprite = GetSpriteForTileId(currentValue);
            if (sprite != null && sprite.texture != null)
                DrawSprite(spriteRect, sprite, GetColorForTileId(currentValue));

            // Error box + create button (ID not found)
            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                float y2 = position.y + lineH + 2;
                float btnW = 80;
                Rect errorRect = new Rect(position.x, y2, position.width - btnW - 4, lineH);
                Rect btnRect = new Rect(position.xMax - btnW, y2, btnW, lineH);

                EditorGUI.HelpBox(errorRect, $"ID \"{currentValue}\" не знайдено!", MessageType.Error);
                if (GUI.Button(btnRect, "+ Створити"))
                    CreateTileEntry(currentValue);
            }

            // Fatal: ID exists but prefab missing
            if (isValid && !HasPrefabForTileId(currentValue))
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
                    GeneratePrefabForTileId(currentValue, spr);
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

            return currentValue;
        }

        private static void DrawSprite(Rect rect, Sprite sprite, Color color)
        {
            var tex = sprite.texture;
            var sr = sprite.textureRect;
            var uv = new Rect(sr.x / tex.width, sr.y / tex.height, sr.width / tex.width, sr.height / tex.height);
            Color prevColor = GUI.color;
            GUI.color = color;
            GUI.DrawTextureWithTexCoords(rect, tex, uv);
            GUI.color = prevColor;
        }

        private sealed class TilePickerPopup : PopupWindowContent
        {
            private const float PopupWidth = 520f;
            private const float PopupHeight = 500f;
            private const float SearchHeight = 20f;
            private const float RowHeight = 44f;
            private const float IconSize = 36f;
            private const float Padding = 6f;
            private const string SearchControlName = "TileIdDrawerSearch";

            private readonly string _currentValue;
            private readonly Action<string> _onSelected;
            private Vector2 _scroll;
            private string _search = string.Empty;
            private bool _focusSearch = true;

            public TilePickerPopup(string currentValue, Action<string> onSelected)
            {
                _currentValue = currentValue;
                _onSelected = onSelected;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);

            public override void OnOpen()
            {
                _focusSearch = true;
            }

            public override void OnGUI(Rect rect)
            {
                DrawSearchField(rect);
                var ids = GetFilteredIds();

                Rect noneRect = new Rect(
                    Padding,
                    Padding + SearchHeight + Padding,
                    rect.width - Padding * 2f,
                    RowHeight);
                bool isNoneSelected = string.IsNullOrEmpty(_currentValue);
                if (isNoneSelected)
                    EditorGUI.DrawRect(noneRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));

                if (GUI.Button(noneRect, "(none)", EditorStyles.miniButton))
                {
                    _onSelected?.Invoke(string.Empty);
                    editorWindow.Close();
                    return;
                }

                Rect viewRect = new Rect(
                    Padding,
                    noneRect.yMax + Padding,
                    rect.width - Padding * 2f,
                    rect.height - noneRect.yMax - Padding * 2f);

                if (ids.Length == 0)
                {
                    EditorGUI.HelpBox(viewRect, "Нічого не знайдено", MessageType.Info);
                    return;
                }

                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f, Mathf.Max(viewRect.height, ids.Length * RowHeight));
                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
                for (int i = 0; i < ids.Length; i++)
                {
                    Rect rowRect = new Rect(0f, i * RowHeight, contentRect.width, RowHeight - 1f);
                    DrawTileRow(rowRect, ids[i]);
                }
                GUI.EndScrollView();
            }

            private void DrawSearchField(Rect rect)
            {
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchHeight);

                GUI.SetNextControlName(SearchControlName);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck())
                    _scroll = Vector2.zero;

                if (_focusSearch)
                {
                    _focusSearch = false;
                    EditorGUI.FocusTextInControl(SearchControlName);
                }
            }

            private string[] GetFilteredIds()
            {
                var ids = GetTileIds();
                if (string.IsNullOrWhiteSpace(_search))
                    return ids;

                var terms = _search.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return ids
                    .Where(id => terms.All(term => id.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToArray();
            }

            private void DrawTileRow(Rect rowRect, string id)
            {
                bool isSelected = string.Equals(_currentValue, id, StringComparison.Ordinal);
                if (isSelected)
                    EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.36f, 0.24f, 0.7f));
                else if (rowRect.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowRect, new Color(0.20f, 0.20f, 0.20f, 0.45f));

                if (GUI.Button(rowRect, GUIContent.none, GUIStyle.none))
                {
                    _onSelected?.Invoke(id);
                    editorWindow.Close();
                    return;
                }

                Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + (RowHeight - IconSize) * 0.5f, IconSize, IconSize);
                var sprite = GetSpriteForTileId(id);
                if (sprite != null && sprite.texture != null)
                    DrawSprite(iconRect, sprite, GetColorForTileId(id));

                Rect labelRect = new Rect(iconRect.xMax + 8f, rowRect.y, rowRect.width - iconRect.width - 14f, RowHeight);
                var labelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
                GUI.Label(labelRect, id, labelStyle);
            }
        }

        // ── Static cache (1-second TTL) ──
        private static TileRegistrySO _cachedRegistry;
        private static string[] _cachedIds = System.Array.Empty<string>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        private static readonly Dictionary<string, Color> _spriteColorCache = new();
        private static readonly Dictionary<string, bool> _prefabCache = new();
        private static double _cacheTime;
        private const double CacheTTL = 1.0;

        private static void EnsureCache()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedRegistry != null && now - _cacheTime < CacheTTL) return;

            _cachedRegistry = FindRegistryInternal();
            _cacheTime = now;
            _spriteCache.Clear();
            _spriteColorCache.Clear();
            _prefabCache.Clear();

            if (_cachedRegistry?.Definitions == null)
            {
                _cachedIds = System.Array.Empty<string>();
                return;
            }

            var ids = new List<string>();
            foreach (var def in _cachedRegistry.Definitions)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                ids.Add(def.Id);
                _prefabCache[def.Id] = def.VisualPrefab != null;
                if (def.VisualPrefab != null)
                {
                    var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (sr != null && sr.sprite != null)
                    {
                        _spriteCache[def.Id] = sr.sprite;
                        _spriteColorCache[def.Id] = sr.color;
                    }
                }
            }
            ids.Sort();
            _cachedIds = ids.ToArray();
        }

        private static void InvalidateCache() => _cacheTime = 0;

        private const string MainTileRegistryPath = "Assets/Moyva/SO/Tile/TileRegistry.asset";
        private const string GraphEditorWindowSettingsPath = "Assets/Moyva/Scripts/Features/GraphSystem/Editor/GraphEditorWindowSettings.asset";
        private const string EditorPreviewSettingsPath = "Assets/Moyva/SO/Generation/EditorPreviewSettings.asset";

        private static TileRegistrySO FindRegistryInternal()
        {
            // Пріоритет 1: preview settings, вибраний у Graph Editor window.
            var windowSettingsObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GraphEditorWindowSettingsPath);
            if (windowSettingsObj != null)
            {
                var windowSo = new SerializedObject(windowSettingsObj);
                var previewSettingsProp = windowSo.FindProperty("previewSettings");
                if (previewSettingsProp?.objectReferenceValue is ScriptableObject selectedPreviewSettingsObj)
                {
                    var previewSo = new SerializedObject(selectedPreviewSettingsObj);
                    var registryProp = previewSo.FindProperty("_tileRegistry");
                    if (registryProp?.objectReferenceValue is TileRegistrySO registryFromWindowPreview)
                        return registryFromWindowPreview;
                }
            }

            // Пріоритет 2: реєстр з legacy EditorPreviewSettings asset.
            var previewSettingsObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(EditorPreviewSettingsPath);
            if (previewSettingsObj != null)
            {
                var so = new SerializedObject(previewSettingsObj);
                var registryProp = so.FindProperty("_tileRegistry");
                if (registryProp?.objectReferenceValue is TileRegistrySO registryFromPreview)
                    return registryFromPreview;
            }

            // Пріоритет 3: головний реєстр за відомим шляхом
            var main = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(MainTileRegistryPath);
            if (main != null) return main;

            // Fallback: шукаємо серед усіх, уникаємо Test/Prototype
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return null;

            var paths = guids.Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p.IndexOf("Test", System.StringComparison.OrdinalIgnoreCase) >= 0 ? 2
                            : p.IndexOf("Prototype", System.StringComparison.OrdinalIgnoreCase) >= 0 ? 1
                            : 0)
                .ToList();

            return AssetDatabase.LoadAssetAtPath<TileRegistrySO>(paths[0]);
        }

        private static TileRegistrySO FindRegistry()
        {
            EnsureCache();
            return _cachedRegistry;
        }

        private static string[] GetTileIds()
        {
            EnsureCache();
            return _cachedIds;
        }

        private static Sprite GetSpriteForTileId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            return _spriteCache.TryGetValue(id, out var s) ? s : null;
        }

        private static Color GetColorForTileId(string id)
        {
            if (string.IsNullOrEmpty(id)) return Color.white;
            EnsureCache();
            return _spriteColorCache.TryGetValue(id, out var color) ? color : Color.white;
        }

        private static void CreateTileEntry(string id)
        {
            var registry = FindRegistry();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Помилка", "TileRegistrySO не знайдено в проєкті.", "OK");
                return;
            }

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");
            arr.arraySize++;
            var newElem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
            newElem.FindPropertyRelative("_id").stringValue = id;
            newElem.FindPropertyRelative("_movementCost").floatValue = 1f;
            newElem.FindPropertyRelative("_visualPrefab").objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateCache();
            Debug.Log($"[TileIdDrawer] Створено нову запис тайла з ID: \"{id}\" в {AssetDatabase.GetAssetPath(registry)}");
        }

        private static readonly Dictionary<string, Sprite> _pendingSprites = new();

        private static bool HasPrefabForTileId(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            EnsureCache();
            return !_prefabCache.TryGetValue(id, out bool has) || has;
        }

        private const string TilePrefabFolder = "Assets/Moyva/Prefabs/Tiles";

        private static void GeneratePrefabForTileId(string id, Sprite sprite)
        {
            var registry = FindRegistry();
            if (registry == null) return;

            EnsureFolder(TilePrefabFolder);
            string safe = id.Replace(' ', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{TilePrefabFolder}/{safe}.prefab");

            var go = new GameObject(safe);
            if (sprite != null) go.AddComponent<SpriteRenderer>().sprite = sprite;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            UnityEngine.Object.DestroyImmediate(go);

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");
            for (int i = 0; i < arr.arraySize; i++)
            {
                var el = arr.GetArrayElementAtIndex(i);
                if (el.FindPropertyRelative("_id")?.stringValue == id)
                {
                    el.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
                    break;
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateCache();
            Debug.Log($"[TileIdDrawer] Згенеровано prefab для \"{id}\": {path}");
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
