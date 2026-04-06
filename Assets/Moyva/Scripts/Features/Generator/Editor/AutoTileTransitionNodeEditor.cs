using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(AutoTileTransitionNode))]
    public sealed class AutoTileTransitionNodeEditor : UnityEditor.Editor
    {
        private bool _validationFoldout;
        private bool _dependenciesFoldout;
        private bool _includeHeightTiers;
        private readonly List<string> _missingIds = new();
        private readonly List<string> _allRequiredIds = new();
        private bool _validated;
        private Vector2 _scrollPos;
        private Vector2 _depsScrollPos;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawNodeHeader();
            DrawNodeProperties();
            EditorGUILayout.Space(8);
            DrawDependencies();
            EditorGUILayout.Space(4);
            DrawValidation();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNodeHeader()
        {
            var node = (NodeBase)target;
            var info = (NodeInfoAttribute)System.Attribute.GetCustomAttribute(
                node.GetType(), typeof(NodeInfoAttribute));
            if (info != null && !string.IsNullOrWhiteSpace(info.Description))
            {
                EditorGUILayout.HelpBox(info.Description, MessageType.Info);
                EditorGUILayout.Space(4);
            }
        }

        private void DrawNodeProperties()
        {
            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(false))
            {
                if (prop.name is "_nodeId" or "_editorPosition") continue;
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        private void DrawDependencies()
        {
            _dependenciesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _dependenciesFoldout, "Залежності (необхідні Tile ID)");

            if (_dependenciesFoldout)
            {
                EditorGUILayout.Space(2);

                string sep    = serializedObject.FindProperty("_separator")?.stringValue ?? "-";
                bool   diag   = serializedObject.FindProperty("_diagonalEdges")?.boolValue ?? true;
                var    exclProp = serializedObject.FindProperty("_excludedTileTypes");
                var    excluded = CollectExcluded(exclProp);

                var registry = FindAsset<TileRegistrySO>();
                if (registry == null)
                {
                    EditorGUILayout.HelpBox("TileRegistrySO не знайдено — підключіть реєстр.", MessageType.Warning);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    return;
                }

                // collect active base types (excluding excluded ones)
                var baseTypes = new SortedSet<string>();
                foreach (var def in registry.Definitions)
                {
                    if (string.IsNullOrEmpty(def.Id)) continue;
                    int idx = def.Id.IndexOf('-');
                    string bt = idx > 0 ? def.Id.Substring(0, idx) : def.Id;
                    if (!excluded.Contains(bt)) baseTypes.Add(bt);
                }

                var suffixes = BuildAllSuffixes(diag);
                _allRequiredIds.Clear();
                foreach (var bt in baseTypes)
                {
                    _allRequiredIds.Add(bt);
                    foreach (var s in suffixes)
                        _allRequiredIds.Add($"{bt}{sep}{s}");
                }

                // count registered vs missing
                var registered = new HashSet<string>();
                foreach (var def in registry.Definitions)
                    if (!string.IsNullOrEmpty(def.Id)) registered.Add(def.Id);

                int total   = _allRequiredIds.Count;
                int present = 0;
                foreach (var id in _allRequiredIds)
                    if (registered.Contains(id)) present++;

                EditorGUILayout.HelpBox(
                    $"Базових типів (без виключень): {baseTypes.Count}\n" +
                    $"Необхідних tile ID: {total}\n" +
                    $"Зареєстровано: {present}  |  Відсутні: {total - present}",
                    total - present == 0 ? MessageType.Info : MessageType.Warning);

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Всі необхідні ID:", EditorStyles.boldLabel);
                _depsScrollPos = EditorGUILayout.BeginScrollView(_depsScrollPos, GUILayout.MaxHeight(220));
                foreach (var id in _allRequiredIds)
                {
                    bool ok = registered.Contains(id);
                    Color prev = GUI.color;
                    GUI.color = ok ? Color.white : new Color(1f, 0.4f, 0.4f);
                    EditorGUILayout.LabelField((ok ? "  \u2713 " : "  \u2717 ") + id);
                    GUI.color = prev;
                }
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawValidation()
        {
            _validationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _validationFoldout, "Валідація реєстру тайлів");

            if (_validationFoldout)
            {
                EditorGUILayout.Space(2);
                _includeHeightTiers = EditorGUILayout.Toggle(
                    "Включити висотні рівні (0–4)", _includeHeightTiers);
                EditorGUILayout.Space(4);

                if (GUILayout.Button("Перевірити реєстр", GUILayout.Height(24)))
                    Validate();

                if (_validated)
                    DrawResults();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void Validate()
        {
            _missingIds.Clear();

            var registry = FindAsset<TileRegistrySO>();
            if (registry == null)
            {
                EditorUtility.DisplayDialog("Помилка",
                    "TileRegistrySO не знайдено в проєкті.", "OK");
                _validated = false;
                return;
            }

            string sep = serializedObject.FindProperty("_separator")?.stringValue ?? "-";
            bool diag = serializedObject.FindProperty("_diagonalEdges")?.boolValue ?? true;
            var exclProp = serializedObject.FindProperty("_excludedTileTypes");
            var excluded = CollectExcluded(exclProp);

            var registered = new HashSet<string>();
            var baseTypes = new HashSet<string>();

            foreach (var def in registry.Definitions)
            {
                if (string.IsNullOrEmpty(def.Id)) continue;
                registered.Add(def.Id);
                int idx = def.Id.IndexOf('-');
                string bt = idx > 0 ? def.Id.Substring(0, idx) : def.Id;
                if (!excluded.Contains(bt)) baseTypes.Add(bt);
            }

            var suffixes = BuildAllSuffixes(diag);
            int maxTier = _includeHeightTiers ? 4 : 0;

            foreach (var bt in baseTypes)
            {
                for (int t = 0; t <= maxTier; t++)
                {
                    string prefix = t > 0 ? $"{bt}{sep}{t}" : bt;

                    if (!registered.Contains(prefix))
                        _missingIds.Add(prefix);

                    foreach (var s in suffixes)
                    {
                        string id = $"{prefix}{sep}{s}";
                        if (!registered.Contains(id))
                            _missingIds.Add(id);
                    }
                }
            }

            _missingIds.Sort();
            _validated = true;
        }

        private void DrawResults()
        {
            EditorGUILayout.Space(4);

            if (_missingIds.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Усі можливі Tile ID вже зареєстровано.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(
                $"Відсутніх ID у реєстрі: {_missingIds.Count}", MessageType.Warning);

            _scrollPos = EditorGUILayout.BeginScrollView(
                _scrollPos, GUILayout.MaxHeight(200));
            foreach (var id in _missingIds)
                EditorGUILayout.LabelField("  \u2022 " + id);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);

            if (GUILayout.Button(
                $"Створити усі відсутні ({_missingIds.Count})",
                GUILayout.Height(28)))
            {
                CreateMissingTiles();
            }
        }

        private void CreateMissingTiles()
        {
            var registry = FindAsset<TileRegistrySO>();
            if (registry == null) return;

            var so = new SerializedObject(registry);
            var arr = so.FindProperty("_definitions");

            foreach (var id in _missingIds)
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

            Debug.Log($"[AutoTileTransition] Створено {_missingIds.Count} записів у TileRegistrySO.");
            _missingIds.Clear();
            _validated = false;
        }

        private static List<string> BuildAllSuffixes(bool diag)
        {
            var list = new List<string>
            {
                "cliff-N", "cliff-E", "cliff-S", "cliff-W",
                "cliff-NE", "cliff-NW", "cliff-SE", "cliff-SW",
                "cliff-NS", "cliff-EW",
                "cliff-SEW", "cliff-NSW", "cliff-NEW", "cliff-NSE",
                "cliff-ALL"
            };

            if (diag)
            {
                string[] dirs = { "NE", "SE", "SW", "NW" };
                int count = dirs.Length;
                for (int mask = 1; mask < (1 << count); mask++)
                {
                    string combo = "corner";
                    for (int b = 0; b < count; b++)
                    {
                        if ((mask & (1 << b)) != 0)
                            combo += "-" + dirs[b];
                    }
                    list.Add(combo);
                }
            }

            return list;
        }

        private static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static HashSet<string> CollectExcluded(SerializedProperty prop)
        {
            var set = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (prop == null) return set;
            for (int i = 0; i < prop.arraySize; i++)
            {
                string v = prop.GetArrayElementAtIndex(i).stringValue?.Trim();
                if (!string.IsNullOrEmpty(v)) set.Add(v);
            }
            return set;
        }
    }
}
