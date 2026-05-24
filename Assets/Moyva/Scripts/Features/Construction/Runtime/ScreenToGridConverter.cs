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
            // Ортографічна камера: Z не впливає на проекцію, ігноруємо
            Vector3 worldPos = _camera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, 0f));
            return _gridProjection.WorldToGrid(worldPos);
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            return _gridProjection.WorldToGrid(new Vector3(worldPosition.x, worldPosition.y, 0f));
        }
    }
}
