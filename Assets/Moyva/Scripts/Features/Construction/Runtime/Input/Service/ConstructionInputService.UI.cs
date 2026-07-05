using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionInputService
    {
        private bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId)
        {
            if (!_blockInteractiveUi || _uiHitTester == null)
                return false;

            return _allowClicksThroughNonInteractiveUi
                ? _uiHitTester.IsPointerOverInteractiveUI(screenPosition, pointerId)
                : _uiHitTester.IsPointerOverAnyUI(screenPosition, pointerId);
        }
    }
}
