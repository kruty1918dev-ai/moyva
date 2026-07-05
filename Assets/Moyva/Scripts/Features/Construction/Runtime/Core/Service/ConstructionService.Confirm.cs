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
                Debug.Log($"[Construction] Confirm requested. count={_pendingPlacements.Count}");

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
                try
                {
                    bool gateReplacementAllowed = _wallTopologyService != null
                        && _wallGateReplacementValidator != null
                        && _wallTopologyService.IsGate(id)
                        && _wallGateReplacementValidator.CanReplaceWallWithGate(pos, id, out _);

                    if (!gateReplacementAllowed && !CanPlaceAt(pos, pos, id, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked))
                    {
                        skippedCount++;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.GridCellValidated, "placement-invalid", $"building={id}, pos={pos}");
                        Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: placement became invalid. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, influenceZoneBlocked={influenceZoneBlocked}, terrainBlocked={terrainBlocked}, townHallBuildRadius={_townHallBuildRadius}.");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.GridCellValidated, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.TerrainValidated, $"building={id}, pos={pos}");

                    if (_objectsMapService.IsOccupied(pos))
                    {
                        if (!gateReplacementAllowed)
                        {
                            skippedCount++;
                            Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: tile occupied.");
                            continue;
                        }

                        _objectsMapService.Unregister(pos);
                    }

                    if (!TryConsumeConstructionResources(pos, id, _activeOwnerId, out var resourceReason))
                    {
                        skippedCount++;
                        _lastActionMessage = resourceReason;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, "resources-blocked", resourceReason);
                        Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: {resourceReason}");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, $"building={id}, owner={_activeOwnerId}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesReserved, $"building={id}, owner={_activeOwnerId}");

                    _objectsMapService.Register(pos, id);
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingRegistered, $"building={id}, pos={pos}");
                    _playerPlacedBuildings[pos] = id;

                    _signalBus.Fire(new BuildingPlacedSignal
                    {
                        BuildingId = id,
                        Position = pos,
                        OwnerId = _activeOwnerId,
                    });
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ConstructionSignalFired, $"building={id}, pos={pos}");

                    try
                    {
                        ApplyBuildingFogReveal(id, pos);
                    }
                    catch (Exception fogEx)
                    {
                        Debug.LogError($"[Construction] Fog reveal failed for '{id}' at {pos}: {fogEx.GetType().Name} - {fogEx.Message}");
                    }

                    confirmedPositions.Add(pos);
                    confirmedCount++;

                    if (VerboseLogs)
                        Debug.Log($"[Construction] Confirm placed '{id}' at {pos}");
                }
                catch (Exception ex)
                {
                    skippedCount++;
                    Debug.LogError($"[Construction] Confirm failed for '{id}' at {pos}: {ex.GetType().Name} - {ex.Message}");
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
            {
                State = BuildingPlacementState.Idle;
                _selectedBuildingId = null;
            }
            else
            {
                State = BuildingPlacementState.Placing;
                _selectedBuildingId = _pendingPlacements[_pendingPlacements.Count - 1].BuildingId;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm completed. confirmed={confirmedCount}, skipped={skippedCount}, remainingPending={_pendingPlacements.Count}, state={State}");

            if (confirmedCount > 0)
                _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.UiUpdated, $"confirmed={confirmedCount}, skipped={skippedCount}");
            else
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, "no-buildings-confirmed", $"skipped={skippedCount}");

            _diagnostics?.Report(flow);
            _diagnosticsSession?.Clear(flow);
        }
    }
}
