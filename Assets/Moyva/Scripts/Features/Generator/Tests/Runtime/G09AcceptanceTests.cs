using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G09 acceptance invariants for composition resolution: all 16 dual-grid
    /// cardinal masks resolve to the exact N/E/S/W flag pattern, rotations
    /// stay consistent, a same-identity neighbour on a different terrace does
    /// NOT merge, the visually highest terrain wins the cell, overlays and
    /// water surfaces resolve through their own lanes.
    /// </summary>
    public sealed class G09AcceptanceTests
    {
        private static TileLayerSample Terrain(string id, float height,
            TileGeometryMode mode = TileGeometryMode.SolidTerrain)
        {
            return new TileLayerSample(
                layerId: id, layerName: id, blueprintLayerGuid: null,
                buildLayerGuid: null, tileId: id + "-tile", presetId: null,
                layerKind: LayerKind.BaseTerrain, sortingOrder: 0, layerOrder: 0,
                terrainPriority: 0, height: height, surfaceHeight: height,
                sourceLayerId: null, tileGeometryMode: mode);
        }

        private static TileStackCell Cell(params TileLayerSample[] samples)
        {
            var cell = new TileStackCell();
            foreach (var s in samples)
                cell.Add(s);
            return cell;
        }

        private static TileNeighborhood Hood(TileStackCell center,
            TileStackCell n = null, TileStackCell e = null,
            TileStackCell s = null, TileStackCell w = null,
            TileStackCell ne = null, TileStackCell se = null,
            TileStackCell sw = null, TileStackCell nw = null)
            => new TileNeighborhood(center, n, e, s, w, ne, se, sw, nw);

        /// <summary>
        /// For every 4-bit cardinal mask (bit0=N, bit1=E, bit2=S, bit3=W) a
        /// matching neighbour pattern must produce exactly that mask back —
        /// the full 16-value dual-grid contract, including 0000 and 1111.
        /// </summary>
        [Test]
        public void AllSixteenCardinalMasks_ResolveVerbatim()
        {
            var resolver = new ResolvedTileCompositionResolver();
            for (int mask = 0; mask < 16; mask++)
            {
                bool n = (mask & 1) != 0, e = (mask & 2) != 0,
                     s = (mask & 4) != 0, w = (mask & 8) != 0;
                TileStackCell Same() => Cell(Terrain("grass", 2f));
                var hood = Hood(Cell(Terrain("grass", 2f)),
                    n ? Same() : Cell(Terrain("water", 0f)),
                    e ? Same() : Cell(Terrain("water", 0f)),
                    s ? Same() : Cell(Terrain("water", 0f)),
                    w ? Same() : Cell(Terrain("water", 0f)));

                var resolved = resolver.Resolve(new Vector2Int(4, 4), hood);
                int resolvedMask =
                    (resolved.NorthMatches ? 1 : 0)
                    | (resolved.EastMatches ? 2 : 0)
                    | (resolved.SouthMatches ? 4 : 0)
                    | (resolved.WestMatches ? 8 : 0);
                Assert.AreEqual(mask, resolvedMask,
                    $"mask {mask:0000} must round-trip through resolve");
            }
        }

        [Test]
        public void SameIdentity_DifferentTerrace_DoesNotMatch()
        {
            var resolver = new ResolvedTileCompositionResolver();
            var hood = Hood(Cell(Terrain("grass", 2f)),
                n: Cell(Terrain("grass", 2f)),
                s: Cell(Terrain("grass", 5f))); // same id, different terrace
            var resolved = resolver.Resolve(Vector2Int.zero, hood);
            Assert.IsTrue(resolved.NorthMatches);
            Assert.IsFalse(resolved.SouthMatches,
                "same identity on another terrace must not merge — needs a cliff edge");
        }

        [Test]
        public void HighestVisualTerrain_WinsMain_NotLayerRank()
        {
            var resolver = new ResolvedTileCompositionResolver();
            // A low BaseTerrain and a high Shore: the higher surface owns the
            // cell even though Shore ranks below BaseTerrain by kind.
            var center = Cell(
                Terrain("grass", 1f),
                new TileLayerSample("shore", "shore", null, null, "shore-tile", null,
                    LayerKind.Shore, 0, 0, 0, 4f, 4f, null));
            var resolved = resolver.Resolve(Vector2Int.zero, Hood(center));
            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("shore", resolved.MainTerrain.LayerId,
                "the visually highest terrain must win the cell");
        }

        [Test]
        public void EmptyStack_ResolvesNothing()
        {
            var resolver = new ResolvedTileCompositionResolver();
            var resolved = resolver.Resolve(Vector2Int.zero, Hood(Cell()));
            Assert.IsFalse(resolved.HasMainTerrain);
            Assert.IsFalse(resolved.HasOverlay);
            Assert.IsFalse(resolved.HasPassage);
            Assert.IsFalse(resolved.HasWaterSurface);
        }

        [Test]
        public void OverlayTerrain_ResolvesAsOverlay_NotMain()
        {
            var resolver = new ResolvedTileCompositionResolver();
            var overlay = new TileLayerSample("path", "path", null, null,
                "path-tile", null, LayerKind.OverlayTerrain, 0, 0, 0, 3f, 3f, null);
            var center = Cell(Terrain("grass", 2f), overlay);
            var resolved = resolver.Resolve(Vector2Int.zero, Hood(center));
            Assert.IsTrue(resolved.HasMainTerrain);
            Assert.AreEqual("grass", resolved.MainTerrain.LayerId);
            Assert.IsTrue(resolved.HasOverlay);
            Assert.AreEqual("path", resolved.Overlay.LayerId);
        }

        [Test]
        public void WaterSurface_ComesFromHighestSurfaceOnlyNeighbour()
        {
            var resolver = new ResolvedTileCompositionResolver();
            var water = new TileLayerSample("water", "water", null, null,
                "water-tile", null, LayerKind.BaseTerrain, 0, 0, 0, 1f, 1f,
                null, TileGeometryMode.SurfaceOnly);
            var hood = Hood(
                Cell(Terrain("grass", 2f)),
                n: Cell(water),
                e: Cell(water.WithHeights(0.5f, 0.5f)));
            var resolved = resolver.Resolve(Vector2Int.zero, hood);
            Assert.IsTrue(resolved.HasWaterSurface,
                "land next to surface-only water must get a water surface");
            Assert.AreEqual(1f, resolved.WaterSurface.SurfaceHeight,
                "the highest adjacent water surface wins");
        }

        [Test]
        public void MaskPattern_IsRotationConsistent()
        {
            var resolver = new ResolvedTileCompositionResolver();
            // Single-open-corner mask 0001 (only N matches) rotated twice must
            // read as 0100 (only S matches) — the same physical shape.
            var north = Hood(Cell(Terrain("g", 1f)),
                n: Cell(Terrain("g", 1f)), e: Cell(Terrain("x", 0f)),
                s: Cell(Terrain("x", 0f)), w: Cell(Terrain("x", 0f)));
            var south = Hood(Cell(Terrain("g", 1f)),
                n: Cell(Terrain("x", 0f)), e: Cell(Terrain("x", 0f)),
                s: Cell(Terrain("g", 1f)), w: Cell(Terrain("x", 0f)));
            var rn = resolver.Resolve(Vector2Int.zero, north);
            var rs = resolver.Resolve(Vector2Int.zero, south);
            Assert.IsTrue(rn.NorthMatches && !rn.SouthMatches && !rn.EastMatches && !rn.WestMatches);
            Assert.IsTrue(rs.SouthMatches && !rs.NorthMatches && !rs.EastMatches && !rs.WestMatches);
        }
    }
}
