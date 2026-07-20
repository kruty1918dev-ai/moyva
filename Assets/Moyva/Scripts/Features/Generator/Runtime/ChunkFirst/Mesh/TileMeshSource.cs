using UnityEngine;

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

    internal readonly struct TileMeshSource
    {
        public TileMeshSource(
            Mesh mesh,
            Material[] materials,
            Matrix4x4 localMatrix,
            float visibleBottomY = float.NaN,
            TileMeshOccludedSides occludedSides = TileMeshOccludedSides.None,
            Vector2 tileCenterXZ = default,
            float tileHalfExtent = 0f)
        {
            Mesh = mesh;
            Materials = materials;
            LocalMatrix = localMatrix;
            VisibleBottomY = visibleBottomY;
            OccludedSides = occludedSides;
            TileCenterXZ = tileCenterXZ;
            TileHalfExtent = tileHalfExtent;
        }

        public Mesh Mesh { get; }
        public Material[] Materials { get; }
        public Matrix4x4 LocalMatrix { get; }
        public float VisibleBottomY { get; }
        public TileMeshOccludedSides OccludedSides { get; }
        public Vector2 TileCenterXZ { get; }
        public float TileHalfExtent { get; }
        public bool HasVisibleBottomY => !float.IsNaN(VisibleBottomY);
        public bool HasTileFootprint => TileHalfExtent > 0.0001f;
        public bool IsValid => Mesh != null && Mesh.vertexCount > 0;
    }
}
