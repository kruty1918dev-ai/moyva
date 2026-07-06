using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Building Definition", fileName = "NewBuildingDefinition")]
    public sealed class BuildingDefinitionAsset : ScriptableObject
    {
        [TabGroup("Basic")]
        [InlineProperty]
        [HideLabel]
        public BuildingIdentity Identity = new BuildingIdentity();

        [TabGroup("Visual")]
        [InlineProperty]
        [HideLabel]
        public BuildingPresentation Presentation = new BuildingPresentation();

        [TabGroup("Footprint")]
        [InlineProperty]
        [HideLabel]
        public BuildingFootprint Footprint = new BuildingFootprint();

        [TabGroup("Placement")]
        [InlineProperty]
        [HideLabel]
        public BuildingPlacementRules Placement = new BuildingPlacementRules();

        [TabGroup("Economy")]
        [InlineProperty]
        [HideLabel]
        public BuildingConstructionData Construction = new BuildingConstructionData();

        [TabGroup("Runtime")]
        [InlineProperty]
        [HideLabel]
        public BuildingRuntimeStats RuntimeStats = new BuildingRuntimeStats();

        [TabGroup("Modules")]
        [SerializeReference]
        [ListDrawerSettings(DraggableItems = true, ShowFoldout = true, DefaultExpandedState = true)]
        public List<BuildingModuleDefinition> Modules = new List<BuildingModuleDefinition>();

        [TabGroup("Preview")]
        [ShowInInspector]
        [ReadOnly]
        public string PreviewSummary => BuildPreviewSummary();

        [TabGroup("Preview")]
        [ShowInInspector]
        [ReadOnly]
        public int FogRevealRadius => BuildingDefinitionCapabilities.GetFogRevealRadius(ToRuntimeDefinition());

        [TabGroup("Preview")]
        [ShowInInspector]
        [ReadOnly]
        public int OccupiedCellCount => Footprint?.OccupiedCells?.Length > 0
            ? Footprint.OccupiedCells.Length
            : Mathf.Max(1, Footprint?.Size.x ?? 1) * Mathf.Max(1, Footprint?.Size.y ?? 1);

        [TabGroup("Validation")]
        [ShowInInspector]
        [ReadOnly]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public IReadOnlyList<BuildingValidationIssue> ValidationIssues => BuildingValidator.Validate(ToRuntimeDefinition());

        public string Id => Identity != null ? Identity.Id : string.Empty;
        public string DisplayName => Identity != null ? Identity.DisplayName : name;
        public BuildingCategory Category => Identity != null ? Identity.Category : BuildingCategory.Civilian;

        [TabGroup("Validation")]
        [Button(ButtonSizes.Medium)]
        public void Normalize()
        {
            EnsureDefaults();
            if (string.IsNullOrWhiteSpace(Identity.Id))
                Identity.Id = CreateIdFromName(!string.IsNullOrWhiteSpace(Identity.DisplayName) ? Identity.DisplayName : name);
            if (string.IsNullOrWhiteSpace(Identity.DisplayName))
                Identity.DisplayName = CreateDisplayNameFromId(Identity.Id);
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
                VisualYOffset = Presentation.VisualYOffset,
                ConstructionCost = CloneCost(Construction.Cost),
                Modules = CloneModuleList(Modules),
                MaxHp = Mathf.Max(1, RuntimeStats.MaxHp),
                CanPlaceInFog = Placement.CanPlaceInFog,
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

            Construction.Cost = CloneCost(legacy.ConstructionCost);

            Placement.RequiresSettlementInfluence = legacy.RequireTownHallInRange;
            Placement.CanPlaceInFog = legacy.CanPlaceInFog;
            Placement.BlockIfSettlementCenterInRange = legacy.BlockIfTownHallAlreadyInRange;
            Placement.InfluenceRadius = Mathf.Max(0, legacy.TownHallProximityRadiusOverride);
            Placement.CreatesSettlementInfluence =
                BuildingDefinitionCapabilities.IsTownHall(legacy)
                || BuildingDefinitionCapabilities.IsCastle(legacy);

            RuntimeStats.MaxHp = Mathf.Max(1, legacy.MaxHp);
            Modules = CloneModuleList(legacy.Modules);
        }

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
                return "New Building";

            var parts = id.Replace('_', '-').Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                    continue;

                parts[i] = char.ToUpperInvariant(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1) : string.Empty);
            }

            return parts.Length > 0 ? string.Join(" ", parts) : id;
        }

        private string BuildPreviewSummary()
        {
            EnsureDefaults();
            string prefab = Presentation.Prefab != null ? Presentation.Prefab.name : "<missing prefab>";
            string footprint = $"{Mathf.Max(1, Footprint.Size.x)}x{Mathf.Max(1, Footprint.Size.y)}";
            int fog = BuildingDefinitionCapabilities.GetFogRevealRadius(ToRuntimeDefinition());
            int modules = Modules != null ? Modules.Count : 0;
            return $"{DisplayName} ({Id}) | prefab={prefab}, footprint={footprint}, modules={modules}, fogReveal={fog}";
        }
    }
}
