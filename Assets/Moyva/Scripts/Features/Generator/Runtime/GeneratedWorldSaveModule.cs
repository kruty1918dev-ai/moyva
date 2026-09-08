using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule")]
    internal sealed class GeneratedWorldSaveModule : IStagedSaveModule
    {
        private const int CurrentVersion = 4;
        private const int MaxCompiledLayers = 1024;
        private const int MaxSamplesPerCell = 64;

        private readonly MapVisualInstantiator _mapVisualInstantiator;
        private readonly ITileTypeRepository _tileTypes;

        public GeneratedWorldSaveModule(
            MapVisualInstantiator mapVisualInstantiator,
            [InjectOptional] ITileTypeRepository tileTypes = null)
        {
            _mapVisualInstantiator = mapVisualInstantiator;
            _tileTypes = tileTypes;
        }

        public void OnSave(ISaveContext context)
        {
            if (!_mapVisualInstantiator.TryGetCurrentWorldData(out var data) || data == null)
                return;

            context.Writer.Write(-CurrentVersion);
            context.Writer.Write(data.Width);
            context.Writer.Write(data.Height);
            context.Writer.Write(MoyvaJsonRuntime.ConfigFingerprint ?? string.Empty);

            string[,] gameplayTiles = CanonicalizeGameplayMap(
                data.GameplayTileMap ?? data.BiomeMap,
                data.Width,
                data.Height,
                "save");
            string[,] visualTiles = data.VisualTileMap ?? data.BiomeMap ?? gameplayTiles;

            WriteStringMap(context, gameplayTiles, data.Width, data.Height);
            WriteStringMap(context, visualTiles, data.Width, data.Height);
            WriteStringMap(context, data.ObjectMap, data.Width, data.Height);
            WriteFloatMap(context, data.HeightMap, data.Width, data.Height);
            WriteStringMap(context, data.BuildingMap, data.Width, data.Height);
            WriteIntMap(context, data.TerrainLevelMap, data.Width, data.Height);
            WriteWorldSettings(context, data);
            WriteCompiledLayers(context, data.CompiledLayers);
            WriteLogicalTileMap(context, data.LogicalTileMap, data.Width, data.Height);

            context.Writer.Write(data.WorldName ?? string.Empty);
            context.Writer.Write(data.Seed);
            context.Writer.Write(data.Size);
            context.Writer.Write(data.MapType);
            context.Writer.Write(data.Difficulty);

            WriteSpawnPositions(context, data.SpawnPositions);
        }

        public void OnLoad(ISaveContext context)
            => PrepareLoad(context)();

        public Action PrepareMissingData() => () => { };

        public Action PrepareLoad(ISaveContext context)
        {
            int markerOrWidth = context.Reader.ReadInt32();
            GeneratedWorldData data = markerOrWidth < 0
                ? ReadVersioned(context, -markerOrWidth)
                : ReadLegacy(context, markerOrWidth);

            ReadMetadata(context, data, markerOrWidth < 0 ? -markerOrWidth : 0);
            if (context.Reader.BaseStream.Position != context.Reader.BaseStream.Length)
                throw new InvalidDataException("Unexpected trailing world-state data.");
            return () =>
            {
                _mapVisualInstantiator.SetPendingWorldData(data);
                // Construction and Units restore after this module and require the populated grid.
                _mapVisualInstantiator.BuildWorld();
            };
        }

        private GeneratedWorldData ReadVersioned(ISaveContext context, int version)
        {
            if (version != 2 && version != 3 && version != CurrentVersion)
            {
                throw new InvalidDataException(
                    $"[GeneratedWorldSave] Unsupported world payload version {version}. " +
                    $"Current version is {CurrentVersion}.");
            }

            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();
            string savedFingerprint = context.Reader.ReadString();
            ValidateDimensions(context.Reader, width, height, 8);
            string localFingerprint = MoyvaJsonRuntime.ConfigFingerprint ?? string.Empty;
            if (!string.IsNullOrEmpty(savedFingerprint)
                && !string.Equals(savedFingerprint, localFingerprint, StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    "[GeneratedWorldSave] The save was created with a different gameplay " +
                    $"configuration fingerprint. Save={savedFingerprint}, Local={localFingerprint}.");
            }

            string[,] gameplayTiles = CanonicalizeGameplayMap(
                ReadStringMap(context, width, height),
                width,
                height,
                "versioned load");
            string[,] visualTiles = ReadStringMap(context, width, height);

            var data = new GeneratedWorldData
            {
                Width = width,
                Height = height,
                GameplayTileMap = gameplayTiles,
                VisualTileMap = visualTiles,
                BiomeMap = MapArrayUtils.CloneStringMap(visualTiles),
                ObjectMap = ReadStringMap(context, width, height),
                HeightMap = ReadFloatMap(context, width, height),
                BuildingMap = ReadStringMap(context, width, height),
            };

            if (version >= 3)
            {
                data.TerrainLevelMap = ReadIntMap(context, width, height);
                ReadWorldSettings(context, data);
                data.CompiledLayers = ReadCompiledLayers(context);
                data.LogicalTileMap = version >= 4
                    ? ReadLogicalTileMap(context, width, height)
                    : ReadLogicalTileMapV3(context, width, height);
            }

            return data;
        }

        private GeneratedWorldData ReadLegacy(ISaveContext context, int width)
        {
            int height = context.Reader.ReadInt32();
            ValidateDimensions(context.Reader, width, height, 7);

            string[,] legacyBiomeMap = ReadStringMap(context, width, height);
            return new GeneratedWorldData
            {
                Width = width,
                Height = height,
                GameplayTileMap = CanonicalizeGameplayMap(
                    legacyBiomeMap,
                    width,
                    height,
                    "legacy load"),
                VisualTileMap = MapArrayUtils.CloneStringMap(legacyBiomeMap),
                BiomeMap = legacyBiomeMap,
                ObjectMap = ReadStringMap(context, width, height),
                HeightMap = ReadFloatMap(context, width, height),
                BuildingMap = ReadStringMap(context, width, height),
            };
        }

        private static void ReadMetadata(ISaveContext context, GeneratedWorldData data, int version)
        {
            if (context.Reader.BaseStream.Position >= context.Reader.BaseStream.Length)
                return;

            data.WorldName = context.Reader.ReadString();
            data.Seed = context.Reader.ReadInt32();
            data.Size = context.Reader.ReadInt32();
            data.MapType = context.Reader.ReadInt32();
            data.Difficulty = context.Reader.ReadInt32();

            if (context.Reader.BaseStream.Position < context.Reader.BaseStream.Length)
                data.SpawnPositions = ReadSpawnPositions(context, data.Width, data.Height, version == 2);
        }

        private static void ValidateDimensions(BinaryReader reader, int width, int height, int minimumBytesPerCell)
        {
            if (width <= 0 || height <= 0
                || (long)width * height > (reader.BaseStream.Length - reader.BaseStream.Position) / minimumBytesPerCell)
                throw new InvalidDataException($"Invalid or truncated world dimensions: {width}x{height}.");
        }

        private string[,] CanonicalizeGameplayMap(
            string[,] source,
            int width,
            int height,
            string operation)
        {
            var result = new string[width, height];
            HashSet<string> unresolved = null;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string tileId = source?[x, y] ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(tileId) || _tileTypes == null)
                    {
                        result[x, y] = tileId;
                        continue;
                    }

                    if (_tileTypes.TryResolveId(tileId, out string canonicalId))
                    {
                        result[x, y] = canonicalId;
                        continue;
                    }

                    result[x, y] = tileId;
                    unresolved ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    unresolved.Add(tileId);
                }
            }

            if (unresolved != null)
            {
                throw new InvalidDataException(
                    $"[GeneratedWorldSave] Unknown semantic tile IDs during {operation}: " +
                    string.Join(", ", unresolved) + ". Add exact aliases to tile-type JSON files.");
            }

            return result;
        }

        private static void WriteStringMap(ISaveContext context, string[,] map, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(map?[x, y] ?? string.Empty);
        }

        private static string[,] ReadStringMap(ISaveContext context, int width, int height)
        {
            var map = new string[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = context.Reader.ReadString();

            return map;
        }

        private static void WriteFloatMap(ISaveContext context, float[,] map, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(map?[x, y] ?? 0f);
        }

        private static float[,] ReadFloatMap(ISaveContext context, int width, int height)
        {
            var map = new float[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    float value = context.Reader.ReadSingle();
                    if (float.IsNaN(value) || float.IsInfinity(value))
                        throw new InvalidDataException($"Non-finite terrain height at ({x},{y}).");
                    map[x, y] = value;
                }

            return map;
        }

        private static void WriteIntMap(ISaveContext context, int[,] map, int width, int height)
        {
            bool hasMap = HasSameSize(map, width, height);
            context.Writer.Write(hasMap);
            if (!hasMap)
                return;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    context.Writer.Write(map[x, y]);
        }

        private static int[,] ReadIntMap(ISaveContext context, int width, int height)
        {
            if (!context.Reader.ReadBoolean())
                return null;

            var map = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = context.Reader.ReadInt32();

            return map;
        }

        private static void WriteWorldSettings(ISaveContext context, GeneratedWorldData data)
        {
            context.Writer.Write((int)data.GridTopology);
            context.Writer.Write((int)data.ProjectionMode);
            context.Writer.Write((int)data.RenderMode);
            context.Writer.Write((int)data.NeighborhoodMode);
            context.Writer.Write(data.ForceChunkFirstCompositeBuild);
            context.Writer.Write(data.CellSize);
            context.Writer.Write(data.HasBaseMapWorldBounds);
            if (!data.HasBaseMapWorldBounds)
                return;

            WriteVector3(context, data.BaseMapWorldBounds.center);
            WriteVector3(context, data.BaseMapWorldBounds.size);
        }

        private static void ReadWorldSettings(ISaveContext context, GeneratedWorldData data)
        {
            data.GridTopology = ReadEnum<GridTopology>(context, nameof(data.GridTopology));
            data.ProjectionMode = ReadEnum<GridProjectionMode>(context, nameof(data.ProjectionMode));
            data.RenderMode = ReadEnum<GridRenderMode>(context, nameof(data.RenderMode));
            data.NeighborhoodMode = ReadEnum<GridNeighborhoodMode>(context, nameof(data.NeighborhoodMode));
            data.ForceChunkFirstCompositeBuild = context.Reader.ReadBoolean();
            data.CellSize = ReadFiniteFloat(context, nameof(data.CellSize));
            if (data.CellSize <= 0f)
                throw new InvalidDataException("Invalid saved world cell size.");

            data.HasBaseMapWorldBounds = context.Reader.ReadBoolean();
            if (data.HasBaseMapWorldBounds)
                data.BaseMapWorldBounds = new Bounds(
                    ReadVector3(context, "world bounds center"),
                    ReadVector3(context, "world bounds size"));
        }

        private static void WriteCompiledLayers(ISaveContext context, IReadOnlyList<CompiledLayerMap> layers)
        {
            int count = layers?.Count ?? 0;
            context.Writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                CompiledLayerMap layer = layers[i] ?? new CompiledLayerMap();
                context.Writer.Write(layer.GraphLayerId ?? string.Empty);
                context.Writer.Write(layer.GridTileId ?? string.Empty);
                context.Writer.Write(layer.BlueprintLayerGuid ?? string.Empty);
                context.Writer.Write(layer.LayerName ?? string.Empty);
                context.Writer.Write(layer.SortingOrder);
                context.Writer.Write(layer.GraphLayerOrder);
                context.Writer.Write(layer.TerrainPriority);
                context.Writer.Write(layer.BuildLayerGuid ?? string.Empty);
                context.Writer.Write(layer.PresetId ?? string.Empty);
                context.Writer.Write(layer.SourceNodeId ?? string.Empty);
                context.Writer.Write(layer.HasRenderableTileOutput);
            }
        }

        private static IReadOnlyList<CompiledLayerMap> ReadCompiledLayers(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();
            if (count < 0 || count > MaxCompiledLayers)
                throw new InvalidDataException("Invalid saved compiled layer count.");

            var layers = new List<CompiledLayerMap>(count);
            for (int i = 0; i < count; i++)
            {
                layers.Add(new CompiledLayerMap
                {
                    GraphLayerId = context.Reader.ReadString(),
                    GridTileId = context.Reader.ReadString(),
                    BlueprintLayerGuid = context.Reader.ReadString(),
                    LayerName = context.Reader.ReadString(),
                    SortingOrder = context.Reader.ReadInt32(),
                    GraphLayerOrder = context.Reader.ReadInt32(),
                    TerrainPriority = context.Reader.ReadInt32(),
                    BuildLayerGuid = context.Reader.ReadString(),
                    PresetId = context.Reader.ReadString(),
                    SourceNodeId = context.Reader.ReadString(),
                    HasRenderableTileOutput = context.Reader.ReadBoolean(),
                });
            }

            return layers;
        }

        private static void WriteLogicalTileMap(ISaveContext context, GraphLogicalTileMap map, int width, int height)
        {
            bool hasMap = map != null && map.Width == width && map.Height == height;
            context.Writer.Write(hasMap);
            if (!hasMap)
                return;

            context.Writer.Write(map.Width);
            context.Writer.Write(map.Height);
            List<GraphTileLayerSample> uniqueSamples = CollectLogicalSamples(map, width, height);
            context.Writer.Write(uniqueSamples.Count);
            for (int i = 0; i < uniqueSamples.Count; i++)
                WriteSample(context, uniqueSamples[i]);

            var sampleIndex = new Dictionary<GraphTileLayerSample, int>(uniqueSamples.Count);
            for (int i = 0; i < uniqueSamples.Count; i++)
                sampleIndex[uniqueSamples[i]] = i;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    IReadOnlyList<GraphTileLayerSample> samples = map.GetCellStack(x, y)?.Samples;
                    int count = samples?.Count ?? 0;
                    context.Writer.Write(count);
                    for (int i = 0; i < count; i++)
                        context.Writer.Write(sampleIndex[samples[i]]);
                }
        }

        private static GraphLogicalTileMap ReadLogicalTileMap(ISaveContext context, int width, int height)
        {
            if (!context.Reader.ReadBoolean())
                return null;

            int savedWidth = context.Reader.ReadInt32();
            int savedHeight = context.Reader.ReadInt32();
            if (savedWidth != width || savedHeight != height)
                throw new InvalidDataException("Saved logical tile map dimensions do not match world dimensions.");

            int uniqueCount = context.Reader.ReadInt32();
            long maxUniqueCount = (long)width * height * MaxSamplesPerCell;
            if (uniqueCount < 0 || uniqueCount > maxUniqueCount)
                throw new InvalidDataException("Invalid saved logical tile sample dictionary size.");

            var uniqueSamples = new GraphTileLayerSample[uniqueCount];
            for (int i = 0; i < uniqueCount; i++)
                uniqueSamples[i] = ReadSample(context);

            var map = new GraphLogicalTileMap(width, height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int count = context.Reader.ReadInt32();
                    if (count < 0 || count > MaxSamplesPerCell)
                        throw new InvalidDataException($"Invalid logical tile sample count at ({x},{y}).");

                    for (int i = 0; i < count; i++)
                    {
                        int sampleIndex = context.Reader.ReadInt32();
                        if (sampleIndex < 0 || sampleIndex >= uniqueSamples.Length)
                            throw new InvalidDataException($"Invalid logical tile sample index at ({x},{y}).");
                        map.AddSample(x, y, uniqueSamples[sampleIndex]);
                    }
                }

            return map;
        }

        private static GraphLogicalTileMap ReadLogicalTileMapV3(ISaveContext context, int width, int height)
        {
            if (!context.Reader.ReadBoolean())
                return null;

            int savedWidth = context.Reader.ReadInt32();
            int savedHeight = context.Reader.ReadInt32();
            if (savedWidth != width || savedHeight != height)
                throw new InvalidDataException("Saved logical tile map dimensions do not match world dimensions.");

            var map = new GraphLogicalTileMap(width, height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int count = context.Reader.ReadInt32();
                    if (count < 0 || count > MaxSamplesPerCell)
                        throw new InvalidDataException($"Invalid logical tile sample count at ({x},{y}).");

                    for (int i = 0; i < count; i++)
                        map.AddSample(x, y, ReadSample(context));
                }

            return map;
        }

        private static List<GraphTileLayerSample> CollectLogicalSamples(
            GraphLogicalTileMap map,
            int width,
            int height)
        {
            var samples = new List<GraphTileLayerSample>();
            var seen = new Dictionary<GraphTileLayerSample, int>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    IReadOnlyList<GraphTileLayerSample> stack = map.GetCellStack(x, y)?.Samples;
                    if (stack == null)
                        continue;

                    for (int i = 0; i < stack.Count; i++)
                    {
                        if (seen.ContainsKey(stack[i]))
                            continue;

                        seen.Add(stack[i], samples.Count);
                        samples.Add(stack[i]);
                    }
                }

            return samples;
        }

        private static void WriteSample(ISaveContext context, GraphTileLayerSample sample)
        {
            context.Writer.Write(sample.GraphLayerId ?? string.Empty);
            context.Writer.Write(sample.GraphLayerName ?? string.Empty);
            context.Writer.Write(sample.BlueprintLayerGuid ?? string.Empty);
            context.Writer.Write(sample.BuildLayerGuid ?? string.Empty);
            context.Writer.Write(sample.TileId ?? string.Empty);
            context.Writer.Write(sample.PresetId ?? string.Empty);
            context.Writer.Write((int)sample.LayerKind);
            context.Writer.Write(sample.SortingOrder);
            context.Writer.Write(sample.GraphLayerOrder);
            context.Writer.Write(sample.TerrainPriority);
            context.Writer.Write(sample.Height);
            context.Writer.Write(sample.SurfaceHeight);
            context.Writer.Write(sample.SourceNodeId ?? string.Empty);
            context.Writer.Write((int)sample.TileGeometryMode);
            context.Writer.Write((int)sample.AuthoredClosurePolicy);
        }

        private static GraphTileLayerSample ReadSample(ISaveContext context)
        {
            string graphLayerId = context.Reader.ReadString();
            string graphLayerName = context.Reader.ReadString();
            string blueprintLayerGuid = context.Reader.ReadString();
            string buildLayerGuid = context.Reader.ReadString();
            string tileId = context.Reader.ReadString();
            string presetId = context.Reader.ReadString();
            LayerKind layerKind = ReadEnum<LayerKind>(context, nameof(GraphTileLayerSample.LayerKind));
            int sortingOrder = context.Reader.ReadInt32();
            int graphLayerOrder = context.Reader.ReadInt32();
            int terrainPriority = context.Reader.ReadInt32();
            float height = ReadFiniteFloat(context, nameof(GraphTileLayerSample.Height));
            float surfaceHeight = ReadFiniteFloat(context, nameof(GraphTileLayerSample.SurfaceHeight));
            string sourceNodeId = context.Reader.ReadString();
            TileGeometryMode tileGeometryMode =
                ReadEnum<TileGeometryMode>(context, nameof(GraphTileLayerSample.TileGeometryMode));
            AuthoredClosurePolicy authoredClosurePolicy =
                ReadEnum<AuthoredClosurePolicy>(context, nameof(GraphTileLayerSample.AuthoredClosurePolicy));

            return new GraphTileLayerSample(
                graphLayerId,
                graphLayerName,
                blueprintLayerGuid,
                buildLayerGuid,
                tileId,
                presetId,
                layerKind,
                sortingOrder,
                graphLayerOrder,
                terrainPriority,
                height,
                surfaceHeight,
                sourceNodeId,
                tileGeometryMode,
                authoredClosurePolicy);
        }

        private static void WriteSpawnPositions(ISaveContext context, SpawnPositionAssignment[] assignments)
        {
            int count = assignments?.Length ?? 0;
            context.Writer.Write(count);
            for (int i = 0; i < count; i++)
            {
                context.Writer.Write(assignments[i].SlotIndex);
                context.Writer.Write(assignments[i].ParticipantId ?? string.Empty);
                context.Writer.Write(false);
                context.Writer.Write(assignments[i].Position.x);
                context.Writer.Write(assignments[i].Position.y);
            }
        }

        private static SpawnPositionAssignment[] ReadSpawnPositions(
            ISaveContext context,
            int width,
            int height,
            bool readLegacyDuplicateX)
        {
            int count = context.Reader.ReadInt32();
            if (count < 0 || count > 16)
                throw new InvalidDataException("Invalid saved spawn count.");
            if (count == 0)
                return null;

            var slots = new HashSet<int>();
            var owners = new HashSet<string>(StringComparer.Ordinal);
            var assignments = new SpawnPositionAssignment[count];
            for (int i = 0; i < count; i++)
            {
                int slotIndex = context.Reader.ReadInt32();
                string participantId = context.Reader.ReadString();
                context.Reader.ReadBoolean();
                int x = context.Reader.ReadInt32();
                if (readLegacyDuplicateX)
                    context.Reader.ReadInt32();
                var position = new Vector2Int(x, context.Reader.ReadInt32());
                if (slotIndex < 0 || !slots.Add(slotIndex)
                    || (!string.IsNullOrWhiteSpace(participantId) && !owners.Add(participantId))
                    || position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
                    throw new InvalidDataException("Invalid or duplicate saved spawn assignment.");
                assignments[i] = new SpawnPositionAssignment
                {
                    SlotIndex = slotIndex,
                    ParticipantId = participantId,
                    Position = position,
                };
            }

            return assignments;
        }

        private static void WriteVector3(ISaveContext context, Vector3 value)
        {
            context.Writer.Write(value.x);
            context.Writer.Write(value.y);
            context.Writer.Write(value.z);
        }

        private static Vector3 ReadVector3(ISaveContext context, string label)
            => new Vector3(
                ReadFiniteFloat(context, $"{label}.x"),
                ReadFiniteFloat(context, $"{label}.y"),
                ReadFiniteFloat(context, $"{label}.z"));

        private static TEnum ReadEnum<TEnum>(ISaveContext context, string label) where TEnum : struct, Enum
        {
            int value = context.Reader.ReadInt32();
            if (!Enum.IsDefined(typeof(TEnum), value))
                throw new InvalidDataException($"Invalid saved enum value for {label}: {value}.");
            return (TEnum)(object)value;
        }

        private static float ReadFiniteFloat(ISaveContext context, string label)
        {
            float value = context.Reader.ReadSingle();
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new InvalidDataException($"Non-finite saved float value for {label}.");
            return value;
        }

        private static bool HasSameSize(Array map, int width, int height)
            => map != null && map.Rank == 2 && map.GetLength(0) == width && map.GetLength(1) == height;
    }
}
