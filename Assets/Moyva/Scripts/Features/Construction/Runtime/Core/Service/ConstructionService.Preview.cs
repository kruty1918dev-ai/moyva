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
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) проігнорована: неправильний стан {State}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                Debug.LogWarning("[Construction] TryPreviewAt: _selectedBuildingId порожній або null");
                return false;
            }

            if (!RevalidateActiveSelectionAvailability(
                    "pointer-click",
                    position))
            {
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] placement-rejected-stale-selection " +
                    $"origin={position}");
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
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}): позиція вже у pending-списку");

                if (TryReplacePendingPlacement(
                        position,
                        _selectedBuildingId))
                    return true;

                _lastActionMessage =
                    $"На клітинці {position} вже є непідтверджене розміщення.";
                LogSyntheticPlacementRejection(
                    ConstructionPlacementAttemptSource.PointerClick,
                    _selectedBuildingId,
                    position,
                    _activeOwnerId,
                    "pending-position-occupied",
                    _lastActionMessage);
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
                LogPlacementAttempt(
                    placementResult,
                    emitRejectedAction: true);
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
                LogPlacementAttempt(
                    placementResult,
                    emitRejectedAction: true);
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] pending-rejected " +
                    $"building='{_selectedBuildingId}' owner='{NormalizeOwnerId(_activeOwnerId)}' " +
                    $"origin={position} code='resources' reason='{placementResult.Reason}'");
                return false;
            }

            LogPlacementAttempt(
                placementResult,
                emitRejectedAction: false);
            if (VerboseLogs)
                Debug.Log($"[Construction] TryPreviewAt({position}) -> VALID для {_selectedBuildingId}");

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
                LogPlacementAttempt(
                    moveResult,
                    emitRejectedAction: true);
                return false;
            }

            if (!moveResult.ResourcesValid)
            {
                _lastActionMessage = moveResult.Reason;
                LogPlacementAttempt(
                    moveResult,
                    emitRejectedAction: true);
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] pending-move-rejected " +
                    $"building='{placement.BuildingId}' from={fromPosition} to={toPosition} " +
                    $"code='resources' reason='{moveResult.Reason}'");
                return false;
            }

            LogPlacementAttempt(
                moveResult,
                emitRejectedAction: false);
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
            _pendingPlacementByPosition.Remove(position);
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

        private bool AddPendingPlacement(
            Vector2Int position,
            string buildingId,
            bool clearRedoHistory,
            Vector2Int? originalPosition = null,
            bool isAffordable = true)
        {
            if (!isAffordable)
            {
                _lastActionMessage =
                    "Недостатньо ресурсів: pending-розміщення не створено.";
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] pending-invariant-rejected " +
                    $"building='{buildingId}' origin={position} code='resources'");
                return false;
            }

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

                PendingPlacement placement =
                    new PendingPlacement(
                        position,
                        buildingId,
                        originalPosition,
                        rotation: _selectedRotation);
                _pendingPlacements.Add(placement);
                _pendingPositions.Add(position);
                _pendingPlacementByPosition[position] = placement;
                MarkPendingPlacementsChanged();

                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = buildingId,
                    RotationQuarterTurns = (int)placement.Rotation,
                    PreviewState = ResolvePreviewState(isAffordable)
                });

                Debug.Log(
                    $"[MoyvaConstructionAvailability] pending-added " +
                    $"building='{buildingId}' origin={position} " +
                    $"pending={_pendingPlacements.Count}");

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

        private bool TryReplacePendingPlacement(
            Vector2Int position,
            string replacementBuildingId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                        replacementBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log("[Construction] Pending replacement building id is empty.");
                    return false;
                }

                int index = FindPendingPlacementIndex(position);
                if (index < 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] Pending replacement at {position} was not found.");
                    return false;
                }

                var current = _pendingPlacements[index];
                if (string.Equals(
                        current.BuildingId,
                        replacementBuildingId,
                        StringComparison.Ordinal))
                {
                    return true;
                }

                BuildingDefinition candidate =
                    _placementBuildingRegistry?.GetById(
                        replacementBuildingId);
                bool hasReplacementModule =
                    BuildingDefinitionCapabilities.TryGetEnabledModule(
                        candidate,
                        out ReplacementPlacementRuleModule
                            replacementModule);
                if (hasReplacementModule
                    && replacementModule.MergeMode
                        == PlacementRuleMergeMode.Disabled)
                {
                    return false;
                }

                if (hasReplacementModule
                    && replacementModule.MergeMode
                        == PlacementRuleMergeMode.Override)
                {
                    if (!CanReplaceBuilding(
                            current.BuildingId,
                            replacementModule)
                        || !IsPendingReplacementOwnerAllowed(
                            current,
                            replacementModule))
                    {
                        return false;
                    }
                }
                else if (_wallTopologyService == null
                         || _wallGateReplacementValidator == null
                         || !_wallTopologyService.IsGate(
                             replacementBuildingId)
                         || !_wallGateReplacementValidator
                             .CanReplaceWallWithGate(
                                 position,
                                 replacementBuildingId,
                                 out _))
                {
                    return false;
                }

                ConstructionPlacementQueryResult placement =
                    EvaluatePlacement(
                        new ConstructionPlacementQueryRequest(
                            replacementBuildingId,
                            position,
                            ignoredPendingPosition: position,
                            includeResources: true,
                            includeDetails: true,
                            ownerId: _activeOwnerId,
                            attemptSource:
                                ConstructionPlacementAttemptSource
                                    .PointerClick,
                            allowUniquePreviewRelocation: false,
                            satisfiedReplacementBuildingId:
                                current.BuildingId,
                            rotation: _selectedRotation));
                if (!placement.CanPreview)
                {
                    _lastActionMessage = placement.Reason;
                    LogPlacementAttempt(
                        placement,
                        emitRejectedAction: true);
                    return false;
                }

                if (!placement.ResourcesValid)
                {
                    _lastActionMessage = placement.Reason;
                    LogPlacementAttempt(
                        placement,
                        emitRejectedAction: true);
                    Debug.LogWarning(
                        $"[MoyvaConstructionAvailability] pending-replacement-rejected " +
                        $"building='{replacementBuildingId}' origin={position} " +
                        $"code='resources' reason='{placement.Reason}'");
                    return false;
                }

                SaveSnapshotForUndo(clearRedoHistory: true);

                PendingPlacement replacement =
                    new PendingPlacement(
                        position,
                        replacementBuildingId,
                        current.OriginalPosition,
                        current.BuildingId,
                        _selectedRotation);
                _pendingPlacements[index] = replacement;
                _pendingPlacementByPosition[position] = replacement;
                MarkPendingPlacementsChanged();
                SetPlacementSelection(
                    replacementBuildingId,
                    BuildingPlacementState.Placing);

                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = replacementBuildingId,
                    RotationQuarterTurns =
                        (int)replacement.Rotation,
                    PreviewState = ResolvePreviewState(
                        placement.ResourcesValid)
                });

                if (VerboseLogs)
                {
                    Debug.Log(
                        $"[Construction] Pending '{current.BuildingId}' at {position} replaced with '{replacementBuildingId}'.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[Construction] Pending replacement failed at {position} for '{replacementBuildingId}': {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private bool IsPendingReplacementOwnerAllowed(
            in PendingPlacement current,
            ReplacementPlacementRuleModule module)
        {
            if (!RequiresSameOwner(module)
                || !current.OriginalPosition.HasValue)
            {
                return true;
            }

            Vector2Int original = current.OriginalPosition.Value;
            if (_factionPlacedBuildings.TryGetValue(
                    original,
                    out var factionPlacement))
            {
                return string.Equals(
                    factionPlacement.FactionId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal);
            }

            return _playerPlacedBuildings.ContainsKey(original);
        }

        private static BuildingPreviewState ResolvePreviewState(bool isAffordable)
            => isAffordable
                ? BuildingPreviewState.Valid
                : BuildingPreviewState.Unaffordable;
    }
}
