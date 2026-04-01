using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Одна точка з'єднання стіни (одне з 8 кіл навколо розміщеної стіни).
    /// Підтримує клік для розміщення стіни та перетягування для прокладання шляху стін.
    /// </summary>
    public class WallConnectionPoint : MonoBehaviour
    {
        private const float DragThresholdPixels = 10f;

        [Inject] private IBuildingPlacementService _placementService;
        [Inject] private IWallService _wallService;

        private Vector2Int _sourceWallPosition;
        private Vector2Int _targetPosition;
        private Vector3 _dragStartScreenPos;
        private bool _isDragging;

        /// <summary>
        /// Налаштувати точку з'єднання.
        /// </summary>
        /// <param name="sourceWallPos">Позиція стіни, від якої йде з'єднання</param>
        /// <param name="targetPos">Позиція тайлу, де буде ця точка</param>
        public void Setup(Vector2Int sourceWallPos, Vector2Int targetPos)
        {
            _sourceWallPosition = sourceWallPos;
            _targetPosition = targetPos;
        }

        private void OnMouseDown()
        {
            _dragStartScreenPos = Input.mousePosition;
            _isDragging = false;
        }

        private void OnMouseDrag()
        {
            if (Vector3.Distance(Input.mousePosition, _dragStartScreenPos) > DragThresholdPixels)
                _isDragging = true;
        }

        private void OnMouseUp()
        {
            if (_isDragging)
            {
                // Перетягування: прокладаємо шлях стін від цільової позиції до позиції курсора
                var cam = Camera.main;
                if (cam == null)
                {
                    Debug.LogError("[WallConnectionPoint] Camera.main не знайдено. Переконайтесь, що камера має тег 'MainCamera'.");
                    _isDragging = false;
                    return;
                }

                var worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
                var gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
                _wallService.DrawWallPath(_targetPosition, gridPos);
            }
            else
            {
                // Звичайний клік: розміщуємо стіну на цільовій позиції
                _placementService.TryPlace(_targetPosition);
            }

            _isDragging = false;
        }
    }
}
