using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionSpawnSetupService
    {
        bool TryPrepareStartingPositions(WorldGeneratedDataSignal signal);
    }

    internal sealed class StartingPositionSpawnSetupService
        : IStartingPositionSpawnSetupService
    {
        private readonly IStartingPositionSelector _selector;
        private readonly IStartingPositionAssignmentFactory _assignmentFactory;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionState _startingPositionState;
        private readonly SignalBus _signalBus;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;

        public StartingPositionSpawnSetupService(
            IStartingPositionSelector selector,
            IStartingPositionAssignmentFactory assignmentFactory,
            IStartingPositionPolicy policy,
            IStartingPositionState startingPositionState,
            SignalBus signalBus,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null)
        {
            _selector = selector;
            _assignmentFactory = assignmentFactory;
            _policy = policy;
            _startingPositionState = startingPositionState;
            _signalBus = signalBus;
            _worldGenerationSignalState = worldGenerationSignalState;
        }

        public bool TryPrepareStartingPositions(WorldGeneratedDataSignal signal)
        {
            int requestedPlayerCount = _policy.ResolveStartPositionCount();
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);

            if (!_policy.ShouldComputeHostStartPositions() || _startingPositionState.IsSet)
                return false;

            List<Vector2Int> startPositions = _selector.PickStartingPositions(signal, requestedPlayerCount);
            Vector2Int startPos = startPositions.Count > 0
                ? startPositions[0]
                : _selector.PickStartingPosition(baseMapSize);
            startPos = StartingPositionMapUtility.ClampToMap(startPos, baseMapSize.x, baseMapSize.y);

            if (startPositions.Count > 0)
            {
                SpawnPositionAssignment[] assignments = _assignmentFactory.BuildSpawnAssignments(
                    startPositions,
                    _policy.Participants,
                    _policy.ResolveLocalPlayerId(),
                    GameLaunchContext.HasWorldSettings,
                    GameLaunchContext.MaxPlayers);
                _startingPositionState.Set(assignments);
            }
            else
            {
                _startingPositionState.Set(startPos);
            }

            if (_startingPositionState.SpawnAssignments.Count > 0)
            {
                var spawnPositionsSignal = new WorldSpawnPositionsSignal
                {
                    StartupSequence = signal.StartupSequence,
                    StartupSessionId = signal.StartupSessionId,
                    Source = signal.Source == WorldGeneratedDataSource.DirectGameplayTest
                        ? WorldSpawnPositionsSource.DirectGameplayTest
                        : WorldSpawnPositionsSource.GeneratedHost,
                    PublishedFrame = Time.frameCount,
                    Assignments = _assignmentFactory.CopySpawnAssignments(_startingPositionState.SpawnAssignments),
                };
                if (_worldGenerationSignalState == null || _worldGenerationSignalState.TryStoreWorldSpawnPositions(spawnPositionsSignal, out spawnPositionsSignal))
                    _signalBus.Fire(spawnPositionsSignal);
            }

            return true;
        }
    }
}
