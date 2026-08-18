
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridChunkSurfaceHandle
    {
        public ConstructionBuildGridChunkSurfaceHandle(
            MapChunkCoord coord,
            GameObject gameObject,
            Mesh mesh,
            MeshRenderer renderer,
            RectInt tileRect,
            Texture2D cellMask,
            byte[] cellMaskBuffer)
        {
            Coord = coord;
            GameObject = gameObject;
            Mesh = mesh;
            Renderer = renderer;
            TileRect = tileRect;
            CellMask = cellMask;
            CellMaskBuffer = cellMaskBuffer;
        }

        public MapChunkCoord Coord { get; }
        public GameObject GameObject { get; }
        public Mesh Mesh { get; }
        public MeshRenderer Renderer { get; }
        public RectInt TileRect { get; }
        public Texture2D CellMask { get; }
        public byte[] CellMaskBuffer { get; }
        public int AppliedMaskRevision { get; set; } = -1;
        public bool MaskDirty { get; set; } = true;

        /// <summary>
        /// True after the first complete CPU mask has been atomically
        /// uploaded to the Texture2D. Dirty updates may continue while the
        /// previous complete texture remains visible.
        /// </summary>
        public bool MaskReady { get; set; }
    }
}
