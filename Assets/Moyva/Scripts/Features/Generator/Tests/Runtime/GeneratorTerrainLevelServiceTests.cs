using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Tests for the derived surface-normal query on the terrain level service.
    /// </summary>
    [TestFixture]
    public sealed class GeneratorTerrainLevelServiceTests
    {
        private IGeneratorTerrainLevelService _service;

        [SetUp]
        public void Setup()
        {
            _service = new GeneratorTerrainLevelService();
        }

        [Test]
        public void TryGetSurfaceNormal_FlatMap_ReturnsUp()
        {
            _service.SetSurfaceHeightMap(UniformHeights(8, 8, 1.5f));

            Assert.IsTrue(_service.TryGetSurfaceNormal(new Vector2Int(4, 4), 1f, out Vector3 normal));
            Assert.AreEqual(0f, normal.x, 0.0001f);
            Assert.AreEqual(1f, normal.y, 0.0001f);
            Assert.AreEqual(0f, normal.z, 0.0001f);
        }

        [Test]
        public void TryGetSurfaceNormal_RisingEastward_TiltsNormalWest()
        {
            var heights = new float[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                heights[x, y] = x; // +1 per step east
            _service.SetSurfaceHeightMap(heights);

            Assert.IsTrue(_service.TryGetSurfaceNormal(new Vector2Int(4, 4), 1f, out Vector3 normal));
            Assert.Less(normal.x, -0.5f, "Normal must tilt away from the uphill side");
            Assert.Greater(normal.y, 0.5f);
            Assert.AreEqual(0f, normal.z, 0.0001f);
        }

        [Test]
        public void TryGetSurfaceNormal_EdgeCell_DoesNotCreateFalseSlope()
        {
            // A flat map ending at x=0 must not tilt toward missing neighbors.
            _service.SetSurfaceHeightMap(UniformHeights(8, 8, 2f));

            Assert.IsTrue(_service.TryGetSurfaceNormal(new Vector2Int(0, 4), 1f, out Vector3 normal));
            Assert.AreEqual(Vector3.up, normal,
                "Out-of-bounds neighbors must fall back to the center height, not zero");
        }

        [Test]
        public void TryGetSurfaceNormal_MissingMap_ReturnsFalse()
        {
            Assert.IsFalse(_service.TryGetSurfaceNormal(new Vector2Int(0, 0), 1f, out Vector3 normal));
            Assert.AreEqual(Vector3.up, normal);
        }

        [Test]
        public void TryGetSurfaceNormal_OutOfBounds_ReturnsFalse()
        {
            _service.SetSurfaceHeightMap(UniformHeights(4, 4, 1f));
            Assert.IsFalse(_service.TryGetSurfaceNormal(new Vector2Int(-1, 0), 1f, out _));
            Assert.IsFalse(_service.TryGetSurfaceNormal(new Vector2Int(4, 0), 1f, out _));
        }

        private static float[,] UniformHeights(int width, int height, float value)
        {
            var map = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = value;
            return map;
        }
    }
}
