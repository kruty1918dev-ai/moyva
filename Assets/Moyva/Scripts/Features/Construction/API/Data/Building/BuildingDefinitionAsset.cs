using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Building Definition", fileName = "NewBuildingDefinition")]
    public sealed class BuildingDefinitionAsset : ScriptableObject
    {
        [NonSerialized] private BuildingDefinition _editorRuntimeCache;
        [NonSerialized] private IReadOnlyList<BuildingValidationIssue> _editorValidationCache;
        [NonSerialized] private string _editorPreviewSummaryCache;

        [TabGroup("Основне")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingIdentity Identity = new BuildingIdentity();

        [TabGroup("Вигляд")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingPresentation Presentation = new BuildingPresentation();

        [TabGroup("Зайняті клітинки")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingFootprint Footprint = new BuildingFootprint();

        [TabGroup("Розміщення")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingPlacementRules Placement = new BuildingPlacementRules();

        [TabGroup("Економіка")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingConstructionData Construction = new BuildingConstructionData();

        [TabGroup("Ігрові параметри")]
        [InlineProperty]
        [HideLabel]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public BuildingRuntimeStats RuntimeStats = new BuildingRuntimeStats();

        [TabGroup("Модулі")]
        [SerializeReference]
        [BuildingModuleList]
        [ListDrawerSettings(DraggableItems = false, HideAddButton = true, ShowFoldout = true, DefaultExpandedState = true)]
        [OnValueChanged(nameof(NotifyEditorDataChanged), IncludeChildren = true)]
        public List<BuildingModuleDefinition> Modules = new List<BuildingModuleDefinition>();

        [TabGroup("Огляд")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Підсумок")]
        [PropertyTooltip("Що робить: Показує стислий опис ролі, модулів і параметрів будівлі.\nВплив у грі: Допомагає швидко перевірити конфігурацію перед запуском гри.")]
        public string PreviewSummary => _editorPreviewSummaryCache ??= BuildPreviewSummary(GetEditorRuntimeDefinition());

        [TabGroup("Огляд")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Радіус відкриття туману")]
        [PropertyTooltip("Що робить: Показує підсумковий радіус відкриття туману війни з активного модуля.\nВплив у грі: Визначає, яку область бачить гравець навколо споруди.")]
        public int FogRevealRadius => BuildingDefinitionCapabilities.GetFogRevealRadius(GetEditorRuntimeDefinition());

        [TabGroup("Огляд")]
        [ShowInInspector]
        [ReadOnly]
        [LabelText("Кількість зайнятих клітинок")]
        [PropertyTooltip("Що робить: Показує фактичну кількість клітинок footprint.\nВплив у грі: Від цієї кількості залежить перевірка місця та блокування сітки.")]
        public int OccupiedCellCount => Footprint?.OccupiedCells?.Length > 0
            ? Footprint.OccupiedCells.Length
            : Mathf.Max(1, Footprint?.Size.x ?? 1) * Mathf.Max(1, Footprint?.Size.y ?? 1);

        [TabGroup("Перевірка")]
        [ShowInInspector]
        [ReadOnly]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        [LabelText("Знайдені проблеми")]
        [PropertyTooltip("Що робить: Перевіряє обов'язкові поля, модулі та правила будівлі.\nВплив у грі: Помилки тут можуть блокувати або ламати будівництво.")]
        public IReadOnlyList<BuildingValidationIssue> ValidationIssues
            => _editorValidationCache ??= BuildingValidator.Validate(GetEditorRuntimeDefinition());

        public string Id => Identity != null ? Identity.Id : string.Empty;
        public string DisplayName => Identity != null ? Identity.DisplayName : name;
        public BuildingCategory Category => Identity != null ? Identity.Category : BuildingCategory.Civilian;

        [TabGroup("Перевірка")]
        [Button("Нормалізувати дані", ButtonSizes.Medium)]
        [PropertyTooltip("Що робить: Заповнює відсутні ID, назву та стандартні вкладені об'єкти.\nВплив у грі: Запобігає помилкам через неповну конфігурацію asset.")]
        public void Normalize()
        {
            EnsureDefaults();
            if (string.IsNullOrWhiteSpace(Identity.Id))
                Identity.Id = CreateIdFromName(!string.IsNullOrWhiteSpace(Identity.DisplayName) ? Identity.DisplayName : name);
            if (string.IsNullOrWhiteSpace(Identity.DisplayName))
                Identity.DisplayName = CreateDisplayNameFromId(Identity.Id);
            NotifyEditorDataChanged();
        }

        /// <summary>
        /// Інвалідує лише editor-preview cache. Не змінює серіалізовані gameplay-дані.
        /// </summary>
        public void NotifyEditorDataChanged()
        {
            _editorRuntimeCache = null;
            _editorValidationCache = null;
            _editorPreviewSummaryCache = null;
        }

        public BuildingDefinition ToRuntimeDefinition()
        {
            EnsureDefaults();

            return new BuildingDefinition
            {
                Id = Identity.Id,
                DisplayName = Identity.DisplayName,
                Category = Identity.Category,
                Icon = Presentation.Icon,
                RuntimePreview = Presentation.RuntimePreview,
                Prefab = Presentation.Prefab,
                Footprint = CloneFootprint(Footprint),
                VisualYOffset = Presentation.VisualYOffset,
                ConstructionCost = CloneCost(Construction.Cost),
                Modules = CloneModuleList(Modules),
                MaxHp = Mathf.Max(1, RuntimeStats.MaxHp),
                CanPlaceInFog = Placement.CanPlaceInFog,
                RequiredTerrainIds = Placement.RequiredTerrainIds != null
                    ? (string[])Placement.RequiredTerrainIds.Clone()
                    : System.Array.Empty<string>(),
                UseCustomTownHallRules = true,
                RequireTownHallInRange = Placement.RequiresSettlementInfluence,
                BlockIfTownHallAlreadyInRange = Placement.BlockIfSettlementCenterInRange,
                TownHallProximityRadiusOverride = ResolvePlacementRadius(),
            };
        }

        public void ApplyLegacy(BuildingDefinition legacy)
        {
            EnsureDefaults();
            if (legacy == null)
                return;

            Identity.Id = legacy.Id;
            Identity.DisplayName = legacy.DisplayName;
            Identity.Category = legacy.Category;
            Identity.Role = GuessRole(legacy);

            Presentation.Icon = legacy.Icon;
            Presentation.RuntimePreview = legacy.RuntimePreview;
            Presentation.Prefab = legacy.Prefab;
            Presentation.VisualYOffset = legacy.VisualYOffset;

            Footprint = CloneFootprint(legacy.Footprint);

            Construction.Cost = CloneCost(legacy.ConstructionCost);

            Placement.RequiresSettlementInfluence = legacy.RequireTownHallInRange;
            Placement.CanPlaceInFog = legacy.CanPlaceInFog;
            Placement.RequiredTerrainIds = legacy.RequiredTerrainIds != null
                ? (string[])legacy.RequiredTerrainIds.Clone()
                : System.Array.Empty<string>();
            Placement.BlockIfSettlementCenterInRange = legacy.BlockIfTownHallAlreadyInRange;
            Placement.InfluenceRadius = Mathf.Max(0, legacy.TownHallProximityRadiusOverride);
            Placement.CreatesSettlementInfluence =
                BuildingDefinitionCapabilities.IsTownHall(legacy)
                || BuildingDefinitionCapabilities.IsCastle(legacy);

            RuntimeStats.MaxHp = Mathf.Max(1, legacy.MaxHp);
            Modules = CloneModuleList(legacy.Modules);
            NotifyEditorDataChanged();
        }

        private void OnValidate() => NotifyEditorDataChanged();

        private BuildingDefinition GetEditorRuntimeDefinition()
            => _editorRuntimeCache ??= ToRuntimeDefinition();

        private int ResolvePlacementRadius()
        {
            if (Placement == null)
                return 0;

            if (Placement.MinDistanceFromSettlementCenters > 0)
                return Placement.MinDistanceFromSettlementCenters;

            return Mathf.Max(0, Placement.InfluenceRadius);
        }

        private void EnsureDefaults()
        {
            Identity ??= new BuildingIdentity();
            Presentation ??= new BuildingPresentation();
            Footprint ??= new BuildingFootprint();
            Placement ??= new BuildingPlacementRules();
            Construction ??= new BuildingConstructionData();
            RuntimeStats ??= new BuildingRuntimeStats();
            Modules ??= new List<BuildingModuleDefinition>();
        }

        private static List<BuildingDefinition.BuildingConstructionCostEntry> CloneCost(
            IReadOnlyList<BuildingDefinition.BuildingConstructionCostEntry> source)
        {
            var result = new List<BuildingDefinition.BuildingConstructionCostEntry>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                var entry = source[i];
                if (entry == null)
                    continue;

                result.Add(new BuildingDefinition.BuildingConstructionCostEntry
                {
                    ResourceId = entry.ResourceId,
                    Amount = entry.Amount,
                });
            }

            return result;
        }

        private static BuildingFootprint CloneFootprint(BuildingFootprint source)
        {
            source ??= new BuildingFootprint();
            return new BuildingFootprint
            {
                Size = new Vector2Int(Mathf.Max(1, source.Size.x), Mathf.Max(1, source.Size.y)),
                Anchor = source.Anchor,
                CustomAnchor = source.CustomAnchor,
                BlocksMovement = source.BlocksMovement,
                BlocksConstruction = source.BlocksConstruction,
                RequiresFlatGround = source.RequiresFlatGround,
                OccupiedCells = source.OccupiedCells != null
                    ? (Vector2Int[])source.OccupiedCells.Clone()
                    : Array.Empty<Vector2Int>(),
                EntranceCells = source.EntranceCells != null
                    ? (Vector2Int[])source.EntranceCells.Clone()
                    : Array.Empty<Vector2Int>(),
                RoadConnectionCells = source.RoadConnectionCells != null
                    ? (Vector2Int[])source.RoadConnectionCells.Clone()
                    : Array.Empty<Vector2Int>(),
            };
        }

        private static List<BuildingModuleDefinition> CloneModuleList(IReadOnlyList<BuildingModuleDefinition> source)
        {
            var result = new List<BuildingModuleDefinition>();
            if (source == null)
                return result;

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                    result.Add(source[i]);
            }

            return result;
        }

        private static BuildingRole GuessRole(BuildingDefinition definition)
        {
            if (definition == null)
                return BuildingRole.Support;
            if (BuildingDefinitionCapabilities.HasEnabledModule<GateBuildingModule>(definition))
                return BuildingRole.Gate;
            if (BuildingDefinitionCapabilities.HasEnabledModule<WallBuildingModule>(definition))
                return BuildingRole.Wall;
            if (BuildingDefinitionCapabilities.IsTownHall(definition) || BuildingDefinitionCapabilities.IsCastle(definition))
                return BuildingRole.SettlementCenter;
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                return BuildingRole.Housing;
            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                return BuildingRole.Storage;
            if (BuildingDefinitionCapabilities.GetRequiredWorkers(definition) > 0
                || !string.IsNullOrWhiteSpace(BuildingDefinitionCapabilities.GetIndustrialResourceId(definition)))
                return BuildingRole.Production;
            if (definition.Category == BuildingCategory.Military)
                return BuildingRole.Defense;
            return BuildingRole.Support;
        }

        private static string CreateIdFromName(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return "new-building";

            var chars = source.Trim().ToLowerInvariant().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                bool valid = char.IsLetterOrDigit(chars[i]) || chars[i] == '-' || chars[i] == '_';
                if (!valid)
                    chars[i] = '-';
            }

            return new string(chars).Trim('-');
        }

        private static string CreateDisplayNameFromId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "Нова будівля";

            var parts = id.Replace('_', '-').Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                    continue;

                parts[i] = char.ToUpperInvariant(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1) : string.Empty);
            }

            return parts.Length > 0 ? string.Join(" ", parts) : id;
        }

        private string BuildPreviewSummary(BuildingDefinition runtimeDefinition)
        {
            EnsureDefaults();
            string prefab = Presentation.Prefab != null ? Presentation.Prefab.name : "<префаб не задано>";
            string footprint = $"{Mathf.Max(1, Footprint.Size.x)}x{Mathf.Max(1, Footprint.Size.y)}";
            int fog = BuildingDefinitionCapabilities.GetFogRevealRadius(runtimeDefinition);
            int modules = Modules != null ? Modules.Count : 0;
            return $"{DisplayName} ({Id}) | префаб: {prefab}, клітинки: {footprint}, модулі: {modules}, відкриття туману: {fog}";
        }
    }
}
