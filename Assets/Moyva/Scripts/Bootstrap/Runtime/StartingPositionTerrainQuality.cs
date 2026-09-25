using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal readonly struct StartingPositionTerrainQuality
    {
        public StartingPositionTerrainQuality(
            bool hardValid,
            int utility,
            float landRatio,
            int nearestWaterDistance,
            float localHeightRange,
            int sampledTiles,
            int connectedLandTiles)
        {
            HardValid = hardValid;
            Utility = utility;
            LandRatio = landRatio;
            NearestWaterDistance = nearestWaterDistance;
            LocalHeightRange = localHeightRange;
            SampledTiles = sampledTiles;
            ConnectedLandTiles = connectedLandTiles;
        }

        public bool HardValid { get; }
        public int Utility { get; }
        public float LandRatio { get; }
        public int NearestWaterDistance { get; }
        public float LocalHeightRange { get; }
        public int SampledTiles { get; }
        public int ConnectedLandTiles { get; }
        public string Reason
        {
            get
            {
                string waterText = NearestWaterDistance == int.MaxValue
                    ? "none"
                    : NearestWaterDistance.ToString();
                return $"landRatio={LandRatio:0.00}; waterDistance={waterText}; " +
                    $"heightRange={LocalHeightRange:0.000}; sampled={SampledTiles}; " +
                    $"connectedLand={ConnectedLandTiles}; " +
                    $"hardValid={HardValid}; utility={Utility}";
            }
        }
    }

    /// <summary>
    /// Pure terrain scorer shared by every launch slot. It deliberately does not
    /// know which network participant owns a slot.
    ///
    /// Hard constraints:
    /// - enough usable land around the start;
    /// - the center is inside the generated maps.
    ///
    /// Soft utility:
    /// - nearby water is useful but not mandatory;
    /// - flatter local terrain is easier to develop;
    /// - a larger contiguous-looking land neighborhood is preferred.
    /// </summary>
    internal sealed class StartingPositionTerrainQualityEvaluator
    {
        private static readonly string[] WaterTokens =
            { "water", "river", "lake", "sea", "ocean", "swamp" };

        private readonly StartingPositionInitializerSettings _settings;
        private readonly ITerrainPlacementPolicy _placementPolicy;
        private readonly Dictionary<string, bool> _waterTileIds =
            new(StringComparer.Ordinal);

        public StartingPositionTerrainQualityEvaluator(
            StartingPositionInitializerSettings settings,
            ITerrainPlacementPolicy placementPolicy = null)
        {
            _settings = settings ?? new StartingPositionInitializerSettings();
            _placementPolicy = placementPolicy;
        }

        public StartingPositionTerrainQuality Evaluate(
            WorldGeneratedDataSignal signal,
            Vector2Int center)
        {
            int radius = Mathf.Max(1, _settings.startTerrainSampleRadius);
            int diameter = radius * 2 + 1;
            var landMask = new bool[diameter, diameter];
            int sampled = 0;
            int land = 0;
            int nearestWater = int.MaxValue;
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;
            float minimumLandHeight = Mathf.Min(
                _settings.startMinHeight, _settings.startMaxHeight);

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var cell = new Vector2Int(center.x + dx, center.y + dy);
                    if (!Contains(signal, cell))
                        continue;

                    sampled++;

                    bool hasHeight = TryReadHeight(signal, cell, out float height);
                    string tileId = ReadTileId(signal, cell);
                    // Tiles the placement policy forbids (e.g. no-spawn shore
                    // sand) count as unusable land: a sand center yields zero
                    // connected land and sand neighbours shrink the pad ratio.
                    bool water = IsWaterTileId(tileId)
                        || IsPlacementBlocked(tileId)
                        || hasHeight && height < minimumLandHeight;
                    if (water)
                    {
                        int distance = Mathf.Abs(dx) + Mathf.Abs(dy);
                        if (distance < nearestWater)
                            nearestWater = distance;
                        continue;
                    }

                    land++;
                    landMask[dx + radius, dy + radius] = true;

                    if (hasHeight)
                    {
                        minHeight = Mathf.Min(minHeight, height);
                        maxHeight = Mathf.Max(maxHeight, height);
                    }
                }
            }

            float landRatio = sampled > 0
                ? land / (float)sampled
                : 0f;

            float heightRange =
                minHeight <= maxHeight
                    ? maxHeight - minHeight
                    : 0f;

            int connectedLand =
                CountConnectedLand(landMask, radius, diameter);

            bool hardValid =
                sampled > 0 &&
                landRatio >= Mathf.Clamp01(_settings.minimumLandRatioAroundStart) &&
                connectedLand >= Mathf.Max(0, _settings.minimumConnectedLandTiles);

            int utility = Mathf.RoundToInt(landRatio * 1000f);

            // Prefer buildable, reasonably flat regions without making perfectly
            // flat terrain a hard requirement.
            float expectedRange = Mathf.Max(0.01f, _settings.preferredLocalHeightRange);
            utility += Mathf.RoundToInt(
                Mathf.Clamp01(1f - heightRange / expectedRange) * 220f);

            if (_settings.preferWaterNearStart && nearestWater != int.MaxValue)
            {
                int minWater = Mathf.Max(0, _settings.preferredWaterMinDistance);
                int maxWater = Mathf.Max(minWater, _settings.preferredWaterMaxDistance);

                if (nearestWater >= minWater && nearestWater <= maxWater)
                {
                    utility += 180;
                }
                else
                {
                    int distanceFromBand = nearestWater < minWater
                        ? minWater - nearestWater
                        : nearestWater - maxWater;

                    utility -= Mathf.Min(120, distanceFromBand * 18);
                }
            }

            return new StartingPositionTerrainQuality(
                hardValid,
                utility,
                landRatio,
                nearestWater,
                heightRange,
                sampled,
                connectedLand);
        }

        /// <summary>
        /// Land cells reachable from the center inside the sampled window.
        /// A high land ratio made of disconnected specks must not pass: the
        /// castle footprint and the first buildings need one contiguous pad.
        /// </summary>
        private static int CountConnectedLand(
            bool[,] landMask,
            int radius,
            int diameter)
        {
            if (!landMask[radius, radius])
                return 0;

            var visited = new bool[diameter, diameter];
            var queue = new Queue<Vector2Int>(diameter * diameter);
            visited[radius, radius] = true;
            queue.Enqueue(new Vector2Int(radius, radius));
            int connected = 0;

            while (queue.Count > 0)
            {
                Vector2Int cell = queue.Dequeue();
                connected++;

                for (int direction = 0; direction < 4; direction++)
                {
                    int nx = cell.x + (direction == 0 ? 1 : direction == 1 ? -1 : 0);
                    int ny = cell.y + (direction == 2 ? 1 : direction == 3 ? -1 : 0);
                    if (nx < 0 || ny < 0 || nx >= diameter || ny >= diameter
                        || visited[nx, ny] || !landMask[nx, ny])
                    {
                        continue;
                    }

                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }

            return connected;
        }

        private bool IsPlacementBlocked(string tileId)
            => _placementPolicy != null
               && !_placementPolicy.AllowsPlacement(
                   tileId,
                   TerrainPlacementOperation.StartingPosition);

        private bool IsWaterTileId(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            if (_waterTileIds.TryGetValue(tileId, out bool water))
                return water;

            water = ContainsWaterToken(tileId);
            _waterTileIds.Add(tileId, water);
            return water;
        }

        private static bool Contains(
            WorldGeneratedDataSignal signal,
            Vector2Int cell)
        {
            if (cell.x < 0 || cell.y < 0)
                return false;

            if (signal.Width > 0 &&
                signal.Height > 0 &&
                (cell.x >= signal.Width || cell.y >= signal.Height))
            {
                return false;
            }

            if (signal.HeightMap != null &&
                (cell.x >= signal.HeightMap.GetLength(0) ||
                 cell.y >= signal.HeightMap.GetLength(1)))
            {
                return false;
            }

            if (signal.TileMap != null &&
                (cell.x >= signal.TileMap.GetLength(0) ||
                 cell.y >= signal.TileMap.GetLength(1)))
            {
                return false;
            }

            return true;
        }

        private static bool TryReadHeight(
            WorldGeneratedDataSignal signal,
            Vector2Int cell,
            out float height)
        {
            if (signal.HeightMap == null ||
                cell.x < 0 ||
                cell.y < 0 ||
                cell.x >= signal.HeightMap.GetLength(0) ||
                cell.y >= signal.HeightMap.GetLength(1))
            {
                height = 0f;
                return false;
            }

            height = signal.HeightMap[cell.x, cell.y];
            return true;
        }

        private static string ReadTileId(
            WorldGeneratedDataSignal signal,
            Vector2Int cell)
        {
            if (signal.TileMap == null ||
                cell.x < 0 ||
                cell.y < 0 ||
                cell.x >= signal.TileMap.GetLength(0) ||
                cell.y >= signal.TileMap.GetLength(1))
            {
                return string.Empty;
            }

            return signal.TileMap[cell.x, cell.y] ?? string.Empty;
        }

        private static bool ContainsWaterToken(string value)
        {
            for (int i = 0; i < WaterTokens.Length; i++)
            {
                if (value.IndexOf(
                        WaterTokens[i],
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
