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
                        return i;
                    }
                }

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
                    return false;
                }

                int index = FindPendingPlacementIndex(position);
                if (index < 0)
                {
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
                    if (!_replacementPolicy.CanReplace(
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
                    return false;
                }

                if (!placement.ResourcesValid)
                {
                    _lastActionMessage = placement.Reason;
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
            if (!_replacementPolicy.RequiresSameOwner(module)
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
        private ConstructionRotation ResolvePlacedRotation(
            Vector2Int origin)
            => _placedRotationByOrigin.TryGetValue(
                origin,
                out ConstructionRotation rotation)
                ? rotation
                : ConstructionRotation.Degrees0;
    }
}
