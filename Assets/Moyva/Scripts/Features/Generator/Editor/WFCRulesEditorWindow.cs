using System.Collections.Generic;
using System.Linq;
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
        private readonly List<int> _sortedRuleIndices = new List<int>();
        private bool _sortByPriority = true;
        private bool _priorityDescending = true;
        private bool _waterLikeIdsBufferInitialized;
        private string _waterLikeIdsBuffer = string.Empty;
        private bool _virtualTileIdsBufferInitialized;
        private string _virtualTileIdsBuffer = string.Empty;

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
            _waterLikeIdsBufferInitialized = false;
            _virtualTileIdsBufferInitialized = false;
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
                }
            }

            if (_wfcDataSettings?.VirtualTileIds != null)
            {
                foreach (var virtualId in _wfcDataSettings.VirtualTileIds)
                {
                    if (string.IsNullOrWhiteSpace(virtualId))
                        continue;

                    if (!_availableTileIDs.Contains(virtualId))
                        _availableTileIDs.Add(virtualId);
                }
            }

            _availableTileIDs = _availableTileIDs
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct()
                .OrderBy(id => id)
                .ToList();
        }

        private void OnGUI()
        {
            if (_wfcDataSettings == null)
            {
                DrawNoDataAssigned();
                return;
            }

            RebuildRuleOrder();

            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawMainArea();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNoDataAssigned()
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("WFC Data Settings не призначено", EditorStyles.boldLabel);
            GUILayout.Space(10);

            _wfcDataSettings = (WFCDataSettings)EditorGUILayout.ObjectField(
                "Обрати вручну:", _wfcDataSettings, typeof(WFCDataSettings), false);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Або оберіть з проекту:", EditorStyles.miniLabel);
            GUILayout.Space(4);

            string[] guids = AssetDatabase.FindAssets("t:WFCDataSettings");
            if (guids.Length == 0)
            {
                EditorGUILayout.HelpBox("У проекті не знайдено жодного WFCDataSettings.\n" +
                    "Створіть через: Assets → Create → Moyva → Generator → WFCDataSettings", MessageType.Info);
            }
            else
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<WFCDataSettings>(path);
                    if (asset == null) continue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
                    if (GUILayout.Button("Обрати", GUILayout.Width(70)))
                    {
                        _wfcDataSettings = asset;
                        LoadAvailableTileIDs();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical("box", GUILayout.Width(250), GUILayout.ExpandHeight(true));
            GUILayout.Label("Tiles & Rules", EditorStyles.boldLabel);

            if (GUILayout.Button("Оновити реєстр", GUILayout.Height(25))) LoadAvailableTileIDs();

            DrawGlobalSettings();

            EditorGUILayout.BeginHorizontal();
            _sortByPriority = EditorGUILayout.ToggleLeft("Сортувати за пріоритетом", _sortByPriority);
            if (_sortByPriority && GUILayout.Button(_priorityDescending ? "DESC" : "ASC", GUILayout.Width(52)))
            {
                _priorityDescending = !_priorityDescending;
                RebuildRuleOrder();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Пересортувати Asset за пріоритетом", GUILayout.Height(22)))
            {
                SortRulesInAssetByPriority();
                RebuildRuleOrder();
            }

            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
            for (int row = 0; row < _sortedRuleIndices.Count; row++)
            {
                int sourceIndex = _sortedRuleIndices[row];
                var rule = _wfcDataSettings.TileRules[sourceIndex];
                GUI.backgroundColor = _selectedRuleIndex == sourceIndex ? Color.cyan : Color.white;

                EditorGUILayout.BeginHorizontal("box");
                Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
                if (_tileSprites.TryGetValue(rule.TileID, out Sprite s)) DrawSprite(iconRect, s);

                if (GUILayout.Button($"[{rule.Priority}] {rule.TileID}", EditorStyles.label))
                    _selectedRuleIndex = sourceIndex;

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    Undo.RecordObject(_wfcDataSettings, "Remove WFC Rule");
                    _wfcDataSettings.TileRules.RemoveAt(sourceIndex);
                    if (_selectedRuleIndex == sourceIndex)
                        _selectedRuleIndex = -1;
                    else if (_selectedRuleIndex > sourceIndex)
                        _selectedRuleIndex--;
                    EditorUtility.SetDirty(_wfcDataSettings);
                    AssetDatabase.SaveAssets();
                    RebuildRuleOrder();
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

        private void DrawGlobalSettings()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("WFC Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");

            if (!_waterLikeIdsBufferInitialized)
            {
                _waterLikeIdsBuffer = JoinIds(_wfcDataSettings.WaterLikeTileIds);
                _waterLikeIdsBufferInitialized = true;
            }

            if (!_virtualTileIdsBufferInitialized)
            {
                _virtualTileIdsBuffer = JoinIdsAllowEmpty(_wfcDataSettings.VirtualTileIds);
                _virtualTileIdsBufferInitialized = true;
            }

            EditorGUI.BeginChangeCheck();
            int passCount = Mathf.Max(1, EditorGUILayout.IntField(
                new GUIContent("Кількість проходів",
                    "Скільки ітерацій полірування WFC виконати. Більше значення дає більше шансів стабілізувати складні переходи."),
                _wfcDataSettings.PassCount));

            bool forceBand = EditorGUILayout.ToggleLeft(
                new GUIContent("Примусова смуга біля води",
                    "Перед WFC замінює сушу біля води на вибраний тайл. Це стабілізує берегові переходи."),
                _wfcDataSettings.ForceTileNearWaterBand);

            string nearWaterTileId = EditorGUILayout.TextField(
                new GUIContent("Тайл біля води",
                    "ID тайла, який буде застосований у прибережній смузі (наприклад grass)."),
                _wfcDataSettings.NearWaterTileId ?? string.Empty);

            int nearWaterRadius = EditorGUILayout.IntSlider(
                new GUIContent("Радіус смуги",
                    "Ширина прибережної смуги в клітинках. 1 — лише безпосередні сусіди води."),
                _wfcDataSettings.NearWaterRadius, 1, 6);

            bool includeDiagonals = EditorGUILayout.ToggleLeft(
                new GUIContent("Враховувати діагоналі",
                    "Якщо увімкнено, прибережна смуга рахується з діагональними сусідами (8-напрямний пошук)."),
                _wfcDataSettings.IncludeDiagonalsForNearWater);

            EditorGUILayout.LabelField(
                new GUIContent("Water-like ID (через кому або новий рядок)",
                    "Список ID, які вважаються водою для побудови прибережної смуги. Наприклад: water, sea, coast, river."),
                EditorStyles.miniLabel);
            string waterLikeInput = EditorGUILayout.TextArea(_waterLikeIdsBuffer, GUILayout.MinHeight(48));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField(
                new GUIContent("Virtual / Flag IDs",
                    "Віртуальні ID для WFC, які не мають існувати в TileRegistry. Наприклад: flag:road, flag:river, marker:bridge."),
                EditorStyles.miniLabel);
            string virtualTileIdsInput = EditorGUILayout.TextArea(_virtualTileIdsBuffer, GUILayout.MinHeight(38));

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_wfcDataSettings, "Edit WFC Global Settings");
                _wfcDataSettings.PassCount = passCount;
                _wfcDataSettings.ForceTileNearWaterBand = forceBand;
                _wfcDataSettings.NearWaterTileId = nearWaterTileId;
                _wfcDataSettings.NearWaterRadius = nearWaterRadius;
                _wfcDataSettings.IncludeDiagonalsForNearWater = includeDiagonals;
                _waterLikeIdsBuffer = waterLikeInput;
                _virtualTileIdsBuffer = virtualTileIdsInput;
                _wfcDataSettings.WaterLikeTileIds = ParseIds(_waterLikeIdsBuffer);
                _wfcDataSettings.VirtualTileIds = ParseIdsAllowEmpty(_virtualTileIdsBuffer);
                EditorUtility.SetDirty(_wfcDataSettings);
                LoadAvailableTileIDs();
            }

            if (GUILayout.Button(new GUIContent(
                "Вибрати ID прибережного тайла",
                "Відкрити список Tile ID з реєстру та вибрати тайл для смуги біля води."), GUILayout.Height(20)))
            {
                ShowIDSelector((selectedId) =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Select Near Water Tile ID");
                    _wfcDataSettings.NearWaterTileId = selectedId;
                    EditorUtility.SetDirty(_wfcDataSettings);
                });
            }

            EditorGUILayout.EndVertical();
        }

        private static string JoinIds(string[] ids)
        {
            if (ids == null || ids.Length == 0) return string.Empty;
            return string.Join(", ", ids.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()));
        }

        private static string JoinIdsAllowEmpty(string[] ids)
        {
            if (ids == null || ids.Length == 0) return string.Empty;
            return string.Join(", ", ids.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()));
        }

        private static string[] ParseIds(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return new[] { "water" };

            var result = source
                .Split(new[] { ',', '\n', '\r', '\t', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(System.StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return result.Length == 0 ? new[] { "water" } : result;
        }

        private static string[] ParseIdsAllowEmpty(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return System.Array.Empty<string>();

            return source
                .Split(new[] { ',', '\n', '\r', ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();
        }

        private void RebuildRuleOrder()
        {
            _sortedRuleIndices.Clear();
            if (_wfcDataSettings == null || _wfcDataSettings.TileRules == null) return;

            for (int i = 0; i < _wfcDataSettings.TileRules.Count; i++)
                _sortedRuleIndices.Add(i);

            if (!_sortByPriority) return;

            _sortedRuleIndices.Sort((a, b) =>
            {
                var ra = _wfcDataSettings.TileRules[a];
                var rb = _wfcDataSettings.TileRules[b];

                int cmp = _priorityDescending
                    ? rb.Priority.CompareTo(ra.Priority)
                    : ra.Priority.CompareTo(rb.Priority);

                if (cmp != 0) return cmp;
                return string.Compare(ra.TileID, rb.TileID, System.StringComparison.OrdinalIgnoreCase);
            });
        }

        private void SortRulesInAssetByPriority()
        {
            if (_wfcDataSettings == null || _wfcDataSettings.TileRules == null) return;

            Undo.RecordObject(_wfcDataSettings, "Sort WFC Rules By Priority");
            var sorted = _priorityDescending
                ? _wfcDataSettings.TileRules.OrderByDescending(r => r.Priority).ThenBy(r => r.TileID).ToList()
                : _wfcDataSettings.TileRules.OrderBy(r => r.Priority).ThenBy(r => r.TileID).ToList();
            _wfcDataSettings.TileRules = sorted;
            _selectedRuleIndex = -1;
            EditorUtility.SetDirty(_wfcDataSettings);
            AssetDatabase.SaveAssets();
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
            DrawGrid3x3(_selectedRuleIndex);

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

        private void DrawGrid3x3(int ruleIndex)
        {
            var rule = _wfcDataSettings.TileRules[ruleIndex];
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
                        DrawConstraintCell(ruleIndex, dir, cellOptions);
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

        private void DrawConstraintCell(int ruleIndex, Neighborhood8 dir, GUILayoutOption[] options)
        {
            var rule = _wfcDataSettings.TileRules[ruleIndex];
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
                ShowSelector(ruleIndex, dir);
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
                    UpdateRuleConstraints(ruleIndex, dir, neighbors);
                    GUIUtility.ExitGUI(); // Перериваємо малювання щоб уникнути помилок ітерації
                }
                GUI.backgroundColor = Color.white;
                currentX += iconSize + 2;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void UpdateRuleConstraints(int ruleIndex, Neighborhood8 dir, List<string> newNeighbors)
        {
            var rule = _wfcDataSettings.TileRules[ruleIndex];
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

            _wfcDataSettings.TileRules[ruleIndex] = rule;
            EditorUtility.SetDirty(_wfcDataSettings);
        }

        private void ShowSelector(int ruleIndex, Neighborhood8 dir)
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
                    var rule = _wfcDataSettings.TileRules[ruleIndex];
                    if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();

                    int idx = rule.Constraints.FindIndex(c => c.Direction == dir);
                    List<string> neighbors = idx >= 0
                        ? new List<string>(rule.Constraints[idx].AllowedNeighbors)
                        : new List<string>();

                    if (!neighbors.Contains(currentId))
                    {
                        neighbors.Add(currentId);
                        UpdateRuleConstraints(ruleIndex, dir, neighbors);
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