using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public static class ChunkAlignedMapSizeUtility
    {
        public const string LogPrefix = "[MOYVA_CHUNK_SIZE]";
        private const int DefaultChunkSize = 16;
        private static readonly HashSet<string> Logged = new HashSet<string>();

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

        public static Vector2Int CropToSceneChunks(
            Vector2Int requested,
            string source,
            Object logContext = null)
        {
            int chunkSize = ResolveSceneChunkSize();
            Vector2Int effective = FullChunkMapSizeUtility.CropDown(
                requested.x,
                requested.y,
                chunkSize);

            if (effective != requested)
                LogCropOnce(source, requested, effective, chunkSize, logContext);

            return effective;
        }

        public static string BuildCropMessage(
            string source,
            Vector2Int requested,
            Vector2Int effective,
            int chunkSize)
        {
            Vector2Int trimmed = new Vector2Int(
                Mathf.Max(0, requested.x - effective.x),
                Mathf.Max(0, requested.y - effective.y));

            return
                $"{LogPrefix} MAP_CROPPED " +
                $"source={source} " +
                $"requested={requested.x}x{requested.y} " +
                $"effective={effective.x}x{effective.y} " +
                $"chunkSize={chunkSize} " +
                $"trimmed={trimmed.x}x{trimmed.y} " +
                "reason=full-chunks-only partialChunks=0";
        }

        public static void LogCropOnce(
            string source,
            Vector2Int requested,
            Vector2Int effective,
            int chunkSize,
            Object logContext = null)
        {
            if (requested == effective)
                return;

            string safeSource = string.IsNullOrWhiteSpace(source)
                ? "Unknown"
                : source;
            string key = $"{safeSource}|{requested.x}x{requested.y}|{effective.x}x{effective.y}|{chunkSize}";
            if (!Logged.Add(key))
                return;
        }
    }
}
