namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Which kind of placement is being evaluated. Spawn-family operations
    /// share the <c>no-spawn</c> tile tag; construction uses <c>no-build</c>.
    /// </summary>
    public enum TerrainPlacementOperation
    {
        /// <summary>Procedural decoration (trees, rocks, grass).</summary>
        Decoration = 0,
        /// <summary>Procedural object/resource spawn on the map.</summary>
        ObjectSpawn = 1,
        /// <summary>Newly recruited unit deployment.</summary>
        UnitDeployment = 2,
        /// <summary>Starting settlement / starting unit cell.</summary>
        StartingPosition = 3,
        /// <summary>New construction placement.</summary>
        Building = 4,
    }

    /// <summary>
    /// Canonical rule for whether a tile type accepts a new placement.
    /// Read-only over the tile-type repository; the tile JSON tags
    /// (<c>no-spawn</c>, <c>no-build</c>) are the editable source of truth.
    /// Movement and save restoration are deliberately not covered here.
    /// </summary>
    public interface ITerrainPlacementPolicy
    {
        bool AllowsPlacement(string tileTypeId, TerrainPlacementOperation operation);
    }
}
