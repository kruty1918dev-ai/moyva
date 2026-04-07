using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// UI scaffold for displaying the current construction status (placement state,
    /// selected building, and preview result).
    ///
    /// HOW TO WIRE IN UNITY:
    /// 1. Add this component to a status panel GameObject.
    /// 2. Drag <c>Text</c> (or <c>TextMeshProUGUI</c> — swap the field type) labels
    ///    into the three fields.
    /// 3. Assign the panel to <see cref="ConstructionUIController.statusDisplay"/>.
    ///    The controller calls <see cref="UpdateState"/> automatically.
    ///
    /// Label values:
    ///   placementStateLabel  → "Idle" | "Placing" | "Confirmed"
    ///   selectedBuildingLabel→ building DisplayName, or "--" when nothing selected
    ///   previewStateLabel    → "✓ Valid" | "✗ Blocked" | "--"
    /// </summary>
    public class ConstructionStatusUI : MonoBehaviour
    {
        [Header("Status Labels (drag in Inspector)")]
        [Tooltip("Shows the current placement state (Idle / Placing / Confirmed).")]
        [SerializeField] private Text placementStateLabel;

        [Tooltip("Shows the currently selected building ID, or '--' when none.")]
        [SerializeField] private Text selectedBuildingLabel;

        [Tooltip("Shows the current preview state (Valid / Blocked / --)." )]
        [SerializeField] private Text previewStateLabel;

        /// <summary>
        /// Update all status labels from the given UI state snapshot.
        /// Called automatically by <see cref="ConstructionUIController"/>.
        /// </summary>
        public void UpdateState(ConstructionUIState state)
        {
            if (placementStateLabel != null)
                placementStateLabel.text = state.PlacementState.ToString();

            if (selectedBuildingLabel != null)
                selectedBuildingLabel.text = state.HasSelection ? state.SelectedBuildingId : "--";

            if (previewStateLabel != null)
            {
                switch (state.LastPreviewState)
                {
                    case BuildingPreviewState.Valid:
                        previewStateLabel.text = "\u2713 Valid";
                        break;
                    case BuildingPreviewState.Blocked:
                        previewStateLabel.text = "\u2717 Blocked";
                        break;
                    default:
                        previewStateLabel.text = "--";
                        break;
                }
            }
        }
    }
}
