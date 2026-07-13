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
        bool AllowBuildingOnHills { get; }
        bool BlockEdgeTerrainTiles { get; }
        string[] BlockedTileIds { get; }
        string[] AllowedTileIds { get; }
        TerrainLevelRestrictionRange[] BlockedTerrainLevelRanges { get; }
    }
}
