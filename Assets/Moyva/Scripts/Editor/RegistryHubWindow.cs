using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    public sealed class RegistryHubWindow : EditorWindow
    {
        // ══════════════════════════════════════════════════════
        //  КОНСТАНТИ
        // ══════════════════════════════════════════════════════

        private enum Tab { Tiles, MapObjects, Units, Buildings, Walls, Resources, AssetHealth, BulkDelete }

        private static readonly string[] TabLabels =
            { "  Тайли", "  Об'єкти", "  Юніти", "  Будівлі", "  Стіни", "  Ресурси", "  Asset health", "  Мультивибір" };

        private static readonly string[][] TabIconCandidates =
        {
            new[] { "TerrainAsset Icon", "Terrain Icon", "Grid Icon" },
            new[] { "d_Prefab Icon", "Prefab Icon" },
            new[] { "d_AvatarSelector", "Avatar Icon" },
            new[] { "d_BuildSettings.Standalone.Small", "BuildSettings.Standalone.Small" },
            new[] { "d_SceneViewOrtho", "SceneViewOrtho" },
            new[] { "d_ScriptableObject Icon", "ScriptableObject Icon" },
            new[] { "d_console.warnicon", "console.warnicon", "d_Profiler.Audio" },
            new[] { "d_FilterByType", "FilterByType" },
        };

        private const string TilePrefabFolder    = "Assets/Moyva/Prefabs/Tiles";
        private const string ObjectPrefabFolder  = "Assets/Moyva/Prefabs/Objects";
        private const string UnitPrefabFolder    = "Assets/Moyva/Prefabs/Units";
        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";
        private const string PrefKeyTileRegistryGuid = "Moyva.RegistryHub.TileRegistry.Guid";
        private const string PrefKeyObjectRegistryGuid = "Moyva.RegistryHub.ObjectRegistry.Guid";
        private const string PrefKeyUnitRegistryGuid = "Moyva.RegistryHub.UnitRegistry.Guid";
        private const string PrefKeyBuildingRegistryGuid = "Moyva.RegistryHub.BuildingRegistry.Guid";
        private const string TileLockKey = "TileRegistrySO";
        private const string ObjectLockKey = "MapObjectRegistrySO";
        private const string UnitLockKey = "UnitRegistrySO";
        private const string BuildingLockKey = "BuildingRegistrySO";
        private const string EconomyLockKey = "EconomyDatabaseSO";
        private const int DefaultPageSize = 120;

        private const float SidebarWidth = 170f;

        // ══════════════════════════════════════════════════════
        //  СТАН ВІКНА
        // ══════════════════════════════════════════════════════

        private Tab _tab;
        private Vector2 _contentScroll;
        private string _searchFilter = "";

        // Tile
        private TileRegistrySO   _tileReg;
        private SerializedObject _tileSO;
        private string           _newTileId = "";
        private float            _newTileCost = 1f;
        private Sprite           _newTileSprite;
        private GameObject       _newTilePrefab;
        private bool             _tileCreateOpen = true;
        private bool             _tileListOpen = true;
        private int              _tilePage;

        // MapObject
        private MapObjectRegistrySO _objReg;
        private SerializedObject    _objSO;
        private string              _newObjId = "";
        private Sprite              _newObjSprite;
        private GameObject          _newObjPrefab;
        private bool                _objCreateOpen = true;
        private bool                _objListOpen = true;
        private int                 _objPage;

        // Unit
        private UnitRegistrySO   _unitReg;
        private SerializedObject _unitSO;
        private string           _newUnitId = "";
        private UnitRole         _newUnitRole = UnitRole.Worker;
        private float            _newUnitStamina = 100f;
        private Vector2          _newUnitStaminaRange = new(-5f, 5f);
        private Sprite           _newUnitSprite;
        private GameObject       _newUnitPrefab;
        private int              _unitRoleFilter;
        private bool             _unitCreateOpen = true;
        private bool             _unitListOpen = true;
        private int              _unitPage;

        // Building
        private BuildingRegistrySO _bldReg;
        private SerializedObject   _bldSO;
        private string             _newBldId = "";
        private string             _newBldName = "";
        private BuildingCategory   _newBldCategory;
        private Sprite             _newBldSprite;
        private GameObject         _newBldPrefab;
        private bool               _bldCreateOpen = true;
        private bool               _bldListOpen = true;
        private int                _bldPage;

        // Shared performance controls
        private int _listPageSize = DefaultPageSize;

        // Prefab -> sprite icon cache to avoid repeated GetComponentInChildren on every repaint.
        private static readonly Dictionary<int, Sprite> _prefabSpriteCache = new();
        private static readonly HashSet<int> _prefabNoSpriteCache = new();

        // Walls
        private int _expandedWall = -1;

        // Inline editing — індекс розгорнутого елемента (-1 = нічого)
        private int _expandedTile = -1;
        private int _expandedObj  = -1;
        private int _expandedUnit = -1;
        private int _expandedBld  = -1;

        // Pending sprites для генерації prefab
        private readonly Dictionary<int, Sprite> _pendingTileSprites = new();
        private readonly Dictionary<int, Sprite> _pendingObjSprites  = new();
        private readonly Dictionary<int, Sprite> _pendingUnitSprites = new();
        private readonly Dictionary<int, Sprite> _pendingBldSprites  = new();

        // Resources tab — expanded SO editors
        private readonly HashSet<string> _expandedResources = new();
        private readonly Dictionary<string, UnityEditor.Editor> _resourceEditors = new();
        private EconomyDatabaseSO _economyDb;

        // Bulk delete tab
        private int _bulkCategoryIndex;
        private string _bulkSearch = string.Empty;
        private bool _bulkOnlyProblems;
        private readonly HashSet<string> _bulkSelectedIds = new();
        private Vector2 _bulkScroll;
        private string _bulkPreviewKey = string.Empty;
        private UnityEngine.Object _bulkPreviewObject;

        // Asset Health tab
        private Vector2 _assetHealthScroll;
        private bool _assetHealthOnlyErrors;
        private string _assetHealthSearch = string.Empty;
        private readonly List<AssetHealthIssue> _assetHealthIssues = new();
        private DateTime _assetHealthLastScanUtc;
        private bool _assetHealthScanQueued;
        private readonly EditorLivePreviewThrottle _livePreviewThrottle = new EditorLivePreviewThrottle(repaintFps: 24d, costlyTickHz: 3d);
        private readonly EditorAssetStaleTracker _tileStale = new EditorAssetStaleTracker();
        private readonly EditorAssetStaleTracker _objStale = new EditorAssetStaleTracker();
        private readonly EditorAssetStaleTracker _unitStale = new EditorAssetStaleTracker();
        private readonly EditorAssetStaleTracker _bldStale = new EditorAssetStaleTracker();
        private readonly EditorAssetStaleTracker _ecoStale = new EditorAssetStaleTracker();
        private readonly EditorWindowPerformanceProfiler _perfProfiler = new EditorWindowPerformanceProfiler();

        // Drag-and-drop
        private const string DragDataKey = "RegistryHub_Move";
        private (Tab source, int index, string id)? _dragPayload;

        // Multi-selection + bulk actions inside registry tabs
        private readonly Dictionary<Tab, HashSet<string>> _selectedIdsByTab = new();
        private readonly Dictionary<Tab, string> _selectionAnchorByTab = new();
        private string _bulkRenamePrefix = "item";
        private int _bulkRenameStart = 1;

        // ══════════════════════════════════════════════════════
        //  МЕНЮ
        // ══════════════════════════════════════════════════════

        [MenuItem("Moyva/Tools/Registry Hub %#r")]
        public static void Open()
        {
            var w = GetWindow<RegistryHubWindow>("Registry Hub");
            w.minSize = new Vector2(660f, 460f);
            w.Show();
        }

        public static void Open(int tabIndex)
        {
            var w = GetWindow<RegistryHubWindow>("Registry Hub");
            w.minSize = new Vector2(660f, 460f);
            w._tab = (Tab)Mathf.Clamp(tabIndex, 0, TabLabels.Length - 1);
            w.Show();
            w.Focus();
        }

        // ══════════════════════════════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════════════════════════════

        private void OnEnable()
        {
            LoadRegistrySelections();
            AutoFind();
            CaptureStaleBaselines();
        }
        private void OnFocus()
        {
            RefreshSOs();
            _prefabSpriteCache.Clear();
            _prefabNoSpriteCache.Clear();
        }

        private void OnDisable()
        {
            SaveRegistrySelections();
            foreach (var editor in _resourceEditors.Values)
                if (editor != null) DestroyImmediate(editor);
            _resourceEditors.Clear();
        }

        private void AutoFind()
        {
            _tileReg ??= MoyvaProjectEditorContext.GetOrFindFirst<TileRegistrySO>();
            _objReg  ??= MoyvaProjectEditorContext.GetOrFindFirst<MapObjectRegistrySO>();
            _unitReg ??= MoyvaProjectEditorContext.GetOrFindFirst<UnitRegistrySO>();
            _bldReg  ??= MoyvaProjectEditorContext.GetOrFindFirst<BuildingRegistrySO>();
            _economyDb ??= MoyvaProjectEditorContext.GetOrFindFirst<EconomyDatabaseSO>();
            RefreshSOs();
            CaptureStaleBaselines();
            SaveRegistrySelections();
        }

        private void RefreshSOs()
        {
            _tileSO = _tileReg ? new SerializedObject(_tileReg) : null;
            _objSO  = _objReg  ? new SerializedObject(_objReg)  : null;
            _unitSO = _unitReg ? new SerializedObject(_unitReg) : null;
            _bldSO  = _bldReg  ? new SerializedObject(_bldReg)  : null;
        }

        // ══════════════════════════════════════════════════════
        //  OnGUI — ГОЛОВНИЙ МАЛЮВАННЯ
        // ══════════════════════════════════════════════════════

        private void OnGUI()
        {
            _perfProfiler.BeginFrame();
            _tileSO?.Update();
            _objSO?.Update();
            _unitSO?.Update();
            _bldSO?.Update();

            _perfProfiler.BeginSection("Toolbar");
            DrawToolbar();
            _perfProfiler.EndSection("Toolbar");

            _perfProfiler.BeginSection("Content");
            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawContent();
            EditorGUILayout.EndHorizontal();
            _perfProfiler.EndSection("Content");

            _perfProfiler.BeginSection("Status");
            DrawStatusBar();
            _perfProfiler.EndSection("Status");

            if (IsActiveRegistryStale())
                EditorWindowSharedUI.DrawWarning("Активний реєстр змінено зовні. Перевірте дані перед Save.", MessageType.Warning);

            _perfProfiler.EndFrame();
        }

        // ── Тулбар ──────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("REGISTRY HUB", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            bool unlocked = EditorRegistryWriteLock.IsUnlocked(GetActiveLockKey());
            bool nextUnlocked = GUILayout.Toggle(unlocked, "Unlock", EditorStyles.toolbarButton, GUILayout.Width(70f));
            if (nextUnlocked != unlocked)
                EditorRegistryWriteLock.SetUnlocked(GetActiveLockKey(), nextUnlocked);
            if (GUILayout.Button(new GUIContent("  _ → -", "Замінити '_' на '-' в усіх ID та їх посиланнях"), EditorStyles.toolbarButton))
                FixUnderscoreIds();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                _tileReg = null; _objReg = null; _unitReg = null; _bldReg = null;
                AutoFind();
                _livePreviewThrottle.TryRepaint(this, force: true);
            }
            EditorGUILayout.EndHorizontal();
        }

        // ── Бічна панель ────────────────────────────────────

        private void DrawSidebar()
        {
            Rect side = EditorGUILayout.BeginVertical(GUILayout.Width(SidebarWidth));
            EditorGUI.DrawRect(side, RegistryEditorStyles.SidebarBg);
            EditorGUILayout.Space(6);

            for (int i = 0; i < TabLabels.Length; i++)
            {
                Tab t = (Tab)i;
                bool active = _tab == t;
                GUIStyle st = active ? RegistryEditorStyles.SidebarTabActive : RegistryEditorStyles.SidebarTab;

                GUIContent content = new(TabLabels[i], FindFirstTabIcon(i));
                if (GUILayout.Button(content, st))
                {
                    _tab = t;
                    _contentScroll = Vector2.zero;
                    GUI.FocusControl(null);
                }
                Rect tabRect = GUILayoutUtility.GetLastRect();

                // Drop target
                if (_dragPayload.HasValue && _dragPayload.Value.source != t)
                {
                    var evt = Event.current;
                    if (tabRect.Contains(evt.mousePosition))
                    {
                        if (evt.type == EventType.DragUpdated)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                            evt.Use();
                        }
                        else if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            var payload = _dragPayload.Value;
                            MoveEntry(payload.id, payload.source, payload.index, t);
                            _dragPayload = null;
                            evt.Use();
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("Знайдені реєстри:", EditorStyles.miniLabel);
            DrawAssetRow("Tile",     _tileReg);
            DrawAssetRow("Object",   _objReg);
            DrawAssetRow("Unit",     _unitReg);
            DrawAssetRow("Building", _bldReg);
            EditorGUILayout.Space(6);

            EditorGUILayout.EndVertical();
        }

        private static Texture FindFirstTabIcon(int tabIndex)
        {
            if (tabIndex < 0 || tabIndex >= TabIconCandidates.Length)
                return null;

            var names = TabIconCandidates[tabIndex];
            for (int i = 0; i < names.Length; i++)
            {
                var texture = EditorGUIUtility.FindTexture(names[i]);
                if (texture != null)
                    return texture;
            }

            return null;
        }

        private static void DrawAssetRow(string label, UnityEngine.Object asset)
        {
            EditorGUILayout.BeginHorizontal();
            Color prev = GUI.color;
            GUI.color = asset ? RegistryEditorStyles.SuccessCol : RegistryEditorStyles.ErrorCol;
            GUILayout.Label(asset ? "\u2713" : "\u2717", GUILayout.Width(14));
            GUI.color = prev;
            GUILayout.Label(label, EditorStyles.miniLabel);
            if (asset && GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(34)))
                EditorGUIUtility.PingObject(asset);
            EditorGUILayout.EndHorizontal();
        }

        // ── Контент ─────────────────────────────────────────

        private void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
            EditorGUILayout.Space(2);

            switch (_tab)
            {
                case Tab.Tiles:      DrawTileTab();   break;
                case Tab.MapObjects: DrawObjTab();    break;
                case Tab.Units:      DrawUnitTab();   break;
                case Tab.Buildings:  DrawBldTab();    break;
                case Tab.Walls:      DrawWallsTab();  break;
                case Tab.Resources:  DrawResourcesTab();  break;
                case Tab.AssetHealth: DrawAssetHealthTab(); break;
                case Tab.BulkDelete: DrawBulkDeleteTab(); break;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        // ── Статус бар ──────────────────────────────────────

        private void DrawStatusBar()
        {
            int tiles = _tileSO?.FindProperty("_definitions")?.arraySize ?? 0;
            int objs  = _objSO?.FindProperty("_definitions")?.arraySize ?? 0;
            int units = _unitSO?.FindProperty("Configs")?.arraySize ?? 0;
            int blds  = _bldSO?.FindProperty("Buildings")?.arraySize ?? 0;
            int walls = _bldSO?.FindProperty("WallCollections")?.arraySize ?? 0;
            string perf = _perfProfiler.BuildSummary();
            string lockState = EditorRegistryWriteLock.IsUnlocked(GetActiveLockKey()) ? "UNLOCKED" : "READONLY";

            Rect r = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.DrawRect(r, RegistryEditorStyles.SidebarBg);
            GUI.Label(r, $"   Тайли: {tiles}  |  Об'єкти: {objs}  |  Юніти: {units}  |  Будівлі: {blds}  |  Стіни: {walls}  |  lock: {lockState}  |  {perf}",
                EditorStyles.centeredGreyMiniLabel);
        }

        // ══════════════════════════════════════════════════════
        //  TILES TAB
        // ══════════════════════════════════════════════════════

        private void DrawTileTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Tile Registry — типи тайлів", RegistryEditorStyles.Accent);

            EditorGUI.BeginChangeCheck();
            _tileReg = (TileRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _tileReg, typeof(TileRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveRegistryPreference(PrefKeyTileRegistryGuid, _tileReg);
                RefreshSOs();
            }
            if (_tileReg && _tileSO?.targetObject != _tileReg) RefreshSOs();
            if (!_tileReg) { EditorGUILayout.HelpBox("Оберіть TileRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_tileReg);
            RegistryEditorStyles.DrawSeparator();
            DrawRegistryMultiSelectionToolbar();

            var defs = _tileSO.FindProperty("_definitions");
            int count = defs?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);
            _tileListOpen = EditorGUILayout.Foldout(_tileListOpen, "Список тайлів", true, EditorStyles.foldoutHeader);

            if (_tileListOpen)
            {
                DrawPaginationToolbar(count, ref _tilePage, "тайлів", out int start, out int endExclusive);
                var tileSelectionScope = new List<string>();
                for (int i = start; i < endExclusive; i++)
                {
                    string scopeId = defs.GetArrayElementAtIndex(i).FindPropertyRelative("_id")?.stringValue;
                    if (!string.IsNullOrWhiteSpace(scopeId) && MatchesFilter(scopeId))
                        tileSelectionScope.Add(scopeId);
                }

                if (count == 0)
                    EditorGUILayout.LabelField("Записів немає. Додайте перший тайл нижче.", RegistryEditorStyles.CenteredMini);

                int removeIdx = -1;
                EditorGUI.BeginChangeCheck();
                for (int i = start; i < endExclusive; i++)
                {
                    var el = defs.GetArrayElementAtIndex(i);
                    var costProp = el.FindPropertyRelative("_movementCost");
                    var prefabProp = el.FindPropertyRelative("_visualPrefab");
                    string id     = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                    float  cost   = costProp?.floatValue ?? 0f;
                    var    prefab = prefabProp?.objectReferenceValue;

                    if (!MatchesFilter(id)) continue;

                    GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                    EditorGUILayout.BeginVertical(style);
                    Rect tileRowRect = GUILayoutUtility.GetRect(0, 0);

                    // Рядок-заголовок: швидкі налаштування доступні без розгортання
                    EditorGUILayout.BeginHorizontal();
                    bool isOpen = _expandedTile == i;
                    if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                        _expandedTile = isOpen ? -1 : i;
                    DrawSelectionMarker(Tab.Tiles, id);
                    DrawPrefabMiniIcon(prefab);
                    DrawIdLabel(id, 130);
                    float newCost = EditorGUILayout.FloatField(cost, GUILayout.Width(52));
                    var newPrefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(150));
                    if (costProp != null && !Mathf.Approximately(newCost, cost))
                        costProp.floatValue = newCost;
                    if (prefabProp != null && newPrefab != prefab)
                        prefabProp.objectReferenceValue = newPrefab;
                    if (DrawDeleteBtn()) removeIdx = i;
                    EditorGUILayout.EndHorizontal();

                    prefab = prefabProp?.objectReferenceValue;

                    if (!prefab)
                        DrawMissingPrefabRow(i, id, el, "_visualPrefab", TilePrefabFolder, _pendingTileSprites, _tileSO);

                    if (isOpen)
                    {
                        DrawInlineEditBox(el, new[] {
                            ("_id",            "ID"),
                            ("_movementCost",  "Movement Cost"),
                            ("_visualPrefab",  "Visual Prefab"),
                        });
                    }

                    EditorGUILayout.EndVertical();
                                        {
                                                int ci = i;
                                                string cid = id;
                                                Rect rowHitRect = new Rect(tileRowRect.x, tileRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - tileRowRect.y);
                                                HandleRowSelectionMouseUp(rowHitRect, Tab.Tiles, cid, tileSelectionScope);
                                                HandleRowContextClick(
                                                rowHitRect,
                        () => ShowMoveMenu(cid, Tab.Tiles, ci));
                                            HandleRowDragStart(
                                                rowHitRect,
                        cid, Tab.Tiles, ci); }
                }

                if (EditorGUI.EndChangeCheck())
                    _tileSO.ApplyModifiedProperties();
                HandleRemove(defs, removeIdx, "_id", _tileSO);
                DrawDeleteAllButton(defs, "_id", _tileSO, "тайлів");
            }
            RegistryEditorStyles.DrawSeparator();

            // ── Створення ──
            _tileCreateOpen = EditorGUILayout.Foldout(_tileCreateOpen, "\u2795 Створити новий тайл", true, EditorStyles.foldoutHeader);
            if (_tileCreateOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                var tileDefs = _tileSO.FindProperty("_definitions");
                _newTileId     = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newTileId, tileDefs, "_id");
                _newTileCost   = EditorGUILayout.FloatField("Movement Cost", _newTileCost);
                _newTileSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newTileSprite, typeof(Sprite), false);
                Rect newTileSpriteRect = GUILayoutUtility.GetLastRect();
                if (SpriteImportDragDropPolicy.HandleDrop(newTileSpriteRect, ref _newTileSprite, "RegistryHub.NewTileSprite"))
                    Repaint();
                SpriteImportDragDropPolicy.EnsureAllowedSprite(ref _newTileSprite, "RegistryHub.NewTileSprite");
                _newTilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newTilePrefab, typeof(GameObject), false);
                if (!_newTilePrefab && !_newTileSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);
                EditorGUI.BeginDisabledGroup(RegistryEditorStyles.ValidateIdFull(_newTileId, tileDefs, "_id") != null);
                if (GUILayout.Button("\u2713 Створити тайл", RegistryEditorStyles.CreateButton)) CreateTile();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateTile()
        {
            string id = _newTileId.Trim();
            if (!ValidateBeforeAdd(id)) return;

            var defs = _tileSO.FindProperty("_definitions");
            if (ContainsId(defs, "_id", id)) { Err($"Tile ID '{id}' вже існує."); return; }

            GameObject prefab = ResolvePrefab(id, _newTilePrefab, _newTileSprite, TilePrefabFolder);

            int idx = defs.arraySize;
            defs.InsertArrayElementAtIndex(idx);
            var el = defs.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("_id").stringValue = id;
            el.FindPropertyRelative("_movementCost").floatValue = _newTileCost;
            el.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
            _tileSO.ApplyModifiedProperties();
            SaveAndNotify($"Тайл '{id}' створено");
            _newTileId = ""; _newTileSprite = null; _newTilePrefab = null;
        }

        // ══════════════════════════════════════════════════════
        //  MAP OBJECTS TAB
        // ══════════════════════════════════════════════════════

        private void DrawObjTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Map Object Registry — об'єкти карти", RegistryEditorStyles.Accent);

            EditorGUI.BeginChangeCheck();
            _objReg = (MapObjectRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _objReg, typeof(MapObjectRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveRegistryPreference(PrefKeyObjectRegistryGuid, _objReg);
                RefreshSOs();
            }
            if (_objReg && _objSO?.targetObject != _objReg) RefreshSOs();
            if (!_objReg) { EditorGUILayout.HelpBox("Оберіть MapObjectRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_objReg);
            RegistryEditorStyles.DrawSeparator();
            DrawRegistryMultiSelectionToolbar();

            var defs = _objSO.FindProperty("_definitions");
            int count = defs?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);
            _objListOpen = EditorGUILayout.Foldout(_objListOpen, "Список об'єктів", true, EditorStyles.foldoutHeader);

            if (_objListOpen)
            {
                DrawPaginationToolbar(count, ref _objPage, "об'єктів", out int start, out int endExclusive);
                var objSelectionScope = new List<string>();
                for (int i = start; i < endExclusive; i++)
                {
                    string scopeId = defs.GetArrayElementAtIndex(i).FindPropertyRelative("_id")?.stringValue;
                    if (!string.IsNullOrWhiteSpace(scopeId) && MatchesFilter(scopeId))
                        objSelectionScope.Add(scopeId);
                }

                if (count == 0)
                    EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);

                int removeIdx = -1;
                EditorGUI.BeginChangeCheck();
                for (int i = start; i < endExclusive; i++)
                {
                    var el = defs.GetArrayElementAtIndex(i);
                    var prefabProp = el.FindPropertyRelative("_visualPrefab");
                    string id = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                    var prefab = prefabProp?.objectReferenceValue;

                    if (!MatchesFilter(id)) continue;

                    GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                    EditorGUILayout.BeginVertical(style);
                    Rect objRowRect = GUILayoutUtility.GetRect(0, 0);

                    EditorGUILayout.BeginHorizontal();
                    bool isOpen = _expandedObj == i;
                    if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                        _expandedObj = isOpen ? -1 : i;
                    DrawSelectionMarker(Tab.MapObjects, id);
                    DrawPrefabMiniIcon(prefab);
                    DrawIdLabel(id, 150);
                    var newPrefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(190));
                    if (prefabProp != null && newPrefab != prefab)
                        prefabProp.objectReferenceValue = newPrefab;
                    if (DrawDeleteBtn()) removeIdx = i;
                    EditorGUILayout.EndHorizontal();

                    prefab = prefabProp?.objectReferenceValue;

                    if (!prefab)
                        DrawMissingPrefabRow(i, id, el, "_visualPrefab", ObjectPrefabFolder, _pendingObjSprites, _objSO);

                    if (isOpen)
                    {
                        DrawInlineEditBox(el, new[] {
                            ("_id",            "ID"),
                            ("_visualPrefab",  "Visual Prefab"),
                        });
                    }

                    EditorGUILayout.EndVertical();
                                        {
                                                int ci = i;
                                                string cid = id;
                                                Rect rowHitRect = new Rect(objRowRect.x, objRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - objRowRect.y);
                                                HandleRowSelectionMouseUp(rowHitRect, Tab.MapObjects, cid, objSelectionScope);
                                                HandleRowContextClick(
                                                rowHitRect,
                        () => ShowMoveMenu(cid, Tab.MapObjects, ci));
                                            HandleRowDragStart(
                                                rowHitRect,
                        cid, Tab.MapObjects, ci); }
                }

                if (EditorGUI.EndChangeCheck())
                    _objSO.ApplyModifiedProperties();
                HandleRemove(defs, removeIdx, "_id", _objSO);
                DrawDeleteAllButton(defs, "_id", _objSO, "об'єктів");
            }
            RegistryEditorStyles.DrawSeparator();

            _objCreateOpen = EditorGUILayout.Foldout(_objCreateOpen, "\u2795 Створити новий об'єкт", true, EditorStyles.foldoutHeader);
            if (_objCreateOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                var objDefs = _objSO.FindProperty("_definitions");
                _newObjId     = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newObjId, objDefs, "_id");
                _newObjSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newObjSprite, typeof(Sprite), false);
                Rect newObjSpriteRect = GUILayoutUtility.GetLastRect();
                if (SpriteImportDragDropPolicy.HandleDrop(newObjSpriteRect, ref _newObjSprite, "RegistryHub.NewObjectSprite"))
                    Repaint();
                SpriteImportDragDropPolicy.EnsureAllowedSprite(ref _newObjSprite, "RegistryHub.NewObjectSprite");
                _newObjPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newObjPrefab, typeof(GameObject), false);
                if (!_newObjPrefab && !_newObjSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);
                EditorGUI.BeginDisabledGroup(RegistryEditorStyles.ValidateIdFull(_newObjId, objDefs, "_id") != null);
                if (GUILayout.Button("\u2713 Створити об'єкт", RegistryEditorStyles.CreateButton)) CreateObj();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateObj()
        {
            string id = _newObjId.Trim();
            if (!ValidateBeforeAdd(id)) return;

            var defs = _objSO.FindProperty("_definitions");
            if (ContainsId(defs, "_id", id)) { Err($"Object ID '{id}' вже існує."); return; }

            GameObject prefab = ResolvePrefab(id, _newObjPrefab, _newObjSprite, ObjectPrefabFolder);

            int idx = defs.arraySize;
            defs.InsertArrayElementAtIndex(idx);
            var el = defs.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("_id").stringValue = id;
            el.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
            _objSO.ApplyModifiedProperties();
            SaveAndNotify($"Об'єкт '{id}' створено");
            _newObjId = ""; _newObjSprite = null; _newObjPrefab = null;
        }

        // ══════════════════════════════════════════════════════
        //  UNITS TAB
        // ══════════════════════════════════════════════════════

        private void DrawUnitTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Unit Registry — класи юнітів", RegistryEditorStyles.Accent);

            EditorGUI.BeginChangeCheck();
            _unitReg = (UnitRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _unitReg, typeof(UnitRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveRegistryPreference(PrefKeyUnitRegistryGuid, _unitReg);
                RefreshSOs();
            }
            if (_unitReg && _unitSO?.targetObject != _unitReg) RefreshSOs();
            if (!_unitReg) { EditorGUILayout.HelpBox("Оберіть UnitRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_unitReg);
            RegistryEditorStyles.DrawSeparator();
            DrawRegistryMultiSelectionToolbar();

            var configs = _unitSO.FindProperty("Configs");
            int count = configs?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);
            EditorGUILayout.HelpBox("Worker — економічні юніти (рубати, молоти, збирати тощо). Military — бойові юніти.", MessageType.Info);
            _unitRoleFilter = GUILayout.Toolbar(_unitRoleFilter, new[] { "Усі", "Worker", "Military" });

            int workerCount = 0;
            int militaryCount = 0;
            for (int i = 0; i < count; i++)
            {
                var roleProp = configs.GetArrayElementAtIndex(i).FindPropertyRelative("Role");
                int roleIndex = roleProp != null ? roleProp.enumValueIndex : (int)UnitRole.Worker;
                if (roleIndex == (int)UnitRole.Military) militaryCount++;
                else workerCount++;
            }
            EditorGUILayout.LabelField($"Worker: {workerCount}   Military: {militaryCount}", EditorStyles.miniLabel);
            _unitListOpen = EditorGUILayout.Foldout(_unitListOpen, "Список юнітів", true, EditorStyles.foldoutHeader);

            if (_unitListOpen)
            {
                DrawPaginationToolbar(count, ref _unitPage, "юнітів", out int start, out int endExclusive);
                var unitSelectionScope = new List<string>();
                for (int i = start; i < endExclusive; i++)
                {
                    var scopeEl = configs.GetArrayElementAtIndex(i);
                    string scopeId = scopeEl.FindPropertyRelative("TypeId")?.stringValue;
                    int scopeRoleIndex = scopeEl.FindPropertyRelative("Role")?.enumValueIndex ?? (int)UnitRole.Worker;
                    UnitRole scopeRole = scopeRoleIndex == (int)UnitRole.Military ? UnitRole.Military : UnitRole.Worker;
                    if (string.IsNullOrWhiteSpace(scopeId) || !MatchesFilter(scopeId))
                        continue;
                    if (_unitRoleFilter == 1 && scopeRole != UnitRole.Worker)
                        continue;
                    if (_unitRoleFilter == 2 && scopeRole != UnitRole.Military)
                        continue;
                    unitSelectionScope.Add(scopeId);
                }

                if (count == 0)
                    EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);

                int removeIdx = -1;
                EditorGUI.BeginChangeCheck();
                for (int i = start; i < endExclusive; i++)
                {
                var el = configs.GetArrayElementAtIndex(i);
                string typeId   = el.FindPropertyRelative("TypeId")?.stringValue ?? "?";
                float  stamina  = el.FindPropertyRelative("BaseStamina")?.floatValue ?? 0f;
                var    prefabProp = el.FindPropertyRelative("Prefab");
                var    prefab   = prefabProp?.objectReferenceValue;
                var    roleProp = el.FindPropertyRelative("Role");
                int    roleIndex = roleProp != null ? roleProp.enumValueIndex : (int)UnitRole.Worker;
                UnitRole role = roleIndex == (int)UnitRole.Military ? UnitRole.Military : UnitRole.Worker;

                if (!MatchesFilter(typeId)) continue;
                if (_unitRoleFilter == 1 && role != UnitRole.Worker) continue;
                if (_unitRoleFilter == 2 && role != UnitRole.Military) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);
                Rect unitRowRect = GUILayoutUtility.GetRect(0, 0);

                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedUnit == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedUnit = isOpen ? -1 : i;
                DrawSelectionMarker(Tab.Units, typeId);
                DrawPrefabMiniIcon(prefab);
                DrawIdLabel(typeId, 115);
                RegistryEditorStyles.DrawBadge(role == UnitRole.Worker ? "Worker" : "Military", role == UnitRole.Worker ? new Color(0.16f, 0.53f, 0.25f) : new Color(0.62f, 0.20f, 0.20f));
                EditorGUILayout.LabelField($"Стаміна: {stamina:F0}", EditorStyles.miniLabel, GUILayout.Width(90));
                var newPrefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(150));
                if (prefabProp != null && newPrefab != prefab)
                    prefabProp.objectReferenceValue = newPrefab;
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                prefab = prefabProp?.objectReferenceValue;

                // FATAL: prefab відсутній
                if (!prefab)
                    DrawMissingPrefabRow(i, typeId, el, "Prefab", UnitPrefabFolder, _pendingUnitSprites, _unitSO);

                if (isOpen)
                {
                    DrawInlineEditBox(el, new[] {
                        ("TypeId",              "Type ID"),
                        ("Role",                "Class"),
                        ("BaseStamina",         "Base Stamina"),
                        ("StaminaRandomRange",  "Stamina Random Range"),
                        ("Prefab",              "Prefab"),
                        ("AnimationSettings",   "Animation Settings"),
                    });
                }

                EditorGUILayout.EndVertical();
                                {
                                        int ci = i;
                                        string cid = typeId;
                                        Rect rowHitRect = new Rect(unitRowRect.x, unitRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - unitRowRect.y);
                                        HandleRowSelectionMouseUp(rowHitRect, Tab.Units, cid, unitSelectionScope);
                                        HandleRowContextClick(
                                        rowHitRect,
                    () => ShowMoveMenu(cid, Tab.Units, ci));
                  HandleRowDragStart(
                                        rowHitRect,
                    cid, Tab.Units, ci); }
                    }

                    if (EditorGUI.EndChangeCheck())
                        _unitSO.ApplyModifiedProperties();
                    HandleRemove(configs, removeIdx, "TypeId", _unitSO);
                    DrawDeleteAllButton(configs, "TypeId", _unitSO, "юнітів");
                    }
            RegistryEditorStyles.DrawSeparator();

            _unitCreateOpen = EditorGUILayout.Foldout(_unitCreateOpen, "\u2795 Створити нового юніта", true, EditorStyles.foldoutHeader);
            if (_unitCreateOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                var unitConfigs = _unitSO.FindProperty("Configs");
                _newUnitId           = RegistryEditorStyles.IdFieldWithDuplicateCheck("Type ID", _newUnitId, unitConfigs, "TypeId");
                _newUnitRole         = (UnitRole)EditorGUILayout.EnumPopup(new GUIContent("Class", "Worker - економічний юніт; Military - бойовий юніт."), _newUnitRole);
                _newUnitStamina      = EditorGUILayout.FloatField("Base Stamina", _newUnitStamina);
                _newUnitStaminaRange = EditorGUILayout.Vector2Field("Stamina Random Range", _newUnitStaminaRange);
                _newUnitSprite       = (Sprite)EditorGUILayout.ObjectField("Sprite", _newUnitSprite, typeof(Sprite), false);
                Rect newUnitSpriteRect = GUILayoutUtility.GetLastRect();
                if (SpriteImportDragDropPolicy.HandleDrop(newUnitSpriteRect, ref _newUnitSprite, "RegistryHub.NewUnitSprite"))
                    Repaint();
                SpriteImportDragDropPolicy.EnsureAllowedSprite(ref _newUnitSprite, "RegistryHub.NewUnitSprite");
                _newUnitPrefab       = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newUnitPrefab, typeof(GameObject), false);
                if (!_newUnitPrefab && !_newUnitSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);
                EditorGUILayout.Space(4);
                EditorGUI.BeginDisabledGroup(RegistryEditorStyles.ValidateIdFull(_newUnitId, unitConfigs, "TypeId") != null);
                if (GUILayout.Button("\u2713 Створити юніта", RegistryEditorStyles.CreateButton)) CreateUnit();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateUnit()
        {
            string id = _newUnitId.Trim();
            if (!ValidateBeforeAdd(id)) return;

            var configs = _unitSO.FindProperty("Configs");
            if (ContainsId(configs, "TypeId", id)) { Err($"Unit Type ID '{id}' вже існує."); return; }

            GameObject prefab = ResolvePrefab(id, _newUnitPrefab, _newUnitSprite, UnitPrefabFolder);

            int idx = configs.arraySize;
            configs.InsertArrayElementAtIndex(idx);
            var el = configs.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("TypeId").stringValue = id;
            el.FindPropertyRelative("Role").enumValueIndex = (int)_newUnitRole;
            el.FindPropertyRelative("BaseStamina").floatValue = _newUnitStamina;
            el.FindPropertyRelative("StaminaRandomRange").vector2Value = _newUnitStaminaRange;
            el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;

            // default animation settings
            var anim = el.FindPropertyRelative("AnimationSettings");
            if (anim != null)
            {
                var dur = anim.FindPropertyRelative("MoveDurationPerTile");
                if (dur != null) dur.floatValue = 0.3f;
                var delay = anim.FindPropertyRelative("DelayOnTile");
                if (delay != null) delay.floatValue = 0.05f;
            }

            _unitSO.ApplyModifiedProperties();
            SaveAndNotify($"Юніт '{id}' створено");
            _newUnitId = ""; _newUnitRole = UnitRole.Worker; _newUnitSprite = null; _newUnitPrefab = null;
        }

        // ══════════════════════════════════════════════════════
        //  BUILDINGS TAB
        // ══════════════════════════════════════════════════════

        private void DrawBldTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Building Registry — будівлі", RegistryEditorStyles.Accent);

            EditorGUI.BeginChangeCheck();
            _bldReg = (BuildingRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _bldReg, typeof(BuildingRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveRegistryPreference(PrefKeyBuildingRegistryGuid, _bldReg);
                RefreshSOs();
            }
            if (_bldReg && _bldSO?.targetObject != _bldReg) RefreshSOs();
            if (!_bldReg) { EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_bldReg);
            RegistryEditorStyles.DrawSeparator();
            DrawRegistryMultiSelectionToolbar();

            var blds = _bldSO.FindProperty("Buildings");
            int count = blds?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Валідувати модулі", EditorStyles.miniButton))
                ValidateAllBuildingModules();
            if (GUILayout.Button(new GUIContent("Наповнити стандартним набором", "Додає та оновлює типовий набір будівель з economy plan прямо в поточному реєстрі."), EditorStyles.miniButton))
                PopulateBuildingRegistryFromHub();
            EditorGUILayout.EndHorizontal();

            if (count == 0)
                EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);
            _bldListOpen = EditorGUILayout.Foldout(_bldListOpen, "Список будівель", true, EditorStyles.foldoutHeader);

            if (_bldListOpen)
            {
                DrawPaginationToolbar(count, ref _bldPage, "будівель", out int start, out int endExclusive);
                var bldSelectionScope = new List<string>();
                for (int i = start; i < endExclusive; i++)
                {
                    var scopeEl = blds.GetArrayElementAtIndex(i);
                    string scopeId = scopeEl.FindPropertyRelative("Id")?.stringValue;
                    string scopeName = scopeEl.FindPropertyRelative("DisplayName")?.stringValue ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(scopeId) && (MatchesFilter(scopeId) || MatchesFilter(scopeName)))
                        bldSelectionScope.Add(scopeId);
                }

                int removeIdx = -1;
                EditorGUI.BeginChangeCheck();
                for (int i = start; i < endExclusive; i++)
                {
                var el = blds.GetArrayElementAtIndex(i);
                string id          = el.FindPropertyRelative("Id")?.stringValue ?? "?";
                string displayName = el.FindPropertyRelative("DisplayName")?.stringValue ?? "";
                int    category    = el.FindPropertyRelative("Category")?.enumValueIndex ?? 0;
                var    prefabProp  = el.FindPropertyRelative("Prefab");
                var    prefab      = prefabProp?.objectReferenceValue;
                BuildingDefinition runtimeDefinition = null;
                if (_bldReg != null && _bldReg.Buildings != null && i < _bldReg.Buildings.Length)
                    runtimeDefinition = _bldReg.Buildings[i];

                if (!MatchesFilter(id) && !MatchesFilter(displayName)) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);
                Rect bldRowRect = GUILayoutUtility.GetRect(0, 0);

                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedBld == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedBld = isOpen ? -1 : i;
                DrawSelectionMarker(Tab.Buildings, id);
                DrawPrefabMiniIcon(prefab);
                DrawIdLabel(id, 90);

                if (!string.IsNullOrEmpty(displayName))
                    EditorGUILayout.LabelField($"\"{displayName}\"", EditorStyles.miniLabel, GUILayout.Width(120));

                string catName = ((BuildingCategory)category).ToString();
                RegistryEditorStyles.DrawBadge(catName, CategoryColor(category));

                int workers = runtimeDefinition != null
                    ? BuildingDefinitionCapabilities.GetRequiredWorkers(runtimeDefinition) : 0;
                bool isHousing = runtimeDefinition != null && BuildingDefinitionCapabilities.IsHousing(runtimeDefinition);
                bool isWarehouse = runtimeDefinition != null && BuildingDefinitionCapabilities.IsWarehouse(runtimeDefinition);
                bool isTownHall = runtimeDefinition != null && BuildingDefinitionCapabilities.IsTownHall(runtimeDefinition);
                bool isCastle = runtimeDefinition != null && BuildingDefinitionCapabilities.IsCastle(runtimeDefinition);
                if (workers > 0)
                    EditorGUILayout.LabelField($"\ud83d\udc77{workers}", EditorStyles.miniLabel, GUILayout.Width(32));
                if (isHousing)
                {
                    int cap = runtimeDefinition != null
                        ? BuildingDefinitionCapabilities.GetHousingCapacity(runtimeDefinition) : 0;
                    RegistryEditorStyles.DrawBadge($"\u2302{cap}", new Color(0.3f, 0.7f, 0.4f));
                }
                if (isWarehouse)
                    RegistryEditorStyles.DrawBadge("WH", new Color(0.25f, 0.55f, 0.9f));
                if (isTownHall)
                    RegistryEditorStyles.DrawBadge("TH", new Color(0.8f, 0.6f, 0.2f));
                if (isCastle)
                    RegistryEditorStyles.DrawBadge("CST", new Color(0.55f, 0.45f, 0.2f));

                if ((BuildingCategory)category == BuildingCategory.Industrial)
                {
                    string resourceId = runtimeDefinition != null
                        ? BuildingDefinitionCapabilities.GetIndustrialResourceId(runtimeDefinition) : string.Empty;
                    if (!string.IsNullOrWhiteSpace(resourceId))
                        RegistryEditorStyles.DrawBadge($"R:{resourceId}", new Color(0.35f, 0.65f, 0.85f));
                }
                int buildCostCount = runtimeDefinition?.ConstructionCost?.Count ?? 0;
                if (buildCostCount > 0)
                    RegistryEditorStyles.DrawBadge($"💰{buildCostCount}", new Color(0.80f, 0.65f, 0.20f));
                var newPrefab = (GameObject)EditorGUILayout.ObjectField(prefab, typeof(GameObject), false, GUILayout.Width(120));
                if (prefabProp != null && newPrefab != prefab)
                    prefabProp.objectReferenceValue = newPrefab;
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                prefab = prefabProp?.objectReferenceValue;

                // FATAL: prefab відсутній
                if (!prefab)
                    DrawMissingPrefabRow(i, id, el, "Prefab", BuildingPrefabFolder, _pendingBldSprites, _bldSO);

                if (isOpen)
                {
                    DrawBuildingInlineEditBox(el, i);
                }

                EditorGUILayout.EndVertical();
                                {
                                        int ci = i;
                                        string cid = id;
                                        Rect rowHitRect = new Rect(bldRowRect.x, bldRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - bldRowRect.y);
                                        HandleRowSelectionMouseUp(rowHitRect, Tab.Buildings, cid, bldSelectionScope);
                                        HandleRowContextClick(
                                        rowHitRect,
                    () => ShowMoveMenu(cid, Tab.Buildings, ci));
                  HandleRowDragStart(
                                        rowHitRect,
                    cid, Tab.Buildings, ci); }
                    }

                    if (EditorGUI.EndChangeCheck())
                        _bldSO.ApplyModifiedProperties();
                    HandleRemove(blds, removeIdx, "Id", _bldSO);
                    DrawDeleteAllButton(blds, "Id", _bldSO, "будівель");
                    }
            RegistryEditorStyles.DrawSeparator();

            _bldCreateOpen = EditorGUILayout.Foldout(_bldCreateOpen, "\u2795 Створити нову будівлю", true, EditorStyles.foldoutHeader);
            if (_bldCreateOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                var bldArr = _bldSO.FindProperty("Buildings");
                _newBldId       = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newBldId, bldArr, "Id");
                _newBldName     = EditorGUILayout.TextField("Display Name", _newBldName);
                _newBldCategory = (BuildingCategory)EditorGUILayout.EnumPopup("Category", _newBldCategory);
                _newBldSprite   = (Sprite)EditorGUILayout.ObjectField("Sprite", _newBldSprite, typeof(Sprite), false);
                Rect newBldSpriteRect = GUILayoutUtility.GetLastRect();
                if (SpriteImportDragDropPolicy.HandleDrop(newBldSpriteRect, ref _newBldSprite, "RegistryHub.NewBuildingSprite"))
                    Repaint();
                SpriteImportDragDropPolicy.EnsureAllowedSprite(ref _newBldSprite, "RegistryHub.NewBuildingSprite");
                _newBldPrefab   = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newBldPrefab, typeof(GameObject), false);
                if (!_newBldPrefab && !_newBldSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);

                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "Модулі будівлі налаштовуються після створення через inline-редактор.",
                    MessageType.Info);

                EditorGUILayout.Space(4);
                EditorGUI.BeginDisabledGroup(RegistryEditorStyles.ValidateIdFull(_newBldId, bldArr, "Id") != null);
                if (GUILayout.Button("\u2713 Створити будівлю", RegistryEditorStyles.CreateButton)) CreateBld();
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateBld()
        {
            string id = _newBldId.Trim();
            if (!ValidateBeforeAdd(id)) return;

            var blds = _bldSO.FindProperty("Buildings");
            if (ContainsId(blds, "Id", id)) { Err($"Building ID '{id}' вже існує."); return; }

            GameObject prefab = ResolvePrefab(id, _newBldPrefab, _newBldSprite, BuildingPrefabFolder);

            int idx = blds.arraySize;
            blds.InsertArrayElementAtIndex(idx);
            var el = blds.GetArrayElementAtIndex(idx);
            el.FindPropertyRelative("Id").stringValue = id;
            el.FindPropertyRelative("DisplayName").stringValue = _newBldName;
            el.FindPropertyRelative("Category").enumValueIndex = (int)_newBldCategory;
            el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            _bldSO.ApplyModifiedProperties();

            SaveAndNotify($"Будівлю '{id}' створено");
            _newBldId = ""; _newBldName = ""; _newBldSprite = null; _newBldPrefab = null;
        }

        private void PopulateBuildingRegistryFromHub()
        {
            if (_bldReg == null)
                return;

            bool confirmed = EditorUtility.DisplayDialog(
                "Наповнити реєстр будівель",
                "Буде додано або оновлено стандартний набір будівель у поточному BuildingRegistrySO.\n\nПродовжити?",
                "Так, наповнити",
                "Скасувати");

            if (!confirmed)
                return;

            BuildingRegistryPopulator.PopulateAndSave(_bldReg);
            RefreshSOs();
            Info($"Реєстр будівель оновлено. Записів: {_bldReg.Buildings?.Length ?? 0}.");
        }

        private void DrawBuildingInlineEditBox(SerializedProperty el, int index)
        {
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(el.FindPropertyRelative("Id"), new GUIContent("ID"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("DisplayName"), new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("Category"), new GUIContent("Category"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("Prefab"), new GUIContent("Prefab"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Вартість будівництва", EditorStyles.boldLabel);
            BuildingConstructionCostEditorShared.DrawCostList(
                el.FindPropertyRelative("ConstructionCost"),
                "Додати ресурс для будівництва");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Правила розміщення", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(el.FindPropertyRelative("UseCustomTownHallRules"), new GUIContent("Кастомні правила ратуші"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("RequireTownHallInRange"), new GUIContent("Потребує ратушу в радіусі"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("BlockIfTownHallAlreadyInRange"), new GUIContent("Блокувати другу ратушу"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("TownHallProximityRadiusOverride"), new GUIContent("Override радіусу ратуші"));

            DrawBuildingModulesSection(el);

            if (_bldReg != null && _bldReg.Buildings != null && index < _bldReg.Buildings.Length)
            {
                var issues = BuildingModuleValidation.Validate(_bldReg.Buildings[index]);
                DrawModuleIssues(issues, el);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawBuildingModulesSection(SerializedProperty buildingProperty)
        {
            var modulesProp = buildingProperty.FindPropertyRelative("Modules");
            if (modulesProp == null || !modulesProp.isArray)
                return;
            BuildingModuleEditorShared.DrawModulesSection(modulesProp, RegistryEditorStyles.SectionBox, () => _bldSO.ApplyModifiedProperties());
        }

        private void DrawModuleIssues(IReadOnlyList<BuildingValidationIssue> issues, SerializedProperty buildingProperty)
        {
            BuildingModuleEditorShared.DrawValidationIssues(issues, buildingProperty, () => _bldSO.ApplyModifiedProperties());
        }

        private void ValidateAllBuildingModules()
        {
            if (_bldReg == null || _bldReg.Buildings == null)
                return;

            BuildingModuleEditorShared.CountValidationIssues(_bldReg.Buildings, out int errors, out int warnings);

            if (errors > 0)
                Err($"Валідація модулів: errors={errors}, warnings={warnings}.");
            else
                Info($"Валідація модулів пройдена: warnings={warnings}.");
        }

        private bool ValidateAndSaveBuildingRegistry()
        {
            if (_bldReg == null || _bldReg.Buildings == null)
            {
                AssetDatabase.SaveAssets();
                return true;
            }

            var failed = BuildingModuleEditorShared.CollectInvalidBuildingIds(_bldReg.Buildings);

            if (failed.Count > 0)
            {
                Err("Збереження скасовано: є критичні помилки модулів у будівлях:\n- " + string.Join("\n- ", failed));
                return false;
            }

            AssetDatabase.SaveAssets();
            return true;
        }

        // ══════════════════════════════════════════════════════
        //  WALLS TAB
        // ══════════════════════════════════════════════════════

        private static readonly string[] WallVariantFieldNames =
        {
            "HorizontalPrefab", "VerticalPrefab",
            "CornerNorthEastPrefab", "CornerNorthWestPrefab",
            "CornerSouthEastPrefab", "CornerSouthWestPrefab",
            "GatePrefab"
        };

        private static readonly string[] WallVariantLabels =
        {
            "Горизонтальна ←→", "Вертикальна ↑↓",
            "Кут NE ↑→", "Кут NW ↑←",
            "Кут SE ↓→", "Кут SW ↓←",
            "Ворота"
        };

        private void DrawWallsTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Wall Collections — колекції стін", new Color(0.65f, 0.55f, 0.30f));

            EditorGUI.BeginChangeCheck();
            _bldReg = (BuildingRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _bldReg, typeof(BuildingRegistrySO), false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveRegistryPreference(PrefKeyBuildingRegistryGuid, _bldReg);
                RefreshSOs();
            }
            if (_bldReg && _bldSO?.targetObject != _bldReg) RefreshSOs();
            if (!_bldReg) { EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO або натисніть ⟳.", MessageType.Warning); return; }

            DrawAssetPath(_bldReg);
            RegistryEditorStyles.DrawSeparator();

            var collections = _bldSO.FindProperty("WallCollections");
            var buildingIds = CollectIds(_bldSO.FindProperty("Buildings"), "Id");
            int count = collections?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Колекції стін ({count})", RegistryEditorStyles.SubHeader);

            var wallSelectionScope = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var scopeEl = collections.GetArrayElementAtIndex(i);
                string scopeCollectionId = scopeEl.FindPropertyRelative("CollectionId")?.stringValue;
                string scopeWallId = scopeEl.FindPropertyRelative("WallBuildingId")?.stringValue ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(scopeCollectionId) && (MatchesFilter(scopeCollectionId) || MatchesFilter(scopeWallId)))
                    wallSelectionScope.Add(scopeCollectionId);
            }

            if (count == 0)
                EditorGUILayout.LabelField("Колекцій стін немає. Додайте першу нижче.", RegistryEditorStyles.CenteredMini);

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = collections.GetArrayElementAtIndex(i);
                string collId  = el.FindPropertyRelative("CollectionId")?.stringValue ?? "?";
                string wallId  = el.FindPropertyRelative("WallBuildingId")?.stringValue ?? "";
                string gateId  = el.FindPropertyRelative("GateBuildingId")?.stringValue ?? "";

                if (!MatchesFilter(collId) && !MatchesFilter(wallId)) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);

                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedWall == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedWall = isOpen ? -1 : i;
                DrawSelectionMarker(Tab.Walls, collId);
                DrawIdLabel(collId, 150);
                EditorGUILayout.LabelField($"wall:{wallId} gate:{gateId}", EditorStyles.miniLabel);

                int missing = CountWallMissing(el);
                if (missing == 0)
                {
                    Color prev = GUI.color;
                    GUI.color = RegistryEditorStyles.SuccessCol;
                    GUILayout.Label("✓", GUILayout.Width(18));
                    GUI.color = prev;
                }
                else
                {
                    Color prev = GUI.color;
                    GUI.color = new Color(1f, 0.6f, 0f);
                    GUILayout.Label($"⚠{missing}", GUILayout.Width(26));
                    GUI.color = prev;
                }

                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                if (isOpen)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(el.FindPropertyRelative("CollectionId"), new GUIContent("ID Колекції"));
                    DrawIdPopup(el.FindPropertyRelative("WallBuildingId"), new GUIContent("ID Стіни"), buildingIds);
                    DrawIdPopup(el.FindPropertyRelative("GateBuildingId"), new GUIContent("ID Воріт"), buildingIds);
                    EditorGUILayout.Space(4);

                    for (int v = 0; v < WallVariantFieldNames.Length; v++)
                    {
                        var prop = el.FindPropertyRelative(WallVariantFieldNames[v]);
                        if (prop == null) continue;

                        bool prefabMissing = prop.objectReferenceValue == null;
                        Color prev = GUI.color;
                        if (prefabMissing) GUI.color = new Color(1f, 0.72f, 0.72f);
                        EditorGUILayout.PropertyField(prop, new GUIContent(WallVariantLabels[v]));
                        GUI.color = prev;
                    }

                    EditorGUILayout.Space(2);
                    WallsAutoSyncBuildingDefinitions(el);
                    EditorGUI.indentLevel--;
                    _bldSO.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();

                Rect wallRowRect = GUILayoutUtility.GetLastRect();
                HandleRowSelectionMouseUp(wallRowRect, Tab.Walls, collId, wallSelectionScope);
            }

            if (removeIdx >= 0 && collections != null)
            {
                string name = collections.GetArrayElementAtIndex(removeIdx).FindPropertyRelative("CollectionId")?.stringValue ?? "?";
                if (EditorUtility.DisplayDialog("Видалити колекцію", $"Видалити '{name}'?", "Так", "Ні"))
                {
                    collections.DeleteArrayElementAtIndex(removeIdx);
                    _bldSO.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
            }

            RegistryEditorStyles.DrawSeparator();

            // Створити нову колекцію
            if (GUILayout.Button("+ Додати колекцію стін", RegistryEditorStyles.CreateButton))
            {
                if (collections != null)
                {
                    int idx = collections.arraySize;
                    collections.arraySize++;
                    var newEl = collections.GetArrayElementAtIndex(idx);
                    newEl.FindPropertyRelative("CollectionId").stringValue = $"wall-collection-{idx}";
                    newEl.FindPropertyRelative("WallBuildingId").stringValue = "wall";
                    newEl.FindPropertyRelative("GateBuildingId").stringValue = "gate";
                    _bldSO.ApplyModifiedProperties();
                    _expandedWall = idx;
                    SaveAndNotify("Колекцію стін додано");
                }
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Відкрити повний редактор стін (Wall Registry Editor)"))
            {
                // WallRegistryWindow знаходиться в Kruty1918.Moyva.Construction.Editor
                var type = System.AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                    .FirstOrDefault(t => t.Name == "WallRegistryWindow" && typeof(EditorWindow).IsAssignableFrom(t));
                if (type != null)
                    EditorWindow.GetWindow(type, false, "Wall Registry");
                else
                    Debug.LogWarning("[RegistryHub] WallRegistryWindow не знайдено в жодній збірці.");
            }
        }

        private static int CountWallMissing(SerializedProperty el)
        {
            int missing = 0;
            foreach (var field in WallVariantFieldNames)
            {
                var p = el.FindPropertyRelative(field);
                if (p != null && p.objectReferenceValue == null) missing++;
            }
            return missing;
        }

        /// <summary>Авто-синхронізує BuildingDefinition для wall/gate при кожному перемальовуванні.</summary>
        private void WallsAutoSyncBuildingDefinitions(SerializedProperty wallCol)
        {
            var buildings = _bldSO.FindProperty("Buildings");
            if (buildings == null) return;

            string wallId = wallCol.FindPropertyRelative("WallBuildingId")?.stringValue;
            string gateId = wallCol.FindPropertyRelative("GateBuildingId")?.stringValue;
            if (string.IsNullOrWhiteSpace(wallId) && string.IsNullOrWhiteSpace(gateId)) return;

            var horizontalPrefab = wallCol.FindPropertyRelative("HorizontalPrefab")?.objectReferenceValue as GameObject;
            var gatePrefab = wallCol.FindPropertyRelative("GatePrefab")?.objectReferenceValue as GameObject;

            bool changed = false;
            changed |= AutoSyncOneBuildingDef(buildings, wallId, "Стіна", horizontalPrefab, BuildingCategory.Walls);
            changed |= AutoSyncOneBuildingDef(buildings, gateId, "Ворота", gatePrefab, BuildingCategory.Walls);

            if (changed)
                _bldSO.ApplyModifiedProperties();
        }

        /// <summary>Гарантує наявність і коректність BuildingDefinition для одного ID. Повертає true якщо щось змінено.</summary>
        private static bool AutoSyncOneBuildingDef(SerializedProperty buildings, string id, string displayName,
            GameObject prefab, BuildingCategory category)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            SerializedProperty target = null;
            for (int i = 0; i < buildings.arraySize; i++)
            {
                var item = buildings.GetArrayElementAtIndex(i);
                if (item.FindPropertyRelative("Id")?.stringValue == id)
                {
                    target = item;
                    break;
                }
            }

            bool created = false;
            if (target == null)
            {
                buildings.arraySize++;
                target = buildings.GetArrayElementAtIndex(buildings.arraySize - 1);
                target.FindPropertyRelative("Id").stringValue = id;
                target.FindPropertyRelative("DisplayName").stringValue = displayName;
                created = true;
            }

            bool changed = created;

            var catProp = target.FindPropertyRelative("Category");
            if (catProp != null && catProp.enumValueIndex != (int)category)
            {
                catProp.enumValueIndex = (int)category;
                changed = true;
            }

            var nameProp = target.FindPropertyRelative("DisplayName");
            if (nameProp != null && string.IsNullOrWhiteSpace(nameProp.stringValue))
            {
                nameProp.stringValue = displayName;
                changed = true;
            }

            if (prefab != null)
            {
                var prefabProp = target.FindPropertyRelative("Prefab");
                if (prefabProp != null && prefabProp.objectReferenceValue != prefab)
                {
                    prefabProp.objectReferenceValue = prefab;
                    changed = true;
                }

                // Авто-іконка зі спрайту prefab
                var iconProp = target.FindPropertyRelative("Icon");
                if (iconProp != null)
                {
                    if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(prefab, out var sprite, out _)
                        && iconProp.objectReferenceValue != sprite)
                    {
                        iconProp.objectReferenceValue = sprite;
                        changed = true;
                    }
                }
            }

            return changed;
        }

        // ══════════════════════════════════════════════════════
        //  RESOURCES TAB
        // ══════════════════════════════════════════════════════

        private void DrawResourcesTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Ресурси — ScriptableObject ассети генератора", RegistryEditorStyles.Accent);
            EditorGUILayout.Space(4);

            DrawEconomyResourcesSection();
            RegistryEditorStyles.DrawSeparator();

            DrawSOSection<DataNoiseSettings>("Noise Settings");
            DrawSOSection<HeightMapSettings>("Height Map Settings");
            DrawSOSection<DataBiomesSettings>("Biomes Settings");
            DrawSOSection<ObjectConnectionRulesSO>("Object Connection Rules");
            DrawSOSection<RiverDataConfig>("River Data Config");
            DrawSOSection<MapObjectTerrainConfig>("Map Object Terrain Config");
            DrawSOSection<GenerationRules>("Generation Rules");
            DrawSOSection<WFCDataSettings>("WFC Data Settings");
        }

        private void DrawEconomyResourcesSection()
        {
            var resources = EconomyResourceEditorShared.LoadResources();
            EditorGUILayout.LabelField($"Economy Resources ({resources.Count})", RegistryEditorStyles.SubHeader);

            EditorGUI.BeginChangeCheck();
            _economyDb = (EconomyDatabaseSO)EditorGUILayout.ObjectField(
                "Economy Database",
                _economyDb,
                typeof(EconomyDatabaseSO),
                false);
            if (EditorGUI.EndChangeCheck())
                MoyvaProjectEditorContext.Set(_economyDb);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Створити ресурс", RegistryEditorStyles.CreateButton, GUILayout.Width(160f)))
            {
                var created = EconomyResourceEditorShared.CreateResourceAssetInteractive("EconomyResourceDefinition");
                if (created != null)
                {
                    if (_economyDb != null)
                        EconomyResourceEditorShared.AddResourceToDatabase(_economyDb, created);

                    Selection.activeObject = created;
                    EditorGUIUtility.PingObject(created);
                }
            }

            if (GUILayout.Button("Додати в БД", GUILayout.Width(170f)))
            {
                ShowAddResourceToDatabaseMenu();
            }
            EditorGUILayout.EndHorizontal();

            if (resources.Count == 0)
            {
                EditorGUILayout.LabelField("Ресурсів не знайдено.", RegistryEditorStyles.CenteredMini);
                return;
            }

            for (int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                if (resource == null) continue;

                string path = AssetDatabase.GetAssetPath(resource);
                string id = string.IsNullOrWhiteSpace(resource.Id) ? "<empty-id>" : resource.Id;
                if (!MatchesFilter(id) && !MatchesFilter(resource.name) && !MatchesFilter(path)) continue;

                string guid = AssetDatabase.AssetPathToGUID(path);
                string key = $"eco:{guid}";
                bool isOpen = _expandedResources.Contains(key);
                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;

                EditorGUILayout.BeginVertical(style);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(isOpen ? "▼" : "▶", EditorStyles.miniLabel, GUILayout.Width(16)))
                {
                    if (isOpen) _expandedResources.Remove(key);
                    else _expandedResources.Add(key);
                }

                EditorGUILayout.LabelField(id, RegistryEditorStyles.EntryTitle, GUILayout.Width(170));
                EditorGUILayout.LabelField(path, EditorStyles.miniLabel);

                if (_economyDb != null)
                {
                    bool linked = EconomyResourceEditorShared.HasResourceInDatabase(_economyDb, resource);
                    if (!linked)
                    {
                        if (GUILayout.Button("+БД", EditorStyles.miniButton, GUILayout.Width(42)))
                            EconomyResourceEditorShared.AddResourceToDatabase(_economyDb, resource);
                    }
                    else
                    {
                        if (GUILayout.Button("-БД", EditorStyles.miniButton, GUILayout.Width(42)))
                            EconomyResourceEditorShared.RemoveResourceFromDatabase(_economyDb, resource);
                    }
                }

                if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(36)))
                    EditorGUIUtility.PingObject(resource);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(48)))
                    Selection.activeObject = resource;
                if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(52)))
                {
                    if (EditorUtility.DisplayDialog(
                            "Видалити ресурс",
                            "Ресурс буде видалено з економічного каталогу та .asset файл буде видалено з проєкту. Продовжити?",
                            "Видалити",
                            "Скасувати"))
                    {
                        if (!EconomyResourceEditorShared.DeleteResourceAsset(_economyDb, resource, out var deleteError))
                            ShowNotification(new GUIContent($"Помилка видалення ресурсу: {deleteError}"));
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (isOpen)
                {
                    if (!_resourceEditors.TryGetValue(key, out UnityEditor.Editor editor) || editor == null || editor.target != resource)
                    {
                        if (editor != null) DestroyImmediate(editor);
                        editor = UnityEditor.Editor.CreateEditor(resource);
                        _resourceEditors[key] = editor;
                    }

                    EditorGUI.indentLevel++;
                    editor.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void ShowAddResourceToDatabaseMenu()
        {
            if (_economyDb == null)
            {
                ShowNotification(new GUIContent("Спочатку оберіть Economy Database"));
                return;
            }

            var available = EconomyResourceEditorShared.LoadResourcesNotInDatabase(_economyDb);
            var menu = new GenericMenu();
            if (available.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("Немає ресурсів для додавання"));
                menu.ShowAsContext();
                return;
            }

            foreach (var resource in available)
            {
                if (resource == null) continue;

                string id = string.IsNullOrWhiteSpace(resource.Id) ? "<empty-id>" : resource.Id;
                string label = BuildResourceMenuLabel(resource, id);

                menu.AddItem(new GUIContent(label), false, () =>
                {
                    EconomyResourceEditorShared.AddResourceToDatabase(_economyDb, resource);
                    Selection.activeObject = resource;
                    EditorGUIUtility.PingObject(resource);
                });
            }

            menu.ShowAsContext();
        }

        private static string BuildResourceMenuLabel(EconomyResourceDefinition resource, string id)
        {
            string path = AssetDatabase.GetAssetPath(resource);
            if (string.IsNullOrWhiteSpace(path))
                return id;

            string folder = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(path));
            if (string.IsNullOrWhiteSpace(folder))
                return id;

            return $"{id} [{folder}]";
        }

        private enum AssetHealthIssueKind
        {
            BrokenReference,
            NullReference,
            DuplicateId,
        }

        private readonly struct AssetHealthIssue
        {
            public AssetHealthIssue(AssetHealthIssueKind kind, MessageType severity, string scope, string message, UnityEngine.Object context)
            {
                Kind = kind;
                Severity = severity;
                Scope = scope;
                Message = message;
                Context = context;
            }

            public AssetHealthIssueKind Kind { get; }
            public MessageType Severity { get; }
            public string Scope { get; }
            public string Message { get; }
            public UnityEngine.Object Context { get; }
        }

        // ══════════════════════════════════════════════════════
        //  ASSET HEALTH TAB
        // ══════════════════════════════════════════════════════

        private void DrawAssetHealthTab()
        {
            TryRunQueuedAssetHealthScan();

            RegistryEditorStyles.DrawColoredHeader("  Asset health — перевірка посилань та ID", new Color(0.78f, 0.34f, 0.22f));
            EditorWindowSharedUI.DrawWarning("Централізована перевірка на: биті посилання, null refs і дублікати ID у ключових реєстрах.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(EditorTooltipStandard.Content("Сканувати", "Запускає перевірку реєстрів на проблеми ассетів.", "Допомагає попередити runtime-падіння через некоректні посилання."), GUILayout.Width(120f)))
                    RequestAssetHealthScan(immediate: true);

                if (GUILayout.Button(EditorTooltipStandard.Content("Очистити", "Очищає список знайдених проблем.", "Дозволяє почати новий цикл перевірки без старих результатів."), GUILayout.Width(100f)))
                    _assetHealthIssues.Clear();

                GUILayout.Space(8f);
                _assetHealthOnlyErrors = GUILayout.Toggle(_assetHealthOnlyErrors, new GUIContent("Лише помилки", EditorTooltipStandard.Build("Фільтрує список і залишає лише error-рівень.", "Фокусує увагу на критичних проблемах перед запуском гри.")), GUILayout.Width(120f));
                GUILayout.Label("Пошук", GUILayout.Width(42f));
                _assetHealthSearch = EditorGUILayout.TextField(_assetHealthSearch);
            }

            int errors = _assetHealthIssues.Count(i => i.Severity == MessageType.Error);
            int warnings = _assetHealthIssues.Count(i => i.Severity == MessageType.Warning);
            int brokenRefs = _assetHealthIssues.Count(i => i.Kind == AssetHealthIssueKind.BrokenReference);
            int nullRefs = _assetHealthIssues.Count(i => i.Kind == AssetHealthIssueKind.NullReference);
            int duplicateIds = _assetHealthIssues.Count(i => i.Kind == AssetHealthIssueKind.DuplicateId);
            string scanStamp = _assetHealthLastScanUtc == default
                ? "ще не сканували"
                : $"{_assetHealthLastScanUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

            EditorGUILayout.LabelField(
                $"Останній скан: {scanStamp} | Помилки: {errors} | Попередження: {warnings} | Broken refs: {brokenRefs} | Null refs: {nullRefs} | Duplicate id: {duplicateIds}",
                EditorStyles.miniLabel);

            RegistryEditorStyles.DrawSeparator();

            _assetHealthScroll = EditorGUILayout.BeginScrollView(_assetHealthScroll);
            if (_assetHealthIssues.Count == 0)
            {
                EditorWindowSharedUI.DrawWarning("Натисніть 'Сканувати', щоб побачити проблеми ассетів.", MessageType.None);
            }
            else
            {
                int shown = 0;
                for (int i = 0; i < _assetHealthIssues.Count; i++)
                {
                    var issue = _assetHealthIssues[i];
                    if (_assetHealthOnlyErrors && issue.Severity != MessageType.Error)
                        continue;

                    if (!string.IsNullOrWhiteSpace(_assetHealthSearch) &&
                        issue.Message.IndexOf(_assetHealthSearch, StringComparison.OrdinalIgnoreCase) < 0 &&
                        issue.Scope.IndexOf(_assetHealthSearch, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    shown++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox($"[{issue.Scope}] {issue.Message}", issue.Severity);
                    if (GUILayout.Button(
                            EditorTooltipStandard.Content("Quick Fix", "Запускає автовиправлення типових проблем для цього scope.", "Зменшує час на ручне відновлення реєстрів."),
                            GUILayout.Width(84f),
                            GUILayout.Height(38f)))
                        QuickFixAssetHealthIssue(issue);

                    if (issue.Context != null && GUILayout.Button(EditorTooltipStandard.Content("Ping", "Показує пов'язаний ассет у Project.", "Прискорює виправлення проблеми у відповідному реєстрі."), GUILayout.Width(56f), GUILayout.Height(38f)))
                        EditorGUIUtility.PingObject(issue.Context);
                    EditorGUILayout.EndHorizontal();
                }

                if (shown == 0)
                    EditorGUILayout.HelpBox("За поточними фільтрами проблем не знайдено.", MessageType.None);
            }
            EditorGUILayout.EndScrollView();
        }

        private void RequestAssetHealthScan(bool immediate)
        {
            if (immediate || _livePreviewThrottle.ShouldRunCostlyTick())
            {
                _assetHealthScanQueued = false;
                RunAssetHealthScan();
                return;
            }

            _assetHealthScanQueued = true;
        }

        private void TryRunQueuedAssetHealthScan()
        {
            if (!_assetHealthScanQueued)
                return;

            if (!_livePreviewThrottle.ShouldRunCostlyTick())
                return;

            _assetHealthScanQueued = false;
            RunAssetHealthScan();
        }

        private void RunAssetHealthScan()
        {
            AutoFind();
            _assetHealthIssues.Clear();

            ScanTileRegistry();
            ScanMapObjectRegistry();
            ScanUnitRegistry();
            ScanBuildingRegistry();
            ScanEconomyDatabase();

            _assetHealthLastScanUtc = DateTime.UtcNow;
            _livePreviewThrottle.TryRepaint(this, force: true);
            ShowNotification(new GUIContent($"Asset health: знайдено {_assetHealthIssues.Count} проблем."));
        }

        private void QuickFixAssetHealthIssue(AssetHealthIssue issue)
        {
            AutoFind();

            int fixedCount = 0;
            RunUndoSafeBulkEdit(
                $"Registry Hub: asset health quick-fix ({issue.Scope})",
                () =>
                {
                    switch (issue.Scope)
                    {
                        case "Tiles":
                            if (issue.Kind == AssetHealthIssueKind.DuplicateId)
                                fixedCount += AssetHealthFixDuplicateIds(_tileSO, "_definitions", "_id", "tile");
                            else if (issue.Kind == AssetHealthIssueKind.NullReference)
                                fixedCount += AssetHealthFixMissingPrefabs(_tileSO, "_definitions", "_id", "_visualPrefab", TilePrefabFolder);
                            else if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                fixedCount += AssetHealthClearBrokenReferences(_tileSO);
                            break;

                        case "MapObjects":
                            if (issue.Kind == AssetHealthIssueKind.DuplicateId)
                                fixedCount += AssetHealthFixDuplicateIds(_objSO, "_definitions", "_id", "object");
                            else if (issue.Kind == AssetHealthIssueKind.NullReference)
                                fixedCount += AssetHealthFixMissingPrefabs(_objSO, "_definitions", "_id", "_visualPrefab", ObjectPrefabFolder);
                            else if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                fixedCount += AssetHealthClearBrokenReferences(_objSO);
                            break;

                        case "Units":
                            if (issue.Kind == AssetHealthIssueKind.DuplicateId)
                                fixedCount += AssetHealthFixDuplicateIds(_unitSO, "Configs", "TypeId", "unit");
                            else if (issue.Kind == AssetHealthIssueKind.NullReference)
                                fixedCount += AssetHealthFixMissingPrefabs(_unitSO, "Configs", "TypeId", "Prefab", UnitPrefabFolder);
                            else if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                fixedCount += AssetHealthClearBrokenReferences(_unitSO);
                            break;

                        case "Buildings":
                            if (issue.Kind == AssetHealthIssueKind.DuplicateId)
                                fixedCount += AssetHealthFixDuplicateIds(_bldSO, "Buildings", "Id", "building");
                            else if (issue.Kind == AssetHealthIssueKind.NullReference)
                                fixedCount += AssetHealthFixMissingPrefabs(_bldSO, "Buildings", "Id", "Prefab", BuildingPrefabFolder);
                            else if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                fixedCount += AssetHealthClearBrokenReferences(_bldSO);
                            break;

                        case "Walls":
                            if (issue.Kind == AssetHealthIssueKind.DuplicateId)
                                fixedCount += AssetHealthFixDuplicateIds(_bldSO, "WallCollections", "CollectionId", "wall-collection");
                            else if (issue.Kind == AssetHealthIssueKind.NullReference)
                                fixedCount += AssetHealthFixMissingWallPrefabs();
                            else if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                fixedCount += AssetHealthClearBrokenReferences(_bldSO);
                            break;

                        case "Economy":
                            if (_economyDb != null)
                            {
                                if (issue.Kind == AssetHealthIssueKind.DuplicateId || issue.Kind == AssetHealthIssueKind.NullReference)
                                    fixedCount += TryFixEconomyCommonIssues(_economyDb);

                                if (issue.Kind == AssetHealthIssueKind.BrokenReference)
                                    fixedCount += AssetHealthClearBrokenReferences(new SerializedObject(_economyDb));
                            }
                            break;
                    }
                },
                GetUndoTargetsForAssetHealthScope(issue.Scope));

            if (fixedCount > 0)
            {
                AssetDatabase.SaveAssets();
                RequestAssetHealthScan(immediate: false);
                ShowNotification(new GUIContent($"Quick-fix: виправлено {fixedCount} змін."));
            }
            else
            {
                ShowNotification(new GUIContent("Quick-fix: типових виправлень для цього кейсу не знайдено."));
            }
        }

        private int AssetHealthFixDuplicateIds(SerializedObject so, string arrayPath, string idPropertyName, string fallbackPrefix)
        {
            if (so == null || so.targetObject == null)
                return 0;

            so.Update();
            var array = so.FindProperty(arrayPath);
            if (array == null || !array.isArray)
                return 0;

            int changed = 0;
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative(idPropertyName);
                if (idProp == null)
                    continue;

                string baseId = (idProp.stringValue ?? string.Empty).Trim().Replace('_', '-').ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(baseId))
                    baseId = $"{fallbackPrefix}-{i + 1}";

                string candidate = baseId;
                int suffix = 2;
                while (used.Contains(candidate))
                    candidate = $"{baseId}-{suffix++}";

                if (!string.Equals(idProp.stringValue, candidate, StringComparison.Ordinal))
                {
                    idProp.stringValue = candidate;
                    changed++;
                }

                used.Add(candidate);
            }

            if (changed > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            }

            return changed;
        }

        private int AssetHealthFixMissingPrefabs(SerializedObject so, string arrayPath, string idPropertyName, string prefabPropertyName, string folder)
        {
            if (so == null || so.targetObject == null)
                return 0;

            so.Update();
            var array = so.FindProperty(arrayPath);
            if (array == null || !array.isArray)
                return 0;

            int changed = 0;
            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var prefabProp = element.FindPropertyRelative(prefabPropertyName);
                if (prefabProp == null || prefabProp.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (prefabProp.objectReferenceValue != null)
                    continue;

                string id = element.FindPropertyRelative(idPropertyName)?.stringValue;
                if (string.IsNullOrWhiteSpace(id))
                    id = $"{SanitizeFileName(arrayPath)}-{i + 1}";

                prefabProp.objectReferenceValue = ResolvePrefab(id, null, null, folder);
                changed++;
            }

            if (changed > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            }

            return changed;
        }

        private int AssetHealthFixMissingWallPrefabs()
        {
            if (_bldSO == null || _bldSO.targetObject == null)
                return 0;

            _bldSO.Update();
            var walls = _bldSO.FindProperty("WallCollections");
            if (walls == null || !walls.isArray)
                return 0;

            string[] fields =
            {
                "HorizontalPrefab",
                "VerticalPrefab",
                "CornerNorthEastPrefab",
                "CornerNorthWestPrefab",
                "CornerSouthEastPrefab",
                "CornerSouthWestPrefab",
                "GatePrefab",
            };

            int changed = 0;
            for (int i = 0; i < walls.arraySize; i++)
            {
                var wall = walls.GetArrayElementAtIndex(i);
                string collectionId = wall.FindPropertyRelative("CollectionId")?.stringValue;
                if (string.IsNullOrWhiteSpace(collectionId))
                    collectionId = $"wall-collection-{i + 1}";

                for (int f = 0; f < fields.Length; f++)
                {
                    string field = fields[f];
                    var prefabProp = wall.FindPropertyRelative(field);
                    if (prefabProp == null || prefabProp.propertyType != SerializedPropertyType.ObjectReference)
                        continue;

                    if (prefabProp.objectReferenceValue != null)
                        continue;

                    prefabProp.objectReferenceValue = CreateEmptyPrefab($"{collectionId}-{field.ToLowerInvariant()}", BuildingPrefabFolder);
                    changed++;
                }
            }

            if (changed > 0)
            {
                _bldSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(_bldSO.targetObject);
            }

            return changed;
        }

        private int AssetHealthClearBrokenReferences(SerializedObject so)
        {
            if (so == null || so.targetObject == null)
                return 0;

            so.Update();
            int changed = 0;
            var iterator = so.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == "m_Script")
                    continue;

                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (iterator.objectReferenceValue == null && iterator.objectReferenceInstanceIDValue != 0)
                {
                    iterator.objectReferenceValue = null;
                    changed++;
                }
            }

            if (changed > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            }

            return changed;
        }

        private void ScanTileRegistry()
        {
            if (_tileSO == null || _tileReg == null)
                return;

            var defs = _tileSO.FindProperty("_definitions");
            AppendDuplicateIdIssues(defs, "_id", "Tiles", _tileReg);
            AppendRequiredObjectReferenceIssues(defs, "_visualPrefab", "Tiles", _tileReg);
            AppendBrokenReferenceIssues(_tileSO, "Tiles", _tileReg);
        }

        private void ScanMapObjectRegistry()
        {
            if (_objSO == null || _objReg == null)
                return;

            var defs = _objSO.FindProperty("_definitions");
            AppendDuplicateIdIssues(defs, "_id", "MapObjects", _objReg);
            AppendRequiredObjectReferenceIssues(defs, "_visualPrefab", "MapObjects", _objReg);
            AppendBrokenReferenceIssues(_objSO, "MapObjects", _objReg);
        }

        private void ScanUnitRegistry()
        {
            if (_unitSO == null || _unitReg == null)
                return;

            var defs = _unitSO.FindProperty("Configs");
            AppendDuplicateIdIssues(defs, "TypeId", "Units", _unitReg);
            AppendRequiredObjectReferenceIssues(defs, "Prefab", "Units", _unitReg);
            AppendBrokenReferenceIssues(_unitSO, "Units", _unitReg);
        }

        private void ScanBuildingRegistry()
        {
            if (_bldSO == null || _bldReg == null)
                return;

            var buildings = _bldSO.FindProperty("Buildings");
            AppendDuplicateIdIssues(buildings, "Id", "Buildings", _bldReg);
            AppendRequiredObjectReferenceIssues(buildings, "Prefab", "Buildings", _bldReg);

            var walls = _bldSO.FindProperty("WallCollections");
            AppendDuplicateIdIssues(walls, "CollectionId", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "HorizontalPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "VerticalPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "CornerNorthEastPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "CornerNorthWestPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "CornerSouthEastPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "CornerSouthWestPrefab", "Walls", _bldReg);
            AppendRequiredObjectReferenceIssues(walls, "GatePrefab", "Walls", _bldReg);

            AppendBrokenReferenceIssues(_bldSO, "Buildings/Walls", _bldReg);
        }

        private void ScanEconomyDatabase()
        {
            if (_economyDb == null)
                return;

            var validationIssues = ValidateEconomyDatabase(_economyDb, _bldReg);
            for (int i = 0; i < validationIssues.Count; i++)
            {
                var issue = validationIssues[i];
                if (string.IsNullOrWhiteSpace(issue.Message))
                    continue;

                string message = issue.Message;
                string lower = message.ToLowerInvariant();
                if (lower.Contains("duplicate") || lower.Contains("дублікат"))
                {
                    _assetHealthIssues.Add(new AssetHealthIssue(AssetHealthIssueKind.DuplicateId, MessageType.Error, "Economy", message, issue.Context != null ? issue.Context : _economyDb));
                }
                else if (lower.Contains("missing") || lower.Contains("unknown") || lower.Contains("not assigned") || lower.Contains("відсут") || lower.Contains("не признач"))
                {
                    var severity = issue.IsError ? MessageType.Error : MessageType.Warning;
                    _assetHealthIssues.Add(new AssetHealthIssue(AssetHealthIssueKind.NullReference, severity, "Economy", message, issue.Context != null ? issue.Context : _economyDb));
                }
            }

            AppendBrokenReferenceIssues(new SerializedObject(_economyDb), "Economy", _economyDb);
        }

        private void AppendDuplicateIdIssues(SerializedProperty array, string idPropertyName, string scope, UnityEngine.Object context)
        {
            if (array == null || !array.isArray)
                return;

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < array.arraySize; i++)
            {
                string id = array.GetArrayElementAtIndex(i).FindPropertyRelative(idPropertyName)?.stringValue?.Trim();
                if (string.IsNullOrWhiteSpace(id))
                {
                    _assetHealthIssues.Add(new AssetHealthIssue(
                        AssetHealthIssueKind.NullReference,
                        MessageType.Warning,
                        scope,
                        $"Елемент #{i + 1} має порожній ID '{idPropertyName}'.",
                        context));
                    continue;
                }

                counts[id] = counts.TryGetValue(id, out int c) ? c + 1 : 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    _assetHealthIssues.Add(new AssetHealthIssue(
                        AssetHealthIssueKind.DuplicateId,
                        MessageType.Error,
                        scope,
                        $"Дублікат ID '{pair.Key}' зустрічається {pair.Value} рази.",
                        context));
                }
            }
        }

        private static int TryFixEconomyCommonIssues(EconomyDatabaseSO database)
        {
            if (database == null)
                return 0;

            var serviceType = FindTypeByName("Kruty1918.Moyva.Economy.Editor.EconomyAutoFixService");
            var method = serviceType?.GetMethod("FixCommonIssues", new[] { typeof(EconomyDatabaseSO) });
            if (serviceType == null || method == null)
                return 0;

            try
            {
                object instance = Activator.CreateInstance(serviceType);
                object result = method.Invoke(instance, new object[] { database });
                return result is int fixedCount ? fixedCount : 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RegistryHub] Economy auto-fix unavailable: {ex.Message}");
                return 0;
            }
        }

        private static List<ReflectedEconomyValidationIssue> ValidateEconomyDatabase(EconomyDatabaseSO database, BuildingRegistrySO buildingRegistry)
        {
            var issues = new List<ReflectedEconomyValidationIssue>();
            if (database == null)
                return issues;

            var serviceType = FindTypeByName("Kruty1918.Moyva.Economy.Editor.EconomyValidationService");
            var method = serviceType?.GetMethod("Validate", new[] { typeof(EconomyDatabaseSO), typeof(BuildingRegistrySO) });
            if (serviceType == null || method == null)
                return issues;

            try
            {
                object instance = Activator.CreateInstance(serviceType);
                object result = method.Invoke(instance, new object[] { database, buildingRegistry });
                if (!(result is System.Collections.IEnumerable enumerable))
                    return issues;

                foreach (var item in enumerable)
                {
                    if (item == null)
                        continue;

                    var itemType = item.GetType();
                    string message = itemType.GetProperty("Message")?.GetValue(item) as string;
                    object severityValue = itemType.GetProperty("Severity")?.GetValue(item);
                    var context = itemType.GetProperty("Context")?.GetValue(item) as UnityEngine.Object;
                    bool isError = string.Equals(severityValue?.ToString(), "Error", StringComparison.Ordinal);
                    issues.Add(new ReflectedEconomyValidationIssue(message, isError, context));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RegistryHub] Economy validation unavailable: {ex.Message}");
            }

            return issues;
        }

        private static Type FindTypeByName(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(fullName, throwOnError: false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private readonly struct ReflectedEconomyValidationIssue
        {
            public ReflectedEconomyValidationIssue(string message, bool isError, UnityEngine.Object context)
            {
                Message = message ?? string.Empty;
                IsError = isError;
                Context = context;
            }

            public string Message { get; }
            public bool IsError { get; }
            public UnityEngine.Object Context { get; }
        }

        private void AppendRequiredObjectReferenceIssues(SerializedProperty array, string propertyName, string scope, UnityEngine.Object context)
        {
            if (array == null || !array.isArray)
                return;

            for (int i = 0; i < array.arraySize; i++)
            {
                var element = array.GetArrayElementAtIndex(i);
                var prop = element.FindPropertyRelative(propertyName);
                if (prop == null || prop.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (prop.objectReferenceValue == null)
                {
                    _assetHealthIssues.Add(new AssetHealthIssue(
                        AssetHealthIssueKind.NullReference,
                        MessageType.Error,
                        scope,
                        $"Елемент #{i + 1}: null reference у полі '{propertyName}'.",
                        context));
                }
            }
        }

        private void AppendBrokenReferenceIssues(SerializedObject serializedObject, string scope, UnityEngine.Object context)
        {
            if (serializedObject == null || serializedObject.targetObject == null)
                return;

            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyPath == "m_Script")
                    continue;

                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (iterator.objectReferenceValue == null && iterator.objectReferenceInstanceIDValue != 0)
                {
                    _assetHealthIssues.Add(new AssetHealthIssue(
                        AssetHealthIssueKind.BrokenReference,
                        MessageType.Error,
                        scope,
                        $"Бите посилання у '{iterator.propertyPath}'.",
                        context));
                }
            }
        }

        private void DrawSOSection<T>(string label) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            EditorGUILayout.LabelField($"{label} ({guids.Length})", RegistryEditorStyles.SubHeader);

            if (guids.Length == 0)
            {
                EditorGUILayout.LabelField("Не знайдено.", RegistryEditorStyles.CenteredMini);
            }
            else
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string guid = guids[i];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    if (asset == null) continue;
                    if (!MatchesFilter(asset.name) && !MatchesFilter(path)) continue;

                    bool isOpen = _expandedResources.Contains(guid);
                    GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                    EditorGUILayout.BeginVertical(style);

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    {
                        if (isOpen) _expandedResources.Remove(guid);
                        else _expandedResources.Add(guid);
                    }
                    EditorGUILayout.LabelField(asset.name, RegistryEditorStyles.EntryTitle, GUILayout.Width(180));
                    EditorGUILayout.LabelField(path, EditorStyles.miniLabel);
                    if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(36)))
                        EditorGUIUtility.PingObject(asset);
                    if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(48)))
                        Selection.activeObject = asset;
                    EditorGUILayout.EndHorizontal();

                    if (isOpen)
                    {
                        EditorGUILayout.Space(2);
                        if (!_resourceEditors.TryGetValue(guid, out UnityEditor.Editor editor) || editor == null || editor.target != asset)
                        {
                            if (editor != null) DestroyImmediate(editor);
                            editor = UnityEditor.Editor.CreateEditor(asset);
                            _resourceEditors[guid] = editor;
                        }
                        EditorGUI.indentLevel++;
                        editor.OnInspectorGUI();
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            RegistryEditorStyles.DrawSeparator();
        }

        // ══════════════════════════════════════════════════════
        //  СПІЛЬНІ ХЕЛПЕРИ
        // ══════════════════════════════════════════════════════

        private void DrawRegistryMultiSelectionToolbar()
        {
            if (!IsMultiSelectionTab(_tab))
                return;

            var selected = GetSelection(_tab);
            int selectedCount = selected.Count;
            if (selectedCount == 0)
                return;

            if (!TryGetRegistryArrayForTab(_tab, out var so, out var arr, out var idProp))
                return;

            EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Мультивибір: {selectedCount}", RegistryEditorStyles.SubHeader, GUILayout.Width(130f));
            if (GUILayout.Button("Скасувати вибір", EditorStyles.miniButton, GUILayout.Width(120f)))
            {
                selected.Clear();
                return;
            }
            if (GUILayout.Button("Копія", EditorStyles.miniButton, GUILayout.Width(70f)))
                DuplicateSelected(_tab, so, arr, idProp);
            Color prevColor = GUI.color;
            GUI.color = RegistryEditorStyles.ErrorCol;
            if (GUILayout.Button("Видалити", EditorStyles.miniButton, GUILayout.Width(80f)))
                DeleteSelected(_tab, so, arr, idProp);
            GUI.color = prevColor;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _bulkRenamePrefix = EditorGUILayout.TextField("Префікс", _bulkRenamePrefix, GUILayout.Width(220f));
            _bulkRenameStart = EditorGUILayout.IntField("Start", _bulkRenameStart, GUILayout.Width(130f));
            _bulkRenameStart = Mathf.Max(0, _bulkRenameStart);
            if (GUILayout.Button("Перейменувати", EditorStyles.miniButton, GUILayout.Width(110f)))
                RenameSelectedWithPrefix(_tab, so, arr, idProp, _bulkRenamePrefix, _bulkRenameStart);
            if (GUILayout.Button("Виправити ID", EditorStyles.miniButton, GUILayout.Width(100f)))
                FixSelectedIds(_tab, so, arr, idProp);
            if (GUILayout.Button("Перенумерувати", EditorStyles.miniButton, GUILayout.Width(110f)))
                RenumberSelected(_tab, so, arr, idProp, _bulkRenameStart);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        private bool IsMultiSelectionTab(Tab tab)
        {
            return tab == Tab.Tiles || tab == Tab.MapObjects || tab == Tab.Units || tab == Tab.Buildings || tab == Tab.Walls;
        }

        private HashSet<string> GetSelection(Tab tab)
        {
            if (!_selectedIdsByTab.TryGetValue(tab, out var set))
            {
                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _selectedIdsByTab[tab] = set;
            }

            return set;
        }

        private bool IsSelected(Tab tab, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            return GetSelection(tab).Contains(id);
        }

        private void DrawSelectionMarker(Tab tab, string id)
        {
            if (IsSelected(tab, id))
            {
                Color prev = GUI.color;
                GUI.color = RegistryEditorStyles.SuccessCol;
                GUILayout.Label("●", GUILayout.Width(12f));
                GUI.color = prev;
            }
            else
            {
                GUILayout.Space(12f);
            }
        }

        private static int FindIndexIgnoreCase(IReadOnlyList<string> list, string value)
        {
            if (list == null || list.Count == 0 || string.IsNullOrWhiteSpace(value))
                return -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private void HandleRowSelectionMouseUp(Rect rowRect, Tab tab, string id, IReadOnlyList<string> orderedIds)
        {
            if (string.IsNullOrWhiteSpace(id) || orderedIds == null || orderedIds.Count == 0)
                return;

            var evt = Event.current;
            if (evt.type != EventType.MouseUp || evt.button != 0)
                return;
            if (!rowRect.Contains(evt.mousePosition))
                return;

            bool isToggle = evt.control || evt.command;
            bool isRange = evt.shift;
            var selected = GetSelection(tab);

            if (isRange)
            {
                if (!_selectionAnchorByTab.TryGetValue(tab, out string anchorId) || string.IsNullOrWhiteSpace(anchorId))
                    anchorId = id;

                int anchorIndex = FindIndexIgnoreCase(orderedIds, anchorId);
                int currentIndex = FindIndexIgnoreCase(orderedIds, id);

                if (anchorIndex < 0 || currentIndex < 0)
                {
                    selected.Clear();
                    selected.Add(id);
                }
                else
                {
                    int min = Mathf.Min(anchorIndex, currentIndex);
                    int max = Mathf.Max(anchorIndex, currentIndex);

                    if (!isToggle)
                        selected.Clear();

                    for (int i = min; i <= max; i++)
                        selected.Add(orderedIds[i]);
                }
            }
            else if (isToggle)
            {
                if (!selected.Add(id))
                    selected.Remove(id);
                _selectionAnchorByTab[tab] = id;
            }
            else
            {
                selected.Clear();
                selected.Add(id);
                _selectionAnchorByTab[tab] = id;
            }

            Repaint();
            evt.Use();
        }

        private bool TryGetRegistryArrayForTab(Tab tab, out SerializedObject so, out SerializedProperty arr, out string idProp)
        {
            so = null;
            arr = null;
            idProp = null;

            switch (tab)
            {
                case Tab.Tiles:
                    so = _tileSO;
                    arr = _tileSO?.FindProperty("_definitions");
                    idProp = "_id";
                    return so != null && arr != null;
                case Tab.MapObjects:
                    so = _objSO;
                    arr = _objSO?.FindProperty("_definitions");
                    idProp = "_id";
                    return so != null && arr != null;
                case Tab.Units:
                    so = _unitSO;
                    arr = _unitSO?.FindProperty("Configs");
                    idProp = "TypeId";
                    return so != null && arr != null;
                case Tab.Buildings:
                    so = _bldSO;
                    arr = _bldSO?.FindProperty("Buildings");
                    idProp = "Id";
                    return so != null && arr != null;
                case Tab.Walls:
                    so = _bldSO;
                    arr = _bldSO?.FindProperty("WallCollections");
                    idProp = "CollectionId";
                    return so != null && arr != null;
                default:
                    return false;
            }
        }

        private void DeleteSelected(Tab tab, SerializedObject so, SerializedProperty arr, string idProp)
        {
            var selected = GetSelection(tab);
            if (selected.Count == 0)
                return;

            if (!EditorUtility.DisplayDialog("Видалити вибрані",
                    $"Видалити {selected.Count} записів з {TabLabels[(int)tab].Trim()}?", "Так", "Ні"))
                return;

            int removed = 0;
            RunUndoSafeBulkEdit(
                $"Registry Hub: delete selected ({TabLabels[(int)tab].Trim()})",
                () =>
                {
                    for (int i = arr.arraySize - 1; i >= 0; i--)
                    {
                        string id = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                        if (!string.IsNullOrWhiteSpace(id) && selected.Contains(id))
                        {
                            arr.DeleteArrayElementAtIndex(i);
                            removed++;
                        }
                    }

                    so.ApplyModifiedProperties();
                },
                so != null ? so.targetObject : null);

            selected.Clear();
            SaveAndNotify($"Видалено {removed} записів");
        }

        private void DuplicateSelected(Tab tab, SerializedObject so, SerializedProperty arr, string idProp)
        {
            var selectedIds = GetSelection(tab).ToList();
            if (selectedIds.Count == 0)
                return;

            int copied = 0;
            var newIds = new List<string>();

            RunUndoSafeBulkEdit(
                $"Registry Hub: duplicate selected ({TabLabels[(int)tab].Trim()})",
                () =>
                {
                    foreach (var id in selectedIds)
                    {
                        int sourceIndex = FindIndexById(arr, idProp, id);
                        if (sourceIndex < 0)
                            continue;

                        arr.InsertArrayElementAtIndex(sourceIndex);
                        var newElement = arr.GetArrayElementAtIndex(sourceIndex);
                        string copyId = MakeUniqueId(arr, idProp, NormalizeId($"{id}-copy", "item"), sourceIndex);
                        newElement.FindPropertyRelative(idProp).stringValue = copyId;
                        newIds.Add(copyId);
                        copied++;
                    }

                    so.ApplyModifiedProperties();
                },
                so != null ? so.targetObject : null);

            var selection = GetSelection(tab);
            selection.Clear();
            foreach (var id in newIds)
                selection.Add(id);
            SaveAndNotify($"Створено копій: {copied}");
        }

        private void RenameSelectedWithPrefix(Tab tab, SerializedObject so, SerializedProperty arr, string idProp, string prefix, int start)
        {
            var selected = GetSelection(tab);
            if (selected.Count == 0)
                return;

            string safePrefix = NormalizeId(prefix, "item");
            int counter = Mathf.Max(0, start);
            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var reserved = CollectNonSelectedIds(arr, idProp, selected);

            RunUndoSafeBulkEdit(
                $"Registry Hub: rename selected ({TabLabels[(int)tab].Trim()})",
                () =>
                {
                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        var idPropRef = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp);
                        if (idPropRef == null)
                            continue;

                        string oldId = idPropRef.stringValue;
                        if (string.IsNullOrWhiteSpace(oldId) || !selected.Contains(oldId))
                            continue;

                        string candidate = $"{safePrefix}-{counter}";
                        counter++;
                        string unique = MakeUniqueFromSet(candidate, reserved);
                        reserved.Add(unique);
                        idPropRef.stringValue = unique;
                        remap[oldId] = unique;
                    }

                    so.ApplyModifiedProperties();
                },
                so != null ? so.targetObject : null);

            RemapSelection(tab, remap);
            SaveAndNotify($"Перейменовано: {remap.Count}");
        }

        private void FixSelectedIds(Tab tab, SerializedObject so, SerializedProperty arr, string idProp)
        {
            var selected = GetSelection(tab);
            if (selected.Count == 0)
                return;

            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var reserved = CollectNonSelectedIds(arr, idProp, selected);

            RunUndoSafeBulkEdit(
                $"Registry Hub: fix selected ids ({TabLabels[(int)tab].Trim()})",
                () =>
                {
                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        var idPropRef = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp);
                        if (idPropRef == null)
                            continue;

                        string oldId = idPropRef.stringValue;
                        if (string.IsNullOrWhiteSpace(oldId) || !selected.Contains(oldId))
                            continue;

                        string normalized = NormalizeId(oldId, "item");
                        string unique = MakeUniqueFromSet(normalized, reserved);
                        reserved.Add(unique);
                        idPropRef.stringValue = unique;
                        remap[oldId] = unique;
                    }

                    so.ApplyModifiedProperties();
                },
                so != null ? so.targetObject : null);

            RemapSelection(tab, remap);
            SaveAndNotify($"Виправлено ID: {remap.Count}");
        }

        private void RenumberSelected(Tab tab, SerializedObject so, SerializedProperty arr, string idProp, int start)
        {
            var selected = GetSelection(tab);
            if (selected.Count == 0)
                return;

            int counter = Mathf.Max(0, start);
            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var reserved = CollectNonSelectedIds(arr, idProp, selected);

            RunUndoSafeBulkEdit(
                $"Registry Hub: renumber selected ({TabLabels[(int)tab].Trim()})",
                () =>
                {
                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        var idPropRef = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp);
                        if (idPropRef == null)
                            continue;

                        string oldId = idPropRef.stringValue;
                        if (string.IsNullOrWhiteSpace(oldId) || !selected.Contains(oldId))
                            continue;

                        string baseName = StripTrailingNumericSuffix(NormalizeId(oldId, "item"));
                        if (string.IsNullOrWhiteSpace(baseName))
                            baseName = "item";

                        string unique = MakeUniqueFromSet($"{baseName}-{counter}", reserved);
                        counter++;
                        reserved.Add(unique);
                        idPropRef.stringValue = unique;
                        remap[oldId] = unique;
                    }

                    so.ApplyModifiedProperties();
                },
                so != null ? so.targetObject : null);

            RemapSelection(tab, remap);
            SaveAndNotify($"Перенумеровано: {remap.Count}");
        }

        private static HashSet<string> CollectNonSelectedIds(SerializedProperty arr, string idProp, HashSet<string> selected)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < arr.arraySize; i++)
            {
                string id = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                if (!selected.Contains(id))
                    set.Add(id);
            }

            return set;
        }

        private void RemapSelection(Tab tab, IReadOnlyDictionary<string, string> remap)
        {
            var selection = GetSelection(tab);
            var updated = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var id in selection)
            {
                if (remap.TryGetValue(id, out string newId) && !string.IsNullOrWhiteSpace(newId))
                    updated.Add(newId);
                else
                    updated.Add(id);
            }

            selection.Clear();
            foreach (var id in updated)
                selection.Add(id);
        }

        private static int FindIndexById(SerializedProperty arr, string idProp, string id)
        {
            if (arr == null || string.IsNullOrWhiteSpace(id))
                return -1;

            for (int i = 0; i < arr.arraySize; i++)
            {
                string candidate = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                if (string.Equals(candidate, id, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return -1;
        }

        private static string NormalizeId(string value, string fallback)
        {
            string normalized = (value ?? string.Empty).Trim();
            normalized = normalized.Replace('_', '-');
            normalized = Regex.Replace(normalized, @"\s+", "-");
            normalized = Regex.Replace(normalized, @"[^a-zA-Z0-9\-]", "-");
            normalized = Regex.Replace(normalized, @"\-+", "-").Trim('-');

            if (string.IsNullOrWhiteSpace(normalized))
                return fallback;

            return normalized;
        }

        private static string StripTrailingNumericSuffix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return Regex.Replace(value, @"-\d+$", string.Empty);
        }

        private static string MakeUniqueFromSet(string candidate, HashSet<string> reserved)
        {
            string baseId = NormalizeId(candidate, "item");
            string result = baseId;
            int suffix = 1;
            while (reserved.Contains(result))
            {
                result = $"{baseId}-{suffix}";
                suffix++;
            }

            return result;
        }

        private static string MakeUniqueId(SerializedProperty arr, string idProp, string candidate, int selfIndex)
        {
            string baseId = NormalizeId(candidate, "item");
            string result = baseId;
            int suffix = 1;

            while (ContainsIdExcept(arr, idProp, result, selfIndex))
            {
                result = $"{baseId}-{suffix}";
                suffix++;
            }

            return result;
        }

        private static bool ContainsIdExcept(SerializedProperty arr, string idProp, string candidate, int exceptIndex)
        {
            for (int i = 0; i < arr.arraySize; i++)
            {
                if (i == exceptIndex)
                    continue;

                string existing = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                if (string.Equals(existing, candidate, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void DrawDeleteAllButton(SerializedProperty arr, string idProp, SerializedObject so, string itemsLabel)
        {
            if (arr == null) return;
            int count = arr.arraySize;
            if (count == 0) return;

            EditorGUILayout.Space(2);
            Color prev = GUI.color;
            GUI.color = RegistryEditorStyles.ErrorCol;
            if (GUILayout.Button($"\u2717 Видалити все ({count} {itemsLabel})", GUILayout.Height(22)))
            {
                if (EditorUtility.DisplayDialog("Видалити все",
                        $"Видалити всі {count} записів? Цю дію неможливо скасувати.", "Так", "Ні"))
                {
                    arr.ClearArray();
                    so.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
            }
            GUI.color = prev;
        }

        private static void DrawAssetPath(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"\u2514 {path}", EditorStyles.miniLabel);
            if (GUILayout.Button("Ping", EditorStyles.miniButton, GUILayout.Width(36)))
                EditorGUIUtility.PingObject(asset);
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(48)))
                Selection.activeObject = asset;
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawIdLabel(string id, float width)
        {
            string err = RegistryEditorStyles.ValidateId(id);
            Color prev = GUI.color;
            if (err != null) GUI.color = RegistryEditorStyles.ErrorCol;
            EditorGUILayout.LabelField(err != null ? $"\u26a0 {id}" : id, RegistryEditorStyles.EntryTitle, GUILayout.Width(width));
            GUI.color = prev;
        }

        private void DrawPaginationToolbar(int totalCount, ref int page, string noun, out int start, out int endExclusive)
        {
            int pageSize = Mathf.Max(20, _listPageSize);
            int totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)pageSize));
            page = Mathf.Clamp(page, 0, totalPages - 1);
            int previewStart = page * pageSize;
            int previewEndExclusive = Mathf.Min(totalCount, previewStart + pageSize);

            EditorGUILayout.BeginHorizontal();
            _listPageSize = EditorGUILayout.IntField("Page", _listPageSize, GUILayout.Width(110));
            _listPageSize = Mathf.Clamp(_listPageSize, 20, 500);
            using (new EditorGUI.DisabledScope(page <= 0))
            {
                if (GUILayout.Button("◀", EditorStyles.miniButton, GUILayout.Width(26))) page--;
            }
            EditorGUILayout.LabelField($"{(totalCount == 0 ? 0 : previewStart + 1)}-{previewEndExclusive} / {totalCount} {noun}", EditorStyles.miniLabel, GUILayout.Width(170));
            using (new EditorGUI.DisabledScope(page >= totalPages - 1))
            {
                if (GUILayout.Button("▶", EditorStyles.miniButton, GUILayout.Width(26))) page++;
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            pageSize = Mathf.Max(20, _listPageSize);
            totalPages = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)pageSize));
            page = Mathf.Clamp(page, 0, totalPages - 1);
            start = page * pageSize;
            endExclusive = Mathf.Min(totalCount, start + pageSize);
        }

        private static void DrawPrefabMiniIcon(UnityEngine.Object prefabOrAsset)
        {
            Rect iconRect = GUILayoutUtility.GetRect(18f, 18f, GUILayout.Width(20f));
            Sprite sprite = ExtractPreviewSprite(prefabOrAsset as GameObject);
            var prefab = prefabOrAsset as GameObject;
            if ((sprite != null && sprite.texture != null) || prefab != null)
            {
                AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(iconRect, prefab, sprite);
                return;
            }

            var fallback = prefabOrAsset != null
                ? EditorGUIUtility.ObjectContent(prefabOrAsset, prefabOrAsset.GetType()).image
                : EditorGUIUtility.FindTexture("Prefab Icon");

            if (fallback != null)
                GUI.DrawTexture(iconRect, fallback, ScaleMode.ScaleToFit, true);
        }

        private static Sprite ExtractPreviewSprite(GameObject prefab)
        {
            if (prefab == null)
                return null;

            int id = prefab.GetInstanceID();
            if (_prefabSpriteCache.TryGetValue(id, out var cachedSprite))
                return cachedSprite;
            if (_prefabNoSpriteCache.Contains(id))
                return null;

            if (AdaptivePrefabPreviewUtility.TryGetPrimarySprite(prefab, out var sprite, out _))
            {
                _prefabSpriteCache[id] = sprite;
                return sprite;
            }

            _prefabNoSpriteCache.Add(id);
            return null;
        }

        private static bool DrawDeleteBtn()
        {
            Color prev = GUI.color;
            GUI.color = RegistryEditorStyles.ErrorCol;
            bool clicked = GUILayout.Button("\u00d7", GUILayout.Width(22), GUILayout.Height(20));
            GUI.color = prev;
            return clicked;
        }

        private static void DrawInlineEditBox(SerializedProperty element, (string prop, string label)[] fields)
        {
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel++;
            foreach (var (prop, label) in fields)
            {
                var p = element.FindPropertyRelative(prop);
                if (p == null) continue;
                EditorGUILayout.PropertyField(p, new GUIContent(label), true);
                // Підсвічуємо ID з помилкою
                if (p.propertyType == SerializedPropertyType.String && label.Contains("ID", StringComparison.OrdinalIgnoreCase))
                {
                    string val = p.stringValue;
                    if (val != null && val.Contains('_'))
                    {
                        Color c = GUI.color;
                        GUI.color = RegistryEditorStyles.ErrorCol;
                        EditorGUILayout.HelpBox("'_' заборонений в ID.", MessageType.Error);
                        GUI.color = c;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawMissingPrefabRow(int index, string id, SerializedProperty element,
            string prefabProp, string prefabFolder, Dictionary<int, Sprite> pendingSprites,
            SerializedObject so)
        {
            Color prev = GUI.color;
            GUI.color = RegistryEditorStyles.ErrorCol;
            EditorGUILayout.HelpBox($"FATAL: \"{id}\" — prefab відсутній!", MessageType.Error);
            GUI.color = prev;

            EditorGUILayout.BeginHorizontal();
            pendingSprites.TryGetValue(index, out Sprite pendSpr);
            var newSpr = (Sprite)EditorGUILayout.ObjectField("Sprite", pendSpr, typeof(Sprite), false);
            if (newSpr != pendSpr) pendingSprites[index] = newSpr;

            if (GUILayout.Button("\u2699 Згенерувати", GUILayout.Width(110)))
            {
                pendingSprites.TryGetValue(index, out Sprite spr);
                GameObject newPrefab = ResolvePrefab(id, null, spr, prefabFolder);
                element.FindPropertyRelative(prefabProp).objectReferenceValue = newPrefab;
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                pendingSprites.Remove(index);
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void HandleRemove(SerializedProperty arr, int idx, string idProp, SerializedObject so)
        {
            if (idx < 0 || arr == null) return;
            string name = arr.GetArrayElementAtIndex(idx).FindPropertyRelative(idProp)?.stringValue ?? "?";
            if (EditorUtility.DisplayDialog("Видалити запис", $"Видалити '{name}'?", "Так", "Ні"))
            {
                arr.DeleteArrayElementAtIndex(idx);
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        private bool MatchesFilter(string text)
        {
            return string.IsNullOrEmpty(_searchFilter)
                || text.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool ValidateBeforeAdd(string id)
        {
            string err = RegistryEditorStyles.ValidateId(id);
            if (err != null) { Err(err); return false; }
            return true;
        }

        private static bool ContainsId(SerializedProperty arr, string idProp, string id)
        {
            if (arr == null) return false;
            for (int i = 0; i < arr.arraySize; i++)
            {
                string existing = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                if (string.Equals(existing, id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static List<string> CollectIds(SerializedProperty arrayProp, string idField)
        {
            var ids = new List<string>();
            if (arrayProp == null) return ids;

            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                string value = arrayProp.GetArrayElementAtIndex(i).FindPropertyRelative(idField)?.stringValue;
                if (!string.IsNullOrWhiteSpace(value))
                    ids.Add(value.Trim());
            }

            return ids
                .Distinct(StringComparer.Ordinal)
                .OrderBy(v => v, StringComparer.Ordinal)
                .ToList();
        }

        private static void DrawIdPopup(SerializedProperty idProp, GUIContent label, IReadOnlyList<string> knownIds)
        {
            if (idProp == null) return;
            idProp.stringValue = DrawIdPopup(label, idProp.stringValue, knownIds);
        }

        private static string DrawIdPopup(GUIContent label, string currentId, IReadOnlyList<string> knownIds)
        {
            currentId ??= string.Empty;

            var values = new List<string> { string.Empty };
            if (knownIds != null)
            {
                for (int i = 0; i < knownIds.Count; i++)
                {
                    string id = knownIds[i];
                    if (!string.IsNullOrWhiteSpace(id) && !values.Contains(id))
                        values.Add(id);
                }
            }

            bool hasMissingCurrent = !string.IsNullOrWhiteSpace(currentId) && !values.Contains(currentId);
            if (hasMissingCurrent)
                values.Add(currentId);

            string[] optionValues = values.ToArray();
            string[] optionLabels = optionValues
                .Select(v => string.IsNullOrEmpty(v)
                    ? "<none>"
                    : hasMissingCurrent && v == currentId
                        ? $"{v} (missing)"
                        : v)
                .ToArray();

            int currentIndex = Array.IndexOf(optionValues, currentId);
            if (currentIndex < 0) currentIndex = 0;

            int selectedIndex = EditorGUILayout.Popup(label, currentIndex, optionLabels);
            if (selectedIndex < 0 || selectedIndex >= optionValues.Length)
                return currentId;

            return optionValues[selectedIndex];
        }

        private static void Err(string msg) => EditorUtility.DisplayDialog("Registry Hub", msg, "OK");

        private void Info(string msg)
        {
            ShowNotification(new GUIContent(msg));
        }

        private void SaveAndNotify(string msg)
        {
            if (!EditorRegistryWriteLock.IsUnlocked(GetActiveLockKey()))
            {
                Err("Збереження заблоковано: увімкніть Unlock для активного реєстру.");
                return;
            }

            if (IsActiveRegistryStale())
            {
                Err("Збереження скасовано: активний реєстр змінено зовні.");
                return;
            }

            AssetDatabase.SaveAssets();
            EditorContentChangeLog.Write("RegistryHub", msg, GetActiveRegistryAsset(), Array.Empty<string>());
            CaptureActiveStaleBaseline();
            ShowNotification(new GUIContent($"\u2713 {msg}"));
        }

        private string GetActiveLockKey()
        {
            switch (_tab)
            {
                case Tab.Tiles:
                    return TileLockKey;
                case Tab.MapObjects:
                    return ObjectLockKey;
                case Tab.Units:
                    return UnitLockKey;
                case Tab.Buildings:
                case Tab.Walls:
                    return BuildingLockKey;
                case Tab.Resources:
                    return EconomyLockKey;
                default:
                    return UnitLockKey;
            }
        }

        private UnityEngine.Object GetActiveRegistryAsset()
        {
            switch (_tab)
            {
                case Tab.Tiles:
                    return _tileReg;
                case Tab.MapObjects:
                    return _objReg;
                case Tab.Units:
                    return _unitReg;
                case Tab.Buildings:
                case Tab.Walls:
                    return _bldReg;
                case Tab.Resources:
                    return _economyDb;
                default:
                    return null;
            }
        }

        private bool IsActiveRegistryStale()
        {
            switch (_tab)
            {
                case Tab.Tiles:
                    return _tileStale.IsStale(_tileReg);
                case Tab.MapObjects:
                    return _objStale.IsStale(_objReg);
                case Tab.Units:
                    return _unitStale.IsStale(_unitReg);
                case Tab.Buildings:
                case Tab.Walls:
                    return _bldStale.IsStale(_bldReg);
                case Tab.Resources:
                    return _ecoStale.IsStale(_economyDb);
                default:
                    return false;
            }
        }

        private void CaptureStaleBaselines()
        {
            _tileStale.Capture(_tileReg);
            _objStale.Capture(_objReg);
            _unitStale.Capture(_unitReg);
            _bldStale.Capture(_bldReg);
            _ecoStale.Capture(_economyDb);
        }

        private void CaptureActiveStaleBaseline()
        {
            switch (_tab)
            {
                case Tab.Tiles:
                    _tileStale.Capture(_tileReg);
                    break;
                case Tab.MapObjects:
                    _objStale.Capture(_objReg);
                    break;
                case Tab.Units:
                    _unitStale.Capture(_unitReg);
                    break;
                case Tab.Buildings:
                case Tab.Walls:
                    _bldStale.Capture(_bldReg);
                    break;
                case Tab.Resources:
                    _ecoStale.Capture(_economyDb);
                    break;
            }
        }

        private static Color CategoryColor(int idx) => idx switch
        {
            0 => new Color(0.85f, 0.30f, 0.30f), // Military
            1 => new Color(0.35f, 0.70f, 0.35f), // Civilian
            2 => new Color(0.40f, 0.50f, 0.80f), // Industrial
            3 => new Color(0.65f, 0.55f, 0.30f), // Walls
            _ => Color.grey,
        };

        private static int BeginUndoGroup(string groupName, params UnityEngine.Object[] targets)
        {
            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(groupName);

            if (targets != null)
            {
                var recorded = new HashSet<int>();
                for (int i = 0; i < targets.Length; i++)
                {
                    var target = targets[i];
                    if (target == null)
                        continue;

                    int id = target.GetInstanceID();
                    if (!recorded.Add(id))
                        continue;

                    Undo.RecordObject(target, groupName);
                }
            }

            return group;
        }

        private static void EndUndoGroup(int group)
        {
            Undo.CollapseUndoOperations(group);
        }

        private static void RunUndoSafeBulkEdit(string groupName, Action action, params UnityEngine.Object[] targets)
        {
            int group = BeginUndoGroup(groupName, targets);
            try
            {
                action?.Invoke();
            }
            finally
            {
                EndUndoGroup(group);
            }
        }

        private UnityEngine.Object[] GetUndoTargetsForAssetHealthScope(string scope)
        {
            switch (scope)
            {
                case "Tiles":
                    return new UnityEngine.Object[] { _tileSO != null ? _tileSO.targetObject : null };
                case "MapObjects":
                    return new UnityEngine.Object[] { _objSO != null ? _objSO.targetObject : null };
                case "Units":
                    return new UnityEngine.Object[] { _unitSO != null ? _unitSO.targetObject : null };
                case "Buildings":
                    return new UnityEngine.Object[] { _bldSO != null ? _bldSO.targetObject : null };
                case "Walls":
                    return new UnityEngine.Object[] { _bldSO != null ? _bldSO.targetObject : null };
                case "Economy":
                    return new UnityEngine.Object[] { _economyDb };
                default:
                    return Array.Empty<UnityEngine.Object>();
            }
        }

        // ── Prefab creation ─────────────────────────────────

        private static GameObject ResolvePrefab(string id, GameObject prefabOverride, Sprite sprite, string folder)
        {
            if (prefabOverride) return prefabOverride;
            if (sprite) return CreatePrefabFromSprite(id, sprite, folder);
            return CreateEmptyPrefab(id, folder);
        }

        private static GameObject CreatePrefabFromSprite(string id, Sprite sprite, string folder)
        {
            EnsureFolder(folder);
            string safe = SanitizeFileName(id);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{safe}.prefab");

            var go = new GameObject(safe);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static GameObject CreateEmptyPrefab(string id, string folder)
        {
            EnsureFolder(folder);
            string safe = SanitizeFileName(id);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{safe}.prefab");

            var go = new GameObject(safe);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static T FindFirst<T>() where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private void SaveRegistrySelections()
        {
            SaveRegistryPreference(PrefKeyTileRegistryGuid, _tileReg);
            SaveRegistryPreference(PrefKeyObjectRegistryGuid, _objReg);
            SaveRegistryPreference(PrefKeyUnitRegistryGuid, _unitReg);
            SaveRegistryPreference(PrefKeyBuildingRegistryGuid, _bldReg);
            MoyvaProjectEditorContext.Set(_economyDb);
        }

        private void LoadRegistrySelections()
        {
            _tileReg = LoadRegistryPreference<TileRegistrySO>(PrefKeyTileRegistryGuid);
            _objReg = LoadRegistryPreference<MapObjectRegistrySO>(PrefKeyObjectRegistryGuid);
            _unitReg = LoadRegistryPreference<UnitRegistrySO>(PrefKeyUnitRegistryGuid);
            _bldReg = LoadRegistryPreference<BuildingRegistrySO>(PrefKeyBuildingRegistryGuid);
            _economyDb = MoyvaProjectEditorContext.Get<EconomyDatabaseSO>();
        }

        private static void SaveRegistryPreference<T>(string key, T asset) where T : UnityEngine.Object
        {
            MoyvaProjectEditorContext.Set(asset);
            if (asset == null)
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            string guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                EditorPrefs.DeleteKey(key);
                return;
            }

            EditorPrefs.SetString(key, guid);
        }

        private static T LoadRegistryPreference<T>(string key) where T : UnityEngine.Object
        {
            var contextAsset = MoyvaProjectEditorContext.Get<T>();
            if (contextAsset != null)
                return contextAsset;

            string guid = EditorPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<T>(path);
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
            if (string.IsNullOrWhiteSpace(id)) return "item";
            char[] inv = System.IO.Path.GetInvalidFileNameChars();
            char[] c = id.Trim().ToCharArray();
            for (int i = 0; i < c.Length; i++)
                if (Array.IndexOf(inv, c[i]) >= 0 || c[i] == '/')
                    c[i] = '-';
            return new string(c);
        }

        // ══════════════════════════════════════════════════════
        //  ПЕРЕМІЩЕННЯ МІЖ РЕЄСТРАМИ (КОНТЕКСТНЕ МЕНЮ)
        // ══════════════════════════════════════════════════════

        private void ShowMoveMenu(string id, Tab source, int sourceIndex)
        {
            var menu = new GenericMenu();
            foreach (Tab target in GetMovableRegistryTabs())
            {
                if (target == source) continue;
                Tab t = target;
                int idx = sourceIndex;
                menu.AddItem(new GUIContent($"Перемістити в → {TabLabels[(int)t].Trim()}"),
                    false, () => MoveEntry(id, source, idx, t));
            }
            menu.ShowAsContext();
        }

        private static IEnumerable<Tab> GetMovableRegistryTabs()
        {
            yield return Tab.Tiles;
            yield return Tab.MapObjects;
            yield return Tab.Units;
            yield return Tab.Buildings;
        }

        private void MoveEntry(string id, Tab source, int sourceIndex, Tab target)
        {
            // Отримуємо prefab з джерела
            GameObject prefab = GetPrefabFromEntry(source, sourceIndex);

            // Додаємо в цільовий реєстр
            if (!AddToRegistry(id, prefab, target))
            {
                Err($"Не вдалося додати '{id}' до {TabLabels[(int)target].Trim()}.");
                return;
            }

            // Видаляємо з джерела
            RemoveFromRegistry(source, sourceIndex);
            Repaint();
            SaveAndNotify($"'{id}' переміщено до {TabLabels[(int)target].Trim()}");
        }

        private GameObject GetPrefabFromEntry(Tab tab, int index)
        {
            switch (tab)
            {
                case Tab.Tiles:
                {
                    var el = _tileSO.FindProperty("_definitions").GetArrayElementAtIndex(index);
                    return el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue as GameObject;
                }
                case Tab.MapObjects:
                {
                    var el = _objSO.FindProperty("_definitions").GetArrayElementAtIndex(index);
                    return el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue as GameObject;
                }
                case Tab.Units:
                {
                    var el = _unitSO.FindProperty("Configs").GetArrayElementAtIndex(index);
                    return el.FindPropertyRelative("Prefab")?.objectReferenceValue as GameObject;
                }
                case Tab.Buildings:
                {
                    var el = _bldSO.FindProperty("Buildings").GetArrayElementAtIndex(index);
                    return el.FindPropertyRelative("Prefab")?.objectReferenceValue as GameObject;
                }
            }
            return null;
        }

        private bool AddToRegistry(string id, GameObject prefab, Tab target)
        {
            switch (target)
            {
                case Tab.Tiles:
                {
                    if (_tileSO == null) return false;
                    var defs = _tileSO.FindProperty("_definitions");
                    if (ContainsId(defs, "_id", id)) return false;
                    int idx = defs.arraySize;
                    defs.InsertArrayElementAtIndex(idx);
                    var el = defs.GetArrayElementAtIndex(idx);
                    el.FindPropertyRelative("_id").stringValue = id;
                    el.FindPropertyRelative("_movementCost").floatValue = 1f;
                    el.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
                    _tileSO.ApplyModifiedProperties();
                    return true;
                }
                case Tab.MapObjects:
                {
                    if (_objSO == null) return false;
                    var defs = _objSO.FindProperty("_definitions");
                    if (ContainsId(defs, "_id", id)) return false;
                    int idx = defs.arraySize;
                    defs.InsertArrayElementAtIndex(idx);
                    var el = defs.GetArrayElementAtIndex(idx);
                    el.FindPropertyRelative("_id").stringValue = id;
                    el.FindPropertyRelative("_visualPrefab").objectReferenceValue = prefab;
                    _objSO.ApplyModifiedProperties();
                    return true;
                }
                case Tab.Units:
                {
                    if (_unitSO == null) return false;
                    var defs = _unitSO.FindProperty("Configs");
                    if (ContainsId(defs, "TypeId", id)) return false;
                    int idx = defs.arraySize;
                    defs.InsertArrayElementAtIndex(idx);
                    var el = defs.GetArrayElementAtIndex(idx);
                    el.FindPropertyRelative("TypeId").stringValue = id;
                    el.FindPropertyRelative("BaseStamina").floatValue = 100f;
                    el.FindPropertyRelative("StaminaRandomRange").vector2Value = new Vector2(-5f, 5f);
                    el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                    _unitSO.ApplyModifiedProperties();
                    return true;
                }
                case Tab.Buildings:
                {
                    if (_bldSO == null) return false;
                    var defs = _bldSO.FindProperty("Buildings");
                    if (ContainsId(defs, "Id", id)) return false;
                    int idx = defs.arraySize;
                    defs.InsertArrayElementAtIndex(idx);
                    var el = defs.GetArrayElementAtIndex(idx);
                    el.FindPropertyRelative("Id").stringValue = id;
                    el.FindPropertyRelative("DisplayName").stringValue = id;
                    el.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
                    _bldSO.ApplyModifiedProperties();
                    return true;
                }
            }
            return false;
        }

        private void RemoveFromRegistry(Tab tab, int index)
        {
            switch (tab)
            {
                case Tab.Tiles:
                    _tileSO.FindProperty("_definitions").DeleteArrayElementAtIndex(index);
                    _tileSO.ApplyModifiedProperties();
                    break;
                case Tab.MapObjects:
                    _objSO.FindProperty("_definitions").DeleteArrayElementAtIndex(index);
                    _objSO.ApplyModifiedProperties();
                    break;
                case Tab.Units:
                    _unitSO.FindProperty("Configs").DeleteArrayElementAtIndex(index);
                    _unitSO.ApplyModifiedProperties();
                    break;
                case Tab.Buildings:
                    _bldSO.FindProperty("Buildings").DeleteArrayElementAtIndex(index);
                    _bldSO.ApplyModifiedProperties();
                    break;
            }
        }

        private static void HandleRowContextClick(Rect rowRect, Action showMenu)
        {
            if (Event.current.type == EventType.ContextClick && rowRect.Contains(Event.current.mousePosition))
            {
                showMenu();
                Event.current.Use();
            }
        }

        private void HandleRowDragStart(Rect rowRect, string id, Tab source, int index)
        {
            var evt = Event.current;
            if (evt.type == EventType.MouseDrag && evt.button == 0 && rowRect.Contains(evt.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DragDataKey, true);
                DragAndDrop.paths = null;
                DragAndDrop.objectReferences = Array.Empty<UnityEngine.Object>();
                _dragPayload = (source, index, id);
                DragAndDrop.StartDrag($"Перемістити '{id}'");
                evt.Use();
            }
        }

        // ══════════════════════════════════════════════════════
        //  BULK DELETE TAB
        // ══════════════════════════════════════════════════════

        private readonly struct BulkEntry
        {
            public readonly string Key;
            public readonly string Id;
            public readonly string Category;
            public readonly bool IsProblem;
            public readonly string Issue;
            public readonly UnityEngine.Object PreviewObject;
            public readonly Sprite PreviewSprite;

            public BulkEntry(string key, string id, string category, bool isProblem, string issue, UnityEngine.Object previewObject, Sprite previewSprite)
            {
                Key = key;
                Id = id;
                Category = category;
                IsProblem = isProblem;
                Issue = issue;
                PreviewObject = previewObject;
                PreviewSprite = previewSprite;
            }
        }

        private static readonly string[] BulkCategoryLabels =
            { "Всі", "Тайли", "Об'єкти", "Юніти", "Будівлі" };

        private void DrawBulkDeleteTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Мультивибір та видалення", RegistryEditorStyles.Accent);
            EditorGUILayout.HelpBox(
                "Оберіть кілька записів із реєстрів та видаліть їх одночасно.\n" +
                "Фільтр за категорією або покажіть усі одночасно. УВАГА: префаби НЕ видаляються з диску.",
                MessageType.Info);

            _bulkCategoryIndex = GUILayout.Toolbar(_bulkCategoryIndex, BulkCategoryLabels);
            EditorGUILayout.Space(2f);
            _bulkSearch = EditorGUILayout.TextField("Пошук", _bulkSearch);
            _bulkOnlyProblems = EditorGUILayout.ToggleLeft("Показати лише проблемні", _bulkOnlyProblems);
            RegistryEditorStyles.DrawSeparator();

            var entries = CollectBulkEntries();
            if (!string.IsNullOrWhiteSpace(_bulkSearch))
                entries = entries.Where(e => e.Id.IndexOf(_bulkSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            if (_bulkOnlyProblems)
                entries = entries.Where(e => e.IsProblem).ToList();
            SyncBulkPreview(entries);

            int problemCount = entries.Count(e => e.IsProblem);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Обрати всі", GUILayout.Width(110f)))
                foreach (var e in entries) _bulkSelectedIds.Add(e.Id);
            if (GUILayout.Button("Зняти всі", GUILayout.Width(110f)))
                _bulkSelectedIds.Clear();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Проблемні: {problemCount}", EditorStyles.miniLabel, GUILayout.Width(100f));
            EditorGUILayout.LabelField($"Обрано: {_bulkSelectedIds.Count}", EditorStyles.miniLabel, GUILayout.Width(90f));
            EditorGUILayout.EndHorizontal();

            _bulkScroll = EditorGUILayout.BeginScrollView(_bulkScroll, "box");
            if (entries.Count == 0)
            {
                EditorWindowSharedUI.DrawWarning("Немає елементів для відображення.", MessageType.None);
            }
            else
            {
                string prevCategory = null;
                int rowIndex = 0;
                foreach (var entry in entries)
                {
                    if (entry.Category != prevCategory)
                    {
                        if (prevCategory != null) EditorGUILayout.Space(2f);
                        EditorGUILayout.LabelField(entry.Category, RegistryEditorStyles.SubHeader);
                        prevCategory = entry.Category;
                    }

                    bool wasSelected = _bulkSelectedIds.Contains(entry.Id);
                    GUIStyle rowStyle = EditorWindowSharedUI.ListRowStyle(wasSelected, rowIndex % 2 != 0);
                    Rect rowRect = EditorGUILayout.BeginHorizontal(rowStyle);
                    bool isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(20f));
                    EditorGUILayout.LabelField(entry.IsProblem ? $"⚠ {entry.Id}" : entry.Id);
                    if (entry.IsProblem)
                    {
                        Color prev = GUI.color;
                        GUI.color = RegistryEditorStyles.ErrorCol;
                        EditorGUILayout.LabelField(entry.Issue, EditorStyles.miniLabel, GUILayout.Width(260f));
                        GUI.color = prev;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rowRect.Contains(Event.current.mousePosition))
                    {
                        _bulkPreviewKey = entry.Key;
                        _bulkPreviewObject = entry.PreviewObject;
                        Repaint();
                    }

                    if (isSelected && !wasSelected) _bulkSelectedIds.Add(entry.Id);
                    else if (!isSelected && wasSelected) _bulkSelectedIds.Remove(entry.Id);
                    rowIndex++;
                }
            }
            EditorGUILayout.EndScrollView();

            DrawBulkPreviewPanel(entries);

            EditorGUILayout.Space(4f);
            int selCount = _bulkSelectedIds.Count;
            using (new EditorGUI.DisabledScope(selCount == 0))
            {
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.85f, 0.25f, 0.25f);
                if (GUILayout.Button($"Видалити вибрані ({selCount}) з реєстрів", GUILayout.Height(30f)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Підтвердити видалення",
                        $"Буде видалено {selCount} записів з реєстрів.\nПрефаби НЕ видаляються з диску.",
                        "Видалити", "Скасувати"))
                    {
                        ExecuteBulkDelete();
                        GUIUtility.ExitGUI();
                    }
                }
                GUI.backgroundColor = prev;
            }
        }

        private void SyncBulkPreview(IReadOnlyList<BulkEntry> entries)
        {
            if (string.IsNullOrEmpty(_bulkPreviewKey))
            {
                _bulkPreviewObject = null;
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Key == _bulkPreviewKey)
                {
                    _bulkPreviewObject = entries[i].PreviewObject;
                    return;
                }
            }

            _bulkPreviewKey = string.Empty;
            _bulkPreviewObject = null;
        }

        private void DrawBulkPreviewPanel(IReadOnlyList<BulkEntry> entries)
        {
            RegistryEditorStyles.DrawSeparator();
            EditorGUILayout.LabelField("Прев'ю елемента", RegistryEditorStyles.SubHeader);

            if (string.IsNullOrEmpty(_bulkPreviewKey))
            {
                EditorGUILayout.HelpBox("Клікніть по рядку елемента, щоб побачити його прев'ю. Чекбокс використовується лише для мультивибору.", MessageType.None);
                return;
            }

            BulkEntry? selectedEntry = null;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Key == _bulkPreviewKey)
                {
                    selectedEntry = entries[i];
                    break;
                }
            }

            if (!selectedEntry.HasValue)
            {
                EditorGUILayout.HelpBox("Обраний елемент більше не існує або не проходить поточний фільтр.", MessageType.Warning);
                return;
            }

            var entry = selectedEntry.Value;
            EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
            EditorGUILayout.LabelField($"Категорія: {entry.Category}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"ID: {entry.Id}", EditorStyles.miniLabel);
            if (entry.IsProblem)
                EditorGUILayout.HelpBox(entry.Issue, MessageType.Warning);

            if (_bulkPreviewObject)
            {
                if (entry.PreviewSprite != null)
                {
                    DrawSpritePreview(entry.PreviewSprite, _bulkPreviewObject as GameObject, 96f);
                }
                else
                {
                    DrawSpritePreview(null, _bulkPreviewObject as GameObject, 96f);
                }

                EditorGUILayout.ObjectField("Asset", _bulkPreviewObject, typeof(UnityEngine.Object), false);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Ping", GUILayout.Width(70f)))
                    EditorGUIUtility.PingObject(_bulkPreviewObject);
                if (GUILayout.Button("Select", GUILayout.Width(70f)))
                    Selection.activeObject = _bulkPreviewObject;
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Для цього елемента немає прив'язаного prefab/asset для прев'ю.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private static void DrawSpritePreview(Sprite sprite, GameObject prefab, float size)
        {
            if (sprite == null && prefab == null)
                return;

            Rect previewRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size));
            GUI.Box(previewRect, GUIContent.none);
            AdaptivePrefabPreviewUtility.DrawPrefabOrSprite(previewRect, prefab, sprite);
        }

        private List<BulkEntry> CollectBulkEntries()
        {
            var result = new List<BulkEntry>();

            void AddFromSO(SerializedObject so, string arrayProp, string idProp, string prefabProp, string category, int catIndex)
            {
                if (_bulkCategoryIndex != 0 && _bulkCategoryIndex != catIndex) return;
                if (so == null) return;
                so.Update();
                var arr = so.FindProperty(arrayProp);
                if (arr == null || !arr.isArray) return;
                for (int i = 0; i < arr.arraySize; i++)
                {
                    var el = arr.GetArrayElementAtIndex(i);
                    string rawId = el.FindPropertyRelative(idProp)?.stringValue;
                    string shownId = string.IsNullOrWhiteSpace(rawId) ? "<порожній ID>" : rawId;

                    var issues = new List<string>();
                    if (string.IsNullOrWhiteSpace(rawId))
                    {
                        issues.Add("Порожній ID");
                    }
                    else
                    {
                        string idErr = RegistryEditorStyles.ValidateId(rawId);
                        if (!string.IsNullOrEmpty(idErr))
                            issues.Add(idErr);
                    }

                    if (!string.IsNullOrEmpty(prefabProp))
                    {
                        var prefab = el.FindPropertyRelative(prefabProp)?.objectReferenceValue;
                        if (!prefab)
                            issues.Add("Відсутній prefab");
                    }

                    var previewObject = string.IsNullOrEmpty(prefabProp)
                        ? null
                        : el.FindPropertyRelative(prefabProp)?.objectReferenceValue;

                    Sprite previewSprite = null;
                    if (previewObject is Sprite spriteAsset)
                    {
                        previewSprite = spriteAsset;
                    }
                    else if (previewObject is GameObject previewGo)
                    {
                        previewSprite = AdaptivePrefabPreviewUtility.TryGetPrimarySprite(previewGo, out var sprite, out _)
                            ? sprite
                            : null;
                    }

                    string key = $"{category}::{shownId}";

                    bool isProblem = issues.Count > 0;
                    string issueText = isProblem ? string.Join("; ", issues) : string.Empty;
                    result.Add(new BulkEntry(key, shownId, category, isProblem, issueText, previewObject, previewSprite));
                }
            }

            AddFromSO(_tileSO, "_definitions", "_id", "_visualPrefab", "Тайли", 1);
            AddFromSO(_objSO, "_definitions", "_id", "_visualPrefab", "Об'єкти", 2);
            AddFromSO(_unitSO, "Configs", "TypeId", "Prefab", "Юніти", 3);
            AddFromSO(_bldSO, "Buildings", "Id", "Prefab", "Будівлі", 4);

            return result;
        }

        private void ExecuteBulkDelete()
        {
            if (_bulkSelectedIds.Count == 0) return;

            void DeleteFromSO(SerializedObject so, string arrayProp, string idProp)
            {
                if (so == null) return;
                so.Update();
                var arr = so.FindProperty(arrayProp);
                if (arr == null) return;
                for (int i = arr.arraySize - 1; i >= 0; i--)
                {
                    string id = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                    if (id != null && _bulkSelectedIds.Contains(id))
                        arr.DeleteArrayElementAtIndex(i);
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(so.targetObject);
            }

            RunUndoSafeBulkEdit(
                "Registry Hub: bulk delete",
                () =>
                {
                    DeleteFromSO(_tileSO, "_definitions", "_id");
                    DeleteFromSO(_objSO, "_definitions", "_id");
                    DeleteFromSO(_unitSO, "Configs", "TypeId");
                    DeleteFromSO(_bldSO, "Buildings", "Id");
                },
                _tileSO != null ? _tileSO.targetObject : null,
                _objSO != null ? _objSO.targetObject : null,
                _unitSO != null ? _unitSO.targetObject : null,
                _bldSO != null ? _bldSO.targetObject : null);

            int count = _bulkSelectedIds.Count;
            _bulkSelectedIds.Clear();
            _bulkPreviewKey = string.Empty;
            _bulkPreviewObject = null;
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent($"✓ Видалено {count} записів"));
        }

        // ══════════════════════════════════════════════════════
        //  ЗАМІНА _ → - В УСІХ ID
        // ══════════════════════════════════════════════════════

        private void FixUnderscoreIds()
        {
            var mapping = new Dictionary<string, string>();
            CollectUnderscoreIds(_tileSO, "_definitions", "_id", mapping);
            CollectUnderscoreIds(_objSO,  "_definitions", "_id", mapping);
            CollectUnderscoreIds(_unitSO, "Configs",      "TypeId", mapping);
            CollectUnderscoreIds(_bldSO,  "Buildings",    "Id", mapping);

            if (mapping.Count == 0)
            {
                EditorUtility.DisplayDialog("Registry Hub", "Жодного ID з '_' не знайдено.", "OK");
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var kv in mapping)
                sb.AppendLine($"  {kv.Key}  \u2192  {kv.Value}");

            if (!EditorUtility.DisplayDialog("Замінити _ на -",
                    $"Знайдено {mapping.Count} ID з '_':\n\n{sb}\nБуде оновлено всі ScriptableObject у проєкті.",
                    "Замінити", "Скасувати"))
                return;

            int assetCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

            try
            {
                for (int g = 0; g < guids.Length; g++)
                {
                    if (g % 50 == 0)
                        EditorUtility.DisplayProgressBar("Заміна ID…",
                            $"{g}/{guids.Length}", (float)g / guids.Length);

                    string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                    var all = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in all)
                    {
                        if (asset == null) continue;
                        var so = new SerializedObject(asset);
                        if (ReplaceStringsInProperties(so, mapping))
                        {
                            so.ApplyModifiedProperties();
                            EditorUtility.SetDirty(asset);
                            assetCount++;
                        }
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            RefreshSOs();

            string msg = $"Замінено {mapping.Count} ID у {assetCount} ассетах.";
            Debug.Log($"[Registry Hub] {msg}");
            ShowNotification(new GUIContent($"\u2713 {msg}"));
        }

        private static void CollectUnderscoreIds(SerializedObject so, string arrayProp, string idProp,
            Dictionary<string, string> mapping)
        {
            if (so == null) return;
            var arr = so.FindProperty(arrayProp);
            if (arr == null) return;

            for (int i = 0; i < arr.arraySize; i++)
            {
                string id = arr.GetArrayElementAtIndex(i).FindPropertyRelative(idProp)?.stringValue;
                if (!string.IsNullOrEmpty(id) && id.Contains('_') && !mapping.ContainsKey(id))
                    mapping[id] = id.Replace('_', '-');
            }
        }

        private static bool ReplaceStringsInProperties(SerializedObject so,
            Dictionary<string, string> mapping)
        {
            bool changed = false;
            var iter = so.GetIterator();
            bool enterChildren = true;
            int safetyLimit = 10000;

            while (iter.NextVisible(enterChildren))
            {
                if (--safetyLimit <= 0) break;

                enterChildren = iter.propertyType != SerializedPropertyType.String;

                if (iter.propertyType != SerializedPropertyType.String) continue;
                string val = iter.stringValue;
                if (string.IsNullOrEmpty(val)) continue;
                if (!mapping.TryGetValue(val, out string newVal)) continue;

                iter.stringValue = newVal;
                changed = true;
            }

            return changed;
        }
    }
}
