using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G06 acceptance invariants for drainage: every land cell's downstream
    /// parent chain terminates at a border/water sink (no cycles), tributaries
    /// merge through shared parents, lakes only in real filled depressions,
    /// rivers only on accumulated-flow land cells, and full determinism.
    /// </summary>
    public sealed class G06AcceptanceTests
    {
        private static WorldGenerationConfig Config() => new WorldGenerationConfig();

        private static WorldGenerationRequest Request(
            int seed, int width, int height, WorldGenerationConfig config = null,
            WorldArchetype archetype = WorldArchetype.Balanced)
        {
            config ??= Config();
            var t = config.TerrainLevels;
            return new WorldGenerationRequest(
                seed, width, height, archetype, 2, config,
                t.WaterLevel, t.ShoreLevel, t.LandLevel, t.HillLevel, t.MaxLevel,
                t.HeightStep, t.WaterSurfaceOffset);
        }

        private static int[,] Ramp(int w, int h, int waterLevel, int landLevel)
        {
            // Gentle west→east downslope so interior cells drain toward x=0.
            var levels = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                levels[x, y] = landLevel + x % 3;
            for (int y = 0; y < h; y++)
                levels[0, y] = waterLevel;
            return levels;
        }

        [Test]
        public void EveryCell_DrainsToSink_NoCycles()
        {
            var t = Config().TerrainLevels;
            int w = 24, h = 24;
            var levels = Ramp(w, h, t.WaterLevel, t.LandLevel);
            var output = new HydrologyStage().Generate(Request(7, w, h), levels);
            var parent = output.FlowParent;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                int cur = x + y * w;
                int guard = w * h + 4;
                var seen = new bool[w * h];
                while (parent[cur % w, cur / w] >= 0 && guard-- > 0)
                {
                    cur = parent[cur % w, cur / w];
                    Assert.IsFalse(seen[cur], $"flow cycle detected through cell {cur}");
                    seen[cur] = true;
                }
                Assert.Greater(guard, 0, "downstream chain never terminated — cycle or unbounded walk");
                Assert.AreEqual(-1, parent[cur % w, cur / w],
                    $"chain must end at an explicit sink, ended at {cur}");
            }
        }

        [Test]
        public void SinkCells_AreBorderOrWater_Only()
        {
            var t = Config().TerrainLevels;
            int w = 20, h = 20;
            var levels = Ramp(w, h, t.WaterLevel, t.LandLevel);
            var output = new HydrologyStage().Generate(Request(8, w, h), levels);
            var parent = output.FlowParent;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (parent[x, y] != -1) continue;
                bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                Assert.IsTrue(border || levels[x, y] == t.WaterLevel,
                    $"interior land cell ({x},{y}) must not be a sink");
            }
        }

        [Test]
        public void Accumulation_MergesUpstream_DownstreamCellsCarryMore()
        {
            var t = Config().TerrainLevels;
            int w = 24, h = 24;
            var levels = Ramp(w, h, t.WaterLevel, t.LandLevel);
            var output = new HydrologyStage().Generate(Request(9, w, h), levels);
            var parent = output.FlowParent;
            var acc = output.Accumulation;

            // A cell's accumulation is at least 1 (itself) and every parent's
            // accumulation includes its child's total upstream weight.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                Assert.GreaterOrEqual(acc[x, y], 1f,
                    $"acc[{x},{y}]={acc[x, y]}");
                int p = parent[x, y];
                if (p >= 0)
                    Assert.GreaterOrEqual(acc[p % w, p / w], acc[x, y],
                        $"downstream must accumulate at least its child's weight: " +
                        $"cell=({x},{y}) acc={acc[x, y]} parent=({p % w},{p / w}) acc={acc[p % w, p / w]}");
            }
        }

        [Test]
        public void DeepDepression_BecomesLake_WithinAreaCap()
        {
            var t = Config().TerrainLevels;
            int w = 16, h = 16;
            var levels = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                levels[x, y] = t.LandLevel + 2;
            // Enclosed bowl — walls are the highest ring, floor dips to land level.
            for (int x = 4; x <= 11; x++)
            for (int y = 4; y <= 11; y++)
                levels[x, y] = t.LandLevel;
            var output = new HydrologyStage().Generate(Request(10, w, h), levels);

            Assert.Greater(output.LakeCellCount, 0, "deep enclosed depression must become a lake");
            int maxLake = UnityEngine.Mathf.RoundToInt(w * h * Config().Hydrology.LakeMaxFraction);
            Assert.LessOrEqual(output.LakeCellCount, maxLake, "lake area must respect the cap");
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (output.LakeMask[x, y])
                    Assert.Greater(output.WaterSurface[x, y], 0f);
        }

        [Test]
        public void HighAccumulationLand_BecomesRiver_WaterNeverDoes()
        {
            var t = Config().TerrainLevels;
            var config = Config();
            config.Hydrology.RiverAccumulationThreshold = 4;
            config.Hydrology.RiverMinSourceLevel = 0;
            int w = 32, h = 32;
            var levels = Ramp(w, h, t.WaterLevel, t.LandLevel);
            var output = new HydrologyStage().Generate(Request(11, w, h, config), levels);

            Assert.Greater(output.RiverCellCount, 0, "long drainage field must produce rivers");
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (output.RiverMask[x, y])
                {
                    Assert.AreNotEqual(t.WaterLevel, levels[x, y], "water is never river");
                    Assert.IsFalse(output.LakeMask[x, y], "lake and river cells are disjoint");
                    Assert.GreaterOrEqual(output.Accumulation[x, y], config.Hydrology.RiverAccumulationThreshold);
                }
        }

        [Test]
        public void SameSeed_SameHydrology()
        {
            var t = Config().TerrainLevels;
            int w = 20, h = 20;
            var a = new HydrologyStage().Generate(Request(12, w, h), Ramp(w, h, t.WaterLevel, t.LandLevel));
            var b = new HydrologyStage().Generate(Request(12, w, h), Ramp(w, h, t.WaterLevel, t.LandLevel));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                Assert.AreEqual(a.RiverMask[x, y], b.RiverMask[x, y]);
                Assert.AreEqual(a.LakeMask[x, y], b.LakeMask[x, y]);
                Assert.AreEqual(a.FlowParent[x, y], b.FlowParent[x, y]);
            }
        }
    }
}
