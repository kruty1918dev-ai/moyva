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
    /// Ідентичність клітинки = id шару графа (layer-based identity), що його ця
    /// клітинка отримала від верхнього (за SortingOrder) blueprint-шару.
    /// Цей id потрапляє у biomeMap і далі в GridService; gameplay-властивості
    /// резолвляться через <see cref="Grid.API.TerrainLayerProfileSO"/>.
    /// </summary>
    internal sealed class GraphTwcMapDataGenerator : IMapDataGenerator
    {
        private readonly GraphAsset _graphAsset;
        private readonly TileWorldCreatorManager _manager;

        /// <summary>
        /// Зіставлення graphLayerId &lt;-&gt; TWC blueprint-шару, оновлюється
        /// після кожної генерації. Дозволяє downstream-коду знати, які шари існують.
        /// </summary>
        internal IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }

        public GraphTwcMapDataGenerator(
            GraphAsset graphAsset,
            TileWorldCreatorManager manager)
        {
            _graphAsset = graphAsset;
            _manager = manager;
        }

        public void GenerateMapData(int width, int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int seed = GetSeedFromGraph();
            GlobalSeed.Set(seed);

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
                               "Перевірте GeneratorInstaller (TileWorldCreator Integration).");
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

                // 2. TWC будує мапу (blueprint-стек + 3D build-стек).
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(_manager);

                // 3. Експортуємо layer-id сітку та висоти.
                var biomeMap = new string[width, height];
                var heightMap = new float[width, height];
                BuildLayerIdGrid(compiled, width, height, biomeMap, heightMap);

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
        /// Для кожної клітинки записує id шару графа з верхнього (найбільший
        /// SortingOrder) blueprint-шару, що містить цю позицію. Висота береться
        /// з defaultLayerHeight цього ж шару.
        /// </summary>
        private void BuildLayerIdGrid(
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height,
            string[,] biomeMap,
            float[,] heightMap)
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

                foreach (var position in blueprint.allPositions)
                {
                    int x = Mathf.RoundToInt(position.x);
                    int y = Mathf.RoundToInt(position.y);
                    if (x < 0 || x >= width || y < 0 || y >= height)
                        continue;

                    biomeMap[x, y] = layerMap.GraphLayerId;
                    heightMap[x, y] = layerHeight;
                }
            }
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
