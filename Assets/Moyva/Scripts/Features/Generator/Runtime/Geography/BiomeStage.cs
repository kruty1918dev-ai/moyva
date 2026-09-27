using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Assigns canonical gameplay tile ids from terrain level + moisture/forest
    /// fields + masks, places strategic object ids, and produces the float
    /// surface-height map the renderer consumes. Biome choice is field-driven
    /// (coherent zones), never per-pixel random — after assignment an explicit
    /// isolated-tile rule snaps lone cells into a unanimous neighbourhood.
    /// </summary>
    internal sealed class BiomeStage
    {
        internal sealed class Output
        {
            public string[,] TileMap;
            public string[,] ObjectMap;
            public float[,] HeightMap;
            public float[,] MoistureField;
            public float[,] ForestField;
            public int MountainCells;
            public int ForestCells;
            public int ObjectCells;
            public int SnappedCells;
        }

        /// <summary>Biome blob metrics: tile id → sorted-descending blob areas.</summary>
        internal sealed class Stats
        {
            public int TotalBlobs;
            public Dictionary<string, List<int>> BlobSizes =
                new Dictionary<string, List<int>>();
        }

        internal Output Generate(
            WorldGenerationRequest request,
            int[,] levels,
            bool[,] riverMask,
            bool[,] lakeMask,
            bool[,] beachMask,
            float[,] waterSurface,
            float[,] landMask,
            float[,] elevation)
        {
            int w = request.Width;
            int h = request.Height;
            int seed = request.Seed;
            var p = request.Config.FindArchetype(request.ArchetypeId());
            var biomes = request.Config.Biomes;
            var tiles = new string[w, h];
            var objects = new string[w, h];
            var heights = new float[w, h];
            var moisture = new float[w, h];
            var forestField = new float[w, h];

            float moistScale = biomes.MoistureScale;
            float forestScale = biomes.ForestScale;
            // Snow caps are a coherent climate field, not per-pixel hash noise.
            float snowScale = Mathf.Max(10f, Mathf.Min(w, h) * 0.35f);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                int level = levels[x, y];
                float m = Mathf.Clamp01(
                    DeterministicNoise.WarpedFbm(seed + 3001, x / moistScale, y / moistScale, 0.3f, 3)
                    + RiverLakeBoost(riverMask, lakeMask, x, y, w, h)
                    - p.Aridity * 0.35f);
                moisture[x, y] = m;
                float f = Mathf.Clamp01(
                    DeterministicNoise.WarpedFbm(seed + 5003, x / forestScale, y / forestScale, 0.45f, 3)
                    * Mathf.Clamp01(m * 1.4f)
                    * Mathf.Lerp(0.4f, 1.4f, p.ForestDensity));
                forestField[x, y] = f;

                if (riverMask[x, y] || lakeMask[x, y] || level == request.WaterLevel)
                {
                    tiles[x, y] = "water";
                    heights[x, y] = waterSurface[x, y];
                    continue;
                }

                float snowField = DeterministicNoise.Fbm(
                    seed + 7013, x / snowScale, y / snowScale, 3);
                string tile = ResolveLandTile(
                    request, level, m, f, snowField, beachMask[x, y]);
                heights[x, y] = level * request.HeightStep;
                tiles[x, y] = tile;
            }

            int snapped = SnapIsolatedTiles(tiles, w, h);

            int mountains = 0, forests = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (tiles[x, y] == "mountain" || tiles[x, y] == "snow") mountains++;
                if (tiles[x, y] == "forest-sparse" || tiles[x, y] == "forest-dense") forests++;
            }

            int objectCells = PlaceObjects(request, tiles, levels, objects, seed);

            return new Output
            {
                TileMap = tiles,
                ObjectMap = objects,
                HeightMap = heights,
                MoistureField = moisture,
                ForestField = forestField,
                MountainCells = mountains,
                ForestCells = forests,
                ObjectCells = objectCells,
                SnappedCells = snapped,
            };
        }

        /// <summary>
        /// Explicit isolated-biome rule: a non-water cell whose full
        /// 8-neighbourhood is one identical non-water tile adopts it. Water
        /// contact (beach/shore cells) and mixed neighbourhoods are untouched —
        /// transitions stay sharp, no global smoothing.
        /// </summary>
        private static int SnapIsolatedTiles(string[,] tiles, int w, int h)
        {
            var snapped = (string[,])tiles.Clone();
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                string t = tiles[x, y];
                if (t == "water")
                    continue;

                string n = null;
                bool unanimous = true;
                int seen = 0;
                for (int dx = -1; dx <= 1 && unanimous; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    string nt = tiles[nx, ny];
                    if (nt == "water") { unanimous = false; break; }
                    seen++;
                    if (n == null) n = nt;
                    else if (nt != n) { unanimous = false; break; }
                }
                if (unanimous && seen >= 3 && n != t)
                {
                    snapped[x, y] = n;
                    count++;
                }
            }
            System.Array.Copy(snapped, tiles, tiles.Length);
            return count;
        }

        /// <summary>Counts 4-connected same-tile blob areas per biome id.</summary>
        internal static Stats MeasureTiles(string[,] tiles)
        {
            int w = tiles.GetLength(0), h = tiles.GetLength(1);
            var seen = new bool[w, h];
            var queue = new int[w * h];
            var stats = new Stats();
            for (int sx = 0; sx < w; sx++)
            for (int sy = 0; sy < h; sy++)
            {
                if (seen[sx, sy]) continue;
                string t = tiles[sx, sy];
                int area = 0, head = 0, tail = 0;
                seen[sx, sy] = true;
                queue[tail++] = sx * h + sy;
                while (head < tail)
                {
                    int c = queue[head++];
                    int cx = c / h, cy = c % h;
                    area++;
                    int[] dxs = { -1, 1, 0, 0 };
                    int[] dys = { 0, 0, -1, 1 };
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = cx + dxs[d], ny = cy + dys[d];
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                        if (seen[nx, ny] || tiles[nx, ny] != t) continue;
                        seen[nx, ny] = true;
                        queue[tail++] = nx * h + ny;
                    }
                }
                if (!stats.BlobSizes.TryGetValue(t, out var list))
                    stats.BlobSizes[t] = list = new List<int>();
                list.Add(area);
                stats.TotalBlobs++;
            }
            foreach (var list in stats.BlobSizes.Values)
                list.Sort((a, b) => b.CompareTo(a));
            return stats;
        }

        private static string ResolveLandTile(
            WorldGenerationRequest request, int level, float moisture, float forest,
            float snowField, bool beach)
        {
            var biomes = request.Config.Biomes;
            var p = request.Config.FindArchetype(request.ArchetypeId());

            if (beach)
                return "sand";

            // High terrain: peaks → mountain/snow.
            if (level >= request.MaxLevel)
                return snowField < p.SnowDensity ? "snow" : "mountain";
            if (level >= request.HillLevel + 1)
                return moisture > 0.6f && snowField < 0.35f
                    ? "snow"
                    : "mountain";
            if (level == request.HillLevel)
                return "hill";

            // Lowlands / plains.
            bool nearWater = beach || moisture > 0.62f;
            if (moisture < biomes.DesertMoistureThreshold + p.Aridity * 0.25f)
                return "sand"; // dry interior → desert
            if (forest > biomes.ForestMoistureThreshold)
                return forest > biomes.ForestMoistureThreshold + 0.18f
                    ? "forest-dense"
                    : "forest-sparse";
            if (level <= request.LandLevel && nearWater)
                return "lowland";
            return "grass";
        }

        private int PlaceObjects(
            WorldGenerationRequest request, string[,] tiles, int[,] levels,
            string[,] objects, int seed)
        {
            int w = request.Width, h = request.Height;
            var obj = request.Config.Objects;
            if (string.IsNullOrWhiteSpace(obj.LumberObjectId) && string.IsNullOrWhiteSpace(obj.StoneObjectId))
                return 0;

            int landCells = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (tiles[x, y] != "water") landCells++;
            int target = Mathf.RoundToInt(landCells * obj.PoiPerThousandLand / 1000f);
            if (target <= 0) return 0;

            int spacing = Mathf.Max(1, obj.PoiMinSpacing);
            var placed = new System.Collections.Generic.List<Vector2Int>();
            int placedCount = 0;

            // POIs must sit on passable land: lumber in/near forests, stone on
            // hills or land adjacent to mountains. Mountains themselves are
            // impassable and can never host a reachable POI.
            var lumberCandidates = new System.Collections.Generic.List<Vector2Int>();
            var stoneCandidates = new System.Collections.Generic.List<Vector2Int>();
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                string t = tiles[x, y];
                if (t == "forest-dense" || t == "forest-sparse")
                {
                    lumberCandidates.Add(new Vector2Int(x, y));
                    continue;
                }
                if (t == "hill" || (IsPassableLand(t) && HasNeighbourTile(tiles, x, y, w, h, "mountain")))
                    stoneCandidates.Add(new Vector2Int(x, y));
            }
            var lumber = lumberCandidates.ToArray();
            var stone = stoneCandidates.ToArray();
            DeterministicNoise.Shuffle(seed ^ 0xABC1, lumber);
            DeterministicNoise.Shuffle(seed ^ 0xBC2A, stone);

            placedCount += PlaceFromCandidates(obj.LumberObjectId, lumber, objects, placed, spacing, target - placedCount);
            placedCount += PlaceFromCandidates(obj.StoneObjectId, stone, objects, placed, spacing, target - placedCount);
            return placedCount;
        }

        private static int PlaceFromCandidates(
            string objectId,
            Vector2Int[] candidates,
            string[,] objects,
            System.Collections.Generic.List<Vector2Int> placed,
            int spacing,
            int budget)
        {
            if (string.IsNullOrWhiteSpace(objectId) || candidates == null || budget <= 0)
                return 0;

            int count = 0;
            foreach (var cell in candidates)
            {
                if (count >= budget)
                    break;

                bool tooClose = false;
                foreach (var other in placed)
                {
                    if (Mathf.Abs(other.x - cell.x) + Mathf.Abs(other.y - cell.y) < spacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                objects[cell.x, cell.y] = objectId;
                placed.Add(cell);
                count++;
            }
            return count;
        }

        private static bool IsPassableLand(string tileId)
            => tileId != "water" && tileId != "mountain" && tileId != "snow";

        private static bool HasNeighbourTile(string[,] tiles, int x, int y, int w, int h, string wanted)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < w && ny < h && tiles[nx, ny] == wanted)
                    return true;
            }
            return false;
        }

        private static float RiverLakeBoost(bool[,] riverMask, bool[,] lakeMask, int x, int y, int w, int h)
        {
            const int radius = 3;
            int x0 = Mathf.Max(0, x - radius), x1 = Mathf.Min(w - 1, x + radius);
            int y0 = Mathf.Max(0, y - radius), y1 = Mathf.Min(h - 1, y + radius);
            for (int nx = x0; nx <= x1; nx++)
            for (int ny = y0; ny <= y1; ny++)
                if (riverMask[nx, ny] || lakeMask[nx, ny])
                    return 0.22f;
            return 0f;
        }
    }
}
