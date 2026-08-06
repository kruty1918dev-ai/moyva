using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileVerticalFillMeshKey : IEquatable<TileVerticalFillMeshKey>
    {
        private const float Quantization = 10000f;

        private readonly int _meshId;
        private readonly int _m00;
        private readonly int _m01;
        private readonly int _m02;
        private readonly int _m10;
        private readonly int _m11;
        private readonly int _m12;
        private readonly int _m20;
        private readonly int _m21;
        private readonly int _m22;
        private readonly int _relativeBottom;
        private readonly int _occludedSides;
        private readonly int _tileHalfExtent;
        private readonly int _authoredClosurePolicy;
        private readonly int _generateMissingClosure;
        private readonly int _northBottom;
        private readonly int _eastBottom;
        private readonly int _southBottom;
        private readonly int _westBottom;

        private TileVerticalFillMeshKey(TileMeshSource source)
        {
            Matrix4x4 matrix = source.LocalMatrix;
            _meshId = source.Mesh != null ? source.Mesh.GetInstanceID() : 0;
            _m00 = Quantize(matrix.m00);
            _m01 = Quantize(matrix.m01);
            _m02 = Quantize(matrix.m02);
            _m10 = Quantize(matrix.m10);
            _m11 = Quantize(matrix.m11);
            _m12 = Quantize(matrix.m12);
            _m20 = Quantize(matrix.m20);
            _m21 = Quantize(matrix.m21);
            _m22 = Quantize(matrix.m22);
            _relativeBottom = Quantize(source.VisibleBottomY - matrix.m13);
            _occludedSides = (int)source.OccludedSides;
            _tileHalfExtent = Quantize(source.TileHalfExtent);
            _authoredClosurePolicy = (int)source.AuthoredClosurePolicy;
            _generateMissingClosure = source.GenerateMissingClosure ? 1 : 0;
            _northBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.North,
                source.VisibleBottomY) - matrix.m13);
            _eastBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.East,
                source.VisibleBottomY) - matrix.m13);
            _southBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.South,
                source.VisibleBottomY) - matrix.m13);
            _westBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.West,
                source.VisibleBottomY) - matrix.m13);
        }

        public static TileVerticalFillMeshKey Create(TileMeshSource source)
            => new TileVerticalFillMeshKey(source);

        public bool Equals(TileVerticalFillMeshKey other)
        {
            return _meshId == other._meshId
                && _m00 == other._m00
                && _m01 == other._m01
                && _m02 == other._m02
                && _m10 == other._m10
                && _m11 == other._m11
                && _m12 == other._m12
                && _m20 == other._m20
                && _m21 == other._m21
                && _m22 == other._m22
                && _relativeBottom == other._relativeBottom
                && _occludedSides == other._occludedSides
                && _tileHalfExtent == other._tileHalfExtent
                && _authoredClosurePolicy == other._authoredClosurePolicy
                && _generateMissingClosure == other._generateMissingClosure
                && _northBottom == other._northBottom
                && _eastBottom == other._eastBottom
                && _southBottom == other._southBottom
                && _westBottom == other._westBottom;
        }

        public override bool Equals(object obj)
            => obj is TileVerticalFillMeshKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _meshId;
                hash = hash * 31 + _m00;
                hash = hash * 31 + _m01;
                hash = hash * 31 + _m02;
                hash = hash * 31 + _m10;
                hash = hash * 31 + _m11;
                hash = hash * 31 + _m12;
                hash = hash * 31 + _m20;
                hash = hash * 31 + _m21;
                hash = hash * 31 + _m22;
                hash = hash * 31 + _relativeBottom;
                hash = hash * 31 + _occludedSides;
                hash = hash * 31 + _tileHalfExtent;
                hash = hash * 31 + _authoredClosurePolicy;
                hash = hash * 31 + _generateMissingClosure;
                hash = hash * 31 + _northBottom;
                hash = hash * 31 + _eastBottom;
                hash = hash * 31 + _southBottom;
                hash = hash * 31 + _westBottom;
                return hash;
            }
        }

        private static int Quantize(float value)
            => Mathf.RoundToInt(value * Quantization);
    }

    internal static class TileVerticalFillMeshUtility
    {
        private const float HeightEpsilon = 0.0001f;
        private const float FlatMeshEpsilon = 0.001f;
        private const float EdgeWeldPrecision = 10000f;

        public static bool TryCreate(TileMeshSource source, out Mesh result)
        {
            result = null;
            if (!source.IsValid || !source.HasVisibleBottomY)
                return false;

            Vector3[] sourceVertices = source.Mesh.vertices;
            if (sourceVertices == null || sourceVertices.Length == 0)
                return false;

            Matrix4x4 linearMatrix = source.LocalMatrix;
            float translationY = linearMatrix.m13;
            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;

            if (Mathf.Abs(linearMatrix.determinant) <= HeightEpsilon)
                return false;

            Matrix4x4 inverseLinear = linearMatrix.inverse;
            float targetBottom = source.VisibleBottomY - translationY;

            var originalRelativeY = new float[sourceVertices.Length];
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < sourceVertices.Length; i++)
            {
                float y = linearMatrix.MultiplyPoint3x4(sourceVertices[i]).y;
                originalRelativeY[i] = y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            float meshHeight = maxY - minY;
            if (meshHeight <= FlatMeshEpsilon)
            {
                if (targetBottom >= minY - HeightEpsilon)
                    return false;

                result = CreateFlatMeshWithSkirt(
                    source,
                    source.Mesh,
                    sourceVertices,
                    linearMatrix,
                    inverseLinear,
                    targetBottom);
                return result != null;
            }

            // Non-flat prefabs may contain authored cliffs, decoration or
            // deliberately closed geometry. PreserveAuthored is the safe default:
            // authored volume meshes are never deformed or clipped implicitly.
            // A designated child may still own a separate generated band below
            // the authored bounds so an elevated short cliff cannot hover above
            // its resolved support surface.
            if (source.AuthoredClosurePolicy == AuthoredClosurePolicy.PreserveAuthored)
            {
                if (!source.GenerateMissingClosure || !source.HasTileFootprint)
                    return false;

                result = CreatePreservedAuthoredMeshWithClosure(
                    source,
                    source.Mesh,
                    translationY + minY);
                return result != null;
            }

            if (targetBottom < minY - HeightEpsilon)
            {
                Vector3[] deformed = (Vector3[])sourceVertices.Clone();
                float bottomBand = minY + Mathf.Max(
                    HeightEpsilon * 10f,
                    meshHeight * 0.02f);

                for (int i = 0; i < deformed.Length; i++)
                {
                    Vector3 relative = linearMatrix.MultiplyPoint3x4(deformed[i]);
                    if (relative.y <= bottomBand)
                    {
                        relative.y = targetBottom;
                        deformed[i] = inverseLinear.MultiplyPoint3x4(relative);
                    }
                }

                result = CopyMesh(
                    source.Mesh,
                    deformed,
                    originalRelativeY,
                    removeFullyHiddenTriangles: false,
                    targetBottom);
                return result != null;
            }

            if (targetBottom > minY + HeightEpsilon)
            {
                // Do not collapse crossing triangles onto the floor. That produced
                // overlapping faces and bright white bloom artifacts. We only remove
                // triangles that are completely hidden below global Y=0.
                result = CopyMesh(
                    source.Mesh,
                    sourceVertices,
                    originalRelativeY,
                    removeFullyHiddenTriangles: true,
                    targetBottom);
                return result != null;
            }

            return false;
        }

        private static Mesh CopyMesh(
            Mesh source,
            Vector3[] vertices,
            IReadOnlyList<float> originalRelativeY,
            bool removeFullyHiddenTriangles,
            float targetBottom)
        {
            if (removeFullyHiddenTriangles)
                return CopyMeshCompacted(source, vertices, originalRelativeY, targetBottom);

            var mesh = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = source.indexFormat,
                vertices = vertices
            };

            CopyVertexChannels(source, mesh);

            mesh.subMeshCount = source.subMeshCount;
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                MeshTopology topology = source.GetTopology(subMesh);
                int[] indices = source.GetIndices(subMesh);
                mesh.SetIndices(indices, topology, subMesh, false);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CopyMeshCompacted(
            Mesh source,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<float> originalRelativeY,
            float targetBottom)
        {
            var indicesBySubMesh = new int[source.subMeshCount][];
            var topologyBySubMesh = new MeshTopology[source.subMeshCount];
            var referenced = new bool[source.vertexCount];
            int referencedCount = 0;

            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                MeshTopology topology = source.GetTopology(subMesh);
                topologyBySubMesh[subMesh] = topology;
                int[] indices = source.GetIndices(subMesh);
                if (topology == MeshTopology.Triangles)
                {
                    indices = RemoveFullyHiddenTriangles(
                        indices,
                        originalRelativeY,
                        targetBottom);
                }

                indicesBySubMesh[subMesh] = indices;
                for (int i = 0; i < indices.Length; i++)
                {
                    int index = indices[i];
                    if (index < 0 || index >= referenced.Length || referenced[index])
                        continue;

                    referenced[index] = true;
                    referencedCount++;
                }
            }

            if (referencedCount == 0)
                return null;

            var remap = new int[source.vertexCount];
            var compactVertices = new Vector3[referencedCount];
            int next = 0;
            for (int i = 0; i < referenced.Length; i++)
            {
                remap[i] = -1;
                if (!referenced[i])
                    continue;

                remap[i] = next;
                compactVertices[next] = vertices[i];
                next++;
            }

            var mesh = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = referencedCount > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16,
                vertices = compactVertices
            };

            CopyVertexChannelsCompacted(source, mesh, referenced, referencedCount);
            mesh.subMeshCount = source.subMeshCount;
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                int[] indices = indicesBySubMesh[subMesh];
                for (int i = 0; i < indices.Length; i++)
                    indices[i] = remap[indices[i]];

                mesh.SetIndices(indices, topologyBySubMesh[subMesh], subMesh, false);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFlatMeshWithSkirt(
    TileMeshSource tileSource,
    Mesh source,
    IReadOnlyList<Vector3> vertices,
    Matrix4x4 linearMatrix,
    Matrix4x4 inverseLinear,
    float targetBottom)
        {
            Dictionary<GeometricEdgeKey, BoundaryEdge> edges =
                CollectBoundaryEdges(source, vertices, linearMatrix);

            var skirtVertices = new List<Vector3>();
            var skirtUvs = new List<Vector2>();
            var skirtTriangles = new List<int>();

            Vector2 generatedSideUv =
                ResolveGeneratedSideUv(tileSource, source);

            foreach (BoundaryEdge edge in edges.Values)
            {
                if (edge.Count != 1)
                    continue;

                Vector3 worldA =
                    tileSource.LocalMatrix.MultiplyPoint3x4(
                        vertices[edge.A]);

                Vector3 worldB =
                    tileSource.LocalMatrix.MultiplyPoint3x4(
                        vertices[edge.B]);

                if (!TryResolveBoundarySide(
                        tileSource,
                        worldA,
                        worldB,
                        out TileMeshOccludedSides side)
                    || (tileSource.OccludedSides & side) != 0)
                {
                    continue;
                }

                Vector3 topA =
                    linearMatrix.MultiplyPoint3x4(
                        vertices[edge.A]);

                Vector3 topB =
                    linearMatrix.MultiplyPoint3x4(
                        vertices[edge.B]);

                float edgeBottom =
                    tileSource.EdgeBottoms.Resolve(
                        side,
                        tileSource.VisibleBottomY)
                    - tileSource.LocalMatrix.m13;

                if (edgeBottom
                    >= Mathf.Min(topA.y, topB.y)
                    - HeightEpsilon)
                {
                    continue;
                }

                Vector3 bottomA =
                    new Vector3(
                        topA.x,
                        edgeBottom,
                        topA.z);

                Vector3 bottomB =
                    new Vector3(
                        topB.x,
                        edgeBottom,
                        topB.z);

                AddOneSidedQuad(
                    skirtVertices,
                    skirtUvs,
                    skirtTriangles,
                    inverseLinear.MultiplyPoint3x4(topA),
                    inverseLinear.MultiplyPoint3x4(topB),
                    inverseLinear.MultiplyPoint3x4(bottomA),
                    inverseLinear.MultiplyPoint3x4(bottomB),
                    generatedSideUv);
            }

            if (skirtVertices.Count == 0)
                return null;

            var skirt = new Mesh
            {
                name = source.name + "_VerticalSkirt",
                indexFormat = skirtVertices.Count > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };

            skirt.SetVertices(skirtVertices);
            skirt.SetUVs(0, skirtUvs);
            skirt.SetTriangles(skirtTriangles, 0, false);
            skirt.RecalculateNormals();
            skirt.RecalculateBounds();

            return CombineSourceWithSkirt(
                source,
                skirt);
        }

        private static Mesh CreatePreservedAuthoredMeshWithClosure(
            TileMeshSource tileSource,
            Mesh source,
            float authoredBottomWorld)
        {
            Vector2 generatedSideUv =
                ResolveGeneratedSideUv(
                    tileSource,
                    source);

            Mesh uniformSideSource =
                CreateMeshWithUniformSideUvs(
                    tileSource,
                    source,
                    generatedSideUv);

            Mesh sourceForCombination =
                uniformSideSource != null
                    ? uniformSideSource
                    : source;

            Mesh skirt =
                CreateAuthoredContourClosureSkirt(
                    tileSource,
                    source,
                    authoredBottomWorld);

            skirt ??=
                CreateAxisAlignedClosureSkirt(
                    tileSource,
                    source,
                    authoredBottomWorld);

            if (skirt == null)
            {
                return uniformSideSource;
            }

            Mesh result =
                CombineSourceWithSkirt(
                    sourceForCombination,
                    skirt);

            if (uniformSideSource != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(
                        uniformSideSource);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(
                        uniformSideSource);
                }
            }

            return result;
        }

        private static Mesh CreateAuthoredContourClosureSkirt(
            TileMeshSource source,
            Mesh mesh,
            float authoredBottomWorld)
        {
            if (mesh == null
                || !mesh.isReadable
                || mesh.vertexCount <= 0
                || !IsFinite(authoredBottomWorld)
                || !IsFinite(source.VisibleBottomY))
            {
                return null;
            }

            Vector3[] sourceVertices =
                mesh.vertices;

            if (sourceVertices == null
                || sourceVertices.Length == 0)
            {
                return null;
            }

            Matrix4x4 linearMatrix =
                source.LocalMatrix;

            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;

            var relativeVertices =
                new Vector3[sourceVertices.Length];

            float minimumY =
                float.PositiveInfinity;

            float maximumY =
                float.NegativeInfinity;

            for (int i = 0;
                 i < sourceVertices.Length;
                 i++)
            {
                Vector3 relative =
                    linearMatrix.MultiplyPoint3x4(
                        sourceVertices[i]);

                relativeVertices[i] = relative;

                minimumY =
                    Mathf.Min(minimumY, relative.y);

                maximumY =
                    Mathf.Max(maximumY, relative.y);
            }

            float meshHeight =
                Mathf.Max(
                    HeightEpsilon,
                    maximumY - minimumY);

            float bottomTolerance =
                Mathf.Max(
                    0.001f,
                    meshHeight * 0.04f);

            Dictionary<GeometricEdgeKey, AuthoredContourEdge>
                contourEdges =
                    CollectAuthoredBottomContourEdges(
                        mesh,
                        relativeVertices,
                        minimumY,
                        bottomTolerance);

            if (contourEdges.Count == 0)
                return null;

            var vertices =
                new List<Vector3>(
                    contourEdges.Count * 4);

            var uvs =
                new List<Vector2>(
                    contourEdges.Count * 4);

            var normals =
                new List<Vector3>(
                    contourEdges.Count * 4);

            var triangles =
                new List<int>(
                    contourEdges.Count * 6);

            Matrix4x4 worldToSource =
                source.LocalMatrix.inverse;

            Vector2 generatedSideUv =
                ResolveGeneratedSideUv(
                    source,
                    mesh);

            foreach (AuthoredContourEdge edge
                     in contourEdges.Values)
            {
                Vector3 worldTopA =
                    source.LocalMatrix.MultiplyPoint3x4(
                        sourceVertices[edge.A]);

                Vector3 worldTopB =
                    source.LocalMatrix.MultiplyPoint3x4(
                        sourceVertices[edge.B]);

                // Вирівнюємо дрібні похибки нижнього кільця.
                if (Mathf.Abs(
                        worldTopA.y - authoredBottomWorld)
                    <= bottomTolerance)
                {
                    worldTopA.y =
                        authoredBottomWorld;
                }

                if (Mathf.Abs(
                        worldTopB.y - authoredBottomWorld)
                    <= bottomTolerance)
                {
                    worldTopB.y =
                        authoredBottomWorld;
                }

                TileMeshOccludedSides side =
                    ResolveContourSide(
                        source,
                        worldTopA,
                        worldTopB,
                        edge.ExpectedNormal);

                if ((source.OccludedSides & side) != 0)
                    continue;

                float bottomY =
                    source.EdgeBottoms.Resolve(
                        side,
                        source.VisibleBottomY);

                if (!IsFinite(bottomY)
                    || bottomY
                    >= Mathf.Min(
                           worldTopA.y,
                           worldTopB.y)
                       - HeightEpsilon)
                {
                    continue;
                }

                Vector3 worldBottomA =
                    worldTopA;

                Vector3 worldBottomB =
                    worldTopB;

                worldBottomA.y = bottomY;
                worldBottomB.y = bottomY;

                /*
                 * Зберігаємо winding таким самим, як у
                 * авторської бокової поверхні.
                 */
                Vector3 generatedNormal =
                    Vector3.Cross(
                        worldBottomA - worldTopA,
                        worldTopB - worldTopA);

                if (generatedNormal.sqrMagnitude
                        > HeightEpsilon
                    && edge.ExpectedNormal.sqrMagnitude
                        > HeightEpsilon
                    && Vector3.Dot(
                           generatedNormal,
                           edge.ExpectedNormal) < 0f)
                {
                    Vector3 temporaryTop =
                        worldTopA;

                    worldTopA =
                        worldTopB;

                    worldTopB =
                        temporaryTop;

                    Vector3 temporaryBottom =
                        worldBottomA;

                    worldBottomA =
                        worldBottomB;

                    worldBottomB =
                        temporaryBottom;
                }

                Vector3 localTopA =
                    worldToSource.MultiplyPoint3x4(
                        worldTopA);

                Vector3 localTopB =
                    worldToSource.MultiplyPoint3x4(
                        worldTopB);

                Vector3 localBottomA =
                    worldToSource.MultiplyPoint3x4(
                        worldBottomA);

                Vector3 localBottomB =
                    worldToSource.MultiplyPoint3x4(
                        worldBottomB);

                Vector3 worldSideNormal =
                    Vector3.Cross(
                        worldBottomA - worldTopA,
                        worldTopB - worldTopA);

                worldSideNormal.y = 0f;

                if (worldSideNormal.sqrMagnitude
                    <= HeightEpsilon)
                {
                    worldSideNormal =
                        edge.ExpectedNormal;

                    worldSideNormal.y = 0f;
                }

                if (worldSideNormal.sqrMagnitude
                    <= HeightEpsilon)
                {
                    worldSideNormal =
                        Vector3.Cross(
                            worldBottomA - worldTopA,
                            worldTopB - worldTopA);
                }

                worldSideNormal.Normalize();

                Vector3 localSideNormal =
                    source.LocalMatrix.transpose
                        .MultiplyVector(worldSideNormal)
                        .normalized;

                Vector3 localQuadNormal =
                    Vector3.Cross(
                        localBottomA - localTopA,
                        localTopB - localTopA)
                    .normalized;

                if (localQuadNormal.sqrMagnitude
                        > HeightEpsilon
                    && Vector3.Dot(
                           localSideNormal,
                           localQuadNormal) < 0f)
                {
                    localSideNormal =
                        -localSideNormal;
                }

                AddOneSidedQuad(
                    vertices,
                    uvs,
                    triangles,
                    localTopA,
                    localTopB,
                    localBottomA,
                    localBottomB,
                    generatedSideUv);

                AddQuadNormals(
                    normals,
                    localSideNormal);
            }

            if (vertices.Count == 0)
                return null;

            var skirt = new Mesh
            {
                name =
                    mesh.name
                    + "_AuthoredContourClosure",

                indexFormat =
                    vertices.Count > 65535
                        ? IndexFormat.UInt32
                        : IndexFormat.UInt16
            };

            skirt.SetVertices(vertices);
            skirt.SetUVs(0, uvs);
            skirt.SetNormals(normals);
            skirt.SetTriangles(
                triangles,
                0,
                false);

            skirt.RecalculateBounds();

            return skirt;
        }

        private static TileMeshOccludedSides ResolveContourSide(
    TileMeshSource source,
    Vector3 worldA,
    Vector3 worldB,
    Vector3 expectedNormal)
        {
            Vector3 midpoint =
                (worldA + worldB) * 0.5f;

            float deltaX =
                midpoint.x
                - source.TileCenterXZ.x;

            float deltaZ =
                midpoint.z
                - source.TileCenterXZ.y;

            if (Mathf.Abs(deltaX) > 0.0001f
                || Mathf.Abs(deltaZ) > 0.0001f)
            {
                if (Mathf.Abs(deltaX)
                    >= Mathf.Abs(deltaZ))
                {
                    return deltaX >= 0f
                        ? TileMeshOccludedSides.East
                        : TileMeshOccludedSides.West;
                }

                return deltaZ >= 0f
                    ? TileMeshOccludedSides.North
                    : TileMeshOccludedSides.South;
            }

            // Резервний варіант для геометрії,
            // центр якої збігається з центром tile.
            if (Mathf.Abs(expectedNormal.x)
                >= Mathf.Abs(expectedNormal.z))
            {
                return expectedNormal.x >= 0f
                    ? TileMeshOccludedSides.East
                    : TileMeshOccludedSides.West;
            }

            return expectedNormal.z >= 0f
                ? TileMeshOccludedSides.North
                : TileMeshOccludedSides.South;
        }

        private readonly struct AuthoredContourEdge
        {
            public AuthoredContourEdge(
                int a,
                int b,
                Vector3 expectedNormal)
            {
                A = a;
                B = b;
                ExpectedNormal =
                    expectedNormal;
            }

            public int A { get; }
            public int B { get; }
            public Vector3 ExpectedNormal { get; }
        }

        private static Dictionary<
            GeometricEdgeKey,
            AuthoredContourEdge>
            CollectAuthoredBottomContourEdges(
                Mesh mesh,
                IReadOnlyList<Vector3> relativeVertices,
                float minimumY,
                float bottomTolerance)
        {
            var result =
                new Dictionary<
                    GeometricEdgeKey,
                    AuthoredContourEdge>();

            for (int subMesh = 0;
                 subMesh < mesh.subMeshCount;
                 subMesh++)
            {
                if (mesh.GetTopology(subMesh)
                    != MeshTopology.Triangles)
                {
                    continue;
                }

                int[] indices =
                    mesh.GetIndices(subMesh);

                for (int i = 0;
                     i + 2 < indices.Length;
                     i += 3)
                {
                    int indexA = indices[i];
                    int indexB = indices[i + 1];
                    int indexC = indices[i + 2];

                    if ((uint)indexA
                            >= (uint)relativeVertices.Count
                        || (uint)indexB
                            >= (uint)relativeVertices.Count
                        || (uint)indexC
                            >= (uint)relativeVertices.Count)
                    {
                        continue;
                    }

                    Vector3 a =
                        relativeVertices[indexA];

                    Vector3 b =
                        relativeVertices[indexB];

                    Vector3 c =
                        relativeVertices[indexC];

                    Vector3 cross =
                        Vector3.Cross(
                            b - a,
                            c - a);

                    if (cross.sqrMagnitude
                        <= HeightEpsilon)
                    {
                        continue;
                    }

                    Vector3 faceNormal =
                        cross.normalized;

                    /*
                     * Горизонтальні bottom/top faces нам не потрібні.
                     * Беремо лише вертикальні або похилі боковини.
                     */
                    if (Mathf.Abs(faceNormal.y) > 0.8f)
                        continue;

                    AddAuthoredBottomContourEdge(
                        result,
                        relativeVertices,
                        indexA,
                        indexB,
                        minimumY,
                        bottomTolerance,
                        faceNormal);

                    AddAuthoredBottomContourEdge(
                        result,
                        relativeVertices,
                        indexB,
                        indexC,
                        minimumY,
                        bottomTolerance,
                        faceNormal);

                    AddAuthoredBottomContourEdge(
                        result,
                        relativeVertices,
                        indexC,
                        indexA,
                        minimumY,
                        bottomTolerance,
                        faceNormal);
                }
            }

            return result;
        }

        private static void AddAuthoredBottomContourEdge(
            Dictionary<
                GeometricEdgeKey,
                AuthoredContourEdge> edges,
            IReadOnlyList<Vector3> vertices,
            int indexA,
            int indexB,
            float minimumY,
            float bottomTolerance,
            Vector3 expectedNormal)
        {
            Vector3 a =
                vertices[indexA];

            Vector3 b =
                vertices[indexB];

            if (Mathf.Abs(a.y - minimumY)
                    > bottomTolerance
                || Mathf.Abs(b.y - minimumY)
                    > bottomTolerance)
            {
                return;
            }

            Vector2 horizontalDelta =
                new Vector2(
                    b.x - a.x,
                    b.z - a.z);

            if (horizontalDelta.sqrMagnitude
                <= HeightEpsilon)
            {
                return;
            }

            var key =
                new GeometricEdgeKey(a, b);

            if (edges.ContainsKey(key))
                return;

            edges[key] =
                new AuthoredContourEdge(
                    indexA,
                    indexB,
                    expectedNormal);
        }

        private static Mesh CreateMeshWithUniformSideUvs(
            TileMeshSource tileSource,
            Mesh source,
            Vector2 sideUv)
        {
            if (source == null
                || !source.isReadable
                || source.vertexCount <= 0
                || source.subMeshCount <= 0)
            {
                return null;
            }

            Vector3[] sourceVertices = source.vertices;
            if (sourceVertices == null
                || sourceVertices.Length == 0)
            {
                return null;
            }

            Vector3[] sourceNormals = source.normals;
            Vector4[] sourceTangents = source.tangents;
            Color32[] sourceColors = source.colors32;

            bool hasNormals =
                sourceNormals != null
                && sourceNormals.Length == sourceVertices.Length;

            bool hasTangents =
                sourceTangents != null
                && sourceTangents.Length == sourceVertices.Length;

            bool hasColors =
                sourceColors != null
                && sourceColors.Length == sourceVertices.Length;

            var sourceUvs = new List<Vector4>[8];
            var outputUvs = new List<Vector4>[8];
            var hasUvChannel = new bool[8];

            for (int channel = 0; channel < 8; channel++)
            {
                var channelUvs = new List<Vector4>(source.vertexCount);
                source.GetUVs(channel, channelUvs);

                if (channelUvs.Count != source.vertexCount)
                    continue;

                sourceUvs[channel] = channelUvs;
                outputUvs[channel] =
                    new List<Vector4>(source.vertexCount * 3);
                hasUvChannel[channel] = true;
            }

            if (!hasUvChannel[0])
            {
                sourceUvs[0] = new List<Vector4>(source.vertexCount);
                for (int i = 0; i < source.vertexCount; i++)
                    sourceUvs[0].Add(Vector4.zero);

                outputUvs[0] =
                    new List<Vector4>(source.vertexCount * 3);
                hasUvChannel[0] = true;
            }

            var outputVertices =
                new List<Vector3>(source.vertexCount * 3);

            var outputNormals =
                hasNormals
                    ? new List<Vector3>(source.vertexCount * 3)
                    : null;

            var outputTangents =
                hasTangents
                    ? new List<Vector4>(source.vertexCount * 3)
                    : null;

            var outputColors =
                hasColors
                    ? new List<Color32>(source.vertexCount * 3)
                    : null;

            var vertexVariants =
                new Dictionary<UniformSideVertexKey, int>(
                    source.vertexCount * 3);

            int GetOrCreateVertex(
                int sourceIndex,
                bool useUniformSideUv,
                Vector3 uniformSideNormal)
            {
                var key =
                    new UniformSideVertexKey(
                        sourceIndex,
                        useUniformSideUv,
                        uniformSideNormal);

                if (vertexVariants.TryGetValue(key, out int existing))
                    return existing;

                int outputIndex = outputVertices.Count;
                vertexVariants[key] = outputIndex;

                outputVertices.Add(sourceVertices[sourceIndex]);

                if (hasNormals)
                {
                    Vector3 normal =
                        useUniformSideUv
                        && uniformSideNormal.sqrMagnitude > HeightEpsilon
                            ? uniformSideNormal
                            : sourceNormals[sourceIndex];

                    outputNormals.Add(normal.normalized);
                }

                if (hasTangents)
                    outputTangents.Add(sourceTangents[sourceIndex]);

                if (hasColors)
                    outputColors.Add(sourceColors[sourceIndex]);

                for (int channel = 0; channel < 8; channel++)
                {
                    if (!hasUvChannel[channel])
                        continue;

                    Vector4 uv = sourceUvs[channel][sourceIndex];
                    if (channel == 0 && useUniformSideUv)
                    {
                        uv.x = sideUv.x;
                        uv.y = sideUv.y;
                    }

                    outputUvs[channel].Add(uv);
                }

                return outputIndex;
            }

            Matrix4x4 linearMatrix = tileSource.LocalMatrix;
            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;

            Matrix4x4 worldNormalToLocal =
                tileSource.LocalMatrix.transpose;

            var outputIndices = new int[source.subMeshCount][];
            var outputTopologies =
                new MeshTopology[source.subMeshCount];

            for (int subMesh = 0;
                 subMesh < source.subMeshCount;
                 subMesh++)
            {
                MeshTopology topology = source.GetTopology(subMesh);
                outputTopologies[subMesh] = topology;

                int[] sourceIndices = source.GetIndices(subMesh);
                var remapped = new int[sourceIndices.Length];

                if (topology != MeshTopology.Triangles)
                {
                    for (int i = 0; i < sourceIndices.Length; i++)
                    {
                        remapped[i] =
                            GetOrCreateVertex(
                                sourceIndices[i],
                                false,
                                Vector3.zero);
                    }

                    outputIndices[subMesh] = remapped;
                    continue;
                }

                for (int i = 0; i + 2 < sourceIndices.Length; i += 3)
                {
                    int indexA = sourceIndices[i];
                    int indexB = sourceIndices[i + 1];
                    int indexC = sourceIndices[i + 2];

                    Vector3 a =
                        linearMatrix.MultiplyPoint3x4(
                            sourceVertices[indexA]);

                    Vector3 b =
                        linearMatrix.MultiplyPoint3x4(
                            sourceVertices[indexB]);

                    Vector3 c =
                        linearMatrix.MultiplyPoint3x4(
                            sourceVertices[indexC]);

                    Vector3 worldCross =
                        Vector3.Cross(b - a, c - a);

                    bool useUniformSideUv = false;
                    Vector3 localSideNormal = Vector3.zero;

                    if (worldCross.sqrMagnitude > HeightEpsilon)
                    {
                        Vector3 worldFaceNormal =
                            worldCross.normalized;

                        useUniformSideUv =
                            Mathf.Abs(worldFaceNormal.y) < 0.85f;

                        if (useUniformSideUv && hasNormals)
                        {
                            Vector3 worldSideNormal =
                                new Vector3(
                                    worldFaceNormal.x,
                                    0f,
                                    worldFaceNormal.z);

                            if (worldSideNormal.sqrMagnitude
                                <= HeightEpsilon)
                            {
                                worldSideNormal =
                                    worldFaceNormal;
                            }

                            worldSideNormal.Normalize();

                            localSideNormal =
                                worldNormalToLocal
                                    .MultiplyVector(worldSideNormal)
                                    .normalized;

                            Vector3 localFaceNormal =
                                Vector3.Cross(
                                    sourceVertices[indexB]
                                        - sourceVertices[indexA],
                                    sourceVertices[indexC]
                                        - sourceVertices[indexA])
                                .normalized;

                            if (localFaceNormal.sqrMagnitude
                                    > HeightEpsilon
                                && Vector3.Dot(
                                       localSideNormal,
                                       localFaceNormal) < 0f)
                            {
                                localSideNormal =
                                    -localSideNormal;
                            }
                        }
                    }

                    remapped[i] =
                        GetOrCreateVertex(
                            indexA,
                            useUniformSideUv,
                            localSideNormal);

                    remapped[i + 1] =
                        GetOrCreateVertex(
                            indexB,
                            useUniformSideUv,
                            localSideNormal);

                    remapped[i + 2] =
                        GetOrCreateVertex(
                            indexC,
                            useUniformSideUv,
                            localSideNormal);
                }

                outputIndices[subMesh] = remapped;
            }

            if (outputVertices.Count == 0)
                return null;

            var result = new Mesh
            {
                name = source.name + "_UniformSideUv",
                indexFormat =
                    outputVertices.Count > 65535
                        ? IndexFormat.UInt32
                        : IndexFormat.UInt16
            };

            result.SetVertices(outputVertices);

            if (hasNormals)
                result.SetNormals(outputNormals);

            if (hasTangents)
                result.SetTangents(outputTangents);

            if (hasColors)
                result.SetColors(outputColors);

            for (int channel = 0; channel < 8; channel++)
            {
                if (hasUvChannel[channel])
                    result.SetUVs(channel, outputUvs[channel]);
            }

            result.subMeshCount = source.subMeshCount;

            for (int subMesh = 0;
                 subMesh < source.subMeshCount;
                 subMesh++)
            {
                result.SetIndices(
                    outputIndices[subMesh],
                    outputTopologies[subMesh],
                    subMesh,
                    false);
            }

            if (!hasNormals)
                result.RecalculateNormals();

            result.RecalculateBounds();
            return result;
        }

        private static Mesh CreateAxisAlignedClosureSkirt(
    TileMeshSource source,
    Mesh mesh,
    float authoredBottomWorld)
        {
            if (!source.HasTileFootprint
                || !IsFinite(authoredBottomWorld)
                || !IsFinite(source.VisibleBottomY))
            {
                return null;
            }

            var vertices = new List<Vector3>(16);
            var uvs = new List<Vector2>(16);
            var triangles = new List<int>(24);

            float half = source.TileHalfExtent;

            float west =
                source.TileCenterXZ.x - half;

            float east =
                source.TileCenterXZ.x + half;

            float south =
                source.TileCenterXZ.y - half;

            float north =
                source.TileCenterXZ.y + half;

            Matrix4x4 worldToSource =
                source.LocalMatrix.inverse;

            Vector2 generatedSideUv =
                ResolveGeneratedSideUv(
                    source,
                    mesh);

            AddClosureSide(
                source,
                TileMeshOccludedSides.North,
                new Vector3(west, authoredBottomWorld, north),
                new Vector3(east, authoredBottomWorld, north),
                generatedSideUv,
                worldToSource,
                vertices,
                uvs,
                triangles);

            AddClosureSide(
                source,
                TileMeshOccludedSides.East,
                new Vector3(east, authoredBottomWorld, north),
                new Vector3(east, authoredBottomWorld, south),
                generatedSideUv,
                worldToSource,
                vertices,
                uvs,
                triangles);

            AddClosureSide(
                source,
                TileMeshOccludedSides.South,
                new Vector3(east, authoredBottomWorld, south),
                new Vector3(west, authoredBottomWorld, south),
                generatedSideUv,
                worldToSource,
                vertices,
                uvs,
                triangles);

            AddClosureSide(
                source,
                TileMeshOccludedSides.West,
                new Vector3(west, authoredBottomWorld, south),
                new Vector3(west, authoredBottomWorld, north),
                generatedSideUv,
                worldToSource,
                vertices,
                uvs,
                triangles);

            if (vertices.Count == 0)
                return null;

            var skirt = new Mesh
            {
                name = mesh.name + "_MissingClosure",
                indexFormat = vertices.Count > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };

            skirt.SetVertices(vertices);
            skirt.SetUVs(0, uvs);
            skirt.SetTriangles(triangles, 0, false);
            skirt.RecalculateNormals();
            skirt.RecalculateBounds();

            return skirt;
        }

        private static void AddClosureSide(
    TileMeshSource source,
    TileMeshOccludedSides side,
    Vector3 worldTopA,
    Vector3 worldTopB,
    Vector2 generatedSideUv,
    Matrix4x4 worldToSource,
    List<Vector3> vertices,
    List<Vector2> uvs,
    List<int> triangles)
        {
            if ((source.OccludedSides & side) != 0)
                return;

            float bottomY =
                source.EdgeBottoms.Resolve(
                    side,
                    source.VisibleBottomY);

            if (!IsFinite(bottomY)
                || bottomY
                >= worldTopA.y - HeightEpsilon)
            {
                return;
            }

            Vector3 worldBottomA = worldTopA;
            Vector3 worldBottomB = worldTopB;

            worldBottomA.y = bottomY;
            worldBottomB.y = bottomY;

            AddOneSidedQuad(
                vertices,
                uvs,
                triangles,
                worldToSource.MultiplyPoint3x4(worldTopA),
                worldToSource.MultiplyPoint3x4(worldTopB),
                worldToSource.MultiplyPoint3x4(worldBottomA),
                worldToSource.MultiplyPoint3x4(worldBottomB),
                generatedSideUv);
        }

        private static Mesh CombineSourceWithSkirt(Mesh source, Mesh skirt)
        {
            var combines = new CombineInstance[source.subMeshCount + 1];
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                combines[subMesh] = new CombineInstance
                {
                    mesh = source,
                    subMeshIndex = subMesh,
                    transform = Matrix4x4.identity
                };
            }

            combines[combines.Length - 1] = new CombineInstance
            {
                mesh = skirt,
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            };

            var combined = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = source.vertexCount + skirt.vertexCount > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };
            combined.CombineMeshes(combines, false, true);
            combined.RecalculateBounds();

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(skirt);
            else
                UnityEngine.Object.DestroyImmediate(skirt);

            return combined;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        internal static bool IsBoundaryEdgeOccluded(
            TileMeshSource source,
            Vector3 worldA,
            Vector3 worldB)
        {
            return TryResolveBoundarySide(source, worldA, worldB, out TileMeshOccludedSides side)
                && (source.OccludedSides & side) != 0;
        }

        private static bool TryResolveBoundarySide(
            TileMeshSource source,
            Vector3 worldA,
            Vector3 worldB,
            out TileMeshOccludedSides side)
        {
            side = TileMeshOccludedSides.None;
            if (!source.HasTileFootprint)
                return false;

            float half = source.TileHalfExtent;
            float tolerance = Mathf.Max(0.001f, half * 0.02f);
            float west = source.TileCenterXZ.x - half;
            float east = source.TileCenterXZ.x + half;
            float south = source.TileCenterXZ.y - half;
            float north = source.TileCenterXZ.y + half;

            if (IsNear(worldA.z, north, tolerance)
                && IsNear(worldB.z, north, tolerance))
            {
                side = TileMeshOccludedSides.North;
                return true;
            }

            if (IsNear(worldA.x, east, tolerance)
                && IsNear(worldB.x, east, tolerance))
            {
                side = TileMeshOccludedSides.East;
                return true;
            }

            if (IsNear(worldA.z, south, tolerance)
                && IsNear(worldB.z, south, tolerance))
            {
                side = TileMeshOccludedSides.South;
                return true;
            }

            if (IsNear(worldA.x, west, tolerance)
                && IsNear(worldB.x, west, tolerance))
            {
                side = TileMeshOccludedSides.West;
                return true;
            }

            return false;
        }

        private static bool IsNear(float value, float target, float tolerance)
            => Mathf.Abs(value - target) <= tolerance;

        private static void AddOneSidedQuad(
    List<Vector3> vertices,
    List<Vector2> uvs,
    List<int> triangles,
    Vector3 topA,
    Vector3 topB,
    Vector3 bottomA,
    Vector3 bottomB,
    Vector2 generatedSideUv)
        {
            int front = vertices.Count;

            vertices.Add(topA);
            vertices.Add(topB);
            vertices.Add(bottomA);
            vertices.Add(bottomB);

            AddQuadUvs(
                uvs,
                generatedSideUv);

            triangles.Add(front);
            triangles.Add(front + 2);
            triangles.Add(front + 1);

            triangles.Add(front + 1);
            triangles.Add(front + 2);
            triangles.Add(front + 3);
        }

        private static void AddQuadUvs(
            List<Vector2> uvs,
            Vector2 generatedSideUv)
        {
            // Palette materials must sample one palette cell.
            // Stretching UV across width/height traverses the entire atlas.
            uvs.Add(generatedSideUv);
            uvs.Add(generatedSideUv);
            uvs.Add(generatedSideUv);
            uvs.Add(generatedSideUv);
        }

        private static void AddQuadNormals(
            List<Vector3> normals,
            Vector3 normal)
        {
            Vector3 normalized =
                normal.sqrMagnitude > HeightEpsilon
                    ? normal.normalized
                    : Vector3.forward;

            normals.Add(normalized);
            normals.Add(normalized);
            normals.Add(normalized);
            normals.Add(normalized);
        }

        private static Vector2 ResolveGeneratedSideUv(
     TileMeshSource tileSource,
     Mesh mesh)
        {
            if (mesh == null
                || !mesh.isReadable
                || mesh.vertexCount <= 0)
            {
                return Vector2.zero;
            }

            Vector3[] vertices =
                mesh.vertices;

            Vector2[] uvs =
                mesh.uv;

            if (vertices == null
                || uvs == null
                || vertices.Length == 0
                || uvs.Length != vertices.Length)
            {
                return Vector2.zero;
            }

            Matrix4x4 linearMatrix =
                tileSource.LocalMatrix;

            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;

            var weights =
                new Dictionary<Vector2Int, float>();

            var representatives =
                new Dictionary<Vector2Int, Vector2>();

            for (int subMesh = 0;
                 subMesh < mesh.subMeshCount;
                 subMesh++)
            {
                if (mesh.GetTopology(subMesh)
                    != MeshTopology.Triangles)
                {
                    continue;
                }

                int[] indices =
                    mesh.GetIndices(subMesh);

                for (int i = 0;
                     i + 2 < indices.Length;
                     i += 3)
                {
                    int indexA = indices[i];
                    int indexB = indices[i + 1];
                    int indexC = indices[i + 2];

                    Vector3 a =
                        linearMatrix.MultiplyPoint3x4(
                            vertices[indexA]);

                    Vector3 b =
                        linearMatrix.MultiplyPoint3x4(
                            vertices[indexB]);

                    Vector3 c =
                        linearMatrix.MultiplyPoint3x4(
                            vertices[indexC]);

                    Vector3 cross =
                        Vector3.Cross(
                            b - a,
                            c - a);

                    float area =
                        cross.magnitude;

                    if (area <= HeightEpsilon)
                        continue;

                    Vector3 normal =
                        cross / area;

                    // Беремо тільки side або bevel faces.
                    if (Mathf.Abs(normal.y) >= 0.85f)
                        continue;

                    /*
                     * Центр UV-трикутника знаходиться всередині
                     * palette cell і не потрапляє на її межу.
                     */
                    Vector2 centroid =
                        (
                            uvs[indexA]
                            + uvs[indexB]
                            + uvs[indexC]
                        ) / 3f;

                    var key =
                        new Vector2Int(
                            Mathf.RoundToInt(
                                centroid.x * 4096f),
                            Mathf.RoundToInt(
                                centroid.y * 4096f));

                    if (weights.TryGetValue(
                            key,
                            out float currentWeight))
                    {
                        weights[key] =
                            currentWeight + area;
                    }
                    else
                    {
                        weights[key] = area;
                        representatives[key] =
                            centroid;
                    }
                }
            }

            bool found = false;

            float strongestWeight =
                float.NegativeInfinity;

            Vector2 result =
                Vector2.zero;

            foreach (KeyValuePair<Vector2Int, float> pair
                     in weights)
            {
                if (found
                    && pair.Value <= strongestWeight)
                {
                    continue;
                }

                if (!representatives.TryGetValue(
                        pair.Key,
                        out Vector2 candidate))
                {
                    continue;
                }

                strongestWeight =
                    pair.Value;

                result =
                    candidate;

                found = true;
            }

            if (found)
                return result;

            /*
             * Fallback: центр першого UV-трикутника,
             * а не UV окремої вершини.
             */
            for (int subMesh = 0;
                 subMesh < mesh.subMeshCount;
                 subMesh++)
            {
                if (mesh.GetTopology(subMesh)
                    != MeshTopology.Triangles)
                {
                    continue;
                }

                int[] indices =
                    mesh.GetIndices(subMesh);

                if (indices.Length < 3)
                    continue;

                return (
                    uvs[indices[0]]
                    + uvs[indices[1]]
                    + uvs[indices[2]]
                ) / 3f;
            }

            return uvs.Length > 0
                ? uvs[0]
                : Vector2.zero;
        }

        private static void AddUvWeight(
            Dictionary<Vector2Int, float> weights,
            Dictionary<Vector2Int, Vector2> representatives,
            Vector2 uv,
            float weight)
        {
            if (float.IsNaN(uv.x)
                || float.IsInfinity(uv.x)
                || float.IsNaN(uv.y)
                || float.IsInfinity(uv.y))
            {
                return;
            }

            var key = new Vector2Int(
                Mathf.RoundToInt(uv.x * 4096f),
                Mathf.RoundToInt(uv.y * 4096f));

            if (weights.TryGetValue(
                    key,
                    out float currentWeight))
            {
                weights[key] =
                    currentWeight + weight;
            }
            else
            {
                weights[key] = weight;
            }

            if (!representatives.ContainsKey(key))
                representatives[key] = uv;
        }

        private static bool TryResolveStrongestUv(
            Dictionary<Vector2Int, float> weights,
            Dictionary<Vector2Int, Vector2> representatives,
            out Vector2 result)
        {
            result = Vector2.zero;

            if (weights == null
                || weights.Count == 0)
            {
                return false;
            }

            float strongestWeight =
                float.NegativeInfinity;

            Vector2Int strongestKey =
                default;

            bool found = false;

            foreach (KeyValuePair<Vector2Int, float> pair
                     in weights)
            {
                if (found
                    && pair.Value <= strongestWeight)
                {
                    continue;
                }

                strongestWeight = pair.Value;
                strongestKey = pair.Key;
                found = true;
            }

            return found
                   && representatives.TryGetValue(
                       strongestKey,
                       out result);
        }

        private static Dictionary<GeometricEdgeKey, BoundaryEdge> CollectBoundaryEdges(
            Mesh source,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 linearMatrix)
        {
            var edges = new Dictionary<GeometricEdgeKey, BoundaryEdge>();
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                if (source.GetTopology(subMesh) != MeshTopology.Triangles)
                    continue;

                int[] indices = source.GetIndices(subMesh);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    AddEdge(edges, vertices, linearMatrix, indices[i], indices[i + 1]);
                    AddEdge(edges, vertices, linearMatrix, indices[i + 1], indices[i + 2]);
                    AddEdge(edges, vertices, linearMatrix, indices[i + 2], indices[i]);
                }
            }

            return edges;
        }

        private static void AddEdge(
            Dictionary<GeometricEdgeKey, BoundaryEdge> edges,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 linearMatrix,
            int a,
            int b)
        {
            Vector3 positionA = linearMatrix.MultiplyPoint3x4(vertices[a]);
            Vector3 positionB = linearMatrix.MultiplyPoint3x4(vertices[b]);
            var key = new GeometricEdgeKey(positionA, positionB);

            if (edges.TryGetValue(key, out BoundaryEdge existing))
            {
                existing.Count++;
                edges[key] = existing;
                return;
            }

            edges[key] = new BoundaryEdge(a, b, 1);
        }

        private static int[] RemoveFullyHiddenTriangles(
            IReadOnlyList<int> indices,
            IReadOnlyList<float> originalRelativeY,
            float targetBottom)
        {
            var visible = new List<int>(indices.Count);
            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                int a = indices[i];
                int b = indices[i + 1];
                int c = indices[i + 2];

                bool hidden = originalRelativeY[a] < targetBottom - HeightEpsilon
                    && originalRelativeY[b] < targetBottom - HeightEpsilon
                    && originalRelativeY[c] < targetBottom - HeightEpsilon;
                if (hidden)
                    continue;

                visible.Add(a);
                visible.Add(b);
                visible.Add(c);
            }

            return visible.ToArray();
        }

        private static void CopyVertexChannels(Mesh source, Mesh destination)
        {
            Vector3[] normals = source.normals;
            if (normals != null && normals.Length == source.vertexCount)
                destination.normals = normals;

            Vector4[] tangents = source.tangents;
            if (tangents != null && tangents.Length == source.vertexCount)
                destination.tangents = tangents;

            Color32[] colors = source.colors32;
            if (colors != null && colors.Length == source.vertexCount)
                destination.colors32 = colors;

            BoneWeight[] boneWeights = source.boneWeights;
            if (boneWeights != null && boneWeights.Length == source.vertexCount)
                destination.boneWeights = boneWeights;

            Matrix4x4[] bindposes = source.bindposes;
            if (bindposes != null && bindposes.Length > 0)
                destination.bindposes = bindposes;

            for (int channel = 0; channel < 8; channel++)
            {
                var uvs = new List<Vector4>(source.vertexCount);
                source.GetUVs(channel, uvs);
                if (uvs.Count == source.vertexCount)
                    destination.SetUVs(channel, uvs);
            }
        }

        private static void CopyVertexChannelsCompacted(
            Mesh source,
            Mesh destination,
            IReadOnlyList<bool> referenced,
            int referencedCount)
        {
            Vector3[] normals = source.normals;
            if (normals != null && normals.Length == source.vertexCount)
            {
                var compact = new Vector3[referencedCount];
                CopyReferenced(normals, compact, referenced);
                destination.normals = compact;
            }

            Vector4[] tangents = source.tangents;
            if (tangents != null && tangents.Length == source.vertexCount)
            {
                var compact = new Vector4[referencedCount];
                CopyReferenced(tangents, compact, referenced);
                destination.tangents = compact;
            }

            Color32[] colors = source.colors32;
            if (colors != null && colors.Length == source.vertexCount)
            {
                var compact = new Color32[referencedCount];
                CopyReferenced(colors, compact, referenced);
                destination.colors32 = compact;
            }

            BoneWeight[] boneWeights = source.boneWeights;
            if (boneWeights != null && boneWeights.Length == source.vertexCount)
            {
                var compact = new BoneWeight[referencedCount];
                CopyReferenced(boneWeights, compact, referenced);
                destination.boneWeights = compact;
            }

            Matrix4x4[] bindposes = source.bindposes;
            if (bindposes != null && bindposes.Length > 0)
                destination.bindposes = bindposes;

            for (int channel = 0; channel < 8; channel++)
            {
                var sourceUvs = new List<Vector4>(source.vertexCount);
                source.GetUVs(channel, sourceUvs);
                if (sourceUvs.Count != source.vertexCount)
                    continue;

                var compactUvs = new List<Vector4>(referencedCount);
                for (int i = 0; i < sourceUvs.Count; i++)
                {
                    if (referenced[i])
                        compactUvs.Add(sourceUvs[i]);
                }

                destination.SetUVs(channel, compactUvs);
            }
        }

        private static void CopyReferenced<T>(
            IReadOnlyList<T> source,
            T[] destination,
            IReadOnlyList<bool> referenced)
        {
            int next = 0;
            for (int i = 0; i < source.Count; i++)
            {
                if (referenced[i])
                    destination[next++] = source[i];
            }
        }

        private readonly struct UniformSideVertexKey
            : IEquatable<UniformSideVertexKey>
        {
            private const float NormalQuantization = 10000f;

            private readonly int _sourceIndex;
            private readonly int _sideVariant;
            private readonly int _normalX;
            private readonly int _normalY;
            private readonly int _normalZ;

            public UniformSideVertexKey(
                int sourceIndex,
                bool sideVariant,
                Vector3 normal)
            {
                _sourceIndex = sourceIndex;
                _sideVariant = sideVariant ? 1 : 0;

                if (sideVariant)
                {
                    _normalX =
                        Mathf.RoundToInt(
                            normal.x * NormalQuantization);

                    _normalY =
                        Mathf.RoundToInt(
                            normal.y * NormalQuantization);

                    _normalZ =
                        Mathf.RoundToInt(
                            normal.z * NormalQuantization);
                }
                else
                {
                    _normalX = 0;
                    _normalY = 0;
                    _normalZ = 0;
                }
            }

            public bool Equals(
                UniformSideVertexKey other)
            {
                return _sourceIndex == other._sourceIndex
                    && _sideVariant == other._sideVariant
                    && _normalX == other._normalX
                    && _normalY == other._normalY
                    && _normalZ == other._normalZ;
            }

            public override bool Equals(object obj)
            {
                return obj is UniformSideVertexKey other
                    && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _sourceIndex;
                    hash = hash * 397 ^ _sideVariant;
                    hash = hash * 397 ^ _normalX;
                    hash = hash * 397 ^ _normalY;
                    hash = hash * 397 ^ _normalZ;
                    return hash;
                }
            }
        }

        private readonly struct QuantizedPoint : IEquatable<QuantizedPoint>,
            IComparable<QuantizedPoint>
        {
            private readonly int _x;
            private readonly int _y;
            private readonly int _z;

            public QuantizedPoint(Vector3 value)
            {
                _x = Mathf.RoundToInt(value.x * EdgeWeldPrecision);
                _y = Mathf.RoundToInt(value.y * EdgeWeldPrecision);
                _z = Mathf.RoundToInt(value.z * EdgeWeldPrecision);
            }

            public int CompareTo(QuantizedPoint other)
            {
                int result = _x.CompareTo(other._x);
                if (result != 0)
                    return result;

                result = _y.CompareTo(other._y);
                return result != 0 ? result : _z.CompareTo(other._z);
            }

            public bool Equals(QuantizedPoint other)
                => _x == other._x && _y == other._y && _z == other._z;

            public override bool Equals(object obj)
                => obj is QuantizedPoint other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _x;
                    hash = hash * 397 ^ _y;
                    hash = hash * 397 ^ _z;
                    return hash;
                }
            }
        }

        private readonly struct GeometricEdgeKey : IEquatable<GeometricEdgeKey>
        {
            private readonly QuantizedPoint _min;
            private readonly QuantizedPoint _max;

            public GeometricEdgeKey(Vector3 a, Vector3 b)
            {
                var pointA = new QuantizedPoint(a);
                var pointB = new QuantizedPoint(b);
                if (pointA.CompareTo(pointB) <= 0)
                {
                    _min = pointA;
                    _max = pointB;
                }
                else
                {
                    _min = pointB;
                    _max = pointA;
                }
            }

            public bool Equals(GeometricEdgeKey other)
                => _min.Equals(other._min) && _max.Equals(other._max);

            public override bool Equals(object obj)
                => obj is GeometricEdgeKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_min.GetHashCode() * 397) ^ _max.GetHashCode();
                }
            }
        }

        private struct BoundaryEdge
        {
            public BoundaryEdge(int a, int b, int count)
            {
                A = a;
                B = b;
                Count = count;
            }

            public int A;
            public int B;
            public int Count;
        }
    }
}