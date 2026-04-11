using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
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
        }

        private static readonly string[] TabLabels =
        {
            " Ресурси",
            " Overridable",
            " Settlement Rules",
            " Population Rules",
            " Workforce Rules",
            " Production Rules",
            " Storage Rules",
            " Caravan Rules",
            " Market Rules",
            " Consumption/Needs",
            " Death/Mortality",
            " Building Rules",
            " AI Extensibility",
            " Валідація",
            " Симуляція",
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
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private Vector2 _validationScroll;
        private Vector2 _simulationScroll;
        private Vector2 _rulesScroll;

        private List<EconomyValidationIssue> _validationIssues = new List<EconomyValidationIssue>();
        private EconomyMigrationReport _migrationReport;

        private EconomySettlementDefinition _simulationSettlement;
        private float _simulationDurationMinutes = 10f;
        private readonly HashSet<EconomyProductionProfile> _simulationProfiles = new HashSet<EconomyProductionProfile>();
        private EconomySimulationResult _simulationResult;

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

            RebuildSerializedObjects();
        }

        private void OnDisable()
        {
            foreach (var editor in _cachedEditors.Values)
            {
                if (editor != null)
                    DestroyImmediate(editor);
            }

            _cachedEditors.Clear();
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
                    DrawRuleCategoryTab(EconomyRuleCategory.SettlementRules, "Settlement Rules");
                    break;

                case Tab.PopulationRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.PopulationRules, "Population Rules");
                    break;

                case Tab.WorkforceRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.WorkforceRules, "Workforce Rules");
                    break;

                case Tab.ProductionRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.ProductionRules, "Production Rules");
                    break;

                case Tab.StorageRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.StorageRules, "Storage Rules");
                    break;

                case Tab.CaravanRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.CaravanRules, "Caravan Rules");
                    break;

                case Tab.MarketRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.MarketRules, "Market Rules");
                    break;

                case Tab.ConsumptionNeedsRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.ConsumptionNeeds, "Consumption/Needs Rules");
                    break;

                case Tab.DeathMortalityRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.DeathMortality, "Death/Mortality Rules");
                    break;

                case Tab.BuildingRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.BuildingRules, "Building Rules");
                    break;

                case Tab.AiExtensibilityRules:
                    DrawRuleCategoryTab(EconomyRuleCategory.AIExtensibility, "AI Extensibility Rules");
                    break;

                case Tab.Validation:
                    DrawValidationTab();
                    break;

                case Tab.Simulation:
                    DrawSimulationTab();
                    break;
            }
        }

        private void DrawOverridableParametersTab()
        {
            if (!EnsureRulesConfigurationSerialized())
                return;

            _rulesConfigurationSo.Update();

            EditorGUILayout.LabelField("Overridable Parameters", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Тут визначається, які параметри гравець може змінювати у розширених налаштуваннях світу.",
                MessageType.Info);

            DrawRulesConfigurationSelector();

            var parameters = _rulesConfigurationSo.FindProperty("_parameters");
            if (parameters == null)
            {
                EditorGUILayout.HelpBox("Список '_parameters' не знайдено в EconomyRulesConfiguration.", MessageType.Error);
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
            EditorGUILayout.HelpBox("Редагуйте параметри категорії. Ці значення використовуються як дефолтний шаблон для runtime і world override.", MessageType.Info);

            DrawRulesConfigurationSelector();

            var parameters = _rulesConfigurationSo.FindProperty("_parameters");
            if (parameters == null)
            {
                EditorGUILayout.HelpBox("Список '_parameters' не знайдено в EconomyRulesConfiguration.", MessageType.Error);
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

        private void DrawRulesConfigurationSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            var next = (EconomyRulesConfiguration)EditorGUILayout.ObjectField(
                new GUIContent("Rules Template"),
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
            _tab = (Tab)GUILayout.Toolbar((int)_tab, TabLabels);
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

        private static void DrawSpriteRect(Rect rect, Sprite sprite)
        {
            if (sprite == null)
                return;

            Texture preview = AssetPreview.GetAssetPreview(sprite);
            if (preview == null)
                preview = AssetPreview.GetMiniThumbnail(sprite);

            if (preview != null)
                GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit, true);
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
    }
}
