using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void Confirm()
        {
            IDiagnosticFlow flow = _diagnosticsSession?.CurrentFlow;
            if (IsDemolishMode)
            {
                ConfirmPendingDemolitions();
                _diagnostics?.SkipStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, "demolish-mode");
                _diagnostics?.Report(flow);
                _diagnosticsSession?.Clear(flow);
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[MoyvaBuildGridDiag] placement-start pending={_pendingPlacements.Count} state={State}");

            if (_pendingPlacements.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] Confirm ignored: no pending placements.");
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, "no-pending-placements");
                _diagnostics?.Report(flow);
                _diagnosticsSession?.Clear(flow);
                return;
            }

            _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, $"pending={_pendingPlacements.Count}");

            var pendingSnapshot = new List<PendingPlacement>(_pendingPlacements);
            var confirmedPositions = new HashSet<Vector2Int>();
            int confirmedCount = 0;
            int skippedCount = 0;

            foreach (var placement in pendingSnapshot)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;
                bool isRelocation = IsRelocation(placement);
                bool hasRelocationSource = placement.OriginalPosition.HasValue;
                Vector2Int? relocationSource = placement.OriginalPosition;
                string relocationOwnerId = _activeOwnerId;
                bool relocationWasFactionOwned = false;
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
                        if (VerboseLogs)
                            Debug.Log($"[MoyvaBuildGridDiag] placement-complete building='{id}' origin={pos} result='relocation-no-op'");
                        continue;
                    }

                    bool gateReplacementAllowed = TryResolveGateReplacement(
                        pos,
                        id,
                        out replacedOrigin,
                        out replacedBuildingId);

                    if (!CanPlaceAt(pos, pos, id, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked, relocationSource))
                    {
                        skippedCount++;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.GridCellValidated, "placement-invalid", $"building={id}, pos={pos}");
                        Debug.LogWarning($"[MoyvaBuildGridDiag] placement-failed building='{id}' origin={pos} reason='validation' occupied={tileOccupied} spacing={spacingBlocked} fog={fogBlocked} influence={influenceZoneBlocked} terrain={terrainBlocked}");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.GridCellValidated, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.TerrainValidated, $"building={id}, pos={pos}");

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && _factionPlacedBuildings.TryGetValue(relocationSource.Value, out var sourceFactionEntry))
                    {
                        relocationWasFactionOwned = true;
                        relocationOwnerId = sourceFactionEntry.FactionId;
                    }
                    else if (!hasRelocationSource
                             && gateReplacementAllowed
                             && _factionPlacedBuildings.TryGetValue(replacedOrigin, out var replacedFactionEntry))
                    {
                        relocationWasFactionOwned = true;
                        relocationOwnerId = replacedFactionEntry.FactionId;
                    }

                    if (gateReplacementAllowed)
                    {
                        UnregisterBuildingFootprint(replacedOrigin, replacedBuildingId);
                        replacementFootprintRemoved = true;
                    }

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && (!gateReplacementAllowed || relocationSource.Value != replacedOrigin))
                    {
                        UnregisterBuildingFootprint(relocationSource.Value, id);
                        relocationFootprintRemoved = true;
                    }

                    if (!TryRegisterBuildingFootprint(pos, id))
                    {
                        skippedCount++;
                        Debug.LogError($"[MoyvaBuildGridDiag] placement-failed building='{id}' origin={pos} reason='footprint-registration'");
                        continue;
                    }
                    targetFootprintRegistered = true;
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingRegistered, $"building={id}, pos={pos}");

                    if (!hasRelocationSource
                        && !TryConsumeConstructionResources(pos, id, relocationOwnerId, out var resourceReason))
                    {
                        skippedCount++;
                        _lastActionMessage = resourceReason;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, "resources-blocked", resourceReason);
                        Debug.LogWarning($"[MoyvaBuildGridDiag] placement-failed building='{id}' origin={pos} reason='{resourceReason}'");
                        continue;
                    }

                    if (!hasRelocationSource)
                    {
                        _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, $"building={id}, owner={relocationOwnerId}");
                        _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesReserved, $"building={id}, owner={relocationOwnerId}");
                    }

                    if (gateReplacementAllowed)
                        RemovePlacedRecordAt(replacedOrigin);
                    if (hasRelocationSource && relocationSource.HasValue)
                        RemovePlacedRecordAt(relocationSource.Value);

                    if (relocationWasFactionOwned)
                        _factionPlacedBuildings[pos] = (id, relocationOwnerId);
                    else
                        _playerPlacedBuildings[pos] = id;

                    modelCommitted = true;
                    confirmedPositions.Add(pos);
                    confirmedCount++;
                }
                catch (Exception ex)
                {
                    skippedCount++;
                    Debug.LogError($"[MoyvaBuildGridDiag] placement-failed building='{id}' origin={pos} reason='{ex.GetType().Name}: {ex.Message}'");
                }
                finally
                {
                    if (!modelCommitted)
                    {
                        if (targetFootprintRegistered)
                            UnregisterBuildingFootprint(pos, id);

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
                    _signalBus.Fire(new BuildingPlacedSignal
                    {
                        BuildingId = id,
                        Position = pos,
                        OwnerId = relocationOwnerId,
                        SourceFactionId = relocationWasFactionOwned ? relocationOwnerId : null,
                        HasRelocationSource = isRelocation && relocationSource.HasValue && relocationSource.Value != pos,
                        RelocationSourcePosition = relocationSource.GetValueOrDefault(),
                    });
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ConstructionSignalFired, $"building={id}, pos={pos}");

                    try
                    {
                        if (isRelocation && relocationSource.HasValue)
                            _fogOfWarService?.UnregisterUnit(GetBuildingFogVisionAreaId(relocationSource.Value));
                        ApplyBuildingFogReveal(id, pos);
                    }
                    catch (Exception fogEx)
                    {
                        Debug.LogError($"[Construction] Fog reveal failed for '{id}' at {pos}: {fogEx.GetType().Name} - {fogEx.Message}");
                    }

                    if (VerboseLogs)
                        Debug.Log($"[MoyvaBuildGridDiag] placement-complete building='{id}' origin={pos} result='{(isRelocation ? "relocated" : "placed")}' source={relocationSource?.ToString() ?? "none"}");
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

            if (VerboseLogs)
                Debug.Log($"[MoyvaBuildGridDiag] placement-batch-complete confirmed={confirmedCount} skipped={skippedCount} remaining={_pendingPlacements.Count} state={State}");

            if (confirmedPositions.Count > 0)
                _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.UiUpdated, $"confirmed={confirmedCount}, skipped={skippedCount}");
            else
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, "no-buildings-confirmed", $"skipped={skippedCount}");

            _diagnostics?.Report(flow);
            _diagnosticsSession?.Clear(flow);
        }
    }
}
