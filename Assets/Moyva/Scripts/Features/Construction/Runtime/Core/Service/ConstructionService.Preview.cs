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
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) проігнорована: неправильний стан {State}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                Debug.LogWarning("[Construction] TryPreviewAt: _selectedBuildingId порожній або null");
                return false;
            }

            if (TryHandleSingletonPreview(position))
                return true;

            bool selectedIsGate = _wallTopologyService != null && _wallTopologyService.IsGate(_selectedBuildingId);
            if (_pendingPositions.Contains(position))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}): позиція вже у pending-списку");

                if (selectedIsGate && TryReplacePendingWallWithGate(position, _selectedBuildingId))
                    return true;

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
                    includeDetails: true));
            if (!placementResult.IsValid)
            {
                _lastActionMessage = placementResult.Reason;
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[MoyvaBuildGridDiag] preview-blocked building='{_selectedBuildingId}' origin={position} reason='{placementResult.Reason}'");

                return false;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] TryPreviewAt({position}) -> VALID для {_selectedBuildingId}");

            return AddPendingPlacement(position, _selectedBuildingId, clearRedoHistory: true);
        }

        public bool HasPendingPlacementAt(Vector2Int position)
        {
            return _pendingPositions.Contains(position);
        }

        public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
        {
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
            {
                buildingId = null;
                return false;
            }

            buildingId = _pendingPlacements[index].BuildingId;
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

        public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition)
        {
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) ignored: state={State}");
                return false;
            }

            int index = FindPendingPlacementIndex(fromPosition);
            if (index < 0)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) ignored: source preview not found.");
                return false;
            }

            if (fromPosition == toPosition)
                return true;

            var placement = _pendingPlacements[index];
            Vector2Int? ignoredOccupiedPosition = placement.OriginalPosition;
            ConstructionPlacementQueryResult moveResult = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    placement.BuildingId,
                    toPosition,
                    fromPosition,
                    ignoredOccupiedPosition,
                    includeResources: true,
                    includeDetails: true));
            if (!moveResult.IsValid)
            {
                _lastActionMessage = moveResult.Reason;
                if (VerboseLogs)
                    Debug.Log($"[MoyvaBuildGridDiag] preview-move-blocked building='{placement.BuildingId}' from={fromPosition} to={toPosition} reason='{moveResult.Reason}'");

                return false;
            }

            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPositions.Remove(fromPosition);
            _pendingPositions.Add(toPosition);
            _pendingPlacements[index] = new PendingPlacement(toPosition, placement.BuildingId, placement.OriginalPosition);
            MarkPendingPlacementsChanged();

            _signalBus.Fire(new BuildingPreviewMovedSignal
            {
                FromPosition = fromPosition,
                ToPosition = toPosition,
                BuildingId = placement.BuildingId
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
                PreviewState = BuildingPreviewState.Valid
            });

            SetPlacementSelection(placement.BuildingId, BuildingPlacementState.Placing);

            if (VerboseLogs)
                Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> VALID. relocation={IsRelocation(placement)}");

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
            MarkPendingPlacementsChanged();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] RemovePendingAt({position}) -> removed '{placement.BuildingId}'. pendingCount={_pendingPlacements.Count}");

            if (_pendingPlacements.Count == 0)
                SetPlacementSelection(_selectedBuildingId, BuildingPlacementState.Placing);

            return true;
        }

        private bool AddPendingPlacement(Vector2Int position, string buildingId, bool clearRedoHistory, Vector2Int? originalPosition = null)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogError($"[Construction] AddPendingPlacement: buildingId порожній на позиції {position}");
                return false;
            }

            if (_signalBus == null)
            {
                Debug.LogError("[Construction] AddPendingPlacement: _signalBus == null");
                return false;
            }

            if (_pendingPlacements == null || _pendingPositions == null)
            {
                Debug.LogError("[Construction] AddPendingPlacement: _pendingPlacements або _pendingPositions == null");
                return false;
            }

            try
            {
                SaveSnapshotForUndo(clearRedoHistory);

                _pendingPlacements.Add(new PendingPlacement(position, buildingId, originalPosition));
                _pendingPositions.Add(position);
                MarkPendingPlacementsChanged();

                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = buildingId,
                    PreviewState = BuildingPreviewState.Valid
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] ✓ Pending placement додана для '{buildingId}' at {position}. relocationFrom={originalPosition?.ToString() ?? "none"}, pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в AddPendingPlacement({position}, {buildingId}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private int FindPendingPlacementIndex(Vector2Int position)
        {
            try
            {
                if (_pendingPlacements == null)
                {
                    Debug.LogError("[Construction] FindPendingPlacementIndex: _pendingPlacements == null");
                    return -1;
                }

                for (int i = 0; i < _pendingPlacements.Count; i++)
                {
                    if (_pendingPlacements[i].Position == position)
                    {
                        if (VerboseLogs)
                            Debug.Log($"[Construction] FindPendingPlacementIndex({position}): знайдена на індексі {i}");
                        return i;
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] FindPendingPlacementIndex({position}): не знайдена (повертаю -1)");

                return -1;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в FindPendingPlacementIndex({position}): {ex.GetType().Name} - {ex.Message}");
                return -1;
            }
        }

        private bool TryReplacePendingWallWithGate(Vector2Int position, string gateBuildingId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gateBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log("[Construction] TryReplacePendingWallWithGate: gateBuildingId порожній");
                    return false;
                }

                if (_wallTopologyService == null || _wallGateReplacementValidator == null)
                {
                    if (VerboseLogs)
                        Debug.Log("[Construction] TryReplacePendingWallWithGate: wall gate dependencies are missing");
                    return false;
                }

                if (!_wallTopologyService.IsGate(gateBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate: '{gateBuildingId}' не є воротами");
                    return false;
                }

                int index = FindPendingPlacementIndex(position);
                if (index < 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): pending placement не знайдена");
                    return false;
                }

                var current = _pendingPlacements[index];
                if (!_wallGateReplacementValidator.CanReplaceWallWithGate(position, gateBuildingId, out _))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): заміна недозволена");
                    return false;
                }

                if (current.BuildingId == gateBuildingId)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): вже ворота '{gateBuildingId}'");
                    return true;
                }

                if (!TryValidateConstructionResources(position, gateBuildingId, _activeOwnerId, position, out var resourceReason))
                {
                    _lastActionMessage = resourceReason;
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}");
                    return false;
                }

                SaveSnapshotForUndo(clearRedoHistory: true);

                _pendingPlacements[index] = new PendingPlacement(position, gateBuildingId, current.OriginalPosition);
                MarkPendingPlacementsChanged();
                SetPlacementSelection(gateBuildingId, BuildingPlacementState.Placing);

                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = gateBuildingId,
                    PreviewState = BuildingPreviewState.Valid
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] ✓ Pending wall at {position} replaced with gate '{gateBuildingId}'.");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в TryReplacePendingWallWithGate({position}, {gateBuildingId}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }
    }
}
