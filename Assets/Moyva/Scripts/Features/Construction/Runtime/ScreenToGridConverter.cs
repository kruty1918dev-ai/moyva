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

        public ScreenToGridConverter(Camera camera)
            : this(camera, null)
        {
        }

        [Inject]
        public ScreenToGridConverter(Camera camera, [InjectOptional] IGridProjection gridProjection)
        {
            _camera = camera;
            _gridProjection = gridProjection ?? new OrthogonalGridProjection();
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
        {
            Vector3 worldPos = ScreenToWorldOnGridPlane(screenPosition);
            return _gridProjection.WorldToGrid(worldPos);
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            Vector3 projectedWorldPosition = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Vector3(worldPosition.x, 0f, worldPosition.y)
                : new Vector3(worldPosition.x, worldPosition.y, 0f);
            return _gridProjection.WorldToGrid(projectedWorldPosition);
        }

        private Vector3 ScreenToWorldOnGridPlane(Vector2 screenPosition)
        {
            if (_camera == null)
                return Vector3.zero;

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, Vector3.zero)
                : new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);

            return _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
        }
    }
}
