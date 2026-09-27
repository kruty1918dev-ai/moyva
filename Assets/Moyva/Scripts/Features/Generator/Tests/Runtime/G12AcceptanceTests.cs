using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G12 — vertical fill (усупи). A tile sitting above its neighbours must
    /// grow side walls only on open sides, reach each side's resolved bottom,
    /// keep outward-facing normals, leave no internal faces between touching
    /// tiles, and meet cleanly at corners when adjacent drops differ. The
    /// builder cache must never return a mesh resolved for a different height
    /// or occlusion pattern.
    /// </summary>
    public sealed class G12AcceptanceTests
    {
        private const float Half = 0.5f;
        private const float Eps = 0.001f;
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                if (o != null)
                    Object.DestroyImmediate(o);
            }
            _created.Clear();
        }

        // ---------- flat top + skirt path ----------

        [Test]
        public void FlatTile_AllSidesOpen_FourOutwardSkirtsReachBottom()
        {
            Mesh top = FlatTop();
            Mesh result = Fill(top,
                Matrix4x4.Translate(new Vector3(0f, 2f, 0f)),
                visibleBottomY: 0f,
                occluded: TileMeshOccludedSides.None);

            Assert.IsNotNull(result);

            // 2 authored top tris (6 indices) + 4 side quads (24 indices).
            Assert.AreEqual(6 + 24, result.triangles.Length);

            Vector3[] v = result.vertices;
            int[] tris = result.triangles;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                Vector3 a = v[tris[i]];
                Vector3 b = v[tris[i + 1]];
                Vector3 c = v[tris[i + 2]];
                Vector3 cross = Vector3.Cross(b - a, c - a);
                if (cross.sqrMagnitude <= 1e-10f)
                    continue;

                Vector3 normal = cross.normalized;
                if (Mathf.Abs(normal.y) > 0.5f)
                    continue; // authored top surface

                Vector3 centre = (a + b + c) / 3f;
                Assert.AreEqual(1f,
                    new Vector2(normal.x, normal.z).magnitude, 0.01f,
                    $"skirt face {i} must be a vertical side wall");
                Assert.Greater(
                    Vector2.Dot(
                        new Vector2(normal.x, normal.z).normalized,
                        new Vector2(centre.x, centre.z).normalized),
                    0.9f,
                    $"skirt face {i} must point outward, away from the tile");
            }

            foreach (Vector3 p in v)
            {
                if (p.y < -Eps)
                    Assert.AreEqual(-2f, p.y, Eps,
                        "skirt bottom must reach the resolved bottom");
            }
        }

        [Test]
        public void FlatTile_OccludedSouth_EmitsNoWallOnThatSide()
        {
            Mesh top = FlatTop();
            Mesh result = Fill(top,
                Matrix4x4.Translate(new Vector3(0f, 2f, 0f)),
                visibleBottomY: 0f,
                occluded: TileMeshOccludedSides.South);

            Assert.IsNotNull(result);
            // Top quad (6) + N + E + W skirts (18) = 24 indices.
            Assert.AreEqual(24, result.triangles.Length,
                "occluded south side must not grow a hidden wall");

            Vector3[] v = result.vertices;
            int[] tris = result.triangles;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                float maxZ = Mathf.Max(v[tris[i]].z,
                    Mathf.Max(v[tris[i + 1]].z, v[tris[i + 2]].z));
                Assert.Greater(maxZ, -Half + Eps,
                    $"tri {i} lies fully on the occluded south plane");
            }
        }

        [Test]
        public void FlatTile_AllOccluded_NoFillNeeded()
        {
            Mesh top = FlatTop();
            var source = MakeSource(top,
                Matrix4x4.Translate(new Vector3(0f, 2f, 0f)),
                visibleBottomY: 0f,
                occluded: TileMeshOccludedSides.North
                    | TileMeshOccludedSides.East
                    | TileMeshOccludedSides.South
                    | TileMeshOccludedSides.West);

            Assert.IsFalse(TileVerticalFillMeshUtility.TryCreate(source, out _),
                "a fully occluded tile must not gain invisible side faces");
        }

        [Test]
        public void FlatTile_PerSideBottoms_ReachIndependentDepths()
        {
            Mesh top = FlatTop();
            // Opposite sides keep the corners out of the measurement.
            Mesh result = Fill(top,
                Matrix4x4.identity,
                visibleBottomY: -1.6f,
                occluded: TileMeshOccludedSides.East | TileMeshOccludedSides.West,
                edgeBottoms: new TileMeshEdgeBottoms(
                    north: -0.8f, east: float.NaN,
                    south: -1.6f, west: float.NaN));

            Assert.IsNotNull(result);

            float northBottom = MinYOnPlane(result, p => p.z > Half - Eps);
            float southBottom = MinYOnPlane(result, p => p.z < -Half + Eps);
            Assert.AreEqual(-0.8f, northBottom, Eps,
                "north skirt must stop at the north edge bottom");
            Assert.AreEqual(-1.6f, southBottom, Eps,
                "south skirt must drop two levels to its own bottom");
        }

        [Test]
        public void FlatTile_CornerWithUnequalDrops_HasNoSlit()
        {
            Mesh top = FlatTop();
            // South drops two levels, west one. At the shared corner the west
            // wall's corner edge must continue down to the deeper bottom,
            // otherwise a vertical slit opens on the diagonal seam.
            Mesh result = Fill(top,
                Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: TileMeshOccludedSides.North | TileMeshOccludedSides.East,
                edgeBottoms: new TileMeshEdgeBottoms(
                    north: float.NaN, east: float.NaN,
                    south: -2f, west: -1f));

            Assert.IsNotNull(result);
            Assert.IsTrue(
                HasTriangleOnPlane(result,
                    p => Mathf.Abs(p.x + Half) < Eps,
                    requireMinY: -1.5f),
                "west side wall must cover the corner column down to the deeper drop");

            Assert.IsTrue(
                HasTriangleOnPlane(result,
                    p => Mathf.Abs(p.z + Half) < Eps,
                    requireMinY: -1.5f),
                "south side wall must reach its own deep bottom");
        }

        // ---------- preserved authored + closure path ----------

        [Test]
        public void SolidTile_AllOccluded_AddsNoInvisibleFaces()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            var source = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: AllSides(),
                closurePolicy: AuthoredClosurePolicy.PreserveAuthored,
                generateMissingClosure: true);

            Assert.IsTrue(TileVerticalFillMeshUtility.TryCreate(source, out Mesh result));
            Track(result);
            Assert.AreEqual(tile.triangles.Length, result.triangles.Length,
                "fully occluded tile must not gain hidden interior faces");
        }

        [Test]
        public void SolidTile_OpenSouth_GrowsWallWithOutwardNormal()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            Mesh result = Fill(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: AllSides() & ~TileMeshOccludedSides.South,
                closurePolicy: AuthoredClosurePolicy.PreserveAuthored,
                generateMissingClosure: true,
                edgeBottoms: new TileMeshEdgeBottoms(
                    float.NaN, float.NaN, -1f, float.NaN));

            Assert.IsNotNull(result);
            Assert.AreEqual(tile.triangles.Length + 6, result.triangles.Length,
                "one open side contributes exactly one contour quad");

            Vector3[] verts = result.vertices;
            int[] tris = result.triangles;
            Vector3[] normals = result.normals;
            bool found = false;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                // generated skirt verts live at or below the authored ring (-d)
                if (verts[tris[i]].y > -0.02f
                    || verts[tris[i + 1]].y > -0.02f
                    || verts[tris[i + 2]].y > -0.02f)
                    continue;

                found = true;
                Vector3 n = Vector3.Cross(
                    verts[tris[i + 1]] - verts[tris[i]],
                    verts[tris[i + 2]] - verts[tris[i]]).normalized;
                Assert.Less(n.z, -0.9f,
                    "south skirt face must face south, not inward");

                float minY = Mathf.Min(verts[tris[i]].y,
                    Mathf.Min(verts[tris[i + 1]].y, verts[tris[i + 2]].y));
                float maxY = Mathf.Max(verts[tris[i]].y,
                    Mathf.Max(verts[tris[i + 1]].y, verts[tris[i + 2]].y));
                Assert.AreEqual(-1f, minY, 0.02f,
                    "skirt must reach the resolved south bottom");
                Assert.AreEqual(-0.0375f, maxY, 0.02f,
                    "skirt must start at the authored bottom ring");
            }
            Assert.IsTrue(found, "expected a generated south skirt quad");
        }

        [Test]
        public void SolidTile_CornerWithUnequalDrops_HasNoSlit()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            Mesh result = Fill(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: TileMeshOccludedSides.North | TileMeshOccludedSides.East,
                closurePolicy: AuthoredClosurePolicy.PreserveAuthored,
                generateMissingClosure: true,
                edgeBottoms: new TileMeshEdgeBottoms(
                    float.NaN, float.NaN, south: -2f, west: -1f));

            Assert.IsNotNull(result);
            Assert.IsTrue(
                HasTriangleOnPlane(result,
                    p => Mathf.Abs(p.x + Half) < Eps,
                    requireMinY: -1.5f),
                "west contour wall must cover the corner to the deeper bottom");
        }

        [Test]
        public void SolidTile_NoClosureConsent_Passthrough()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            var source = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: TileMeshOccludedSides.None,
                closurePolicy: AuthoredClosurePolicy.PreserveAuthored,
                generateMissingClosure: false);

            Assert.IsFalse(TileVerticalFillMeshUtility.TryCreate(source, out _),
                "PreserveAuthored without closure consent must not modify the mesh");
        }

        // ---------- GeneratedClosure (deform) path ----------

        [Test]
        public void GeneratedClosure_AllOccluded_DoesNotStretchHiddenSides()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            var source = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: AllSides(),
                closurePolicy: AuthoredClosurePolicy.GeneratedClosure);

            Assert.IsFalse(TileVerticalFillMeshUtility.TryCreate(source, out _),
                "a fully occluded GeneratedClosure tile must keep its authored "
                + "bottom ring instead of stretching invisible walls downward");
        }

        [Test]
        public void GeneratedClosure_OpenSouth_StretchesOnlyOpenCornerVerts()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            const float authoredBottom = -0.0375f;
            Mesh result = Fill(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: AllSides() & ~TileMeshOccludedSides.South,
                closurePolicy: AuthoredClosurePolicy.GeneratedClosure);

            Assert.IsNotNull(result);
            Vector3[] v = result.vertices;
            foreach (Vector3 p in v)
            {
                bool pulledNorth =
                    p.z > Half - 0.1f && p.y < authoredBottom - Eps;
                Assert.IsFalse(pulledNorth,
                    "occluded north bottom-ring verts must not be pulled down");

                if (p.y < authoredBottom - Eps)
                    Assert.AreEqual(-1f, p.y, Eps,
                        "pulled verts must land exactly on the target bottom");
            }
            Assert.IsTrue(AnyVert(v, p => p.y < -0.9f),
                "the open south side must still stretch to the bottom");
        }

        [Test]
        public void GeneratedClosure_Output_HasNoDegenerateTriangles()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            Mesh result = Fill(tile, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: TileMeshOccludedSides.None,
                closurePolicy: AuthoredClosurePolicy.GeneratedClosure);

            Assert.IsNotNull(result);
            Vector3[] v = result.vertices;
            int[] tris = result.triangles;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                Vector3 cross = Vector3.Cross(
                    v[tris[i + 1]] - v[tris[i]],
                    v[tris[i + 2]] - v[tris[i]]);
                Assert.Greater(cross.sqrMagnitude, 1e-10f,
                    $"degenerate triangle at index {i}");
            }
        }

        // ---------- cache key ----------

        [Test]
        public void CacheKey_DiscriminatesDepthOcclusionAndBottoms()
        {
            Mesh tile = SolidBeveledTileMeshUtility.GetOrCreate();
            var a = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f, occluded: TileMeshOccludedSides.None);

            // Identical inputs share the key.
            var same = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f, occluded: TileMeshOccludedSides.None);
            Assert.AreEqual(
                TileVerticalFillMeshKey.Create(a),
                TileVerticalFillMeshKey.Create(same),
                "identical sources must hit the same cache entry");

            // A different drop depth must not reuse this mesh.
            var deeper = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -2f, occluded: TileMeshOccludedSides.None);
            Assert.AreNotEqual(
                TileVerticalFillMeshKey.Create(a),
                TileVerticalFillMeshKey.Create(deeper),
                "cache must not return another height's fill mesh");

            // Occlusion changes the emitted sides.
            var occluded = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f, occluded: TileMeshOccludedSides.South);
            Assert.AreNotEqual(
                TileVerticalFillMeshKey.Create(a),
                TileVerticalFillMeshKey.Create(occluded));

            // Per-side bottoms differ -> different skirts.
            var bottom = MakeSource(tile, Matrix4x4.identity,
                visibleBottomY: -1f, occluded: TileMeshOccludedSides.None,
                edgeBottoms: new TileMeshEdgeBottoms(-3f, -1f, -1f, -1f));
            Assert.AreNotEqual(
                TileVerticalFillMeshKey.Create(a),
                TileVerticalFillMeshKey.Create(bottom));

            // World-Y translation alone must not defeat the cache: the fill
            // is produced in local space.
            var lifted = MakeSource(tile,
                Matrix4x4.Translate(new Vector3(0f, 5f, 0f)),
                visibleBottomY: 4f, occluded: TileMeshOccludedSides.None);
            Assert.AreEqual(
                TileVerticalFillMeshKey.Create(a),
                TileVerticalFillMeshKey.Create(lifted),
                "same local geometry at another world height shares the mesh");
        }

        [Test]
        public void BoundaryEdgeOcclusion_ResolvesFootprintSides()
        {
            Mesh top = FlatTop();
            var source = MakeSource(top, Matrix4x4.identity,
                visibleBottomY: -1f,
                occluded: TileMeshOccludedSides.South);

            Assert.IsTrue(TileVerticalFillMeshUtility.IsBoundaryEdgeOccluded(
                source,
                new Vector3(-Half, 0f, -Half),
                new Vector3(Half, 0f, -Half)),
                "south boundary edge must report occluded");
            Assert.IsFalse(TileVerticalFillMeshUtility.IsBoundaryEdgeOccluded(
                source,
                new Vector3(-Half, 0f, Half),
                new Vector3(Half, 0f, Half)),
                "north boundary edge is open");
            Assert.IsFalse(TileVerticalFillMeshUtility.IsBoundaryEdgeOccluded(
                source,
                new Vector3(-0.1f, 0f, 0f),
                new Vector3(0.1f, 0f, 0f)),
                "an interior edge is not a footprint boundary");
        }

        // ---------- helpers ----------

        private Mesh FlatTop()
        {
            var mesh = new Mesh
            {
                vertices = new[]
                {
                    new Vector3(-Half, 0f, -Half), new Vector3(-Half, 0f, Half),
                    new Vector3(Half, 0f, Half), new Vector3(Half, 0f, -Half)
                },
                triangles = new[] { 0, 1, 2, 0, 2, 3 },
                uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right }
            };
            mesh.RecalculateBounds();
            _created.Add(mesh);
            return mesh;
        }

        private TileMeshSource MakeSource(
            Mesh mesh,
            Matrix4x4 localMatrix,
            float visibleBottomY,
            TileMeshOccludedSides occluded,
            AuthoredClosurePolicy closurePolicy = AuthoredClosurePolicy.GeneratedClosure,
            bool generateMissingClosure = false,
            TileMeshEdgeBottoms edgeBottoms = default)
        {
            return new TileMeshSource(
                mesh,
                new Material[0],
                localMatrix,
                visibleBottomY: visibleBottomY,
                occludedSides: occluded,
                tileCenterXZ: Vector2.zero,
                tileHalfExtent: Half,
                authoredClosurePolicy: closurePolicy,
                edgeBottoms: edgeBottoms,
                generateMissingClosure: generateMissingClosure);
        }

        private Mesh Fill(
            Mesh mesh,
            Matrix4x4 localMatrix,
            float visibleBottomY,
            TileMeshOccludedSides occluded,
            AuthoredClosurePolicy closurePolicy = AuthoredClosurePolicy.GeneratedClosure,
            bool generateMissingClosure = false,
            TileMeshEdgeBottoms edgeBottoms = default)
        {
            var source = MakeSource(mesh, localMatrix, visibleBottomY, occluded,
                closurePolicy, generateMissingClosure, edgeBottoms);
            if (!TileVerticalFillMeshUtility.TryCreate(source, out Mesh result))
                return null;
            Track(result);
            return result;
        }

        private void Track(Mesh mesh)
        {
            if (mesh != null)
                _created.Add(mesh);
        }

        private static TileMeshOccludedSides AllSides()
            => TileMeshOccludedSides.North | TileMeshOccludedSides.East
               | TileMeshOccludedSides.South | TileMeshOccludedSides.West;

        private bool HasTriangleOnPlane(
            Mesh mesh,
            System.Func<Vector3, bool> onPlane,
            float requireMinY)
        {
            Vector3[] v = mesh.vertices;
            int[] tris = mesh.triangles;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                if (!onPlane(v[tris[i]])
                    || !onPlane(v[tris[i + 1]])
                    || !onPlane(v[tris[i + 2]]))
                    continue;

                if (Mathf.Min(v[tris[i]].y,
                        Mathf.Min(v[tris[i + 1]].y, v[tris[i + 2]].y))
                    <= requireMinY)
                    return true;
            }
            return false;
        }

        private static float MinYWhere(
            Vector3[] verts, System.Func<Vector3, bool> filter)
        {
            float min = float.PositiveInfinity;
            foreach (Vector3 p in verts)
            {
                if (filter(p))
                    min = Mathf.Min(min, p.y);
            }
            return min;
        }

        /// <summary>Min y among triangles whose verts all sit on the plane.</summary>
        private static float MinYOnPlane(
            Mesh mesh, System.Func<Vector3, bool> onPlane)
        {
            float min = float.PositiveInfinity;
            Vector3[] v = mesh.vertices;
            int[] tris = mesh.triangles;
            for (int i = 0; i + 2 < tris.Length; i += 3)
            {
                if (!onPlane(v[tris[i]])
                    || !onPlane(v[tris[i + 1]])
                    || !onPlane(v[tris[i + 2]]))
                    continue;
                min = Mathf.Min(min, Mathf.Min(v[tris[i]].y,
                    Mathf.Min(v[tris[i + 1]].y, v[tris[i + 2]].y)));
            }
            return min;
        }

        private static bool AnyVert(
            Vector3[] verts, System.Func<Vector3, bool> filter)
        {
            foreach (Vector3 p in verts)
            {
                if (filter(p))
                    return true;
            }
            return false;
        }
    }
}
