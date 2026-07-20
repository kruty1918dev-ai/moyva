using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ScreenToGridConverterSurfaceTests
    {
        private sealed class TestGridService : IGridService
        {
            public int GridWidth => 1;
            public int GridHeight => 2;

            public string GetTileData(Vector2Int position)
                => TryGetTileData(position, out string tileId) ? tileId : null;

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                bool exists = position.x == 0 && position.y >= 0 && position.y < GridHeight;
                tileTypeId = exists ? "terrain" : null;
                return exists;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class TestTerrainQuery : IGeneratedTerrainLevelQuery
        {
            private readonly IReadOnlyDictionary<Vector2Int, float> _surfaceByTile;

            public TestTerrainQuery(IReadOnlyDictionary<Vector2Int, float> surfaceByTile)
            {
                _surfaceByTile = surfaceByTile;
            }

            public bool HasExplicitTerrainSurfaceMap => true;

            public bool TryGetTerrainLevel(Vector2Int position, out int level)
            {
                if (_surfaceByTile.TryGetValue(position, out float surface))
                {
                    level = Mathf.RoundToInt(surface);
                    return true;
                }

                level = 0;
                return false;
            }

            public bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY)
                => _surfaceByTile.TryGetValue(position, out surfaceY);
        }

        private sealed class SplitDepthGeometry : IConstructionGridGeometryService
        {
            private readonly bool _elevatedBandMapsToElevatedTile;

            public SplitDepthGeometry(bool elevatedBandMapsToElevatedTile)
            {
                _elevatedBandMapsToElevatedTile = elevatedBandMapsToElevatedTile;
            }

            public bool TryGetCellCenter(Vector2Int tile, out Vector3 center)
            {
                center = new Vector3(0f, 0f, tile.y == 0 ? 8f : 10f);
                return tile.x == 0 && tile.y >= 0 && tile.y <= 1;
            }

            public bool TryGetCellSize(out Vector2 size)
            {
                size = Vector2.one;
                return true;
            }

            public bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile)
            {
                if (worldPosition.z >= 7f && worldPosition.z < 9f)
                {
                    tile = _elevatedBandMapsToElevatedTile
                        ? Vector2Int.zero
                        : new Vector2Int(0, 1);
                    return true;
                }

                if (worldPosition.z >= 9f && worldPosition.z <= 11f)
                {
                    tile = new Vector2Int(0, 1);
                    return true;
                }

                tile = default;
                return false;
            }

            public bool TryGetGridPlaneY(out float y)
            {
                y = 0f;
                return true;
            }
        }

        [Test]
        public void SurfaceAwarePicking_SelectsNearestElevatedGridTile()
        {
            var surfaces = new Dictionary<Vector2Int, float>
            {
                [Vector2Int.zero] = 2f,
                [new Vector2Int(0, 1)] = 0f,
            };
            var converter = new ScreenToGridConverter(
                camera: null,
                gridProjection: null,
                gridGeometry: new SplitDepthGeometry(elevatedBandMapsToElevatedTile: true),
                gridService: new TestGridService(),
                terrainLevelQuery: new TestTerrainQuery(surfaces));
            var ray = new Ray(
                new Vector3(0f, 10f, 0f),
                new Vector3(0f, -1f, 1f).normalized);

            bool resolved = converter.TryResolveTerrainSurfaceTile(ray, out Vector2Int tile);

            Assert.IsTrue(resolved);
            Assert.AreEqual(Vector2Int.zero, tile);
        }

        [Test]
        public void SurfaceAwarePicking_RejectsCandidatePlaneWhenCellHasDifferentSurface()
        {
            var surfaces = new Dictionary<Vector2Int, float>
            {
                [Vector2Int.zero] = 2f,
                [new Vector2Int(0, 1)] = 0f,
            };
            var converter = new ScreenToGridConverter(
                camera: null,
                gridProjection: null,
                gridGeometry: new SplitDepthGeometry(elevatedBandMapsToElevatedTile: false),
                gridService: new TestGridService(),
                terrainLevelQuery: new TestTerrainQuery(surfaces));
            var ray = new Ray(
                new Vector3(0f, 10f, 0f),
                new Vector3(0f, -1f, 1f).normalized);

            bool resolved = converter.TryResolveTerrainSurfaceTile(ray, out Vector2Int tile);

            Assert.IsTrue(resolved);
            Assert.AreEqual(new Vector2Int(0, 1), tile);
        }
    }
}
