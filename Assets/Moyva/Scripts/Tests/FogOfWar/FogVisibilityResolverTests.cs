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
            _settings.TerrainRaySamplesPerTile = 5;
            _settings.TerrainVisibilityThreshold = 0.5f;
            _settings.PartialVisibilityDetectionMultiplier = 1f;
            _settings.TerrainRayStepTiles = 0.5f;
            _settings.ObserverEyeHeightOffset = 0.35f;
            _settings.TargetSampleHeightOffset = 0.1f;
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

        [Test]
        public void ComputeVisibleTiles_PlateauCenter_HidesTilesImmediatelyBehindDownhillEdge()
        {
            var heightMap = new float[MapW, MapH];
            for (int x = 2; x <= 7; x++)
                heightMap[x, 10] = 1f;

            _resolver.SetHeightMap(heightMap);

            var result = new List<Vector2Int>(_resolver.ComputeVisibleTiles(new Vector2Int(4, 10), 8, MapW, MapH));

            Assert.IsFalse(result.Contains(new Vector2Int(8, 10)), "The first low tile behind a steep edge should be hidden from the plateau center.");
            Assert.IsFalse(result.Contains(new Vector2Int(9, 10)), "The blind zone should cover the immediate low-ground tiles behind the edge.");
            Assert.Contains(new Vector2Int(11, 10), result, "After the blind zone, lower terrain can become visible again.");
        }

        [Test]
        public void ComputeVisibleTiles_ObserverAtEdge_SeesLowGroundImmediatelyBelow()
        {
            var heightMap = new float[MapW, MapH];
            for (int x = 2; x <= 7; x++)
                heightMap[x, 10] = 1f;

            _resolver.SetHeightMap(heightMap);

            var result = new List<Vector2Int>(_resolver.ComputeVisibleTiles(new Vector2Int(7, 10), 8, MapW, MapH));

            Assert.Contains(new Vector2Int(8, 10), result, "Standing on the edge should collapse the blind zone directly below the edge.");
            Assert.Contains(new Vector2Int(9, 10), result);
        }

        [Test]
        public void ComputeVisibleTiles_LowGround_SeesHighTargetStandingOnUpperEdge()
        {
            var heightMap = new float[MapW, MapH];
            for (int x = 8; x <= 12; x++)
                heightMap[x, 10] = 1f;

            _resolver.SetHeightMap(heightMap);

            var result = new List<Vector2Int>(_resolver.ComputeVisibleTiles(new Vector2Int(6, 10), 4, MapW, MapH));

            Assert.Contains(new Vector2Int(8, 10), result, "A unit below should see a figure standing on the upper edge.");
            Assert.IsFalse(result.Contains(new Vector2Int(10, 10)), "A unit deeper on the plateau should remain hidden from below.");
        }

        [Test]
        public void GetVisibilityFactor_ThinPartialCover_ReturnsFractionalVisibility()
        {
            _settings.EnableTerrainEdgeLineOfSight = false;
            var heightMap = new float[MapW, MapH];
            heightMap[7, 5] = 2f;
            _resolver.SetHeightMap(heightMap);

            float visibility = _heightVisionService.GetVisibilityFactor(new Vector2Int(5, 5), new Vector2Int(9, 6), 5, 12);

            Assert.Greater(visibility, 0f, "Some corner rays should pass around a thin blocker.");
            Assert.Less(visibility, 1f, "A blocker crossing only part of the target tile should not count as fully visible.");
        }

        [Test]
        public void ComputeVisibleTiles_PartialVisibility_UsesConfiguredThreshold()
        {
            _settings.EnableTerrainEdgeLineOfSight = false;
            var heightMap = new float[MapW, MapH];
            heightMap[7, 5] = 2f;
            _resolver.SetHeightMap(heightMap);
            var target = new Vector2Int(9, 6);

            _settings.TerrainVisibilityThreshold = 0.75f;
            var strict = new List<Vector2Int>(_resolver.ComputeVisibleTiles(new Vector2Int(5, 5), 5, MapW, MapH));

            _settings.TerrainVisibilityThreshold = 0.5f;
            var permissive = new List<Vector2Int>(_resolver.ComputeVisibleTiles(new Vector2Int(5, 5), 5, MapW, MapH));

            Assert.IsFalse(strict.Contains(target));
            Assert.Contains(target, permissive);
        }

        [Test]
        public void ComputeVisibility_ReturnsVisibilityCoefficientForTargetTiles()
        {
            _settings.EnableTerrainEdgeLineOfSight = false;
            var heightMap = new float[MapW, MapH];
            heightMap[7, 5] = 2f;
            _resolver.SetHeightMap(heightMap);
            var target = new Vector2Int(9, 6);

            var visibility = _resolver.ComputeVisibility(new Vector2Int(5, 5), 5, MapW, MapH);

            var targetVisibility = default(FogTileVisibility);
            bool found = false;
            foreach (var tileVisibility in visibility)
            {
                if (tileVisibility.Tile != target)
                    continue;

                targetVisibility = tileVisibility;
                found = true;
                break;
            }

            Assert.IsTrue(found, "Partially visible tiles should be returned by coefficient API.");
            Assert.Greater(targetVisibility.Visibility, 0f);
            Assert.Less(targetVisibility.Visibility, 1f);
        }
    }
}
