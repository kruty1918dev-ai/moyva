using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionAutoloadRecoveryService
    {
        void RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal);
        Vector2Int ResolveStartupCameraTarget(int width, int height, bool preferStartTile);
    }

    internal sealed class StartingPositionAutoloadRecoveryService
        : IStartingPositionAutoloadRecoveryService
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private readonly IStartingPositionLoadedFogRepairService _loadedFogRepairService;
        private readonly IStartingPositionCameraTargetResolver _cameraTargetResolver;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionLocalSpawnResolver _localSpawnResolver;
        private readonly IStartingPositionState _startingPositionState;

        public StartingPositionAutoloadRecoveryService(
            IStartingPositionLoadedFogRepairService loadedFogRepairService,
            IStartingPositionCameraTargetResolver cameraTargetResolver,
            IStartingPositionPolicy policy,
            IStartingPositionLocalSpawnResolver localSpawnResolver,
            IStartingPositionState startingPositionState)
        {
            _loadedFogRepairService = loadedFogRepairService;
            _cameraTargetResolver = cameraTargetResolver;
            _policy = policy;
            _localSpawnResolver = localSpawnResolver;
            _startingPositionState = startingPositionState;
        }

        public void RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal)
        {
            Debug.Log($"{DirectDiagTag} Workflow.RepairLoadedFogIfNeeded ENTER map={signal.Width}x{signal.Height}, canRunPolicyAvailable={_policy != null}, localSpawnResolver={_localSpawnResolver != null}, stateSet={_startingPositionState.IsSet}.");
            _loadedFogRepairService.RepairLoadedFogIfNeeded(
                signal,
                _policy.CanRunStartLogic,
                _localSpawnResolver.TryGetLocalSpawnPosition,
                _startingPositionState.IsSet,
                _startingPositionState.StartPosition,
                _cameraTargetResolver.TryGetClosestUnitPosition);
            Debug.Log($"{DirectDiagTag} Workflow.RepairLoadedFogIfNeeded EXIT.");
        }

        public Vector2Int ResolveStartupCameraTarget(int width, int height, bool preferStartTile)
        {
            Debug.Log($"{DirectDiagTag} Workflow.ResolveStartupCameraTarget ENTER width={width}, height={height}, preferStartTile={preferStartTile}, stateSet={_startingPositionState.IsSet}.");
            Vector2Int target = _cameraTargetResolver.ResolveStartupCameraTarget(
                width,
                height,
                preferStartTile,
                _localSpawnResolver.TryGetLocalSpawnPosition,
                ResolveRepairCenter);
            Debug.Log($"{DirectDiagTag} Workflow.ResolveStartupCameraTarget RESULT target={target}.");
            return target;
        }

        private Vector2Int ResolveRepairCenter(bool[,] snapshot, int width, int height)
        {
            return _loadedFogRepairService.ResolveRepairCenter(
                snapshot,
                width,
                height,
                _localSpawnResolver.TryGetLocalSpawnPosition,
                _startingPositionState.IsSet,
                _startingPositionState.StartPosition,
                _cameraTargetResolver.TryGetClosestUnitPosition);
        }
    }
}
