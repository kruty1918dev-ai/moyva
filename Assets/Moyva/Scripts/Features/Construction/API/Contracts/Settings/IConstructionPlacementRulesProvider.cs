using Kruty1918.Moyva.WorldCreation.API;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionPlacementRulesProvider
    {
        int MinSpacing { get; }
        int TownHallBuildRadius { get; }
        bool EnableInfluenceZoneRules { get; }
        bool EnableTerrainRules { get; }
        bool EnableFogRules { get; }
        bool RequireVisibleFogTile { get; }
        bool AllowBuildingOnWater { get; }

        /// <summary>
        /// Canonical "build anywhere on free dry land" policy: when enabled,
        /// placement ignores terrain/biome/level/adjacency/spacing, fog
        /// visibility and settlement-influence location rules. The only
        /// natural-surface rejection is water overlap; bounds, occupancy,
        /// reservations, authority, cost and availability checks still apply.
        /// </summary>
        bool AllowBuildingAnywhereExceptWater { get; }

        bool AllowBuildingOnHills { get; }
        bool BlockEdgeTerrainTiles { get; }
        string[] BlockedTileIds { get; }
        string[] AllowedTileIds { get; }
        TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges { get; }
    }
}
