using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Окремий menu-only utility для виконання GraphAsset без запуску повної побудови світу.
    /// Повертає лише фінальні карти, придатні для рендера у Texture2D.
    /// </summary>
    public static class MenuWorldPreviewGenerator
    {
        public static bool TryGenerate(
            GraphAsset graphAsset,
            int width,
            int height,
            int seed,
            out MenuWorldPreviewData previewData,
            out string errorMessage)
        {
            previewData = null;
            errorMessage = null;

            if (graphAsset == null)
            {
                errorMessage = "GraphAsset is not assigned.";
                return false;
            }

            Vector2Int mapSize = ResolveMapSize(graphAsset, width, height);
            int previousSeed = GlobalSeed.Current;
            var previousRandomState = UnityEngine.Random.state;

            try
            {
                GlobalSeed.Set(seed);
                UnityEngine.Random.InitState(seed);

                var context = new NodeContext(seed)
                {
                    MapSize = mapSize
                };

                var layerDataList = new List<WorldLayerData>();
                context.RegisterService(layerDataList);

                RegisterServices(context, graphAsset);

                var result = new GraphRunner().Execute(graphAsset, context);
                if (!result.Success)
                {
                    errorMessage = string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? "Graph execution failed."
                        : result.ErrorMessage;
                    return false;
                }

                var outputNode = graphAsset.Nodes
                    .OfType<OutputNode>()
                    .FirstOrDefault();

                if (outputNode == null)
                {
                    errorMessage = "OutputNode was not found in the graph.";
                    return false;
                }

                object[] outputs = result.GetOutputs(outputNode.NodeId);
                if (outputs == null || outputs.Length == 0)
                {
                    errorMessage = "Graph did not produce any output maps.";
                    return false;
                }

                var biomeMap = NormalizeStringMap(outputs.Length > 0 ? outputs[0] as string[,] : null, mapSize.x, mapSize.y);
                var objectMap = NormalizeStringMap(outputs.Length > 1 ? outputs[1] as string[,] : null, mapSize.x, mapSize.y);
                var heightMap = NormalizeFloatMap(outputs.Length > 2 ? outputs[2] as float[,] : null, mapSize.x, mapSize.y);
                var buildingMap = NormalizeStringMap(outputs.Length > 3 ? outputs[3] as string[,] : null, mapSize.x, mapSize.y);

                if (layerDataList.Count > 0)
                {
                    var layerBiome = BuildBiomeMapFromLayers(layerDataList, mapSize.x, mapSize.y);
                    MergeEmptyBiomeCells(biomeMap, layerBiome, mapSize.x, mapSize.y);
                }

                previewData = new MenuWorldPreviewData(
                    mapSize.x,
                    mapSize.y,
                    seed,
                    biomeMap,
                    objectMap,
                    heightMap,
                    buildingMap);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                GlobalSeed.Set(previousSeed);
                UnityEngine.Random.state = previousRandomState;
            }
        }

        private static void RegisterServices(NodeContext context, GraphAsset graphAsset)
        {
            context.RegisterService<INoiseProvider>(new NoiseMapGeneratorService());
            context.RegisterService<IRiverPathfinder>(new RiverPathfinder());
            context.RegisterService<IGeneratorDataRegistry>(new GeneratorDataRegistry());
            context.RegisterService<IGeneratorTerrainLevelService>(new GeneratorTerrainLevelService());

            if (graphAsset.SharedSettings != null)
            {
                context.ApplySharedSettings(graphAsset.SharedSettings);
                context.RegisterService(graphAsset.SharedSettings);
            }

            if (graphAsset.TileRegistry != null)
                context.RegisterService(graphAsset.TileRegistry);
        }

        private static Vector2Int ResolveMapSize(GraphAsset graphAsset, int width, int height)
        {
            if (width > 0 && height > 0)
                return new Vector2Int(width, height);

            if (graphAsset?.SharedSettings != null && graphAsset.SharedSettings.HasMapSize)
                return graphAsset.SharedSettings.MapSize;

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }

        private static string[,] NormalizeStringMap(string[,] source, int width, int height)
        {
            var result = new string[width, height];
            if (source == null)
                return result;

            int copyWidth = Mathf.Min(width, source.GetLength(0));
            int copyHeight = Mathf.Min(height, source.GetLength(1));
            for (int x = 0; x < copyWidth; x++)
            for (int y = 0; y < copyHeight; y++)
                result[x, y] = source[x, y];

            return result;
        }

        private static float[,] NormalizeFloatMap(float[,] source, int width, int height)
        {
            var result = new float[width, height];
            if (source == null)
                return result;

            int copyWidth = Mathf.Min(width, source.GetLength(0));
            int copyHeight = Mathf.Min(height, source.GetLength(1));
            for (int x = 0; x < copyWidth; x++)
            for (int y = 0; y < copyHeight; y++)
                result[x, y] = source[x, y];

            return result;
        }

        private static string[,] BuildBiomeMapFromLayers(List<WorldLayerData> layers, int mapWidth, int mapHeight)
        {
            var biomeMap = new string[mapWidth, mapHeight];
            if (layers == null || layers.Count == 0)
                return biomeMap;

            var sorted = new List<WorldLayerData>(layers);
            sorted.Sort((a, b) => b.SortingOrder.CompareTo(a.SortingOrder));

            var pixelCache = new Color[sorted.Count][];
            var texWidths = new int[sorted.Count];
            var texHeights = new int[sorted.Count];

            for (int layerIndex = 0; layerIndex < sorted.Count; layerIndex++)
            {
                var texture = sorted[layerIndex].TileTexture;
                if (texture == null)
                    continue;

                texWidths[layerIndex] = texture.width;
                texHeights[layerIndex] = texture.height;
                pixelCache[layerIndex] = texture.GetPixels();
            }

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int layerIndex = 0; layerIndex < sorted.Count; layerIndex++)
                    {
                        var pixels = pixelCache[layerIndex];
                        if (pixels == null)
                            continue;

                        int textureWidth = texWidths[layerIndex];
                        int textureHeight = texHeights[layerIndex];
                        int textureX = Mathf.Clamp((x * textureWidth) / mapWidth, 0, textureWidth - 1);
                        int textureY = Mathf.Clamp((y * textureHeight) / mapHeight, 0, textureHeight - 1);

                        if (pixels[textureY * textureWidth + textureX].a <= 0f)
                            continue;

                        biomeMap[x, y] = sorted[layerIndex].LayerTileID;
                        break;
                    }
                }
            }

            return biomeMap;
        }

        private static void MergeEmptyBiomeCells(string[,] target, string[,] source, int mapWidth, int mapHeight)
        {
            if (target == null || source == null)
                return;

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (string.IsNullOrEmpty(target[x, y]) && !string.IsNullOrEmpty(source[x, y]))
                        target[x, y] = source[x, y];
                }
            }
        }
    }
}