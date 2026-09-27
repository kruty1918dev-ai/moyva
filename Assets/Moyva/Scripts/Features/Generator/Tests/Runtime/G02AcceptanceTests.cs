using System.Globalization;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G02 acceptance invariants for the elevation stage: finite deterministic
    /// output inside [0,1], height bounds consistent with the level quantizer,
    /// and relief that reads as large forms (world-scale gradient structure).
    /// The audit test dumps min/max, a height histogram and adjacent-gradient
    /// stats so the same metrics can be collected before/after the change.
    /// </summary>
    public sealed class G02AcceptanceTests
    {
        private static readonly (WorldArchetype archetype, int size, int seed)[] AuditSeeds =
        {
            (WorldArchetype.Continents, 128, 642924492),
            (WorldArchetype.Islands, 64, 2143675486),
            (WorldArchetype.Desert, 64, 1242375142),
            (WorldArchetype.Highlands, 64, 777),
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

        /// <summary>
        /// Synthetic plateau mask: land core in the middle, ocean rim — isolates
        /// the elevation stage from landmass-shape details.
        /// </summary>
        private static float[,] SyntheticMask(int w, int h)
        {
            var mask = new float[w, h];
            float half = Mathf.Min(w, h) * 0.5f;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float d = Mathf.Sqrt(
                    (x - w * 0.5f) * (x - w * 0.5f) + (y - h * 0.5f) * (y - h * 0.5f));
                mask[x, y] = Mathf.Clamp01(1.1f - d / (half * 0.8f));
            }
            return mask;
        }

        [Test]
        public void Elevation_IsFinite_InBounds_AndDeterministic()
        {
            var stage = new ElevationStage();
            var landmass = new LandmassStage();
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var request = Request(seed, archetype, size, size);
                var mask = landmass.Generate(request);
                var a = stage.Generate(request, mask);
                var b = stage.Generate(request, mask);

                for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    Assert.IsFalse(float.IsNaN(a[x, y]) || float.IsInfinity(a[x, y]),
                        $"{archetype}: non-finite elevation at {x},{y}");
                    Assert.GreaterOrEqual(a[x, y], 0f);
                    Assert.LessOrEqual(a[x, y], 1f);
                    Assert.AreEqual(a[x, y], b[x, y], $"{archetype}: nondeterministic {x},{y}");
                }
            }
        }

        [Test]
        public void OceanFloor_StaysBelowSeaLevelBand()
        {
            // TerrainLevelStage treats elevation < 0.36 as water; the stage must
            // never raise clearly-ocean mask cells near that bound via noise.
            var stage = new ElevationStage();
            var mask = SyntheticMask(64, 64);
            var elevation = stage.Generate(Request(5, WorldArchetype.Balanced, 64, 64), mask);
            for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
            {
                if (mask[x, y] >= 0.5f) continue;
                Assert.LessOrEqual(elevation[x, y], 0.34f,
                    $"ocean cell {x},{y} too high ({elevation[x, y]:F3})");
            }
        }

        [Test]
        public void LandRelief_ReadsAsLargeForms()
        {
            var stage = new ElevationStage();
            var mask = SyntheticMask(96, 96);
            var stats = Measure(stage.Generate(
                Request(11, WorldArchetype.Balanced, 96, 96), mask));

            TestContext.WriteLine($"G02-RELIEF {StatsJson(stats)}");
            // Mean adjacent gradient is dominated by cell-scale noise when scales
            // are too small; on world-scale forms it stays well under this bound.
            Assert.LessOrEqual(stats.MeanAdjacentGradient, 0.05f,
                "relief too noisy — mean adjacent gradient above large-form bound");
            Assert.GreaterOrEqual(stats.Max - stats.Min, 0.5f,
                "relief too flat — no readable macro height contrast");
        }

        [Test]
        public void Highlands_FormLongRanges_NotSpeckle()
        {
            var stage = new ElevationStage();
            var landmass = new LandmassStage();
            var request = Request(777, WorldArchetype.Highlands, 64, 64);
            var mask = landmass.Generate(request);
            var elevation = stage.Generate(request, mask);
            var stats = Measure(elevation);

            int highCells = 0;
            for (int x = 0; x < 64; x++)
            for (int y = 0; y < 64; y++)
                if (elevation[x, y] > 0.7f) highCells++;

            Assert.Greater(stats.Max, 0.75f, "highlands should reach mountain heights");
            Assert.Greater(highCells, 30,
                "highlands ranges should cover a meaningful area, not isolated pixels");
        }

        [Test]
        public void AuditSeeds_WriteElevationMetrics()
        {
            var stage = new ElevationStage();
            var landmass = new LandmassStage();
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var request = Request(seed, archetype, size, size);
                var mask = landmass.Generate(request);
                var stats = Measure(stage.Generate(request, mask));
                TestContext.WriteLine(
                    $"G02-METRIC {{\"archetype\":\"{request.ArchetypeId()}\",\"size\":{size}," +
                    $"\"seed\":{seed},{StatsJson(stats)}}}");
            }
            Assert.Pass();
        }

        private sealed class ElevStats
        {
            public float Min, Max, Mean, MeanAdjacentGradient, MaxAdjacentGradient;
            public int NonFinite;
            public int[] Histogram;
        }

        private static ElevStats Measure(float[,] elev)
        {
            int w = elev.GetLength(0), h = elev.GetLength(1);
            var s = new ElevStats
            {
                Min = float.MaxValue, Max = float.MinValue,
                Histogram = new int[16]
            };
            float sum = 0f, gradSum = 0f;
            int gradCount = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float v = elev[x, y];
                if (float.IsNaN(v) || float.IsInfinity(v))
                {
                    s.NonFinite++;
                    continue;
                }
                s.Min = Mathf.Min(s.Min, v);
                s.Max = Mathf.Max(s.Max, v);
                sum += v;
                int bucket = Mathf.Clamp((int)(v * 16f), 0, 15);
                s.Histogram[bucket]++;
                if (x + 1 < w)
                {
                    float g = Mathf.Abs(elev[x + 1, y] - v);
                    gradSum += g;
                    s.MaxAdjacentGradient = Mathf.Max(s.MaxAdjacentGradient, g);
                    gradCount++;
                }
                if (y + 1 < h)
                {
                    float g = Mathf.Abs(elev[x, y + 1] - v);
                    gradSum += g;
                    s.MaxAdjacentGradient = Mathf.Max(s.MaxAdjacentGradient, g);
                    gradCount++;
                }
            }
            s.Mean = sum / (w * h);
            s.MeanAdjacentGradient = gradCount > 0 ? gradSum / gradCount : 0f;
            return s;
        }

        private static string StatsJson(ElevStats s)
        {
            var sb = new StringBuilder();
            sb.Append("\"min\":").Append(F(s.Min))
                .Append(",\"max\":").Append(F(s.Max))
                .Append(",\"mean\":").Append(F(s.Mean))
                .Append(",\"meanAdjGrad\":").Append(F(s.MeanAdjacentGradient))
                .Append(",\"maxAdjGrad\":").Append(F(s.MaxAdjacentGradient))
                .Append(",\"nonFinite\":").Append(s.NonFinite)
                .Append(",\"histogram\":[");
            for (int i = 0; i < s.Histogram.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(s.Histogram[i]);
            }
            return sb.Append(']').ToString();
        }

        private static string F(float v) => v.ToString("F4", CultureInfo.InvariantCulture);
    }
}
