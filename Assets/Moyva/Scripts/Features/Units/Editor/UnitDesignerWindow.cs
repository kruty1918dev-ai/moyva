using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    public sealed partial class UnitDesignerWindow : EditorWindow
    {
        private const string UnitPrefabFolder = "Assets/Moyva/Prefabs/Units";
        private const string RegistryGuidPrefsKey = "Moyva.UnitDesigner.RegistryGuid";
        private const string SelectedTypePrefsKey = "Moyva.UnitDesigner.SelectedTypeId";
        private const string RegistryLockKey = "UnitRegistrySO";
        private const string UnitListWidthPrefsKey = "Moyva.UnitDesigner.Layout.UnitListWidth";
        private const string UnitDetailsWidthPrefsKey = "Moyva.UnitDesigner.Layout.UnitDetailsWidth";
        private const string GeneratorListWidthPrefsKey = "Moyva.UnitDesigner.Layout.GeneratorListWidth";
        private const string GeneratorSettingsWidthPrefsKey = "Moyva.UnitDesigner.Layout.GeneratorSettingsWidth";
        private const string CombatListWidthPrefsKey = "Moyva.UnitDesigner.Layout.CombatListWidth";
        private const string CombatRulesWidthPrefsKey = "Moyva.UnitDesigner.Layout.CombatRulesWidth";
        private const float SplitterWidth = 6f;
        private const float MinUnitListPanelWidth = 240f;
        private const float MinDetailsPanelWidth = 300f;
        private const float MinPreviewPanelWidth = 280f;
        private const float DefaultUnitListPanelWidth = 300f;
        private const float DefaultUnitDetailsPanelWidth = 440f;
        private const float DefaultGeneratorSettingsPanelWidth = 440f;
        private const float DefaultCombatRulesPanelWidth = 460f;

        // Кольори для UI візуалізації
        private static readonly Color Accent = new Color(0.18f, 0.62f, 0.67f);
        private static readonly Color Good = new Color(0.1f, 0.72f, 0.42f);
        private static readonly Color Warn = new Color(0.94f, 0.6f, 0.12f);
        private static readonly Color Bad = new Color(0.9f, 0.24f, 0.25f);
        private static readonly Color GridLine = new Color(1f, 1f, 1f, 0.08f);
        private static readonly Color VisionFill = new Color(0.1f, 0.72f, 0.86f, 0.22f);
        private static readonly Color VisionOutline = new Color(0.1f, 0.72f, 0.86f, 0.7f);

        // Основні дані реєстру
        private UnitRegistrySO _registry;
        private SerializedObject _registryObject;
        private SerializedProperty _configs;

        // Scroll-позиції для панелей
        private Vector2 _listScroll;
        private Vector2 _detailsScroll;
        private Vector2 _previewScroll;
        private float _unitListPanelWidth = DefaultUnitListPanelWidth;
        private float _unitDetailsPanelWidth = DefaultUnitDetailsPanelWidth;
        private float _generatorListPanelWidth = DefaultUnitListPanelWidth;
        private float _generatorSettingsPanelWidth = DefaultGeneratorSettingsPanelWidth;
        private float _combatListPanelWidth = DefaultUnitListPanelWidth;
        private float _combatRulesPanelWidth = DefaultCombatRulesPanelWidth;

        // Вибір та фільтрування
        private int _selectedIndex = -1;
        private string _search = string.Empty;
        private UnitRole? _roleFilter;
        private bool _onlyProblems;

        // Налаштування прев'ю
        private bool _autoPlayPreview = true;
        private bool _showVisionPreview = true;
        private bool _showStatsPreview = true;
        private bool _showAnimationPreview = true;
        private bool _showDetailedStatePreview = true;

        // Макет та вкладки для вузького вікна
        private int _verticalLayoutTab;
        private bool _showBatchTools;

        // Пакетні операції
        private bool _batchTargetFiltered = true;
        private bool _batchApplyRole;
        private UnitRole _batchRole = UnitRole.Worker;
        private bool _batchApplyPrefab;
        private GameObject _batchPrefab;
        private bool _batchApplyAnimationDefaults;
        private float _batchAnimationDuration = 0.3f;
        private float _batchAnimationDelay = 0.05f;

        // Автоматичні анімації
        private string _autoAnimationSearchFolder = "Assets/Moyva/Art";
        private string _autoAnimationPrefix = string.Empty;
        private int _autoAnimationFps = 10;
        private bool _autoAnimationReplaceByType = true;

        // Симуляція та прев'ю
        private float _previewSimulationSpeed = 1f;
        private float _previewSimulationTime;
        private bool _pauseSimulation;
        private DesignerPresetLibrarySO _designerPresetLibrary;
        private int _selectedUnitPresetIndex;

        // Прев'ю анімацій
        private int _previewAnimationIndex = -1;
        private float _animationPlaybackTime = 0f;
        private bool _isPlayingAnimation = false;
        private double _lastAnimationFrameTime;
        private float _pendingPreviewDelta;
        private Sprite _currentPreviewSprite;
        private string _lastBlockedApplyReason = string.Empty;
        private bool _diffBeforeApplyEnabled = true;

        // Утиліти для оптимізації редактора
        private readonly EditorLivePreviewThrottle _livePreviewThrottle = new EditorLivePreviewThrottle(repaintFps: 30d, costlyTickHz: 30d);
        private readonly EditorAssetStaleTracker _staleTracker = new EditorAssetStaleTracker();
        private readonly EditorWindowPerformanceProfiler _perfProfiler = new EditorWindowPerformanceProfiler();

        // Фасади для структурованого редагування різних систем юніта
        private IUnitDesignerIdentityFacade _identityFacade;
        private IUnitDesignerPrefabFacade _prefabFacade;
        private IUnitDesignerAnimationFacade _animationFacade;
        private IUnitDesignerPreviewFacade _previewFacade;
        private IUnitDesignerCombatFacade _combatFacade;

        /// <summary>
        /// Відкриває вікно дизайнера юнітів. Викликається з меню Moyva/Tools/Unit Designer.
        /// </summary>
        [MenuItem("Moyva/Tools/Unit Designer %#u", priority = 32)]
        public static void Open()
        {
            var window = GetWindow<UnitDesignerWindow>("Unit Designer");
            window.minSize = new Vector2(480f, 300f);
            window.Show();
            window.Focus();
        }

        /// <summary>
        /// Відкриває вікно дизайнера з визначеним реєстром та опціональним виділенням юніта.
        /// </summary>
        /// <param name="registry">Реєстр юнітів для редагування</param>
        /// <param name="typeId">Опціональний TypeId юніта для автоматичного виділення</param>
        public static void Open(UnitRegistrySO registry, string typeId = null)
        {
            var window = GetWindow<UnitDesignerWindow>("Unit Designer");
            window.minSize = new Vector2(480f, 300f);
            window._registry = registry;
            window.RefreshSerializedObject();
            window.SelectByTypeId(typeId);
            window.SaveRegistryPreference();
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            LoadRegistryPreference();
            if (_registry == null)
                _registry = FindFirstRegistry();

            RefreshSerializedObject();
            SelectByTypeId(EditorPrefs.GetString(SelectedTypePrefsKey, string.Empty));
            if (_selectedIndex < 0)
                AutoSelectUnit();

            LoadLayoutPreferences();
            _staleTracker.Capture(_registry);

            InitializeSafeEditMode();
            InitializeFacades();
            _designerPresetLibrary ??= MoyvaProjectEditorContext.GetOrFindFirst<DesignerPresetLibrarySO>();

            EditorApplication.update += OnEditorUpdate;
            InitializeGeneratorMapDesigner();
            _combatFacade.Initialize();
        }

        /// <summary>
        /// Звільнення ресурсів: збереження налаштувань, видалення обробників подій, очищення фасадів.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            SaveRegistryPreference();
            SaveSelectedPreference();
            SaveLayoutPreferences();
            DisposeSafeEditMode();
            DisposeGeneratorMapDesigner();
            _combatFacade?.Dispose();
        }

        /// <summary>
        /// Перевірка актуальності даних при відновленні вікна з фокусу. Оновлює GUI якщо ассет змінено зовні.
        /// </summary>
        private void OnFocus()
        {
            RefreshSerializedObject();
            ClampSelection();
            RefreshGeneratorMapSerializedObjects();

            if (_staleTracker.IsStale(_registry))
                ShowNotification(new GUIContent("Дані застарілі: ассет змінено зовні. Оновіть/збережіть зміни."));
        }

        private void OnGUI()
        {
            _perfProfiler.BeginFrame();
            if (_registryObject != null)
                _registryObject.Update();

            _perfProfiler.BeginSection("GeneratorMap");
            UpdateGeneratorMapSerializedObjects();
            _perfProfiler.EndSection("GeneratorMap");

            _perfProfiler.BeginSection("Toolbar");
            DrawToolbar();
            _perfProfiler.EndSection("Toolbar");

            _perfProfiler.BeginSection("MainBody");
            DrawMainBody();
            _perfProfiler.EndSection("MainBody");

            _perfProfiler.BeginSection("SafeEdit");
            DrawSafeEditPanel();
            _perfProfiler.EndSection("SafeEdit");

            DrawStatusBar();

            if (TryCommitRegistryChanges("Auto UI Apply", blockOnCriticalValidation: false))
                SaveSelectedPreference();

            _perfProfiler.EndFrame();
        }

        /// <summary>
        /// Циклічне оновлення (60 FPS): обробка симуляції прев'ю, анімацій, пріоритизація дорогих операцій через throttle.
        /// </summary>
        private void OnEditorUpdate()
        {
            if (!_showAnimationPreview)
            {
                _pendingPreviewDelta = 0f;
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (_lastAnimationFrameTime <= 0)
                _lastAnimationFrameTime = now;

            float deltaTime = Mathf.Max(0f, (float)(now - _lastAnimationFrameTime));
            _lastAnimationFrameTime = now;

            bool previewActive = _autoPlayPreview || _isPlayingAnimation;
            if (!previewActive)
            {
                _pendingPreviewDelta = 0f;
                return;
            }

            float simulationScale = _pauseSimulation ? 0f : Mathf.Max(0.05f, _previewSimulationSpeed);
            float scaledDelta = deltaTime * simulationScale;
            _pendingPreviewDelta += scaledDelta;

            if (!_livePreviewThrottle.ShouldRunCostlyTick())
                return;

            float previewStepDelta = _pendingPreviewDelta;
            _pendingPreviewDelta = 0f;

            if (_autoPlayPreview)
                _previewSimulationTime += previewStepDelta;

            // Update animation playback
            if (_autoPlayPreview && _isPlayingAnimation && _previewAnimationIndex >= 0 && HasSelectedUnit())
            {
                var unit = SelectedUnitProperty();
                var animationClips = unit.FindPropertyRelative("AnimationClips");
                if (animationClips != null && _previewAnimationIndex < animationClips.arraySize)
                {
                    var clip = animationClips.GetArrayElementAtIndex(_previewAnimationIndex);
                    var durationRef = clip.FindPropertyRelative("Duration");
                    float duration = durationRef != null ? Mathf.Max(0.01f, durationRef.floatValue) : 1f;

                    _animationPlaybackTime += previewStepDelta;

                    if (_animationPlaybackTime > duration)
                    {
                        var loopRef = clip.FindPropertyRelative("Loop");
                        if (loopRef != null && loopRef.boolValue)
                            _animationPlaybackTime = 0f;
                        else
                            _isPlayingAnimation = false;
                    }
                }
            }

            // Keep the editor loop and repaint running even when the mouse is idle.
            EditorApplication.QueuePlayerLoopUpdate();
            _livePreviewThrottle.TryRepaint(this);
        }

        /// <summary>
        /// Малювання тулбара з фільтрами, кнопками керування реєстром та тумблерами для режимів прев'ю.
        /// Адаптивний макет для вузьких та широких вікон.
        /// </summary>
        private void DrawToolbar()
        {
            bool compactMode = position.width < 700f;

            if (compactMode)
                DrawCompactToolbar();
            else
                DrawFullToolbar();
        }

        private void DrawFullToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var newRegistry = (UnitRegistrySO)EditorGUILayout.ObjectField(
                EditorTooltipStandard.Content("Registry", "Обирає UnitRegistrySO для редагування у вікні.", "Визначає набір класів юнітів, що буде використано в грі."),
                _registry,
                typeof(UnitRegistrySO),
                false,
                GUILayout.Width(Mathf.Clamp(position.width * 0.25f, 200f, 300f)));

            if (newRegistry != _registry)
            {
                _registry = newRegistry;
                MoyvaProjectEditorContext.Set(_registry);
                RefreshSerializedObject();
                AutoSelectUnit();
                SaveRegistryPreference();
                _staleTracker.Capture(_registry);
            }

            if (GUILayout.Button(IconContent("d_Search Icon", "Auto", EditorTooltipStandard.Build("Автоматично шукає UnitRegistrySO у проєкті.", "Дозволяє швидко підключити коректний реєстр юнітів.")), EditorStyles.toolbarButton, GUILayout.Width(56f)))
            {
                _registry = FindFirstRegistry();
                MoyvaProjectEditorContext.Set(_registry);
                RefreshSerializedObject();
                AutoSelectUnit();
                SaveRegistryPreference();
                _staleTracker.Capture(_registry);
            }

            EditorGUI.BeginDisabledGroup(_registry == null);
            if (GUILayout.Button(IconContent("d_Project", "Ping", EditorTooltipStandard.Build("Показує активний реєстр у Project.", "Спрощує перевірку й ручне редагування ассета.")), EditorStyles.toolbarButton, GUILayout.Width(56f)))
            {
                EditorGUIUtility.PingObject(_registry);
                Selection.activeObject = _registry;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(IconContent("", "Clear", EditorTooltipStandard.Build("Очищає активний registry у цьому вікні.", "Дозволяє швидко переключитися на інший реєстр.")), EditorStyles.toolbarButton, GUILayout.Width(56f)))
            {
                _registry = null;
                MoyvaProjectEditorContext.Set<UnitRegistrySO>(null);
                RefreshSerializedObject();
                SaveRegistryPreference();
            }

            DrawWorkspaceModeToolbar();

            GUILayout.FlexibleSpace();

            _autoPlayPreview = GUILayout.Toggle(_autoPlayPreview, IconContent("d_PlayButton", "Play", EditorTooltipStandard.Build("Автоматично програє preview руху/анімації.", "Полегшує візуальну перевірку читабельності анімацій.")), EditorStyles.toolbarButton, GUILayout.Width(56f));
            _showVisionPreview = GUILayout.Toggle(_showVisionPreview, IconContent("d_scenevis_visible_hover", "Vision", EditorTooltipStandard.Build("Вмикає індикатор дальності огляду.", "Допомагає балансувати бачення юнітів у Fog of War.")), EditorStyles.toolbarButton, GUILayout.Width(56f));
            _showStatsPreview = GUILayout.Toggle(_showStatsPreview, IconContent("d_Profiler.GlobalIllumination", "Stats", EditorTooltipStandard.Build("Відображає ключові метрики у preview.", "Допомагає швидше оцінити баланс параметрів юніта.")), EditorStyles.toolbarButton, GUILayout.Width(56f));
            _diffBeforeApplyEnabled = GUILayout.Toggle(_diffBeforeApplyEnabled, IconContent("d_FilterByLabel", "Diff", EditorTooltipStandard.Build("Показує diff перед explicit Apply-операціями.", "Зменшує ризик випадкових змін даних юнітів.")), EditorStyles.toolbarButton, GUILayout.Width(56f));
            if (GUILayout.Button(IconContent("d_RectTransform Icon", "Layout", EditorTooltipStandard.Build("Скидає ручні ширини панелей Unit Designer.", "Корисно, якщо одна з вкладок стала занадто вузькою або широкою.")), EditorStyles.toolbarButton, GUILayout.Width(64f)))
                ResetLayoutPreferences();
            bool unlocked = EditorRegistryWriteLock.IsUnlocked(RegistryLockKey);
            bool nextUnlocked = GUILayout.Toggle(unlocked, IconContent("", "Unlock", EditorTooltipStandard.Build("Дозволяє редагування бойового реєстру лише після explicit unlock.", "Захищає від випадкових змін у production-даних.")), EditorStyles.toolbarButton, GUILayout.Width(64f));
            if (nextUnlocked != unlocked)
                EditorRegistryWriteLock.SetUnlocked(RegistryLockKey, nextUnlocked);
            _safeEditMode = GUILayout.Toggle(_safeEditMode, IconContent("", "Safe", EditorTooltipStandard.Build("Вмикає Safe Edit Mode для масових змін.", "Знижує ризик поломок балансу від batch-операцій.")), EditorStyles.toolbarButton, GUILayout.Width(56f));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawCompactToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var newRegistry = (UnitRegistrySO)EditorGUILayout.ObjectField(
                _registry,
                typeof(UnitRegistrySO),
                false,
                GUILayout.Width(Mathf.Clamp(position.width * 0.4f, 140f, 200f)));

            if (newRegistry != _registry)
            {
                _registry = newRegistry;
                MoyvaProjectEditorContext.Set(_registry);
                RefreshSerializedObject();
                AutoSelectUnit();
                SaveRegistryPreference();
                _staleTracker.Capture(_registry);
            }

            if (GUILayout.Button(IconContent("d_Search Icon", "Auto", ""), EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
                _registry = FindFirstRegistry();
                MoyvaProjectEditorContext.Set(_registry);
                RefreshSerializedObject();
                AutoSelectUnit();
                SaveRegistryPreference();
                _staleTracker.Capture(_registry);
            }

            EditorGUI.BeginDisabledGroup(_registry == null);
            if (GUILayout.Button(IconContent("d_Project", "Ping", ""), EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
                EditorGUIUtility.PingObject(_registry);
                Selection.activeObject = _registry;
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(IconContent("", "Clr", ""), EditorStyles.toolbarButton, GUILayout.Width(42f)))
            {
                _registry = null;
                MoyvaProjectEditorContext.Set<UnitRegistrySO>(null);
                RefreshSerializedObject();
                SaveRegistryPreference();
            }

            GUILayout.FlexibleSpace();

            _autoPlayPreview = GUILayout.Toggle(_autoPlayPreview, IconContent("d_PlayButton", "Play", ""), EditorStyles.toolbarButton, GUILayout.Width(42f));
            _showVisionPreview = GUILayout.Toggle(_showVisionPreview, IconContent("d_scenevis_visible_hover", "Vision", ""), EditorStyles.toolbarButton, GUILayout.Width(42f));
            _showStatsPreview = GUILayout.Toggle(_showStatsPreview, IconContent("d_Profiler.GlobalIllumination", "Stats", ""), EditorStyles.toolbarButton, GUILayout.Width(42f));
            if (GUILayout.Button(IconContent("d_RectTransform Icon", "Lay", ""), EditorStyles.toolbarButton, GUILayout.Width(42f)))
                ResetLayoutPreferences();
            bool unlocked = EditorRegistryWriteLock.IsUnlocked(RegistryLockKey);
            bool nextUnlocked = GUILayout.Toggle(unlocked, IconContent("", "Unl", ""), EditorStyles.toolbarButton, GUILayout.Width(42f));
            if (nextUnlocked != unlocked)
                EditorRegistryWriteLock.SetUnlocked(RegistryLockKey, nextUnlocked);
            _safeEditMode = GUILayout.Toggle(_safeEditMode, IconContent("", "Safe", ""), EditorStyles.toolbarButton, GUILayout.Width(42f));

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Малювання основного вмісту вікна: список юнітів, налаштування, прев'ю або спеціалізовані робочі простори.
        /// Обирає макет на основі ширини вікна та активного режиму.
        /// </summary>
        private void DrawMainBody()
        {
            if (_registry == null || _registryObject == null || _configs == null)
            {
                DrawEmptyState();
                return;
            }

            if (IsGeneratorMapWorkspaceActive())
            {
                DrawGeneratorMapWorkspace();
                return;
            }

            if (_combatFacade.IsWorkspaceActive())
            {
                _combatFacade.DrawWorkspace();
                return;
            }

            // Adaptive layout: horizontal for wide windows, vertical for narrow
            bool useVerticalLayout = position.width < 700f;

            if (useVerticalLayout)
                DrawVerticalLayout();
            else
                DrawHorizontalLayout();
        }

        /// <summary>
        /// Трирівневий горизонтальний макет: список (25%), налаштування (35%), прев'ю (35%).
        /// </summary>
        /// <summary>
        /// Трирівневий горизонтальний макет: список (25%), налаштування (35%), прев'ю (35%).
        /// </summary>
        private void DrawHorizontalLayout()
        {
            ClampLayoutWidths();

            EditorGUILayout.BeginHorizontal();
            DrawUnitListPanel(GUILayout.Width(_unitListPanelWidth), GUILayout.ExpandHeight(true));
            DrawColumnSplitter(ref _unitListPanelWidth, MinUnitListPanelWidth, ResolveMaxPanelWidth(MinDetailsPanelWidth + MinPreviewPanelWidth), UnitListWidthPrefsKey);
            DrawDetailsPanel(GUILayout.Width(_unitDetailsPanelWidth), GUILayout.MinWidth(MinDetailsPanelWidth), GUILayout.ExpandHeight(true));
            DrawColumnSplitter(ref _unitDetailsPanelWidth, MinDetailsPanelWidth, ResolveMaxPanelWidth(MinUnitListPanelWidth + MinPreviewPanelWidth), UnitDetailsWidthPrefsKey);
            DrawPreviewPanel(GUILayout.MinWidth(MinPreviewPanelWidth), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Вертикальний макет для вузьких вікон із вкладками: Юніти → Налаштування → Preview.
        /// </summary>
        private void DrawVerticalLayout()
        {
            EditorGUILayout.BeginVertical();

            // Tabbed view for narrow windows
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            _verticalLayoutTab = GUILayout.Toolbar(_verticalLayoutTab, new[] { "Юніти", "Налаштування", "Preview" }, EditorStyles.toolbarButton,
                GUILayout.Width(250f));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_verticalLayoutTab == 0)
                DrawUnitListPanel(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            else if (_verticalLayoutTab == 1)
                DrawDetailsPanel(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            else
                DrawPreviewPanel(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            EditorGUILayout.EndVertical();
        }

        private void DrawEmptyState()
        {
            float padding = Mathf.Clamp(position.width * 0.08f, 20f, 60f);
            EditorGUILayout.Space(Mathf.Max(16f, position.height * 0.1f));
            Rect rect = GUILayoutUtility.GetRect(0f, Mathf.Max(200f, position.height * 0.7f), GUILayout.ExpandWidth(true));
            rect = new Rect(rect.x + padding, rect.y, rect.width - padding * 2f, rect.height);
            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.18f, 0.2f, 0.22f) : new Color(0.88f, 0.9f, 0.92f));

            GUILayout.BeginArea(rect);
            GUILayout.FlexibleSpace();
            GUILayout.Label(IconContent("d_AvatarSelector", "Unit Designer", ""), CenterHeaderStyle());
            GUILayout.Label("UnitRegistrySO не знайдено", CenterTitleStyle());
            GUILayout.Label("Оберіть або створіть asset для налаштування юнітів.", CenterMiniStyle());
            GUILayout.Space(12f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Знайти", "Пошук UnitRegistrySO у Assets."), GUILayout.Width(120f), GUILayout.Height(24f)))
            {
                _registry = FindFirstRegistry();
                RefreshSerializedObject();
                AutoSelectUnit();
            }
            if (GUILayout.Button(new GUIContent("Створити", "Створити новий UnitRegistry.asset."), GUILayout.Width(120f), GUILayout.Height(24f)))
                CreateRegistryAsset();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Малювання списку юнітів з фільтруванням по ролі, пошуком, статусом валідації.
        /// Підтримує операції додавання, дублювання та пакетних змін.
        /// </summary>
        /// <summary>
        /// Малювання списку юнітів з фільтруванням по ролі, пошуком, статусом валідації.
        /// Підтримує операції додавання, дублювання та пакетних змін.
        /// </summary>
        private void DrawUnitListPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Юніти", IconContent("d_AvatarSelector", "", "Список класів юнітів у registry."));

            EditorGUILayout.BeginHorizontal();
            _search = GUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField);
            if (GUILayout.Button(IconContent("d_TreeEditor.Trash", "", "Очистити пошук."), EditorStyles.toolbarButton, GUILayout.Width(26f)))
                _search = string.Empty;
            EditorGUILayout.EndHorizontal();

            // Adaptive filter layout
            bool compactFilters = position.width < 550f;
            if (compactFilters)
            {
                DrawRoleFilterButton(null, "Всі");
                EditorGUILayout.BeginHorizontal();
                DrawRoleFilterButton(UnitRole.Worker, "Worker");
                DrawRoleFilterButton(UnitRole.Military, "Military");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                DrawRoleFilterButton(null, "Всі");
                DrawRoleFilterButton(UnitRole.Worker, "Worker");
                DrawRoleFilterButton(UnitRole.Military, "Military");
                EditorGUILayout.EndHorizontal();
            }

            _onlyProblems = EditorGUILayout.ToggleLeft(new GUIContent("Показати тільки проблемні", "Фільтрує записи без ID, з '_' у ID, дублікати або без prefab."), _onlyProblems);

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Новий", "Додати нового юніта з унікальним TypeId."), GUILayout.Height(24f)))
                AddNewUnit();
            EditorGUI.BeginDisabledGroup(!HasSelectedUnit());
            if (GUILayout.Button(new GUIContent("Дубль", "Скопіювати вибраного юніта з новим TypeId."), GUILayout.Height(24f)))
                DuplicateSelectedUnit();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            DrawBatchOperationsPanel();

            DrawListStats();

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            int visibleCount = 0;
            for (int i = 0; i < _configs.arraySize; i++)
            {
                var item = _configs.GetArrayElementAtIndex(i);
                if (!PassesFilters(item, i))
                    continue;

                visibleCount++;
                DrawUnitRow(item, i);
            }

            if (visibleCount == 0)
                EditorGUILayout.HelpBox("Немає юнітів за поточним фільтром.", MessageType.Info);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Малювання панелі налаштування: ідентичність, префаб, компоненти, характеристики, анімація, бойові параметри.
        /// </summary>
        /// <summary>
        /// Малювання панелі налаштування: ідентичність, префаб, компоненти, характеристики, анімація, бойові параметри.
        /// </summary>
        private void DrawDetailsPanel(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Налаштування", IconContent("d_Settings", "", "Редагування вибраного UnitClassConfig."));

            if (!HasSelectedUnit())
            {
                EditorGUILayout.HelpBox("Оберіть юніта зі списку або створіть нового.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var unit = SelectedUnitProperty();
            string typeId = GetString(unit, "TypeId");
            DrawValidationSummary(unit, _selectedIndex);

            _detailsScroll = EditorGUILayout.BeginScrollView(_detailsScroll);
            DrawQuickUnitParametersSection(unit);
            DrawUnitPresetSection();
            _identityFacade.DrawSection(unit);
            _prefabFacade.DrawSection(unit, typeId);
            _prefabFacade.DrawComponentsSection(unit);
            DrawStatsSection(unit);
            _combatFacade.DrawCompactSection(unit);
            _animationFacade.DrawSection(unit);
            _identityFacade.DrawDangerSection(unit);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Малювання панелі прев'ю: JPEG спрайт, симуляція руху, дальність огляду, статистика, детальний стан юніта.
        /// </summary>
        /// <summary>
        /// Малювання панелі прев'ю: JPEG спрайт, симуляція руху, дальність огляду, статистика, детальний стан юніта.
        /// </summary>
        private void DrawPreviewPanel(params GUILayoutOption[] options)
        {
            _previewFacade.DrawPanel(options);
        }

        private void DrawPreviewPanelInternal(params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(PanelStyle(), options);
            DrawPanelHeader("Preview", IconContent("d_PreMatCube", "", "Візуальне preview prefab, огляду, стаміни і руху."));

            if (!HasSelectedUnit())
            {
                EditorGUILayout.HelpBox("Preview з'явиться після вибору юніта.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var unit = SelectedUnitProperty();
            var prefab = GetObject<GameObject>(unit, "Prefab");
            var sprite = ResolveSprite(prefab);

            _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Швидкість симуляції", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _previewSimulationSpeed = EditorGUILayout.Slider(new GUIContent("Множник", "Керує швидкістю preview для руху та анімацій."), _previewSimulationSpeed, 0.05f, 4f);
            if (GUILayout.Button(_pauseSimulation ? "▶" : "⏸", GUILayout.Width(32f), GUILayout.Height(18f)))
                _pauseSimulation = !_pauseSimulation;
            if (GUILayout.Button("Reset", GUILayout.Width(54f), GUILayout.Height(18f)))
            {
                _previewSimulationTime = 0f;
                _animationPlaybackTime = 0f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("0.25x", GUILayout.Width(50f), GUILayout.Height(18f))) _previewSimulationSpeed = 0.25f;
            if (GUILayout.Button("0.5x", GUILayout.Width(46f), GUILayout.Height(18f))) _previewSimulationSpeed = 0.5f;
            if (GUILayout.Button("1x", GUILayout.Width(34f), GUILayout.Height(18f))) _previewSimulationSpeed = 1f;
            if (GUILayout.Button("2x", GUILayout.Width(34f), GUILayout.Height(18f))) _previewSimulationSpeed = 2f;
            if (GUILayout.Button("4x", GUILayout.Width(34f), GUILayout.Height(18f))) _previewSimulationSpeed = 4f;
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(_pauseSimulation ? "Пауза" : $"t: {_previewSimulationTime:0.00}s", EditorStyles.miniLabel, GUILayout.Width(90f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Adaptive preview size based on window width
            float previewMinHeight = Mathf.Clamp(position.width * 0.4f, 180f, 320f);
            Rect previewRect = GUILayoutUtility.GetRect(Mathf.Max(180f, position.width * 0.3f), previewMinHeight, GUILayout.ExpandWidth(true));
            DrawAnimatedPreview(previewRect, unit, prefab, sprite);

            EditorGUILayout.Space(6f);
            if (_showStatsPreview)
                DrawStatVisualization(unit);

            if (_showVisionPreview)
                DrawVisionGrid(unit);

            _showDetailedStatePreview = EditorGUILayout.ToggleLeft(
                new GUIContent("Детальний стан юніта", "Показує всі ключові параметри у симуляції прев'ю."),
                _showDetailedStatePreview);

            if (_showDetailedStatePreview)
                DrawDetailedStatePreview(unit);

            DrawQuickHints(unit, prefab, sprite);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Ініціалізація фасадів для модульного редагування: Identity, Prefab, Animation, Preview, Combat.
        /// </summary>
        /// <summary>
        /// Ініціалізація фасадів для модульного редагування: Identity, Prefab, Animation, Preview, Combat.
        /// </summary>
        private void InitializeFacades()
        {
            _identityFacade = new UnitDesignerIdentityFacade(DrawIdentitySection, DrawDangerSection);
            _prefabFacade = new UnitDesignerPrefabFacade(DrawPrefabSection, DrawPrefabComponentsSection);
            _animationFacade = new UnitDesignerAnimationFacade(DrawAnimationSection);
            _previewFacade = new UnitDesignerPreviewFacade(DrawPreviewPanelInternal);
            _combatFacade = new UnitDesignerCombatFacade(
                InitializeCombatDesigner,
                DisposeCombatDesigner,
                IsCombatWorkspaceActive,
                OnCombatWorkspaceSelected,
                DrawCombatWorkspace,
                DrawCombatCompactSection);
        }

        /// <summary>
        /// Малювання одного рядка у списку юнітів: спрайт, TypeId, роль, характеристики, статус валідації.
        /// Адаптивний розмір для компактного та розширеного режимів.
        /// </summary>
        /// <summary>
        /// Малювання одного рядка у списку юнітів: спрайт, TypeId, роль, характеристики, статус валідації.
        /// Адаптивний розмір для компактного та розширеного режимів.
        /// </summary>
        private void DrawUnitRow(SerializedProperty unit, int index)
        {
            string typeId = GetString(unit, "TypeId");
            UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);
            var prefab = GetObject<GameObject>(unit, "Prefab");
            var sprite = ResolveSprite(prefab);
            bool selected = index == _selectedIndex;
            string validation = ValidateUnit(unit, index);

            // Adaptive row height and layout
            bool compactMode = position.width < 400f;
            float rowHeight = compactMode ? 48f : 62f;

            Rect row = GUILayoutUtility.GetRect(0f, rowHeight, GUILayout.ExpandWidth(true));
            Color bg = selected
                ? new Color(Accent.r, Accent.g, Accent.b, EditorGUIUtility.isProSkin ? 0.42f : 0.25f)
                : (EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.86f, 0.86f, 0.86f));

            DrawPanelBackground(row, bg);

            float iconSize = compactMode ? 32f : 46f;
            Rect iconRect = new Rect(row.x + 8f, row.y + (rowHeight - iconSize) * 0.5f, iconSize, iconSize);
            DrawSpriteOrPrefab(iconRect, sprite, prefab, true);

            Rect titleRect = new Rect(iconRect.xMax + 8f, row.y + 7f, Mathf.Max(100f, row.width - iconSize - 140f), 18f);
            GUI.Label(titleRect, string.IsNullOrWhiteSpace(typeId) ? "<без TypeId>" : typeId, selected ? SelectedRowTitleStyle() : RowTitleStyle());

            if (!compactMode)
            {
                Rect roleRect = new Rect(titleRect.x, titleRect.yMax + 2f, 88f, 18f);
                DrawBadge(roleRect, role.ToString(), role == UnitRole.Worker ? Good : Warn);

                Rect statsRect = new Rect(roleRect.xMax + 8f, roleRect.y, Mathf.Max(120f, row.width - roleRect.xMax - 70f), 18f);
                GUI.Label(statsRect, $"HP {GetInt(unit, "HitPoints")}  |  DMG {GetCombatDamageTotal(unit)}", EditorStyles.miniLabel);
            }

            Rect statusRect = new Rect(row.xMax - 54f, row.y + (rowHeight - 18f) * 0.5f, 42f, 18f);
            DrawBadge(statusRect, validation == null ? "OK" : "!", validation == null ? Good : Bad);

            if (!compactMode)
            {
                Rect prefabRect = new Rect(titleRect.x, row.yMax - 20f, Mathf.Max(150f, row.width - 70f), 16f);
                GUI.Label(prefabRect, prefab != null ? prefab.name : "Prefab не задано", EditorStyles.miniLabel);
            }

            if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
            {
                _selectedIndex = index;
                SaveSelectedPreference();
                GUI.FocusControl(null);
                Event.current.Use();
            }
        }

        /// <summary>
        /// Малювання розділу ідентичності юніта: TypeId, роль, валідація.
        /// </summary>
        /// <summary>
        /// Малювання розділу ідентичності юніта: TypeId, роль, валідація.
        /// </summary>
        private void DrawIdentitySection(SerializedProperty unit)
        {
            BeginSection("Ідентичність", "d_FilterByType", "TypeId, роль і базова класифікація юніта.");

            var idProp = unit.FindPropertyRelative("TypeId");
            EditorGUILayout.PropertyField(idProp, new GUIContent("Код типу", "Унікальний ID класу юніта. Не використовуйте '_', бо цей символ зарезервований для instance ID."));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(unit.FindPropertyRelative("Role"), new GUIContent("Роль", "Worker для економічних юнітів, Military для бойових."));
            if (GUILayout.Button(new GUIContent("Авто", "Визначити роль."), GUILayout.Width(56f), GUILayout.Height(18f)))
                ApplyAutoRole(unit);
            EditorGUILayout.EndHorizontal();

            string validation = ValidateUnit(unit, _selectedIndex);
            if (validation != null)
                EditorGUILayout.HelpBox(validation, MessageType.Warning);

            EndSection();
        }

        private void DrawQuickUnitParametersSection(SerializedProperty unit)
        {
            BeginSection("Швидке налаштування", "d_UnityEditor.InspectorWindow", "Основні поля UnitClassConfig без переходу в Registry Hub.");

            var idProp = unit.FindPropertyRelative("TypeId");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(idProp, new GUIContent("TypeId", "Унікальний ID класу юніта. Це поле зберігається у UnitRegistrySO."));
            if (GUILayout.Button(new GUIContent("Унік.", "Згенерувати унікальний TypeId на основі поточного значення."), GUILayout.Width(58f), GUILayout.Height(18f)))
                idProp.stringValue = GenerateUniqueId(string.IsNullOrWhiteSpace(idProp.stringValue) ? "unit" : idProp.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(unit.FindPropertyRelative("Role"), new GUIContent("Роль", "Основна роль юніта у gameplay."));
            EditorGUILayout.PropertyField(unit.FindPropertyRelative("CombatType"), new GUIContent("Клас бою", "Піхота, кавалерія або облогова машина."));
            EditorGUILayout.EndHorizontal();

            DrawQuickIntegerRow(unit, "База", "HitPoints", "HP", 1, 300, "BaseLevel", "Рівень", 1, 10, "VisionRange", "Огляд", 1, 20);

            var staminaProp = unit.FindPropertyRelative("BaseStamina");
            var staminaRangeProp = unit.FindPropertyRelative("StaminaRandomRange");
            if (staminaProp != null)
                staminaProp.floatValue = EditorGUILayout.Slider(new GUIContent("Стаміна", "Середній запас стаміни юніта."), Mathf.Max(0f, staminaProp.floatValue), 0f, 300f);
            if (staminaRangeProp != null)
                staminaRangeProp.vector2Value = EditorGUILayout.Vector2Field(new GUIContent("Розкид стаміни", "Мінімальний/максимальний випадковий зсув до базової стаміни."), staminaRangeProp.vector2Value);

            DrawQuickIntegerRow(unit, "Сила атаки", "PenetratingDamage", "Колюча", 0, 300, "CuttingDamage", "Ріжуча", 0, 300, "CrushingDamage", "Дроб.", 0, 300);
            DrawQuickIntegerRow(unit, "Захист", "PenetratingDefense", "Колючий", 0, 300, "CuttingDefense", "Ріжучий", 0, 300, "CrushingDefense", "Дроб.", 0, 300);

            int attackPower = GetCombatDamageTotal(unit);
            int defensePower = Mathf.Max(0, GetInt(unit, "PenetratingDefense")) + Mathf.Max(0, GetInt(unit, "CuttingDefense")) + Mathf.Max(0, GetInt(unit, "CrushingDefense"));
            EditorGUILayout.LabelField($"Підсумок: сила {attackPower}, захист {defensePower}, HP {Mathf.Max(1, GetInt(unit, "HitPoints"))}", EditorStyles.miniBoldLabel);

            EndSection();
        }

        private static void DrawQuickIntegerRow(
            SerializedProperty unit,
            string label,
            string firstProperty,
            string firstLabel,
            int firstMin,
            int firstMax,
            string secondProperty,
            string secondLabel,
            int secondMin,
            int secondMax,
            string thirdProperty,
            string thirdLabel,
            int thirdMin,
            int thirdMax)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(86f));
            DrawQuickIntField(unit, firstProperty, firstLabel, firstMin, firstMax);
            DrawQuickIntField(unit, secondProperty, secondLabel, secondMin, secondMax);
            DrawQuickIntField(unit, thirdProperty, thirdLabel, thirdMin, thirdMax);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawQuickIntField(SerializedProperty unit, string propertyName, string label, int min, int max)
        {
            var property = unit.FindPropertyRelative(propertyName);
            if (property == null)
                return;

            int value = Mathf.Clamp(property.intValue, min, max);
            property.intValue = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(label), value, GUILayout.MinWidth(74f)), min, max);
        }

        /// <summary>
        /// Малювання розділу префабу: об'єкт, спрайт, кнопки для створення та редагування.
        /// </summary>
        /// <summary>
        /// Малювання розділу префабу: об'єкт, спрайт, кнопки для створення та редагування.
        /// </summary>
        private void DrawPrefabSection(SerializedProperty unit, string typeId)
        {
            BeginSection("Візуал", "d_Prefab Icon", "Prefab або sprite, з якого буде створено 2D prefab юніта.");

            var prefabProp = unit.FindPropertyRelative("Prefab");
            EditorGUILayout.PropertyField(prefabProp, new GUIContent("Префаб", "Префаб юніта. Бажано мати SpriteRenderer у корені або дочірніх об'єктах."));

            var prefab = prefabProp.objectReferenceValue as GameObject;
            var sprite = ResolveSprite(prefab);
            Rect strip = GUILayoutUtility.GetRect(0f, 48f, GUILayout.ExpandWidth(true));
            DrawPanelBackground(strip, EditorGUIUtility.isProSkin ? new Color(0.16f, 0.17f, 0.18f) : new Color(0.9f, 0.91f, 0.92f));
            DrawSpriteOrPrefab(new Rect(strip.x + 8f, strip.y + 4f, 40f, 40f), sprite, prefab, true);
            GUI.Label(new Rect(strip.x + 58f, strip.y + 6f, strip.width - 66f, 18f), prefab ? prefab.name : "Prefab не задано", RowTitleStyle());
            GUI.Label(new Rect(strip.x + 58f, strip.y + 24f, strip.width - 66f, 16f), sprite ? $"Sprite: {sprite.name}" : "SpriteRenderer не знайдено", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Ping", "Показати prefab у Project."), GUILayout.Height(20f)))
            {
                if (prefab != null)
                {
                    EditorGUIUtility.PingObject(prefab);
                    Selection.activeObject = prefab;
                }
            }

            if (GUILayout.Button(new GUIContent("Створити", "Створити prefab зі sprite."), GUILayout.Height(20f)))
                CreatePrefabForSelected(unit, typeId);
            EditorGUILayout.EndHorizontal();

            EndSection();
        }

        /// <summary>
        /// Малювання компонентів префабу: швидка редакція спрайту, SpriteRenderer, Animator параметри.
        /// </summary>
        /// <summary>
        /// Малювання компонентів префабу: швидка редакція спрайту, SpriteRenderer, Animator параметри.
        /// </summary>
        private void DrawPrefabComponentsSection(SerializedProperty unit)
        {
            BeginSection("Компоненти префабу", "d_Transform Icon", "Швидке редагування спрайтів та компонентів префабу.");

            var prefabProp = unit.FindPropertyRelative("Prefab");
            var customSpriteProp = unit.FindPropertyRelative("CustomSprite");
            var prefab = prefabProp.objectReferenceValue as GameObject;

            if (prefab == null)
            {
                EditorGUILayout.HelpBox("Оберіть префаб вище, щоб редагувати його компоненти.", MessageType.Info);
                EndSection();
                return;
            }

            // Custom sprite
            EditorGUI.BeginChangeCheck();
            var quickSprite = (Sprite)EditorGUILayout.ObjectField(
                new GUIContent("Спрайт (швидка редакція)", "Встановіть спрайт тут для швидкої зміни"),
                customSpriteProp.objectReferenceValue,
                typeof(Sprite),
                false);
            Rect quickSpriteRect = GUILayoutUtility.GetLastRect();
            if (SpriteImportDragDropPolicy.HandleDrop(quickSpriteRect, ref quickSprite, "UnitDesigner.CustomSprite"))
                Repaint();

            if (EditorGUI.EndChangeCheck())
            {
                if (SpriteImportDragDropPolicy.EnsureAllowedSprite(ref quickSprite, "UnitDesigner.CustomSprite"))
                    customSpriteProp.objectReferenceValue = quickSprite;
                else
                    customSpriteProp.objectReferenceValue = null;
            }

            // SpriteRenderer quick access
            var spriteRenderer = prefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SpriteRenderer у префабі", EditorStyles.boldLabel);
                if (GUILayout.Button(new GUIContent("Редагувати", "Редагувати SpriteRenderer прямо"), GUILayout.Width(80f), GUILayout.Height(18f)))
                {
                    EditorGUIUtility.PingObject(spriteRenderer);
                    Selection.activeObject = prefab;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Sprite", spriteRenderer.sprite, typeof(Sprite), false);
                EditorGUILayout.ColorField("Color", spriteRenderer.color);
                EditorGUI.EndDisabledGroup();

                // Quick button to replace sprite
                Sprite currentSprite = spriteRenderer.sprite;
                Sprite newSprite = (Sprite)EditorGUILayout.ObjectField("Замінити на", currentSprite, typeof(Sprite), false);
                Rect replaceRect = GUILayoutUtility.GetLastRect();
                if (SpriteImportDragDropPolicy.HandleDrop(replaceRect, ref newSprite, "UnitDesigner.PrefabSpriteRenderer"))
                    Repaint();

                SpriteImportDragDropPolicy.EnsureAllowedSprite(ref newSprite, "UnitDesigner.PrefabSpriteRenderer");
                if (newSprite != currentSprite)
                {
                    spriteRenderer.sprite = newSprite;
                    EditorUtility.SetDirty(prefab);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("SpriteRenderer не знайдено у префабі. Додайте компонент.", MessageType.Warning);
            }

            // Animator quick access
            var animator = prefab.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Animator параметри", EditorStyles.boldLabel);

                // Show animator parameters from the controller
                var controller = animator.runtimeAnimatorController;
                EditorGUILayout.HelpBox($"Animator контролер: {controller.name}\n\nПараметри можна редагувати у контролері або у редакторі префабу.", MessageType.Info);

                if (GUILayout.Button(new GUIContent("Редагувати контролер", "Відкрити Animator параметри"), GUILayout.Height(20f)))
                {
                    AssetDatabase.OpenAsset(controller);
                }
                EditorGUILayout.EndVertical();
            }
            else if (animator != null)
            {
                EditorGUILayout.HelpBox("Animator має бути налаштований з контролером, щоб використовувати анімаціями.", MessageType.Warning);
            }

            // Button to open prefab editor
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Відкрити в Prefab Editor", "Редагувати префаб у вікні редактора"), GUILayout.Height(24f)))
            {
                AssetDatabase.OpenAsset(prefab);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EndSection();
        }

        /// <summary>
        /// Малювання розділу характеристик: базова стаміна, випадковий розкид, дальність огляду.
        /// </summary>
        /// <summary>
        /// Малювання розділу характеристик: базова стаміна, випадковий розкид, дальність огляду.
        /// </summary>
        private void DrawStatsSection(SerializedProperty unit)
        {
            BeginSection("Характеристики", "d_Profiler.CPU", "Параметри, які впливають на gameplay: стаміна, розкид, дальність огляду.");

            var staminaProp = unit.FindPropertyRelative("BaseStamina");
            var rangeProp = unit.FindPropertyRelative("StaminaRandomRange");
            var visionProp = unit.FindPropertyRelative("VisionRange");

            staminaProp.floatValue = EditorGUILayout.Slider(new GUIContent("Базова стаміна", "Середній запас стаміни юніта."), Mathf.Max(0f, staminaProp.floatValue), 0f, 300f);
            rangeProp.vector2Value = EditorGUILayout.Vector2Field(new GUIContent("Розкид стаміни", "Мінімальний/максимальний випадковий зсув до базової стаміни."), rangeProp.vector2Value);
            visionProp.intValue = EditorGUILayout.IntSlider(new GUIContent("Дальність огляду", "Скільки тайлів юніт відкриває у Fog of War."), Mathf.Max(1, visionProp.intValue), 1, 20);

            DrawStaminaRangePreview(staminaProp.floatValue, rangeProp.vector2Value);
            EndSection();
        }

        /// <summary>
        /// Малювання розділу анімацій руху: тривалість, затримка на тайлі, клипи анімацій.
        /// </summary>
        /// <summary>
        /// Малювання розділу анімацій руху: тривалість, затримка на тайлі, клипи анімацій.
        /// </summary>
        private void DrawAnimationSection(SerializedProperty unit)
        {
            BeginSection("Анімація руху", "d_AnimationClip Icon", "Параметри PathAnimationSettings, які впливають на швидкість і паузу між тайлами.");

            var animation = unit.FindPropertyRelative("AnimationSettings");
            var duration = animation?.FindPropertyRelative("MoveDurationPerTile");
            var delay = animation?.FindPropertyRelative("DelayOnTile");

            if (duration != null)
                duration.floatValue = EditorGUILayout.Slider(new GUIContent("Тривалість кроку", "Скільки секунд займає рух між двома сусідніми тайлами."), Mathf.Max(0.02f, duration.floatValue), 0.02f, 2f);

            if (delay != null)
                delay.floatValue = EditorGUILayout.Slider(new GUIContent("Затримка на тайлі", "Пауза після завершення кроку перед наступним рухом."), Mathf.Max(0f, delay.floatValue), 0f, 1f);

            EditorGUILayout.BeginHorizontal();
            _showAnimationPreview = EditorGUILayout.ToggleLeft(new GUIContent("Програвати preview", "Показувати рух по маршруту."), _showAnimationPreview);
            if (GUILayout.Button(new GUIContent("Default", "Встановити стандартні значення: 0.3s рух, 0.05s затримка"), GUILayout.Width(70f), GUILayout.Height(18f)))
            {
                if (duration != null) duration.floatValue = 0.3f;
                if (delay != null) delay.floatValue = 0.05f;
            }
            EditorGUILayout.EndHorizontal();

            EndSection();

            // New animation clips section
            DrawAnimationClipsSection(unit);
        }

        private void DrawAnimationClipsSection(SerializedProperty unit)
        {
            BeginSection("Анімації юніта", "d_PlayButton", "Додайте анімації для удару, спокою, поранення та інших дій.");

            var animationClips = unit.FindPropertyRelative("AnimationClips");
            if (animationClips == null)
            {
                EndSection();
                return;
            }

            DrawAutoAnimationTools(unit, animationClips);
            EditorGUILayout.Space(4f);

            // Add animation button
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Всього анімацій:", animationClips.arraySize.ToString(), EditorStyles.miniLabel, GUILayout.Width(120f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("+", "Додати нову анімацію до списку"), GUILayout.Width(30f), GUILayout.Height(18f)))
            {
                animationClips.arraySize++;
                var newClip = animationClips.GetArrayElementAtIndex(animationClips.arraySize - 1);
                newClip.FindPropertyRelative("Name").stringValue = $"Animation {animationClips.arraySize}";
                newClip.FindPropertyRelative("Type").enumValueIndex = 0; // Idle
                newClip.FindPropertyRelative("Loop").boolValue = true;
                newClip.FindPropertyRelative("Duration").floatValue = 1f;
            }
            EditorGUILayout.EndHorizontal();

            // List of animations
            for (int i = 0; i < animationClips.arraySize; i++)
            {
                var clip = animationClips.GetArrayElementAtIndex(i);
                if (clip == null)
                    continue;

                DrawAnimationClipItem(clip, i, animationClips);
            }

            EndSection();
        }

        private void DrawAutoAnimationTools(SerializedProperty unit, SerializedProperty animationClips)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Швидке автоналаштування", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Автоматично збирає Idle/Move/Attack/TakeDamage/Die зі спрайтів за назвою.", EditorStyles.wordWrappedMiniLabel);

            DefaultAsset folderAsset = AssetDatabase.IsValidFolder(_autoAnimationSearchFolder)
                ? AssetDatabase.LoadAssetAtPath<DefaultAsset>(_autoAnimationSearchFolder)
                : null;

            var newFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField(
                new GUIContent("Папка спрайтів", "Папка, де шукати спрайти для анімацій."),
                folderAsset,
                typeof(DefaultAsset),
                false);

            if (newFolderAsset != folderAsset)
            {
                string newPath = AssetDatabase.GetAssetPath(newFolderAsset);
                if (AssetDatabase.IsValidFolder(newPath))
                    _autoAnimationSearchFolder = newPath;
            }

            if (string.IsNullOrWhiteSpace(_autoAnimationPrefix))
                _autoAnimationPrefix = SuggestAutoAnimationPrefix(unit);

            _autoAnimationPrefix = EditorGUILayout.TextField(
                new GUIContent("Префікс імені", "Спільна частина імен спрайтів. Напр.: archer, worker, cossack."),
                _autoAnimationPrefix);
            _autoAnimationFps = EditorGUILayout.IntSlider(new GUIContent("FPS", "Буде застосовано до автогенерованих спрайт-анімацій."), Mathf.Clamp(_autoAnimationFps, 1, 60), 1, 60);
            _autoAnimationReplaceByType = EditorGUILayout.ToggleLeft(
                new GUIContent("Заміняти існуючі за типом", "Якщо ввімкнено - оновить існуючі Idle/Move/... замість дублювання."),
                _autoAnimationReplaceByType);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Автостворити набір", "Створити та заповнити базові анімації з папки."), GUILayout.Height(22f)))
                RequestAutoGenerateAnimationSet(unit, animationClips);

            Color old = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.95f, 0.36f, 0.32f);
            if (GUILayout.Button(new GUIContent("Очистити всі", "Видалити всі анімації юніта."), GUILayout.Width(110f), GUILayout.Height(22f)))
                RequestClearAllAnimations(unit, animationClips);
            GUI.backgroundColor = old;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void AutoGenerateAnimationSet(SerializedProperty unit, SerializedProperty animationClips)
        {
            if (animationClips == null)
                return;

            if (string.IsNullOrWhiteSpace(_autoAnimationSearchFolder) || !AssetDatabase.IsValidFolder(_autoAnimationSearchFolder))
            {
                EditorUtility.DisplayDialog("Автогенерація анімацій", "Оберіть валідну папку зі спрайтами.", "OK");
                return;
            }

            string prefix = string.IsNullOrWhiteSpace(_autoAnimationPrefix)
                ? SuggestAutoAnimationPrefix(unit)
                : _autoAnimationPrefix;

            if (string.IsNullOrWhiteSpace(prefix))
            {
                EditorUtility.DisplayDialog("Автогенерація анімацій", "Не вдалося визначити префікс. Вкажіть його вручну.", "OK");
                return;
            }

            var groupedSprites = CollectAnimationSpritesByType(_autoAnimationSearchFolder, prefix);
            int createdCount = 0;

            foreach (var kvp in groupedSprites)
            {
                var type = kvp.Key;
                var sprites = kvp.Value;
                if (sprites == null || sprites.Count == 0)
                    continue;

                SerializedProperty clip = _autoAnimationReplaceByType
                    ? FindAnimationClipByType(animationClips, type)
                    : null;

                if (clip == null)
                {
                    animationClips.arraySize++;
                    clip = animationClips.GetArrayElementAtIndex(animationClips.arraySize - 1);
                }

                ConfigureAnimationClip(clip, type, sprites, _autoAnimationFps);
                createdCount++;
            }

            if (createdCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Автогенерація анімацій",
                    $"Не знайдено кадрів для префікса '{prefix}' у '{_autoAnimationSearchFolder}'.\n" +
                    "Приклад імен: archer_idle_01, archer_attack_02, archer_move_03.",
                    "OK");
                return;
            }

            _autoAnimationPrefix = prefix;
            _previewAnimationIndex = -1;
            _isPlayingAnimation = false;
            EditorUtility.SetDirty(_registry);
            EditorUtility.DisplayDialog("Автогенерація анімацій", $"Створено/оновлено анімацій: {createdCount}.", "OK");
        }

        private void RequestAutoGenerateAnimationSet(SerializedProperty unit, SerializedProperty animationClips)
        {
            if (animationClips == null)
                return;

            if (!_safeEditMode)
            {
                AutoGenerateAnimationSet(unit, animationClips);
                return;
            }

            string targetTypeId = GetString(unit, "TypeId");
            int fallbackIndex = _selectedIndex;
            string prefix = string.IsNullOrWhiteSpace(_autoAnimationPrefix)
                ? SuggestAutoAnimationPrefix(unit)
                : _autoAnimationPrefix;

            if (string.IsNullOrWhiteSpace(prefix))
            {
                EditorUtility.DisplayDialog("Автогенерація анімацій", "Не вдалося визначити префікс. Вкажіть його вручну.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_autoAnimationSearchFolder) || !AssetDatabase.IsValidFolder(_autoAnimationSearchFolder))
            {
                EditorUtility.DisplayDialog("Автогенерація анімацій", "Оберіть валідну папку зі спрайтами.", "OK");
                return;
            }

            var groupedSprites = CollectAnimationSpritesByType(_autoAnimationSearchFolder, prefix);
            var previewLines = new List<string>();
            int affected = 0;
            foreach (var kvp in groupedSprites)
            {
                if (kvp.Value == null || kvp.Value.Count == 0)
                    continue;

                bool exists = _autoAnimationReplaceByType && FindAnimationClipByType(animationClips, kvp.Key) != null;
                string action = exists ? "Оновити" : "Додати";
                previewLines.Add($"{action} {kvp.Key}: {kvp.Value.Count} кадр(ів)");
                affected++;
            }

            if (affected == 0)
            {
                EditorUtility.DisplayDialog(
                    "Автогенерація анімацій",
                    $"Не знайдено кадрів для префікса '{prefix}' у '{_autoAnimationSearchFolder}'.",
                    "OK");
                return;
            }

            QueueSafeEditOperation(
                "Масова автогенерація анімацій",
                $"Юніт: {(string.IsNullOrWhiteSpace(targetTypeId) ? "<без TypeId>" : targetTypeId)}. Буде змінено анімацій: {affected}.",
                previewLines,
                () =>
                {
                    var targetUnit = ResolveTargetUnitForSafeEdit(targetTypeId, fallbackIndex);
                    if (targetUnit == null)
                        return;

                    var targetClips = targetUnit.FindPropertyRelative("AnimationClips");
                    if (targetClips == null)
                        return;

                    AutoGenerateAnimationSet(targetUnit, targetClips);
                });
        }

        private void RequestClearAllAnimations(SerializedProperty unit, SerializedProperty animationClips)
        {
            if (animationClips == null)
                return;

            string targetTypeId = GetString(unit, "TypeId");
            int fallbackIndex = _selectedIndex;
            int clipsCount = animationClips.arraySize;
            if (clipsCount <= 0)
                return;

            if (!_safeEditMode)
            {
                if (EditorUtility.DisplayDialog("Очистити анімації", "Видалити всі анімації для цього юніта?", "Так", "Скасувати"))
                {
                    animationClips.arraySize = 0;
                    _previewAnimationIndex = -1;
                    _isPlayingAnimation = false;
                }
                return;
            }

            var previewLines = new List<string>();
            for (int i = 0; i < clipsCount; i++)
            {
                var clip = animationClips.GetArrayElementAtIndex(i);
                string clipName = clip?.FindPropertyRelative("Name")?.stringValue;
                previewLines.Add($"Видалити анімацію: {(string.IsNullOrWhiteSpace(clipName) ? $"#{i + 1}" : clipName)}");
            }

            QueueSafeEditOperation(
                "Масове очищення анімацій",
                $"Юніт: {(string.IsNullOrWhiteSpace(targetTypeId) ? "<без TypeId>" : targetTypeId)}. Буде видалено {clipsCount} анімацій.",
                previewLines,
                () =>
                {
                    var targetUnit = ResolveTargetUnitForSafeEdit(targetTypeId, fallbackIndex);
                    if (targetUnit == null)
                        return;

                    var targetClips = targetUnit.FindPropertyRelative("AnimationClips");
                    if (targetClips == null)
                        return;

                    targetClips.arraySize = 0;
                    _previewAnimationIndex = -1;
                    _isPlayingAnimation = false;
                });
        }

        private static Dictionary<AnimationType, List<Sprite>> CollectAnimationSpritesByType(string folder, string prefix)
        {
            var result = new Dictionary<AnimationType, List<Sprite>>
            {
                { AnimationType.Idle, new List<Sprite>() },
                { AnimationType.Move, new List<Sprite>() },
                { AnimationType.Attack, new List<Sprite>() },
                { AnimationType.TakeDamage, new List<Sprite>() },
                { AnimationType.Die, new List<Sprite>() },
            };

            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            string normalizedPrefix = prefix.Trim().ToLowerInvariant();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    continue;

                if (!SpriteImportDragDropPolicy.IsAllowedSprite(sprite, out _))
                    continue;

                string name = sprite.name.ToLowerInvariant();
                if (!name.Contains(normalizedPrefix))
                    continue;

                if (MatchesAny(name, "idle", "stand", "rest"))
                    result[AnimationType.Idle].Add(sprite);
                else if (MatchesAny(name, "move", "walk", "run"))
                    result[AnimationType.Move].Add(sprite);
                else if (MatchesAny(name, "attack", "hit", "slash", "strike", "shoot"))
                    result[AnimationType.Attack].Add(sprite);
                else if (MatchesAny(name, "damage", "hurt", "hitreact", "take"))
                    result[AnimationType.TakeDamage].Add(sprite);
                else if (MatchesAny(name, "die", "death", "dead"))
                    result[AnimationType.Die].Add(sprite);
            }

            foreach (var kvp in result)
                kvp.Value.Sort((a, b) => string.Compare(a != null ? a.name : string.Empty, b != null ? b.name : string.Empty, StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private static bool MatchesAny(string value, params string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                if (value.Contains(tokens[i]))
                    return true;
            }

            return false;
        }

        private static SerializedProperty FindAnimationClipByType(SerializedProperty animationClips, AnimationType type)
        {
            if (animationClips == null)
                return null;

            for (int i = 0; i < animationClips.arraySize; i++)
            {
                var clip = animationClips.GetArrayElementAtIndex(i);
                var typeRef = clip?.FindPropertyRelative("Type");
                if (typeRef != null && typeRef.enumValueIndex == (int)type)
                    return clip;
            }

            return null;
        }

        private static void ConfigureAnimationClip(SerializedProperty clip, AnimationType type, List<Sprite> sprites, int fps)
        {
            if (clip == null)
                return;

            var nameRef = clip.FindPropertyRelative("Name");
            var typeRef = clip.FindPropertyRelative("Type");
            var animClipRef = clip.FindPropertyRelative("AnimationClip");
            var spritesRef = clip.FindPropertyRelative("SpriteFrames");
            var loopRef = clip.FindPropertyRelative("Loop");
            var durationRef = clip.FindPropertyRelative("Duration");
            var fpsRef = clip.FindPropertyRelative("SpriteFPS");
            var animatorParamRef = clip.FindPropertyRelative("AnimatorParameterName");

            if (nameRef != null)
                nameRef.stringValue = type.ToString();
            if (typeRef != null)
                typeRef.enumValueIndex = (int)type;
            if (animClipRef != null)
                animClipRef.objectReferenceValue = null;
            if (fpsRef != null)
                fpsRef.intValue = Mathf.Clamp(fps, 1, 60);
            if (animatorParamRef != null && string.IsNullOrWhiteSpace(animatorParamRef.stringValue))
                animatorParamRef.stringValue = type.ToString();

            if (loopRef != null)
                loopRef.boolValue = type == AnimationType.Idle || type == AnimationType.Move;

            if (spritesRef != null)
            {
                spritesRef.arraySize = sprites.Count;
                for (int i = 0; i < sprites.Count; i++)
                    spritesRef.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }

            if (durationRef != null)
            {
                float safeFps = Mathf.Max(1f, fps);
                durationRef.floatValue = Mathf.Max(0.1f, sprites.Count / safeFps);
            }
        }

        private static string SuggestAutoAnimationPrefix(SerializedProperty unit)
        {
            if (unit == null)
                return string.Empty;

            string typeId = GetString(unit, "TypeId");
            if (!string.IsNullOrWhiteSpace(typeId))
            {
                string sanitized = Regex.Replace(typeId.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
                if (!string.IsNullOrWhiteSpace(sanitized))
                    return sanitized;
            }

            var prefab = GetObject<GameObject>(unit, "Prefab");
            if (prefab != null)
            {
                string prefabName = Regex.Replace(prefab.name.Trim().ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
                if (!string.IsNullOrWhiteSpace(prefabName))
                    return prefabName;
            }

            return string.Empty;
        }

        private void DrawAnimationClipItem(SerializedProperty clip, int index, SerializedProperty animationClips)
        {
            var nameRef = clip.FindPropertyRelative("Name");
            var typeRef = clip.FindPropertyRelative("Type");
            var animClipRef = clip.FindPropertyRelative("AnimationClip");
            var spritesRef = clip.FindPropertyRelative("SpriteFrames");
            var loopRef = clip.FindPropertyRelative("Loop");
            var durationRef = clip.FindPropertyRelative("Duration");

            string clipName = nameRef?.stringValue ?? "Animation";
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Header with name and delete button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{index}] {clipName}", EditorStyles.boldLabel, GUILayout.MinWidth(150f));
            GUILayout.FlexibleSpace();

            Color oldBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.95f, 0.36f, 0.32f);
            if (GUILayout.Button("×", GUILayout.Width(24f), GUILayout.Height(18f)))
            {
                animationClips.DeleteArrayElementAtIndex(index);
                GUI.backgroundColor = oldBg;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            GUI.backgroundColor = oldBg;
            EditorGUILayout.EndHorizontal();

            // Type and name
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(typeRef, new GUIContent("Тип", "Категорія анімації (Idle, Move, Attack, TakeDamage, Die)"), GUILayout.MaxWidth(200f));
            EditorGUILayout.PropertyField(nameRef, new GUIContent("Назва", "Унікальна назва анімації"), GUILayout.MinWidth(150f));
            EditorGUILayout.EndHorizontal();

            // Animation clip or sprite frames
            EditorGUILayout.PropertyField(animClipRef, new GUIContent("Клип Animator", "Клип зі встановленого Animator контролера"));

            if (GUILayout.Button(new GUIContent("Або вибрати спрайти", "Замість AnimationClip можна використовувати простий список спрайтів для побудови анімації"), GUILayout.Height(20f)))
            {
                // Will be handled by PropertyField
            }

            // Show sprite frames if available
            if (spritesRef != null && spritesRef.arraySize > 0)
            {
                EditorGUILayout.PropertyField(spritesRef, new GUIContent("Спрайти", "Список спрайтів для анімації"), true);
                if (spritesRef.arraySize > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("FPS", GUILayout.MaxWidth(40f));
                    var fpsRef = clip.FindPropertyRelative("SpriteFPS");
                    if (fpsRef != null)
                        fpsRef.intValue = EditorGUILayout.IntSlider(new GUIContent("", "Кількість кадрів на секунду для спрайт-анімації"), Mathf.Max(1, fpsRef.intValue), 1, 60, GUILayout.MinWidth(100f));
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Loop and duration
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(loopRef, new GUIContent("Зациклити", "Повторювати анімацію нескінченно"), GUILayout.MaxWidth(200f));
            EditorGUILayout.PropertyField(durationRef, new GUIContent("Тривалість (сек)", "Приблизна середня тривалість однієї проходки анімації"), GUILayout.MinWidth(150f));
            EditorGUILayout.EndHorizontal();

            // Preview button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool isCurrentlyPlaying = _isPlayingAnimation && _previewAnimationIndex == index;
            Color btnColor = GUI.backgroundColor;
            if (isCurrentlyPlaying)
                GUI.backgroundColor = new Color(0.2f, 0.6f, 0.8f);

            if (GUILayout.Button(new GUIContent(isCurrentlyPlaying ? "⏸️ Зупинити" : "▶️ Переглянути", "Програти цю анімацію в панелі прев'ю"), GUILayout.Width(140f), GUILayout.Height(20f)))
            {
                if (isCurrentlyPlaying)
                {
                    _isPlayingAnimation = false;
                    _previewAnimationIndex = -1;
                }
                else
                {
                    _previewAnimationIndex = index;
                    _animationPlaybackTime = 0f;
                    _lastAnimationFrameTime = EditorApplication.timeSinceStartup;
                    _isPlayingAnimation = true;
                }
            }
            GUI.backgroundColor = btnColor;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawDangerSection(SerializedProperty unit)
        {
            BeginSection("Реєстр", "d_TreeEditor.Trash", "Операції над записом у UnitRegistrySO.");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Дублювати", "Створити копію вибраного запису.")))
                DuplicateSelectedUnit();

            Color old = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.95f, 0.36f, 0.32f);
            if (GUILayout.Button(new GUIContent("Видалити", "Видалити юніта з registry.")))
                DeleteSelectedUnit();
            GUI.backgroundColor = old;
            EditorGUILayout.EndHorizontal();

            EndSection();
        }

        private void DrawAnimatedPreview(Rect rect, SerializedProperty unit, GameObject prefab, Sprite sprite)
        {
            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.13f, 0.15f) : new Color(0.78f, 0.82f, 0.86f));

            float gridSize = Mathf.Clamp(rect.width / 12f, 16f, 24f);
            DrawPreviewGrid(rect, gridSize);

            float padding = Mathf.Clamp(rect.width * 0.08f, 20f, 40f);
            Rect pathRect = new Rect(rect.x + padding, rect.y + rect.height * 0.54f, rect.width - padding * 2f, rect.height * 0.3f);
            Vector2 a = new Vector2(pathRect.xMin, pathRect.center.y);
            Vector2 b = new Vector2(pathRect.center.x, pathRect.yMin + Mathf.Max(6f, pathRect.height * 0.15f));
            Vector2 c = new Vector2(pathRect.xMax, pathRect.center.y);
            DrawLine(a, b, new Color(1f, 1f, 1f, 0.28f), 2f);
            DrawLine(b, c, new Color(1f, 1f, 1f, 0.28f), 2f);
            DrawNode(a);
            DrawNode(b);
            DrawNode(c);

            // Check if we're currently previewing an animation
            Sprite displaySprite = sprite;
            string statusLabel = "Рух між тайлами";
            Vector2 centerPos = new Vector2(rect.center.x, rect.y + rect.height * 0.4f);
            float movementDuration = Mathf.Max(0.02f, GetNestedFloat(unit, "AnimationSettings", "MoveDurationPerTile", 0.3f));
            float movementDelay = Mathf.Max(0f, GetNestedFloat(unit, "AnimationSettings", "DelayOnTile", 0.05f));
            float cycleTime = (movementDuration + movementDelay) * 4f;
            float normalizedTime = _autoPlayPreview && _showAnimationPreview && cycleTime > 0.001f
                ? _previewSimulationTime % cycleTime
                : 0f;

            if (_isPlayingAnimation && _previewAnimationIndex >= 0)
            {
                var animationClips = unit.FindPropertyRelative("AnimationClips");
                if (animationClips != null && _previewAnimationIndex < animationClips.arraySize)
                {
                    var clip = animationClips.GetArrayElementAtIndex(_previewAnimationIndex);
                    var nameRef = clip.FindPropertyRelative("Name");
                    var typeRef = clip.FindPropertyRelative("Type");
                    var spritesRef = clip.FindPropertyRelative("SpriteFrames");
                    var durationRef = clip.FindPropertyRelative("Duration");
                    var fpsRef = clip.FindPropertyRelative("SpriteFPS");
                    var loopRef = clip.FindPropertyRelative("Loop");

                    string clipName = nameRef != null ? nameRef.stringValue : "Unknown";
                    string clipType = typeRef != null ? typeRef.enumDisplayNames[Mathf.Clamp(typeRef.enumValueIndex, 0, typeRef.enumDisplayNames.Length - 1)] : "Unknown";
                    int fps = fpsRef != null ? Mathf.Max(1, fpsRef.intValue) : 0;
                    bool loop = loopRef != null && loopRef.boolValue;

                    // Get the current sprite frame from the animation
                    if (spritesRef != null && spritesRef.arraySize > 0)
                    {
                        float duration = durationRef != null ? durationRef.floatValue : 1f;
                        float frameTime = Mathf.Max(0.01f, duration / spritesRef.arraySize);
                        int frameIndex = Mathf.FloorToInt(_animationPlaybackTime / frameTime) % spritesRef.arraySize;
                        var frame = spritesRef.GetArrayElementAtIndex(frameIndex);
                        displaySprite = frame.objectReferenceValue as Sprite;
                        statusLabel = $"{clipType}: {clipName} | кадр {frameIndex + 1}/{spritesRef.arraySize} | {fps} FPS | {(loop ? "Loop" : "Once")} | sim x{_previewSimulationSpeed:0.00}";
                    }
                    else
                    {
                        statusLabel = $"{clipType}: {clipName} | AnimationClip/без спрайтів | sim x{_previewSimulationSpeed:0.00}";
                    }
                }
            }
            else
            {
                float segment = movementDuration + movementDelay;
                int phase = segment > 0.001f ? Mathf.FloorToInt(normalizedTime / segment) % 4 : 0;
                float local = segment > 0.001f ? normalizedTime - phase * segment : 0f;
                bool waiting = local > movementDuration;
                float moveT = movementDuration > 0.001f ? Mathf.Clamp01(local / movementDuration) : 1f;

                Vector2 from;
                Vector2 to;
                string phaseName;
                switch (phase)
                {
                    case 0:
                        from = a;
                        to = b;
                        phaseName = "A->B";
                        break;
                    case 1:
                        from = b;
                        to = c;
                        phaseName = "B->C";
                        break;
                    case 2:
                        from = c;
                        to = b;
                        phaseName = "C->B";
                        break;
                    default:
                        from = b;
                        to = a;
                        phaseName = "B->A";
                        break;
                }

                centerPos = waiting ? to : Vector2.Lerp(from, to, moveT);
                centerPos.y -= Mathf.Clamp(rect.height * 0.16f, 18f, 44f);

                float tilesPerSecond = 1f / Mathf.Max(0.02f, movementDuration + movementDelay);
                float effectiveSpeed = _pauseSimulation ? 0f : tilesPerSecond * Mathf.Max(0.05f, _previewSimulationSpeed);
                statusLabel = waiting
                    ? $"Пауза ({phaseName}) | speed {effectiveSpeed:0.00} tile/s | delay {movementDelay:0.00}s"
                    : $"Рух ({phaseName}) | speed {effectiveSpeed:0.00} tile/s | t {moveT:0.00} | sim x{_previewSimulationSpeed:0.00}";
            }

            float spriteSize = Mathf.Clamp(rect.width * 0.3f, 40f, 100f);
            Rect spriteRect = new Rect(centerPos.x - spriteSize * 0.5f, centerPos.y - spriteSize * 0.9f, spriteSize, spriteSize);
            DrawSpriteOrPrefab(spriteRect, displaySprite, prefab, true);

            Rect labelRect = new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, 20f);
            string id = GetString(unit, "TypeId");
            GUI.Label(labelRect, string.IsNullOrWhiteSpace(id) ? "<без TypeId>" : id, PreviewTitleStyle());

            Rect stateRect = new Rect(rect.x + 12f, rect.yMax - 24f, rect.width - 24f, 16f);
            GUI.Label(stateRect, statusLabel, EditorStyles.centeredGreyMiniLabel);

            if (displaySprite == null && prefab == null)
            {
                GUI.Label(new Rect(rect.x + 16f, rect.center.y - 12f, rect.width - 32f, 24f), "Додайте prefab або створіть його", CenterMiniStyle());
            }
        }

        private void DrawDetailedStatePreview(SerializedProperty unit)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Симуляція стану (усі параметри)", EditorStyles.boldLabel);

            string typeId = GetString(unit, "TypeId");
            UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);
            int combatTypeIdx = GetEnumIndex(unit, "CombatType");
            string combatType = Enum.IsDefined(typeof(UnitCombatType), combatTypeIdx)
                ? ((UnitCombatType)combatTypeIdx).ToString()
                : combatTypeIdx.ToString(CultureInfo.InvariantCulture);

            int level = GetInt(unit, "BaseLevel");
            int hp = GetInt(unit, "HitPoints");
            float stamina = GetFloat(unit, "BaseStamina");
            Vector2 staminaRange = GetVector2(unit, "StaminaRandomRange");
            int vision = GetInt(unit, "VisionRange");

            int cuttingDmg = Mathf.Max(0, GetInt(unit, "CuttingDamage"));
            int penetratingDmg = Mathf.Max(0, GetInt(unit, "PenetratingDamage"));
            int crushingDmg = Mathf.Max(0, GetInt(unit, "CrushingDamage"));
            int totalDamage = cuttingDmg + penetratingDmg + crushingDmg;

            int cuttingDef = Mathf.Max(0, GetInt(unit, "CuttingDefense"));
            int penetratingDef = Mathf.Max(0, GetInt(unit, "PenetratingDefense"));
            int crushingDef = Mathf.Max(0, GetInt(unit, "CrushingDefense"));
            int totalDefense = cuttingDef + penetratingDef + crushingDef;

            float moveDuration = Mathf.Max(0.02f, GetNestedFloat(unit, "AnimationSettings", "MoveDurationPerTile", 0.3f));
            float moveDelay = Mathf.Max(0f, GetNestedFloat(unit, "AnimationSettings", "DelayOnTile", 0.05f));
            float speed = 1f / Mathf.Max(0.02f, moveDuration + moveDelay);
            float effectiveSpeed = _pauseSimulation ? 0f : speed * Mathf.Max(0.05f, _previewSimulationSpeed);

            var prefab = GetObject<GameObject>(unit, "Prefab");
            var customSprite = GetObject<Sprite>(unit, "CustomSprite");

            var clips = unit.FindPropertyRelative("AnimationClips");
            int clipCount = clips != null ? clips.arraySize : 0;

            string activeAnimation = "Немає";
            if (_isPlayingAnimation && clips != null && _previewAnimationIndex >= 0 && _previewAnimationIndex < clips.arraySize)
            {
                var clip = clips.GetArrayElementAtIndex(_previewAnimationIndex);
                var nameRef = clip.FindPropertyRelative("Name");
                var typeRef = clip.FindPropertyRelative("Type");
                var fpsRef = clip.FindPropertyRelative("SpriteFPS");
                var durationRef = clip.FindPropertyRelative("Duration");
                var loopRef = clip.FindPropertyRelative("Loop");
                var spritesRef = clip.FindPropertyRelative("SpriteFrames");
                var animatorParamRef = clip.FindPropertyRelative("AnimatorParameterName");

                string clipName = nameRef != null ? nameRef.stringValue : "Unknown";
                string clipType = typeRef != null ? typeRef.enumDisplayNames[Mathf.Clamp(typeRef.enumValueIndex, 0, typeRef.enumDisplayNames.Length - 1)] : "Unknown";
                int fps = fpsRef != null ? Mathf.Max(1, fpsRef.intValue) : 0;
                float duration = durationRef != null ? Mathf.Max(0.01f, durationRef.floatValue) : 0f;
                bool loop = loopRef != null && loopRef.boolValue;
                int frames = spritesRef != null ? spritesRef.arraySize : 0;
                string animatorParam = animatorParamRef != null ? animatorParamRef.stringValue : string.Empty;

                activeAnimation = $"{clipType}/{clipName} | {frames} frames | {fps} FPS | {duration:0.00}s | {(loop ? "Loop" : "Once")}";
                if (!string.IsNullOrWhiteSpace(animatorParam))
                    activeAnimation += $" | Animator: {animatorParam}";
            }

            EditorGUILayout.LabelField($"TypeId: {(string.IsNullOrWhiteSpace(typeId) ? "<порожній>" : typeId)}");
            EditorGUILayout.LabelField($"Role/Combat: {role} / {combatType}");
            EditorGUILayout.LabelField($"Level/HP: {level} / {hp}");
            EditorGUILayout.LabelField($"Stamina: {stamina:0.0} ({stamina + staminaRange.x:0.0}..{stamina + staminaRange.y:0.0})");
            EditorGUILayout.LabelField($"Vision: {vision}");
            EditorGUILayout.LabelField($"Prefab: {(prefab != null ? prefab.name : "<не задано>")}");
            EditorGUILayout.LabelField($"CustomSprite: {(customSprite != null ? customSprite.name : "<не задано>")}");
            EditorGUILayout.LabelField($"Damage C/P/Cr: {cuttingDmg}/{penetratingDmg}/{crushingDmg} (total {totalDamage})");
            EditorGUILayout.LabelField($"Defense C/P/Cr: {cuttingDef}/{penetratingDef}/{crushingDef} (total {totalDefense})");
            EditorGUILayout.LabelField($"Movement: duration {moveDuration:0.00}s, delay {moveDelay:0.00}s, speed {speed:0.00} tile/s");
            EditorGUILayout.LabelField($"Simulation: speed x{_previewSimulationSpeed:0.00}, effective {effectiveSpeed:0.00} tile/s, time {_previewSimulationTime:0.00}s");
            EditorGUILayout.LabelField($"Animations: {clipCount} | Active: {activeAnimation}", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawStatVisualization(SerializedProperty unit)
        {
            float stamina = GetFloat(unit, "BaseStamina");
            Vector2 range = GetVector2(unit, "StaminaRandomRange");
            int vision = GetInt(unit, "VisionRange");
            float duration = GetNestedFloat(unit, "AnimationSettings", "MoveDurationPerTile", 0.3f);

            DrawMetricBar("Стаміна", stamina, 0f, 300f, Good, "Чим вище, тим довше юніт може виконувати дії до виснаження.");
            DrawMetricBar("Розкид", Mathf.Abs(range.x) + Mathf.Abs(range.y), 0f, 80f, Warn, "Ширина випадкового діапазону стартової стаміни.");
            DrawMetricBar("Огляд", vision, 1f, 20f, Accent, "Радіус активної видимості Fog of War.");
            DrawMetricBar("Швидкість", 1f / Mathf.Max(0.02f, duration), 0.5f, 10f, new Color(0.7f, 0.48f, 1f), "Вища шкала означає швидший рух між тайлами.");
        }

        private void DrawVisionGrid(SerializedProperty unit)
        {
            int radius = Mathf.Clamp(GetInt(unit, "VisionRange"), 1, 12);

            // Adaptive height based on window width
            float minHeight = Mathf.Clamp(position.width * 0.3f, 120f, 200f);
            Rect rect = GUILayoutUtility.GetRect(120f, minHeight, GUILayout.ExpandWidth(true));
            DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.14f, 0.16f, 0.17f) : new Color(0.86f, 0.89f, 0.9f));

            int side = radius * 2 + 1;
            float padding = Mathf.Max(12f, rect.width * 0.05f);
            float cellSize = Mathf.Floor(Mathf.Min((rect.width - padding * 2f) / side, (rect.height - 40f) / side));
            Vector2 start = new Vector2(rect.center.x - side * cellSize * 0.5f, rect.y + 28f);
            float sqrRadius = (radius + 0.5f) * (radius + 0.5f);

            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 18f), $"Огляд: {radius} тайл(ів)", EditorStyles.boldLabel);

            for (int y = 0; y < side; y++)
            {
                for (int x = 0; x < side; x++)
                {
                    int dx = x - radius;
                    int dy = y - radius;
                    var cellRect = new Rect(start.x + x * cellSize, start.y + y * cellSize, cellSize - 1f, cellSize - 1f);
                    bool inside = dx * dx + dy * dy <= sqrRadius;
                    EditorGUI.DrawRect(cellRect, inside ? VisionFill : new Color(0f, 0f, 0f, 0.08f));
                    if (dx == 0 && dy == 0)
                        EditorGUI.DrawRect(cellRect, Good);
                }
            }

            Handles.BeginGUI();
            Handles.color = VisionOutline;
            Handles.DrawWireDisc(new Vector3(rect.center.x, start.y + radius * cellSize + cellSize * 0.5f, 0f), Vector3.forward, radius * cellSize + cellSize * 0.5f);
            Handles.EndGUI();
        }

        private void DrawQuickHints(SerializedProperty unit, GameObject prefab, Sprite sprite)
        {
            string typeId = GetString(unit, "TypeId");
            var hints = new List<string>();

            if (string.IsNullOrWhiteSpace(typeId))
                hints.Add("TypeId порожній.");
            else if (typeId.Contains("_"))
                hints.Add("TypeId містить '_' — цей символ зарезервований.");

            if (prefab == null)
                hints.Add("Prefab не задано.");
            else if (sprite == null)
                hints.Add("У prefab не знайдено SpriteRenderer.");

            if (GetFloat(unit, "BaseStamina") <= 0f)
                hints.Add("Base Stamina має бути більше 0.");

            if (hints.Count == 0)
            {
                EditorGUILayout.HelpBox("Юніт виглядає готовим для використання у runtime.", MessageType.Info);
                return;
            }

            for (int i = 0; i < hints.Count; i++)
                EditorGUILayout.HelpBox(hints[i], MessageType.Warning);
        }

        private void DrawListStats()
        {
            int total = _configs.arraySize;
            int problems = 0;
            int workers = 0;
            int military = 0;
            for (int i = 0; i < total; i++)
            {
                var unit = _configs.GetArrayElementAtIndex(i);
                if (ValidateUnit(unit, i) != null)
                    problems++;

                UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);
                if (role == UnitRole.Worker) workers++;
                if (role == UnitRole.Military) military++;
            }

            EditorGUILayout.BeginHorizontal();
            DrawMiniCounter("Всього", total, Accent);
            DrawMiniCounter("Worker", workers, Good);
            DrawMiniCounter("Military", military, Warn);
            DrawMiniCounter("Проблем", problems, problems > 0 ? Bad : Good);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawValidationSummary(SerializedProperty unit, int index)
        {
            string validation = ValidateUnit(unit, index);
            if (validation == null)
            {
                EditorGUILayout.HelpBox("Запис валідний: ID унікальний, prefab заданий, базові параметри в нормі.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(validation, MessageType.Error);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("🔧 Автоправка", "Спробувати автоматично вирішити помилку."), GUILayout.Height(24f), GUILayout.Width(140f)))
            {
                if (TryAutoFixUnit(unit, index))
                    TryCommitRegistryChanges("Автоправка юніта");
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4f);
        }

        private void DrawRoleFilterButton(UnitRole? role, string label)
        {
            bool active = _roleFilter == role;
            Color old = GUI.backgroundColor;
            if (active)
                GUI.backgroundColor = Accent;

            if (GUILayout.Button(new GUIContent(label, role.HasValue ? $"Показати тільки {label}." : "Показати всі ролі."), EditorStyles.miniButton))
                _roleFilter = role;

            GUI.backgroundColor = old;
        }

        private void DrawBatchOperationsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _showBatchTools = EditorGUILayout.Foldout(_showBatchTools, "Batch Operations", true);
            if (!_showBatchTools)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Масові зміни для групи юнітів", EditorStyles.miniBoldLabel);
            _batchTargetFiltered = EditorGUILayout.ToggleLeft(
                new GUIContent("Тільки за поточним фільтром", "Якщо вимкнено - застосування йде до всіх юнітів у registry."),
                _batchTargetFiltered);

            EditorGUILayout.Space(2f);
            _batchApplyRole = EditorGUILayout.ToggleLeft(new GUIContent("Змінити Role"), _batchApplyRole);
            using (new EditorGUI.DisabledGroupScope(!_batchApplyRole))
                _batchRole = (UnitRole)EditorGUILayout.EnumPopup(new GUIContent("Нова Role"), _batchRole);

            _batchApplyPrefab = EditorGUILayout.ToggleLeft(new GUIContent("Змінити Prefab reference"), _batchApplyPrefab);
            using (new EditorGUI.DisabledGroupScope(!_batchApplyPrefab))
            {
                _batchPrefab = (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Новий Prefab"),
                    _batchPrefab,
                    typeof(GameObject),
                    false);
            }

            _batchApplyAnimationDefaults = EditorGUILayout.ToggleLeft(new GUIContent("Застосувати animation defaults"), _batchApplyAnimationDefaults);
            using (new EditorGUI.DisabledGroupScope(!_batchApplyAnimationDefaults))
            {
                _batchAnimationDuration = EditorGUILayout.Slider(new GUIContent("Move Duration / Tile"), Mathf.Max(0.02f, _batchAnimationDuration), 0.02f, 2f);
                _batchAnimationDelay = EditorGUILayout.Slider(new GUIContent("Delay On Tile"), Mathf.Max(0f, _batchAnimationDelay), 0f, 1f);
            }

            int targetCount = CollectBatchTargetIndices().Count;
            EditorGUILayout.LabelField($"Цільових юнітів: {targetCount}", EditorStyles.miniLabel);

            if (_batchApplyPrefab && _batchPrefab == null)
                EditorGUILayout.HelpBox("Для batch зміни prefab reference оберіть prefab asset.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!CanApplyBatchOperation() || targetCount <= 0);
            if (GUILayout.Button(new GUIContent("Застосувати batch", "Виконати обрані масові зміни для цільової групи."), GUILayout.Height(22f)))
                RequestApplyBatchOperations();
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(new GUIContent("Reset", "Скинути налаштування batch-операцій."), GUILayout.Width(64f), GUILayout.Height(22f)))
                ResetBatchOperationState();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private bool CanApplyBatchOperation()
        {
            if (_batchApplyPrefab && _batchPrefab == null)
                return false;

            return _batchApplyRole || _batchApplyPrefab || _batchApplyAnimationDefaults;
        }

        private void ResetBatchOperationState()
        {
            _batchApplyRole = false;
            _batchRole = UnitRole.Worker;
            _batchApplyPrefab = false;
            _batchPrefab = null;
            _batchApplyAnimationDefaults = false;
            _batchAnimationDuration = 0.3f;
            _batchAnimationDelay = 0.05f;
        }

        private List<int> CollectBatchTargetIndices()
        {
            var result = new List<int>();
            if (_configs == null)
                return result;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                var unit = _configs.GetArrayElementAtIndex(i);
                if (unit == null)
                    continue;

                if (_batchTargetFiltered && !PassesFilters(unit, i))
                    continue;

                result.Add(i);
            }

            return result;
        }

        private void RequestApplyBatchOperations()
        {
            if (!CanApplyBatchOperation())
                return;

            List<int> targetIndices = CollectBatchTargetIndices();
            if (targetIndices.Count == 0)
                return;

            var targetTypeIds = new List<string>(targetIndices.Count);
            var fallbackIndices = new List<int>(targetIndices.Count);
            var previewLines = new List<string>();
            for (int i = 0; i < targetIndices.Count; i++)
            {
                int fallbackIndex = targetIndices[i];
                var unit = _configs.GetArrayElementAtIndex(fallbackIndex);
                if (unit == null)
                    continue;

                string typeId = GetString(unit, "TypeId");
                targetTypeIds.Add(typeId);
                fallbackIndices.Add(fallbackIndex);

                if (string.IsNullOrWhiteSpace(typeId))
                    typeId = $"#{fallbackIndex + 1}";

                var parts = new List<string>();
                if (_batchApplyRole)
                    parts.Add($"Role -> {_batchRole}");
                if (_batchApplyPrefab)
                    parts.Add($"Prefab -> {_batchPrefab.name}");
                if (_batchApplyAnimationDefaults)
                    parts.Add($"Anim -> {_batchAnimationDuration:0.00}/{_batchAnimationDelay:0.00}");

                previewLines.Add($"{typeId}: {string.Join(", ", parts)}");
            }

            Action applyAction = () => ApplyBatchOperationsToTargets(targetTypeIds, fallbackIndices);

            if (_safeEditMode)
            {
                QueueSafeEditOperation(
                    "Batch-операції Unit Designer",
                    $"Буде оновлено юнітів: {targetTypeIds.Count}.",
                    previewLines,
                    applyAction);
                return;
            }

            _registryObject?.Update();
            applyAction.Invoke();
            TryCommitRegistryChanges("Batch operations");
        }

        private void ApplyBatchOperationsToTargets(List<string> targetTypeIds, List<int> fallbackIndices)
        {
            if (_configs == null || targetTypeIds == null || fallbackIndices == null)
                return;

            int count = Mathf.Min(targetTypeIds.Count, fallbackIndices.Count);
            if (count <= 0)
                return;

            Undo.IncrementCurrentGroup();
            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Unit Designer: Batch Edit");
            if (_registry != null)
                Undo.RecordObject(_registry, "Unit Designer: Batch Edit");

            for (int i = 0; i < count; i++)
            {
                var unit = ResolveTargetUnitForSafeEdit(targetTypeIds[i], fallbackIndices[i]);
                if (unit == null)
                    continue;

                if (_batchApplyRole)
                    unit.FindPropertyRelative("Role").enumValueIndex = (int)_batchRole;

                if (_batchApplyPrefab)
                    unit.FindPropertyRelative("Prefab").objectReferenceValue = _batchPrefab;

                if (_batchApplyAnimationDefaults)
                    SetAnimationDefaults(unit, _batchAnimationDuration, _batchAnimationDelay);
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        private bool PassesFilters(SerializedProperty unit, int index)
        {
            string typeId = GetString(unit, "TypeId");
            var prefab = GetObject<GameObject>(unit, "Prefab");
            UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);

            if (_roleFilter.HasValue && role != _roleFilter.Value)
                return false;

            if (_onlyProblems && ValidateUnit(unit, index) == null)
                return false;

            if (string.IsNullOrWhiteSpace(_search))
                return true;

            string query = _search.Trim();
            return Contains(typeId, query)
                || Contains(role.ToString(), query)
                || Contains(prefab != null ? prefab.name : string.Empty, query);
        }

        private void AddNewUnit()
        {
            EnsureConfigList();
            _registryObject.Update();

            int index = _configs.arraySize;
            _configs.InsertArrayElementAtIndex(index);
            var unit = _configs.GetArrayElementAtIndex(index);
            string id = GenerateUniqueId("unit");

            unit.FindPropertyRelative("TypeId").stringValue = id;
            unit.FindPropertyRelative("Role").enumValueIndex = (int)UnitRole.Worker;
            unit.FindPropertyRelative("BaseStamina").floatValue = 100f;
            unit.FindPropertyRelative("VisionRange").intValue = 3;
            unit.FindPropertyRelative("Prefab").objectReferenceValue = null;
            unit.FindPropertyRelative("StaminaRandomRange").vector2Value = new Vector2(-5f, 5f);
            SetCombatDefaults(unit);
            SetAnimationDefaults(unit);

            if (TryCommitRegistryChanges("Додавання юніта"))
            {
                _selectedIndex = index;
                SaveSelectedPreference();
            }
        }

        private void DuplicateSelectedUnit()
        {
            if (!HasSelectedUnit())
                return;

            _registryObject.Update();
            int source = _selectedIndex;
            int destination = _configs.arraySize;
            _configs.InsertArrayElementAtIndex(destination);

            var sourceUnit = _configs.GetArrayElementAtIndex(source);
            var duplicated = _configs.GetArrayElementAtIndex(destination);
            CopyUnitSerializedValues(sourceUnit, duplicated);

            string sourceId = GetString(sourceUnit, "TypeId");
            duplicated.FindPropertyRelative("TypeId").stringValue = GenerateUniqueId(string.IsNullOrWhiteSpace(sourceId) ? "unit" : sourceId);

            if (TryCommitRegistryChanges("Дублювання юніта"))
            {
                _selectedIndex = destination;
                SaveSelectedPreference();
            }
        }

        private void DeleteSelectedUnit()
        {
            if (!HasSelectedUnit())
                return;

            var unit = SelectedUnitProperty();
            string typeId = GetString(unit, "TypeId");
            if (!EditorUtility.DisplayDialog("Видалити юніта", $"Видалити '{typeId}' з UnitRegistrySO? Prefab asset не буде видалено.", "Видалити", "Скасувати"))
                return;

            _configs.DeleteArrayElementAtIndex(_selectedIndex);
            if (TryCommitRegistryChanges("Видалення юніта"))
            {
                _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _configs.arraySize - 1);
                SaveSelectedPreference();
            }
        }

        private void CreatePrefabForSelected(SerializedProperty unit, string typeId)
        {
            SpritePickerPopup.Show(spriteAsset =>
            {
                if (spriteAsset == null || !HasSelectedUnit())
                    return;

                _registryObject.Update();
                CreatePrefabForSelected(SelectedUnitProperty(), typeId, spriteAsset);
            });
        }

        private void CreatePrefabForSelected(SerializedProperty unit, string typeId, Sprite sprite)
        {
            if (sprite == null)
                return;

            EnsureFolder(UnitPrefabFolder);
            string safeId = SanitizeAssetName(string.IsNullOrWhiteSpace(typeId) ? GenerateUniqueId("unit") : typeId);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{UnitPrefabFolder}/{safeId}.prefab");
            var go = new GameObject(safeId);
            go.AddComponent<SpriteRenderer>().sprite = sprite;
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            DestroyImmediate(go);

            unit.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            if (string.IsNullOrWhiteSpace(typeId))
                unit.FindPropertyRelative("TypeId").stringValue = GenerateUniqueId(safeId);

            if (TryCommitRegistryChanges("Створення prefab для юніта"))
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void ApplyAutoRole(SerializedProperty unit)
        {
            string id = GetString(unit, "TypeId");
            var prefab = GetObject<GameObject>(unit, "Prefab");
            string source = $"{id} {prefab?.name}".ToLowerInvariant();
            var roleProp = unit.FindPropertyRelative("Role");

            if (source.Contains("soldier") || source.Contains("warrior") || source.Contains("archer") || source.Contains("guard") || source.Contains("military") || source.Contains("koz") || source.Contains("cossack"))
                roleProp.enumValueIndex = (int)UnitRole.Military;
            else
                roleProp.enumValueIndex = (int)UnitRole.Worker;
        }

        private void AutoSelectUnit()
        {
            _selectedIndex = -1;
            if (_configs == null || _configs.arraySize == 0)
                return;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (ValidateUnit(_configs.GetArrayElementAtIndex(i), i) != null)
                {
                    _selectedIndex = i;
                    return;
                }
            }

            _selectedIndex = 0;
        }

        private void SelectByTypeId(string typeId)
        {
            if (_configs == null || string.IsNullOrWhiteSpace(typeId))
                return;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (string.Equals(GetString(_configs.GetArrayElementAtIndex(i), "TypeId"), typeId, StringComparison.Ordinal))
                {
                    _selectedIndex = i;
                    return;
                }
            }
        }

        private void RefreshSerializedObject()
        {
            _registryObject = _registry != null ? new SerializedObject(_registry) : null;
            _configs = _registryObject?.FindProperty("Configs");
            EnsureConfigList();
            ClampSelection();
        }

        private void EnsureConfigList()
        {
            if (_registry == null)
                return;

            if (_registry.Configs == null)
            {
                Undo.RecordObject(_registry, "Create Unit Config List");
                _registry.Configs = new List<UnitClassConfig>();
                EditorUtility.SetDirty(_registry);
                _registryObject = new SerializedObject(_registry);
                _configs = _registryObject.FindProperty("Configs");
            }
        }

        private void ClampSelection()
        {
            if (_configs == null || _configs.arraySize == 0)
            {
                _selectedIndex = -1;
                return;
            }

            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _configs.arraySize - 1);
        }

        private bool HasSelectedUnit()
        {
            return _configs != null && _selectedIndex >= 0 && _selectedIndex < _configs.arraySize;
        }

        private SerializedProperty SelectedUnitProperty()
        {
            return HasSelectedUnit() ? _configs.GetArrayElementAtIndex(_selectedIndex) : null;
        }

        private string ValidateUnit(SerializedProperty unit, int index)
        {
            string id = GetString(unit, "TypeId");
            if (string.IsNullOrWhiteSpace(id))
                return "TypeId не може бути порожнім.";

            if (id.Contains("_"))
                return "TypeId містить '_' — символ зарезервований для instance ID.";

            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (i == index)
                    continue;

                string other = GetString(_configs.GetArrayElementAtIndex(i), "TypeId");
                if (string.Equals(id, other, StringComparison.OrdinalIgnoreCase))
                    return $"TypeId дублюється з записом #{i + 1}.";
            }

            if (GetObject<GameObject>(unit, "Prefab") == null)
                return "Prefab не задано.";

            if (GetFloat(unit, "BaseStamina") <= 0f)
                return "Base Stamina має бути більше 0.";

            if (GetInt(unit, "VisionRange") < 1)
                return "Vision Range має бути не менше 1.";

            string combatValidation = ValidateCombatFields(unit);
            if (combatValidation != null)
                return combatValidation;

            return null;
        }

        private bool TryCommitRegistryChanges(string source, bool blockOnCriticalValidation = true)
        {
            if (_registryObject == null)
                return false;

            if (!EditorRegistryWriteLock.IsUnlocked(RegistryLockKey))
            {
                _lastBlockedApplyReason = "Реєстр заблокований. Увімкніть Unlock для редагування.";
                return false;
            }

            if (_staleTracker.IsStale(_registry))
            {
                _lastBlockedApplyReason = "Дані застарілі: ассет змінено зовні.";
                return false;
            }

            string baselineSnapshot = SerializedDiffPreviewUtility.CaptureSnapshot(_registry);

            string criticalValidation = ValidateCriticalFieldsBeforeApply();
            if (!string.IsNullOrWhiteSpace(criticalValidation))
            {
                _lastBlockedApplyReason = criticalValidation;
                if (blockOnCriticalValidation)
                {
                    EditorUtility.DisplayDialog(
                        "Валідація перед збереженням",
                        $"Операція '{source}' скасована: {criticalValidation}",
                        "OK");
                }

                return false;
            }

            if (blockOnCriticalValidation && _diffBeforeApplyEnabled)
            {
                var changes = SerializedDiffPreviewUtility.BuildDiff(_registryObject, baselineSnapshot, maxItems: 220);
                if (!ConfirmDiffBeforeApply(source, changes))
                    return false;
            }

            bool changed = _registryObject.ApplyModifiedProperties();
            if (changed && _registry != null)
                EditorUtility.SetDirty(_registry);

            if (changed)
            {
                var changeRows = SerializedDiffPreviewUtility.BuildDiff(_registryObject, baselineSnapshot, maxItems: 120);
                EditorContentChangeLog.Write("UnitDesigner", source, _registry, changeRows);
                _staleTracker.Capture(_registry);
                _lastBlockedApplyReason = string.Empty;
            }

            return changed;
        }

        private static bool ConfirmDiffBeforeApply(string source, List<string> changes)
        {
            if (changes == null || changes.Count == 0)
                return true;

            const int previewLimit = 20;
            int shown = Mathf.Min(previewLimit, changes.Count);
            var previewLines = new List<string>(shown + 3);
            for (int i = 0; i < shown; i++)
                previewLines.Add($"- {changes[i]}");

            if (changes.Count > shown)
                previewLines.Add($"... ще {changes.Count - shown} змін.");

            string message =
                $"Операція: {source}\n" +
                $"Змінені поля: {changes.Count}\n\n" +
                string.Join("\n", previewLines);

            return EditorUtility.DisplayDialog(
                "Diff before apply",
                message,
                "Застосувати",
                "Скасувати");
        }

        private string ValidateCriticalFieldsBeforeApply()
        {
            if (_configs == null)
                return null;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _configs.arraySize; i++)
            {
                SerializedProperty unit = _configs.GetArrayElementAtIndex(i);
                if (unit == null)
                    continue;

                string id = GetString(unit, "TypeId");
                if (string.IsNullOrWhiteSpace(id))
                    return $"Запис #{i + 1}: TypeId не може бути порожнім.";

                if (id.Contains("_"))
                    return $"Запис '{id}': TypeId містить '_' (зарезервований символ).";

                if (!seen.Add(id))
                    return $"Дублікат TypeId: '{id}'.";

                if (GetFloat(unit, "BaseStamina") <= 0f)
                    return $"Запис '{id}': Base Stamina має бути більше 0.";

                if (GetInt(unit, "VisionRange") < 1)
                    return $"Запис '{id}': Vision Range має бути не менше 1.";

                string combatValidation = ValidateCombatFields(unit);
                if (!string.IsNullOrWhiteSpace(combatValidation))
                    return $"Запис '{id}': {combatValidation}";
            }

            return null;
        }

        private bool TryAutoFixUnit(SerializedProperty unit, int index)
        {
            bool fixed_something = false;

            // Fix 1: Empty TypeId
            string id = GetString(unit, "TypeId");
            if (string.IsNullOrWhiteSpace(id))
            {
                unit.FindPropertyRelative("TypeId").stringValue = GenerateUniqueId("unit");
                fixed_something = true;
            }

            // Fix 2: TypeId contains '_'
            id = GetString(unit, "TypeId");
            if (id.Contains("_"))
            {
                string cleaned = id.Replace("_", "-");
                if (!ContainsTypeId(cleaned) || cleaned == id)
                {
                    unit.FindPropertyRelative("TypeId").stringValue = cleaned;
                    fixed_something = true;
                }
                else
                {
                    // If cleaned ID is already taken, generate new unique one
                    unit.FindPropertyRelative("TypeId").stringValue = GenerateUniqueId(id);
                    fixed_something = true;
                }
            }

            // Fix 3: TypeId is duplicate - generate unique one
            id = GetString(unit, "TypeId");
            for (int i = 0; i < _configs.arraySize; i++)
            {
                if (i == index)
                    continue;

                string other = GetString(_configs.GetArrayElementAtIndex(i), "TypeId");
                if (string.Equals(id, other, StringComparison.OrdinalIgnoreCase))
                {
                    unit.FindPropertyRelative("TypeId").stringValue = GenerateUniqueId(id);
                    fixed_something = true;
                    break;
                }
            }

            // Fix 4: Base Stamina is 0 or negative
            var staminaProp = unit.FindPropertyRelative("BaseStamina");
            if (staminaProp != null && staminaProp.floatValue <= 0f)
            {
                staminaProp.floatValue = 100f;
                fixed_something = true;
            }

            // Fix 5: Vision Range is less than 1
            var visionProp = unit.FindPropertyRelative("VisionRange");
            if (visionProp != null && visionProp.intValue < 1)
            {
                visionProp.intValue = 3;
                fixed_something = true;
            }

            // Fix 6: Combat fields (if there's a method to fix them)
            if (TryAutoFixCombatFields(unit))
                fixed_something = true;

            if (fixed_something)
            {
                EditorApplication.delayCall += () => Repaint();
            }

            return fixed_something;
        }

        private bool TryAutoFixCombatFields(SerializedProperty unit)
        {
            // Combat field fixing is implemented in the partial class
            return AutoFixCombatFields(unit);
        }

        private bool AutoFixCombatFields(SerializedProperty unit)
        {
            bool fixed_something = false;

            // Fix 1: HitPoints < 1
            var hpProp = unit.FindPropertyRelative("HitPoints");
            if (hpProp != null && hpProp.intValue < 1)
            {
                hpProp.intValue = 100;
                fixed_something = true;
            }

            // Fix 2: BaseLevel < 1
            var levelProp = unit.FindPropertyRelative("BaseLevel");
            if (levelProp != null && levelProp.intValue < 1)
            {
                levelProp.intValue = 1;
                fixed_something = true;
            }

            // Fix 3: Military unit with no damage
            UnitRole role = (UnitRole)Mathf.Clamp(GetEnumIndex(unit, "Role"), 0, Enum.GetValues(typeof(UnitRole)).Length - 1);
            if (role == UnitRole.Military && GetCombatDamageTotal(unit) <= 0)
            {
                // Assign some default damage
                var cuttingDmg = unit.FindPropertyRelative("CuttingDamage");
                if (cuttingDmg != null)
                    cuttingDmg.intValue = 15;
                fixed_something = true;
            }

            return fixed_something;
        }

        private string GenerateUniqueId(string seed)
        {
            string baseId = ToKebabId(seed);
            if (string.IsNullOrWhiteSpace(baseId))
                baseId = "unit";

            string candidate = baseId;
            int suffix = 1;
            while (ContainsTypeId(candidate))
            {
                candidate = $"{baseId}-{suffix:00}";
                suffix++;
            }

            return candidate;
        }

        private bool ContainsTypeId(string typeId)
        {
            if (_configs == null)
                return false;

            for (int i = 0; i < _configs.arraySize; i++)
            {
                string id = GetString(_configs.GetArrayElementAtIndex(i), "TypeId");
                if (string.Equals(id, typeId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void CopyUnitSerializedValues(SerializedProperty source, SerializedProperty destination)
        {
            destination.FindPropertyRelative("TypeId").stringValue = source.FindPropertyRelative("TypeId").stringValue;
            destination.FindPropertyRelative("Role").enumValueIndex = source.FindPropertyRelative("Role").enumValueIndex;
            destination.FindPropertyRelative("BaseStamina").floatValue = source.FindPropertyRelative("BaseStamina").floatValue;
            destination.FindPropertyRelative("VisionRange").intValue = source.FindPropertyRelative("VisionRange").intValue;
            destination.FindPropertyRelative("Prefab").objectReferenceValue = source.FindPropertyRelative("Prefab").objectReferenceValue;
            destination.FindPropertyRelative("StaminaRandomRange").vector2Value = source.FindPropertyRelative("StaminaRandomRange").vector2Value;
            CopyCombatSerializedValues(source, destination);

            var sourceAnimation = source.FindPropertyRelative("AnimationSettings");
            var destinationAnimation = destination.FindPropertyRelative("AnimationSettings");
            if (sourceAnimation == null || destinationAnimation == null)
                return;

            CopyRelativeFloat(sourceAnimation, destinationAnimation, "MoveDurationPerTile");
            CopyRelativeFloat(sourceAnimation, destinationAnimation, "DelayOnTile");
        }

        private static void CopyRelativeFloat(SerializedProperty source, SerializedProperty destination, string propertyName)
        {
            var sourceProperty = source.FindPropertyRelative(propertyName);
            var destinationProperty = destination.FindPropertyRelative(propertyName);
            if (sourceProperty != null && destinationProperty != null)
                destinationProperty.floatValue = sourceProperty.floatValue;
        }

        private static void SetAnimationDefaults(SerializedProperty unit)
        {
            var animation = unit.FindPropertyRelative("AnimationSettings");
            animation?.FindPropertyRelative("MoveDurationPerTile")?.SetFloat(0.3f);
            animation?.FindPropertyRelative("DelayOnTile")?.SetFloat(0.05f);
        }

        private static void SetAnimationDefaults(SerializedProperty unit, float moveDurationPerTile, float delayOnTile)
        {
            var animation = unit.FindPropertyRelative("AnimationSettings");
            animation?.FindPropertyRelative("MoveDurationPerTile")?.SetFloat(Mathf.Max(0.02f, moveDurationPerTile));
            animation?.FindPropertyRelative("DelayOnTile")?.SetFloat(Mathf.Max(0f, delayOnTile));
        }

        private static Sprite ResolveSprite(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var renderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
            return renderer != null ? renderer.sprite : null;
        }

        private static void DrawSpriteOrPrefab(Rect rect, Sprite sprite, GameObject prefab, bool framed)
        {
            if (framed)
                DrawPanelBackground(rect, EditorGUIUtility.isProSkin ? new Color(0.09f, 0.1f, 0.11f) : new Color(0.72f, 0.75f, 0.78f));

            Rect padded = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f);
            if (sprite != null && sprite.texture != null)
            {
                Rect uv = new Rect(
                    sprite.textureRect.x / sprite.texture.width,
                    sprite.textureRect.y / sprite.texture.height,
                    sprite.textureRect.width / sprite.texture.width,
                    sprite.textureRect.height / sprite.texture.height);
                GUI.DrawTextureWithTexCoords(padded, sprite.texture, uv, true);
                return;
            }

            Texture preview = prefab != null ? AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab) : null;
            if (preview != null)
            {
                GUI.DrawTexture(padded, preview, ScaleMode.ScaleToFit, true);
                return;
            }

            GUI.Label(padded, IconContent("d_AvatarSelector", "?", ""), CenterTitleStyle());
        }

        private static void DrawPreviewGrid(Rect rect, float step)
        {
            Handles.BeginGUI();
            Handles.color = GridLine;
            for (float x = rect.x; x < rect.xMax; x += step)
                Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));

            for (float y = rect.y; y < rect.yMax; y += step)
                Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));

            Handles.EndGUI();
        }

        private static void DrawLine(Vector2 from, Vector2 to, Color color, float width)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAPolyLine(width, from, to);
            Handles.EndGUI();
        }

        private static void DrawNode(Vector2 center)
        {
            Rect rect = new Rect(center.x - 4f, center.y - 4f, 8f, 8f);
            EditorGUI.DrawRect(rect, Accent);
        }

        private static void DrawMetricBar(string label, float value, float min, float max, Color color, string tooltip)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y, 90f, rect.height), new GUIContent(label, tooltip), EditorStyles.miniLabel);

            Rect bar = new Rect(rect.x + 94f, rect.y + 7f, rect.width - 150f, 12f);
            DrawPanelBackground(bar, EditorGUIUtility.isProSkin ? new Color(0.07f, 0.08f, 0.09f) : new Color(0.74f, 0.76f, 0.78f));
            float t = Mathf.InverseLerp(min, max, value);
            EditorGUI.DrawRect(new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(t), bar.height), color);
            GUI.Label(new Rect(bar.xMax + 8f, rect.y, 48f, rect.height), value.ToString("0.##", CultureInfo.InvariantCulture), EditorStyles.miniLabel);
        }

        private static void DrawStaminaRangePreview(float baseValue, Vector2 randomRange)
        {
            float min = Mathf.Max(0f, baseValue + Mathf.Min(randomRange.x, randomRange.y));
            float max = Mathf.Max(0f, baseValue + Mathf.Max(randomRange.x, randomRange.y));
            Rect rect = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y, 102f, rect.height), new GUIContent("Діапазон", "Фактичний стартовий діапазон стаміни."), EditorStyles.miniLabel);
            Rect bar = new Rect(rect.x + 106f, rect.y + 8f, rect.width - 116f, 12f);
            DrawPanelBackground(bar, EditorGUIUtility.isProSkin ? new Color(0.07f, 0.08f, 0.09f) : new Color(0.74f, 0.76f, 0.78f));
            float left = Mathf.InverseLerp(0f, 300f, min);
            float right = Mathf.InverseLerp(0f, 300f, max);
            EditorGUI.DrawRect(new Rect(bar.x + bar.width * left, bar.y, bar.width * Mathf.Max(0.02f, right - left), bar.height), Good);
            GUI.Label(new Rect(bar.x, bar.yMax + 2f, bar.width, 14f), $"{min:0.#} .. {max:0.#}", EditorStyles.centeredGreyMiniLabel);
        }

        private static void DrawPanelHeader(string text, GUIContent icon)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 30f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, Accent * 0.72f);
            if (icon != null && icon.image != null)
                GUI.DrawTexture(new Rect(rect.x + 8f, rect.y + 6f, 18f, 18f), icon.image, ScaleMode.ScaleToFit);

            GUI.Label(new Rect(rect.x + 32f, rect.y, rect.width - 40f, rect.height), text, HeaderTextStyle());
        }

        private static void BeginSection(string title, string iconName, string tooltip)
        {
            EditorGUILayout.BeginVertical(SectionStyle());
            EditorGUILayout.BeginHorizontal();
            var icon = IconContent(iconName, string.Empty, tooltip);
            if (icon.image != null)
                GUILayout.Label(icon.image, GUILayout.Width(18f), GUILayout.Height(18f));
            GUILayout.Label(new GUIContent(title, tooltip), SectionHeaderStyle());
            EditorGUILayout.EndHorizontal();
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }

        private static void DrawMiniCounter(string label, int value, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(52f, 32f, GUILayout.ExpandWidth(true));
            DrawPanelBackground(rect, color * 0.45f);
            GUI.Label(new Rect(rect.x, rect.y + 3f, rect.width, 14f), value.ToString(CultureInfo.InvariantCulture), CounterValueStyle());
            GUI.Label(new Rect(rect.x, rect.y + 17f, rect.width, 12f), label, CounterLabelStyle());
        }

        private static void DrawBadge(Rect rect, string text, Color color)
        {
            EditorWindowSharedUI.DrawBadgeRect(rect, text, color * 0.88f);
        }

        private static void DrawPanelBackground(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), new Color(1f, 1f, 1f, 0.08f));
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), new Color(0f, 0f, 0f, 0.25f));
        }

        private void DrawStatusBar()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 22f, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.12f, 0.12f, 0.12f) : new Color(0.78f, 0.78f, 0.78f));
            string registryPath = _registry != null ? AssetDatabase.GetAssetPath(_registry) : "registry не вибрано";
            string selected = HasSelectedUnit() ? GetString(SelectedUnitProperty(), "TypeId") : "немає вибору";

            string validationInfo = string.IsNullOrWhiteSpace(_lastBlockedApplyReason)
                ? "validation: OK"
                : "validation: BLOCKED";
            bool stale = _staleTracker.IsStale(_registry);
            string lockInfo = EditorRegistryWriteLock.IsUnlocked(RegistryLockKey) ? "lock: UNLOCKED" : "lock: READONLY";
            string staleInfo = stale ? "stale: YES" : "stale: no";
            string perfInfo = _perfProfiler.BuildSummary();
            GUI.Label(new Rect(rect.x + 8f, rect.y + 2f, rect.width - 16f, rect.height - 4f), $"{registryPath}  |  вибрано: {selected}  |  {validationInfo}  |  {lockInfo}  |  {staleInfo}  |  {perfInfo}", EditorStyles.miniLabel);
        }

        private void LoadRegistryPreference()
        {
            _registry = MoyvaProjectEditorContext.Get<UnitRegistrySO>();
            if (_registry != null)
                return;

            string guid = EditorPrefs.GetString(RegistryGuidPrefsKey, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrWhiteSpace(path))
                _registry = AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(path);
        }

        private void SaveRegistryPreference()
        {
            MoyvaProjectEditorContext.Set(_registry);
            if (_registry == null)
            {
                EditorPrefs.DeleteKey(RegistryGuidPrefsKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(_registry);
            string guid = AssetDatabase.AssetPathToGUID(path);
            if (!string.IsNullOrWhiteSpace(guid))
                EditorPrefs.SetString(RegistryGuidPrefsKey, guid);
        }

        private void SaveSelectedPreference()
        {
            if (!HasSelectedUnit())
                return;

            EditorPrefs.SetString(SelectedTypePrefsKey, GetString(SelectedUnitProperty(), "TypeId"));
        }

        private void LoadLayoutPreferences()
        {
            _unitListPanelWidth = EditorPrefs.GetFloat(UnitListWidthPrefsKey, DefaultUnitListPanelWidth);
            _unitDetailsPanelWidth = EditorPrefs.GetFloat(UnitDetailsWidthPrefsKey, DefaultUnitDetailsPanelWidth);
            _generatorListPanelWidth = EditorPrefs.GetFloat(GeneratorListWidthPrefsKey, DefaultUnitListPanelWidth);
            _generatorSettingsPanelWidth = EditorPrefs.GetFloat(GeneratorSettingsWidthPrefsKey, DefaultGeneratorSettingsPanelWidth);
            _combatListPanelWidth = EditorPrefs.GetFloat(CombatListWidthPrefsKey, DefaultUnitListPanelWidth);
            _combatRulesPanelWidth = EditorPrefs.GetFloat(CombatRulesWidthPrefsKey, DefaultCombatRulesPanelWidth);
            ClampLayoutWidths();
        }

        private void SaveLayoutPreferences()
        {
            ClampLayoutWidths();
            EditorPrefs.SetFloat(UnitListWidthPrefsKey, _unitListPanelWidth);
            EditorPrefs.SetFloat(UnitDetailsWidthPrefsKey, _unitDetailsPanelWidth);
            EditorPrefs.SetFloat(GeneratorListWidthPrefsKey, _generatorListPanelWidth);
            EditorPrefs.SetFloat(GeneratorSettingsWidthPrefsKey, _generatorSettingsPanelWidth);
            EditorPrefs.SetFloat(CombatListWidthPrefsKey, _combatListPanelWidth);
            EditorPrefs.SetFloat(CombatRulesWidthPrefsKey, _combatRulesPanelWidth);
        }

        private void ResetLayoutPreferences()
        {
            _unitListPanelWidth = DefaultUnitListPanelWidth;
            _unitDetailsPanelWidth = DefaultUnitDetailsPanelWidth;
            _generatorListPanelWidth = DefaultUnitListPanelWidth;
            _generatorSettingsPanelWidth = DefaultGeneratorSettingsPanelWidth;
            _combatListPanelWidth = DefaultUnitListPanelWidth;
            _combatRulesPanelWidth = DefaultCombatRulesPanelWidth;
            SaveLayoutPreferences();
            Repaint();
        }

        private void ClampLayoutWidths()
        {
            _unitListPanelWidth = Mathf.Clamp(_unitListPanelWidth, MinUnitListPanelWidth, ResolveMaxPanelWidth(MinDetailsPanelWidth + MinPreviewPanelWidth));
            _unitDetailsPanelWidth = Mathf.Clamp(_unitDetailsPanelWidth, MinDetailsPanelWidth, ResolveMaxPanelWidth(MinUnitListPanelWidth + MinPreviewPanelWidth));
            _generatorListPanelWidth = Mathf.Clamp(_generatorListPanelWidth, MinUnitListPanelWidth, ResolveMaxPanelWidth(MinDetailsPanelWidth + MinPreviewPanelWidth));
            _generatorSettingsPanelWidth = Mathf.Clamp(_generatorSettingsPanelWidth, MinDetailsPanelWidth, ResolveMaxPanelWidth(MinUnitListPanelWidth + MinPreviewPanelWidth));
            _combatListPanelWidth = Mathf.Clamp(_combatListPanelWidth, MinUnitListPanelWidth, ResolveMaxPanelWidth(MinDetailsPanelWidth + MinPreviewPanelWidth));
            _combatRulesPanelWidth = Mathf.Clamp(_combatRulesPanelWidth, MinDetailsPanelWidth, ResolveMaxPanelWidth(MinUnitListPanelWidth + MinPreviewPanelWidth));
        }

        private float ResolveMaxPanelWidth(float reservedWidth)
        {
            return Mathf.Max(MinUnitListPanelWidth, position.width - reservedWidth - SplitterWidth * 2f - 36f);
        }

        private void DrawColumnSplitter(ref float width, float minWidth, float maxWidth, string preferenceKey)
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
                        width = Mathf.Clamp(width + current.delta.x, minWidth, Mathf.Max(minWidth, maxWidth));
                        Repaint();
                        current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                    {
                        GUIUtility.hotControl = 0;
                        EditorPrefs.SetFloat(preferenceKey, width);
                        current.Use();
                    }
                    break;
            }

            Color color = EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.16f)
                : new Color(0f, 0f, 0f, 0.18f);
            EditorGUI.DrawRect(new Rect(rect.center.x - 1f, rect.y + 8f, 2f, Mathf.Max(0f, rect.height - 16f)), color);
        }

        private void CreateRegistryAsset()
        {
            EnsureFolder("Assets/Moyva/SO/Units");
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Moyva/SO/Units/UnitRegistry.asset");
            var asset = CreateInstance<UnitRegistrySO>();
            asset.Configs = new List<UnitClassConfig>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _registry = asset;
            RefreshSerializedObject();
            SaveRegistryPreference();
            EditorGUIUtility.PingObject(asset);
        }

        private static UnitRegistrySO FindFirstRegistry()
        {
            string[] guids = AssetDatabase.FindAssets("t:UnitRegistrySO");
            if (guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.Ordinal);
            return AssetDatabase.LoadAssetAtPath<UnitRegistrySO>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Replace('\\', '/').Trim('/').Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
                return;

            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private static string ToKebabId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string value = input.Trim();
            value = Regex.Replace(value, "([a-z0-9])([A-Z])", "$1-$2");
            value = Regex.Replace(value, "[^A-Za-z0-9-]+", "-");
            value = Regex.Replace(value, "-+", "-").Trim('-').ToLowerInvariant();
            return value;
        }

        private static string SanitizeAssetName(string input)
        {
            string value = ToKebabId(input);
            return string.IsNullOrWhiteSpace(value) ? "unit" : value;
        }

        private static bool Contains(string source, string query)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetString(SerializedProperty property, string relativeName)
            => property?.FindPropertyRelative(relativeName)?.stringValue ?? string.Empty;

        private static float GetFloat(SerializedProperty property, string relativeName)
            => property?.FindPropertyRelative(relativeName)?.floatValue ?? 0f;

        private static int GetInt(SerializedProperty property, string relativeName)
            => property?.FindPropertyRelative(relativeName)?.intValue ?? 0;

        private static int GetEnumIndex(SerializedProperty property, string relativeName)
            => property?.FindPropertyRelative(relativeName)?.enumValueIndex ?? 0;

        private static Vector2 GetVector2(SerializedProperty property, string relativeName)
            => property?.FindPropertyRelative(relativeName)?.vector2Value ?? Vector2.zero;

        private static T GetObject<T>(SerializedProperty property, string relativeName) where T : UnityEngine.Object
            => property?.FindPropertyRelative(relativeName)?.objectReferenceValue as T;

        private static float GetNestedFloat(SerializedProperty property, string containerName, string relativeName, float fallback)
        {
            var nested = property?.FindPropertyRelative(containerName)?.FindPropertyRelative(relativeName);
            return nested != null ? nested.floatValue : fallback;
        }

        private static GUIContent IconContent(string iconName, string fallback, string tooltip)
        {
            GUIContent content = !string.IsNullOrWhiteSpace(iconName) ? EditorGUIUtility.IconContent(iconName) : null;
            if (content == null || content.image == null)
                return new GUIContent(fallback, tooltip);

            return new GUIContent(string.IsNullOrEmpty(fallback) ? content.text : fallback, content.image, tooltip);
        }

        private static GUIStyle PanelStyle()
        {
            return EditorWindowSharedUI.PanelStyle(
                padding: new RectOffset(8, 8, 8, 8),
                margin: new RectOffset(6, 6, 6, 6));
        }

        private static GUIStyle SectionStyle()
        {
            return new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 4, 4),
            };
        }

        private static GUIStyle HeaderTextStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle RowTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                clipping = TextClipping.Ellipsis,
            };
        }

        private static GUIStyle SelectedRowTitleStyle()
        {
            var style = RowTitleStyle();
            style.normal.textColor = Color.white;
            return style;
        }

        private static GUIStyle PreviewTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle SectionHeaderStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
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

        private static GUIStyle CounterValueStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle CounterLabelStyle()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
            };
        }

        private static GUIStyle CenterHeaderStyle()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
            };
        }

        private static GUIStyle CenterTitleStyle()
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
            };
        }

        private static GUIStyle CenterMiniStyle()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private sealed class SpritePickerPopup : EditorWindow
        {
            private static Action<Sprite> _callback;
            private readonly List<SpriteAssetEntry> _sprites = new List<SpriteAssetEntry>();
            private Vector2 _scroll;
            private string _search = string.Empty;

            public static void Show(Action<Sprite> callback)
            {
                _callback = callback;
                var window = CreateInstance<SpritePickerPopup>();
                window.titleContent = new GUIContent("Sprite Picker");
                window.position = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 420f, 500f);
                window.ShowUtility();
            }

            private void OnEnable()
            {
                RefreshSprites();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField("Оберіть sprite для нового prefab", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    _search = EditorGUILayout.TextField("Пошук", _search);
                    if (GUILayout.Button("Оновити", GUILayout.Width(72f)))
                        RefreshSprites();
                }

                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                for (int i = 0; i < _sprites.Count; i++)
                {
                    var entry = _sprites[i];
                    var sprite = entry.Sprite;
                    if (sprite == null)
                        continue;

                    if (!string.IsNullOrWhiteSpace(_search) && !Contains(sprite.name, _search) && !Contains(entry.Path, _search))
                        continue;

                    Rect row = GUILayoutUtility.GetRect(0f, 38f, GUILayout.ExpandWidth(true));
                    DrawSpriteOrPrefab(new Rect(row.x, row.y + 3f, 32f, 32f), sprite, null, true);
                    GUI.Label(new Rect(row.x + 40f, row.y + 2f, row.width - 110f, 18f), sprite.name, EditorStyles.boldLabel);
                    GUI.Label(new Rect(row.x + 40f, row.y + 19f, row.width - 110f, 16f), entry.Path, EditorStyles.miniLabel);
                    if (GUI.Button(new Rect(row.xMax - 64f, row.y + 7f, 60f, 22f), "Обрати"))
                    {
                        _callback?.Invoke(sprite);
                        Close();
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            private void RefreshSprites()
            {
                _sprites.Clear();
                string[] guids = AssetDatabase.FindAssets("t:Sprite");
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite != null && SpriteImportDragDropPolicy.IsAllowedSprite(sprite, out _))
                        _sprites.Add(new SpriteAssetEntry(sprite, path));
                }

                _sprites.Sort(CompareSpriteEntries);
            }

            private static int CompareSpriteEntries(SpriteAssetEntry left, SpriteAssetEntry right)
            {
                int nameComparison = string.Compare(left.Sprite != null ? left.Sprite.name : string.Empty, right.Sprite != null ? right.Sprite.name : string.Empty, StringComparison.OrdinalIgnoreCase);
                return nameComparison != 0
                    ? nameComparison
                    : string.Compare(left.Path, right.Path, StringComparison.OrdinalIgnoreCase);
            }

            private readonly struct SpriteAssetEntry
            {
                public SpriteAssetEntry(Sprite sprite, string path)
                {
                    Sprite = sprite;
                    Path = path ?? string.Empty;
                }

                public Sprite Sprite { get; }
                public string Path { get; }
            }
        }
    }

    internal static class SerializedPropertyFloatExtensions
    {
        public static void SetFloat(this SerializedProperty property, float value)
        {
            if (property != null)
                property.floatValue = value;
        }
    }
}