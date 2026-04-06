using System;
using System.Linq;
using System.Reflection;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Adapter: implements IMapDataGenerator by executing a GraphAsset via GraphRunner.
    /// Drop-in replacement for the old linear MapDataGenerator.
    /// </summary>
    internal sealed class GraphBasedMapDataGenerator : IMapDataGenerator
    {
        private readonly GraphAsset _graphAsset;
        private readonly IGraphRunner _graphRunner;
        private readonly INoiseProvider _noiseProvider;
        private readonly IVirtualHeightMapGenerator _virtualHeightMapGenerator;
        private readonly IBiomeResolver _biomeResolver;
        private readonly IRiverPathfinder _riverPathfinder;
        private readonly IWFCService _wfcService;

        public GraphBasedMapDataGenerator(
            GraphAsset graphAsset,
            IGraphRunner graphRunner,
            INoiseProvider noiseProvider,
            IVirtualHeightMapGenerator virtualHeightMapGenerator,
            IBiomeResolver biomeResolver,
            IRiverPathfinder riverPathfinder,
            IWFCService wfcService)
        {
            _graphAsset = graphAsset;
            _graphRunner = graphRunner;
            _noiseProvider = noiseProvider;
            _virtualHeightMapGenerator = virtualHeightMapGenerator;
            _biomeResolver = biomeResolver;
            _riverPathfinder = riverPathfinder;
            _wfcService = wfcService;
        }

        public void GenerateMapData(int width, int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int generationSeed = GetSeedFromGraph();
            var previousRandomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(generationSeed);

            try
            {
            var context = new NodeContext(generationSeed);
            context.MapSize = new Vector2Int(width, height);

            context.RegisterService(_noiseProvider);
            context.RegisterService(_virtualHeightMapGenerator);
            context.RegisterService(_biomeResolver);
            context.RegisterService(_riverPathfinder);
            context.RegisterService(_wfcService);

            var result = _graphRunner.Execute(_graphAsset, context);

            if (!result.Success)
            {
                Debug.LogError($"[GraphBasedGenerator] Execution failed: {result.ErrorMessage}");
                onComplete?.Invoke(
                    new string[width, height],
                    new string[width, height],
                    new float[width, height],
                    new string[width, height]);
                return;
            }

            // Find OutputNode
            var outputNode = _graphAsset.Nodes.FirstOrDefault(
                n => n is Nodes.OutputNode);

            if (outputNode == null)
            {
                Debug.LogError("[GraphBasedGenerator] No OutputNode found in graph.");
                onComplete?.Invoke(
                    new string[width, height],
                    new string[width, height],
                    new float[width, height],
                    new string[width, height]);
                return;
            }

            var outputs = result.GetOutputs(outputNode.NodeId);
            if (outputs == null || outputs.Length < 1)
            {
                Debug.LogError("[GraphBasedGenerator] OutputNode produced no data.");
                onComplete?.Invoke(
                    new string[width, height],
                    new string[width, height],
                    new float[width, height],
                    new string[width, height]);
                return;
            }

            var biomeMap = outputs[0] as string[,] ?? new string[width, height];
            var objectMap = outputs.Length > 1
                ? outputs[1] as string[,] ?? new string[width, height]
                : new string[width, height];
            var heightMap = outputs.Length > 2
                ? outputs[2] as float[,] ?? new float[width, height]
                : new float[width, height];
            var buildingMap = outputs.Length > 3
                ? outputs[3] as string[,] ?? new string[width, height]
                : new string[width, height];

            foreach (var log in result.Logs)
            {
                if (log.Status == NodeStatus.Error)
                    Debug.LogError($"[GraphRunner] {log.NodeTitle}: {log.Message}");
                else if (log.Status == NodeStatus.Warning)
                    Debug.LogWarning($"[GraphRunner] {log.NodeTitle}: {log.Message}");
            }

            onComplete?.Invoke(biomeMap, objectMap, heightMap, buildingMap);
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
            }
        }

        private int GetSeedFromGraph()
        {
            // Try to read seed from HeightSourceNode's private _noiseSettings field.
            var noiseSettingsField = typeof(Nodes.HeightSourceNode).GetField(
                "_noiseSettings",
                BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var node in _graphAsset.Nodes)
            {
                if (node is Nodes.HeightSourceNode)
                {
                    var settings = noiseSettingsField?.GetValue(node) as DataNoiseSettings;
                    if (settings != null)
                        return settings.Seed;
                }
            }

            return 42;
        }
    }
}
