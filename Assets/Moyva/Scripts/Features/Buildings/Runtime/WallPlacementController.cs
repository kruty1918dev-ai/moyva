using System.Collections.Generic;
using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Контролер розміщення стін.
    /// Після кожного розміщення стіни відображає 8 кружечків-індикаторів
    /// навколо останньої розміщеної стіни. Гравець може:
    ///   – клікнути кружечок → розмістити стіну у відповідному напрямку;
    ///   – утримувати і тягнути від кружечка → будувати ланцюг стін до позиції пальця/курсора.
    /// </summary>
    public class WallPlacementController : MonoBehaviour
    {
        [Tooltip("ID типу будівлі, що вважається стіною (наприклад, \"wall-stone\").")]
        [SerializeField] private string _wallBuildingId = "wall-stone";

        [Tooltip("Префаб кружечка-індикатора напрямку.")]
        [SerializeField] private GameObject _directionCirclePrefab;

        [Tooltip("Колір кружечка у стані hover.")]
        [SerializeField] private Color _circleHoverColor = new Color(0.4f, 0.8f, 1f, 0.9f);

        // Вісім напрямків (4 кардинальні + 4 діагональні).
        private static readonly Vector2Int[] Directions = new Vector2Int[]
        {
            new Vector2Int( 0,  1),  // North
            new Vector2Int( 1,  1),  // NE
            new Vector2Int( 1,  0),  // East
            new Vector2Int( 1, -1),  // SE
            new Vector2Int( 0, -1),  // South
            new Vector2Int(-1, -1),  // SW
            new Vector2Int(-1,  0),  // West
            new Vector2Int(-1,  1),  // NW
        };

        private readonly List<GameObject> _circles = new List<GameObject>();

        private Vector2Int _lastWallPosition;
        private bool _hasLastWall;

        // Стан перетягування.
        private bool _isDragging;
        private int _dragDirectionIndex = -1;

        [Inject] private IBuildingPlacementService _placementService;
        [Inject] private SignalBus _signalBus;

        private void Start()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            DestroyCircles();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            if (signal.BuildingId != _wallBuildingId) return;

            _lastWallPosition = signal.Position;
            _hasLastWall = true;
            ShowCircles(_lastWallPosition);
        }

        // ──────────────────────────── відображення кружечків ───────────────────────────

        private void ShowCircles(Vector2Int center)
        {
            DestroyCircles();
            if (_directionCirclePrefab == null) return;

            for (int i = 0; i < Directions.Length; i++)
            {
                var worldPos = new Vector3(
                    center.x + Directions[i].x,
                    center.y + Directions[i].y,
                    -0.2f);

                var circle = Instantiate(_directionCirclePrefab, worldPos, Quaternion.identity, transform);
                circle.name = $"WallCircle_{i}";

                // Зберігаємо індекс напрямку через простий компонент.
                var handler = circle.AddComponent<WallCircleHandler>();
                handler.Init(i, this);

                _circles.Add(circle);
            }
        }

        private void DestroyCircles()
        {
            foreach (var c in _circles)
                if (c != null) Destroy(c);
            _circles.Clear();
        }

        // ──────────────────────────── обробка кліку / перетягування ───────────────────

        /// <summary>Викликається WallCircleHandler при натисканні на кружечок.</summary>
        internal void OnCirclePointerDown(int directionIndex)
        {
            _isDragging = true;
            _dragDirectionIndex = directionIndex;
        }

        /// <summary>Викликається WallCircleHandler при відпусканні кружечка (короткий клік).</summary>
        internal void OnCirclePointerUp(int directionIndex, bool wasDragged)
        {
            _isDragging = false;

            if (!wasDragged && _hasLastWall)
                PlaceWallInDirection(directionIndex);
        }

        /// <summary>Викликається кожен кадр із поточною позицією курсору/пальця, якщо активне перетягування.</summary>
        internal void OnDragUpdate(Vector2Int worldTilePosition)
        {
            if (!_isDragging || !_hasLastWall) return;

            var path = BuildLinePath(_lastWallPosition, worldTilePosition);
            foreach (var pos in path)
                PlaceWallAt(pos);
        }

        private void PlaceWallInDirection(int directionIndex)
        {
            if (directionIndex < 0 || directionIndex >= Directions.Length) return;
            PlaceWallAt(_lastWallPosition + Directions[directionIndex]);
        }

        private void PlaceWallAt(Vector2Int position)
        {
            var prevSelected = _placementService.SelectedBuildingId;
            _placementService.SelectBuilding(_wallBuildingId);
            _placementService.PlaceBuilding(position);
            if (prevSelected != null && prevSelected != _wallBuildingId)
                _placementService.SelectBuilding(prevSelected);
        }

        // ──────────────────────────── алгоритм лінії (Bresenham) ───────────────────────

        /// <summary>
        /// Повертає список тайл-позицій від <paramref name="from"/> до <paramref name="to"/>
        /// (не включаючи from, тому що там вже стоїть стіна).
        /// </summary>
        private static List<Vector2Int> BuildLinePath(Vector2Int from, Vector2Int to)
        {
            var path = new List<Vector2Int>();
            int x = from.x, y = from.y;
            int dx = Mathf.Abs(to.x - x), dy = Mathf.Abs(to.y - y);
            int sx = to.x > x ? 1 : -1;
            int sy = to.y > y ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                var pos = new Vector2Int(x, y);
                if (pos != from) path.Add(pos);
                if (x == to.x && y == to.y) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 <  dx) { err += dx; y += sy; }
            }

            return path;
        }
    }
}
