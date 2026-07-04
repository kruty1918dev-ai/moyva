using System;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Signals;
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
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly SignalBus _signalBus;
        private readonly IStartingPositionWorkflowService _workflowService;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;
        private int _lastHandledWorldRevision;
        private int _lastHandledSpawnRevision;

        public StartingPositionInitializer(
            SignalBus signalBus,
            IStartingPositionWorkflowService workflowService,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null)
        {
            _signalBus = signalBus;
            _workflowService = workflowService;
            _worldDiagnostics = worldDiagnostics;
            _worldGenerationSignalState = worldGenerationSignalState;
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.Construct workflow={workflowService != null}, signalBus={signalBus != null}.");
        }

        public void Initialize()
        {
            System.Diagnostics.Debug.Assert(_signalBus != null);
            System.Diagnostics.Debug.Assert(_workflowService != null);

            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.Initialize subscribing signals.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.Initialize subscribe-start.");
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _worldDiagnostics?.BootstrapSubscribedWorldSignals($"frame={UnityEngine.Time.frameCount}");
            ReplayCachedWorldSignalsIfAvailable();
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.Initialize subscribe-complete.");
            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.Initialize subscribed frame={UnityEngine.Time.frameCount}");
        }

        public void Dispose()
        {
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.Dispose unsubscribing signals.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.Dispose unsubscribe-start.");
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.Dispose unsubscribe-complete.");
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (ShouldSkipSpawnSignal(signal))
                return;

            int assignments = signal.Assignments?.Length ?? 0;
            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldSpawnPositions RECEIVED frame={UnityEngine.Time.frameCount}, assignments={assignments}, source=payload-unknown");
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.OnWorldSpawnPositions received assignments={assignments}, forwardingToWorkflow=true.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.OnWorldSpawnPositions assignments={assignments}.");
            _workflowService.HandleWorldSpawnPositions(signal);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            if (ShouldSkipWorldSignal(signal))
                return;

            _worldDiagnostics?.BootstrapWorldGeneratedReceived($"map={signal.Width}x{signal.Height}, frame={UnityEngine.Time.frameCount}");
            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldGenerated RECEIVED frame={UnityEngine.Time.frameCount}, map={signal.Width}x{signal.Height}");
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.OnWorldGenerated received map={signal.Width}x{signal.Height}, forwardingToWorkflow=true.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.OnWorldGenerated map={signal.Width}x{signal.Height}.");
            _workflowService.HandleWorldGenerated(signal);
        }

        private void ReplayCachedWorldSignalsIfAvailable()
        {
            if (_worldGenerationSignalState == null)
                return;

            if (_worldGenerationSignalState.TryGetWorldGeneratedData(out var worldSignal))
            {
                UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldGenerated REPLAY frame={UnityEngine.Time.frameCount}, map={worldSignal.Width}x{worldSignal.Height}");
                OnWorldGenerated(worldSignal);
            }

            if (_worldGenerationSignalState.TryGetWorldSpawnPositions(out var spawnSignal))
            {
                UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldSpawnPositions REPLAY frame={UnityEngine.Time.frameCount}, assignments={spawnSignal.Assignments?.Length ?? 0}");
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

            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldGenerated SKIP duplicate revision={signal.SnapshotRevision}, sequence={signal.StartupSequence}, source={signal.Source}.");
            return true;
        }

        private bool ShouldSkipSpawnSignal(WorldSpawnPositionsSignal signal)
        {
            if (signal.SnapshotRevision <= 0 || signal.SnapshotRevision != _lastHandledSpawnRevision)
            {
                _lastHandledSpawnRevision = signal.SnapshotRevision;
                return false;
            }

            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldSpawnPositions SKIP duplicate revision={signal.SnapshotRevision}, sequence={signal.StartupSequence}, source={signal.Source}.");
            return true;
        }
    }
}
