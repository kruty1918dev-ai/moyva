using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    public class WFCRulesEditorWindow : EditorWindow
    {
        private WFCDataSettings _wfcDataSettings;
        private int _selectedRuleIndex = -1;

        private Vector2 _sidebarScroll;
        private Vector2 _mainScroll;

        private List<string> _availableTileIDs = new List<string>();
        private Dictionary<string, Sprite> _tileSprites = new Dictionary<string, Sprite>();

        public static void OpenWindow(WFCDataSettings settings)
        {
            WFCRulesEditorWindow window = GetWindow<WFCRulesEditorWindow>("WFC Rules Editor");
            window._wfcDataSettings = settings;
            window.minSize = new Vector2(900, 750);
            window.Show();
        }

        private void OnEnable()
        {
            LoadAvailableTileIDs();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedoPerformed;

        private void OnUndoRedoPerformed() => Repaint();

        private void LoadAvailableTileIDs()
        {
            _availableTileIDs.Clear();
            _tileSprites.Clear();

            string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                TileRegistrySO registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);

                if (registry != null && registry.Definitions != null)
                {
                    foreach (var def in registry.Definitions)
                    {
                        if (string.IsNullOrEmpty(def.Id)) continue;
                        _availableTileIDs.Add(def.Id);
                        if (def.VisualPrefab != null)
                        {
                            var spriteRenderer = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>();
                            if (spriteRenderer != null && spriteRenderer.sprite != null)
                                _tileSprites[def.Id] = spriteRenderer.sprite;
                        }
                    }
                    _availableTileIDs.Sort();
                }
            }
        }

        private void OnGUI()
        {
            if (_wfcDataSettings == null)
            {
                EditorGUILayout.LabelField("No WFC Data Settings assigned.", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawMainArea();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(250), GUILayout.ExpandHeight(true));
            GUILayout.Label("Tiles & Rules", EditorStyles.boldLabel);

            if (GUILayout.Button("Оновити реєстр", GUILayout.Height(25))) LoadAvailableTileIDs();

            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
            for (int i = 0; i < _wfcDataSettings.TileRules.Count; i++)
            {
                var rule = _wfcDataSettings.TileRules[i];
                GUI.backgroundColor = _selectedRuleIndex == i ? Color.cyan : Color.white;

                EditorGUILayout.BeginHorizontal("box");
                Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
                if (_tileSprites.TryGetValue(rule.TileID, out Sprite s)) DrawSprite(iconRect, s);

                if (GUILayout.Button(rule.TileID, EditorStyles.label)) _selectedRuleIndex = i;

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    Undo.RecordObject(_wfcDataSettings, "Remove WFC Rule");
                    _wfcDataSettings.TileRules.RemoveAt(i);
                    _selectedRuleIndex = -1;
                    EditorUtility.SetDirty(_wfcDataSettings);
                    AssetDatabase.SaveAssets();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndScrollView();
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndVertical();
                    return;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+ Додати правило", GUILayout.Height(40)))
            {
                Undo.RecordObject(_wfcDataSettings, "Add WFC Rule");
                _wfcDataSettings.TileRules.Add(new WFCTileRule
                {
                    TileID = "new",
                    TileCentralID = "grass",
                    Constraints = new List<DirectionalConstraint>() // ГАРАНТОВАНА ІНІЦІАЛІЗАЦІЯ
                });
                _selectedRuleIndex = _wfcDataSettings.TileRules.Count - 1;
                EditorUtility.SetDirty(_wfcDataSettings);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        private void DrawMainArea()
        {
            if (_selectedRuleIndex < 0 || _selectedRuleIndex >= _wfcDataSettings.TileRules.Count) return;

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
            WFCTileRule currentRule = _wfcDataSettings.TileRules[_selectedRuleIndex];

            // Перевірка на null для існуючих правил
            if (currentRule.Constraints == null) currentRule.Constraints = new List<DirectionalConstraint>();

            EditorGUILayout.BeginVertical("box");

            // Вибір Result ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Результат ID:", GUILayout.Width(120));
            if (GUILayout.Button(currentRule.TileID, EditorStyles.popup))
            {
                ShowIDSelector((selectedId) =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Tile ID");
                    var r = _wfcDataSettings.TileRules[_selectedRuleIndex];
                    r.TileID = selectedId;
                    _wfcDataSettings.TileRules[_selectedRuleIndex] = r;
                    EditorUtility.SetDirty(_wfcDataSettings);
                });
            }
            EditorGUILayout.EndHorizontal();

            // Вибір Central ID
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Замінює (Central ID):", GUILayout.Width(120));
            if (GUILayout.Button(currentRule.TileCentralID, EditorStyles.popup))
            {
                ShowIDSelector((selectedId) =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Central ID");
                    var r = _wfcDataSettings.TileRules[_selectedRuleIndex];
                    r.TileCentralID = selectedId;
                    _wfcDataSettings.TileRules[_selectedRuleIndex] = r;
                    EditorUtility.SetDirty(_wfcDataSettings);
                });
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            int newPriority = EditorGUILayout.IntField("Пріоритет:", currentRule.Priority);
            float newThreshold = EditorGUILayout.Slider("Match Threshold:", currentRule.MatchThreshold, 0.5f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wfcDataSettings, "Edit Rule Properties");
                currentRule.Priority = newPriority;
                currentRule.MatchThreshold = newThreshold;
                _wfcDataSettings.TileRules[_selectedRuleIndex] = currentRule;
                EditorUtility.SetDirty(_wfcDataSettings);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
            DrawGrid3x3(ref currentRule);

            EditorGUILayout.EndScrollView();
        }

        private void ShowIDSelector(System.Action<string> onSelected)
        {
            GenericMenu menu = new GenericMenu();
            foreach (string id in _availableTileIDs)
            {
                string currentId = id;
                GUIContent content;

                // Перевіряємо, чи є спрайт для цього ID
                if (_tileSprites.TryGetValue(currentId, out Sprite sprite) && sprite != null)
                {
                    // Використовуємо текстуру спрайту для відображення іконки в меню
                    content = new GUIContent(currentId, sprite.texture);
                }
                else
                {
                    content = new GUIContent(currentId);
                }

                menu.AddItem(content, false, () => onSelected?.Invoke(currentId));
            }
            menu.ShowAsContext();
        }

        private void DrawGrid3x3(ref WFCTileRule rule)
        {
            float cellSize = 180f;
            GUILayoutOption[] cellOptions = { GUILayout.Width(cellSize), GUILayout.Height(cellSize) };

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DrawIDPreviewBlock("ЗАМІНЮЄ (Central)", rule.TileCentralID);
            GUILayout.Space(40);
            DrawIDPreviewBlock("РЕЗУЛЬТАТ (ID)", rule.TileID);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);

            for (int row = 0; row < 3; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int col = 0; col < 3; col++)
                {
                    if (row == 1 && col == 1)
                        DrawVisualTile(rule.TileID, "ЦЕНТР (Preview)", cellOptions);
                    else
                    {
                        Neighborhood8 dir = GetDirFromGrid(row, col);
                        DrawConstraintCell(ref rule, dir, cellOptions);
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawIDPreviewBlock(string label, string tileId)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            GUILayout.Label(label, EditorStyles.centeredGreyMiniLabel);
            Rect iconRect = GUILayoutUtility.GetRect(50, 50);
            GUI.Box(iconRect, "");
            if (_tileSprites.TryGetValue(tileId, out Sprite s))
                DrawSprite(new Rect(iconRect.x + 5, iconRect.y + 5, 40, 40), s);
            GUI.contentColor = Color.cyan;
            GUILayout.Label(tileId, EditorStyles.centeredGreyMiniLabel);
            GUI.contentColor = Color.white;
            EditorGUILayout.EndVertical();
        }

        private void DrawVisualTile(string id, string label, GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical("box", options);
            GUILayout.Label(label, EditorStyles.centeredGreyMiniLabel);
            Rect rect = GUILayoutUtility.GetRect(64, 64, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (_tileSprites.TryGetValue(id, out Sprite s)) DrawSprite(rect, s);
            GUILayout.Label(id, EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawConstraintCell(ref WFCTileRule rule, Neighborhood8 dir, GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical("box", options);
            GUILayout.Label(dir.ToString().ToUpper(), EditorStyles.centeredGreyMiniLabel);

            // ЗАХИСТ ВІД NULL
            if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();

            int idx = rule.Constraints.FindIndex(c => c.Direction == dir);
            List<string> neighbors = idx >= 0 ? rule.Constraints[idx].AllowedNeighbors : new List<string>();

            float iconSize = 40f;
            float cellSize = 180f; // Define cellSize here to match DrawGrid3x3

            // Кнопка "+" зверху
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+", GUILayout.Height(20)))
            {
                ShowSelector(rule, dir);
            }
            GUI.backgroundColor = Color.white;

            // Малюємо список іконок без вкладеного ScrollView для уникнення лагів
            EditorGUILayout.BeginHorizontal();
            float currentX = 0;
            for (int i = 0; i < neighbors.Count; i++)
            {
                if (currentX + iconSize > cellSize - 10)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    currentX = 0;
                }

                string nId = neighbors[i];
                Rect slotRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
                GUI.Box(slotRect, "");

                if (_tileSprites.TryGetValue(nId, out Sprite s))
                    DrawSprite(new Rect(slotRect.x + 2, slotRect.y + 2, iconSize - 4, iconSize - 4), s);

                Rect xRect = new Rect(slotRect.xMax - 14, slotRect.y, 14, 14);
                GUI.backgroundColor = Color.red;
                if (GUI.Button(xRect, "x", EditorStyles.miniButton))
                {
                    Undo.RecordObject(_wfcDataSettings, "Remove Neighbor");
                    neighbors.RemoveAt(i);
                    UpdateRuleConstraints(ref rule, dir, neighbors);
                    GUIUtility.ExitGUI(); // Перериваємо малювання щоб уникнути помилок ітерації
                }
                GUI.backgroundColor = Color.white;
                currentX += iconSize + 2;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void UpdateRuleConstraints(ref WFCTileRule rule, Neighborhood8 dir, List<string> newNeighbors)
        {
            if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();
            int idx = rule.Constraints.FindIndex(c => c.Direction == dir);

            if (newNeighbors.Count == 0)
            {
                if (idx >= 0) rule.Constraints.RemoveAt(idx);
            }
            else
            {
                if (idx >= 0)
                {
                    var c = rule.Constraints[idx];
                    c.AllowedNeighbors = newNeighbors;
                    rule.Constraints[idx] = c;
                }
                else
                {
                    rule.Constraints.Add(new DirectionalConstraint { Direction = dir, AllowedNeighbors = newNeighbors });
                }
            }
            EditorUtility.SetDirty(_wfcDataSettings);
        }

        private void ShowSelector(WFCTileRule rule, Neighborhood8 dir)
        {
            GenericMenu menu = new GenericMenu();
            foreach (string id in _availableTileIDs)
            {
                string currentId = id;
                GUIContent content;

                if (_tileSprites.TryGetValue(currentId, out Sprite sprite) && sprite != null)
                {
                    content = new GUIContent(currentId, sprite.texture);
                }
                else
                {
                    content = new GUIContent(currentId);
                }

                menu.AddItem(content, false, () =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Add Neighbor");
                    if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();

                    int idx = rule.Constraints.FindIndex(c => c.Direction == dir);
                    List<string> neighbors = idx >= 0
                        ? new List<string>(rule.Constraints[idx].AllowedNeighbors)
                        : new List<string>();

                    if (!neighbors.Contains(currentId))
                    {
                        neighbors.Add(currentId);
                        UpdateRuleConstraints(ref rule, dir, neighbors);
                    }
                });
            }
            menu.ShowAsContext();
        }

        private void DrawSprite(Rect rect, Sprite sprite)
        {
            if (sprite == null) return;
            Texture tex = sprite.texture;
            Rect r = sprite.textureRect;
            Rect uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);
            GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
        }

        private Neighborhood8 GetDirFromGrid(int row, int col)
        {
            if (row == 0 && col == 0) return Neighborhood8.TopLeft;
            if (row == 0 && col == 1) return Neighborhood8.Top;
            if (row == 0 && col == 2) return Neighborhood8.TopRight;
            if (row == 1 && col == 0) return Neighborhood8.Left;
            if (row == 1 && col == 2) return Neighborhood8.Right;
            if (row == 2 && col == 0) return Neighborhood8.BottomLeft;
            if (row == 2 && col == 1) return Neighborhood8.Bottom;
            return Neighborhood8.BottomRight;
        }
    }
}