using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Strategic validation + spawn proposal. Scores land cells for start
    /// quality, picks one balanced start per player, verifies mutual
    /// connectivity over passable terrain (carving deterministic river fords
    /// when a river is the only barrier), and computes parity metrics.
    /// </summary>
    internal sealed class FairnessStage
    {
        internal sealed class Output
        {
            public SpawnHint[] Spawns;
            public bool ConnectivityOk;
            public float MinPairwiseDistance;
            public float OpportunityGap;
            public float MinOpportunityScore;
            public float MaxOpportunityScore;
            public int LargestLandComponent;
            public int FordCount;
            public List<string> GateFailures = new List<string>();
        }

        internal Output Evaluate(
            WorldGenerationRequest request,
            string[,] tiles,
            int[,] levels,
            bool[,] riverMask,
            bool[,] lakeMask,
            float[,] waterSurface,
            float[,] forestField,
            float[,] heightMap)
        {
            int w = request.Width;
            int h = request.Height;
            var fairness = request.Config.Fairness;
            int seed = request.Seed;
            int playerCount = Mathf.Max(1, request.PlayerCount);
            var result = new Output();

            int landCells = CountLand(tiles, w, h);
            float landFraction = (float)landCells / (w * h);
            if (landFraction < fairness.MinLandFraction)
                result.GateFailures.Add($"land-fraction {landFraction:F2} < {fairness.MinLandFraction:F2}");
            if (landFraction > fairness.MaxLandFraction)
                result.GateFailures.Add($"land-fraction {landFraction:F2} > {fairness.MaxLandFraction:F2}");

            var scores = ScoreCells(request, tiles, levels, forestField);
            var components = BuildComponents(tiles, w, h);
            result.LargestLandComponent = LargestComponent(components, w, h, out _);

            var spawns = PickSpawns(request, tiles, scores, components, playerCount, seed);
            if (spawns.Count < playerCount)
            {
                result.GateFailures.Add($"spawn-candidates {spawns.Count} < players {playerCount}");
                Finalize(result, spawns, scores, tiles, w, h, fairness, request);
                return result;
            }

            result.FordCount = fairness.RequireMutualConnectivity
                ? EnsureConnectivity(tiles, riverMask, levels, waterSurface, heightMap, components, spawns, w, h, seed, request)
                : 0;
            result.ConnectivityOk = fairness.RequireMutualConnectivity
                ? AllConnected(components, spawns)
                : true;

            Finalize(result, spawns, scores, tiles, w, h, fairness, request);
            return result;
        }

        private void Finalize(
            Output result, List<Vector2Int> spawns, float[,] scores,
            string[,] tiles, int w, int h, API.WorldGenerationConfig.FairnessSettings fairness,
            WorldGenerationRequest request)
        {
            if (spawns.Count == 0)
            {
                result.Spawns = System.Array.Empty<SpawnHint>();
                return;
            }

            float min = float.MaxValue, max = 0f;
            var hints = new SpawnHint[spawns.Count];
            for (int i = 0; i < spawns.Count; i++)
            {
                float s = scores[spawns[i].x, spawns[i].y];
                hints[i] = new SpawnHint(spawns[i], s);
                min = Mathf.Min(min, s);
                max = Mathf.Max(max, s);
            }
            result.Spawns = hints;
            result.MinOpportunityScore = min;
            result.MaxOpportunityScore = max;
            result.OpportunityGap = max > 0.001f ? max / Mathf.Max(0.001f, min) - 1f : 0f;
            result.MinPairwiseDistance = MinPairwise(spawns);

            if (result.OpportunityGap > fairness.MaxOpportunityGap)
                result.GateFailures.Add(
                    $"opportunity-gap {result.OpportunityGap:F2} > {fairness.MaxOpportunityGap:F2}");

            float minSep = Mathf.Min(w, h) * Mathf.Sqrt(2f) * fairness.MinSeparationFraction;
            if (spawns.Count > 1 && result.MinPairwiseDistance < minSep)
                result.GateFailures.Add(
                    $"spawn-distance {result.MinPairwiseDistance:F1} < {minSep:F1}");

            if (min < fairness.MinUsableLandFraction)
                result.GateFailures.Add(
                    $"usable-land {min:F2} < {fairness.MinUsableLandFraction:F2}");
        }

        private static bool IsPassable(string tile)
            => tile != "water" && tile != "mountain" && !string.IsNullOrEmpty(tile);

        private static bool IsStartableLand(string tile)
            => tile == "grass" || tile == "lowland" || tile == "sand"
               || tile == "forest-sparse" || tile == "forest-dense" || tile == "hill";

        private int CountLand(string[,] tiles, int w, int h)
        {
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (tiles[x, y] != "water") count++;
            return count;
        }

        private float[,] ScoreCells(
            WorldGenerationRequest request, string[,] tiles, int[,] levels, float[,] forestField)
        {
            int w = request.Width, h = request.Height;
            int radius = Mathf.Max(2, request.Config.Fairness.OpportunityRadius);
            var scores = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!IsStartableLand(tiles[x, y]))
                    continue;

                int total = 0, usable = 0;
                float quality = 0f, forest = 0f;
                bool nearWater = false;
                int x0 = Mathf.Max(0, x - radius), x1 = Mathf.Min(w - 1, x + radius);
                int y0 = Mathf.Max(0, y - radius), y1 = Mathf.Min(h - 1, y + radius);
                for (int nx = x0; nx <= x1; nx++)
                for (int ny = y0; ny <= y1; ny++)
                {
                    float d = Mathf.Sqrt((nx - x) * (nx - x) + (ny - y) * (ny - y));
                    if (d > radius) continue;
                    total++;
                    string t = tiles[nx, ny];
                    if (t == "water") { if (d <= 3f) nearWater = true; continue; }
                    if (!IsPassable(t)) continue;
                    usable++;
                    quality += t switch
                    {
                        "grass" => 1f,
                        "lowland" => 0.9f,
                        "forest-sparse" => 0.9f,
                        "forest-dense" => 0.75f,
                        "hill" => 0.7f,
                        "sand" => 0.6f,
                        "snow" => 0.45f,
                        _ => 0.4f,
                    };
                    forest += forestField[nx, ny];
                }

                float usableFrac = total > 0 ? (float)usable / total : 0f;
                float avgQuality = usable > 0 ? quality / usable : 0f;
                float waterBonus = nearWater ? 0.08f : 0f;
                float forestBonus = usable > 0 ? forest / usable * 0.15f : 0f;
                scores[x, y] = usableFrac * 0.55f + avgQuality * 0.37f + waterBonus + forestBonus;
            }
            return scores;
        }

        private int[,] BuildComponents(string[,] tiles, int w, int h)
        {
            var comp = new int[w, h];
            int nextId = 0;
            var queue = new Queue<Vector2Int>();
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!IsPassable(tiles[x, y]) || comp[x, y] != 0)
                    continue;
                nextId++;
                comp[x, y] = nextId;
                queue.Enqueue(new Vector2Int(x, y));
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue();
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = c.x + HydrologyDx[d];
                        int ny = c.y + HydrologyDy[d];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (comp[nx, ny] != 0 || !IsPassable(tiles[nx, ny])) continue;
                        comp[nx, ny] = nextId;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
            return comp;
        }

        private int LargestComponent(int[,] comp, int w, int h, out int largestId)
        {
            var counts = new Dictionary<int, int>();
            int best = 0;
            largestId = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (comp[x, y] == 0) continue;
                counts.TryGetValue(comp[x, y], out int c);
                counts[comp[x, y]] = ++c;
                if (c > best)
                {
                    best = c;
                    largestId = comp[x, y];
                }
            }
            return best;
        }

        private List<Vector2Int> PickSpawns(
            WorldGenerationRequest request, string[,] tiles, float[,] scores,
            int[,] components, int playerCount, int seed)
        {
            int w = request.Width, h = request.Height;
            float minSep = Mathf.Min(w, h) * Mathf.Sqrt(2f)
                * request.Config.Fairness.MinSeparationFraction;

            var chosen = new List<Vector2Int>();
            var used = new bool[w, h];
            int largestComp = LargestComponent(components, w, h, out int largestId);
            // When mutual connectivity is required, islands outside the main
            // landmass cannot host reachable spawns — restrict candidates to
            // the largest component (falling back to all passable land if it
            // is too small to separate the requested player count).
            bool restrictToLargest = request.Config.Fairness.RequireMutualConnectivity
                && largestComp > playerCount * 4;

            for (int pick = 0; pick < playerCount; pick++)
            {
                Vector2Int best = new Vector2Int(-1, -1);
                float bestValue = -1f;
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (used[x, y] || !IsStartableLand(tiles[x, y])) continue;
                    float s = scores[x, y];
                    if (s <= 0.001f) continue;
                    if (components[x, y] == 0) continue;
                    if (restrictToLargest && components[x, y] != largestId) continue;

                    float sepBonus = 1f;
                    if (chosen.Count > 0)
                    {
                        float d = MinDistTo(chosen, x, y);
                        sepBonus = Mathf.Clamp01(d / Mathf.Max(1f, minSep));
                    }
                    float value = s * Mathf.Lerp(0.35f, 1f, sepBonus);
                    // Deterministic tie-break.
                    value += DeterministicNoise.Hash01(seed, x, y, 88) * 0.0001f;
                    if (value > bestValue)
                    {
                        bestValue = value;
                        best = new Vector2Int(x, y);
                    }
                }
                if (best.x < 0) break;
                chosen.Add(best);
                used[best.x, best.y] = true;
            }
            return chosen;
        }

        /// <summary>
        /// When a river is the only barrier between spawn components, convert
        /// boundary river cells to passable sand fords. Deterministic: the ford
        /// closest to the straight line between the two spawns is chosen.
        /// </summary>
        private int EnsureConnectivity(
            string[,] tiles, bool[,] riverMask, int[,] levels, float[,] waterSurface,
            float[,] heightMap, int[,] components, List<Vector2Int> spawns, int w, int h,
            int seed, WorldGenerationRequest request)
        {
            int fords = 0;
            int guard = 0;
            while (!AllConnected(components, spawns) && guard++ < w * h / 4)
            {
                // Find a river cell adjacent to two different spawn components.
                int bestX = -1, bestY = -1;
                float bestDist = float.MaxValue;
                var spawnComps = new HashSet<int>();
                foreach (var s in spawns) spawnComps.Add(components[s.x, s.y]);

                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (!riverMask[x, y]) continue;
                    var adjacent = new HashSet<int>();
                    for (int dir = 0; dir < 8; dir++)
                    {
                        int nx = x + HydrologyDx[dir];
                        int ny = y + HydrologyDy[dir];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        int c = components[nx, ny];
                        if (c != 0 && spawnComps.Contains(c)) adjacent.Add(c);
                    }
                    if (adjacent.Count < 2) continue;

                    float d = DistToSpawnLine(spawns, x, y);
                    if (d < bestDist - 0.001f
                        || (Mathf.Abs(d - bestDist) <= 0.001f
                            && DeterministicNoise.Hash01(seed, x, y, 77) < 0.5f))
                    {
                        bestDist = d;
                        bestX = x; bestY = y;
                    }
                }
                if (bestX < 0) break;

                // Convert to ford: passable sand at channel level.
                riverMask[bestX, bestY] = false;
                tiles[bestX, bestY] = "sand";
                float fordHeight = levels[bestX, bestY] * request.HeightStep;
                waterSurface[bestX, bestY] = fordHeight;
                if (heightMap != null)
                    heightMap[bestX, bestY] = fordHeight;
                fords++;
                components = BuildComponents(tiles, w, h);
            }
            return fords;
        }

        private static bool AllConnected(int[,] components, List<Vector2Int> spawns)
        {
            if (spawns.Count <= 1) return true;
            int first = components[spawns[0].x, spawns[0].y];
            if (first == 0) return false;
            for (int i = 1; i < spawns.Count; i++)
                if (components[spawns[i].x, spawns[i].y] != first)
                    return false;
            return true;
        }

        private static float MinPairwise(List<Vector2Int> spawns)
        {
            float min = float.MaxValue;
            for (int i = 0; i < spawns.Count; i++)
            for (int j = i + 1; j < spawns.Count; j++)
            {
                float d = Vector2Int.Distance(spawns[i], spawns[j]);
                if (d < min) min = d;
            }
            return min == float.MaxValue ? 0f : min;
        }

        private static float MinDistTo(List<Vector2Int> chosen, int x, int y)
        {
            float min = float.MaxValue;
            foreach (var c in chosen)
                min = Mathf.Min(min, Vector2Int.Distance(c, new Vector2Int(x, y)));
            return min;
        }

        private static float DistToSpawnLine(List<Vector2Int> spawns, int x, int y)
        {
            // Distance to the segment between the two most separated spawns.
            if (spawns.Count < 2) return 0f;
            int ia = 0, ib = 1;
            float best = -1f;
            for (int i = 0; i < spawns.Count; i++)
            for (int j = i + 1; j < spawns.Count; j++)
            {
                float d = Vector2Int.Distance(spawns[i], spawns[j]);
                if (d > best) { best = d; ia = i; ib = j; }
            }
            Vector2 a = spawns[ia], b = spawns[ib], p = new Vector2(x, y);
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude < 0.001f ? 0f
                : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t);
        }

        private static readonly int[] HydrologyDx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] HydrologyDy = { -1, -1, -1, 0, 0, 1, 1, 1 };
    }
}
