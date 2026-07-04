using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorWorldBuildBridge
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private const string SideWallLogTag = "[MoyvaTWCHeight:SideWalls]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorIdMappingSO _mapping;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly IGeneratorTerrainLevelService _terrainLevelService;
        private readonly HashSet<string> _loggedMissingLayers = new HashSet<string>();
        private readonly HashSet<string> _loggedInvalidBuildLayers = new HashSet<string>();

        public TileWorldCreatorWorldBuildBridge(
            TileWorldCreatorManager manager,
            TileWorldCreatorIdMappingSO mapping,
            TileWorldCreatorBuildOptions options,
            IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _manager = manager;
            _mapping = mapping;
            _options = options ?? new TileWorldCreatorBuildOptions();
            _terrainLevelService = terrainLevelService;
        }

        public TileWorldCreatorWorldBuildResult Build(GeneratedWorldData worldData)
        {
            if (_manager == null || _mapping == null || worldData == null)
                return TileWorldCreatorWorldBuildResult.Disabled;

            _terrainLevelService?.Clear();

            var configuration = _manager.configuration;
            if (configuration == null)
            {
                Debug.LogWarning($"{LogTag} TileWorldCreatorManager has no configuration assigned.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            LogBuildStart(worldData, configuration);

            if (_options.ApplyIntegerTerrainHeights)
                EnsureTerrainLevelMap(worldData);

            LogLevelMap("after EnsureTerrainLevelMap", worldData?.TerrainLevelMap);

            if (_options.NormalizeTerrainLevelsForTileWorldCreator)
            {
                LogLevelMap("before NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
                NormalizeTerrainLevelsForTileWorldCreator(worldData);
                LogLevelMap("after NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
            }

            if (_options.ExpandSandShoreBand)
                ExpandSandShoreBand(worldData);

            if (_options.ApplyIntegerTerrainHeights && _options.NormalizeTerrainLevelsForTileWorldCreator)
            {
                LogLevelMap("before post-shore NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
                NormalizeTerrainLevelsForTileWorldCreator(worldData);
                LogLevelMap("after post-shore NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
            }

            var terrainLayerPositions = new Dictionary<string, HashSet<Vector2>>();
            var objectLayerPositions = new Dictionary<string, HashSet<Vector2>>();
            var buildingLayerPositions = new Dictionary<string, HashSet<Vector2>>();
            var mappedTerrainIds = new HashSet<string>();
            var mappedObjectIds = new HashSet<string>();
            var mappedBuildingIds = new HashSet<string>();

            CollectMappedPositions(
                worldData.BiomeMap,
                _mapping.TryResolveTerrainLayer,
                configuration,
                terrainLayerPositions,
                mappedTerrainIds);
            LogMappedLayerSummary("terrain", terrainLayerPositions, mappedTerrainIds, configuration);

            if (_options.SendObjectIdsToTileWorldCreator)
            {
                CollectMappedPositions(
                    worldData.ObjectMap,
                    _mapping.TryResolveObjectLayer,
                    configuration,
                    objectLayerPositions,
                    mappedObjectIds);
                LogMappedLayerSummary("objects", objectLayerPositions, mappedObjectIds, configuration);
            }

            if (_options.SendBuildingIdsToTileWorldCreator)
            {
                CollectMappedPositions(
                    worldData.BuildingMap,
                    _mapping.TryResolveBuildingLayer,
                    configuration,
                    buildingLayerPositions,
                    mappedBuildingIds);
                LogMappedLayerSummary("buildings", buildingLayerPositions, mappedBuildingIds, configuration);
            }

            bool hasAnyMappedLayer = terrainLayerPositions.Count > 0
                || objectLayerPositions.Count > 0
                || buildingLayerPositions.Count > 0;

            if (!hasAnyMappedLayer)
            {
                Debug.LogWarning($"{LogTag} TWC build disabled: no mapped terrain/object/building positions were collected. BiomeMap={FormatMapSize(worldData.BiomeMap)}.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            try
            {
                PrepareConfiguration(configuration, worldData);
                EnsureTerrainBuildLayerConfiguration(configuration);

                if (_options.ResetConfigurationBeforeBuild)
                {
                    Debug.Log($"{LogTag} ResetConfiguration before build. Renderers before reset={CountComponentsInManager<Renderer>()}, transforms before reset={CountManagerChildren()}.");
                    _manager.ResetConfiguration();
                }

                ApplyPositions(terrainLayerPositions);
                ApplyPositions(objectLayerPositions);
                ApplyPositions(buildingLayerPositions);

                var occlusion = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);
                if (occlusion.RemovedCellCount > 0)
                    Debug.Log($"{LogTag} Removed {occlusion.RemovedCellCount} occluded TWC tile cells across {occlusion.ProcessedLayerCount} build layers before spawning. occupied={occlusion.OccupiedCellCount}, skipped={occlusion.SkippedLayerCount}.");

                Debug.Log($"{LogTag} Calling ExecuteBuildLayers(FromScratch). Config size={configuration.width}x{configuration.height}, cellSize={configuration.cellSize}, mergeTiles={configuration.mergeTiles}, terrainLayers={terrainLayerPositions.Count}. Runtime cells were already pushed with AddCellsToLayerByGuid, so GenerateCompleteMap is intentionally skipped because it re-executes and resets blueprint layers.");
                Debug.Log(
                    $"{WorldGenDiagTag} TWCBuild.START manager={_manager.name}, config={configuration.name}, map={configuration.width}x{configuration.height}, " +
                    $"frame={Time.frameCount}, childrenBefore={CountManagerChildren()}, asyncHint=coroutine/delayed");

                var buildStopwatch = System.Diagnostics.Stopwatch.StartNew();
                _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
                buildStopwatch.Stop();

                Debug.Log($"{LogTag} ExecuteBuildLayers returned. Immediate renderers={CountComponentsInManager<Renderer>()}, meshFilters={CountComponentsInManager<MeshFilter>()}, childTransforms={CountManagerChildren()}. TWC can continue spawning tiles by coroutine; projector will keep logging while tracking.");
                Debug.Log(
                    $"{WorldGenDiagTag} TWCBuild.RETURN manager={_manager.name}, frame={Time.frameCount}, elapsedMs={buildStopwatch.ElapsedMilliseconds}, " +
                    $"childrenAfterReturn={CountManagerChildren()}, mayContinueAsync=true");

                if (_options.ApplyIntegerTerrainHeights)
                    ApplyIntegerTerrainHeights(worldData, configuration);

                PublishTerrainHeights(worldData, configuration);
                bool hasBaseMapWorldBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                    _manager.transform,
                    worldData.Width,
                    worldData.Height,
                    configuration.cellSize,
                    out Bounds baseMapWorldBounds);

                return new TileWorldCreatorWorldBuildResult(
                    mappedTerrainIds,
                    mappedObjectIds,
                    mappedBuildingIds,
                    _options.ReplaceMappedTerrainVisuals,
                    _options.ReplaceMappedObjectVisuals,
                    _options.ReplaceMappedBuildingVisuals,
                    _options.SuppressMoyvaLayerDataWhenTerrainMapped && mappedTerrainIds.Count > 0,
                    configuration.cellSize,
                    hasBaseMapWorldBounds,
                    baseMapWorldBounds);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{LogTag} Build failed: {ex}");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }
        }

        private void PrepareConfiguration(Configuration configuration, GeneratedWorldData worldData)
        {
            Debug.Log($"{LogTag} PrepareConfiguration before: config='{configuration.name}', size={configuration.width}x{configuration.height}, cellSize={configuration.cellSize}, lastCellSize={configuration.lastCellSize}, clusterCellSize={configuration.clusterCellSize}, mergeTiles={configuration.mergeTiles}, useGlobalSeed={configuration.useGlobalRandomSeed}, globalSeed={configuration.globalRandomSeed}, currentSeed={configuration.currentRandomSeed}.");

            if (_options.SyncConfigurationSize)
            {
                configuration.width = Mathf.Max(1, worldData.Width);
                configuration.height = Mathf.Max(1, worldData.Height);
            }

            if (_options.ConfigurationCellSizeOverride > 0f)
            {
                configuration.cellSize = _options.ConfigurationCellSizeOverride;
                configuration.lastCellSize = _options.ConfigurationCellSizeOverride;
            }

            if (_options.UseWorldSeed)
            {
                configuration.useGlobalRandomSeed = true;
                configuration.globalRandomSeed = NormalizeTileWorldCreatorSeed(worldData.Seed);
            }

            if (configuration.currentRandomSeed == 0)
                configuration.currentRandomSeed = (uint)NormalizeTileWorldCreatorSeed(worldData.Seed);

            if (_options.ApplyIntegerTerrainHeights && configuration.mergeTiles)
            {
                Debug.LogWarning($"{LogTag} Disabling Configuration.mergeTiles for Moyva per-cell height projection. TWC merged cluster meshes cannot be shifted per terrain cell.");
                configuration.mergeTiles = false;
            }

            Debug.Log($"{LogTag} PrepareConfiguration after: config='{configuration.name}', size={configuration.width}x{configuration.height}, cellSize={configuration.cellSize}, lastCellSize={configuration.lastCellSize}, clusterCellSize={configuration.clusterCellSize}, mergeTiles={configuration.mergeTiles}, useGlobalSeed={configuration.useGlobalRandomSeed}, globalSeed={configuration.globalRandomSeed}, currentSeed={configuration.currentRandomSeed}.");
        }

        private static int NormalizeTileWorldCreatorSeed(int seed)
        {
            return seed == 0 ? 1 : seed;
        }

        private void EnsureTerrainBuildLayerConfiguration(Configuration configuration)
        {
            if (configuration == null || _mapping?.TerrainLayers == null)
                return;

            var configuredLayerGuids = new HashSet<string>();
            foreach (var mapping in _mapping.TerrainLayers)
            {
                if (mapping == null || mapping.TilePreset == null)
                    continue;
                if (!TryResolveBlueprintLayerGuid(configuration, mapping, out string blueprintLayerGuid))
                    continue;
                if (!configuredLayerGuids.Add(blueprintLayerGuid))
                    continue;

                var buildLayer = FindTilesBuildLayer(configuration, blueprintLayerGuid);
                if (buildLayer == null)
                {
                    LogInvalidBuildLayerOnce(mapping, "no TilesBuildLayer is assigned to the resolved blueprint layer");
                    continue;
                }

                bool useDualGrid = ShouldUseDualGrid(mapping.TilePreset, mapping.UseDualGrid);
                if (!HasUsableTilePreset(mapping.TilePreset, useDualGrid))
                {
                    LogInvalidBuildLayerOnce(mapping, $"tile preset '{mapping.TilePreset.name}' has no usable {(useDualGrid ? "dual" : "normal")} prefab references");
                    continue;
                }

                bool oldUseDualGrid = buildLayer.useDualGrid;
                bool oldScaleToCell = buildLayer.scaleTileToCellSize;
                bool oldMeshOverride = buildLayer.meshGenerationOverride;
                bool oldLayerMerge = buildLayer.mergeTiles;
                float oldLayerYOffset = buildLayer.layerYOffset;

                buildLayer.SetBlueprintLayer(blueprintLayerGuid);
                buildLayer.SetNewTilePreset(mapping.TilePreset);
                buildLayer.useDualGrid = useDualGrid;
                buildLayer.scaleTileToCellSize = mapping.ScaleTileToCellSize || useDualGrid;
                buildLayer.layerYOffset = 0f;
                if (_options.ApplyIntegerTerrainHeights && buildLayer.meshGenerationOverride && buildLayer.mergeTiles)
                {
                    Debug.LogWarning($"{LogTag} Disabling buildLayer.mergeTiles for '{buildLayer.layerName}' because meshGenerationOverride was forcing merged cluster meshes.");
                    buildLayer.mergeTiles = false;
                }

                if (buildLayer.tileLayers == null)
                    buildLayer.tileLayers = new List<TilesBuildLayer.TileLayers>();
                if (buildLayer.tileLayers.Count == 0)
                    buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());
                for (int i = 0; i < buildLayer.tileLayers.Count; i++)
                {
                    if (buildLayer.tileLayers[i] == null)
                        buildLayer.tileLayers[i] = new TilesBuildLayer.TileLayers();
                    buildLayer.tileLayers[i].heightOffset = 0f;
                }

                Debug.Log($"{LogTag} Prepared terrain build layer: idPattern='{mapping.IdPattern}', blueprintGuid='{blueprintLayerGuid}', blueprintName='{mapping.BlueprintLayerName}', buildLayer='{buildLayer.layerName}', preset='{mapping.TilePreset.name}', presetGrid={mapping.TilePreset.gridtype}, useDualGrid {oldUseDualGrid}->{buildLayer.useDualGrid}, scaleToCell {oldScaleToCell}->{buildLayer.scaleTileToCellSize}, layerYOffset {oldLayerYOffset}->{buildLayer.layerYOffset}, meshOverride={oldMeshOverride}, buildMerge {oldLayerMerge}->{buildLayer.mergeTiles}, tileLayers={buildLayer.tileLayers.Count}.");
            }
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

                    if (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, System.StringComparison.Ordinal)
                        || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, System.StringComparison.Ordinal))
                    {
                        return buildLayer;
                    }
                }
            }

            return null;
        }

        private static bool ShouldUseDualGrid(TilePreset preset, bool mappingPreference)
            => mappingPreference
                || preset != null && preset.gridtype == TilePreset.GridType.dual
                || preset != null && !HasUsableTilePreset(preset, false) && HasUsableTilePreset(preset, true);

        private static bool HasUsableTilePreset(TilePreset preset, bool useDualGrid)
        {
            if (preset == null)
                return false;

            if (useDualGrid)
            {
                return preset.DUALGRD_fillTile != null
                    || preset.DUALGRD_edgeTile != null
                    || preset.DUALGRD_cornerTile != null
                    || preset.DUALGRD_invertedCornerTile != null
                    || preset.DUALGRD_doubleInteriorCornerTile != null;
            }

            return preset.NRMGRD_fillTile != null
                || preset.NRMGRD_singleTile != null
                || preset.NRMGRD_edgeFillTile != null
                || preset.NRMGRD_cornerFillTile != null
                || preset.NRMGRD_interiorCornerTile != null
                || preset.NRMGRD_doubleCornerTile != null
                || preset.NRMGRD_threeWayFillTile != null
                || preset.NRMGRD_edgeCornerFillTile != null;
        }

        private void LogInvalidBuildLayerOnce(TileWorldCreatorIdMappingSO.LayerMapping mapping, string reason)
        {
            string key = $"{mapping?.IdPattern}:{reason}";
            if (_loggedInvalidBuildLayers.Add(key))
                Debug.LogWarning($"{LogTag} Cannot prepare TWC terrain layer for ID pattern '{mapping?.IdPattern}': {reason}.");
        }

        private void CollectMappedPositions(
            string[,] sourceMap,
            TryResolveLayer resolveLayer,
            Configuration configuration,
            Dictionary<string, HashSet<Vector2>> positionsByLayerGuid,
            HashSet<string> mappedIds)
        {
            if (sourceMap == null)
                return;

            int width = sourceMap.GetLength(0);
            int height = sourceMap.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string id = sourceMap[x, y];
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    if (!resolveLayer(id, out var mapping))
                        continue;

                    if (!TryResolveBlueprintLayerGuid(configuration, mapping, out string layerGuid))
                        continue;

                    if (!positionsByLayerGuid.TryGetValue(layerGuid, out var positions))
                    {
                        positions = new HashSet<Vector2>();
                        positionsByLayerGuid[layerGuid] = positions;
                    }

                    positions.Add(new Vector2(x, y));
                    mappedIds.Add(id);
                }
            }
        }

        private bool TryResolveBlueprintLayerGuid(
            Configuration configuration,
            TileWorldCreatorIdMappingSO.LayerMapping mapping,
            out string layerGuid)
        {
            layerGuid = null;
            if (configuration == null || mapping == null)
                return false;

            if (!string.IsNullOrWhiteSpace(mapping.BlueprintLayerGuid)
                && configuration.GetBlueprintLayerByGuid(mapping.BlueprintLayerGuid) != null)
            {
                layerGuid = mapping.BlueprintLayerGuid;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(mapping.BlueprintLayerName))
            {
                string resolvedGuid = configuration.GetBlueprintLayerGuid(mapping.BlueprintLayerName);
                if (!string.IsNullOrWhiteSpace(resolvedGuid))
                {
                    layerGuid = resolvedGuid;
                    return true;
                }
            }

            string logKey = $"{mapping.IdPattern}:{mapping.BlueprintLayerGuid}:{mapping.BlueprintLayerName}";
            if (_loggedMissingLayers.Add(logKey))
            {
                Debug.LogWarning($"{LogTag} Cannot resolve TWC blueprint layer for ID pattern '{mapping.IdPattern}'.");
            }

            return false;
        }

        private void ApplyPositions(Dictionary<string, HashSet<Vector2>> positionsByLayerGuid)
        {
            foreach (var layerPositions in positionsByLayerGuid)
            {
                if (layerPositions.Value == null || layerPositions.Value.Count == 0)
                    continue;

                Debug.Log($"{LogTag} AddCellsToLayerByGuid layerGuid='{layerPositions.Key}', positions={layerPositions.Value.Count}, bounds={FormatPositionBounds(layerPositions.Value)}.");
                _manager.AddCellsToLayerByGuid(layerPositions.Key, layerPositions.Value);
            }
        }

        private void ExpandSandShoreBand(GeneratedWorldData worldData)
        {
            string[,] biomeMap = worldData?.BiomeMap;
            if (biomeMap == null)
                return;

            int width = biomeMap.GetLength(0);
            int height = biomeMap.GetLength(1);
            if (width == 0 || height == 0)
                return;

            string shoreId = _options.ShoreBandTileId;
            if (string.IsNullOrWhiteSpace(shoreId))
                return;

            // 1) Збираємо стартовий water-set ДО будь-яких модифікацій.
            bool[,] originalIsWater = new bool[width, height];
            bool[,] shoreBand = new bool[width, height];
            int originalWaterCount = 0;
            int originalSandCount = 0;
            int convertedToShoreCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    originalIsWater[x, y] = IsWaterBiome(biomeMap[x, y]);
                    if (originalIsWater[x, y])
                        originalWaterCount++;
                    if (IsSandBiome(biomeMap[x, y]))
                        originalSandCount++;
                }
            }

            // 2) Land-side: будь-яка не-water клітинка, що має 8-сусіда water → sand.
            //    Це гарантовано заповнює береговий ряд піском.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (originalIsWater[x, y])
                        continue;

                    string current = biomeMap[x, y];
                    if (IsSandBiome(current))
                        continue;

                    if (HasNeighbour(originalIsWater, x, y, width, height, waterValue: true))
                    {
                        biomeMap[x, y] = shoreId;
                        shoreBand[x, y] = true;
                        convertedToShoreCount++;
                    }
                }
            }

            // Water-side клітини не змінюємо: інакше пісок отримує водний level
            // і візуально просідає нижче сусідньої води.
            MarkExistingSandShoreBand(biomeMap, originalIsWater, shoreBand, width, height);
            int shoreBandCount = CountTrue(shoreBand, width, height);
            int raisedLevelCount = RaiseShoreLevelsToWater(worldData.TerrainLevelMap, shoreBand, originalIsWater, width, height);
            Debug.Log($"{LogTag} ExpandSandShoreBand: size={width}x{height}, shoreId='{shoreId}', originalWater={originalWaterCount}, originalSand={originalSandCount}, convertedLandToShore={convertedToShoreCount}, finalShoreBand={shoreBandCount}, raisedLevelCells={raisedLevelCount}, levels={FormatLevelStats(worldData.TerrainLevelMap)}.");
        }

        private static void MarkExistingSandShoreBand(string[,] biomeMap, bool[,] originalIsWater, bool[,] shoreBand, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!IsSandBiome(biomeMap[x, y]))
                        continue;

                    if (HasNeighbour(originalIsWater, x, y, width, height, waterValue: true))
                        shoreBand[x, y] = true;
                }
            }
        }

        private static int RaiseShoreLevelsToWater(int[,] levelMap, bool[,] shoreBand, bool[,] originalIsWater, int width, int height)
        {
            if (levelMap == null
                || levelMap.GetLength(0) != width
                || levelMap.GetLength(1) != height)
            {
                return 0;
            }

            int changedCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!shoreBand[x, y])
                        continue;

                    int targetLevel = Mathf.Max(0, levelMap[x, y]);
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                                continue;

                            int neighbourX = x + offsetX;
                            int neighbourY = y + offsetY;
                            if (neighbourX < 0 || neighbourY < 0 || neighbourX >= width || neighbourY >= height)
                                continue;
                            if (!originalIsWater[neighbourX, neighbourY])
                                continue;

                            targetLevel = Mathf.Max(targetLevel, levelMap[neighbourX, neighbourY]);
                        }
                    }

                    if (levelMap[x, y] != targetLevel)
                        changedCount++;
                    levelMap[x, y] = targetLevel;
                }
            }

            return changedCount;
        }

        private static bool IsWaterBiome(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            string lower = id.ToLowerInvariant();
            return lower.StartsWith("water") || lower.Contains("ocean") || lower == "sea";
        }

        private static bool IsSandBiome(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;
            string lower = id.ToLowerInvariant();
            return lower.StartsWith("sand") || lower.StartsWith("grass-coast") || lower == "beach" || lower == "coast";
        }

        private static bool HasNeighbour(bool[,] map, int x, int y, int width, int height, bool waterValue)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    if (map[nx, ny] == waterValue) return true;
                }
            }
            return false;
        }

        private void ApplyIntegerTerrainHeights(GeneratedWorldData worldData, Configuration configuration)
        {
            EnsureTerrainLevelMap(worldData);

            int[,] levelMap = worldData?.TerrainLevelMap;

            if (levelMap == null)
            {
                Debug.LogWarning($"{LogTag} ApplyIntegerTerrainHeights skipped: TerrainLevelMap is null after fallback attempt.");
                return;
            }

            var managerGo = _manager.gameObject;
            var projector = managerGo.GetComponentInChildren<TileWorldCreatorHeightProjector>(true);
            if (projector == null)
            {
                var projectorGo = new GameObject("Moyva TWC Height Projector")
                {
                    hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                projectorGo.transform.SetParent(managerGo.transform, false);
                projector = projectorGo.AddComponent<TileWorldCreatorHeightProjector>();
                projector.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            float cellSize = configuration != null && configuration.cellSize > 0.0001f
                ? configuration.cellSize
                : 1f;

            Debug.Log($"{LogTag} Configure height projector: manager='{managerGo.name}', projector='{projector.name}', cellSize={cellSize}, heightStep={_options.TerrainHeightStep}, trackingSeconds={_options.TerrainHeightTrackingSeconds}, levelStats={FormatLevelStats(levelMap)}, currentRenderers={CountComponentsInManager<Renderer>()}, currentMeshFilters={CountComponentsInManager<MeshFilter>()}, currentChildTransforms={CountManagerChildren()}.");

            projector.Configure(
                managerGo.transform,
                levelMap,
                cellSize,
                _options.TerrainHeightStep,
                _options.TerrainHeightTrackingSeconds);

            if (_options.GenerateTerrainSideWalls)
            {
                ApplyTerrainSideWalls(managerGo.transform, levelMap, configuration, cellSize);
            }
            else
            {
                var existingWallBuilder = managerGo.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true);
                if (existingWallBuilder != null)
                    existingWallBuilder.ClearWalls("GenerateTerrainSideWalls option is disabled");
            }

            ApplyTerrainMeshOptimizer(managerGo.transform);
        }

        private void ApplyTerrainSideWalls(Transform managerRoot, int[,] levelMap, Configuration configuration, float cellSize)
        {
            if (managerRoot == null || levelMap == null)
                return;

            var wallBuilder = managerRoot.GetComponentInChildren<TileWorldCreatorTerrainSideWallBuilder>(true);
            if (wallBuilder == null)
            {
                var wallGo = new GameObject("Moyva TWC Terrain Side Walls")
                {
                    hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                wallGo.transform.SetParent(managerRoot, false);
                wallBuilder = wallGo.AddComponent<TileWorldCreatorTerrainSideWallBuilder>();
                wallBuilder.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            float baseY = ResolveTerrainBaseHeight(configuration);
            Debug.Log($"{SideWallLogTag} Configure side walls from bridge: manager='{managerRoot.name}', cellSize={cellSize}, heightStep={_options.TerrainHeightStep}, baseY={baseY:0.###}, material='{(_options.TerrainSideWallMaterial != null ? _options.TerrainSideWallMaterial.name : "<runtime>")}', color={FormatColor(_options.TerrainSideWallColor)}, levelStats={FormatLevelStats(levelMap)}.");
            wallBuilder.Configure(
                managerRoot,
                levelMap,
                cellSize,
                _options.TerrainHeightStep,
                baseY,
                _options.TerrainSideWallMaterial,
                _options.TerrainSideWallColor,
                _options.GenerateTerrainSideWallsAtMapBorder);
        }

        private void ApplyTerrainMeshOptimizer(Transform managerRoot)
        {
            if (managerRoot == null)
                return;

            var optimizer = managerRoot.GetComponentInChildren<TileWorldCreatorRuntimeMeshOptimizer>(true);
            if (!_options.CombineTerrainMeshesAfterHeightProjection)
            {
                if (optimizer != null)
                    optimizer.ClearConfiguration("CombineTerrainMeshesAfterHeightProjection option is disabled");
                return;
            }

            if (optimizer == null)
            {
                var optimizerGo = new GameObject("Moyva TWC Runtime Mesh Optimizer")
                {
                    hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
                };
                optimizerGo.transform.SetParent(managerRoot, false);
                optimizer = optimizerGo.AddComponent<TileWorldCreatorRuntimeMeshOptimizer>();
                optimizer.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            optimizer.Configure(
                managerRoot,
                _options.TerrainMeshCombineClustersPerFrame,
                _options.TerrainMeshCombineDeactivateSourceObjects);
        }

        private float ResolveTerrainBaseHeight(Configuration configuration)
        {
            if (configuration == null || _mapping?.TerrainLayers == null)
                return 0f;

            bool hasBaseHeight = false;
            float baseHeight = 0f;
            foreach (var terrainMapping in _mapping.TerrainLayers)
            {
                if (!TryResolveBlueprintLayerGuid(configuration, terrainMapping, out string layerGuid))
                    continue;

                var layer = configuration.GetBlueprintLayerByGuid(layerGuid);
                if (layer == null)
                    continue;

                baseHeight = hasBaseHeight ? Mathf.Min(baseHeight, layer.defaultLayerHeight) : layer.defaultLayerHeight;
                hasBaseHeight = true;
            }

            return hasBaseHeight ? baseHeight : 0f;
        }

        private void PublishTerrainHeights(GeneratedWorldData worldData, Configuration configuration)
        {
            if (_terrainLevelService == null || worldData == null)
                return;

            if (worldData.TerrainLevelMap != null)
                _terrainLevelService.SetLevelMap(worldData.TerrainLevelMap);

            float[,] surfaceHeightMap = BuildTerrainSurfaceHeightMap(worldData, configuration);
            if (surfaceHeightMap != null)
                _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
        }

        private float[,] BuildTerrainSurfaceHeightMap(GeneratedWorldData worldData, Configuration configuration)
        {
            int width = Mathf.Max(0, worldData.Width);
            int height = Mathf.Max(0, worldData.Height);
            if (width <= 0 || height <= 0)
                return null;

            var surfaceHeightMap = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float baseHeight = ResolveFallbackSurfaceBaseHeight(worldData, x, y);
                    if (TryGetBiomeId(worldData.BiomeMap, x, y, out string biomeId)
                        && _mapping.TryResolveTerrainLayer(biomeId, out var mapping))
                    {
                        baseHeight = ResolveMappedTwcTerrainBaseHeight(configuration, mapping);
                    }

                    surfaceHeightMap[x, y] = baseHeight + ResolveIntegerTerrainHeightOffset(worldData, x, y);
                }
            }

            return surfaceHeightMap;
        }

        private float ResolveMappedTwcTerrainBaseHeight(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            if (configuration == null || mapping == null)
                return 0f;

            if (!TryResolveBlueprintLayerGuid(configuration, mapping, out string blueprintLayerGuid))
                return 0f;

            var blueprint = configuration.GetBlueprintLayerByGuid(blueprintLayerGuid);
            var buildLayer = FindTilesBuildLayer(configuration, blueprintLayerGuid);
            return ResolveTilesBuildLayerTopHeight(blueprint, buildLayer);
        }

        private static float ResolveTilesBuildLayerTopHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
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

        private float ResolveIntegerTerrainHeightOffset(GeneratedWorldData worldData, int x, int y)
        {
            if (!_options.ApplyIntegerTerrainHeights || worldData?.TerrainLevelMap == null)
                return 0f;

            if (x < 0 || x >= worldData.TerrainLevelMap.GetLength(0)
                || y < 0 || y >= worldData.TerrainLevelMap.GetLength(1))
            {
                return 0f;
            }

            return Mathf.Max(0, worldData.TerrainLevelMap[x, y]) * _options.TerrainHeightStep;
        }

        private static float ResolveFallbackSurfaceBaseHeight(GeneratedWorldData worldData, int x, int y)
        {
            if (worldData?.HeightMap == null
                || x < 0 || x >= worldData.HeightMap.GetLength(0)
                || y < 0 || y >= worldData.HeightMap.GetLength(1))
            {
                return 0f;
            }

            float value = worldData.HeightMap[x, y];
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }

        private static bool TryGetBiomeId(string[,] biomeMap, int x, int y, out string biomeId)
        {
            biomeId = null;
            if (biomeMap == null
                || x < 0 || x >= biomeMap.GetLength(0)
                || y < 0 || y >= biomeMap.GetLength(1))
            {
                return false;
            }

            biomeId = biomeMap[x, y];
            return !string.IsNullOrWhiteSpace(biomeId);
        }

        private void EnsureTerrainLevelMap(GeneratedWorldData worldData)
        {
            if (worldData == null || worldData.TerrainLevelMap != null)
                return;

            int[,] levelMap = BuildFallbackTerrainLevelMap(worldData);
            if (levelMap == null)
                return;

            worldData.TerrainLevelMap = levelMap;
            Debug.LogWarning($"{LogTag} TerrainLevelMap was missing; applied fallback integer terrain levels from height/biome map. HeightMap={FormatMapSize(worldData.HeightMap)}, BiomeMap={FormatMapSize(worldData.BiomeMap)}, fallbackStats={FormatLevelStats(levelMap)}.");
        }

        private void NormalizeTerrainLevelsForTileWorldCreator(GeneratedWorldData worldData)
        {
            if (worldData?.TerrainLevelMap == null || worldData.BiomeMap == null)
                return;

            int width = Mathf.Min(worldData.TerrainLevelMap.GetLength(0), worldData.BiomeMap.GetLength(0));
            int height = Mathf.Min(worldData.TerrainLevelMap.GetLength(1), worldData.BiomeMap.GetLength(1));
            if (width <= 0 || height <= 0)
                return;

            FindCapturedLandLevelRange(worldData, width, height, out int capturedMin, out int capturedMax);
            Debug.Log($"{LogTag} NormalizeTerrainLevelsForTileWorldCreator input: size={width}x{height}, capturedLandRange={capturedMin}..{capturedMax}, targetLevels water={_options.WaterTerrainLevel}, shore={_options.ShoreTerrainLevel}, land={_options.LandTerrainLevel}, hill={_options.HillTerrainLevel}, max={_options.MaxTerrainLevel}.");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string biomeId = worldData.BiomeMap[x, y];
                    bool nearWater = HasWaterNeighbour(worldData.BiomeMap, x, y, width, height);
                    worldData.TerrainLevelMap[x, y] = ResolveVisualTerrainLevel(
                        biomeId,
                        worldData.TerrainLevelMap[x, y],
                        capturedMin,
                        capturedMax,
                        nearWater);
                }
            }
        }

        private static void FindCapturedLandLevelRange(GeneratedWorldData worldData, int width, int height, out int capturedMin, out int capturedMax)
        {
            capturedMin = int.MaxValue;
            capturedMax = int.MinValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsWaterBiome(worldData.BiomeMap[x, y]))
                        continue;

                    int value = worldData.TerrainLevelMap[x, y];
                    capturedMin = Mathf.Min(capturedMin, value);
                    capturedMax = Mathf.Max(capturedMax, value);
                }
            }

            if (capturedMin == int.MaxValue || capturedMax == int.MinValue)
            {
                capturedMin = 0;
                capturedMax = 0;
            }
        }

        private int ResolveVisualTerrainLevel(string biomeId, int capturedLevel, int capturedMin, int capturedMax, bool nearWater)
        {
            if (IsWaterBiome(biomeId))
                return _options.WaterTerrainLevel;

            string lower = biomeId?.ToLowerInvariant() ?? string.Empty;
            if (IsSandBiome(lower) && nearWater)
                return Mathf.Max(_options.ShoreTerrainLevel, _options.WaterTerrainLevel);

            if (lower.Contains("mountain") || lower.Contains("snow"))
                return _options.MaxTerrainLevel;

            if (lower.Contains("hill") || lower.Contains("stone"))
                return Mathf.Clamp(_options.HillTerrainLevel, _options.LandTerrainLevel, _options.MaxTerrainLevel);

            int explicitLevel = ResolveExplicitLevelFromTileId(lower);
            if (explicitLevel >= 0)
                return Mathf.Clamp(explicitLevel, _options.LandTerrainLevel, _options.MaxTerrainLevel);

            return ProjectCapturedLevel(capturedLevel, capturedMin, capturedMax, _options.LandTerrainLevel, _options.MaxTerrainLevel);
        }

        private static int ResolveExplicitLevelFromTileId(string lower)
        {
            if (string.IsNullOrWhiteSpace(lower))
                return -1;

            const string marker = "level-";
            int index = lower.IndexOf(marker, System.StringComparison.Ordinal);
            if (index < 0)
                return -1;

            int start = index + marker.Length;
            if (start >= lower.Length || !char.IsDigit(lower[start]))
                return -1;

            return lower[start] - '0';
        }

        private static int ProjectCapturedLevel(int capturedLevel, int capturedMin, int capturedMax, int targetMin, int targetMax)
        {
            if (targetMax <= targetMin)
                return targetMin;

            if (capturedMax <= capturedMin)
                return Mathf.Clamp(capturedLevel, targetMin, targetMax);

            float t = Mathf.InverseLerp(capturedMin, capturedMax, capturedLevel);
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(targetMin, targetMax, t)), targetMin, targetMax);
        }

        private int[,] BuildFallbackTerrainLevelMap(GeneratedWorldData worldData)
        {
            if (worldData?.BiomeMap == null)
                return null;

            int width = worldData.BiomeMap.GetLength(0);
            int height = worldData.BiomeMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return null;

            var levelMap = new int[width, height];
            if (HasSameSize(worldData.HeightMap, width, height))
            {
                FindLandHeightRange(worldData, width, height, out float minHeight, out float maxHeight);
                Debug.Log($"{LogTag} BuildFallbackTerrainLevelMap uses HeightMap: size={width}x{height}, landHeightRange={minHeight:0.###}..{maxHeight:0.###}, targetLevelRange={_options.LandTerrainLevel}..{_options.MaxTerrainLevel}.");
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        string biomeId = worldData.BiomeMap[x, y];
                        if (IsWaterBiome(biomeId))
                        {
                            levelMap[x, y] = _options.WaterTerrainLevel;
                            continue;
                        }

                        bool nearWater = HasWaterNeighbour(worldData.BiomeMap, x, y, width, height);
                        if (IsSandBiome(biomeId) && nearWater)
                        {
                            levelMap[x, y] = Mathf.Max(_options.ShoreTerrainLevel, _options.WaterTerrainLevel);
                            continue;
                        }

                        float t = maxHeight > minHeight
                            ? Mathf.InverseLerp(minHeight, maxHeight, worldData.HeightMap[x, y])
                            : 0f;
                        levelMap[x, y] = Mathf.Clamp(
                            Mathf.RoundToInt(Mathf.Lerp(_options.LandTerrainLevel, _options.MaxTerrainLevel, t)),
                            _options.LandTerrainLevel,
                            _options.MaxTerrainLevel);
                    }
                }

                return levelMap;
            }

            Debug.LogWarning($"{LogTag} BuildFallbackTerrainLevelMap falls back to biome-only levels because HeightMap size does not match. BiomeMap={width}x{height}, HeightMap={FormatMapSize(worldData.HeightMap)}.");

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    levelMap[x, y] = ResolveBiomeFallbackLevel(worldData.BiomeMap[x, y]);
                }
            }

            return levelMap;
        }

        private static bool HasSameSize(System.Array map, int width, int height)
            => map != null
               && map.Rank == 2
               && map.GetLength(0) == width
               && map.GetLength(1) == height;

        private static void FindLandHeightRange(GeneratedWorldData worldData, int width, int height, out float minHeight, out float maxHeight)
        {
            minHeight = float.PositiveInfinity;
            maxHeight = float.NegativeInfinity;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsWaterBiome(worldData.BiomeMap[x, y]))
                        continue;

                    float value = worldData.HeightMap[x, y];
                    minHeight = Mathf.Min(minHeight, value);
                    maxHeight = Mathf.Max(maxHeight, value);
                }
            }

            if (float.IsInfinity(minHeight) || float.IsInfinity(maxHeight))
            {
                minHeight = 0f;
                maxHeight = 0f;
            }
        }

        private static int ResolveBiomeFallbackLevel(string biomeId)
        {
            if (string.IsNullOrWhiteSpace(biomeId))
                return 1;

            string lower = biomeId.ToLowerInvariant();
            if (IsWaterBiome(lower))
                return 0;
            if (lower.Contains("sand") || lower.Contains("beach") || lower.Contains("coast"))
                return 1;
            if (lower.Contains("mountain") || lower.Contains("snow"))
                return 3;
            if (lower.Contains("hill"))
                return 2;
            if (lower.Contains("forest"))
                return 2;

            return 1;
        }

        private static bool HasWaterNeighbour(string[,] biomeMap, int x, int y, int width, int height)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    if (IsWaterBiome(biomeMap[nx, ny]))
                        return true;
                }
            }

            return false;
        }

        private void LogBuildStart(GeneratedWorldData worldData, Configuration configuration)
        {
            Debug.Log($"{LogTag} Build start: manager='{_manager.name}', config='{configuration.name}', mapping='{_mapping.name}', world={worldData.Width}x{worldData.Height}, seed={worldData.Seed}, biomeMap={FormatMapSize(worldData.BiomeMap)}, heightMap={FormatMapSize(worldData.HeightMap)}, terrainLevelMap={FormatMapSize(worldData.TerrainLevelMap)}, options={FormatOptions()}.");
            Debug.Log($"{LogTag} TWC docs checkpoint: Configuration owns Blueprint Layers and Build Layers; GenerateCompleteMap executes blueprint layers then build layers; Moyva pushes runtime cells directly, so it uses ExecuteBuildLayers after AddCellsToLayerByGuid; dual-grid presets require Use Dual Grid; TWC tile localPosition is tilePosition*cellSize on X/Z and layer height on Y; merged clusters prevent per-cell post height shifts.");
            LogConfigurationFolders(configuration);
            LogLevelMap("initial", worldData.TerrainLevelMap);
            Debug.Log($"{LogTag} HeightMap stats: {FormatFloatMapStats(worldData.HeightMap)}.");
        }

        private string FormatOptions()
        {
            return $"replaceTerrain={_options.ReplaceMappedTerrainVisuals}, suppressMoyvaLayers={_options.SuppressMoyvaLayerDataWhenTerrainMapped}, resetConfig={_options.ResetConfigurationBeforeBuild}, syncSize={_options.SyncConfigurationSize}, useWorldSeed={_options.UseWorldSeed}, cellSizeOverride={_options.ConfigurationCellSizeOverride}, applyIntegerHeights={_options.ApplyIntegerTerrainHeights}, heightStep={_options.TerrainHeightStep}, trackingSeconds={_options.TerrainHeightTrackingSeconds}, normalizeLevels={_options.NormalizeTerrainLevelsForTileWorldCreator}, water={_options.WaterTerrainLevel}, shore={_options.ShoreTerrainLevel}, land={_options.LandTerrainLevel}, hill={_options.HillTerrainLevel}, max={_options.MaxTerrainLevel}, sideWalls={_options.GenerateTerrainSideWalls}, sideWallBorder={_options.GenerateTerrainSideWallsAtMapBorder}, sideWallMaterial='{(_options.TerrainSideWallMaterial != null ? _options.TerrainSideWallMaterial.name : "<runtime>")}', meshOptimize={_options.CombineTerrainMeshesAfterHeightProjection}, meshOptimizeClustersPerFrame={_options.TerrainMeshCombineClustersPerFrame}, meshOptimizeDeactivateObjects={_options.TerrainMeshCombineDeactivateSourceObjects}, expandShore={_options.ExpandSandShoreBand}, shoreId='{_options.ShoreBandTileId}'";
        }

        private static void LogLevelMap(string label, int[,] levelMap)
        {
            Debug.Log($"{LogTag} LevelMap {label}: {FormatLevelStats(levelMap)}.");
        }

        private static string FormatLevelStats(int[,] levelMap)
        {
            if (levelMap == null)
                return "null";

            int width = levelMap.GetLength(0);
            int height = levelMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            int min = int.MaxValue;
            int max = int.MinValue;
            var histogram = new SortedDictionary<int, int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = levelMap[x, y];
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                    histogram.TryGetValue(value, out int count);
                    histogram[value] = count + 1;
                }
            }

            var builder = new StringBuilder();
            builder.Append(width).Append('x').Append(height)
                .Append(", min=").Append(min)
                .Append(", max=").Append(max)
                .Append(", histogram=");
            AppendHistogram(builder, histogram, 16);
            AppendLevelSamples(builder, levelMap, width, height);
            return builder.ToString();
        }

        private static string FormatFloatMapStats(float[,] map)
        {
            if (map == null)
                return "null";

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            double sum = 0d;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float value = map[x, y];
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                    sum += value;
                }
            }

            double avg = sum / (width * height);
            return $"{width}x{height}, min={min:0.###}, max={max:0.###}, avg={avg:0.###}, samples=(0,0:{map[0, 0]:0.###}), (mid:{map[width / 2, height / 2]:0.###}), (last:{map[width - 1, height - 1]:0.###})";
        }

        private static void AppendHistogram(StringBuilder builder, SortedDictionary<int, int> histogram, int maxEntries)
        {
            builder.Append('{');
            int index = 0;
            foreach (var pair in histogram)
            {
                if (index > 0)
                    builder.Append(", ");
                if (index >= maxEntries)
                {
                    builder.Append("...");
                    break;
                }

                builder.Append(pair.Key).Append(':').Append(pair.Value);
                index++;
            }
            builder.Append('}');
        }

        private static void AppendLevelSamples(StringBuilder builder, int[,] levelMap, int width, int height)
        {
            builder.Append(", samples=")
                .Append("(0,0:").Append(levelMap[0, 0]).Append(')')
                .Append(", (mid:").Append(levelMap[width / 2, height / 2]).Append(')')
                .Append(", (last:").Append(levelMap[width - 1, height - 1]).Append(')');
        }

        private static string FormatMapSize(System.Array map)
        {
            if (map == null)
                return "null";
            return map.Rank == 2 ? $"{map.GetLength(0)}x{map.GetLength(1)}" : $"rank{map.Rank}";
        }

        private void LogConfigurationFolders(Configuration configuration)
        {
            int blueprintFolderCount = configuration.blueprintLayerFolders?.Count ?? 0;
            int buildFolderCount = configuration.buildLayerFolders?.Count ?? 0;
            int blueprintLayerCount = 0;
            int buildLayerCount = 0;

            if (configuration.blueprintLayerFolders != null)
            {
                for (int i = 0; i < configuration.blueprintLayerFolders.Count; i++)
                    blueprintLayerCount += configuration.blueprintLayerFolders[i]?.blueprintLayers?.Count ?? 0;
            }

            if (configuration.buildLayerFolders != null)
            {
                for (int i = 0; i < configuration.buildLayerFolders.Count; i++)
                    buildLayerCount += configuration.buildLayerFolders[i]?.buildLayers?.Count ?? 0;
            }

            Debug.Log($"{LogTag} TWC configuration layers: blueprintFolders={blueprintFolderCount}, blueprintLayers={blueprintLayerCount}, buildFolders={buildFolderCount}, buildLayers={buildLayerCount}.");
        }

        private void LogMappedLayerSummary(string label, Dictionary<string, HashSet<Vector2>> positionsByLayerGuid, HashSet<string> mappedIds, Configuration configuration)
        {
            var builder = new StringBuilder();
            builder.Append(LogTag).Append(' ')
                .Append("Mapped ").Append(label)
                .Append(": layers=").Append(positionsByLayerGuid.Count)
                .Append(", uniqueIds=").Append(mappedIds.Count)
                .Append(", ids=").Append(FormatStringSet(mappedIds, 24));

            foreach (var pair in positionsByLayerGuid)
            {
                string layerName = configuration.GetBlueprintLayerByGuid(pair.Key)?.layerName ?? "<missing blueprint>";
                builder.Append(" | ").Append(layerName)
                    .Append("(").Append(pair.Key).Append(")")
                    .Append(" count=").Append(pair.Value?.Count ?? 0)
                    .Append(" bounds=").Append(FormatPositionBounds(pair.Value));
            }

            Debug.Log(builder.ToString());
        }

        private static string FormatStringSet(HashSet<string> values, int maxEntries)
        {
            if (values == null || values.Count == 0)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            int index = 0;
            foreach (string value in values)
            {
                if (index > 0)
                    builder.Append(", ");
                if (index >= maxEntries)
                {
                    builder.Append("...");
                    break;
                }

                builder.Append(value);
                index++;
            }
            builder.Append(']');
            return builder.ToString();
        }

        private static string FormatPositionBounds(HashSet<Vector2> positions)
        {
            if (positions == null || positions.Count == 0)
                return "empty";

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            int index = 0;
            var samples = new StringBuilder();

            foreach (Vector2 position in positions)
            {
                minX = Mathf.Min(minX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxX = Mathf.Max(maxX, position.x);
                maxY = Mathf.Max(maxY, position.y);
                if (index < 6)
                {
                    if (samples.Length > 0)
                        samples.Append(", ");
                    samples.Append('(').Append(position.x).Append(',').Append(position.y).Append(')');
                }
                index++;
            }

            return $"min=({minX},{minY}), max=({maxX},{maxY}), samples=[{samples}]";
        }

        private static int CountTrue(bool[,] values, int width, int height)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (values[x, y])
                    count++;
            return count;
        }

        private static string FormatColor(Color color)
            => $"r={color.r.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, g={color.g.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, b={color.b.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, a={color.a.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}";

        private int CountComponentsInManager<T>() where T : Component
            => _manager != null ? _manager.GetComponentsInChildren<T>(true).Length : 0;

        private int CountManagerChildren()
            => _manager != null ? _manager.GetComponentsInChildren<Transform>(true).Length - 1 : 0;

        private delegate bool TryResolveLayer(string id, out TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }

    internal readonly struct TileWorldCreatorWorldBuildResult
    {
        private readonly HashSet<string> _terrainIds;
        private readonly HashSet<string> _objectIds;
        private readonly HashSet<string> _buildingIds;

        public static TileWorldCreatorWorldBuildResult Disabled => new TileWorldCreatorWorldBuildResult(
            null,
            null,
            null,
            false,
            false,
            false,
            false,
            1f,
            false,
            default);

        public TileWorldCreatorWorldBuildResult(
            HashSet<string> terrainIds,
            HashSet<string> objectIds,
            HashSet<string> buildingIds,
            bool replaceTerrainVisuals,
            bool replaceObjectVisuals,
            bool replaceBuildingVisuals,
            bool suppressMoyvaLayerData,
            float cellSize,
            bool hasBaseMapWorldBounds,
            Bounds baseMapWorldBounds)
        {
            _terrainIds = terrainIds;
            _objectIds = objectIds;
            _buildingIds = buildingIds;
            ReplaceMappedTerrainVisuals = replaceTerrainVisuals;
            ReplaceMappedObjectVisuals = replaceObjectVisuals;
            ReplaceMappedBuildingVisuals = replaceBuildingVisuals;
            SuppressMoyvaLayerData = suppressMoyvaLayerData;
            CellSize = cellSize > 0.0001f ? cellSize : 1f;
            HasBaseMapWorldBounds = hasBaseMapWorldBounds;
            BaseMapWorldBounds = baseMapWorldBounds;
        }

        public bool ReplaceMappedTerrainVisuals { get; }
        public bool ReplaceMappedObjectVisuals { get; }
        public bool ReplaceMappedBuildingVisuals { get; }
        public bool SuppressMoyvaLayerData { get; }
        public float CellSize { get; }
        public bool HasBaseMapWorldBounds { get; }
        public Bounds BaseMapWorldBounds { get; }

        public bool ShouldReplaceTerrainVisual(string id)
            => ReplaceMappedTerrainVisuals && Contains(_terrainIds, id);

        public bool ShouldReplaceObjectVisual(string id)
            => ReplaceMappedObjectVisuals && Contains(_objectIds, id);

        public bool ShouldReplaceBuildingVisual(string id)
            => ReplaceMappedBuildingVisuals && Contains(_buildingIds, id);

        private static bool Contains(HashSet<string> ids, string id)
            => ids != null && !string.IsNullOrWhiteSpace(id) && ids.Contains(id);
    }
}
