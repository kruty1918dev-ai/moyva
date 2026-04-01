using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallPlacementService : IWallPlacementService, IInitializable, IDisposable
    {
        private readonly IConstructionService _constructionService;
        private readonly IScreenToGridConverter _screenToGridConverter;
        private readonly SignalBus _signalBus;

        private Vector2Int? _dragStartPosition;

        [Inject]
        public WallPlacementService(
            IConstructionService constructionService,
            IScreenToGridConverter screenToGridConverter,
            SignalBus signalBus)
        {
            _constructionService = constructionService;
            _screenToGridConverter = screenToGridConverter;
            _signalBus = signalBus;
        }

        public void Initialize() { }

        public void Dispose() { }

        public void ShowWallHandles(Vector2Int wallPosition)
        {
            _signalBus.Fire(new ShowWallHandlesSignal { Center = wallPosition, Hide = false });
        }

        public void DragWall(Vector2Int startPosition, Vector2 touchWorldPosition)
        {
            _dragStartPosition = startPosition;
            Vector2Int endGrid = _screenToGridConverter.WorldToGrid(touchWorldPosition);

            foreach (Vector2Int tile in BresenhamLine(startPosition, endGrid))
                _constructionService.TryPreviewAt(tile);
        }

        public void EndDrag()
        {
            if (_dragStartPosition.HasValue)
            {
                _signalBus.Fire(new ShowWallHandlesSignal { Center = _dragStartPosition.Value, Hide = true });
                _dragStartPosition = null;
            }
        }

        // Алгоритм Bresenham для побудови лінії між двома тайлами
        private static IEnumerable<Vector2Int> BresenhamLine(Vector2Int start, Vector2Int end)
        {
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Vector2Int(x0, y0);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx)  { err += dx; y0 += sy; }
            }
        }
    }
}
