using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerMaskUtility
    {
        bool[,] ExtractOutputMask(LayerOutputSnapshot snapshot, LayerOutputKind outputKind, int width, int height);
        bool[,] Normalize(bool[,] source, int width, int height);
        IEnumerable<Vector2> EnumeratePositions(bool[,] mask);
    }

    internal sealed class GraphCompilerMaskUtility : IGraphCompilerMaskUtility
    {
        public bool[,] ExtractOutputMask(LayerOutputSnapshot snapshot, LayerOutputKind outputKind, int width, int height)
        {
            if (snapshot == null)
                return null;
            if (snapshot.LayerMask != null)
                return Normalize(snapshot.LayerMask, width, height);
            if (outputKind == LayerOutputKind.Masks)
                return null;
            if (snapshot.BiomeMap != null)
                return BuildMaskFromStringMap(snapshot.BiomeMap, width, height);
            if (snapshot.ObjectMap != null)
                return BuildMaskFromStringMap(snapshot.ObjectMap, width, height);
            if (snapshot.BuildingMap != null)
                return BuildMaskFromStringMap(snapshot.BuildingMap, width, height);
            return null;
        }

        public bool[,] Normalize(bool[,] source, int width, int height)
        {
            if (source == null)
                return null;
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            if (source.GetLength(0) != safeWidth
                || source.GetLength(1) != safeHeight)
            {
                Debug.LogError(
                    $"[GraphCompilerMaskUtility] Snapshot mask has invalid size " +
                    $"{source.GetLength(0)}x{source.GetLength(1)}; expected " +
                    $"{safeWidth}x{safeHeight}. The mask was rejected.");
                return null;
            }

            var result = new bool[safeWidth, safeHeight];
            System.Array.Copy(source, result, source.Length);
            return result;
        }

        public IEnumerable<Vector2> EnumeratePositions(bool[,] mask)
        {
            if (mask == null)
                yield break;
            for (int x = 0; x < mask.GetLength(0); x++)
            for (int y = 0; y < mask.GetLength(1); y++)
                if (mask[x, y])
                    yield return new Vector2(x, y);
        }

        private bool[,] BuildMaskFromStringMap(string[,] source, int width, int height)
        {
            if (source == null)
                return null;
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            if (source.GetLength(0) != safeWidth
                || source.GetLength(1) != safeHeight)
            {
                Debug.LogError(
                    $"[GraphCompilerMaskUtility] Snapshot string map has invalid size " +
                    $"{source.GetLength(0)}x{source.GetLength(1)}; expected " +
                    $"{safeWidth}x{safeHeight}. The map was rejected.");
                return null;
            }

            var result = new bool[safeWidth, safeHeight];
            for (int x = 0; x < safeWidth; x++)
            for (int y = 0; y < safeHeight; y++)
                result[x, y] = !string.IsNullOrEmpty(source[x, y]);
            return result;
        }
    }
}
