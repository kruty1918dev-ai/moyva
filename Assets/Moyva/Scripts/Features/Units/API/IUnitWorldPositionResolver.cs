using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitWorldPositionResolver
    {
        /// <summary>True when units live on the XZ plane — yaw facing/bob are valid presentation.</summary>
        bool Uses3DWorldPlane { get; }

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
