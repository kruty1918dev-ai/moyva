using Kruty1918.Moyva.FogOfWar.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Plain NUnit tests for FogNoiseGenerator.
    /// No Zenject or Unity runtime dependency.
    /// </summary>
    [TestFixture]
    public class FogNoiseGeneratorTests
    {
        // ─── 1. Generate_ReturnsCorrectSize ──────────────────────────────────

        [Test]
        public void Generate_ReturnsCorrectSize()
        {
            var gen = new FogNoiseGenerator(42);
            var map = gen.Generate(8, 12);
            Assert.AreEqual(8,  map.GetLength(0));
            Assert.AreEqual(12, map.GetLength(1));
        }

        // ─── 2. Generate_ValuesInRangeZeroToOne ──────────────────────────────

        [Test]
        public void Generate_ValuesInRangeZeroToOne()
        {
            var gen = new FogNoiseGenerator(0);
            var map = gen.Generate(10, 10);
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                {
                    Assert.GreaterOrEqual(map[x, y], 0f, $"Value at ({x},{y}) < 0");
                    Assert.LessOrEqual   (map[x, y], 1f, $"Value at ({x},{y}) > 1");
                }
        }

        // ─── 3. Generate_SameSeed_SameOutput ─────────────────────────────────

        [Test]
        public void Generate_SameSeed_SameOutput()
        {
            var gen1 = new FogNoiseGenerator(1337);
            var gen2 = new FogNoiseGenerator(1337);

            var map1 = gen1.Generate(5, 5);
            var map2 = gen2.Generate(5, 5);

            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    Assert.AreEqual(map1[x, y], map2[x, y],
                        $"Different values at ({x},{y}) with same seed.");
        }

        // ─── 4. Generate_DifferentSeeds_DifferentOutput ──────────────────────

        [Test]
        public void Generate_DifferentSeeds_DifferentOutput()
        {
            var gen1 = new FogNoiseGenerator(1);
            var gen2 = new FogNoiseGenerator(9999);

            var map1 = gen1.Generate(8, 8);
            var map2 = gen2.Generate(8, 8);

            bool anyDifference = false;
            for (int x = 0; x < 8 && !anyDifference; x++)
                for (int y = 0; y < 8 && !anyDifference; y++)
                    if (map1[x, y] != map2[x, y])
                        anyDifference = true;

            Assert.IsTrue(anyDifference, "Different seeds produced identical noise maps.");
        }
    }
}
