using System;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    // Runs the production logical map pipeline without terrain mesh construction.
    // The third-party Configuration is transient execution state, not authored config.
    public sealed class OwnedGameplayMap : IDisposable
    {
        private readonly Configuration _configuration;
        public MenuWorldPreviewData Data { get; private set; }
        public OwnedGameplayMap(DiContainer container, GameObject root, GraphAsset graph,
            TileRegistrySO tiles, MapObjectRegistrySO objects, int size, int seed)
        {
            var manager = root.AddComponent<TileWorldCreatorManager>();
            _configuration = ScriptableObject.CreateInstance<Configuration>();
            manager.configuration = _configuration;
            int previousSeed = GlobalSeed.Current;
            var previousRandom = UnityEngine.Random.state;
            try
            {
                GeneratorBindingGroups.InstallGraphEvaluation(container, graph, manager, tiles, objects, root);
                var result = container.Resolve<IGraphTwcMapGenerationPipeline>().Generate(
                    new GraphTwcMapGenerationRequest(graph, manager, size, size, null, seed));
                if (result.LogicalMap == null) throw new InvalidOperationException("Production graph pipeline produced no logical world.");
                Data = new MenuWorldPreviewData(result.BiomeMap.GetLength(0), result.BiomeMap.GetLength(1), seed,
                    result.BiomeMap, result.ObjectMap, result.HeightMap, result.BuildingMap);
            }
            catch { Dispose(); throw; }
            finally { GlobalSeed.Set(previousSeed); UnityEngine.Random.state = previousRandom; }
        }
        public void Dispose()
        {
            if (_configuration == null) return;
            foreach (var folder in _configuration.blueprintLayerFolders)
                foreach (var layer in folder.blueprintLayers) DestroyOwned(layer);
            foreach (var folder in _configuration.buildLayerFolders)
                foreach (var layer in folder.buildLayers) DestroyOwned(layer);
            DestroyOwned(_configuration);
        }
        private static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(value);
            else UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
