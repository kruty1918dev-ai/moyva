using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Generator.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomPropertyDrawer(typeof(MapObjectIdAttribute))]
    public class MapObjectIdDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight;
            if (property.propertyType == SerializedPropertyType.String)
            {
                string val = property.stringValue;
                if (!string.IsNullOrEmpty(val))
                {
                    if (System.Array.IndexOf(GetObjectIds(), val) < 0)
                        h += EditorGUIUtility.singleLineHeight + 4;
                    else if (!HasPrefabForObjectId(val))
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

            string[] ids = GetObjectIds();
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
                PopupWindow.Show(buttonRect, new MapObjectPickerPopup(
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
            Sprite sprite = GetSpriteForObjectId(currentValue);
            GameObject prefab = GetPrefabForObjectId(currentValue);
            if ((sprite != null && sprite.texture != null) || prefab != null)
                AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(spriteRect, prefab, sprite);

            // Error box + create button (ID not found)
            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                float y2 = position.y + lineH + 2;
                float btnW = 80;
                Rect errorRect = new Rect(position.x, y2, position.width - btnW - 4, lineH);
                Rect btnRect = new Rect(position.xMax - btnW, y2, btnW, lineH);

                EditorGUI.HelpBox(errorRect, $"ID \"{currentValue}\" не знайдено!", MessageType.Error);
                if (GUI.Button(btnRect, "+ Створити"))
                    CreateObjectEntry(currentValue);
            }

            // Fatal: ID exists but prefab missing
            if (isValid && !HasPrefabForObjectId(currentValue))
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
                    GeneratePrefabForObjectId(currentValue, spr);
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

        private sealed class MapObjectPickerPopup : PopupWindowContent
        {
            private const float PopupWidth = 420f;
            private const float PopupHeight = 360f;
            private const float RowHeight = 22f;
            private const float IconSize = 16f;
            private const float Padding = 6f;

            private readonly string _currentValue;
            private readonly Action<string> _onSelected;
            private Vector2 _scroll;

            public MapObjectPickerPopup(string currentValue, Action<string> onSelected)
            {
                _currentValue = currentValue;
                _onSelected = onSelected;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);

            public override void OnGUI(Rect rect)
            {
                var ids = GetObjectIds();

                Rect noneRect = new Rect(Padding, Padding, rect.width - Padding * 2f, RowHeight);
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
                    Padding + RowHeight + Padding,
                    rect.width - Padding * 2f,
                    rect.height - (Padding * 3f + RowHeight));

                Rect contentRect = new Rect(0f, 0f, viewRect.width - 14f, Mathf.Max(2f, ids.Length * RowHeight));
                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
                for (int i = 0; i < ids.Length; i++)
                {
                    Rect rowRect = new Rect(0f, i * RowHeight, contentRect.width, RowHeight - 1f);
                    DrawMapObjectRow(rowRect, ids[i]);
                }
                GUI.EndScrollView();
            }

            private void DrawMapObjectRow(Rect rowRect, string id)
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
                var sprite = GetSpriteForObjectId(id);
                var prefab = GetPrefabForObjectId(id);
                if ((sprite != null && sprite.texture != null) || prefab != null)
                    AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, prefab, sprite);

                Rect labelRect = new Rect(iconRect.xMax + 6f, rowRect.y + 2f, rowRect.width - iconRect.width - 10f, RowHeight - 2f);
                GUI.Label(labelRect, id, EditorStyles.label);
            }
        }

        // ── Static cache (1-second TTL) ──
        private static MapObjectRegistrySO _cachedRegistry;
        private static string[] _cachedIds = System.Array.Empty<string>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
        private static readonly Dictionary<string, GameObject> _prefabObjectCache = new();
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
            _prefabObjectCache.Clear();
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
                    _prefabObjectCache[def.Id] = def.VisualPrefab;
                    if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(def.VisualPrefab, out var sprite, out _))
                        _spriteCache[def.Id] = sprite;
                }
            }
            ids.Sort();
            _cachedIds = ids.ToArray();
        }

        private static void InvalidateCache() => _cacheTime = 0;

        private static MapObjectRegistrySO FindRegistryInternal()
        {
            string[] guids = AssetDatabase.FindAssets("t:MapObjectRegistrySO");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<MapObjectRegistrySO>(path);
        }

        private static MapObjectRegistrySO FindRegistry()
        {
            EnsureCache();
            return _cachedRegistry;
        }

        private static string[] GetObjectIds()
        {
            EnsureCache();
            return _cachedIds;
        }

        private static Sprite GetSpriteForObjectId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            return _spriteCache.TryGetValue(id, out var s) ? s : null;
        }

        private static GameObject GetPrefabForObjectId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            return _prefabObjectCache.TryGetValue(id, out var prefab) ? prefab : null;
        }

        private static void CreateObjectEntry(string id)
        {
            var registry = FindRegistry();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Помилка", "MapObjectRegistrySO не знайдено в проєкті.", "OK");
                return;
            }

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");
            arr.arraySize++;
            var newElem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
            newElem.FindPropertyRelative("_id").stringValue = id;
            newElem.FindPropertyRelative("_visualPrefab").objectReferenceValue = null;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateCache();
            Debug.Log($"[MapObjectIdDrawer] Створено новий запис MapObject з ID: \"{id}\" в {AssetDatabase.GetAssetPath(registry)}");
        }

        private static readonly Dictionary<string, Sprite> _pendingSprites = new();

        private static bool HasPrefabForObjectId(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;
            EnsureCache();
            return !_prefabCache.TryGetValue(id, out bool has) || has;
        }

        private const string ObjectPrefabFolder = "Assets/Moyva/Prefabs/Objects";

        private static void GeneratePrefabForObjectId(string id, Sprite sprite)
        {
            var registry = FindRegistry();
            if (registry == null) return;

            EnsureFolder(ObjectPrefabFolder);
            string safe = id.Replace(' ', '-');
            string path = AssetDatabase.GenerateUniqueAssetPath($"{ObjectPrefabFolder}/{safe}.prefab");

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
            Debug.Log($"[MapObjectIdDrawer] Згенеровано prefab для \"{id}\": {path}");
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
