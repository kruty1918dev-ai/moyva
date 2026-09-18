using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Authoritative screen/world pointer resolver for terrain-aware grid input.
    /// </summary>
    public interface IWorldPointerGridResolver
    {
        bool TryScreenToGrid(Vector2 screenPosition, out Vector2Int tile);
        Vector2Int ScreenToGrid(Vector2 screenPosition);
        bool TryWorldToGrid(Vector3 worldPosition, out Vector2Int tile);
        Vector2Int WorldToGrid(Vector3 worldPosition);
    }
}
