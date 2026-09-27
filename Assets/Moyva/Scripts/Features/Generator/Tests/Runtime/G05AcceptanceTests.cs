using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Geography;
using NUnit.Framework;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G05 acceptance invariants for the coast mask: continuous shoreline
    /// semantics — beach only on low land that touches water (orthogonal or
    /// diagonal, ocean/lake/river alike), cliff coasts on elevated edges,
    /// shallow banding near land, and never a beach cell on water itself.
    /// </summary>
    public sealed class G05AcceptanceTests
    {
        private static WorldGenerationConfig Config() => new WorldGenerationConfig();

        private static WorldGenerationRequest Request(
            int seed, int width, int height, WorldGenerationConfig config = null)
        {
            config ??= Config();
            var t = config.TerrainLevels;
            return new WorldGenerationRequest(
                seed, width, height, WorldArchetype.Balanced, 2, config,
                t.WaterLevel, t.ShoreLevel, t.LandLevel, t.HillLevel, t.MaxLevel,
                t.HeightStep, t.WaterSurfaceOffset);
        }

        private static bool[,] Empty(int w, int h) => new bool[w, h];

        private static float[,] DeepWaterSurface(int w, int h, int waterLevel, float step, float offset)
        {
            var s = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                s[x, y] = waterLevel * step + offset;
            return s;
        }

        [Test]
        public void LowLand_TouchingWater_BecomesBeach_AndSnapsToShoreLevel()
        {
            var t = Config().TerrainLevels;
            var levels = new int[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                levels[x, y] = t.LandLevel;
            levels[0, 0] = t.WaterLevel;
            var surface = DeepWaterSurface(8, 8, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            var beach = new CoastStage().Generate(
                Request(1, 8, 8), levels, Empty(8, 8), Empty(8, 8), surface);

            // Orthogonal neighbour of water.
            Assert.IsTrue(beach[1, 0], "land adjacent to water must be beach");
            Assert.AreEqual(t.ShoreLevel, levels[1, 0], "beach cell snaps to shore level");
            // Diagonal neighbour of water.
            Assert.IsTrue(beach[1, 1], "diagonal water contact must count");
            // Far from water → not beach.
            Assert.IsFalse(beach[7, 7]);
            Assert.AreEqual(t.LandLevel, levels[7, 7]);
        }

        [Test]
        public void ElevatedCoast_IsCliff_NotBeach()
        {
            var t = Config().TerrainLevels;
            var levels = new int[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                levels[x, y] = t.HillLevel;
            levels[0, 0] = t.WaterLevel;
            var surface = DeepWaterSurface(8, 8, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            var beach = new CoastStage().Generate(
                Request(2, 8, 8), levels, Empty(8, 8), Empty(8, 8), surface);

            Assert.IsFalse(beach[1, 0], "elevated coast is a cliff, not a beach");
            Assert.AreEqual(t.HillLevel, levels[1, 0], "cliff elevation must be preserved");
        }

        [Test]
        public void InteriorLake_AndRiver_ProduceBeach()
        {
            var t = Config().TerrainLevels;
            var levels = new int[10, 10];
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                levels[x, y] = t.LandLevel;

            var lake = Empty(10, 10);
            lake[5, 5] = true;
            var river = Empty(10, 10);
            river[2, 2] = river[2, 3] = true;
            var surface = DeepWaterSurface(10, 10, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            var beach = new CoastStage().Generate(
                Request(3, 10, 10), levels, lake, river, surface);

            Assert.IsTrue(beach[5, 6], "lake shore must be beach");
            Assert.IsTrue(beach[2, 4], "river bank must be beach");
        }

        [Test]
        public void NarrowStrait_BothSidesBeach_AndWaterNeverBeach()
        {
            var t = Config().TerrainLevels;
            var levels = new int[12, 4];
            for (int x = 0; x < 12; x++)
            for (int y = 0; y < 4; y++)
                levels[x, y] = t.LandLevel;
            // One-cell-wide water strait down the middle.
            for (int y = 0; y < 4; y++)
                levels[6, y] = t.WaterLevel;
            var surface = DeepWaterSurface(12, 4, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            var beach = new CoastStage().Generate(
                Request(4, 12, 4), levels, Empty(12, 4), Empty(12, 4), surface);

            for (int y = 0; y < 4; y++)
            {
                Assert.IsTrue(beach[5, y], "west strait bank must be beach");
                Assert.IsTrue(beach[7, y], "east strait bank must be beach");
                Assert.IsFalse(beach[6, y], "water itself is never beach");
            }
        }

        [Test]
        public void MapCorner_Water_ContactsBeach()
        {
            var t = Config().TerrainLevels;
            var levels = new int[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                levels[x, y] = t.LandLevel;
            // Water touching two map edges at the corner.
            levels[0, 0] = levels[0, 1] = levels[1, 0] = t.WaterLevel;
            var surface = DeepWaterSurface(8, 8, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            var beach = new CoastStage().Generate(
                Request(5, 8, 8), levels, Empty(8, 8), Empty(8, 8), surface);

            Assert.IsTrue(beach[1, 1], "corner diagonal water contact must be beach");
        }

        [Test]
        public void ShallowBand_RaisesWaterNearLand_ButNeverBelowDeep()
        {
            var t = Config().TerrainLevels;
            var config = Config();
            config.Coast.ShallowBandWidth = 2;
            var levels = new int[16, 16];
            for (int x = 0; x < 16; x++)
            for (int y = 0; y < 16; y++)
                levels[x, y] = t.WaterLevel;
            // Land peninsula on the west edge.
            for (int y = 0; y < 16; y++)
                levels[0, y] = t.LandLevel;
            float deep = t.WaterLevel * t.HeightStep + t.WaterSurfaceOffset;
            var surface = DeepWaterSurface(16, 16, t.WaterLevel, t.HeightStep, t.WaterSurfaceOffset);

            new CoastStage().Generate(
                Request(6, 16, 16, config), levels, Empty(16, 16), Empty(16, 16), surface);

            Assert.Greater(surface[1, 1], deep, "water next to land must be shallowed");
            Assert.GreaterOrEqual(surface[15, 15], deep, "deep water never goes below deep surface");
            Assert.AreEqual(deep, surface[15, 15], "far water keeps the deep surface");
        }
    }
}
