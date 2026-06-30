using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor
{
    [CreateAssetMenu(menuName = "Moyva/Construction/Templates/Building Archetype", fileName = "BuildingArchetype")]
    public sealed class BuildingArchetypeSO : ScriptableObject
    {
        [Required]
        public string DisplayName = "Building Archetype";

        public BuildingRole Role = BuildingRole.Support;
        public BuildingCategory Category = BuildingCategory.Civilian;

        [InlineProperty]
        public BuildingFootprint Footprint = new BuildingFootprint();

        [InlineProperty]
        public BuildingPlacementRules Placement = new BuildingPlacementRules();

        [InlineProperty]
        public BuildingConstructionData Construction = new BuildingConstructionData();

        [InlineProperty]
        public BuildingRuntimeStats RuntimeStats = new BuildingRuntimeStats();

        [SerializeReference]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        public List<BuildingModuleDefinition> Modules = new List<BuildingModuleDefinition>();

        public void ApplyTo(BuildingDefinitionAsset asset)
        {
            if (asset == null)
                return;

            asset.Normalize();
            asset.Identity.Role = Role;
            asset.Identity.Category = Category;
            asset.Footprint = CloneFootprint(Footprint);
            asset.Placement = ClonePlacement(Placement);
            asset.Construction = CloneConstruction(Construction);
            asset.RuntimeStats = CloneRuntimeStats(RuntimeStats);
            asset.Modules = Modules != null
                ? new List<BuildingModuleDefinition>(Modules)
                : new List<BuildingModuleDefinition>();
        }

        private static BuildingFootprint CloneFootprint(BuildingFootprint source)
        {
            source ??= new BuildingFootprint();
            return new BuildingFootprint
            {
                Size = source.Size,
                Anchor = source.Anchor,
                CustomAnchor = source.CustomAnchor,
                BlocksMovement = source.BlocksMovement,
                BlocksConstruction = source.BlocksConstruction,
                RequiresFlatGround = source.RequiresFlatGround,
                OccupiedCells = source.OccupiedCells != null ? (Vector2Int[])source.OccupiedCells.Clone() : System.Array.Empty<Vector2Int>(),
                EntranceCells = source.EntranceCells != null ? (Vector2Int[])source.EntranceCells.Clone() : System.Array.Empty<Vector2Int>(),
                RoadConnectionCells = source.RoadConnectionCells != null ? (Vector2Int[])source.RoadConnectionCells.Clone() : System.Array.Empty<Vector2Int>(),
            };
        }

        private static BuildingPlacementRules ClonePlacement(BuildingPlacementRules source)
        {
            source ??= new BuildingPlacementRules();
            return new BuildingPlacementRules
            {
                CanPlaceInFog = source.CanPlaceInFog,
                RequiresSettlementInfluence = source.RequiresSettlementInfluence,
                CreatesSettlementInfluence = source.CreatesSettlementInfluence,
                BlockIfSettlementCenterInRange = source.BlockIfSettlementCenterInRange,
                InfluenceRadius = source.InfluenceRadius,
                MinDistanceFromSettlementCenters = source.MinDistanceFromSettlementCenters,
                RequiredTerrainIds = source.RequiredTerrainIds != null ? (string[])source.RequiredTerrainIds.Clone() : System.Array.Empty<string>(),
                RequiredNeighborOffsets = source.RequiredNeighborOffsets != null ? (Vector2Int[])source.RequiredNeighborOffsets.Clone() : System.Array.Empty<Vector2Int>(),
                RequiresWaterNearby = source.RequiresWaterNearby,
                RequiresForestNearby = source.RequiresForestNearby,
                RequiresMountainNearby = source.RequiresMountainNearby,
                RequiresRoadNearby = source.RequiresRoadNearby,
                NearbyTileRequirements = source.NearbyTileRequirements != null ? (TileRequirementDefinition[])source.NearbyTileRequirements.Clone() : System.Array.Empty<TileRequirementDefinition>(),
            };
        }

        private static BuildingConstructionData CloneConstruction(BuildingConstructionData source)
        {
            source ??= new BuildingConstructionData();
            return new BuildingConstructionData
            {
                Cost = source.Cost != null
                    ? new List<BuildingDefinition.BuildingConstructionCostEntry>(source.Cost)
                    : new List<BuildingDefinition.BuildingConstructionCostEntry>(),
                BuildTurns = source.BuildTurns,
                RequiresBuilder = source.RequiresBuilder,
                WorkRequired = source.WorkRequired,
            };
        }

        private static BuildingRuntimeStats CloneRuntimeStats(BuildingRuntimeStats source)
        {
            source ??= new BuildingRuntimeStats();
            return new BuildingRuntimeStats
            {
                MaxHp = source.MaxHp,
                Armor = source.Armor,
                Flags = source.Flags,
                RuntimeTags = source.RuntimeTags != null ? new List<string>(source.RuntimeTags) : new List<string>(),
            };
        }
    }
}
