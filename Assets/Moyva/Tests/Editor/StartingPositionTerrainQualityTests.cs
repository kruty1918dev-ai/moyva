using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Startup
{
    /// <summary>
    /// Start-territory adequacy (P021): a candidate must provide one
    /// contiguous buildable pad, not merely a high land ratio. Speckled
    /// archipelagos and water-covered centers are hard-invalid even when the
    /// sampled window is mostly land.
    /// </summary>
    public sealed class StartingPositionTerrainQualityTests
    {
        private const int Size = 32;
        private static readonly Vector2Int Center = new Vector2Int(16, 16);

        private StartingPositionTerrainQualityEvaluator _evaluator;

        [SetUp]
        public void SetUp()
        {
            _evaluator = new StartingPositionTerrainQualityEvaluator(
                new StartingPositionInitializerSettings
                {
                    startTerrainSampleRadius = 3,
                    minimumLandRatioAroundStart = 0.5f,
                    minimumConnectedLandTiles = 9,
                    preferWaterNearStart = false,
                });
        }

        [Test]
        public void FullyConnectedLand_IsHardValid()
        {
            var signal = BuildSignal();

            var quality = _evaluator.Evaluate(signal, Center);

            Assert.IsTrue(quality.HardValid, quality.Reason);
            Assert.AreEqual(49, quality.ConnectedLandTiles);
        }

        [Test]
        public void WaterCenter_IsHardInvalidDespiteHighLandRatio()
        {
            var signal = BuildSignal();
            signal.TileMap[Center.x, Center.y] = "deep_water";

            var quality = _evaluator.Evaluate(signal, Center);

            Assert.IsFalse(quality.HardValid, quality.Reason);
            Assert.AreEqual(0, quality.ConnectedLandTiles);
            Assert.Greater(
                quality.LandRatio,
                0.9f,
                "Land ratio alone must not rescue a water-covered start.");
        }

        [Test]
        public void IsolatedPadInsideWaterRing_IsHardInvalid()
        {
            var signal = BuildSignal();
            // Centre cell is land but the whole ring-1 is water: only one
            // connected land tile remains while the ratio stays high.
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                signal.TileMap[Center.x + dx, Center.y + dy] = "deep_water";
            }

            var quality = _evaluator.Evaluate(signal, Center);

            Assert.AreEqual(1, quality.ConnectedLandTiles);
            Assert.IsFalse(quality.HardValid, quality.Reason);
        }

        [Test]
        public void NarrowIsthmus_KeepsRegionHardValid()
        {
            var signal = BuildSignal();
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                signal.TileMap[Center.x + dx, Center.y + dy] = "deep_water";
            }
            // Re-open a single-cell corridor to the outer land.
            signal.TileMap[Center.x + 1, Center.y] = "grass";

            var quality = _evaluator.Evaluate(signal, Center);

            Assert.IsTrue(quality.HardValid, quality.Reason);
            Assert.AreEqual(
                49 - 7,
                quality.ConnectedLandTiles);
        }

        [Test]
        public void ConnectedGateDisabled_LandRatioAloneDecides()
        {
            _evaluator = new StartingPositionTerrainQualityEvaluator(
                new StartingPositionInitializerSettings
                {
                    startTerrainSampleRadius = 3,
                    minimumLandRatioAroundStart = 0.5f,
                    minimumConnectedLandTiles = 0,
                    preferWaterNearStart = false,
                });
            var signal = BuildSignal();
            signal.TileMap[Center.x, Center.y] = "deep_water";

            var quality = _evaluator.Evaluate(signal, Center);

            Assert.IsTrue(quality.HardValid, quality.Reason);
        }

        private static WorldGeneratedDataSignal BuildSignal()
        {
            var heights = new float[Size, Size];
            var tiles = new string[Size, Size];
            for (int x = 0; x < Size; x++)
            for (int y = 0; y < Size; y++)
            {
                heights[x, y] = 0.5f;
                tiles[x, y] = "grass";
            }

            return new WorldGeneratedDataSignal
            {
                Width = Size,
                Height = Size,
                HeightMap = heights,
                TileMap = tiles,
                Source = WorldGeneratedDataSource.GeneratedHost,
            };
        }
    }
}
