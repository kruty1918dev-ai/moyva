using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Optional grid surface query for generated maps with elevated terrain.
    /// </summary>
    public interface IGridTerrainSurfaceQuery
    {
        bool HasExplicitTerrainSurfaceMap { get; }
        int TerrainSurfaceVersion { get; }
        bool TryGetTerrainLevel(Vector2Int position, out int level);
        bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY);
    }
}
