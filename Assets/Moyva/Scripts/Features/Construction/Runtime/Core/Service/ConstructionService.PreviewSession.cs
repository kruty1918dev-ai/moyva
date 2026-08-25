using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public bool TryPreviewAt(Vector2Int position)
        {
            if (!CanActiveOwnerMutate(
                    "create construction preview",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return false;
            }

            if (State != BuildingPlacementState.Placing)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                return false;
            }

            if (!RevalidateActiveSelectionAvailability(
                    "pointer-click",
                    position))
            {
                return false;
            }

            if (TryHandleUniqueBuildingPreview(
                    position,
                    out bool uniquePlacementSucceeded))
            {
                return uniquePlacementSucceeded;
            }

            if (_pendingPositions.Contains(position))
            {

                if (TryReplacePendingPlacement(
                        position,
                        _selectedBuildingId))
                    return true;

                _lastActionMessage =
                    $"На клітинці {position} вже є непідтверджене розміщення.";
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            ConstructionPlacementQueryResult placementResult = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    _selectedBuildingId,
                    position,
                    includeResources: true,
                    includeDetails: true,
                    ownerId: _activeOwnerId,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerClick,
                    allowUniquePreviewRelocation: true,
                    rotation: _selectedRotation));
            if (!placementResult.CanPreview)
            {
                _lastActionMessage = placementResult.Reason;
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            if (!placementResult.ResourcesValid)
            {
                _lastActionMessage = placementResult.Reason;
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Unaffordable
                });
                return false;
            }


            return AddPendingPlacement(
                position,
                _selectedBuildingId,
                clearRedoHistory: true,
                isAffordable: true);
        }

        public bool HasPendingPlacementAt(Vector2Int position)
        {
            return _pendingPositions.Contains(position);
        }

        public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
        {
            if (!_pendingPlacementByPosition.TryGetValue(
                    position,
                    out PendingPlacement placement))
            {
                buildingId = null;
                return false;
            }

            buildingId = placement.BuildingId;
            return !string.IsNullOrWhiteSpace(buildingId);
        }

        public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
        {
            var snapshot = new Dictionary<Vector2Int, string>(_pendingPlacements.Count);
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                if (string.IsNullOrWhiteSpace(placement.BuildingId))
                    continue;

                snapshot[placement.Position] = placement.BuildingId;
            }

            return new ReadOnlyDictionary<Vector2Int, string>(snapshot);
        }

        public bool TryGetPendingPlacementIntent(
            Vector2Int position,
            out ConstructionPlacementCommitIntent intent)
        {
            if (!_pendingPlacementByPosition.TryGetValue(
                    position,
                    out PendingPlacement placement))
            {
                intent = ConstructionPlacementCommitIntent.None;
                return false;
            }

            intent = new ConstructionPlacementCommitIntent(
                placement.OriginalPosition,
                placement.ReplacedPendingBuildingId,
                placement.Rotation);
            return true;
        }

        public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition)
        {
            if (!CanActiveOwnerMutate(
                    "move construction preview",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return false;
            }

            if (State != BuildingPlacementState.Placing)
            {
                return false;
            }

            int index = FindPendingPlacementIndex(fromPosition);
            if (index < 0)
            {
                return false;
            }

            if (fromPosition == toPosition)
                return true;

            var placement = _pendingPlacements[index];
            if (!string.IsNullOrWhiteSpace(
                    placement.ReplacedPendingBuildingId))
            {
                _lastActionMessage =
                    "A replacement preview must remain on the preview it replaces.";
                return false;
            }

            Vector2Int? ignoredOccupiedPosition = placement.OriginalPosition;
            ConstructionPlacementQueryResult moveResult = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    placement.BuildingId,
                    toPosition,
                    fromPosition,
                    ignoredOccupiedPosition,
                    includeResources: true,
                    includeDetails: true,
                    ownerId: _activeOwnerId,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PreviewMove,
                    allowUniquePreviewRelocation: false,
                    rotation: placement.Rotation));
            if (!moveResult.CanPreview)
            {
                _lastActionMessage = moveResult.Reason;
                return false;
            }

            if (!moveResult.ResourcesValid)
            {
                _lastActionMessage = moveResult.Reason;
                return false;
            }

            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPositions.Remove(fromPosition);
            _pendingPositions.Add(toPosition);
            _pendingPlacementByPosition.Remove(fromPosition);
            PendingPlacement movedPlacement = new PendingPlacement(
                toPosition,
                placement.BuildingId,
                placement.OriginalPosition,
                placement.ReplacedPendingBuildingId,
                placement.Rotation);
            _pendingPlacements[index] = movedPlacement;
            _pendingPlacementByPosition[toPosition] = movedPlacement;
            MarkPendingPlacementsChanged();

            _signalBus.Fire(new BuildingPreviewMovedSignal
            {
                FromPosition = fromPosition,
                ToPosition = toPosition,
                BuildingId = placement.BuildingId,
                RotationQuarterTurns = (int)placement.Rotation,
            });

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = fromPosition,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = toPosition,
                BuildingId = placement.BuildingId,
                RotationQuarterTurns = (int)placement.Rotation,
                PreviewState = ResolvePreviewState(
                    moveResult.ResourcesValid)
            });

            SetPlacementSelection(placement.BuildingId, BuildingPlacementState.Placing);

            return true;
        }

        public bool RemovePendingAt(Vector2Int position)
        {
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
                return false;

            var placement = _pendingPlacements[index];
            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPlacements.RemoveAt(index);
            _pendingPositions.Remove(position);
            _pendingPlacementByPosition.Remove(position);
            MarkPendingPlacementsChanged();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            if (_pendingPlacements.Count == 0)
                SetPlacementSelection(_selectedBuildingId, BuildingPlacementState.Placing);

            return true;
        }
    }
}
