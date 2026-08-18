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
        private const double ConfirmPerfWarnThresholdMs = 8d;
        private const double ConfirmStageWarnThresholdMs = 4d;

        private readonly List<PendingPlacement> _confirmPendingSnapshot = new();
        private readonly HashSet<Vector2Int> _confirmConfirmedPositions = new();

        private static double ConfirmElapsedMs(double startedAt)
            => (Time.realtimeSinceStartupAsDouble - startedAt) * 1000d;

        private static void AddConfirmStage(
            ref double accumulatorMs,
            double startedAt)
        {
            accumulatorMs += ConfirmElapsedMs(startedAt);
        }

        private static void LogConfirmPerformance(
            int pendingAtStart,
            int confirmedCount,
            int skippedCount,
            int remainingCount,
            double totalMs,
            double snapshotMs,
            double validationMs,
            double footprintMs,
            double resourcesMs,
            double signalMs,
            double fogMs,
            double cleanupMs,
            double selectionMs,
            double diagnosticsMs)
        {
            if (!Debug.isDebugBuild)
                return;

            double maxStage = Math.Max(
                validationMs,
                Math.Max(
                    footprintMs,
                    Math.Max(
                        resourcesMs,
                        Math.Max(
                            signalMs,
                            Math.Max(
                                fogMs,
                                Math.Max(
                                    cleanupMs,
                                    Math.Max(
                                        selectionMs,
                                        diagnosticsMs)))))));

            string message =
                $"{PerfLogTag} confirm " +
                $"pending={pendingAtStart} confirmed={confirmedCount} " +
                $"skipped={skippedCount} remaining={remainingCount} " +
                $"totalMs={totalMs:F3} " +
                $"snapshot={snapshotMs:F3} " +
                $"validation={validationMs:F3} " +
                $"footprint={footprintMs:F3} " +
                $"resources={resourcesMs:F3} " +
                $"signal={signalMs:F3} " +
                $"fog={fogMs:F3} " +
                $"cleanup={cleanupMs:F3} " +
                $"selection={selectionMs:F3} " +
                $"diagnostics={diagnosticsMs:F3}";

            if (totalMs >= ConfirmPerfWarnThresholdMs
                || maxStage >= ConfirmStageWarnThresholdMs)
            {
                Debug.LogWarning(message);
            }
            else
            {
                Debug.Log(message);
            }
        }

        public void Confirm()
        {
            double confirmStartedAt =
                Time.realtimeSinceStartupAsDouble;
            int pendingAtStart = _pendingPlacements.Count;

            double snapshotMs = 0d;
            double validationMs = 0d;
            double footprintMs = 0d;
            double resourcesMs = 0d;
            double signalMs = 0d;
            double fogMs = 0d;
            double cleanupMs = 0d;
            double selectionMs = 0d;
            double diagnosticsMs = 0d;

            using var commitAudit =
                ConstructionCommitAudit.Begin(
                    _pendingPlacements.Count,
                    IsDemolishMode,
                    State.ToString());

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

            double snapshotStartedAt =
                Time.realtimeSinceStartupAsDouble;

            Debug.Log(
                $"[MoyvaConstructionAvailability] confirm-start " +
                $"pending={_pendingPlacements.Count} owner='{NormalizeOwnerId(_activeOwnerId)}'");

            _confirmPendingSnapshot.Clear();
            _confirmPendingSnapshot.AddRange(_pendingPlacements);
            _confirmConfirmedPositions.Clear();

            List<PendingPlacement> pendingSnapshot =
                _confirmPendingSnapshot;
            HashSet<Vector2Int> confirmedPositions =
                _confirmConfirmedPositions;

            AddConfirmStage(
                ref snapshotMs,
                snapshotStartedAt);
            commitAudit.Step("snapshot");
            int confirmedCount = 0;
            int skippedCount = 0;

            foreach (var placement in pendingSnapshot)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;
                commitAudit.ObservePlacement(id, pos);
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

                    double validationStartedAt =
                        Time.realtimeSinceStartupAsDouble;

                    bool gateReplacementAllowed = TryResolveGateReplacement(
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
                        placement.ReplacedPendingBuildingId);

                    AddConfirmStage(
                        ref validationMs,
                        validationStartedAt);

                    if (!canPlace)
                    {
                        skippedCount++;
                        _diagnostics?.FailStep(
                            flow,
                            ConstructionDiagnosticSteps.GridCellValidated,
                            "placement-invalid",
                            $"building={id}, pos={pos}");
                        Debug.LogWarning(
                            $"[MoyvaBuildGridDiag] placement-failed " +
                            $"building='{id}' origin={pos} reason='validation' " +
                            $"occupied={tileOccupied} spacing={spacingBlocked} " +
                            $"fog={fogBlocked} influence={influenceZoneBlocked} " +
                            $"terrain={terrainBlocked}");
                        commitAudit.Step("validation-failed");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.GridCellValidated, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.TerrainValidated, $"building={id}, pos={pos}");
                    commitAudit.Step("validation");

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && _factionPlacedBuildings.TryGetValue(relocationSource.Value, out var sourceFactionEntry))
                    {
                        relocationWasFactionOwned = true;
                        relocationOwnerId = sourceFactionEntry.FactionId;
                    }
                    else if (!hasRelocationSource
                             && gateReplacementAllowed
                             && _factionPlacedBuildings.ContainsKey(
                                 replacedOrigin))
                    {
                        // Replacement changes the occupant, not the actor. Keep
                        // the active owner (especially when RequireSameOwner is
                        // disabled) instead of inheriting the replaced object.
                        relocationWasFactionOwned = true;
                    }

                    double footprintStartedAt =
                        Time.realtimeSinceStartupAsDouble;

                    if (gateReplacementAllowed)
                    {
                        UnregisterBuildingFootprint(
                            replacedOrigin,
                            replacedBuildingId);
                        replacementFootprintRemoved = true;
                    }

                    if (hasRelocationSource
                        && relocationSource.HasValue
                        && (!gateReplacementAllowed
                            || relocationSource.Value != replacedOrigin))
                    {
                        UnregisterBuildingFootprint(
                            relocationSource.Value,
                            id);
                        relocationFootprintRemoved = true;
                    }

                    bool footprintRegistered =
                        TryRegisterBuildingFootprint(pos, id);

                    AddConfirmStage(
                        ref footprintMs,
                        footprintStartedAt);

                    if (!footprintRegistered)
                    {
                        skippedCount++;
                        Debug.LogError(
                            $"[MoyvaBuildGridDiag] placement-failed " +
                            $"building='{id}' origin={pos} " +
                            $"reason='footprint-registration'");
                        continue;
                    }

                    targetFootprintRegistered = true;
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingRegistered, $"building={id}, pos={pos}");
                    commitAudit.Step("footprint");

                    double resourcesStartedAt =
                        Time.realtimeSinceStartupAsDouble;

                    string resourceReason = null;
                    bool resourcesAccepted =
                        hasRelocationSource
                        || TryConsumeConstructionResources(
                            pos,
                            id,
                            relocationOwnerId,
                            out resourceReason);

                    AddConfirmStage(
                        ref resourcesMs,
                        resourcesStartedAt);

                    if (!resourcesAccepted)
                    {
                        skippedCount++;
                        _lastActionMessage = resourceReason;
                        _signalBus.Fire(
                            new BuildingPreviewChangedSignal
                            {
                                Position = pos,
                                BuildingId = id,
                                PreviewState =
                                    BuildingPreviewState.Unaffordable,
                            });
                        _diagnostics?.FailStep(
                            flow,
                            ConstructionDiagnosticSteps.ResourcesChecked,
                            "resources-blocked",
                            resourceReason);
                        Debug.LogWarning(
                            $"[MoyvaBuildGridDiag] placement-failed " +
                            $"building='{id}' origin={pos} " +
                            $"reason='{resourceReason}'");
                        Debug.LogWarning(
                            $"[MoyvaConstructionAvailability] confirm-blocked " +
                            $"building='{id}' origin={pos} code='resources' " +
                            $"reason='{resourceReason}'");
                        commitAudit.Step("resources-failed");
                        continue;
                    }

                    if (!hasRelocationSource)
                    {
                        _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, $"building={id}, owner={relocationOwnerId}");
                        _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesReserved, $"building={id}, owner={relocationOwnerId}");
                    }

                    commitAudit.Step("resources");

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
                    commitAudit.Step("model-commit");
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
                    InvalidatePlacementAvailabilityCache();

                    double signalStartedAt =
                        Time.realtimeSinceStartupAsDouble;

                    _signalBus.Fire(new BuildingPlacedSignal
                    {
                        BuildingId = id,
                        Position = pos,
                        OwnerId = relocationOwnerId,
                        SourceFactionId = relocationWasFactionOwned ? relocationOwnerId : null,
                        HasRelocationSource = isRelocation && relocationSource.HasValue && relocationSource.Value != pos,
                        RelocationSourcePosition = relocationSource.GetValueOrDefault(),
                    });

                    AddConfirmStage(
                        ref signalMs,
                        signalStartedAt);
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ConstructionSignalFired, $"building={id}, pos={pos}");
                    commitAudit.Step("building-placed-signal");

                    double fogStartedAt =
                        Time.realtimeSinceStartupAsDouble;

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
                    finally
                    {
                        AddConfirmStage(
                            ref fogMs,
                            fogStartedAt);
                    }

                    commitAudit.Step("fog-reveal");

                    if (VerboseLogs)
                        Debug.Log($"[MoyvaBuildGridDiag] placement-complete building='{id}' origin={pos} result='{(isRelocation ? "relocated" : "placed")}' source={relocationSource?.ToString() ?? "none"}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MoyvaBuildGridDiag] placement-notification-failed building='{id}' origin={pos} reason='{ex.GetType().Name}: {ex.Message}'");
                }
            }

            double cleanupStartedAt =
                Time.realtimeSinceStartupAsDouble;

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

            AddConfirmStage(
                ref cleanupMs,
                cleanupStartedAt);
            commitAudit.Step("preview-cleanup");

            double selectionStartedAt =
                Time.realtimeSinceStartupAsDouble;

            _undoSnapshots.Clear();
            _redoSnapshots.Clear();

            if (_pendingPlacements.Count == 0)
                SetPlacementSelection(null, BuildingPlacementState.Idle);
            else
                SetPlacementSelection(
                    _pendingPlacements[_pendingPlacements.Count - 1].BuildingId,
                    BuildingPlacementState.Placing);

            AddConfirmStage(
                ref selectionMs,
                selectionStartedAt);
            commitAudit.Step("selection-state");

            if (VerboseLogs)
                Debug.Log($"[MoyvaBuildGridDiag] placement-batch-complete confirmed={confirmedCount} skipped={skippedCount} remaining={_pendingPlacements.Count} state={State}");

            double diagnosticsStartedAt =
                Time.realtimeSinceStartupAsDouble;

            if (confirmedPositions.Count > 0)
                _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.UiUpdated, $"confirmed={confirmedCount}, skipped={skippedCount}");
            else
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, "no-buildings-confirmed", $"skipped={skippedCount}");

            Debug.Log(
                $"[MoyvaConstructionAvailability] confirm-end " +
                $"confirmed={confirmedCount} blocked={skippedCount} " +
                $"remaining={_pendingPlacements.Count}");

            commitAudit.SetOutcome(
                confirmedCount,
                skippedCount,
                _pendingPlacements.Count);
            commitAudit.Step("finalize");

            _diagnostics?.Report(flow);
            _diagnosticsSession?.Clear(flow);

            AddConfirmStage(
                ref diagnosticsMs,
                diagnosticsStartedAt);

            double totalMs =
                ConfirmElapsedMs(confirmStartedAt);

            LogConfirmPerformance(
                pendingAtStart,
                confirmedCount,
                skippedCount,
                _pendingPlacements.Count,
                totalMs,
                snapshotMs,
                validationMs,
                footprintMs,
                resourcesMs,
                signalMs,
                fogMs,
                cleanupMs,
                selectionMs,
                diagnosticsMs);
        }
    }
}
