using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G11 acceptance: every logical tile keeps a visible bevel without
    /// deforming the seams. Verifies bevel width, winding, normals/tangents
    /// and uniform UV density on the generated solid tile, plus the
    /// surface-only filter's upward-face selection and payload integrity.
    /// </summary>
    public sealed class G11AcceptanceTests
    {
        private const float Half = 0.5f;
        private const float TopInset = 0.0375f;
        private const float BevelDrop = 0.0375f;
        private const float Eps = 0.0001f;

        private readonly List<Mesh> _ownedMeshes = new List<Mesh>();

        [TearDown]
        public void TearDown()
        {
            foreach (Mesh mesh in _ownedMeshes)
            {
                if (mesh != null)
                    Object.DestroyImmediate(mesh);
            }
            _ownedMeshes.Clear();
        }

        private Mesh Track(Mesh mesh)
        {
            if (mesh != null)
                _ownedMeshes.Add(mesh);
            return mesh;
        }

        // ---- SolidBeveledTileMeshUtility -------------------------------------

        [Test]
        public void SolidBeveledTile_IsSharedReadableAndComplete()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();

            Assert.IsNotNull(mesh);
            Assert.IsTrue(mesh.isReadable);
            Assert.AreSame(mesh, SolidBeveledTileMeshUtility.GetOrCreate());
            // One plateau quad plus a four-face bevel ring: 5 quads.
            Assert.AreEqual(20, mesh.vertexCount);
            Assert.AreEqual(30, mesh.triangles.Length);
            Assert.AreEqual(1, mesh.subMeshCount);
        }

        [Test]
        public void SolidBeveledTile_TopIsInsetAndBevelRingClosesAtFullFootprint()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            float plateauEdge = Half - TopInset;

            bool sawTop = false;
            bool sawBottom = false;
            foreach (Vector3 vertex in mesh.vertices)
            {
                float radius = Mathf.Max(Mathf.Abs(vertex.x), Mathf.Abs(vertex.z));
                if (Mathf.Abs(vertex.y) <= Eps)
                {
                    sawTop = true;
                    Assert.AreEqual(plateauEdge, radius, 0.0001f,
                        "plateau vertex must sit on the inset top edge");
                }
                else
                {
                    sawBottom = true;
                    Assert.AreEqual(-BevelDrop, vertex.y, 0.0001f,
                        "bevel bottom vertex must sit at the bevel drop");
                    Assert.AreEqual(Half, radius, 0.0001f,
                        "bevel bottom vertex must sit on the full 1x1 footprint");
                }
            }
            Assert.IsTrue(sawTop && sawBottom);
        }

        [Test]
        public void SolidBeveledTile_NoDegenerateTriangles()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                Vector3 cross = Vector3.Cross(
                    vertices[triangles[i + 1]] - vertices[triangles[i]],
                    vertices[triangles[i + 2]] - vertices[triangles[i]]);
                Assert.Greater(cross.sqrMagnitude, 1e-8f,
                    $"degenerate triangle at index {i}");
            }
        }

        [Test]
        public void SolidBeveledTile_WindingMatchesAssignedNormals()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            Assert.AreEqual(vertices.Length, normals.Length);
            for (int i = 0; i < normals.Length; i++)
            {
                Assert.AreEqual(1f, normals[i].magnitude, 0.001f,
                    $"normal {i} must be unit length");
            }

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                int ia = triangles[i];
                int ib = triangles[i + 1];
                int ic = triangles[i + 2];
                Vector3 geometric = Vector3.Cross(
                    vertices[ib] - vertices[ia],
                    vertices[ic] - vertices[ia]).normalized;

                Assert.Greater(Vector3.Dot(geometric, normals[ia]), 0.99f,
                    $"inverted winding or normal at triangle {i}");
                Assert.Greater(Vector3.Dot(geometric, normals[ib]), 0.99f,
                    $"inverted winding or normal at triangle {i}");
                Assert.Greater(Vector3.Dot(geometric, normals[ic]), 0.99f,
                    $"inverted winding or normal at triangle {i}");
            }
        }

        [Test]
        public void SolidBeveledTile_BevelNormalsAreFortyFiveDegreesOutward()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float expectedBevelY = 1f / Mathf.Sqrt(2f);

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                Vector3 faceNormal = Vector3.Cross(
                    vertices[triangles[i + 1]] - vertices[triangles[i]],
                    vertices[triangles[i + 2]] - vertices[triangles[i]])
                    .normalized;

                if (Mathf.Abs(faceNormal.y) > 0.9f)
                {
                    Assert.AreEqual(1f, faceNormal.y, 0.001f,
                        "plateau triangle must face straight up");
                    continue;
                }

                Assert.AreEqual(expectedBevelY, faceNormal.y, 0.001f,
                    $"bevel face {i} must sit at 45 degrees");
                Vector3 horizontal = new Vector3(faceNormal.x, 0f, faceNormal.z);
                Assert.AreEqual(expectedBevelY, horizontal.magnitude, 0.001f,
                    $"bevel face {i} must retain its 45-degree outward tilt");

                Vector3 centre =
                    (vertices[triangles[i]]
                     + vertices[triangles[i + 1]]
                     + vertices[triangles[i + 2]]) / 3f;
                Vector2 outward = new Vector2(centre.x, centre.z).normalized;
                Vector2 normalSide =
                    new Vector2(faceNormal.x, faceNormal.z).normalized;
                // Mitered corners skew each triangle centroid along the edge,
                // so only the sign of the dot is meaningful here.
                Assert.Greater(Vector2.Dot(outward, normalSide), 0f,
                    $"bevel face {i} must point outward, not inward");
            }
        }

        [Test]
        public void SolidBeveledTile_TangentsAreUnitAndOrthogonalToNormals()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector4[] tangents = mesh.tangents;
            Vector3[] normals = mesh.normals;

            Assert.AreEqual(mesh.vertexCount, tangents.Length);
            for (int i = 0; i < tangents.Length; i++)
            {
                Vector3 tangent = tangents[i];
                Assert.AreEqual(1f, tangent.magnitude, 0.01f,
                    $"tangent {i} must be unit length");
                Assert.AreEqual(1f, Mathf.Abs(tangents[i].w), 0.001f,
                    $"tangent {i} must carry a +-1 handedness");
                Assert.LessOrEqual(
                    Mathf.Abs(Vector3.Dot(tangent, normals[i])), 0.01f,
                    $"tangent {i} must be orthogonal to the normal");
            }
        }

        [Test]
        public void SolidBeveledTile_UvDensityIsUniformAcrossBevels()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;

            Assert.AreEqual(vertices.Length, uvs.Length);
            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                float geometryArea =
                    Vector3.Cross(b - a, c - a).magnitude * 0.5f;

                Vector2 ua = uvs[triangles[i]];
                Vector2 ub = uvs[triangles[i + 1]];
                Vector2 uc = uvs[triangles[i + 2]];
                float uvArea = Mathf.Abs(
                    (ub.x - ua.x) * (uc.y - ua.y)
                    - (uc.x - ua.x) * (ub.y - ua.y)) * 0.5f;

                Assert.AreEqual(1f, uvArea / geometryArea, 0.01f,
                    $"texel density diverges on triangle {i} — a checker "
                    + "stretches across the bevel");
            }
        }

        [Test]
        public void SolidBeveledTile_SurfaceHasNoHoles_OnlyBottomRingIsOpen()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            var edgeUse = new Dictionary<EdgeKey, int>();
            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                AddEdge(edgeUse, vertices[triangles[i]], vertices[triangles[i + 1]]);
                AddEdge(edgeUse, vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                AddEdge(edgeUse, vertices[triangles[i + 2]], vertices[triangles[i]]);
            }

            int boundaryCount = 0;
            var cornerDegree = new Dictionary<QuantizedPosition, int>();
            foreach (KeyValuePair<EdgeKey, int> pair in edgeUse)
            {
                if (pair.Value != 1)
                    continue;

                boundaryCount++;
                AssertBottomRingPoint(pair.Key.A);
                AssertBottomRingPoint(pair.Key.B);
                CountCorner(cornerDegree, pair.Key.A);
                CountCorner(cornerDegree, pair.Key.B);
            }

            Assert.AreEqual(4, boundaryCount,
                "only the four authored bottom-ring edges may be open; "
                + "any other single-use edge is a hole in the surface");
            Assert.AreEqual(4, cornerDegree.Count,
                "the open boundary must be the square bottom ring");
            foreach (int degree in cornerDegree.Values)
                Assert.AreEqual(2, degree, "bottom ring must be a closed loop");
        }

        [Test]
        public void SolidBeveledTile_SharedTopEdgeVerticesShareUv()
        {
            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;

            var byPosition = new Dictionary<QuantizedPosition, Vector2>();
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Mathf.Abs(vertices[i].y) > Eps)
                    continue;

                var key = new QuantizedPosition(vertices[i]);
                if (!byPosition.TryGetValue(key, out Vector2 uv))
                {
                    byPosition[key] = uvs[i];
                    continue;
                }

                Assert.LessOrEqual(Vector2.Distance(uv, uvs[i]), 0.0001f,
                    $"coincident plateau-edge vertices diverge in UV at {vertices[i]} "
                    + "— the texture tears across the plateau/bevel seam");
            }
        }

        // ---- TileSurfaceOnlyMeshUtility --------------------------------------

        [Test]
        public void SurfaceOnly_KeepsUpwardFacesAndDropsDegenerate()
        {
            Mesh probe = Track(CreateProbeMesh());
            var source = new TileMeshSource(probe, null, Matrix4x4.identity);

            SurfaceOnlyMeshBuildStatus status =
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh result);
            Track(result);

            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Created, status);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.subMeshCount);
            // Upward plateau quad + upward 45-degree slope quad; the
            // degenerate sliver, the bottom quad and the vertical quad go.
            Assert.AreEqual(12, result.vertexCount);
            Assert.AreEqual(6, result.GetIndices(0, true).Length);
            Assert.AreEqual(6, result.GetIndices(1, true).Length);

            Vector3[] vertices = result.vertices;
            for (int subMesh = 0; subMesh < result.subMeshCount; subMesh++)
            {
                int[] indices = result.GetIndices(subMesh, true);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    for (int k = 0; k < 3; k++)
                        Assert.Less(indices[i + k], vertices.Length,
                            "remapped index must stay in range");

                    Vector3 normal = Vector3.Cross(
                        vertices[indices[i + 1]] - vertices[indices[i]],
                        vertices[indices[i + 2]] - vertices[indices[i]]);
                    Assert.Greater(normal.sqrMagnitude, 1e-8f,
                        "degenerate triangle survived the filter");
                    Assert.Greater(normal.normalized.y, 0.0001f,
                        "non-upward triangle survived the filter");
                }
            }
        }

        [Test]
        public void SurfaceOnly_PreservesAuthoredVertexPayloads()
        {
            Mesh probe = Track(CreateProbeMesh());
            var source = new TileMeshSource(probe, null, Matrix4x4.identity);

            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Created,
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh result));
            Track(result);

            var sourcePayloads = new HashSet<VertexPayload>();
            Vector3[] srcVerts = probe.vertices;
            Vector3[] srcNormals = probe.normals;
            Vector2[] srcUvs = probe.uv;
            Color32[] srcColors = probe.colors32;
            for (int i = 0; i < srcVerts.Length; i++)
                sourcePayloads.Add(new VertexPayload(
                    srcVerts[i], srcNormals[i], srcUvs[i], srcColors[i]));

            Vector3[] outVerts = result.vertices;
            Vector3[] outNormals = result.normals;
            Vector2[] outUvs = result.uv;
            Color32[] outColors = result.colors32;
            Assert.AreEqual(outVerts.Length, outNormals.Length);
            Assert.AreEqual(outVerts.Length, outUvs.Length);
            Assert.AreEqual(outVerts.Length, outColors.Length);
            for (int i = 0; i < outVerts.Length; i++)
            {
                Assert.IsTrue(sourcePayloads.Contains(new VertexPayload(
                        outVerts[i], outNormals[i], outUvs[i], outColors[i])),
                    $"compacted vertex {i} lost its authored payload");
            }
        }

        [Test]
        public void SurfaceOnly_DownwardOnlyMesh_ReturnsEmpty()
        {
            var mesh = Track(new Mesh { name = "G11DownOnly" });
            mesh.vertices = new[]
            {
                new Vector3(-0.4f, 0f, 0.4f),
                new Vector3(-0.4f, 0f, -0.4f),
                new Vector3(0.4f, 0f, -0.4f)
            };
            mesh.normals = new[] { Vector3.down, Vector3.down, Vector3.down };
            mesh.uv = new[] { Vector2.zero, Vector2.up, Vector2.one };
            mesh.SetTriangles(new[] { 0, 1, 2 }, 0, false);

            var source = new TileMeshSource(mesh, null, Matrix4x4.identity);
            SurfaceOnlyMeshBuildStatus status =
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh result);
            Track(result);

            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Empty, status);
            Assert.IsNull(result);
        }

        [Test]
        public void SurfaceOnly_InvalidInputs_ReturnFailed()
        {
            var missingMesh = new TileMeshSource(null, null, Matrix4x4.identity);
            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Failed,
                TileSurfaceOnlyMeshUtility.Create(missingMesh, out Mesh result));
            Track(result);

            Mesh unreadable = Track(CreateProbeMesh());
            unreadable.UploadMeshData(true);
            var unreadableSource =
                new TileMeshSource(unreadable, null, Matrix4x4.identity);
            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Failed,
                TileSurfaceOnlyMeshUtility.Create(unreadableSource, out result));
            Track(result);

            Mesh probe = Track(CreateProbeMesh());
            var collapsedTransform = new TileMeshSource(
                probe, null,
                Matrix4x4.Scale(new Vector3(0.0005f, 1f, 0.0005f)));
            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Failed,
                TileSurfaceOnlyMeshUtility.Create(collapsedTransform, out result));
            Track(result);
        }

        [Test]
        public void SurfaceOnly_UpwardIsEvaluatedInWorldSpace()
        {
            Mesh probe = Track(CreateProbeMesh());
            var source = new TileMeshSource(
                probe, null,
                Matrix4x4.TRS(
                    Vector3.zero,
                    Quaternion.Euler(180f, 0f, 0f),
                    Vector3.one));

            Assert.AreEqual(SurfaceOnlyMeshBuildStatus.Created,
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh result));
            Track(result);

            // The authored downward quad is the only face that ends up facing
            // up in world space once the tile is flipped upside-down.
            Assert.AreEqual(6, result.vertexCount);
            Assert.AreEqual(6, result.GetIndices(0, true).Length);
            Assert.AreEqual(0, result.GetIndices(1, true).Length);

            Vector3[] vertices = result.vertices;
            int[] indices = result.GetIndices(0, true);
            for (int i = 0; i + 2 < indices.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(
                    vertices[indices[i + 1]] - vertices[indices[i]],
                    vertices[indices[i + 2]] - vertices[indices[i]]);
                Assert.Greater(normal.sqrMagnitude, 1e-8f);
            }
        }

        // ---- helpers ---------------------------------------------------------

        private static Mesh CreateProbeMesh()
        {
            var mesh = new Mesh { name = "G11SurfaceProbe" };
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();
            var subMesh0 = new List<int>();
            var subMesh1 = new List<int>();

            void AddTriangle(
                int subMesh,
                Vector3 a,
                Vector3 b,
                Vector3 c,
                Vector3 normal)
            {
                List<int> target = subMesh == 0 ? subMesh0 : subMesh1;
                foreach (Vector3 p in new[] { a, b, c })
                {
                    int index = vertices.Count;
                    vertices.Add(p);
                    normals.Add(normal);
                    uvs.Add(new Vector2(index * 0.111f, index * 0.271f));
                    colors.Add(new Color32(
                        (byte)(37 + index),
                        (byte)(91 + index * 3),
                        (byte)(140 + index * 7),
                        255));
                    target.Add(index);
                }
            }

            // Upward plateau quad (submesh 0).
            AddTriangle(0,
                new Vector3(-0.4f, 1f, -0.4f),
                new Vector3(-0.4f, 1f, 0.4f),
                new Vector3(0.4f, 1f, 0.4f),
                Vector3.up);
            AddTriangle(0,
                new Vector3(-0.4f, 1f, -0.4f),
                new Vector3(0.4f, 1f, 0.4f),
                new Vector3(0.4f, 1f, -0.4f),
                Vector3.up);
            // Degenerate sliver (submesh 0) — all three vertices coincide.
            AddTriangle(0,
                new Vector3(0.3f, 0.7f, 0.3f),
                new Vector3(0.3f, 0.7f, 0.3f),
                new Vector3(0.3f, 0.7f, 0.3f),
                Vector3.up);
            // Downward bottom quad (submesh 0).
            AddTriangle(0,
                new Vector3(-0.4f, 0f, 0.4f),
                new Vector3(-0.4f, 0f, -0.4f),
                new Vector3(0.4f, 0f, -0.4f),
                Vector3.down);
            AddTriangle(0,
                new Vector3(-0.4f, 0f, 0.4f),
                new Vector3(0.4f, 0f, -0.4f),
                new Vector3(0.4f, 0f, 0.4f),
                Vector3.down);
            // 45-degree upward slope (submesh 1) — a bevel-like face.
            AddTriangle(1,
                new Vector3(-0.3f, 0.9f, 0.3f),
                new Vector3(-0.3f, 0.4f, 0.8f),
                new Vector3(0.3f, 0.4f, 0.8f),
                new Vector3(0f, 0.7071f, 0.7071f));
            AddTriangle(1,
                new Vector3(-0.3f, 0.9f, 0.3f),
                new Vector3(0.3f, 0.4f, 0.8f),
                new Vector3(0.3f, 0.9f, 0.3f),
                new Vector3(0f, 0.7071f, 0.7071f));
            // Vertical side quad (submesh 1).
            AddTriangle(1,
                new Vector3(0.5f, 0f, -0.4f),
                new Vector3(0.5f, 1f, -0.4f),
                new Vector3(0.5f, 1f, 0.4f),
                Vector3.right);
            AddTriangle(1,
                new Vector3(0.5f, 0f, -0.4f),
                new Vector3(0.5f, 1f, 0.4f),
                new Vector3(0.5f, 0f, 0.4f),
                Vector3.right);

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.colors32 = colors.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(subMesh0, 0, false);
            mesh.SetTriangles(subMesh1, 1, false);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddEdge(
            Dictionary<EdgeKey, int> edgeUse,
            Vector3 a,
            Vector3 b)
        {
            var key = new EdgeKey(a, b);
            edgeUse[key] = edgeUse.TryGetValue(key, out int count)
                ? count + 1
                : 1;
        }

        private static void AssertBottomRingPoint(QuantizedPosition position)
        {
            Assert.AreEqual(-375, position.Y, 1,
                "open edge endpoint must sit on the authored bottom ring");
            int radius = Math.Max(Math.Abs(position.X), Math.Abs(position.Z));
            Assert.AreEqual(5000, radius, 1,
                "open edge endpoint must sit on the full 1x1 footprint");
        }

        private static void CountCorner(
            Dictionary<QuantizedPosition, int> cornerDegree,
            QuantizedPosition position)
        {
            cornerDegree[position] =
                cornerDegree.TryGetValue(position, out int degree)
                    ? degree + 1
                    : 1;
        }

        private readonly struct QuantizedPosition : IEquatable<QuantizedPosition>
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;

            public QuantizedPosition(Vector3 position)
            {
                X = Mathf.RoundToInt(position.x * 10000f);
                Y = Mathf.RoundToInt(position.y * 10000f);
                Z = Mathf.RoundToInt(position.z * 10000f);
            }

            public bool Equals(QuantizedPosition other)
                => X == other.X && Y == other.Y && Z == other.Z;

            public override bool Equals(object obj)
                => obj is QuantizedPosition other && Equals(other);

            public override int GetHashCode()
                => X * 73856093 ^ Y * 19349663 ^ Z * 83492791;
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            public readonly QuantizedPosition A;
            public readonly QuantizedPosition B;

            public EdgeKey(Vector3 a, Vector3 b)
            {
                A = new QuantizedPosition(a);
                B = new QuantizedPosition(b);
            }

            public bool Equals(EdgeKey other)
                => (A.Equals(other.A) && B.Equals(other.B))
                   || (A.Equals(other.B) && B.Equals(other.A));

            public override bool Equals(object obj)
                => obj is EdgeKey other && Equals(other);

            public override int GetHashCode()
                => A.GetHashCode() ^ B.GetHashCode();
        }

        private readonly struct VertexPayload : IEquatable<VertexPayload>
        {
            private readonly QuantizedPosition _position;
            private readonly Vector3 _normal;
            private readonly Vector2 _uv;
            private readonly Color32 _color;

            public VertexPayload(
                Vector3 position,
                Vector3 normal,
                Vector2 uv,
                Color32 color)
            {
                _position = new QuantizedPosition(position);
                _normal = normal;
                _uv = uv;
                _color = color;
            }

            public bool Equals(VertexPayload other)
                => _position.Equals(other._position)
                   && _normal == other._normal
                   && _uv == other._uv
                   && _color.Equals(other._color);

            public override bool Equals(object obj)
                => obj is VertexPayload other && Equals(other);

            public override int GetHashCode()
                => _position.GetHashCode();
        }
    }
}
