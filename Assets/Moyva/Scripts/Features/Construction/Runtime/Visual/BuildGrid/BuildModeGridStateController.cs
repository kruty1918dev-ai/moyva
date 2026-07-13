using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Owns the mutually exclusive build-grid mode, selected definition and hover cell.
    /// It deliberately contains no rendering or placement-rule logic.
    /// </summary>
    internal sealed class BuildModeGridStateController
    {
        public BuildModeGridState State { get; private set; } = BuildModeGridState.Hidden;
        public string SelectedBuildingId { get; private set; }
        public Vector2Int? HoverPosition { get; private set; }
        public ConstructionBuildGridTileVisualState HoverVisualState { get; private set; }

        public bool SetConstructionModeActive(bool active)
        {
            if (active && State != BuildModeGridState.Hidden)
                return false;
            if (!active && State == BuildModeGridState.Hidden
                && SelectedBuildingId == null
                && !HoverPosition.HasValue)
            {
                return false;
            }

            BuildModeGridState nextState = active
                ? BuildModeGridState.General
                : BuildModeGridState.Hidden;
            bool changed = State != nextState || SelectedBuildingId != null || HoverPosition.HasValue;

            State = nextState;
            SelectedBuildingId = null;
            ClearHover();
            return changed;
        }

        public bool SetSelection(string buildingId, bool isDemolishMode)
        {
            if (State == BuildModeGridState.Hidden)
                return false;

            string normalizedId = string.IsNullOrWhiteSpace(buildingId) || isDemolishMode
                ? null
                : buildingId.Trim();
            BuildModeGridState nextState = normalizedId == null
                ? BuildModeGridState.General
                : BuildModeGridState.BuildingSelected;
            bool changed = State != nextState
                || !string.Equals(SelectedBuildingId, normalizedId, StringComparison.Ordinal);

            State = nextState;
            SelectedBuildingId = normalizedId;
            ClearHover();
            return changed;
        }

        public bool SetHover(Vector2Int position, ConstructionBuildGridTileVisualState visualState)
        {
            if (State == BuildModeGridState.Hidden || visualState == ConstructionBuildGridTileVisualState.Missing)
                return ClearHover();

            if (HoverPosition == position && HoverVisualState == visualState)
                return false;

            HoverPosition = position;
            HoverVisualState = visualState;
            return true;
        }

        public bool ClearHover()
        {
            if (!HoverPosition.HasValue && HoverVisualState == ConstructionBuildGridTileVisualState.Missing)
                return false;

            HoverPosition = null;
            HoverVisualState = ConstructionBuildGridTileVisualState.Missing;
            return true;
        }
    }
}
