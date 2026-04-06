using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(WFCDataSettings))]
    public class WFCDataSettingsEditor : UnityEditor.Editor
    {
        private const string TilePrefabFolder = "Assets/Moyva/Prefabs/Tiles";

        private readonly Dictionary<string, Sprite> _pendingSprites = new();
        private Vector2 _missingPrefabsScroll;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            WFCDataSettings settings = (WFCDataSettings)target;

            DrawTileIdValidation(settings);

            if (GUILayout.Button("Open WFC Rules Editor"))
            {
                WFCRulesEditorWindow.OpenWindow(settings);
            }
        }

        private void DrawTileIdValidation(WFCDataSettings settings)
        {
            if (settings.TileRules == null || settings.TileRules.Count == 0) return;

            var registry = FindFirstRegistry();
            if (registry == null)
            {
                EditorGUILayout.HelpBox("TileRegistrySO не знайдено в проєкті.", MessageType.Warning);
                return;
            }

            var knownIds = LoadKnownTileIds();
            var usedIds = CollectUsedIds(settings);
            if (usedIds.Count == 0) return;

            if (knownIds.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "TileRegistrySO порожній або не містить ID. Всі ID з WFC правил будуть вважатися відсутніми.",
                    MessageType.Warning);
            }

            var unknownIds = new HashSet<string>();
            foreach (var id in usedIds)
            {
                if (!knownIds.Contains(id))
                    unknownIds.Add(id);
            }

            if (unknownIds.Count > 0)
            {
                var sb = new StringBuilder("Невідомі Tile ID у WFC правилах:\n");
                foreach (var id in unknownIds)
                    sb.AppendLine($"  • {id}");
                EditorGUILayout.HelpBox(sb.ToString().TrimEnd(), MessageType.Warning);

                if (GUILayout.Button($"Створити всі відсутні ID ({unknownIds.Count})", GUILayout.Height(24)))
                {
                    CreateMissingIds(registry, unknownIds);
                    EditorGUILayout.Space(4);
                }
            }

            DrawMissingPrefabSection(registry, usedIds);
        }

        private void DrawMissingPrefabSection(TileRegistrySO registry, HashSet<string> usedIds)
        {
            var missingPrefabIds = CollectMissingPrefabIds(registry, usedIds);
            if (missingPrefabIds.Count == 0)
            {
                EditorGUILayout.HelpBox("Усі ID, які використовує WFC, мають prefab.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(
                $"ID існує, але prefab відсутній: {missingPrefabIds.Count}.\n" +
                "Вкажи sprite і створи prefab з автоприв'язкою до TileRegistry.",
                MessageType.Warning);

            _missingPrefabsScroll = EditorGUILayout.BeginScrollView(_missingPrefabsScroll, GUILayout.MaxHeight(220));
            foreach (var id in missingPrefabIds)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(id, GUILayout.Width(260));

                _pendingSprites.TryGetValue(id, out var sprite);
                var newSprite = (Sprite)EditorGUILayout.ObjectField(sprite, typeof(Sprite), false, GUILayout.Width(160));
                if (newSprite != sprite)
                    _pendingSprites[id] = newSprite;

                EditorGUI.BeginDisabledGroup(newSprite == null);
                if (GUILayout.Button("Згенерувати", GUILayout.Width(110)))
                    CreatePrefabForId(registry, id, newSprite);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!HasAnySelectedSprite(missingPrefabIds));
            if (GUILayout.Button("Згенерувати prefab для вибраних", GUILayout.Height(24)))
            {
                int created = 0;
                foreach (var id in missingPrefabIds)
                {
                    if (_pendingSprites.TryGetValue(id, out var spr) && spr != null)
                    {
                        if (CreatePrefabForId(registry, id, spr))
                            created++;
                    }
                }
                if (created > 0)
                    AssetDatabase.SaveAssets();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        private static HashSet<string> CollectUsedIds(WFCDataSettings settings)
        {
            var usedIds = new HashSet<string>();
            foreach (var rule in settings.TileRules)
            {
                if (!string.IsNullOrWhiteSpace(rule.TileID))
                    usedIds.Add(rule.TileID.Trim());
                if (!string.IsNullOrWhiteSpace(rule.TileCentralID))
                    usedIds.Add(rule.TileCentralID.Trim());

                if (rule.Constraints == null) continue;
                foreach (var c in rule.Constraints)
                {
                    if (c.AllowedNeighbors == null) continue;
                    foreach (var n in c.AllowedNeighbors)
                        if (!string.IsNullOrWhiteSpace(n))
                            usedIds.Add(n.Trim());
                }
            }
            return usedIds;
        }

        private static List<string> CollectMissingPrefabIds(TileRegistrySO registry, HashSet<string> usedIds)
        {
            var list = new List<string>();
            if (registry.Definitions == null) return list;

            var known = new Dictionary<string, TileTypeDefinition>();
            foreach (var def in registry.Definitions)
            {
                if (def == null || string.IsNullOrWhiteSpace(def.Id)) continue;
                known[def.Id.Trim()] = def;
            }

            foreach (var id in usedIds)
            {
                if (known.TryGetValue(id, out var def) && def.VisualPrefab == null)
                    list.Add(id);
            }

            list.Sort();
            return list;
        }

        private static void CreateMissingIds(TileRegistrySO registry, HashSet<string> unknownIds)
        {
            var sorted = new List<string>(unknownIds);
            sorted.Sort();

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");
            if (arr == null)
            {
                EditorUtility.DisplayDialog("WFC", "Не вдалося знайти _definitions у TileRegistrySO.", "OK");
                return;
            }

            foreach (var id in sorted)
            {
                arr.arraySize++;
                var elem = arr.GetArrayElementAtIndex(arr.arraySize - 1);
                elem.FindPropertyRelative("_id").stringValue = id;
                elem.FindPropertyRelative("_movementCost").floatValue = 1f;
                elem.FindPropertyRelative("_visualPrefab").objectReferenceValue = null;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            InvalidateKnownIdsCache();
        }

        private bool CreatePrefabForId(TileRegistrySO registry, string id, Sprite sprite)
        {
            if (sprite == null) return false;

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");
            if (arr == null) return false;

            int idx = FindDefinitionIndex(arr, id);
            if (idx < 0) return false;

            GameObject prefab = CreatePrefabFromSprite(id, sprite, TilePrefabFolder);
            if (prefab == null) return false;

            var elem = arr.GetArrayElementAtIndex(idx);
            elem.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            return true;
        }

        private static int FindDefinitionIndex(SerializedProperty arr, string id)
        {
            for (int i = 0; i < arr.arraySize; i++)
            {
                var p = arr.GetArrayElementAtIndex(i).FindPropertyRelative("_id");
                if (string.Equals(p?.stringValue, id, System.StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        private static GameObject CreatePrefabFromSprite(string id, Sprite sprite, string folder)
        {
            EnsureFolder(folder);
            string safe = SanitizeFileName(id);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{safe}.prefab");

            var go = new GameObject(safe);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return;
            string[] parts = folder.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets") return;

            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }

        private static string SanitizeFileName(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return "tile";
            var chars = id.Trim().ToCharArray();
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            for (int i = 0; i < chars.Length; i++)
                if (System.Array.IndexOf(invalid, chars[i]) >= 0 || chars[i] == '/')
                    chars[i] = '-';
            return new string(chars);
        }

        private bool HasAnySelectedSprite(List<string> ids)
        {
            foreach (var id in ids)
                if (_pendingSprites.TryGetValue(id, out var s) && s != null)
                    return true;
            return false;
        }

        private static TileRegistrySO FindFirstRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
        }

        private static HashSet<string> _cachedKnownIds;
        private static double _knownIdsTime;

        private static void InvalidateKnownIdsCache()
        {
            _cachedKnownIds = null;
            _knownIdsTime = 0;
        }

        private static HashSet<string> LoadKnownTileIds()
        {
            double now = EditorApplication.timeSinceStartup;
            if (_cachedKnownIds != null && now - _knownIdsTime < 2.0)
                return _cachedKnownIds;

            _cachedKnownIds = LoadKnownTileIdsInternal();
            _knownIdsTime = now;
            return _cachedKnownIds;
        }

        private static HashSet<string> LoadKnownTileIdsInternal()
        {
            var result = new HashSet<string>();
            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return result;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (registry?.Definitions == null) return result;

            foreach (var def in registry.Definitions)
                if (!string.IsNullOrEmpty(def.Id))
                    result.Add(def.Id);

            return result;
        }
    }
}