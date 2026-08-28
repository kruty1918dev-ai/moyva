using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule")]
    internal sealed class GeneratedWorldSaveModule : ISaveModule
    {
        private const int CurrentVersion = 2;

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

            context.Writer.Write(data.WorldName ?? string.Empty);
            context.Writer.Write(data.Seed);
            context.Writer.Write(data.Size);
            context.Writer.Write(data.MapType);
            context.Writer.Write(data.Difficulty);

            WriteSpawnPositions(context, data.SpawnPositions);
        }

        public void OnLoad(ISaveContext context)
        {
            int markerOrWidth = context.Reader.ReadInt32();
            GeneratedWorldData data = markerOrWidth < 0
                ? ReadVersioned(context, -markerOrWidth)
                : ReadLegacy(context, markerOrWidth);

            if (data == null)
                return;

            ReadMetadata(context, data);
            _mapVisualInstantiator.SetPendingWorldData(data);
        }

        private GeneratedWorldData ReadVersioned(ISaveContext context, int version)
        {
            if (version != CurrentVersion)
            {
                throw new InvalidDataException(
                    $"[GeneratedWorldSave] Unsupported world payload version {version}. " +
                    $"Current version is {CurrentVersion}.");
            }

            int width = context.Reader.ReadInt32();
            int height = context.Reader.ReadInt32();
            if (width <= 0 || height <= 0)
                return null;

            string savedFingerprint = context.Reader.ReadString();
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

            return new GeneratedWorldData
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
        }

        private GeneratedWorldData ReadLegacy(ISaveContext context, int width)
        {
            int height = context.Reader.ReadInt32();
            if (width <= 0 || height <= 0)
                return null;

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

        private static void ReadMetadata(ISaveContext context, GeneratedWorldData data)
        {
            if (context.Reader.BaseStream.Position >= context.Reader.BaseStream.Length)
                return;

            data.WorldName = context.Reader.ReadString();
            data.Seed = context.Reader.ReadInt32();
            data.Size = context.Reader.ReadInt32();
            data.MapType = context.Reader.ReadInt32();
            data.Difficulty = context.Reader.ReadInt32();

            if (context.Reader.BaseStream.Position < context.Reader.BaseStream.Length)
                data.SpawnPositions = ReadSpawnPositions(context);
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
                Debug.LogError(
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
                    map[x, y] = context.Reader.ReadSingle();

            return map;
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

        private static SpawnPositionAssignment[] ReadSpawnPositions(ISaveContext context)
        {
            int count = context.Reader.ReadInt32();
            if (count <= 0)
                return null;

            var assignments = new SpawnPositionAssignment[count];
            for (int i = 0; i < count; i++)
            {
                int slotIndex = context.Reader.ReadInt32();
                string participantId = context.Reader.ReadString();
                context.Reader.ReadBoolean();
                assignments[i] = new SpawnPositionAssignment
                {
                    SlotIndex = slotIndex,
                    ParticipantId = participantId,
                    Position = new UnityEngine.Vector2Int(context.Reader.ReadInt32(), context.Reader.ReadInt32()),
                };
            }

            return assignments;
        }
    }
}
