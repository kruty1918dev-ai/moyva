using System;
using System.Text;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualGridWriter : IMapVisualGridWriter
    {
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const int MaxMissingTileSamples = 8;
        private readonly IGridService _gridService;
        private readonly IMapVisualTileIdResolver _tileIds;

        public MapVisualGridWriter(IGridService gridService, IMapVisualTileIdResolver tileIds)
        {
            _gridService = gridService;
            _tileIds = tileIds;
        }

        public int Write(GeneratedWorldData worldData)
        {
            if (worldData == null)
                throw new ArgumentNullException(nameof(worldData));

            double startedAt =
                Time.realtimeSinceStartupAsDouble;

            ValidateBiomeIdentitySource(worldData);
            EnsureGridMatchesWorld(worldData);

            int count = WriteMap(
                worldData.BiomeMap,
                false);
            WriteMap(
                worldData.ObjectMap,
                true);

            ValidateWrittenTileIdentityInvariant(
                worldData.Width,
                worldData.Height,
                startedAt);
            return count;
        }

        private int WriteMap(string[,] map, bool resolveTileIds)
        {
            if (map == null)
                return 0;

            int filled = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                string id = map[x, y];
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                if (resolveTileIds && !_tileIds.TryResolve(id, out _, out id))
                    continue;

                map[x, y] = id;
                _gridService.SetTileData(new Vector2Int(x, y), id);
                filled++;
            }

            return filled;
        }

        private static void ValidateBiomeIdentitySource(
            GeneratedWorldData worldData)
        {
            int width = Mathf.Max(1, worldData.Width);
            int height = Mathf.Max(1, worldData.Height);
            string[,] biomeMap = worldData.BiomeMap;

            if (biomeMap == null)
            {
                throw new InvalidOperationException(
                    "Generated world has no BiomeMap. Every generated cell must have a TileTypeId.");
            }

            if (biomeMap.GetLength(0) != width
                || biomeMap.GetLength(1) != height)
            {
                throw new InvalidOperationException(
                    $"BiomeMap size {biomeMap.GetLength(0)}x{biomeMap.GetLength(1)} " +
                    $"does not match generated world size {width}x{height}.");
            }

            int missingCount = 0;
            StringBuilder samples = null;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!string.IsNullOrWhiteSpace(
                        biomeMap[x, y]))
                {
                    continue;
                }

                missingCount++;
                if (missingCount > MaxMissingTileSamples)
                    continue;

                samples ??= new StringBuilder(96);
                if (samples.Length > 0)
                    samples.Append(", ");
                samples.Append('(')
                    .Append(x)
                    .Append(',')
                    .Append(y)
                    .Append(')');
            }

            if (missingCount == 0)
                return;

            string sampleText =
                samples?.ToString() ?? "none";
            string message =
                $"Generated BiomeMap violates TileTypeId invariant: " +
                $"missing={missingCount}/{width * height}, " +
                $"samples={sampleText}.";

            Debug.LogError(
                $"{PerfLogTag} {message}");
            throw new InvalidOperationException(message);
        }

        private void ValidateWrittenTileIdentityInvariant(
            int width,
            int height,
            double startedAt)
        {
            int missingCount = 0;
            StringBuilder samples = null;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var position = new Vector2Int(x, y);
                if (_gridService.TryGetTileTypeId(
                        position,
                        out _))
                {
                    continue;
                }

                missingCount++;
                if (missingCount > MaxMissingTileSamples)
                    continue;

                samples ??= new StringBuilder(96);
                if (samples.Length > 0)
                    samples.Append(", ");
                samples.Append('(')
                    .Append(x)
                    .Append(',')
                    .Append(y)
                    .Append(')');
            }

            if (missingCount > 0)
            {
                string sampleText =
                    samples?.ToString() ?? "none";
                string message =
                    $"Grid write violates TileTypeId invariant: " +
                    $"missing={missingCount}/{width * height}, " +
                    $"samples={sampleText}.";

                Debug.LogError(
                    $"{PerfLogTag} {message}");
                throw new InvalidOperationException(message);
            }

            if (Debug.isDebugBuild)
            {
                double elapsedMs =
                    (Time.realtimeSinceStartupAsDouble - startedAt)
                    * 1000d;
                Debug.Log(
                    $"{PerfLogTag} tile-id invariant validated: " +
                    $"grid={width}x{height} " +
                    $"assigned={width * height} missing=0 " +
                    $"elapsedMs={elapsedMs:F3}");
            }
        }

        private void EnsureGridMatchesWorld(GeneratedWorldData worldData)
        {
            int width = Mathf.Max(1, worldData.Width);
            int height = Mathf.Max(1, worldData.Height);
            if (_gridService.GridWidth == width && _gridService.GridHeight == height)
                return;

            if (_gridService is IGridResizeService resizeService)
                resizeService.Resize(width, height);
            else
                Debug.LogWarning($"[MapVisualInstantiator] Grid size {_gridService.GridWidth}x{_gridService.GridHeight} does not match world size {width}x{height}, and the grid service cannot resize.");
        }
    }
}
