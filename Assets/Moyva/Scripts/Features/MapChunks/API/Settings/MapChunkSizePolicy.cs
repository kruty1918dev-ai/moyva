using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    /// <summary>
    /// Canonical Moyva map/chunk contract: every runtime chunk is exactly
    /// 16x16 logical tiles. Requested map dimensions are normalized before
    /// logical map generation so there are no logical tiles outside chunks.
    /// </summary>
    public static class MapChunkSizePolicy
    {
        public const int ChunkSize = 16;

        public static Vector2Int CropMapSize(int requestedWidth, int requestedHeight)
            => new Vector2Int(CropAxis(requestedWidth), CropAxis(requestedHeight));

        public static int CropAxis(int requested)
        {
            int safe = Mathf.Max(1, requested);
            if (safe < ChunkSize)
                return ChunkSize;
            return (safe / ChunkSize) * ChunkSize;
        }

        public static bool IsFullChunkMap(int width, int height)
            => width >= ChunkSize
               && height >= ChunkSize
               && width % ChunkSize == 0
               && height % ChunkSize == 0;
    }
}
