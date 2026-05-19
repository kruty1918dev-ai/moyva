using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    public sealed class BuildingDesignerWindow : EditorWindow
    {
        private enum WorkspaceMode
        {
            Definition = 0,
            Placement = 1,
            PlayerView = 2,
        }

        private enum ScenarioPaintMode
        {
            Candidate = 0,
            Occupied = 1,
            Pending = 2,
            TownHall = 3,
            Castle = 4,
            Fog = 5,
            Terrain = 6,
            Erase = 7,
        }

        private enum PreviewToolMode
        {
            Build = 0,
            Move = 1,
            Erase = 2,
            Paint = 3,
        }

        private enum BuildingCreationStep
        {
            Identity = 0,
            Gameplay = 1,
            Cost = 2,
            Visual = 3,
            Confirm = 4,
        }

        private enum BuildingCreationRole
        {
            SettlementCenter = 0,
            Housing = 1,
            Storage = 2,
            Production = 3,
            Defense = 4,
            Wall = 5,
            Support = 6,
        }

        private enum TileIssueKind
        {
            None = 0,
            Occupied = 1,
            Pending = 2,
            Fog = 3,
            MissingInfluence = 4,
            InfluenceOverlap = 5,
            Spacing = 6,
            MissingResources = 7,
            TerrainMismatch = 8,
            ScenarioConfiguration = 9,
        }

        private sealed class PlacementPreview
        {
            public bool IsAllowed;
            public BuildingPlacementEvaluationResult Evaluation;
            public string OverrideMessage;
            public bool IsGateReplacement;
        }

        private sealed class TileDiagnostic
        {
            public Vector2Int Tile;
            public bool CanBuild;
            public bool HasWarning;
            public TileIssueKind MainIssue;
            public string Title;
            public string Reason;
            public string Fix;
            public readonly List<TileIssueKind> Issues = new List<TileIssueKind>();
        }

        private const string PrefKeyRegistryGuid = "Moyva.BuildingDesigner.RegistryGuid";
        private const string BuildingDragPayloadKey = "Moyva.BuildingDesigner.DragBuildingId";
        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";
        private const string RegistryLockKey = "BuildingRegistrySO";
        private const string ListWidthPrefsKey = "Moyva.BuildingDesigner.Layout.ListWidth";
        private const string MainWidthPrefsKey = "Moyva.BuildingDesigner.Layout.MainWidth";
        private const string RightWidthPrefsKey = "Moyva.BuildingDesigner.Layout.RightWidth";
        private const string VerticalTabsInlinePrefsKey = "Moyva.BuildingDesigner.Layout.VerticalTabsInline";
        private const float SplitterWidth = 6f;
        private const float HorizontalLayoutPadding = 36f;
        private const float MinListPanelWidth = 260f;
        private const float MinMainPanelWidth = 420f;
        private const float MinRightPanelWidth = 300f;
        private const float DefaultListPanelWidth = 320f;
        private const float DefaultMainPanelWidth = 520f;
        private const float DefaultRightPanelWidth = 360f;
        private const float MinimumHorizontalLayoutWidth = MinListPanelWidth + MinMainPanelWidth + MinRightPanelWidth + SplitterWidth * 2f + HorizontalLayoutPadding;

        private static readonly string[] WorkspaceLabels = { "Будівля", "Симуляція", "Гравець" };
        private static readonly string[] VerticalLayoutLabels = { "Список", "Робоча зона", "Інфо" };
        private static readonly string[] CategoryFilterLabels = { "Усі", "Military", "Civilian", "Industrial", "Walls" };
        private static readonly string[] PaintModeLabels = { "Тест", "Об'єкт", "Pending", "Ратуша", "Замок", "Туман", "Тайл", "Стерти" };
        private static readonly string[] PreviewToolLabels = { "Build", "Move", "Erase", "Paint" };
        private static readonly string[] CreationStepLabels = { "Назва", "Роль", "Вартість", "Візуал", "Створити" };
        private static readonly string[] TerrainLabels = { "grass", "forest-dense", "water", "stone", "road" };

        private static readonly Color PanelColor = new Color(0.18f, 0.19f, 0.20f, 1f);
        private static readonly Color SelectedColor = new Color(0.22f, 0.45f, 0.50f, 1f);
        private static readonly Color AllowedColor = new Color(0.23f, 0.58f, 0.33f, 1f);
        private static readonly Color BlockedColor = new Color(0.72f, 0.27f, 0.24f, 1f);
        private static readonly Color WarningColor = new Color(0.83f, 0.56f, 0.18f, 1f);
        private static readonly Color InfluenceColor = new Color(0.95f, 0.78f, 0.24f, 0.18f);
        private static readonly Color TownHallInfluenceColor = new Color(0.96f, 0.85f, 0.10f, 0.28f);
        private static readonly Color FogRevealColor = new Color(0.20f, 0.62f, 0.92f, 0.16f);
        private static readonly Color ZoneIntersectionColor = new Color(0.50f, 0.34f, 0.82f, 0.23f);

        private BuildingRegistrySO _registry;
        private SerializedObject _registryObject;
        private WorkspaceMode _workspaceMode = WorkspaceMode.Placement;
        private Vector2 _listScroll;
        private Vector2 _detailsScroll;
        private Vector2 _rightScroll;
        private Vector2 _resourceBudgetScroll;
        private float _listPanelWidth = DefaultListPanelWidth;
        private float _mainPanelWidth = DefaultMainPanelWidth;
        private float _rightPanelWidth = DefaultRightPanelWidth;
        private int _verticalLayoutTab;
        private bool _verticalLayoutTabsInlineMenu;
        private string _search = string.Empty;
        private int _categoryFilterIndex;
        private int _selectedIndex;
        private bool _showUsageGuide = true;
        private bool _showTechnicalBuildingFields;
        private bool _showOnlyBuildable;
        private bool _showCreationWizard = true;
        private bool _creationWizardCompactMode;
        private float _creationWizardContentHeight = 270f;
        private Vector2 _creationWizardScroll;
        private int _dragSourceBuildingIndex = -1;
        private DesignerPresetLibrarySO _designerPresetLibrary;
        private int _selectedBuildingPresetIndex;
        private bool _diffBeforeSaveEnabled = true;
        private string _lastSavedRegistrySnapshot = string.Empty;
        private readonly EditorAssetStaleTracker _staleTracker = new EditorAssetStaleTracker();
        private readonly EditorWindowPerformanceProfiler _perfProfiler = new EditorWindowPerformanceProfiler();

        private string _newBuildingId = "new-building";
        private string _newBuildingName = "Нова будівля";
        private BuildingCategory _newBuildingCategory = BuildingCategory.Civilian;
        private BuildingCreationStep _newBuildingStep;
        private BuildingCreationRole _newBuildingRole = BuildingCreationRole.Support;
        private int _newBuildingCostLogs = 20;
        private int _newBuildingCostBoards = 0;
        private int _newBuildingCostStone = 0;
        private int _newBuildingCostMoney = 50;
        private int _newBuildingBuildTurns = 2;
        private int _newBuildingWorkers;
        private int _newBuildingInfluence = 0;
        private int _newBuildingFogReveal = 0;
        private int _newBuildingHousingCapacity = 4;
        private int _newBuildingStorageCapacity = 200;
        private string _newBuildingProductionResourceId = "logs";
        private Sprite _newBuildingSprite;
        private GameObject _newBuildingPrefab;

        private int _scenarioWidth = 15;
        private int _scenarioHeight = 11;
        private int _scenarioMinSpacing;
        private int _scenarioTownHallRadius = 4;
        private bool _scenarioFogEnabled = true;
        private bool _showCoordinates;
        private PreviewToolMode _previewToolMode = PreviewToolMode.Build;
        private ScenarioPaintMode _paintMode = ScenarioPaintMode.Candidate;
        private string _dragPreviewBuildingId;
        private bool _showAdvancedScenarioTools;
        private int _terrainIndex;
        private string _paintBuildingId = string.Empty;
        private int _paintBuildingSelectionIndex;
        private Vector2Int _candidatePosition = new Vector2Int(7, 5);
        private Vector2Int? _hoveredScenarioTile;
        private bool _isMovingScenarioPlacement;
        private bool _movingPlacementPending;
        private Vector2Int _movingPlacementSourceTile;
        private string _movingPlacementBuildingId;
        private readonly Dictionary<Vector2Int, string> _scenarioOccupants = new Dictionary<Vector2Int, string>();
        private readonly List<BuildingPlacementSimulationEntry> _scenarioPending = new List<BuildingPlacementSimulationEntry>();
        private readonly HashSet<Vector2Int> _scenarioFog = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, string> _scenarioTerrain = new Dictionary<Vector2Int, string>();
        private readonly Dictionary<string, int> _scenarioResourceStock = new Dictionary<string, int>(StringComparer.Ordinal);

        [MenuItem("Moyva/Tools/Building Designer", priority = 32)]
        public static void Open()
        {
            var window = GetWindow<BuildingDesignerWindow>("Building Designer");
            window.minSize = new Vector2(1180f, 680f);
            window.Show();
            window.Focus();
        }

        [MenuItem("Moyva/Construction/Building Designer", priority = 0)]
        public static void OpenConstructionMenu()
        {
            Open();
        }

        private void OnEnable()
        {
            LoadRegistryPreference();
            if (_registry == null)
                _registry = FindFirstRegistry();

            _designerPresetLibrary ??= MoyvaProjectEditorContext.GetOrFindFirst<DesignerPresetLibrarySO>();

            LoadLayoutPreferences();
            RefreshSerializedObject();
            ClampScenarioCandidate();
            RefreshSavedSnapshot();
            _staleTracker.Capture(_registry);
        }

        private void OnDisable()
        {
            SaveLayoutPreferences();
            SaveRegistryPreference();
        }

        private void OnGUI()
        {
            _perfProfiler.BeginFrame();
            _registryObject?.Update();

            _perfProfiler.BeginSection("Toolbar");
            DrawToolbar();
            _perfProfiler.EndSection("Toolbar");

            _perfProfiler.BeginSection("BodyLayout");
            DrawAdaptiveLayout();
            _perfProfiler.EndSection("BodyLayout");

            if (_registryObject != null && _registryObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_registry);
                Repaint();
            }

            if (_staleTracker.IsStale(_registry))
                EditorWindowSharedUI.DrawWarning("Дані застарілі: BuildingRegistry змінено зовні. Оновіть або перезбережіть.", MessageType.Warning);

            EditorGUILayout.LabelField(_perfProfiler.BuildSummary(), EditorStyles.miniLabel);
            _perfProfiler.EndFrame();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("BUILDING DESIGNER", EditorStyles.boldLabel, GUILayout.Width(150f));

                EditorGUI.BeginChangeCheck();
                _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField(
                    EditorTooltipStandard.Content(
                        "Registry",
                        "Вибір BuildingRegistrySO для редагування.",
                        "Це джерело правди для меню будівництва, preview та placement-перевірок."),
                    _registry,
                    typeof(BuildingRegistrySO),
                    false,
                    GUILayout.Width(240f));
                if (EditorGUI.EndChangeCheck())
                {
                    MoyvaProjectEditorContext.Set(_registry);
                    SaveRegistryPreference();
                    RefreshSerializedObject();
                    _selectedIndex = 0;
                    _staleTracker.Capture(_registry);
                }

                if (GUILayout.Button(IconContent("d_Refresh", "Auto", "Автоматично знайти BuildingRegistrySO у проєкті."), EditorStyles.toolbarButton, GUILayout.Width(42f)))
                {
                    _registry = FindFirstRegistry();
                    MoyvaProjectEditorContext.Set(_registry);
                    RefreshSerializedObject();
                    _staleTracker.Capture(_registry);
                }

                if (GUILayout.Button(IconContent("", "Clr", "Очистити активний реєстр будівель у цьому вікні."), EditorStyles.toolbarButton, GUILayout.Width(42f)))
                {
                    _registry = null;
                    MoyvaProjectEditorContext.Set<BuildingRegistrySO>(null);
                    RefreshSerializedObject();
                    SaveRegistryPreference();
                }

                GUILayout.Space(6f);
                _workspaceMode = (WorkspaceMode)GUILayout.Toolbar((int)_workspaceMode, WorkspaceLabels, EditorStyles.toolbarButton, GUILayout.Width(310f));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(IconContent("d_Project", "Hub", "Відкрити Registry Hub."), EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    EditorApplication.ExecuteMenuItem("Moyva/Tools/Registry Hub");

                if (GUILayout.Button(IconContent("SaveActive", "Save", "Зберегти зміни в BuildingRegistrySO зараз."), EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    SaveRegistry();

                if (GUILayout.Button(IconContent("d_RectTransform Icon", "Layout", "Скинути ручні ширини панелей Building Designer."), EditorStyles.toolbarButton, GUILayout.Width(64f)))
                    ResetLayoutPreferences();

                _diffBeforeSaveEnabled = GUILayout.Toggle(
                    _diffBeforeSaveEnabled,
                    EditorTooltipStandard.Content("Diff", "Показує diff полів перед Save реєстру.", "Зменшує ризик випадково зберегти некоректні зміни будівель."),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(58f));

                bool unlocked = EditorRegistryWriteLock.IsUnlocked(RegistryLockKey);
                bool nextUnlocked = GUILayout.Toggle(
                    unlocked,
                    IconContent("", "Unl", "Увімкнути редагування BuildingRegistrySO."),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(42f));
                if (nextUnlocked != unlocked)
                    EditorRegistryWriteLock.SetUnlocked(RegistryLockKey, nextUnlocked);
            }
        }

        private void DrawAdaptiveLayout()
        {
            bool useVerticalLayout = position.width < MinimumHorizontalLayoutWidth;
            if (useVerticalLayout)
                DrawVerticalLayout();
            else
                DrawHorizontalLayout();
        }

        private void DrawHorizontalLayout()
        {
            ClampLayoutWidths();

            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                _perfProfiler.BeginSection("List");
                DrawBuildingList(GUILayout.Width(_listPanelWidth), GUILayout.ExpandHeight(true));
                _perfProfiler.EndSection("List");

                DrawColumnSplitter(ref _listPanelWidth, ref _mainPanelWidth, MinListPanelWidth, MinMainPanelWidth, ListWidthPrefsKey, MainWidthPrefsKey);

                _perfProfiler.BeginSection("Main");
                DrawMainPanel(GUILayout.Width(_mainPanelWidth), GUILayout.MinWidth(MinMainPanelWidth), GUILayout.ExpandHeight(true));
                _perfProfiler.EndSection("Main");

                DrawColumnSplitter(ref _mainPanelWidth, ref _rightPanelWidth, MinMainPanelWidth, MinRightPanelWidth, MainWidthPrefsKey, RightWidthPrefsKey);

                _perfProfiler.BeginSection("Right");
                DrawRightPanel(GUILayout.Width(_rightPanelWidth), GUILayout.MinWidth(MinRightPanelWidth), GUILayout.ExpandHeight(true));
                _perfProfiler.EndSection("Right");
            }
        }

        private void DrawVerticalLayout()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawVerticalLayoutTabSelector();

                if (_verticalLayoutTab == 0)
                    DrawBuildingList(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                else if (_verticalLayoutTab == 1)
                    DrawMainPanel(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                else
                    DrawRightPanel(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            }
        }

        private void DrawVerticalLayoutTabSelector()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();

                if (_verticalLayoutTabsInlineMenu)
                {
                    if (GUILayout.Button(IconContent("d_FilterByLabel", "Tabs", "Розгорнути вкладки у сегментований toolbar."), EditorStyles.toolbarButton, GUILayout.Width(54f)))
                        _verticalLayoutTabsInlineMenu = false;

                    int nextTab = EditorGUILayout.Popup(
                        Mathf.Clamp(_verticalLayoutTab, 0, VerticalLayoutLabels.Length - 1),
                        VerticalLayoutLabels,
                        EditorStyles.toolbarPopup,
                        GUILayout.Width(Mathf.Clamp(position.width - 120f, 150f, 230f)));

                    if (nextTab != _verticalLayoutTab)
                        _verticalLayoutTab = nextTab;
                }
                else
                {
                    _verticalLayoutTab = GUILayout.Toolbar(
                        Mathf.Clamp(_verticalLayoutTab, 0, VerticalLayoutLabels.Length - 1),
                        VerticalLayoutLabels,
                        EditorStyles.toolbarButton,
                        GUILayout.Width(290f));

                    if (GUILayout.Button(IconContent("d_FilterByLabel", "Menu", "Згорнути вкладки у компактне меню."), EditorStyles.toolbarButton, GUILayout.Width(56f)))
                        _verticalLayoutTabsInlineMenu = true;
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void LoadLayoutPreferences()
        {
            _listPanelWidth = EditorPrefs.GetFloat(ListWidthPrefsKey, DefaultListPanelWidth);
            _mainPanelWidth = EditorPrefs.GetFloat(MainWidthPrefsKey, DefaultMainPanelWidth);
            _rightPanelWidth = EditorPrefs.GetFloat(RightWidthPrefsKey, DefaultRightPanelWidth);
            _verticalLayoutTabsInlineMenu = EditorPrefs.GetBool(VerticalTabsInlinePrefsKey, false);
            ClampLayoutWidths();
        }

        private void SaveLayoutPreferences()
        {
            ClampLayoutWidths();
            EditorPrefs.SetFloat(ListWidthPrefsKey, _listPanelWidth);
            EditorPrefs.SetFloat(MainWidthPrefsKey, _mainPanelWidth);
            EditorPrefs.SetFloat(RightWidthPrefsKey, _rightPanelWidth);
            EditorPrefs.SetBool(VerticalTabsInlinePrefsKey, _verticalLayoutTabsInlineMenu);
        }

        private void ResetLayoutPreferences()
        {
            _listPanelWidth = DefaultListPanelWidth;
            _mainPanelWidth = DefaultMainPanelWidth;
            _rightPanelWidth = DefaultRightPanelWidth;
            _verticalLayoutTabsInlineMenu = false;
            SaveLayoutPreferences();
            Repaint();
        }

        private void ClampLayoutWidths()
        {
            NormalizeThreeColumnWidths(ref _listPanelWidth, ref _mainPanelWidth, ref _rightPanelWidth, MinListPanelWidth, MinMainPanelWidth, MinRightPanelWidth);
        }

        private float ResolveThreeColumnContentWidth()
        {
            float minTotalWidth = MinListPanelWidth + MinMainPanelWidth + MinRightPanelWidth;
            return Mathf.Max(minTotalWidth, position.width - SplitterWidth * 2f - HorizontalLayoutPadding);
        }

        private void NormalizeThreeColumnWidths(ref float leftWidth, ref float middleWidth, ref float rightWidth, float leftMinWidth, float middleMinWidth, float rightMinWidth)
        {
            float availableWidth = ResolveThreeColumnContentWidth();
            float minTotalWidth = leftMinWidth + middleMinWidth + rightMinWidth;
            if (availableWidth <= minTotalWidth)
            {
                leftWidth = Mathf.Max(leftMinWidth, leftWidth);
                middleWidth = Mathf.Max(middleMinWidth, middleWidth);
                rightWidth = Mathf.Max(rightMinWidth, rightWidth);
                return;
            }

            leftWidth = Mathf.Max(leftMinWidth, leftWidth);
            middleWidth = Mathf.Max(middleMinWidth, middleWidth);
            rightWidth = Mathf.Max(rightMinWidth, rightWidth);

            float availableExtraWidth = availableWidth - minTotalWidth;
            float currentExtraWidth = leftWidth - leftMinWidth + middleWidth - middleMinWidth + rightWidth - rightMinWidth;
            if (currentExtraWidth <= 0.5f)
            {
                leftWidth = leftMinWidth + availableExtraWidth * 0.24f;
                middleWidth = middleMinWidth + availableExtraWidth * 0.50f;
                rightWidth = availableWidth - leftWidth - middleWidth;
                return;
            }

            leftWidth = leftMinWidth + availableExtraWidth * ((leftWidth - leftMinWidth) / currentExtraWidth);
            middleWidth = middleMinWidth + availableExtraWidth * ((middleWidth - middleMinWidth) / currentExtraWidth);
            rightWidth = Mathf.Max(rightMinWidth, availableWidth - leftWidth - middleWidth);
        }

        private void DrawColumnSplitter(ref float leftWidth, ref float rightWidth, float leftMinWidth, float rightMinWidth, string leftPreferenceKey, string rightPreferenceKey)
        {
            Rect rect = GUILayoutUtility.GetRect(SplitterWidth, 1f, GUILayout.Width(SplitterWidth), GUILayout.ExpandHeight(true));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

            int controlId = GUIUtility.GetControlID(FocusType.Passive, rect);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (rect.Contains(current.mousePosition) && current.button == 0)
                    {
                        GUIUtility.hotControl = controlId;
                        current.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlId)
                    {
                        float appliedDelta = Mathf.Clamp(current.delta.x, leftMinWidth - leftWidth, rightWidth - rightMinWidth);
                        leftWidth += appliedDelta;
                        rightWidth -= appliedDelta;
                        Repaint();
                        current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        EditorPrefs.SetFloat(leftPreferenceKey, leftWidth);
                        EditorPrefs.SetFloat(rightPreferenceKey, rightWidth);
                        current.Use();
                    }
                    break;
            }

            Color color = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.16f)
                : new Color(0f, 0f, 0f, 0.18f);
            EditorGUI.DrawRect(new Rect(rect.center.x - 1f, rect.y + 8f, 2f, Mathf.Max(0f, rect.height - 16f)), color);
        }

        private void DrawBuildingList(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(PanelStyle(), options))
            {
                DrawSectionTitle("Будівлі", "Список джерел правди для меню будівництва, preview та симуляції.");
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _categoryFilterIndex = EditorGUILayout.Popup(_categoryFilterIndex, CategoryFilterLabels);
                    _showOnlyBuildable = EditorGUILayout.ToggleLeft("Лише ті, що можна збудувати", _showOnlyBuildable, GUILayout.Width(196f));
                }

                if (_registry == null)
                {
                    EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO або натисніть refresh.", MessageType.Warning);
                    return;
                }

                var buildingsProperty = _registryObject?.FindProperty("Buildings");
                if (buildingsProperty == null)
                {
                    EditorGUILayout.HelpBox("У реєстрі не знайдено масив Buildings.", MessageType.Error);
                    return;
                }

                DrawCreateBox(buildingsProperty);
                DrawPaletteResourceBudget();

                EditorGUILayout.Space(6f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Записи: {buildingsProperty.arraySize}", EditorStyles.miniBoldLabel);
                    if (GUILayout.Button("Дублювати", EditorStyles.miniButton, GUILayout.Width(82f)))
                        DuplicateSelectedBuilding(buildingsProperty);
                    if (GUILayout.Button("Видалити", EditorStyles.miniButton, GUILayout.Width(72f)))
                        DeleteSelectedBuilding(buildingsProperty);
                }

                _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
                for (int index = 0; index < buildingsProperty.arraySize; index++)
                {
                    var buildingProperty = buildingsProperty.GetArrayElementAtIndex(index);
                    if (!PassesListFilter(buildingProperty))
                        continue;

                    DrawBuildingListRow(buildingProperty, index);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawCreateBox(SerializedProperty buildingsProperty)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                _showCreationWizard = EditorGUILayout.Foldout(_showCreationWizard, "Майстер нової будівлі", true, EditorStyles.foldoutHeader);
                if (!_showCreationWizard)
                    return;

                using (new EditorGUILayout.HorizontalScope())
                {
                    _creationWizardCompactMode = EditorGUILayout.ToggleLeft("Compact", _creationWizardCompactMode, GUILayout.Width(84f));
                    _creationWizardContentHeight = EditorGUILayout.Slider("Висота", _creationWizardContentHeight, 150f, 520f);
                }

                EditorGUILayout.Space(4f);
                _newBuildingStep = (BuildingCreationStep)GUILayout.Toolbar((int)_newBuildingStep, CreationStepLabels, GUILayout.Height(24f));
                EditorGUILayout.Space(4f);

                _creationWizardScroll = EditorGUILayout.BeginScrollView(_creationWizardScroll, GUILayout.Height(_creationWizardContentHeight));
                switch (_newBuildingStep)
                {
                    case BuildingCreationStep.Identity:
                        DrawBuildingCreationIdentityStep();
                        break;
                    case BuildingCreationStep.Gameplay:
                        DrawBuildingCreationGameplayStep();
                        break;
                    case BuildingCreationStep.Cost:
                        DrawBuildingCreationCostStep();
                        break;
                    case BuildingCreationStep.Visual:
                        DrawBuildingCreationVisualStep();
                        break;
                    default:
                        DrawBuildingCreationConfirmStep(buildingsProperty);
                        break;
                }
                EditorGUILayout.EndScrollView();

                DrawBuildingCreationNavigation(buildingsProperty);
            }
        }

        private void DrawBuildingCreationIdentityStep()
        {
            EditorGUILayout.LabelField("1. Хто це в грі", EditorStyles.boldLabel);
            _newBuildingName = EditorGUILayout.TextField("Назва", _newBuildingName);

            EditorGUILayout.LabelField("Тип", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawCreationCategoryButton(BuildingCategory.Civilian, "Civilian");
                DrawCreationCategoryButton(BuildingCategory.Industrial, "Industrial");
                DrawCreationCategoryButton(BuildingCategory.Military, "Military");
                DrawCreationCategoryButton(BuildingCategory.Walls, "Walls");
            }

            if (!_creationWizardCompactMode)
                DrawCreationPreviewCard();
        }

        private void DrawBuildingCreationGameplayStep()
        {
            EditorGUILayout.LabelField("2. Роль і наслідки", EditorStyles.boldLabel);
            if (!_creationWizardCompactMode)
                EditorGUILayout.LabelField("Оберіть роль, а редактор сам підбере базову семантику будівлі.", EditorStyles.wordWrappedMiniLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawCreationRoleButton(BuildingCreationRole.SettlementCenter, "Центр поселення", "Створює контроль території і стартову зону забудови.");
                DrawCreationRoleButton(BuildingCreationRole.Housing, "Житло", "Дає населення і майбутню робочу силу.");
                DrawCreationRoleButton(BuildingCreationRole.Storage, "Склад", "Підсилює економіку через запас ресурсів.");
                DrawCreationRoleButton(BuildingCreationRole.Production, "Виробництво", "Потребує працівників і створює економічну користь.");
                DrawCreationRoleButton(BuildingCreationRole.Defense, "Оборона", "Утримує ключову зону і працює як сильна точка.");
                DrawCreationRoleButton(BuildingCreationRole.Wall, "Стіна", "Формує периметр і блокує напрямки руху.");
                DrawCreationRoleButton(BuildingCreationRole.Support, "Підтримка", "Допоміжна будівля без важкої системної ролі.");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _newBuildingWorkers = EditorGUILayout.IntSlider("Працівники", _newBuildingWorkers, 0, 12);
                _newBuildingBuildTurns = EditorGUILayout.IntSlider("Час", _newBuildingBuildTurns, 1, 12);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _newBuildingInfluence = EditorGUILayout.IntSlider("Вплив", _newBuildingInfluence, 0, 12);
                _newBuildingFogReveal = EditorGUILayout.IntSlider("Туман", _newBuildingFogReveal, 0, 8);
            }

            if (_newBuildingRole == BuildingCreationRole.Housing)
                _newBuildingHousingCapacity = EditorGUILayout.IntSlider("Місткість житла", _newBuildingHousingCapacity, 1, 40);
            if (_newBuildingRole == BuildingCreationRole.Storage)
                _newBuildingStorageCapacity = EditorGUILayout.IntSlider("Місткість складу", _newBuildingStorageCapacity, 50, 2000);
            if (_newBuildingRole == BuildingCreationRole.Production)
                _newBuildingProductionResourceId = EditorGUILayout.TextField("Ресурс виробництва", _newBuildingProductionResourceId);
        }

        private void DrawBuildingCreationCostStep()
        {
            EditorGUILayout.LabelField("3. Вартість", EditorStyles.boldLabel);
            if (!_creationWizardCompactMode)
                EditorGUILayout.LabelField("Задайте базову ціну без технічного списку ресурсів.", EditorStyles.wordWrappedMiniLabel);
            _newBuildingCostLogs = EditorGUILayout.IntSlider("Дерево", _newBuildingCostLogs, 0, 500);
            _newBuildingCostBoards = EditorGUILayout.IntSlider("Дошки", _newBuildingCostBoards, 0, 500);
            _newBuildingCostStone = EditorGUILayout.IntSlider("Камінь", _newBuildingCostStone, 0, 500);
            _newBuildingCostMoney = EditorGUILayout.IntSlider("Гроші", _newBuildingCostMoney, 0, 2000);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMetricCard("Сума", BuildNewBuildingCostSummary(), WarningColor);
                DrawMetricCard("Час", $"~{_newBuildingBuildTurns} ходи", SelectedColor);
            }
        }

        private void DrawBuildingCreationVisualStep()
        {
            EditorGUILayout.LabelField("4. Візуал", EditorStyles.boldLabel);
            DrawNewBuildingSpritePreview(_creationWizardCompactMode ? 120f : 170f);
            _newBuildingSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newBuildingSprite, typeof(Sprite), false);
            Rect newBuildingSpriteRect = GUILayoutUtility.GetLastRect();
            if (SpriteImportDragDropPolicy.HandleDrop(newBuildingSpriteRect, ref _newBuildingSprite, "BuildingDesigner.NewBuildingSprite"))
                Repaint();
            SpriteImportDragDropPolicy.EnsureAllowedSprite(ref _newBuildingSprite, "BuildingDesigner.NewBuildingSprite");

            _newBuildingPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _newBuildingPrefab, typeof(GameObject), false);
        }

        private void DrawBuildingCreationConfirmStep(SerializedProperty buildingsProperty)
        {
            EditorGUILayout.LabelField("5. Перевірка перед створенням", EditorStyles.boldLabel);
            if (!_creationWizardCompactMode)
                DrawCreationPreviewCard();
            string suggestedId = GenerateUniqueId(SuggestIdFromDisplayName(_newBuildingName), buildingsProperty);
            _newBuildingId = suggestedId;
            EditorGUILayout.LabelField("ID буде створено автоматично і сховано з основної картки.", EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawBuildingCreationNavigation(SerializedProperty buildingsProperty)
        {
            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginDisabledGroup(_newBuildingStep == BuildingCreationStep.Identity);
                if (GUILayout.Button("Назад", GUILayout.Height(24f)))
                    _newBuildingStep = (BuildingCreationStep)Mathf.Max(0, (int)_newBuildingStep - 1);
                EditorGUI.EndDisabledGroup();

                if (_newBuildingStep != BuildingCreationStep.Confirm)
                {
                    if (GUILayout.Button("Далі", GUILayout.Height(24f)))
                        _newBuildingStep = (BuildingCreationStep)Mathf.Min((int)BuildingCreationStep.Confirm, (int)_newBuildingStep + 1);
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newBuildingName));
                    if (GUILayout.Button("Створити будівлю", PrimaryButtonStyle()))
                        CreateBuilding(buildingsProperty, _newBuildingId);
                    EditorGUI.EndDisabledGroup();
                }
            }
        }

        private void DrawBuildingListRow(SerializedProperty buildingProperty, int index)
        {
            string id = GetString(buildingProperty, "Id", "?");
            string displayName = GetString(buildingProperty, "DisplayName", string.Empty);
            var category = (BuildingCategory)GetEnumIndex(buildingProperty, "Category");
            var definition = GetRuntimeDefinition(index);
            var placement = EvaluatePlacement(definition, _candidatePosition);
            bool affordable = IsAffordable(definition, out string affordabilityReason);
            string placementReason = GetPlacementReason(placement);
            string blockedReason = affordable ? placementReason : affordabilityReason;
            var issues = definition != null ? BuildingModuleValidation.Validate(definition) : Array.Empty<BuildingValidationIssue>();
            bool hasErrors = BuildingModuleValidation.HasErrors(issues);

            if (_showOnlyBuildable && !affordable)
                return;

            Rect row = GUILayoutUtility.GetRect(0f, 92f, GUILayout.ExpandWidth(true));
            bool selected = index == _selectedIndex;
            Color background = selected
                ? new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, 0.58f)
                : (EditorGUIUtility.isProSkin ? new Color(0.21f, 0.22f, 0.23f) : new Color(0.86f, 0.87f, 0.88f));

            EditorGUI.DrawRect(row, background);
            DrawBorder(row, selected ? SelectedColor : new Color(0f, 0f, 0f, 0.18f), selected ? 2f : 1f);

            Rect iconRect = new Rect(row.x + 8f, row.y + 8f, 42f, 42f);
            DrawSprite(iconRect, ExtractSprite(definition));

            float badgeX = row.xMax - 218f;
            Rect titleRect = new Rect(iconRect.xMax + 8f, row.y + 7f, Mathf.Max(80f, badgeX - iconRect.xMax - 16f), 20f);
            GUI.Label(titleRect, string.IsNullOrWhiteSpace(displayName) ? "Без назви" : displayName, EditorStyles.boldLabel);
            GUI.Label(new Rect(titleRect.x, titleRect.yMax + 1f, titleRect.width, 16f), BuildListSubtitle(definition), EditorStyles.miniLabel);

            string priceText = BuildCostLabel(definition).Replace("Вартість: ", string.Empty);
            string timeText = BuildConstructionTimeLabel(definition).Replace("Час будівництва: ", string.Empty);
            GUI.Label(new Rect(titleRect.x, titleRect.y + 36f, titleRect.width, 15f), $"Ціна: {priceText}", EditorStyles.miniLabel);
            GUI.Label(new Rect(titleRect.x, titleRect.y + 52f, titleRect.width, 15f), $"Час: {timeText}", EditorStyles.miniLabel);
            GUI.Label(new Rect(titleRect.x, titleRect.y + 68f, titleRect.width, 15f), BuildEffectsLabel(definition).Replace("Ефекти: ", string.Empty), EditorStyles.miniLabel);

            DrawBadgeRect(new Rect(badgeX, row.y + 9f, 96f, 18f), category.ToString(), CategoryColor(category));
            DrawBadgeRect(new Rect(badgeX + 100f, row.y + 9f, 54f, 18f), affordable ? "КОШТ" : "НЕМА", affordable ? AllowedColor : WarningColor);
            DrawBadgeRect(new Rect(badgeX + 158f, row.y + 9f, 54f, 18f), placement.IsAllowed ? "ТАЙЛ" : "БЛОК", placement.IsAllowed ? AllowedColor : BlockedColor);
            if (hasErrors)
                DrawBadgeRect(new Rect(badgeX + 100f, row.y + 31f, 68f, 18f), "ERR", BlockedColor);

            GUI.Label(new Rect(badgeX, row.y + 53f, 210f, 30f), string.IsNullOrWhiteSpace(blockedReason) ? "Перетягніть на preview-мапу." : blockedReason, EditorStyles.wordWrappedMiniLabel);

            EditorGUIUtility.AddCursorRect(row, MouseCursor.Link);
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && row.Contains(currentEvent.mousePosition))
            {
                _selectedIndex = index;
                _paintBuildingId = id;
                _dragSourceBuildingIndex = index;
                GUI.FocusControl(null);
                if (currentEvent.clickCount == 2)
                    _workspaceMode = WorkspaceMode.Definition;
                currentEvent.Use();
                Repaint();
            }
            else if (currentEvent.type == EventType.MouseDrag && currentEvent.button == 0 && _dragSourceBuildingIndex == index)
            {
                BeginBuildingDrag(index, id, string.IsNullOrWhiteSpace(displayName) ? id : displayName);
            }
            else if (currentEvent.type == EventType.MouseUp && _dragSourceBuildingIndex == index)
            {
                _dragSourceBuildingIndex = -1;
            }

            GUILayout.Space(3f);
        }

        private void BeginBuildingDrag(int index, string buildingId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            _selectedIndex = index;
            _paintBuildingId = buildingId;
            _workspaceMode = WorkspaceMode.Placement;
            _verticalLayoutTab = 1;
            _dragPreviewBuildingId = buildingId;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(BuildingDragPayloadKey, buildingId);
            DragAndDrop.objectReferences = Array.Empty<UnityEngine.Object>();
            DragAndDrop.StartDrag($"Розмістити: {displayName}");
            Event.current.Use();
            Repaint();
        }

        private void DrawPaletteResourceBudget()
        {
            EnsureScenarioResourceStock();

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Ресурси для sandbox-тесту", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Ці значення визначають, які будівлі зараз можна побудувати за бюджетом.", EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("+100 до всіх", EditorStyles.miniButton, GUILayout.Width(92f)))
                        OffsetScenarioBudget(100);
                    if (GUILayout.Button("Очистити", EditorStyles.miniButton, GUILayout.Width(72f)))
                        ClearScenarioBudget();
                }

                _resourceBudgetScroll = EditorGUILayout.BeginScrollView(_resourceBudgetScroll, GUILayout.Height(120f));
                foreach (var key in new List<string>(_scenarioResourceStock.Keys))
                {
                    int value = _scenarioResourceStock[key];
                    _scenarioResourceStock[key] = EditorGUILayout.IntField(key, Mathf.Max(0, value));
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawMainPanel(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(options))
            {
                if (_registry == null)
                {
                    EditorWindowSharedUI.DrawWarning("Building Designer потребує BuildingRegistrySO.", MessageType.Warning);
                    return;
                }

                _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);
                switch (_workspaceMode)
                {
                    case WorkspaceMode.Definition:
                        DrawDefinitionWorkspace();
                        break;
                    case WorkspaceMode.PlayerView:
                        DrawPlayerWorkspace();
                        break;
                    default:
                        DrawPlacementWorkspace();
                        break;
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawDefinitionWorkspace()
        {
            var buildingProperty = GetSelectedBuildingProperty();
            var definition = GetSelectedDefinition();
            if (buildingProperty == null || definition == null)
            {
                EditorWindowSharedUI.DrawWarning("Оберіть будівлю у списку.", MessageType.Info);
                return;
            }

            DrawSectionTitle("Геймдизайн будівлі", "Роль, вартість, вплив і сценарії перевірки. Технічні поля сховані нижче.");

            DrawBuildingPresetsSection(definition);
            DrawBuildingGameplaySummary(definition);

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("DisplayName"), new GUIContent("Назва у UI", "Текст, який бачить гравець у меню будівництва."));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Category"), new GUIContent("Категорія", "Категорія впливає на фільтри та групування в UI."));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Icon"), new GUIContent("Icon", "Fallback-іконка для menu preview, якщо prefab не має SpriteRenderer."));
            }

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Вартість", EditorStyles.boldLabel);
                BuildingConstructionCostEditorShared.DrawCostList(
                    buildingProperty.FindPropertyRelative("ConstructionCost"),
                    "Додати ресурс");
            }

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                _showTechnicalBuildingFields = EditorGUILayout.Foldout(_showTechnicalBuildingFields, "Технічні поля", true, EditorStyles.foldoutHeader);
                if (_showTechnicalBuildingFields)
                {
                    EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Id"), new GUIContent("ID", "Унікальний код будівлі в реєстрі."));
                    EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Prefab"), new GUIContent("Prefab", "Prefab для runtime preview та ігрового представлення."));
                }
            }

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Правила розміщення", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("UseCustomTownHallRules"), new GUIContent("Кастомні правила центру", "Увімкнути окремий набір правил для town hall / castle логіки."));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("RequireTownHallInRange"), new GUIContent("Потребує центр у радіусі", "Будівля дозволена лише поруч із центром поселення."));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("BlockIfTownHallAlreadyInRange"), new GUIContent("Блокувати поруч з центром", "Забороняє дублювання у зоні впливу центру."));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("TownHallProximityRadiusOverride"), new GUIContent("Override радіусу", "Ручне перевизначення відстані до центру поселення."));
            }

            BuildingModuleEditorShared.DrawModulesSection(
                buildingProperty.FindPropertyRelative("Modules"),
                CardStyle(),
                () => _registryObject.ApplyModifiedProperties());

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Валідація", EditorStyles.boldLabel);
                BuildingModuleEditorShared.DrawValidationIssues(
                    BuildingModuleValidation.Validate(definition),
                    buildingProperty,
                    () => _registryObject.ApplyModifiedProperties());
            }
        }

        private void DrawBuildingPresetsSection(BuildingDefinition selectedDefinition)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Designer Presets", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                _designerPresetLibrary = (DesignerPresetLibrarySO)EditorGUILayout.ObjectField(
                    EditorTooltipStandard.Content("Preset Library", "Обирає спільну бібліотеку preset-ів.", "Дозволяє швидко уніфікувати налаштування будівель між сценаріями."),
                    _designerPresetLibrary,
                    typeof(DesignerPresetLibrarySO),
                    false);
                if (EditorGUI.EndChangeCheck())
                    MoyvaProjectEditorContext.Set(_designerPresetLibrary);

                if (_designerPresetLibrary == null)
                {
                    EditorGUILayout.HelpBox("Призначте DesignerPresetLibrarySO для застосування building preset-ів.", MessageType.Info);
                    return;
                }

                var presets = _designerPresetLibrary.BuildingPresets;
                if (presets == null || presets.Count == 0)
                {
                    EditorGUILayout.HelpBox("У бібліотеці немає Building preset-ів.", MessageType.Warning);
                    return;
                }

                var names = new string[presets.Count];
                for (int index = 0; index < presets.Count; index++)
                {
                    string name = presets[index] != null ? presets[index].Name : string.Empty;
                    names[index] = string.IsNullOrWhiteSpace(name) ? $"Building Preset {index + 1}" : name;
                }

                _selectedBuildingPresetIndex = Mathf.Clamp(_selectedBuildingPresetIndex, 0, presets.Count - 1);
                _selectedBuildingPresetIndex = EditorGUILayout.Popup("Preset", _selectedBuildingPresetIndex, names);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginDisabledGroup(selectedDefinition == null);
                    if (GUILayout.Button("Apply Preset", GUILayout.Height(22f)))
                    {
                        var preset = presets[_selectedBuildingPresetIndex];
                        if (preset == null || preset.Template == null)
                        {
                            EditorUtility.DisplayDialog("Building Preset", "Обраний preset порожній.", "OK");
                        }
                        else
                        {
                            Undo.RecordObject(_registry, $"Building: apply preset {preset.Name}");
                            if (DesignerPresetApplier.ApplyBuildingPreset(preset, selectedDefinition))
                            {
                                RefreshSerializedObject();
                                EditorUtility.SetDirty(_registry);
                                SaveRegistry();
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Ping Library", GUILayout.Height(22f)))
                        EditorGUIUtility.PingObject(_designerPresetLibrary);
                }
            }
        }

        private void DrawPlacementWorkspace()
        {
            var definition = GetSelectedDefinition();
            var previewDefinition = ResolveDragPreviewDefinition(definition);
            DrawSectionTitle("Візуальна діагностика", "Мапа автоматично показує доступність, блокери, туман, вплив і причини проблем.");

            DrawScenarioControls();
            DrawScenarioGrid(previewDefinition);
            DrawPlacementStatus(previewDefinition);
            DrawTileRequirementStatus(previewDefinition);
        }

        private void DrawPlayerWorkspace()
        {
            var definition = GetSelectedDefinition();
            DrawSectionTitle("Ігровий preview", "Те, як будівля читається для гравця: роль, вартість, вплив і сценарний ефект.");
            DrawBuildingGameplaySummary(definition);
            DrawPlayerMenuPreview(definition);
            DrawBuildingFacts(definition);
        }

        private void DrawRightPanel(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(PanelStyle(), options))
            {
                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
                DrawUsageGuide();
                DrawSelectedSummary();
                DrawScenarioLegend();
                DrawAllBuildingsAtCandidate();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawUsageGuide()
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                _showUsageGuide = EditorGUILayout.Foldout(_showUsageGuide, "Як користуватися", true, EditorStyles.foldoutHeader);
                if (!_showUsageGuide)
                    return;

                EditorGUILayout.LabelField("1. Оберіть будівлю і подивіться її роль, вартість і вплив.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("2. Перемикайте Будівля / Симуляція / Гравець у верхній панелі.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("3. Toolbar Build/Move/Erase/Paint працює як scene-інструмент для preview.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("4. Build: клік ставить pending; Move: перетягування розміщення; Erase: клік видаляє.", EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.LabelField("5. ПКМ по тайлу або Delete/Backspace на наведеному тайлі прибирає placement.", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawScenarioControls()
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawStatusBanner("Автоматичні оверлеї увімкнені", SelectedColor);
                EditorGUILayout.LabelField("Редактор сам підсвічує зелені/червоні тайли, блокери, туман, вплив, ресурси і проблеми сценарію.", EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(3f);
                _previewToolMode = (PreviewToolMode)GUILayout.Toolbar((int)_previewToolMode, PreviewToolLabels, GUILayout.Height(24f));
                if (_previewToolMode == PreviewToolMode.Build)
                    EditorGUILayout.LabelField("Режим Build: клік ставить pending будівлю на тайл.", EditorStyles.miniLabel);
                else if (_previewToolMode == PreviewToolMode.Move)
                    EditorGUILayout.LabelField("Режим Move: затисніть ЛКМ на будівлі і перетягніть в нову позицію.", EditorStyles.miniLabel);
                else if (_previewToolMode == PreviewToolMode.Erase)
                    EditorGUILayout.LabelField("Режим Erase: клік по тайлу прибирає preview-розміщення.", EditorStyles.miniLabel);
                else
                    EditorGUILayout.LabelField("Режим Paint: детальне малювання сценарію (туман, terrain, центри).", EditorStyles.miniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _scenarioWidth = EditorGUILayout.IntSlider("Ширина тест-сценарію", _scenarioWidth, 7, 25);
                    _scenarioHeight = EditorGUILayout.IntSlider("Висота тест-сценарію", _scenarioHeight, 7, 19);
                }

                _scenarioMinSpacing = EditorGUILayout.IntSlider("Мінімальна дистанція", _scenarioMinSpacing, 0, 5);
                _scenarioTownHallRadius = EditorGUILayout.IntSlider("Радіус впливу ратуші", _scenarioTownHallRadius, 0, 12);

                if (_previewToolMode == PreviewToolMode.Paint)
                {
                    EditorGUILayout.Space(4f);
                    _paintMode = (ScenarioPaintMode)GUILayout.Toolbar((int)_paintMode, PaintModeLabels);
                    if (_paintMode == ScenarioPaintMode.Terrain)
                        _terrainIndex = EditorGUILayout.Popup("Тип тайла", _terrainIndex, TerrainLabels);
                    if (_paintMode == ScenarioPaintMode.Occupied || _paintMode == ScenarioPaintMode.Pending)
                        DrawScenarioPaintBuildingPicker();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Очистити сценарій"))
                        ClearScenario();
                    if (GUILayout.Button("Очистити поточний тайл"))
                        RemovePlacementAt(_hoveredScenarioTile ?? _candidatePosition);
                    if (GUILayout.Button("Додати центр"))
                        SeedInfluenceCenter();
                    if (GUILayout.Button("Показати всю мапу"))
                        _scenarioFog.Clear();
                }

                _showAdvancedScenarioTools = EditorGUILayout.Foldout(_showAdvancedScenarioTools, "Додаткові інструменти сценарію", true);
                if (_showAdvancedScenarioTools)
                {
                    _scenarioFogEnabled = EditorGUILayout.Toggle("Туман блокує будівництво", _scenarioFogEnabled);
                    _showCoordinates = EditorGUILayout.Toggle("Координати", _showCoordinates);
                }
            }

            ClampScenarioCandidate();
        }

        private void DrawPlacementStatus(BuildingDefinition definition)
        {
            var inspectedTile = _hoveredScenarioTile ?? _candidatePosition;
            var diagnostic = BuildTileDiagnostic(definition, inspectedTile);
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawStatusBanner(diagnostic.Title, diagnostic.CanBuild ? AllowedColor : BlockedColor);
                EditorGUILayout.LabelField($"Тайл: {inspectedTile} | Будівля: {(definition != null ? GetPlayerFacingName(definition) : "-")}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(diagnostic.Reason, EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space(3f);
                DrawBadgeLine(diagnostic.Fix, diagnostic.CanBuild ? AllowedColor : WarningColor);

                if (definition != null)
                {
                    int fogRadius = GetFogDispersalRadius(definition);
                    int influenceRadius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
                    DrawBadgeLine($"Розсіювання туману: {fogRadius} тайл(ів)", fogRadius > 0 ? FogRevealColor : Color.gray);
                    DrawBadgeLine($"Радіус впливу: {influenceRadius} тайл(ів)", influenceRadius > 0 ? TownHallInfluenceColor : Color.gray);
                    DrawBadgeLine($"Тайлів у перетині туману і впливу: {CountZoneIntersectionTiles()}", Color.gray);
                }
            }
        }

        private void DrawScenarioGrid(BuildingDefinition definition)
        {
            float availableWidth = Mathf.Max(320f, position.width - 700f);
            float cellSize = Mathf.Floor(Mathf.Clamp(availableWidth / Mathf.Max(1, _scenarioWidth), 18f, 34f));
            Rect gridRect = GUILayoutUtility.GetRect(_scenarioWidth * cellSize, _scenarioHeight * cellSize, GUILayout.ExpandWidth(false));
            EditorGUI.DrawRect(new Rect(gridRect.x - 1f, gridRect.y - 1f, gridRect.width + 2f, gridRect.height + 2f), new Color(0f, 0f, 0f, 0.35f));

            var currentEvent = Event.current;
            _hoveredScenarioTile = null;
            if (!TryGetDraggedBuildingId(out _))
                _dragPreviewBuildingId = null;
            if (currentEvent.type == EventType.DragExited)
            {
                _dragPreviewBuildingId = null;
                _dragSourceBuildingIndex = -1;
            }

            if (_isMovingScenarioPlacement)
            {
                Vector2Int dropTile;
                bool hasDropTile = TryGetGridTileAtPosition(gridRect, cellSize, currentEvent.mousePosition, out dropTile);
                if (hasDropTile)
                    _candidatePosition = dropTile;

                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                {
                    if (hasDropTile)
                        PlaceScenarioPlacement(dropTile, _movingPlacementBuildingId, _movingPlacementPending);
                    else
                        PlaceScenarioPlacement(_movingPlacementSourceTile, _movingPlacementBuildingId, _movingPlacementPending);

                    EndMovePlacement();
                    currentEvent.Use();
                    Repaint();
                }
            }

            for (int row = 0; row < _scenarioHeight; row++)
            {
                int tileY = _scenarioHeight - 1 - row;
                for (int tileX = 0; tileX < _scenarioWidth; tileX++)
                {
                    var tile = new Vector2Int(tileX, tileY);
                    var cellRect = new Rect(gridRect.x + tileX * cellSize, gridRect.y + row * cellSize, cellSize - 1f, cellSize - 1f);
                    DrawScenarioCell(cellRect, tile, definition);

                    if (!cellRect.Contains(currentEvent.mousePosition))
                        continue;

                    _hoveredScenarioTile = tile;
                    EditorGUIUtility.AddCursorRect(cellRect, MouseCursor.Link);

                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
                    {
                        RemovePlacementAt(tile);
                        currentEvent.Use();
                        continue;
                    }

                    if (HandleScenarioCellDrag(cellRect, tile))
                        continue;

                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        HandleScenarioCellClick(tile, definition);
                        currentEvent.Use();
                    }
                }
            }

            if (_hoveredScenarioTile.HasValue
                && currentEvent.type == EventType.KeyDown
                && (currentEvent.keyCode == KeyCode.Delete || currentEvent.keyCode == KeyCode.Backspace))
            {
                RemovePlacementAt(_hoveredScenarioTile.Value);
                currentEvent.Use();
            }
        }

        private void DrawScenarioCell(Rect rect, Vector2Int tile, BuildingDefinition definition)
        {
            EditorGUI.DrawRect(rect, TerrainColor(GetTerrain(tile)));

            bool isRevealedByFog = IsCoveredByAnyFogReveal(tile);
            bool isInfluenceTile = IsCoveredByAnyInfluence(tile);
            bool isTownHallTile = IsCoveredByTownHallInfluence(tile);

            if (isRevealedByFog)
                EditorGUI.DrawRect(rect, FogRevealColor);

            if (isInfluenceTile)
                EditorGUI.DrawRect(rect, InfluenceColor);

            if (isTownHallTile)
                EditorGUI.DrawRect(rect, TownHallInfluenceColor);

            if (isRevealedByFog && isInfluenceTile)
                EditorGUI.DrawRect(rect, ZoneIntersectionColor);

            var diagnostic = BuildTileDiagnostic(definition, tile);
            if (definition != null)
            {
                EditorGUI.DrawRect(rect, diagnostic.CanBuild
                    ? new Color(0.1f, 0.75f, 0.2f, 0.23f)
                    : new Color(0.85f, 0.15f, 0.12f, 0.20f));
            }

            if (_scenarioOccupants.TryGetValue(tile, out var occupantId))
            {
                EditorGUI.DrawRect(Shrink(rect, 3f), new Color(0.23f, 0.27f, 0.32f, 0.92f));
                GUI.Label(rect, ShortLabel(GetScenarioLabel(occupantId)), CenteredCellStyle());
            }

            if (TryGetPending(tile, out var pendingEntry))
            {
                DrawBorder(rect, new Color(0.35f, 0.75f, 0.95f, 1f), 2f);
                GUI.Label(rect, ShortLabel(GetScenarioLabel(pendingEntry.BuildingId)), CenteredCellStyle());
            }

            if (_scenarioFogEnabled && _scenarioFog.Contains(tile))
                EditorGUI.DrawRect(rect, new Color(0.02f, 0.03f, 0.04f, 0.58f));

            string issueLabel = BuildTileIssueShortLabel(diagnostic.MainIssue);
            if (!string.IsNullOrWhiteSpace(issueLabel))
                DrawBadgeRect(new Rect(rect.x + 2f, rect.y + 2f, Mathf.Min(rect.width - 4f, 30f), 14f), issueLabel, TileIssueColor(diagnostic.MainIssue));

            if (diagnostic.HasWarning && diagnostic.CanBuild)
                DrawBorder(Shrink(rect, 2f), WarningColor, 2f);

            if (tile == _candidatePosition)
            {
                DrawBorder(rect, diagnostic.CanBuild ? Color.green : Color.red, 3f);
            }
            else if (_hoveredScenarioTile.HasValue && _hoveredScenarioTile.Value == tile)
            {
                DrawBorder(rect, Color.white, 2f);
            }
            else
            {
                DrawBorder(rect, new Color(0f, 0f, 0f, 0.22f), 1f);
            }

            if (_showCoordinates)
                GUI.Label(new Rect(rect.x + 2f, rect.y + 1f, rect.width - 4f, 12f), $"{tile.x},{tile.y}", CoordinateStyle());
        }

        private bool HandleScenarioCellDrag(Rect cellRect, Vector2Int tile)
        {
            if (_isMovingScenarioPlacement)
                return false;

            var currentEvent = Event.current;
            if ((currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
                || !cellRect.Contains(currentEvent.mousePosition)
                || !TryGetDraggedBuildingId(out string buildingId))
            {
                return false;
            }

            _dragPreviewBuildingId = buildingId;
            _paintBuildingId = buildingId;
            _candidatePosition = tile;
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                PlaceDraggedBuildingPreview(tile, buildingId);
                DragAndDrop.SetGenericData(BuildingDragPayloadKey, null);
                _dragPreviewBuildingId = null;
                _dragSourceBuildingIndex = -1;
            }

            currentEvent.Use();
            Repaint();
            return true;
        }

        private void HandleScenarioCellClick(Vector2Int tile, BuildingDefinition definition)
        {
            switch (_previewToolMode)
            {
                case PreviewToolMode.Build:
                    PlaceScenarioPlacement(tile, ResolveBuildPlacementId(definition), pending: true);
                    break;
                case PreviewToolMode.Move:
                    BeginMovePlacement(tile);
                    break;
                case PreviewToolMode.Erase:
                    RemovePlacementAt(tile);
                    break;
                default:
                    ApplyPaint(tile);
                    break;
            }
        }

        private string ResolveBuildPlacementId(BuildingDefinition definition)
        {
            if (definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                return definition.Id;

            return ResolvePaintBuildingId();
        }

        private bool BeginMovePlacement(Vector2Int tile)
        {
            if (TryGetPlacementAt(tile, out string buildingId, out bool pending))
            {
                _isMovingScenarioPlacement = true;
                _movingPlacementPending = pending;
                _movingPlacementSourceTile = tile;
                _movingPlacementBuildingId = buildingId;
                RemovePlacementAt(tile);
                return true;
            }

            return false;
        }

        private void EndMovePlacement()
        {
            _isMovingScenarioPlacement = false;
            _movingPlacementPending = false;
            _movingPlacementSourceTile = default;
            _movingPlacementBuildingId = null;
        }

        private bool TryGetPlacementAt(Vector2Int tile, out string buildingId, out bool pending)
        {
            if (TryGetPending(tile, out var pendingEntry))
            {
                buildingId = pendingEntry.BuildingId;
                pending = true;
                return !string.IsNullOrWhiteSpace(buildingId);
            }

            if (_scenarioOccupants.TryGetValue(tile, out var occupantId) && !string.IsNullOrWhiteSpace(occupantId))
            {
                buildingId = occupantId;
                pending = false;
                return true;
            }

            buildingId = null;
            pending = false;
            return false;
        }

        private void PlaceScenarioPlacement(Vector2Int tile, string buildingId, bool pending)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            SelectBuildingById(buildingId);
            _candidatePosition = tile;
            _paintBuildingId = buildingId;
            RemovePlacementAt(tile);
            if (pending)
                _scenarioPending.Add(new BuildingPlacementSimulationEntry(tile, buildingId));
            else
                _scenarioOccupants[tile] = buildingId;
        }

        private void RemovePlacementAt(Vector2Int tile)
        {
            _scenarioOccupants.Remove(tile);
            RemovePending(tile);
            Repaint();
        }

        private bool TryGetGridTileAtPosition(Rect gridRect, float cellSize, Vector2 position, out Vector2Int tile)
        {
            if (!gridRect.Contains(position))
            {
                tile = default;
                return false;
            }

            int tileX = Mathf.Clamp(Mathf.FloorToInt((position.x - gridRect.x) / cellSize), 0, _scenarioWidth - 1);
            int rowFromTop = Mathf.Clamp(Mathf.FloorToInt((position.y - gridRect.y) / cellSize), 0, _scenarioHeight - 1);
            int tileY = _scenarioHeight - 1 - rowFromTop;
            tile = new Vector2Int(tileX, tileY);
            return true;
        }

        private void PlaceDraggedBuildingPreview(Vector2Int tile, string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            PlaceScenarioPlacement(tile, buildingId, pending: true);
        }

        private void DrawTileRequirementStatus(BuildingDefinition definition)
        {
            if (definition == null)
                return;

            var requirements = BuildingDefinitionCapabilities.GetTileRequirements(definition);
            if (requirements.Length == 0)
                return;

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Вимоги до тайлів для роботи", EditorStyles.boldLabel);
                for (int requirementIndex = 0; requirementIndex < requirements.Length; requirementIndex++)
                {
                    var requirement = requirements[requirementIndex];
                    if (requirement == null)
                        continue;

                    int count = CountTerrainNear(_candidatePosition, requirement.TileId, requirement.Radius);
                    bool passed = count >= requirement.MinimumTileCount;
                    DrawBadgeLine(
                        $"{requirement.TileId}: {count}/{requirement.MinimumTileCount} у радіусі {requirement.Radius}",
                        passed ? AllowedColor : WarningColor);
                }
            }
        }

        private void DrawPlayerMenuPreview(BuildingDefinition definition)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                if (definition == null)
                {
                    EditorGUILayout.HelpBox("Оберіть будівлю.", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    foreach (BuildingCategory category in Enum.GetValues(typeof(BuildingCategory)))
                    {
                        Color previousColor = GUI.backgroundColor;
                        GUI.backgroundColor = category == definition.Category ? SelectedColor : Color.white;
                        GUILayout.Button(category.ToString(), GUILayout.Height(24f));
                        GUI.backgroundColor = previousColor;
                    }
                }

                Rect buttonRect = GUILayoutUtility.GetRect(240f, 150f, GUILayout.ExpandWidth(false));
                EditorGUI.DrawRect(buttonRect, EditorGUIUtility.isProSkin ? new Color(0.15f, 0.17f, 0.19f) : new Color(0.86f, 0.88f, 0.90f));
                DrawBorder(buttonRect, SelectedColor, 2f);

                var icon = ExtractSprite(definition);
                Rect iconRect = new Rect(buttonRect.x + 12f, buttonRect.y + 12f, 80f, 80f);
                if (icon != null)
                    DrawSprite(iconRect, icon);
                else
                    GUI.Label(iconRect, "Без іконки", CenteredCellStyle());

                Rect textRect = new Rect(buttonRect.x + 102f, buttonRect.y + 18f, buttonRect.width - 114f, 90f);
                GUI.Label(textRect, GetPlayerFacingName(definition), PlayerButtonTitleStyle());
                GUI.Label(new Rect(textRect.x, textRect.y + 50f, textRect.width, 18f), definition.Category.ToString(), EditorStyles.miniLabel);

                var preview = EvaluatePlacement(definition, _candidatePosition);
                DrawStatusBanner(preview.IsAllowed ? "Preview розміщення: дозволено" : "Preview розміщення: заблоковано", preview.IsAllowed ? AllowedColor : BlockedColor);
            }
        }

        private void DrawBuildingFacts(BuildingDefinition definition)
        {
            if (definition == null)
                return;

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Пояснення для геймдизайну", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(BuildPurposeText(definition), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(BuildBenefitText(definition), EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(BuildSynergyText(definition), EditorStyles.wordWrappedMiniLabel);
            }
        }

        private void DrawBuildingGameplaySummary(BuildingDefinition definition)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                if (definition == null)
                {
                    EditorGUILayout.HelpBox("Оберіть будівлю, щоб побачити її роль, вартість і вплив.", MessageType.Info);
                    return;
                }

                DrawBuildingDesignHero(definition);
                EditorGUILayout.Space(6f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMetricCard("Вартість", BuildCompactCostLabel(definition), WarningColor);
                    DrawMetricCard("Час", BuildConstructionTurnsLabel(definition), SelectedColor);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMetricCard("Працівники", BuildWorkersValue(definition), CategoryColor(definition.Category));
                    DrawMetricCard("Вплив", BuildInfluenceValue(definition), TownHallInfluenceColor);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMetricCard("Туман", BuildFogValue(definition), FogRevealColor);
                    DrawMetricCard("Дає", BuildPlayerGainShort(definition), AllowedColor);
                }

                DrawDesignerInfoRow("Користь для геймплею", BuildBenefitText(definition), AllowedColor);
                DrawDesignerInfoRow("Чому важлива", BuildBuildingImportanceText(definition), SelectedColor);
                DrawDesignerInfoRow("Синергія", BuildSynergyText(definition), CategoryColor(definition.Category));

                DrawBuildingRoleAssessment(definition);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Відкрити Economy Designer", GUILayout.Height(22f)))
                        EditorApplication.ExecuteMenuItem("Moyva/Tools/Designers/Economy Designer");

                    if (GUILayout.Button("Відкрити Registry Hub", GUILayout.Height(22f)))
                        EditorApplication.ExecuteMenuItem("Moyva/Tools/Registry Hub");
                }
            }
        }

        private void DrawBuildingDesignHero(BuildingDefinition definition)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 118f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.16f, 0.17f) : new Color(0.88f, 0.90f, 0.92f));
            DrawBorder(rect, CategoryColor(definition.Category), 2f);

            Rect iconRect = new Rect(rect.x + 12f, rect.y + 12f, 88f, 88f);
            DrawSprite(iconRect, ExtractSprite(definition));

            Rect textRect = new Rect(iconRect.xMax + 14f, rect.y + 12f, rect.width - 126f, 92f);
            GUI.Label(new Rect(textRect.x, textRect.y, textRect.width, 26f), GetPlayerFacingName(definition), PlayerButtonTitleStyle());
            DrawBadgeRect(new Rect(textRect.x, textRect.y + 32f, 112f, 18f), BuildDesignerTypeLabel(definition), CategoryColor(definition.Category));
            GUI.Label(new Rect(textRect.x, textRect.y + 56f, textRect.width, 38f), BuildPurposeText(definition), EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawBuildingRoleAssessment(BuildingDefinition definition)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Візуальна оцінка ролі", EditorStyles.miniBoldLabel);
                DrawRoleScore("Економіка", ResolveEconomyScore(definition), AllowedColor);
                DrawRoleScore("Територія", ResolveTerritoryScore(definition), TownHallInfluenceColor);
                DrawRoleScore("Логістика", ResolveLogisticsScore(definition), SelectedColor);
                DrawRoleScore("Оборона", ResolveDefenseScore(definition), BlockedColor);
                DrawRoleScore("Виробництво", ResolveProductionScore(definition), WarningColor);
            }
        }

        private static void DrawDesignerInfoRow(string title, string value, Color accent)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawBadgeLine(title, accent);
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawMetricCard(string title, string value, Color accent)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(110f)))
            {
                Rect bar = GUILayoutUtility.GetRect(0f, 4f, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(bar, accent);
                EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(value, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawRoleScore(string label, int score, Color color)
        {
            score = Mathf.Clamp(score, 0, 5);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(92f));
                Rect rect = GUILayoutUtility.GetRect(0f, 13f, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.16f));
                Rect fill = new Rect(rect.x, rect.y, rect.width * (score / 5f), rect.height);
                EditorGUI.DrawRect(fill, color);
                GUI.Label(rect, score <= 0 ? "-" : new string('|', score), CenteredCellStyle());
            }
        }

        private static string BuildCompactCostLabel(BuildingDefinition definition)
        {
            var cost = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            if (cost == null || cost.Count == 0)
                return "безкоштовно";

            int total = 0;
            int types = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                if (cost[i] == null || cost[i].Amount <= 0)
                    continue;
                total += cost[i].Amount;
                types++;
            }

            return total <= 0 ? "безкоштовно" : $"{total} / {types} тип.";
        }

        private static string BuildWorkersValue(BuildingDefinition definition)
        {
            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            return workers <= 0 ? "не потребує" : $"до {workers}";
        }

        private string BuildInfluenceValue(BuildingDefinition definition)
        {
            int influence = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
            return influence <= 0 ? "немає" : $"{influence} тайл.";
        }

        private static string BuildFogValue(BuildingDefinition definition)
        {
            int fog = GetFogDispersalRadius(definition);
            return fog <= 0 ? "не відкриває" : $"{fog} тайл.";
        }

        private static string BuildPlayerGainShort(BuildingDefinition definition)
        {
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return $"+{BuildingDefinitionCapabilities.GetHousingCapacity(definition)} житла";
            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "запас ресурсів";
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return "виробництво";
            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return "контроль";
            if (definition != null && definition.Category == BuildingCategory.Walls)
                return "периметр";
            return "підтримка";
        }

        private static string BuildBuildingImportanceText(BuildingDefinition definition)
        {
            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return "Це ядро поселення: воно пояснює, звідки місто росте і які тайли стають доступними.";
            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return "Це сильна точка контролю: вона стабілізує оборону і резервує територію.";
            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Це економічний буфер: він зменшує ризик зупинки будівництва через нестачу ресурсів.";
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return "Це база росту населення: без житла економіці бракує майбутньої робочої сили.";
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return "Це виробничий вузол: він перетворює працівників і ресурси на довгострокову користь.";
            if (definition != null && definition.Category == BuildingCategory.Walls)
                return "Це частина оборонного контуру: вона формує межі, проходи і безпечні напрямки.";
            return "Це підтримуюча споруда, яка робить планування поселення гнучкішим.";
        }

        private static int ResolveEconomyScore(BuildingDefinition definition)
        {
            int score = 0;
            if (BuildingDefinitionCapabilities.IsHousing(definition)) score += 2;
            if (BuildingDefinitionCapabilities.IsWarehouse(definition)) score += 3;
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0) score += 2;
            return Mathf.Clamp(score, 0, 5);
        }

        private static int ResolveTerritoryScore(BuildingDefinition definition)
        {
            int influence = BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0);
            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return Mathf.Clamp(3 + influence / 3, 0, 5);
            return influence > 0 ? 2 : 0;
        }

        private static int ResolveLogisticsScore(BuildingDefinition definition)
        {
            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return 5;
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return 2;
            return 0;
        }

        private static int ResolveDefenseScore(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;
            if (definition.Category == BuildingCategory.Walls)
                return 5;
            if (definition.Category == BuildingCategory.Military || BuildingDefinitionCapabilities.IsCastle(definition))
                return 4;
            return 0;
        }

        private static int ResolveProductionScore(BuildingDefinition definition)
        {
            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            if (workers <= 0)
                return 0;
            return Mathf.Clamp(2 + workers / 2, 0, 5);
        }

        private static string BuildDesignerTypeLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Economy";

            if (definition.Category == BuildingCategory.Industrial)
                return "Industrial";

            if (definition.Category == BuildingCategory.Military || definition.Category == BuildingCategory.Walls)
                return "Military";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Storage";

            return "Economy";
        }

        private static string BuildPurposeText(BuildingDefinition definition)
        {
            if (definition == null)
                return "Будівля підсилює розвиток поселення у вибраному напрямку.";

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return "Центр поселення: задає ядро забудови та відкриває контроль території.";

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return "Оборонний центр: утримує ключову зону і стабілізує фронт.";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Складська опора: накопичує ресурси для безперервного розвитку економіки.";

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return "Житлова інфраструктура: підтримує зростання населення та розширення робочої сили.";

            if (definition.Category == BuildingCategory.Walls)
                return "Лінія оборони: формує периметр і контролює напрямки руху ворога.";

            return "Економічний вузол: виконує спеціалізовану роль у виробничому ланцюгу.";
        }

        private static string BuildBenefitText(BuildingDefinition definition)
        {
            if (definition == null)
                return "Додає гнучкість плануванню міста.";

            int housing = BuildingDefinitionCapabilities.GetHousingCapacity(definition);
            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);

            if (housing > 0)
                return $"Збільшує місткість населення на {housing} і прискорює розвиток поселення.";

            if (workers > 0)
                return $"Дає стабільний виробничий ефект за наявності до {workers} працівників.";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Підвищує стійкість економіки через зберігання і перерозподіл ресурсів.";

            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return "Розширює контроль мапи, відкриває нові точки забудови і знижує ризик блокування розвитку.";

            return "Покращує контроль простору і тактичну керованість території.";
        }

        private static string BuildSynergyText(BuildingDefinition definition)
        {
            if (definition == null)
                return "Добре комбінується з базовими економічними та оборонними будівлями.";

            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return "Синергія: житло, склади, виробничі та оборонні будівлі в межах контрольованої зони.";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Синергія: виробничі будівлі, що споживають/створюють ресурси, та центри поселення.";

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return "Синергія: виробничі будівлі і центр поселення для кращого розподілу робочої сили.";

            if (definition.Category == BuildingCategory.Walls)
                return "Синергія: ворота, замок/ратуша та вузли оборони на підступах до міста.";

            return "Синергія: склади, житло і центр поселення для стабільного економічного циклу.";
        }

        private static string BuildConstructionTurnsLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "~1 хід";

            int workers = Mathf.Max(0, BuildingDefinitionCapabilities.GetRequiredWorkers(definition));
            int costTypes = BuildingDefinitionCapabilities.GetConstructionCost(definition)?.Count ?? 0;
            int influence = Mathf.Max(0, BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0));
            int turns = Mathf.Clamp(1 + costTypes + (workers / 2) + (influence / 3), 1, 12);
            return $"~{turns} ходів";
        }

        private string BuildInfluenceSummary(BuildingDefinition definition)
        {
            int fogRadius = GetFogDispersalRadius(definition);
            int influenceRadius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
            return $"Fog reveal radius: {fogRadius} | Influence radius: {influenceRadius}";
        }

        private void DrawConstructionCostPresentation(BuildingDefinition definition)
        {
            var costs = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            var costMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < costs.Count; i++)
            {
                var entry = costs[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;

                string resourceId = entry.ResourceId.Trim();
                if (costMap.ContainsKey(resourceId))
                    costMap[resourceId] += entry.Amount;
                else
                    costMap[resourceId] = entry.Amount;
            }

            DrawCostLine("Logs", ResolveCostAmount(costMap, "logs", "log", "wood", "timber"));
            DrawCostLine("Boards", ResolveCostAmount(costMap, "boards", "board", "planks", "plank"));
            DrawCostLine("Stone", ResolveCostAmount(costMap, "stone", "rocks"));
            DrawCostLine("Iron", ResolveCostAmount(costMap, "iron", "metal"));
            DrawCostLine("Money", ResolveCostAmount(costMap, "money", "gold", "coins", "coin"));
        }

        private static void DrawCostLine(string label, int amount)
        {
            string value = amount > 0 ? amount.ToString() : "-";
            EditorGUILayout.LabelField($"• {label}: {value}", EditorStyles.wordWrappedMiniLabel);
        }

        private static int ResolveCostAmount(Dictionary<string, int> costMap, params string[] aliases)
        {
            int amount = 0;
            for (int i = 0; i < aliases.Length; i++)
            {
                string alias = aliases[i];
                foreach (var pair in costMap)
                {
                    if (pair.Key.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0)
                        amount += pair.Value;
                }
            }

            return amount;
        }

        private static string BuildGameplayRoleLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Роль: невідома";

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return "Роль: центр поселення";

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return "Роль: замок / сильний центр";

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return "Роль: житло для населення";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Роль: сховище ресурсів";

            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return "Роль: виробнича будівля";

            if (definition.Category == BuildingCategory.Walls)
                return "Роль: оборона / кордон";

            return "Роль: допоміжна будівля";
        }

        private static string BuildGameplayImpactLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Вплив: невідомий";

            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return "Вплив: відкриває або контролює центр поселення";

            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            int housing = BuildingDefinitionCapabilities.GetHousingCapacity(definition);

            if (workers > 0)
                return $"Вплив: потребує {workers} працівників і генерує економічне навантаження";

            if (housing > 0)
                return $"Вплив: додає {housing} житлової місткості";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Вплив: змінює зберігання і логістику ресурсів";

            return "Вплив: змінює карту без прямої виробничої ролі";
        }

        private static string BuildFogLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Туман: невідомо";

            int radius = GetFogDispersalRadius(definition);
            if (radius <= 0)
                return "Туман: не впливає";

            return $"Туман: рекомендуваний радіус розсіювання {radius}";
        }

        private static string BuildCostLabel(BuildingDefinition definition)
        {
            var cost = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            if (cost == null || cost.Count == 0)
                return "Вартість: безкоштовна";

            int total = 0;
            for (int i = 0; i < cost.Count; i++)
            {
                if (cost[i] == null)
                    continue;

                total += Mathf.Max(0, cost[i].Amount);
            }

            return $"Вартість: {total} ресурсних одиниць ({cost.Count} тип(ів))";
        }

        private static string BuildConstructionTimeLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Час будівництва: невідомо";

            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            int costTypes = BuildingDefinitionCapabilities.GetConstructionCost(definition)?.Count ?? 0;
            int influence = BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0);
            int estimatedSeconds = 12 + costTypes * 8 + workers * 4 + Mathf.Clamp(influence, 0, 8);
            return $"Час будівництва: ~{estimatedSeconds} c (sandbox-оцінка)";
        }

        private static string BuildWorkersLabel(BuildingDefinition definition)
        {
            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            return workers <= 0 ? "Працівники: не потребує" : $"Працівники: до {workers}";
        }

        private static string BuildEffectsLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Ефекти: невідомо";

            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return "Ефекти: формує контроль території та стабілізує розширення поселення";

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return "Ефекти: підвищує ліміт населення і темп росту поселення";

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return "Ефекти: покращує зберігання, зменшує втрати ресурсів у логістиці";

            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return "Ефекти: додає виробничий вузол економіки";

            return "Ефекти: локально змінює можливості розміщення";
        }

        private bool IsAffordable(BuildingDefinition definition, out string reason)
        {
            reason = string.Empty;
            if (definition == null)
            {
                reason = "Немає опису будівлі.";
                return false;
            }

            EnsureScenarioResourceStock();
            var cost = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            if (cost == null || cost.Count == 0)
                return true;

            for (int i = 0; i < cost.Count; i++)
            {
                var entry = cost[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;

                _scenarioResourceStock.TryGetValue(entry.ResourceId, out int available);
                if (available < entry.Amount)
                {
                    reason = $"Брак ресурсу: {entry.ResourceId} ({available}/{entry.Amount}).";
                    return false;
                }
            }

            return true;
        }

        private static string GetPlacementReason(PlacementPreview preview)
        {
            if (preview == null)
                return "Немає даних placement-перевірки.";

            if (preview.IsAllowed)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(preview.OverrideMessage))
                return preview.OverrideMessage;

            if (preview.Evaluation != null && preview.Evaluation.Blockers != null && preview.Evaluation.Blockers.Count > 0)
                return FormatBlocker(preview.Evaluation.Blockers[0]);

            return "Поточний тайл не проходить умови розміщення.";
        }

        private TileDiagnostic BuildTileDiagnostic(BuildingDefinition definition, Vector2Int tile)
        {
            var diagnostic = new TileDiagnostic
            {
                Tile = tile,
                CanBuild = false,
                MainIssue = TileIssueKind.None,
                Title = "Немає будівлі",
                Reason = "Оберіть будівлю, щоб редактор показав доступність і причини блокування.",
                Fix = "Оберіть будівлю у списку.",
            };

            if (definition == null)
                return diagnostic;

            var placement = EvaluatePlacement(definition, tile);
            diagnostic.CanBuild = placement.IsAllowed;

            AddPlacementIssues(diagnostic, placement);
            AddResourceIssue(diagnostic, definition);
            AddTerrainIssues(diagnostic, definition, tile);
            AddScenarioContextIssues(diagnostic, tile);
            ResolveDiagnosticText(diagnostic);
            return diagnostic;
        }

        private void AddPlacementIssues(TileDiagnostic diagnostic, PlacementPreview placement)
        {
            if (diagnostic == null || placement == null || placement.IsAllowed)
                return;

            if (!string.IsNullOrWhiteSpace(placement.OverrideMessage))
            {
                AddDiagnosticIssue(diagnostic, TileIssueKind.ScenarioConfiguration);
                return;
            }

            if (placement.Evaluation == null || placement.Evaluation.Blockers == null)
            {
                AddDiagnosticIssue(diagnostic, TileIssueKind.ScenarioConfiguration);
                return;
            }

            for (int i = 0; i < placement.Evaluation.Blockers.Count; i++)
            {
                var blocker = placement.Evaluation.Blockers[i];
                if (blocker == null)
                    continue;

                switch (blocker.Kind)
                {
                    case BuildingPlacementBlockerKind.OccupiedTile:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.Occupied);
                        break;
                    case BuildingPlacementBlockerKind.Spacing:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.Spacing);
                        break;
                    case BuildingPlacementBlockerKind.Fog:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.Fog);
                        break;
                    case BuildingPlacementBlockerKind.InfluenceRequired:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.MissingInfluence);
                        break;
                    case BuildingPlacementBlockerKind.InfluenceOverlap:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.InfluenceOverlap);
                        break;
                    default:
                        AddDiagnosticIssue(diagnostic, TileIssueKind.ScenarioConfiguration);
                        break;
                }
            }
        }

        private void AddResourceIssue(TileDiagnostic diagnostic, BuildingDefinition definition)
        {
            if (diagnostic == null || definition == null)
                return;

            if (!IsAffordable(definition, out _))
            {
                AddDiagnosticIssue(diagnostic, TileIssueKind.MissingResources);
                diagnostic.CanBuild = false;
            }
        }

        private void AddTerrainIssues(TileDiagnostic diagnostic, BuildingDefinition definition, Vector2Int tile)
        {
            if (diagnostic == null || definition == null)
                return;

            var requirements = BuildingDefinitionCapabilities.GetTileRequirements(definition);
            for (int i = 0; i < requirements.Length; i++)
            {
                var requirement = requirements[i];
                if (requirement == null)
                    continue;

                int count = CountTerrainNear(tile, requirement.TileId, requirement.Radius);
                if (count < requirement.MinimumTileCount)
                {
                    AddDiagnosticIssue(diagnostic, TileIssueKind.TerrainMismatch);
                    diagnostic.HasWarning = true;
                }
            }
        }

        private void AddScenarioContextIssues(TileDiagnostic diagnostic, Vector2Int tile)
        {
            if (diagnostic == null)
                return;

            if (TryGetPending(tile, out _))
                AddDiagnosticIssue(diagnostic, TileIssueKind.Pending);
            if (_scenarioFogEnabled && _scenarioFog.Contains(tile))
                AddDiagnosticIssue(diagnostic, TileIssueKind.Fog);
        }

        private static void AddDiagnosticIssue(TileDiagnostic diagnostic, TileIssueKind issue)
        {
            if (diagnostic == null || issue == TileIssueKind.None || diagnostic.Issues.Contains(issue))
                return;

            diagnostic.Issues.Add(issue);
            if (diagnostic.MainIssue == TileIssueKind.None)
                diagnostic.MainIssue = issue;
        }

        private static void ResolveDiagnosticText(TileDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return;

            if (diagnostic.Issues.Count == 0 && diagnostic.CanBuild)
            {
                diagnostic.Title = "Можна будувати";
                diagnostic.Reason = "Тайл проходить правила placement і бюджет сценарію дозволяє будівництво.";
                diagnostic.Fix = "Це хороша тестова позиція.";
                return;
            }

            if (diagnostic.Issues.Count == 0)
            {
                diagnostic.Title = "Потрібна перевірка";
                diagnostic.Reason = "Редактор не отримав конкретної причини блокування.";
                diagnostic.Fix = "Перевірте сценарій, реєстр і базові правила будівлі.";
                diagnostic.MainIssue = TileIssueKind.ScenarioConfiguration;
                return;
            }

            diagnostic.Title = diagnostic.CanBuild ? "Є попередження" : "Заблоковано";
            diagnostic.Reason = HumanizeIssue(diagnostic.MainIssue);
            diagnostic.Fix = BuildIssueFix(diagnostic.MainIssue);
        }

        private static string HumanizeIssue(TileIssueKind issue)
        {
            switch (issue)
            {
                case TileIssueKind.Occupied:
                    return "Тут уже є будівля або pending-preview.";
                case TileIssueKind.Pending:
                    return "На тайлі вже запланована інша будівля.";
                case TileIssueKind.Fog:
                    return "Тайл у тумані, тому будівництво тут не читається як дозволене.";
                case TileIssueKind.MissingInfluence:
                    return "Позиція поза зоною контролю ратуші або замку.";
                case TileIssueKind.InfluenceOverlap:
                    return "Зона нового центру конфліктує з уже існуючою зоною.";
                case TileIssueKind.Spacing:
                    return "Будівля стоїть занадто близько до іншого об'єкта.";
                case TileIssueKind.MissingResources:
                    return "У sandbox-бюджеті не вистачає ресурсів для будівництва.";
                case TileIssueKind.TerrainMismatch:
                    return "Поруч недостатньо потрібного типу місцевості для цієї будівлі.";
                default:
                    return "Сценарій або налаштування будівлі потребують уваги.";
            }
        }

        private static string BuildIssueFix(TileIssueKind issue)
        {
            switch (issue)
            {
                case TileIssueKind.Occupied:
                case TileIssueKind.Pending:
                    return "Звільніть тайл або виберіть сусідню позицію.";
                case TileIssueKind.Fog:
                    return "Додайте центр або будівлю, що відкриває туман, ближче до цієї зони.";
                case TileIssueKind.MissingInfluence:
                    return "Поставте ратушу/замок ближче або збільште радіус впливу.";
                case TileIssueKind.InfluenceOverlap:
                    return "Відсуньте центр далі від існуючої зони контролю.";
                case TileIssueKind.Spacing:
                    return "Збільште відстань або зменште мінімальну дистанцію сценарію.";
                case TileIssueKind.MissingResources:
                    return "Додайте ресурси в sandbox-бюджет або зменште вартість будівлі.";
                case TileIssueKind.TerrainMismatch:
                    return "Змініть terrain поруч або послабте tile requirement.";
                default:
                    return "Перевірте правила будівлі та сценарій.";
            }
        }

        private static string BuildTileIssueShortLabel(TileIssueKind issue)
        {
            switch (issue)
            {
                case TileIssueKind.Occupied: return "OCC";
                case TileIssueKind.Pending: return "PND";
                case TileIssueKind.Fog: return "FOG";
                case TileIssueKind.MissingInfluence: return "RNG";
                case TileIssueKind.InfluenceOverlap: return "OVR";
                case TileIssueKind.Spacing: return "DST";
                case TileIssueKind.MissingResources: return "RES";
                case TileIssueKind.TerrainMismatch: return "TILE";
                case TileIssueKind.ScenarioConfiguration: return "CFG";
                default: return string.Empty;
            }
        }

        private static Color TileIssueColor(TileIssueKind issue)
        {
            switch (issue)
            {
                case TileIssueKind.Fog:
                    return new Color(0.05f, 0.07f, 0.10f, 0.95f);
                case TileIssueKind.MissingInfluence:
                case TileIssueKind.InfluenceOverlap:
                    return WarningColor;
                case TileIssueKind.TerrainMismatch:
                    return new Color(0.45f, 0.35f, 0.18f, 0.95f);
                case TileIssueKind.MissingResources:
                    return new Color(0.86f, 0.52f, 0.10f, 0.95f);
                case TileIssueKind.None:
                    return AllowedColor;
                default:
                    return BlockedColor;
            }
        }

        private void EnsureScenarioResourceStock()
        {
            var ids = CollectCostResourceIds();
            for (int i = 0; i < ids.Count; i++)
            {
                string resourceId = ids[i];
                if (_scenarioResourceStock.ContainsKey(resourceId))
                    continue;

                _scenarioResourceStock[resourceId] = 0;
            }
        }

        private List<string> CollectCostResourceIds()
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var source = _registry?.Buildings ?? Array.Empty<BuildingDefinition>();
            for (int i = 0; i < source.Length; i++)
            {
                var definition = source[i];
                var cost = BuildingDefinitionCapabilities.GetConstructionCost(definition);
                if (cost == null)
                    continue;

                for (int c = 0; c < cost.Count; c++)
                {
                    var entry = cost[c];
                    if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId))
                        continue;

                    string id = entry.ResourceId.Trim();
                    if (seen.Add(id))
                        result.Add(id);
                }
            }

            return result;
        }

        private void OffsetScenarioBudget(int delta)
        {
            var keys = new List<string>(_scenarioResourceStock.Keys);
            for (int i = 0; i < keys.Count; i++)
                _scenarioResourceStock[keys[i]] = Mathf.Max(0, _scenarioResourceStock[keys[i]] + delta);
        }

        private void ClearScenarioBudget()
        {
            var keys = new List<string>(_scenarioResourceStock.Keys);
            for (int i = 0; i < keys.Count; i++)
                _scenarioResourceStock[keys[i]] = 0;
        }

        private static string BuildUnlocksLabel(BuildingDefinition definition)
        {
            if (definition == null)
                return "Відкриває/покращує: невідомо";

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return "Відкриває/покращує: старт і розвиток поселення";

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return "Відкриває/покращує: оборонні та столичні сценарії";

            if (definition.Category == BuildingCategory.Walls)
                return "Відкриває/покращує: периметр оборони і контроль проходів";

            if (BuildingDefinitionCapabilities.GetIndustrialResourceId(definition).Length > 0)
                return "Відкриває/покращує: ресурсний ланцюг виробництва";

            return "Відкриває/покращує: варіанти планування міста";
        }

        private static int GetFogDispersalRadius(BuildingDefinition definition)
        {
            if (definition == null)
                return 0;

            if (BuildingDefinitionCapabilities.IsTownHall(definition))
                return Mathf.Max(3, BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0));

            if (BuildingDefinitionCapabilities.IsCastle(definition))
                return Mathf.Max(2, BuildingDefinitionCapabilities.GetInfluenceRadius(definition, 0));

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return 2;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return 1;

            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0)
                return 1;

            return 0;
        }

        private void DrawSelectedSummary()
        {
            var definition = GetSelectedDefinition();
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Поточна будівля", HeaderStyle());
                if (definition == null)
                {
                    EditorGUILayout.HelpBox("Немає вибраної будівлі.", MessageType.Info);
                    return;
                }

                DrawMiniIcon(ExtractSprite(definition), 72f);
                EditorGUILayout.LabelField(GetPlayerFacingName(definition), EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Технічний ID сховано у foldout технічних полів.", EditorStyles.miniLabel);
                DrawBadge(definition.Category.ToString(), CategoryColor(definition.Category));
            }
        }

        private void DrawScenarioLegend()
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Легенда", HeaderStyle());
                DrawBadgeLine("Зелений: можна поставити", AllowedColor);
                DrawBadgeLine("Червоний: заблоковано", BlockedColor);
                DrawBadgeLine("Жовтий: зона впливу", InfluenceColor);
                DrawBadgeLine("Золотий: контроль ратуші", TownHallInfluenceColor);
                DrawBadgeLine("Блакитний: fog reveal", FogRevealColor);
                DrawBadgeLine("Фіолетовий: перетин туману і впливу", ZoneIntersectionColor);
                DrawBadgeLine("Темний: туман", new Color(0.05f, 0.06f, 0.08f));
                DrawBadgeLine("Блакитна рамка: pending", new Color(0.35f, 0.75f, 0.95f));
                DrawBadgeLine("ПКМ/Delete: видалення placement", WarningColor);
                DrawBadgeLine("Move drag: переміщення preview", SelectedColor);
            }
        }

        private void DrawAllBuildingsAtCandidate()
        {
            if (_registry == null || _registry.Buildings == null)
                return;

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField($"Усі будівлі на {_candidatePosition}", HeaderStyle());
                for (int index = 0; index < _registry.Buildings.Length; index++)
                {
                    var definition = _registry.Buildings[index];
                    if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                        continue;

                    var preview = EvaluatePlacement(definition, _candidatePosition);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawBadge(preview.IsAllowed ? "OK" : "NO", preview.IsAllowed ? AllowedColor : BlockedColor);
                        if (GUILayout.Button(GetScenarioLabel(definition.Id), EditorStyles.miniLabel))
                            _selectedIndex = index;
                    }
                }
            }
        }

        private PlacementPreview EvaluatePlacement(BuildingDefinition definition, Vector2Int positionToEvaluate)
        {
            if (definition == null || _registry == null)
            {
                return new PlacementPreview
                {
                    IsAllowed = false,
                    OverrideMessage = "Будівля або реєстр не задані.",
                };
            }

            if (IsGate(definition.Id))
                return EvaluateGatePlacement(definition, positionToEvaluate);

            var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = _registry,
                BuildingId = definition.Id,
                Position = positionToEvaluate,
                MinSpacing = _scenarioMinSpacing,
                TownHallBuildRadius = _scenarioTownHallRadius,
                IsOccupied = ScenarioIsOccupied,
                GetOccupantId = ScenarioGetOccupantId,
                IsFogBlocked = _scenarioFogEnabled ? ScenarioIsFogBlocked : null,
                PendingPlacements = _scenarioPending,
            });

            return new PlacementPreview
            {
                IsAllowed = result.IsValid,
                Evaluation = result,
            };
        }

        private PlacementPreview EvaluateGatePlacement(BuildingDefinition definition, Vector2Int positionToEvaluate)
        {
            string occupantId = ScenarioGetOccupantId(positionToEvaluate);
            var collection = _registry.GetWallCollectionByBuildingId(definition.Id);
            bool replacesWall = collection != null && collection.IsWall(occupantId);
            if (replacesWall)
            {
                return new PlacementPreview
                {
                    IsAllowed = true,
                    IsGateReplacement = true,
                    OverrideMessage = "Ворота замінять стіну на цьому тайлі. Це спеціальний сценарій заміни в межах wall-колекції.",
                };
            }

            return new PlacementPreview
            {
                IsAllowed = false,
                OverrideMessage = "Ворота можна поставити тільки на тайл зі стіною тієї ж wall-колекції.",
            };
        }

        private void ApplyPaint(Vector2Int tile)
        {
            switch (_paintMode)
            {
                case ScenarioPaintMode.Occupied:
                    _scenarioOccupants[tile] = ResolvePaintBuildingId();
                    RemovePending(tile);
                    break;
                case ScenarioPaintMode.Pending:
                    RemovePending(tile);
                    _scenarioPending.Add(new BuildingPlacementSimulationEntry(tile, ResolvePaintBuildingId()));
                    _scenarioOccupants.Remove(tile);
                    break;
                case ScenarioPaintMode.TownHall:
                    _scenarioOccupants[tile] = ResolveFirstCenterId(townHall: true);
                    RemovePending(tile);
                    break;
                case ScenarioPaintMode.Castle:
                    _scenarioOccupants[tile] = ResolveFirstCenterId(townHall: false);
                    RemovePending(tile);
                    break;
                case ScenarioPaintMode.Fog:
                    if (!_scenarioFog.Add(tile))
                        _scenarioFog.Remove(tile);
                    break;
                case ScenarioPaintMode.Terrain:
                    _scenarioTerrain[tile] = TerrainLabels[Mathf.Clamp(_terrainIndex, 0, TerrainLabels.Length - 1)];
                    break;
                case ScenarioPaintMode.Erase:
                    _scenarioOccupants.Remove(tile);
                    RemovePending(tile);
                    _scenarioFog.Remove(tile);
                    _scenarioTerrain.Remove(tile);
                    break;
                default:
                    _candidatePosition = tile;
                    break;
            }

            Repaint();
        }

        private void ClearScenario()
        {
            _scenarioOccupants.Clear();
            _scenarioPending.Clear();
            _scenarioFog.Clear();
            _scenarioTerrain.Clear();
        }

        private void SeedInfluenceCenter()
        {
            var center = new Vector2Int(_scenarioWidth / 2, _scenarioHeight / 2);
            _scenarioOccupants[center] = ResolveFirstCenterId(townHall: true);
            _candidatePosition = new Vector2Int(Mathf.Min(center.x + 2, _scenarioWidth - 1), center.y);
        }

        private bool ScenarioIsOccupied(Vector2Int tile)
        {
            return _scenarioOccupants.ContainsKey(tile);
        }

        private string ScenarioGetOccupantId(Vector2Int tile)
        {
            return _scenarioOccupants.TryGetValue(tile, out var occupantId) ? occupantId : null;
        }

        private bool ScenarioIsFogBlocked(Vector2Int tile)
        {
            return _scenarioFog.Contains(tile);
        }

        private bool IsCoveredByAnyInfluence(Vector2Int tile)
        {
            foreach (var pair in _scenarioOccupants)
            {
                var definition = _registry?.GetById(pair.Value);
                if (!BuildingPlacementEvaluator.IsInfluenceCenter(definition))
                    continue;

                int radius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pair.Key, tile) <= radius)
                    return true;
            }

            for (int index = 0; index < _scenarioPending.Count; index++)
            {
                var pending = _scenarioPending[index];
                var definition = _registry?.GetById(pending.BuildingId);
                if (!BuildingPlacementEvaluator.IsInfluenceCenter(definition))
                    continue;

                int radius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pending.Position, tile) <= radius)
                    return true;
            }

            return false;
        }

        private bool IsCoveredByAnyFogReveal(Vector2Int tile)
        {
            foreach (var pair in _scenarioOccupants)
            {
                var definition = _registry?.GetById(pair.Value);
                int radius = GetFogDispersalRadius(definition);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pair.Key, tile) <= radius)
                    return true;
            }

            for (int index = 0; index < _scenarioPending.Count; index++)
            {
                var pending = _scenarioPending[index];
                var definition = _registry?.GetById(pending.BuildingId);
                int radius = GetFogDispersalRadius(definition);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pending.Position, tile) <= radius)
                    return true;
            }

            return false;
        }

        private bool IsCoveredByTownHallInfluence(Vector2Int tile)
        {
            foreach (var pair in _scenarioOccupants)
            {
                var definition = _registry?.GetById(pair.Value);
                if (!BuildingDefinitionCapabilities.IsTownHall(definition))
                    continue;

                int radius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pair.Key, tile) <= radius)
                    return true;
            }

            for (int index = 0; index < _scenarioPending.Count; index++)
            {
                var pending = _scenarioPending[index];
                var definition = _registry?.GetById(pending.BuildingId);
                if (!BuildingDefinitionCapabilities.IsTownHall(definition))
                    continue;

                int radius = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius);
                if (radius > 0 && BuildingPlacementEvaluator.GetChebyshevDistance(pending.Position, tile) <= radius)
                    return true;
            }

            return false;
        }

        private int CountFogRevealTiles()
        {
            int count = 0;
            for (int x = 0; x < _scenarioWidth; x++)
            {
                for (int y = 0; y < _scenarioHeight; y++)
                {
                    if (IsCoveredByAnyFogReveal(new Vector2Int(x, y)))
                        count++;
                }
            }

            return count;
        }

        private int CountTownHallInfluenceTiles()
        {
            int count = 0;
            for (int x = 0; x < _scenarioWidth; x++)
            {
                for (int y = 0; y < _scenarioHeight; y++)
                {
                    if (IsCoveredByTownHallInfluence(new Vector2Int(x, y)))
                        count++;
                }
            }

            return count;
        }

        private int CountZoneIntersectionTiles()
        {
            int count = 0;
            for (int x = 0; x < _scenarioWidth; x++)
            {
                for (int y = 0; y < _scenarioHeight; y++)
                {
                    var tile = new Vector2Int(x, y);
                    if (IsCoveredByAnyFogReveal(tile) && IsCoveredByAnyInfluence(tile))
                        count++;
                }
            }

            return count;
        }

        private bool TryGetPending(Vector2Int tile, out BuildingPlacementSimulationEntry entry)
        {
            for (int index = 0; index < _scenarioPending.Count; index++)
            {
                entry = _scenarioPending[index];
                if (entry.Position == tile)
                    return true;
            }

            entry = default;
            return false;
        }

        private void RemovePending(Vector2Int tile)
        {
            for (int index = _scenarioPending.Count - 1; index >= 0; index--)
            {
                if (_scenarioPending[index].Position == tile)
                    _scenarioPending.RemoveAt(index);
            }
        }

        private int CountTerrainNear(Vector2Int center, string tileId, int radius)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return 0;

            int count = 0;
            int normalizedRadius = Mathf.Max(0, radius);
            for (int offsetX = -normalizedRadius; offsetX <= normalizedRadius; offsetX++)
            {
                for (int offsetY = -normalizedRadius; offsetY <= normalizedRadius; offsetY++)
                {
                    var tile = new Vector2Int(center.x + offsetX, center.y + offsetY);
                    if (!IsInScenario(tile))
                        continue;

                    if (string.Equals(GetTerrain(tile), tileId, StringComparison.OrdinalIgnoreCase))
                        count++;
                }
            }

            return count;
        }

        private bool IsInScenario(Vector2Int tile)
        {
            return tile.x >= 0 && tile.y >= 0 && tile.x < _scenarioWidth && tile.y < _scenarioHeight;
        }

        private string GetTerrain(Vector2Int tile)
        {
            return _scenarioTerrain.TryGetValue(tile, out var tileId) ? tileId : TerrainLabels[0];
        }

        private string ResolvePaintBuildingId()
        {
            if (!string.IsNullOrWhiteSpace(_paintBuildingId))
                return _paintBuildingId.Trim();

            return GetSelectedDefinition()?.Id ?? "building";
        }

        private void DrawScenarioPaintBuildingPicker()
        {
            var source = _registry?.Buildings ?? Array.Empty<BuildingDefinition>();
            var ids = new List<string>();
            var labels = new List<string>();

            for (int i = 0; i < source.Length; i++)
            {
                var definition = source[i];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                ids.Add(definition.Id);
                labels.Add(GetScenarioLabel(definition.Id));
            }

            if (ids.Count == 0)
            {
                EditorGUILayout.HelpBox("Немає доступних будівель для сценарного розміщення.", MessageType.Info);
                return;
            }

            string selectedId = string.IsNullOrWhiteSpace(_paintBuildingId)
                ? (GetSelectedDefinition()?.Id ?? ids[0])
                : _paintBuildingId;

            _paintBuildingSelectionIndex = Mathf.Clamp(ids.IndexOf(selectedId), 0, ids.Count - 1);
            int nextIndex = EditorGUILayout.Popup("Будівля для сценарію", _paintBuildingSelectionIndex, labels.ToArray());
            _paintBuildingSelectionIndex = Mathf.Clamp(nextIndex, 0, ids.Count - 1);
            _paintBuildingId = ids[_paintBuildingSelectionIndex];
        }

        private string ResolveFirstCenterId(bool townHall)
        {
            var source = _registry?.Buildings ?? Array.Empty<BuildingDefinition>();
            for (int index = 0; index < source.Length; index++)
            {
                var definition = source[index];
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                    continue;

                if (townHall && BuildingDefinitionCapabilities.IsTownHall(definition))
                    return definition.Id;
                if (!townHall && BuildingDefinitionCapabilities.IsCastle(definition))
                    return definition.Id;
            }

            return GetSelectedDefinition()?.Id ?? (townHall ? "town-hall" : "castle");
        }

        private bool IsGate(string buildingId)
        {
            if (_registry == null || string.IsNullOrWhiteSpace(buildingId))
                return false;

            var collection = _registry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsGate(buildingId);
        }

        private bool PassesListFilter(SerializedProperty buildingProperty)
        {
            string id = GetString(buildingProperty, "Id", string.Empty);
            string displayName = GetString(buildingProperty, "DisplayName", string.Empty);
            if (!string.IsNullOrWhiteSpace(_search)
                && id.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0
                && displayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            if (_categoryFilterIndex <= 0)
                return true;

            return GetEnumIndex(buildingProperty, "Category") == _categoryFilterIndex - 1;
        }

        private void CreateBuilding(SerializedProperty buildingsProperty, string generatedId)
        {
            string id = string.IsNullOrWhiteSpace(generatedId) ? GenerateUniqueId("building", buildingsProperty) : generatedId.Trim();
            int index = buildingsProperty.arraySize;
            buildingsProperty.InsertArrayElementAtIndex(index);
            var buildingProperty = buildingsProperty.GetArrayElementAtIndex(index);

            buildingProperty.FindPropertyRelative("Id").stringValue = id;
            buildingProperty.FindPropertyRelative("DisplayName").stringValue = string.IsNullOrWhiteSpace(_newBuildingName) ? id : _newBuildingName.Trim();
            buildingProperty.FindPropertyRelative("Category").enumValueIndex = (int)_newBuildingCategory;
            buildingProperty.FindPropertyRelative("Icon").objectReferenceValue = _newBuildingSprite;
            buildingProperty.FindPropertyRelative("Prefab").objectReferenceValue = ResolvePrefab(id, _newBuildingPrefab, _newBuildingSprite);
            ApplyWizardConstructionCost(buildingProperty.FindPropertyRelative("ConstructionCost"));
            ApplyWizardRoleModules(buildingProperty.FindPropertyRelative("Modules"));
            buildingProperty.FindPropertyRelative("UseCustomTownHallRules").boolValue = false;
            buildingProperty.FindPropertyRelative("RequireTownHallInRange").boolValue = _newBuildingRole != BuildingCreationRole.SettlementCenter && _newBuildingRole != BuildingCreationRole.Defense;
            buildingProperty.FindPropertyRelative("BlockIfTownHallAlreadyInRange").boolValue = _newBuildingRole == BuildingCreationRole.SettlementCenter || _newBuildingRole == BuildingCreationRole.Defense;
            buildingProperty.FindPropertyRelative("TownHallProximityRadiusOverride").intValue = _newBuildingInfluence;

            _selectedIndex = index;
            _registryObject.ApplyModifiedProperties();
            SaveRegistry();
            _workspaceMode = WorkspaceMode.Definition;
            _newBuildingId = GenerateUniqueId("new-building", buildingsProperty);
            _newBuildingName = "Нова будівля";
            _newBuildingStep = BuildingCreationStep.Identity;
            _newBuildingRole = BuildingCreationRole.Support;
            _newBuildingCategory = BuildingCategory.Civilian;
            _newBuildingCostLogs = 20;
            _newBuildingCostBoards = 0;
            _newBuildingCostStone = 0;
            _newBuildingCostMoney = 50;
            _newBuildingBuildTurns = 2;
            _newBuildingWorkers = 0;
            _newBuildingInfluence = 0;
            _newBuildingFogReveal = 0;
            _newBuildingHousingCapacity = 4;
            _newBuildingStorageCapacity = 200;
            _newBuildingProductionResourceId = "logs";
            _newBuildingSprite = null;
            _newBuildingPrefab = null;
        }

        private void ApplyWizardConstructionCost(SerializedProperty costProperty)
        {
            if (costProperty == null)
                return;

            costProperty.arraySize = 0;
            AddCostEntry(costProperty, "logs", _newBuildingCostLogs);
            AddCostEntry(costProperty, "boards", _newBuildingCostBoards);
            AddCostEntry(costProperty, "stone", _newBuildingCostStone);
            AddCostEntry(costProperty, "money", _newBuildingCostMoney);
        }

        private static void AddCostEntry(SerializedProperty costProperty, string resourceId, int amount)
        {
            if (costProperty == null || string.IsNullOrWhiteSpace(resourceId) || amount <= 0)
                return;

            int index = costProperty.arraySize;
            costProperty.InsertArrayElementAtIndex(index);
            var entry = costProperty.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("ResourceId").stringValue = resourceId;
            entry.FindPropertyRelative("Amount").intValue = amount;
        }

        private void ApplyWizardRoleModules(SerializedProperty modulesProperty)
        {
            if (modulesProperty == null)
                return;

            modulesProperty.arraySize = 0;
            switch (_newBuildingRole)
            {
                case BuildingCreationRole.SettlementCenter:
                    AddManagedModule(modulesProperty, new TownHallBuildingModule { BuildRadius = Mathf.Max(1, _newBuildingInfluence), IsCentral = true });
                    break;
                case BuildingCreationRole.Housing:
                    AddManagedModule(modulesProperty, new HousingBuildingModule { Capacity = Mathf.Max(1, _newBuildingHousingCapacity) });
                    if (_newBuildingWorkers <= 0)
                        AddManagedModule(modulesProperty, new WorkerlessBuildingModule());
                    break;
                case BuildingCreationRole.Storage:
                    AddManagedModule(modulesProperty, new WarehouseBuildingModule { MaxCapacity = Mathf.Max(1, _newBuildingStorageCapacity) });
                    AddManagedModule(modulesProperty, new WorkerlessBuildingModule());
                    break;
                case BuildingCreationRole.Production:
                    AddManagedModule(modulesProperty, new ProductionBuildingModule
                    {
                        ResourceId = string.IsNullOrWhiteSpace(_newBuildingProductionResourceId) ? "logs" : _newBuildingProductionResourceId.Trim(),
                        WorkersRequired = Mathf.Max(1, _newBuildingWorkers),
                        Priority = 5,
                    });
                    break;
                case BuildingCreationRole.Defense:
                    AddManagedModule(modulesProperty, new CastleBuildingModule
                    {
                        ExclusionRadius = Mathf.Max(1, _newBuildingInfluence),
                        GarrisonCapacity = Mathf.Max(0, _newBuildingWorkers),
                    });
                    break;
                case BuildingCreationRole.Wall:
                    AddManagedModule(modulesProperty, new WallBuildingModule { HitPoints = 100, IsPassable = false });
                    AddManagedModule(modulesProperty, new WorkerlessBuildingModule());
                    break;
                default:
                    AddManagedModule(modulesProperty, new WorkerlessBuildingModule());
                    break;
            }
        }

        private static void AddManagedModule(SerializedProperty modulesProperty, BuildingModuleDefinition module)
        {
            if (modulesProperty == null || module == null)
                return;

            int index = modulesProperty.arraySize;
            modulesProperty.InsertArrayElementAtIndex(index);
            modulesProperty.GetArrayElementAtIndex(index).managedReferenceValue = module;
        }

        private void DrawCreationCategoryButton(BuildingCategory category, string label)
        {
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = _newBuildingCategory == category ? CategoryColor(category) : Color.white;
            if (GUILayout.Button(label, GUILayout.Height(24f)))
                _newBuildingCategory = category;
            GUI.backgroundColor = previousColor;
        }

        private void DrawCreationRoleButton(BuildingCreationRole role, string title, string body)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 42f, GUILayout.ExpandWidth(true));
            bool selected = _newBuildingRole == role;
            EditorGUI.DrawRect(rect, selected ? new Color(SelectedColor.r, SelectedColor.g, SelectedColor.b, 0.52f) : new Color(0f, 0f, 0f, 0.10f));
            DrawBorder(rect, selected ? SelectedColor : new Color(0f, 0f, 0f, 0.15f), selected ? 2f : 1f);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 4f, rect.width - 16f, 16f), title, EditorStyles.miniBoldLabel);
            GUI.Label(new Rect(rect.x + 8f, rect.y + 21f, rect.width - 16f, 17f), body, EditorStyles.miniLabel);

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _newBuildingRole = role;
                ApplyCreationRoleDefaults(role);
                Event.current.Use();
                Repaint();
            }
        }

        private void ApplyCreationRoleDefaults(BuildingCreationRole role)
        {
            switch (role)
            {
                case BuildingCreationRole.SettlementCenter:
                    _newBuildingCategory = BuildingCategory.Civilian;
                    _newBuildingWorkers = 0;
                    _newBuildingInfluence = Mathf.Max(_newBuildingInfluence, 5);
                    _newBuildingFogReveal = Mathf.Max(_newBuildingFogReveal, 5);
                    break;
                case BuildingCreationRole.Housing:
                    _newBuildingCategory = BuildingCategory.Civilian;
                    _newBuildingWorkers = 0;
                    _newBuildingFogReveal = Mathf.Max(_newBuildingFogReveal, 2);
                    break;
                case BuildingCreationRole.Storage:
                    _newBuildingCategory = BuildingCategory.Civilian;
                    _newBuildingWorkers = 0;
                    _newBuildingFogReveal = Mathf.Max(_newBuildingFogReveal, 1);
                    break;
                case BuildingCreationRole.Production:
                    _newBuildingCategory = BuildingCategory.Industrial;
                    _newBuildingWorkers = Mathf.Max(_newBuildingWorkers, 2);
                    _newBuildingFogReveal = Mathf.Max(_newBuildingFogReveal, 1);
                    break;
                case BuildingCreationRole.Defense:
                    _newBuildingCategory = BuildingCategory.Military;
                    _newBuildingInfluence = Mathf.Max(_newBuildingInfluence, 4);
                    _newBuildingFogReveal = Mathf.Max(_newBuildingFogReveal, 3);
                    break;
                case BuildingCreationRole.Wall:
                    _newBuildingCategory = BuildingCategory.Walls;
                    _newBuildingWorkers = 0;
                    break;
                default:
                    _newBuildingCategory = BuildingCategory.Civilian;
                    break;
            }
        }

        private void DrawCreationPreviewCard()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawNewBuildingSpritePreview(116f);
                DrawBadgeLine(BuildCreationRoleLabel(_newBuildingRole), CategoryColor(_newBuildingCategory));
                EditorGUILayout.LabelField(BuildCreationPurposeText(), EditorStyles.wordWrappedMiniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMetricCard("Ціна", BuildNewBuildingCostSummary(), WarningColor);
                    DrawMetricCard("Час", $"~{_newBuildingBuildTurns} ходи", SelectedColor);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMetricCard("Працівники", _newBuildingWorkers <= 0 ? "не потребує" : $"до {_newBuildingWorkers}", CategoryColor(_newBuildingCategory));
                    DrawMetricCard("Вплив", _newBuildingInfluence <= 0 ? "немає" : $"{_newBuildingInfluence} тайл.", TownHallInfluenceColor);
                }
            }
        }

        private void DrawNewBuildingSpritePreview(float height)
        {
            Rect previewRect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(previewRect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.14f, 0.15f) : new Color(0.84f, 0.86f, 0.88f));
            DrawBorder(previewRect, _newBuildingSprite != null ? CategoryColor(_newBuildingCategory) : WarningColor, 2f);

            if (_newBuildingSprite != null)
            {
                Rect spriteRect = FitRect(Shrink(previewRect, 14f), _newBuildingSprite.textureRect.width, _newBuildingSprite.textureRect.height);
                DrawSprite(spriteRect, _newBuildingSprite);
            }
            else
            {
                GUI.Label(previewRect, "Перетягніть sprite будівлі сюди", CenteredCellStyle());
            }

            if (SpriteImportDragDropPolicy.HandleDrop(previewRect, ref _newBuildingSprite, "BuildingDesigner.NewBuildingSprite"))
                Repaint();
        }

        private string BuildNewBuildingCostSummary()
        {
            int total = Mathf.Max(0, _newBuildingCostLogs)
                + Mathf.Max(0, _newBuildingCostBoards)
                + Mathf.Max(0, _newBuildingCostStone)
                + Mathf.Max(0, _newBuildingCostMoney);
            return total <= 0 ? "безкоштовно" : total.ToString();
        }

        private static string BuildCreationRoleLabel(BuildingCreationRole role)
        {
            switch (role)
            {
                case BuildingCreationRole.SettlementCenter: return "Роль: центр поселення";
                case BuildingCreationRole.Housing: return "Роль: житло";
                case BuildingCreationRole.Storage: return "Роль: склад";
                case BuildingCreationRole.Production: return "Роль: виробництво";
                case BuildingCreationRole.Defense: return "Роль: оборона";
                case BuildingCreationRole.Wall: return "Роль: стіна";
                default: return "Роль: підтримка";
            }
        }

        private string BuildCreationPurposeText()
        {
            switch (_newBuildingRole)
            {
                case BuildingCreationRole.SettlementCenter:
                    return "Задає центр розвитку, відкриває контроль території і пояснює, де місто може рости.";
                case BuildingCreationRole.Housing:
                    return $"Додає житлову місткість: +{_newBuildingHousingCapacity}.";
                case BuildingCreationRole.Storage:
                    return $"Створює економічний буфер і зберігає до {_newBuildingStorageCapacity} ресурсів.";
                case BuildingCreationRole.Production:
                    return $"Потребує {_newBuildingWorkers} працівників і працює як виробничий вузол.";
                case BuildingCreationRole.Defense:
                    return "Контролює небезпечну зону і підтримує оборонний периметр.";
                case BuildingCreationRole.Wall:
                    return "Формує фізичну межу і керує напрямками проходу.";
                default:
                    return "Підтримує місто без важкої системної залежності.";
            }
        }

        private void DuplicateSelectedBuilding(SerializedProperty buildingsProperty)
        {
            if (buildingsProperty.arraySize == 0)
                return;

            int sourceIndex = Mathf.Clamp(_selectedIndex, 0, buildingsProperty.arraySize - 1);
            int insertIndex = sourceIndex + 1;
            buildingsProperty.InsertArrayElementAtIndex(insertIndex);
            var duplicate = buildingsProperty.GetArrayElementAtIndex(insertIndex);
            string sourceId = GetString(buildingsProperty.GetArrayElementAtIndex(sourceIndex), "Id", "building");
            string newId = GenerateUniqueId(sourceId + "-copy", buildingsProperty);
            duplicate.FindPropertyRelative("Id").stringValue = newId;
            duplicate.FindPropertyRelative("DisplayName").stringValue = GetString(duplicate, "DisplayName", sourceId) + " Copy";
            _selectedIndex = insertIndex;
            _registryObject.ApplyModifiedProperties();
            SaveRegistry();
        }

        private void DeleteSelectedBuilding(SerializedProperty buildingsProperty)
        {
            if (buildingsProperty.arraySize == 0)
                return;

            int index = Mathf.Clamp(_selectedIndex, 0, buildingsProperty.arraySize - 1);
            string id = GetString(buildingsProperty.GetArrayElementAtIndex(index), "Id", "?");
            if (!EditorUtility.DisplayDialog("Видалити будівлю", $"Видалити '{id}' з BuildingRegistrySO?", "Видалити", "Скасувати"))
                return;

            buildingsProperty.DeleteArrayElementAtIndex(index);
            _selectedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, buildingsProperty.arraySize - 1));
            _registryObject.ApplyModifiedProperties();
            SaveRegistry();
        }

        private string ValidateNewBuildingId(string id, SerializedProperty buildingsProperty)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "ID не може бути порожнім.";
            if (id.Contains("_"))
                return "Для ID використовуйте '-' замість '_'.";
            if (ContainsId(buildingsProperty, id.Trim()))
                return "Такий ID уже існує.";
            return null;
        }

        private void QuickFixSelectedBuildingTypicalIssues()
        {
            if (_registryObject == null)
                return;

            var buildings = _registryObject.FindProperty("Buildings");
            if (buildings == null || !buildings.isArray || buildings.arraySize == 0)
                return;

            int index = Mathf.Clamp(_selectedIndex, 0, buildings.arraySize - 1);
            var building = buildings.GetArrayElementAtIndex(index);
            if (building == null)
                return;

            var idProp = building.FindPropertyRelative("Id");
            var nameProp = building.FindPropertyRelative("DisplayName");
            var prefabProp = building.FindPropertyRelative("Prefab");
            var iconProp = building.FindPropertyRelative("Icon");

            string rawId = idProp?.stringValue ?? string.Empty;
            string sanitizedId = string.IsNullOrWhiteSpace(rawId) ? "building" : rawId.Trim().Replace('_', '-').ToLowerInvariant();
            string uniqueId = GenerateUniqueId(sanitizedId, buildings);

            if (idProp != null)
                idProp.stringValue = uniqueId;

            if (nameProp != null && string.IsNullOrWhiteSpace(nameProp.stringValue))
                nameProp.stringValue = uniqueId;

            if (prefabProp != null && prefabProp.objectReferenceValue == null)
                prefabProp.objectReferenceValue = CreateEmptyPrefab(uniqueId);

            if (iconProp != null && iconProp.objectReferenceValue == null)
            {
                var prefab = prefabProp?.objectReferenceValue as GameObject;
                if (prefab != null)
                {
                    var renderer = prefab.GetComponentInChildren<SpriteRenderer>();
                    if (renderer != null && renderer.sprite != null)
                        iconProp.objectReferenceValue = renderer.sprite;
                }
            }

            _registryObject.ApplyModifiedProperties();
            SaveRegistry();
        }

        private bool ContainsId(SerializedProperty buildingsProperty, string id)
        {
            if (buildingsProperty == null || string.IsNullOrWhiteSpace(id))
                return false;

            for (int index = 0; index < buildingsProperty.arraySize; index++)
            {
                var buildingProperty = buildingsProperty.GetArrayElementAtIndex(index);
                if (string.Equals(GetString(buildingProperty, "Id", string.Empty), id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private string GenerateUniqueId(string baseId, SerializedProperty buildingsProperty)
        {
            string normalizedBaseId = string.IsNullOrWhiteSpace(baseId) ? "building" : baseId.Trim().Replace('_', '-').ToLowerInvariant();
            if (!ContainsId(buildingsProperty, normalizedBaseId))
                return normalizedBaseId;

            for (int index = 2; index < 999; index++)
            {
                string candidate = normalizedBaseId + "-" + index;
                if (!ContainsId(buildingsProperty, candidate))
                    return candidate;
            }

            return normalizedBaseId + "-new";
        }

        private GameObject ResolvePrefab(string id, GameObject prefabOverride, Sprite sprite)
        {
            if (prefabOverride != null)
                return prefabOverride;
            if (sprite != null)
                return CreatePrefabFromSprite(id, sprite);
            return CreateEmptyPrefab(id);
        }

        private static GameObject CreatePrefabFromSprite(string id, Sprite sprite)
        {
            EnsureFolder(BuildingPrefabFolder);
            string safeName = SanitizeAssetName(id);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingPrefabFolder}/{safeName}.prefab");
            var instance = new GameObject(safeName);
            instance.AddComponent<SpriteRenderer>().sprite = sprite;
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            DestroyImmediate(instance);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static GameObject CreateEmptyPrefab(string id)
        {
            EnsureFolder(BuildingPrefabFolder);
            string safeName = SanitizeAssetName(id);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingPrefabFolder}/{safeName}.prefab");
            var instance = new GameObject(safeName);
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            DestroyImmediate(instance);
            AssetDatabase.Refresh();
            return prefab;
        }

        private static string SanitizeAssetName(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? "building" : id.Replace('/', '-').Replace('\\', '-');
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Replace('\\', '/').TrimEnd('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
                return;

            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private SerializedProperty GetSelectedBuildingProperty()
        {
            var buildingsProperty = _registryObject?.FindProperty("Buildings");
            if (buildingsProperty == null || buildingsProperty.arraySize == 0)
                return null;

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, buildingsProperty.arraySize - 1);
            return buildingsProperty.GetArrayElementAtIndex(_selectedIndex);
        }

        private BuildingDefinition GetSelectedDefinition()
        {
            if (_registry == null || _registry.Buildings == null || _registry.Buildings.Length == 0)
                return null;

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _registry.Buildings.Length - 1);
            return _registry.Buildings[_selectedIndex];
        }

        private BuildingDefinition GetRuntimeDefinition(int index)
        {
            if (_registry == null || _registry.Buildings == null || index < 0 || index >= _registry.Buildings.Length)
                return null;
            return _registry.Buildings[index];
        }

        private BuildingDefinition ResolveDragPreviewDefinition(BuildingDefinition fallback)
        {
            if (!TryGetDraggedBuildingId(out string buildingId))
                return fallback;

            _dragPreviewBuildingId = buildingId;
            return _registry?.GetById(buildingId) ?? fallback;
        }

        private bool TryGetDraggedBuildingId(out string buildingId)
        {
            buildingId = DragAndDrop.GetGenericData(BuildingDragPayloadKey) as string;
            return !string.IsNullOrWhiteSpace(buildingId);
        }

        private void SelectBuildingById(string buildingId)
        {
            if (_registry == null || _registry.Buildings == null || string.IsNullOrWhiteSpace(buildingId))
                return;

            for (int index = 0; index < _registry.Buildings.Length; index++)
            {
                var definition = _registry.Buildings[index];
                if (definition == null || !string.Equals(definition.Id, buildingId, StringComparison.Ordinal))
                    continue;

                _selectedIndex = index;
                return;
            }
        }

        private void RefreshSerializedObject()
        {
            _registryObject = _registry != null ? new SerializedObject(_registry) : null;
            if (_registry != null && _registry.Buildings != null)
                _selectedIndex = Mathf.Clamp(_selectedIndex, 0, Mathf.Max(0, _registry.Buildings.Length - 1));
        }

        private BuildingRegistrySO FindFirstRegistry()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(BuildingRegistrySO)}");
            if (guids.Length == 0)
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
        }

        private void SaveRegistry()
        {
            if (!EditorRegistryWriteLock.IsUnlocked(RegistryLockKey))
            {
                EditorUtility.DisplayDialog("Readonly lock", "BuildingRegistry заблокований. Увімкніть Unlock для збереження.", "OK");
                return;
            }

            if (_staleTracker.IsStale(_registry))
            {
                EditorUtility.DisplayDialog("Дані застарілі", "BuildingRegistry змінено зовні. Перечитайте дані перед збереженням.", "OK");
                return;
            }

            if (_registryObject != null)
                _registryObject.ApplyModifiedProperties();

            var changesForLog = _registry != null
                ? SerializedDiffPreviewUtility.BuildDiff(_registry, _lastSavedRegistrySnapshot, maxItems: 220)
                : new List<string>();

            if (_diffBeforeSaveEnabled && _registry != null)
            {
                if (!ConfirmSaveWithDiff("Building Registry", changesForLog))
                    return;
            }

            if (_registry != null)
                EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssets();
            EditorContentChangeLog.Write("BuildingDesigner", "SaveRegistry", _registry, changesForLog);
            RefreshSavedSnapshot();
            _staleTracker.Capture(_registry);
        }

        private void RefreshSavedSnapshot()
        {
            _lastSavedRegistrySnapshot = SerializedDiffPreviewUtility.CaptureSnapshot(_registry);
        }

        private static bool ConfirmSaveWithDiff(string source, List<string> changes)
        {
            if (changes == null || changes.Count == 0)
                return true;

            const int previewLimit = 18;
            int shown = Mathf.Min(previewLimit, changes.Count);
            var lines = new List<string>(shown + 2);
            for (int i = 0; i < shown; i++)
                lines.Add($"- {changes[i]}");

            if (changes.Count > shown)
                lines.Add($"... ще {changes.Count - shown} змін.");

            string message =
                $"Джерело: {source}\n" +
                $"Змінені поля: {changes.Count}\n\n" +
                string.Join("\n", lines);

            return EditorUtility.DisplayDialog("Diff before save", message, "Зберегти", "Скасувати");
        }

        private void LoadRegistryPreference()
        {
            _registry = MoyvaProjectEditorContext.Get<BuildingRegistrySO>();
            if (_registry != null)
                return;

            string guid = EditorPrefs.GetString(PrefKeyRegistryGuid, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(path))
                _registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
        }

        private void SaveRegistryPreference()
        {
            MoyvaProjectEditorContext.Set(_registry);
            if (_registry == null)
            {
                EditorPrefs.DeleteKey(PrefKeyRegistryGuid);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_registry);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrWhiteSpace(guid))
                EditorPrefs.SetString(PrefKeyRegistryGuid, guid);
        }

        private void ClampScenarioCandidate()
        {
            _candidatePosition = new Vector2Int(
                Mathf.Clamp(_candidatePosition.x, 0, Mathf.Max(0, _scenarioWidth - 1)),
                Mathf.Clamp(_candidatePosition.y, 0, Mathf.Max(0, _scenarioHeight - 1)));
        }

        private static string GetString(SerializedProperty property, string relativeName, string fallback)
        {
            return property?.FindPropertyRelative(relativeName)?.stringValue ?? fallback;
        }

        private static int GetEnumIndex(SerializedProperty property, string relativeName)
        {
            return property?.FindPropertyRelative(relativeName)?.enumValueIndex ?? 0;
        }

        private static Sprite ExtractSprite(BuildingDefinition definition)
        {
            if (definition == null)
                return null;

            if (definition.Prefab != null)
            {
                var renderers = definition.Prefab.GetComponentsInChildren<SpriteRenderer>(true);
                for (int index = 0; index < renderers.Length; index++)
                {
                    if (renderers[index] != null && renderers[index].sprite != null)
                        return renderers[index].sprite;
                }
            }

            return definition.Icon;
        }

        private static string ShortLabel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "?";
            string trimmed = id.Trim();
            return trimmed.Length <= 3 ? trimmed.ToUpperInvariant() : trimmed.Substring(0, 3).ToUpperInvariant();
        }

        private string GetScenarioLabel(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return "Без назви";

            var definition = _registry?.GetById(buildingId);
            if (definition == null)
                return "Без назви";

            return string.IsNullOrWhiteSpace(definition.DisplayName)
                ? "Без назви"
                : definition.DisplayName;
        }

        private static string SuggestIdFromDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return "building";

            string normalized = displayName.Trim().ToLowerInvariant();
            var chars = normalized.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if ((chars[i] >= 'a' && chars[i] <= 'z') || (chars[i] >= '0' && chars[i] <= '9'))
                    continue;

                chars[i] = '-';
            }

            string collapsed = new string(chars).Replace("--", "-").Trim('-');
            return string.IsNullOrWhiteSpace(collapsed) ? "building" : collapsed;
        }

        private static string BuildListSubtitle(BuildingDefinition definition)
        {
            if (definition == null)
                return "Налаштування недоступні";

            int workers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition);
            int influence = BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, 0);
            if (workers > 0)
                return $"Виробнича роль • працівники: {workers}";

            if (influence > 0)
                return $"Зона впливу: {influence}";

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return $"Житло: +{BuildingDefinitionCapabilities.GetHousingCapacity(definition)}";

            return "Підтримуюча роль для сценарію";
        }

        private static string GetPlayerFacingName(BuildingDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.DisplayName))
                return "Без назви";

            return definition.DisplayName.Trim();
        }

        private static string FormatBlocker(BuildingPlacementBlocker blocker)
        {
            if (blocker == null)
                return string.Empty;

            string positionText = blocker.Position.HasValue ? $" ({blocker.Position.Value})" : string.Empty;
            return $"{blocker.Kind}{positionText}: {blocker.Message}";
        }

        private static Rect Shrink(Rect rect, float padding)
        {
            return new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2f, rect.height - padding * 2f);
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawMiniIcon(Sprite sprite, float size)
        {
            Rect rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            DrawSprite(rect, sprite);
        }

        private static void DrawSprite(Rect rect, Sprite sprite)
        {
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.18f));
            if (sprite == null || sprite.texture == null)
            {
                GUI.Label(rect, "-", CenteredCellStyle());
                return;
            }

            Rect padded = Shrink(rect, 3f);
            Rect fitted = FitRect(padded, sprite.textureRect.width, sprite.textureRect.height);
            Rect uv = new Rect(
                sprite.textureRect.x / sprite.texture.width,
                sprite.textureRect.y / sprite.texture.height,
                sprite.textureRect.width / sprite.texture.width,
                sprite.textureRect.height / sprite.texture.height);
            GUI.DrawTextureWithTexCoords(fitted, sprite.texture, uv, true);
        }

        private static Rect FitRect(Rect container, float contentWidth, float contentHeight)
        {
            if (contentWidth <= 0f || contentHeight <= 0f || container.width <= 0f || container.height <= 0f)
                return container;

            float contentAspect = contentWidth / contentHeight;
            float containerAspect = container.width / container.height;
            if (contentAspect > containerAspect)
            {
                float height = container.width / contentAspect;
                return new Rect(container.x, container.y + (container.height - height) * 0.5f, container.width, height);
            }

            float width = container.height * contentAspect;
            return new Rect(container.x + (container.width - width) * 0.5f, container.y, width, container.height);
        }

        private static void DrawBadge(string text, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(Mathf.Max(34f, GUI.skin.label.CalcSize(new GUIContent(text)).x + 12f), 18f, GUILayout.Height(18f));
            DrawBadgeRect(rect, text, color);
        }

        private static void DrawBadgeRect(Rect rect, string text, Color color)
        {
            EditorWindowSharedUI.DrawBadgeRect(rect, text, color);
        }

        private static void DrawBadgeLine(string text, Color color)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBadge(" ", color);
                EditorGUILayout.LabelField(text, EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawStatusBanner(string text, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, text, BannerStyle());
        }

        private static void DrawSectionTitle(string title, string subtitle)
        {
            EditorGUILayout.LabelField(title, HeaderStyle());
            EditorGUILayout.LabelField(subtitle, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(6f);
        }

        private static Color CategoryColor(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Military:
                    return new Color(0.78f, 0.28f, 0.25f);
                case BuildingCategory.Civilian:
                    return new Color(0.25f, 0.62f, 0.36f);
                case BuildingCategory.Industrial:
                    return new Color(0.28f, 0.48f, 0.76f);
                case BuildingCategory.Walls:
                    return new Color(0.48f, 0.48f, 0.50f);
                default:
                    return Color.gray;
            }
        }

        private static Color TerrainColor(string tileId)
        {
            switch (tileId)
            {
                case "forest-dense":
                    return new Color(0.16f, 0.40f, 0.22f);
                case "water":
                    return new Color(0.15f, 0.38f, 0.60f);
                case "stone":
                    return new Color(0.42f, 0.42f, 0.42f);
                case "road":
                    return new Color(0.55f, 0.48f, 0.36f);
                default:
                    return new Color(0.33f, 0.49f, 0.28f);
            }
        }

        private static GUIStyle PanelStyle()
        {
            return EditorWindowSharedUI.PanelStyle(
                padding: new RectOffset(8, 8, 8, 8),
                margin: new RectOffset(2, 2, 4, 4));
        }

        private static GUIContent IconContent(string iconName, string fallback, string tooltip)
        {
            GUIContent content = !string.IsNullOrWhiteSpace(iconName) ? EditorGUIUtility.IconContent(iconName) : null;
            if (content == null || content.image == null)
                return new GUIContent(fallback, tooltip);

            return new GUIContent(string.IsNullOrEmpty(fallback) ? content.text : fallback, content.image, tooltip);
        }

        private static GUIStyle CardStyle()
        {
            return EditorWindowSharedUI.PanelStyle(
                padding: new RectOffset(8, 8, 7, 7),
                margin: new RectOffset(2, 2, 3, 5));
        }

        private static GUIStyle PrimaryButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                fixedHeight = 26f,
            };
            return style;
        }

        private static GUIStyle HeaderStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
            };
        }

        private static GUIStyle BadgeStyle()
        {
            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle BannerStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle CenteredCellStyle()
        {
            return new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle CoordinateStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 8,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle PlayerButtonTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                wordWrap = true,
            };
        }
    }
}