using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class WaterChannelCandidatePlannerTests
    {
        private static RecipeChannelDetectionConfig Config()
            => new RecipeChannelDetectionConfig
            {
                Enabled = true,
                MinRegionCells = 1,
                MaxCorridorLengthCells = 32,
                FrontierMarginCells = 4,
                MaxVisitedCells = 4096,
                MaxPairEvaluations = 32,
                MinorCorrectionMaxCutMeters = 0.6f,
                MajorCorrectionMaxCutMeters = 2.5f,
                MaxExcavationVolumeMeters = 8f,
            };

        private static float[,] Terrain(int w, int h, float value)
        {
            var surface = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                surface[x, y] = value;
            return surface;
        }

        private static void Water(bool[,] wet, float[,] surface, int x, int y, float level)
        {
            wet[x, y] = true;
            surface[x, y] = level;
        }

        private static WaterChannelCandidate Single(WaterChannelCandidatePlan plan)
        {
            Assert.AreEqual(1, plan.Candidates.Count, "expected exactly one evaluated corridor");
            return plan.Candidates[0];
        }

        [Test]
        public void Disabled_ReturnsEmptyPlan()
        {
            var wet = new bool[4, 4];
            var surface = Terrain(4, 4, 0.5f);
            var config = Config();
            config.Enabled = false;

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, config);

            Assert.AreEqual(0, plan.Regions.Length);
            Assert.AreEqual(0, plan.Candidates.Count);
            Assert.AreEqual(0, plan.Rejections.Count);
        }

        [Test]
        public void LowLandBetweenRegions_FindsNaturalCorridor()
        {
            // Water -- land below the waterline -- water: the corridor already
            // floods at source level, so zero excavation is required.
            const int w = 7, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 1, 0, 0f);
            Water(wet, surface, 5, 0, 0f);
            Water(wet, surface, 6, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            Assert.AreEqual(2, plan.Regions.Length);
            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.Natural, candidate.Status);
            Assert.AreEqual(WaterChannelRejectReason.None, candidate.RejectReason);
            Assert.AreEqual(3, candidate.LengthCells);
            Assert.AreEqual(0f, candidate.ExcavationVolumeMeters, 0.0001f);
            Assert.AreEqual(0f, candidate.MaxCellCutMeters, 0.0001f);
            Assert.LessOrEqual(candidate.SaddleMeters, candidate.SourceSurfaceMeters);
        }

        [Test]
        public void HighRidge_IsRejectedNotCompensated()
        {
            const int w = 7, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 10f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 1, 0, 0f);
            Water(wet, surface, 5, 0, 0f);
            Water(wet, surface, 6, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.Rejected, candidate.Status);
            Assert.AreEqual(WaterChannelRejectReason.RidgeTooHigh, candidate.RejectReason);
        }

        [Test]
        public void SlightHump_IsMinorCorrection()
        {
            const int w = 7, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 0.3f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 1, 0, 0f);
            Water(wet, surface, 5, 0, 0f);
            Water(wet, surface, 6, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.MinorCorrection, candidate.Status);
            Assert.AreEqual(0.3f, candidate.MaxCellCutMeters, 0.001f);
            Assert.AreEqual(3, candidate.CellsAboveSourceLevel);
        }

        [Test]
        public void LongLowDetour_BeatsShortRidge()
        {
            // Two 7-cell seas; a ridge wall blocks the direct crossing except
            // for a low gap at the bottom rows. The bottleneck search must
            // prefer the longer all-low corridor over cutting the ridge.
            const int w = 7, h = 7;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            for (int y = 0; y < h; y++)
            {
                Water(wet, surface, 0, y, 0f);
                Water(wet, surface, 6, y, 0f);
            }
            for (int y = 0; y <= 4; y++)
                surface[3, y] = 5f;

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.Natural, candidate.Status);
            Assert.AreEqual(-0.2f, candidate.SaddleMeters, 0.0001f);
            foreach (var cell in candidate.LandCells)
                Assert.IsFalse(cell.x == 3 && cell.y <= 4, "corridor cut through the ridge");
            Assert.Greater(candidate.LengthCells, 3, "detour corridor should be longer than the direct ridge cut");
        }

        [Test]
        public void HigherRegion_IsSource()
        {
            const int w = 7, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 0.5f);
            Water(wet, surface, 0, 0, 1.0f);
            Water(wet, surface, 1, 0, 1.0f);
            Water(wet, surface, 5, 0, -0.5f);
            Water(wet, surface, 6, 0, -0.5f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(0, candidate.SourceRegionId, "higher water region must be the source");
            Assert.AreEqual(1, candidate.TargetRegionId);
            Assert.AreEqual(1.0f, candidate.SourceSurfaceMeters, 0.0001f);
            Assert.AreEqual(-0.5f, candidate.TargetSurfaceMeters, 0.0001f);
            Assert.AreEqual(WaterChannelCandidateStatus.Natural, candidate.Status);
        }

        [Test]
        public void ThirdRegion_ChainsInsteadOfCrossing()
        {
            // Three separate lakes in a row; the middle one sits between the
            // other two, so pairs must be A-B and B-C (a channel A-C would have
            // to cross B's water, which corridors never do).
            const int w = 9, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 4, 0, 0f);
            Water(wet, surface, 8, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            Assert.AreEqual(3, plan.Regions.Length);
            Assert.AreEqual(2, plan.Candidates.Count);
            foreach (var candidate in plan.Candidates)
            {
                Assert.AreEqual(WaterChannelCandidateStatus.Natural, candidate.Status);
                Assert.AreEqual(
                    1,
                    Mathf.Abs(candidate.SourceRegionId - candidate.TargetRegionId),
                    "only neighbouring regions connect directly");
            }
        }

        [Test]
        public void IsthmusCorridor_RecordsLandSplit()
        {
            // Two seas across a land band. A ridge on the left edge forces the
            // corridor into the strip's interior, so flooding it severs the
            // land into a 3-cell and a 9-cell fragment.
            const int w = 5, h = 5;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            for (int x = 0; x < w; x++)
            {
                Water(wet, surface, x, 0, 0f);
                Water(wet, surface, x, 4, 0f);
            }
            for (int y = 1; y <= 3; y++)
                surface[0, y] = 5f;

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.Natural, candidate.Status);
            Assert.AreEqual(2, candidate.LandFragmentCount);
            Assert.AreEqual(3, candidate.SmallestLandFragmentCells);
        }

        [Test]
        public void ExcavationVolumeCap_RejectsLongShallowCut()
        {
            // Every corridor cell needs only a shallow cut, but the total
            // volume exceeds the budget — a hard reject, not a weighted loss.
            const int w = 14, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 1.0f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 13, 0, 0f);
            var config = Config();
            config.MaxExcavationVolumeMeters = 8f; // 12 cells x 1.0m = 12 > 8

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, config);

            var candidate = Single(plan);
            Assert.AreEqual(WaterChannelCandidateStatus.Rejected, candidate.Status);
            Assert.AreEqual(WaterChannelRejectReason.ExcavationTooLarge, candidate.RejectReason);
        }

        [Test]
        public void RegionsBeyondCorridorRange_AreNotEvaluated()
        {
            const int w = 64, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 63, 0, 0f);
            var config = Config();
            config.MaxCorridorLengthCells = 8;

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, config);

            Assert.AreEqual(0, plan.Candidates.Count);
            Assert.AreEqual(0, plan.Rejections.Count);
        }

        [Test]
        public void SearchBoundsCut_ReportsNoCorridor()
        {
            // The only traversable route leaves the pair's bounded search box,
            // so the bounded search correctly reports no corridor rather than
            // pretending one exists at any cost.
            const int w = 4, h = 3;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 0.2f);
            surface[1, 0] = float.NaN;
            surface[2, 0] = float.NaN;
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 3, 0, 0f);
            var config = Config();
            config.FrontierMarginCells = 0;

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, config);

            Assert.AreEqual(0, plan.Candidates.Count);
            Assert.AreEqual(1, plan.Rejections.Count);
            Assert.AreEqual(WaterChannelRejectReason.NoCorridor, plan.Rejections[0].Reason);
        }

        [Test]
        public void MissingWaterSurface_IsReportedNotGuessed()
        {
            const int w = 7, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, 0.2f);
            // Water cells without a finite surface: no level to reason about.
            Water(wet, surface, 0, 0, float.NaN);
            Water(wet, surface, 1, 0, float.NaN);
            Water(wet, surface, 5, 0, 0f);
            Water(wet, surface, 6, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            Assert.AreEqual(0, plan.Candidates.Count);
            Assert.AreEqual(1, plan.Rejections.Count);
            Assert.AreEqual(
                WaterChannelRejectReason.MissingWaterSurface, plan.Rejections[0].Reason);
        }

        [Test]
        public void SingleRegion_NoCandidates()
        {
            const int w = 4, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 1, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            Assert.AreEqual(1, plan.Regions.Length);
            Assert.AreEqual(0, plan.Candidates.Count);
        }

        [Test]
        public void Deterministic_SameInputSameResult()
        {
            const int w = 9, h = 9;
            var wet = new bool[w, h];
            var surface = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                surface[x, y] = 0.3f + Mathf.Sin(x * 1.7f + y * 0.9f) * 0.4f;
            for (int y = 0; y < h; y++)
            {
                Water(wet, surface, 0, y, 0f);
                Water(wet, surface, 8, y, 0f);
            }
            Water(wet, surface, 4, 0, 0.5f);
            Water(wet, surface, 4, 1, 0.5f);

            var config = Config();
            var first = WaterChannelCandidatePlanner.Build(wet, surface, config);
            var second = WaterChannelCandidatePlanner.Build(wet, surface, config);

            Assert.AreEqual(first.Regions.Length, second.Regions.Length);
            Assert.AreEqual(first.Candidates.Count, second.Candidates.Count);
            Assert.AreEqual(first.Rejections.Count, second.Rejections.Count);
            for (int i = 0; i < first.Candidates.Count; i++)
            {
                var a = first.Candidates[i];
                var b = second.Candidates[i];
                Assert.AreEqual(a.SourceRegionId, b.SourceRegionId);
                Assert.AreEqual(a.TargetRegionId, b.TargetRegionId);
                Assert.AreEqual(a.Status, b.Status);
                Assert.AreEqual(a.LengthCells, b.LengthCells);
                Assert.AreEqual(a.ExcavationVolumeMeters, b.ExcavationVolumeMeters, 0.0001f);
                CollectionAssert.AreEqual(a.LandCells, b.LandCells);
            }
        }

        [Test]
        public void EqualLevels_SourceIsLowerRegionId()
        {
            const int w = 5, h = 1;
            var wet = new bool[w, h];
            var surface = Terrain(w, h, -0.2f);
            Water(wet, surface, 0, 0, 0f);
            Water(wet, surface, 4, 0, 0f);

            var plan = WaterChannelCandidatePlanner.Build(wet, surface, Config());

            var candidate = Single(plan);
            Assert.AreEqual(0, candidate.SourceRegionId);
            Assert.AreEqual(1, candidate.TargetRegionId);
        }

        // The map-facing overload must leave the logical map untouched and
        // resolve water via the winner's surface-only geometry.
        [Test]
        public void LogicalMapOverload_IsReadOnly()
        {
            var map = new LogicalTileMap(5, 1);
            float[,] before = new float[5, 1];
            for (int x = 0; x < 5; x++)
            {
                var sample = new TileLayerSample(
                    "terrain", "Terrain", null, null, "grass", "grass",
                    LayerKind.BaseTerrain, 0, 0, 0, -0.2f, -0.2f, null,
                    TileGeometryMode.SolidTerrain);
                map.AddSample(x, 0, sample);
            }
            map.AddSample(0, 0, new TileLayerSample(
                "water", "Water", null, null, "water", "water",
                LayerKind.BaseTerrain, 0, 0, 0, 0f, 0f, null,
                TileGeometryMode.SurfaceOnly));
            map.AddSample(4, 0, new TileLayerSample(
                "water", "Water", null, null, "water", "water",
                LayerKind.BaseTerrain, 0, 0, 0, 0f, 0f, null,
                TileGeometryMode.SurfaceOnly));
            for (int x = 0; x < 5; x++)
                before[x, 0] = map.SurfaceHeights[x, 0];

            var plan = WaterChannelCandidatePlanner.Build(
                map, new[] { "water" }, Config());

            Assert.AreEqual(2, plan.Regions.Length);
            Assert.AreEqual(1, plan.Candidates.Count);
            for (int x = 0; x < 5; x++)
                Assert.AreEqual(before[x, 0], map.SurfaceHeights[x, 0], "map mutated");
        }
    }
}
