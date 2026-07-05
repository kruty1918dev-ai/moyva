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
                State = BuildingPlacementState.Placing;

            if (VerboseLogs)
                Debug.Log($"[Construction] UndoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public void RedoLast()
        {
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

            if (State == BuildingPlacementState.Idle)
                State = BuildingPlacementState.Placing;

            if (_pendingPlacements.Count > 0)
                _selectedBuildingId = _pendingPlacements[_pendingPlacements.Count - 1].BuildingId;

            if (VerboseLogs)
                Debug.Log($"[Construction] RedoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public bool TryDemolishAt(Vector2Int position)
        {
            if (!_isActive || !IsDemolishMode)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: active={_isActive}, demolish={IsDemolishMode}");
                return false;
            }

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

                if (!_playerPlacedBuildings.ContainsKey(pos) || !_objectsMapService.IsOccupied(pos))
                    continue;

                _objectsMapService.Unregister(pos);
                _playerPlacedBuildings.Remove(pos);
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
                    PreviewState = BuildingPreviewState.None
                });
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            if (clearRedoHistory)
                _redoSnapshots.Clear();

            ClearPendingDemolitionsPreview();

            _undoSnapshots.Clear();

            _signalBus.Fire(new BuildingCancelledSignal());
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

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

        private void SaveSnapshotForUndo(bool clearRedoHistory)
        {
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

            for (int i = 0; i < snapshot.Count; i++)
            {
                var placement = snapshot[i];
                _pendingPlacements.Add(placement);
                _pendingPositions.Add(placement.Position);
            }

            var previousByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < previous.Count; i++)
                previousByPosition[previous[i].Position] = previous[i];

            var currentByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < _pendingPlacements.Count; i++)
                currentByPosition[_pendingPlacements[i].Position] = _pendingPlacements[i];

            foreach (var pair in previousByPosition)
            {
                if (!currentByPosition.TryGetValue(pair.Key, out var current) || current.BuildingId != pair.Value.BuildingId)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        PreviewState = BuildingPreviewState.None
                    });
                }
            }

            foreach (var pair in currentByPosition)
            {
                if (!previousByPosition.TryGetValue(pair.Key, out var previousPlacement) || previousPlacement.BuildingId != pair.Value.BuildingId)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        PreviewState = BuildingPreviewState.Valid
                    });
                }
            }
        }
    }
}
