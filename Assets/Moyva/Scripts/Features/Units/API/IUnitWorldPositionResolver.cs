using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitWorldPositionResolver
    {
        Vector3 ResolveWorldPosition(
            Vector2Int gridPosition,
            float surfacePivotOffsetY = 0.05f);

        float ResolveSurfacePivotOffsetY(
            GameObject unitObject,
            Vector2Int gridPosition,
            float fallbackOffsetY = 0.05f);

        void AlignBottomToSurface(
            GameObject unitObject,
            Vector2Int gridPosition,
            float clearance = 0.02f);

        bool TryGetTerrainSurfaceY(Vector2Int gridPosition, out float surfaceY);
    }
}
