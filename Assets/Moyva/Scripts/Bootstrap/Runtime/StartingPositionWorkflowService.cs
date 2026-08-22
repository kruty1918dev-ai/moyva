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

    internal sealed partial class StartingPositionWorkflowService
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
