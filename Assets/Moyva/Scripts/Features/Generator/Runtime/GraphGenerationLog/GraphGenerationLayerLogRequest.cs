using System.Collections.Generic;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct GraphGenerationLayerLogRequest
    {
        public GraphGenerationLayerLogRequest(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated,
            Object context = null)
        {
            Source = source;
            Graph = graph;
            Manager = manager;
            CompiledLayers = compiledLayers;
            ValidationReport = validationReport;
            SkippedLayerIds = skippedLayerIds;
            Seed = seed;
            MapSize = mapSize;
            BuildLayersWereGenerated = buildLayersWereGenerated;
            Context = context;
        }

        public string Source { get; }
        public GraphAsset Graph { get; }
        public TileWorldCreatorManager Manager { get; }
        public IReadOnlyList<CompiledLayerMap> CompiledLayers { get; }
        public GraphValidationReport ValidationReport { get; }
        public ISet<string> SkippedLayerIds { get; }
        public int Seed { get; }
        public Vector2Int MapSize { get; }
        public bool BuildLayersWereGenerated { get; }
        public Object Context { get; }
    }
}
