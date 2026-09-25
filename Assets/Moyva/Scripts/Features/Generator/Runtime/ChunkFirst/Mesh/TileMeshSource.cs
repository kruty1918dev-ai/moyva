using UnityEngine;
using Kruty1918.Moyva.Generator.API;

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

    /// <summary>
    /// Absolute world-space surface heights at the four footprint corners of a
    /// dual-grid fragment (the four surrounding cell centers) plus the flat
    /// reference height the fragment was placed at. Consumed by
    /// <see cref="TileSurfaceHeightWarpUtility"/> to shear the fragment onto
    /// the shared bilinear height field so adjacent fragments meet seamlessly
    /// on slopes.
    /// </summary>
    internal readonly struct TileMeshCornerHeights
    {
        public TileMeshCornerHeights(
            float northWest,
            float northEast,
            float southWest,
            float southEast,
            float reference)
        {
            NorthWest = northWest;
            NorthEast = northEast;
            SouthWest = southWest;
            SouthEast = southEast;
            Reference = reference;
            HasValues = true;
        }

        public float NorthWest { get; }
        public float NorthEast { get; }
        public float SouthWest { get; }
        public float SouthEast { get; }
        /// <summary>World height of the unwarped fragment surface plane.</summary>
        public float Reference { get; }
        public bool HasValues { get; }

        /// <summary>
        /// Bilinear height at normalized footprint coordinates
        /// (u: west-&gt;east, v: south-&gt;north).
        /// </summary>
        public float Evaluate(float u, float v)
        {
            float south = Mathf.Lerp(SouthWest, SouthEast, u);
            float north = Mathf.Lerp(NorthWest, NorthEast, u);
            return Mathf.Lerp(south, north, v);
        }

        public bool IsFlat(float epsilon)
            => Mathf.Abs(NorthWest - Reference) <= epsilon
               && Mathf.Abs(NorthEast - Reference) <= epsilon
               && Mathf.Abs(SouthWest - Reference) <= epsilon
               && Mathf.Abs(SouthEast - Reference) <= epsilon;
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
            bool generateMissingClosure = false,
            TileMeshCornerHeights cornerHeights = default)
        {
            Mesh = mesh;
            Materials = materials;
            LocalMatrix = localMatrix;
            LayerId = graphLayerId;
            LayerName = graphLayerName;
            VisibleBottomY = visibleBottomY;
            OccludedSides = occludedSides;
            TileCenterXZ = tileCenterXZ;
            TileHalfExtent = tileHalfExtent;
            AuthoredClosurePolicy = authoredClosurePolicy;
            EdgeBottoms = edgeBottoms;
            TileGeometryMode = tileGeometryMode;
            GenerateMissingClosure = generateMissingClosure;
            CornerHeights = cornerHeights;
        }

        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Matrix4x4 LocalMatrix { get; }
        public string LayerId { get; }
        public string LayerName { get; }
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
        /// <summary>
        /// World-space corner surface heights used by the slope warp. Only set
        /// for dual-grid terrain fragments; all other sources stay flat.
        /// </summary>
        public TileMeshCornerHeights CornerHeights { get; }
        public bool HasCornerHeights => CornerHeights.HasValues && HasTileFootprint;
        public bool HasVisibleBottomY => !float.IsNaN(VisibleBottomY);
        public bool HasTileFootprint => TileHalfExtent > 0.0001f;
        public bool IsValid => Mesh != null && Mesh.vertexCount > 0;
    }
}
