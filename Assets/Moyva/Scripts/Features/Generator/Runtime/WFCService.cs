using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class WFCService : IWFCService
    {
        private readonly WFCDataSettings _wfcDataSettings;
        private readonly WFCTileRule[] _rulesByPriority;

        private static readonly Vector2Int[] Offsets = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // Top
            new Vector2Int(1, 1),   // TopRight
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, -1),  // BottomRight
            new Vector2Int(0, -1),  // Bottom
            new Vector2Int(-1, -1), // BottomLeft
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(-1, 1)   // TopLeft
        };

        public WFCService(WFCDataSettings wFCDataSettings)
        {
            _wfcDataSettings = wFCDataSettings;
            _rulesByPriority = _wfcDataSettings?.TileRules == null
                ? System.Array.Empty<WFCTileRule>()
                : _wfcDataSettings.TileRules
                    .OrderByDescending(r => r.Priority)
                    .ThenBy(r => r.TileID)
                    .ToArray();
        }

        public void Apply(string[,] biomeMap, float[,] heightMap)
        {
            int width = biomeMap.GetLength(0);
            int height = biomeMap.GetLength(1);
            const string WaterID = "water";

            // ПЕРШИЙ КРОК: Заповнюємо контур водою
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        biomeMap[x, y] = WaterID;
                    }
                }
            }

            // ПІДГОТОВКА: за потреби робимо гарантовану смугу тайлу біля води
            ApplyNearWaterBand(biomeMap, width, height);

            // ДРУГИЙ КРОК: Побудова індексу позицій за типом тайлу
            var tilePositions = BuildPositionIndex(biomeMap, width, height);

            // ТРЕТІЙ КРОК: Ітерація за пріоритетом правил
            for (int pass = 0; pass < _wfcDataSettings.PassCount; pass++)
            {
                bool anyChanges = false;

                foreach (var rule in _rulesByPriority)
                {
                    // Якщо результат == центр — нічого не зміниться
                    if (rule.TileID == rule.TileCentralID)
                        continue;

                    if (ApplyRuleUntilStable(rule, biomeMap, width, height, tilePositions))
                        anyChanges = true;
                }

                if (!anyChanges) break;
            }
        }

        private void ApplyNearWaterBand(string[,] biomeMap, int width, int height)
        {
            if (_wfcDataSettings == null || !_wfcDataSettings.ForceTileNearWaterBand)
                return;

            if (string.IsNullOrWhiteSpace(_wfcDataSettings.NearWaterTileId))
                return;

            int radius = Mathf.Max(1, _wfcDataSettings.NearWaterRadius);
            bool includeDiagonals = _wfcDataSettings.IncludeDiagonalsForNearWater;
            var waterLikeIds = BuildWaterLikeIdSet(_wfcDataSettings.WaterLikeTileIds);

            var toReplace = new List<Vector2Int>();
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    string current = biomeMap[x, y];
                    if (IsWaterLike(current, waterLikeIds))
                        continue;

                    if (HasWaterLikeNeighbor(biomeMap, x, y, width, height, radius, includeDiagonals, waterLikeIds))
                        toReplace.Add(new Vector2Int(x, y));
                }
            }

            foreach (var p in toReplace)
                biomeMap[p.x, p.y] = _wfcDataSettings.NearWaterTileId;
        }

        private static HashSet<string> BuildWaterLikeIdSet(string[] ids)
        {
            var set = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (!string.IsNullOrWhiteSpace(id))
                        set.Add(id.Trim());
                }
            }

            if (set.Count == 0)
                set.Add("water");

            return set;
        }

        private static bool IsWaterLike(string tileId, HashSet<string> waterLikeIds)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            string normalized = tileId.Trim();
            if (waterLikeIds.Contains(normalized))
                return true;

            string lowered = normalized.ToLowerInvariant();
            return lowered.Contains("water")
                || lowered.Contains("sea")
                || lowered.Contains("lake")
                || lowered.Contains("river");
        }

        private static bool HasWaterLikeNeighbor(
            string[,] biomeMap,
            int x,
            int y,
            int width,
            int height,
            int radius,
            bool includeDiagonals,
            HashSet<string> waterLikeIds)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    if (!includeDiagonals && Mathf.Abs(dx) + Mathf.Abs(dy) > radius)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    if (IsWaterLike(biomeMap[nx, ny], waterLikeIds))
                        return true;
                }
            }

            return false;
        }

        private bool ApplyRuleUntilStable(
            WFCTileRule rule,
            string[,] biomeMap,
            int width,
            int height,
            Dictionary<string, HashSet<Vector2Int>> tilePositions)
        {
            bool changedAny = false;

            while (true)
            {
                if (!tilePositions.TryGetValue(rule.TileCentralID, out var positions) || positions.Count == 0)
                    return changedAny;

                var snapshot = new List<Vector2Int>(positions);
                var changes = new List<Vector2Int>();

                foreach (var pos in snapshot)
                {
                    if (CheckRule(pos.x, pos.y, biomeMap, width, height, rule))
                        changes.Add(pos);
                }

                if (changes.Count == 0)
                    return changedAny;

                foreach (var pos in changes)
                {
                    string oldTile = biomeMap[pos.x, pos.y];
                    biomeMap[pos.x, pos.y] = rule.TileID;

                    if (tilePositions.TryGetValue(oldTile, out var oldSet))
                        oldSet.Remove(pos);

                    if (!tilePositions.TryGetValue(rule.TileID, out var newSet))
                    {
                        newSet = new HashSet<Vector2Int>();
                        tilePositions[rule.TileID] = newSet;
                    }

                    newSet.Add(pos);
                }

                changedAny = true;
            }
        }

        private Dictionary<string, HashSet<Vector2Int>> BuildPositionIndex(
            string[,] map, int width, int height)
        {
            var index = new Dictionary<string, HashSet<Vector2Int>>();
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    string id = map[x, y];
                    if (!index.TryGetValue(id, out var set))
                    {
                        set = new HashSet<Vector2Int>();
                        index[id] = set;
                    }
                    set.Add(new Vector2Int(x, y));
                }
            }
            return index;
        }

        private bool CheckRule(int x, int y, string[,] map, int width, int height, WFCTileRule rule)
        {
            if (rule.Constraints == null || rule.Constraints.Count == 0) return true;

            int matchedConstraints = 0;
            const string BoundaryTileID = "water"; // ID тайла, який ми вважаємо "безоднею" за межами

            foreach (var constraint in rule.Constraints)
            {
                Vector2Int offset = Offsets[(int)constraint.Direction];
                int nx = x + offset.x;
                int ny = y + offset.y;

                string neighborTile;

                // ПЕРЕВІРКА МЕЖ
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                {
                    // Якщо ми за межами, кажемо, що там "water"
                    neighborTile = BoundaryTileID;
                }
                else
                {
                    neighborTile = map[nx, ny];
                }

                bool isAllowed = false;
                for (int i = 0; i < constraint.AllowedNeighbors.Count; i++)
                {
                    if (constraint.AllowedNeighbors[i] == neighborTile)
                    {
                        isAllowed = true;
                        break;
                    }
                }

                if (isAllowed) matchedConstraints++;
            }

            float matchRate = (float)matchedConstraints / rule.Constraints.Count;
            return matchRate >= rule.MatchThreshold;
        }
    }
}