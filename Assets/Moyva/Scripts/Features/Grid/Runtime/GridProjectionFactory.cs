using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public static class GridProjectionFactory
    {
        public static IGridProjection Create(MoyvaProjectSettingsSO settings)
        {
            settings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            settings.Normalize();

            return settings.DefaultProjectionMode switch
            {
                GridProjectionMode.Orthographic3D => new Orthographic3DGridProjection(settings),
                GridProjectionMode.Isometric2D => new IsometricGridProjection(settings),
                GridProjectionMode.Isometric3DPreview => new IsometricGridProjection(settings),
                GridProjectionMode.HexPointy2D => new HexAxialGridProjection(settings),
                GridProjectionMode.HexFlat2D => new HexAxialGridProjection(settings),
                _ => new OrthogonalGridProjection(settings),
            };
        }
    }

    public static class GridSurfacePlacementUtility
    {
        public const float DefaultSurfaceClearance = 0.02f;

        public static bool Uses3DWorldPlane(IGridProjection projection)
        {
            return projection != null && projection.WorldPlane == GridWorldPlane.XZ;
        }

        public static bool TryResolveRendererBounds(GameObject rootObject, out Bounds bounds)
        {
            bounds = default;
            if (rootObject == null)
                return false;

            bool hasBounds = false;
            var renderers = rootObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || !renderer.enabled)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return hasBounds;
        }

        public static bool TryResolveTopOffsetY(GameObject rootObject, out float topOffsetY)
        {
            topOffsetY = 0f;
            if (rootObject == null || !TryResolveRendererBounds(rootObject, out var bounds))
                return false;

            topOffsetY = bounds.max.y - rootObject.transform.position.y;
            return true;
        }

        public static void AlignBottomToSurface(GameObject instance, float surfaceY, float clearance = DefaultSurfaceClearance)
        {
            if (instance == null)
                return;

            float targetY = surfaceY + Mathf.Max(0f, clearance);
            if (!TryResolveRendererBounds(instance, out var bounds))
            {
                var position = instance.transform.position;
                position.y = targetY;
                instance.transform.position = position;
                return;
            }

            float deltaY = targetY - bounds.min.y;
            if (Mathf.Abs(deltaY) <= 0.0001f)
                return;

            instance.transform.position += Vector3.up * deltaY;
        }
    }
}