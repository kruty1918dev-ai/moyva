using System.Collections.Generic;
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

            var displayOptions = BuildDisplayOptions(ids, currentValue, isValid, out int selectedIndex);

            EditorGUI.BeginProperty(position, label, property);

            int newIndex = EditorGUI.Popup(popupRect, label.text, selectedIndex, displayOptions);
            if (newIndex != selectedIndex)
            {
                string selected = displayOptions[newIndex];
                if (selected == "(none)")
                    property.stringValue = string.Empty;
                else if (!selected.StartsWith("\u26a0 "))
                    property.stringValue = selected;
            }

            GUI.color = prevColor;

            // Sprite preview
            Sprite sprite = GetSpriteForTileId(currentValue);
            if (sprite != null)
                GUI.DrawTexture(spriteRect, sprite.texture, ScaleMode.ScaleToFit);

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

        private static string[] BuildDisplayOptions(string[] ids, string currentValue, bool isValid, out int selectedIndex)
        {
            var list = new List<string> { "(none)" };
            list.AddRange(ids);

            if (!isValid && !string.IsNullOrEmpty(currentValue))
            {
                list.Insert(0, $"\u26a0 {currentValue}");
                selectedIndex = 0;
            }
            else
            {
                selectedIndex = string.IsNullOrEmpty(currentValue) ? 0 : list.IndexOf(currentValue);
                if (selectedIndex < 0) selectedIndex = 0;
            }

            return list.ToArray();
        }

        // ── Static cache (1-second TTL) ──
        private static TileRegistrySO _cachedRegistry;
        private static string[] _cachedIds = System.Array.Empty<string>();
        private static readonly Dictionary<string, Sprite> _spriteCache = new();
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
                        _spriteCache[def.Id] = sr.sprite;
                }
            }
            ids.Sort();
            _cachedIds = ids.ToArray();
        }

        private static void InvalidateCache() => _cacheTime = 0;

        private static TileRegistrySO FindRegistryInternal()
        {
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
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
