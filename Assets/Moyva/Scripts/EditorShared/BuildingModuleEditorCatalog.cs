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

            Option<GarrisonBuildingModule>(
                "Оборона", "Гарнізон",
                "Єдине місце для налаштування місткості гарнізону будь-якої будівлі.",
                "garrison", "гарнізон", "захисники"),

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

            Option<TerrainPlacementRuleModule>(
                "Правила", "Правила terrain",
                "Успадковує, замінює або вимикає terrain-правила для конкретної будівлі.",
                "terrain", "земля", "рівень", "placement"),
            Option<FogPlacementRuleModule>(
                "Правила", "Правила туману",
                "Налаштовує потрібний стан Fog of War для розміщення.",
                "fog", "туман", "visibility"),
            Option<SettlementInfluenceRequirementBuildingModule>(
                "Правила", "Вимога впливу поселення",
                "Налаштовує залежність від зони поселення без перевірки типу будівлі.",
                "influence", "вплив", "поселення"),
            Option<SpacingPlacementRuleModule>(
                "Правила", "Відступ при розміщенні",
                "Перевизначає глобальний мінімальний відступ для цієї будівлі.",
                "spacing", "відступ", "distance"),
            Option<ReplacementPlacementRuleModule>(
                "Правила", "Заміна будівлі",
                "Дозволяє ставити будівлю лише як заміну налаштованих ID або тегів.",
                "replacement", "replace", "заміна"),
            Option<BuildingPrerequisiteModule>(
                "Правила", "Передумови будівлі",
                "Вимагає побудовані будівлі з налаштованими ID або тегами.",
                "prerequisite", "unlock", "передумова"),
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

        public static bool IsLegacyModule(Type moduleType)
            => moduleType == typeof(WarehouseBuildingModule)
                || moduleType == typeof(BarnBuildingModule)
                || moduleType == typeof(WorkerlessBuildingModule);

        public static string GetConflictReason(
            IReadOnlyList<BuildingModuleDefinition> currentModules,
            Type candidateType)
        {
            if (candidateType == null)
                return "Невідомий тип модуля.";

            bool hasWall =
                Has<WallBuildingModule>(currentModules);
            bool hasGate =
                Has<GateBuildingModule>(currentModules);
            bool hasProduction =
                Has<ProductionBuildingModule>(currentModules);
            bool hasGarrison =
                Has<GarrisonBuildingModule>(currentModules);

            for (int index = 0;
                 index < (currentModules?.Count ?? 0);
                 index++)
            {
                BuildingModuleDefinition existing =
                    currentModules[index];
                if (existing != null
                    && existing.GetType() == candidateType)
                {
                    return "Цей модуль вже додано до будівлі.";
                }
            }

            // Housing, TownHall and Workerless are orthogonal capabilities.
            // Workerless may coexist with Production when recipes themselves
            // do not require workers; runtime validation owns that rule.

            if (candidateType == typeof(WallBuildingModule)
                && hasGate)
            {
                return "Будівля не може одночасно бути стіною і воротами.";
            }

            if (candidateType == typeof(GateBuildingModule)
                && hasWall)
            {
                return "Будівля не може одночасно бути воротами і стіною.";
            }

            if (candidateType == typeof(GarrisonBuildingModule)
                && (hasWall || hasGate))
            {
                return "Гарнізон не додається до сегмента стіни або воріт.";
            }

            if ((candidateType == typeof(WallBuildingModule)
                 || candidateType == typeof(GateBuildingModule))
                && hasGarrison)
            {
                return "Стіна або ворота несумісні з окремим модулем гарнізону.";
            }

            if ((candidateType == typeof(WallBuildingModule)
                 || candidateType == typeof(GateBuildingModule))
                && hasProduction)
            {
                return "Стіна або ворота несумісні з виробничим модулем.";
            }

            if (candidateType == typeof(ProductionBuildingModule)
                && (hasWall || hasGate))
            {
                return "Виробничий модуль несумісний зі стіною або воротами.";
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
                if (modules[index] is TModule typed
                    && typed.IsEnabled)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
