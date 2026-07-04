using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionWorkflowService
    {
        void HandleWorldGenerated(WorldGeneratedDataSignal signal);
        void HandleWorldSpawnPositions(WorldSpawnPositionsSignal signal);
    }

    internal sealed class StartingPositionWorkflowService
        : IStartingPositionWorkflowService
    {
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly ISaveService _saveService;
        private readonly IStartingPositionState _startingPositionState;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionSpawnSetupService _spawnSetupService;
        private readonly IStartingPositionRevealPresentationService _revealPresentationService;
        private readonly IStartingPositionAutoloadRecoveryService _autoloadRecoveryService;
        private readonly IStartingPositionWorkflowState _workflowState;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly ISaveLoadDiagnostics _saveLoadDiagnostics;
        private readonly ISaveLoadDiagnosticsSession _saveLoadDiagnosticsSession;

        public StartingPositionWorkflowService(
            ISaveService saveService,
            IStartingPositionState startingPositionState,
            IStartingPositionPolicy policy,
            IStartingPositionSpawnSetupService spawnSetupService,
            IStartingPositionRevealPresentationService revealPresentationService,
            IStartingPositionAutoloadRecoveryService autoloadRecoveryService,
            IStartingPositionWorkflowState workflowState,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnostics saveLoadDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnosticsSession saveLoadDiagnosticsSession = null)
        {
            _saveService = saveService;
            _startingPositionState = startingPositionState;
            _policy = policy;
            _spawnSetupService = spawnSetupService;
            _revealPresentationService = revealPresentationService;
            _autoloadRecoveryService = autoloadRecoveryService;
            _workflowState = workflowState;
            _worldDiagnostics = worldDiagnostics;
            _saveLoadDiagnostics = saveLoadDiagnostics;
            _saveLoadDiagnosticsSession = saveLoadDiagnosticsSession;
            Debug.Log($"{DirectDiagTag} Workflow.Construct state={startingPositionState != null}, policy={policy != null}, spawnSetup={spawnSetupService != null}, fogReveal={revealPresentationService != null}, loadedFogRepair={autoloadRecoveryService != null}, cameraService={revealPresentationService != null}, saveService={saveService != null}.");
        }

        public void HandleWorldGenerated(WorldGeneratedDataSignal signal)
        {
            ResetForNewStartupWorldIfNeeded(signal.StartupSequence, signal.StartupSessionId, $"world:{signal.Source}");
            Debug.Log($"{DirectDiagTag} Workflow.HandleWorldGenerated ENTER signalNull=false, map={signal.Width}x{signal.Height}, pendingWorldBefore={_workflowState.HasPendingWorldGeneratedSignal}, startStateSet={_startingPositionState.IsSet}.");
            _workflowState.PendingWorldGeneratedSignal = signal;
            _workflowState.HasPendingWorldGeneratedSignal = true;
            Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.OnWorldGenerated map={signal.Width}x{signal.Height}, startStateSet={_startingPositionState.IsSet}, startLogicApplied={_workflowState.StartLogicApplied}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, slot={GameLaunchContext.SaveSlot}.");

            Debug.Log($"{DirectDiagTag} Workflow.HandleWorldGenerated CALL TryApplyStartLogic reason=HandleWorldGenerated.");
            TryApplyStartLogic("HandleWorldGenerated");
        }

        public void HandleWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            ResetForNewStartupWorldIfNeeded(signal.StartupSequence, signal.StartupSessionId, $"spawns:{signal.Source}");
            Debug.Log($"{DirectDiagTag} Workflow.HandleWorldSpawnPositions ENTER signalNull={(signal.Assignments == null)}, assignments={signal.Assignments?.Length ?? 0}, startStateSetBefore={_startingPositionState.IsSet}.");
            if (signal.Assignments == null || signal.Assignments.Length == 0)
            {
                Debug.LogWarning($"{StartingPositionInitializer.DebugTag} Bootstrap.OnWorldSpawnPositions ignored empty assignments.");
                return;
            }

            if (_workflowState.HasPendingWorldGeneratedSignal
                && signal.StartupSequence > 0
                && _workflowState.PendingWorldGeneratedSignal.StartupSequence > 0
                && signal.StartupSequence < _workflowState.PendingWorldGeneratedSignal.StartupSequence)
            {
                Debug.LogWarning($"{StartDiagTag} Workflow.HandleWorldSpawnPositions SKIP stale sequence={signal.StartupSequence}, pendingWorldSequence={_workflowState.PendingWorldGeneratedSignal.StartupSequence}, source={signal.Source}.");
                return;
            }

            _startingPositionState.Set(signal.Assignments);
            Debug.Log($"{DirectDiagTag} Workflow.HandleWorldSpawnPositions stateSetAfter={_startingPositionState.IsSet}, localSpawn={_startingPositionState.StartPosition}.");
            Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.OnWorldSpawnPositions assignments={signal.Assignments.Length}, hasPendingWorld={_workflowState.HasPendingWorldGeneratedSignal}, startLogicApplied={_workflowState.StartLogicApplied}.");

            if (!_workflowState.HasPendingWorldGeneratedSignal)
                return;

            if (_workflowState.StartLogicApplied)
            {
                ReapplyStartRevealIfNeeded(_workflowState.PendingWorldGeneratedSignal);
                return;
            }

            Debug.Log($"{DirectDiagTag} Workflow.HandleWorldSpawnPositions CALL TryApplyStartLogic reason=HandleWorldSpawnPositions.");
            TryApplyStartLogic("HandleWorldSpawnPositions");
        }

        private void TryApplyStartLogic(string caller)
        {
            bool hasPendingWorldSignal = _workflowState.HasPendingWorldGeneratedSignal;
            WorldGeneratedDataSignal signal = hasPendingWorldSignal
                ? _workflowState.PendingWorldGeneratedSignal
                : default;
            bool autoLoad = GameLaunchContext.IsAutoLoadEnabled();
            int slot = GameLaunchContext.SaveSlot;
            bool hasSave = _saveService != null && _saveService.HasSave(slot);
            bool canRun = _policy.CanRunStartLogic();
            bool shouldCompute = _policy.ShouldComputeHostStartPositions();
            bool isMultiplayerContext = _policy.IsMultiplayerLaunchContext();
            int participantCount = _policy.Participants?.Count ?? 0;
            int launchExtraSlots = ResolveLaunchExtraSlotsEquivalent(participantCount);

            Debug.Log(
                $"{PolicyDiagTag} TryApplyStartLogic ENTER caller={caller ?? "unknown"}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, " +
                $"maxPlayers={GameLaunchContext.MaxPlayers}, launchExtraSlots={launchExtraSlots}, autoLoad={autoLoad}, hasSave={hasSave}, hasWorld={hasPendingWorldSignal}, " +
                $"map={signal.Width}x{signal.Height}, startStateSet={_startingPositionState.IsSet}, startLogicApplied={_workflowState.StartLogicApplied}, " +
                $"startRevealApplied={_workflowState.StartRevealApplied}, canRun={canRun}, shouldCompute={shouldCompute}, isMultiplayerContext={isMultiplayerContext}, participants={participantCount}.");
            Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic ENTER reason={caller ?? "unknown"}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, autoLoad={autoLoad}, hasSave={hasSave}, hasWorld={hasPendingWorldSignal}, map={signal.Width}x{signal.Height}, startStateSet={_startingPositionState.IsSet}, startLogicApplied={_workflowState.StartLogicApplied}, startRevealApplied={_workflowState.StartRevealApplied}.");
            Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic POLICY canRun={canRun}, shouldCompute={shouldCompute}, isMultiplayerContext={isMultiplayerContext}.");

            if (!hasPendingWorldSignal)
            {
                Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason=no-world-signal.");
                Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=no-world-signal caller={caller ?? "unknown"}.");
                Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic skipped startLogicApplied={_workflowState.StartLogicApplied}, hasWorld={hasPendingWorldSignal}.");
                return;
            }

            if (_workflowState.StartLogicApplied)
            {
                Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason=already-applied.");
                Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=already-applied caller={caller ?? "unknown"}.");
                Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic skipped startLogicApplied={_workflowState.StartLogicApplied}, hasWorld={hasPendingWorldSignal}.");
                return;
            }

            Debug.Log($"{StartDiagTag} TryApplyStartLogic branch autoLoad={autoLoad}, hasSave={hasSave}, slot={slot}, canRun={canRun}, shouldCompute={shouldCompute}, startStateSet={_startingPositionState.IsSet}, startLogicApplied={_workflowState.StartLogicApplied}, hasPendingWorld={_workflowState.HasPendingWorldGeneratedSignal}.");
            Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic begin map={signal.Width}x{signal.Height}, startStateSet={_startingPositionState.IsSet}, canRun={canRun}, shouldCompute={shouldCompute}, autoLoad={autoLoad}, hasSave={hasSave}, slot={slot}.");

            if (autoLoad && hasSave)
            {
                _worldDiagnostics?.BeginStartingPositionLoadRecovery(
                    $"caller={caller ?? "unknown"}, slot={slot}, map={signal.Width}x{signal.Height}");
                Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason=autoload-has-save.");
                Debug.Log($"{StartDiagTag} TryApplyStartLogic path=auto-load map={signal.Width}x{signal.Height}, slot={slot}.");
                Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=autoload-has-save caller={caller ?? "unknown"}, slot={slot}.");
                Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic auto-load path: repair-check and camera teleport.");
                bool repaired = _autoloadRecoveryService.RepairLoadedFogIfNeeded(signal);
                _worldDiagnostics?.FogSnapshotValidated(
                    $"slot={slot}, repaired={repaired}, map={signal.Width}x{signal.Height}");
                if (repaired)
                    _worldDiagnostics?.FogRepairApplied($"slot={slot}, map={signal.Width}x{signal.Height}");
                else
                    _worldDiagnostics?.FogRepairSkipped($"slot={slot}, reason=snapshot-usable-or-policy-blocked");
                Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
                _revealPresentationService.TeleportMainCamera(
                    _autoloadRecoveryService.ResolveStartupCameraTarget(baseMapSize.x, baseMapSize.y, preferStartTile: false),
                    signal);
                _worldDiagnostics?.CameraFocused($"source=autoload, map={baseMapSize.x}x{baseMapSize.y}");
                _saveLoadDiagnostics?.CompleteStep(_saveLoadDiagnosticsSession?.CurrentFlow, SaveLoadDiagnosticSteps.CameraFocused, $"source=autoload, map={baseMapSize.x}x{baseMapSize.y}");
                _saveLoadDiagnostics?.CompleteStep(_saveLoadDiagnosticsSession?.CurrentFlow, SaveLoadDiagnosticSteps.LoadCompleted, $"slot={slot}, map={baseMapSize.x}x{baseMapSize.y}");
                _saveLoadDiagnostics?.Report(_saveLoadDiagnosticsSession?.CurrentFlow);
                _saveLoadDiagnosticsSession?.Clear(_saveLoadDiagnosticsSession?.CurrentFlow);
                _workflowState.StartLogicApplied = true;
                return;
            }

            _worldDiagnostics?.BeginStartingPositionNewGame(
                $"caller={caller ?? "unknown"}, map={signal.Width}x{signal.Height}, mode={GameLaunchContext.Mode}");

            Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic CALL SpawnSetup.TryPrepareStartingPositions.");
            bool spawnSetupTriggered = _spawnSetupService.TryPrepareStartingPositions(signal);
            Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic SpawnSetup result={spawnSetupTriggered}, startStateSet={_startingPositionState.IsSet}.");
            if (spawnSetupTriggered)
            {
                Debug.Log($"{StartDiagTag} TryApplyStartLogic spawn-setup-triggered shouldCompute={shouldCompute}, startStateSetAfter={_startingPositionState.IsSet}, startLogicAppliedAfter={_workflowState.StartLogicApplied}.");
                if (_workflowState.StartLogicApplied)
                {
                    Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason=already-applied.");
                    Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=already-applied caller={caller ?? "unknown"}, after=spawn-setup-reentrant.");
                    Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic reentrant apply completed after WorldSpawnPositionsSignal.");
                    return;
                }
            }

            if (!canRun)
            {
                Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason=policy-blocked.");
                Debug.LogWarning($"{StartDiagTag} TryApplyStartLogic blocked canRun={canRun}, shouldCompute={shouldCompute}, startStateSet={_startingPositionState.IsSet}, autoLoad={autoLoad}, hasSave={hasSave}.");
                Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=policy-blocked caller={caller ?? "unknown"}, shouldCompute={shouldCompute}, isMultiplayerContext={isMultiplayerContext}, participants={participantCount}, sessionManager={_policy.HasSessionManager}.");
                Debug.LogWarning($"{StartingPositionInitializer.DebugTag} Bootstrap.TryApplyStartLogic blocked canRun={canRun}, startStateSet={_startingPositionState.IsSet}, sessionManager={_policy.HasSessionManager}, multiplayerLaunch={isMultiplayerContext}.");
                return;
            }

            if (!_startingPositionState.IsSet)
            {
                Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic EXIT reason={(shouldCompute && !spawnSetupTriggered ? "spawn-setup-failed" : "start-state-not-set")}.");
                if (shouldCompute && !spawnSetupTriggered)
                    Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=spawn-setup-failed caller={caller ?? "unknown"}, shouldCompute={shouldCompute}, participants={participantCount}.");
                Debug.LogWarning($"{PolicyDiagTag} TryApplyStartLogic EXIT reason=start-state-not-set caller={caller ?? "unknown"}, shouldCompute={shouldCompute}, spawnSetupTriggered={spawnSetupTriggered}.");
                return;
            }

            Vector2Int baseMapSizeForCall = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Vector2Int directCenter = _revealPresentationService.ResolveRevealCenter(baseMapSizeForCall.x, baseMapSizeForCall.y);
            Debug.Log($"{DirectDiagTag} Workflow.TryApplyStartLogic CALL ApplyStartReveal center={directCenter}, source=start-state.");
            Debug.Log($"{StartDiagTag} TryApplyStartLogic path=new-game-reveal map={signal.Width}x{signal.Height}, teleportCamera=true.");
            ApplyStartReveal(signal, teleportCamera: true);
        }

        private void ApplyStartReveal(WorldGeneratedDataSignal signal, bool teleportCamera)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Vector2Int revealCenter = _revealPresentationService.ResolveRevealCenter(baseMapSize.x, baseMapSize.y);
            bool revealChanged = !_workflowState.StartRevealApplied
                || _workflowState.AppliedStartRevealWidth != baseMapSize.x
                || _workflowState.AppliedStartRevealHeight != baseMapSize.y
                || _workflowState.AppliedStartRevealCenter != revealCenter;
            Debug.Log($"{DirectDiagTag} Workflow.ApplyStartReveal ENTER center={revealCenter}, source=start-state, map={baseMapSize.x}x{baseMapSize.y}, startRevealAppliedBefore={_workflowState.StartRevealApplied}.");
            Debug.Log($"{StartDiagTag} ApplyStartReveal begin center={revealCenter}, baseMap={baseMapSize.x}x{baseMapSize.y}, signal={signal.Width}x{signal.Height}, revealChanged={revealChanged}, teleportCamera={teleportCamera}, startRevealApplied={_workflowState.StartRevealApplied}, cameraTeleported={_workflowState.StartupCameraTeleported}.");
            Debug.Log($"{StartingPositionInitializer.DebugTag} Bootstrap.ApplyStartReveal center={revealCenter}, map={signal.Width}x{signal.Height}, baseMap={baseMapSize.x}x{baseMapSize.y}, revealChanged={revealChanged}, teleportCamera={teleportCamera}, alreadyTeleported={_workflowState.StartupCameraTeleported}.");

            if (revealChanged)
            {
                Debug.Log($"{DirectDiagTag} Workflow.ApplyStartReveal CALL FogRevealService.ApplyReveal/RevealingMethod.");
                _revealPresentationService.ApplyReveal(baseMapSize.x, baseMapSize.y, revealCenter);
                _worldDiagnostics?.FogRevealApplied($"center={revealCenter}, map={baseMapSize.x}x{baseMapSize.y}");
                _workflowState.StartRevealApplied = true;
                _workflowState.AppliedStartRevealWidth = baseMapSize.x;
                _workflowState.AppliedStartRevealHeight = baseMapSize.y;
                _workflowState.AppliedStartRevealCenter = revealCenter;
            }

            if (teleportCamera && !_workflowState.StartupCameraTeleported)
            {
                _revealPresentationService.TeleportMainCamera(revealCenter, signal);
                _worldDiagnostics?.CameraFocused($"center={revealCenter}, map={baseMapSize.x}x{baseMapSize.y}");
                _workflowState.StartupCameraTeleported = true;
            }

            _workflowState.StartLogicApplied = true;
            Debug.Log($"{DirectDiagTag} Workflow.ApplyStartReveal EXIT startRevealAppliedAfter={_workflowState.StartRevealApplied}.");
            Debug.Log($"{StartDiagTag} ApplyStartReveal result center={revealCenter}, revealApplied={_workflowState.StartRevealApplied}, startLogicApplied={_workflowState.StartLogicApplied}, cameraTeleported={_workflowState.StartupCameraTeleported}, appliedMap={_workflowState.AppliedStartRevealWidth}x{_workflowState.AppliedStartRevealHeight}.");
            Debug.Log($"[Bootstrap] Стартова позиція: {revealCenter}. Туман розкрито, камеру переміщено.");
        }

        private void ReapplyStartRevealIfNeeded(WorldGeneratedDataSignal signal)
        {
            Debug.Log($"{DirectDiagTag} Workflow.ReapplyStartRevealIfNeeded ENTER map={signal.Width}x{signal.Height}.");
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            Vector2Int revealCenter = _revealPresentationService.ResolveRevealCenter(baseMapSize.x, baseMapSize.y);
            bool revealCenterChanged = _workflowState.AppliedStartRevealCenter != revealCenter;
            if (_workflowState.StartRevealApplied
                && _workflowState.AppliedStartRevealWidth == baseMapSize.x
                && _workflowState.AppliedStartRevealHeight == baseMapSize.y
                && _workflowState.AppliedStartRevealCenter == revealCenter)
            {
                return;
            }

            ApplyStartReveal(signal, teleportCamera: !_workflowState.StartupCameraTeleported || revealCenterChanged);
        }

        private void ResetForNewStartupWorldIfNeeded(long startupSequence, string startupSessionId, string reason)
        {
            if (startupSequence <= 0)
                return;

            if (_workflowState.CurrentStartupSequence == startupSequence
                && string.Equals(_workflowState.CurrentStartupSessionId, startupSessionId))
            {
                return;
            }

            _workflowState.CurrentStartupSequence = startupSequence;
            _workflowState.CurrentStartupSessionId = startupSessionId;
            _workflowState.StartLogicApplied = false;
            _workflowState.StartRevealApplied = false;
            _workflowState.StartupCameraTeleported = false;
            _workflowState.AppliedStartRevealCenter = default;
            _workflowState.AppliedStartRevealWidth = 0;
            _workflowState.AppliedStartRevealHeight = 0;
            _workflowState.HasPendingWorldGeneratedSignal = false;
            _workflowState.PendingWorldGeneratedSignal = default;
            _startingPositionState.Reset();
            Debug.Log($"{StartDiagTag} Workflow.ResetForNewStartupWorld sequence={startupSequence}, session={startupSessionId ?? "<null>"}, reason={reason}.");
        }

        private static int ResolveLaunchExtraSlotsEquivalent(int participantCount)
        {
            int normalizedParticipantCount = Mathf.Max(1, participantCount);
            return Mathf.Max(0, GameLaunchContext.MaxPlayers - normalizedParticipantCount);
        }
    }
}
