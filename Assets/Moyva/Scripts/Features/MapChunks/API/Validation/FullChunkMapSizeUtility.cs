using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public static class FullChunkMapSizeUtility
    {
        public static bool IsAligned(int width, int height, int chunkSize)
        {
            int chunk = Mathf.Max(1, chunkSize);
            return width >= chunk
                && height >= chunk
                && width % chunk == 0
                && height % chunk == 0;
        }

        public static Vector2Int CropDown(int width, int height, int chunkSize)
        {
            int chunk = Mathf.Max(1, chunkSize);
            return new Vector2Int(
                CropAxis(width, chunk),
                CropAxis(height, chunk));
        }

        private static int CropAxis(int value, int chunkSize)
        {
            int safe = Mathf.Max(1, value);
            if (safe < chunkSize)
                return chunkSize;
            return safe - safe % chunkSize;
        }
    }
}
