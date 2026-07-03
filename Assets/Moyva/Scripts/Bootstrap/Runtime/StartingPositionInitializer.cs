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

        public StartingPositionInitializer(
            SignalBus signalBus,
            IStartingPositionWorkflowService workflowService,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _signalBus = signalBus;
            _workflowService = workflowService;
            _worldDiagnostics = worldDiagnostics;
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
            int assignments = signal.Assignments?.Length ?? 0;
            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldSpawnPositions RECEIVED frame={UnityEngine.Time.frameCount}, assignments={assignments}, source=payload-unknown");
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.OnWorldSpawnPositions received assignments={assignments}, forwardingToWorkflow=true.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.OnWorldSpawnPositions assignments={assignments}.");
            _workflowService.HandleWorldSpawnPositions(signal);
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal signal)
        {
            _worldDiagnostics?.BootstrapWorldGeneratedReceived($"map={signal.Width}x{signal.Height}, frame={UnityEngine.Time.frameCount}");
            UnityEngine.Debug.Log($"{WorldGenDiagTag} Receiver.Bootstrap.WorldGenerated RECEIVED frame={UnityEngine.Time.frameCount}, map={signal.Width}x{signal.Height}");
            UnityEngine.Debug.Log($"{DirectDiagTag} StartingPositionInitializer.OnWorldGenerated received map={signal.Width}x{signal.Height}, forwardingToWorkflow=true.");
            UnityEngine.Debug.Log($"{PolicyDiagTag} StartingPositionInitializer.OnWorldGenerated map={signal.Width}x{signal.Height}.");
            _workflowService.HandleWorldGenerated(signal);
        }
    }
}
