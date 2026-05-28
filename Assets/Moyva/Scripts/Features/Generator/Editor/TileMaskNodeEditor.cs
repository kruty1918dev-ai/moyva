using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    [CustomEditor(typeof(TileMaskNode))]
    public sealed class TileMaskNodeEditor : UnityEditor.Editor
    {
        private const string GraphEditorWindowSettingsPath =
            "Assets/Moyva/Scripts/Features/GraphSystem/Editor/GraphEditorWindowSettings.asset";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            var info = (NodeInfoAttribute)System.Attribute.GetCustomAttribute(
                typeof(TileMaskNode), typeof(NodeInfoAttribute));
            if (info != null && !string.IsNullOrWhiteSpace(info.Description))
                EditorGUILayout.HelpBox(info.Description, MessageType.Info);

            EditorGUILayout.Space(4);

            // _matchByBaseKey
            var matchByKeyProp = serializedObject.FindProperty("_matchByBaseKey");
            EditorGUILayout.PropertyField(matchByKeyProp,
                new GUIContent("Збіг по базовому ключу",
                    "Якщо увімкнено — тайл 'water' співпадатиме з 'water-deep-001', 'water-shallow-002' тощо."));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Цільові тайли", EditorStyles.boldLabel);

            var targetProp = serializedObject.FindProperty("_targetTileIds");

            // List of selected tiles with remove button
            if (targetProp.arraySize > 0)
            {
                EditorGUILayout.Space(2);
                int removeAt = -1;
                for (int i = 0; i < targetProp.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        string id = targetProp.GetArrayElementAtIndex(i).stringValue;
                        EditorGUILayout.LabelField(string.IsNullOrEmpty(id) ? "(empty)" : id,
                            GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("×", GUILayout.Width(22f)))
                            removeAt = i;
                    }
                }
                if (removeAt >= 0)
                {
                    targetProp.DeleteArrayElementAtIndex(removeAt);
                    serializedObject.ApplyModifiedProperties();
                    NotifyGraphEditorChanged();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.Space(2);
            }
            else
            {
                EditorGUILayout.HelpBox("Список порожній — маска буде all-false.", MessageType.Warning);
            }

            // Buttons row
            Rect rowRect = GUILayoutUtility.GetRect(
                GUIContent.none, EditorStyles.miniButton,
                GUILayout.Height(EditorGUIUtility.singleLineHeight + 4f));
            float clearWidth = 120f;
            Rect btnRect = targetProp.arraySize > 0
                ? new Rect(rowRect.x, rowRect.y, rowRect.width - clearWidth - 6f, rowRect.height)
                : rowRect;
            Rect clearRect = new Rect(btnRect.xMax + 6f, rowRect.y, clearWidth, rowRect.height);

            if (EditorGUI.DropdownButton(btnRect,
                new GUIContent("⊕ Додати тайли..."),
                FocusType.Keyboard))
            {
                var ids     = GetTileIdsFromRegistry();
                var sprites = GetTileSpritesFromRegistry();

                var already = new HashSet<string>();
                for (int i = 0; i < targetProp.arraySize; i++)
                    already.Add(targetProp.GetArrayElementAtIndex(i).stringValue);

                var so   = serializedObject;
                var path = targetProp.propertyPath;

                PopupWindow.Show(btnRect, new MultiTilePickerPopup(
                    ids, sprites, already,
                    added =>
                    {
                        so.Update();
                        var p = so.FindProperty(path);
                        foreach (var id in added)
                        {
                            bool dup = false;
                            for (int k = 0; k < p.arraySize; k++)
                                if (p.GetArrayElementAtIndex(k).stringValue == id) { dup = true; break; }
                            if (!dup)
                            {
                                p.arraySize++;
                                p.GetArrayElementAtIndex(p.arraySize - 1).stringValue = id;
                            }
                        }
                        so.ApplyModifiedProperties();
                        NotifyGraphEditorChanged();
                    }));
            }

            if (targetProp.arraySize > 0 && GUI.Button(clearRect, "Очистити всі", EditorStyles.miniButton))
            {
                targetProp.ClearArray();
                serializedObject.ApplyModifiedProperties();
                NotifyGraphEditorChanged();
                GUIUtility.ExitGUI();
            }

            if (serializedObject.ApplyModifiedProperties())
                NotifyGraphEditorChanged();
        }

        // ── Helpers ──

        private void NotifyGraphEditorChanged()
        {
            EditorUtility.SetDirty(target);
            Repaint();

            var graphWindowType = System.Type.GetType(
                "Kruty1918.Moyva.GraphSystem.Editor.GraphEditorWindow, Kruty1918.Moyva.GraphSystem.Editor");
            if (graphWindowType == null) return;

            var requestAutoRun = graphWindowType.GetMethod(
                "RequestAutoRun",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            foreach (var window in Resources.FindObjectsOfTypeAll(graphWindowType))
            {
                requestAutoRun?.Invoke(window, null);
                if (window is EditorWindow ew) ew.Repaint();
            }
        }

        private static List<string> GetTileIdsFromRegistry()
        {
            var reg = LoadTileRegistry();
            if (reg?.Definitions == null) return new List<string>();
            return reg.Definitions
                .Where(d => !string.IsNullOrEmpty(d.Id))
                .Select(d => d.Id)
                .OrderBy(id => id)
                .ToList();
        }

        private static Dictionary<string, Sprite> GetTileSpritesFromRegistry()
        {
            var sprites = new Dictionary<string, Sprite>();
            var reg = LoadTileRegistry();
            if (reg?.Definitions == null) return sprites;
            foreach (var def in reg.Definitions)
            {
                if (string.IsNullOrEmpty(def.Id) || def.VisualPrefab == null) continue;
                if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(def.VisualPrefab, out var sprite, out _))
                    sprites[def.Id] = sprite;
            }
            return sprites;
        }

        private static TileRegistrySO LoadTileRegistry()
        {
            if (TryGetRegistryFromGraphWindowSettings(out var reg)) return reg;

            const string path = "Assets/Moyva/SO/Tile/TileRegistry.asset";
            var direct = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (direct != null) return direct;

            var guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<TileRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static bool TryGetRegistryFromGraphWindowSettings(out TileRegistrySO registry)
        {
            registry = null;
            var settingsObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GraphEditorWindowSettingsPath);
            if (settingsObj == null) return false;

            var windowSo = new SerializedObject(settingsObj);
            ScriptableObject previewObj = null;

            var directProp = windowSo.FindProperty("previewSettings");
            if (directProp?.objectReferenceValue is ScriptableObject direct)
            {
                previewObj = direct;
            }
            else
            {
                var guidProp = windowSo.FindProperty("previewSettingsGuid");
                string guid  = guidProp?.stringValue;
                if (!string.IsNullOrEmpty(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath))
                        previewObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                }
            }

            if (previewObj == null) return false;

            var previewSo    = new SerializedObject(previewObj);
            var registryProp = previewSo.FindProperty("_tileRegistry");
            registry = registryProp?.objectReferenceValue as TileRegistrySO;
            return true;
        }

        // ── Multi-tile picker popup ──

        private sealed class MultiTilePickerPopup : PopupWindowContent
        {
            private const float PopupWidth  = 560f;
            private const float PopupHeight = 520f;
            private const float SearchH     = 20f;
            private const float BtnH        = 24f;
            private const float RowH        = 36f;
            private const float IconSize    = 28f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "TileMaskSearch";

            private readonly List<string>                _ids;
            private readonly Dictionary<string, Sprite>  _sprites;
            private readonly HashSet<string>             _existing;
            private readonly System.Action<List<string>> _onConfirm;
            private readonly HashSet<string>             _checked = new();
            private Vector2 _scroll;
            private string  _search      = string.Empty;
            private bool    _focusSearch = true;

            public MultiTilePickerPopup(
                List<string> ids,
                Dictionary<string, Sprite> sprites,
                HashSet<string> current,
                System.Action<List<string>> onConfirm)
            {
                _ids       = ids;
                _sprites   = sprites;
                _existing  = new HashSet<string>(current);
                _onConfirm = onConfirm;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);
            public override void OnOpen() { _focusSearch = true; }

            public override void OnGUI(Rect rect)
            {
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchH);
                GUI.SetNextControlName(SearchCtrl);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) _scroll = Vector2.zero;
                if (_focusSearch) { _focusSearch = false; EditorGUI.FocusTextInControl(SearchCtrl); }

                var filtered = GetFiltered();

                float btnY = searchRect.yMax + Padding;
                Rect selAllRect = new Rect(Padding, btnY, (rect.width - Padding * 3f) * 0.5f, BtnH);
                Rect clearRect  = new Rect(selAllRect.xMax + Padding, btnY, selAllRect.width, BtnH);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUI.Button(selAllRect, $"Вибрати всі ({filtered.Count})"))
                    foreach (var id in filtered) _checked.Add(id);

                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUI.Button(clearRect, "Зняти всі")) _checked.Clear();
                GUI.backgroundColor = Color.white;

                float listTop    = btnY + BtnH + Padding;
                float listH      = rect.height - listTop - BtnH - Padding * 3f;
                Rect  viewRect   = new Rect(Padding, listTop, rect.width - Padding * 2f, listH);
                Rect  contentRect = new Rect(0f, 0f, viewRect.width - 14f,
                    Mathf.Max(listH, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
                for (int i = 0; i < filtered.Count; i++)
                {
                    string id           = filtered[i];
                    Rect   rowRect      = new Rect(0f, i * RowH, contentRect.width, RowH - 1f);
                    bool   isChecked    = _checked.Contains(id);
                    bool   alreadyAdded = _existing.Contains(id);

                    if (isChecked)
                        EditorGUI.DrawRect(rowRect, new Color(0.24f, 0.48f, 0.24f, 0.55f));
                    else if (alreadyAdded)
                        EditorGUI.DrawRect(rowRect, new Color(0.4f, 0.4f, 0.0f, 0.3f));
                    else if (rowRect.Contains(Event.current.mousePosition))
                        EditorGUI.DrawRect(rowRect, new Color(1f, 1f, 1f, 0.07f));

                    Rect toggleRect = new Rect(rowRect.x + 2f, rowRect.y + (RowH - 16f) * 0.5f, 16f, 16f);
                    bool newChecked = GUI.Toggle(toggleRect, isChecked, GUIContent.none);
                    if (newChecked != isChecked)
                    {
                        if (newChecked) _checked.Add(id);
                        else _checked.Remove(id);
                    }

                    Rect iconRect = new Rect(toggleRect.xMax + 4f,
                        rowRect.y + (RowH - IconSize) * 0.5f, IconSize, IconSize);
                    if (_sprites.TryGetValue(id, out Sprite sprite) && sprite != null)
                    {
                        AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, null, sprite);
                    }
                    else
                    {
                        GUI.Box(iconRect, string.Empty);
                    }

                    Rect labelRect  = new Rect(iconRect.xMax + 5f, rowRect.y,
                        rowRect.width - iconRect.xMax - 6f, RowH);
                    string rowLabel = alreadyAdded ? $"{id}  (вже є)" : id;
                    GUI.Label(labelRect, rowLabel,
                        alreadyAdded ? EditorStyles.miniLabel : EditorStyles.label);

                    if (Event.current.type == EventType.MouseDown &&
                        rowRect.Contains(Event.current.mousePosition))
                    {
                        if (_checked.Contains(id)) _checked.Remove(id);
                        else _checked.Add(id);
                        Event.current.Use();
                    }
                }
                GUI.EndScrollView();

                int addCount = _checked.Count(id => !_existing.Contains(id));
                Rect addRect = new Rect(Padding, rect.height - BtnH - Padding,
                    rect.width - Padding * 2f, BtnH);
                GUI.backgroundColor = addCount > 0 ? new Color(0.2f, 0.7f, 0.2f) : Color.gray;
                EditorGUI.BeginDisabledGroup(addCount == 0);
                if (GUI.Button(addRect, $"Додати вибрані ({addCount})"))
                {
                    var toAdd = _checked.Where(id => !_existing.Contains(id)).ToList();
                    _onConfirm?.Invoke(toAdd);
                    editorWindow.Close();
                }
                EditorGUI.EndDisabledGroup();
                GUI.backgroundColor = Color.white;
            }

            private List<string> GetFiltered()
            {
                if (string.IsNullOrWhiteSpace(_search)) return new List<string>(_ids);
                var terms = _search.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                return _ids.Where(id =>
                    System.Array.TrueForAll(terms,
                        t => id.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }
        }
    }
}
