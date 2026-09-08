using Kruty1918.Moyva.SaveSystem;
using System;
using System.Text;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldBuildOrchestrator : IMapVisualWorldBuildOrchestrator
    {
        private readonly IMapVisualWorldState _state;
        private readonly IMapVisualWorldDataFactory _dataFactory;
        private readonly IGeneratedWorldDataIntegrityService _integrity;
        private readonly IMapVisualGridWriter _gridWriter;
        private readonly IMapVisualWorldSignalPublisher _signals;
        private readonly ITileWorldCreatorWorldBuildBridge _tileWorldCreatorBridge;
        private readonly MapVisualFallbackPresenter _fallbackPresenter;

        public MapVisualWorldBuildOrchestrator(
            IMapVisualWorldState state,
            IMapVisualWorldDataFactory dataFactory,
            IGeneratedWorldDataIntegrityService integrity,
            IMapVisualGridWriter gridWriter,
            IMapVisualWorldSignalPublisher signals,
            [InjectOptional] ITileWorldCreatorWorldBuildBridge tileWorldCreatorBridge = null,
            [InjectOptional] MapVisualFallbackPresenter fallbackPresenter = null)
        {
            _state = state;
            _dataFactory = dataFactory;
            _integrity = integrity;
            _gridWriter = gridWriter;
            _signals = signals;
            _tileWorldCreatorBridge = tileWorldCreatorBridge;
            _fallbackPresenter = fallbackPresenter;
        }

        public void BuildWorld()
        {
            bool hasPendingWorld = _state.HasPendingWorldData;
            if (GameLaunchContext.Mode == GameLaunchMode.MenuLoadGame && !hasPendingWorld)
                throw new GeneratedWorldDataIntegrityException(
                    $"Cannot continue slot {GameLaunchContext.SaveSlot:D2}: no saved world was restored. New generation is disabled for a load request.");
            string source = ResolveSource(hasPendingWorld);

            GeneratedWorldData worldData = _state.TryConsumePendingWorldData(out var pending)
                ? pending
                : _dataFactory.Generate();
            worldData = _integrity != null
                ? _integrity.EnsureReadyForBuild(worldData, source)
                : worldData;

            if (worldData == null)
                throw new GeneratedWorldDataIntegrityException(
                    "[GeneratedWorldIntegrity] Cannot build generated world: world data is null and no integrity service is bound.");

            TileWorldCreatorWorldBuildResult visualBuildResult =
                _tileWorldCreatorBridge?.Build(worldData)
                ?? TileWorldCreatorWorldBuildResult.Disabled;
            if (visualBuildResult.Succeeded)
            {
                _fallbackPresenter?.Clear();
                ApplyVisualBounds(worldData, visualBuildResult);
            }
            else
            {
                TileWorldCreatorWorldBuildResult fallbackResult =
                    _fallbackPresenter?.Present(worldData)
                    ?? TileWorldCreatorWorldBuildResult.Disabled;
                if (fallbackResult.Succeeded)
                {
                    ApplyVisualBounds(worldData, fallbackResult);
                }
                else
                {
                    string reason =
                        "[MapVisualInstantiator] Generated world data is available, " +
                        "but neither TileWorldCreator nor the fallback terrain presenter produced a visible map.";
                    Debug.LogError(reason);
                    throw new GeneratedWorldDataIntegrityException(reason);
                }
            }

            _gridWriter.Write(worldData);
            _state.SetCurrentWorldData(worldData);
            _signals.Publish(worldData, source);
        }

        private static void ApplyVisualBounds(
            GeneratedWorldData worldData,
            TileWorldCreatorWorldBuildResult result)
        {
            if (worldData == null || !result.HasBaseMapWorldBounds)
                return;

            worldData.HasBaseMapWorldBounds = true;
            worldData.BaseMapWorldBounds = result.BaseMapWorldBounds;
            worldData.CellSize = result.CellSize;
        }

        private static string ResolveSource(bool hasPendingWorld)
        {
            if (hasPendingWorld)
                return "pending-save";
            return GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest ? "direct-test" : "new";
        }
    }

    internal interface IGeneratedWorldDataIntegrityService
    {
        GeneratedWorldData EnsureReadyForBuild(GeneratedWorldData data, string source);
    }

    internal sealed class GeneratedWorldDataIntegrityException : InvalidOperationException
    {
        public GeneratedWorldDataIntegrityException(string message)
            : base(message)
        {
        }
    }

    internal static class GeneratedWorldDataDefaults
    {
        internal const string FallbackTileId = "grass";
        internal const float FallbackLandHeight = 0.5f;
    }

    internal sealed class GeneratedWorldDataIntegrityService : IGeneratedWorldDataIntegrityService
    {
        private const string LogTag = "[GeneratedWorldIntegrity]";
        private const int MaxRepairSamples = 8;

        private readonly IMapVisualTileIdResolver _tileIds;
        private readonly ITileTypeRepository _tileTypes;

        public GeneratedWorldDataIntegrityService(
            IMapVisualTileIdResolver tileIds,
            ITileTypeRepository tileTypes = null)
        {
            _tileIds = tileIds;
            _tileTypes = tileTypes;
        }

        public GeneratedWorldData EnsureReadyForBuild(GeneratedWorldData data, string source)
        {
            if (data == null)
                throw Fail(source, "world generation returned null data");

            if (!TryResolveDimensions(data, out int width, out int height, out string dimensionReason))
                throw Fail(source, dimensionReason);

            data.Width = width;
            data.Height = height;

            var repairs = new StringBuilder(256);
            string[,] sourceTiles = data.GameplayTileMap ?? data.BiomeMap ?? data.VisualTileMap;

            data.BiomeMap = RepairVisualMap(data.BiomeMap ?? sourceTiles, width, height, nameof(data.BiomeMap), repairs);
            data.VisualTileMap = RepairVisualMap(data.VisualTileMap ?? data.BiomeMap, width, height, nameof(data.VisualTileMap), repairs);
            data.GameplayTileMap = RepairGameplayMap(data.GameplayTileMap ?? data.BiomeMap, width, height, nameof(data.GameplayTileMap), repairs);
            data.ObjectMap = RepairStringMap(data.ObjectMap, width, height, nameof(data.ObjectMap), fillMissingCells: false, repairs);
            data.BuildingMap = RepairStringMap(data.BuildingMap, width, height, nameof(data.BuildingMap), fillMissingCells: false, repairs);
            data.HeightMap = RepairHeightMap(data.HeightMap, width, height, repairs);

            EnsureStartableLand(data, repairs);

            int filledTiles = CountFilled(data.GameplayTileMap, width, height);
            if (filledTiles <= 0)
                throw Fail(source, "world has no usable gameplay tiles after repair");

            if (repairs.Length > 0)
            {
                Debug.LogWarning(
                    $"{LogTag} Repaired generated world before build. " +
                    $"source='{source}', size={width}x{height}, filledTiles={filledTiles}/{width * height}. " +
                    $"repairs={repairs}");
            }
            else
            {
                Debug.Log(
                    $"{LogTag} World data accepted. source='{source}', " +
                    $"size={width}x{height}, filledTiles={filledTiles}/{width * height}.");
            }

            return data;
        }

        private string[,] RepairGameplayMap(string[,] source, int width, int height, string mapName, StringBuilder repairs)
        {
            var map = RepairStringMap(source, width, height, mapName, fillMissingCells: true, repairs);
            int fallbackCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string resolved = ResolveGameplayTileId(map[x, y], out bool usedFallback);
                if (usedFallback)
                    fallbackCount++;
                map[x, y] = resolved;
            }

            AppendRepair(repairs, fallbackCount, $"{mapName} unresolved ids -> {GeneratedWorldDataDefaults.FallbackTileId}");
            return map;
        }

        private string[,] RepairVisualMap(string[,] source, int width, int height, string mapName, StringBuilder repairs)
        {
            var map = RepairStringMap(source, width, height, mapName, fillMissingCells: true, repairs);
            int fallbackCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string resolved = ResolveVisualTileId(map[x, y], out bool usedFallback);
                if (usedFallback)
                    fallbackCount++;
                map[x, y] = resolved;
            }

            AppendRepair(repairs, fallbackCount, $"{mapName} unresolved visual ids -> {GeneratedWorldDataDefaults.FallbackTileId}");
            return map;
        }

        private static string[,] RepairStringMap(
            string[,] source,
            int width,
            int height,
            string mapName,
            bool fillMissingCells,
            StringBuilder repairs)
        {
            bool resized = source == null || source.GetLength(0) != width || source.GetLength(1) != height;
            var result = resized ? new string[width, height] : source;
            int copiedWidth = source != null ? Mathf.Min(width, source.GetLength(0)) : 0;
            int copiedHeight = source != null ? Mathf.Min(height, source.GetLength(1)) : 0;
            int missingCount = 0;
            StringBuilder samples = null;

            if (resized)
            {
                for (int x = 0; x < copiedWidth; x++)
                for (int y = 0; y < copiedHeight; y++)
                    result[x, y] = source[x, y];

                AppendRepair(repairs, 1, $"{mapName} resized/created to {width}x{height}");
            }

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!string.IsNullOrWhiteSpace(result[x, y]))
                    continue;

                missingCount++;
                if (fillMissingCells)
                    result[x, y] = GeneratedWorldDataDefaults.FallbackTileId;

                if (missingCount > MaxRepairSamples)
                    continue;

                samples ??= new StringBuilder(96);
                if (samples.Length > 0)
                    samples.Append(", ");
                samples.Append('(').Append(x).Append(',').Append(y).Append(')');
            }

            if (fillMissingCells && missingCount > 0)
            {
                AppendRepair(
                    repairs,
                    missingCount,
                    $"{mapName} blank cells filled with {GeneratedWorldDataDefaults.FallbackTileId}; samples={samples}");
            }

            return result;
        }

        private static float[,] RepairHeightMap(float[,] source, int width, int height, StringBuilder repairs)
        {
            bool resized = source == null || source.GetLength(0) != width || source.GetLength(1) != height;
            var result = resized ? new float[width, height] : source;
            int copiedWidth = source != null ? Mathf.Min(width, source.GetLength(0)) : 0;
            int copiedHeight = source != null ? Mathf.Min(height, source.GetLength(1)) : 0;

            if (resized)
            {
                for (int x = 0; x < copiedWidth; x++)
                for (int y = 0; y < copiedHeight; y++)
                    result[x, y] = source[x, y];

                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (x >= copiedWidth || y >= copiedHeight)
                        result[x, y] = GeneratedWorldDataDefaults.FallbackLandHeight;
                }

                AppendRepair(repairs, 1, $"HeightMap resized/created to {width}x{height}");
            }

            return result;
        }

        private void EnsureStartableLand(GeneratedWorldData data, StringBuilder repairs)
        {
            if (HasStartableLand(data))
                return;

            int width = data.Width;
            int height = data.Height;
            int centerX = Mathf.Clamp(width / 2, 0, width - 1);
            int centerY = Mathf.Clamp(height / 2, 0, height - 1);
            int radius = Mathf.Clamp(Mathf.Min(width, height) / 12, 1, 4);
            int patched = 0;

            for (int x = Mathf.Max(0, centerX - radius); x <= Mathf.Min(width - 1, centerX + radius); x++)
            for (int y = Mathf.Max(0, centerY - radius); y <= Mathf.Min(height - 1, centerY + radius); y++)
            {
                data.GameplayTileMap[x, y] = GeneratedWorldDataDefaults.FallbackTileId;
                data.BiomeMap[x, y] = GeneratedWorldDataDefaults.FallbackTileId;
                data.VisualTileMap[x, y] = GeneratedWorldDataDefaults.FallbackTileId;
                data.HeightMap[x, y] = GeneratedWorldDataDefaults.FallbackLandHeight;
                patched++;
            }

            AppendRepair(
                repairs,
                patched,
                $"no startable land found; stamped {patched} fallback land cells around ({centerX},{centerY})");
        }

        private bool HasStartableLand(GeneratedWorldData data)
        {
            for (int x = 0; x < data.Width; x++)
            for (int y = 0; y < data.Height; y++)
            {
                string tileId = data.GameplayTileMap[x, y];
                if (string.IsNullOrWhiteSpace(tileId) || IsWaterTileId(tileId))
                    continue;

                if (data.HeightMap != null
                    && data.HeightMap.GetLength(0) == data.Width
                    && data.HeightMap.GetLength(1) == data.Height
                    && data.HeightMap[x, y] < 0.35f)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private string ResolveGameplayTileId(string tileId, out bool usedFallback)
        {
            usedFallback = false;
            tileId = Normalize(tileId);

            if (_tileTypes != null && _tileTypes.TryResolveId(tileId, out string canonicalId))
                return canonicalId;

            if (_tileIds != null
                && _tileIds.TryResolve(tileId, out _, out string visualId)
                && _tileTypes != null
                && _tileTypes.TryResolveId(visualId, out canonicalId))
            {
                return canonicalId;
            }

            if (TryInferGameplayTileId(tileId, out string inferredId))
            {
                if (_tileTypes == null)
                    return inferredId;

                if (_tileTypes.TryResolveId(inferredId, out canonicalId))
                    return canonicalId;
            }

            usedFallback = true;
            return GeneratedWorldDataDefaults.FallbackTileId;
        }

        private string ResolveVisualTileId(string tileId, out bool usedFallback)
        {
            usedFallback = false;
            tileId = Normalize(tileId);

            if (_tileIds != null && _tileIds.TryResolve(tileId, out _, out string resolvedTileId))
                return resolvedTileId;

            if (TryInferGameplayTileId(tileId, out string inferredId))
                return inferredId;

            usedFallback = true;
            return GeneratedWorldDataDefaults.FallbackTileId;
        }

        private static bool TryResolveDimensions(GeneratedWorldData data, out int width, out int height, out string reason)
        {
            width = data.Width;
            height = data.Height;

            if (width <= 0 || height <= 0)
            {
                if (TryUseDimensions(data.GameplayTileMap, out width, out height)
                    || TryUseDimensions(data.BiomeMap, out width, out height)
                    || TryUseDimensions(data.VisualTileMap, out width, out height)
                    || TryUseDimensions(data.ObjectMap, out width, out height)
                    || TryUseDimensions(data.BuildingMap, out width, out height)
                    || TryUseDimensions(data.HeightMap, out width, out height)
                    || TryUseDimensions(data.TerrainLevelMap, out width, out height))
                {
                    reason = string.Empty;
                    return true;
                }
            }

            if (width <= 0 || height <= 0)
            {
                reason = $"world dimensions are invalid: {data.Width}x{data.Height}";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static bool TryUseDimensions(Array map, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (map == null || map.Rank != 2 || map.GetLength(0) <= 0 || map.GetLength(1) <= 0)
                return false;

            width = map.GetLength(0);
            height = map.GetLength(1);
            return true;
        }

        private static int CountFilled(string[,] map, int width, int height)
        {
            if (map == null)
                return 0;

            int count = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!string.IsNullOrWhiteSpace(map[x, y]))
                    count++;
            }

            return count;
        }

        private static bool TryInferGameplayTileId(string tileId, out string inferredId)
        {
            inferredId = null;
            string value = Normalize(tileId);
            if (ContainsAny(value, "water", "river", "lake", "sea", "ocean", "swamp"))
                inferredId = "water";
            else if (ContainsAny(value, "sand", "coast", "beach", "shore"))
                inferredId = "sand";
            else if (ContainsAny(value, "snow"))
                inferredId = "snow";
            else if (ContainsAny(value, "forest-dense"))
                inferredId = "forest-dense";
            else if (ContainsAny(value, "forest"))
                inferredId = "forest-sparse";
            else if (ContainsAny(value, "hill", "mountain", "stone"))
                inferredId = "hill";
            else if (ContainsAny(value, "grass", "lowland"))
                inferredId = "grass";

            return inferredId != null;
        }

        private static bool IsWaterTileId(string tileId)
            => ContainsAny(tileId, "water", "river", "lake", "sea", "ocean", "swamp");

        private static bool ContainsAny(string value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (value.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value)
                ? GeneratedWorldDataDefaults.FallbackTileId
                : value.Trim();

        private static void AppendRepair(StringBuilder repairs, int count, string message)
        {
            if (count <= 0)
                return;

            if (repairs.Length > 0)
                repairs.Append("; ");
            repairs.Append(message);
        }

        private static GeneratedWorldDataIntegrityException Fail(string source, string reason)
        {
            string message =
                $"{LogTag} Cannot build generated world. source='{source}', reason='{reason}'.";
            Debug.LogError(message);
            return new GeneratedWorldDataIntegrityException(message);
        }
    }
}
