using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class RecipeHydrologyPlannerTests
    {
        private static RecipeHydrologyConfig Config(float sinkMax = -1f)
        {
            return new RecipeHydrologyConfig
            {
                Enabled = true,
                SinkMaxMeters = sinkMax,
                RiverAccumulationThreshold = 4,
                RiverMinSourceMeters = 0.1f,
                RiverMaxFraction = 0.5f,
                LakeMinDepthMeters = 0.5f,
                LakeMaxFraction = 0.5f,
                WaterSurfaceOffsetMeters = -0.03f,
                WaterfallMinDropMeters = 0.5f,
                SeedSalt = 7331,
            };
        }

        [Test]
        public void Disabled_ReturnsEmptyPlan()
        {
            var terrain = new float[8, 8];
            var config = Config();
            config.Enabled = false;
            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(8, 8), config, 1);
            Assert.AreEqual(0, plan.RiverCellCount);
            Assert.AreEqual(0, plan.LakeCellCount);
        }

        [Test]
        public void Deterministic_SameSeedSameMasks()
        {
            var terrain = new float[16, 16];
            for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                terrain[x, y] = (15 - x) * 0.5f + Mathf.Abs(y - 7.5f) * 0.25f;

            var config = Config();
            var first = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(16, 16), config, 42);
            var second = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(16, 16), config, 42);

            Assert.AreEqual(first.RiverCellCount, second.RiverCellCount);
            Assert.AreEqual(first.LakeCellCount, second.LakeCellCount);
            for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
            {
                Assert.AreEqual(first.RiverMask[x, y], second.RiverMask[x, y]);
                Assert.AreEqual(first.WaterSurface[x, y], second.WaterSurface[x, y]);
            }
        }

        [Test]
        public void Valley_RiverFollowsCenterAndDrainsToBorder()
        {
            // Valley: height rises away from the center row and toward the west,
            // so drainage converges on the center row and exits east.
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = (w - 1 - x) * 0.5f + Mathf.Abs(y - (h - 1) * 0.5f) * 1f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 7);

            Assert.Greater(plan.RiverCellCount, 0);
            int centerRiverCells = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y])
                    continue;
                if (y == 7 || y == 8)
                    centerRiverCells++;
                Assert.IsFalse(float.IsNaN(plan.WaterSurface[x, y]));
            }
            Assert.Greater(centerRiverCells, 0,
                "The valley center row must carry river cells.");

            // The drainage spine is the center row: the highest accumulation
            // on the map must sit there (off the border, which is always a
            // sink and never a river cell).
            float maxAcc = -1f;
            int maxX = -1, maxY = -1;
            for (int x = 1; x < w - 1; x++)
            for (int y = 1; y < h - 1; y++)
            {
                if (plan.Accumulation[x, y] > maxAcc)
                {
                    maxAcc = plan.Accumulation[x, y];
                    maxX = x;
                    maxY = y;
                }
            }
            Assert.IsTrue(maxY == 7 || maxY == 8,
                $"Accumulation maximum must sit on the valley spine, got ({maxX},{maxY}).");
            Assert.IsTrue(plan.RiverMask[maxX, maxY],
                "The spine cell with maximum accumulation must be a river cell.");
        }

        [Test]
        public void Depression_BecomesLakeWithFilledSurface()
        {
            // 10x10 plateau at 2 m with a 3x3 pit at 0.5 m in the middle.
            var terrain = new float[10, 10];
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                terrain[x, y] = 2f;
            for (int x = 4; x <= 6; x++)
            for (int y = 4; y <= 6; y++)
                terrain[x, y] = 0.5f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(10, 10), Config(), 3);

            Assert.AreEqual(9, plan.LakeCellCount);
            Assert.IsTrue(plan.LakeMask[5, 5]);
            // Lake surface equals the flooded rim height plus offset.
            Assert.AreEqual(2f - 0.03f, plan.WaterSurface[5, 5], 0.001f);
        }

        [Test]
        public void SinkLayerMask_StopsRiversEarly()
        {
            const int w = 16, h = 8;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = (w - 1 - x) * 0.5f;

            // Ocean covering the eastern half of the map.
            var sink = new bool[w, h];
            for (int x = 10; x < w; x++)
            for (int y = 0; y < h; y++)
                sink[x, y] = true;

            var plan = RecipeHydrologyPlanner.Build(terrain, sink, new Vector2Int(w, h), Config(), 5);

            Assert.Greater(plan.RiverCellCount, 0);
            // The terminal receiver cell at the sink edge may be marked so the
            // mouth ends in visible water, but rivers must never continue
            // through the sink: columns deeper than the entry stay unmarked.
            for (int x = 11; x < w; x++)
            for (int y = 0; y < h; y++)
                Assert.IsFalse(plan.RiverMask[x, y], "River must not run through the sink.");
        }

        [Test]
        public void RiverDescendsAlongFlowParent()
        {
            const int w = 16, h = 8;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = (w - 1 - x) * 0.5f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 9);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y])
                    continue;
                int parent = plan.FlowParent[x, y];
                // A terminal receiver at the map border is a flood root:
                // it drains out of the map and legitimately has no parent.
                bool isBorder = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                if (parent < 0)
                {
                    Assert.IsTrue(isBorder,
                        $"River cell ({x},{y}) without a downstream parent must lie on the map border.");
                    continue;
                }
                int px = parent % w;
                int py = parent / w;
                Assert.GreaterOrEqual(
                    plan.Filled[x, y] + 0.0001f, plan.Filled[px, py],
                    "Water must not flow uphill.");
            }
        }

        [Test]
        public void CliffDrop_MarksWaterfall()
        {
            // West plateau at 2 m, east lowland at 0.25 m; a cliff at x == 8.
            const int w = 16, h = 8;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = x < 8 ? 2f : 0.25f;

            var config = Config();
            config.RiverAccumulationThreshold = 2;
            config.RiverMinSourceMeters = 0.1f;
            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), config, 11);

            Assert.Greater(plan.RiverCellCount, 0);
            bool anyWaterfall = false;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (plan.WaterfallMask[x, y])
                    anyWaterfall = true;
            Assert.IsTrue(anyWaterfall, "Expected at least one waterfall cell at the cliff.");
        }

        [Test]
        public void RiverCells_DrainToSinkWithoutCycles()
        {
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = (w - 1 - x) * 0.5f + Mathf.Abs(y - 7.5f) * 0.5f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 21);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y])
                    continue;
                // Walk the flow-parent chain: it must terminate at a sink
                // (parent -1) within w*h steps — a cycle can never terminate.
                int index = x + y * w;
                int steps = 0;
                while (index >= 0 && steps <= w * h)
                {
                    index = plan.FlowParent[index % w, index / w];
                    steps++;
                }
                Assert.LessOrEqual(steps, w * h,
                    $"River cell ({x},{y}) does not reach a sink — flow cycle.");
            }
        }

        [Test]
        public void LakeCells_ShareBasinSurface()
        {
            // 10x10 plateau at 2 m with a 3x3 pit at 0.5 m in the middle.
            var terrain = new float[10, 10];
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                terrain[x, y] = 2f;
            for (int x = 4; x <= 6; x++)
            for (int y = 4; y <= 6; y++)
                terrain[x, y] = 0.5f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(10, 10), Config(), 3);

            float surface = float.NaN;
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
            {
                if (!plan.LakeMask[x, y])
                    continue;
                if (float.IsNaN(surface))
                    surface = plan.WaterSurface[x, y];
                Assert.AreEqual(surface, plan.WaterSurface[x, y], 0.0001f,
                    $"Lake cell ({x},{y}) breaks basin level.");
            }
            Assert.IsFalse(float.IsNaN(surface), "No lake cells found.");
        }

        [Test]
        public void WaterCells_BedBelowSurface()
        {
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = (w - 1 - x) * 0.5f + Mathf.Abs(y - 7.5f) * 0.5f;

            var config = Config();
            config.ChannelDepthMeters = 0.35f;
            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), config, 33);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y] && !plan.LakeMask[x, y])
                    continue;
                float bed = plan.BedHeight[x, y];
                float surface = plan.WaterSurface[x, y];
                Assert.IsFalse(float.IsNaN(bed), $"Water cell ({x},{y}) has no bed.");
                Assert.Greater(surface, bed, $"Water cell ({x},{y}): bed above surface.");
                if (plan.RiverMask[x, y])
                {
                    Assert.LessOrEqual(bed, surface - config.ChannelDepthMeters + 0.0001f,
                        $"River bed ({x},{y}) must carve at least channelDepth.");
                    Assert.LessOrEqual(bed, terrain[x, y] + 0.0001f,
                        $"River bed ({x},{y}) must not float above the floor.");
                }
            }
        }

        [Test]
        public void Accumulation_MonotoneAlongFlowParent()
        {
            // Terraced slope: 0.5 m plateaus of equal flooded height — the
            // case that broke plain descending-sort accumulation. Every
            // child's accumulation must reach its parent, so a parent can
            // never hold less than a child.
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = Mathf.Floor((w - 1 - x) * 0.5f) * 0.5f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 17);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                int parent = plan.FlowParent[x, y];
                if (parent < 0)
                    continue;
                int px = parent % w;
                int py = parent / w;
                Assert.GreaterOrEqual(
                    plan.Accumulation[px, py] + 0.0001f, plan.Accumulation[x, y],
                    $"acc[parent ({px},{py})] < acc[child ({x},{y})] — accumulation order broken.");
            }
        }

        [Test]
        public void RiverCells_ReachesReceiverThroughMarkedCells()
        {
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = Mathf.Floor((w - 1 - x) * 0.5f) * 0.5f
                    + Mathf.Abs(y - 7.5f) * 0.25f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 23);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y])
                    continue;

                // Walk the flow chain: every cell on it must be water
                // (river or lake) until the chain terminates at a receiver.
                int index = x + y * w;
                for (int steps = 0; steps <= w * h; steps++)
                {
                    int cx = index % w;
                    int cy = index / w;
                    Assert.IsTrue(
                        plan.RiverMask[cx, cy] || plan.LakeMask[cx, cy]
                            || !float.IsNaN(plan.WaterSurface[cx, cy]),
                        $"River chain from ({x},{y}) breaks at ({cx},{cy}) — land on the channel.");
                    int p = plan.FlowParent[cx, cy];
                    if (p < 0)
                        break;
                    index = p;
                    if (steps == w * h)
                        Assert.Fail($"River cell ({x},{y}) chain never terminates.");
                }
            }
        }

        [Test]
        public void RiverCells_AreFourConnected()
        {
            const int w = 16, h = 16;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = Mathf.Floor((w - 1 - x) * 0.5f) * 0.5f
                    + Mathf.Abs(y - 7.5f) * 0.25f;

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 29);

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!plan.RiverMask[x, y])
                    continue;

                bool waterNeighbor = false;
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + dx[d];
                    int ny = y + dy[d];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    {
                        waterNeighbor = true; // border drain counts
                        break;
                    }
                    if (plan.RiverMask[nx, ny] || plan.LakeMask[nx, ny]
                        || !float.IsNaN(plan.WaterSurface[nx, ny]))
                    {
                        waterNeighbor = true;
                        break;
                    }
                }
                Assert.IsTrue(waterNeighbor,
                    $"River cell ({x},{y}) has no cardinal water neighbour — orphan puddle.");
            }
        }

        [Test]
        public void LakeInterior_NoFalseWaterfalls()
        {
            // Pit lake on a plateau: floor terrain varies under a flat water
            // surface. The waterfall check compares flooded levels, so no
            // lake cell may be marked — the outflow is level by construction.
            const int w = 12, h = 12;
            var terrain = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                terrain[x, y] = 2f;
            for (int x = 4; x <= 7; x++)
            for (int y = 4; y <= 7; y++)
                terrain[x, y] = 0.5f;
            terrain[6, 6] = 1.2f; // uneven floor under the same sheet

            var plan = RecipeHydrologyPlanner.Build(terrain, null, new Vector2Int(w, h), Config(), 5);

            Assert.Greater(plan.LakeCellCount, 0);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                Assert.IsFalse(
                    plan.LakeMask[x, y] && plan.WaterfallMask[x, y],
                    $"Lake cell ({x},{y}) marked as waterfall on a level surface.");
        }
    }
}
