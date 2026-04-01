using Kruty1918.Moyva.Buildings.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// MonoBehaviour, що обробляє клавіатурні скорочення для режиму будівництва:
    ///   Ctrl+Z → Undo (скасувати останнє розміщення)
    ///   Ctrl+Y → Redo (повторити скасоване розміщення)
    ///   Escape  → Cancel (скасувати всю сесію будівництва)
    /// </summary>
    public class BuildingInputHandler : MonoBehaviour
    {
        [Inject] private IBuildingPlacementService _placementService;

        private void Update()
        {
            if (!_placementService.IsPlacingMode) return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (ctrl && Input.GetKeyDown(KeyCode.Z))
            {
                _placementService.Undo();
                return;
            }

            if (ctrl && Input.GetKeyDown(KeyCode.Y))
            {
                _placementService.Redo();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _placementService.Cancel();
            }
        }
    }
}
