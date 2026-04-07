using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Immutable snapshot of the current construction UI state.
    /// Passed to sub-UI components so they can update their display.
    /// </summary>
    public sealed class ConstructionUIState
    {
        /// <summary>Current placement session state (Idle / Placing / Confirmed).</summary>
        public BuildingPlacementState PlacementState { get; }

        /// <summary>ID of the building the player currently has selected, or null.</summary>
        public string SelectedBuildingId { get; }

        /// <summary>Preview state on the last hovered tile (None / Valid / Blocked).</summary>
        public BuildingPreviewState LastPreviewState { get; }

        /// <summary>Grid position of the last preview tile.</summary>
        public Vector2Int LastPreviewPosition { get; }

        /// <summary>True when a build session is active (building selected, awaiting tile).</summary>
        public bool IsPlacing => PlacementState == BuildingPlacementState.Placing;

        /// <summary>True when a building has been selected.</summary>
        public bool HasSelection => !string.IsNullOrEmpty(SelectedBuildingId);

        public ConstructionUIState(
            BuildingPlacementState placementState,
            string selectedBuildingId,
            BuildingPreviewState lastPreviewState,
            Vector2Int lastPreviewPosition)
        {
            PlacementState = placementState;
            SelectedBuildingId = selectedBuildingId;
            LastPreviewState = lastPreviewState;
            LastPreviewPosition = lastPreviewPosition;
        }

        /// <summary>Default idle state with no selection.</summary>
        public static ConstructionUIState Default =>
            new ConstructionUIState(BuildingPlacementState.Idle, null, BuildingPreviewState.None, Vector2Int.zero);
    }
}
