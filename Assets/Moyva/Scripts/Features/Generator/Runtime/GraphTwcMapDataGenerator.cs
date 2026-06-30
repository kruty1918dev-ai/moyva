using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
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

        private bool _hasLastBaseMapWorldBounds;
        private Bounds _lastBaseMapWorldBounds;

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

            // Launch context has priority for real gameplay starts; SharedSettings keeps direct play-mode aligned with preview.
            var shared = _graphAsset?.SharedSettings;
            if (GameLaunchContext.TryGetWorldDimensions(out int launchWidth, out int launchHeight))
            {
                width = launchWidth;
                height = launchHeight;
            }
            else if (shared != null && shared.HasMapSize)
            {
                width = shared.MapWidth;
                height = shared.MapHeight;
            }

            int baseMapWidth = Mathf.Max(1, width);
            int baseMapHeight = Mathf.Max(1, height);

            if (_manager == null || _manager.configuration == null)
            {
                Debug.LogError("[GraphTwcGenerator] TileWorldCreatorManager/Configuration відсутній. " +
                               "Перевірте MoyvaTileWorldCreatorGraphBinding на TileWorldCreatorManager.");
                GraphGenerationLayerLog.Emit(
                    "Runtime GraphTwcMapDataGenerator",
                    _graphAsset,
                    _manager,
                    LastCompiledLayers,
                    null,
                    null,
                    seed,
                    new Vector2Int(baseMapWidth, baseMapHeight),
                    false);
                EmitEmpty(width, height, onComplete);
                return;
            }

            try
            {
                var validator = new GraphValidator();
                var report = validator.ValidateDetailed(_graphAsset);
                var globalErrors = GetGlobalValidationErrors(report);
                if (globalErrors.Count > 0)
                {
                    Debug.LogError(
                        $"[GraphTwcGenerator] Graph validation failed with {globalErrors.Count} global error(s):\n{FormatValidationIssues(globalErrors)}");
                    GraphGenerationLayerLog.Emit(
                        "Runtime GraphTwcMapDataGenerator",
                        _graphAsset,
                        _manager,
                        Array.Empty<CompiledLayerMap>(),
                        report,
                        null,
                        seed,
                        new Vector2Int(baseMapWidth, baseMapHeight),
                        false);
                    EmitEmpty(width, height, onComplete);
                    return;
                }

                var skippedLayerIds = GetInvalidLayerIds(report);
                if (skippedLayerIds.Count > 0)
                {
                    Debug.LogWarning(
                        $"[GraphTwcGenerator] {skippedLayerIds.Count} layer(s) skipped because of validation errors:\n{FormatValidationReport(report)}");
                }

                // 1. Граф -> TWC Configuration.
                var compiled = GraphToConfigurationCompiler.Compile(
                    _graphAsset,
                    _manager,
                    seed,
                    skippedLayerIds,
                    new Vector2Int(baseMapWidth, baseMapHeight));
                LastCompiledLayers = compiled;

                // Gameplay-мапи лишаються розміром базової згенерованої мапи.
                // Розширення окремих TWC blueprint-шарів через border padding не
                // повинно збільшувати fog/grid/world data.
                width = baseMapWidth;
                height = baseMapHeight;
                LastCellSize = _manager.configuration.cellSize > 0.0001f
                    ? _manager.configuration.cellSize
                    : 1f;
                _hasLastBaseMapWorldBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                    _manager.transform,
                    width,
                    height,
                    LastCellSize,
                    out _lastBaseMapWorldBounds);

                // 2. TWC будує мапу (blueprint-стек + 3D build-стек).
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(_manager);
                GraphGenerationLayerLog.Emit(
                    "Runtime GraphTwcMapDataGenerator",
                    _graphAsset,
                    _manager,
                    compiled,
                    report,
                    skippedLayerIds,
                    seed,
                    new Vector2Int(width, height),
                    true);

                // 3. Експортуємо фінальну logical tile map з того самого helper-а,
                // який використовує Graph Editor preview.
                var logicalMap = GraphLogicalTileMapBuilder.Build(
                    _graphAsset,
                    _manager,
                    compiled,
                    width,
                    height);
                GraphLogicalTileMapDiagnostics.EmitAndCompare(
                    "Scene build mask",
                    _graphAsset,
                    seed,
                    logicalMap);
                var biomeMap = logicalMap.TileIds;
                var heightMap = logicalMap.LayerHeights;
                var surfaceHeightMap = logicalMap.SurfaceHeights;
                PublishTerrainHeights(surfaceHeightMap);

                // Об'єкти/будівлі у новому конвеєрі генерує не граф, а ігролад/TWC build-шари.
                var objectMap = new string[width, height];
                var buildingMap = new string[width, height];

                onComplete?.Invoke(biomeMap, objectMap, heightMap, buildingMap);
            }
            catch (Exception ex)
            {
                _hasLastBaseMapWorldBounds = false;
                Debug.LogError($"[GraphTwcGenerator] Помилка генерації: {ex}");
                EmitEmpty(width, height, onComplete);
            }
        }

        private static string FormatValidationReport(GraphValidationReport report)
        {
            if (report == null || report.Issues.Count == 0)
                return string.Empty;

            return FormatValidationIssues(report.Issues);
        }

        private static string FormatValidationIssues(IEnumerable<GraphValidationIssue> issues)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var issue in issues)
                builder.AppendLine($"  - {issue}");
            return builder.ToString();
        }

        private static List<GraphValidationIssue> GetGlobalValidationErrors(GraphValidationReport report)
        {
            var result = new List<GraphValidationIssue>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == ValidationSeverity.Error && string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue);
            }

            return result;
        }

        private static HashSet<string> GetInvalidLayerIds(GraphValidationReport report)
        {
            var result = new HashSet<string>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == ValidationSeverity.Error && !string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue.LayerId);
            }

            return result;
        }

        internal bool TryGetLastBaseMapWorldBounds(out Bounds bounds)
        {
            bounds = _lastBaseMapWorldBounds;
            return _hasLastBaseMapWorldBounds;
        }

        private void PublishTerrainHeights(float[,] surfaceHeightMap)
        {
            if (_terrainLevelService == null)
                return;

            _terrainLevelService.SetLevelMap(BuildLevelMapFromSurfaceHeights(surfaceHeightMap));
            _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
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
