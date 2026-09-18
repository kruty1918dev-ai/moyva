using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Optional generated-world geometry query used by input and overlay systems.
    /// </summary>
    public interface IGridWorldGeometryQuery
    {
        bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile);
        bool TryGetGridPlaneY(out float y);
    }
}
