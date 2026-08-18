using System.Collections.Generic;
using Kruty1918.Moyva.InputRouting.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionInteractiveUiHitTester
    {
        bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId);
        bool IsPointerOverAnyUI(Vector2 screenPosition, int pointerId);
    }

    internal sealed class ConstructionInteractiveUiHitTester : IConstructionInteractiveUiHitTester
    {
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);
        private PointerEventData _pointerEventData;
        private int _cachedRaycastFrame = -1;
        private int _cachedPointerId;
        private Vector2 _cachedScreenPosition;
        private bool _hasCachedRaycast;

        public ConstructionInteractiveUiHitTester(
            [Zenject.InjectOptional] IGameplayInputPolicy inputPolicy = null)
        {
            _inputPolicy = inputPolicy;
        }

        public bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId)
        {
            if (_inputPolicy != null)
                return _inputPolicy.IsPointerOverUi(screenPosition, pointerId, interactiveOnly: true);

            RaycastUi(screenPosition, pointerId);

            for (int resultIndex = 0; resultIndex < _uiRaycastResults.Count; resultIndex++)
            {
                if (_uiRaycastResults[resultIndex].gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }

            return false;
        }

        public bool IsPointerOverAnyUI(Vector2 screenPosition, int pointerId)
        {
            if (_inputPolicy != null)
                return _inputPolicy.IsPointerOverUi(screenPosition, pointerId, interactiveOnly: false);

            RaycastUi(screenPosition, pointerId);
            return _uiRaycastResults.Count > 0;
        }

        private void RaycastUi(Vector2 screenPosition, int pointerId)
        {
            int frame = Time.frameCount;
            if (_hasCachedRaycast
                && _cachedRaycastFrame == frame
                && _cachedPointerId == pointerId
                && _cachedScreenPosition == screenPosition)
            {
                return;
            }

            _cachedRaycastFrame = frame;
            _cachedPointerId = pointerId;
            _cachedScreenPosition = screenPosition;
            _hasCachedRaycast = true;

            var eventSystem = EventSystem.current;
            _uiRaycastResults.Clear();
            if (eventSystem == null)
                return;

            _pointerEventData ??= new PointerEventData(eventSystem);
            _pointerEventData.Reset();
            _pointerEventData.pointerId = pointerId;
            _pointerEventData.position = screenPosition;

            eventSystem.RaycastAll(_pointerEventData, _uiRaycastResults);
        }
    }
}
