using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Адаптер: реалізує IMapDataGenerator через виконання GraphAsset у GraphRunner.
    /// Замінює лінійний MapDataGenerator на граф-базовану генерацію.
    /// Використовує ISeedProvider для отримання seed без рефлексії.
    /// </summary>
    internal sealed class GraphBasedMapDataGenerator : IMapDataGenerator
    {
       /// <summary>
        /// Заповнюється після кожного GenerateMapData.
        /// Містить шари які SingleTileLayerNode(и) додали під час виконання графа.
        /// </summary>
        internal WorldLayerData[] LastLayerData { get; private set; }
        internal int[,] LastTerrainLevelMap { get; private set; }
        internal HillLevelDataMap LastHillLevelData { get; private set; }

        /// <summary>
        /// true, якщо BiomeMap цього запуску була зібрана виключно з layer-даних
        /// (без валідного BiomeMap-виходу з OutputNode).
        /// </summary>
        internal bool LastBiomeMapDerivedFromLayers { get; private set; }

        private readonly GraphAsset _graphAsset;
        private readonly IGraphRunner _graphRunner;
        private readonly INoiseProvider _noiseProvider;
        private readonly IVirtualHeightMapGenerator _virtualHeightMapGenerator;
        private readonly IBiomeResolver _biomeResolver;
        private readonly IRiverPathfinder _riverPathfinder;
        private readonly IWFCService _wfcService;
        private readonly TileRegistrySO _tileRegistry;
        private readonly IGeneratorDataRegistry _generatorDataRegistry;
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        public GraphBasedMapDataGenerator(
            GraphAsset graphAsset,
            IGraphRunner graphRunner,
            INoiseProvider noiseProvider,
            IVirtualHeightMapGenerator virtualHeightMapGenerator,
            IBiomeResolver biomeResolver,
            IRiverPathfinder riverPathfinder,
            IWFCService wfcService,
            [Zenject.InjectOptional] TileRegistrySO tileRegistry = null,
            [Zenject.InjectOptional] IGeneratorDataRegistry generatorDataRegistry = null,
            [Zenject.InjectOptional] IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _graphAsset = graphAsset;
            _graphRunner = graphRunner;
            _noiseProvider = noiseProvider;
            _virtualHeightMapGenerator = virtualHeightMapGenerator;
            _biomeResolver = biomeResolver;
            _riverPathfinder = riverPathfinder;
            _wfcService = wfcService;
            _tileRegistry = tileRegistry;
            _generatorDataRegistry = generatorDataRegistry;
            _terrainLevelService = terrainLevelService;
        }

        public void GenerateMapData(int width, int height,
            Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int generationSeed = GetSeedFromGraph();
            GlobalSeed.Set(generationSeed);
            var previousRandomState = UnityEngine.Random.state;
            UnityEngine.Random.InitState(generationSeed);

            try
            {
                LastBiomeMapDerivedFromLayers = false;
                LastTerrainLevelMap = null;
                LastHillLevelData = null;
                _generatorDataRegistry?.Clear();
                _terrainLevelService?.Clear();

                // Якщо GraphSharedSettings задає розмір мапи — використовуємо його,
                // щоб розмір плей-моду збігався з розміром превью в редакторі.
                var sharedSettings = _graphAsset?.SharedSettings;
                if (!GameLaunchContext.HasWorldSettings && sharedSettings != null && sharedSettings.HasMapSize)
                {
                    width  = sharedSettings.MapWidth;
                    height = sharedSettings.MapHeight;
                }

                var context = new NodeContext(generationSeed);
                context.MapSize = new Vector2Int(width, height);

                // Реєструємо сервіси з перевіркою на null
                if (_noiseProvider != null)
                    context.RegisterService(_noiseProvider);
                if (_virtualHeightMapGenerator != null)
                    context.RegisterService(_virtualHeightMapGenerator);
                if (_biomeResolver != null)
                    context.RegisterService(_biomeResolver);
                if (_riverPathfinder != null)
                    context.RegisterService(_riverPathfinder);
                if (_wfcService != null)
                    context.RegisterService(_wfcService);
                if (_tileRegistry != null)
                    context.RegisterService(_tileRegistry);
                if (_generatorDataRegistry != null)
                    context.RegisterService(_generatorDataRegistry);

                // Реєструємо GraphSharedSettings
                if (sharedSettings != null)
                {
                    context.RegisterService(sharedSettings);
                }

                var layerDataList = new List<WorldLayerData>();
                context.RegisterService(layerDataList);

                var result = _graphRunner.Execute(_graphAsset, context);

                LastLayerData = layerDataList.Count > 0 ? layerDataList.ToArray() : null;

                if (!result.Success)
                {
                    Debug.LogError($"[GraphBasedGenerator] Execution failed: {result.ErrorMessage}");
                    Debug.LogError(BuildGraphDiagnostics(result));

                    // Fallback: якщо є дані шарів, відновлюємо BiomeMap із них.
                    // Для кожної комірки (x,y) беремо tileId верхнього видимого шару.
                    if (layerDataList.Count > 0)
                    {
                        Debug.LogWarning("[GraphBasedGenerator] Trying layer-based BiomeMap fallback.");
                        var fallbackBiome = BuildBiomeMapFromLayers(layerDataList, width, height);
                        onComplete?.Invoke(
                            fallbackBiome,
                            new string[width, height],
                            new float[width, height],
                            new string[width, height]);
                    }
                    else
                    {
                        onComplete?.Invoke(
                            new string[width, height],
                            new string[width, height],
                            new float[width, height],
                            new string[width, height]);
                    }
                    return;
                }

                CaptureHillTerrainLevelData(result);

                // OutputNode і його мапи опціональні: граф може працювати лише side-effects (напр. LayerData).
                var biomeMap = new string[width, height];
                var objectMap = new string[width, height];
                var heightMap = new float[width, height];
                var buildingMap = new string[width, height];

                var outputNode = _graphAsset?.Nodes
                    .Where(n => n != null)
                    .FirstOrDefault(n => n is Nodes.OutputNode);

                if (outputNode == null)
                {
                    Debug.LogWarning("[GraphBasedGenerator] No OutputNode found in graph. Using empty maps and keeping side-effect outputs.");
                }
                else
                {
                    var outputs = result.GetOutputs(outputNode.NodeId);
                    if (outputs == null || outputs.Length < 1)
                    {
                        Debug.LogWarning("[GraphBasedGenerator] OutputNode produced no map data. Using empty maps.");
                    }
                    else
                    {
                        biomeMap = outputs[0] as string[,] ?? biomeMap;
                        objectMap = outputs.Length > 1
                            ? outputs[1] as string[,] ?? objectMap
                            : objectMap;
                        heightMap = outputs.Length > 2
                            ? outputs[2] as float[,] ?? heightMap
                            : heightMap;
                        buildingMap = outputs.Length > 3
                            ? outputs[3] as string[,] ?? buildingMap
                            : buildingMap;
                    }
                }

                // Якщо граф відпрацював успішно, але BiomeMap порожня/частково порожня,
                // дозаповнюємо її з layer-даних, щоб GridService не отримував null tileId.
                if (layerDataList.Count > 0)
                {
                    bool biomeWasCompletelyEmpty = IsBiomeMapCompletelyEmpty(biomeMap, width, height);
                    var layerBiome = BuildBiomeMapFromLayers(layerDataList, width, height);
                    MergeEmptyBiomeCells(biomeMap, layerBiome, width, height);

                    if (biomeWasCompletelyEmpty && !IsBiomeMapCompletelyEmpty(biomeMap, width, height))
                    {
                        LastBiomeMapDerivedFromLayers = true;
                    }
                }

                foreach (var log in result.Logs)
                {
                    if (log.Status == NodeStatus.Error)
                        Debug.LogError($"[GraphRunner] {log.NodeTitle}: {log.Message}");
                    else if (log.Status == NodeStatus.Warning)
                        Debug.LogWarning($"[GraphRunner] {log.NodeTitle}: {log.Message}");
                }

                onComplete?.Invoke(biomeMap, objectMap, heightMap, buildingMap);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GraphBasedGenerator] Unhandled exception: {ex}");
                Debug.LogError(BuildGraphDiagnostics(null));
                onComplete?.Invoke(
                    new string[width, height],
                    new string[width, height],
                    new float[width, height],
                    new string[width, height]);
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
            }
        }

        private void CaptureHillTerrainLevelData(GraphExecutionResult result)
        {
            if (result == null || _graphAsset?.Nodes == null)
                return;

            foreach (var node in _graphAsset.Nodes)
            {
                if (node is not Nodes.HillGeneratorNode)
                    continue;

                var outputs = result.GetOutputs(node.NodeId);
                if (outputs == null || outputs.Length == 0)
                    continue;

                var levelMap = outputs.Length > 1 ? outputs[1] as int[,] : null;
                var hillLevelData = outputs.Length > 2 ? outputs[2] as HillLevelDataMap : null;

                if (hillLevelData != null)
                {
                    LastHillLevelData = hillLevelData.Clone();
                    _terrainLevelService?.SetHillLevelData(hillLevelData);
                    LastTerrainLevelMap = _terrainLevelService?.CopyLevelMap() ?? BuildLevelMapFromHillData(hillLevelData);
                    _generatorDataRegistry?.Set("hill-levels", LastHillLevelData);
                    _generatorDataRegistry?.Set($"hill-levels:{node.NodeId}", LastHillLevelData);
                    return;
                }

                if (levelMap != null)
                {
                    LastTerrainLevelMap = CloneLevelMap(levelMap);
                    _terrainLevelService?.SetLevelMap(LastTerrainLevelMap);
                    _generatorDataRegistry?.Set("hill-level-map", CloneLevelMap(LastTerrainLevelMap));
                    _generatorDataRegistry?.Set($"hill-level-map:{node.NodeId}", CloneLevelMap(LastTerrainLevelMap));
                    return;
                }
            }
        }

        private static int[,] BuildLevelMapFromHillData(HillLevelDataMap data)
        {
            if (data == null)
                return null;

            var result = new int[data.Width, data.Height];
            for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
                result[x, y] = Mathf.Max(0, data.GetTile(x, y).Level);

            return result;
        }

        private static int[,] CloneLevelMap(int[,] source)
        {
            if (source == null)
                return null;

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var result = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                result[x, y] = Mathf.Max(0, source[x, y]);

            return result;
        }

        private string BuildGraphDiagnostics(GraphExecutionResult result)
        {
            var sb = new StringBuilder(1024);
            sb.AppendLine("[GraphBasedGenerator] Diagnostics dump:");
            sb.AppendLine($"- Seed: {GetSeedFromGraph()}");
            sb.AppendLine($"- GraphAsset is null: {_graphAsset == null}");

            if (_graphAsset == null)
                return sb.ToString();

            var nodes = _graphAsset.Nodes;
            var connections = _graphAsset.Connections;

            sb.AppendLine($"- Nodes count: {nodes?.Count ?? 0}");
            sb.AppendLine($"- Connections count: {connections?.Count ?? 0}");

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node == null)
                    {
                        sb.AppendLine($"  [Node#{i}] NULL");
                        continue;
                    }

                    if (string.IsNullOrEmpty(node.NodeId))
                    {
                        sb.AppendLine($"  [Node#{i}] Empty NodeId, Title='{node.Title}', Type={node.GetType().Name}");
                    }
                }
            }

            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    var c = connections[i];
                    if (string.IsNullOrEmpty(c.SourceNodeId) || string.IsNullOrEmpty(c.TargetNodeId))
                    {
                        sb.AppendLine(
                            $"  [Conn#{i}] Invalid IDs: Source='{c.SourceNodeId ?? "<null>"}', Target='{c.TargetNodeId ?? "<null>"}', SourcePort={c.SourcePortIndex}, TargetPort={c.TargetPortIndex}");
                    }
                }
            }

            if (result?.Logs != null && result.Logs.Count > 0)
            {
                var tailCount = Math.Min(8, result.Logs.Count);
                sb.AppendLine($"- Last {tailCount} runner logs:");
                for (int i = result.Logs.Count - tailCount; i < result.Logs.Count; i++)
                {
                    var l = result.Logs[i];
                    sb.AppendLine($"  [{l.Status}] NodeId='{l.NodeId}' Title='{l.NodeTitle}' Msg='{l.Message}' TimeMs={l.DurationMs}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Відновлює BiomeMap з набору шарів.
        /// Для кожної комірки (x, y) йдемо від шару з найвищим SortingOrder (верхній) до нижнього.
        /// Перший шар, у якого піксель у цій позиції має alpha > 0, дає TileID для комірки.
        /// Якщо всі шари прозорі — комірка залишається порожньою.
        /// </summary>
        private static string[,] BuildBiomeMapFromLayers(
            List<WorldLayerData> layers, int mapW, int mapH)
        {
            var biome = new string[mapW, mapH];

            // Сортуємо від найвищого до найнижчого SortingOrder → верхній шар перевіряється першим.
            var sorted = new List<WorldLayerData>(layers);
            sorted.Sort((a, b) => b.SortingOrder.CompareTo(a.SortingOrder));

            // Кешуємо пікселі кожного шару одним GetPixels() щоб уникнути зайвих звернень до GPU.
            var pixelCache = new Color[sorted.Count][];
            var texWidths  = new int[sorted.Count];
            var texHeights = new int[sorted.Count];

            for (int li = 0; li < sorted.Count; li++)
            {
                var tex = sorted[li].TileTexture;
                if (tex == null) continue;
                texWidths[li]  = tex.width;
                texHeights[li] = tex.height;
                pixelCache[li] = tex.GetPixels();
            }

            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    for (int li = 0; li < sorted.Count; li++)
                    {
                        var pixels = pixelCache[li];
                        if (pixels == null) continue;

                        int tw = texWidths[li];
                        int th = texHeights[li];

                        // Пропорційний mapінг комірки → texel (ті ж індекси що й в SingleTileLayerNode).
                        int texX = Mathf.Clamp((x * tw) / mapW, 0, tw - 1);
                        int texY = Mathf.Clamp((y * th) / mapH, 0, th - 1);
                        float alpha = pixels[texY * tw + texX].a;

                        if (alpha > 0f)
                        {
                            biome[x, y] = sorted[li].LayerTileID;
                            break; // Знайшли найвищий видимий шар — далі не перевіряємо.
                        }
                    }
                }
            }

            return biome;
        }

        private static void MergeEmptyBiomeCells(string[,] target, string[,] source, int mapW, int mapH)
        {
            if (target == null || source == null)
                return;

            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    if (string.IsNullOrEmpty(target[x, y]) && !string.IsNullOrEmpty(source[x, y]))
                    {
                        target[x, y] = source[x, y];
                    }
                }
            }
        }

        private static bool IsBiomeMapCompletelyEmpty(string[,] biomeMap, int mapW, int mapH)
        {
            if (biomeMap == null)
                return true;

            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    if (!string.IsNullOrEmpty(biomeMap[x, y]))
                        return false;
                }
            }

            return true;
        }

        private int GetSeedFromGraph()
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return launchSeed;

            if (_graphAsset?.Nodes == null)
                return GlobalSeed.DefaultSeed;

            foreach (var node in _graphAsset.Nodes)
            {
                if (node == null) continue;
                if (node is ISeedProvider seedProvider)
                    return seedProvider.Seed;
            }

            return GlobalSeed.DefaultSeed;
        }
    }
}
