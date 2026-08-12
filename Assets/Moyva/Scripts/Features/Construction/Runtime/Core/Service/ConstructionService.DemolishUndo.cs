using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void Cancel()
        {
            ResetSession(clearRedoHistory: false);
        }

        public void UndoLast()
        {
            EndPendingUndoBatch();

            if (_undoSnapshots.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] UndoLast ignored: undo history is empty.");
                return;
            }

            _redoSnapshots.Add(ClonePendingSnapshot());

            int snapshotIndex = _undoSnapshots.Count - 1;
            var snapshot = _undoSnapshots[snapshotIndex];
            _undoSnapshots.RemoveAt(snapshotIndex);

            ApplyPendingSnapshot(snapshot);

            if (_pendingPlacements.Count == 0 && State == BuildingPlacementState.Idle)
                SetPlacementSelection(_selectedBuildingId, BuildingPlacementState.Placing);

            if (VerboseLogs)
                Debug.Log($"[Construction] UndoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public void RedoLast()
        {
            EndPendingUndoBatch();

            if (_redoSnapshots.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: redo stack is empty.");
                return;
            }

            if (!_isActive)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: construction mode is not active.");
                return;
            }

            _undoSnapshots.Add(ClonePendingSnapshot());

            int snapshotIndex = _redoSnapshots.Count - 1;
            var snapshot = _redoSnapshots[snapshotIndex];
            _redoSnapshots.RemoveAt(snapshotIndex);

            ApplyPendingSnapshot(snapshot);

            if (_pendingPlacements.Count > 0)
            {
                SetPlacementSelection(
                    _pendingPlacements[_pendingPlacements.Count - 1].BuildingId,
                    BuildingPlacementState.Placing);
            }
            else if (State == BuildingPlacementState.Idle)
            {
                SetPlacementSelection(_selectedBuildingId, BuildingPlacementState.Placing);
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] RedoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public bool TryDemolishAt(Vector2Int position)
        {
            if (!CanActiveOwnerAct(out string turnReason))
            {
                Debug.LogWarning($"[Construction] Demolition rejected: {turnReason}");
                return false;
            }

            if (!_isActive || !IsDemolishMode)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: active={_isActive}, demolish={IsDemolishMode}");
                return false;
            }

            position = ResolvePlacedOrigin(position);
            if (!_playerPlacedBuildings.TryGetValue(position, out var buildingId))
            {
                Debug.LogWarning($"[Construction] TryDemolishAt({position}): будівля не була розміщена гравцем.");
                return false;
            }

            if (_pendingDemolitionPositions.Contains(position))
            {
                for (int i = _pendingDemolitions.Count - 1; i >= 0; i--)
                {
                    if (_pendingDemolitions[i].Position == position)
                    {
                        var pendingBuildingId = _pendingDemolitions[i].BuildingId;
                        _pendingDemolitions.RemoveAt(i);
                        _pendingDemolitionPositions.Remove(position);

                        _signalBus.Fire(new BuildingPreviewChangedSignal
                        {
                            Position = position,
                            BuildingId = pendingBuildingId,
                            PreviewState = BuildingPreviewState.None
                        });

                        if (VerboseLogs)
                            Debug.Log($"[Construction] Pending demolition unmarked for '{pendingBuildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

                        return true;
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: pending position exists but mark entry was not found.");
                return true;
            }

            _pendingDemolitions.Add(new PendingDemolition(position, buildingId));
            _pendingDemolitionPositions.Add(position);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = buildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending demolition marked for '{buildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

            return true;
        }

        public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
            => new ReadOnlyDictionary<Vector2Int, string>(_playerPlacedBuildings);

        private void ConfirmPendingDemolitions()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm demolish requested. count={_pendingDemolitions.Count}");

            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                var pos = demolition.Position;
                var id = demolition.BuildingId;

                if (!_playerPlacedBuildings.ContainsKey(pos))
                    continue;

                UnregisterBuildingFootprint(pos, id);
                _playerPlacedBuildings.Remove(pos);
                _placedRotationByOrigin.Remove(pos);
                InvalidatePlacementAvailabilityCache();
                _signalBus.Fire(new BuildingDemolishedSignal
                {
                    BuildingId = id,
                    Position = pos,
                    OwnerId = _activeOwnerId,
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm demolished '{id}' at {pos}");
            }

            _pendingDemolitions.Clear();
            _pendingDemolitionPositions.Clear();

            if (VerboseLogs)
                Debug.Log("[Construction] Confirm demolish completed.");
        }

        private void ResetSession(bool clearRedoHistory)
        {
            ResetPendingUndoBatchState();

            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession requested. pendingCount={_pendingPlacements.Count}, redoCount={_redoSnapshots.Count}, clearRedoHistory={clearRedoHistory}");

            if (!clearRedoHistory && _pendingPlacements.Count > 0)
            {
                _redoSnapshots.Clear();
                _redoSnapshots.Add(ClonePendingSnapshot());
            }

            for (int i = _pendingPlacements.Count - 1; i >= 0; i--)
            {
                var placement = _pendingPlacements[i];
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                        Position = placement.Position,
                        BuildingId = placement.BuildingId,
                        RotationQuarterTurns =
                            (int)placement.Rotation,
                        PreviewState = BuildingPreviewState.None
                });
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _pendingPlacementByPosition.Clear();
            MarkPendingPlacementsChanged();
            if (clearRedoHistory)
                _redoSnapshots.Clear();

            ClearPendingDemolitionsPreview();

            _undoSnapshots.Clear();

            SetPlacementSelection(null, BuildingPlacementState.Idle);
            _signalBus.Fire(new BuildingCancelledSignal());

            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession completed. state=Idle, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        private void ClearPendingDemolitionsPreview()
        {
            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = demolition.Position,
                    BuildingId = demolition.BuildingId,
                    PreviewState = BuildingPreviewState.None
                });
            }

            _pendingDemolitions.Clear();
            _pendingDemolitionPositions.Clear();
        }

        public void BeginPendingUndoBatch(string reason = null)
        {
            if (_pendingUndoBatchDepth == 0)
            {
                _pendingUndoBatchSnapshot = ClonePendingSnapshot();
                _pendingUndoBatchChanged = false;
                _pendingUndoBatchClearRedoHistory = false;
                _pendingUndoBatchStartCount =
                    _pendingPlacements.Count;
                _pendingUndoBatchReason =
                    string.IsNullOrWhiteSpace(reason)
                        ? "unspecified"
                        : reason;

                if (VerboseLogs)
                {
                    Debug.Log(
                        $"{PerfLogTag} undo-batch begin " +
                        $"reason={_pendingUndoBatchReason} " +
                        $"pending={_pendingUndoBatchStartCount}");
                }
            }

            _pendingUndoBatchDepth++;
        }

        public void EndPendingUndoBatch()
        {
            if (_pendingUndoBatchDepth <= 0)
                return;

            _pendingUndoBatchDepth--;
            if (_pendingUndoBatchDepth > 0)
                return;

            if (_pendingUndoBatchChanged)
            {
                _undoSnapshots.Add(
                    _pendingUndoBatchSnapshot
                    ?? new List<PendingPlacement>());

                if (_pendingUndoBatchClearRedoHistory)
                    _redoSnapshots.Clear();

                if (VerboseLogs)
                {
                    int finalCount = _pendingPlacements.Count;
                    Debug.Log(
                        $"{PerfLogTag} undo-batch commit " +
                        $"reason={_pendingUndoBatchReason} " +
                        $"start={_pendingUndoBatchStartCount} " +
                        $"end={finalCount} " +
                        $"delta={finalCount - _pendingUndoBatchStartCount} " +
                        $"undoCount={_undoSnapshots.Count}");
                }
            }
            else if (VerboseLogs)
            {
                Debug.Log(
                    $"{PerfLogTag} undo-batch end-no-change " +
                    $"reason={_pendingUndoBatchReason}");
            }

            ResetPendingUndoBatchState();
        }

        private void ResetPendingUndoBatchState()
        {
            _pendingUndoBatchDepth = 0;
            _pendingUndoBatchSnapshot = null;
            _pendingUndoBatchChanged = false;
            _pendingUndoBatchClearRedoHistory = false;
            _pendingUndoBatchStartCount = 0;
            _pendingUndoBatchReason = null;
        }

        private void SaveSnapshotForUndo(bool clearRedoHistory)
        {
            if (_pendingUndoBatchDepth > 0)
            {
                _pendingUndoBatchChanged = true;
                _pendingUndoBatchClearRedoHistory |=
                    clearRedoHistory;
                return;
            }

            _undoSnapshots.Add(ClonePendingSnapshot());
            if (clearRedoHistory)
                _redoSnapshots.Clear();
        }

        private List<PendingPlacement> ClonePendingSnapshot()
        {
            return new List<PendingPlacement>(_pendingPlacements);
        }

        private void ApplyPendingSnapshot(List<PendingPlacement> snapshot)
        {
            var previous = ClonePendingSnapshot();

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _pendingPlacementByPosition.Clear();

            for (int i = 0; i < snapshot.Count; i++)
            {
                var placement = snapshot[i];
                _pendingPlacements.Add(placement);
                _pendingPositions.Add(placement.Position);
                _pendingPlacementByPosition[placement.Position] =
                    placement;
            }
            MarkPendingPlacementsChanged();

            if (VerboseLogs)
            {
                Debug.Log(
                    $"{PerfLogTag} pending-index rebuilt after snapshot: " +
                    $"count={_pendingPlacementByPosition.Count}");
            }

            var previousByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < previous.Count; i++)
                previousByPosition[previous[i].Position] = previous[i];

            var currentByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < _pendingPlacements.Count; i++)
                currentByPosition[_pendingPlacements[i].Position] = _pendingPlacements[i];

            foreach (var pair in previousByPosition)
            {
                if (!currentByPosition.TryGetValue(pair.Key, out var current)
                    || current.BuildingId != pair.Value.BuildingId
                    || current.Rotation != pair.Value.Rotation)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        RotationQuarterTurns =
                            (int)pair.Value.Rotation,
                        PreviewState = BuildingPreviewState.None
                    });
                }
            }

            foreach (var pair in currentByPosition)
            {
                if (!previousByPosition.TryGetValue(pair.Key, out var previousPlacement)
                    || previousPlacement.BuildingId != pair.Value.BuildingId
                    || previousPlacement.Rotation != pair.Value.Rotation)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        RotationQuarterTurns =
                            (int)pair.Value.Rotation,
                        PreviewState = BuildingPreviewState.Valid
                    });
                }
            }
        }
    }
}
