using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G03 acceptance invariants for level quantization: a pure
    /// elevation→level function (both sides of every threshold map to the
    /// right level, monotone, in-range), shore pinning, and the explicit
    /// isolated-step rule that only fires on unanimous neighbourhoods.
    /// </summary>
    public sealed class G03AcceptanceTests
    {
        private static WorldGenerationConfig Config() => new WorldGenerationConfig();

        private static WorldGenerationRequest Request(
            int seed, int width, int height,
            WorldArchetype archetype = WorldArchetype.Balanced,
            WorldGenerationConfig config = null)
        {
            config ??= Config();
            var t = config.TerrainLevels;
            return new WorldGenerationRequest(
                seed, width, height, archetype, 2, config,
                t.WaterLevel, t.ShoreLevel, t.LandLevel, t.HillLevel, t.MaxLevel,
                t.HeightStep, t.WaterSurfaceOffset);
        }

        private static float[,] Fill(float v, int w, int h)
        {
            var m = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                m[x, y] = v;
            return m;
        }

        [Test]
        public void Quantize_MapsBothSidesOfSeaLevel()
        {
            var stage = new TerrainLevelStage();
            var t = Config().TerrainLevels;
            var mask = Fill(1f, 8, 8);
            var elev = Fill(0.7f, 8, 8);
            elev[2, 2] = 0.355f; // just below sea level
            elev[3, 3] = 0.365f; // just above
            elev[4, 4] = 1f;     // top of range

            var levels = stage.Quantize(Request(1, 8, 8), elev, mask);
            Assert.AreEqual(t.WaterLevel, levels[2, 2], "below sea level must be water");
            Assert.GreaterOrEqual(levels[3, 3], t.LandLevel, "above sea level must be land");
            Assert.AreEqual(t.MaxLevel, levels[4, 4], "elev 1 must reach max level");
        }

        [Test]
        public void Quantize_LandMask_OverridesElevation()
        {
            var stage = new TerrainLevelStage();
            var t = Config().TerrainLevels;
            var elev = Fill(0.9f, 8, 8);   // high elevation...
            var mask = Fill(0.1f, 8, 8);   // ...but ocean mask
            var levels = stage.Quantize(Request(2, 8, 8), elev, mask);
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                Assert.AreEqual(t.WaterLevel, levels[x, y]);
        }

        [Test]
        public void Quantize_IsMonotoneInElevation()
        {
            // No per-cell jitter: cells ordered by elevation must produce
            // non-decreasing levels (same value → same level, on both sides of
            // every rounding boundary).
            var stage = new TerrainLevelStage();
            int n = 64;
            var mask = Fill(1f, n, n);
            var elev = new float[n, n];
            var samples = new List<(float e, int idx)>();
            for (int x = 0; x < n; x++)
            for (int y = 0; y < n; y++)
            {
                elev[x, y] = 0.37f + 0.6f * ((x * 31 + y * 17) % n) / n;
                samples.Add((elev[x, y], x * n + y));
            }
            var levels = stage.Quantize(Request(3, n, n), elev, mask);
            samples.Sort((a, b) => a.e.CompareTo(b.e));

            int prev = levels[samples[0].idx / n, samples[0].idx % n];
            foreach (var (e, idx) in samples)
            {
                int l = levels[idx / n, idx % n];
                Assert.GreaterOrEqual(l, prev,
                    $"elevation {e:F4} mapped to {l} after a lower elevation mapped to {prev}");
                prev = l;
            }
        }

        [Test]
        public void Quantize_IsolatedSpike_And_Pit_SnapToPlateau()
        {
            var stage = new TerrainLevelStage();
            var t = Config().TerrainLevels;
            var mask = Fill(1f, 16, 16);
            var elev = Fill(0.7f, 16, 16);
            var flat = stage.Quantize(Request(4, 16, 16), elev, mask);
            int plateau = flat[5, 5];

            elev[8, 8] = 0.98f;  // one-tile spike
            elev[3, 12] = 0.40f; // one-tile pit
            var levels = stage.Quantize(Request(4, 16, 16), elev, mask);

            Assert.AreEqual(plateau, levels[8, 8], "isolated spike must snap to plateau level");
            Assert.AreEqual(plateau, levels[3, 12], "isolated pit must snap to plateau level");
        }

        [Test]
        public void Quantize_MixedNeighbourhood_KeepsTacticalStep()
        {
            // A cell at a real terrain boundary (half low / half high neighbours)
            // must NOT be smoothed — only unanimous surroundings snap.
            var stage = new TerrainLevelStage();
            var mask = Fill(1f, 16, 16);
            var elev = new float[16, 16];
            for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                elev[x, y] = x < 8 ? 0.55f : 0.95f;
            elev[7, 7] = 0.95f; // odd cell touching both halves

            var levels = stage.Quantize(Request(5, 16, 16), elev, mask);
            int lowLevel = levels[3, 3];
            Assert.Greater(levels[7, 7], lowLevel,
                "boundary cell at a mixed edge must keep its own level");
        }

        [Test]
        public void Quantize_ShoreCells_PinToShoreLevel()
        {
            var stage = new TerrainLevelStage();
            var t = Config().TerrainLevels;
            var mask = new float[16, 16];
            var elev = new float[16, 16];
            for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                bool land = x < 10;
                mask[x, y] = land ? 0.9f : 0.1f;
                elev[x, y] = land ? 0.55f : 0.1f;
            }
            var levels = stage.Quantize(Request(6, 16, 16), elev, mask);

            for (int y = 0; y < 16; y++)
            {
                Assert.AreEqual(t.ShoreLevel, levels[9, y], $"coastal cell 9,{y}");
                Assert.AreEqual(t.WaterLevel, levels[10, y]);
            }
        }

        [Test]
        public void Quantize_IsDeterministic_AndWithinConfigRange()
        {
            var stage = new TerrainLevelStage();
            var t = Config().TerrainLevels;
            var landmass = new LandmassStage();
            var elevationStage = new ElevationStage();
            var request = Request(1242375142, 64, 64, WorldArchetype.Desert);
            var mask = landmass.Generate(request);
            var elev = elevationStage.Generate(request, mask);

            var a = stage.Quantize(request, elev, mask);
            var b = stage.Quantize(request, elev, mask);
            for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
            {
                Assert.AreEqual(a[x, y], b[x, y]);
                Assert.GreaterOrEqual(a[x, y], t.WaterLevel);
                Assert.LessOrEqual(a[x, y], t.MaxLevel);
            }
        }
    }
}
