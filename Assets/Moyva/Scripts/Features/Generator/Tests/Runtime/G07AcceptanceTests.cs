using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G07 acceptance invariants for recipe-space hydrology: the plan works in
    /// meters on the recipe relief field — drainage terminates at sinks/border
    /// with no cycles, rivers only on accumulated-flow cells above the source
    /// minimum, beds sit a channel depth below surfaces, waterfalls require a
    /// real downstream drop, and Merge unions plans without losing data.
    /// </summary>
    public sealed class G07AcceptanceTests
    {
        private static RecipeHydrologyConfig Config() => new RecipeHydrologyConfig
        {
            Enabled = true,
            RiverAccumulationThreshold = 4,
            RiverMinSourceMeters = 0.1f,
            WaterfallMinDropMeters = 0.4f,
        };

        private static float[,] Downslope(int w, int h)
        {
            // Strict west→east descent so flow marches to the x=0 border.
            var t = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                t[x, y] = 5f + x * 0.5f;
            return t;
        }

        [Test]
        public void DisabledConfig_ReturnsEmptyPlan()
        {
            var config = Config();
            config.Enabled = false;
            var plan = RecipeHydrologyPlanner.Build(
                Downslope(8, 8), null, new Vector2Int(8, 8), config, 1);
            Assert.AreEqual(0, plan.RiverCellCount);
            Assert.AreEqual(0, plan.LakeCellCount);
        }

        [Test]
        public void EveryCell_DrainsToBorderOrSink_NoCycles()
        {
            int w = 20, h = 20;
            var plan = RecipeHydrologyPlanner.Build(
                Downslope(w, h), null, new Vector2Int(w, h), Config(), 2);
            var parent = plan.FlowParent;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                int cur = x + y * w;
                var seen = new bool[w * h];
                int guard = w * h + 4;
                while (parent[cur % w, cur / w] >= 0 && guard-- > 0)
                {
                    cur = parent[cur % w, cur / w];
                    Assert.IsFalse(seen[cur], $"flow cycle through cell {cur}");
                    seen[cur] = true;
                }
                Assert.Greater(guard, 0, "unterminated downstream chain");
                Assert.AreEqual(-1, parent[cur % w, cur / w],
                    "chain must end at a sink or border cell");
                int sx = cur % w, sy = cur / w;
                Assert.IsTrue(sx == 0 || sy == 0 || sx == w - 1 || sy == h - 1,
                    $"non-border sink ({sx},{sy}) without a sink mask");
            }
        }

        [Test]
        public void RiverCells_HaveBedBelowSurface_AndRespectSourceMinimum()
        {
            int w = 24, h = 24;
            var terrain = Downslope(w, h);
            var config = Config();
            var plan = RecipeHydrologyPlanner.Build(
                terrain, null, new Vector2Int(w, h), config, 3);

            Assert.Greater(plan.RiverCellCount, 0, "downslope field must produce rivers");
            float depth = Mathf.Max(0.05f, config.ChannelDepthMeters);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y]) continue;
                Assert.IsFalse(float.IsNaN(plan.WaterSurface[x, y]), "river needs a surface");
                Assert.IsFalse(float.IsNaN(plan.BedHeight[x, y]), "river needs a bed");
                Assert.LessOrEqual(plan.BedHeight[x, y],
                    plan.WaterSurface[x, y] - depth + 0.0001f,
                    "bed must sit at least channel depth below the surface");
                Assert.GreaterOrEqual(terrain[x, y], config.RiverMinSourceMeters,
                    "river source terrain must clear the minimum");
            }
        }

        [Test]
        public void NonWaterCells_KeepNaNSurfaceAndBed()
        {
            int w = 16, h = 16;
            var config = Config();
            var terrain = Downslope(w, h);
            var plan = RecipeHydrologyPlanner.Build(
                terrain, null, new Vector2Int(w, h), config, 4);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (plan.RiverMask[x, y] || plan.LakeMask[x, y]) continue;
                // Border cells and low terrain are flood sinks — they carry a
                // surface deliberately as drainage outlets.
                bool sink = x == 0 || y == 0 || x == w - 1 || y == h - 1
                    || terrain[x, y] <= config.SinkMaxMeters;
                if (sink) continue;
                Assert.IsTrue(float.IsNaN(plan.WaterSurface[x, y]),
                    $"dry cell ({x},{y}) must not carry a water surface");
                Assert.IsTrue(float.IsNaN(plan.BedHeight[x, y]),
                    $"dry cell ({x},{y}) must not carry a bed height");
            }
        }

        [Test]
        public void WaterfallMask_RequiresRealDownstreamDrop()
        {
            int w = 12, h = 12;
            // Cliff at x=6: west half high, east half low — drains west,
            // so the drop edge sits at x=7 flowing down into x=6.
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = x >= 6 ? 8f - (w - 1 - x) : 2f - x * 0.1f;
            var config = Config();
            var plan = RecipeHydrologyPlanner.Build(
                terrain, null, new Vector2Int(w, h), config, 5);

            var parent = plan.FlowParent;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.WaterfallMask[x, y]) continue;
                Assert.IsTrue(plan.RiverMask[x, y] || plan.LakeMask[x, y],
                    "waterfall only on water cells");
                int p = parent[x, y];
                Assert.GreaterOrEqual(p, 0);
                Assert.GreaterOrEqual(plan.Filled[x, y] - plan.Filled[p % w, p / w],
                    config.WaterfallMinDropMeters - 0.0001f,
                    "waterfall must sit on a real drop");
                Assert.IsFalse(p % w == 0 || p / w == 0 || p % w == w - 1 || p / w == h - 1,
                    "no waterfall hanging off the world border");
            }
        }

        [Test]
        public void FlatTerrain_ProducesNoWaterfalls()
        {
            var t = new float[12, 12];
            for (int x = 0; x < 12; x++)
            for (int y = 0; y < 12; y++)
                t[x, y] = 1f + x * 0.01f; // barely-sloped, never a 0.4m drop
            var plan = RecipeHydrologyPlanner.Build(
                t, null, new Vector2Int(12, 12), Config(), 6);
            for (int x = 0; x < 12; x++)
            for (int y = 0; y < 12; y++)
                Assert.IsFalse(plan.WaterfallMask[x, y],
                    "flat-ish water must never spawn a waterfall strip");
        }

        [Test]
        public void Merge_UnionsMasks_AndKeepsFirstFiniteValues()
        {
            int w = 8, h = 8;
            var terrain = Downslope(w, h);
            var config = Config();
            var a = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), config, 7);
            var b = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), config, 8);
            // Seed an extra lake cell only in b to prove OR-merge.
            b.LakeMask[3, 3] = true;
            b.LakeCellCount++;
            b.WaterSurface[3, 3] = 1.5f;

            var merged = RecipeHydrologyPlanner.Merge(new[] { a, b });
            Assert.IsTrue(merged.LakeMask[3, 3], "merge must OR lake masks");
            Assert.AreEqual(1.5f, merged.WaterSurface[3, 3]);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (a.RiverMask[x, y] || b.RiverMask[x, y])
                    Assert.IsTrue(merged.RiverMask[x, y]);
        }

        [Test]
        public void SameSeed_SamePlan()
        {
            int w = 16, h = 16;
            var terrain = Downslope(w, h);
            var a = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 9);
            var b = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 9);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                Assert.AreEqual(a.RiverMask[x, y], b.RiverMask[x, y]);
                Assert.AreEqual(a.FlowParent[x, y], b.FlowParent[x, y]);
            }
        }
    }
}
