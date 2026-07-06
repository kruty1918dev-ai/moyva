using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionGridGeometryService
    {
        bool TryGetCellCenter(Vector2Int tile, out Vector3 center);
        bool TryGetCellSize(out Vector2 size);
        bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile);
        bool TryGetGridPlaneY(out float y);
    }
}
