using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallVisualResolver
    {
        bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation);
        bool TryResolvePreviewVisual(Vector2Int position, string buildingId, out GameObject prefab);
    }
}
