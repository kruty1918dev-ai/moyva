using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class RecipeHydrologyStoreTests
    {
        private static RecipeHydrologyPlan Plan()
        {
            const int w = 4, h = 4;
            var plan = new RecipeHydrologyPlan
            {
                RiverMask = new bool[w, h],
                LakeMask = new bool[w, h],
                WaterfallMask = new bool[w, h],
                WaterSurface = new float[w, h],
                BedHeight = new float[w, h],
                Filled = new float[w, h],
                FlowParent = new int[w, h],
                Accumulation = new float[w, h],
            };
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                plan.WaterSurface[x, y] = float.NaN;
                plan.BedHeight[x, y] = float.NaN;
                plan.FlowParent[x, y] = -1;
            }

            // River cell (1,1) pouring down into sink cell (2,1).
            plan.RiverMask[1, 1] = true;
            plan.WaterfallMask[1, 1] = true;
            plan.WaterSurface[1, 1] = 2.5f;
            plan.BedHeight[1, 1] = 2.15f;
            plan.Filled[1, 1] = 2.53f;
            plan.FlowParent[1, 1] = 2 + 1 * w;
            plan.WaterSurface[2, 1] = 0.2f;
            plan.Filled[2, 1] = 0.2f;
            // Lake cell (3,3).
            plan.LakeMask[3, 3] = true;
            plan.WaterSurface[3, 3] = 1.4f;
            plan.BedHeight[3, 3] = 0.9f;
            return plan;
        }

        [Test]
        public void EmptyStore_ReportsNoHydrology()
        {
            var store = new RecipeHydrologyStore();
            Assert.IsFalse(store.HasHydrology);
            Assert.IsFalse(store.IsWaterCell(new Vector2Int(1, 1)));
            Assert.IsFalse(store.TryGetWaterfall(new Vector2Int(1, 1), out _, out _, out _));
        }

        [Test]
        public void TryGetWaterfall_ReturnsDownstreamAndHeights()
        {
            var store = new RecipeHydrologyStore();
            store.Replace(Plan());

            Assert.IsTrue(store.TryGetWaterfall(
                new Vector2Int(1, 1), out Vector2Int down, out float upper, out float lower));
            Assert.AreEqual(new Vector2Int(2, 1), down);
            Assert.AreEqual(2.5f, upper, 0.0001f);
            Assert.AreEqual(0.2f, lower, 0.0001f);
        }

        [Test]
        public void NonWaterfallCell_ReturnsFalse()
        {
            var store = new RecipeHydrologyStore();
            store.Replace(Plan());
            Assert.IsFalse(store.TryGetWaterfall(new Vector2Int(0, 0), out _, out _, out _));
            Assert.IsFalse(store.TryGetWaterfall(new Vector2Int(3, 3), out _, out _, out _));
        }

        [Test]
        public void WaterQueries_MatchMasks()
        {
            var store = new RecipeHydrologyStore();
            store.Replace(Plan());

            Assert.IsTrue(store.IsRiverCell(new Vector2Int(1, 1)));
            Assert.IsTrue(store.IsLakeCell(new Vector2Int(3, 3)));
            Assert.IsTrue(store.IsWaterCell(new Vector2Int(3, 3)));
            Assert.IsFalse(store.IsWaterCell(new Vector2Int(0, 0)));

            Assert.IsTrue(store.TryGetWaterSurface(new Vector2Int(3, 3), out float y));
            Assert.AreEqual(1.4f, y, 0.0001f);
            Assert.IsFalse(store.TryGetWaterSurface(new Vector2Int(0, 0), out _));
        }

        [Test]
        public void NewQueries_BedFlowAndKind()
        {
            var store = new RecipeHydrologyStore();
            store.Replace(Plan());

            Assert.AreEqual(RecipeWaterKind.River, store.GetWaterKind(new Vector2Int(1, 1)));
            Assert.AreEqual(RecipeWaterKind.Lake, store.GetWaterKind(new Vector2Int(3, 3)));
            Assert.AreEqual(RecipeWaterKind.Sink, store.GetWaterKind(new Vector2Int(2, 1)));
            Assert.AreEqual(RecipeWaterKind.None, store.GetWaterKind(new Vector2Int(0, 0)));

            Assert.IsTrue(store.TryGetBedHeight(new Vector2Int(1, 1), out float bed));
            Assert.AreEqual(2.15f, bed, 0.0001f);
            Assert.IsFalse(store.TryGetBedHeight(new Vector2Int(0, 0), out _));

            Assert.IsTrue(store.TryGetFlowDirection(new Vector2Int(1, 1), out Vector2Int down));
            Assert.AreEqual(new Vector2Int(2, 1), down);
            // Lake cell has no flow parent in this fixture.
            Assert.IsFalse(store.TryGetFlowDirection(new Vector2Int(3, 3), out _));
            Assert.IsFalse(store.TryGetFlowDirection(new Vector2Int(0, 0), out _));
        }

        [Test]
        public void Clear_ResetsStore()
        {
            var store = new RecipeHydrologyStore();
            store.Replace(Plan());
            store.Clear();
            Assert.IsFalse(store.HasHydrology);
            Assert.IsFalse(store.TryGetWaterfall(new Vector2Int(1, 1), out _, out _, out _));
        }
    }
}
