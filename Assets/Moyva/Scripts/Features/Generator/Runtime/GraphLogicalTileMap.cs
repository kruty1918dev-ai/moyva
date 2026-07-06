using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMap
    {
        public GraphLogicalTileMap(int width, int height)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            TileIds = new string[Width, Height];
            GraphLayerIds = new string[Width, Height];
            LayerNames = new string[Width, Height];
            LayerHeights = new float[Width, Height];
            SurfaceHeights = new float[Width, Height];
        }

        public int Width { get; }
        public int Height { get; }
        public string[,] TileIds { get; }
        public string[,] GraphLayerIds { get; }
        public string[,] LayerNames { get; }
        public float[,] LayerHeights { get; }
        public float[,] SurfaceHeights { get; }

        public Dictionary<string, bool[,]> BuildLayerMatrices()
        {
            var matrices = new Dictionary<string, bool[,]>(StringComparer.Ordinal);
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                AddCell(matrices, x, y);
            return matrices;
        }

        private void AddCell(Dictionary<string, bool[,]> matrices, int x, int y)
        {
            string layerId = GraphLayerIds[x, y];
            if (string.IsNullOrEmpty(layerId))
                return;

            if (!matrices.TryGetValue(layerId, out var matrix))
            {
                matrix = new bool[Width, Height];
                matrices[layerId] = matrix;
            }

            matrix[x, y] = true;
        }
    }
}
