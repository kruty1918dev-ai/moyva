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

        private sealed class PlacementPreview
        {
            public bool IsAllowed;
            public BuildingPlacementEvaluationResult Evaluation;
            public string OverrideMessage;
            public bool IsGateReplacement;
        }

        private const string PrefKeyRegistryGuid = "Moyva.BuildingDesigner.RegistryGuid";
        private const string BuildingPrefabFolder = "Assets/Moyva/Prefabs/Buildings";

        private static readonly string[] WorkspaceLabels = { "Будівля", "Симуляція", "Гравець" };
        private static readonly string[] CategoryFilterLabels = { "Усі", "Military", "Civilian", "Industrial", "Walls" };
        private static readonly string[] PaintModeLabels = { "Тест", "Об'єкт", "Pending", "Ратуша", "Замок", "Туман", "Тайл", "Стерти" };
        private static readonly string[] TerrainLabels = { "grass", "forest-dense", "water", "stone", "road" };

        private static readonly Color PanelColor = new Color(0.18f, 0.19f, 0.20f, 1f);
        private static readonly Color SelectedColor = new Color(0.22f, 0.45f, 0.50f, 1f);
        private static readonly Color AllowedColor = new Color(0.23f, 0.58f, 0.33f, 1f);
        private static readonly Color BlockedColor = new Color(0.72f, 0.27f, 0.24f, 1f);
        private static readonly Color WarningColor = new Color(0.83f, 0.56f, 0.18f, 1f);
        private static readonly Color InfluenceColor = new Color(0.95f, 0.78f, 0.24f, 0.18f);

        private BuildingRegistrySO _registry;
        private SerializedObject _registryObject;
        private WorkspaceMode _workspaceMode = WorkspaceMode.Placement;
        private Vector2 _listScroll;
        private Vector2 _detailsScroll;
        private Vector2 _rightScroll;
        private string _search = string.Empty;
        private int _categoryFilterIndex;
        private int _selectedIndex;

        private string _newBuildingId = "new-building";
        private string _newBuildingName = "Нова будівля";
        private BuildingCategory _newBuildingCategory = BuildingCategory.Civilian;
        private Sprite _newBuildingSprite;
        private GameObject _newBuildingPrefab;

        private int _scenarioWidth = 15;
        private int _scenarioHeight = 11;
        private int _scenarioMinSpacing;
        private int _scenarioTownHallRadius = 4;
        private bool _scenarioFogEnabled = true;
        private bool _showPlacementHeatmap = true;
        private bool _showInfluence = true;
        private bool _showCoordinates;
        private ScenarioPaintMode _paintMode = ScenarioPaintMode.Candidate;
        private int _terrainIndex;
        private string _paintBuildingId = string.Empty;
        private Vector2Int _candidatePosition = new Vector2Int(7, 5);
        private readonly Dictionary<Vector2Int, string> _scenarioOccupants = new Dictionary<Vector2Int, string>();
        private readonly List<BuildingPlacementSimulationEntry> _scenarioPending = new List<BuildingPlacementSimulationEntry>();
        private readonly HashSet<Vector2Int> _scenarioFog = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, string> _scenarioTerrain = new Dictionary<Vector2Int, string>();

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

            RefreshSerializedObject();
            ClampScenarioCandidate();
        }

        private void OnDisable()
        {
            SaveRegistryPreference();
        }

        private void OnGUI()
        {
            _registryObject?.Update();

            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBuildingList(GUILayout.Width(300f));
                DrawMainPanel(GUILayout.MinWidth(440f));
                DrawRightPanel(GUILayout.Width(360f));
            }

            if (_registryObject != null && _registryObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_registry);
                Repaint();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("BUILDING DESIGNER", EditorStyles.boldLabel, GUILayout.Width(150f));

                EditorGUI.BeginChangeCheck();
                _registry = (BuildingRegistrySO)EditorGUILayout.ObjectField(_registry, typeof(BuildingRegistrySO), false, GUILayout.Width(240f));
                if (EditorGUI.EndChangeCheck())
                {
                    SaveRegistryPreference();
                    RefreshSerializedObject();
                    _selectedIndex = 0;
                }

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), EditorStyles.toolbarButton, GUILayout.Width(28f)))
                {
                    _registry = FindFirstRegistry();
                    RefreshSerializedObject();
                }

                GUILayout.Space(6f);
                _workspaceMode = (WorkspaceMode)GUILayout.Toolbar((int)_workspaceMode, WorkspaceLabels, EditorStyles.toolbarButton, GUILayout.Width(310f));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Registry Hub", EditorStyles.toolbarButton, GUILayout.Width(92f)))
                    EditorApplication.ExecuteMenuItem("Moyva/Tools/Registry Hub");

                if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(58f)))
                    SaveRegistry();
            }
        }

        private void DrawBuildingList(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(PanelStyle(), options))
            {
                EditorGUILayout.LabelField("Будівлі", HeaderStyle());
                _search = EditorGUILayout.TextField(_search, EditorStyles.toolbarSearchField);
                _categoryFilterIndex = EditorGUILayout.Popup(_categoryFilterIndex, CategoryFilterLabels);

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
                EditorGUILayout.LabelField("Швидке створення", EditorStyles.boldLabel);
                _newBuildingId = EditorGUILayout.TextField("ID", _newBuildingId);
                _newBuildingName = EditorGUILayout.TextField("Назва", _newBuildingName);
                _newBuildingCategory = (BuildingCategory)EditorGUILayout.EnumPopup("Категорія", _newBuildingCategory);
                _newBuildingSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newBuildingSprite, typeof(Sprite), false);
                _newBuildingPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _newBuildingPrefab, typeof(GameObject), false);

                string validation = ValidateNewBuildingId(_newBuildingId, buildingsProperty);
                if (!string.IsNullOrEmpty(validation))
                    EditorGUILayout.HelpBox(validation, MessageType.Warning);

                EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(validation));
                if (GUILayout.Button("Створити будівлю", PrimaryButtonStyle()))
                    CreateBuilding(buildingsProperty);
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawBuildingListRow(SerializedProperty buildingProperty, int index)
        {
            string id = GetString(buildingProperty, "Id", "?");
            string displayName = GetString(buildingProperty, "DisplayName", string.Empty);
            var category = (BuildingCategory)GetEnumIndex(buildingProperty, "Category");
            var definition = GetRuntimeDefinition(index);
            var placement = EvaluatePlacement(definition, _candidatePosition);
            var issues = definition != null ? BuildingModuleValidation.Validate(definition) : Array.Empty<BuildingValidationIssue>();
            bool hasErrors = BuildingModuleValidation.HasErrors(issues);

            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = index == _selectedIndex ? SelectedColor : Color.white;
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                GUI.backgroundColor = previousColor;
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Toggle(index == _selectedIndex, string.Empty, GUILayout.Width(18f)) && _selectedIndex != index)
                        _selectedIndex = index;

                    DrawMiniIcon(ExtractSprite(definition), 28f);

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(displayName) ? id : displayName, EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(id, EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();
                    DrawBadge(category.ToString(), CategoryColor(category));
                    DrawBadge(placement.IsAllowed ? "OK" : "BLOCK", placement.IsAllowed ? AllowedColor : BlockedColor);
                    if (hasErrors)
                        DrawBadge("ERR", BlockedColor);
                }
            }
        }

        private void DrawMainPanel(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(options))
            {
                if (_registry == null)
                {
                    EditorGUILayout.HelpBox("Building Designer потребує BuildingRegistrySO.", MessageType.Warning);
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
                EditorGUILayout.HelpBox("Оберіть будівлю у списку.", MessageType.Info);
                return;
            }

            DrawSectionTitle("Дані будівлі", "Це джерело правди для runtime registry і меню будівництва.");

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Id"), new GUIContent("ID"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("DisplayName"), new GUIContent("Назва у UI"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Category"), new GUIContent("Категорія"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Icon"), new GUIContent("Icon"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("Prefab"), new GUIContent("Prefab"));
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
                EditorGUILayout.LabelField("Правила розміщення", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("UseCustomTownHallRules"), new GUIContent("Кастомні правила центру"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("RequireTownHallInRange"), new GUIContent("Потребує центр у радіусі"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("BlockIfTownHallAlreadyInRange"), new GUIContent("Блокувати поруч з центром"));
                EditorGUILayout.PropertyField(buildingProperty.FindPropertyRelative("TownHallProximityRadiusOverride"), new GUIContent("Override радіусу"));
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

        private void DrawPlacementWorkspace()
        {
            var definition = GetSelectedDefinition();
            DrawSectionTitle("Симуляція розміщення", "Клік по мапі ставить тестову позицію або редагує сценарій.");

            DrawScenarioControls();
            DrawPlacementStatus(definition);
            DrawScenarioGrid(definition);
            DrawTileRequirementStatus(definition);
        }

        private void DrawPlayerWorkspace()
        {
            var definition = GetSelectedDefinition();
            DrawSectionTitle("Як це бачить гравець", "Кнопка меню, категорія, назва, іконка та placement-preview з поточного сценарію.");
            DrawPlayerMenuPreview(definition);
            DrawBuildingFacts(definition);
        }

        private void DrawRightPanel(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(PanelStyle(), options))
            {
                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);
                DrawSelectedSummary();
                DrawScenarioLegend();
                DrawAllBuildingsAtCandidate();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawScenarioControls()
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _scenarioWidth = EditorGUILayout.IntSlider("Ширина", _scenarioWidth, 7, 25);
                    _scenarioHeight = EditorGUILayout.IntSlider("Висота", _scenarioHeight, 7, 19);
                }

                _scenarioMinSpacing = EditorGUILayout.IntSlider("Min spacing", _scenarioMinSpacing, 0, 5);
                _scenarioTownHallRadius = EditorGUILayout.IntSlider("Радіус центру", _scenarioTownHallRadius, 0, 12);
                _scenarioFogEnabled = EditorGUILayout.Toggle("Перевіряти туман", _scenarioFogEnabled);
                _showPlacementHeatmap = EditorGUILayout.Toggle("Heatmap можна/не можна", _showPlacementHeatmap);
                _showInfluence = EditorGUILayout.Toggle("Показувати influence", _showInfluence);
                _showCoordinates = EditorGUILayout.Toggle("Координати", _showCoordinates);

                EditorGUILayout.Space(4f);
                _paintMode = (ScenarioPaintMode)GUILayout.Toolbar((int)_paintMode, PaintModeLabels);
                if (_paintMode == ScenarioPaintMode.Terrain)
                    _terrainIndex = EditorGUILayout.Popup("TileId", _terrainIndex, TerrainLabels);
                if (_paintMode == ScenarioPaintMode.Occupied || _paintMode == ScenarioPaintMode.Pending)
                    _paintBuildingId = EditorGUILayout.TextField("BuildingId", string.IsNullOrWhiteSpace(_paintBuildingId) ? GetSelectedDefinition()?.Id ?? string.Empty : _paintBuildingId);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Очистити сценарій"))
                        ClearScenario();
                    if (GUILayout.Button("Додати центр"))
                        SeedInfluenceCenter();
                    if (GUILayout.Button("Показати всю мапу"))
                        _scenarioFog.Clear();
                }
            }

            ClampScenarioCandidate();
        }

        private void DrawPlacementStatus(BuildingDefinition definition)
        {
            var placement = EvaluatePlacement(definition, _candidatePosition);
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                string title = placement.IsAllowed ? "Сценарій проходить" : "Сценарій блокується";
                DrawStatusBanner(title, placement.IsAllowed ? AllowedColor : BlockedColor);
                EditorGUILayout.LabelField($"Позиція: {_candidatePosition} | Будівля: {(definition != null ? definition.Id : "-")}", EditorStyles.miniLabel);

                if (!string.IsNullOrEmpty(placement.OverrideMessage))
                    EditorGUILayout.HelpBox(placement.OverrideMessage, placement.IsAllowed ? MessageType.Info : MessageType.Warning);

                if (placement.Evaluation != null)
                {
                    for (int blockerIndex = 0; blockerIndex < placement.Evaluation.Blockers.Count; blockerIndex++)
                    {
                        var blocker = placement.Evaluation.Blockers[blockerIndex];
                        EditorGUILayout.HelpBox(FormatBlocker(blocker), MessageType.Warning);
                    }

                    for (int noteIndex = 0; noteIndex < placement.Evaluation.Notes.Count; noteIndex++)
                        EditorGUILayout.LabelField(placement.Evaluation.Notes[noteIndex], EditorStyles.wordWrappedMiniLabel);
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
            for (int row = 0; row < _scenarioHeight; row++)
            {
                int tileY = _scenarioHeight - 1 - row;
                for (int tileX = 0; tileX < _scenarioWidth; tileX++)
                {
                    var tile = new Vector2Int(tileX, tileY);
                    var cellRect = new Rect(gridRect.x + tileX * cellSize, gridRect.y + row * cellSize, cellSize - 1f, cellSize - 1f);
                    DrawScenarioCell(cellRect, tile, definition);

                    if (cellRect.Contains(currentEvent.mousePosition) && currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        ApplyPaint(tile);
                        currentEvent.Use();
                    }
                }
            }
        }

        private void DrawScenarioCell(Rect rect, Vector2Int tile, BuildingDefinition definition)
        {
            EditorGUI.DrawRect(rect, TerrainColor(GetTerrain(tile)));

            if (_showInfluence && IsCoveredByAnyInfluence(tile))
                EditorGUI.DrawRect(rect, InfluenceColor);

            if (_showPlacementHeatmap && definition != null)
            {
                var preview = EvaluatePlacement(definition, tile);
                EditorGUI.DrawRect(rect, preview.IsAllowed
                    ? new Color(0.1f, 0.75f, 0.2f, 0.23f)
                    : new Color(0.85f, 0.15f, 0.12f, 0.20f));
            }

            if (_scenarioOccupants.TryGetValue(tile, out var occupantId))
            {
                EditorGUI.DrawRect(Shrink(rect, 3f), new Color(0.23f, 0.27f, 0.32f, 0.92f));
                GUI.Label(rect, ShortLabel(occupantId), CenteredCellStyle());
            }

            if (TryGetPending(tile, out var pendingEntry))
            {
                DrawBorder(rect, new Color(0.35f, 0.75f, 0.95f, 1f), 2f);
                GUI.Label(rect, ShortLabel(pendingEntry.BuildingId), CenteredCellStyle());
            }

            if (_scenarioFogEnabled && _scenarioFog.Contains(tile))
                EditorGUI.DrawRect(rect, new Color(0.02f, 0.03f, 0.04f, 0.58f));

            if (tile == _candidatePosition)
            {
                var preview = EvaluatePlacement(definition, tile);
                DrawBorder(rect, preview.IsAllowed ? Color.green : Color.red, 3f);
            }
            else
            {
                DrawBorder(rect, new Color(0f, 0f, 0f, 0.22f), 1f);
            }

            if (_showCoordinates)
                GUI.Label(new Rect(rect.x + 2f, rect.y + 1f, rect.width - 4f, 12f), $"{tile.x},{tile.y}", CoordinateStyle());
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
                    GUI.DrawTexture(iconRect, icon.texture, ScaleMode.ScaleToFit, true);
                else
                    GUI.Label(iconRect, "No icon", CenteredCellStyle());

                Rect textRect = new Rect(buttonRect.x + 102f, buttonRect.y + 18f, buttonRect.width - 114f, 90f);
                GUI.Label(textRect, string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName, PlayerButtonTitleStyle());
                GUI.Label(new Rect(textRect.x, textRect.y + 50f, textRect.width, 18f), definition.Category.ToString(), EditorStyles.miniLabel);

                var preview = EvaluatePlacement(definition, _candidatePosition);
                DrawStatusBanner(preview.IsAllowed ? "Placement preview: valid" : "Placement preview: blocked", preview.IsAllowed ? AllowedColor : BlockedColor);
            }
        }

        private void DrawBuildingFacts(BuildingDefinition definition)
        {
            if (definition == null)
                return;

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Факти", EditorStyles.boldLabel);
                DrawBadgeLine(BuildingDefinitionCapabilities.IsTownHall(definition) ? "Центр поселення: так" : "Центр поселення: ні", BuildingDefinitionCapabilities.IsTownHall(definition) ? AllowedColor : Color.gray);
                DrawBadgeLine(BuildingDefinitionCapabilities.IsCastle(definition) ? "Замок/столиця: так" : "Замок/столиця: ні", BuildingDefinitionCapabilities.IsCastle(definition) ? AllowedColor : Color.gray);
                DrawBadgeLine($"Працівники: {BuildingDefinitionCapabilities.GetRequiredWorkers(definition)}", Color.gray);
                DrawBadgeLine($"Житло: {BuildingDefinitionCapabilities.GetHousingCapacity(definition)}", Color.gray);
                DrawBadgeLine($"Радіус впливу: {BuildingPlacementEvaluator.ResolveInfluenceRadius(definition, _scenarioTownHallRadius)}", Color.gray);

                var issues = BuildingModuleValidation.Validate(definition);
                if (issues.Count == 0)
                    EditorGUILayout.HelpBox("Модулі валідні.", MessageType.Info);
                else
                {
                    for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++)
                    {
                        var issue = issues[issueIndex];
                        EditorGUILayout.HelpBox($"[{issue.Code}] {issue.Message}", issue.Severity == BuildingValidationSeverity.Error ? MessageType.Error : MessageType.Warning);
                    }
                }
            }
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
                EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName, EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(definition.Id, EditorStyles.miniLabel, GUILayout.Height(18f));
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
                DrawBadgeLine("Темний: туман", new Color(0.05f, 0.06f, 0.08f));
                DrawBadgeLine("Блакитна рамка: pending", new Color(0.35f, 0.75f, 0.95f));
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
                        if (GUILayout.Button(definition.Id, EditorStyles.miniLabel))
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
                    OverrideMessage = $"Ворота замінять стіну '{occupantId}' на цьому тайлі. Runtime пропускає загальну CanPlaceAt-перевірку для такої заміни.",
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

        private void CreateBuilding(SerializedProperty buildingsProperty)
        {
            string id = _newBuildingId.Trim();
            int index = buildingsProperty.arraySize;
            buildingsProperty.InsertArrayElementAtIndex(index);
            var buildingProperty = buildingsProperty.GetArrayElementAtIndex(index);

            buildingProperty.FindPropertyRelative("Id").stringValue = id;
            buildingProperty.FindPropertyRelative("DisplayName").stringValue = string.IsNullOrWhiteSpace(_newBuildingName) ? id : _newBuildingName.Trim();
            buildingProperty.FindPropertyRelative("Category").enumValueIndex = (int)_newBuildingCategory;
            buildingProperty.FindPropertyRelative("Icon").objectReferenceValue = _newBuildingSprite;
            buildingProperty.FindPropertyRelative("Prefab").objectReferenceValue = ResolvePrefab(id, _newBuildingPrefab, _newBuildingSprite);
            buildingProperty.FindPropertyRelative("ConstructionCost").arraySize = 0;
            buildingProperty.FindPropertyRelative("Modules").arraySize = 0;
            buildingProperty.FindPropertyRelative("UseCustomTownHallRules").boolValue = false;
            buildingProperty.FindPropertyRelative("RequireTownHallInRange").boolValue = true;
            buildingProperty.FindPropertyRelative("BlockIfTownHallAlreadyInRange").boolValue = false;
            buildingProperty.FindPropertyRelative("TownHallProximityRadiusOverride").intValue = 0;

            _selectedIndex = index;
            _registryObject.ApplyModifiedProperties();
            SaveRegistry();
            _newBuildingId = GenerateUniqueId("new-building", buildingsProperty);
            _newBuildingName = "Нова будівля";
            _newBuildingSprite = null;
            _newBuildingPrefab = null;
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
            if (_registryObject != null)
                _registryObject.ApplyModifiedProperties();
            if (_registry != null)
                EditorUtility.SetDirty(_registry);
            AssetDatabase.SaveAssets();
        }

        private void LoadRegistryPreference()
        {
            string guid = EditorPrefs.GetString(PrefKeyRegistryGuid, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(path))
                _registry = AssetDatabase.LoadAssetAtPath<BuildingRegistrySO>(path);
        }

        private void SaveRegistryPreference()
        {
            if (_registry == null)
                return;

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
            if (definition.Icon != null)
                return definition.Icon;
            if (definition.Prefab == null)
                return null;

            var renderers = definition.Prefab.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null && renderers[index].sprite != null)
                    return renderers[index].sprite;
            }

            return null;
        }

        private static string ShortLabel(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "?";
            string trimmed = id.Trim();
            return trimmed.Length <= 3 ? trimmed.ToUpperInvariant() : trimmed.Substring(0, 3).ToUpperInvariant();
        }

        private static string FormatBlocker(BuildingPlacementBlocker blocker)
        {
            if (blocker == null)
                return string.Empty;

            string positionText = blocker.Position.HasValue ? $" ({blocker.Position.Value})" : string.Empty;
            string buildingText = string.IsNullOrWhiteSpace(blocker.BuildingId) ? string.Empty : $" [{blocker.BuildingId}]";
            return $"{blocker.Kind}{positionText}{buildingText}: {blocker.Message}";
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
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.18f));
            if (sprite != null)
                GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit, true);
            else
                GUI.Label(rect, "-", CenteredCellStyle());
        }

        private static void DrawBadge(string text, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(Mathf.Max(34f, GUI.skin.label.CalcSize(new GUIContent(text)).x + 12f), 18f, GUILayout.Height(18f));
            EditorGUI.DrawRect(rect, color);
            GUI.Label(rect, text, BadgeStyle());
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
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8),
            };
            return style;
        }

        private static GUIStyle CardStyle()
        {
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 7, 7),
                margin = new RectOffset(2, 2, 3, 5),
            };
            return style;
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