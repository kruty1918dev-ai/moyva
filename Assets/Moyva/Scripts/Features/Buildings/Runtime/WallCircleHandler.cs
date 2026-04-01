using UnityEngine;
using UnityEngine.EventSystems;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Прикріплюється до кожного кружечка-індикатора стіни.
    /// Перехоплює події натискання, переміщення та відпускання,
    /// делегуючи їх до <see cref="WallPlacementController"/>.
    /// Підтримує і мишу, і тач-введення через EventSystem.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WallCircleHandler : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private int _directionIndex;
        private WallPlacementController _controller;

        private bool _pointerDown;
        private bool _dragged;

        // Мінімальна відстань у пікселях для початку drag.
        private const float DragThreshold = 10f;
        private Vector2 _pointerDownScreenPos;

        public void Init(int directionIndex, WallPlacementController controller)
        {
            _directionIndex = directionIndex;
            _controller = controller;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDown = true;
            _dragged = false;
            _pointerDownScreenPos = eventData.position;
            _controller.OnCirclePointerDown(_directionIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_pointerDown) return;

            if (!_dragged && Vector2.Distance(eventData.position, _pointerDownScreenPos) > DragThreshold)
                _dragged = true;

            if (_dragged)
            {
                var worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
                var tilePos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
                _controller.OnDragUpdate(tilePos);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _controller.OnCirclePointerUp(_directionIndex, _dragged);
            _pointerDown = false;
            _dragged = false;
        }
    }
}
