// AI-context consolidation: related partials live together by responsibility.
// No gameplay behavior is intentionally changed by this file organization.
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using System.Collections.ObjectModel;

// ---- Consolidated from ConstructionService.Confirm.cs ----
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
            if (!CanActiveOwnerAct(out string turnReason))
            {
                _lastActionMessage = turnReason;
                Debug.LogWarning($"[Construction] Confirm rejected: {turnReason}");
                return;
            }

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
                        relocationOwnerId = sourceFactionEntry.FactionId;
                    }

                    double footprintStartedAt =
                        Time.realtimeSinceStartupAsDouble;

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

                    _playerPlacedBuildings.Remove(pos);
                    _factionPlacedBuildings[pos] =
                        (id, NormalizeOwnerId(relocationOwnerId));
                    _placedRotationByOrigin[pos] =
                        placement.Rotation;

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

                    double signalStartedAt =
                        Time.realtimeSinceStartupAsDouble;

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

// ---- Consolidated from ConstructionService.DemolishUndo.cs ----
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
            if (!CanActiveOwnerMutate(
                    "undo construction preview",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return;
            }

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
            if (!CanActiveOwnerMutate(
                    "redo construction preview",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return;
            }

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

            position = _footprints.ResolveOrigin(position);
            if (!TryResolveCommittedBuildingForOwner(
                    position,
                    _activeOwnerId,
                    out Vector2Int origin,
                    out string buildingId,
                    out string ownershipReason))
            {
                _lastActionMessage = ownershipReason;
                Debug.LogWarning(
                    $"[Construction] TryDemolishAt({position}) rejected: {ownershipReason}");
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
        {
            string activeOwner = NormalizeOwnerId(_activeOwnerId);
            var snapshot = new Dictionary<Vector2Int, string>();
            foreach (var pair in _factionPlacedBuildings)
            {
                if (string.Equals(
                        NormalizeOwnerId(pair.Value.FactionId),
                        activeOwner,
                        System.StringComparison.Ordinal))
                {
                    snapshot[pair.Key] = pair.Value.BuildingId;
                }
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (!snapshot.ContainsKey(pair.Key))
                    snapshot[pair.Key] = pair.Value;
            }

            return new ReadOnlyDictionary<Vector2Int, string>(snapshot);
        }

        private void ConfirmPendingDemolitions()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm demolish requested. count={_pendingDemolitions.Count}");

            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                var pos = demolition.Position;
                var id = demolition.BuildingId;

                if (!TryDemolishByFaction(
                        pos,
                        _activeOwnerId))
                {
                    continue;
                }

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

            _signalBus.Fire(new BuildingCancelledSignal());
            SetPlacementSelection(null, BuildingPlacementState.Idle);
            ApplyBootstrapCastleSelectionIfNeeded();

            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession completed. state={State}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
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
