using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionTerrainAlignmentService
    {
        Vector3 ResolveWorldPosition(Vector2Int tile, float layerOffset);
        Vector3 ResolveAlignedInstancePosition(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f);
        void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f);
    }
}
