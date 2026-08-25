using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class ChunkAlignedMapSizeUtility
    {
        private const int DefaultChunkSize = 16;

        public static int ResolveSceneChunkSize()
        {
            int value = TileWorldCreatorChunkBatchingUtility.ResolveSceneChunkSize();
            return value > 0 ? value : DefaultChunkSize;
        }

        public static Vector2Int CropToSceneChunks(int width, int height)
            => FullChunkMapSizeUtility.CropDown(
                width,
                height,
                ResolveSceneChunkSize());

        public static Vector2Int CropToSceneChunks(Vector2Int requested)
        {
            return FullChunkMapSizeUtility.CropDown(
                requested.x,
                requested.y,
                ResolveSceneChunkSize());
        }
    }
}
