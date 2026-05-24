using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor
{
    public sealed class MoyvaDeveloperHubWindow : EditorWindow
    {
        private enum Section
        {
            Dashboard,
            ProjectContext,
            Tools,
            Search,
            Assets,
            Monitor,
        }

        private const float SidebarWidth = 218f;
        private const float HeaderHeight = 74f;
        private const string AllCategories = "Усі";
        private const string OnboardingDonePrefsKey = "Moyva.DeveloperHub.OnboardingDone";

        private static readonly Color Accent = new Color(0.16f, 0.58f, 0.64f);
        private static readonly Color Accent2 = new Color(0.84f, 0.54f, 0.16f);
        private static readonly Color Good = new Color(0.16f, 0.62f, 0.38f);
        private static readonly Color Warn = new Color(0.86f, 0.55f, 0.14f);
        private static readonly Color Bad = new Color(0.82f, 0.24f, 0.24f);
        private static readonly Color Info = new Color(0.25f, 0.42f, 0.75f);

        private static readonly ToolDefinition[] Tools =
        {
            new ToolDefinition("Developer Hub", "Системи", "Moyva/Tools/Developer Hub", "Головний редакторський хаб: інструменти, прев'ю, пошук і моніторинг.", "d_UnityEditor.ConsoleWindow", true, "hub", "developer", "dashboard", "preview"),
            new ToolDefinition("Project Settings", "Системи", "Moyva/Project/Global Settings", "Глобальні режими сітки, проекції, preview та майбутні project-wide defaults.", "d_Settings", true, "project", "settings", "grid", "projection", "hex", "isometric", "3d"),
            new ToolDefinition("Registry Hub", "Реєстри", "Moyva/Tools/Registry Hub", "Центральний редактор тайлів, об'єктів, юнітів, будівель, стін і ресурсів.", "d_FilterByType", true, "registry", "tile", "object", "unit", "building", "wall", "resource", "додати", "змінити"),
            new ToolDefinition("Registry Factory Legacy", "Реєстри", "Moyva/Tools/Registry Factory (Legacy)", "Старий entry point, який перенаправляє у Registry Hub для сумісності зі звичним меню.", "d_FilterByType", false, "registry", "legacy", "factory", "old", "compatibility"),
            new ToolDefinition("Audio Designer", "Реєстри", "Moyva/Tools/Audio Designer", "Звуки під ключ, preview, ефекти, pool і вибір AudioKey.", "d_AudioSource Icon", true, "audio", "sound", "звук", "preview", "effects", "pool"),
            new ToolDefinition("Unit Designer", "Реєстри", "Moyva/Tools/Unit Designer", "Детальне налаштування юнітів, prefab, характеристики, preview руху та огляду.", "d_AvatarSelector", true, "unit", "юніт", "prefab", "stats", "vision", "animation"),
            new ToolDefinition("Building Designer", "Будівництво", "Moyva/Tools/Building Designer", "Візуальний редактор будівель із placement-симуляцією, heatmap і player-facing preview.", "d_BuildSettings.Standalone.Small", true, "building", "construction", "placement", "simulation", "heatmap", "будівля", "споруда"),
            new ToolDefinition("Редактор економіки", "Реєстри", "Moyva/Tools/Редактор Економіки", "Каталог ресурсів, settlement profiles, production, правила і перевірки економіки.", "d_UnityEditor.InspectorWindow", true, "economy", "resource", "production", "warehouse", "caravan", "економіка"),
            new ToolDefinition("Graph Editor", "Генерація світу", "Moyva/Graph Editor", "Візуальний граф генерації світу, nodes, preview і preset workflow.", "d_GraphView Icon", true, "graph", "generator", "world", "node", "preview", "map"),
            new ToolDefinition("World Defaults", "Генерація світу", "Moyva/World/Базові налаштування світу", "Базові world creation defaults і bootstrap-facing world параметри.", "d_SceneAsset Icon", false, "world", "defaults", "generation", "seed", "map"),
            new ToolDefinition("Object Rules: Empty Preset", "Генерація світу", "Assets/Moyva/ObjectConnectionRules/Apply Preset/Empty", "Context action для ObjectConnectionRules: застосувати порожній preset.", "d_ScriptableObject Icon", false, "object", "connection", "rules", "preset", "empty"),
            new ToolDefinition("Object Rules: River Preset", "Генерація світу", "Assets/Moyva/ObjectConnectionRules/Apply Preset/River", "Context action для ObjectConnectionRules: застосувати river preset.", "d_ScriptableObject Icon", false, "object", "connection", "rules", "preset", "river"),
            new ToolDefinition("Object Rules: Road Preset", "Генерація світу", "Assets/Moyva/ObjectConnectionRules/Apply Preset/Road", "Context action для ObjectConnectionRules: застосувати road preset.", "d_ScriptableObject Icon", false, "object", "connection", "rules", "preset", "road"),
            new ToolDefinition("Bootstrap Config", "Запуск гри", "Moyva/Bootstrap/Installer Config Editor", "Налаштування bootstrap installer config asset і стартових runtime параметрів.", "d_Settings", false, "bootstrap", "installer", "config", "start", "game"),
            new ToolDefinition("Gameplay Startup Graphics", "Запуск гри", "Moyva/Tools/Gameplay Startup Graphics", "Дефолти стартової графіки, zoom-профілі та дев-override піксельної оптимізації.", "d_PreMatCube", true, "graphics", "startup", "zoom", "pixel", "override", "quality"),
            new ToolDefinition("Стартовий спавн", "Запуск гри", "Moyva/Bootstrap/Дизайнер стартового спавну", "Візуальний дизайнер стартових позицій гравців.", "d_SceneViewOrtho", false, "spawn", "player", "start", "позиції", "старт"),
            new ToolDefinition("Multiplayer Config", "Системи", "Moyva/Multiplayer/Config Hub", "Налаштування multiplayer, Relay/Lobby/LAN режимів і конфігів.", "d_NetworkView Icon", false, "multiplayer", "relay", "lobby", "lan", "network"),
            new ToolDefinition("Calendar Config", "Системи", "Moyva/Calendar/Config Hub", "Календар, session clock і пов'язані параметри часу.", "d_UnityEditor.AnimationWindow", false, "calendar", "time", "session", "day", "season"),
            new ToolDefinition("Save System Designer", "Системи", "Moyva/Save System/Designer Tool", "Огляд save-system модулів, paths і runtime save workflow.", "d_SaveAs", false, "save", "load", "profile", "snapshot", "storage"),
            new ToolDefinition("Fog Vision Tuner", "Системи", "Moyva/Tools/Fog of War/Vision Tuner", "Налаштування Fog of War, vision radius і debug preview.", "d_SceneVisibility", false, "fog", "vision", "visibility", "туман", "огляд"),
            new ToolDefinition("Topology Resolver", "Будівництво", "Moyva/Construction/Topology Resolver Editor", "Редактор resolver topology для будівельних/стіночних правил.", "d_Grid.PaintTool", false, "construction", "topology", "resolver", "building", "wall"),
            new ToolDefinition("Construction UI Setup", "Будівництво", "Moyva/Construction/UI Setup Tool", "Setup helper для будівельного UI.", "d_UIElementsDebugger", false, "construction", "ui", "setup", "button", "canvas"),
            new ToolDefinition("Wall Registry", "Будівництво", "Moyva/Construction/Wall Registry Editor", "Окремий редактор wall collections і пов'язаних wall definitions.", "d_Grid.BoxTool", false, "wall", "construction", "registry", "collection"),
            new ToolDefinition("Prefab 2D Preview", "Візуал", "Moyva/Windows/Prefab 2D Preview Setup", "Швидкий setup і preview для 2D prefab assets.", "d_Prefab Icon", false, "prefab", "2d", "preview", "sprite", "visual"),
            new ToolDefinition("Force 2D Selected Prefab", "Візуал", "Moyva/Setup/Force 2D on Selected Prefab", "Переводить вибраний prefab у 2D-friendly вигляд.", "d_Transform Icon", false, "prefab", "2d", "selected", "sprite"),
            new ToolDefinition("Показати Sprite Bounds", "Візуал", "Moyva/Gizmo/Show Sprite Bounds", "Увімкнути gizmo bounds для sprite debugging.", "d_ViewToolOrbit", false, "gizmo", "sprite", "bounds", "debug"),
            new ToolDefinition("Сховати 3D Bounds", "Візуал", "Moyva/Gizmo/Hide 3D Bounds", "Прибрати зайві 3D bounds gizmos, коли працюєш у 2D.", "d_ViewToolMove", false, "gizmo", "3d", "bounds", "debug"),
            new ToolDefinition("Наповнити реєстр будівель", "Утиліти", "Moyva/Інструменти/Наповнити реєстр будівель", "Bulk helper для building registry population.", "d_Toolbar Plus", false, "building", "populate", "registry", "bulk"),
            new ToolDefinition("Folder Setup", "Утиліти", "Assets/Moyva/Script/Folder Setup", "Context action для feature folders: API/Runtime/Editor, asmdef і namespace cleanup.", "d_Folder Icon", false, "folder", "asmdef", "namespace", "feature", "setup"),
        };

        private static readonly RegistryTabShortcut[] RegistryTabs =
        {
            new RegistryTabShortcut("Tiles", "Тайли", 0, "Відкрити Registry Hub одразу на вкладці tile definitions.", "tile", "terrain", "grid", "тайл"),
            new RegistryTabShortcut("Map Objects", "Об'єкти", 1, "Відкрити MapObjects registry для генерації мапи.", "object", "map", "generator", "об'єкт"),
            new RegistryTabShortcut("Units", "Юніти", 2, "Відкрити registry вкладку юнітів у спільному hub.", "unit", "юніт", "worker", "military"),
            new RegistryTabShortcut("Buildings", "Будівлі", 3, "Відкрити building definitions і town/castle gameplay дані.", "building", "construction", "town", "castle", "будівля"),
            new RegistryTabShortcut("Walls", "Стіни", 4, "Відкрити wall collections і сегменти оборони.", "wall", "стіна", "construction", "defense"),
            new RegistryTabShortcut("Resources", "Ресурси", 5, "Відкрити ресурси, які використовуються economy/registry workflow.", "resource", "economy", "ресурс"),
            new RegistryTabShortcut("Bulk Delete", "Bulk", 6, "Відкрити вкладку масового очищення registry entries.", "bulk", "delete", "clean", "cleanup"),
        };

        private static readonly RegistryDefinition[] Registries =
        {
            new RegistryDefinition("Tile Registry", "TileRegistrySO", "Тайли", "Registry Hub", "Moyva/Tools/Registry Hub", "_definitions", "Definitions"),
            new RegistryDefinition("Map Object Registry", "MapObjectRegistrySO", "Об'єкти", "Registry Hub", "Moyva/Tools/Registry Hub", "_definitions", "Definitions"),
            new RegistryDefinition("Unit Registry", "UnitRegistrySO", "Юніти", "Unit Designer", "Moyva/Tools/Unit Designer", "Configs"),
            new RegistryDefinition("Building Registry", "BuildingRegistrySO", "Будівлі", "Registry Hub", "Moyva/Tools/Registry Hub", "Buildings", "WallCollections"),
            new RegistryDefinition("Audio Registry", "AudioRegistrySO", "Звуки", "Audio Designer", "Moyva/Tools/Audio Designer", "_sounds", "Sounds"),
            new RegistryDefinition("Economy Catalog", "EconomyDatabaseSO", "Економіка", "Редактор економіки", "Moyva/Tools/Редактор Економіки", "_resources", "_settlements", "_warehousePolicies", "_productionProfiles", "_caravanTemplates", "_aiRuleProfiles", "_mapObjectEconomyEntries"),
            new RegistryDefinition("Economy Rules", "EconomyRulesConfiguration", "Економіка", "Редактор економіки", "Moyva/Tools/Редактор Економіки"),
            new RegistryDefinition("Graph Assets", "GraphAsset", "Графи", "Graph Editor", "Moyva/Graph Editor"),
            new RegistryDefinition("Noise Settings", "DataNoiseSettings", "Генерація світу", "Graph Editor", "Moyva/Graph Editor"),
            new RegistryDefinition("Height Map Settings", "HeightMapSettings", "Генерація світу", "Graph Editor", "Moyva/Graph Editor"),
            new RegistryDefinition("World Defaults", "WorldCreationDefaultsSO", "Світ", "World Defaults", "Moyva/World/Базові налаштування світу"),
            new RegistryDefinition("Bootstrap Config", "BootstrapInstallerConfigSO", "Запуск", "Bootstrap Config", "Moyva/Bootstrap/Installer Config Editor"),
            new RegistryDefinition("Fog Settings", "FogOfWarSettings", "Туман", "Fog Vision Tuner", "Moyva/Tools/Fog of War/Vision Tuner"),
            new RegistryDefinition("Calendar Session", "CalendarSessionConfigSO", "Календар", "Calendar Config", "Moyva/Calendar/Config Hub"),
            new RegistryDefinition("Multiplayer Config", "MultiplayerConfig", "Мережа", "Multiplayer Config", "Moyva/Multiplayer/Config Hub"),
        };

        private readonly List<RegistryStatus> _registryStatuses = new List<RegistryStatus>();
        private readonly List<UnityEngine.Object> _assetSearchResults = new List<UnityEngine.Object>();

        private Section _section;
        private string _globalSearch = string.Empty;
        private string _selectedToolCategory = AllCategories;
        private string _assetSearch = string.Empty;
        private string _monitorFilter = string.Empty;
        private Vector2 _sidebarScroll;
        private Vector2 _contentScroll;
        private Vector2 _rightPanelScroll;
        private UnityEngine.Object _selectedAsset;
        private UnityEditor.Editor _selectedAssetEditor;
        private double _lastRefreshTime;
        private readonly Dictionary<string, IMoyvaHubPreviewProvider> _previewProvidersByMenuPath = new Dictionary<string, IMoyvaHubPreviewProvider>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _missingHubMenuPaths = new List<string>();
        private bool _showOnboardingTour;
        private int _onboardingStep;

        private static readonly TourStep[] OnboardingSteps =
        {
            new TourStep("Project Context", "Признач активні SO для ключових редакторів.", Section.ProjectContext),
            new TourStep("Registry Hub", "Перевір тайли, об'єкти, юніти і будівлі в одному місці.", Section.Tools),
            new TourStep("Asset Health", "Запусти скан проблем посилань і дублікатів ID.", Section.Monitor),
            new TourStep("Search by Action", "Спробуй: 'додати юніта', 'відкрити fog', 'перевірити економіку'.", Section.Search),
        };

        [MenuItem("Moyva/Tools/Developer Hub %#h", priority = 0)]
        public static void Open()
        {
            var window = GetWindow<MoyvaDeveloperHubWindow>("Moyva Hub");
            window.minSize = new Vector2(1060f, 640f);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            MoyvaProjectEditorContext.AssetChanged += OnProjectContextAssetChanged;
            RefreshStatus();
            RefreshPreviewProviders();
            _showOnboardingTour = !EditorPrefs.GetBool(OnboardingDonePrefsKey, false);
            _onboardingStep = Mathf.Clamp(_onboardingStep, 0, Mathf.Max(0, OnboardingSteps.Length - 1));
        }

        private void OnDisable()
        {
            MoyvaProjectEditorContext.AssetChanged -= OnProjectContextAssetChanged;
            if (_selectedAssetEditor != null)
                DestroyImmediate(_selectedAssetEditor);
        }

        private void OnGUI()
        {
            if (EditorApplication.timeSinceStartup - _lastRefreshTime > 8f)
                RefreshStatus();

            if (Event.current.type == EventType.Layout)
                RefreshPreviewProviders();

            DrawHeader();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSidebar(GUILayout.Width(SidebarWidth));
                DrawContent();
            }
        }

        private void DrawHeader()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.11f, 0.13f, 0.15f) : new Color(0.78f, 0.83f, 0.86f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), Accent);

            Rect titleRect = new Rect(rect.x + 18f, rect.y + 10f, 360f, 30f);
            GUI.Label(titleRect, "Moyva Developer Hub", HeaderTitleStyle());
            GUI.Label(new Rect(titleRect.x, titleRect.yMax + 2f, 520f, 18f), "Одна панель для реєстрів, дизайнерів, конфігів, моніторингу і пошуку.", HeaderSubtitleStyle());

            Rect searchRect = new Rect(rect.xMax - 474f, rect.y + 16f, 300f, 30f);
            EditorGUI.BeginChangeCheck();
            string search = GUI.TextField(searchRect, _globalSearch, SearchStyle());
            if (EditorGUI.EndChangeCheck())
            {
                _globalSearch = search;
                if (!string.IsNullOrWhiteSpace(_globalSearch))
                    _section = Section.Search;
            }

            GUI.Label(new Rect(searchRect.x + 10f, searchRect.y + 7f, searchRect.width - 20f, 16f), string.IsNullOrEmpty(_globalSearch) ? "Пошук: tool, registry, sound, fog, unit..." : string.Empty, PlaceholderStyle());

            if (GUI.Button(new Rect(searchRect.xMax + 8f, searchRect.y, 72f, 30f), "Пошук", PrimaryButtonStyle()))
                _section = Section.Search;

            if (GUI.Button(new Rect(searchRect.xMax + 86f, searchRect.y, 76f, 30f), "Refresh", SecondaryButtonStyle()))
                RefreshStatus();
        }

        private void DrawSidebar(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(SidebarStyle(), options))
            {
                _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);
                DrawNavButton(Section.Dashboard, "Головна", "стан проєкту");
                DrawNavButton(Section.ProjectContext, "Контекст", "активні SO");
                DrawNavButton(Section.Tools, "Інструменти", $"{Tools.Length} входів");
                DrawNavButton(Section.Search, "Пошук", "усе одразу");
                DrawNavButton(Section.Assets, "Assets", "registry + config");
                DrawNavButton(Section.Monitor, "Моніторинг", "порожні / відсутні");

                GUILayout.Space(12f);
                DrawSidebarBlock("Швидкий старт", new[]
                {
                    Tools.First(t => t.Title == "Registry Hub"),
                    Tools.First(t => t.Title == "Audio Designer"),
                    Tools.First(t => t.Title == "Unit Designer"),
                    Tools.First(t => t.Title == "Graph Editor"),
                });
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawContent()
        {
            _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);
            switch (_section)
            {
                case Section.ProjectContext:
                    DrawProjectContextSection();
                    break;
                case Section.Tools:
                    DrawToolsSection();
                    break;
                case Section.Search:
                    DrawSearchSection();
                    break;
                case Section.Assets:
                    DrawAssetsSection();
                    break;
                case Section.Monitor:
                    DrawMonitorSection();
                    break;
                default:
                    DrawDashboard();
                    break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawDashboard()
        {
            DrawSectionTitle("Головна", "Те, що розробнику найчастіше потрібно бачити першим.");
            DrawOnboardingTour();
            DrawHeroMetrics();
            DrawProjectContextDashboardCard();
            DrawHubCoverageDiagnostics();
            DrawRecommendations();
            DrawPrimaryTools();
            DrawToolsPreviewGallery();
            DrawRegistryTabShortcuts();
            DrawRegistryOverview();
        }

        private void DrawHeroMetrics()
        {
            int missing = _registryStatuses.Count(s => s.AssetCount == 0);
            int empty = _registryStatuses.Count(s => s.AssetCount > 0 && s.HasCountableEntries && s.EntryCount == 0);
            int entries = _registryStatuses.Sum(s => s.EntryCount);
            int scenes = EditorBuildSettings.scenes.Count(s => s.enabled);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawMetricCard("Editor Tools", Tools.Length.ToString(), "доступно з одного місця", Accent);
                DrawMetricCard("Registry Assets", _registryStatuses.Sum(s => s.AssetCount).ToString(), "знайдено у проєкті", Good);
                DrawMetricCard("Entries", entries.ToString(), "налаштовані записи", Info);
                DrawMetricCard("Attention", (missing + empty).ToString(), $"{missing} missing, {empty} empty", missing + empty > 0 ? Warn : Good);
                DrawMetricCard("Build Scenes", scenes.ToString(), "enabled scenes", Accent2);
            }
        }

        private void DrawProjectContextDashboardCard()
        {
            int scriptableDefinitions = Registries.Count(definition => MoyvaProjectEditorContext.ResolveScriptableObjectType(definition.TypeName) != null);
            int selected = Registries.Count(definition =>
            {
                Type type = MoyvaProjectEditorContext.ResolveScriptableObjectType(definition.TypeName);
                return type != null && MoyvaProjectEditorContext.Get(definition.TypeName, type) != null;
            });

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawCardHeader("Project Context", $"{selected}/{scriptableDefinitions} активних ScriptableObject", selected == scriptableDefinitions ? Good : Warn);
                EditorGUILayout.LabelField("Тут задається один набір активних registry/config assets для всіх редакторів. Якщо дизайнер змінює свій SO, він оновлює цей контекст.", WrapMiniLabelStyle());
                if (GUILayout.Button("Відкрити централізований вибір", PrimaryButtonStyle(), GUILayout.Height(28f)))
                    _section = Section.ProjectContext;
            }
        }

        private void DrawProjectContextSection()
        {
            DrawSectionTitle("Project Context", "Централізований набір ScriptableObject, який використовують designer/editor windows.");
            EditorGUILayout.HelpBox(
                "Обери тут активні SO для проєкту. Registry Hub, Unit Designer, Building Designer, Audio Designer та інші підключені редактори читають ці значення при відкритті, а локальна зміна в редакторі записує нове значення назад сюди.",
                MessageType.Info);

            foreach (var group in Registries.GroupBy(definition => definition.Area).OrderBy(group => group.Key))
            {
                DrawSectionTitle(group.Key, string.Empty, false);
                using (new EditorGUILayout.VerticalScope(CardStyle()))
                {
                    foreach (var definition in group)
                        DrawProjectContextRow(definition);
                }
            }
        }

        private void DrawProjectContextRow(RegistryDefinition definition)
        {
            Type assetType = MoyvaProjectEditorContext.ResolveScriptableObjectType(definition.TypeName);
            if (assetType == null)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawBadge("Skip", Warn, GUILayout.Width(54f));
                    EditorGUILayout.LabelField(definition.Label, ToolTitleStyle(), GUILayout.Width(170f));
                    EditorGUILayout.LabelField($"{definition.TypeName} не є ScriptableObject asset або тип не скомпільовано.", WrapMiniLabelStyle());
                }
                return;
            }

            UnityEngine.Object active = MoyvaProjectEditorContext.Get(definition.TypeName, assetType);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBadge(active != null ? "Active" : "Empty", active != null ? Good : Warn, GUILayout.Width(62f));
                EditorGUILayout.LabelField(new GUIContent(definition.Label, definition.TypeName), ToolTitleStyle(), GUILayout.Width(170f));

                EditorGUI.BeginChangeCheck();
                UnityEngine.Object next = EditorGUILayout.ObjectField(active, assetType, false, GUILayout.MinWidth(240f));
                if (EditorGUI.EndChangeCheck())
                {
                    MoyvaProjectEditorContext.Set(definition.TypeName, next);
                    if (next != null)
                        SelectAsset(next);
                    RefreshStatus();
                }

                if (GUILayout.Button("Auto", GUILayout.Width(52f)))
                {
                    UnityEngine.Object found = MoyvaProjectEditorContext.FindFirstAsset(assetType);
                    MoyvaProjectEditorContext.Set(definition.TypeName, found);
                    if (found != null)
                        SelectAsset(found);
                    RefreshStatus();
                }

                using (new EditorGUI.DisabledScope(active == null))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(52f)))
                        Ping(active);
                    if (GUILayout.Button("Inspector", GUILayout.Width(78f)))
                    {
                        SelectAsset(active);
                        _section = Section.Assets;
                    }
                    if (GUILayout.Button("Clear", GUILayout.Width(52f)))
                    {
                        MoyvaProjectEditorContext.Set(definition.TypeName, null);
                        RefreshStatus();
                    }
                }

                if (GUILayout.Button(definition.ToolLabel, GUILayout.Width(132f)))
                    ExecuteMenu(definition.ToolMenuPath);
            }

            string path = MoyvaProjectEditorContext.GetAssetPath(active);
            if (!string.IsNullOrWhiteSpace(path))
                EditorGUILayout.LabelField(path, MenuPathStyle());
        }

        private void DrawRecommendations()
        {
            var issues = BuildRecommendations();
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawCardHeader("Що варто перевірити", issues.Count == 0 ? "усе ключове виглядає готовим" : "хаб підказує наступний крок", issues.Count == 0 ? Good : Warn);

                if (issues.Count == 0)
                {
                    EditorGUILayout.LabelField("Ключові registry/config assets знайдені. Можна переходити до редагування або запуску гри.", WrapLabelStyle());
                    return;
                }

                for (int i = 0; i < Mathf.Min(issues.Count, 5); i++)
                    DrawRecommendationRow(issues[i]);
            }
        }

        private void DrawPrimaryTools()
        {
            DrawSectionTitle("Швидкі дії", "Найкоротший шлях до додати, змінити або перевірити.", false);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var tool in Tools.Where(t => t.Primary).Take(5))
                    DrawCompactToolCard(tool);
            }
        }

        private void DrawRegistryOverview()
        {
            DrawSectionTitle("Registry Overview", "Стан основних ScriptableObject джерел даних.", false);
            for (int i = 0; i < _registryStatuses.Count; i += 3)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int j = 0; j < 3 && i + j < _registryStatuses.Count; j++)
                        DrawRegistryCard(_registryStatuses[i + j]);
                }
            }
        }

        private void DrawToolsSection()
        {
            DrawSectionTitle("Інструменти", "Усі editor windows і setup actions, які хаб може відкрити напряму.");
            DrawCategoryTabs();
            DrawToolsPreviewGallery();
            if (_selectedToolCategory == AllCategories || _selectedToolCategory == "Реєстри")
                DrawRegistryTabShortcuts();

            var filtered = Tools.Where(PassesToolFilters).ToArray();
            if (filtered.Length == 0)
            {
                EditorGUILayout.HelpBox("Нічого не знайдено за поточним фільтром.", MessageType.Info);
                return;
            }

            foreach (var group in filtered.GroupBy(t => t.Category).OrderBy(g => g.Key))
            {
                DrawSectionTitle(group.Key, $"{group.Count()} tool(s)", false);
                foreach (var tool in group)
                    DrawToolRow(tool);
            }
        }

        private void DrawSearchSection()
        {
            DrawSectionTitle("Глобальний пошук", "Шукає по editor tools, ключових словах, registry/config assets і шляхах.");

            using (new EditorGUILayout.HorizontalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Keyword", GUILayout.Width(64f));
                _globalSearch = EditorGUILayout.TextField(_globalSearch, GUILayout.MinWidth(280f));
                if (GUILayout.Button("Очистити", GUILayout.Width(88f)))
                    _globalSearch = string.Empty;
                if (GUILayout.Button("Refresh", GUILayout.Width(82f)))
                    RefreshStatus();
            }

            string query = _globalSearch.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                EditorGUILayout.HelpBox("Введи слово: наприклад audio, unit, fog, world, building, resource, config або частину назви asset.", MessageType.Info);
                DrawPrimaryTools();
                return;
            }

            var toolResults = ResolveSmartToolSearch(query);
            var tabResults = RegistryTabs.Where(t => t.Matches(query)).ToArray();
            RefreshAssetSearch(query);

            DrawSectionTitle($"Tools ({toolResults.Length})", "відкривають редактори або setup actions", false);
            foreach (var tool in toolResults)
                DrawToolRow(tool);

            DrawSectionTitle($"Registry Tabs ({tabResults.Length})", "один клік до потрібної вкладки Registry Hub", false);
            foreach (var tab in tabResults)
                DrawRegistryTabRow(tab);

            DrawSectionTitle($"Assets ({_assetSearchResults.Count})", "можна ping/open або редагувати праворуч", false);
            DrawAssetResults(_assetSearchResults);
        }

        private void DrawOnboardingTour()
        {
            if (!_showOnboardingTour || OnboardingSteps.Length == 0)
                return;

            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawCardHeader("Designer Onboarding Tour", $"Крок {_onboardingStep + 1}/{OnboardingSteps.Length}", Accent);
                var step = OnboardingSteps[Mathf.Clamp(_onboardingStep, 0, OnboardingSteps.Length - 1)];
                EditorGUILayout.LabelField(step.Title, ToolTitleStyle());
                EditorGUILayout.LabelField(step.Description, WrapMiniLabelStyle());

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_onboardingStep <= 0))
                    {
                        if (GUILayout.Button("Назад", GUILayout.Width(72f)))
                            _onboardingStep = Mathf.Max(0, _onboardingStep - 1);
                    }

                    if (GUILayout.Button("Перейти", PrimaryButtonStyle(), GUILayout.Width(90f)))
                        _section = step.TargetSection;

                    GUILayout.FlexibleSpace();

                    if (_onboardingStep < OnboardingSteps.Length - 1)
                    {
                        if (GUILayout.Button("Далі", GUILayout.Width(72f)))
                            _onboardingStep = Mathf.Min(OnboardingSteps.Length - 1, _onboardingStep + 1);
                    }
                    else if (GUILayout.Button("Завершити", GUILayout.Width(86f)))
                    {
                        _showOnboardingTour = false;
                        EditorPrefs.SetBool(OnboardingDonePrefsKey, true);
                    }
                }
            }
        }

        private ToolDefinition[] ResolveSmartToolSearch(string query)
        {
            var normalized = query.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return Array.Empty<ToolDefinition>();

            var results = new List<(ToolDefinition tool, int score)>();
            foreach (var tool in Tools)
            {
                var score = tool.Score(normalized);
                score += ScoreActionIntent(tool, normalized);
                if (score > 0)
                    results.Add((tool, score));
            }

            return results
                .OrderByDescending(pair => pair.score)
                .ThenBy(pair => pair.tool.Title)
                .Select(pair => pair.tool)
                .ToArray();
        }

        private static int ScoreActionIntent(ToolDefinition tool, string query)
        {
            int score = 0;
            bool hasCreateIntent = Contains(query, "додати") || Contains(query, "створити") || Contains(query, "create");
            bool hasValidateIntent = Contains(query, "перевір") || Contains(query, "валіда") || Contains(query, "health");
            bool hasFixIntent = Contains(query, "виправ") || Contains(query, "fix") || Contains(query, "quick fix");
            bool hasPreviewIntent = Contains(query, "preview") || Contains(query, "прев") || Contains(query, "перегляд");

            if (hasCreateIntent && (tool.Matches("registry") || tool.Matches("designer")))
                score += 3;
            if (hasValidateIntent && (tool.Matches("health") || tool.Matches("config") || tool.Matches("hub")))
                score += 4;
            if (hasFixIntent && (tool.Matches("registry") || tool.Matches("economy") || tool.Matches("audio")))
                score += 2;
            if (hasPreviewIntent && (tool.Matches("preview") || tool.Matches("fog") || tool.Matches("graph")))
                score += 2;

            return score;
        }
        private void DrawRegistryTabShortcuts()
        {
            DrawSectionTitle("Registry Tabs", "Прямий перехід до потрібної вкладки Registry Hub.", false);
            for (int i = 0; i < RegistryTabs.Length; i += 4)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int j = 0; j < 4 && i + j < RegistryTabs.Length; j++)
                        DrawRegistryTabCard(RegistryTabs[i + j]);
                }
            }
        }

        private void DrawAssetsSection()
        {
            DrawSectionTitle("Assets", "Registry/config assets із inline Inspector для швидкого редагування.");

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(420f)))
                {
                    using (new EditorGUILayout.HorizontalScope(CardStyle()))
                    {
                        EditorGUILayout.LabelField("Фільтр", GUILayout.Width(52f));
                        _assetSearch = EditorGUILayout.TextField(_assetSearch);
                        if (GUILayout.Button("Refresh", GUILayout.Width(82f)))
                            RefreshStatus();
                    }

                    var assets = CollectKnownAssets(_assetSearch).ToList();
                    DrawAssetResults(assets);
                }

                DrawSelectedAssetPanel(GUILayout.Width(Mathf.Clamp(position.width * 0.38f, 360f, 520f)));
            }
        }

        private void DrawMonitorSection()
        {
            DrawSectionTitle("Моніторинг", "Порожні registry, missing config assets і швидкі переходи до потрібного editor tool.");

            using (new EditorGUILayout.HorizontalScope(CardStyle()))
            {
                EditorGUILayout.LabelField("Фільтр", GUILayout.Width(52f));
                _monitorFilter = EditorGUILayout.TextField(_monitorFilter);
                if (GUILayout.Button("Refresh", GUILayout.Width(82f)))
                    RefreshStatus();
            }

            foreach (var status in _registryStatuses.Where(PassesMonitorFilter))
                DrawMonitorRow(status);
        }

        private void DrawCategoryTabs()
        {
            var categories = new[] { AllCategories }.Concat(Tools.Select(t => t.Category).Distinct().OrderBy(c => c)).ToArray();
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (string category in categories)
                {
                    bool selected = _selectedToolCategory == category;
                    Color old = GUI.backgroundColor;
                    if (selected)
                        GUI.backgroundColor = Accent;

                    if (GUILayout.Button(category, selected ? ActivePillStyle() : PillStyle(), GUILayout.Height(25f)))
                        _selectedToolCategory = category;

                    GUI.backgroundColor = old;
                }
            }
        }

        private void DrawToolRow(ToolDefinition tool)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIcon(tool.IconName, GUILayout.Width(28f), GUILayout.Height(28f));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(tool.Title, ToolTitleStyle());
                        EditorGUILayout.LabelField(tool.Description, WrapMiniLabelStyle());
                        EditorGUILayout.LabelField(tool.MenuPath, MenuPathStyle());
                    }

                    GUILayout.FlexibleSpace();
                    DrawBadge(tool.Category, Accent2, GUILayout.Width(96f));
                    if (GUILayout.Button("Open", PrimaryButtonStyle(), GUILayout.Width(86f), GUILayout.Height(30f)))
                        OpenTool(tool);
                }
            }
        }

        private void DrawToolsPreviewGallery()
        {
            DrawSectionTitle("Preview Gallery", "Прев'ю інструментів. Подвійний клік по preview відкриває пов'язані налаштування.", false);

            var filtered = Tools.Where(PassesToolFilters).ToArray();
            if (filtered.Length == 0)
            {
                EditorGUILayout.HelpBox("Немає інструментів для показу прев'ю за поточним фільтром.", MessageType.Info);
                return;
            }

            const int columns = 3;
            for (int i = 0; i < filtered.Length; i += columns)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (int j = 0; j < columns && i + j < filtered.Length; j++)
                        DrawToolPreviewCard(filtered[i + j]);
                }
            }
        }

        private void DrawToolPreviewCard(ToolDefinition tool)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle(), GUILayout.MinWidth(240f), GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField(tool.Title, ToolTitleStyle());
                EditorGUILayout.LabelField(tool.MenuPath, MenuPathStyle());

                Rect rect = GUILayoutUtility.GetRect(220f, 112f, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.14f, 0.16f) : new Color(0.86f, 0.88f, 0.9f));

                IMoyvaHubPreviewProvider provider = null;
                if (_previewProvidersByMenuPath.TryGetValue(tool.MenuPath, out provider) && provider != null)
                {
                    provider.DrawHubPreview(rect);
                }
                else
                {
                    DrawGenericPreview(rect, tool);
                }

                string summary = provider != null ? provider.GetHubPreviewSummary() : tool.Description;
                EditorGUILayout.LabelField(summary, WrapMiniLabelStyle(), GUILayout.Height(30f));

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Open", PrimaryButtonStyle(), GUILayout.Height(24f)))
                        OpenTool(tool);
                    if (GUILayout.Button("Settings", GUILayout.Height(24f)))
                        OpenToolSettings(tool, provider);
                }

                HandlePreviewDoubleClick(rect, tool, provider);
            }
        }

        private static void DrawGenericPreview(Rect rect, ToolDefinition tool)
        {
            float tint = Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup * 0.6f)) * 0.08f;
            EditorGUI.DrawRect(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f), new Color(0.16f + tint, 0.22f + tint, 0.28f + tint));

            GUIStyle center = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            GUI.Label(rect, tool.Title, center);
        }

        private void HandlePreviewDoubleClick(Rect rect, ToolDefinition tool, IMoyvaHubPreviewProvider provider)
        {
            Event evt = Event.current;
            if (evt.type != EventType.MouseDown || evt.button != 0 || evt.clickCount != 2)
                return;
            if (!rect.Contains(evt.mousePosition))
                return;

            OpenToolSettings(tool, provider);
            evt.Use();
        }

        private void OpenToolSettings(ToolDefinition tool, IMoyvaHubPreviewProvider provider)
        {
            if (provider is IMoyvaHubSettingsOpener opener && opener.OpenHubSettingsFromPreview())
                return;

            OpenTool(tool);
        }

        private void DrawHubCoverageDiagnostics()
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                DrawCardHeader("Доступність інструментів через Hub", "Перевірка menu coverage для Moyva/Tools", _missingHubMenuPaths.Count == 0 ? Good : Warn);

                if (_missingHubMenuPaths.Count == 0)
                {
                    EditorGUILayout.LabelField("Усі menu paths із Hub знайдені в Unity Editor меню.", WrapMiniLabelStyle());
                    return;
                }

                EditorGUILayout.LabelField($"Виявлено {_missingHubMenuPaths.Count} paths, які зараз відсутні в меню:", WrapMiniLabelStyle());
                for (int i = 0; i < Mathf.Min(8, _missingHubMenuPaths.Count); i++)
                    EditorGUILayout.LabelField($"- {_missingHubMenuPaths[i]}", WrapMiniLabelStyle());
            }
        }

        private void DrawCompactToolCard(ToolDefinition tool)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle(), GUILayout.MinWidth(158f), GUILayout.MaxWidth(220f)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIcon(tool.IconName, GUILayout.Width(24f), GUILayout.Height(24f));
                    EditorGUILayout.LabelField(tool.Title, ToolTitleStyle());
                }

                GUILayout.Label(tool.Description, WrapMiniLabelStyle(), GUILayout.Height(44f));
                if (GUILayout.Button("Open", PrimaryButtonStyle(), GUILayout.Height(28f)))
                    OpenTool(tool);
            }
        }
        private void DrawRegistryTabCard(RegistryTabShortcut tab)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle(), GUILayout.MinWidth(136f), GUILayout.ExpandWidth(true)))
            {
                DrawCardHeader(tab.Title, tab.Area, Accent);
                GUILayout.Label(tab.Description, WrapMiniLabelStyle(), GUILayout.Height(34f));
                if (GUILayout.Button("Open Tab", PrimaryButtonStyle(), GUILayout.Height(26f)))
                    RegistryHubWindow.Open(tab.TabIndex);
            }
        }

        private void DrawRegistryTabRow(RegistryTabShortcut tab)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawBadge(tab.Area, Accent, GUILayout.Width(82f));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(tab.Title, ToolTitleStyle());
                        EditorGUILayout.LabelField(tab.Description, WrapMiniLabelStyle());
                    }

                    if (GUILayout.Button("Open Tab", PrimaryButtonStyle(), GUILayout.Width(92f), GUILayout.Height(28f)))
                        RegistryHubWindow.Open(tab.TabIndex);
                }
            }
        }

        private void DrawRegistryCard(RegistryStatus status)
        {
            Color color = ResolveStatusColor(status);
            using (new EditorGUILayout.VerticalScope(CardStyle(), GUILayout.MinWidth(230f), GUILayout.ExpandWidth(true)))
            {
                DrawCardHeader(status.Definition.Label, status.Definition.Area, color);
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawMiniStat("Assets", status.AssetCount.ToString(), color);
                    DrawMiniStat("Entries", status.HasCountableEntries ? status.EntryCount.ToString() : "-", Info);
                    DrawMiniStat("State", ResolveStatusLabel(status), color);
                }

                GUILayout.Space(5f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Редагувати", GUILayout.Height(24f)))
                        ExecuteMenu(status.Definition.ToolMenuPath);

                    using (new EditorGUI.DisabledScope(status.FirstAsset == null))
                    {
                        if (GUILayout.Button("Inspector", GUILayout.Height(24f)))
                        {
                            SelectAsset(status.FirstAsset);
                            _section = Section.Assets;
                        }
                    }
                }
            }
        }

        private void DrawMonitorRow(RegistryStatus status)
        {
            Color color = ResolveStatusColor(status);
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawBadge(ResolveStatusLabel(status), color, GUILayout.Width(82f));
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(status.Definition.Label, ToolTitleStyle());
                        string detail = status.AssetCount == 0
                            ? $"{status.Definition.TypeName} asset не знайдено."
                            : $"Assets: {status.AssetCount}, entries: {(status.HasCountableEntries ? status.EntryCount.ToString() : "n/a")}, first: {status.FirstAssetPath}";
                        EditorGUILayout.LabelField(detail, WrapMiniLabelStyle());
                    }

                    if (GUILayout.Button(status.Definition.ToolLabel, GUILayout.Width(146f), GUILayout.Height(28f)))
                        ExecuteMenu(status.Definition.ToolMenuPath);

                    using (new EditorGUI.DisabledScope(status.FirstAsset == null))
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(54f), GUILayout.Height(28f)))
                            Ping(status.FirstAsset);
                    }
                }
            }
        }

        private void DrawAssetResults(IReadOnlyList<UnityEngine.Object> assets)
        {
            if (assets.Count == 0)
            {
                EditorGUILayout.HelpBox("Assets не знайдено.", MessageType.Info);
                return;
            }

            foreach (var asset in assets)
            {
                if (asset == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(asset);
                using (new EditorGUILayout.VerticalScope(CardStyle()))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Texture icon = AssetPreview.GetMiniThumbnail(asset);
                        if (icon != null)
                            GUILayout.Label(icon, GUILayout.Width(24f), GUILayout.Height(24f));

                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField(asset.name, ToolTitleStyle());
                            EditorGUILayout.LabelField(path, MenuPathStyle());
                        }

                        if (GUILayout.Button("Edit", PrimaryButtonStyle(), GUILayout.Width(62f), GUILayout.Height(26f)))
                        {
                            SelectAsset(asset);
                            _section = Section.Assets;
                        }

                        if (GUILayout.Button("Ping", GUILayout.Width(52f), GUILayout.Height(26f)))
                            Ping(asset);
                    }
                }
            }
        }

        private void DrawSelectedAssetPanel(params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle(), options))
            {
                DrawCardHeader("Inline Inspector", _selectedAsset != null ? _selectedAsset.name : "обери asset зліва", _selectedAsset != null ? Accent : Info);
                if (_selectedAsset == null)
                {
                    EditorGUILayout.HelpBox("Обери registry/config asset, і тут з'явиться його inspector. Так можна швидко щось змінити, не виходячи з hub.", MessageType.Info);
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField(_selectedAsset, typeof(UnityEngine.Object), false);
                    if (GUILayout.Button("Ping", GUILayout.Width(54f)))
                        Ping(_selectedAsset);
                    if (GUILayout.Button("Open", GUILayout.Width(58f)))
                        AssetDatabase.OpenAsset(_selectedAsset);
                }

                _rightPanelScroll = EditorGUILayout.BeginScrollView(_rightPanelScroll);
                UnityEditor.Editor.CreateCachedEditor(_selectedAsset, null, ref _selectedAssetEditor);
                if (_selectedAssetEditor != null)
                    _selectedAssetEditor.OnInspectorGUI();
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawRecommendationRow(Recommendation recommendation)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawBadge(recommendation.Severity, recommendation.Color, GUILayout.Width(74f));
                EditorGUILayout.LabelField(recommendation.Text, WrapLabelStyle());
                if (GUILayout.Button(recommendation.ActionLabel, GUILayout.Width(142f), GUILayout.Height(24f)))
                    ExecuteMenu(recommendation.MenuPath);
            }
        }

        private void DrawNavButton(Section section, string label, string detail)
        {
            bool active = _section == section;
            Rect rect = GUILayoutUtility.GetRect(0f, 44f, GUILayout.ExpandWidth(true));
            Color color = active ? Accent : (EditorGUIUtility.isProSkin ? new Color(0.16f, 0.17f, 0.18f) : new Color(0.84f, 0.86f, 0.88f));
            EditorGUI.DrawRect(rect, color);
            if (active)
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4f, rect.height), Accent2);

            GUI.Label(new Rect(rect.x + 12f, rect.y + 6f, rect.width - 24f, 18f), label, active ? SidebarActiveStyle() : SidebarTitleStyle());
            GUI.Label(new Rect(rect.x + 12f, rect.y + 24f, rect.width - 24f, 14f), detail, active ? SidebarActiveMiniStyle() : SidebarMiniStyle());

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                _section = section;
                GUI.FocusControl(null);
                Event.current.Use();
            }

            GUILayout.Space(3f);
        }

        private void DrawSidebarBlock(string title, IEnumerable<ToolDefinition> tools)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle()))
            {
                EditorGUILayout.LabelField(title, ToolTitleStyle());
                foreach (var tool in tools)
                {
                    if (GUILayout.Button(tool.Title, GUILayout.Height(24f)))
                        OpenTool(tool);
                }
            }
        }

        private void DrawSectionTitle(string title, string subtitle, bool spacing = true)
        {
            if (spacing)
                GUILayout.Space(8f);

            EditorGUILayout.LabelField(title, SectionTitleStyle());
            if (!string.IsNullOrWhiteSpace(subtitle))
                EditorGUILayout.LabelField(subtitle, WrapMiniLabelStyle());
            GUILayout.Space(6f);
        }

        private void DrawCardHeader(string title, string subtitle, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 34f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color * (EditorGUIUtility.isProSkin ? 0.72f : 0.86f));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 3f, rect.width - 20f, 16f), title, CardHeaderStyle());
            GUI.Label(new Rect(rect.x + 10f, rect.y + 18f, rect.width - 20f, 13f), subtitle, CardHeaderMiniStyle());
        }

        private void DrawMetricCard(string title, string value, string detail, Color color)
        {
            using (new EditorGUILayout.VerticalScope(CardStyle(), GUILayout.MinWidth(150f), GUILayout.ExpandWidth(true)))
            {
                DrawCardHeader(title, detail, color);
                GUILayout.Space(6f);
                EditorGUILayout.LabelField(value, MetricValueStyle());
            }
        }

        private void DrawMiniStat(string title, string value, Color color)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(56f)))
            {
                EditorGUILayout.LabelField(value, MiniStatValueStyle(color));
                EditorGUILayout.LabelField(title, CenterMiniStyle());
            }
        }

        private void DrawBadge(string text, Color color, params GUILayoutOption[] options)
        {
            Rect rect = GUILayoutUtility.GetRect(48f, 20f, BadgeStyle(), options);
            EditorGUI.DrawRect(rect, color * 0.9f);
            GUI.Label(rect, text, BadgeStyle());
        }

        private void DrawIcon(string iconName, params GUILayoutOption[] options)
        {
            GUIContent content = EditorGUIUtility.IconContent(iconName);
            if (content != null && content.image != null)
                GUILayout.Label(content.image, options);
            else
                GUILayout.Label(string.Empty, options);
        }

        private void RefreshStatus()
        {
            _registryStatuses.Clear();
            foreach (var definition in Registries)
                _registryStatuses.Add(BuildRegistryStatus(definition));

            RefreshMenuCoverage();

            _lastRefreshTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void RefreshPreviewProviders()
        {
            _previewProvidersByMenuPath.Clear();
            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                if (!(windows[i] is IMoyvaHubPreviewProvider provider))
                    continue;
                if (string.IsNullOrWhiteSpace(provider.HubToolMenuPath))
                    continue;
                _previewProvidersByMenuPath[provider.HubToolMenuPath] = provider;
            }
        }

        private void OnProjectContextAssetChanged(string typeName, UnityEngine.Object asset)
        {
            RefreshStatus();
            Repaint();
        }

        private void RefreshMenuCoverage()
        {
            _missingHubMenuPaths.Clear();

            string[] submenus;
            try
            {
                submenus = Unsupported.GetSubmenus("Moyva/Tools") ?? Array.Empty<string>();
            }
            catch
            {
                submenus = Array.Empty<string>();
            }

            var available = new HashSet<string>(submenus, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Tools.Length; i++)
            {
                string path = Tools[i].MenuPath;
                if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("Moyva/Tools/", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (available.Contains(path))
                    continue;
                _missingHubMenuPaths.Add(path);
            }
        }

        private RegistryStatus BuildRegistryStatus(RegistryDefinition definition)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{definition.TypeName}");
            UnityEngine.Object firstAsset = null;
            string firstPath = string.Empty;
            int entries = 0;

            Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset == null)
                    continue;

                if (firstAsset == null)
                {
                    firstAsset = asset;
                    firstPath = path;
                }

                entries += CountSerializedEntries(asset, definition.CountProperties);
            }

            return new RegistryStatus(definition, guids.Length, entries, firstAsset, firstPath, definition.CountProperties.Length > 0);
        }

        private static int CountSerializedEntries(UnityEngine.Object asset, IReadOnlyList<string> propertyNames)
        {
            if (asset == null || propertyNames == null || propertyNames.Count == 0)
                return 0;

            int count = 0;
            var serializedObject = new SerializedObject(asset);
            for (int i = 0; i < propertyNames.Count; i++)
            {
                var property = serializedObject.FindProperty(propertyNames[i]);
                if (property != null && property.isArray && property.propertyType != SerializedPropertyType.String)
                    count += property.arraySize;
            }

            return count;
        }

        private List<Recommendation> BuildRecommendations()
        {
            var result = new List<Recommendation>();
            foreach (var status in _registryStatuses)
            {
                if (status.AssetCount == 0)
                {
                    result.Add(new Recommendation("Missing", $"Не знайдено {status.Definition.Label}. Відкрий {status.Definition.ToolLabel}, щоб створити або призначити asset.", status.Definition.ToolMenuPath, status.Definition.ToolLabel, Bad));
                    continue;
                }

                if (status.HasCountableEntries && status.EntryCount == 0)
                    result.Add(new Recommendation("Empty", $"{status.Definition.Label} існує, але не має записів. Варто додати базові дані.", status.Definition.ToolMenuPath, status.Definition.ToolLabel, Warn));
            }

            if (EditorBuildSettings.scenes.All(s => !s.enabled))
                result.Add(new Recommendation("Build", "У Build Settings немає enabled scenes. Перевір список сцен перед Android/desktop build.", "File/Build Settings...", "Build Settings", Warn));

            return result;
        }

        private bool PassesToolFilters(ToolDefinition tool)
        {
            if (_selectedToolCategory != AllCategories && tool.Category != _selectedToolCategory)
                return false;

            return string.IsNullOrWhiteSpace(_globalSearch) || tool.Matches(_globalSearch);
        }

        private bool PassesMonitorFilter(RegistryStatus status)
        {
            if (string.IsNullOrWhiteSpace(_monitorFilter))
                return true;

            string query = _monitorFilter.Trim();
            return Contains(status.Definition.Label, query)
                || Contains(status.Definition.Area, query)
                || Contains(status.Definition.TypeName, query)
                || Contains(status.FirstAssetPath, query)
                || Contains(ResolveStatusLabel(status), query);
        }

        private void RefreshAssetSearch(string query)
        {
            _assetSearchResults.Clear();
            foreach (var asset in CollectKnownAssets(query))
            {
                if (asset != null && !_assetSearchResults.Contains(asset))
                    _assetSearchResults.Add(asset);
            }
        }

        private IEnumerable<UnityEngine.Object> CollectKnownAssets(string query)
        {
            query = query?.Trim() ?? string.Empty;
            foreach (var definition in Registries)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{definition.TypeName}");
                Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (asset == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(query)
                        || Contains(asset.name, query)
                        || Contains(path, query)
                        || Contains(definition.Label, query)
                        || Contains(definition.Area, query)
                        || Contains(definition.TypeName, query))
                    {
                        yield return asset;
                    }
                }
            }
        }

        private void OpenTool(ToolDefinition tool)
        {
            if (tool == null)
                return;

            ExecuteMenu(tool.MenuPath);
        }

        private void ExecuteMenu(string menuPath)
        {
            if (string.IsNullOrWhiteSpace(menuPath))
                return;

            if (EditorApplication.ExecuteMenuItem(menuPath))
                return;

            ShowNotification(new GUIContent($"Не вдалося відкрити: {menuPath}"));
            Debug.LogWarning($"[MoyvaDeveloperHub] Menu item not found or unavailable: {menuPath}");
        }

        private void SelectAsset(UnityEngine.Object asset)
        {
            _selectedAsset = asset;
            _rightPanelScroll = Vector2.zero;
            if (_selectedAssetEditor != null)
            {
                DestroyImmediate(_selectedAssetEditor);
                _selectedAssetEditor = null;
            }
        }

        private static void Ping(UnityEngine.Object asset)
        {
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static Color ResolveStatusColor(RegistryStatus status)
        {
            if (status.AssetCount == 0)
                return Bad;

            if (status.HasCountableEntries && status.EntryCount == 0)
                return Warn;

            return Good;
        }

        private static string ResolveStatusLabel(RegistryStatus status)
        {
            if (status.AssetCount == 0)
                return "Missing";

            if (status.HasCountableEntries && status.EntryCount == 0)
                return "Empty";

            return "OK";
        }

        private static bool Contains(string source, string query)
        {
            return !string.IsNullOrEmpty(source)
                && !string.IsNullOrEmpty(query)
                && source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static GUIStyle SidebarStyle()
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(8, 4, 8, 8),
            };
        }

        private static GUIStyle CardStyle()
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 10),
                margin = new RectOffset(6, 6, 6, 6),
            };
        }

        private static GUIStyle HeaderTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 22,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.1f, 0.12f, 0.14f) },
            };
        }

        private static GUIStyle HeaderSubtitleStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 11,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.72f, 0.78f, 0.8f) : new Color(0.22f, 0.27f, 0.3f) },
            };
        }

        private static GUIStyle SearchStyle()
        {
            return new GUIStyle(EditorStyles.textField)
            {
                fontSize = 13,
                fixedHeight = 30f,
                padding = new RectOffset(10, 10, 6, 5),
            };
        }

        private static GUIStyle PlaceholderStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.55f, 0.6f, 0.63f, 0.7f) },
            };
        }

        private static GUIStyle PrimaryButtonStyle()
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black },
            };
        }

        private static GUIStyle SecondaryButtonStyle()
        {
            return new GUIStyle(GUI.skin.button)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.84f, 0.9f, 0.92f) : new Color(0.12f, 0.16f, 0.18f) },
            };
        }

        private static GUIStyle SidebarTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
            };
        }

        private static GUIStyle SidebarMiniStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.66f, 0.68f) : new Color(0.38f, 0.42f, 0.44f) },
            };
        }

        private static GUIStyle SidebarActiveStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle SidebarActiveMiniStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.78f) },
            };
        }

        private static GUIStyle SectionTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                normal = { textColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.12f, 0.15f, 0.18f) },
            };
        }

        private static GUIStyle ToolTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                clipping = TextClipping.Ellipsis,
            };
        }

        private static GUIStyle WrapMiniLabelStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.74f, 0.78f, 0.8f) : new Color(0.28f, 0.31f, 0.33f) },
            };
        }

        private static GUIStyle WrapLabelStyle()
        {
            return new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
            };
        }

        private static GUIStyle MenuPathStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.68f, 0.74f) : new Color(0.18f, 0.42f, 0.48f) },
            };
        }

        private static GUIStyle CardHeaderStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle CardHeaderMiniStyle()
        {
            return new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.82f) },
            };
        }

        private static GUIStyle MetricValueStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 26,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private static GUIStyle MiniStatValueStyle(Color color)
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = color },
            };
        }

        private static GUIStyle CenterMiniStyle()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
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

        private static GUIStyle PillStyle()
        {
            return new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25f,
            };
        }

        private static GUIStyle ActivePillStyle()
        {
            return new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25f,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };
        }

        private sealed class ToolDefinition
        {
            public readonly string Title;
            public readonly string Category;
            public readonly string MenuPath;
            public readonly string Description;
            public readonly string IconName;
            public readonly bool Primary;
            private readonly string[] _keywords;

            public ToolDefinition(string title, string category, string menuPath, string description, string iconName, bool primary, params string[] keywords)
            {
                Title = title;
                Category = category;
                MenuPath = menuPath;
                Description = description;
                IconName = iconName;
                Primary = primary;
                _keywords = keywords ?? Array.Empty<string>();
            }

            public bool Matches(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return true;

                return Contains(Title, query)
                    || Contains(Category, query)
                    || Contains(MenuPath, query)
                    || Contains(Description, query)
                    || _keywords.Any(keyword => Contains(keyword, query));
            }

            public int Score(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return 0;

                int score = 0;
                if (Contains(Title, query)) score += 6;
                if (Contains(Category, query)) score += 3;
                if (Contains(MenuPath, query)) score += 2;
                if (Contains(Description, query)) score += 2;
                if (_keywords.Any(keyword => Contains(keyword, query))) score += 4;
                return score;
            }
        }

        private readonly struct TourStep
        {
            public readonly string Title;
            public readonly string Description;
            public readonly Section TargetSection;

            public TourStep(string title, string description, Section targetSection)
            {
                Title = title;
                Description = description;
                TargetSection = targetSection;
            }
        }

        private sealed class RegistryDefinition
        {
            public readonly string Label;
            public readonly string TypeName;
            public readonly string Area;
            public readonly string ToolLabel;
            public readonly string ToolMenuPath;
            public readonly string[] CountProperties;

            public RegistryDefinition(string label, string typeName, string area, string toolLabel, string toolMenuPath, params string[] countProperties)
            {
                Label = label;
                TypeName = typeName;
                Area = area;
                ToolLabel = toolLabel;
                ToolMenuPath = toolMenuPath;
                CountProperties = countProperties ?? Array.Empty<string>();
            }
        }
        private sealed class RegistryTabShortcut
        {
            public readonly string Title;
            public readonly string Area;
            public readonly int TabIndex;
            public readonly string Description;
            private readonly string[] _keywords;

            public RegistryTabShortcut(string title, string area, int tabIndex, string description, params string[] keywords)
            {
                Title = title;
                Area = area;
                TabIndex = tabIndex;
                Description = description;
                _keywords = keywords ?? Array.Empty<string>();
            }

            public bool Matches(string query)
            {
                if (string.IsNullOrWhiteSpace(query))
                    return true;

                return Contains(Title, query)
                    || Contains(Area, query)
                    || Contains(Description, query)
                    || _keywords.Any(keyword => Contains(keyword, query));
            }
        }

        private readonly struct RegistryStatus
        {
            public readonly RegistryDefinition Definition;
            public readonly int AssetCount;
            public readonly int EntryCount;
            public readonly UnityEngine.Object FirstAsset;
            public readonly string FirstAssetPath;
            public readonly bool HasCountableEntries;

            public RegistryStatus(RegistryDefinition definition, int assetCount, int entryCount, UnityEngine.Object firstAsset, string firstAssetPath, bool hasCountableEntries)
            {
                Definition = definition;
                AssetCount = assetCount;
                EntryCount = entryCount;
                FirstAsset = firstAsset;
                FirstAssetPath = firstAssetPath;
                HasCountableEntries = hasCountableEntries;
            }
        }

        private readonly struct Recommendation
        {
            public readonly string Severity;
            public readonly string Text;
            public readonly string MenuPath;
            public readonly string ActionLabel;
            public readonly Color Color;

            public Recommendation(string severity, string text, string menuPath, string actionLabel, Color color)
            {
                Severity = severity;
                Text = text;
                MenuPath = menuPath;
                ActionLabel = actionLabel;
                Color = color;
            }
        }
    }
}
