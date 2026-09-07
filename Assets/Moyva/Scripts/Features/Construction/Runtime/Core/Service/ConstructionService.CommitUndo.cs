using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public ConstructionConfirmResult ConfirmPending()
        {
            int pendingBefore = _pendingPlacements.Count;
            Confirm();

            int pendingAfter = _pendingPlacements.Count;
            int confirmed = Math.Max(0, pendingBefore - pendingAfter);
            int rejected = Math.Max(0, pendingBefore - confirmed);
            if (confirmed > 0)
            {
                return new ConstructionConfirmResult(
                    ConstructionConfirmStatus.Succeeded,
                    rejected > 0
                        ? $"Placed {confirmed}; {rejected} placement(s) still need attention."
                        : $"Placed {confirmed} building(s).",
                    confirmed,
                    rejected);
            }

            string reason = string.IsNullOrWhiteSpace(_lastActionMessage)
                ? pendingBefore == 0
                    ? "Place a building preview before confirming."
                    : "The placement was rejected."
                : _lastActionMessage;
            return new ConstructionConfirmResult(
                ConstructionConfirmStatus.Rejected,
                reason,
                0,
                rejected);
        }

        public void Confirm()
        {
            if (!CanActiveOwnerAct(out string turnReason))
            {
                _lastActionMessage = turnReason;
                LogPlacementCommitRejected(
                    null,
                    null,
                    turnReason);
                return;
            }

            if (IsDemolishMode)
            {
                ConfirmPendingDemolitions();
                return;
            }

            if (_pendingPlacements.Count == 0)
            {
                _lastActionMessage =
                    "No pending construction placements to confirm.";
                LogPlacementCommitRejected(
                    null,
                    null,
                    "No pending construction placements to confirm.");
                return;
            }

            _confirmPendingSnapshot.Clear();
            _confirmPendingSnapshot.AddRange(_pendingPlacements);
            _confirmConfirmedPositions.Clear();

            List<PendingPlacement> pendingSnapshot =
                _confirmPendingSnapshot;
            HashSet<Vector2Int> confirmedPositions =
                _confirmConfirmedPositions;

            foreach (var placement in pendingSnapshot)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;
                bool isRelocation = IsRelocation(placement);
                bool hasRelocationSource = placement.OriginalPosition.HasValue;
                Vector2Int? relocationSource = placement.OriginalPosition;
                string relocationOwnerId = _activeOwnerId;
                bool modelCommitted = false;
                bool targetFootprintRegistered = false;
                bool relocationFootprintRemoved = false;
                bool replacementFootprintRemoved = false;
                Vector2Int replacedOrigin = default;
                string replacedBuildingId = null;
                try
                {
                    if (hasRelocationSource && relocationSource.Value == pos)
                    {
                        confirmedPositions.Add(pos);
                        continue;
                    }

                    bool gateReplacementAllowed = _replacementPolicy.TryResolveGateReplacement(
                        pos,
                        id,
                        out replacedOrigin,
                        out replacedBuildingId);

                    bool canPlace = CanPlaceAt(
                        pos,
                        pos,
                        id,
                        out var tileOccupied,
                        out var spacingBlocked,
                        out var fogBlocked,
                        out var influenceZoneBlocked,
                        out var terrainBlocked,
                        out string placementReason,
                        relocationSource,
                        placement.ReplacedPendingBuildingId,
                        placement.Rotation);

                    if (!canPlace)
                    {
                        LogPlacementCommitRejected(
                            id,
                            pos,
                            ResolveConfirmPlacementReason(
                                placementReason,
                                tileOccupied,
                                spacingBlocked,
                                fogBlocked,
                                influenceZoneBlocked,
                                terrainBlocked));
                        continue;
                    }

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && _factionPlacedBuildings.TryGetValue(relocationSource.Value, out var sourceFactionEntry))
                    {
                        relocationOwnerId = sourceFactionEntry.FactionId;
                    }

                    if (gateReplacementAllowed)
                    {
                        _footprints.Unregister(
                            replacedOrigin,
                            replacedBuildingId);
                        replacementFootprintRemoved = true;
                    }

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && (!gateReplacementAllowed
                            || relocationSource.Value != replacedOrigin))
                    {
                        _footprints.Unregister(
                            relocationSource.Value,
                            id);
                        relocationFootprintRemoved = true;
                    }

                    bool footprintRegistered =
                        _footprints.TryRegister(
                            pos,
                            id,
                            placement.Rotation);

                    if (!footprintRegistered)
                    {
                        Debug.LogError(
                            $"[MoyvaBuildGridDiag] placement-failed " +
                            $"building='{id}' origin={pos} " +
                            $"reason='footprint-registration'");
                        continue;
                    }

                    targetFootprintRegistered = true;

                    string resourceReason = null;
                    bool resourcesAccepted =
                        hasRelocationSource
                        || TryConsumeConstructionResources(
                            pos,
                            id,
                            relocationOwnerId,
                            out resourceReason);

                    if (!resourcesAccepted)
                    {
                        _lastActionMessage = resourceReason;
                        LogPlacementCommitRejected(
                            id,
                            pos,
                            resourceReason);
                        _signalBus.Fire(
                            new BuildingPreviewChangedSignal
                            {
                                Position = pos,
                                BuildingId = id,
                                PreviewState =
                                    BuildingPreviewState.Unaffordable,
                            });
                        continue;
                    }

                    if (gateReplacementAllowed)
                        RemovePlacedRecordAt(replacedOrigin);
                    if (hasRelocationSource && relocationSource.HasValue)
                        RemovePlacedRecordAt(relocationSource.Value);

                    _playerPlacedBuildings.Remove(pos);
                    _factionPlacedBuildings[pos] =
                        (id, NormalizeOwnerId(relocationOwnerId));
                    _placedRotationByOrigin[pos] =
                        placement.Rotation;

                    modelCommitted = true;
                    confirmedPositions.Add(pos);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MoyvaBuildGridDiag] placement-failed building='{id}' origin={pos} reason='{ex.GetType().Name}: {ex.Message}'");
                }
                finally
                {
                    if (!modelCommitted)
                    {
                        if (targetFootprintRegistered)
                            _footprints.Unregister(pos, id);

                        if (relocationFootprintRemoved && relocationSource.HasValue)
                            RestoreBuildingFootprintOrLog(relocationSource.Value, id, "relocation");

                        if (replacementFootprintRemoved
                            && (!relocationFootprintRemoved
                                || !relocationSource.HasValue
                                || replacedOrigin != relocationSource.Value))
                        {
                            RestoreBuildingFootprintOrLog(replacedOrigin, replacedBuildingId, "gate-replacement");
                        }
                    }
                }

                if (!modelCommitted)
                    continue;

                try
                {
                    InvalidatePlacementAvailabilityCache();

                    _signalBus.Fire(new BuildingPlacedSignal
                    {
                        BuildingId = id,
                        Position = pos,
                        OwnerId = relocationOwnerId,
                        SourceFactionId = relocationOwnerId,
                        HasRelocationSource = isRelocation && relocationSource.HasValue && relocationSource.Value != pos,
                        RelocationSourcePosition = relocationSource.GetValueOrDefault(),
                        RotationQuarterTurns =
                            (int)placement.Rotation,
                    });
                    RecordConstructionAction(
                        relocationOwnerId,
                        isRelocation ? "building-relocate" : "building-place");

                    try
                    {
                        if (isRelocation && relocationSource.HasValue)
                            _buildingFogEffects.Remove(
                                relocationSource.Value,
                                relocationOwnerId);
                        _buildingFogEffects.ApplyOnPlaced(
                            id,
                            pos,
                            relocationOwnerId);
                    }
                    catch (Exception fogEx)
                    {
                        Debug.LogError($"[Construction] Fog reveal failed for '{id}' at {pos}: {fogEx.GetType().Name} - {fogEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MoyvaBuildGridDiag] placement-notification-failed building='{id}' origin={pos} reason='{ex.GetType().Name}: {ex.Message}'");
                }
            }

            if (confirmedPositions.Count > 0)
            {
                for (int index = _pendingPlacements.Count - 1; index >= 0; index--)
                {
                    var pending = _pendingPlacements[index];
                    if (!confirmedPositions.Contains(pending.Position))
                        continue;

                    _pendingPlacements.RemoveAt(index);
                    _pendingPositions.Remove(pending.Position);
                    _pendingPlacementByPosition.Remove(
                        pending.Position);
                    _pendingPlacementStatuses.Remove(pending.Position);
                    MarkPendingPlacementsChanged();

                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pending.Position,
                        BuildingId = pending.BuildingId,
                        PreviewState = BuildingPreviewState.None
                    });
                }
            }

            _undoSnapshots.Clear();
            _redoSnapshots.Clear();

            if (_pendingPlacements.Count == 0)
                SetPlacementSelection(null, BuildingPlacementState.Idle);
            else
                SetPlacementSelection(
                    _pendingPlacements[_pendingPlacements.Count - 1].BuildingId,
                    BuildingPlacementState.Placing);

        }

        private string DescribePlacementQueryRejection(
            ConstructionPlacementQueryResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.Reason))
                return result.Reason;

            IReadOnlyList<BuildingPlacementBlocker> blockers =
                result.EvaluationResult?.Blockers;
            if (blockers != null && blockers.Count > 0)
            {
                BuildingPlacementBlocker blocker = blockers[0];
                string blockerPosition = blocker.Position.HasValue
                    ? $" at {blocker.Position.Value}"
                    : string.Empty;
                string blockerBuilding = string.IsNullOrWhiteSpace(
                    blocker.BuildingId)
                    ? string.Empty
                    : $" building='{blocker.BuildingId}'";
                string blockerMessage =
                    string.IsNullOrWhiteSpace(blocker.Message)
                        ? "Placement blocker."
                        : blocker.Message;

                return
                    $"{blocker.Kind}{blockerPosition}{blockerBuilding}: {blockerMessage}";
            }

            if (!result.AvailabilityValid)
                return "Placement availability is invalid.";
            if (!result.SpatialValid)
                return "Spatial placement rules rejected this tile.";
            if (!result.ResourcesValid)
                return "Construction resources are invalid.";
            if (!result.AuthorityValid)
                return "Construction authority rejected this commit.";

            return "Placement query was rejected without a reason.";
        }

        private string ResolveConfirmPlacementReason(
            string reason,
            bool tileOccupied,
            bool spacingBlocked,
            bool fogBlocked,
            bool influenceZoneBlocked,
            bool terrainBlocked)
        {
            if (!string.IsNullOrWhiteSpace(reason))
                return reason;

            if (tileOccupied)
                return "Tile or footprint is occupied.";
            if (spacingBlocked)
                return "Minimum spacing rule blocked placement.";
            if (fogBlocked)
                return "Fog of war blocked placement.";
            if (influenceZoneBlocked)
                return "Influence-zone rules blocked placement.";
            if (terrainBlocked)
                return "Terrain rules blocked placement.";

            return "Placement commit was rejected without a detailed reason.";
        }

        private void LogPlacementCommitRejected(
            string buildingId,
            Vector2Int? position,
            string reason)
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;

            string message = string.IsNullOrWhiteSpace(reason)
                ? "Commit was rejected without a reason."
                : reason;
            string id = string.IsNullOrWhiteSpace(buildingId)
                ? _selectedBuildingId
                : buildingId;
            string at = position.HasValue
                ? $" at {position.Value}"
                : string.Empty;

            Debug.LogWarning(
                $"[Construction] Commit rejected for '{id}'{at}, owner='{_activeOwnerId}', state={State}, pending={_pendingPlacements.Count}: {message}");
        }
    }
}
