using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void Confirm()
        {
            if (!CanActiveOwnerAct(out string turnReason))
            {
                _lastActionMessage = turnReason;
                return;
            }

            if (IsDemolishMode)
            {
                ConfirmPendingDemolitions();
                return;
            }

            if (_pendingPlacements.Count == 0)
                return;

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
                        relocationSource,
                        placement.ReplacedPendingBuildingId,
                        placement.Rotation);

                    if (!canPlace)
                    {
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
                                relocationSource.Value);
                        _buildingFogEffects.Apply(id, pos);
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
    }
}
