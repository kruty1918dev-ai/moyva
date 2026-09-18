using System;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// На старті нової гри обирає випадкову точку на мапі,
    /// рівномірно розкриває круг туману навколо неї (імітація стартової позиції)
    /// та миттєво переміщує камеру туди.
    ///
    /// При завантаженні збереження перевіряє, що туман має валідну видиму ділянку.
    /// </summary>
    internal sealed class StartingPositionInitializer : IInitializable, IDisposable
    {
        internal const string StartVisionAnchorId = "bootstrap-start-vision-anchor";
        internal const string StartRevealAnchorId = "bootstrap-start-vision-anchor-initial";
        internal const string DebugTag = "[MoyvaFogTrace]";
        private readonly SignalBus _signalBus;
        private readonly IStartingPositionWorkflowService _workflowService;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;
        private int _lastHandledWorldRevision;
        private int _lastHandledSpawnRevision;

        public StartingPositionInitializer(
            SignalBus signalBus,
            IStartingPositionWorkflowService workflowService,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null)
        {
            _signalBus = signalBus;
            _workflowService = workflowService;
            _worldGenerationSignalState = worldGenerationSignalState;
        }

        public void Initialize()
        {
            System.Diagnostics.Debug.Assert(_signalBus != null);
            System.Diagnostics.Debug.Assert(_workflowService != null);
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            Debug.Log($"{DebugTag} Initializer subscribed. cachedState={_worldGenerationSignalState != null}.");
            ReplayCachedWorldSignalsIfAvailable();
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (ShouldSkipSpawnSignal(signal))
            {
                Debug.Log($"{DebugTag} WorldSpawnPositions skipped as duplicate revision={signal.SnapshotRevision}.");
                return;
            }

            _workflowService.HandleWorldSpawnPositions(signal);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            if (ShouldSkipWorldSignal(signal))
            {
                Debug.Log($"{DebugTag} WorldGenerated skipped as duplicate revision={signal.SnapshotRevision}.");
                return;
            }

            _workflowService.HandleWorldGenerated(signal);
        }

        private void ReplayCachedWorldSignalsIfAvailable()
        {
            if (_worldGenerationSignalState == null)
                return;

            if (_worldGenerationSignalState.TryGetWorldGeneratedData(out var worldSignal))
            {
                Debug.Log($"{DebugTag} Replaying cached WorldGenerated revision={worldSignal.SnapshotRevision}.");
                OnWorldGenerated(worldSignal);
            }

            if (_worldGenerationSignalState.TryGetWorldSpawnPositions(out var spawnSignal))
            {
                Debug.Log($"{DebugTag} Replaying cached WorldSpawnPositions revision={spawnSignal.SnapshotRevision}, assignments={spawnSignal.Assignments?.Length ?? 0}.");
                OnWorldSpawnPositions(spawnSignal);
            }
        }

        private bool ShouldSkipWorldSignal(WorldGeneratedDataSignal signal)
        {
            if (signal.SnapshotRevision <= 0 || signal.SnapshotRevision != _lastHandledWorldRevision)
            {
                _lastHandledWorldRevision = signal.SnapshotRevision;
                return false;
            }
            return true;
        }

        private bool ShouldSkipSpawnSignal(WorldSpawnPositionsSignal signal)
        {
            if (signal.SnapshotRevision <= 0 || signal.SnapshotRevision != _lastHandledSpawnRevision)
            {
                _lastHandledSpawnRevision = signal.SnapshotRevision;
                return false;
            }
            return true;
        }
    }
}
