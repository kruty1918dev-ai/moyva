using UnityEngine;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    [System.Flags]
    internal enum TileMeshOccludedSides
    {
        None = 0,
        North = 1 << 0,
        East = 1 << 1,
        South = 1 << 2,
        West = 1 << 3
    }

    internal readonly struct TileMeshEdgeBottoms
    {
        public TileMeshEdgeBottoms(float north, float east, float south, float west)
        {
            North = north;
            East = east;
            South = south;
            West = west;
            HasValues = true;
        }

        public float North { get; }
        public float East { get; }
        public float South { get; }
        public float West { get; }
        public bool HasValues { get; }

        public float Resolve(TileMeshOccludedSides side, float fallback)
        {
            if (!HasValues)
                return fallback;

            float value = side switch
            {
                TileMeshOccludedSides.North => North,
                TileMeshOccludedSides.East => East,
                TileMeshOccludedSides.South => South,
                TileMeshOccludedSides.West => West,
                _ => fallback
            };
            return float.IsNaN(value) || float.IsInfinity(value)
                ? fallback
                : value;
        }
    }

    internal readonly struct TileMeshSource
    {
        public TileMeshSource(
            Mesh mesh,
            Material[] materials,
            Matrix4x4 localMatrix,
            string graphLayerId = null,
            string graphLayerName = null,
            float visibleBottomY = float.NaN,
            TileMeshOccludedSides occludedSides = TileMeshOccludedSides.None,
            Vector2 tileCenterXZ = default,
            float tileHalfExtent = 0f,
            AuthoredClosurePolicy authoredClosurePolicy = AuthoredClosurePolicy.PreserveAuthored,
            TileMeshEdgeBottoms edgeBottoms = default,
            TileGeometryMode tileGeometryMode = TileGeometryMode.SolidTerrain,
            bool generateMissingClosure = false)
        {
            Mesh = mesh;
            Materials = materials;
            LocalMatrix = localMatrix;
            GraphLayerId = graphLayerId;
            GraphLayerName = graphLayerName;
            VisibleBottomY = visibleBottomY;
            OccludedSides = occludedSides;
            TileCenterXZ = tileCenterXZ;
            TileHalfExtent = tileHalfExtent;
            AuthoredClosurePolicy = authoredClosurePolicy;
            EdgeBottoms = edgeBottoms;
            TileGeometryMode = tileGeometryMode;
            GenerateMissingClosure = generateMissingClosure;
        }

        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Matrix4x4 LocalMatrix { get; }
        public string GraphLayerId { get; }
        public string GraphLayerName { get; }
        public float VisibleBottomY { get; }
        public TileMeshOccludedSides OccludedSides { get; }
        public Vector2 TileCenterXZ { get; }
        public float TileHalfExtent { get; }
        public AuthoredClosurePolicy AuthoredClosurePolicy { get; }
        public TileMeshEdgeBottoms EdgeBottoms { get; }
        public TileGeometryMode TileGeometryMode { get; }
        /// <summary>
        /// Adds only the missing band below an authored volume. The authored
        /// vertices and triangles remain untouched; one designated child mesh
        /// owns the generated closure so multi-part prefabs cannot duplicate it.
        /// </summary>
        public bool GenerateMissingClosure { get; }
        public bool HasVisibleBottomY => !float.IsNaN(VisibleBottomY);
        public bool HasTileFootprint => TileHalfExtent > 0.0001f;
        public bool IsValid => Mesh != null && Mesh.vertexCount > 0;
    }
}
