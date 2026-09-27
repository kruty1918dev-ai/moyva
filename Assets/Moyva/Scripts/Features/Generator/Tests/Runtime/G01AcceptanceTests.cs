using System.Globalization;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G01 acceptance invariants for the landmass stage: deterministic masks,
    /// archetype-distinct macro geography, land-ratio tracking, and removal of
    /// speck components only where they contradict the requested map type.
    /// Also dumps before/after landmass metrics for the G01 audit seeds.
    /// </summary>
    public sealed class G01AcceptanceTests
    {
        // Audit fixtures from the G01 task card.
        private static readonly (WorldArchetype archetype, int size, int seed)[] AuditSeeds =
        {
            (WorldArchetype.Continents, 128, 642924492),
            (WorldArchetype.Islands, 64, 2143675486),
            (WorldArchetype.Desert, 64, 1242375142),
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

        [Test]
        public void SameSeedAndConfig_ProduceIdenticalMasks()
        {
            var stage = new LandmassStage();
            var archetypes = new[]
            {
                WorldArchetype.Continents, WorldArchetype.Pangaea, WorldArchetype.Islands
            };
            foreach (var archetype in archetypes)
            {
                var a = stage.Generate(Request(642924492, archetype, 64, 64));
                var b = stage.Generate(Request(642924492, archetype, 64, 64));
                for (int x = 0; x < 64; x++)
                for (int y = 0; y < 64; y++)
                    Assert.AreEqual(a[x, y], b[x, y], $"{archetype} mask {x},{y}");
            }
        }

        [Test]
        public void Archetypes_ProduceDistinctLandShapes()
        {
            var stage = new LandmassStage();
            var continents = stage.Generate(Request(9, WorldArchetype.Continents, 64, 64));
            var pangaea = stage.Generate(Request(9, WorldArchetype.Pangaea, 64, 64));
            var islands = stage.Generate(Request(9, WorldArchetype.Islands, 64, 64));

            var sc = LandmassStage.Measure(continents);
            var sp = LandmassStage.Measure(pangaea);
            var si = LandmassStage.Measure(islands);

            Assert.Greater(sp.LandFraction, sc.LandFraction,
                "pangaea should carry more land than continents");
            Assert.Greater(sc.LandFraction, si.LandFraction,
                "continents should carry more land than islands");

            int n = 64 * 64;
            Assert.Greater(DiffCells(pangaea, islands), n / 10,
                "pangaea and islands masks should diverge meaningfully");
            Assert.Greater(DiffCells(continents, islands), n / 10,
                "continents and islands masks should diverge meaningfully");
        }

        [Test]
        public void LandFraction_TracksConfiguredRatio_AcrossArchetypes()
        {
            var stage = new LandmassStage();
            var config = Config();
            foreach (var p in config.Archetypes)
            {
                var archetype = ArchetypeOf(p.Id);
                var stats = LandmassStage.Measure(
                    stage.Generate(Request(17, archetype, 64, 64, config)));
                Assert.LessOrEqual(
                    System.Math.Abs(stats.LandFraction - p.LandRatio), 0.08f,
                    $"{p.Id}: landFraction {stats.LandFraction:F3} vs target {p.LandRatio:F3}");
            }
        }

        [Test]
        public void Mask_IsNeverAllWaterOrAllLand()
        {
            var stage = new LandmassStage();
            for (int seed = 1; seed <= 8; seed++)
            foreach (WorldArchetype archetype in System.Enum.GetValues(typeof(WorldArchetype)))
            {
                var stats = LandmassStage.Measure(
                    stage.Generate(Request(seed, archetype, 48, 48)));
                Assert.Greater(stats.LandCells, 0, $"{archetype} seed {seed}: all water");
                Assert.Less(stats.LandCells, 48 * 48, $"{archetype} seed {seed}: all land");
            }
        }

        [Test]
        public void Pruning_SinksSpecks_ButKeepsArchetypeIslands()
        {
            var stage = new LandmassStage();
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var request = Request(seed, archetype, size, size);
                var before = LandmassStage.Measure(stage.BuildRawMask(request));
                var after = LandmassStage.Measure(stage.Generate(request));
                int n = size * size;

                TestContext.WriteLine(
                    $"{archetype} {size}² seed {seed}: components {before.ComponentCount}→{after.ComponentCount}, " +
                    $"land {before.LandFraction:F3}→{after.LandFraction:F3}, " +
                    $"largest {before.LargestComponentCells}→{after.LargestComponentCells}");

                Assert.LessOrEqual(after.ComponentCount, before.ComponentCount,
                    $"{archetype}: pruning must not create components");
                Assert.GreaterOrEqual(after.LargestComponentCells, 1);

                // Speck floor scales with the seeded centre count: single-continent
                // archetypes tolerate no small blobs, archipelagos keep small islands.
                // Assert at half the implemented threshold so the invariant has margin.
                int centreCount = Mathf.Max(1,
                    request.Config.FindArchetype(request.ArchetypeId()).CentreCount);
                int speckFloor = Mathf.Max(2, Mathf.RoundToInt(n * 0.01f / centreCount));
                foreach (int area in after.ComponentSizes)
                    Assert.GreaterOrEqual(area, speckFloor,
                        $"{archetype}: surviving component of {area} cells is a speck");
            }
        }

        [Test]
        public void Islands_KeepMoreComponents_ThanSingleContinentArchetypes()
        {
            var stage = new LandmassStage();
            var islands = LandmassStage.Measure(
                stage.Generate(Request(2143675486, WorldArchetype.Islands, 64, 64)));
            var pangaea = LandmassStage.Measure(
                stage.Generate(Request(2143675486, WorldArchetype.Pangaea, 64, 64)));

            Assert.GreaterOrEqual(islands.ComponentCount, 3,
                "islands archetype should keep several distinct landmasses");
            Assert.Greater(islands.ComponentCount, pangaea.ComponentCount,
                "islands should keep more components than pangaea");
            Assert.GreaterOrEqual(
                pangaea.LargestComponentCells, (int)(pangaea.LandCells * 0.6f),
                "pangaea land should be dominated by one supercontinent");
        }

        [Test]
        public void AuditSeeds_WriteBeforeAfterMetrics()
        {
            var stage = new LandmassStage();
            var sb = new StringBuilder();
            sb.Append("{\"fixtures\":[");
            bool first = true;
            foreach (var (archetype, size, seed) in AuditSeeds)
            {
                var request = Request(seed, archetype, size, size);
                var config = request.Config.FindArchetype(request.ArchetypeId());
                var before = LandmassStage.Measure(stage.BuildRawMask(request));
                var after = LandmassStage.Measure(stage.Generate(request));
                var entry = new StringBuilder()
                    .Append("{\"archetype\":\"").Append(request.ArchetypeId())
                    .Append("\",\"size\":").Append(size)
                    .Append(",\"seed\":").Append(seed)
                    .Append(",\"targetLandRatio\":").Append(config.LandRatio.ToString("F4", CultureInfo.InvariantCulture))
                    .Append(",\"before\":").Append(StatsJson(before))
                    .Append(",\"after\":").Append(StatsJson(after))
                    .Append('}');
                if (!first) sb.Append(',');
                first = false;
                sb.Append(entry);
                TestContext.WriteLine($"G01-METRIC {entry}");
            }
            sb.Append("]}");

            var dir = Path.Combine(
                Directory.GetCurrentDirectory(), "Temp", "ai", "moyva-parallel", "G01");
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "landmass-metrics.json"), sb.ToString());
            Assert.Pass();
        }

        private static string StatsJson(LandmassStage.Stats s)
        {
            var sb = new StringBuilder();
            sb.Append("{\"landFraction\":").Append(s.LandFraction.ToString("F4", CultureInfo.InvariantCulture))
                .Append(",\"landCells\":").Append(s.LandCells)
                .Append(",\"componentCount\":").Append(s.ComponentCount)
                .Append(",\"largestComponent\":").Append(s.LargestComponentCells)
                .Append(",\"componentSizes\":[");
            for (int i = 0; i < s.ComponentSizes.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(s.ComponentSizes[i]);
            }
            return sb.Append("]}").ToString();
        }

        private static int DiffCells(float[,] a, float[,] b)
        {
            int diff = 0;
            for (int x = 0; x < a.GetLength(0); x++)
            for (int y = 0; y < a.GetLength(1); y++)
                if (System.Math.Abs(a[x, y] - b[x, y]) > 1e-6f) diff++;
            return diff;
        }

        private static WorldArchetype ArchetypeOf(string id) => id switch
        {
            "continents" => WorldArchetype.Continents,
            "pangaea" => WorldArchetype.Pangaea,
            "islands" => WorldArchetype.Islands,
            "highlands" => WorldArchetype.Highlands,
            "desert" => WorldArchetype.Desert,
            _ => WorldArchetype.Balanced,
        };
    }
}
