using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualInstantiator : IMapInstantiator, IInitializable, System.IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IMapVisualWorldState _state;
        private readonly IMapVisualWorldBuildOrchestrator _orchestrator;

        internal bool HasPendingWorldData => _state.HasPendingWorldData;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IMapObjectRegistryService objectRegistry,
            IMapObjectVisualRegistryService mapObjectVisualRegistryService,
            [InjectOptional] IUnitClassConfig unitClassConfig,
            [InjectOptional] IUnitFactory unitFactory,
            [InjectOptional] IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus,
            [InjectOptional] IGraphTwcMapDataDiagnostics graphTwcDiagnostics,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IWaterLayerMaterialSettings waterLayerMaterialSettings = null,
            [InjectOptional] ITileWorldCreatorWorldBuildBridge tileWorldCreatorBridge = null,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null,
            [InjectOptional] IMapVisualWorldState state = null,
            [InjectOptional] IMapVisualWorldBuildOrchestrator orchestrator = null)
        {
            _signalBus = signalBus;
            _state = state ?? new MapVisualWorldState();
            _orchestrator = orchestrator ?? MapVisualOrchestratorFactory.Create(
                tileRegistry,
                gridService,
                mapDataGenerator,
                signalBus,
                _state,
                gridProjection,
                graphTwcDiagnostics,
                worldGenerationSignalState,
                tileWorldCreatorBridge);
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
        }

        public void BuildWorld()
        {
            _orchestrator.BuildWorld();
        }

        internal void SetPendingWorldData(GeneratedWorldData data)
        {
            _state.SetPendingWorldData(data);
        }

        internal bool TryGetCurrentWorldData(out GeneratedWorldData data)
        {
            return _state.TryGetCurrentWorldData(out data);
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            _state.ApplySpawnPositions(signal.Assignments);
        }
    }
}
