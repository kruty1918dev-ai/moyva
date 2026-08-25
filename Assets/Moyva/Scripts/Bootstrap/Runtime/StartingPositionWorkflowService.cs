using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;

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
        private readonly ISaveService _saveService;
        private readonly IStartingPositionState _startingPositionState;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionSpawnSetupService _spawnSetupService;
        private readonly IStartingPositionRevealPresentationService _revealPresentationService;
        private readonly IStartingPositionAutoloadRecoveryService _autoloadRecoveryService;
        private readonly IStartingPositionWorkflowState _workflowState;
        public StartingPositionWorkflowService(
            ISaveService saveService,
            IStartingPositionState startingPositionState,
            IStartingPositionPolicy policy,
            IStartingPositionSpawnSetupService spawnSetupService,
            IStartingPositionRevealPresentationService revealPresentationService,
            IStartingPositionAutoloadRecoveryService autoloadRecoveryService,
            IStartingPositionWorkflowState workflowState)
        {
            _saveService = saveService;
            _startingPositionState = startingPositionState;
            _policy = policy;
            _spawnSetupService = spawnSetupService;
            _revealPresentationService = revealPresentationService;
            _autoloadRecoveryService = autoloadRecoveryService;
            _workflowState = workflowState;
        }

        public void HandleWorldGenerated(WorldGeneratedDataSignal signal)
        {
            ResetForNewStartupWorldIfNeeded(signal.StartupSequence, signal.StartupSessionId);
            _workflowState.PendingWorldGeneratedSignal = signal;
            _workflowState.HasPendingWorldGeneratedSignal = true;
            TryApplyStartLogic();
        }

        public void HandleWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            ResetForNewStartupWorldIfNeeded(signal.StartupSequence, signal.StartupSessionId);
            if (signal.Assignments == null || signal.Assignments.Length == 0)
            {
                return;
            }

            if (_workflowState.HasPendingWorldGeneratedSignal
                && signal.StartupSequence > 0
                && _workflowState.PendingWorldGeneratedSignal.StartupSequence > 0
                && signal.StartupSequence < _workflowState.PendingWorldGeneratedSignal.StartupSequence)
            {
                return;
            }

            _startingPositionState.Set(signal.Assignments);

            if (!_workflowState.HasPendingWorldGeneratedSignal)
                return;

            if (_workflowState.StartLogicApplied)
            {
                ReapplyStartRevealIfNeeded(_workflowState.PendingWorldGeneratedSignal);
                return;
            }
            TryApplyStartLogic();
        }

        private void ResetForNewStartupWorldIfNeeded(long startupSequence, string startupSessionId)
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
        }

    }
}
