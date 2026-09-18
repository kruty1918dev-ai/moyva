using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionAutoloadRecoveryService
    {
        bool RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal);
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

        public bool RepairLoadedFogIfNeeded(WorldGeneratedDataSignal signal)
        {
            bool repaired = _loadedFogRepairService.RepairLoadedFogIfNeeded(
                signal,
                _policy.CanRunStartLogic,
                _localSpawnResolver.TryGetLocalSpawnPosition,
                _startingPositionState.IsSet,
                _startingPositionState.StartPosition,
                _cameraTargetResolver.TryGetClosestUnitPosition);
            return repaired;
        }

        public Vector2Int ResolveStartupCameraTarget(int width, int height, bool preferStartTile)
        {
            Vector2Int target = _cameraTargetResolver.ResolveStartupCameraTarget(
                width,
                height,
                preferStartTile,
                _localSpawnResolver.TryGetLocalSpawnPosition,
                ResolveRepairCenter);
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
