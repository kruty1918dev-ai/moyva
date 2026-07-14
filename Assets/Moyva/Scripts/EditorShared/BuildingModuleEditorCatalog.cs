using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Editor.Shared
{
    public sealed class BuildingModuleEditorDescriptor
    {
        public BuildingModuleEditorDescriptor(
            Type moduleType,
            string category,
            string displayName,
            string description,
            Func<BuildingModuleDefinition> factory,
            params string[] searchAliases)
        {
            ModuleType = moduleType ?? throw new ArgumentNullException(nameof(moduleType));
            Category = category ?? string.Empty;
            DisplayName = displayName ?? moduleType.Name;
            Description = description ?? string.Empty;
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            SearchAliases = searchAliases ?? Array.Empty<string>();
        }

        public Type ModuleType { get; }
        public string TypeName => ModuleType.Name;
        public string Category { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public Func<BuildingModuleDefinition> Factory { get; }
        public IReadOnlyList<string> SearchAliases { get; }
        public string MenuPath => $"{Category}/{DisplayName}";

        public BuildingModuleDefinition Create() => Factory();

        public bool MatchesSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;

            query = query.Trim();
            if (Contains(Category, query)
                || Contains(DisplayName, query)
                || Contains(TypeName, query)
                || Contains(Description, query))
            {
                return true;
            }

            for (int index = 0; index < SearchAliases.Count; index++)
            {
                if (Contains(SearchAliases[index], query))
                    return true;
            }

            return false;
        }

        private static bool Contains(string source, string query)
            => !string.IsNullOrWhiteSpace(source)
               && source.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// Єдине джерело назв, описів і правил доступності модулів у всіх редакторах Moyva.
    /// </summary>
    public static class BuildingModuleEditorCatalog
    {
        private static readonly BuildingModuleEditorDescriptor[] Descriptors =
        {
            Option<TownHallBuildingModule>(
                "Поселення", "Модуль ратуші",
                "Позначає будівлю як ратушу та задає радіус будівництва поселення.",
                "town hall", "ратуша", "центр"),
            Option<CastleBuildingModule>(
                "Поселення", "Модуль замку",
                "Створює ключовий центр фракції з гарнізоном і радіусом виключення.",
                "castle", "замок", "столиця"),
            Option<SettlementCenterBuildingModule>(
                "Поселення", "Центр поселення",
                "Створює зону впливу поселення та мінімальну відстань до інших центрів.",
                "settlement center", "поселення", "вплив"),

            Option<HousingBuildingModule>(
                "Економіка", "Житловий модуль",
                "Додає місткість житла та, за потреби, можливість гарнізону.",
                "housing", "житло", "населення"),
            Option<WarehouseBuildingModule>(
                "Економіка", "Модуль складу",
                "Надає будівлі зберігання матеріальних ресурсів.",
                "warehouse", "склад", "ресурси"),
            Option<BarnBuildingModule>(
                "Економіка", "Модуль амбару",
                "Надає будівлі зберігання харчових ресурсів.",
                "barn", "амбар", "їжа"),
            Option<ProductionBuildingModule>(
                "Економіка", "Виробничий модуль",
                "Додає рецепти виробництва, потребу в робітниках і пріоритет.",
                "production", "виробництво", "рецепт"),
            Option<WorkforceBuildingModule>(
                "Економіка", "Модуль робочої сили",
                "Задає кількість, тип і пріоритет робітників для будівлі.",
                "workforce", "робітники", "працівники"),
            Option<StorageBuildingModule>(
                "Економіка", "Універсальне сховище",
                "Налаштовує тип, місткість і дозволені ресурси сховища.",
                "storage", "сховище", "місткість"),
            Option<WorkerlessBuildingModule>(
                "Економіка", "Модуль без робітників",
                "Позначає будівлю як таку, що працює без призначеного населення.",
                "workerless", "без робітників"),

            Option<DefenseBuildingModule>(
                "Оборона", "Оборонний модуль",
                "Задає броню, гарнізон, дальність, шкоду та бонус огляду.",
                "defense", "оборона", "атака"),
            Option<WallBuildingModule>(
                "Оборона", "Модуль стіни",
                "Надає будівлі поведінку сегмента стіни.",
                "wall", "стіна"),
            Option<GateBuildingModule>(
                "Оборона", "Модуль воріт",
                "Надає будівлі поведінку воріт і швидкість відкриття.",
                "gate", "ворота"),

            Option<FogRevealBuildingModule>(
                "Світ", "Відкриття туману війни",
                "Відкриває туман війни навколо збудованої або активної споруди.",
                "fog reveal", "туман", "видимість"),
            Option<TileRequirementBuildingModule>(
                "Світ", "Вимоги до тайлів",
                "Дозволяє будівництво лише за наявності потрібних типів місцевості.",
                "tile requirement", "тайли", "місцевість"),

            Option<BuildingPerPlayerLimitModule>(
                "Правила", "Ліміт будівель на гравця",
                "Обмежує кількість копій цієї будівлі для кожного власника. Значення 0 вимикає ліміт.",
                "limit", "maximum", "максимум", "ліміт", "гравець"),
        };

        public static IReadOnlyList<BuildingModuleEditorDescriptor> Options => Descriptors;

        public static BuildingModuleEditorDescriptor Find(Type moduleType)
        {
            if (moduleType == null)
                return null;

            for (int index = 0; index < Descriptors.Length; index++)
            {
                if (Descriptors[index].ModuleType == moduleType)
                    return Descriptors[index];
            }

            return null;
        }

        public static BuildingModuleEditorDescriptor Find(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            for (int index = 0; index < Descriptors.Length; index++)
            {
                if (string.Equals(Descriptors[index].TypeName, typeName, StringComparison.Ordinal))
                    return Descriptors[index];
            }

            return null;
        }

        public static string GetConflictReason(
            IReadOnlyList<BuildingModuleDefinition> currentModules,
            Type candidateType)
        {
            if (candidateType == null)
                return "Невідомий тип модуля.";

            bool hasTownHall = Has<TownHallBuildingModule>(currentModules);
            bool hasHousing = Has<HousingBuildingModule>(currentModules);
            bool hasWorkerless = Has<WorkerlessBuildingModule>(currentModules);
            bool hasWall = Has<WallBuildingModule>(currentModules);
            bool hasGate = Has<GateBuildingModule>(currentModules);
            bool hasProduction = Has<ProductionBuildingModule>(currentModules);

            for (int index = 0; index < (currentModules?.Count ?? 0); index++)
            {
                BuildingModuleDefinition existing = currentModules[index];
                if (existing != null && existing.GetType() == candidateType)
                    return "Цей модуль уже додано до будівлі.";
            }

            if (candidateType == typeof(TownHallBuildingModule) && hasHousing)
                return "Модуль ратуші несумісний із житловим модулем.";
            if (candidateType == typeof(HousingBuildingModule))
            {
                if (hasTownHall)
                    return "Житловий модуль несумісний із модулем ратуші.";
                if (hasWorkerless || hasWall || hasGate)
                    return "Житловий модуль несумісний із модулем без робітників, стіною або воротами.";
            }
            if ((candidateType == typeof(WorkerlessBuildingModule)
                 || candidateType == typeof(WallBuildingModule)
                 || candidateType == typeof(GateBuildingModule))
                && hasHousing)
            {
                return "Цей модуль несумісний із житловим модулем.";
            }
            if ((candidateType == typeof(WorkerlessBuildingModule)
                 || candidateType == typeof(WallBuildingModule)
                 || candidateType == typeof(GateBuildingModule))
                && hasProduction)
            {
                return "Цей модуль несумісний із виробничим модулем.";
            }
            if (candidateType == typeof(ProductionBuildingModule)
                && (hasWorkerless || hasWall || hasGate))
            {
                return "Виробничий модуль несумісний із модулем без робітників, стіною або воротами.";
            }

            return null;
        }

        private static BuildingModuleEditorDescriptor Option<TModule>(
            string category,
            string displayName,
            string description,
            params string[] aliases)
            where TModule : BuildingModuleDefinition, new()
        {
            return new BuildingModuleEditorDescriptor(
                typeof(TModule),
                category,
                displayName,
                description,
                () => new TModule(),
                aliases);
        }

        private static bool Has<TModule>(IReadOnlyList<BuildingModuleDefinition> modules)
            where TModule : BuildingModuleDefinition
        {
            for (int index = 0; index < (modules?.Count ?? 0); index++)
            {
                if (modules[index] is TModule)
                    return true;
            }

            return false;
        }
    }
}
