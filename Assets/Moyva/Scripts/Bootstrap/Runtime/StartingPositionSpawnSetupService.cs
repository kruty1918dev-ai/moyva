using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        : IStartingPositionSpawnSetupService, IDisposable
    {
        private readonly IStartingPositionSelector _selector;
        private readonly IStartingPositionAssignmentFactory _assignmentFactory;
        private readonly IStartingPositionPolicy _policy;
        private readonly IStartingPositionState _startingPositionState;
        private readonly SignalBus _signalBus;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;
        private CancellationTokenSource _preparation;
        private WorldGeneratedDataSignal _preparingWorld;
        private bool _disposed;

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
            if (_disposed)
                return false;
            int requestedPlayerCount = _policy.ResolveStartPositionCount();
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            bool shouldCompute = _policy.ShouldComputeHostStartPositions();

            if (!shouldCompute || _startingPositionState.IsSet)
            {
                Debug.Log($"{StartingPositionInitializer.DebugTag} Host start position preparation skipped. " +
                          $"shouldCompute={shouldCompute}, stateIsSet={_startingPositionState.IsSet}, " +
                          $"requestedPlayers={requestedPlayerCount}, map={baseMapSize.x}x{baseMapSize.y}.");
                return false;
            }

            if (_preparation != null)
            {
                if (_preparingWorld.StartupSequence == signal.StartupSequence &&
                    _preparingWorld.StartupSessionId == signal.StartupSessionId)
                    return false;

                _preparation.Cancel();
            }

            _preparingWorld = signal;
            var preparation = new CancellationTokenSource();
            _preparation = preparation;
            _ = PrepareAsync(signal, requestedPlayerCount, baseMapSize, preparation);
            return false;
        }

        private async Task PrepareAsync(WorldGeneratedDataSignal signal, int requestedPlayerCount,
            Vector2Int baseMapSize, CancellationTokenSource preparation)
        {
            try
            {
                var elapsed = System.Diagnostics.Stopwatch.StartNew();
                List<Vector2Int> startPositions = await _selector.PickStartingPositionsAsync(
                    signal, requestedPlayerCount, preparation.Token);
                preparation.Token.ThrowIfCancellationRequested();
                if (!_policy.ShouldComputeHostStartPositions() || _startingPositionState.IsSet)
                    return;

                PublishStartingPositions(signal, requestedPlayerCount, baseMapSize, startPositions);
                Debug.Log($"{StartingPositionInitializer.DebugTag} Host start positions prepared in {elapsed.ElapsedMilliseconds} ms.");
            }
            catch (OperationCanceledException) when (preparation.IsCancellationRequested) { }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                if (ReferenceEquals(_preparation, preparation))
                    _preparation = null;
                preparation.Dispose();
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _preparation?.Cancel();
        }

        private void PublishStartingPositions(WorldGeneratedDataSignal signal, int requestedPlayerCount,
            Vector2Int baseMapSize, List<Vector2Int> startPositions)
        {
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
                Debug.Log($"{StartingPositionInitializer.DebugTag} Publishing start positions. " +
                          $"assignments={_startingPositionState.SpawnAssignments.Count}, requestedPlayers={requestedPlayerCount}, " +
                          $"localPlayer='{_policy.ResolveLocalPlayerId()}', map={baseMapSize.x}x{baseMapSize.y}.");
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

        }
    }
}
