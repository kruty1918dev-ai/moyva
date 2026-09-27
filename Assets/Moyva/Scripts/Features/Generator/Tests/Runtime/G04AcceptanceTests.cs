using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G04 acceptance invariants for biome assignment: deterministic fields in
    /// → deterministic tiles out, only allow-listed gameplay tile ids, coherent
    /// biome zones (no 1-cell specks), desert produces real sand tiles, and
    /// snow caps form blobs rather than per-pixel speckle.
    /// </summary>
    public sealed class G04AcceptanceTests
    {
        private static readonly (WorldArchetype archetype, int size, int seed)[] AuditSeeds =
        {
            (WorldArchetype.Continents, 128, 642924492),
            (WorldArchetype.Islands, 64, 2143675486),
            (WorldArchetype.Desert, 64, 1242375142),
        };

        private static readonly HashSet<string> KnownTiles = new HashSet<string>
        {
            "water", "grass", "lowland", "sand",
            "forest-sparse", "forest-dense", "hill", "mountain", "snow"
        };

        private static WorldGenerationConfig Config() => new WorldGenerationConfig();

        private static WorldGenerationRequest Request(
            int seed, WorldArchetype archetype, int width, int height,
            WorldGenerationConfig config = null)
        {
            config ??= Config();
            var t = config.TerrainLevels;
            return new WorldGenerationRequest(
                seed, width, height, archetype, 2, config,
                t.WaterLevel, t.ShoreLevel, t.LandLevel, t.HillLevel, t.MaxLevel,
                t.HeightStep, t.WaterSurfaceOffset);
        }

        private static WorldGeographyResult GenerateWorld(
            int seed, WorldArchetype archetype, int size)
            => new WorldGeographyEngine().Generate(Request(seed, archetype, size, size));

        [Test]
        public void Biomes_AreDeterministic_AndCanonical()
        {
            var a = GenerateWorld(42, WorldArchetype.Balanced, 48);
            var b = GenerateWorld(42, WorldArchetype.Balanced, 48);
            for (int x = 0; x < 48; x++)
            for (int y = 0; y < 48; y++)
            {
                Assert.AreEqual(a.TileMap[x, y], b.TileMap[x, y]);
                Assert.IsTrue(KnownTiles.Contains(a.TileMap[x, y]),
                    $"unknown biome id '{a.TileMap[x, y]}' at {x},{y}");
            }
        }

        [Test]
        public void Biomes_HaveNoSingleCellSpecks()
        {
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var r = GenerateWorld(seed, archetype, size);
                for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    string t = r.TileMap[x, y];
                    if (t == "water") continue;

                    string n = null;
                    bool unanimous = true;
                    int seen = 0;
                    for (int dx = -1; dx <= 1 && unanimous; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx, ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                        string nt = r.TileMap[nx, ny];
                        if (nt == "water") { unanimous = false; break; }
                        seen++;
                        if (n == null) n = nt;
                        else if (nt != n) { unanimous = false; break; }
                    }
                    if (unanimous && seen >= 3)
                        Assert.AreEqual(n, t,
                            $"{archetype}: 1-cell {t} speck inside {n} at {x},{y}");
                }
            }
        }

        [Test]
        public void Desert_ProducesRealSandBiome()
        {
            var r = GenerateWorld(1242375142, WorldArchetype.Desert, 64);
            int sand = 0, land = 0;
            for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
            {
                if (r.TileMap[x, y] == "water") continue;
                land++;
                if (r.TileMap[x, y] == "sand") sand++;
            }
            Assert.Greater(sand, land / 3,
                "desert archetype must produce real 'sand' biome tiles, not visuals");
        }

        /// <summary>
        /// Feeds the stage a synthetic level field: one contiguous peak block at
        /// MaxLevel (8×6) plus one lone MaxLevel spike, inside uniform lowland.
        /// The snow/mountain split inside the cap must come out as a few coherent
        /// blobs (field-driven), and the lone spike must be snapped away.
        /// </summary>
        [Test]
        public void SnowCaps_FormBlobs_NotSpeckle()
        {
            const int size = 48;
            var request = Request(777, WorldArchetype.Highlands, size, size);
            var t = request.Config.TerrainLevels;

            var levels = new int[size, size];
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                levels[x, y] = t.LandLevel;
            for (int x = 20; x < 28; x++)
            for (int y = 20; y < 26; y++)
                levels[x, y] = t.MaxLevel;
            // Lone peak spike on a beach plateau: beach forces "sand" on all its
            // neighbours, so the spike is a guaranteed speck that must snap.
            levels[40, 40] = t.MaxLevel;

            var water = new bool[size, size];
            var beach = new bool[size, size];
            for (int x = 38; x <= 42; x++)
            for (int y = 38; y <= 42; y++)
                if (x != 40 || y != 40) beach[x, y] = true;
            var waterSurface = new float[size, size];
            var landMask = new float[size, size];
            var elevation = new float[size, size];
            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                landMask[x, y] = 1f;

            var output = new BiomeStage().Generate(
                request, levels, water, water, beach, waterSurface, landMask, elevation);
            var stats = BiomeStage.MeasureTiles(output.TileMap);

            int capBlobs = 0, largest = 0;
            foreach (var kv in stats.BlobSizes)
            {
                if (kv.Key != "snow" && kv.Key != "mountain") continue;
                capBlobs += kv.Value.Count;
                if (kv.Value[0] > largest) largest = kv.Value[0];
            }
            Assert.Greater(capBlobs, 0, "peak block produced no mountain/snow tiles");
            Assert.LessOrEqual(capBlobs, 4,
                $"cap split into {capBlobs} specks — snow field is not coherent");
            Assert.GreaterOrEqual(largest, 20,
                $"largest cap blob is {largest} cells — no coherent range");
            Assert.AreEqual("sand", output.TileMap[40, 40],
                "lone peak inside a uniform sand neighbourhood must snap");
            Assert.Greater(output.SnappedCells, 0, "isolated-tile rule never fired");
        }

        [Test]
        public void AuditSeeds_WriteBiomeMetrics()
        {
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var r = GenerateWorld(seed, archetype, size);
                var stats = BiomeStage.MeasureTiles(r.TileMap);
                var sb = new StringBuilder();
                sb.Append("G04-METRIC {\"archetype\":\"").Append(archetype.ToString().ToLowerInvariant())
                    .Append("\",\"size\":").Append(size)
                    .Append(",\"seed\":").Append(seed)
                    .Append(",\"blobs\":").Append(stats.TotalBlobs)
                    .Append(",\"byTile\":{");
                bool first = true;
                foreach (var kv in stats.BlobSizes)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('"').Append(kv.Key).Append("\":[");
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append(kv.Value[i]);
                    }
                    sb.Append(']');
                }
                TestContext.WriteLine(sb.Append("}}").ToString());
            }
            Assert.Pass();
        }
    }
}
