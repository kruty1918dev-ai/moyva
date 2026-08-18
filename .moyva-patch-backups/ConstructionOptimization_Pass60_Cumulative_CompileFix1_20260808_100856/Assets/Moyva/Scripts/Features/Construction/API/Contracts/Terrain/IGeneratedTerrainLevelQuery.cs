using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IGeneratedTerrainLevelQuery
    {
        bool HasExplicitTerrainSurfaceMap { get; }
        bool TryGetTerrainLevel(Vector2Int position, out int level);
        bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY);
    }

    /// <summary>
    /// Optional version source for caches derived from generated terrain
    /// surfaces. Kept separate so test and legacy query implementations do not
    /// need to implement it.
    /// </summary>
    public interface IGeneratedTerrainSurfaceVersionQuery
    {
        int TerrainSurfaceVersion { get; }
    }
}
