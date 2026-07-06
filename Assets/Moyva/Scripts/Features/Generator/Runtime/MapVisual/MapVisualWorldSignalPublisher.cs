using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldSignalPublisher : IMapVisualWorldSignalPublisher
    {
        private readonly SignalBus _signalBus;
        private readonly IGridProjection _projection;
        private readonly IGraphTwcMapDataDiagnostics _graphDiagnostics;
        private readonly IWorldGenerationSignalState _signalState;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly ISaveLoadDiagnostics _saveLoadDiagnostics;
        private readonly ISaveLoadDiagnosticsSession _saveLoadDiagnosticsSession;

        public MapVisualWorldSignalPublisher(
            SignalBus signalBus,
            IGridProjection projection,
            [InjectOptional] IGraphTwcMapDataDiagnostics graphDiagnostics = null,
            [InjectOptional] IWorldGenerationSignalState signalState = null,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnostics saveLoadDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnosticsSession saveLoadDiagnosticsSession = null)
        {
            _signalBus = signalBus;
            _projection = projection;
            _graphDiagnostics = graphDiagnostics;
            _signalState = signalState;
            _worldDiagnostics = worldDiagnostics;
            _saveLoadDiagnostics = saveLoadDiagnostics;
            _saveLoadDiagnosticsSession = saveLoadDiagnosticsSession;
        }

        public void Publish(GeneratedWorldData worldData, string source)
        {
            long sequence = BeginCycle(worldData, source, out string sessionId);
            _signalBus.Fire(new WorldBuiltSignal());
            PublishSavedSpawns(worldData, sequence, sessionId);
            var signal = CreateWorldGeneratedSignal(worldData, source, sequence, sessionId);
            signal = _signalState != null ? _signalState.StoreWorldGeneratedData(signal) : signal;
            _worldDiagnostics?.WorldGeneratedSignalFired($"map={worldData.Width}x{worldData.Height}, frame={Time.frameCount}");
            _saveLoadDiagnostics?.CompleteStep(_saveLoadDiagnosticsSession?.CurrentFlow,
                SaveLoadDiagnosticSteps.WorldGeneratedDataSignalFired,
                $"source={source}, map={worldData.Width}x{worldData.Height}");
            _signalBus.Fire(signal);
        }

        public void PublishSavedSpawns(GeneratedWorldData worldData, long startupSequence, string startupSessionId)
        {
            if (worldData?.SpawnPositions == null || worldData.SpawnPositions.Length == 0)
                return;

            var signal = new WorldSpawnPositionsSignal
            {
                StartupSequence = startupSequence,
                StartupSessionId = startupSessionId,
                Source = WorldSpawnPositionsSource.SavedGame,
                PublishedFrame = Time.frameCount,
                Assignments = (SpawnPositionAssignment[])worldData.SpawnPositions.Clone(),
            };
            if (_signalState == null || _signalState.TryStoreWorldSpawnPositions(signal, out signal))
                _signalBus.Fire(signal);
        }

        private WorldGeneratedDataSignal CreateWorldGeneratedSignal(GeneratedWorldData worldData,
            string source, long sequence, string sessionId)
        {
            bool hasBounds = TryResolveBounds(worldData, out Bounds bounds);
            return new WorldGeneratedDataSignal
            {
                StartupSequence = sequence,
                StartupSessionId = sessionId,
                Source = ResolveSource(source),
                PublishedFrame = Time.frameCount,
                Width = worldData.Width,
                Height = worldData.Height,
                GridTopology = (int)worldData.GridTopology,
                ProjectionMode = (int)worldData.ProjectionMode,
                RenderMode = (int)worldData.RenderMode,
                NeighborhoodMode = (int)worldData.NeighborhoodMode,
                CellSize = ResolveCellSize(),
                HasMapWorldBounds = hasBounds,
                MapWorldBoundsCenter = bounds.center,
                MapWorldBoundsSize = bounds.size,
                TileMap = MapArrayUtils.CloneStringMap(worldData.BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(worldData.ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(worldData.HeightMap),
                TerrainLevelMap = MapArrayUtils.CloneIntMap(worldData.TerrainLevelMap),
            };
        }

        private long BeginCycle(GeneratedWorldData worldData, string source, out string sessionId)
        {
            sessionId = $"{GameLaunchContext.Mode}:{source}:{GameLaunchContext.SaveSlot}:{GameLaunchContext.WorldName}:{GameLaunchContext.Seed}:{worldData.Width}x{worldData.Height}:{GameLaunchContext.MaxPlayers}";
            return _signalState?.BeginWorldSnapshotCycle(sessionId) ?? 0L;
        }

        private bool TryResolveBounds(GeneratedWorldData worldData, out Bounds bounds)
        {
            if (_graphDiagnostics != null && _graphDiagnostics.TryGetLastBaseMapWorldBounds(out bounds))
                return true;
            bounds = _projection.GetWorldBounds(worldData.Width, worldData.Height);
            return true;
        }

        private float ResolveCellSize() => _graphDiagnostics?.LastCellSize > 0.0001f ? _graphDiagnostics.LastCellSize : 1f;

        private static WorldGeneratedDataSource ResolveSource(string source) => source switch
        {
            "pending-save" => WorldGeneratedDataSource.LoadedSave,
            "direct-test" => WorldGeneratedDataSource.DirectGameplayTest,
            _ => WorldGeneratedDataSource.GeneratedHost,
        };
    }
}
