using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Реалізація сервісу з'єднання стін.
    /// Показує 8 точок навколо розміщеної стіни та прокладає шлях стін
    /// між двома позиціями алгоритмом Брезенхема.
    /// </summary>
    internal sealed class WallConnectionService : IWallConnectionService
    {
        private static readonly Vector2Int[] Directions =
        {
            new(-1,  1), new(0,  1), new(1,  1),
            new(-1,  0),             new(1,  0),
            new(-1, -1), new(0, -1), new(1, -1)
        };

        private readonly IGridService _gridService;
        private readonly IBuildingConstructionService _constructionService;
        private readonly SignalBus _signalBus;

        public WallConnectionService(
            IGridService gridService,
            IBuildingConstructionService constructionService,
            SignalBus signalBus)
        {
            _gridService = gridService;
            _constructionService = constructionService;
            _signalBus = signalBus;
        }

        public void ShowConnectionPoints(Vector2Int wallPosition)
        {
            var points = new List<Vector2Int>(8);
            foreach (var dir in Directions)
            {
                var neighbour = wallPosition + dir;
                if (_gridService.TryGetTileData(neighbour, out _))
                    points.Add(neighbour);
            }

            _signalBus.Fire(new WallConnectionPointsShownSignal
            {
                WallPosition = wallPosition,
                ConnectionPoints = points.ToArray()
            });
        }

        public void HideConnectionPoints()
        {
            _signalBus.Fire(new WallConnectionPointsHiddenSignal());
        }

        /// <summary>
        /// Прокладає шлях стін від <paramref name="from"/> до <paramref name="to"/>
        /// за алгоритмом Брезенхема. Кожна позиція передається в
        /// <see cref="IBuildingConstructionService.PlaceAt"/>.
        /// </summary>
        public void PlaceWallPath(Vector2Int from, Vector2Int to)
        {
            foreach (var pos in GetLinePath(from, to))
                _constructionService.PlaceAt(pos);
        }

        private static IEnumerable<Vector2Int> GetLinePath(Vector2Int from, Vector2Int to)
        {
            int x0 = from.x, y0 = from.y;
            int x1 = to.x,   y1 = to.y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                yield return new Vector2Int(x0, y0);
                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 <  dx) { err += dx; y0 += sy; }
            }
        }
    }
}
