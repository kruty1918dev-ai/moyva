using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Plain NUnit tests for FogVisibilityResolver.
    /// No Zenject dependency.
    /// </summary>
    [TestFixture]
    public class FogVisibilityResolverTests
    {
        private const int MapW = 20;
        private const int MapH = 20;

        private class StubGridService : IGridService
        {
            public int GridWidth  => MapW;
            public int GridHeight => MapH;
            public string GetTileData(Vector2Int p) => "grass";
            public bool TryGetTileData(Vector2Int p, out string id) { id = "grass"; return true; }
            public void SetTileData(Vector2Int p, string id) { }
        }

        private FogVisibilityResolver _resolver;
        private HeightAwareVisionService _heightVisionService;
        private FogOfWarSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<FogOfWarSettings>();
            _settings.MaxVisionRange = 8;
            _settings.ElevationStep = 0.25f;
            _settings.MaxObserverHeightBonus = 4;
            _settings.MaxDownhillVisionBonus = 2;
            _settings.MaxUphillVisionPenalty = 4;
            _heightVisionService = new HeightAwareVisionService(_settings);
            _resolver = new FogVisibilityResolver(new StubGridService(), _heightVisionService, _settings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        // ─── 1. AlwaysIncludesOrigin ──────────────────────────────────────────

        [Test]
        public void ComputeVisibleTiles_AlwaysIncludesOrigin()
        {
            var origin = new Vector2Int(5, 5);
            var result = _resolver.ComputeVisibleTiles(origin, 3, MapW, MapH);
            Assert.Contains(origin, new List<Vector2Int>(result));
        }

        // ─── 2. RangeZero_ReturnsOnlyOrigin ──────────────────────────────────

        [Test]
        public void ComputeVisibleTiles_RangeZero_ClampsToMinimumRadiusOne()
        {
            var origin = new Vector2Int(5, 5);
            var result = _resolver.ComputeVisibleTiles(origin, 0, MapW, MapH);
            Assert.AreEqual(9, result.Count);
            Assert.Contains(origin, new List<Vector2Int>(result));
        }

        // ─── 3. Range1_ReturnsAtLeastOriginAndNeighbours ─────────────────────

        [Test]
        public void ComputeVisibleTiles_Range1_ReturnsAtLeastOriginAndNeighbours()
        {
            var origin = new Vector2Int(5, 5);
            var result = _resolver.ComputeVisibleTiles(origin, 1, MapW, MapH);

            Assert.AreEqual(9, result.Count);
            Assert.Contains(origin, new List<Vector2Int>(result));
        }

        // ─── 4. NoDuplicates ─────────────────────────────────────────────────

        [Test]
        public void ComputeVisibleTiles_NoDuplicates()
        {
            var origin = new Vector2Int(5, 5);
            var result = _resolver.ComputeVisibleTiles(origin, 4, MapW, MapH);
            var set = new HashSet<Vector2Int>(result);
            Assert.AreEqual(set.Count, result.Count, "Result contains duplicates.");
        }

        // ─── 5. AllTilesWithinMapBounds ──────────────────────────────────────

        [Test]
        public void ComputeVisibleTiles_AllTilesWithinMapBounds()
        {
            var origin = new Vector2Int(1, 1);
            var result = _resolver.ComputeVisibleTiles(origin, 5, MapW, MapH);
            foreach (var t in result)
            {
                Assert.GreaterOrEqual(t.x, 0, $"Tile {t} out of bounds (x < 0)");
                Assert.Less(t.x, MapW,        $"Tile {t} out of bounds (x >= {MapW})");
                Assert.GreaterOrEqual(t.y, 0, $"Tile {t} out of bounds (y < 0)");
                Assert.Less(t.y, MapH,        $"Tile {t} out of bounds (y >= {MapH})");
            }
        }

        // ─── 6. LargerRange_MoreTiles ────────────────────────────────────────

        [Test]
        public void ComputeVisibleTiles_LargerRange_MoreTiles()
        {
            var origin = new Vector2Int(8, 8);
            var small  = _resolver.ComputeVisibleTiles(origin, 1, MapW, MapH);
            var large  = _resolver.ComputeVisibleTiles(origin, 5, MapW, MapH);
            Assert.Greater(large.Count, small.Count);
        }

        // ─── 7. NullGridService_FallbackToCircle_DoesNotThrow ────────────────

        [Test]
        public void NullGridService_FallbackToCircle_DoesNotThrow()
        {
            var fallbackResolver = new FogVisibilityResolver(null, _heightVisionService, _settings);
            Assert.DoesNotThrow(() =>
            {
                var result = fallbackResolver.ComputeVisibleTiles(
                    new Vector2Int(5, 5), 3, MapW, MapH);
                Assert.IsNotNull(result);
                Assert.GreaterOrEqual(result.Count, 1);
            });
        }

        [Test]
        public void ComputeVisibleTiles_HighGround_ExtendsVisionBeyondBaseRange()
        {
            var heightMap = new float[MapW, MapH];
            heightMap[5, 5] = 0.75f;
            _resolver.SetHeightMap(heightMap);

            var result = _resolver.ComputeVisibleTiles(new Vector2Int(5, 5), 1, MapW, MapH);

            Assert.Contains(new Vector2Int(7, 5), new List<Vector2Int>(result));
        }

        [Test]
        public void ComputeVisibleTiles_LowGround_CannotSeeTallTile()
        {
            var heightMap = new float[MapW, MapH];
            heightMap[5, 5] = 0.05f;
            heightMap[6, 5] = 1f;
            _resolver.SetHeightMap(heightMap);

            var result = _resolver.ComputeVisibleTiles(new Vector2Int(5, 5), 1, MapW, MapH);

            Assert.IsFalse(new List<Vector2Int>(result).Contains(new Vector2Int(6, 5)));
        }
    }
}
