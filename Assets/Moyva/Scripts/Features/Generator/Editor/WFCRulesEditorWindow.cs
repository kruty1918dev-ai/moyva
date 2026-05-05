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
        private readonly HashSet<int> _selectedRuleIndices = new HashSet<int>();

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

            TileRegistrySO registry = _wfcDataSettings?.TileRegistry;
            if (registry == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:TileRegistrySO");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    registry = AssetDatabase.LoadAssetAtPath<TileRegistrySO>(path);
                }
            }

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
            string selLabel = _selectedRuleIndices.Count > 0
                ? $"Tiles & Rules  ({_selectedRuleIndices.Count} вибрано)"
                : "Tiles & Rules";
            GUILayout.Label(selLabel, EditorStyles.boldLabel);

            // Показуємо який реєстр активний
            if (_wfcDataSettings.TileRegistry != null)
                EditorGUILayout.HelpBox($"Реєстр: {_wfcDataSettings.TileRegistry.name}", MessageType.None);
            else
                EditorGUILayout.HelpBox("Реєстр: авто (перший у проєкті)", MessageType.None);

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
                bool isSelected = _selectedRuleIndices.Contains(sourceIndex);
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

                EditorGUILayout.BeginHorizontal("box");
                Rect iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
                if (_tileSprites.TryGetValue(rule.TileID, out Sprite s)) DrawSprite(iconRect, s);

                bool multiSelectHeld = Event.current.control || Event.current.command;
                if (GUILayout.Button($"[{rule.Priority}] {rule.TileID}", EditorStyles.label))
                {
                    if (multiSelectHeld)
                    {
                        if (isSelected) _selectedRuleIndices.Remove(sourceIndex);
                        else _selectedRuleIndices.Add(sourceIndex);
                    }
                    else
                    {
                        _selectedRuleIndices.Clear();
                        _selectedRuleIndices.Add(sourceIndex);
                    }
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    Undo.RecordObject(_wfcDataSettings, "Remove WFC Rule");
                    _wfcDataSettings.TileRules.RemoveAt(sourceIndex);
                    var newSel = new HashSet<int>();
                    foreach (int si in _selectedRuleIndices)
                    {
                        if (si == sourceIndex) continue;
                        newSel.Add(si > sourceIndex ? si - 1 : si);
                    }
                    _selectedRuleIndices.Clear();
                    foreach (int si in newSel) _selectedRuleIndices.Add(si);
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

            if (_selectedRuleIndices.Count > 1)
                EditorGUILayout.HelpBox($"Вибрано: {_selectedRuleIndices.Count}  |  Ctrl+клік = мультивибір", MessageType.None);
            else
                EditorGUILayout.HelpBox("Ctrl+клік = мультивибір", MessageType.None);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+ Додати правило", GUILayout.Height(40)))
            {
                Undo.RecordObject(_wfcDataSettings, "Add WFC Rule");
                _wfcDataSettings.TileRules.Add(new WFCTileRule
                {
                    TileID = "new",
                    TileCentralID = "grass",
                    Constraints = new List<DirectionalConstraint>()
                });
                int newIdx = _wfcDataSettings.TileRules.Count - 1;
                _selectedRuleIndices.Clear();
                _selectedRuleIndices.Add(newIdx);
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
                Rect r = GUILayoutUtility.GetLastRect();
                ShowIDSelector(r, _wfcDataSettings.NearWaterTileId, (selectedId) =>
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
            _selectedRuleIndices.Clear();
            EditorUtility.SetDirty(_wfcDataSettings);
            AssetDatabase.SaveAssets();
        }

        private void DrawMainArea()
        {
            var validIndices = new List<int>();
            foreach (int i in _selectedRuleIndices)
                if (i >= 0 && i < _wfcDataSettings.TileRules.Count)
                    validIndices.Add(i);

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);

            if (validIndices.Count == 0)
                EditorGUILayout.HelpBox("Правило не вибрано. Натисніть на правило в списку.\nCtrl+клік = мультивибір", MessageType.Info);
            else if (validIndices.Count == 1)
                DrawMainAreaSingle(validIndices[0]);
            else
                DrawMainAreaMulti(validIndices);

            EditorGUILayout.EndScrollView();
        }

        private void DrawMainAreaSingle(int ruleIndex)
        {
            WFCTileRule currentRule = _wfcDataSettings.TileRules[ruleIndex];
            if (currentRule.Constraints == null) currentRule.Constraints = new List<DirectionalConstraint>();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Результат ID:", GUILayout.Width(120));
            if (GUILayout.Button(currentRule.TileID, EditorStyles.popup))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowIDSelector(r, currentRule.TileID, selectedId =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Tile ID");
                    var rule = _wfcDataSettings.TileRules[ruleIndex];
                    rule.TileID = selectedId;
                    _wfcDataSettings.TileRules[ruleIndex] = rule;
                    EditorUtility.SetDirty(_wfcDataSettings);
                });
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Замінює (Central ID):", GUILayout.Width(120));
            if (GUILayout.Button(currentRule.TileCentralID, EditorStyles.popup))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowIDSelector(r, currentRule.TileCentralID, selectedId =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Central ID");
                    var rule = _wfcDataSettings.TileRules[ruleIndex];
                    rule.TileCentralID = selectedId;
                    _wfcDataSettings.TileRules[ruleIndex] = rule;
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
                _wfcDataSettings.TileRules[ruleIndex] = currentRule;
                EditorUtility.SetDirty(_wfcDataSettings);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(20);
            DrawGrid3x3(ruleIndex);
        }

        private void DrawMainAreaMulti(List<int> ruleIndices)
        {
            EditorGUILayout.LabelField($"Редагування {ruleIndices.Count} правил одночасно", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Показані лише спільні значення. Зміни застосовуються до всіх вибраних правил.", MessageType.Info);

            EditorGUILayout.BeginVertical("box");

            // TileID
            string firstTileId = _wfcDataSettings.TileRules[ruleIndices[0]].TileID;
            bool tileIdSame    = ruleIndices.All(i => _wfcDataSettings.TileRules[i].TileID == firstTileId);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Результат ID:", GUILayout.Width(120));
            if (tileIdSame)
            {
                if (GUILayout.Button(firstTileId, EditorStyles.popup))
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    ShowIDSelector(r, firstTileId, selectedId =>
                    {
                        Undo.RecordObject(_wfcDataSettings, "Change Tile IDs (multi)");
                        foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.TileID = selectedId; _wfcDataSettings.TileRules[i] = ru; }
                        EditorUtility.SetDirty(_wfcDataSettings);
                    });
                }
            }
            else
            {
                GUI.color = Color.gray; GUILayout.Label("— (різні) —", GUILayout.ExpandWidth(true)); GUI.color = Color.white;
                if (GUILayout.Button("Призначити всім", GUILayout.Width(120)))
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    ShowIDSelector(r, firstTileId, selectedId =>
                    {
                        Undo.RecordObject(_wfcDataSettings, "Set Tile IDs (multi)");
                        foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.TileID = selectedId; _wfcDataSettings.TileRules[i] = ru; }
                        EditorUtility.SetDirty(_wfcDataSettings);
                    });
                }
            }
            EditorGUILayout.EndHorizontal();

            // TileCentralID
            string firstCentral = _wfcDataSettings.TileRules[ruleIndices[0]].TileCentralID;
            bool centralSame    = ruleIndices.All(i => _wfcDataSettings.TileRules[i].TileCentralID == firstCentral);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Замінює (Central ID):", GUILayout.Width(120));
            if (centralSame)
            {
                if (GUILayout.Button(firstCentral, EditorStyles.popup))
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    ShowIDSelector(r, firstCentral, selectedId =>
                    {
                        Undo.RecordObject(_wfcDataSettings, "Change Central IDs (multi)");
                        foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.TileCentralID = selectedId; _wfcDataSettings.TileRules[i] = ru; }
                        EditorUtility.SetDirty(_wfcDataSettings);
                    });
                }
            }
            else
            {
                GUI.color = Color.gray; GUILayout.Label("— (різні) —", GUILayout.ExpandWidth(true)); GUI.color = Color.white;
                if (GUILayout.Button("Призначити всім", GUILayout.Width(120)))
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    ShowIDSelector(r, firstCentral, selectedId =>
                    {
                        Undo.RecordObject(_wfcDataSettings, "Set Central IDs (multi)");
                        foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.TileCentralID = selectedId; _wfcDataSettings.TileRules[i] = ru; }
                        EditorUtility.SetDirty(_wfcDataSettings);
                    });
                }
            }
            EditorGUILayout.EndHorizontal();

            // Priority
            int firstPriority = _wfcDataSettings.TileRules[ruleIndices[0]].Priority;
            bool prioritySame = ruleIndices.All(i => _wfcDataSettings.TileRules[i].Priority == firstPriority);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Пріоритет:", GUILayout.Width(120));
            if (prioritySame)
            {
                EditorGUI.BeginChangeCheck();
                int newP = EditorGUILayout.IntField(firstPriority);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Priorities (multi)");
                    foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.Priority = newP; _wfcDataSettings.TileRules[i] = ru; }
                    EditorUtility.SetDirty(_wfcDataSettings);
                }
            }
            else { GUI.color = Color.gray; GUILayout.Label("— (різні) —"); GUI.color = Color.white; }
            EditorGUILayout.EndHorizontal();

            // MatchThreshold
            float firstThresh = _wfcDataSettings.TileRules[ruleIndices[0]].MatchThreshold;
            bool threshSame   = ruleIndices.All(i => _wfcDataSettings.TileRules[i].MatchThreshold == firstThresh);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Match Threshold:", GUILayout.Width(120));
            if (threshSame)
            {
                EditorGUI.BeginChangeCheck();
                float newT = EditorGUILayout.Slider(firstThresh, 0.5f, 1f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_wfcDataSettings, "Change Thresholds (multi)");
                    foreach (int i in ruleIndices) { var ru = _wfcDataSettings.TileRules[i]; ru.MatchThreshold = newT; _wfcDataSettings.TileRules[i] = ru; }
                    EditorUtility.SetDirty(_wfcDataSettings);
                }
            }
            else { GUI.color = Color.gray; GUILayout.Label("— (різні) —"); GUI.color = Color.white; }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.Space(20);
            DrawGrid3x3Multi(ruleIndices);
        }

        private void DrawGrid3x3Multi(List<int> ruleIndices)
        {
            float cellSize = 180f;
            GUILayoutOption[] cellOptions = { GUILayout.Width(cellSize), GUILayout.Height(cellSize) };

            // Center preview
            string firstId = _wfcDataSettings.TileRules[ruleIndices[0]].TileID;
            bool allSame   = ruleIndices.All(i => _wfcDataSettings.TileRules[i].TileID == firstId);

            EditorGUILayout.BeginVertical();
            for (int row = 0; row < 3; row++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                for (int col = 0; col < 3; col++)
                {
                    if (row == 1 && col == 1)
                        DrawVisualTile(allSame ? firstId : string.Empty, allSame ? $"ЦЕНТР ({firstId})" : $"ЦЕНТР (різні)", cellOptions);
                    else
                    {
                        Neighborhood8 dir = GetDirFromGrid(row, col);
                        DrawConstraintCellMulti(ruleIndices, dir, cellOptions);
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawConstraintCellMulti(List<int> ruleIndices, Neighborhood8 dir, GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical("box", options);
            GUILayout.Label($"{dir.ToString().ToUpper()} (×{ruleIndices.Count})", EditorStyles.centeredGreyMiniLabel);

            // Intersection of neighbors across all selected rules
            HashSet<string> intersection = null;
            foreach (int ri in ruleIndices)
            {
                var rule = _wfcDataSettings.TileRules[ri];
                if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();
                int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                var nb = cIdx >= 0 ? rule.Constraints[cIdx].AllowedNeighbors : new List<string>();
                if (intersection == null) intersection = new HashSet<string>(nb);
                else intersection.IntersectWith(nb);
            }
            var commonNeighbors = intersection != null ? intersection.ToList() : new List<string>();

            float iconSize = 40f;
            float cellWidth = 180f;

            bool hasAny = HasAnyNeighborAcrossRules(ruleIndices, dir);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+", GUILayout.Height(20)))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowSelectorMulti(r, ruleIndices, dir);
            }
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("++ за словом", GUILayout.Height(20)))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowBulkSelectorMulti(r, ruleIndices, dir);
            }
            EditorGUI.BeginDisabledGroup(!hasAny);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕ всі", GUILayout.Height(20), GUILayout.Width(46)))
            {
                Undo.RecordObject(_wfcDataSettings, "Clear Neighbors (multi)");
                foreach (int ri in ruleIndices) UpdateRuleConstraints(ri, dir, new List<string>());
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            float currentX = 0;
            for (int i = 0; i < commonNeighbors.Count; i++)
            {
                if (currentX + iconSize > cellWidth - 10)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    currentX = 0;
                }
                string nId = commonNeighbors[i];
                Rect slotRect = GUILayoutUtility.GetRect(iconSize, iconSize, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
                GUI.Box(slotRect, "");
                if (_tileSprites.TryGetValue(nId, out Sprite sp))
                    DrawSprite(new Rect(slotRect.x + 2, slotRect.y + 2, iconSize - 4, iconSize - 4), sp);

                Rect xRect = new Rect(slotRect.xMax - 14, slotRect.y, 14, 14);
                GUI.backgroundColor = Color.red;
                if (GUI.Button(xRect, "x", EditorStyles.miniButton))
                {
                    Undo.RecordObject(_wfcDataSettings, "Remove Neighbor (multi)");
                    foreach (int ri in ruleIndices)
                    {
                        var rule = _wfcDataSettings.TileRules[ri];
                        if (rule.Constraints == null) continue;
                        int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                        if (cIdx < 0) continue;
                        var nb = new List<string>(rule.Constraints[cIdx].AllowedNeighbors);
                        nb.Remove(nId);
                        UpdateRuleConstraints(ri, dir, nb);
                    }
                    GUIUtility.ExitGUI();
                }
                GUI.backgroundColor = Color.white;
                currentX += iconSize + 2;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private bool HasAnyNeighborAcrossRules(List<int> ruleIndices, Neighborhood8 dir)
        {
            foreach (int ri in ruleIndices)
            {
                var rule = _wfcDataSettings.TileRules[ri];
                if (rule.Constraints == null) continue;
                int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                if (cIdx >= 0 && rule.Constraints[cIdx].AllowedNeighbors?.Count > 0) return true;
            }
            return false;
        }

        private void ShowSelectorMulti(Rect anchor, List<int> ruleIndices, Neighborhood8 dir)
        {
            PopupWindow.Show(anchor, new WFCTilePickerPopup(
                string.Empty, _availableTileIDs, _tileSprites,
                selectedId =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Add Neighbor (multi)");
                    foreach (int ri in ruleIndices)
                    {
                        var rule = _wfcDataSettings.TileRules[ri];
                        if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();
                        int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                        var nb = cIdx >= 0 ? new List<string>(rule.Constraints[cIdx].AllowedNeighbors) : new List<string>();
                        if (!nb.Contains(selectedId)) { nb.Add(selectedId); UpdateRuleConstraints(ri, dir, nb); }
                    }
                }));
        }

        private void ShowBulkSelectorMulti(Rect anchor, List<int> ruleIndices, Neighborhood8 dir)
        {
            var union = new HashSet<string>();
            foreach (int ri in ruleIndices)
            {
                var rule = _wfcDataSettings.TileRules[ri];
                if (rule.Constraints == null) continue;
                int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                if (cIdx >= 0 && rule.Constraints[cIdx].AllowedNeighbors != null)
                    foreach (var id in rule.Constraints[cIdx].AllowedNeighbors) union.Add(id);
            }
            PopupWindow.Show(anchor, new WFCBulkAddPopup(
                _availableTileIDs, _tileSprites, union,
                addedIds =>
                {
                    if (addedIds.Count == 0) return;
                    Undo.RecordObject(_wfcDataSettings, "Bulk Add Neighbors (multi)");
                    foreach (int ri in ruleIndices)
                    {
                        var rule = _wfcDataSettings.TileRules[ri];
                        if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();
                        int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                        var nb = cIdx >= 0 ? new List<string>(rule.Constraints[cIdx].AllowedNeighbors) : new List<string>();
                        foreach (var id in addedIds) if (!nb.Contains(id)) nb.Add(id);
                        UpdateRuleConstraints(ri, dir, nb);
                    }
                }));
        }

        private void ShowBulkSelector(Rect anchor, int ruleIndex, Neighborhood8 dir)
        {
            var rule = _wfcDataSettings.TileRules[ruleIndex];
            if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();
            int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
            var existing = cIdx >= 0 ? new HashSet<string>(rule.Constraints[cIdx].AllowedNeighbors) : new HashSet<string>();

            PopupWindow.Show(anchor, new WFCBulkAddPopup(
                _availableTileIDs, _tileSprites, existing,
                addedIds =>
                {
                    if (addedIds.Count == 0) return;
                    Undo.RecordObject(_wfcDataSettings, "Bulk Add Neighbors");
                    var r2 = _wfcDataSettings.TileRules[ruleIndex];
                    if (r2.Constraints == null) r2.Constraints = new List<DirectionalConstraint>();
                    int i2 = r2.Constraints.FindIndex(c => c.Direction == dir);
                    List<string> neighbors = i2 >= 0
                        ? new List<string>(r2.Constraints[i2].AllowedNeighbors)
                        : new List<string>();
                    foreach (var id in addedIds)
                        if (!neighbors.Contains(id))
                            neighbors.Add(id);
                    UpdateRuleConstraints(ruleIndex, dir, neighbors);
                }));
        }

        private void ShowIDSelector(Rect anchor, string current, System.Action<string> onSelected)
        {
            PopupWindow.Show(anchor, new WFCTilePickerPopup(
                current, _availableTileIDs, _tileSprites, onSelected));
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

            // Кнопки: + (одиночний) і ++ (масовий)
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+", GUILayout.Height(20)))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowSelector(r, ruleIndex, dir);
            }
            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("++ за словом", GUILayout.Height(20)))
            {
                Rect r = GUILayoutUtility.GetLastRect();
                ShowBulkSelector(r, ruleIndex, dir);
            }
            EditorGUI.BeginDisabledGroup(neighbors.Count == 0);
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("✕ всі", GUILayout.Height(20), GUILayout.Width(46)))
            {
                Undo.RecordObject(_wfcDataSettings, "Clear Neighbors");
                UpdateRuleConstraints(ruleIndex, dir, new List<string>());
                GUIUtility.ExitGUI();
            }
            EditorGUI.EndDisabledGroup();
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

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

        private void ShowSelector(Rect anchor, int ruleIndex, Neighborhood8 dir)
        {
            PopupWindow.Show(anchor, new WFCTilePickerPopup(
                string.Empty, _availableTileIDs, _tileSprites,
                selectedId =>
                {
                    Undo.RecordObject(_wfcDataSettings, "Add Neighbor");
                    var rule = _wfcDataSettings.TileRules[ruleIndex];
                    if (rule.Constraints == null) rule.Constraints = new List<DirectionalConstraint>();

                    int cIdx = rule.Constraints.FindIndex(c => c.Direction == dir);
                    List<string> neighbors = cIdx >= 0
                        ? new List<string>(rule.Constraints[cIdx].AllowedNeighbors)
                        : new List<string>();

                    if (!neighbors.Contains(selectedId))
                    {
                        neighbors.Add(selectedId);
                        UpdateRuleConstraints(ruleIndex, dir, neighbors);
                    }
                }));
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

        // ── Custom tile picker popup ──────────────────────────────────────────
        private sealed class WFCTilePickerPopup : PopupWindowContent        {
            private const float PopupWidth  = 480f;
            private const float PopupHeight = 460f;
            private const float SearchH     = 22f;
            private const float RowH        = 40f;
            private const float IconSize    = 32f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "WFCTilePickerSearch";

            private readonly string              _current;
            private readonly List<string>        _ids;
            private readonly Dictionary<string, Sprite> _sprites;
            private readonly System.Action<string> _onSelected;

            private Vector2 _scroll;
            private string  _search = string.Empty;
            private bool    _focusSearch = true;

            public WFCTilePickerPopup(
                string current,
                List<string> ids,
                Dictionary<string, Sprite> sprites,
                System.Action<string> onSelected)
            {
                _current    = current;
                _ids        = ids;
                _sprites    = sprites;
                _onSelected = onSelected;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);

            public override void OnOpen() => _focusSearch = true;

            public override void OnGUI(Rect rect)
            {
                // Search field
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchH);
                GUI.SetNextControlName(SearchCtrl);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) _scroll = Vector2.zero;
                if (_focusSearch)
                {
                    _focusSearch = false;
                    EditorGUI.FocusTextInControl(SearchCtrl);
                }

                // Filter
                var filtered = new List<string>();
                if (string.IsNullOrWhiteSpace(_search))
                {
                    filtered.AddRange(_ids);
                }
                else
                {
                    var terms = _search.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in _ids)
                        if (System.Array.TrueForAll(terms, t => id.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0))
                            filtered.Add(id);
                }

                Rect listRect = new Rect(Padding, searchRect.yMax + Padding, rect.width - Padding * 2f,
                    rect.height - searchRect.yMax - Padding * 2f);
                Rect content  = new Rect(0f, 0f, listRect.width - 14f,
                    Mathf.Max(listRect.height, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(listRect, _scroll, content);
                for (int i = 0; i < filtered.Count; i++)
                {
                    string id = filtered[i];
                    Rect row = new Rect(0f, i * RowH, content.width, RowH - 1f);

                    bool selected = string.Equals(id, _current, System.StringComparison.Ordinal);
                    if (selected)
                        EditorGUI.DrawRect(row, new Color(0.24f, 0.48f, 0.24f, 0.7f));
                    else if (row.Contains(Event.current.mousePosition))
                        EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.08f));

                    // Sprite icon
                    Rect iconRect = new Rect(row.x + 4f, row.y + (RowH - IconSize) * 0.5f, IconSize, IconSize);
                    if (_sprites.TryGetValue(id, out Sprite spr) && spr != null)
                    {
                        var tex = spr.texture;
                        var sr  = spr.textureRect;
                        var uv  = new Rect(sr.x / tex.width, sr.y / tex.height, sr.width / tex.width, sr.height / tex.height);
                        GUI.DrawTextureWithTexCoords(iconRect, tex, uv, true);
                    }
                    else
                    {
                        GUI.Box(iconRect, "");
                    }

                    // Label
                    Rect labelRect = new Rect(iconRect.xMax + 6f, row.y, row.width - iconRect.xMax - 10f, RowH);
                    GUI.Label(labelRect, id);

                    // Click
                    if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
                    {
                        _onSelected?.Invoke(id);
                        editorWindow.Close();
                        Event.current.Use();
                    }
                }
                GUI.EndScrollView();
            }
        }

        // ── Bulk add popup ────────────────────────────────────────────────────
        private sealed class WFCBulkAddPopup : PopupWindowContent
        {
            private const float PopupWidth  = 500f;
            private const float PopupHeight = 520f;
            private const float SearchH     = 22f;
            private const float BtnH        = 26f;
            private const float RowH        = 36f;
            private const float IconSize    = 28f;
            private const float Padding     = 6f;
            private const string SearchCtrl = "WFCBulkSearch";

            private readonly List<string>              _ids;
            private readonly Dictionary<string, Sprite> _sprites;
            private readonly HashSet<string>           _existing;
            private readonly System.Action<List<string>> _onAdd;

            private readonly HashSet<string> _checked = new HashSet<string>();
            private Vector2 _scroll;
            private string  _search = string.Empty;
            private bool    _focusSearch = true;

            public WFCBulkAddPopup(
                List<string> ids,
                Dictionary<string, Sprite> sprites,
                HashSet<string> existing,
                System.Action<List<string>> onAdd)
            {
                _ids      = ids;
                _sprites  = sprites;
                _existing = existing;
                _onAdd    = onAdd;
            }

            public override Vector2 GetWindowSize() => new Vector2(PopupWidth, PopupHeight);
            public override void OnOpen() => _focusSearch = true;

            public override void OnGUI(Rect rect)
            {
                // ── Пошукове поле ──
                Rect searchRect = new Rect(Padding, Padding, rect.width - Padding * 2f, SearchH);
                GUI.SetNextControlName(SearchCtrl);
                EditorGUI.BeginChangeCheck();
                _search = EditorGUI.TextField(searchRect, _search, EditorStyles.toolbarSearchField);
                if (EditorGUI.EndChangeCheck()) _scroll = Vector2.zero;
                if (_focusSearch) { _focusSearch = false; EditorGUI.FocusTextInControl(SearchCtrl); }

                // ── Фільтр ──
                var filtered = GetFiltered();

                // ── Кнопки зверху ──
                float btnY = searchRect.yMax + Padding;
                Rect selectAllRect = new Rect(Padding, btnY, (rect.width - Padding * 3f) * 0.5f, BtnH);
                Rect clearRect     = new Rect(selectAllRect.xMax + Padding, btnY, selectAllRect.width, BtnH);

                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.3f);
                if (GUI.Button(selectAllRect, $"Вибрати всі ({filtered.Count})"))
                {
                    foreach (var id in filtered) _checked.Add(id);
                }
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUI.Button(clearRect, "Зняти всі"))
                {
                    _checked.Clear();
                }
                GUI.backgroundColor = Color.white;

                // ── Список ──
                float listTop = btnY + BtnH + Padding;
                float listH   = rect.height - listTop - BtnH - Padding * 3f;
                Rect listRect = new Rect(Padding, listTop, rect.width - Padding * 2f, listH);
                Rect content  = new Rect(0f, 0f, listRect.width - 14f,
                    Mathf.Max(listH, filtered.Count * RowH));

                _scroll = GUI.BeginScrollView(listRect, _scroll, content);
                for (int i = 0; i < filtered.Count; i++)
                {
                    string id  = filtered[i];
                    Rect row   = new Rect(0f, i * RowH, content.width, RowH - 1f);
                    bool chk   = _checked.Contains(id);
                    bool alrdy = _existing.Contains(id);

                    if (chk)
                        EditorGUI.DrawRect(row, new Color(0.24f, 0.48f, 0.24f, 0.55f));
                    else if (alrdy)
                        EditorGUI.DrawRect(row, new Color(0.4f, 0.4f, 0.0f, 0.3f));
                    else if (row.Contains(Event.current.mousePosition))
                        EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.07f));

                    // Checkbox (ліворуч)
                    Rect chkRect  = new Rect(row.x + 2f, row.y + (RowH - 16f) * 0.5f, 16f, 16f);
                    bool newChk   = GUI.Toggle(chkRect, chk, GUIContent.none);
                    if (newChk != chk) { if (newChk) _checked.Add(id); else _checked.Remove(id); }

                    // Іконка
                    Rect iconRect = new Rect(chkRect.xMax + 4f, row.y + (RowH - IconSize) * 0.5f, IconSize, IconSize);
                    if (_sprites.TryGetValue(id, out Sprite spr) && spr != null)
                    {
                        var tex = spr.texture;
                        var sr  = spr.textureRect;
                        var uv  = new Rect(sr.x / tex.width, sr.y / tex.height, sr.width / tex.width, sr.height / tex.height);
                        GUI.DrawTextureWithTexCoords(iconRect, tex, uv, true);
                    }
                    else
                    {
                        GUI.Box(iconRect, "");
                    }

                    // Назва + "(вже є)" якщо дублікат
                    Rect lblRect = new Rect(iconRect.xMax + 5f, row.y, row.width - iconRect.xMax - 6f, RowH);
                    string lbl  = alrdy ? $"{id}  (вже є)" : id;
                    GUI.Label(lblRect, lbl, alrdy ? EditorStyles.miniLabel : EditorStyles.label);

                    // Click рядка = тогл
                    if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
                    {
                        if (_checked.Contains(id)) _checked.Remove(id); else _checked.Add(id);
                        Event.current.Use();
                    }
                }
                GUI.EndScrollView();

                // ── Кнопка "Додати" внизу ──
                int addCount = 0;
                foreach (var id in _checked) if (!_existing.Contains(id)) addCount++;

                Rect addRect = new Rect(Padding, rect.height - BtnH - Padding, rect.width - Padding * 2f, BtnH);
                GUI.backgroundColor = addCount > 0 ? new Color(0.2f, 0.7f, 0.2f) : Color.gray;
                EditorGUI.BeginDisabledGroup(addCount == 0);
                if (GUI.Button(addRect, $"Додати вибрані ({addCount})"))
                {
                    var toAdd = new List<string>();
                    foreach (var id in _checked)
                        if (!_existing.Contains(id)) toAdd.Add(id);
                    _onAdd?.Invoke(toAdd);
                    editorWindow.Close();
                }
                EditorGUI.EndDisabledGroup();
                GUI.backgroundColor = Color.white;
            }

            private List<string> GetFiltered()
            {
                if (string.IsNullOrWhiteSpace(_search))
                    return new List<string>(_ids);

                var terms = _search.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                var result = new List<string>();
                foreach (var id in _ids)
                    if (System.Array.TrueForAll(terms, t => id.IndexOf(t, System.StringComparison.OrdinalIgnoreCase) >= 0))
                        result.Add(id);
                return result;
            }
        }
    }
}