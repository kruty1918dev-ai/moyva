using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ScreenToGridConverter : IScreenToGridConverter
    {
        private readonly Camera _camera;

        [Inject]
        public ScreenToGridConverter(Camera camera)
        {
            _camera = camera;
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
        {
            // Ортографічна камера: Z не впливає на проекцію, ігноруємо
            Vector3 worldPos = _camera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, 0f));
            return WorldToGrid(new Vector2(worldPos.x, worldPos.y));
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x),
                Mathf.RoundToInt(worldPosition.y));
        }
    }
}
