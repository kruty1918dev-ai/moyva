using System;
using System.Collections.Generic;
using System.Linq;
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

        private enum Tab { Tiles, MapObjects, Units, Buildings, Walls, Resources, BulkDelete }

        private static readonly string[] TabLabels =
            { "  Тайли", "  Об'єкти", "  Юніти", "  Будівлі", "  Стіни", "  Ресурси", "  Мультивибір" };

        private static readonly string[][] TabIconCandidates =
        {
            new[] { "TerrainAsset Icon", "Terrain Icon", "Grid Icon" },
            new[] { "d_Prefab Icon", "Prefab Icon" },
            new[] { "d_AvatarSelector", "Avatar Icon" },
            new[] { "d_BuildSettings.Standalone.Small", "BuildSettings.Standalone.Small" },
            new[] { "d_SceneViewOrtho", "SceneViewOrtho" },
            new[] { "d_ScriptableObject Icon", "ScriptableObject Icon" },
            new[] { "d_FilterByType", "FilterByType" },
        };

        private const string TilePrefabFolder    = "Assets/Moyva/Prefabs/Tiles";
        private const string ObjectPrefabFolder  = "Assets/Moyva/Prefabs/Objects";
        private const string UnitPrefabFolder    = "Assets/Moyva/Prefabs/Units";
        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";

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

        // MapObject
        private MapObjectRegistrySO _objReg;
        private SerializedObject    _objSO;
        private string              _newObjId = "";
        private Sprite              _newObjSprite;
        private GameObject          _newObjPrefab;
        private bool                _objCreateOpen = true;

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

        // Building
        private BuildingRegistrySO _bldReg;
        private SerializedObject   _bldSO;
        private string             _newBldId = "";
        private string             _newBldName = "";
        private BuildingCategory   _newBldCategory;
        private Sprite             _newBldSprite;
        private GameObject         _newBldPrefab;
        private int                _newBldWorkers;
        private int                _newBldPriority;
        private bool               _newBldIsHousing;
        private int                _newBldHousingCap;
        private bool               _newBldIsTownHall;
        private bool               _newBldIsCastle;
        private bool               _bldCreateOpen = true;

        private string             _newBldIndustrialResourceId = string.Empty;

        private enum CivilianBuildingType
        {
            Regular,
            TownHall,
            Castle,
        }
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

        // Drag-and-drop
        private const string DragDataKey = "RegistryHub_Move";
        private (Tab source, int index, string id)? _dragPayload;

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
            w._tab = (Tab)Mathf.Clamp(tabIndex, 0, 6);
            w.Show();
            w.Focus();
        }

        // ══════════════════════════════════════════════════════
        //  LIFECYCLE
        // ══════════════════════════════════════════════════════

        private void OnEnable()  => AutoFind();
        private void OnFocus()   => RefreshSOs();

        private void OnDisable()
        {
            foreach (var editor in _resourceEditors.Values)
                if (editor != null) DestroyImmediate(editor);
            _resourceEditors.Clear();
        }

        private void AutoFind()
        {
            _tileReg ??= FindFirst<TileRegistrySO>();
            _objReg  ??= FindFirst<MapObjectRegistrySO>();
            _unitReg ??= FindFirst<UnitRegistrySO>();
            _bldReg  ??= FindFirst<BuildingRegistrySO>();
            RefreshSOs();
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
            _tileSO?.Update();
            _objSO?.Update();
            _unitSO?.Update();
            _bldSO?.Update();

            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            DrawSidebar();
            DrawContent();
            EditorGUILayout.EndHorizontal();

            DrawStatusBar();
        }

        // ── Тулбар ──────────────────────────────────────────

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("REGISTRY HUB", EditorStyles.boldLabel, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            _searchFilter = EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
            if (GUILayout.Button(new GUIContent("  _ \u2192 -", "Замінити '_' на '-' в усіх ID та їх посиланнях"), EditorStyles.toolbarButton))
                FixUnderscoreIds();
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.toolbarButton, GUILayout.Width(28)))
            {
                _tileReg = null; _objReg = null; _unitReg = null; _bldReg = null;
                AutoFind();
                Repaint();
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

            Rect r = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.DrawRect(r, RegistryEditorStyles.SidebarBg);
            GUI.Label(r, $"   Тайли: {tiles}  |  Об'єкти: {objs}  |  Юніти: {units}  |  Будівлі: {blds}  |  Стіни: {walls}",
                EditorStyles.centeredGreyMiniLabel);
        }

        // ══════════════════════════════════════════════════════
        //  TILES TAB
        // ══════════════════════════════════════════════════════

        private void DrawTileTab()
        {
            RegistryEditorStyles.DrawColoredHeader("  Tile Registry — типи тайлів", RegistryEditorStyles.Accent);

            _tileReg = (TileRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _tileReg, typeof(TileRegistrySO), false);
            if (_tileReg && _tileSO?.targetObject != _tileReg) RefreshSOs();
            if (!_tileReg) { EditorGUILayout.HelpBox("Оберіть TileRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_tileReg);
            RegistryEditorStyles.DrawSeparator();

            var defs = _tileSO.FindProperty("_definitions");
            int count = defs?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);

            if (count == 0)
                EditorGUILayout.LabelField("Записів немає. Додайте перший тайл нижче.", RegistryEditorStyles.CenteredMini);

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = defs.GetArrayElementAtIndex(i);
                string id     = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                float  cost   = el.FindPropertyRelative("_movementCost")?.floatValue ?? 0f;
                var    prefab = el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue;

                if (!MatchesFilter(id)) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);
                Rect tileRowRect = GUILayoutUtility.GetRect(0, 0);

                // Рядок-заголовок (клік — розгортання)
                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedTile == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedTile = isOpen ? -1 : i;
                DrawIdLabel(id, 150);
                EditorGUILayout.LabelField($"Рух: {cost:F1}", EditorStyles.miniLabel, GUILayout.Width(70));
                EditorGUILayout.LabelField(prefab ? $"\u2713 {prefab.name}" : "\u2717 Немає prefab", EditorStyles.miniLabel);
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                // FATAL: prefab відсутній
                if (!prefab)
                    DrawMissingPrefabRow(i, id, el, "_visualPrefab", TilePrefabFolder, _pendingTileSprites, _tileSO);

                // Інлайн-редагування
                if (isOpen)
                {
                    DrawInlineEditBox(el, new[] {
                        ("_id",            "ID"),
                        ("_movementCost",  "Movement Cost"),
                        ("_visualPrefab",  "Visual Prefab"),
                    });
                    _tileSO.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                { int ci = i; string cid = id; HandleRowContextClick(
                    new Rect(tileRowRect.x, tileRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - tileRowRect.y),
                    () => ShowMoveMenu(cid, Tab.Tiles, ci));
                  HandleRowDragStart(
                    new Rect(tileRowRect.x, tileRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - tileRowRect.y),
                    cid, Tab.Tiles, ci); }
            }

            HandleRemove(defs, removeIdx, "_id", _tileSO);
            DrawDeleteAllButton(defs, "_id", _tileSO, "тайлів");
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

            _objReg = (MapObjectRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _objReg, typeof(MapObjectRegistrySO), false);
            if (_objReg && _objSO?.targetObject != _objReg) RefreshSOs();
            if (!_objReg) { EditorGUILayout.HelpBox("Оберіть MapObjectRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_objReg);
            RegistryEditorStyles.DrawSeparator();

            var defs = _objSO.FindProperty("_definitions");
            int count = defs?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);

            if (count == 0)
                EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = defs.GetArrayElementAtIndex(i);
                string id = el.FindPropertyRelative("_id")?.stringValue ?? "?";
                var prefab = el.FindPropertyRelative("_visualPrefab")?.objectReferenceValue;

                if (!MatchesFilter(id)) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);
                Rect objRowRect = GUILayoutUtility.GetRect(0, 0);

                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedObj == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedObj = isOpen ? -1 : i;
                DrawIdLabel(id, 180);
                EditorGUILayout.LabelField(prefab ? $"\u2713 {prefab.name}" : "\u2717 Немає prefab", EditorStyles.miniLabel);
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                // FATAL: prefab відсутній
                if (!prefab)
                    DrawMissingPrefabRow(i, id, el, "_visualPrefab", ObjectPrefabFolder, _pendingObjSprites, _objSO);

                if (isOpen)
                {
                    DrawInlineEditBox(el, new[] {
                        ("_id",            "ID"),
                        ("_visualPrefab",  "Visual Prefab"),
                    });
                    _objSO.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                { int ci = i; string cid = id; HandleRowContextClick(
                    new Rect(objRowRect.x, objRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - objRowRect.y),
                    () => ShowMoveMenu(cid, Tab.MapObjects, ci));
                  HandleRowDragStart(
                    new Rect(objRowRect.x, objRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - objRowRect.y),
                    cid, Tab.MapObjects, ci); }
            }

            HandleRemove(defs, removeIdx, "_id", _objSO);
            DrawDeleteAllButton(defs, "_id", _objSO, "об'єктів");
            RegistryEditorStyles.DrawSeparator();

            _objCreateOpen = EditorGUILayout.Foldout(_objCreateOpen, "\u2795 Створити новий об'єкт", true, EditorStyles.foldoutHeader);
            if (_objCreateOpen)
            {
                EditorGUILayout.BeginVertical(RegistryEditorStyles.SectionBox);
                var objDefs = _objSO.FindProperty("_definitions");
                _newObjId     = RegistryEditorStyles.IdFieldWithDuplicateCheck("ID", _newObjId, objDefs, "_id");
                _newObjSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newObjSprite, typeof(Sprite), false);
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

            _unitReg = (UnitRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _unitReg, typeof(UnitRegistrySO), false);
            if (_unitReg && _unitSO?.targetObject != _unitReg) RefreshSOs();
            if (!_unitReg) { EditorGUILayout.HelpBox("Оберіть UnitRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_unitReg);
            RegistryEditorStyles.DrawSeparator();

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

            if (count == 0)
                EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = configs.GetArrayElementAtIndex(i);
                string typeId   = el.FindPropertyRelative("TypeId")?.stringValue ?? "?";
                float  stamina  = el.FindPropertyRelative("BaseStamina")?.floatValue ?? 0f;
                var    prefab   = el.FindPropertyRelative("Prefab")?.objectReferenceValue;
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
                DrawIdLabel(typeId, 130);
                RegistryEditorStyles.DrawBadge(role == UnitRole.Worker ? "Worker" : "Military", role == UnitRole.Worker ? new Color(0.16f, 0.53f, 0.25f) : new Color(0.62f, 0.20f, 0.20f));
                EditorGUILayout.LabelField($"Стаміна: {stamina:F0}", EditorStyles.miniLabel, GUILayout.Width(90));
                EditorGUILayout.LabelField(prefab ? $"\u2713 {prefab.name}" : "\u2717 Немає", EditorStyles.miniLabel);
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

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
                    _unitSO.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                { int ci = i; string cid = typeId; HandleRowContextClick(
                    new Rect(unitRowRect.x, unitRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - unitRowRect.y),
                    () => ShowMoveMenu(cid, Tab.Units, ci));
                  HandleRowDragStart(
                    new Rect(unitRowRect.x, unitRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - unitRowRect.y),
                    cid, Tab.Units, ci); }
            }

            HandleRemove(configs, removeIdx, "TypeId", _unitSO);
            DrawDeleteAllButton(configs, "TypeId", _unitSO, "юнітів");
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

            _bldReg = (BuildingRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _bldReg, typeof(BuildingRegistrySO), false);
            if (_bldReg && _bldSO?.targetObject != _bldReg) RefreshSOs();
            if (!_bldReg) { EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO або натисніть \u27f3.", MessageType.Warning); return; }

            DrawAssetPath(_bldReg);
            RegistryEditorStyles.DrawSeparator();

            var blds = _bldSO.FindProperty("Buildings");
            int count = blds?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Записи ({count})", RegistryEditorStyles.SubHeader);

            if (count == 0)
                EditorGUILayout.LabelField("Записів немає.", RegistryEditorStyles.CenteredMini);

            int removeIdx = -1;
            for (int i = 0; i < count; i++)
            {
                var el = blds.GetArrayElementAtIndex(i);
                string id          = el.FindPropertyRelative("Id")?.stringValue ?? "?";
                string displayName = el.FindPropertyRelative("DisplayName")?.stringValue ?? "";
                int    category    = el.FindPropertyRelative("Category")?.enumValueIndex ?? 0;
                var    prefab      = el.FindPropertyRelative("Prefab")?.objectReferenceValue;

                if (!MatchesFilter(id) && !MatchesFilter(displayName)) continue;

                GUIStyle style = i % 2 == 0 ? RegistryEditorStyles.Card : RegistryEditorStyles.CardAlt;
                EditorGUILayout.BeginVertical(style);
                Rect bldRowRect = GUILayoutUtility.GetRect(0, 0);

                EditorGUILayout.BeginHorizontal();
                bool isOpen = _expandedBld == i;
                if (GUILayout.Button(isOpen ? "\u25BC" : "\u25B6", EditorStyles.miniLabel, GUILayout.Width(16)))
                    _expandedBld = isOpen ? -1 : i;
                DrawIdLabel(id, 100);

                if (!string.IsNullOrEmpty(displayName))
                    EditorGUILayout.LabelField($"\"{displayName}\"", EditorStyles.miniLabel, GUILayout.Width(120));

                string catName = ((BuildingCategory)category).ToString();
                RegistryEditorStyles.DrawBadge(catName, CategoryColor(category));

                int workers = el.FindPropertyRelative("RequiredWorkers")?.intValue ?? 0;
                bool isHousing = el.FindPropertyRelative("IsHousing")?.boolValue ?? false;
                bool isTownHall = el.FindPropertyRelative("IsTownHall")?.boolValue ?? false;
                bool isCastle = el.FindPropertyRelative("IsCastle")?.boolValue ?? false;
                if (workers > 0)
                    EditorGUILayout.LabelField($"\ud83d\udc77{workers}", EditorStyles.miniLabel, GUILayout.Width(32));
                if (isHousing)
                {
                    int cap = el.FindPropertyRelative("HousingCapacity")?.intValue ?? 0;
                    RegistryEditorStyles.DrawBadge($"\u2302{cap}", new Color(0.3f, 0.7f, 0.4f));
                }
                if (isTownHall)
                    RegistryEditorStyles.DrawBadge("TH", new Color(0.8f, 0.6f, 0.2f));
                if (isCastle)
                    RegistryEditorStyles.DrawBadge("CST", new Color(0.55f, 0.45f, 0.2f));

                if ((BuildingCategory)category == BuildingCategory.Industrial)
                {
                    string resourceId = el.FindPropertyRelative("IndustrialResourceId")?.stringValue ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(resourceId))
                        RegistryEditorStyles.DrawBadge($"R:{resourceId}", new Color(0.35f, 0.65f, 0.85f));
                }
                EditorGUILayout.LabelField(prefab ? $"\u2713" : "\u2717", EditorStyles.miniLabel, GUILayout.Width(16));
                if (DrawDeleteBtn()) removeIdx = i;
                EditorGUILayout.EndHorizontal();

                // FATAL: prefab відсутній
                if (!prefab)
                    DrawMissingPrefabRow(i, id, el, "Prefab", BuildingPrefabFolder, _pendingBldSprites, _bldSO);

                if (isOpen)
                {
                    DrawBuildingInlineEditBox(el);
                    _bldSO.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                { int ci = i; string cid = id; HandleRowContextClick(
                    new Rect(bldRowRect.x, bldRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - bldRowRect.y),
                    () => ShowMoveMenu(cid, Tab.Buildings, ci));
                  HandleRowDragStart(
                    new Rect(bldRowRect.x, bldRowRect.y, position.width, GUILayoutUtility.GetLastRect().yMax - bldRowRect.y),
                    cid, Tab.Buildings, ci); }
            }

            HandleRemove(blds, removeIdx, "Id", _bldSO);
            DrawDeleteAllButton(blds, "Id", _bldSO, "будівель");
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
                _newBldPrefab   = (GameObject)EditorGUILayout.ObjectField("Prefab (override)", _newBldPrefab, typeof(GameObject), false);
                if (!_newBldPrefab && !_newBldSprite)
                    EditorGUILayout.HelpBox("Prefab буде створено автоматично (порожній).", MessageType.Info);

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Економіка", EditorStyles.boldLabel);
                _newBldWorkers   = EditorGUILayout.IntField(new GUIContent("Робітники", "Кількість робітників для роботи будівлі. 0 = не потребує."), Mathf.Max(0, _newBldWorkers));
                _newBldPriority  = EditorGUILayout.IntField(new GUIContent("Пріоритет", "Пріоритет розподілу робітників (вищий = важливіший)."), Mathf.Max(0, _newBldPriority));

                if (_newBldCategory == BuildingCategory.Civilian)
                {
                    _newBldIsHousing = EditorGUILayout.Toggle(new GUIContent("Житлова", "Чи надає ця будівля житло мешканцям."), _newBldIsHousing);
                    if (_newBldIsHousing)
                        _newBldHousingCap = EditorGUILayout.IntField(new GUIContent("Ліміт жителів", "Макс. кількість мешканців у цій будівлі."), Mathf.Max(0, _newBldHousingCap));
                }
                else
                {
                    _newBldIsHousing = false;
                    _newBldHousingCap = 0;
                }

                DrawNewBuildingClassSettings();

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
            el.FindPropertyRelative("RequiredWorkers").intValue = _newBldWorkers;
            el.FindPropertyRelative("EconomyPriority").intValue = _newBldPriority;
            el.FindPropertyRelative("IsHousing").boolValue = _newBldIsHousing;
            el.FindPropertyRelative("HousingCapacity").intValue = _newBldHousingCap;
            el.FindPropertyRelative("IsTownHall").boolValue = _newBldIsTownHall;
            el.FindPropertyRelative("IsCastle").boolValue = _newBldIsCastle;
            el.FindPropertyRelative("IndustrialResourceId").stringValue = (_newBldIndustrialResourceId ?? string.Empty).Trim();
            _bldSO.ApplyModifiedProperties();
            SaveAndNotify($"Будівлю '{id}' створено");
            _newBldId = ""; _newBldName = ""; _newBldSprite = null; _newBldPrefab = null;
            _newBldWorkers = 0; _newBldPriority = 0; _newBldIsHousing = false; _newBldHousingCap = 0; _newBldIsTownHall = false; _newBldIsCastle = false; _newBldIndustrialResourceId = string.Empty;
        }

        private void DrawBuildingInlineEditBox(SerializedProperty el)
        {
            EditorGUILayout.Space(2);
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(el.FindPropertyRelative("Id"), new GUIContent("ID"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("DisplayName"), new GUIContent("Display Name"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("Category"), new GUIContent("Category"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("Prefab"), new GUIContent("Prefab"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("RequiredWorkers"), new GUIContent("Робітники"));
            EditorGUILayout.PropertyField(el.FindPropertyRelative("EconomyPriority"), new GUIContent("Пріоритет економіки"));

            DrawBuildingClassSettings(el);

            var categoryProp = el.FindPropertyRelative("Category");
            var category = categoryProp != null ? (BuildingCategory)categoryProp.enumValueIndex : BuildingCategory.Civilian;

            var isHousingProp = el.FindPropertyRelative("IsHousing");
            var housingCapProp = el.FindPropertyRelative("HousingCapacity");
            if (category == BuildingCategory.Civilian)
            {
                EditorGUILayout.PropertyField(isHousingProp, new GUIContent("Житлова"));
                if (isHousingProp != null && isHousingProp.boolValue)
                    EditorGUILayout.PropertyField(housingCapProp, new GUIContent("Ліміт жителів"));
            }
            else
            {
                if (isHousingProp != null) isHousingProp.boolValue = false;
                if (housingCapProp != null) housingCapProp.intValue = 0;
            }

            EditorGUI.indentLevel--;
        }

        private void DrawBuildingClassSettings(SerializedProperty el)
        {
            var categoryProp = el.FindPropertyRelative("Category");
            var isTownHallProp = el.FindPropertyRelative("IsTownHall");
            var isCastleProp = el.FindPropertyRelative("IsCastle");
            var industrialResourceProp = el.FindPropertyRelative("IndustrialResourceId");

            if (categoryProp == null) return;
            var category = (BuildingCategory)categoryProp.enumValueIndex;

            if (category == BuildingCategory.Civilian)
            {
                var selected = CivilianBuildingType.Regular;
                if (isTownHallProp != null && isTownHallProp.boolValue)
                    selected = CivilianBuildingType.TownHall;
                else if (isCastleProp != null && isCastleProp.boolValue)
                    selected = CivilianBuildingType.Castle;

                selected = (CivilianBuildingType)EditorGUILayout.EnumPopup(
                    new GUIContent("Тип цивільної", "Для Civilian-класу: звичайна, ратуша або замок."),
                    selected);

                if (isTownHallProp != null)
                    isTownHallProp.boolValue = selected == CivilianBuildingType.TownHall;
                if (isCastleProp != null)
                    isCastleProp.boolValue = selected == CivilianBuildingType.Castle;
            }
            else
            {
                if (isTownHallProp != null)
                    isTownHallProp.boolValue = false;
                if (isCastleProp != null)
                    isCastleProp.boolValue = false;
            }

            if (category == BuildingCategory.Industrial)
            {
                DrawIdPopup(
                    industrialResourceProp,
                    new GUIContent("Ресурс (resourceId)", "Який ресурс пов'язаний з індустріальною будівлею."),
                    EconomyResourceEditorShared.LoadResourceIds());
            }
        }

        private void DrawNewBuildingClassSettings()
        {
            if (_newBldCategory == BuildingCategory.Civilian)
            {
                var selected = CivilianBuildingType.Regular;
                if (_newBldIsTownHall)
                    selected = CivilianBuildingType.TownHall;
                else if (_newBldIsCastle)
                    selected = CivilianBuildingType.Castle;

                selected = (CivilianBuildingType)EditorGUILayout.EnumPopup(
                    new GUIContent("Тип цивільної", "Для Civilian-класу: звичайна, ратуша або замок."),
                    selected);
                _newBldIsTownHall = selected == CivilianBuildingType.TownHall;
                _newBldIsCastle = selected == CivilianBuildingType.Castle;
            }
            else
            {
                _newBldIsTownHall = false;
                _newBldIsCastle = false;
            }

            if (_newBldCategory == BuildingCategory.Industrial)
            {
                _newBldIndustrialResourceId = DrawIdPopup(
                    new GUIContent("Ресурс (resourceId)", "Який ресурс виробляє/обробляє індустріальна будівля."),
                    _newBldIndustrialResourceId,
                    EconomyResourceEditorShared.LoadResourceIds());
            }
            else
            {
                _newBldIndustrialResourceId = string.Empty;
            }
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

            _bldReg = (BuildingRegistrySO)EditorGUILayout.ObjectField("Registry Asset", _bldReg, typeof(BuildingRegistrySO), false);
            if (_bldReg && _bldSO?.targetObject != _bldReg) RefreshSOs();
            if (!_bldReg) { EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO або натисніть ⟳.", MessageType.Warning); return; }

            DrawAssetPath(_bldReg);
            RegistryEditorStyles.DrawSeparator();

            var collections = _bldSO.FindProperty("WallCollections");
            var buildingIds = CollectIds(_bldSO.FindProperty("Buildings"), "Id");
            int count = collections?.arraySize ?? 0;
            EditorGUILayout.LabelField($"Колекції стін ({count})", RegistryEditorStyles.SubHeader);

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
                    var sr = prefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (sr != null && sr.sprite != null && iconProp.objectReferenceValue != sr.sprite)
                    {
                        iconProp.objectReferenceValue = sr.sprite;
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

            _economyDb = (EconomyDatabaseSO)EditorGUILayout.ObjectField(
                "Economy Database",
                _economyDb,
                typeof(EconomyDatabaseSO),
                false);

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

        private void SaveAndNotify(string msg)
        {
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent($"\u2713 {msg}"));
        }

        private static Color CategoryColor(int idx) => idx switch
        {
            0 => new Color(0.85f, 0.30f, 0.30f), // Military
            1 => new Color(0.35f, 0.70f, 0.35f), // Civilian
            2 => new Color(0.40f, 0.50f, 0.80f), // Industrial
            3 => new Color(0.65f, 0.55f, 0.30f), // Walls
            _ => Color.grey,
        };

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
            foreach (Tab target in Enum.GetValues(typeof(Tab)))
            {
                if (target == source) continue;
                Tab t = target;
                int idx = sourceIndex;
                menu.AddItem(new GUIContent($"Перемістити в → {TabLabels[(int)t].Trim()}"),
                    false, () => MoveEntry(id, source, idx, t));
            }
            menu.ShowAsContext();
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
                EditorGUILayout.HelpBox("Немає елементів для відображення.", MessageType.None);
            }
            else
            {
                string prevCategory = null;
                foreach (var entry in entries)
                {
                    if (entry.Category != prevCategory)
                    {
                        if (prevCategory != null) EditorGUILayout.Space(2f);
                        EditorGUILayout.LabelField(entry.Category, RegistryEditorStyles.SubHeader);
                        prevCategory = entry.Category;
                    }

                    bool wasSelected = _bulkSelectedIds.Contains(entry.Id);
                    GUIStyle rowStyle = wasSelected ? RegistryEditorStyles.CardAlt : RegistryEditorStyles.Card;
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
                    DrawSpritePreview(entry.PreviewSprite, 96f);
                }
                else
                {
                    var texture = AssetPreview.GetAssetPreview(_bulkPreviewObject) ?? AssetPreview.GetMiniThumbnail(_bulkPreviewObject);
                    if (texture != null)
                    {
                        Rect previewRect = GUILayoutUtility.GetRect(96f, 96f, GUILayout.Width(96f));
                        GUI.Box(previewRect, GUIContent.none);
                        GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit);
                    }
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

        private static void DrawSpritePreview(Sprite sprite, float size)
        {
            if (sprite == null)
                return;

            Rect previewRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size));
            GUI.Box(previewRect, GUIContent.none);

            Texture preview = AssetPreview.GetAssetPreview(sprite);
            if (preview == null)
                preview = AssetPreview.GetMiniThumbnail(sprite);
            if (preview != null)
                GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit, true);
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
                        var sr = previewGo.GetComponentInChildren<SpriteRenderer>(true);
                        previewSprite = sr != null ? sr.sprite : null;
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

            DeleteFromSO(_tileSO, "_definitions", "_id");
            DeleteFromSO(_objSO, "_definitions", "_id");
            DeleteFromSO(_unitSO, "Configs", "TypeId");
            DeleteFromSO(_bldSO, "Buildings", "Id");

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
