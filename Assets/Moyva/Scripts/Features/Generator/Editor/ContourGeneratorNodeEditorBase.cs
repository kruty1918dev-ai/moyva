using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Базовий клас для редакторів нодів, що генерують контурні тайли.
    /// Містить спільну логіку: 3×3 сітку напрямків, виключення, валідацію та tile picker popup.
    /// Підкласи надають: назву масиву тайлів, підпис центральної клітинки,
    /// список пропускаємих полів та додаткові секції.
    /// </summary>
    public abstract class ContourGeneratorNodeEditorBase : UnityEditor.Editor
    {
        // ── Спільні статичні дані для сітки ──

        private static readonly Dictionary<HillDirection, (int col, int row)> k_GridPositions = new()
        {
            { HillDirection.CornerNW, (0, 2) },
            { HillDirection.North,    (1, 2) },
            { HillDirection.CornerNE, (2, 2) },
            { HillDirection.West,     (0, 1) },
            { HillDirection.East,     (2, 1) },
            { HillDirection.CornerSW, (0, 0) },
            { HillDirection.South,    (1, 0) },
            { HillDirection.CornerSE, (2, 0) },
        };

        private static readonly Dictionary<HillDirection, (int col, int row)> k_InnerGridPositions = new()
        {
            { HillDirection.InnerCornerNW, (0, 2) },
            { HillDirection.InnerCornerNE, (2, 2) },
            { HillDirection.InnerCornerSW, (0, 0) },
            { HillDirection.InnerCornerSE, (2, 0) },
        };

        private static readonly Dictionary<HillDirection, string> k_Labels = new()
        {
            { HillDirection.North,         "N"   },
            { HillDirection.South,         "S"   },
            { HillDirection.East,          "E"   },
            { HillDirection.West,          "W"   },
            { HillDirection.CornerNE,      "NE"  },
            { HillDirection.CornerNW,      "NW"  },
            { HillDirection.CornerSE,      "SE"  },
            { HillDirection.CornerSW,      "SW"  },
            { HillDirection.InnerCornerNE, "iNE" },
            { HillDirection.InnerCornerNW, "iNW" },
            { HillDirection.InnerCornerSE, "iSE" },
            { HillDirection.InnerCornerSW, "iSW" },
        };

        protected const float CellSize    = 60f;
        protected const float CellPadding = 4f;
        protected const float GridSize    = CellSize * 3 + CellPadding * 4;

        // ── Стан ──

        private bool _outerFoldout      = true;
        private bool _innerFoldout      = true;
        private bool _validationFoldout;
        private bool _exclusionsFoldout = true;

        private readonly List<HillDirection> _missingEntries = new();
        private bool _validated;

        private readonly Dictionary<HillDirection, int> _indexCache = new();

        private GUIStyle _centeredWhiteMini;
        private GUIStyle _centerBoldWhite;

        private GUIStyle CenteredWhiteMini => _centeredWhiteMini ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            { normal = { textColor = Color.white } };

        // ── Abstract / Virtual ──

        /// <summary>Назва serialized-поля масиву тайлів (наприклад "_hillTiles" або "_contourTiles").</summary>
        protected abstract string TileArrayPropertyName { get; }

        /// <summary>Текст у центральній клітинці сітки (наприклад "▲\nhill" або "≈\nwater").</summary>
        protected abstract string CenterCellLabel { get; }

        /// <summary>Набір додаткових імен полів, які пропускаються у DrawMainProperties.</summary>
        protected virtual HashSet<string> GetExtraSkippedProperties() => new HashSet<string>();

        /// <summary>Секції між Exclusions і сіткою (наприклад, порогові рівні та превью для Hill).</summary>
        protected virtual void DrawMiddleSections() { }

        // ── Lifecycle ──

        protected virtual void OnDisable()
        {
            _centeredWhiteMini = null;
            _centerBoldWhite   = null;
        }

        // ── OnInspectorGUI ──

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RebuildIndexCache();

            DrawNodeHeader();
            EditorGUILayout.Space(4);
            DrawMainProperties();
            EditorGUILayout.Space(8);
            DrawExclusionsEditor();
            EditorGUILayout.Space(8);
            DrawMiddleSections();
            DrawOuterGrid();
            EditorGUILayout.Space(8);
            DrawInnerGrid();
            EditorGUILayout.Space(8);
            DrawValidation();

            if (serializedObject.ApplyModifiedProperties())
                NotifyGraphEditorChanged();
        }

        // ── Секції ──

        protected void DrawNodeHeader()
        {
            var node = (NodeBase)target;
            var info = (NodeInfoAttribute)System.Attribute.GetCustomAttribute(
                node.GetType(), typeof(NodeInfoAttribute));
            if (info != null && !string.IsNullOrWhiteSpace(info.Description))
                EditorGUILayout.HelpBox(info.Description, MessageType.Info);
        }

        protected void DrawMainProperties()
        {
            var skipped = new HashSet<string>
            {
                "_nodeId",
                "_editorPosition",
                "_excludedNeighborTileTypes",
                TileArrayPropertyName
            };
            foreach (var extra in GetExtraSkippedProperties())
                skipped.Add(extra);

            var prop = serializedObject.GetIterator();
            prop.NextVisible(true);
            while (prop.NextVisible(false))
            {
                if (skipped.Contains(prop.name)) continue;
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        protected void DrawExclusionsEditor()
        {
            _exclusionsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _exclusionsFoldout, "Виключення (сусідні тайли)");

            if (_exclusionsFoldout)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Якщо у кардинального сусіда є тайл з цього списку — правило контуру пропускається. " +
                    "Порівнюється повний ID тайла або базовий ключ до '-'.",
                    MessageType.None);

                var exclProp = serializedObject.FindProperty("_excludedNeighborTileTypes");

                if (exclProp.arraySize > 0)
                {
                    EditorGUILayout.Space(2);
                    int removeAt = -1;
                    for (int i = 0; i < exclProp.arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            string id = exclProp.GetArrayElementAtIndex(i).stringValue;
                            EditorGUILayout.LabelField(string.IsNullOrEmpty(id) ? "(empty)" : id,
                                GUILayout.ExpandWidth(true));
                            if (GUILayout.Button("×", GUILayout.Width(22f)))
                                removeAt = i;
                        }
                    }
                    if (removeAt >= 0)
                    {
                        exclProp.DeleteArrayElementAtIndex(removeAt);
                        serializedObject.ApplyModifiedProperties();
                        NotifyGraphEditorChanged();
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.Space(2);
                }

                Rect rowRect = GUILayoutUtility.GetRect(
                    GUIContent.none, EditorStyles.miniButton,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight + 4f));
                float clearWidth = 120f;
                Rect btnRect = exclProp.arraySize > 0
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
                    for (int i = 0; i < exclProp.arraySize; i++)
                        already.Add(exclProp.GetArrayElementAtIndex(i).stringValue);

                    var so   = serializedObject;
                    var path = exclProp.propertyPath;

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

                if (exclProp.arraySize > 0 && GUI.Button(clearRect, "Очистити всі", EditorStyles.miniButton))
                {
                    exclProp.ClearArray();
                    serializedObject.ApplyModifiedProperties();
                    NotifyGraphEditorChanged();
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawOuterGrid()
        {
            _outerFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_outerFoldout,
                "Зовнішні краї та кути");

            if (_outerFoldout)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Клітинки по периметру 3×3 сітки. " +
                    "Центр — сам тайл контуру (не налаштовується). " +
                    "Призначте Tile ID для кожного напрямку краю.",
                    MessageType.None);
                EditorGUILayout.Space(4);
                DrawDirectionGrid(k_GridPositions, showCenter: true);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawInnerGrid()
        {
            _innerFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_innerFoldout,
                "Внутрішні кути");

            if (_innerFoldout)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Внутрішні кути застосовуються, коли кардинальні краї відсутні, " +
                    "але діагональний сусід є нижчого рівня (увігнутий кут).",
                    MessageType.None);
                EditorGUILayout.Space(4);
                DrawDirectionGrid(k_InnerGridPositions, showCenter: false);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawValidation()
        {
            _validationFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
                _validationFoldout, "Валідація Tile ID");

            if (_validationFoldout)
            {
                if (GUILayout.Button("Перевірити"))
                    RunValidation();

                if (_validated)
                {
                    if (_missingEntries.Count == 0)
                        EditorGUILayout.HelpBox("Всі напрямки мають Tile ID.", MessageType.Info);
                    else
                        foreach (var dir in _missingEntries)
                            EditorGUILayout.HelpBox($"Відсутній Tile ID: {dir}", MessageType.Warning);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        // ── Grid drawing ──

        private void DrawDirectionGrid(
            Dictionary<HillDirection, (int col, int row)> positions,
            bool showCenter)
        {
            float totalWidth = Mathf.Min(GridSize, EditorGUIUtility.currentViewWidth - 32f);
            float scale = totalWidth / GridSize;
            float cell  = CellSize * scale;
            float pad   = CellPadding * scale;

            var gridRect      = GUILayoutUtility.GetRect(totalWidth, totalWidth);
            var tileArrayProp = serializedObject.FindProperty(TileArrayPropertyName);

            if (showCenter)
                DrawCenterCell(gridRect, cell, pad, scale);

            foreach (var (dir, (col, row)) in positions)
                DrawDirectionCell(GetCellRect(gridRect, col, row, cell, pad), dir, scale, tileArrayProp);
        }

        private void DrawCenterCell(Rect gridRect, float cell, float pad, float scale)
        {
            var r  = GetCellRect(gridRect, 1, 1, cell, pad);
            int fs = Mathf.RoundToInt(9 * scale);
            EditorGUI.DrawRect(r, new Color(0.3f, 0.3f, 0.3f, 0.4f));

            if (_centerBoldWhite == null || _centerBoldWhite.fontSize != fs)
            {
                _centerBoldWhite = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    fontSize = fs,
                    normal   = { textColor = Color.white }
                };
            }

            GUI.Label(r, CenterCellLabel, _centerBoldWhite);
        }

        private static Rect GetCellRect(Rect gridRect, int col, int row, float cell, float pad)
        {
            float x = gridRect.x + pad + col * (cell + pad);
            float y = gridRect.y + pad + (2 - row) * (cell + pad);
            return new Rect(x, y, cell, cell);
        }

        private void DrawDirectionCell(
            Rect cellRect, HillDirection dir, float scale,
            SerializedProperty tileArrayProp)
        {
            bool hasEntry = _indexCache.TryGetValue(dir, out int arrayIndex);

            var bgColor = hasEntry
                ? new Color(0.2f, 0.45f, 0.2f, 0.5f)
                : new Color(0.45f, 0.2f, 0.2f, 0.4f);
            EditorGUI.DrawRect(cellRect, bgColor);

            string label    = k_Labels.TryGetValue(dir, out string lbl) ? lbl : dir.ToString();
            int    fontSize = Mathf.RoundToInt(9f * scale);
            var    labelRect = new Rect(cellRect.x, cellRect.y + 2f, cellRect.width, fontSize + 4f);
            GUI.Label(labelRect, label, CenteredWhiteMini);

            float fieldY    = labelRect.yMax + 2f;
            float fieldH    = cellRect.yMax - fieldY - 2f;
            var   fieldRect = new Rect(cellRect.x + 2f, fieldY, cellRect.width - 4f,
                                       Mathf.Max(fieldH, EditorGUIUtility.singleLineHeight));

            if (hasEntry && tileArrayProp != null)
            {
                var tileIdProp = tileArrayProp
                    .GetArrayElementAtIndex(arrayIndex)
                    .FindPropertyRelative("TileId");
                EditorGUI.PropertyField(fieldRect, tileIdProp, GUIContent.none);
            }
            else
            {
                if (GUI.Button(fieldRect, "+", EditorStyles.miniButton))
                    AddEntry(dir);
            }
        }

        // ── Data helpers ──

        private void RebuildIndexCache()
        {
            _indexCache.Clear();
            var prop = serializedObject.FindProperty(TileArrayPropertyName);
            if (prop == null) return;

            for (int i = 0; i < prop.arraySize; i++)
            {
                var entry   = prop.GetArrayElementAtIndex(i);
                var dirProp = entry.FindPropertyRelative("Direction");
                if (dirProp == null) continue;
                _indexCache[(HillDirection)dirProp.enumValueIndex] = i;
            }
        }

        private void AddEntry(HillDirection dir)
        {
            var prop = serializedObject.FindProperty(TileArrayPropertyName);
            prop.arraySize++;
            var newEntry = prop.GetArrayElementAtIndex(prop.arraySize - 1);
            newEntry.FindPropertyRelative("Direction").enumValueIndex = (int)dir;
            newEntry.FindPropertyRelative("TileId").stringValue        = "";
            serializedObject.ApplyModifiedProperties();
            _indexCache[dir] = prop.arraySize - 1;
            NotifyGraphEditorChanged();
        }

        private void RunValidation()
        {
            _missingEntries.Clear();
            _validated = true;

            var tileArrayProp = serializedObject.FindProperty(TileArrayPropertyName);
            var present = new HashSet<HillDirection>();
            for (int i = 0; i < tileArrayProp.arraySize; i++)
            {
                var entry  = tileArrayProp.GetArrayElementAtIndex(i);
                var dir    = (HillDirection)entry.FindPropertyRelative("Direction").enumValueIndex;
                var tileId = entry.FindPropertyRelative("TileId").stringValue;
                if (!string.IsNullOrEmpty(tileId))
                    present.Add(dir);
            }

            foreach (HillDirection dir in System.Enum.GetValues(typeof(HillDirection)))
                if (!present.Contains(dir))
                    _missingEntries.Add(dir);
        }

        protected void NotifyGraphEditorChanged()
        {
            EditorUtility.SetDirty(target);
            Repaint();
            NotifyOpenGraphEditors();
        }

        private static void NotifyOpenGraphEditors()
        {
            var graphWindowType = System.Type.GetType(
                "Kruty1918.Moyva.GraphSystem.Editor.GraphEditorWindow, Kruty1918.Moyva.GraphSystem.Editor");
            if (graphWindowType == null) return;

            var requestAutoRun = graphWindowType.GetMethod(
                "RequestAutoRun",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            foreach (var window in Resources.FindObjectsOfTypeAll(graphWindowType))
            {
                requestAutoRun?.Invoke(window, null);
                if (window is EditorWindow editorWindow)
                    editorWindow.Repaint();
            }
        }

        // ── Tile registry helpers ──

        protected static List<string> GetTileIdsFromRegistry()
        {
            var reg = LoadTileRegistry();
            if (reg?.Definitions == null) return new List<string>();
            return reg.Definitions
                .Where(d => !string.IsNullOrEmpty(d.Id))
                .Select(d => d.Id)
                .OrderBy(id => id)
                .ToList();
        }

        protected static Dictionary<string, Sprite> GetTileSpritesFromRegistry()
        {
            var sprites = new Dictionary<string, Sprite>();
            var reg = LoadTileRegistry();
            if (reg?.Definitions == null) return sprites;
            foreach (var def in reg.Definitions)
            {
                if (string.IsNullOrEmpty(def.Id) || def.VisualPrefab == null) continue;
                var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null && sr.sprite != null)
                    sprites[def.Id] = sr.sprite;
            }
            return sprites;
        }

        protected static TileRegistrySO LoadTileRegistry()
        {
            if (TryGetRegistryFromGraphWindowSettings(out var reg)) return reg;

            const string path = "Assets/Moyva/SO/Tile/TileRegistry.asset";
            var direct = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
            if (direct != null) return direct;

            var guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<TileRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private const string GraphEditorWindowSettingsPath =
            "Assets/Moyva/Scripts/Features/GraphSystem/Editor/GraphEditorWindowSettings.asset";

        private static bool TryGetRegistryFromGraphWindowSettings(out TileRegistrySO registry)
        {
            registry = null;

            var windowSettingsObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(GraphEditorWindowSettingsPath);
            if (windowSettingsObj == null) return false;

            var windowSo = new SerializedObject(windowSettingsObj);
            ScriptableObject previewSettingsObj = null;

            var previewSettingsProp = windowSo.FindProperty("previewSettings");
            if (previewSettingsProp?.objectReferenceValue is ScriptableObject direct)
            {
                previewSettingsObj = direct;
            }
            else
            {
                var guidProp = windowSo.FindProperty("previewSettingsGuid");
                string guid  = guidProp?.stringValue;
                if (!string.IsNullOrEmpty(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!string.IsNullOrEmpty(assetPath))
                        previewSettingsObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                }
            }

            if (previewSettingsObj == null) return false;

            var previewSo    = new SerializedObject(previewSettingsObj);
            var registryProp = previewSo.FindProperty("_tileRegistry");
            registry = registryProp?.objectReferenceValue as TileRegistrySO;
            return true;
        }

        protected static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // ── Multi-tile picker popup ──

        protected sealed class MultiTilePickerPopup : PopupWindowContent
        {
            private const float PopupWidth  = 560f;
            private const float PopupHeight = 520f;
            private const float SearchH     = 20f;
            private const float BtnH        = 24f;
            private const float RowH        = 36f;
            private const float IconSize    = 28f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "MultiTileSearch";

            private readonly List<string>                    _ids;
            private readonly Dictionary<string, Sprite>      _sprites;
            private readonly HashSet<string>                 _existing;
            private readonly System.Action<List<string>>     _onConfirm;
            private readonly HashSet<string>                 _checked = new();
            private Vector2 _scroll;
            private string  _search      = string.Empty;
            private bool    _focusSearch = true;

            public MultiTilePickerPopup(
                List<string> ids,
                Dictionary<string, Sprite> sprites,
                HashSet<string> current,
                System.Action<List<string>> onConfirm)
            {
                _ids      = ids;
                _sprites  = sprites;
                _existing = new HashSet<string>(current);
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

                List<string> filtered = GetFiltered();

                float btnY = searchRect.yMax + Padding;
                Rect selAllRect = new Rect(Padding, btnY, (rect.width - Padding * 3f) * 0.5f, BtnH);
                Rect clearRect  = new Rect(selAllRect.xMax + Padding, btnY, selAllRect.width, BtnH);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUI.Button(selAllRect, $"Вибрати всі ({filtered.Count})"))
                    foreach (var id in filtered) _checked.Add(id);

                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUI.Button(clearRect, "Зняти всі")) _checked.Clear();
                GUI.backgroundColor = Color.white;

                float listTop     = btnY + BtnH + Padding;
                float listH       = rect.height - listTop - BtnH - Padding * 3f;
                Rect viewRect     = new Rect(Padding, listTop, rect.width - Padding * 2f, listH);
                Rect contentRect  = new Rect(0f, 0f, viewRect.width - 14f,
                    Mathf.Max(listH, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(viewRect, _scroll, contentRect);
                for (int i = 0; i < filtered.Count; i++)
                {
                    string id         = filtered[i];
                    Rect   rowRect    = new Rect(0f, i * RowH, contentRect.width, RowH - 1f);
                    bool   isChecked  = _checked.Contains(id);
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
                        var tex = sprite.texture;
                        var sr  = sprite.textureRect;
                        var uv  = new Rect(sr.x / tex.width, sr.y / tex.height,
                                           sr.width / tex.width, sr.height / tex.height);
                        GUI.DrawTextureWithTexCoords(iconRect, tex, uv, true);
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
                var terms  = _search.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                return _ids.Where(id =>
                    System.Array.TrueForAll(terms,
                        t => id.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();
            }
        }
    }
}
