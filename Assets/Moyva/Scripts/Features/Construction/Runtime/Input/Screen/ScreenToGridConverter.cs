using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ScreenToGridConverter : IScreenToGridConverter
    {
        private readonly Camera _camera;
        private readonly IGridProjection _gridProjection;
        private readonly IConstructionGridGeometryService _gridGeometry;

        public ScreenToGridConverter(Camera camera)
            : this(camera, null, null)
        {
        }

        [Inject]
        public ScreenToGridConverter(
            Camera camera,
            [InjectOptional] IGridProjection gridProjection,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null)
        {
            _camera = camera;
            _gridProjection = gridProjection ?? new OrthogonalGridProjection();
            _gridGeometry = gridGeometry;
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
        {
            Vector3 worldPos = ScreenToWorldOnGridPlane(screenPosition);
            return TryUseGeneratedGrid(worldPos, out Vector2Int tile)
                ? tile
                : _gridProjection.WorldToGrid(worldPos);
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            Vector3 projectedWorldPosition = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Vector3(worldPosition.x, 0f, worldPosition.y)
                : new Vector3(worldPosition.x, worldPosition.y, 0f);
            return TryUseGeneratedGrid(projectedWorldPosition, out Vector2Int tile)
                ? tile
                : _gridProjection.WorldToGrid(projectedWorldPosition);
        }

        private Vector3 ScreenToWorldOnGridPlane(Vector2 screenPosition)
        {
            if (_camera == null)
                return Vector3.zero;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, new Vector3(0f, ResolveGridPlaneY(), 0f))
                : new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
        }

        private bool TryUseGeneratedGrid(Vector3 worldPosition, out Vector2Int tile)
        {
            tile = default;
            return _gridGeometry != null && _gridGeometry.TryGetCellAtWorld(worldPosition, out tile);
        }

        private float ResolveGridPlaneY()
        {
            return _gridGeometry != null && _gridGeometry.TryGetGridPlaneY(out float y)
                ? y
                : 0f;
        }
    }
}
