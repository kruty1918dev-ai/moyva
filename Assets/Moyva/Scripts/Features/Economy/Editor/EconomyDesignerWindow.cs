using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomyDesignerWindow : EditorWindow
    {
        private enum Tab
        {
            Resources = 0,
            OverridableParameters = 1,
            SettlementRules = 2,
            PopulationRules = 3,
            WorkforceRules = 4,
            ProductionRules = 5,
            StorageRules = 6,
            CaravanRules = 7,
            MarketRules = 8,
            ConsumptionNeedsRules = 9,
            DeathMortalityRules = 10,
            BuildingRules = 11,
            AiExtensibilityRules = 12,
            Validation = 13,
            Simulation = 14,
            EntitiesSettings = 15,
        }

        private enum TabGroup
        {
            Дані = 0,
            Правила = 1,
            Інструменти = 2,
            Сутності = 3,
        }

        private enum CivilianBuildingType
        {
            Regular = 0,
            TownHall = 1,
            Castle = 2,
        }

        private static readonly string[] TabGroupLabels =
        {
            "Дані",
            "Правила",
            "Інструменти",
            "Сутності",
        };

        private static readonly Dictionary<Tab, string> TabLabels = new Dictionary<Tab, string>
        {
            { Tab.Resources, "Ресурси" },
            { Tab.OverridableParameters, "Оверайди" },
            { Tab.SettlementRules, "Поселення" },
            { Tab.PopulationRules, "Населення" },
            { Tab.WorkforceRules, "Робоча сила" },
            { Tab.ProductionRules, "Виробництво" },
            { Tab.StorageRules, "Склади" },
            { Tab.CaravanRules, "Каравани" },
            { Tab.MarketRules, "Ринок" },
            { Tab.ConsumptionNeedsRules, "Споживання" },
            { Tab.DeathMortalityRules, "Смертність" },
            { Tab.BuildingRules, "Будівлі" },
            { Tab.AiExtensibilityRules, "ШІ" },
            { Tab.Validation, "Валідація" },
            { Tab.Simulation, "Симуляція" },
            { Tab.EntitiesSettings, "Налаштування сутностей" },
        };

        private static readonly Dictionary<TabGroup, Tab[]> TabsByGroup = new Dictionary<TabGroup, Tab[]>
        {
            { TabGroup.Дані, new[] { Tab.Resources, Tab.OverridableParameters } },
            {
                TabGroup.Правила,
                new[]
                {
                    Tab.SettlementRules,
                    Tab.PopulationRules,
                    Tab.WorkforceRules,
                    Tab.ProductionRules,
                    Tab.StorageRules,
                    Tab.CaravanRules,
                    Tab.MarketRules,
                    Tab.ConsumptionNeedsRules,
                    Tab.DeathMortalityRules,
                    Tab.BuildingRules,
                    Tab.AiExtensibilityRules,
                }
            },
            { TabGroup.Інструменти, new[] { Tab.Validation, Tab.Simulation } },
            { TabGroup.Сутності, new[] { Tab.EntitiesSettings } },
        };

        private static readonly (string Label, string PropertyName)[] BulkCategoryDefs =
        {
            ("Поселення",    "_settlements"),
            ("Ресурси",      "_resources"),
            ("Склади",       "_warehousePolicies"),
            ("Виробництво",  "_productionProfiles"),
            ("Каравани",     "_caravanTemplates"),
            ("ШІ Правила",   "_aiRuleProfiles"),
        };

        private static readonly string[] BulkCategoryLabels =
        {
            "Всі",
            "Поселення",
            "Ресурси",
            "Склади",
            "Виробництво",
            "Каравани",
            "ШІ Правила",
        };

        // ═══════════════════════════════════════════════════════
        //  ТУЛТІПИ — детальні підказки українською
        // ═══════════════════════════════════════════════════════

        private static class Tips
        {
            // Головне вікно
            public const string WindowHeader =
                "Центральний редактор економіки (Economy Hub).\n" +
                "Тут налаштовується ВСЕ: поселення, ресурси, склади, виробництво, каравани, AI-правила та загальні правила.\n" +
                "Жоден економічний параметр не зашитий у код — все читається з конфігів цього хабу.";

            public const string DatabaseField =
                "Головний ScriptableObject-каталог економіки.\n" +
                "ЦЕ НЕ один ресурс: каталог містить списки ресурсів, поселень, складських політик, виробничих профілів, караванних шаблонів та AI-правил.\n" +
                "Приклад: створіть файл 'EconomyCatalog.asset' в папці Assets/Data/Economy.";

            public const string AutoFindBtn =
                "Шукає перший EconomyDatabaseSO у проєкті.\n" +
                "Корисно коли не пам'ятаєте де файл.\n" +
                "Приклад: натисніть після імпорту нового пакету з готовим конфігом.";

            public const string CreateDatabaseBtn =
                "Створює новий порожній EconomyDatabaseSO (каталог економіки).\n" +
                "Версія схеми буде встановлена на актуальну.\n" +
                "Приклад: для нового проєкту — створіть каталог в Assets/Data/Economy/EconomyCatalog.asset.";

            // Вкладки
            public const string SettlementsTab =
                "Поселення — центральна одиниця економіки.\n" +
                "Кожне поселення прив'язане до ратуші (центральна будівля).\n" +
                "Параметри: ID, тип (Village/Castle), центральна будівля, радіус.\n" +
                "Приклад: 'village-01', тип Village, центр 'town-hall', радіус 15.";

            public const string ResourcesTab =
                "Ресурси — все що виробляється, споживається і торгується.\n" +
                "Параметри: ID, назва, категорія (Food/Materials), іконка, ліміт стеку.\n" +
                "Приклад: 'wheat', назва 'Пшениця', категорія Food, ліміт 999.";

            public const string WarehousesTab =
                "Складські політики визначають правила зберігання.\n" +
                "Тип складу: Амбар (їжа) або Склад (матеріали).\n" +
                "Для кожного ресурсу можна задати пріоритет і резерв.\n" +
                "Приклад: Амбар з резервом 50 одиниць пшениці.";

            public const string ProductionTab =
                "Виробничі профілі описують рецепти будівель.\n" +
                "Параметри: ID будівлі, рецепт, тривалість циклу, кількість виходу.\n" +
                "Будівля бере input з пулу поселення і кладе output назад.\n" +
                "Приклад: 'sawmill' рецепт 'wood-to-planks', цикл 60с, вихід 2.";

            public const string CaravansTab =
                "Караванні шаблони — які ресурси може возити караван.\n" +
                "Ліміт: 1 караван на 1 поселення (налаштовується в Хаб Правил).\n" +
                "Параметри: дозволені ресурси, ємність, пріоритет.\n" +
                "Приклад: караван 'trade-caravan-01', ємність 100, дозволені: wood, stone.";

            public const string AiRulesTab =
                "Профілі AI-торгівлі та автоматизації.\n" +
                "Наразі AI-автономна економіка вимкнена — це точки розширюваності.\n" +
                "Параметри: пороги нестачі/надлишку, консервативні витрати.\n" +
                "Приклад: при запасі їжі <30 — AI не продає їжу.";

            public const string RulesHubTab =
                "Хаб Правил — ВСІ економічні параметри (правила 1-61).\n" +
                "11 груп: Поселення, Населення, Робоча сила, Виробництво,\n" +
                "Склади, Каравани, Ринок, Споживання, Смертність, Будівлі, ШІ.\n" +
                "Приклад: MaxSettlements=3, MinTownHallDistance=25, вага голоду=0.015.";

            public const string ValidationTab =
                "Перевірка цілісності даних та діапазонів.\n" +
                "Шукає: дублікати ID, порожні посилання, некоректні числа.\n" +
                "'Виправити' — безпечне автоматичне виправлення типових помилок.\n" +
                "Приклад: якщо два ресурси мають ID 'wood' — буде помилка дублікату.";

            public const string OverridableTab =
                "Оверайди параметрів для налаштувань світу.\n" +
                "Тут визначається, які параметри можна змінювати перед стартом гри.\n" +
                "Приклад: дозволити змінювати лише Population і Market, а критичні параметри лишити фіксованими.";

            public const string RulesTemplateField =
                "Базовий шаблон економічних правил.\n" +
                "Всі категорії читають параметри саме з цього ассета.\n" +
                "Якщо список порожній — заповніть дефолтними правилами.";

            public const string AllowAllOverridesBtn =
                "Увімкнути можливість оверайду для всіх параметрів одразу.\n" +
                "Зручно для швидких тестів балансу або sandbox-режиму.";

            public const string DisallowAllOverridesBtn =
                "Вимкнути можливість оверайду для всіх параметрів одразу.\n" +
                "Після цього можна вручну увімкнути лише потрібні параметри.";

            public const string FillDefaultRulesBtn =
                "Заповнити список параметрів дефолтними правилами хабу.\n" +
                "Використовуйте, якщо шаблон створено порожнім або очищено.";

            public const string SimulationTab =
                "Детерміністична симуляція виробництва.\n" +
                "Обирається поселення + профілі → обчислюється вихід за N хвилин.\n" +
                "Це preview, не реальна гра — просто оцінка балансу.\n" +
                "Приклад: за 10 хв з лісопилки вийде ~20 дощок.";

            public const string BulkDeleteTab =
                "Мультивибір — оберіть кілька елементів зі списків бази та видаліть їх одразу.\n" +
                "Фільтр за категорією (Поселення, Ресурси тощо) або покажіть усі одночасно.\n" +
                "УВАГА: .asset-файли НЕ видаляються з диску — лише прибирається посилання з бази.\n" +
                "Приклад: прибрати 10 застарілих виробничих профілів за один клік.";

            // Entity operations
            public const string CreateEntityBtn =
                "Створити новий ScriptableObject-ассет цього типу.\n" +
                "Оберіть папку, задайте ім'я — ассет буде додано в список.\n" +
                "Приклад: 'EconomyResourceDefinition' → оберіть Assets/Data/Economy/Resources/.";

            public const string AddSelectedBtn =
                "Додати вже існуючий ресурс у базу через меню вибору.\n" +
                "Показуються лише ассети, які ще НЕ додані в каталог.\n" +
                "Приклад: натисніть 'Додати', оберіть 'WheatResource.asset'.";

            public const string RemoveEntityBtn =
                "Видалити обраний елемент.\n" +
                "Для ресурсів: видаляється і посилання з бази, і сам .asset файл.\n" +
                "Для інших вкладок: видаляється лише посилання з бази.";

            public const string PingBtn =
                "Підсвітити ассет у вікні Project.\n" +
                "Допомагає швидко знайти файл на диску.\n" +
                "Приклад: натисніть щоб побачити де лежить EconomyDatabase.asset.";

            // Rules Hub
            public const string RulesConfigField =
                "ScriptableObject з усіма правилами економіки.\n" +
                "Містить 11 груп: від поселень до AI-розширюваності.\n" +
                "Runtime-сервіси зчитують лише цей конфіг — жодних hardcoded значень.\n" +
                "Приклад: EconomyRulesConfig.asset з дефолтами для балансу.";

            public const string CreateRulesConfigBtn =
                "Створити новий EconomyRulesConfigSO з дефолтними значеннями.\n" +
                "Всі групи правил будуть заповнені стартовими пресетами.\n" +
                "Приклад: MaxSettlements=3, FoodDecayPerTurn=0.02, bazova ціна Wood=5.";

            // Validation
            public const string RunValidationBtn =
                "Запустити перевірку всіх даних економіки.\n" +
                "Шукає: дублікати ID, нульові посилання, невалідні діапазони,\n" +
                "відсутні конфіги правил та проблеми крос-зв'язків.\n" +
                "Приклад: після масових змін — натисніть для перевірки цілісності.";

            public const string FixIssuesBtn =
                "Автоматично виправити типові помилки.\n" +
                "Безпечні операції: trim пробілів, clamp чисел у допустимі діапазони,\n" +
                "виправлення пріоритетів. НЕ видаляє дані.\n" +
                "Приклад: якщо MaxSettlements<1 → буде встановлено 1.";

            public const string RunMigrationBtn =
                "Мігрувати схему даних до нової версії.\n" +
                "При оновленні структури ScriptableObject — мігрує поля.\n" +
                "Приклад: v1→v2 може додати нові поля з дефолтними значеннями.";

            // Simulation
            public const string SimSettlementField =
                "Поселення для якого запускається симуляція.\n" +
                "Оберіть з наявних EconomySettlementDefinition.\n" +
                "Приклад: 'village-01' щоб оцінити його виробничу потужність.";

            public const string SimDurationField =
                "Тривалість симуляції у хвилинах ігрового часу.\n" +
                "Більша тривалість — точніша оцінка довготривалого балансу.\n" +
                "Приклад: 10 хв для швидкої перевірки, 60 хв для аналізу балансу.";

            public const string SimSelectDefaultsBtn =
                "Обрати лише профілі помічені як 'активні за замовчуванням'.\n" +
                "Зручно для перевірки стандартної конфігурації.\n" +
                "Приклад: якщо IsActiveByDefault=true — профіль буде обрано.";

            public const string SimRunBtn =
                "Запустити детерміністичний розрахунок.\n" +
                "Використовує виробничі профілі та тривалість для оцінки виходу.\n" +
                "Результат: список ресурсів з обчисленою кількістю.\n" +
                "Приклад: sawmill за 10хв → Planks: 20.";

            // Cross-system
            public const string OpenRegistryHubBtn =
                "Відкрити Registry Hub для реєстрації юнітів, будівель, тайлів.\n" +
                "Будівля спочатку реєструється там, а потім прив'язується тут.\n" +
                "Приклад: створіть 'sawmill' в Registry Hub → потім додайте виробничий профіль тут.";

            public const string OpenRegistryBuildingsBtn =
                "Перейти до вкладки 'Будівлі' в Registry Hub.\n" +
                "Там можна побачити та відредагувати BuildingDefinition.\n" +
                "Приклад: перевірити що будівля 'sawmill' існує перед додаванням профілю.";
        }

        private readonly Dictionary<Tab, string> _searchByTab = new Dictionary<Tab, string>();
        private readonly Dictionary<Tab, UnityEngine.Object> _selectedByTab = new Dictionary<Tab, UnityEngine.Object>();
        private readonly Dictionary<Tab, HashSet<UnityEngine.Object>> _multiSelectedByTab = new Dictionary<Tab, HashSet<UnityEngine.Object>>();
        private readonly Dictionary<UnityEngine.Object, UnityEditor.Editor> _cachedEditors = new Dictionary<UnityEngine.Object, UnityEditor.Editor>();

        private readonly EconomyValidationService _validationService = new EconomyValidationService();
        private readonly EconomyAutoFixService _autoFixService = new EconomyAutoFixService();
        private readonly EconomySimulationService _simulationService = new EconomySimulationService();
        private readonly EconomyDataMigrationService _migrationService = new EconomyDataMigrationService();

        private EconomyDatabaseSO _database;
        private SerializedObject _databaseSo;
        private EconomyRulesConfiguration _rulesConfiguration;
        private SerializedObject _rulesConfigurationSo;

        private Tab _tab;
        private TabGroup _tabGroup;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _validationScroll;
        private Vector2 _simulationScroll;
        private Vector2 _rulesScroll;
        private Vector2 _entitiesLeftScroll;
        private Vector2 _entitiesRightScroll;

        private int _entitiesSubTab;
        private int _selectedBuildingEntityIndex = -1;
        private int _selectedUnitEntityIndex = -1;
        private string _entitiesBuildingSearch = string.Empty;
        private string _entitiesUnitSearch = string.Empty;

        private BuildingRegistrySO _buildingRegistry;
        private SerializedObject _buildingRegistrySo;
        private UnitRegistrySO _unitRegistry;
        private SerializedObject _unitRegistrySo;

        private Kruty1918.Moyva.Grid.API.TileRegistrySO _tileRegistry;
        private List<EconomyResourceDefinition> _cachedResources = new List<EconomyResourceDefinition>();
        private Dictionary<string, Sprite> _resourceIconCache = new Dictionary<string, Sprite>();
        
        // Кеш для UI даних щоб не пересчитувати кожний фрейм
        private string[] _resourceDisplayNames;
        private Dictionary<string, int> _resourceIdToIndex = new Dictionary<string, int>();
        private bool _resourceCacheDirty = true;
        
        // Кеш для тайлів
        private string[] _tileDisplayIds;
        private Dictionary<string, int> _tileIdToIndex = new Dictionary<string, int>();
        private Dictionary<string, Sprite> _tileIconCache = new Dictionary<string, Sprite>();
        private bool _tileCacheDirty = true;
        
        // Кеш для AssetPreview результатів
        private Dictionary<Sprite, Texture> _assetPreviewCache = new Dictionary<Sprite, Texture>();
    
        // Кеш для List сутностей щоб не переобчислювати кожний фрейм
        private struct EntityCacheEntry
        {
            public int Index;
            public string RowLabel;
            public string SearchBlob;
            public string Category;
            public Sprite Sprite;
        }

        private List<EntityCacheEntry> _buildingEntityCache = new List<EntityCacheEntry>();
        private List<EntityCacheEntry> _unitEntityCache = new List<EntityCacheEntry>();
        private readonly List<int> _filteredBuildingIndices = new List<int>();
        private readonly List<int> _filteredUnitIndices = new List<int>();
        private string _lastBuildingSearch;
        private string _lastUnitSearch;
        private int _lastBuildingRegistrySize = -1;
        private int _lastUnitRegistrySize = -1;
        private static readonly string[] EntitiesSubTabLabels = { "Споруди", "Живі істоти" };

        private List<EconomyValidationIssue> _validationIssues = new List<EconomyValidationIssue>();
        private EconomyMigrationReport _migrationReport;

        private EconomySettlementDefinition _simulationSettlement;
        private float _simulationDurationMinutes = 10f;
        private readonly HashSet<EconomyProductionProfile> _simulationProfiles = new HashSet<EconomyProductionProfile>();
        private EconomySimulationResult _simulationResult;
        private readonly StringBuilder _buildingInfoBuffer = new StringBuilder(256);
        private readonly StringBuilder _unitInfoBuffer = new StringBuilder(256);

        private int _bulkCategoryIndex;
        private string _bulkSearch = string.Empty;
        private readonly HashSet<UnityEngine.Object> _bulkSelection = new HashSet<UnityEngine.Object>();
        private Vector2 _bulkScroll;
        private const string EconomyRootFolder = "Assets/Moyva/SO/Economy";

        [MenuItem("Moyva/Tools/Редактор Економіки")]
        public static void OpenWindow()
        {
            var window = GetWindow<EconomyDesignerWindow>("Редактор Економіки");
            window.minSize = new Vector2(940f, 560f);
            window.Show();
        }

        private void OnEnable()
        {
            if (_database == null)
                _database = FindFirstAsset<EconomyDatabaseSO>();

            if (_rulesConfiguration == null)
                _rulesConfiguration = FindFirstAsset<EconomyRulesConfiguration>();

            if (_buildingRegistry == null)
                _buildingRegistry = FindFirstAsset<BuildingRegistrySO>();

            if (_unitRegistry == null)
                _unitRegistry = FindFirstAsset<UnitRegistrySO>();

            if (_tileRegistry == null)
                _tileRegistry = FindFirstAsset<Kruty1918.Moyva.Grid.API.TileRegistrySO>();

            RefreshResourceCache();
            RefreshTileCache();
            RebuildSerializedObjects();
            
                // Ініціалізувати кеш сутностей
                RebuildBuildingEntityCache();
                RebuildUnitEntityCache();
        }

        private void OnDisable()
        {
            foreach (var editor in _cachedEditors.Values)
            {
                if (editor != null)
                    DestroyImmediate(editor);
            }

            _cachedEditors.Clear();
            _assetPreviewCache.Clear();
            _resourceIconCache.Clear();
            _resourceIdToIndex.Clear();
            _tileIdToIndex.Clear();
            _tileIconCache.Clear();
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawDatabaseSelector();
            DrawCrossSystemBar();
            DrawTabToolbar();

            EditorGUILayout.Space(4f);
            switch (_tab)
            {
                case Tab.Resources:
                    DrawEntityTab<EconomyResourceDefinition>(
                        "Ресурси",
                        Tips.ResourcesTab,
                        "_resources",
                        resource => resource == null ? string.Empty : resource.Id,
                        "Resource");
                    break;

                case Tab.OverridableParameters:
                    DrawOverridableParametersTab();
                    break;

                case Tab.SettlementRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.SettlementRules, "Правила: Поселення");
                    break;

                case Tab.PopulationRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.PopulationRules, "Правила: Населення");
                    break;

                case Tab.WorkforceRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.WorkforceRules, "Правила: Робоча сила");
                    break;

                case Tab.ProductionRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.ProductionRules, "Правила: Виробництво");
                    break;

                case Tab.StorageRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.StorageRules, "Правила: Склади");
                    break;

                case Tab.CaravanRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.CaravanRules, "Правила: Каравани");
                    break;

                case Tab.MarketRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.MarketRules, "Правила: Ринок");
                    break;

                case Tab.ConsumptionNeedsRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.ConsumptionNeeds, "Правила: Споживання");
                    break;

                case Tab.DeathMortalityRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.DeathMortality, "Правила: Смертність");
                    break;

                case Tab.BuildingRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.BuildingRules, "Правила: Будівлі");
                    break;

                case Tab.AiExtensibilityRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.AIExtensibility, "Правила: ШІ та розширюваність");
                    break;

                case Tab.Validation:
                    DrawValidationTab();
                    break;

                case Tab.Simulation:
                    DrawSimulationTab();
                    break;

                case Tab.EntitiesSettings:
                    DrawEntitiesSettingsTab();
                    break;
            }
        }

        private void DrawOverridableParametersTab()
        {
            if (!EnsureRulesConfigurationSerialized())
                return;

            _rulesConfigurationSo.Update();

            EditorGUILayout.LabelField("Оверайди параметрів", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.OverridableTab, MessageType.Info);

            DrawRulesConfigurationSelector();

            var parameters = _rulesConfigurationSo.FindProperty("_parameters");
            if (parameters == null)
            {
                EditorGUILayout.HelpBox("Список '_parameters' не знайдено в EconomyRulesConfiguration.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Дозволити усе", Tips.AllowAllOverridesBtn), GUILayout.Width(160f)))
            {
                Undo.RecordObject(_rulesConfiguration, "Economy: allow all overrides");
                for (int i = 0; i < parameters.arraySize; i++)
                {
                    var element = parameters.GetArrayElementAtIndex(i);
                    var overridableProp = element.FindPropertyRelative("_isOverridable");
                    if (overridableProp != null)
                        overridableProp.boolValue = true;
                }

                if (_rulesConfigurationSo.ApplyModifiedProperties())
                    EditorUtility.SetDirty(_rulesConfiguration);
            }

            if (GUILayout.Button(new GUIContent("Заборонити усе", Tips.DisallowAllOverridesBtn), GUILayout.Width(160f)))
            {
                Undo.RecordObject(_rulesConfiguration, "Economy: disallow all overrides");
                for (int i = 0; i < parameters.arraySize; i++)
                {
                    var element = parameters.GetArrayElementAtIndex(i);
                    var overridableProp = element.FindPropertyRelative("_isOverridable");
                    if (overridableProp != null)
                        overridableProp.boolValue = false;
                }

                if (_rulesConfigurationSo.ApplyModifiedProperties())
                    EditorUtility.SetDirty(_rulesConfiguration);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (parameters.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Список параметрів порожній. Натисніть кнопку нижче щоб заповнити дефолтними правилами.", MessageType.Warning);
                if (GUILayout.Button(new GUIContent("Заповнити дефолтними правилами", Tips.FillDefaultRulesBtn), GUILayout.Height(30f)))
                {
                    _rulesConfiguration.InitializeDefaults();
                    _rulesConfigurationSo = new SerializedObject(_rulesConfiguration);
                }
                return;
            }

            _rulesScroll = EditorGUILayout.BeginScrollView(_rulesScroll, "box");
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var element = parameters.GetArrayElementAtIndex(i);
                var idProp = element.FindPropertyRelative("_id");
                var nameProp = element.FindPropertyRelative("_displayName");
                var categoryProp = element.FindPropertyRelative("_category");
                var defaultProp = element.FindPropertyRelative("_defaultValue");
                var overridableProp = element.FindPropertyRelative("_isOverridable");

                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(nameProp.stringValue) ? idProp.stringValue : nameProp.stringValue, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"ID: {idProp.stringValue}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Категорія: {(EconomyRuleCategory)categoryProp.enumValueIndex}", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Дефолт: {defaultProp.stringValue}", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(overridableProp, new GUIContent("Гравець може змінити"), GUILayout.Width(190f));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (_rulesConfigurationSo.ApplyModifiedProperties())
                EditorUtility.SetDirty(_rulesConfiguration);
        }

        private void DrawRuleCategoryTab(EconomyRuleCategory category, string title)
        {
            if (!EnsureRulesConfigurationSerialized())
                return;

            _rulesConfigurationSo.Update();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(GetRuleCategoryHint(category), MessageType.Info);

            DrawRulesConfigurationSelector();

            var parameters = _rulesConfigurationSo.FindProperty("_parameters");
            if (parameters == null)
            {
                EditorGUILayout.HelpBox("Список '_parameters' не знайдено в EconomyRulesConfiguration.", MessageType.Error);
                return;
            }

            if (parameters.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Список параметрів порожній. Натисніть кнопку нижче щоб заповнити дефолтними правилами.", MessageType.Warning);
                if (GUILayout.Button(new GUIContent("Заповнити дефолтними правилами", Tips.FillDefaultRulesBtn), GUILayout.Height(30f)))
                {
                    _rulesConfiguration.InitializeDefaults();
                    _rulesConfigurationSo = new SerializedObject(_rulesConfiguration);
                }
                return;
            }

            bool found = false;
            _rulesScroll = EditorGUILayout.BeginScrollView(_rulesScroll, "box");
            for (int i = 0; i < parameters.arraySize; i++)
            {
                var element = parameters.GetArrayElementAtIndex(i);
                var categoryProp = element.FindPropertyRelative("_category");
                if (categoryProp.enumValueIndex != (int)category)
                    continue;

                found = true;
                var nameProp = element.FindPropertyRelative("_displayName");
                var idProp = element.FindPropertyRelative("_id");
                var descriptionProp = element.FindPropertyRelative("_description");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(nameProp.stringValue) ? idProp.stringValue : nameProp.stringValue, EditorStyles.boldLabel);
                if (!string.IsNullOrWhiteSpace(descriptionProp.stringValue))
                    EditorGUILayout.HelpBox(descriptionProp.stringValue, MessageType.None);

                EditorGUILayout.PropertyField(nameProp, new GUIContent("Назва"));
                EditorGUILayout.PropertyField(idProp, new GUIContent("ID"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_parameterType"), new GUIContent("Тип"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_defaultValue"), new GUIContent("Дефолтне значення"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_minValue"), new GUIContent("Мін"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_maxValue"), new GUIContent("Макс"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_step"), new GUIContent("Крок"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_isOverridable"), new GUIContent("Гравець може змінити"));
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();

            if (!found)
                EditorGUILayout.HelpBox("У цій категорії ще немає параметрів.", MessageType.Info);

            if (_rulesConfigurationSo.ApplyModifiedProperties())
                EditorUtility.SetDirty(_rulesConfiguration);
        }

        private static string GetRuleCategoryHint(EconomyRuleCategory category)
        {
            switch (category)
            {
                case EconomyRuleCategory.SettlementRules:
                    return "Правила поселень: ліміт поселень, мінімальна відстань між ратушами, базові умови існування поселення.";
                case EconomyRuleCategory.PopulationRules:
                    return "Правила населення: приріст, прибуття нових жителів, ключові демографічні параметри.";
                case EconomyRuleCategory.WorkforceRules:
                    return "Правила робочої сили: пріоритети розподілу робітників і баланс між базовими потребами та виробництвом.";
                case EconomyRuleCategory.ProductionRules:
                    return "Правила виробництва: швидкість циклів, вимоги до input, коефіцієнти продуктивності та псування їжі.";
                case EconomyRuleCategory.StorageRules:
                    return "Правила складів: модель зберігання, логіка загального пулу поселення, обмеження на типи складів.";
                case EconomyRuleCategory.CaravanRules:
                    return "Правила караванів: ліміти, вантажомісткість, вага і розмір товарів, особливі обмеження логістики.";
                case EconomyRuleCategory.MarketRules:
                    return "Правила ринку: формула ціни, вплив запасів і обсягу угоди, граничні мультиплікатори ціни.";
                case EconomyRuleCategory.ConsumptionNeeds:
                    return "Правила споживання: потреби за віком, витрати їжі/води/дров/одягу і наслідки дефіциту.";
                case EconomyRuleCategory.DeathMortality:
                    return "Правила смертності: ризики від голоду, холоду, хвороб, війни та інші причини втрат населення.";
                case EconomyRuleCategory.BuildingRules:
                    return "Правила будівель: пауза виробництва, maintenance, обмеження рецептів і поведінка пошкоджених будівель.";
                case EconomyRuleCategory.AIExtensibility:
                    return "Правила ШІ-розширюваності: підготовка параметрів для майбутньої автономної AI-економіки та торгівлі.";
                default:
                    return "Редагуйте параметри категорії. Ці значення використовуються як дефолтний шаблон для runtime і world override.";
            }
        }

        private void DrawRulesConfigurationSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = (EconomyRulesConfiguration)EditorGUILayout.ObjectField(
                new GUIContent("Шаблон правил", Tips.RulesTemplateField),
                _rulesConfiguration,
                typeof(EconomyRulesConfiguration),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                _rulesConfiguration = next;
                _rulesConfigurationSo = _rulesConfiguration == null ? null : new SerializedObject(_rulesConfiguration);
            }

            if (GUILayout.Button("Показати", GUILayout.Width(90f)) && _rulesConfiguration != null)
                EditorGUIUtility.PingObject(_rulesConfiguration);

            EditorGUILayout.EndHorizontal();
        }

        private bool EnsureRulesConfigurationSerialized()
        {
            if (_rulesConfiguration == null)
            {
                EditorGUILayout.HelpBox("Оберіть EconomyRulesConfiguration для редагування параметрів хабу.", MessageType.Warning);
                DrawRulesConfigurationSelector();
                return false;
            }

            if (_rulesConfigurationSo == null || _rulesConfigurationSo.targetObject != _rulesConfiguration)
                _rulesConfigurationSo = new SerializedObject(_rulesConfiguration);

            _rulesConfiguration.EnsureDefaultUiFormattingParameters();
            _rulesConfigurationSo.Update();

            var parameters = _rulesConfigurationSo.FindProperty("_parameters");
            if (parameters != null && parameters.arraySize == 0)
            {
                _rulesConfiguration.InitializeDefaults();
                _rulesConfigurationSo = new SerializedObject(_rulesConfiguration);
                EditorUtility.SetDirty(_rulesConfiguration);
                AssetDatabase.SaveAssets();
            }

            return true;
        }

        // ═══════════════════════════════════════════════════════
        //  КРОС-СИСТЕМНА НАВІГАЦІЯ
        // ═══════════════════════════════════════════════════════

        private void DrawCrossSystemBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Зв'язані системи:", EditorStyles.miniLabel, GUILayout.Width(110));

            if (GUILayout.Button(new GUIContent("📋 Registry Hub", Tips.OpenRegistryHubBtn), EditorStyles.toolbarButton, GUILayout.Width(120)))
                OpenRegistryHub(-1);

            if (GUILayout.Button(new GUIContent("🏠 Будівлі", Tips.OpenRegistryBuildingsBtn), EditorStyles.toolbarButton, GUILayout.Width(90)))
                OpenRegistryHub(3);

            if (GUILayout.Button(new GUIContent("⚔ Юніти",
                "Перейти до вкладки 'Юніти' в Registry Hub.\n" +
                "Юніти — робоча сила для будівель.\n" +
                "Приклад: перевірити що юніт 'worker' зареєстрований."), EditorStyles.toolbarButton, GUILayout.Width(80)))
                OpenRegistryHub(2);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private static void OpenRegistryHub(int tabIndex)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                .FirstOrDefault(t => t.Name == "RegistryHubWindow" && typeof(EditorWindow).IsAssignableFrom(t));

            if (type == null)
            {
                Debug.LogWarning("[Економіка] RegistryHubWindow не знайдено.");
                return;
            }

            if (tabIndex >= 0)
            {
                var openMethod = type.GetMethod("Open", new[] { typeof(int) });
                if (openMethod != null)
                {
                    openMethod.Invoke(null, new object[] { tabIndex });
                    return;
                }
            }

            EditorWindow.GetWindow(type, false, "Registry Hub");
        }

        private void DrawRulesHubTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            _databaseSo.Update();
            var rulesProperty = _databaseSo.FindProperty("_rulesConfig");
            if (rulesProperty == null)
            {
                EditorGUILayout.HelpBox("Властивість '_rulesConfig' не знайдена в EconomyDatabaseSO.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Хаб Правил Економіки", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.RulesHubTab, MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var assignedRules = (EconomyRulesConfigSO)EditorGUILayout.ObjectField(
                new GUIContent("Конфіг Правил", Tips.RulesConfigField),
                (EconomyRulesConfigSO)rulesProperty.objectReferenceValue,
                typeof(EconomyRulesConfigSO), false);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_database, "Економіка: призначити конфіг правил");
                rulesProperty.objectReferenceValue = assignedRules;
                _databaseSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(_database);
            }

            if (GUILayout.Button(new GUIContent("Створити Конфіг", Tips.CreateRulesConfigBtn), GUILayout.Width(160f)))
                CreateAndAssignRulesConfig(rulesProperty);

            EditorGUILayout.EndHorizontal();

            var rulesConfig = (EconomyRulesConfigSO)rulesProperty.objectReferenceValue;
            if (rulesConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "Конфіг правил не призначено. Створіть або оберіть EconomyRulesConfigSO щоб увімкнути всі налаштування хабу.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(4f);
            DrawObjectInspector(rulesConfig);
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Редактор Економіки", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.WindowHeader, MessageType.Info);
        }

        private void DrawDatabaseSelector()
        {
            EditorGUI.BeginChangeCheck();
            _database = (EconomyDatabaseSO)EditorGUILayout.ObjectField(
                new GUIContent("Економічний Каталог", Tips.DatabaseField),
                _database, typeof(EconomyDatabaseSO), false);
            if (EditorGUI.EndChangeCheck())
                RebuildSerializedObjects();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Знайти", Tips.AutoFindBtn), GUILayout.Width(120f)))
            {
                _database = FindFirstAsset<EconomyDatabaseSO>();
                RebuildSerializedObjects();
            }

            if (GUILayout.Button(new GUIContent("Створити Каталог", Tips.CreateDatabaseBtn), GUILayout.Width(160f)))
                CreateDatabaseAsset();

            using (new EditorGUI.DisabledScope(_database == null))
            {
                if (GUILayout.Button(new GUIContent("Показати", Tips.PingBtn), GUILayout.Width(80f)))
                    EditorGUIUtility.PingObject(_database);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_database == null)
                EditorGUILayout.HelpBox("Оберіть EconomyDatabaseSO щоб почати редагування.", MessageType.Warning);
        }

        private void DrawTabToolbar()
        {
            _tabGroup = (TabGroup)GUILayout.Toolbar((int)_tabGroup, TabGroupLabels);

            if (!TabsByGroup.TryGetValue(_tabGroup, out var groupTabs) || groupTabs == null || groupTabs.Length == 0)
                return;

            if (!groupTabs.Contains(_tab))
                _tab = groupTabs[0];

            var labels = groupTabs.Select(t => TabLabels.TryGetValue(t, out var label) ? label : t.ToString()).ToArray();
            int currentIndex = Mathf.Max(0, Array.IndexOf(groupTabs, _tab));
            int nextIndex = GUILayout.Toolbar(currentIndex, labels);
            nextIndex = Mathf.Clamp(nextIndex, 0, groupTabs.Length - 1);
            _tab = groupTabs[nextIndex];
        }

        private void DrawEntityTab<T>(string title, string hint, string listPropertyName, Func<T, string> displayName, string defaultAssetName)
            where T : ScriptableObject
        {
            if (!EnsureDatabaseSerialized())
                return;

            _databaseSo.Update();
            var listProperty = _databaseSo.FindProperty(listPropertyName);
            if (listProperty == null)
            {
                EditorGUILayout.HelpBox($"Серіалізований список '{listPropertyName}' не знайдено.", MessageType.Error);
                return;
            }

            var items = ReadObjectList<T>(listProperty);
            var selected = GetSelected<T>();
            if (!IsValidUnityObject(selected))
            {
                selected = null;
                SetSelected<T>(null);
            }
            var selectedSet = GetMultiSelectionSetForCurrentTab();
            var search = GetSearch();
            var filtered = items
                .Where(IsValidUnityObject)
                .Where(item => string.IsNullOrWhiteSpace(search) || (displayName(item) ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(hint, MessageType.None);
            EditorGUILayout.LabelField("Підказка: Ctrl/Cmd + клік для мультивибору.", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            DrawEntityListPanel(items, filtered, selected, selectedSet, listPropertyName, displayName, defaultAssetName);
            DrawEntityInspectorPanel(selected, selectedSet);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntityListPanel<T>(
            List<T> allItems,
            List<T> filteredItems,
            T selected,
            HashSet<UnityEngine.Object> selectedSet,
            string listPropertyName,
            Func<T, string> displayName,
            string defaultAssetName)
            where T : ScriptableObject
        {
            bool isResourceTab = typeof(T) == typeof(EconomyResourceDefinition);
            int selectedCount = selectedSet.Count(obj => IsValidUnityObject(obj));

            EditorGUILayout.BeginVertical(GUILayout.Width(320f));
            SetSearch(EditorGUILayout.TextField("Пошук", GetSearch()));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Створити", Tips.CreateEntityBtn), GUILayout.Width(90f)))
            {
                if (isResourceTab)
                {
                    var created = EconomyResourceEditorShared.CreateResourceAssetInProjectFolder("EconomyResourceDefinition");
                    if (created != null)
                    {
                        EconomyResourceEditorShared.AddResourceToDatabase(_database, created);
                        SetSelected(created as T);
                        Selection.activeObject = created;
                        EditorGUIUtility.PingObject(created);
                    }
                }
                else
                {
                    CreateAndAddAsset<T>(listPropertyName, defaultAssetName);
                }
            }

            if (GUILayout.Button(new GUIContent("Додати", Tips.AddSelectedBtn), GUILayout.Width(110f)))
            {
                if (isResourceTab)
                {
                    ShowAddResourceMenu(resource =>
                    {
                        if (EconomyResourceEditorShared.AddResourceToDatabase(_database, resource))
                            SetSelected(resource as T);
                    });
                }
                else
                {
                    var active = Selection.activeObject as T;
                    if (active != null)
                        AddAssetReference(listPropertyName, active);
                }
            }

            if (isResourceTab && GUILayout.Button(new GUIContent("Додати всі", "Додати у базу всі ресурси (.asset), які знайдені в проєкті."), GUILayout.Width(95f)))
            {
                int added = EconomyResourceEditorShared.AddAllResourcesToDatabase(_database);
                ShowNotification(new GUIContent($"Додано ресурсів: {added}"));
            }

            using (new EditorGUI.DisabledScope(selectedCount == 0))
            {
                if (GUILayout.Button(new GUIContent(selectedCount > 1 ? $"Видалити ({selectedCount})" : "Видалити", Tips.RemoveEntityBtn), GUILayout.Width(90f)))
                {
                    var typedSelection = selectedSet
                        .Where(IsValidUnityObject)
                        .OfType<T>()
                        .ToList();

                    if (typedSelection.Count == 0)
                        return;

                    if (isResourceTab)
                    {
                        bool confirmed = EditorUtility.DisplayDialog(
                            "Видалити ресурс",
                            typedSelection.Count > 1
                                ? $"Буде видалено {typedSelection.Count} ресурсів з каталогу та .asset файли буде видалено з проєкту. Продовжити?"
                                : "Ресурс буде видалено з економічного каталогу та .asset файл буде видалено з проєкту. Продовжити?",
                            "Видалити",
                            "Скасувати");

                        if (!confirmed)
                            return;

                        int deleted = 0;
                        for (int i = 0; i < typedSelection.Count; i++)
                        {
                            var resource = typedSelection[i] as EconomyResourceDefinition;
                            if (resource == null)
                                continue;

                            if (EconomyResourceEditorShared.DeleteResourceAsset(_database, resource, out var deleteError))
                            {
                                deleted++;
                            }
                            else
                            {
                                ShowNotification(new GUIContent($"Помилка видалення ресурсу: {deleteError}"));
                            }
                        }

                        selectedSet.Clear();
                        SetSelected<T>(null);
                        if (deleted > 0)
                            ShowNotification(new GUIContent($"Видалено ресурсів: {deleted}"));
                    }
                    else
                    {
                        int removed = 0;
                        for (int i = 0; i < typedSelection.Count; i++)
                        {
                            RemoveAssetReference(listPropertyName, typedSelection[i]);
                            removed++;
                        }

                        selectedSet.Clear();
                        SetSelected<T>(null);
                        if (removed > 1)
                            ShowNotification(new GUIContent($"Видалено елементів: {removed}"));
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, "box");
            if (filteredItems.Count == 0)
            {
                EditorGUILayout.HelpBox("Немає записів що відповідають фільтру.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < filteredItems.Count; i++)
                {
                    var item = filteredItems[i];
                    if (!IsValidUnityObject(item))
                        continue;

                    var label = string.IsNullOrWhiteSpace(displayName(item)) ? item.name : displayName(item);
                    var isSelected = selectedSet.Contains(item);
                    if (isResourceTab && item is EconomyResourceDefinition resource)
                    {
                        EditorGUILayout.BeginHorizontal("box", GUILayout.Height(42f));

                        var iconRect = GUILayoutUtility.GetRect(36f, 36f, GUILayout.Width(36f), GUILayout.Height(36f));
                        EditorGUI.DrawRect(iconRect, new Color(0.14f, 0.14f, 0.14f));
                        if (resource.Icon != null)
                            DrawSpriteRect(iconRect, resource.Icon);
                        else
                            EditorGUI.LabelField(iconRect, "-", EditorStyles.centeredGreyMiniLabel);

                        GUILayout.Label(label, isSelected ? EditorStyles.boldLabel : EditorStyles.label);
                        GUILayout.FlexibleSpace();

                        EditorGUILayout.EndHorizontal();
                        var rowRect = GUILayoutUtility.GetLastRect();

                        if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                        {
                            HandleEntityItemClick(item, selectedSet);
                            Event.current.Use();
                        }
                    }
                    else
                    {
                        var nextSelected = GUILayout.Toggle(isSelected, label, "Button");
                        if (nextSelected != isSelected)
                            HandleEntityItemToggle(item, nextSelected, selectedSet);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"Всього: {allItems.Count}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void ShowAddResourceMenu(Action<EconomyResourceDefinition> onPicked)
        {
            var available = EconomyResourceEditorShared.LoadResourcesNotInDatabase(_database);
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

                menu.AddItem(new GUIContent(label), false, () => onPicked?.Invoke(resource));
            }

            menu.ShowAsContext();
        }

        private void DrawSpriteRect(Rect rect, Sprite sprite)
        {
            if (sprite == null)
                return;

            if (_assetPreviewCache.TryGetValue(sprite, out var preview) && preview != null)
            {
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);
                return;
            }

            preview = AssetPreview.GetAssetPreview(sprite);
            if (preview != null)
            {
                _assetPreviewCache[sprite] = preview;
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);
            }
            else
            {
                if (AssetPreview.IsLoadingAssetPreview(sprite.GetInstanceID()))
                    Repaint();

                DrawSpriteRectDirect(rect, sprite);
            }
        }

        private static void DrawSpriteRectDirect(Rect rect, Sprite sprite)
        {
            if (sprite == null)
                return;

            Texture tex = sprite.texture;
            if (tex != null)
            {
                Rect r = sprite.textureRect;
                Rect uv = new Rect(r.x / tex.width, r.y / tex.height, r.width / tex.width, r.height / tex.height);
                GUI.DrawTextureWithTexCoords(rect, tex, uv, true);
            }
        }

        private void DrawEntitySpritePreview(Sprite sprite, float size = 36f)
        {
            var iconRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            EditorGUI.DrawRect(iconRect, new Color(0.14f, 0.14f, 0.14f));

            if (sprite != null)
                DrawSpriteRect(iconRect, sprite);
            else
                EditorGUI.LabelField(iconRect, "-", EditorStyles.centeredGreyMiniLabel);
        }

        private static Sprite ResolveBuildingSprite(SerializedProperty buildingProperty)
        {
            if (buildingProperty == null)
                return null;

            var explicitIcon = buildingProperty.FindPropertyRelative("Icon")?.objectReferenceValue as Sprite;
            if (explicitIcon != null)
                return explicitIcon;

            var prefab = buildingProperty.FindPropertyRelative("Prefab")?.objectReferenceValue as GameObject;
            return ExtractSpriteFromPrefab(prefab);
        }

        private static Sprite ResolveUnitSprite(SerializedProperty unitProperty)
        {
            if (unitProperty == null)
                return null;

            var prefab = unitProperty.FindPropertyRelative("Prefab")?.objectReferenceValue as GameObject;
            return ExtractSpriteFromPrefab(prefab);
        }

        private static Sprite ExtractSpriteFromPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer != null && renderer.sprite != null)
                    return renderer.sprite;
            }

            return null;
        }

        private static void DrawSerializedPropertyChildren(SerializedProperty parentProperty)
        {
            if (parentProperty == null)
                return;

            var iterator = parentProperty.Copy();
            var endProperty = iterator.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private void DrawBuildingEntityInspector(SerializedProperty buildingProperty)
        {
            if (buildingProperty == null)
                return;

            var idProp = buildingProperty.FindPropertyRelative("Id");
            var displayNameProp = buildingProperty.FindPropertyRelative("DisplayName");
            var categoryProp = buildingProperty.FindPropertyRelative("Category");
            var iconProp = buildingProperty.FindPropertyRelative("Icon");
            var prefabProp = buildingProperty.FindPropertyRelative("Prefab");
            var requiredWorkersProp = buildingProperty.FindPropertyRelative("RequiredWorkers");
            var economyPriorityProp = buildingProperty.FindPropertyRelative("EconomyPriority");
            var isHousingProp = buildingProperty.FindPropertyRelative("IsHousing");
            var isWarehouseProp = buildingProperty.FindPropertyRelative("IsWarehouse");
            var housingCapacityProp = buildingProperty.FindPropertyRelative("HousingCapacity");
            var isTownHallProp = buildingProperty.FindPropertyRelative("IsTownHall");
            var isCastleProp = buildingProperty.FindPropertyRelative("IsCastle");
            var industrialResourceIdProp = buildingProperty.FindPropertyRelative("IndustrialResourceId");
            var requiresTilesProp = buildingProperty.FindPropertyRelative("RequiresTiles");
            var tileRequirementsProp = buildingProperty.FindPropertyRelative("TileRequirements");
            var useCustomTownHallRulesProp = buildingProperty.FindPropertyRelative("UseCustomTownHallRules");
            var requireTownHallInRangeProp = buildingProperty.FindPropertyRelative("RequireTownHallInRange");
            var blockIfTownHallAlreadyInRangeProp = buildingProperty.FindPropertyRelative("BlockIfTownHallAlreadyInRange");
            var townHallRadiusOverrideProp = buildingProperty.FindPropertyRelative("TownHallProximityRadiusOverride");

            EditorGUILayout.PropertyField(idProp);
            EditorGUILayout.PropertyField(displayNameProp);
            EditorGUILayout.PropertyField(categoryProp);
            EditorGUILayout.PropertyField(iconProp);
            EditorGUILayout.PropertyField(prefabProp);

            var category = categoryProp != null
                ? (Kruty1918.Moyva.Construction.API.BuildingCategory)categoryProp.enumValueIndex
                : Kruty1918.Moyva.Construction.API.BuildingCategory.Civilian;

            bool isWall = category == Kruty1918.Moyva.Construction.API.BuildingCategory.Walls;
            bool isIndustrial = category == Kruty1918.Moyva.Construction.API.BuildingCategory.Industrial;
            bool isCivilian = category == Kruty1918.Moyva.Construction.API.BuildingCategory.Civilian;

            if (!isCivilian)
            {
                if (isTownHallProp != null) isTownHallProp.boolValue = false;
                if (isCastleProp != null) isCastleProp.boolValue = false;
            }

            if (isCivilian)
            {
                var selected = CivilianBuildingType.Regular;
                if (isTownHallProp != null && isTownHallProp.boolValue)
                    selected = CivilianBuildingType.TownHall;
                else if (isCastleProp != null && isCastleProp.boolValue)
                    selected = CivilianBuildingType.Castle;

                selected = (CivilianBuildingType)EditorGUILayout.EnumPopup(
                    new GUIContent("Тип цивільної споруди", "Для Civilian-категорії: звичайна, ратуша або замок. Ратуша і замок взаємовиключні."),
                    selected);

                if (isTownHallProp != null)
                    isTownHallProp.boolValue = selected == CivilianBuildingType.TownHall;
                if (isCastleProp != null)
                    isCastleProp.boolValue = selected == CivilianBuildingType.Castle;
            }

            if (!isIndustrial && industrialResourceIdProp != null)
                industrialResourceIdProp.stringValue = string.Empty;

            bool isTownHall = isTownHallProp != null && isTownHallProp.boolValue;
            bool isCastle = isCastleProp != null && isCastleProp.boolValue;

            if (isTownHall && isCastle && isCastleProp != null)
            {
                isCastleProp.boolValue = false;
                isCastle = false;
            }

            bool isCentral = isTownHall || isCastle;

            if (isWall || isCentral)
            {
                if (requiredWorkersProp != null) requiredWorkersProp.intValue = 0;
                if (economyPriorityProp != null) economyPriorityProp.intValue = 0;
                if (isWarehouseProp != null) isWarehouseProp.boolValue = false;
                if (housingCapacityProp != null) housingCapacityProp.intValue = 0;
                if (industrialResourceIdProp != null) industrialResourceIdProp.stringValue = string.Empty;
                if (requiresTilesProp != null) requiresTilesProp.boolValue = false;
                if (tileRequirementsProp != null && tileRequirementsProp.isArray) tileRequirementsProp.arraySize = 0;

                if (useCustomTownHallRulesProp != null) useCustomTownHallRulesProp.boolValue = false;
                if (requireTownHallInRangeProp != null) requireTownHallInRangeProp.boolValue = false;
                if (blockIfTownHallAlreadyInRangeProp != null) blockIfTownHallAlreadyInRangeProp.boolValue = true;
                if (townHallRadiusOverrideProp != null) townHallRadiusOverrideProp.intValue = 0;

                if (isHousingProp != null) isHousingProp.boolValue = false;
            }

            if (isWall)
            {
                if (isHousingProp != null) isHousingProp.boolValue = false;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Економічні параметри", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(isWall || isCentral))
            {
                EditorGUILayout.PropertyField(requiredWorkersProp);
                EditorGUILayout.PropertyField(economyPriorityProp);
                EditorGUILayout.PropertyField(isWarehouseProp);
            }

            using (new EditorGUI.DisabledScope(isWall || isCentral))
            {
                EditorGUILayout.PropertyField(isHousingProp);
            }

            bool isHousing = isHousingProp != null && isHousingProp.boolValue && !isWall && !isCentral;
            if (!isHousing && housingCapacityProp != null)
                housingCapacityProp.intValue = 0;

            using (new EditorGUI.DisabledScope(!isHousing))
            {
                if (housingCapacityProp != null)
                    EditorGUILayout.PropertyField(housingCapacityProp);
            }

            using (new EditorGUI.DisabledScope(!isIndustrial || isWall || isCentral))
            {
                if (industrialResourceIdProp != null)
                    DrawResourceSelector(industrialResourceIdProp);
            }

            // Вимоги до тайлів
            DrawTileRequirementsUI(buildingProperty);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Правила розміщення", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(isCentral))
            {
                if (useCustomTownHallRulesProp != null)
                    EditorGUILayout.PropertyField(useCustomTownHallRulesProp);
            }

            bool usesCustomTownHallRules = !isCentral
                && useCustomTownHallRulesProp != null
                && useCustomTownHallRulesProp.boolValue;

            bool defaultRequireTownHall = !isCentral;
            bool defaultBlockTownHallAlreadyInRange = isCentral;

            if (!usesCustomTownHallRules)
            {
                if (requireTownHallInRangeProp != null)
                    requireTownHallInRangeProp.boolValue = defaultRequireTownHall;
                if (blockIfTownHallAlreadyInRangeProp != null)
                    blockIfTownHallAlreadyInRangeProp.boolValue = defaultBlockTownHallAlreadyInRange;
                if (townHallRadiusOverrideProp != null)
                    townHallRadiusOverrideProp.intValue = 0;
            }

            using (new EditorGUI.DisabledScope(!usesCustomTownHallRules))
            {
                if (requireTownHallInRangeProp != null)
                    EditorGUILayout.PropertyField(requireTownHallInRangeProp);
                if (blockIfTownHallAlreadyInRangeProp != null)
                    EditorGUILayout.PropertyField(blockIfTownHallAlreadyInRangeProp);
                if (townHallRadiusOverrideProp != null)
                    EditorGUILayout.PropertyField(townHallRadiusOverrideProp);
            }

            if (isWall)
            {
                EditorGUILayout.HelpBox(
                    "Для стін економічні параметри, житло, склади та промисловий ресурс недоступні.",
                    MessageType.Info);
            }
            else if (isTownHall)
            {
                EditorGUILayout.HelpBox(
                    "Ратуша не може бути одночасно замком. Для ратуші вимкнені робітники, економічний пріоритет, склад, житло та ресурсний профіль; правила розміщення також фіксовані.",
                    MessageType.Info);
            }
            else if (isCastle)
            {
                EditorGUILayout.HelpBox(
                    "Замок не може бути одночасно ратушею. Для замку вимкнені робітники, економічний пріоритет, склад, житло, ресурсний профіль і тайлові вимоги; правила розміщення фіксовані.",
                    MessageType.Info);
            }
            else if (!isIndustrial)
            {
                EditorGUILayout.HelpBox(
                    "Поле Industrial Resource Id активне лише для Industrial-споруд.",
                    MessageType.None);
            }
        }

        private void DrawUnitEntityInspector(SerializedProperty unitProperty)
        {
            if (unitProperty == null)
                return;

            var typeIdProp = unitProperty.FindPropertyRelative("TypeId");
            var roleProp = unitProperty.FindPropertyRelative("Role");
            var baseStaminaProp = unitProperty.FindPropertyRelative("BaseStamina");
            var visionRangeProp = unitProperty.FindPropertyRelative("VisionRange");
            var prefabProp = unitProperty.FindPropertyRelative("Prefab");
            var staminaRandomRangeProp = unitProperty.FindPropertyRelative("StaminaRandomRange");
            var animationSettingsProp = unitProperty.FindPropertyRelative("AnimationSettings");

            EditorGUILayout.PropertyField(typeIdProp);
            EditorGUILayout.PropertyField(roleProp);
            EditorGUILayout.PropertyField(baseStaminaProp);
            EditorGUILayout.PropertyField(visionRangeProp);
            EditorGUILayout.PropertyField(prefabProp);
            EditorGUILayout.PropertyField(staminaRandomRangeProp);
            EditorGUILayout.PropertyField(animationSettingsProp, true);

            if (roleProp != null)
            {
                var role = (Kruty1918.Moyva.Units.API.UnitRole)roleProp.enumValueIndex;
                EditorGUILayout.HelpBox(
                    role == Kruty1918.Moyva.Units.API.UnitRole.Worker
                        ? "Worker використовується як цивільний/економічний юніт. Іконка береться з prefab SpriteRenderer."
                        : "Military використовується як бойовий юніт. Іконка також береться з prefab SpriteRenderer.",
                    MessageType.Info);
            }
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

        private void DrawEntityInspectorPanel<T>(T selected, HashSet<UnityEngine.Object> selectedSet) where T : ScriptableObject
        {
            EditorGUILayout.BeginVertical("box");
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

            var typedSelection = selectedSet
                .Where(IsValidUnityObject)
                .OfType<T>()
                .ToList();

            if (typedSelection.Count > 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Обрано {typedSelection.Count} елементів", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Мультивибір: зміна спільних полів застосовується до всіх обраних елементів.", MessageType.Info);
                DrawMultiObjectInspector(typedSelection);

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            if (selected == null)
            {
                EditorGUILayout.HelpBox("Оберіть елемент зліва для редагування.", MessageType.None);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("Показати", Tips.PingBtn), GUILayout.Width(70f)))
                EditorGUIUtility.PingObject(selected);
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            DrawObjectInspector(selected);
            if (EditorGUI.EndChangeCheck() && selected is EconomyResourceDefinition resource)
            {
                if (EconomyResourceEditorShared.TrySyncResourceAssetName(resource, out var syncError))
                {
                    Repaint();
                }
                else if (!string.IsNullOrWhiteSpace(syncError))
                {
                    ShowNotification(new GUIContent($"Помилка перейменування ресурсу: {syncError}"));
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawMultiObjectInspector<T>(IReadOnlyList<T> targets) where T : ScriptableObject
        {
            if (targets == null || targets.Count <= 1)
                return;

            var unityTargets = targets
                .Where(IsValidUnityObject)
                .Cast<UnityEngine.Object>()
                .ToArray();

            if (unityTargets.Length <= 1)
                return;

            var serialized = new SerializedObject(unityTargets);
            serialized.Update();

            var iterator = serialized.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (iterator.propertyPath == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.PropertyField(iterator, true);
                    continue;
                }

                if (iterator.propertyPath == "_id" ||
                    iterator.propertyPath == "_displayName" ||
                    iterator.propertyPath == "_icon" ||
                    iterator.propertyPath == "_category")
                    continue;

                EditorGUILayout.PropertyField(iterator, true);
            }

            if (serialized.ApplyModifiedProperties())
            {
                for (int i = 0; i < unityTargets.Length; i++)
                    EditorUtility.SetDirty(unityTargets[i]);
            }
        }

        private void DrawObjectInspector(UnityEngine.Object target)
        {
            if (!_cachedEditors.TryGetValue(target, out var editor) || editor == null)
            {
                UnityEditor.Editor.CreateCachedEditor(target, null, ref editor);
                _cachedEditors[target] = editor;
            }

            if (editor != null)
                editor.OnInspectorGUI();
        }

        private void DrawValidationTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            EditorGUILayout.LabelField("Валідація", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.ValidationTab, MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Запустити Перевірку", Tips.RunValidationBtn), GUILayout.Height(28f)))
                _validationIssues = _validationService.Validate(_database).ToList();

            if (GUILayout.Button(new GUIContent("Виправити Помилки", Tips.FixIssuesBtn), GUILayout.Height(28f)))
            {
                Undo.RecordObject(_database, "Економіка: виправлення помилок");
                var fixedCount = _autoFixService.FixCommonIssues(_database);
                _validationIssues = _validationService.Validate(_database).ToList();
                ShowNotification(new GUIContent($"Виправлено {fixedCount} проблем."));
                AssetDatabase.SaveAssets();
            }

            if (GUILayout.Button(new GUIContent("Мігрувати Схему", Tips.RunMigrationBtn), GUILayout.Height(28f)))
            {
                Undo.RecordObject(_database, "Економіка: міграція даних");
                _migrationReport = _migrationService.Migrate(_database);
                _databaseSo?.Update();
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndHorizontal();

            _validationScroll = EditorGUILayout.BeginScrollView(_validationScroll, "box");
            if (_validationIssues.Count == 0)
            {
                EditorGUILayout.HelpBox("Проблем не знайдено. Запустіть перевірку для оновлення.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < _validationIssues.Count; i++)
                {
                    var issue = _validationIssues[i];
                    var messageType = issue.Severity == EconomyValidationSeverity.Error ? MessageType.Error : MessageType.Warning;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox(issue.Message, messageType);
                    if (issue.Context != null && GUILayout.Button(new GUIContent("Показати", Tips.PingBtn), GUILayout.Width(64f), GUILayout.Height(38f)))
                        EditorGUIUtility.PingObject(issue.Context);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(6f);
            DrawMigrationStatus();
        }

        private void DrawMigrationStatus()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Схема", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Поточна підтримувана версія", EconomySchema.CurrentVersion.ToString());
            EditorGUILayout.LabelField("Версія бази даних", _database != null ? _database.SchemaVersion.ToString() : "—");

            if (_migrationReport == null)
            {
                EditorGUILayout.HelpBox("Запустіть міграцію щоб побачити кроки.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            var header = _migrationReport.Changed
                ? $"Міграцію виконано: {_migrationReport.FromVersion} → {_migrationReport.ToVersion}"
                : $"Змін немає: {_migrationReport.FromVersion} → {_migrationReport.ToVersion}";
            EditorGUILayout.HelpBox(header, _migrationReport.Changed ? MessageType.Info : MessageType.None);

            for (var i = 0; i < _migrationReport.Steps.Count; i++)
                EditorGUILayout.LabelField($"{i + 1}. {_migrationReport.Steps[i]}", EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawSimulationTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            EditorGUILayout.LabelField("Симуляція (Preview)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.SimulationTab, MessageType.Info);

            _simulationSettlement = (EconomySettlementDefinition)EditorGUILayout.ObjectField(
                new GUIContent("Поселення", Tips.SimSettlementField),
                _simulationSettlement,
                typeof(EconomySettlementDefinition),
                false);

            _simulationDurationMinutes = EditorGUILayout.FloatField(
                new GUIContent("Тривалість (хвилини)", Tips.SimDurationField),
                Mathf.Max(0f, _simulationDurationMinutes));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Обрати за замовч.", Tips.SimSelectDefaultsBtn), GUILayout.Width(140f)))
                SelectDefaultSimulationProfiles();

            if (GUILayout.Button(new GUIContent("Обрати всі",
                "Обрати ВСІ виробничі профілі для симуляції.\n" +
                "Дозволяє оцінити максимальну виробничу потужність.\n" +
                "Приклад: всі будівлі працюють одночасно."), GUILayout.Width(120f)))
            {
                _simulationProfiles.Clear();
                foreach (var profile in _database.ProductionProfiles.Where(profile => profile != null))
                    _simulationProfiles.Add(profile);
            }

            if (GUILayout.Button(new GUIContent("Очистити",
                "Зняти вибір з усіх профілів.\nПриклад: щоб обрати лише конкретні будівлі."), GUILayout.Width(90f)))
                _simulationProfiles.Clear();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            _simulationScroll = EditorGUILayout.BeginScrollView(_simulationScroll, "box", GUILayout.Height(220f));
            var profiles = _database.ProductionProfiles.Where(profile => profile != null).OrderBy(profile => profile.BuildingId ?? string.Empty, StringComparer.Ordinal).ToList();
            if (profiles.Count == 0)
            {
                EditorGUILayout.HelpBox("Немає виробничих профілів у базі.", MessageType.None);
            }
            else
            {
                for (var i = 0; i < profiles.Count; i++)
                {
                    var profile = profiles[i];
                    var selected = _simulationProfiles.Contains(profile);
                    var next = EditorGUILayout.ToggleLeft(
                        $"{profile.BuildingId} → {profile.RecipeId} (цикл {profile.CycleDurationSeconds:0.##}с, вихід {profile.OutputAmountPerCycle})",
                        selected);
                    if (next && !selected)
                        _simulationProfiles.Add(profile);
                    else if (!next && selected)
                        _simulationProfiles.Remove(profile);
                }
            }
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(new GUIContent("Запустити Симуляцію", Tips.SimRunBtn), GUILayout.Height(30f)))
            {
                var input = new EconomySimulationInput
                {
                    Settlement = _simulationSettlement,
                    DurationMinutes = _simulationDurationMinutes,
                    ProductionProfiles = _simulationProfiles.OrderBy(profile => profile.BuildingId ?? string.Empty, StringComparer.Ordinal).ToList(),
                };
                _simulationResult = _simulationService.Simulate(input);
            }

            DrawSimulationResults();
        }

        private void DrawSimulationResults()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Результат", EditorStyles.boldLabel);

            if (_simulationResult == null)
            {
                EditorGUILayout.HelpBox("Запустіть симуляцію щоб побачити оцінку.", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.LabelField("Поселення", _simulationResult.Settlement == null ? "(не вказано)" : _simulationResult.Settlement.SettlementId);
            EditorGUILayout.LabelField("Тривалість", $"{_simulationResult.DurationMinutes:0.##} хвилин");

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Оцінка ресурсів", EditorStyles.miniBoldLabel);
            if (_simulationResult.ResourceTotals.Count == 0)
            {
                EditorGUILayout.LabelField("Немає виходу для обраних профілів.");
            }
            else
            {
                foreach (var pair in _simulationResult.ResourceTotals)
                    EditorGUILayout.LabelField(pair.Key, pair.Value.ToString());
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Журнал", EditorStyles.miniBoldLabel);
            if (_simulationResult.Log.Count == 0)
            {
                EditorGUILayout.LabelField("Записів немає.");
            }
            else
            {
                for (var i = 0; i < _simulationResult.Log.Count; i++)
                    EditorGUILayout.LabelField($"{i + 1}. {_simulationResult.Log[i]}", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawEntitiesSettingsTab()
        {
            EditorGUILayout.LabelField("Налаштування сутностей", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "У цій секції редагуються економічні параметри сутностей з реєстрів. " +
                "Підвкладка 'Споруди' — будівлі/стіни, підвкладка 'Живі істоти' — юніти та майбутні живі сутності.",
                MessageType.Info);

            _entitiesSubTab = GUILayout.Toolbar(_entitiesSubTab, EntitiesSubTabLabels);
            EditorGUILayout.Space(4f);

            if (_entitiesSubTab == 0)
                DrawBuildingsEntitiesTab();
            else
                DrawLivingEntitiesTab();
        }

        private void DrawBuildingsEntitiesTab()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _buildingRegistry = (BuildingRegistrySO)EditorGUILayout.ObjectField(
                new GUIContent("Building Registry", "Реєстр будівель і стін (BuildingDefinition + WallCollection)."),
                _buildingRegistry,
                typeof(BuildingRegistrySO),
                false);
            if (EditorGUI.EndChangeCheck())
                _buildingRegistrySo = _buildingRegistry == null ? null : new SerializedObject(_buildingRegistry);

            if (GUILayout.Button("Знайти", GUILayout.Width(80f)))
            {
                _buildingRegistry = FindFirstAsset<BuildingRegistrySO>();
                _buildingRegistrySo = _buildingRegistry == null ? null : new SerializedObject(_buildingRegistry);
            }

            using (new EditorGUI.DisabledScope(_buildingRegistry == null))
            {
                if (GUILayout.Button("Показати", GUILayout.Width(80f)))
                    EditorGUIUtility.PingObject(_buildingRegistry);
            }

            EditorGUILayout.EndHorizontal();

            if (_buildingRegistrySo == null)
            {
                EditorGUILayout.HelpBox("Оберіть BuildingRegistrySO, щоб редагувати економічні параметри споруд.", MessageType.Warning);
                return;
            }

            _buildingRegistrySo.Update();
            var buildingsProp = _buildingRegistrySo.FindProperty("Buildings");
            if (buildingsProp == null || !buildingsProp.isArray)
            {
                EditorGUILayout.HelpBox("Властивість 'Buildings' не знайдена в BuildingRegistrySO.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(360f));
            _entitiesBuildingSearch = EditorGUILayout.TextField("Пошук споруди", _entitiesBuildingSearch ?? string.Empty);
            _entitiesLeftScroll = EditorGUILayout.BeginScrollView(_entitiesLeftScroll);
            // Перевірити чи потрібно оновити кеш: змінився розмір реєстру або пошук
            if (_lastBuildingRegistrySize != buildingsProp.arraySize || _lastBuildingSearch != _entitiesBuildingSearch)
            {
                RebuildBuildingEntityCache();
                _lastBuildingSearch = _entitiesBuildingSearch;
            }

            for (int fi = 0; fi < _filteredBuildingIndices.Count; fi++)
            {
                var entry = _buildingEntityCache[_filteredBuildingIndices[fi]];
                bool selected = _selectedBuildingEntityIndex == entry.Index;

                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(42f));
                DrawEntitySpritePreview(entry.Sprite);

                if (GUILayout.Toggle(selected, $"{entry.RowLabel}  •  {entry.Category}", "Button", GUILayout.ExpandHeight(true), GUILayout.MinHeight(36f)) && !selected)
                    _selectedBuildingEntityIndex = entry.Index;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"Знайдено: {_filteredBuildingIndices.Count} / {buildingsProp.arraySize}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            _entitiesRightScroll = EditorGUILayout.BeginScrollView(_entitiesRightScroll);
            if (_selectedBuildingEntityIndex < 0 || _selectedBuildingEntityIndex >= buildingsProp.arraySize)
            {
                EditorGUILayout.HelpBox("Оберіть споруду зліва для редагування параметрів.", MessageType.None);
            }
            else
            {
                var selectedProp = buildingsProp.GetArrayElementAtIndex(_selectedBuildingEntityIndex);
                var selectedId = selectedProp.FindPropertyRelative("Id")?.stringValue;
                var sprite = ResolveBuildingSprite(selectedProp);

                EditorGUILayout.BeginHorizontal();
                DrawEntitySpritePreview(sprite, 56f);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"Споруда: {selectedId}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    selectedProp.FindPropertyRelative("Icon")?.objectReferenceValue == null
                        ? "Іконка взята з prefab SpriteRenderer"
                        : "Іконка взята з поля Icon",
                    EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                DrawBuildingDefaultInfoPanel(selectedProp);
                DrawBuildingEntityInspector(selectedProp);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (_buildingRegistrySo.ApplyModifiedProperties())
                CommitSharedAssetChange(_buildingRegistry);
        }

        private void DrawLivingEntitiesTab()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            _unitRegistry = (UnitRegistrySO)EditorGUILayout.ObjectField(
                new GUIContent("Unit Registry", "Реєстр юнітів (живі істоти) з економічно релевантними параметрами."),
                _unitRegistry,
                typeof(UnitRegistrySO),
                false);
            if (EditorGUI.EndChangeCheck())
                _unitRegistrySo = _unitRegistry == null ? null : new SerializedObject(_unitRegistry);

            if (GUILayout.Button("Знайти", GUILayout.Width(80f)))
            {
                _unitRegistry = FindFirstAsset<UnitRegistrySO>();
                _unitRegistrySo = _unitRegistry == null ? null : new SerializedObject(_unitRegistry);
            }

            using (new EditorGUI.DisabledScope(_unitRegistry == null))
            {
                if (GUILayout.Button("Показати", GUILayout.Width(80f)))
                    EditorGUIUtility.PingObject(_unitRegistry);
            }

            EditorGUILayout.EndHorizontal();

            if (_unitRegistrySo == null)
            {
                EditorGUILayout.HelpBox("Оберіть UnitRegistrySO, щоб редагувати параметри живих істот.", MessageType.Warning);
                return;
            }

            _unitRegistrySo.Update();
            var configsProp = _unitRegistrySo.FindProperty("Configs");
            if (configsProp == null || !configsProp.isArray)
            {
                EditorGUILayout.HelpBox("Властивість 'Configs' не знайдена в UnitRegistrySO.", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(360f));
            _entitiesUnitSearch = EditorGUILayout.TextField("Пошук юніта", _entitiesUnitSearch ?? string.Empty);
            // Перевірити чи потрібно оновити кеш
            if (_lastUnitRegistrySize != configsProp.arraySize || _lastUnitSearch != _entitiesUnitSearch)
            {
                RebuildUnitEntityCache();
                _lastUnitSearch = _entitiesUnitSearch;
            }
            _entitiesLeftScroll = EditorGUILayout.BeginScrollView(_entitiesLeftScroll);

            for (int fi = 0; fi < _filteredUnitIndices.Count; fi++)
            {
                var entry = _unitEntityCache[_filteredUnitIndices[fi]];
                bool selected = _selectedUnitEntityIndex == entry.Index;

                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(42f));
                DrawEntitySpritePreview(entry.Sprite);

                if (GUILayout.Toggle(selected, $"{entry.RowLabel}  •  {entry.Category}", "Button", GUILayout.ExpandHeight(true), GUILayout.MinHeight(36f)) && !selected)
                    _selectedUnitEntityIndex = entry.Index;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.LabelField($"Знайдено: {_filteredUnitIndices.Count} / {configsProp.arraySize}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            _entitiesRightScroll = EditorGUILayout.BeginScrollView(_entitiesRightScroll);
            if (_selectedUnitEntityIndex < 0 || _selectedUnitEntityIndex >= configsProp.arraySize)
            {
                EditorGUILayout.HelpBox("Оберіть юніта зліва для редагування параметрів.", MessageType.None);
            }
            else
            {
                var selectedProp = configsProp.GetArrayElementAtIndex(_selectedUnitEntityIndex);
                var selectedTypeId = selectedProp.FindPropertyRelative("TypeId")?.stringValue;
                var sprite = ResolveUnitSprite(selectedProp);

                EditorGUILayout.BeginHorizontal();
                DrawEntitySpritePreview(sprite, 56f);
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"Жива істота: {selectedTypeId}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Іконка взята з prefab SpriteRenderer", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                DrawUnitDefaultInfoPanel(selectedProp);
                DrawUnitEntityInspector(selectedProp);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            if (_unitRegistrySo.ApplyModifiedProperties())
                CommitSharedAssetChange(_unitRegistry);
        }

        private static void CommitSharedAssetChange(UnityEngine.Object asset)
        {
            if (asset == null)
                return;

            EditorUtility.SetDirty(asset);
        }

        private void DrawBuildingDefaultInfoPanel(SerializedProperty buildingProperty)
        {
            if (buildingProperty == null || _buildingRegistry == null || _buildingRegistry.Buildings == null)
                return;

            var id = buildingProperty.FindPropertyRelative("Id")?.stringValue;
            if (string.IsNullOrWhiteSpace(id))
                return;

            BuildingDefinition definition = null;
            var source = _buildingRegistry.Buildings;
            for (int i = 0; i < source.Length; i++)
            {
                var candidate = source[i];
                if (candidate != null && candidate.Id == id)
                {
                    definition = candidate;
                    break;
                }
            }

            if (definition == null)
                return;

            _buildingInfoBuffer.Clear();
            if (!BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, _buildingInfoBuffer))
                return;

            EditorGUILayout.HelpBox(_buildingInfoBuffer.ToString().TrimEnd(), MessageType.Info);
        }

        private void DrawUnitDefaultInfoPanel(SerializedProperty unitProperty)
        {
            if (unitProperty == null || _unitRegistry == null || _unitRegistry.Configs == null)
                return;

            var typeId = unitProperty.FindPropertyRelative("TypeId")?.stringValue;
            if (string.IsNullOrWhiteSpace(typeId))
                return;

            UnitClassConfig config = null;
            var source = _unitRegistry.Configs;
            for (int i = 0; i < source.Count; i++)
            {
                var candidate = source[i];
                if (candidate != null && candidate.TypeId == typeId)
                {
                    config = candidate;
                    break;
                }
            }

            if (config == null)
                return;

            _unitInfoBuffer.Clear();

            if (!string.IsNullOrWhiteSpace(config.TypeId))
                _unitInfoBuffer.AppendLine($"TypeId: {config.TypeId}");

            _unitInfoBuffer.AppendLine(config.Role == Kruty1918.Moyva.Units.API.UnitRole.Military
                ? "Прапорець: бойовий юніт"
                : "Прапорець: економічний юніт");

            if (config.BaseStamina > 0f)
                _unitInfoBuffer.AppendLine($"Базова стаміна: {config.BaseStamina:0.#}");

            if (config.VisionRange > 0)
                _unitInfoBuffer.AppendLine($"Дальність огляду: {config.VisionRange}");

            if (config.StaminaRandomRange != Vector2.zero)
                _unitInfoBuffer.AppendLine($"Рандом стаміни: {config.StaminaRandomRange.x:0.#} .. {config.StaminaRandomRange.y:0.#}");

            if (config.Prefab != null)
                _unitInfoBuffer.AppendLine("Прапорець: має prefab");

            if (_unitInfoBuffer.Length == 0)
                return;

            EditorGUILayout.HelpBox(_unitInfoBuffer.ToString().TrimEnd(), MessageType.Info);
        }

        private bool EnsureDatabaseSerialized()
        {
            if (_database == null)
            {
                EditorGUILayout.HelpBox("Спочатку оберіть EconomyDatabaseSO.", MessageType.Warning);
                return false;
            }

            if (_databaseSo == null || _databaseSo.targetObject != _database)
                _databaseSo = new SerializedObject(_database);

            return true;
        }

        private void RebuildSerializedObjects()
        {
            _databaseSo = _database == null ? null : new SerializedObject(_database);
            _rulesConfigurationSo = _rulesConfiguration == null ? null : new SerializedObject(_rulesConfiguration);
            _buildingRegistrySo = _buildingRegistry == null ? null : new SerializedObject(_buildingRegistry);
            _unitRegistrySo = _unitRegistry == null ? null : new SerializedObject(_unitRegistry);
            _simulationResult = null;
            _migrationReport = null;
            _bulkSelection.Clear();
            _selectedByTab.Clear();
            _multiSelectedByTab.Clear();
        }

        private void CreateDatabaseAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Створити Базу Економіки", "EconomyDatabase", "asset", "Оберіть розташування для EconomyDatabaseSO.");
            if (string.IsNullOrEmpty(path))
                return;

            var asset = CreateInstance<EconomyDatabaseSO>();
            asset.SchemaVersion = EconomySchema.CurrentVersion;
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            _database = asset;
            RebuildSerializedObjects();
            EditorGUIUtility.PingObject(asset);
        }

        private void CreateAndAddAsset<T>(string listPropertyName, string defaultAssetName)
            where T : ScriptableObject
        {
            string groupFolder = BuildEconomyGroupFolder(defaultAssetName);
            EnsureFolder(EconomyRootFolder);
            EnsureFolder(groupFolder);

            string baseName = string.IsNullOrWhiteSpace(defaultAssetName)
                ? typeof(T).Name
                : $"Economy{defaultAssetName}";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{groupFolder}/{baseName}.asset");

            var asset = CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            AddAssetReference(listPropertyName, asset);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        private static string BuildEconomyGroupFolder(string defaultAssetName)
        {
            if (string.IsNullOrWhiteSpace(defaultAssetName))
                return EconomyRootFolder + "/Data";

            string cleaned = defaultAssetName.Trim();
            return EconomyRootFolder + "/" + cleaned;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            string[] parts = folderPath.Split('/');
            if (parts.Length == 0)
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

        private void AddAssetReference(string listPropertyName, UnityEngine.Object target)
        {
            if (target == null || !EnsureDatabaseSerialized())
                return;

            Undo.RecordObject(_database, "Економіка: додати посилання");

            _databaseSo.Update();
            var property = _databaseSo.FindProperty(listPropertyName);
            if (property == null)
                return;

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue == target)
                    return;
            }

            var insertIndex = property.arraySize;
            property.InsertArrayElementAtIndex(insertIndex);
            property.GetArrayElementAtIndex(insertIndex).objectReferenceValue = target;
            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);
            SetSelected(target);
        }

        private void CreateAndAssignRulesConfig(SerializedProperty rulesProperty)
        {
            if (rulesProperty == null)
                return;

            var path = EditorUtility.SaveFilePanelInProject(
                "Створити Конфіг Правил Економіки",
                "EconomyRulesConfig",
                "asset",
                "Оберіть розташування для EconomyRulesConfigSO.");

            if (string.IsNullOrEmpty(path))
                return;

            var asset = CreateInstance<EconomyRulesConfigSO>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(_database, "Економіка: призначити конфіг правил");
            rulesProperty.objectReferenceValue = asset;
            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);

            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        private void RemoveAssetReference(string listPropertyName, UnityEngine.Object target)
        {
            if (target == null || !EnsureDatabaseSerialized())
                return;

            Undo.RecordObject(_database, "Економіка: видалити посилання");

            _databaseSo.Update();
            var property = _databaseSo.FindProperty(listPropertyName);
            if (property == null)
                return;

            for (var i = 0; i < property.arraySize; i++)
            {
                var item = property.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue != target)
                    continue;

                property.DeleteArrayElementAtIndex(i);
                break;
            }

            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);
            SetSelected<UnityEngine.Object>(null);
        }

        private static List<T> ReadObjectList<T>(SerializedProperty listProperty)
            where T : UnityEngine.Object
        {
            var result = new List<T>();
            if (listProperty == null || !listProperty.isArray)
                return result;

            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                if (element != null && element.objectReferenceValue is T typed)
                    result.Add(typed);
            }

            return result;
        }

        private static bool IsValidUnityObject(UnityEngine.Object obj)
        {
            return obj != null;
        }

        private static bool IsValidUnityObject<T>(T obj) where T : UnityEngine.Object
        {
            return obj != null;
        }

        private void SelectDefaultSimulationProfiles()
        {
            _simulationProfiles.Clear();
            foreach (var profile in _database.ProductionProfiles.Where(profile => profile != null && profile.IsActiveByDefault))
                _simulationProfiles.Add(profile);
        }

        private static T FindFirstAsset<T>() where T : UnityEngine.Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        private string GetSearch()
        {
            return _searchByTab.TryGetValue(_tab, out var value) ? value : string.Empty;
        }

        private void SetSearch(string value)
        {
            _searchByTab[_tab] = value ?? string.Empty;
        }

        private T GetSelected<T>() where T : UnityEngine.Object
        {
            if (!_selectedByTab.TryGetValue(_tab, out var value))
                return null;

            if (value == null)
                return null;

            if (value is not T typed)
                return null;

            if (!IsValidUnityObject(typed))
            {
                _selectedByTab[_tab] = null;
                var selectedSet = GetMultiSelectionSetForCurrentTab();
                selectedSet.RemoveWhere(obj => !IsValidUnityObject(obj));
                return null;
            }

            return typed;
        }

        private void SetSelected<T>(T value) where T : UnityEngine.Object
        {
            _selectedByTab[_tab] = value;

            var selectedSet = GetMultiSelectionSetForCurrentTab();
            selectedSet.Clear();
            if (value != null)
                selectedSet.Add(value);
        }

        private HashSet<UnityEngine.Object> GetMultiSelectionSetForCurrentTab()
        {
            if (!_multiSelectedByTab.TryGetValue(_tab, out var selectedSet))
            {
                selectedSet = new HashSet<UnityEngine.Object>();
                _multiSelectedByTab[_tab] = selectedSet;
            }

            selectedSet.RemoveWhere(obj => !IsValidUnityObject(obj));
            return selectedSet;
        }

        private static bool IsMultiSelectModifierPressed()
        {
            var evt = Event.current;
            return evt != null && (evt.control || evt.command);
        }

        private void HandleEntityItemClick<T>(T item, HashSet<UnityEngine.Object> selectedSet)
            where T : UnityEngine.Object
        {
            if (!IsMultiSelectModifierPressed())
            {
                SetSelected(item);
                return;
            }

            if (selectedSet.Contains(item))
            {
                selectedSet.Remove(item);
                if (_selectedByTab.TryGetValue(_tab, out var currentSelected) && currentSelected == item)
                    _selectedByTab[_tab] = selectedSet.FirstOrDefault(obj => IsValidUnityObject(obj));
                return;
            }

            selectedSet.Add(item);
            _selectedByTab[_tab] = item;
        }

        private void HandleEntityItemToggle<T>(T item, bool nextSelected, HashSet<UnityEngine.Object> selectedSet)
            where T : UnityEngine.Object
        {
            if (!IsMultiSelectModifierPressed())
            {
                if (nextSelected)
                {
                    SetSelected(item);
                }
                else
                {
                    selectedSet.Clear();
                    _selectedByTab[_tab] = null;
                }

                return;
            }

            if (nextSelected)
            {
                selectedSet.Add(item);
                _selectedByTab[_tab] = item;
                return;
            }

            selectedSet.Remove(item);
            if (_selectedByTab.TryGetValue(_tab, out var currentSelected) && currentSelected == item)
                _selectedByTab[_tab] = selectedSet.FirstOrDefault(obj => IsValidUnityObject(obj));
        }

        // ═══════════════════════════════════════════════════════
        //  МУЛЬТИВИБІР — ВИДАЛЕННЯ КІЛЬКОХ ЕЛЕМЕНТІВ
        // ═══════════════════════════════════════════════════════

        private struct BulkItem
        {
            public UnityEngine.Object Asset;
            public string DisplayName;
            public string Category;
            public string PropertyName;
        }

        private void DrawBulkDeleteTab()
        {
            if (!EnsureDatabaseSerialized())
                return;

            _databaseSo.Update();

            EditorGUILayout.LabelField("Мультивибір та видалення", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(Tips.BulkDeleteTab, MessageType.Info);

            _bulkCategoryIndex = GUILayout.Toolbar(_bulkCategoryIndex, BulkCategoryLabels);
            EditorGUILayout.Space(2f);
            _bulkSearch = EditorGUILayout.TextField("Пошук", _bulkSearch);

            var items = CollectBulkItems();
            if (!string.IsNullOrWhiteSpace(_bulkSearch))
                items = items.Where(i => i.DisplayName.IndexOf(_bulkSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Обрати всі", GUILayout.Width(110f)))
                foreach (var item in items)
                    _bulkSelection.Add(item.Asset);
            if (GUILayout.Button("Зняти всі", GUILayout.Width(110f)))
                _bulkSelection.Clear();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Обрано: {_bulkSelection.Count(s => s != null)}", EditorStyles.miniLabel, GUILayout.Width(90f));
            EditorGUILayout.EndHorizontal();

            _bulkScroll = EditorGUILayout.BeginScrollView(_bulkScroll, "box");
            if (items.Count == 0)
            {
                EditorGUILayout.HelpBox("Немає елементів для відображення.", MessageType.None);
            }
            else
            {
                foreach (var item in items)
                {
                    var wasSelected = _bulkSelection.Contains(item.Asset);
                    EditorGUILayout.BeginHorizontal();
                    var isSelected = EditorGUILayout.Toggle(wasSelected, GUILayout.Width(20f));
                    EditorGUILayout.LabelField($"[{item.Category}]  {item.DisplayName}");
                    if (GUILayout.Button("↗", GUILayout.Width(24f), GUILayout.Height(16f)))
                        EditorGUIUtility.PingObject(item.Asset);
                    EditorGUILayout.EndHorizontal();

                    if (isSelected && !wasSelected) _bulkSelection.Add(item.Asset);
                    else if (!isSelected && wasSelected) _bulkSelection.Remove(item.Asset);
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4f);
            var selectedCount = _bulkSelection.Count(s => s != null);
            using (new EditorGUI.DisabledScope(selectedCount == 0))
            {
                if (GUILayout.Button($"Видалити вибрані ({selectedCount}) зі списків бази", GUILayout.Height(30f)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Підтвердити видалення",
                        $"Буде видалено {selectedCount} позицій зі списків бази даних.\nФайли .asset НЕ видаляються з диску.",
                        "Видалити", "Скасувати"))
                    {
                        BulkRemoveSelected();
                        _bulkSelection.Clear();
                        GUIUtility.ExitGUI();
                    }
                }
            }
        }

        private List<BulkItem> CollectBulkItems()
        {
            var result = new List<BulkItem>();
            var start = _bulkCategoryIndex == 0 ? 0 : _bulkCategoryIndex - 1;
            var end   = _bulkCategoryIndex == 0 ? BulkCategoryDefs.Length : _bulkCategoryIndex;

            for (var ci = start; ci < end; ci++)
            {
                var def = BulkCategoryDefs[ci];
                var prop = _databaseSo.FindProperty(def.PropertyName);
                if (prop == null || !prop.isArray)
                    continue;

                for (var i = 0; i < prop.arraySize; i++)
                {
                    var element = prop.GetArrayElementAtIndex(i);
                    var asset = element?.objectReferenceValue;
                    if (asset == null)
                        continue;

                    result.Add(new BulkItem
                    {
                        Asset        = asset,
                        DisplayName  = asset.name,
                        Category     = def.Label,
                        PropertyName = def.PropertyName,
                    });
                }
            }

            return result;
        }

        private void BulkRemoveSelected()
        {
            if (_bulkSelection.Count == 0)
                return;

            Undo.RecordObject(_database, "Економіка: мультивидалення");
            _databaseSo.Update();

            foreach (var def in BulkCategoryDefs)
            {
                var prop = _databaseSo.FindProperty(def.PropertyName);
                if (prop == null || !prop.isArray)
                    continue;

                for (var i = prop.arraySize - 1; i >= 0; i--)
                {
                    var element = prop.GetArrayElementAtIndex(i);
                    if (element.objectReferenceValue != null && _bulkSelection.Contains(element.objectReferenceValue))
                        prop.DeleteArrayElementAtIndex(i);
                }
            }

            _databaseSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(_database);
            AssetDatabase.SaveAssets();
        }

        // ═══════════════════════════════════════════════════════
        //  МЕТОДИ ДЛЯ ВИБОРУ РЕСУРСІВ ТА ТАЙЛІВ
        // ═══════════════════════════════════════════════════════

        private void RefreshResourceCache()
        {
            _cachedResources.Clear();
            _resourceIconCache.Clear();
            _resourceIdToIndex.Clear();
            
            var guids = AssetDatabase.FindAssets($"t:{nameof(EconomyResourceDefinition)}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var resource = AssetDatabase.LoadAssetAtPath<EconomyResourceDefinition>(path);
                if (resource != null)
                {
                    _resourceIdToIndex[resource.Id] = _cachedResources.Count;
                    _cachedResources.Add(resource);
                    if (resource.Icon != null)
                        _resourceIconCache[resource.Id] = resource.Icon;
                }
            }
            
            // Будуємо UI кеш (DisplayNames + Index mapping)
            _resourceDisplayNames = _cachedResources.Select(r => r.DisplayName).ToArray();
            _resourceCacheDirty = false;
        }

        private void RefreshTileCache()
        {
            _tileIdToIndex.Clear();
            _tileIconCache.Clear();
            
            if (_tileRegistry?.Definitions != null && _tileRegistry.Definitions.Length > 0)
            {
                _tileDisplayIds = new string[_tileRegistry.Definitions.Length];
                for (int i = 0; i < _tileRegistry.Definitions.Length; i++)
                {
                    var definition = _tileRegistry.Definitions[i];
                    var tileId = definition.Id;
                    _tileDisplayIds[i] = tileId;
                    _tileIdToIndex[tileId] = i;

                    var visualPrefab = definition.VisualPrefab;
                    if (visualPrefab == null)
                        continue;

                    var spriteRenderer = visualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                        _tileIconCache[tileId] = spriteRenderer.sprite;
                }
            }
            else
            {
                _tileDisplayIds = new string[0];
            }
            
            _tileCacheDirty = false;
        }

        private Sprite GetResourceIcon(string resourceId)
        {
            if (string.IsNullOrEmpty(resourceId))
                return null;

            if (_resourceIconCache.TryGetValue(resourceId, out var icon))
                return icon;

            // Спробуємо завантажити ресурс на лету
            var resource = _cachedResources.FirstOrDefault(r => r.Id == resourceId);
            if (resource?.Icon != null)
            {
                _resourceIconCache[resourceId] = resource.Icon;
                return resource.Icon;
            }

            return null;
        }

        private Sprite GetTileIcon(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
                return null;

            if (_tileCacheDirty)
                RefreshTileCache();

            return _tileIconCache.TryGetValue(tileId, out var icon) ? icon : null;
        }

        private void DrawResourceSelector(SerializedProperty resourceIdProp)
        {
            if (resourceIdProp == null)
                return;

            // Fallback: якщо кеш не ініціалізований, то ініціалізуємо
            if (_resourceDisplayNames == null || _resourceDisplayNames.Length == 0)
            {
                RefreshResourceCache();
            }

            // Якщо ресурсів ще нема, показуємо текстове поле
            if (_resourceDisplayNames == null || _resourceDisplayNames.Length == 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ресурс:");
                resourceIdProp.stringValue = EditorGUILayout.TextField(resourceIdProp.stringValue);
                EditorGUILayout.EndHorizontal();
                return;
            }

            string currentId = resourceIdProp.stringValue;
            
            // Швидкий пошук індексу через словник замість LINQ
            int selectedIndex = _resourceIdToIndex.TryGetValue(currentId, out var idx) ? idx : 0;

            EditorGUILayout.BeginHorizontal();

            // Вибір ресурсу через dropdown (використовуємо кешований масив DisplayNames)
            int newIndex = EditorGUILayout.Popup("Ресурс", selectedIndex, _resourceDisplayNames);

            if (newIndex >= 0 && newIndex < _cachedResources.Count)
            {
                resourceIdProp.stringValue = _cachedResources[newIndex].Id;
            }

            // Показ іконки ресурсу
            var icon = GetResourceIcon(resourceIdProp.stringValue);
            if (icon != null)
            {
                DrawSpriteIconSmall(icon);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSpriteIconSmall(Sprite sprite)
        {
            if (sprite == null)
                return;

            Rect iconRect = GUILayoutUtility.GetRect(36f, 36f, GUILayout.Width(36f), GUILayout.Height(36f));
            EditorGUI.DrawRect(iconRect, new Color(0.2f, 0.2f, 0.2f));
            
            if (_assetPreviewCache.TryGetValue(sprite, out var preview) && preview != null)
            {
                GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit, true);
                return;
            }

            preview = AssetPreview.GetAssetPreview(sprite);
            if (preview != null)
            {
                _assetPreviewCache[sprite] = preview;
                GUI.DrawTexture(iconRect, preview, ScaleMode.ScaleToFit, true);
            }
            else
            {
                if (AssetPreview.IsLoadingAssetPreview(sprite.GetInstanceID()))
                    Repaint();

                DrawSpriteRectDirect(iconRect, sprite);
            }
        }

        private void DrawTileRequirementsUI(SerializedProperty buildingProperty)
        {
            if (buildingProperty == null)
                return;

            var requiresTilesProp = buildingProperty.FindPropertyRelative("RequiresTiles");
            var tileRequirementsProp = buildingProperty.FindPropertyRelative("TileRequirements");

            if (requiresTilesProp == null || tileRequirementsProp == null)
                return;

            if (_tileCacheDirty)
                RefreshTileCache();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Вимоги до тайлів", EditorStyles.boldLabel);

            requiresTilesProp.boolValue = EditorGUILayout.Toggle("Потребує тайлів", requiresTilesProp.boolValue);

            if (!requiresTilesProp.boolValue)
                return;

            EditorGUILayout.HelpBox("Задайте тайли й умови, необхідні для роботи цієї будівлі.", MessageType.Info);

            EditorGUILayout.LabelField($"Налаштовано вимог: {tileRequirementsProp.arraySize}", EditorStyles.miniLabel);

            if (GUILayout.Button("+ Додати вимогу до тайла", GUILayout.Height(28)))
            {
                tileRequirementsProp.arraySize++;
                var newElement = tileRequirementsProp.GetArrayElementAtIndex(tileRequirementsProp.arraySize - 1);
                newElement.FindPropertyRelative("TileId").stringValue = "";
                newElement.FindPropertyRelative("Radius").intValue = 3;
                newElement.FindPropertyRelative("MinimumTileCount").intValue = 1;
            }

            for (int i = 0; i < tileRequirementsProp.arraySize; i++)
            {
                var reqProp = tileRequirementsProp.GetArrayElementAtIndex(i);
                if (DrawTileRequirementItem(reqProp, i, tileRequirementsProp))
                    break;
            }
        }

        private bool DrawTileRequirementItem(SerializedProperty reqProp, int index, SerializedProperty arrayProp)
        {
            var tileIdProp = reqProp.FindPropertyRelative("TileId");
            var radiusProp = reqProp.FindPropertyRelative("Radius");
            var countProp = reqProp.FindPropertyRelative("MinimumTileCount");
            var tileId = tileIdProp?.stringValue ?? string.Empty;
            var tileIcon = GetTileIcon(tileId);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            DrawEntitySpritePreview(tileIcon, 42f);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Тайл #{index + 1}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(tileId) ? "ID тайла не обрано" : $"ID: {tileId}",
                EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Видалити", GUILayout.Width(90f), GUILayout.Height(24f)))
            {
                arrayProp.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2f);

            if (_tileDisplayIds != null && _tileDisplayIds.Length > 0)
            {
                int selectedIdx = _tileIdToIndex.TryGetValue(tileIdProp.stringValue, out var idx) ? idx : 0;
                selectedIdx = EditorGUILayout.Popup("Тайл", selectedIdx, _tileDisplayIds);
                if (selectedIdx >= 0 && selectedIdx < _tileDisplayIds.Length)
                    tileIdProp.stringValue = _tileDisplayIds[selectedIdx];
            }
            else
            {
                tileIdProp.stringValue = EditorGUILayout.TextField("Тайл", tileIdProp.stringValue);
            }

            EditorGUILayout.BeginHorizontal();
            radiusProp.intValue = EditorGUILayout.IntField("Радіус пошуку", radiusProp.intValue);
            countProp.intValue = EditorGUILayout.IntField("Мін. кількість", countProp.intValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            return false;
        }

        private void RebuildBuildingEntityCache()
        {
            _buildingEntityCache.Clear();
            _filteredBuildingIndices.Clear();

            if (_buildingRegistrySo == null)
                return;

            var buildingsProp = _buildingRegistrySo.FindProperty("Buildings");
            if (buildingsProp == null || !buildingsProp.isArray)
                return;

            string searchLower = string.IsNullOrWhiteSpace(_entitiesBuildingSearch)
                ? null
                : _entitiesBuildingSearch.ToLowerInvariant();

            for (int i = 0; i < buildingsProp.arraySize; i++)
            {
                var element = buildingsProp.GetArrayElementAtIndex(i);
                var id = element.FindPropertyRelative("Id")?.stringValue ?? string.Empty;
                var displayName = element.FindPropertyRelative("DisplayName")?.stringValue ?? string.Empty;
                var categoryProp = element.FindPropertyRelative("Category");
                string category = categoryProp != null
                    ? ((Kruty1918.Moyva.Construction.API.BuildingCategory)categoryProp.enumValueIndex).ToString()
                    : "Unknown";
                var sprite = ResolveBuildingSprite(element);

                string rowLabel = string.IsNullOrWhiteSpace(displayName) ? id : $"{displayName} ({id})";
                if (string.IsNullOrWhiteSpace(rowLabel))
                    rowLabel = $"<порожній #{i}>";

                string searchBlob = (rowLabel + " " + category).ToLowerInvariant();

                var entry = new EntityCacheEntry
                {
                    Index = i,
                    RowLabel = rowLabel,
                    SearchBlob = searchBlob,
                    Category = category,
                    Sprite = sprite
                };
                _buildingEntityCache.Add(entry);

                if (searchLower == null || searchBlob.Contains(searchLower))
                    _filteredBuildingIndices.Add(_buildingEntityCache.Count - 1);
            }

            _lastBuildingRegistrySize = buildingsProp.arraySize;
        }

        private void RebuildUnitEntityCache()
        {
            _unitEntityCache.Clear();
            _filteredUnitIndices.Clear();

            if (_unitRegistrySo == null)
                return;

            var unitsProp = _unitRegistrySo.FindProperty("Configs");
            if (unitsProp == null || !unitsProp.isArray)
                return;

            string searchLower = string.IsNullOrWhiteSpace(_entitiesUnitSearch)
                ? null
                : _entitiesUnitSearch.ToLowerInvariant();

            for (int i = 0; i < unitsProp.arraySize; i++)
            {
                var element = unitsProp.GetArrayElementAtIndex(i);
                var id = element.FindPropertyRelative("TypeId")?.stringValue ?? string.Empty;
                var displayName = element.FindPropertyRelative("DisplayName")?.stringValue ?? string.Empty;
                var roleProp = element.FindPropertyRelative("Role");
                string role = roleProp != null
                    ? ((Kruty1918.Moyva.Units.API.UnitRole)roleProp.enumValueIndex).ToString()
                    : "Unknown";
                var sprite = ResolveUnitSprite(element);

                string rowLabel = string.IsNullOrWhiteSpace(displayName) ? id : $"{displayName} ({id})";
                if (string.IsNullOrWhiteSpace(rowLabel))
                    rowLabel = $"<порожній #{i}>";

                string searchBlob = (rowLabel + " " + role).ToLowerInvariant();

                var entry = new EntityCacheEntry
                {
                    Index = i,
                    RowLabel = rowLabel,
                    SearchBlob = searchBlob,
                    Category = role,
                    Sprite = sprite
                };
                _unitEntityCache.Add(entry);

                if (searchLower == null || searchBlob.Contains(searchLower))
                    _filteredUnitIndices.Add(_unitEntityCache.Count - 1);
            }

            _lastUnitRegistrySize = unitsProp.arraySize;
        }
    }
}
