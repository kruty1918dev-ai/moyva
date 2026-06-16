using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Новий runtime-генератор: граф є джерелом інструкцій, а саму генерацію
    /// виконує TileWorldCreator. Граф компілюється у TWC <see cref="Configuration"/>
    /// (кожен шар графа -> blueprint-шар), після чого TWC будує мапу
    /// (<see cref="TileWorldCreatorManager.GenerateCompleteMap"/>).
    ///
    /// Ідентичність клітинки = gameplay tile id, який береться з TilePreset
    /// відповідного build-шару (fallback: id шару графа).
    /// Цей id потрапляє у biomeMap і далі в GridService.
    /// </summary>
    internal sealed class GraphTwcMapDataGenerator : IMapDataGenerator
    {
        private readonly GraphAsset _graphAsset;
        private readonly TileWorldCreatorManager _manager;
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        /// <summary>
        /// Зіставлення graphLayerId &lt;-&gt; TWC blueprint-шару, оновлюється
        /// після кожної генерації. Дозволяє downstream-коду знати, які шари існують.
        /// </summary>
        internal IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }
        internal float LastCellSize { get; private set; } = 1f;

        public GraphTwcMapDataGenerator(
            GraphAsset graphAsset,
            TileWorldCreatorManager manager,
            IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _graphAsset = graphAsset;
            _manager = manager;
            _terrainLevelService = terrainLevelService;
        }

        public void GenerateMapData(int width, int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int seed = GetSeedFromGraph();
            GlobalSeed.Set(seed);
            _terrainLevelService?.Clear();

            // Розмір з SharedSettings має пріоритет, щоб play-mode збігався з превʼю.
            var shared = _graphAsset?.SharedSettings;
            if (!GameLaunchContext.HasWorldSettings && shared != null && shared.HasMapSize)
            {
                width = shared.MapWidth;
                height = shared.MapHeight;
            }

            if (_manager == null || _manager.configuration == null)
            {
                Debug.LogError("[GraphTwcGenerator] TileWorldCreatorManager/Configuration відсутній. " +
                               "Перевірте MoyvaTileWorldCreatorGraphBinding на TileWorldCreatorManager.");
                EmitEmpty(width, height, onComplete);
                return;
            }

            try
            {
                // 1. Граф -> TWC Configuration.
                var compiled = GraphToConfigurationCompiler.Compile(_graphAsset, _manager, seed);
                LastCompiledLayers = compiled;

                // Узгоджуємо фактичний розмір з конфігурацією.
                width = _manager.configuration.width;
                height = _manager.configuration.height;
                LastCellSize = _manager.configuration.cellSize > 0.0001f
                    ? _manager.configuration.cellSize
                    : 1f;

                // 2. TWC будує мапу (blueprint-стек + 3D build-стек).
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(_manager);

                // 3. Експортуємо layer-id сітку та висоти.
                var biomeMap = new string[width, height];
                var heightMap = new float[width, height];
                var surfaceHeightMap = new float[width, height];
                BuildLayerIdGrid(compiled, width, height, biomeMap, heightMap, surfaceHeightMap);
                PublishTerrainHeights(surfaceHeightMap);

                // Об'єкти/будівлі у новому конвеєрі генерує не граф, а ігролад/TWC build-шари.
                var objectMap = new string[width, height];
                var buildingMap = new string[width, height];

                onComplete?.Invoke(biomeMap, objectMap, heightMap, buildingMap);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphTwcGenerator] Помилка генерації: {ex}");
                EmitEmpty(width, height, onComplete);
            }
        }

        /// <summary>
        /// Для кожної клітинки записує gameplay tile id з верхнього (найбільший
        /// SortingOrder) blueprint-шару, що містить цю позицію.
        /// Висота береться з defaultLayerHeight цього ж шару.
        /// </summary>
        private void BuildLayerIdGrid(
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height,
            string[,] biomeMap,
            float[,] heightMap,
            float[,] surfaceHeightMap)
        {
            if (compiled == null)
                return;

            // Шари у порядку зростання SortingOrder: пізніший перекриває раніший.
            var ordered = new List<CompiledLayerMap>(compiled);
            ordered.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));

            foreach (var layerMap in ordered)
            {
                if (layerMap == null || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                    continue;

                var blueprint = _manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
                if (blueprint == null || blueprint.allPositions == null)
                    continue;

                float layerHeight = blueprint.defaultLayerHeight;
                float surfaceHeight = ResolveTwcLayerSurfaceHeight(blueprint, layerMap.BlueprintLayerGuid);

                foreach (var position in blueprint.allPositions)
                {
                    int x = Mathf.RoundToInt(position.x);
                    int y = Mathf.RoundToInt(position.y);
                    if (x < 0 || x >= width || y < 0 || y >= height)
                        continue;

                    biomeMap[x, y] = !string.IsNullOrWhiteSpace(layerMap.GridTileId)
                        ? layerMap.GridTileId
                        : layerMap.GraphLayerId;
                    heightMap[x, y] = layerHeight;
                    surfaceHeightMap[x, y] = surfaceHeight;
                }
            }
        }

        private void PublishTerrainHeights(float[,] surfaceHeightMap)
        {
            if (_terrainLevelService == null)
                return;

            _terrainLevelService.SetLevelMap(BuildLevelMapFromSurfaceHeights(surfaceHeightMap));
            _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
        }

        private float ResolveTwcLayerSurfaceHeight(BlueprintLayer blueprint, string blueprintLayerGuid)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
            TilesBuildLayer buildLayer = FindTilesBuildLayer(_manager?.configuration, blueprintLayerGuid);
            if (buildLayer == null)
                return baseHeight;

            float layerBaseHeight = baseHeight + buildLayer.layerYOffset;
            if (buildLayer.tileLayers == null || buildLayer.tileLayers.Count == 0)
                return layerBaseHeight;

            bool hasTileLayer = false;
            float topHeight = layerBaseHeight;
            for (int i = 0; i < buildLayer.tileLayers.Count; i++)
            {
                var tileLayer = buildLayer.tileLayers[i];
                if (tileLayer == null)
                    continue;

                float candidate = layerBaseHeight + tileLayer.heightOffset;
                topHeight = hasTileLayer ? Mathf.Max(topHeight, candidate) : candidate;
                hasTileLayer = true;
            }

            return hasTileLayer ? topHeight : layerBaseHeight;
        }

        private static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer)
                        continue;

                    if (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                        || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal))
                    {
                        return buildLayer;
                    }
                }
            }

            return null;
        }

        private static int[,] BuildLevelMapFromSurfaceHeights(float[,] surfaceHeightMap)
        {
            if (surfaceHeightMap == null)
                return null;

            int width = surfaceHeightMap.GetLength(0);
            int height = surfaceHeightMap.GetLength(1);
            var levelMap = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                levelMap[x, y] = Mathf.Max(0, Mathf.RoundToInt(surfaceHeightMap[x, y]));

            return levelMap;
        }

        private static void EmitEmpty(int width, int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            onComplete?.Invoke(
                new string[width, height],
                new string[width, height],
                new float[width, height],
                new string[width, height]);
        }

        private int GetSeedFromGraph()
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return launchSeed;

            if (_graphAsset?.Nodes == null)
                return GlobalSeed.DefaultSeed;

            foreach (var node in _graphAsset.Nodes)
            {
                if (node is ISeedProvider seedProvider)
                    return seedProvider.Seed;
            }

            return GlobalSeed.DefaultSeed;
        }
    }
}
