using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IGeneratedTerrainLevelQuery
    {
        bool TryGetTerrainLevel(Vector2Int position, out int level);
    }
}
