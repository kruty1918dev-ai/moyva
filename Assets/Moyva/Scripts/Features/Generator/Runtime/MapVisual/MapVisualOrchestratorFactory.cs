using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MapVisualOrchestratorFactory
    {
        public static IMapVisualWorldBuildOrchestrator Create(
            TileRegistrySO tileRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            SignalBus signalBus,
            IMapVisualWorldState state,
            IGridProjection projection,
            IGraphTwcMapDataDiagnostics graphDiagnostics,
            IWorldGenerationDiagnostics worldDiagnostics,
            ISaveLoadDiagnostics saveLoadDiagnostics,
            ISaveLoadDiagnosticsSession saveLoadDiagnosticsSession,
            IWorldGenerationSignalState signalState)
        {
            projection ??= new OrthogonalGridProjection();
            var tileResolver = new MapVisualTileIdResolver(tileRegistry);
            return new MapVisualWorldBuildOrchestrator(
                state,
                new MapVisualWorldDataFactory(gridService, projection, mapDataGenerator, graphDiagnostics, worldDiagnostics),
                new MapVisualGridWriter(gridService, tileResolver),
                new MapVisualWorldSignalPublisher(signalBus, projection, graphDiagnostics, signalState, worldDiagnostics,
                    saveLoadDiagnostics, saveLoadDiagnosticsSession),
                worldDiagnostics);
        }
    }
}
