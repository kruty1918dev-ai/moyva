using System.Collections.Generic;
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
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);
        private PointerEventData _pointerEventData;

        public bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId)
        {
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
            RaycastUi(screenPosition, pointerId);
            return _uiRaycastResults.Count > 0;
        }

        private void RaycastUi(Vector2 screenPosition, int pointerId)
        {
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
