using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IGeneratedTerrainLevelQuery
    {
        bool HasExplicitTerrainSurfaceMap { get; }
        bool TryGetTerrainLevel(Vector2Int position, out int level);
        bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY);
    }
}
