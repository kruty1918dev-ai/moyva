using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldBuildOrchestrator : IMapVisualWorldBuildOrchestrator
    {
        private readonly IMapVisualWorldState _state;
        private readonly IMapVisualWorldDataFactory _dataFactory;
        private readonly IMapVisualGridWriter _gridWriter;
        private readonly IMapVisualWorldSignalPublisher _signals;
        private readonly ITileWorldCreatorWorldBuildBridge _tileWorldCreatorBridge;
        private readonly MapVisualFallbackPresenter _fallbackPresenter;

        public MapVisualWorldBuildOrchestrator(
            IMapVisualWorldState state,
            IMapVisualWorldDataFactory dataFactory,
            IMapVisualGridWriter gridWriter,
            IMapVisualWorldSignalPublisher signals,
            [InjectOptional] ITileWorldCreatorWorldBuildBridge tileWorldCreatorBridge = null,
            [InjectOptional] MapVisualFallbackPresenter fallbackPresenter = null)
        {
            _state = state;
            _dataFactory = dataFactory;
            _gridWriter = gridWriter;
            _signals = signals;
            _tileWorldCreatorBridge = tileWorldCreatorBridge;
            _fallbackPresenter = fallbackPresenter;
        }

        public void BuildWorld()
        {
            bool hasPendingWorld = _state.HasPendingWorldData;
            string source = ResolveSource(hasPendingWorld);

            GeneratedWorldData worldData = _state.TryConsumePendingWorldData(out var pending)
                ? pending
                : _dataFactory.Generate();
            if (worldData == null)
            {
                Debug.LogError("[MapVisualInstantiator] BuildWorld received null world data.");
                return;
            }

            TileWorldCreatorWorldBuildResult visualBuildResult =
                _tileWorldCreatorBridge?.Build(worldData)
                ?? TileWorldCreatorWorldBuildResult.Disabled;
            if (visualBuildResult.Succeeded)
            {
                _fallbackPresenter?.Clear();
                ApplyVisualBounds(worldData, visualBuildResult);
            }
            else
            {
                TileWorldCreatorWorldBuildResult fallbackResult =
                    _fallbackPresenter?.Present(worldData)
                    ?? TileWorldCreatorWorldBuildResult.Disabled;
                if (fallbackResult.Succeeded)
                {
                    ApplyVisualBounds(worldData, fallbackResult);
                }
                else
                {
                    Debug.LogError(
                        "[MapVisualInstantiator] Generated world data is available, " +
                        "but neither TileWorldCreator nor the fallback terrain presenter produced a visible map.");
                }
            }

            _gridWriter.Write(worldData);
            _state.SetCurrentWorldData(worldData);
            _signals.Publish(worldData, source);
        }

        private static void ApplyVisualBounds(
            GeneratedWorldData worldData,
            TileWorldCreatorWorldBuildResult result)
        {
            if (worldData == null || !result.HasBaseMapWorldBounds)
                return;

            worldData.HasBaseMapWorldBounds = true;
            worldData.BaseMapWorldBounds = result.BaseMapWorldBounds;
            worldData.CellSize = result.CellSize;
        }

        private static string ResolveSource(bool hasPendingWorld)
        {
            if (hasPendingWorld)
                return "pending-save";
            return GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest ? "direct-test" : "new";
        }
    }
}
