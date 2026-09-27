using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G16 acceptance invariants for waterfall fronts: a fall exists only on a
    /// real water-to-water drop ≥ minDrop (never flat water, never a drop onto
    /// dry land, never a world-border drain), contiguous same-direction edges
    /// merge into one front, corner diagonals suppressed when orthogonal pours
    /// cover the same ledge, and anchors are deterministic.
    /// </summary>
    public sealed class G16AcceptanceTests
    {
        private const float MinDrop = 0.5f;

        private static void Grid(int w, int h,
            out bool[,] sheet, out float[,] surfaces, out bool[,] target,
            float fill = 5f)
        {
            sheet = new bool[w, h];
            surfaces = new float[w, h];
            target = new bool[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                surfaces[x, y] = fill;
        }

        [Test]
        public void RealDrop_ProducesFront()
        {
            Grid(8, 8, out var sheet, out var surf, out var target);
            // High water at (3,3) pours east onto low water at (4,3).
            sheet[3, 3] = true; surf[3, 3] = 5f; target[3, 3] = true;
            sheet[4, 3] = true; surf[4, 3] = 4f; target[4, 3] = true;

            var field = WaterfallFieldPlanner.Build(8, 8, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(1, field.Fronts.Count);
            var front = field.Fronts[0];
            Assert.AreEqual(1, front.WidthCells);
            Assert.AreEqual(new Vector2Int(1, 0), front.Dir);
            Assert.AreEqual(1f, front.Drop, 0.001f);
            Assert.IsTrue(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(1, 0)),
                "front must mark its drop edge covered");
        }

        [Test]
        public void FlatWater_ProducesNoFront()
        {
            Grid(8, 8, out var sheet, out var surf, out var target);
            for (int x = 2; x <= 4; x++)
            {
                sheet[x, 3] = true; target[x, 3] = true; surf[x, 3] = 5f;
            }
            var field = WaterfallFieldPlanner.Build(8, 8, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(0, field.Fronts.Count, "flat water must not spawn falls");
        }

        [Test]
        public void DropOntoDryLand_ProducesNoFront()
        {
            Grid(8, 8, out var sheet, out var surf, out var target);
            sheet[3, 3] = true; surf[3, 3] = 5f; target[3, 3] = true;
            // (4,3) is lower but has NO hydrology water → dry cliff, not a fall.
            surf[4, 3] = 4f;

            var field = WaterfallFieldPlanner.Build(8, 8, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(0, field.Fronts.Count,
                "pour onto a dry cell must not emit a waterfall");
        }

        [Test]
        public void DropBelowThreshold_ProducesNoFront()
        {
            Grid(8, 8, out var sheet, out var surf, out var target);
            sheet[3, 3] = true; surf[3, 3] = 5f; target[3, 3] = true;
            sheet[4, 3] = true; surf[4, 3] = 5f - MinDrop + 0.01f; target[4, 3] = true;

            var field = WaterfallFieldPlanner.Build(8, 8, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(0, field.Fronts.Count, "sub-threshold drop must not fall");
        }

        [Test]
        public void ContiguousSameDirectionEdges_MergeIntoOneFront()
        {
            Grid(10, 4, out var sheet, out var surf, out var target);
            // A 3-cell ledge pouring south onto a low water row.
            for (int x = 2; x <= 4; x++)
            {
                sheet[x, 2] = true; surf[x, 2] = 5f; target[x, 2] = true;
                sheet[x, 3] = true; surf[x, 3] = 4f; target[x, 3] = true;
            }
            var field = WaterfallFieldPlanner.Build(10, 4, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(1, field.Fronts.Count, "one ledge = one front");
            Assert.AreEqual(3, field.Fronts[0].WidthCells);
            Assert.AreEqual(new Vector2Int(0, 1), field.Fronts[0].Dir);
        }

        [Test]
        public void HeightStep_SplitsFront()
        {
            Grid(10, 4, out var sheet, out var surf, out var target);
            for (int x = 2; x <= 4; x++)
            {
                sheet[x, 2] = true; target[x, 2] = true;
                sheet[x, 3] = true; surf[x, 3] = 4f; target[x, 3] = true;
            }
            surf[2, 2] = 5f; surf[3, 2] = 5f;
            surf[4, 2] = 7f; // last edge sits on a different terrace
            var field = WaterfallFieldPlanner.Build(10, 4, 1f, sheet, surf, target, MinDrop);
            int southFronts = 0;
            foreach (var f in field.Fronts)
                if (f.Dir == new Vector2Int(0, 1))
                    southFronts++;
            Assert.AreEqual(2, southFronts,
                "a height-divergent edge must split into its own front");
        }

        [Test]
        public void CornerDiagonal_IsSuppressed_WhenOrthoPoursCoverLedge()
        {
            Grid(6, 6, out var sheet, out var surf, out var target);
            // Upper cell pours west AND south; the diagonal SW edge is the same
            // ledge corner and must not double-emit.
            sheet[3, 3] = true; surf[3, 3] = 5f; target[3, 3] = true;
            sheet[2, 3] = true; surf[2, 3] = 4f; target[2, 3] = true;
            sheet[3, 4] = true; surf[3, 4] = 4f; target[3, 4] = true;
            sheet[2, 4] = true; surf[2, 4] = 4f; target[2, 4] = true;

            var field = WaterfallFieldPlanner.Build(6, 6, 1f, sheet, surf, target, MinDrop);
            Assert.IsFalse(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(-1, 1)),
                "cornered diagonal must be suppressed by its ortho pours");
            int diagonalEdges = 0;
            foreach (var f in field.Fronts)
                if (f.Dir.x != 0 && f.Dir.y != 0)
                    diagonalEdges += f.WidthCells;
            Assert.AreEqual(0, diagonalEdges);
        }

        [Test]
        public void Anchor_IsDeterministic_MiddleEdgeCell()
        {
            Grid(10, 4, out var sheet, out var surf, out var target);
            for (int x = 2; x <= 4; x++)
            {
                sheet[x, 2] = true; surf[x, 2] = 5f; target[x, 2] = true;
                sheet[x, 3] = true; surf[x, 3] = 4f; target[x, 3] = true;
            }
            var a = WaterfallFieldPlanner.Build(10, 4, 1f, sheet, surf, target, MinDrop);
            var b = WaterfallFieldPlanner.Build(10, 4, 1f, sheet, surf, target, MinDrop);
            Assert.AreEqual(a.Fronts[0].Anchor, b.Fronts[0].Anchor);
            Assert.AreEqual(new Vector2Int(3, 2), a.Fronts[0].Anchor,
                "3-cell front anchors on its middle edge");
        }

        [Test]
        public void DisabledOrNullInputs_ReturnEmptyField()
        {
            Grid(4, 4, out var sheet, out var surf, out var target);
            var f1 = WaterfallFieldPlanner.Build(4, 4, 1f, sheet, surf, target, 0f);
            Assert.AreEqual(0, f1.Fronts.Count, "zero minDrop disables the field");
            var f2 = WaterfallFieldPlanner.Build(4, 4, 1f, null, surf, target, MinDrop);
            Assert.AreEqual(0, f2.Fronts.Count);
        }
    }
}
