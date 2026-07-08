using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldBuildOrchestrator : IMapVisualWorldBuildOrchestrator
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private readonly IMapVisualWorldState _state;
        private readonly IMapVisualWorldDataFactory _dataFactory;
        private readonly IMapVisualGridWriter _gridWriter;
        private readonly IMapVisualWorldSignalPublisher _signals;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly ITileWorldCreatorWorldBuildBridge _tileWorldCreatorBridge;

        public MapVisualWorldBuildOrchestrator(
            IMapVisualWorldState state,
            IMapVisualWorldDataFactory dataFactory,
            IMapVisualGridWriter gridWriter,
            IMapVisualWorldSignalPublisher signals,
            [InjectOptional] ITileWorldCreatorWorldBuildBridge tileWorldCreatorBridge = null,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _state = state;
            _dataFactory = dataFactory;
            _gridWriter = gridWriter;
            _signals = signals;
            _tileWorldCreatorBridge = tileWorldCreatorBridge;
            _worldDiagnostics = worldDiagnostics;
        }

        public void BuildWorld()
        {
            bool hasPendingWorld = _state.HasPendingWorldData;
            string source = ResolveSource(hasPendingWorld);
            _worldDiagnostics?.MapVisualBuildWorldCalled($"source={source}, frame={Time.frameCount}, hasPendingWorld={hasPendingWorld}");

            GeneratedWorldData worldData = _state.TryConsumePendingWorldData(out var pending)
                ? pending
                : _dataFactory.Generate();
            if (worldData == null)
            {
                _worldDiagnostics?.FailStartupStep(WorldGenerationDiagnosticSteps.MapVisualBuildWorld, "world-data-null");
                Debug.LogError("[MapVisualInstantiator] BuildWorld received null world data.");
                return;
            }

            _tileWorldCreatorBridge?.Build(worldData);
            int filledCells = _gridWriter.Write(worldData);
            Debug.Log($"{WorldGenDiagTag} MapVisual.GridFill.DONE map={worldData.Width}x{worldData.Height}, filledCells={filledCells}");
            _state.SetCurrentWorldData(worldData);
            _signals.Publish(worldData, source);
        }

        private static string ResolveSource(bool hasPendingWorld)
        {
            if (hasPendingWorld)
                return "pending-save";
            return GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest ? "direct-test" : "new";
        }
    }
}
